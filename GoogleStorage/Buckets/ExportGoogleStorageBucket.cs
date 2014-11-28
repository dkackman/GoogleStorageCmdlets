using System;
using System.IO;
using System.Threading.Tasks;
using System.Management.Automation;

using Newtonsoft.Json;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsData.Export, "GoogleStorageBucket")]
    public class ExportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeMetaData { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter BreakOnError { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var t = GetBucketContents();
                var contents = t.Result;

                foreach (var item in contents.items)
                {
                    if (IncludeMetaData)
                    {
                        SaveMetaData(item);
                    }

                    var path = Path.Combine(Destination, item.name);
                    Task t1 = DownloadItem(item, path);
                    t1.Wait();
                    WriteVerbose(string.Format("Object {0} saved to {1}", item.name, path));
                }
            }
            catch (HaltCommandException)
            {
            }
            catch (PipelineStoppedException)
            {
            }
            catch (AggregateException e)
            {
                WriteAggregateException(e);
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, e.Message, ErrorCategory.ReadError, null));
            }
        }

        private async Task DownloadItem(dynamic item, string path)
        {
            try
            {
                if (!Force && File.Exists(path))
                {
                    throw new InvalidOperationException(string.Format("The file {0} already exists. Use -Force to overwrite existing files", path));
                }

                var downloader = new FileDownloader(item.mediaLink, path, GetContentType(item.contentType));
                var cancelToken = GetCancellationToken();
                var access_token = await GetAccessToken(cancelToken);
                await downloader.Download(cancelToken, access_token);
            }
            catch (HaltCommandException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (BreakOnError)
                {
                    throw;
                }

                //WriteError(new ErrorRecord(e, e.Message, ErrorCategory.ReadError, null));
            }
        }

        private static string GetContentType(string contentType)
        {
            int semicolon = contentType.IndexOf(';');
            if (semicolon != -1)
            {
                return contentType.Substring(0, semicolon);
            }

            return contentType;
        }

        private void SaveMetaData(dynamic item)
        {
            // build out the folder strucutre that might be embedded in the item name
            Directory.CreateDirectory(Path.Combine(Destination, Path.GetDirectoryName(item.name)));

            string path = Path.Combine(Destination, item.name + ".json");

            if (!Force && File.Exists(path))
            {
                WriteVerbose(string.Format("{0} exists. Skipping", path));
            }
            else
            {
                WriteVerbose(string.Format("Saving {0} metadata", item.name));

                using (var writer = new StreamWriter(path))
                {
                    string json = JsonConvert.SerializeObject(item);
                    writer.Write(json);
                }
            }
        }

        private async Task<dynamic> GetBucketContents()
        {
            dynamic google = CreateClient();

            return await google.storage.v1.b(Bucket).o.get(GetCancellationToken());
        }
    }
}

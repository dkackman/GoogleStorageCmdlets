using System;
using System.IO;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsData.Export, "GoogleStorageObject", SupportsShouldProcess = true)]
    public class ExportGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Destination { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter IncludeMetaData { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var api = CreateApiWrapper();
                var t = api.GetObject(Bucket, ObjectName);
                var item = t.Result;

                if (ShouldProcess(string.Format("{0}/{1}", Bucket, ObjectName), "export"))
                {
                    string path = Path.Combine(Destination, item.name).Replace('/', Path.DirectorySeparatorChar);

                    bool process = true;
                    if (File.Exists(path)) // if the file exists confirm the overwrite
                    {
                        var msg = string.Format("Do you want to overwrite the file {0}?", path);
                        process = Force || ShouldContinue(msg, "Overwrite file?");
                    }

                    if (process)
                    {
                        var task = api.ExportObject(new Tuple<dynamic, string>(item, path), IncludeMetaData);
                        task.Wait(api.CancellationToken);

                        WriteVerbose(string.Format("Exported {0} to {1}", item.name, path));
                    }
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
    }
}
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Dynamic;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Add, "GoogleStorageBucket")]
    public class AddGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                Debug.Assert(!string.IsNullOrEmpty(project));

                var t = AddBucket(project);
                dynamic result = t.Result;
                WriteVerbose(string.Format("Bucket {0} added to project {1}", Bucket, project));
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

        private async Task<dynamic> AddBucket(string project)
        {
            dynamic google = CreateClient();
            dynamic args = new ExpandoObject();
            args.name = Bucket;

            return await google.storage.v1.b.post(GetCancellationToken(), args, project: new PostUrlParam(project));
        }
    }
}

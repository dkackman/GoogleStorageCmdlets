using System;
using System.Diagnostics;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBuckets")]
    public class GetGoogleStorageBuckets : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        [Parameter(Mandatory = false)]
        public string DisplayProperty { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                Debug.Assert(!string.IsNullOrEmpty(project));

                var api = CreateApiWrapper(project);
                var t = api.GetBuckets();
                dynamic result = t.Result;

                bool verbose = this.MyInvocation.BoundParameters.ContainsKey("Verbose");
                foreach (var item in result.items)
                {
                    if (verbose)
                    {
                        WriteObject(item, true);
                    }
                    else if (!string.IsNullOrEmpty(DisplayProperty))
                    {
                        WriteDynamicObject(item, DisplayProperty);
                    }
                    else
                    {
                        WriteObject(item.id);
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

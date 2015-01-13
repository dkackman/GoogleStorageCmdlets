using System;
using System.Diagnostics;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    /// <summary>
    /// Lists all the Buckets in a Project
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBuckets")]
    public class GetGoogleStorageBuckets : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The Google Storage project name 
        /// If not set uses <see cref="GoogleStorage.Config.SetGoogleStorageConfig.Project"/>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        /// <summary>
        /// The property name of each bucket to display. If not set Bucket name is dispalyed.
        /// Ignored if Verbose flag is set
        /// </summary>
        [Parameter(Mandatory = false)]
        public string DisplayProperty { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                Debug.Assert(!string.IsNullOrEmpty(project));

                using (var api = CreateApiWrapper(project))
                {
                    dynamic result = api.GetBuckets().WaitForResult(GetCancellationToken());

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

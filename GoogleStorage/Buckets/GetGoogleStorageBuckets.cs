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

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                if (string.IsNullOrEmpty(project))
                {
                    throw new InvalidOperationException("No project in configuration or as Cmdlet parameter");
                }

                using (var api = CreateApiWrapper(project))
                {
                    dynamic result = api.GetBuckets().WaitForResult(GetCancellationToken());

                    foreach (var item in result.items)
                    {
                        WriteDynamicObject(item);
                    }
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}

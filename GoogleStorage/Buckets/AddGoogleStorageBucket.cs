using System;
using System.Diagnostics;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    /// <summary>
    /// Adds a Bucket to Google Storage
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "GoogleStorageBucket")]
    public class AddGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The Google Storage project name where the bucket will be added
        /// If not set uses <see cref="GoogleStorage.Config.SetGoogleStorageConfig.Project"/>
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        /// <summary>
        /// The name of the bucket to add
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var project = GetProjectName(Project);
                if (string.IsNullOrEmpty(project))
                {
                    throw new InvalidOperationException("No project in configuration or as Cmdlet parameter");
                }

                using (var api = CreateApiWrapper())
                {
                    dynamic result = api.AddBucket(project, Bucket).WaitForResult(CancellationToken);
                    WriteVerbose(string.Format("Bucket {0} added to project {1}", Bucket, project));
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}

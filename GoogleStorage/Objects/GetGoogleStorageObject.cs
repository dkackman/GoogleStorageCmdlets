using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageObject")]
    public class GetGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The bucket where the object exists
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// The name of the object
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ObjectName { get; set; }

        /// <summary>
        /// The property name of the object to display. If not set Object name is displayed.
        /// Ignored if Verbose flag is set
        /// </summary>
        [Parameter(Mandatory = false)]
        public string DisplayProperty { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {
                    var item = api.GetObject(Bucket, ObjectName).WaitForResult(GetCancellationToken());

                    bool verbose = this.MyInvocation.BoundParameters.ContainsKey("Verbose");
                    if (verbose)
                    {
                        WriteDynamicObject(item, DisplayProperty);
                    }
                    else
                    {
                        WriteObject(item.name);
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

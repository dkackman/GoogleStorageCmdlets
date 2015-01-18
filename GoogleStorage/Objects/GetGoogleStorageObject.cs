using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    /// <summary>
    /// Retreives the properties of a google storage object
    /// </summary>
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

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {
                    var item = api.GetObject(Bucket, ObjectName).WaitForResult(GetCancellationToken());

                    WriteDynamicObject(item);
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}

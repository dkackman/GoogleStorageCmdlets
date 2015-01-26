using System;
using System.Management.Automation;


namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageObjectACL")]
    public class GetGoogleStorageObjectACL : GoogleStorageAuthenticatedCmdlet
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
                    dynamic acls = api.GetObjectACL(Bucket, ObjectName).WaitForResult(CancellationToken);
                    foreach(var acl in acls.items)
                    {
                        WriteDynamicObject(acl);
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

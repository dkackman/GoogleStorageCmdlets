using System;
using System.Management.Automation;


namespace GoogleStorage.Objects
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBucketACL")]
    public class GetGoogleStorageObjectACL : GoogleStorageAuthenticatedCmdlet
    {
        /// <summary>
        /// The bucket 
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        /// <summary>
        /// An entity name to retrieve the ACL for
        /// If not set all ACLs are returned
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string EntityName { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                using (var api = CreateApiWrapper())
                {
                    if (string.IsNullOrEmpty(EntityName))
                    {
                        dynamic acls = api.GetBucketACL(Bucket).WaitForResult(CancellationToken);
                        foreach (var acl in acls.items)
                        {
                            WriteDynamicObject(acl);
                        }
                    }
                    else
                    {
                        dynamic acl = api.GetBucketACL(Bucket, EntityName).WaitForResult(CancellationToken);
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

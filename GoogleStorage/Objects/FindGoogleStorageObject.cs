﻿using System;
using System.Management.Automation;

namespace GoogleStorage.Objects
{
    /// <summary>
    /// Indicates whether an object exists in a particular bucket
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "GoogleStorageObject")]
    public class FindGoogleStorageObject : GoogleStorageAuthenticatedCmdlet
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
                    bool exists = api.FindObject(Bucket, ObjectName).WaitForResult(CancellationToken);
                    WriteObject(exists);
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}

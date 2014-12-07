﻿using System;
using System.Management.Automation;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsCommon.Find, "GoogleStorageBucket")]
    public class FindGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var api = CreateApiWrapper();
                var t = api.FindBucket(Bucket);

                WriteObject(t.Result);
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

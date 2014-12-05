﻿using System;
using System.Threading.Tasks;
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
                var endpoint = GetBucketEndPoint();
                Task<dynamic> t = endpoint.get();
                WriteObject(t.Result != null);
            }
            catch (HaltCommandException)
            {
            }
            catch (PipelineStoppedException)
            {
            }
            catch (AggregateException e)
            {
                foreach (var error in e.InnerExceptions)
                {
                    WriteVerbose(error.Message);
                }
                WriteObject(false);
            }
            catch (Exception e)
            {
                WriteVerbose(e.Message);
                WriteObject(false);
            }
        }

        private dynamic GetBucketEndPoint()
        {
            dynamic google = CreateClient();

            return google.storage.v1.b(Bucket);
        }
    }
}
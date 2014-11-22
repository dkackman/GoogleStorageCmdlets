﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Management.Automation;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    [Cmdlet(VerbsCommon.Get, "GoogleStorageBuckets")]
    public class GetGoogleStorageBuckets : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = false)]
        public string Project { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var t = GetBuckets();
                dynamic result = t.Result;

                foreach (var item in result.items)
                {
                    WriteObject(item);
                    Host.UI.WriteLine("");
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

        private async Task<dynamic> GetBuckets()
        {
            var project = GetProjectName(Project);
            Debug.Assert(!string.IsNullOrEmpty(project));

            dynamic google = CreateClient();
            return await google.storage.v1.b.get(GetCancellationToken(), project: project);
        }
    }
}
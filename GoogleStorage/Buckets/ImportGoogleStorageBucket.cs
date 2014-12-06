using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Collections.Generic;

using GoogleStorage.ProducerConsumer;

namespace GoogleStorage.Buckets
{
    [Cmdlet(VerbsData.Import, "GoogleStorageBucket", SupportsShouldProcess = true)]
    public class ImportGoogleStorageBucket : GoogleStorageAuthenticatedCmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string Source { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Force { get; set; }
    }
}

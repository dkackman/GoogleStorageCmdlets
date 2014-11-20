using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Security;

namespace GoogleStorage
{
    [Cmdlet(VerbsSecurity.Grant, "GoogleStorageAuth")]
    public class GrantGoogleStorageAuth : PSCmdlet
    {
        public GrantGoogleStorageAuth()
        {
            Persist = false;
            ShowBrowser = false;
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter ShowBrowser { get; set; }

        protected override void BeginProcessing()
        {

        }

        protected override void ProcessRecord()
        {
            var storage = new PersistantStorage();

        }
    }

    [Cmdlet(VerbsSecurity.Revoke, "GoogleStorageAuth")]
    public class RevokeGoogleStorageAuth : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var storage = new PersistantStorage();

        }
    }
}

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
    class GrantGoogleStorageAuth : PSCmdlet
    {
        public bool Persist { get; set; }

        public bool ShowBrowser { get; set; }

        protected override void BeginProcessing()
        {
            
        }

        protected override void ProcessRecord()
        {
        //    this.Host.UI.Write
            var storage = new PersistantStorage();
            
        }
    }

    [Cmdlet(VerbsSecurity.Revoke, "GoogleStorageAuth")]
    class RevokeGoogleStorageAuth : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var storage = new PersistantStorage();

        }
    }
}

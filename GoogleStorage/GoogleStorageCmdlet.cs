using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage
{
    public abstract class GoogleStorageCmdlet : PSCmdlet
    {
        protected PSCredential GetConfig()
        {
            var credential = this.GetPersistedVariableValue<PSCredential>("config", d =>
            {
                var encrypted = d.Password as string;
                return new PSCredential(d.UserName, encrypted.FromEncyptedString());
            });

            if (credential == null)
            {
                throw new InvalidOperationException("Google Storage config not set. Call Set-GoogleStorageConfig first");
            }
            return credential;
        }
    }
}

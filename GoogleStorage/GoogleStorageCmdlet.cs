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
        protected dynamic GetPersistedVariableValue(string name, object defaultValue = null)
        {
            return GetPersistedVariableValue<dynamic>(name, o => o, defaultValue);
        }

        protected T GetPersistedVariableValue<T>(string name, Func<dynamic, T> convert, object defaultValue = null)
        {
            // if the variable isn't in session state - see if it is persisted and add it to the session
            if (GetVariableValue(name, null) == null)
            {
                var storage = new PersistantStorage();
                if (storage.ObjectExists(name))
                {
                    var o = storage.RetrieveObject(name);
                    SessionState.PSVariable.Set(name, convert(o));
                    WriteVerbose(name + " retreived from presistant storage.");
                }
            }
            return (T)GetVariableValue(name, defaultValue);
        }

        protected void SetPersistedVariableValue(string name, object value, bool persist = true)
        {
            SessionState.PSVariable.Set(name, value);

            if (persist)
            {
                var storage = new PersistantStorage();
                storage.StoreObject(name, value);
                WriteVerbose(name + " stored for future sessions.");
            }
        }

        protected void ClearPersistedVariableValue(string name)
        {
            SessionState.PSVariable.Remove(name);

            var storage = new PersistantStorage();
            storage.RemoveObject(name);
            WriteVerbose(name + " cleared from session and persistant storage.");
        }

        protected bool AssertVariableValue(string name, string message)
        {
            if (GetVariableValue(name, null) == null)
            {
                WriteError(new ErrorRecord(new InvalidOperationException(message), message, ErrorCategory.ObjectNotFound, null));
                return false;
            }

            return true;
        }
    }
}

using System;
using System.Management.Automation;

namespace GoogleStorage
{
    static class PersistableState
    {
        public static dynamic GetPersistedVariableValue(this PSCmdlet cmdlet, string name, object defaultValue = null)
        {
            return cmdlet.GetPersistedVariableValue<dynamic>(name, o => o, defaultValue);
        }

        public static T GetPersistedVariableValue<T>(this PSCmdlet cmdlet, string name, Func<dynamic, T> convert, object defaultValue = null)
        {
            // if the variable isn't in session state - see if it is persted and add it to the session
            if (cmdlet.GetVariableValue(name, null) == null)
            {
                var storage = new PersistantStorage();
                if (storage.ObjectExists(name))
                {
                    var o = storage.RetrieveObject(name);
                    cmdlet.SessionState.PSVariable.Set(name, convert(o));
                    cmdlet.WriteVerbose(name + " retreived from presistant storage.");
                }
            }
            return (T)cmdlet.GetVariableValue(name, defaultValue);
        }

        public static void SetPersistedVariableValue(this PSCmdlet cmdlet, string name, object value, bool persist = true)
        {
            cmdlet.SessionState.PSVariable.Set(name, value);

            if (persist)
            {
                var storage = new PersistantStorage();
                storage.StoreObject(name, value);
                cmdlet.WriteVerbose(name + " stored for future sessions.");
            }
        }

        public static void ClearPersistedVariableValue(this PSCmdlet cmdlet, string name)
        {
            cmdlet.SessionState.PSVariable.Remove(name);

            var storage = new PersistantStorage();
            storage.RemoveObject(name);
            cmdlet.WriteVerbose(name + " cleared from session and persistant storage.");
        }

        public static bool AssertVariableValue(this PSCmdlet cmdlet, string name, string message)
        {
            if(cmdlet.GetVariableValue(name, null) == null)
            {
                cmdlet.WriteError(new ErrorRecord(new InvalidOperationException(message), message, ErrorCategory.ObjectNotFound, null));
                return false;
            }

            return true;
        }
    }
}

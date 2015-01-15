using System;
using System.Dynamic;
using System.Diagnostics;
using System.Threading;
using System.Management.Automation;
using System.Collections.Generic;

namespace GoogleStorage
{
    public abstract class GoogleStorageCmdlet : PSCmdlet
    {
        private CancellationTokenSource _cancelTokenSource;

        protected void WriteDynamicObject(dynamic o, string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                WriteObject(o);
            }
            else
            {                
                IDictionary<string, object> d = o as IDictionary<string, object>;
                Debug.Assert(d != null); 
                
                if (d.ContainsKey(propertyName))
                {
                    WriteObject(d[propertyName]);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Property {0} does not exist on result object", propertyName));
                }
            }
        }

        protected string GetProjectName(string projectName)
        {
            // a project set at a cmdlet property will take precendence over the config
            if (!string.IsNullOrEmpty(projectName))
            {
                return projectName;
            }

            dynamic config = GetConfig();

            return config.Project;
        }

        protected dynamic GetConfig()
        {
            var config = this.GetPersistedVariableValue("config", d =>
                {
                    // convert the serialized encrypted string into an in memory securestring
                    dynamic result = new ExpandoObject();
                    result.ClientId = d.ClientId;
                    result.ClientSecret = ((string)d.ClientSecret).FromEncyptedString();
                    result.Project = d.Project;
                    return result;
                },
                null);

            if (config == null)
            {
                throw new InvalidOperationException("Google Storage config not set. Call Set-GoogleStorageConfig first");
            }

            return config;
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
            }
        }

        protected void ClearPersistedVariableValue(string name)
        {
            SessionState.PSVariable.Remove(name);

            var storage = new PersistantStorage();
            storage.RemoveObject(name);
        }

        protected CancellationToken GetCancellationToken()
        {
            if (_cancelTokenSource == null)
            {
                throw new NullReferenceException("CancellationTokenSource is null. The base class BeginProccessing was not called.");
            }

            return _cancelTokenSource.Token;
        }

        protected void Cancel()
        {
            if (_cancelTokenSource == null)
            {
                throw new NullReferenceException("CancellationTokenSource is null. The base class BeginProccessing was not called.");
            }

            if (!_cancelTokenSource.IsCancellationRequested)
            {
                _cancelTokenSource.Cancel();
            }
        }

        protected override void BeginProcessing()
        {
            Debug.Assert(_cancelTokenSource == null);
            _cancelTokenSource = new CancellationTokenSource();
        }

        protected override void EndProcessing()
        {
            try
            {
                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }
        }

        protected override void StopProcessing()
        {
            WriteVerbose("Cancelling...");
            Cancel();
        }

        protected void HandleException(Exception e)
        {
            if (e is AggregateException)
            {
                foreach (var error in ((AggregateException)e).InnerExceptions)
                {
                    HandleException(e);
                }
            }
            else
            {
                var category = e.GetCategory();
                WriteError(new ErrorRecord(e, category.ToString(), category, null));
            }
        }
    }
}

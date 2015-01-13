using System;
using System.Management.Automation;

namespace GoogleStorage.Config
{
    /// <summary>
    /// Clears any currently set configuration (clear both persisted and non-persisted configuration)
    /// </summary>
    [Cmdlet(VerbsCommon.Clear, "GoogleStorageConfig")]
    public class ClearGoogleStorageConfig : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                ClearPersistedVariableValue("config");
                WriteVerbose("Configuration cleared");
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                        e,
                        "ClearGoogleStorageConfig",
                        ErrorCategory.NotSpecified,
                        ""));
            }
        }
    }
}

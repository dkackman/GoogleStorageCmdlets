using System;
using System.Management.Automation;

namespace GoogleStorage.Config
{
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

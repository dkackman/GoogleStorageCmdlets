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

using System.Management.Automation;

namespace GoogleStorage.Config
{
    [Cmdlet(VerbsSecurity.Revoke, "GoogleStorageAccess")]
    public class RevokeGoogleStorageAccess : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            ClearPersistedVariableValue("access");
            WriteVerbose("Google Storage authorization revoked");
        }
    }
}

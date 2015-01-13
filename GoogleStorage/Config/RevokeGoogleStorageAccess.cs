using System.Management.Automation;

namespace GoogleStorage.Config
{
    /// <summary>
    /// Revokes OAuth2 access to Google Storage
    /// </summary>
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

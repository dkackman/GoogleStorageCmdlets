using System;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Diagnostics;

namespace GoogleStorage
{
    [Cmdlet(VerbsSecurity.Grant, "GoogleStorageAccess")]
    public class GrantGoogleStorageAccess : GoogleStorageCmdlet
    {
        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter ShowBrowser { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var startTask = StartAuth();
                var response = startTask.Result;

                WriteWarning("This action requires authorization with Google Storage");
                if (!ShowBrowser)
                {
                    WriteObject("Navigate to this Url in a web browser:");
                    WriteObject(response.verification_url);
                    WriteObject("");
                }
                else
                {
                    Process.Start((string)response.verification_url);
                }

                WriteObject("Enter this code in the authorize web page to grant access to Google Storage");
                WriteObject(response.user_code);
                WriteObject("");

                WriteVerbose("Waiting for authorization...");

                var confirmTask = ConfirmAuth(response);
                var access = confirmTask.Result;

                access.expiry = DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in);
                SetPersistedVariableValue("access", access, Persist);
                WriteVerbose("Authorized");
            }
            catch (AggregateException e)
            {
                WriteAggregateException(e);
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, e.Message, ErrorCategory.ReadError, null));
            }
        }

        private async Task<dynamic> ConfirmAuth(dynamic response)
        {
            var oauth = new GoogleOAuth2("https://www.googleapis.com/auth/devstorage.read_write");
            dynamic access = await oauth.WaitForConfirmation(response, GetConfig(), GetCancellationToken());
            return access;
        }

        private async Task<dynamic> StartAuth()
        {
            var oauth = new GoogleOAuth2("https://www.googleapis.com/auth/devstorage.read_write");
            dynamic response = await oauth.StartAuthentication(GetConfig(), GetCancellationToken());
            return response;
        }
    }

    [Cmdlet(VerbsSecurity.Revoke, "GoogleStorageAuth")]
    public class RevokeGoogleStorageAccess : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            ClearPersistedVariableValue("access");
        }
    }
}

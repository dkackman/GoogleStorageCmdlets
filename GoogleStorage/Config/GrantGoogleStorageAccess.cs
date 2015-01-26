using System;
using System.Management.Automation;
using System.Diagnostics;

namespace GoogleStorage.Config
{
    /// <summary>
    /// Initiates Oauth2 authentication and authorization for Google Storage
    /// </summary>
    [Cmdlet(VerbsSecurity.Grant, "GoogleStorageAccess")]
    public class GrantGoogleStorageAccess : GoogleStorageCmdlet
    {
        /// <summary>
        /// Flag indicating whether to save the auth result
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        /// <summary>
        /// Flag indicating whether the Cmdlet will attempt to open a browser nevigated to
        /// the verification url. Uses the shell to open the browser. Will not work in headless oepration
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter ShowBrowser { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var config = GetConfig();

                using (var oauth = new GoogleOAuth2(config.ClientId, GoogleStorageApi.AuthScope))
                {
                    var confirmToken = oauth.StartAuthentication(CancellationToken).WaitForResult(CancellationToken);

                    WriteWarning("This action requires authorization with Google Storage");
                    if (!ShowBrowser)
                    {
                        WriteVerbose("Navigate to this Url in a web browser:");
                        WriteObject(confirmToken.verification_url);
                    }
                    else
                    {
                        WriteVerbose("Opening web browser at the verifcation url.");
                        Process.Start((string)confirmToken.verification_url);
                    }

                    WriteVerbose("Enter this code in the authorization web page to grant access of Google Storage to the Google Storage Cmdlets");
                    WriteObject(confirmToken.user_code);

                    WriteVerbose("Waiting for authorization...");
                    var access = oauth.WaitForConfirmation(confirmToken, config.ClientSecret, CancellationToken).WaitForResult(CancellationToken);

                    SetPersistedVariableValue("access", access, Persist);
                    WriteVerbose("Authorized");
                }
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }
    }
}

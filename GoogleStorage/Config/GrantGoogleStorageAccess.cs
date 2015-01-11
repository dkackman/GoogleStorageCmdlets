using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Diagnostics;

namespace GoogleStorage.Config
{
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
                dynamic config = GetConfig();
                var cancelToken = GetCancellationToken();

                var oauth = new GoogleOAuth2(GoogleStorageApi.AuthScope);
                var confirmToken = oauth.StartAuthentication(config.ClientId, cancelToken).WaitForResult(cancelToken);

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
                var access = oauth.WaitForConfirmation(confirmToken, config.ClientId, config.ClientSecret, cancelToken).WaitForResult(cancelToken);                

                SetPersistedVariableValue("access", access, Persist);
                WriteVerbose("Authorized");
            }
            catch (HaltCommandException)
            {
            }
            catch (PipelineStoppedException)
            {
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
    }
}

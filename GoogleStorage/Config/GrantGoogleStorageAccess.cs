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
        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter ShowBrowser { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                dynamic config = GetConfig();
                var cancelToken = GetCancellationToken();

                var startTask = StartAuth(config, cancelToken);
                var confirmToken = startTask.Result;

                WriteWarning("This action requires authorization with Google Storage");
                if (!ShowBrowser)
                {
                    WriteVerbose("Navigate to this Url in a web browser:");
                    WriteObject(confirmToken.verification_url);
                }
                else
                {
                    Process.Start((string)confirmToken.verification_url);
                }

                WriteVerbose("Enter this code in the authorization web page to grant access to Google Storage to the Google Storage Cmdlets");
                WriteObject(confirmToken.user_code);

                WriteVerbose("Waiting for authorization...");

                var confirmTask = ConfirmAuth(confirmToken, config, cancelToken);
                var access = confirmTask.Result;

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

        private static async Task<dynamic> ConfirmAuth(dynamic response, dynamic config, CancellationToken cancelToken)
        {
            var oauth = new GoogleOAuth2(GoogleStorageApi.AuthScope);
            dynamic access = await oauth.WaitForConfirmation(response, config, cancelToken);
            return access;
        }

        private static async Task<dynamic> StartAuth(dynamic config, CancellationToken cancelToken)
        {
            var oauth = new GoogleOAuth2(GoogleStorageApi.AuthScope);
            dynamic response = await oauth.StartAuthentication(config, cancelToken);
            return response;
        }
    }
}

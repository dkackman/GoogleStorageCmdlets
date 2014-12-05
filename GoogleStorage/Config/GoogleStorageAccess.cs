﻿using System;
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
                var startTask = StartAuth();
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

                WriteVerbose("Enter this code in the authorize web page to grant access to Google Storage");
                WriteObject(confirmToken.user_code);

                WriteVerbose("Waiting for authorization...");

                var confirmTask = ConfirmAuth(confirmToken);
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

    [Cmdlet(VerbsSecurity.Revoke, "GoogleStorageAccess")]
    public class RevokeGoogleStorageAccess : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            ClearPersistedVariableValue("access");
        }
    }
}

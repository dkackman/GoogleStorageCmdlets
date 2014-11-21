using System;
using System.Management.Automation;
using System.Security;

namespace GoogleStorage
{
    [Cmdlet(VerbsCommon.Set, "GoogleStorageConfig")]
    public class SetGoogleStorageConfig : PSCmdlet
    {
        public SetGoogleStorageConfig()
        {
            Persist = true;
            _prompt = false;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NoPrompt")]
        public string ClientId { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "NoPrompt")]
        public string ClientSecret { get; set; }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        public bool Persist { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "Prompt")]
        public SwitchParameter Prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }
        private bool _prompt;

        protected override void ProcessRecord()
        {
            PSCredential credential = null;

            if (_prompt)
            {
                credential = Host.UI.PromptForCredential("Enter Google Client Id and Secret", "Supply your Google Client Id as User name and Client Secret as Password", ClientId, ClientSecret, PSCredentialTypes.Generic, PSCredentialUIOptions.None);
            }
            else // use the CmdLet's paramters 
            {
                var password = new SecureString();
                Array.ForEach(ClientSecret.ToCharArray(), password.AppendChar);
                password.MakeReadOnly();
                credential = new PSCredential(ClientId, password);
            }

            if (credential != null)
            {
                this.SetPersistedVariableValue("config", credential);
            }
        }
    }

    [Cmdlet(VerbsCommon.Show, "GoogleStorageConfig")]
    public class ShowGoogleStorageConfig : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                var credential = this.GetPersistedVariableValue<PSCredential>("config", d =>
                    {
                        var encrypted = d.Password as string;
                        return new PSCredential(d.UserName, encrypted.FromEncyptedString());
                    }); 

                if (credential == null)
                {
                    WriteError(new ErrorRecord(
                            new InvalidOperationException("Google Storage config not set. Call Set-GoogleStorageConfig first"),
                            "ShowGoogleStorageConfig",
                            ErrorCategory.ObjectNotFound,
                            "config"));
                }
                else
                {
                    WriteObject(GetVariableValue("config", null));
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                        e,
                        "ShowGoogleStorageConfig",
                        ErrorCategory.NotSpecified,
                        "config"));
            }
        }
    }

    [Cmdlet(VerbsCommon.Clear, "GoogleStorageConfig")]
    public class ClearGoogleStorageConfig : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                this.ClearPersistedVariableValue("config"); 
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

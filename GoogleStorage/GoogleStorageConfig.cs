using System;
using System.Management.Automation;
using System.Dynamic;
using System.Security;

namespace GoogleStorage
{
    [Cmdlet(VerbsCommon.Set, "GoogleStorageConfig")]
    public class SetGoogleStorageConfig : GoogleStorageCmdlet
    {
        public SetGoogleStorageConfig()
        {
            Persist = true;
        }

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string ClientId { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public SecureString ClientSecret { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Project { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        protected override void ProcessRecord()
        {
            dynamic config = new ExpandoObject();
            config.ClientId = ClientId;
            config.ClientSecret = ClientSecret;
            config.Project = Project;

            this.SetPersistedVariableValue("config", config, Persist);
        }
    }

    [Cmdlet(VerbsCommon.Get, "GoogleStorageConfig")]
    public class GetGoogleStorageConfig : GoogleStorageCmdlet
    {
        protected override void ProcessRecord()
        {
            try
            {
                var config = GetConfig();

                if (config != null)
                {
                    WriteObject(config);
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(
                        e,
                        "GetGoogleStorageConfig",
                        ErrorCategory.NotSpecified,
                        "config"));
            }
        }
    }

    [Cmdlet(VerbsCommon.Clear, "GoogleStorageConfig")]
    public class ClearGoogleStorageConfig : GoogleStorageCmdlet
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

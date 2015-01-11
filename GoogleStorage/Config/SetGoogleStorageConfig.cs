using System.Management.Automation;
using System.Dynamic;
using System.Security;

namespace GoogleStorage.Config
{
    [Cmdlet(VerbsCommon.Set, "GoogleStorageConfig")]
    public class SetGoogleStorageConfig : GoogleStorageCmdlet
    {
        /// <summary>
        /// The OAuth2 client id for authentication access
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
        public string ClientId { get; set; }

        /// <summary>
        /// The OAuth2 client secret for authentication access
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public SecureString ClientSecret { get; set; }

        /// <summary>
        /// The default project name to use for subsequent google storage access
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Project { get; set; }

        /// <summary>
        /// Flag indicating whether to save the auth result
        /// </summary>
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
}

using System;
using System.Security;
using System.Threading.Tasks;
using System.Management.Automation;

namespace GoogleStorage
{
    public abstract class GoogleStorageAuthenticatedCmdlet : GoogleStorageCmdlet
    {
        private const string UserAgent = "GoogleStorageCmdlets/0.1";

        /// <summary>
        /// Flag indicating that no authentication is needed for the command execution
        /// i.e. the storage item being operated on is publically shared
        /// Stored authentication will be ignored
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter NoAuth { get; set; }

        protected GoogleStorageApi CreateApiWrapper()
        {
            var cancelToken = GetCancellationToken();
            string access_token = GetAccessToken().WaitForResult(cancelToken);

            return new GoogleStorageApi(UserAgent, access_token, cancelToken);
        }

        protected async Task<string> GetAccessToken(bool persist = true)
        {
            if (NoAuth)
            {
                return "";
            }

            var access = this.GetPersistedVariableValue<dynamic>("access", d =>
                {
                    d.access_token = ((string)d.access_token).FromEncyptedString();
                    return d;
                });

            if (access == null)
            {
                throw new AccessViolationException("Access token not set. Call Grant-GoogleStorageAccess first");
            }

            if (DateTime.UtcNow >= access.expiry)
            {
                var oauth = new GoogleOAuth2(GoogleStorageApi.AuthScope);
                access = await oauth.RefreshAccessToken(access, GetConfig(), GetCancellationToken());
                
                var storage = new PersistantStorage();
                SetPersistedVariableValue("access", access, persist || storage.ObjectExists("access")); // re-persist access token if already saved
            }

            SecureString token = access.access_token;
            return token.ToUnsecureString();
        }
    }
}

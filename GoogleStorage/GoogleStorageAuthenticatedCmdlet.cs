using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Security;

namespace GoogleStorage
{
    public abstract class GoogleStorageAuthenticatedCmdlet : GoogleStorageCmdlet
    {
        private const string UserAgent = "GoogleStorageCmdlets/0.1";

        [Parameter(Mandatory = false)]
        public SwitchParameter NoAuth { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter Persist { get; set; }

        protected GoogleStorageApi CreateApiWrapper()
        {
            return CreateApiWrapper("");
        }

        protected GoogleStorageApi CreateApiWrapper(string project)
        {
            var cancelToken = GetCancellationToken();
            string access_token = GetAccessToken(cancelToken).WaitForResult(cancelToken);

            return new GoogleStorageApi(project, UserAgent, access_token, cancelToken);
        }

        protected async Task<string> GetAccessToken(CancellationToken cancelToken)
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
                access = await oauth.RefreshAccessToken(access, GetConfig(), cancelToken);
                
                var storage = new PersistantStorage();
                SetPersistedVariableValue("access", access, Persist || storage.ObjectExists("access")); // re-persist access token if already saved
            }

            SecureString token = access.access_token;
            return token.ToUnsecureString();
        }
    }
}

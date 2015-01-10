using System;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Security;

namespace GoogleStorage
{
    public abstract class GoogleStorageAuthenticatedCmdlet : GoogleStorageCmdlet
    {
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
            var t = GetAccessToken(cancelToken);
            t.Wait(cancelToken);
            string access_token = t.Result;

            return new GoogleStorageApi(project, GoogleStorageAuthenticatedCmdlet.UserAgent, access_token, cancelToken);
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

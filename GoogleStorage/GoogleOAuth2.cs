using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;

using Microsoft.CSharp.RuntimeBinder;

using DynamicRestProxy.PortableHttpClient;

namespace GoogleStorage
{
    /// <summary>
    /// Helper class to deal with google oauth2 device flow:
    /// https://developers.google.com/accounts/docs/OAuth2ForDevices?csw=1
    /// 1) Initiate authorization
    /// 2) Direct user to enter code at specific url on any machine
    /// 3) Poll google until it indicates that auth has been granted
    /// </summary>
    public sealed class GoogleOAuth2 : IDisposable
    {
        private string _clientId;
        // the set of scopes to authorize
        private string _scope;
        private dynamic _google;

        public GoogleOAuth2(string clientId, string scope)
        {
            Debug.Assert(!string.IsNullOrEmpty(clientId));
            Debug.Assert(!string.IsNullOrEmpty(scope));

            _google = new DynamicRestClient("https://accounts.google.com/o/oauth2/");
            _clientId = clientId;
            _scope = scope;
        }

        public void Dispose()
        {
            ((IDisposable)_google).Dispose();
        }

        /// <summary>
        /// Begins the device oauth flow
        /// </summary>
        /// <param name="clientId">The app clientId</param>
        /// <returns>dynamic object with the verfication_url, device_code, and user_code.
        /// This should be passed back to WaitForFonfirmation</returns>
        public async Task<dynamic> StartAuthentication()
        {
            return await StartAuthentication(CancellationToken.None);
        }

        /// <summary>
        /// Begins the device oauth flow
        /// </summary>
        /// <param name="clientId">The app clientId</param>
        /// <param name="cancelToken">async cancellation token</param>
        /// <returns>dynamic object with the verfication_url, device_code, and user_code.
        /// This should be passed back to WaitForFonfirmation</returns>
        public async Task<dynamic> StartAuthentication(CancellationToken cancelToken)
        {
            return await _google.device.code.post(cancelToken, client_id: _clientId, scope: _scope);
        }

        /// <summary>
        /// Refreshes an expired access_token
        /// </summary>
        /// <param name="access">dynamic object with current access_token and the refresh_token</param>
        /// <param name="config">dynamic object with clientid and client secret</param>
        /// <param name="cancelToken">async cancellation token</param>
        /// <returns>new acess_token and the refresh_token</returns>
        public async Task<dynamic> RefreshAccessToken(string refresh_token, SecureString clientSecret, CancellationToken cancelToken)
        {
            Debug.Assert(!string.IsNullOrEmpty(refresh_token));

            var response = await _google.token.post(cancelToken, client_id: _clientId, client_secret: clientSecret.ToUnsecureString(), refresh_token: refresh_token, grant_type: "refresh_token");

            response.refresh_token = refresh_token; // the new access token doesn't have a new refresh token so move our current one here for long term storage
            // remember when the token expires so we know when to refresh it
            response.expiry = DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in);

            // replace the plain text access_token with a securestring
            response.access_token = ((string)response.access_token).ToSecureString();

            return response;
        }

        /// <summary>
        /// Here we spin wait and poll google to see if the user has authenticated and authorized the app
        /// Once the auth code is supplied to google, the proper access_token and refresh_token are returned here
        /// </summary>
        /// <returns></returns>
        public async Task<dynamic> WaitForConfirmation(dynamic confirmToken, SecureString clientSecret, CancellationToken cancelToken)
        {
            long expiration = confirmToken.expires_in;
            long interval = confirmToken.interval;
            long time = interval;

            // we are using the device flow so enter the code in the browser
            // here poll google for success
            while (time < expiration)
            {
                Thread.Sleep((int)interval * 1000);
                cancelToken.ThrowIfCancellationRequested();

                dynamic tokenResponse = await _google.token.post(cancelToken, client_id: _clientId, client_secret: clientSecret.ToUnsecureString(), code: confirmToken.device_code, grant_type: "http://oauth.net/grant_type/device/1.0");
                try
                {
                    if (tokenResponse.access_token != null)
                    {
                        // remember when the token expires so we know when to refresh it
                        tokenResponse.expiry = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResponse.expires_in);
                        // replace the plain text access_token with a securestring
                        tokenResponse.access_token = ((string)tokenResponse.access_token).ToSecureString();
                        return tokenResponse;
                    }
                }
                catch (RuntimeBinderException)
                {
                }

                time += interval;
            }

            throw new OperationCanceledException("Authorization from user timed out");
        }
    }
}

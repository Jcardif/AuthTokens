using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AuthTokens.Helpers;
using Microsoft.Identity.Client;

namespace AuthTokens.Services
{
    public class IdentityService
    {
        private readonly string _redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";


        private bool _integratedAuthAvailable;
        private IPublicClientApplication _client;
        private AuthenticationResult _authenticationResult;

        public event EventHandler LoggedIn;

        public event EventHandler LoggedOut;

        public void InitializeWithAadAndPersonalMsAccounts(string clientId)
        {
            _integratedAuthAvailable = false;
            _client = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(_redirectUri)
                .Build();
        }

        public void InitializeWithPersonalMsAccount(string clientId)
        {
            _integratedAuthAvailable = false;
            _client = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AadAuthorityAudience.PersonalMicrosoftAccount)
                .WithRedirectUri(_redirectUri)
                .Build();
        }

        public void InitializeWithAadMultipleOrgs(string clientId, bool integratedAuth = false)
        {
            _integratedAuthAvailable = integratedAuth;
            _client = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                .WithRedirectUri(_redirectUri)
                .Build();
        }

        public void InitializeWithAadSingleOrg(string clientId, string tenant, bool integratedAuth = false)
        {
            _integratedAuthAvailable = integratedAuth;
            _client = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
                .WithRedirectUri(_redirectUri)
                .Build();
        }

        public bool IsLoggedIn() => _authenticationResult != null;

        public async Task<LoginResultType> LoginAsync(List<string> scopes)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return LoginResultType.NoNetworkAvailable;
            }

            try
            {
                var accounts = await _client.GetAccountsAsync();

                _authenticationResult = await _client
                    .AcquireTokenInteractive(scopes)
                    .WithAccount(accounts.FirstOrDefault())
                    .ExecuteAsync();

                if (!IsAuthorized())
                {
                    _authenticationResult = null;
                    return LoginResultType.Unauthorized;
                }

                LoggedIn?.Invoke(this, EventArgs.Empty);
                return LoginResultType.Success;
            }
            catch (MsalClientException ex)
            {
                if (ex.ErrorCode == "authentication_canceled")
                {
                    return LoginResultType.CancelledByUser;
                }

                return LoginResultType.UnknownError;
            }
            catch (Exception)
            {
                return LoginResultType.UnknownError;
            }
        }

        public bool IsAuthorized()
        {
            // TODO WTS: You can also add extra authorization checks here.
            // i.e.: Checks permisions of _authenticationResult.Account.Username in a database.
            return true;
        }

        public string GetAccountUserName()
        {
            return _authenticationResult?.Account?.Username;
        }

        public async Task LogoutAsync()
        {
            try
            {
                var accounts = await _client.GetAccountsAsync();
                var account = accounts.FirstOrDefault();
                if (account != null)
                {
                    await _client.RemoveAsync(account);
                }

                _authenticationResult = null;
                LoggedOut?.Invoke(this, EventArgs.Empty);
            }
            catch (MsalException)
            {
                // TODO WTS: LogoutAsync can fail please handle exceptions as appropriate to your scenario
                // For more info on MsalExceptions see
                // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/exceptions
            }
        }

        public async Task<string> GetAccessTokenForGraphAsync(List<string> scopes) => await GetAccessTokenAsync(scopes);

        public async Task<string> GetAccessTokenAsync(List<string> scopes)
        {
            var acquireTokenSuccess = await AcquireTokenSilentAsync(scopes);
            if (acquireTokenSuccess)
            {
                return _authenticationResult.AccessToken;
            }
            else
            {
                try
                {
                    // Interactive authentication is required
                    var accounts = await _client.GetAccountsAsync();
                    _authenticationResult = await _client.AcquireTokenInteractive(scopes)
                                                         .WithAccount(accounts.FirstOrDefault())
                                                         .ExecuteAsync();
                    return _authenticationResult.AccessToken;
                }
                catch (MsalException)
                {
                    // AcquireTokenSilent and AcquireTokenInteractive failed, the session will be closed.
                    _authenticationResult = null;
                    LoggedOut?.Invoke(this, EventArgs.Empty);
                    return string.Empty;
                }
            }
        }
        
        private async Task<bool> AcquireTokenSilentAsync(List<string> scopes)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            try
            {
                var accounts = await _client.GetAccountsAsync();
                _authenticationResult = await _client.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                                                     .ExecuteAsync();
                return true;
            }
            catch (MsalUiRequiredException)
            {
                if (_integratedAuthAvailable)
                {
                    try
                    {
                        _authenticationResult = await _client.AcquireTokenByIntegratedWindowsAuth(scopes)
                                                             .ExecuteAsync();
                        return true;
                    }
                    catch (MsalUiRequiredException)
                    {
                        // Interactive authentication is required
                        return false;
                    }
                }
                else
                {
                    // Interactive authentication is required
                    return false;
                }
            }
            catch (MsalException)
            {
                // TODO WTS: Silentauth failed, please handle this exception as appropriate to your scenario
                // For more info on MsalExceptions see
                // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/exceptions
                return false;
            }
        }
    }
}

﻿using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;


namespace ProjApp.MsalClient
{
    /// <summary>
    /// This is a wrapper for PCA. It is singleton and can be utilized by both application and the MAM callback
    /// </summary>
    public class PCASocialWrapper : IPCAWrapper
    {
        private IConfiguration _configuration;
        private static Sttings _settings { get; set; }

        internal IPublicClientApplication PCA { get; }

        internal bool UseEmbedded { get; set; } = false;
        public string[] Scopes { get; set; }

        // public constructor
        public PCASocialWrapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _settings = _configuration.GetRequiredSection("Settings").Get<Sttings>();
            Scopes = _settings.ScopesForTwitter.ToStringArray();

            // Create PCA once. Make sure that all the config parameters below are passed
            PCA = PublicClientApplicationBuilder
                                        .Create(_settings.ClientIdForTwitter)
                                        .WithB2CAuthority(_settings.AuthorityForTwitter)
                                        .WithRedirectUri(PlatformConfig.Instance.RedirectUri)
                                        .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                                        .Build();
        }

        /// <summary>
        /// Acquire the token silently
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns>Authentication result</returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string[] scopes)
        {
            var accts = await PCA.GetAccountsAsync().ConfigureAwait(false);
            var acct = accts.FirstOrDefault();

            var authResult = await PCA.AcquireTokenSilent(scopes, acct)
                                        .ExecuteAsync().ConfigureAwait(false);
            return authResult;

        }

        /// <summary>
        /// Perform the interactive acquisition of the token for the given scope
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns></returns>
        public async Task<AuthenticationResult> AcquireTokenSilentAsync(string[] scopes)
        {
            var accts = await PCA.GetAccountsAsync(_settings.PolicySignUpSignInForTwitter).ConfigureAwait(false);
            var acct = accts.FirstOrDefault();

            var authResult = await PCA.AcquireTokenSilent(scopes, acct)
                                        .ExecuteAsync().ConfigureAwait(false);
            return authResult;

        }

        /// <summary>
        /// Perform the interactive acquisition of the token for the given scope
        /// </summary>
        /// <param name="scopes">desired scopes</param>
        /// <returns></returns>
        public async Task<AuthenticationResult> AcquireTokenInteractiveAsync(string[] scopes)
        {
            var systemWebViewOptions = new SystemWebViewOptions();
#if IOS
            // embedded view is not supported on Android
            if (UseEmbedded)
            {

                return await PCA.AcquireTokenInteractive(scopes)
                                        .WithUseEmbeddedWebView(true)
                                        .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                                        .ExecuteAsync()
                                        .ConfigureAwait(false);
            }

            // Hide the privacy prompt in iOS
            systemWebViewOptions.iOSHidePrivacyPrompt = true;
#endif

            var accounts = await PCA.GetAccountsAsync(_settings.PolicySignUpSignInForTwitter).ConfigureAwait(false); ;
            var acct = accounts.FirstOrDefault();

            return await PCA.AcquireTokenInteractive(scopes)
                                    .WithB2CAuthority(_settings.AuthorityForTwitter)
                                    .WithAccount(accounts.FirstOrDefault())
                                    .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                                    .WithUseEmbeddedWebView(true)
                                    .ExecuteAsync()
                                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Signout may not perform the complete signout as company portal may hold
        /// the token.
        /// </summary>
        /// <returns></returns>
        public async Task SignOutAsync()
        {
            var accounts = await PCA.GetAccountsAsync().ConfigureAwait(false);
            foreach (var acct in accounts)
            {
                await PCA.RemoveAsync(acct).ConfigureAwait(false);
            }
        }
    }
}
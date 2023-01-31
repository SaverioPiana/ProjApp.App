
using Microsoft.Identity.Client;

namespace MsalAuthInMaui.MsalClient
{
    /// <summary>
    /// This is a wrapper for PCA. It is singleton and can be utilized by both application and the MAM callback
    /// </summary>
    public class PCAWrapper
    {
        /// <summary>
        /// This is the singleton used by consumers
        /// </summary>
        public static PCAWrapper Instance { get; private set; } = new PCAWrapper();

        internal IPublicClientApplication PCA { get; }

        internal bool UseEmbedded { get; set; } = false;

        internal const string Authority = "https://login.microsoftonline.com/952b33b9-99b2-405f-b80c-b9202ff19446";
        internal const string ClientId = "479dcae7-94b7-4430-a3ff-b5c1cd6ec338";
        internal const string TenantId = "952b33b9-99b2-405f-b80c-b9202ff19446";
        public static string[] Scopes = { "api://479dcae7-94b7-4430-a3ff-b5c1cd6ec338/access_as_user" };

        // private constructor for singleton
        private PCAWrapper()
        {
            // Create PCA once. Make sure that all the config parameters below are passed
            PCA = PublicClientApplicationBuilder
                                        .Create(ClientId)
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
        internal async Task<AuthenticationResult> AcquireTokenInteractiveAsync(string[] scopes)
        {
            SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions();
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

            return await PCA.AcquireTokenInteractive(scopes)
                                    .WithAuthority(Authority)
                                    .WithTenantId(TenantId)
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
        internal async Task SignOutAsync()
        {
            var accounts = await PCA.GetAccountsAsync().ConfigureAwait(false);
            foreach (var acct in accounts)
            {
                await PCA.RemoveAsync(acct).ConfigureAwait(false);
            }
        }
    }
}

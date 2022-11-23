using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace Dax.Interactive;

internal static class ConnectionHelper
{
    private static readonly string[] AzureADScopes = new string[1]
    {
      "https://analysis.windows.net/powerbi/api/.default"
    };
    internal static IntPtr GetCurrentProcessMainWindowHandle()
    {
        using var current = Process.GetCurrentProcess();
        return current.MainWindowHandle;
    }
    public static IPublicClientApplication CreatePublicClientApplication()
    {
        return PublicClientApplicationBuilder.Create("7f67af8a-fedc-4b08-8b4e-37c4d127b6cf")
                                            .WithAuthority("https://login.microsoftonline.com/common")
                                            // .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                                            .WithRedirectUri("http://localhost:31333")
                                            .Build();
    }

    public static async Task<AuthenticationResult> AcquireTokenInteractiveAsync(string userPrincipalName, string claims, CancellationToken cancellationToken)
    {

        var prompt = Prompt.SelectAccount; // Force a sign-in as the MSAL web browser might contain cookies for the current user and we don't necessarily want to re-sign-in the same user
        var scopes = ConnectionHelper.AzureADScopes;
        var loginHint = userPrincipalName;

        var publicClient = CreatePublicClientApplication();
        var parameterBuilder = publicClient.AcquireTokenInteractive((IEnumerable<string>)  scopes)
                                            // .WithUseEmbeddedWebView(false)
                                            // .WithLoginHint(loginHint)
                                            .WithExtraQueryParameters("msafed=0")
                                            .WithPrompt(prompt)
                                            .WithClaims(claims);

        var mainwindowHwnd = GetCurrentProcessMainWindowHandle();
        parameterBuilder.WithParentActivityOrWindow(mainwindowHwnd);

        var msalAuthenticationResult = await parameterBuilder.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        return msalAuthenticationResult;

    }

}
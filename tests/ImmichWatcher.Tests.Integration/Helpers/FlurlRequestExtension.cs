using Flurl.Http;

namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

public static class FlurlRequestExtension
{
    public static IFlurlRequest WithAuthentication(this IFlurlRequest request, string accessToken)
    {
        return request.WithCookie("immich_access_token", accessToken)
            .WithCookie("immich_auth_type", "password")
            .WithCookie("immich_is_authenticated", true);
    }
}
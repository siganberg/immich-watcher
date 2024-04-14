using Flurl.Http;
using Siganberg.ImmichWatcher.Tests.Integration.Models;

namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

internal class ImmichApiManager
{
    private readonly string _testPassword;
    private readonly string _testEmail;
    private readonly string _httpProxymanLocal;
    
    private ServerInfoConfigResponse _serverInfoConfig = default!;
    public LoginInfoResponse LoginInfo = default!;
    public CreateApiKeyResponse ApiKey = default!;

    public ImmichApiManager(string httpAddress, string username, string password)
    {
        _testPassword = password;
        _testEmail = username;
        _httpProxymanLocal = httpAddress;
    }

    public async Task InitializedServerAsync()
    {
        _serverInfoConfig = await GetServerInfoConfigAsync();

        if (!_serverInfoConfig.IsInitialized)
            await SignUpAdmin();
        
        await LoginAsync();
        await GetOrCreateApiKey();
    }

    private async Task SignUpAdmin()
    {
        await new FlurlClient(_httpProxymanLocal)
            .Request("/api/auth/admin-sign-up")
            .PostJsonAsync(new
            {
                email = _testEmail,
                password = _testPassword,
                name = "Admin User"
            });
    }

    private Task<ServerInfoConfigResponse> GetServerInfoConfigAsync()
    {
        return new FlurlClient(_httpProxymanLocal)
            .Request("/api/server-info/config")
            .GetJsonAsync<ServerInfoConfigResponse>();
    }

    private async Task GetOrCreateApiKey()
    {
        await ClearOldApiKeysAsync();

        ApiKey = await new FlurlClient(_httpProxymanLocal)
            .Request("/api/api-key")
            .WithAuthentication(LoginInfo.AccessToken)
            .PostJsonAsync(new
            {
                name = "IntegrationTest API Key"
            })
            .ReceiveJson<CreateApiKeyResponse>();
    }

    private async Task ClearOldApiKeysAsync()
    {
        var apiKeys = await new FlurlClient(_httpProxymanLocal)
            .Request("/api/api-key")
            .WithAuthentication(LoginInfo.AccessToken)
            .GetJsonAsync<GetApiKeyResponse[]>();

        foreach (var apiKey in apiKeys)
            DeleteApiKeyAsync(apiKey);
    }

    private void DeleteApiKeyAsync(GetApiKeyResponse apiKey)
    {
        _ = new FlurlClient(_httpProxymanLocal)
            .Request($"/api/api-key/{apiKey.Id}")
            .WithAuthentication(LoginInfo.AccessToken)
            .DeleteAsync()
            .GetAwaiter()
            .GetResult();
    }

    private async Task ServerOnboardedAsync()
    {
        if (_serverInfoConfig.IsOnboarded) return;

        await new FlurlClient(_httpProxymanLocal)
            .Request("/api/server-info/admin-onboarding")
            .WithAuthentication(LoginInfo.AccessToken)
            .PostAsync();
    }

    private async Task LoginAsync()
    {
        LoginInfo = await new FlurlClient(_httpProxymanLocal)
            .Request("/api/auth/login")
            .PostJsonAsync(new
            {
                email = _testEmail,
                password = _testPassword
            }).ReceiveJson<LoginInfoResponse>();

        if (_serverInfoConfig.IsOnboarded) return;
        
        await ServerOnboardedAsync();
    }
}
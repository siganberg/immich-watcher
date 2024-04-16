using Flurl.Http;
using Polly;
using Siganberg.ImmichWatcher.Tests.Integration.Models;

namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

public class ImmichApiManager
{
    private readonly string _testPassword;
    private readonly string _testEmail;
    private readonly string _immichUrl;
    
    private ServerInfoConfigResponse _serverInfoConfig = default!;
    private LoginInfoResponse _loginInfo = default!;
    internal CreateApiKeyResponse ApiKey = default!;

    public ImmichApiManager(string immichUrl, string username, string password)
    {
        _testPassword = password;
        _testEmail = username;
        _immichUrl = immichUrl;
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
        await new FlurlClient(_immichUrl)
            .Request("/api/auth/admin-sign-up")
            .PostJsonAsync(new
            {
                email = _testEmail,
                password = _testPassword,
                name = "Admin User"
            });
    }

    private async Task<ServerInfoConfigResponse> GetServerInfoConfigAsync()
    {
        var request = new FlurlClient(_immichUrl)
            .Request("/api/server-info/config");
        
        var result = await Policy.Handle<FlurlHttpException>()
            .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(1))
            .ExecuteAsync(() => request.GetJsonAsync<ServerInfoConfigResponse>());
        
        return result;
    }

    private async Task GetOrCreateApiKey()
    {
        await ClearOldApiKeysAsync();

        ApiKey = await new FlurlClient(_immichUrl)
            .Request("/api/api-key")
            .WithAuthentication(_loginInfo.AccessToken)
            .PostJsonAsync(new
            {
                name = "IntegrationTest API Key"
            })
            .ReceiveJson<CreateApiKeyResponse>();
    }

    private async Task ClearOldApiKeysAsync()
    {
        var apiKeys = await new FlurlClient(_immichUrl)
            .Request("/api/api-key")
            .WithAuthentication(_loginInfo.AccessToken)
            .GetJsonAsync<GetApiKeyResponse[]>();

        foreach (var apiKey in apiKeys)
            DeleteApiKeyAsync(apiKey);
    }

    private void DeleteApiKeyAsync(GetApiKeyResponse apiKey)
    {
        _ = new FlurlClient(_immichUrl)
            .Request($"/api/api-key/{apiKey.Id}")
            .WithAuthentication(_loginInfo.AccessToken)
            .DeleteAsync()
            .GetAwaiter()
            .GetResult();
    }

    private async Task ServerOnboardedAsync()
    {
        if (_serverInfoConfig.IsOnboarded) return;

        await new FlurlClient(_immichUrl)
            .Request("/api/server-info/admin-onboarding")
            .WithAuthentication(_loginInfo.AccessToken)
            .PostAsync();
    }

    private async Task LoginAsync()
    {
        _loginInfo = await new FlurlClient(_immichUrl)
            .Request("/api/auth/login")
            .PostJsonAsync(new
            {
                email = _testEmail,
                password = _testPassword
            }).ReceiveJson<LoginInfoResponse>();

        if (_serverInfoConfig.IsOnboarded) return;
        
        await ServerOnboardedAsync();
    }

    public async Task ResetAssetAsync()
    {
        var ids = await new FlurlClient(_immichUrl)
            .Request("/api/asset")
            .WithAuthentication(_loginInfo.AccessToken)
            .GetJsonAsync<Asset[]>();
        
        await new FlurlClient(_immichUrl)
            .Request("/api/asset")
            .WithAuthentication(_loginInfo.AccessToken)
            .SendJsonAsync(HttpMethod.Delete, new
            {
                force = true, 
                ids = ids.Select(a => a.Id).ToArray()
            });
    }
}
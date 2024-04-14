namespace Siganberg.ImmichWatcher.Tests.Integration.Models;

internal class CreateApiKeyResponse
{
    public string Secret { get; set; } = string.Empty;
}

internal class GetApiKeyResponse
{
    public Guid Id { get; set; }
}
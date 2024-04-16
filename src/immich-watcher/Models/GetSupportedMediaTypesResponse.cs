namespace Siganberg.ImmichWatcher.Models;

internal class GetSupportedMediaTypesResponse
{
    public string[] Image { get; set; } = [];
    public string[] Video { get; set; } = [];
}
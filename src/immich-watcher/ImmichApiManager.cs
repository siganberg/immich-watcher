using Flurl.Http;
using Siganberg.ImmichWatcher.Models;

namespace Siganberg.ImmichWatcher;

internal class ImmichApiManager
{

    private readonly string _apiKey;
    private readonly string _immichUrl; 

    public ImmichApiManager(string apiKey, string? immichUrl)
    {
        _apiKey = apiKey;
        _immichUrl = immichUrl ?? "http://localhost:3001";
    }

    public async Task<UploadResponse> UploadFileAsync(string path, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(path);
        
        // TODO: add sidecar

        return await new FlurlClient(_immichUrl)
            .WithHeader("x-api-key", _apiKey)
            .Request("api/asset/upload")
            .PostMultipartAsync(a =>
            {
                a.AddFile("assetData", path);
                a.AddString("deviceId", "immich-watcher");
                a.AddString("deviceAssetId", $"{Path.GetFileName(path)}-{fileInfo.Length}");
                a.AddString("fileModifiedAt", fileInfo.LastWriteTime.ToString("yyyy-MM-dd hh:mm:ss"));
                a.AddString("fileCreatedAt", fileInfo.CreationTime.ToString("yyyy-MM-dd hh:mm:ss"));
            }, cancellationToken: cancellationToken)
            .ReceiveJson<UploadResponse>();
    }

    public async Task<GetSupportedMediaTypesResponse> GetSupportedMediaTypes()
    {
        const string endpoint = "/api/server-info/media-types";
        return  await new FlurlClient(_immichUrl)
            .Request(endpoint)
            .GetJsonAsync<GetSupportedMediaTypesResponse>();
    }
    
}
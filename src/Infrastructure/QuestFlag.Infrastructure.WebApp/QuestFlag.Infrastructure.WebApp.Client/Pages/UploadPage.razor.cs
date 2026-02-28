using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using QuestFlag.Infrastructure.Client;

namespace QuestFlag.Infrastructure.WebApp.Client.Pages;

public partial class UploadPage
{
    private string _taskName = "bulk-data";
    private string _category = "default";
    private string _tagsInput = "";
    private string _extraData = "";
    private string? _error;
    private string? _token;

    private List<UploadItem> _uploadQueue = new();

    protected override async Task OnInitializedAsync()
    {
        _token = await JS.InvokeAsync<string>("localStorage.getItem", "jwt_token");
        if (string.IsNullOrEmpty(_token))
        {
            Nav.NavigateTo("/login");
            return;
        }

        // Initialize the Typed Client with Bearer token
        UploadApi.SetBearerToken(_token);
    }

    private async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_taskName))
        {
            _error = "Task Name is required to formulate the storage path.";
            return;
        }

        _error = null;
        var tags = _tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var file in e.GetMultipleFiles(maximumFileCount: 20))
        {
            var item = new UploadItem { File = file };
            _uploadQueue.Add(item);

            // Fire and forget individual uploads so they run concurrently
            _ = ProcessUploadAsync(item, tags);
        }
    }

    private async Task ProcessUploadAsync(UploadItem item, string[] tags)
    {
        item.IsUploading = true;
        StateHasChanged();

        try
        {
            // Allowed size up to 100MB per file in this UI config
            using var stream = item.File.OpenReadStream(maxAllowedSize: 104857600);

            await UploadApi.UploadFileAsync(
                fileStream: stream,
                fileName: item.File.Name,
                taskName: _taskName,
                category: _category,
                tags: tags,
                extraData: _extraData
            );

            item.IsDone = true;
        }
        catch (Exception ex)
        {
            item.HasError = true;
            item.ErrorMessage = "Failed to upload.";
            Console.WriteLine(ex);
        }
        finally
        {
            item.IsUploading = false;
            StateHasChanged();
        }
    }

    private void ClearDone()
    {
        _uploadQueue.RemoveAll(q => q.IsDone);
    }

    private class UploadItem
    {
        public required IBrowserFile File { get; set; }
        public bool IsUploading { get; set; }
        public bool IsDone { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

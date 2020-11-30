using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DndBoard.Client.Store;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DndBoard.Client.Components
{
    public partial class ImagesComponent : ComponentBase
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Uninitialized value
        private ElementReference _files;
        private Dictionary<string, ElementReference> _filesRefs
            = new Dictionary<string, ElementReference>();
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Uninitialized value
        private IEnumerable<string> _filesIds;
        private string _boardId;

        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        [Inject]
        private HttpClient _httpClient { get; set; }
        [Inject]
        private AppState _appState { get; set; }


        protected override void OnInitialized()
        {
            _appState.BoardIdChanged += OnBoardIdChanged;
            _appState.ChatHubManager.SetNofifyFilesUpdateHandler(OnFilesUpdated);
        }

        public void OnFilesUpdated(string boardId)
        {
            _ = RefreshFilesAsync();
            async Task RefreshFilesAsync()
            {
                await ReloadFilesIds();
                _appState.FilesRefs = _filesRefs.Values.ToArray();
                await _appState.InvokeFilesRefsChanged();
            }
        }

        public async Task OnBoardIdChanged(string boardId)
        {
            _boardId = boardId;
            await ReinitializeFileInput();
            await ReloadFilesIds();
            _appState.FilesRefs = _filesRefs.Values.ToArray();
            await _appState.InvokeFilesRefsChanged();
        }

        [JSInvokable]
        public async Task ReloadFilesIds()
        {
            _filesIds = await _httpClient.GetFromJsonAsync<IEnumerable<string>>(
                $"images/getfilesids/{_boardId}"
            );
            StateHasChanged();
            await Task.Delay(300); // WTF? Images not really loaded by this time?
        }

        private async Task ReinitializeFileInput()
        {
            await _jsRuntime.InvokeAsync<string>(
                "reinitializeFileInput",
                new object[] { _boardId, _files, DotNetObjectReference.Create(this) }
            );
        }
    }
}

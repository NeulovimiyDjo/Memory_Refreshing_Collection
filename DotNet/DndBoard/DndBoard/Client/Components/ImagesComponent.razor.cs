using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DndBoard.Client.Components
{
    public partial class ImagesComponent : ComponentBase
    {
        private ElementReference _files;
        private IEnumerable<string> _filesIds;
        [Inject]
        private IJSRuntime _jsRuntime { get; set; }
        [Inject]
        private HttpClient _httpClient { get; set; }

        [JSInvokable]
        public async Task ReloadFilesIds()
        {
            _filesIds = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("images/getfilesids");
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            _filesIds = await _httpClient.GetFromJsonAsync<IEnumerable<string>>("images/getfilesids");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await InitializeFileInput();
        }

        private async Task InitializeFileInput()
        {
            await _jsRuntime.InvokeAsync<string>(
                "initializeFileInput",
                new object[] { _files, DotNetObjectReference.Create(this) }
            );
        }
    }
}

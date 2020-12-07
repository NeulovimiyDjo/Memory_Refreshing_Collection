//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using DndBoard.Client.BaseComponents;
//using DndBoard.Client.Helpers;
//using DndBoard.Client.Store;
//using DndBoard.Shared;
//using Microsoft.AspNetCore.Components;
//using Microsoft.JSInterop;

//namespace DndBoard.Client.Components
//{
//    public partial class ImagesComponent : CanvasBaseComponent
//    {
//#pragma warning disable IDE0044 // Add readonly modifier
//#pragma warning disable CS0649 // Uninitialized value
//        private ElementReference _files;
//        // Dictionary for refs is/was shit, old values dont get cleaned
//        private Dictionary<string, ElementReference> _filesRefs
//            = new Dictionary<string, ElementReference>();
//#pragma warning restore IDE0044 // Add readonly modifier
//#pragma warning restore CS0649 // Uninitialized value
//        private IEnumerable<string> _filesIds;
//        private string _boardId;
//        private readonly List<Coords> _coords = new List<Coords>();

//        [Inject]
//        private IJSRuntime _jsRuntime { get; set; }
//        [Inject]
//        private HttpClient _httpClient { get; set; }
//        [Inject]
//        private AppState _appState { get; set; }


//        protected override void OnInitialized()
//        {
//            _appState.BoardIdChanged += OnBoardIdChanged;
//            _appState.ChatHubManager.SetNofifyFilesUpdateHandler(OnFilesUpdated);
//            _appState.FilesRefsChanged += Redraw;
//        }

//        public void OnFilesUpdated(string boardId)
//        {
//            _ = RefreshFilesAsync();
//            async Task RefreshFilesAsync()
//            {
//                await ReloadFilesIds();
//                _appState.MapImages = _filesRefs.Values.ToList();
//                await _appState.InvokeFilesRefsChanged();
//            }
//        }

//        private async Task Redraw()
//        {
//            _coords.Clear();
//            for (int i = 0; i < _appState.MapImages.Count; i++)
//                _coords.Add(new Coords { X = 50, Y = 50 + i * 100 });

//            await CanvasMapRenderer.RedrawImagesByCoords(Canvas, _appState.MapImages);
//        }

//        public async Task OnBoardIdChanged(string boardId)
//        {
//            _boardId = boardId;
//            await ReinitializeFileInput();
//            OnFilesUpdated(_boardId);
//        }

//        [JSInvokable]
//        public async Task ReloadFilesIds()
//        {
//            _filesIds = await _httpClient.GetFromJsonAsync<IEnumerable<string>>(
//                $"images/getfilesids/{_boardId}"
//            );
//            StateHasChanged();
//            await Task.Delay(300); // WTF? Images not really loaded by this time?
//        }

//        private async Task ReinitializeFileInput()
//        {
//            await _jsRuntime.InvokeAsync<string>(
//                "reinitializeFileInput",
//                new object[] { _boardId, _files, DotNetObjectReference.Create(this) }
//            );
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DndBoard.Client.BaseComponents;
using DndBoard.Client.Helpers;
using DndBoard.Client.Models;
using DndBoard.Client.Store;
using DndBoard.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace DndBoard.Client.Components
{
    public partial class ImagesComponent2 : CanvasBaseComponent
    {
        private string _boardId;

        [Inject]
        private HttpClient _httpClient { get; set; }
        [Inject]
        private AppState _appState { get; set; }


        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            int maxAllowedFiles = 777;

            List<UploadedFile> files = new();
            foreach (IBrowserFile imageFile in e.GetMultipleFiles(maxAllowedFiles))
            {
                UploadedFile uploadedFile = await ConvertToUploadedFile(imageFile);
                files.Add(uploadedFile);
            }

            await SendFilesToServer(files);
        }

        private static async Task<UploadedFile> ConvertToUploadedFile(IBrowserFile imageFile)
        {
            string format = "image/png";

            IBrowserFile resizedImageFile = await imageFile
                                .RequestImageFileAsync(format, 100, 100);

            Stream stream = resizedImageFile.OpenReadStream();
            MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            stream.Close();

            UploadedFile uploadedFile = new();
            uploadedFile.FileName = resizedImageFile.Name;
            uploadedFile.FileContent = ms.ToArray();
            ms.Close();

            return uploadedFile;
        }

        private async Task SendFilesToServer(List<UploadedFile> files)
        {
            UploadedFiles uploadedFiles = new();
            uploadedFiles.BoardId = _boardId;
            uploadedFiles.Files = files.ToArray();

            await _httpClient.PostAsJsonAsync(
                "/api/fileupload/PostFiles", uploadedFiles
            );
        }

        private async Task OnRightClick(MouseEventArgs mouseEventArgs)
        {
            Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);
            MapImage clickedImage = GetClickedImage(coords);
            if (clickedImage is null)
                return;

            await _httpClient.PostAsync(
                $"/api/FileUpload/DeleteFile/{_boardId}/{clickedImage.Id}",
                null
            );
        }

        private async Task OnClick(MouseEventArgs mouseEventArgs)
        {
            Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);
            MapImage clickedImage = GetClickedImage(coords);
            if (clickedImage is null)
                return;
 
            CoordsChangeData coordsChangeData = new CoordsChangeData
            {
                ImageId = Guid.NewGuid().ToString(),
                Coords = new Coords { X = 10, Y = 10 },
                ModelId = clickedImage.Id,
            };
            string coordsChangeDataJson = JsonSerializer.Serialize(coordsChangeData);
            await _appState.ChatHubManager.SendCoordsAsync(coordsChangeDataJson);
            await _appState.InvokeFilesRefsChanged();
        }

        private MapImage GetClickedImage(Coords coords)
        {
            foreach (MapImage img in _appState.ModelImages)
            {
                if (coords.X >= img.Coords.X && coords.X <= img.Coords.X + 100
                    && coords.Y >= img.Coords.Y && coords.Y <= img.Coords.Y + 100)
                {
                    return img;
                }
            }

            return null;
        }

        protected override void OnInitialized()
        {
            _appState.BoardIdChanged += OnBoardIdChanged;
            _appState.ChatHubManager.SetNofifyFilesUpdateHandler(OnFilesUpdated);
        }

        private void OnFilesUpdated(string boardId)
        {
            _ = RefreshFilesAsync();
            async Task RefreshFilesAsync()
            {
                await ReloadFiles();
                await Redraw();
                await _appState.ChatHubManager.RequestAllCoords();
            }
        }

        private async Task Redraw()
        {
            await CanvasMapRenderer.RedrawImagesByCoords(Canvas, _appState.ModelImages);
        }

        private async Task OnBoardIdChanged(string boardId)
        {
            _boardId = boardId;
            OnFilesUpdated(_boardId);
        }

        private async Task ReloadFiles()
        {
            List<string> fileIds = await _httpClient.GetFromJsonAsync<List<string>>(
                $"/api/images/getfilesids/{_boardId}"
            );

            _appState.MapImages = new(); // Old image refs become invalid, so recreate.
            _appState.ModelImages = new(); // Otherwise existing refs don't get updated.
            StateHasChanged();

            _appState.ModelImages = fileIds.Select(id => new MapImage { Id = id }).ToList();
            for (int i = 0; i < _appState.ModelImages.Count; i++)
                _appState.ModelImages[i].Coords = new Coords { X = 50, Y = 50 + i * 110 };

            StateHasChanged();
            await Task.Delay(300); // Wait for images to get downloaded. Use loaded event?
        }
    }
}

using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DndBoard.Client.BaseComponents;
using DndBoard.Client.Helpers;
using DndBoard.Client.Store;
using DndBoard.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DndBoard.Client.Components
{
    public partial class MapComponent : CanvasBaseComponent
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Uninitialized value
        private bool _pressed;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Uninitialized value
        List<Coords> _coords;
        [Inject]
        private CanvasMapRenderer _canvasMapRenderer { get; set; }
        [Inject]
        private AppState _appState { get; set; }

        protected override void OnInitialized()
        {
            _appState.ChatHubManager.SetCoordsReceivedHandler(CoordsReceivedHandler);
            _appState.FilesRefsChanged += Redraw;
        }

        private async void CoordsReceivedHandler(string coordsJson)
        {
            _coords = JsonSerializer.Deserialize<List<Coords>>(coordsJson);
            await Redraw();
        }

        private async Task Redraw()
        {
            await _canvasMapRenderer.RedrawImagesByCoords(_coords,
                Canvas, _appState.FilesRefs);
        }

        private async Task OnMouseMoveAsync(MouseEventArgs mouseEventArgs)
        {
            if (_pressed)
            {
                Coords coords = await GetCanvasCoordinatesAsync(mouseEventArgs);
                _coords[0] = coords;
                string coordsJson = JsonSerializer.Serialize(_coords);
                await _appState.ChatHubManager.SendCoordsAsync(coordsJson);
            }
        }

        private void OnMouseDown(MouseEventArgs mouseEventArgs) => _pressed = true;
        private void OnMouseUp(MouseEventArgs mouseEventArgs) => _pressed = false;
        private void OnMouseOut(MouseEventArgs mouseEventArgs) => _pressed = false;
    }
}

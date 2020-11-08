using System.Threading.Tasks;
using DndBoard.Client.BaseComponents;
using DndBoard.Client.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DndBoard.Client.Components
{
    public partial class MapComponent : CanvasBaseComponent
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Uninitialized value
        private ElementReference _testImage;
        private bool _pressed;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Uninitialized value
        [Inject]
        private CanvasMapRenderer _canvasMapRenderer { get; set; }
        [Parameter]
        public ChatHubManager ChatHubManager { get; set; }

        protected override void OnInitialized()
        {
            ChatHubManager.SetCoordsReceivedHandler(CoordsReceivedHandler);
        }

        private async void CoordsReceivedHandler(string message)
        {
            string[] coords = message.Split(":");
            double clientX = double.Parse(coords[0].Trim());
            double clientY = double.Parse(coords[1].Trim());
            await _canvasMapRenderer.RedrawImagesByCoords(clientX, clientY, Canvas, _testImage);
        }

        private async Task OnMouseMoveAsync(MouseEventArgs mouseEventArgs)
        {
            if (_pressed)
            {
                (double clientX, double clientY) = await GetCanvasCoordinatesAsync(mouseEventArgs);
                await ChatHubManager.SendCoordsAsync($"{clientX} : {clientY}");
            }
        }

        private void OnMouseDown(MouseEventArgs mouseEventArgs) => _pressed = true;
        private void OnMouseUp(MouseEventArgs mouseEventArgs) => _pressed = false;
        private void OnMouseOut(MouseEventArgs mouseEventArgs) => _pressed = false;
    }
}

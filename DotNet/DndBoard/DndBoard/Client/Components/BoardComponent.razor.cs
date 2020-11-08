using System;
using System.Threading.Tasks;
using DndBoard.Client.Helpers;
using Microsoft.AspNetCore.Components;

namespace DndBoard.Client.Components
{
    public partial class BoardComponent : ComponentBase, IDisposable
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Uninitialized value
        private string _boardId;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Uninitialized value
        [Inject]
        private ChatHubManager _chatHubManager { get; set; }


        public void Dispose()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _chatHubManager.CloseConnectionAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        protected override async Task OnInitializedAsync()
        {
            _chatHubManager.SetupConnectionAsync();
            await _chatHubManager.StartConnectionAsync();
        }

        private async Task ConnectAsync()
        {
            await _chatHubManager.ConnectAsync(_boardId);
        }
    }
}

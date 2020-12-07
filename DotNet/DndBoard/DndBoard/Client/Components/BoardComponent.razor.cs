using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DndBoard.Client.Helpers;
using DndBoard.Client.Models;
using DndBoard.Client.Store;
using Microsoft.AspNetCore.Components;

namespace DndBoard.Client.Components
{
    public partial class BoardComponent : ComponentBase, IDisposable
    {
        private string _boardId;
        private string _connectedBoardId;
        [Inject]
        private ChatHubManager _chatHubManager { get; set; }
        [Inject]
        private AppState _appState { get; set; }


        public void Dispose()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _chatHubManager.CloseConnectionAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        protected override async Task OnInitializedAsync()
        {
            _appState.ChatHubManager = _chatHubManager;
            _chatHubManager.SetupConnectionAsync();
            _chatHubManager.SetConnectedHandler(ConnectedHanlder);
            await _chatHubManager.StartConnectionAsync();
        }

        private async Task ConnectAsync()
        {
            await _chatHubManager.ConnectAsync(_boardId);
        }

        private void ConnectedHanlder(string boardId)
        {
            _appState.MapImages = new List<MapImage>();
            _appState.ModelImages = new List<MapImage>();
            _connectedBoardId = boardId;
            StateHasChanged();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _appState.InvokeBoardIdChanged(boardId);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}

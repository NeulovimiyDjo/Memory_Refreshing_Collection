using System;
using System.Threading.Tasks;
using DndBoard.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace DndBoard.Client.Helpers
{
    public class ChatHubManager
    {
        private string _boardId;
        private HubConnection _hubConnection;
        private readonly NavigationManager _navigationManager;

        public ChatHubManager(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public void SetupConnectionAsync() =>
            _hubConnection = SetupSignalRConnection(BoardsHubContract.BaseAddress);

        public async Task StartConnectionAsync() =>
            await _hubConnection.StartAsync();

        public async Task CloseConnectionAsync() =>
            await _hubConnection.DisposeAsync();

        public void SetConnectedHandler(Action<string> handler) =>
            _hubConnection.On(BoardsHubContract.Connected, handler);

        public void SetCoordsReceivedHandler(Action<string> handler) =>
            _hubConnection.On(BoardsHubContract.CoordsChanged, handler);

        public void SetNofifyFilesUpdateHandler(Action<string> handler) =>
           _hubConnection.On(BoardsHubContract.NotifyFilesUpdate, handler);

        public async Task SendCoordsAsync(string coordsJson) =>
            await _hubConnection.SendAsync(BoardsHubContract.CoordsChanged, _boardId, coordsJson);

        public async Task ConnectAsync(string boardId)
        {
            _boardId = boardId;
            await _hubConnection.SendAsync(BoardsHubContract.Connect, _boardId);
        }


        private HubConnection SetupSignalRConnection(string hubUri)
        {
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri(hubUri))
                .Build();

            return connection;
        }
    }
}

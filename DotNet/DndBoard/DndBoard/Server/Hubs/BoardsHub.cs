using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DndBoard.Shared;
using Microsoft.AspNetCore.SignalR;

namespace DndBoard.Server.Hubs
{
    public class BoardsHub : Hub
    {
        private readonly BoardsManager _boardsManager;

        public BoardsHub(BoardsManager boardsManager)
        {
            _boardsManager = boardsManager;
        }

        [HubMethodName(BoardsHubContract.ImageRemoved)]
        public async Task SendImageRemoved(string boardId, string imageId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            board.ImagesOnMap.RemoveAll(img => img.Id == imageId);
            await Clients.Group(boardId).SendAsync(BoardsHubContract.ImageRemoved, imageId);
        }

        [HubMethodName(BoardsHubContract.RequestAllCoords)]
        public async Task RequestAllCoords(string boardId)
        {
            Board board = _boardsManager.GetBoard(boardId);
            foreach (MyImage img in board.ImagesOnMap)
            {
                CoordsChangeData coordsChangeData = new CoordsChangeData
                {
                    ImageId = img.Id,
                    Coords = img.Coords,
                    ModelId = img.ModelId,
                };

                string coordsChangeDataJson = JsonSerializer.Serialize(coordsChangeData);
                await Clients.Caller.SendAsync(BoardsHubContract.CoordsChanged, coordsChangeDataJson);
            }
        }

        [HubMethodName(BoardsHubContract.CoordsChanged)]
        public async Task SendCoords(string boardId, string coordsChangeDataJson)
        {
            Board board = _boardsManager.GetBoard(boardId);
            CoordsChangeData coordsChangeData = JsonSerializer
                .Deserialize<CoordsChangeData>(coordsChangeDataJson);

            if (!board.ImagesOnMap.Exists(img => img.Id == coordsChangeData.ImageId))
                board.ImagesOnMap.Add(new MyImage
                {
                    Id = coordsChangeData.ImageId,
                    Coords = coordsChangeData.Coords,
                    ModelId = coordsChangeData.ModelId,
                });
            else
                board.ImagesOnMap.Find(img => img.Id == coordsChangeData.ImageId)
                    .Coords = coordsChangeData.Coords;

            await Clients.Group(boardId).SendAsync(BoardsHubContract.CoordsChanged, coordsChangeDataJson);
        }

        [HubMethodName(BoardsHubContract.Connect)]
        public async Task Connect(string boardId)
        {
            if (!_boardsManager.BoardExists(boardId))
                _boardsManager.AddBoard(new Board
                {
                    BoardId = boardId,
                    ImagesOnMap = new List<MyImage>()
                });

            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
            await Clients.Caller.SendAsync(BoardsHubContract.Connected, boardId);
        }
    }
}

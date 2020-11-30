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


        [HubMethodName(BoardsHubContract.CoordsChanged)]
        public async Task SendCoords(string boardId, string coordsJson)
        {
            Board board = _boardsManager.GetBoard(boardId);
            List<Coords> coords = JsonSerializer.Deserialize<List<Coords>>(coordsJson);
            board.CoordsList = coords;
            await Clients.Group(boardId).SendAsync(BoardsHubContract.CoordsChanged, coordsJson);
        }

        [HubMethodName(BoardsHubContract.Connect)]
        public async Task Connect(string boardId)
        {
            if (!_boardsManager.BoardExists(boardId))
                _boardsManager.AddBoard(new Board
                    {
                        BoardId = boardId,
                        CoordsList = new List<Coords> { new Coords { X = 33, Y = 111 } }
                    }
                );

            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);

            Board board = _boardsManager.GetBoard(boardId);
            await Clients.Caller.SendAsync(BoardsHubContract.Connected, boardId);

            List<Coords> coords = board.CoordsList;
            string coordsJson = JsonSerializer.Serialize(coords);
            await Clients.Caller.SendAsync(BoardsHubContract.CoordsChanged, coordsJson);
        }
    }
}

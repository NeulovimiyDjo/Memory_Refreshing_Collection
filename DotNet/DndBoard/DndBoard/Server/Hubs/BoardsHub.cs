using System.Collections.Concurrent;
using System.Threading.Tasks;
using DndBoard.Shared;
using Microsoft.AspNetCore.SignalR;

namespace DndBoard.Server.Hubs
{
    public class BoardsHub : Hub
    {
        private static readonly ConcurrentDictionary<string, Board> _boards =
            new ConcurrentDictionary<string, Board>();

        [HubMethodName(BoardsHubContract.CoordsChanged)]
        public async Task SendCoords(string boardId, string coords)
        {
            Board board = _boards[boardId];
            string[] coordsArr = coords.Split(":");
            board.X = int.Parse(coordsArr[0].Trim());
            board.Y = int.Parse(coordsArr[1].Trim());

            await Clients.All.SendAsync(BoardsHubContract.CoordsChanged, coords);
        }

        [HubMethodName(BoardsHubContract.Connect)]
        public async Task Connect(string boardId)
        {
            if (!_boards.ContainsKey(boardId))
                _boards.TryAdd(boardId, new Board { X = 33, Y = 111 });

            Board board = _boards[boardId];
            await Clients.Caller.SendAsync(BoardsHubContract.CoordsChanged, $"{board.X} : {board.Y}");
        }
    }
}

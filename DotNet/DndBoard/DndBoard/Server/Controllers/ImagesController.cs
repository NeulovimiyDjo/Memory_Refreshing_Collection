using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using DndBoard.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using DndBoard.Shared;

namespace DndBoard.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;
        private readonly BoardsManager _boardManager;
        private readonly IHubContext<BoardsHub> _boardsHubContext;

        public ImagesController(
            ILogger<ImagesController> logger,
            BoardsManager boardManager,
            IHubContext<BoardsHub> boardsHubContext)
        {
            _logger = logger;
            _boardManager = boardManager;
            _boardsHubContext = boardsHubContext;
        }


        [HttpPost]
        public async Task<IActionResult> PostFiles(
            [FromForm(Name = "boardId")] string boardId,
            [FromForm(Name = "file")] IEnumerable<IFormFile> files)
        {
            _logger.LogInformation($"---------------------{files}-----------------------");

            //check if file was fully uploaded
            if (files == null || files.Count() == 0)
                return BadRequest("Upload a new File");

            foreach (IFormFile file in files)
            {
                using MemoryStream ms = new MemoryStream();
                await file.CopyToAsync(ms);

                Board board = _boardManager.GetBoard(boardId);
                board.AddFile(ms.ToArray());
            }
            await _boardsHubContext.Clients.Group(boardId).SendAsync(BoardsHubContract.NotifyFilesUpdate, boardId);

            return Ok("do something with this data....");
        }

        [HttpGet("{boardId}/{fileId}")]
        public async Task<IActionResult> GetFile(string boardId, string fileId)
        {
            Board board = _boardManager.GetBoard(boardId);
            byte[] file = board.GetFile(fileId);
            return File(file, "image/png");
        }

        [HttpGet("{boardId}")]
        public async Task<IEnumerable<string>> GetFilesIds(
            [FromRoute(Name = "boardId")] string boardId)
        {
            Board board = _boardManager.GetBoard(boardId);
            return board.GetFilesIds();
        }
    }
}

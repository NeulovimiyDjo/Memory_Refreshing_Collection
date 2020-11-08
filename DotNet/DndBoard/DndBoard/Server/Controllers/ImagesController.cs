using DndBoard.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Concurrent;

namespace DndBoard.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ImagesController : ControllerBase
    {
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(ILogger<ImagesController> logger)
        {
            _logger = logger;
        }

        private static ConcurrentDictionary<string, byte[]> _fileContent =
            new ConcurrentDictionary<string, byte[]>();

        [HttpPost]
        public async Task<IActionResult> PostFiles(
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
                _fileContent.TryAdd(_fileContent.Count().ToString(), ms.ToArray());
            }

            return Ok("do something with this data....");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            if (_fileContent.ContainsKey(id))
                return File(_fileContent[id], "image/png");

            return BadRequest("File with this id doesn't exist");
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetFilesIds()
        {
            return _fileContent.Keys.ToList();
        }
    }
}

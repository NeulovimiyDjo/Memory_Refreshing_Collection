using System.Collections.Generic;
using System.Threading.Tasks;
using DndBoard.Client.Helpers;
using DndBoard.Client.Models;

namespace DndBoard.Client.Store
{
    public class AppState
    {
        public ChatHubManager ChatHubManager;
        public List<MapImage> MapImages;
        public List<MapImage> ModelImages;

        public event FilesRefsChangedHandler FilesRefsChanged;
        public event BoardIdChangedHandler BoardIdChanged;

        public async Task InvokeFilesRefsChanged()
            => await FilesRefsChanged?.Invoke();
        public async Task InvokeBoardIdChanged(string boardId)
            => await BoardIdChanged?.Invoke(boardId);
    }
}

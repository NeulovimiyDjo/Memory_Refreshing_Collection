using System.Threading.Tasks;
using DndBoard.Client.Helpers;
using Microsoft.AspNetCore.Components;

namespace DndBoard.Client.Store
{
    public class AppState
    {
        public ChatHubManager ChatHubManager;
        public ElementReference[] FilesRefs;

        public event FilesRefsChangedHandler FilesRefsChanged;
        public event BoardIdChangedHandler BoardIdChanged;

        public async Task InvokeFilesRefsChanged()
            => await FilesRefsChanged?.Invoke();
        public async Task InvokeBoardIdChanged(string boardId)
            => await BoardIdChanged?.Invoke(boardId);
    }
}

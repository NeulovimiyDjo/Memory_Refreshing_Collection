using System.Threading.Tasks;

namespace DndBoard.Client
{
    public delegate Task FilesRefsChangedHandler();
    public delegate Task BoardIdChangedHandler(string boardId);
}

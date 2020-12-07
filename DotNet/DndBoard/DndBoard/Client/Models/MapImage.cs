using DndBoard.Shared;
using Microsoft.AspNetCore.Components;

namespace DndBoard.Client.Models
{
    public class MapImage
    {
        public string Id { get; init; }
        public ElementReference Ref { get; set; }
        public Coords Coords { get; set; }
        public string ModelId { get; set; }
    }
}

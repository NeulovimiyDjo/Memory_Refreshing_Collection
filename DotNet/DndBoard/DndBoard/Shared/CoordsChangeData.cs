namespace DndBoard.Shared
{
    public record CoordsChangeData
    {
        public string ImageId { get; set; }
        public Coords Coords { get; set; }
        public string ModelId { get; set; }
    }
}

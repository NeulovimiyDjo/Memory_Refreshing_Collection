namespace DndBoard.Shared
{
    public static class BoardsHubContract
    {
        public const string BaseAddress = "/boardshub";

        public const string NotifyFilesUpdate = nameof(NotifyFilesUpdate);
        public const string CoordsChanged = nameof(CoordsChanged);
        public const string Connect = nameof(Connect);
        public const string Connected = nameof(Connected);
    }
}

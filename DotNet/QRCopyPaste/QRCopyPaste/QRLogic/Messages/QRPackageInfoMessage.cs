namespace QRCopyPaste
{
    public class QRPackageInfoMessage
    {
        public string MsgIntegrity { get; set; }
        public int NumberOfParts { get; set; }
        public string DataType { get; set; }
        public string DataHash { get; set; }
        public int SenderDelay { get; set; }
    }
}

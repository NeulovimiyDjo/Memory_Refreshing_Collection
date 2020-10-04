using System.Windows.Media;

namespace QRCopyPaste
{
    public interface ISenderViewModel
    {
        public ImageSource ImageSource { get; set; }
        public int SenderProgress { get; set; }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;

namespace QRCopyPaste
{
    public class QRSender
    {
        private static QRPackage _lastQRPackage = null;

        private static bool _stopRequested = false;
        private static bool _isRunning = false;
        private static ISenderViewModel _senderViewModel;

        public QRSender(ISenderViewModel senderViewModel)
        {
            _senderViewModel = senderViewModel;
        }


        public async Task SendData<TData>(TData data)
        {
            if (_isRunning)
                throw new Exception("Data sending is already in progress.");
            _isRunning = true;

            var qrPackage = QRPackageCreator.CreateQRPackage(data);
            _lastQRPackage = qrPackage;

            await SendQRMessageSettingsAsync(qrPackage.QRPackageInfoMessage);
            await SendAllDataPartsAsync(qrPackage.QRDataPartsMessages);

            _senderViewModel.ImageSource = null; // Remove last DataPart QR from screen.
            _isRunning = false;
            _stopRequested = false;
        }


        public static async Task ResendLast(int[] selectiveIDs = null)
        {
            if (_lastQRPackage == null)
                throw new Exception("There is nothing to resend.");
            if (_isRunning)
                throw new Exception("Data sending is already in progress.");
            _isRunning = true;

            var qrPackage = _lastQRPackage;

            await SendQRMessageSettingsAsync(qrPackage.QRPackageInfoMessage);
            await SendAllDataPartsAsync(qrPackage.QRDataPartsMessages, selectiveIDs);

            _senderViewModel.ImageSource = null; // Remove last DataPart QR from screen.
            _isRunning = false;
            _stopRequested = false;
        }


        public static void RequestStop()
        {
            _stopRequested = true;
        }


        private static async Task SendAllDataPartsAsync(string[] dataParts, int[] selectiveIDs = null)
        {
            _senderViewModel.SenderProgress = 1;

            var numberOfParts = dataParts.Length;
            for (int i = 0; i < numberOfParts; i++)
            {
                if (_stopRequested)
                    return;

                _senderViewModel.SenderProgress = 1 + 99 * (i + 1) / numberOfParts;


                if (selectiveIDs != null && !selectiveIDs.Contains(i))
                    continue;

                string dataPart = dataParts[i];

                var dataPartWritableBitmap = QRSenderHelper.CreateQRWritableBitampFromString(dataPart);
                _senderViewModel.ImageSource = dataPartWritableBitmap;

                await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
            }
        }


        private static async Task SendQRMessageSettingsAsync(string settingsMessage)
        {
            var settingsWritableBitmap = QRSenderHelper.CreateQRWritableBitampFromString(settingsMessage);
            _senderViewModel.ImageSource = settingsWritableBitmap;

            await Task.Delay(QRSenderSettings.SendDelayMilliseconds);
        }
    }
}

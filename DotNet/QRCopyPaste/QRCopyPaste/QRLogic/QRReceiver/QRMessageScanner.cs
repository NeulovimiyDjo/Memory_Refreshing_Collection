using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using ZXing;

namespace QRCopyPaste
{
    public class QRMessageScanner
    {
        private static IReceiverViewModel _receiverViewModel;

        private static Stopwatch _stopwatchSinceLastSuccessfulQRRead = new Stopwatch();
        private static Dictionary<string, Dictionary<int, string>> _receivedItemsCache = new Dictionary<string, Dictionary<int, string>>();


        public QRMessageScanner(IReceiverViewModel receiverViewModel)
        {
            _receiverViewModel = receiverViewModel;
        }



        public async Task<QRPackageInfoMessage> TryWaitForQRPackageInfoMessageAsync()
        {
            var barcodeResult = await WaitForSuccessfullyDecodedQRAsync(QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds, int.MaxValue);
            var qrPackageInfoMessageStr = barcodeResult.Text;

            var qrPackageInfoMessage = TryDeserialize<QRPackageInfoMessage>(qrPackageInfoMessageStr);
            return qrPackageInfoMessage;
        }


        public async Task<Dictionary<int, string>> ScanDataParts_UntilAllReceived_OrTimeoutAsync(QRPackageInfoMessage qrPackageInfoMessage)
        {
            if (!_receivedItemsCache.ContainsKey(qrPackageInfoMessage.DataHash))
                _receivedItemsCache.Add(qrPackageInfoMessage.DataHash, new Dictionary<int, string>());
            var dataParts = _receivedItemsCache[qrPackageInfoMessage.DataHash];

            _receiverViewModel.ReceiverProgress = 1 + 99 * dataParts.Count / qrPackageInfoMessage.NumberOfParts;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var maxScanTime = qrPackageInfoMessage.SenderDelay * (qrPackageInfoMessage.NumberOfParts + 1) * 3 / 2;

            _stopwatchSinceLastSuccessfulQRRead.Restart();
            while (dataParts.Count < qrPackageInfoMessage.NumberOfParts
                && stopwatch.ElapsedMilliseconds < maxScanTime
                && _stopwatchSinceLastSuccessfulQRRead.ElapsedMilliseconds < QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead)
            {
                await TryWaitForQRDataPartMessage_AndAddToDataPartsAsync(qrPackageInfoMessage, dataParts);
            }

            return dataParts;
        }


        private static async Task TryWaitForQRDataPartMessage_AndAddToDataPartsAsync(QRPackageInfoMessage qrPackageInfoMessage, Dictionary<int, string> dataParts)
        {
            var delay = qrPackageInfoMessage.SenderDelay * 1 / 7; // Scan has to be faster than the display time.
            var dataPartResult = await WaitForSuccessfullyDecodedQRAsync(delay, qrPackageInfoMessage.SenderDelay * 3 / 2);
            if (dataPartResult == null)
                return; ; // Couldn't read QR code or there were none (e.g. didn't recieve all parts but sender already stopped).

            var currentDataStr = dataPartResult.Text;
            var currentData = TryDeserialize<QRDataPartMessage>(currentDataStr);
            if (currentData == null || currentData.MsgIntegrity != Constants.QRDataPartMessageIntegrityCheckID)
                return; // Was not a QRDataPartMessage (or failed to deserialize for some other reason).

            if (!dataParts.ContainsKey(currentData.ID) && HashHelper.GetStringHash(currentData.Data) == currentData.DataHash)
            {
                dataParts.Add(currentData.ID, currentData.Data);
            }

            _receiverViewModel.ReceiverProgress = 1 + 99 * dataParts.Count / qrPackageInfoMessage.NumberOfParts;
        }


        private static async Task<Result> WaitForSuccessfullyDecodedQRAsync(int delay, int maxScanTime)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Result barcodeResult = null;
            while (barcodeResult == null && stopwatch.ElapsedMilliseconds < maxScanTime)
            {
                var bitmap = QRMessageScannerHelper.CreateBitmapFromScreen();
                barcodeResult = QRMessageScannerHelper.GetBarcodeResultFromQRBitmap(bitmap);
                await Task.Delay(delay);
            }

            if (barcodeResult != null)
                _stopwatchSinceLastSuccessfulQRRead.Restart();

            return barcodeResult;
        }


        private static TData TryDeserialize<TData>(string dataStr)
        {
            try
            {
                var data = JsonSerializer.Deserialize<TData>(dataStr);
                return data;
            }
            catch (JsonException)
            {
                return default;
            }
        }
    }
}

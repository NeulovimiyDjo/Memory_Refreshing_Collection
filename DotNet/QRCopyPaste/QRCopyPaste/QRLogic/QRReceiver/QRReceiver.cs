using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCopyPaste
{
    public class QRReceiver
    {
        private static bool _stopRequested = false;
        private static bool _isRunning = false;
        private static IReceiverViewModel _receiverViewModel;
        private static QRMessageScanner _qrMessageScanner;

        private static int _scanCycle = 0;
        private static Stopwatch _stopwatchSinceLastSuccessfulQRRead = new Stopwatch();
        private static Dictionary<string, Dictionary<int, string>> _receivedItemsCache = new Dictionary<string, Dictionary<int, string>>();


        public QRReceiver(IReceiverViewModel receiverViewModel)
        {
            _receiverViewModel = receiverViewModel;
            _qrMessageScanner = new QRMessageScanner(_receiverViewModel);
        }


        public bool StartScanning(Action<object> messageReceivedAction, Action<string> onErrorAction)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(() => RunScansUntilStopRequested(messageReceivedAction, onErrorAction));
                return true;
            }

            return false;
        }


        public static void ClearCache()
        {
            _receivedItemsCache.Clear();
            _receiverViewModel.ReceiverProgress = 0;
        }


        private async Task RunScansUntilStopRequested(Action<object> messageReceivedAction, Action<string> onErrorAction)
        {
            while (!_stopRequested)
            {
                try
                {
                    await ScanForFullMessageAsync(messageReceivedAction);
                }
                catch (Exception ex)
                {
                    onErrorAction(ex.Message);
                }
            }
        }


        private async Task ScanForFullMessageAsync(Action<object> messageReceivedAction)
        {
            var qrPackageInfoMessage = await _qrMessageScanner.TryWaitForQRPackageInfoMessageAsync();
            if (qrPackageInfoMessage == null || qrPackageInfoMessage.MsgIntegrity != Constants.QRPackageInfoMessageIntegrityCheckID)
                return; // Was not a QRPackageInfoMessage (or failed to deserialize for some other reason).

            _scanCycle++;
            _receiverViewModel.ScanCycle = _scanCycle;
            _receiverViewModel.ReceiverProgress = 1;

            var dataParts = await _qrMessageScanner.ScanDataParts_UntilAllReceived_OrTimeoutAsync(qrPackageInfoMessage);
            ThrowIfNotAllDataPartsReceived(qrPackageInfoMessage, dataParts);

            var fullDataStr = string.Join("", dataParts.ToList().OrderBy(p => p.Key).Select(p => p.Value));
            ThrowIfFullDataHashIsWrong(qrPackageInfoMessage, fullDataStr);

            var fullData = ConvertToInitialTypeFromString(fullDataStr, qrPackageInfoMessage.DataType);
            messageReceivedAction(fullData);
        }


        private static void ThrowIfNotAllDataPartsReceived(QRPackageInfoMessage qrPackageInfoMessage, Dictionary<int, string> dataParts)
        {
            if (dataParts.Count != qrPackageInfoMessage.NumberOfParts)
            {
                var idsNotReceived = new List<string>();
                for (int i = 0; i < qrPackageInfoMessage.NumberOfParts; i++)
                {
                    if (!dataParts.ContainsKey(i))
                        idsNotReceived.Add(i.ToString());
                }

                var missingPartsCount = qrPackageInfoMessage.NumberOfParts - dataParts.Count;
                throw new Exception(
                    $"Data was not fully received.\n" +
                    $"{dataParts.Count} out of {qrPackageInfoMessage.NumberOfParts} parts received.\n" +
                    $"{missingPartsCount} parts missing.\n" +
                    $"Missing IDs list:\n" +
                    string.Join(" ", idsNotReceived)
                );
            }
        }


        private static void ThrowIfFullDataHashIsWrong(QRPackageInfoMessage qrPackageInfoMessage, string fullDataStr)
        {
            var dataHash = HashHelper.GetStringHash(fullDataStr);
            if (dataHash != qrPackageInfoMessage.DataHash)
                throw new Exception($"Received data is incorrect. Hashes differ.");
        }


        private static object ConvertToInitialTypeFromString(string zippedDataStr, string dataType)
        {
            if (dataType == Constants.StringTypeName)
            {
                var unzippedDataBytes = GetUnzippedDataBytes(zippedDataStr);
                var unzippedDataStr = Encoding.UTF8.GetString(unzippedDataBytes);
                return unzippedDataStr;
            }
            else if (dataType == Constants.ByteArrayTypeName)
            {
                var unzippedDataBytes = GetUnzippedDataBytes(zippedDataStr);
                return unzippedDataBytes;
            }
            else
            {
                throw new Exception($"Unsupported data type {dataType} in {nameof(ConvertToInitialTypeFromString)}.");
            }
        }


        private static byte[] GetUnzippedDataBytes(string zippedDataStr)
        {
            var zippedDataBytesStream = new MemoryStream(Convert.FromBase64String(zippedDataStr));
            var unzippedDataBytesStream = new MemoryStream();
            GZip.Decompress(zippedDataBytesStream, unzippedDataBytesStream, true);

            var unzippedDataBytes = unzippedDataBytesStream.ToArray();
            return unzippedDataBytes;
        }
    }
}

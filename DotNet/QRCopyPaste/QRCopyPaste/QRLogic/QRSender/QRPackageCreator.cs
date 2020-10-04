using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace QRCopyPaste
{
    public static class QRPackageCreator
    {
        public static QRPackage CreateQRPackage<TData>(TData data)
        {
            string zippedStringDataToSend;
            string dataType;

            if (data is string unzippedDataStr)
            {
                var unzippedDataBytes = Encoding.UTF8.GetBytes(unzippedDataStr);
                zippedStringDataToSend = GetZippedStringDataToSend(unzippedDataBytes);
                dataType = Constants.StringTypeName;
            }
            else if (data is byte[] unzippedDataBytes)
            {
                zippedStringDataToSend = GetZippedStringDataToSend(unzippedDataBytes);
                dataType = Constants.ByteArrayTypeName;
            }
            else
            {
                throw new NotSupportedException($"Unsupported data type {data.GetType()} during {nameof(QRPackage)} creation.");
            }


            var qrMessagesPackage = CreateQRPackageFromString(zippedStringDataToSend, dataType);
            return qrMessagesPackage;
        }


        private static string GetZippedStringDataToSend(byte[] unzippedDataBytes)
        {
            var unzippedDataBytesStream = new MemoryStream(unzippedDataBytes);
            var zippedDataBytesStream = new MemoryStream();
            GZip.Compress(unzippedDataBytesStream, zippedDataBytesStream, true, 4096, 9);

            var zippedDataBytes = zippedDataBytesStream.ToArray();
            var zippedStringDataToSend = Convert.ToBase64String(zippedDataBytes);
            return zippedStringDataToSend;
        }


        private static QRPackage CreateQRPackageFromString(string data, string dataType)
        {
            var dataParts = SplitStringToChunks(data, QRSenderSettings.ChunkSize).ToArray();
            var dataPartsMessages = CreateQRDataPartsMessages(dataParts);

            var dataHash = HashHelper.GetStringHash(data);
            var settingsMessage = CreateQRSettingsMessage(dataPartsMessages, dataType, dataHash);

            var qrMessagesPackage = new QRPackage
            {
                QRPackageInfoMessage = settingsMessage,
                QRDataPartsMessages = dataPartsMessages,
            };

            return qrMessagesPackage;
        }


        private static string CreateQRSettingsMessage(string[] dataParts, string dataType, string dataHash)
        {
            var qrPackageInfoMessage = new QRPackageInfoMessage
            {
                MsgIntegrity = Constants.QRPackageInfoMessageIntegrityCheckID,
                NumberOfParts = dataParts.Length,
                SenderDelay = QRSenderSettings.SendDelayMilliseconds,
                DataType = dataType,
                DataHash = dataHash,
            };

            var qrPackageInfoMessageStr = JsonSerializer.Serialize(qrPackageInfoMessage);
            return qrPackageInfoMessageStr;
        }


        private static string[] CreateQRDataPartsMessages(string[] dataParts)
        {
            var dataPartsMessages = new List<string>();

            for (int i = 0; i < dataParts.Length; i++)
            {
                var qrDataPartsMessage = new QRDataPartMessage
                {
                    MsgIntegrity = Constants.QRDataPartMessageIntegrityCheckID,
                    ID = i,
                    Data = dataParts[i],
                    DataHash = HashHelper.GetStringHash(dataParts[i]),
                };

                var dataPartsMessage = JsonSerializer.Serialize(qrDataPartsMessage);
                dataPartsMessages.Add(dataPartsMessage);
            }

            return dataPartsMessages.ToArray();
        }


        private static IEnumerable<string> SplitStringToChunks(string fullStr, int chunkSize)
        {
            var numberOfChunks =
                fullStr.Length % chunkSize == 0
                ? fullStr.Length / chunkSize
                : fullStr.Length / chunkSize + 1;


            var chunks =
                Enumerable.Range(0, numberOfChunks)
                .Select(chunkNum =>
                {
                    int substringSize =
                        chunkNum * chunkSize + chunkSize > fullStr.Length
                        ? fullStr.Length % chunkSize
                        : chunkSize;

                    return fullStr.Substring(chunkNum * chunkSize, substringSize);
                });


            return chunks;
        }
    }
}

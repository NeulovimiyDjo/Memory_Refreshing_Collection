using System;
using System.Drawing;
using System.Windows;
using ZXing;

namespace QRCodeSender
{
	public class QRCodeCreator
	{
		public Bitmap GenerateQRCodeFromClipboard()
		{
			string text = Clipboard.GetText();
			if (!string.IsNullOrWhiteSpace(text))
			{
				return this.GenerateTextQRCode(text);
			}
			return null;
		}

		private Bitmap GenerateTextQRCode(string text)
		{
			return ((IBarcodeWriter)new BarcodeWriter
			{
				Options = 
				{
					Hints = 
					{
						{
							EncodeHintType.CHARACTER_SET,
							"UTF-8"
						}
					}
				},
				Format = BarcodeFormat.QR_CODE
			}).Write(text);
		}
	}
}

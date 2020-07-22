using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using ZXing;

namespace QRCodeLoader
{
	public class QRCodeWatcher
	{
		public event QRCodeWatcher.EventArgsMessageHandler MessageCallback = delegate(object s, EventArgsMessage e)
		{
		};

		public QRCodeWatcher(int pingMsec, SynchronizationContext mainThreadContext)
		{
			if (pingMsec <= 1)
			{
				throw new ArgumentException("Ping must be positive");
			}
			this.ping = pingMsec;
			this.mainThreadContext = mainThreadContext;
		}

		private void FillBitmapPool()
		{
			this.bitmapPool = new Dictionary<Screen, QRCodeWatcher.BitmapAndGraphics>();
			try
			{
				foreach (Screen screen in Screen.AllScreens)
				{
					Rectangle bounds = screen.Bounds;
					Bitmap bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, PixelFormat.Format32bppArgb);
					this.bitmapPool.Add(screen, new QRCodeWatcher.BitmapAndGraphics
					{
						Bitmap = bitmap,
						Graphics = Graphics.FromImage(bitmap)
					});
				}
			}
			catch (Exception exception)
			{
				this.logger.Error(exception, "Error filling bitmappool");
				this.MessageCallback(this, new EventArgsMessage("Error filling bitmappool, restart app"));
				throw;
			}
		}

		public async Task DetectBarcodeAsync(CancellationToken ct)
		{
			await Task.Factory.StartNew(delegate()
			{
				for (;;)
				{
					try
					{
						Thread.Sleep(this.ping);
						this.Detect();
						ct.ThrowIfCancellationRequested();
					}
					catch (Exception ex)
					{
						this.logger.Error(ex, "Error in detection cycle.");
						this.MessageCallback(this, new EventArgsMessage("Error: " + ex.ToString()));
					}
				}
			}, ct);
		}

		private void Detect()
		{
			this.CaptureScreens();
			foreach (KeyValuePair<Screen, QRCodeWatcher.BitmapAndGraphics> keyValuePair in this.bitmapPool)
			{
				string text;
				if (this.DetectTextBarcode(keyValuePair.Value.Bitmap, out text))
				{
					string textFromClipboard = this.GetTextFromClipboard();
					if (text != textFromClipboard)
					{
						this.CopyToClipboard(text);
						this.MessageCallback(this, new EventArgsMessage("Text was copied to clipboard."));
					}
				}
			}
		}

		private string GetTextFromClipboard()
		{
			string result = "";
			this.mainThreadContext.Send(delegate(object o)
			{
				result = Clipboard.GetText();
			}, null);
			return result;
		}

		private void CopyToClipboard(string text)
		{
			this.mainThreadContext.Send(delegate(object o)
			{
				Clipboard.SetData(DataFormats.Text, text);
			}, null);
		}

		private bool DetectTextBarcode(Bitmap image, out string result)
		{
			result = string.Empty;
			if (image == null)
			{
				return false;
			}
			IBarcodeReader barcodeReader = new BarcodeReader();
			Result result2;
			try
			{
				result2 = barcodeReader.Decode(image);
			}
			catch
			{
				return false;
			}
			if (result2 != null && result2.BarcodeFormat == BarcodeFormat.QR_CODE)
			{
				result = result2.Text;
				return true;
			}
			return false;
		}

		private void CaptureScreens()
		{
			if (this.bitmapPool == null)
			{
				this.bitmapPool = new Dictionary<Screen, QRCodeWatcher.BitmapAndGraphics>();
			}
			List<Screen> list = new List<Screen>();
			foreach (KeyValuePair<Screen, QRCodeWatcher.BitmapAndGraphics> keyValuePair in this.bitmapPool)
			{
				if (!Screen.AllScreens.Contains(keyValuePair.Key))
				{
					list.Add(keyValuePair.Key);
				}
			}
			foreach (Screen key in list)
			{
				this.bitmapPool.Remove(key);
			}
			foreach (Screen screen in Screen.AllScreens)
			{
				try
				{
					if (!this.bitmapPool.ContainsKey(screen))
					{
						Rectangle bounds = screen.Bounds;
						Bitmap bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, PixelFormat.Format32bppArgb);
						this.bitmapPool.Add(screen, new QRCodeWatcher.BitmapAndGraphics
						{
							Bitmap = bitmap,
							Graphics = Graphics.FromImage(bitmap)
						});
					}
					Rectangle bounds2 = screen.Bounds;
					Bitmap bitmap2 = this.bitmapPool[screen].Bitmap;
					this.bitmapPool[screen].Graphics.CopyFromScreen(bounds2.Left, bounds2.Top, 0, 0, bounds2.Size);
				}
				catch (Win32Exception)
				{
				}
			}
		}

		private int ping = 1000;
		private Logger logger = LogManager.GetCurrentClassLogger();
		private SynchronizationContext mainThreadContext;
		private Dictionary<Screen, QRCodeWatcher.BitmapAndGraphics> bitmapPool;
		public delegate void EventArgsMessageHandler(object sender, EventArgsMessage e);

		private struct BitmapAndGraphics
		{
			public Bitmap Bitmap { get; set; }
			public Graphics Graphics { get; set; }
		}
	}
}

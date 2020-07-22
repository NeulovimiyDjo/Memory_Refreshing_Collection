using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QRCodeSender.Properties;

namespace QRCodeSender
{
	public class QRCodeWindowViewModel : INotifyPropertyChanged
	{
		public event QRCodeWindowViewModel.EventArgsMessageHandler ReportError = delegate(object s, EventArgsMessage e)
		{
		};

		public Window Parent { get; set; }
		public SynchronizationContext ParentContext { get; set; }

		public ICommand GenerateQRCode
		{
			get
			{
				DelegateCommand delegateCommand = new DelegateCommand();
				delegateCommand.CommandAction = delegate()
				{
					this.GenerateQRCodeInternal();
				};
				delegateCommand.CanExecuteFunc = (() => Application.Current.MainWindow != null);
				return delegateCommand;
			}
		}

		public ICommand ShowBriefly
		{
			get
			{
				DelegateCommand delegateCommand = new DelegateCommand();
				delegateCommand.CommandAction = delegate()
				{
					this.ShowWindowForDuration(this.Parent, this.ParentContext, Settings.Default.Ping);
				};
				delegateCommand.CanExecuteFunc = (() => Application.Current.MainWindow != null);
				return delegateCommand;
			}
		}

		public ImageSource ImageSource
		{
			get
			{
				return this.imageSource;
			}
			set
			{
				this.imageSource = value;
				this.ThisPropertyChanged("ImageSource");
			}
		}

		private void ShowWindowForDuration(Window window, SynchronizationContext context, int duration)
		{
			SendOrPostCallback c1 = null;
			SendOrPostCallback c2 = null;
			Task.Factory.StartNew(delegate()
			{
				SynchronizationContext context2 = context;
				SendOrPostCallback d;
				if ((d = c1) == null)
				{
					d = (c1 = delegate(object o)
					{
						window.Show();
					});
				}
				context2.Send(d, null);
				Thread.Sleep(duration);
				SynchronizationContext context3 = context;
				SendOrPostCallback d2;
				if ((d2 = c2) == null)
				{
					d2 = (c2 = delegate(object o)
					{
						window.Hide();
					});
				}
				context3.Send(d2, null);
			});
		}

		private void GenerateQRCodeInternal()
		{
			QRCodeCreator qrcodeCreator = new QRCodeCreator();
			Bitmap bitmap = null;
			try
			{
				bitmap = qrcodeCreator.GenerateQRCodeFromClipboard();
			}
			catch (Exception ex)
			{
				this.ReportError(this, new EventArgsMessage("Ошибка генерации QR Кода: " + ex.Message));
			}
			if (bitmap != null)
			{
				this.ImageSource = this.BitmapToImageSource(bitmap);
			}
		}

		private BitmapImage BitmapToImageSource(Bitmap bitmap)
		{
			BitmapImage result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				bitmap.Save(memoryStream, ImageFormat.Bmp);
				memoryStream.Position = 0L;
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memoryStream;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				result = bitmapImage;
			}
			return result;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected void ThisPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private ImageSource imageSource;
		public delegate void EventArgsMessageHandler(object sender, EventArgsMessage e);
	}
}

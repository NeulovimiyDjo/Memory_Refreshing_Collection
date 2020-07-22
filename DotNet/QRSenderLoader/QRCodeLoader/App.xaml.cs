using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using QRCodeLoader.Properties;

namespace QRCodeLoader
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			this.notifyIcon = (TaskbarIcon)base.FindResource("NotifyIcon");
			QRCodeWatcher qrcodeWatcher = new QRCodeWatcher(Settings.Default.Ping, SynchronizationContext.Current);
			qrcodeWatcher.MessageCallback += delegate(object sender, EventArgsMessage message)
			{
				this.notifyIcon.ShowBalloonTip("QRCode", message.Text, BalloonIcon.Info);
			};
			qrcodeWatcher.DetectBarcodeAsync(this.cts.Token);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.cts.Cancel();
			this.notifyIcon.Dispose();
			base.OnExit(e);
		}

		private TaskbarIcon notifyIcon;

		private CancellationTokenSource cts = new CancellationTokenSource();
	}
}

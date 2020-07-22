using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace QRCodeSender
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			QRCodeWindow qrcodeWindow = new QRCodeWindow();
			base.MainWindow = qrcodeWindow;
			qrcodeWindow.Show();
			qrcodeWindow.Hide();
			base.OnStartup(e);
			this.notifyIcon = (TaskbarIcon)base.FindResource("NotifyIcon");
			qrcodeWindow.ViewModel.ReportError += delegate(object sender, EventArgsMessage message)
			{
				this.notifyIcon.ShowBalloonTip("QR Code", message.Text, BalloonIcon.Info);
			};
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

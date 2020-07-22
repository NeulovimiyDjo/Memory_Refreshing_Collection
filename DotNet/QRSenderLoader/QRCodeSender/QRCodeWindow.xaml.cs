using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using QRCodeSender.Properties;

namespace QRCodeSender
{
	public partial class QRCodeWindow : Window
	{
		public QRCodeWindow()
		{
			this.InitializeComponent();
			RenderOptions.SetCachingHint(this.QRCode, CachingHint.Cache);
			RenderOptions.SetBitmapScalingMode(this.QRCode, BitmapScalingMode.NearestNeighbor);
			this.VM.Parent = this;
			this.VM.ParentContext = SynchronizationContext.Current;
		}

		public QRCodeWindowViewModel ViewModel
		{
			get
			{
				return this.VM;
			}
		}

		private void OnHotKeyPressed()
		{
			this.VM.GenerateQRCode.Execute(null);
			this.VM.ShowBriefly.Execute(null);
		}

		[DllImport("User32.dll")]
		private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

		[DllImport("User32.dll")]
		private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
			this._source = HwndSource.FromHwnd(windowInteropHelper.Handle);
			this._source.AddHook(new HwndSourceHook(this.HwndHook));
			this.RegisterHotKey();
		}

		protected override void OnClosed(EventArgs e)
		{
			this._source.RemoveHook(new HwndSourceHook(this.HwndHook));
			this._source = null;
			this.UnregisterHotKey();
			base.OnClosed(e);
		}

		private void RegisterHotKey()
		{
			WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
			uint hotKeyModifier = (uint)Settings.Default.HotKeyModifier;
			uint hotKey = (uint)Settings.Default.HotKey;
			var r = QRCodeWindow.RegisterHotKey(windowInteropHelper.Handle, 9000, hotKeyModifier, hotKey);
		}

		private void UnregisterHotKey()
		{
			QRCodeWindow.UnregisterHotKey(new WindowInteropHelper(this).Handle, 9000);
		}

		private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == 786 && wParam.ToInt32() == 9000)
			{
				this.OnHotKeyPressed();
				handled = true;
			}
			return IntPtr.Zero;
		}

		private HwndSource _source;
		private const int HOTKEY_ID = 9000;
	}
}

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;

namespace QRCodeSender
{
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			this.InitializeComponent();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.VM.CloseCommand.Execute(null);
		}
	}
}

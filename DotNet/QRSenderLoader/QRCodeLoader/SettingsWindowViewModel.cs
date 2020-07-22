using System;
using System.Windows;
using System.Windows.Input;
using QRCodeLoader.Properties;

namespace QRCodeLoader
{
	public class SettingsWindowViewModel
	{
		public string Ping
		{
			get
			{
				return Settings.Default.Ping.ToString();
			}
			set
			{
				Settings.Default.Ping = int.Parse(value);
			}
		}

		public ICommand CloseCommand
		{
			get
			{
				DelegateCommand delegateCommand = new DelegateCommand();
				delegateCommand.CommandAction = delegate()
				{
					this.SaveSettings();
				};
				delegateCommand.CanExecuteFunc = (() => Application.Current.MainWindow != null);
				return delegateCommand;
			}
		}

		private void SaveSettings()
		{
			Settings.Default.Save();
		}
	}
}

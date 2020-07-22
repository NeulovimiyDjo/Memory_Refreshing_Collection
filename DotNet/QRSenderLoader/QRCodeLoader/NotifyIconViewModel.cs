using System;
using System.Windows;
using System.Windows.Input;

namespace QRCodeLoader
{
	public class NotifyIconViewModel
	{
		public ICommand ShowWindowCommand
		{
			get
			{
				DelegateCommand delegateCommand = new DelegateCommand();
				delegateCommand.CanExecuteFunc = (() => Application.Current.MainWindow == null);
				delegateCommand.CommandAction = delegate()
				{
					Application.Current.MainWindow = new SettingsWindow();
					Application.Current.MainWindow.Show();
				};
				return delegateCommand;
			}
		}

		public ICommand ExitApplicationCommand
		{
			get
			{
				DelegateCommand delegateCommand = new DelegateCommand();
				delegateCommand.CommandAction = delegate()
				{
					Application.Current.Shutdown();
				};
				return delegateCommand;
			}
		}
	}
}

﻿using System;
using System.Windows;
using System.Windows.Input;

namespace QRCodeSender
{
	public class NotifyIconViewModel
	{
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

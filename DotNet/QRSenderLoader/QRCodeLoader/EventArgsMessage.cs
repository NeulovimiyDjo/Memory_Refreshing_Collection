using System;

namespace QRCodeLoader
{
	public class EventArgsMessage : EventArgs
	{
		public string Text { get; private set; }

		public EventArgsMessage(string text)
		{
			this.Text = text;
		}
	}
}

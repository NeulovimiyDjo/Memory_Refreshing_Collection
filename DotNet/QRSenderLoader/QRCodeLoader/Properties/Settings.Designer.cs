using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace QRCodeLoader.Properties
{
	// Token: 0x0200000A RID: 10
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.4.0.0")]
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600002E RID: 46 RVA: 0x000028CF File Offset: 0x00000ACF
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600002F RID: 47 RVA: 0x000028D6 File Offset: 0x00000AD6
		// (set) Token: 0x06000030 RID: 48 RVA: 0x000028E8 File Offset: 0x00000AE8
		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("500")]
		public int Ping
		{
			get
			{
				return (int)this["Ping"];
			}
			set
			{
				this["Ping"] = value;
			}
		}

		// Token: 0x04000010 RID: 16
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}

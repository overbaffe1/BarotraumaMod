using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004EE RID: 1262
	internal sealed class ModsControlsSettingsMenu : ModsSettingsMenuBase
	{
		// Token: 0x0600522D RID: 21037 RVA: 0x002C1984 File Offset: 0x002BFB84
		public ModsControlsSettingsMenu(GUIFrame contentFrame, IPackageManagementService packageManagementService, IConfigService configService, SettingsMenu settingsMenuInstance) : base(contentFrame, packageManagementService, configService, settingsMenuInstance)
		{
		}

		// Token: 0x0600522E RID: 21038 RVA: 0x002C1991 File Offset: 0x002BFB91
		protected override void DisposeInternal()
		{
		}

		// Token: 0x0600522F RID: 21039 RVA: 0x002C1993 File Offset: 0x002BFB93
		public override void ApplyInstalledModChanges()
		{
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Xml.Linq;
using Barotrauma.LuaCs.Data;
using OneOf;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F0 RID: 1264
	internal abstract class ModsSettingsMenuBase : IDisposable
	{
		// Token: 0x170014D8 RID: 5336
		// (get) Token: 0x06005239 RID: 21049 RVA: 0x002C213B File Offset: 0x002C033B
		// (set) Token: 0x0600523A RID: 21050 RVA: 0x002C2143 File Offset: 0x002C0343
		public GUIFrame ContentFrame { get; private set; }

		// Token: 0x170014D9 RID: 5337
		// (get) Token: 0x0600523B RID: 21051 RVA: 0x002C214C File Offset: 0x002C034C
		// (set) Token: 0x0600523C RID: 21052 RVA: 0x002C2154 File Offset: 0x002C0354
		private protected IPackageManagementService PackageManagementService { protected get; private set; }

		// Token: 0x170014DA RID: 5338
		// (get) Token: 0x0600523D RID: 21053 RVA: 0x002C215D File Offset: 0x002C035D
		// (set) Token: 0x0600523E RID: 21054 RVA: 0x002C2165 File Offset: 0x002C0365
		private protected IConfigService ConfigService { protected get; private set; }

		// Token: 0x170014DB RID: 5339
		// (get) Token: 0x0600523F RID: 21055 RVA: 0x002C216E File Offset: 0x002C036E
		// (set) Token: 0x06005240 RID: 21056 RVA: 0x002C2176 File Offset: 0x002C0376
		private protected SettingsMenu SettingsMenuInstance { protected get; private set; }

		// Token: 0x06005241 RID: 21057 RVA: 0x002C217F File Offset: 0x002C037F
		protected ModsSettingsMenuBase(GUIFrame contentFrame, IPackageManagementService packageManagementService, IConfigService configService, SettingsMenu settingsMenuInstance)
		{
			this.ContentFrame = contentFrame;
			this.PackageManagementService = packageManagementService;
			this.ConfigService = configService;
			this.SettingsMenuInstance = settingsMenuInstance;
		}

		// Token: 0x06005242 RID: 21058
		protected abstract void DisposeInternal();

		// Token: 0x06005243 RID: 21059
		public abstract void ApplyInstalledModChanges();

		// Token: 0x06005244 RID: 21060 RVA: 0x002C21B0 File Offset: 0x002C03B0
		public void Dispose()
		{
			this.DisposeInternal();
			GUIFrame contentFrame = this.ContentFrame;
			if (contentFrame != null)
			{
				contentFrame.Parent.RemoveChild(this.ContentFrame);
			}
			this.SettingsMenuInstance = null;
			this.ContentFrame = null;
			this.PackageManagementService = null;
			this.ConfigService = null;
			this.NewValuesCache.Clear();
		}

		// Token: 0x04002B9F RID: 11167
		protected readonly ConcurrentDictionary<ISettingBase, OneOf<string, XElement>> NewValuesCache = new ConcurrentDictionary<ISettingBase, OneOf<string, XElement>>();
	}
}

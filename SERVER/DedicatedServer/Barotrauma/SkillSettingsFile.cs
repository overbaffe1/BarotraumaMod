using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200014B RID: 331
	internal sealed class SkillSettingsFile : ContentFile
	{
		// Token: 0x06001C52 RID: 7250 RVA: 0x000CEE29 File Offset: 0x000CD029
		public SkillSettingsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C53 RID: 7251 RVA: 0x000CEE34 File Offset: 0x000CD034
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			ContentXElement mainElement = doc.Root.FromPackage(this.ContentPackage);
			bool allowOverriding = mainElement.IsOverride();
			if (allowOverriding)
			{
				mainElement = mainElement.FirstElement();
			}
			SkillSettings prefab = new SkillSettings(mainElement, this);
			SkillSettings.Prefabs.Add(prefab, allowOverriding);
		}

		// Token: 0x06001C54 RID: 7252 RVA: 0x000CEE8D File Offset: 0x000CD08D
		public override void UnloadFile()
		{
			SkillSettings.Prefabs.RemoveByFile(this, null);
		}

		// Token: 0x06001C55 RID: 7253 RVA: 0x000CEE9B File Offset: 0x000CD09B
		public override void Sort()
		{
			SkillSettings.Prefabs.Sort();
		}
	}
}

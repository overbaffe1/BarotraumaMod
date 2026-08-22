using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000241 RID: 577
	internal sealed class SkillSettingsFile : ContentFile
	{
		// Token: 0x06003736 RID: 14134 RVA: 0x00214979 File Offset: 0x00212B79
		public SkillSettingsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003737 RID: 14135 RVA: 0x00214984 File Offset: 0x00212B84
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

		// Token: 0x06003738 RID: 14136 RVA: 0x002149DD File Offset: 0x00212BDD
		public override void UnloadFile()
		{
			SkillSettings.Prefabs.RemoveByFile(this, null);
		}

		// Token: 0x06003739 RID: 14137 RVA: 0x002149EB File Offset: 0x00212BEB
		public override void Sort()
		{
			SkillSettings.Prefabs.Sort();
		}
	}
}

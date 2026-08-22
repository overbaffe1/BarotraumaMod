using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000139 RID: 313
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class JobsFile : ContentFile
	{
		// Token: 0x06001C07 RID: 7175 RVA: 0x000CE1E5 File Offset: 0x000CC3E5
		public JobsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C08 RID: 7176 RVA: 0x000CE1F0 File Offset: 0x000CC3F0
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			this.LoadElements(doc.Root.FromPackage(this.ContentPackage), false);
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x000CE228 File Offset: 0x000CC428
		private void LoadElements(ContentXElement mainElement, bool isOverride)
		{
			foreach (ContentXElement element in mainElement.Elements())
			{
				Identifier identifier = element.NameAsIdentifier();
				if (identifier == "ItemRepairPriorities")
				{
					using (IEnumerator<ContentXElement> enumerator2 = element.Elements().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							ContentXElement subElement = enumerator2.Current;
							ItemRepairPriority prio = new ItemRepairPriority(subElement, this);
							ItemRepairPriority.Prefabs.Add(prio, isOverride);
						}
						continue;
					}
				}
				if (element.IsOverride())
				{
					this.LoadElements(element, true);
				}
				else
				{
					JobPrefab job = new JobPrefab(element, this);
					JobPrefab.Prefabs.Add(job, isOverride);
				}
			}
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x000CE304 File Offset: 0x000CC504
		public override void UnloadFile()
		{
			JobPrefab.Prefabs.RemoveByFile(this);
			ItemRepairPriority.Prefabs.RemoveByFile(this);
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x000CE31C File Offset: 0x000CC51C
		public override void Sort()
		{
			JobPrefab.Prefabs.SortAll();
		}
	}
}

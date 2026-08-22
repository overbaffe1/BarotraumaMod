using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200022F RID: 559
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class JobsFile : ContentFile
	{
		// Token: 0x060036E6 RID: 14054 RVA: 0x00213C7D File Offset: 0x00211E7D
		public JobsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036E7 RID: 14055 RVA: 0x00213C88 File Offset: 0x00211E88
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			this.LoadElements(doc.Root.FromPackage(this.ContentPackage), false);
		}

		// Token: 0x060036E8 RID: 14056 RVA: 0x00213CC0 File Offset: 0x00211EC0
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

		// Token: 0x060036E9 RID: 14057 RVA: 0x00213D9C File Offset: 0x00211F9C
		public override void UnloadFile()
		{
			JobPrefab.Prefabs.RemoveByFile(this);
			ItemRepairPriority.Prefabs.RemoveByFile(this);
		}

		// Token: 0x060036EA RID: 14058 RVA: 0x00213DB4 File Offset: 0x00211FB4
		public override void Sort()
		{
			JobPrefab.Prefabs.SortAll();
		}
	}
}

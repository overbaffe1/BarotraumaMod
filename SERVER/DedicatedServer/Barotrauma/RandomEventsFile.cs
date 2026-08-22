using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000148 RID: 328
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class RandomEventsFile : ContentFile
	{
		// Token: 0x06001C45 RID: 7237 RVA: 0x000CEA51 File Offset: 0x000CCC51
		public RandomEventsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C46 RID: 7238 RVA: 0x000CEA5C File Offset: 0x000CCC5C
		public void LoadFromXElement(ContentXElement parentElement, bool overriding)
		{
			Identifier elemName = new Identifier(parentElement.Name.ToString());
			if (parentElement.IsOverride())
			{
				using (IEnumerator<ContentXElement> enumerator = parentElement.Elements().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentXElement element = enumerator.Current;
						this.LoadFromXElement(element, true);
					}
					return;
				}
			}
			if (elemName == "randomevents")
			{
				using (IEnumerator<ContentXElement> enumerator2 = parentElement.Elements().GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ContentXElement element2 = enumerator2.Current;
						this.LoadFromXElement(element2, overriding);
					}
					return;
				}
			}
			if (elemName == "eventprefabs")
			{
				using (IEnumerator<ContentXElement> enumerator3 = parentElement.Elements().GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						ContentXElement subElement = enumerator3.Current;
						Identifier fallbackIdentifier = subElement.NameAsIdentifier();
						if (fallbackIdentifier == "traitorevent")
						{
							ContentXElement element3 = subElement;
							fallbackIdentifier = default(Identifier);
							TraitorEventPrefab prefab = new TraitorEventPrefab(element3, this, fallbackIdentifier);
							EventPrefab.Prefabs.Add(prefab, overriding);
						}
						else
						{
							ContentXElement element4 = subElement;
							fallbackIdentifier = default(Identifier);
							EventPrefab prefab2 = new EventPrefab(element4, this, fallbackIdentifier);
							EventPrefab.Prefabs.Add(prefab2, overriding);
						}
					}
					return;
				}
			}
			if (!(elemName == "eventsprites"))
			{
				if (elemName == "eventset")
				{
					EventSet prefab3 = new EventSet(parentElement, this, null);
					EventSet.Prefabs.Add(prefab3, overriding);
					return;
				}
				if (elemName == "clear")
				{
					EventPrefab.Prefabs.AddOverrideFile(this);
					EventSet.Prefabs.AddOverrideFile(this);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 3);
				defaultInterpolatedStringHandler.AppendLiteral("RandomEventsFile: Invalid ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
				defaultInterpolatedStringHandler.AppendLiteral(" element: ");
				defaultInterpolatedStringHandler.AppendFormatted<XName>(parentElement.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" in ");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, parentElement.ContentPackage, false, false);
			}
		}

		// Token: 0x06001C47 RID: 7239 RVA: 0x000CEC8C File Offset: 0x000CCE8C
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			ContentXElement rootElement = doc.Root.FromPackage(this.ContentPackage);
			this.LoadFromXElement(rootElement, false);
			EventSet.RefreshAllEventPrefabs();
		}

		// Token: 0x06001C48 RID: 7240 RVA: 0x000CECC8 File Offset: 0x000CCEC8
		public override void UnloadFile()
		{
			EventPrefab.Prefabs.RemoveByFile(this);
			EventSet.Prefabs.RemoveByFile(this);
			EventSet.RefreshAllEventPrefabs();
		}

		// Token: 0x06001C49 RID: 7241 RVA: 0x000CECE5 File Offset: 0x000CCEE5
		public override void Sort()
		{
			EventPrefab.Prefabs.SortAll();
			EventSet.Prefabs.SortAll();
			EventSet.RefreshAllEventPrefabs();
		}
	}
}

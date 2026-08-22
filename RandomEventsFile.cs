using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200023E RID: 574
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class RandomEventsFile : ContentFile
	{
		// Token: 0x06003729 RID: 14121 RVA: 0x0021452A File Offset: 0x0021272A
		public RandomEventsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x0600372A RID: 14122 RVA: 0x00214534 File Offset: 0x00212734
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
			if (elemName == "eventsprites")
			{
				using (IEnumerator<ContentXElement> enumerator4 = parentElement.Elements().GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						ContentXElement subElement2 = enumerator4.Current;
						EventSprite prefab3 = new EventSprite(subElement2, this);
						EventSprite.Prefabs.Add(prefab3, overriding);
					}
					return;
				}
			}
			if (elemName == "eventset")
			{
				EventSet prefab4 = new EventSet(parentElement, this, null);
				EventSet.Prefabs.Add(prefab4, overriding);
				return;
			}
			if (elemName == "clear")
			{
				EventPrefab.Prefabs.AddOverrideFile(this);
				EventSet.Prefabs.AddOverrideFile(this);
				EventSprite.Prefabs.AddOverrideFile(this);
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

		// Token: 0x0600372B RID: 14123 RVA: 0x002147C4 File Offset: 0x002129C4
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

		// Token: 0x0600372C RID: 14124 RVA: 0x00214800 File Offset: 0x00212A00
		public override void UnloadFile()
		{
			EventPrefab.Prefabs.RemoveByFile(this);
			EventSet.Prefabs.RemoveByFile(this);
			EventSet.RefreshAllEventPrefabs();
			EventSprite.Prefabs.RemoveByFile(this);
		}

		// Token: 0x0600372D RID: 14125 RVA: 0x00214828 File Offset: 0x00212A28
		public override void Sort()
		{
			EventPrefab.Prefabs.SortAll();
			EventSet.Prefabs.SortAll();
			EventSet.RefreshAllEventPrefabs();
			EventSprite.Prefabs.SortAll();
		}
	}
}

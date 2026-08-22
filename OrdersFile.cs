using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000238 RID: 568
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class OrdersFile : ContentFile
	{
		// Token: 0x06003713 RID: 14099 RVA: 0x00214271 File Offset: 0x00212471
		public OrdersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003714 RID: 14100 RVA: 0x0021427C File Offset: 0x0021247C
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
			if (elemName == "order")
			{
				OrderPrefab prefab = new OrderPrefab(parentElement, this);
				OrderPrefab.Prefabs.Add(prefab, overriding);
				return;
			}
			if (elemName == "ordercategory")
			{
				OrderCategoryIcon prefab2 = new OrderCategoryIcon(parentElement, this);
				OrderCategoryIcon.OrderCategoryIcons.Add(prefab2, overriding);
				return;
			}
			if (elemName == "orders")
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
			if (elemName == "clear")
			{
				OrderCategoryIcon.OrderCategoryIcons.AddOverrideFile(this);
				OrderPrefab.Prefabs.AddOverrideFile(this);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
			defaultInterpolatedStringHandler.AppendLiteral("OrdersFile: Invalid ");
			defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral(" element: ");
			defaultInterpolatedStringHandler.AppendFormatted<XName>(parentElement.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" in ");
			defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, (parentElement != null) ? parentElement.ContentPackage : null, false, false);
		}

		// Token: 0x06003715 RID: 14101 RVA: 0x00214420 File Offset: 0x00212620
		public sealed override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			ContentXElement rootElement = doc.Root.FromPackage(this.ContentPackage);
			this.LoadFromXElement(rootElement, false);
		}

		// Token: 0x06003716 RID: 14102 RVA: 0x00214457 File Offset: 0x00212657
		public sealed override void UnloadFile()
		{
			OrderCategoryIcon.OrderCategoryIcons.RemoveByFile(this);
			OrderPrefab.Prefabs.RemoveByFile(this);
		}

		// Token: 0x06003717 RID: 14103 RVA: 0x0021446F File Offset: 0x0021266F
		public sealed override void Sort()
		{
			OrderCategoryIcon.OrderCategoryIcons.SortAll();
			OrderPrefab.Prefabs.SortAll();
		}
	}
}

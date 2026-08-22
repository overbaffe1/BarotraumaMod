using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000142 RID: 322
	[RequiredByCorePackage(new Type[]
	{

	})]
	internal sealed class OrdersFile : ContentFile
	{
		// Token: 0x06001C34 RID: 7220 RVA: 0x000CE7D9 File Offset: 0x000CC9D9
		public OrdersFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C35 RID: 7221 RVA: 0x000CE7E4 File Offset: 0x000CC9E4
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

		// Token: 0x06001C36 RID: 7222 RVA: 0x000CE988 File Offset: 0x000CCB88
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

		// Token: 0x06001C37 RID: 7223 RVA: 0x000CE9BF File Offset: 0x000CCBBF
		public sealed override void UnloadFile()
		{
			OrderCategoryIcon.OrderCategoryIcons.RemoveByFile(this);
			OrderPrefab.Prefabs.RemoveByFile(this);
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x000CE9D7 File Offset: 0x000CCBD7
		public sealed override void Sort()
		{
			OrderCategoryIcon.OrderCategoryIcons.SortAll();
			OrderPrefab.Prefabs.SortAll();
		}
	}
}

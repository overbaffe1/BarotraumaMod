using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000135 RID: 309
	internal abstract class GenericPrefabFile<T> : ContentFile where T : Prefab
	{
		// Token: 0x06001BF2 RID: 7154 RVA: 0x000CDF88 File Offset: 0x000CC188
		protected GenericPrefabFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BF3 RID: 7155
		protected abstract bool MatchesSingular(Identifier identifier);

		// Token: 0x06001BF4 RID: 7156
		protected abstract bool MatchesPlural(Identifier identifier);

		// Token: 0x17000827 RID: 2087
		// (get) Token: 0x06001BF5 RID: 7157
		protected abstract PrefabCollection<T> Prefabs { get; }

		// Token: 0x06001BF6 RID: 7158
		protected abstract T CreatePrefab(ContentXElement element);

		// Token: 0x06001BF7 RID: 7159 RVA: 0x000CDF94 File Offset: 0x000CC194
		private void LoadFromXElement(ContentXElement parentElement, bool overriding)
		{
			Identifier elemName = parentElement.NameAsIdentifier();
			IEnumerable<ContentXElement> childElements = parentElement.Elements();
			if (parentElement.IsOverride())
			{
				using (IEnumerator<ContentXElement> enumerator = childElements.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentXElement element = enumerator.Current;
						this.LoadFromXElement(element, true);
					}
					return;
				}
			}
			if (elemName == "clear")
			{
				this.Prefabs.AddOverrideFile(this);
				return;
			}
			if (this.MatchesSingular(elemName))
			{
				T prefab = this.CreatePrefab(parentElement);
				try
				{
					this.Prefabs.Add(prefab, overriding);
					return;
				}
				catch
				{
					prefab.Dispose();
					this.Prefabs.Remove(prefab);
					throw;
				}
			}
			if (this.MatchesPlural(elemName))
			{
				using (IEnumerator<ContentXElement> enumerator2 = childElements.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ContentXElement element2 = enumerator2.Current;
						this.LoadFromXElement(element2, overriding);
					}
					return;
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 3);
			defaultInterpolatedStringHandler.AppendLiteral("GenericPrefabFile: Invalid ");
			defaultInterpolatedStringHandler.AppendFormatted(base.GetType().Name);
			defaultInterpolatedStringHandler.AppendLiteral(" element: ");
			defaultInterpolatedStringHandler.AppendFormatted<XName>(parentElement.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" in ");
			defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.Path);
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.ContentPackage, false, false);
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x000CE118 File Offset: 0x000CC318
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

		// Token: 0x06001BF9 RID: 7161 RVA: 0x000CE14F File Offset: 0x000CC34F
		public sealed override void UnloadFile()
		{
			this.Prefabs.RemoveByFile(this);
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x000CE15D File Offset: 0x000CC35D
		public sealed override void Sort()
		{
			this.Prefabs.SortAll();
		}
	}
}

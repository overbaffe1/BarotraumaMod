using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200022B RID: 555
	internal abstract class GenericPrefabFile<T> : ContentFile where T : Prefab
	{
		// Token: 0x060036D1 RID: 14033 RVA: 0x00213A20 File Offset: 0x00211C20
		protected GenericPrefabFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036D2 RID: 14034
		protected abstract bool MatchesSingular(Identifier identifier);

		// Token: 0x060036D3 RID: 14035
		protected abstract bool MatchesPlural(Identifier identifier);

		// Token: 0x17000E88 RID: 3720
		// (get) Token: 0x060036D4 RID: 14036
		protected abstract PrefabCollection<T> Prefabs { get; }

		// Token: 0x060036D5 RID: 14037
		protected abstract T CreatePrefab(ContentXElement element);

		// Token: 0x060036D6 RID: 14038 RVA: 0x00213A2C File Offset: 0x00211C2C
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

		// Token: 0x060036D7 RID: 14039 RVA: 0x00213BB0 File Offset: 0x00211DB0
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

		// Token: 0x060036D8 RID: 14040 RVA: 0x00213BE7 File Offset: 0x00211DE7
		public sealed override void UnloadFile()
		{
			this.Prefabs.RemoveByFile(this);
		}

		// Token: 0x060036D9 RID: 14041 RVA: 0x00213BF5 File Offset: 0x00211DF5
		public sealed override void Sort()
		{
			this.Prefabs.SortAll();
		}
	}
}

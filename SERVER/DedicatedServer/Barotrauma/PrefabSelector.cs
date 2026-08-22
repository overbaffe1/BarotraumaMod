using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.Threading;

namespace Barotrauma
{
	// Token: 0x02000275 RID: 629
	[NullableContext(1)]
	[Nullable(0)]
	public class PrefabSelector<T> : IEnumerable<!0>, IEnumerable where T : Prefab
	{
		// Token: 0x17000D59 RID: 3417
		// (get) Token: 0x06002CE6 RID: 11494 RVA: 0x0012834C File Offset: 0x0012654C
		[Nullable(2)]
		public T BasePrefab
		{
			[NullableContext(2)]
			get
			{
				T result;
				using (new ReadLock(this.rwl))
				{
					result = this.basePrefabInternal;
				}
				return result;
			}
		}

		// Token: 0x17000D5A RID: 3418
		// (get) Token: 0x06002CE7 RID: 11495 RVA: 0x00128388 File Offset: 0x00126588
		[Nullable(2)]
		public T ActivePrefab
		{
			[NullableContext(2)]
			get
			{
				T activePrefabInternal;
				using (new ReadLock(this.rwl))
				{
					activePrefabInternal = this.activePrefabInternal;
				}
				return activePrefabInternal;
			}
		}

		// Token: 0x06002CE8 RID: 11496 RVA: 0x001283C4 File Offset: 0x001265C4
		public void Add(T prefab, bool isOverride)
		{
			using (new WriteLock(this.rwl))
			{
				this.AddInternal(prefab, isOverride);
			}
		}

		// Token: 0x06002CE9 RID: 11497 RVA: 0x00128400 File Offset: 0x00126600
		public void RemoveIfContains(T prefab)
		{
			using (new WriteLock(this.rwl))
			{
				this.RemoveIfContainsInternal(prefab);
			}
		}

		// Token: 0x06002CEA RID: 11498 RVA: 0x0012843C File Offset: 0x0012663C
		public void Remove(T prefab)
		{
			using (new WriteLock(this.rwl))
			{
				this.RemoveInternal(prefab);
			}
		}

		// Token: 0x06002CEB RID: 11499 RVA: 0x00128478 File Offset: 0x00126678
		public void RemoveByFile(ContentFile file, [Nullable(new byte[]
		{
			2,
			1
		})] Action<T> callback = null)
		{
			List<T> removed = new List<T>();
			using (new WriteLock(this.rwl))
			{
				for (int i = this.overrides.Count - 1; i >= 0; i--)
				{
					T prefab = this.overrides[i];
					if (prefab.ContentFile == file)
					{
						this.RemoveInternal(prefab);
						removed.Add(prefab);
					}
				}
				T p = this.basePrefabInternal;
				if (p != null)
				{
					ContentFile baseFile = p.ContentFile;
					if (baseFile == file)
					{
						this.RemoveInternal(this.basePrefabInternal);
						removed.Add(p);
					}
				}
			}
			if (callback != null)
			{
				removed.ForEach(callback);
			}
		}

		// Token: 0x06002CEC RID: 11500 RVA: 0x00128538 File Offset: 0x00126738
		public void Sort()
		{
			using (new WriteLock(this.rwl))
			{
				this.SortInternal();
			}
		}

		// Token: 0x17000D5B RID: 3419
		// (get) Token: 0x06002CED RID: 11501 RVA: 0x00128574 File Offset: 0x00126774
		public bool IsEmpty
		{
			get
			{
				bool isEmptyInternal;
				using (new ReadLock(this.rwl))
				{
					isEmptyInternal = this.isEmptyInternal;
				}
				return isEmptyInternal;
			}
		}

		// Token: 0x06002CEE RID: 11502 RVA: 0x001285B0 File Offset: 0x001267B0
		public bool Contains(T prefab)
		{
			bool result;
			using (new ReadLock(this.rwl))
			{
				result = this.ContainsInternal(prefab);
			}
			return result;
		}

		// Token: 0x06002CEF RID: 11503 RVA: 0x001285F0 File Offset: 0x001267F0
		public bool IsOverride(T prefab)
		{
			bool result;
			using (new ReadLock(this.rwl))
			{
				result = this.IsOverrideInternal(prefab);
			}
			return result;
		}

		// Token: 0x17000D5C RID: 3420
		// (get) Token: 0x06002CF0 RID: 11504 RVA: 0x00128630 File Offset: 0x00126830
		[Nullable(2)]
		private T activePrefabInternal
		{
			[NullableContext(2)]
			get
			{
				if (this.overrides.Count <= 0)
				{
					return this.basePrefabInternal;
				}
				return this.overrides.First<T>();
			}
		}

		// Token: 0x06002CF1 RID: 11505 RVA: 0x00128654 File Offset: 0x00126854
		private void AddInternal(T prefab, bool isOverride)
		{
			if (isOverride)
			{
				if (this.overrides.Contains(prefab))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Duplicate prefab in PrefabSelector (");
					defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted(prefab.ContentFile.ContentPackage.Name);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this.overrides.Add(prefab);
			}
			else
			{
				if (this.basePrefabInternal != null)
				{
					MapEntityPrefab mapEntityPrefab = prefab as MapEntityPrefab;
					string text;
					if (mapEntityPrefab == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("\"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\"");
						text = defaultInterpolatedStringHandler2.ToStringAndClear();
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(6, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("\"");
						defaultInterpolatedStringHandler3.AppendFormatted(mapEntityPrefab.OriginalName);
						defaultInterpolatedStringHandler3.AppendLiteral("\", \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\"");
						text = defaultInterpolatedStringHandler3.ToStringAndClear();
					}
					string prefabName = text;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(116, 6);
					defaultInterpolatedStringHandler4.AppendLiteral("Failed to add the prefab ");
					defaultInterpolatedStringHandler4.AppendFormatted(prefabName);
					defaultInterpolatedStringHandler4.AppendLiteral(" (");
					defaultInterpolatedStringHandler4.AppendFormatted<Type>(prefab.GetType());
					defaultInterpolatedStringHandler4.AppendLiteral(") from \"");
					ContentPackage contentPackage = prefab.ContentPackage;
					defaultInterpolatedStringHandler4.AppendFormatted(((contentPackage != null) ? contentPackage.Name : null) ?? "[NULL]");
					defaultInterpolatedStringHandler4.AppendLiteral("\" (");
					ContentPackage contentPackage2 = prefab.ContentPackage;
					defaultInterpolatedStringHandler4.AppendFormatted(((contentPackage2 != null) ? contentPackage2.Dir : null) ?? "");
					defaultInterpolatedStringHandler4.AppendLiteral("): ");
					defaultInterpolatedStringHandler4.AppendLiteral("a prefab with the same identifier from \"");
					ContentPackage contentPackage3 = this.activePrefabInternal.ContentPackage;
					defaultInterpolatedStringHandler4.AppendFormatted(((contentPackage3 != null) ? contentPackage3.Name : null) ?? "[NULL]");
					defaultInterpolatedStringHandler4.AppendLiteral("\" (");
					ContentPackage contentPackage4 = this.activePrefabInternal.ContentPackage;
					defaultInterpolatedStringHandler4.AppendFormatted(((contentPackage4 != null) ? contentPackage4.Dir : null) ?? "");
					defaultInterpolatedStringHandler4.AppendLiteral(") already exists; try overriding");
					throw new InvalidOperationException(defaultInterpolatedStringHandler4.ToStringAndClear());
				}
				this.basePrefabInternal = prefab;
			}
			this.SortInternal();
		}

		// Token: 0x06002CF2 RID: 11506 RVA: 0x00128902 File Offset: 0x00126B02
		private void RemoveIfContainsInternal(T prefab)
		{
			if (!this.ContainsInternal(prefab))
			{
				return;
			}
			this.RemoveInternal(prefab);
		}

		// Token: 0x06002CF3 RID: 11507 RVA: 0x00128918 File Offset: 0x00126B18
		private void RemoveInternal(T prefab)
		{
			if (this.basePrefabInternal == prefab)
			{
				this.basePrefabInternal = default(T);
			}
			else
			{
				if (!this.overrides.Contains(prefab))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Can't remove prefab from PrefabSelector (");
					defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted(prefab.ContentFile.ContentPackage.Name);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this.overrides.Remove(prefab);
			}
			prefab.Dispose();
			this.SortInternal();
		}

		// Token: 0x06002CF4 RID: 11508 RVA: 0x00128A00 File Offset: 0x00126C00
		private void SortInternal()
		{
			this.overrides.Sort(delegate(T p1, T p2)
			{
				ContentPackage contentPackage = p1.ContentPackage;
				int num = (contentPackage != null) ? contentPackage.Index : int.MaxValue;
				ContentPackage contentPackage2 = p2.ContentPackage;
				return num - ((contentPackage2 != null) ? contentPackage2.Index : int.MaxValue);
			});
		}

		// Token: 0x17000D5D RID: 3421
		// (get) Token: 0x06002CF5 RID: 11509 RVA: 0x00128A2C File Offset: 0x00126C2C
		private bool isEmptyInternal
		{
			get
			{
				return this.basePrefabInternal == null && this.overrides.Count == 0;
			}
		}

		// Token: 0x06002CF6 RID: 11510 RVA: 0x00128A4B File Offset: 0x00126C4B
		private bool ContainsInternal(T prefab)
		{
			return this.basePrefabInternal == prefab || this.overrides.Contains(prefab);
		}

		// Token: 0x06002CF7 RID: 11511 RVA: 0x00128A6E File Offset: 0x00126C6E
		private int IndexOfInternal(T prefab)
		{
			if (this.basePrefabInternal != prefab)
			{
				return this.overrides.IndexOf(prefab);
			}
			return this.overrides.Count;
		}

		// Token: 0x06002CF8 RID: 11512 RVA: 0x00128A9B File Offset: 0x00126C9B
		private bool IsOverrideInternal(T prefab)
		{
			return this.IndexOfInternal(prefab) > 0;
		}

		// Token: 0x06002CF9 RID: 11513 RVA: 0x00128AA7 File Offset: 0x00126CA7
		public IEnumerator<T> GetEnumerator()
		{
			PrefabSelector<T>.<GetEnumerator>d__27 <GetEnumerator>d__ = new PrefabSelector<T>.<GetEnumerator>d__27(0);
			<GetEnumerator>d__.<>4__this = this;
			return <GetEnumerator>d__;
		}

		// Token: 0x06002CFA RID: 11514 RVA: 0x00128AB6 File Offset: 0x00126CB6
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0400161B RID: 5659
		private readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();

		// Token: 0x0400161C RID: 5660
		[Nullable(2)]
		private T basePrefabInternal;

		// Token: 0x0400161D RID: 5661
		private readonly List<T> overrides = new List<T>();
	}
}

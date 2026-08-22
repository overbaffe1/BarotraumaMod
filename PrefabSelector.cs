using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Barotrauma.Threading;

namespace Barotrauma
{
	// Token: 0x02000348 RID: 840
	[NullableContext(1)]
	[Nullable(0)]
	public class PrefabSelector<T> : IEnumerable<!0>, IEnumerable where T : Prefab
	{
		// Token: 0x17001180 RID: 4480
		// (get) Token: 0x06004209 RID: 16905 RVA: 0x00248758 File Offset: 0x00246958
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

		// Token: 0x17001181 RID: 4481
		// (get) Token: 0x0600420A RID: 16906 RVA: 0x00248794 File Offset: 0x00246994
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

		// Token: 0x0600420B RID: 16907 RVA: 0x002487D0 File Offset: 0x002469D0
		public void Add(T prefab, bool isOverride)
		{
			using (new WriteLock(this.rwl))
			{
				this.AddInternal(prefab, isOverride);
			}
		}

		// Token: 0x0600420C RID: 16908 RVA: 0x0024880C File Offset: 0x00246A0C
		public void RemoveIfContains(T prefab)
		{
			using (new WriteLock(this.rwl))
			{
				this.RemoveIfContainsInternal(prefab);
			}
		}

		// Token: 0x0600420D RID: 16909 RVA: 0x00248848 File Offset: 0x00246A48
		public void Remove(T prefab)
		{
			using (new WriteLock(this.rwl))
			{
				this.RemoveInternal(prefab);
			}
		}

		// Token: 0x0600420E RID: 16910 RVA: 0x00248884 File Offset: 0x00246A84
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

		// Token: 0x0600420F RID: 16911 RVA: 0x00248944 File Offset: 0x00246B44
		public void Sort()
		{
			using (new WriteLock(this.rwl))
			{
				this.SortInternal();
			}
		}

		// Token: 0x17001182 RID: 4482
		// (get) Token: 0x06004210 RID: 16912 RVA: 0x00248980 File Offset: 0x00246B80
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

		// Token: 0x06004211 RID: 16913 RVA: 0x002489BC File Offset: 0x00246BBC
		public bool Contains(T prefab)
		{
			bool result;
			using (new ReadLock(this.rwl))
			{
				result = this.ContainsInternal(prefab);
			}
			return result;
		}

		// Token: 0x06004212 RID: 16914 RVA: 0x002489FC File Offset: 0x00246BFC
		public bool IsOverride(T prefab)
		{
			bool result;
			using (new ReadLock(this.rwl))
			{
				result = this.IsOverrideInternal(prefab);
			}
			return result;
		}

		// Token: 0x17001183 RID: 4483
		// (get) Token: 0x06004213 RID: 16915 RVA: 0x00248A3C File Offset: 0x00246C3C
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

		// Token: 0x06004214 RID: 16916 RVA: 0x00248A60 File Offset: 0x00246C60
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

		// Token: 0x06004215 RID: 16917 RVA: 0x00248D0E File Offset: 0x00246F0E
		private void RemoveIfContainsInternal(T prefab)
		{
			if (!this.ContainsInternal(prefab))
			{
				return;
			}
			this.RemoveInternal(prefab);
		}

		// Token: 0x06004216 RID: 16918 RVA: 0x00248D24 File Offset: 0x00246F24
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

		// Token: 0x06004217 RID: 16919 RVA: 0x00248E0C File Offset: 0x0024700C
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

		// Token: 0x17001184 RID: 4484
		// (get) Token: 0x06004218 RID: 16920 RVA: 0x00248E38 File Offset: 0x00247038
		private bool isEmptyInternal
		{
			get
			{
				return this.basePrefabInternal == null && this.overrides.Count == 0;
			}
		}

		// Token: 0x06004219 RID: 16921 RVA: 0x00248E57 File Offset: 0x00247057
		private bool ContainsInternal(T prefab)
		{
			return this.basePrefabInternal == prefab || this.overrides.Contains(prefab);
		}

		// Token: 0x0600421A RID: 16922 RVA: 0x00248E7A File Offset: 0x0024707A
		private int IndexOfInternal(T prefab)
		{
			if (this.basePrefabInternal != prefab)
			{
				return this.overrides.IndexOf(prefab);
			}
			return this.overrides.Count;
		}

		// Token: 0x0600421B RID: 16923 RVA: 0x00248EA7 File Offset: 0x002470A7
		private bool IsOverrideInternal(T prefab)
		{
			return this.IndexOfInternal(prefab) > 0;
		}

		// Token: 0x0600421C RID: 16924 RVA: 0x00248EB3 File Offset: 0x002470B3
		public IEnumerator<T> GetEnumerator()
		{
			PrefabSelector<T>.<GetEnumerator>d__27 <GetEnumerator>d__ = new PrefabSelector<T>.<GetEnumerator>d__27(0);
			<GetEnumerator>d__.<>4__this = this;
			return <GetEnumerator>d__;
		}

		// Token: 0x0600421D RID: 16925 RVA: 0x00248EC2 File Offset: 0x002470C2
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x04002263 RID: 8803
		private readonly ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();

		// Token: 0x04002264 RID: 8804
		[Nullable(2)]
		private T basePrefabInternal;

		// Token: 0x04002265 RID: 8805
		private readonly List<T> overrides = new List<T>();
	}
}

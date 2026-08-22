using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000274 RID: 628
	[NullableContext(1)]
	[Nullable(0)]
	public class PrefabCollection<T> : IEnumerable<!0>, IEnumerable where T : Prefab
	{
		// Token: 0x06002CC9 RID: 11465 RVA: 0x00127874 File Offset: 0x00125A74
		public PrefabCollection()
		{
			Type[] interfaces = typeof(T).GetInterfaces();
			this.implementsVariants = interfaces.Any((Type i) => i.Name.Contains("IImplementsVariants"));
		}

		// Token: 0x06002CCA RID: 11466 RVA: 0x001278D8 File Offset: 0x00125AD8
		[NullableContext(2)]
		public PrefabCollection([Nullable(new byte[]
		{
			2,
			1
		})] Action<T, bool> onAdd, [Nullable(new byte[]
		{
			2,
			1
		})] Action<T> onRemove, Action onSort, [Nullable(new byte[]
		{
			2,
			1
		})] Action<ContentFile> onAddOverrideFile, [Nullable(new byte[]
		{
			2,
			1
		})] Action<ContentFile> onRemoveOverrideFile) : this()
		{
			this.OnAdd = onAdd;
			this.OnRemove = onRemove;
			this.OnSort = onSort;
			this.OnAddOverrideFile = onAddOverrideFile;
			this.OnRemoveOverrideFile = onRemoveOverrideFile;
		}

		// Token: 0x06002CCB RID: 11467 RVA: 0x00127905 File Offset: 0x00125B05
		[NullableContext(2)]
		public PrefabCollection(Action onSort) : this()
		{
			this.OnSort = onSort;
		}

		// Token: 0x06002CCC RID: 11468 RVA: 0x00127914 File Offset: 0x00125B14
		public IOrderedEnumerable<T> GetOrdered()
		{
			if (typeof(T).IsAssignableFrom(typeof(PrefabWithUintIdentifier)))
			{
				return from p in this
				orderby (p as PrefabWithUintIdentifier).UintIdentifier
				select p;
			}
			return from p in this
			orderby p.Identifier
			select p;
		}

		// Token: 0x06002CCD RID: 11469 RVA: 0x00127987 File Offset: 0x00125B87
		private bool IsPrefabOverriddenByFile(T prefab)
		{
			return this.topMostOverrideFile != null && this.topMostOverrideFile.ContentPackage.Index > prefab.ContentFile.ContentPackage.Index;
		}

		// Token: 0x06002CCE RID: 11470 RVA: 0x001279BC File Offset: 0x00125BBC
		[NullableContext(2)]
		private static bool IsInheritanceValid(T prefab)
		{
			if (prefab == null)
			{
				return false;
			}
			IImplementsVariants<T> implementsVariants = prefab as IImplementsVariants<T>;
			return implementsVariants == null || implementsVariants.VariantOf.IsEmpty || (implementsVariants.ParentPrefab != null && PrefabCollection<T>.IsInheritanceValid(implementsVariants.ParentPrefab));
		}

		// Token: 0x06002CCF RID: 11471 RVA: 0x00127A10 File Offset: 0x00125C10
		private void HandleInheritance(Identifier prefabIdentifier)
		{
			this.HandleInheritance(prefabIdentifier.ToEnumerable<Identifier>());
		}

		// Token: 0x06002CD0 RID: 11472 RVA: 0x00127A20 File Offset: 0x00125C20
		private void HandleInheritance(IEnumerable<Identifier> identifiers)
		{
			if (!this.implementsVariants)
			{
				return;
			}
			foreach (Identifier id in identifiers)
			{
				T prefab;
				if (this.TryGet(id, out prefab, false))
				{
					IImplementsVariants<T> implementsVariants = prefab as IImplementsVariants<T>;
					if (implementsVariants != null && !implementsVariants.VariantOf.IsEmpty)
					{
						implementsVariants.ParentPrefab = default(T);
					}
				}
			}
			PrefabCollection<T>.InheritanceTreeCollection inheritanceTreeCollection = new PrefabCollection<T>.InheritanceTreeCollection(this);
			inheritanceTreeCollection.AddNodesAndInheritors(identifiers);
			inheritanceTreeCollection.InvokeCallbacks();
		}

		// Token: 0x17000D55 RID: 3413
		// (get) Token: 0x06002CD1 RID: 11473 RVA: 0x00127AC0 File Offset: 0x00125CC0
		[Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})]
		public IEnumerable<KeyValuePair<Identifier, PrefabSelector<T>>> AllPrefabs
		{
			[return: Nullable(new byte[]
			{
				1,
				0,
				1,
				1
			})]
			get
			{
				PrefabCollection<T>.<get_AllPrefabs>d__19 <get_AllPrefabs>d__ = new PrefabCollection<T>.<get_AllPrefabs>d__19(-2);
				<get_AllPrefabs>d__.<>4__this = this;
				return <get_AllPrefabs>d__;
			}
		}

		// Token: 0x17000D56 RID: 3414
		public T this[Identifier identifier]
		{
			get
			{
				Prefab.DisallowCallFromConstructor();
				T prefab = this.prefabs[identifier].ActivePrefab;
				if (prefab != null && !this.IsPrefabOverriddenByFile(prefab) && PrefabCollection<T>.IsInheritanceValid(prefab))
				{
					return prefab;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Prefab of identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" cannot be returned because it was overridden by \"");
				defaultInterpolatedStringHandler.AppendFormatted<ContentPath>(this.topMostOverrideFile.Path);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				throw new IndexOutOfRangeException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x17000D57 RID: 3415
		public T this[string identifier]
		{
			get
			{
				return this[identifier.ToIdentifier()];
			}
		}

		// Token: 0x06002CD4 RID: 11476 RVA: 0x00127B81 File Offset: 0x00125D81
		[NullableContext(2)]
		public bool TryGet(Identifier identifier, [NotNullWhen(true)] out T result)
		{
			return this.TryGet(identifier, out result, true);
		}

		// Token: 0x06002CD5 RID: 11477 RVA: 0x00127B8C File Offset: 0x00125D8C
		[NullableContext(2)]
		private bool TryGet(Identifier identifier, [NotNullWhen(true)] out T result, bool requireInheritanceValid)
		{
			Prefab.DisallowCallFromConstructor();
			PrefabSelector<T> selector;
			if (this.prefabs.TryGetValue(identifier, out selector) && selector.ActivePrefab != null)
			{
				result = selector.ActivePrefab;
				return !requireInheritanceValid || PrefabCollection<T>.IsInheritanceValid(result);
			}
			result = default(T);
			return false;
		}

		// Token: 0x06002CD6 RID: 11478 RVA: 0x00127BE0 File Offset: 0x00125DE0
		public bool TryGet(string identifier, [Nullable(2)] out T result)
		{
			return this.TryGet(identifier.ToIdentifier(), out result);
		}

		// Token: 0x17000D58 RID: 3416
		// (get) Token: 0x06002CD7 RID: 11479 RVA: 0x00127BEF File Offset: 0x00125DEF
		public IEnumerable<Identifier> Keys
		{
			get
			{
				return this.prefabs.Keys;
			}
		}

		// Token: 0x06002CD8 RID: 11480 RVA: 0x00127BFC File Offset: 0x00125DFC
		[return: Nullable(2)]
		public T Find(Predicate<T> predicate)
		{
			Prefab.DisallowCallFromConstructor();
			foreach (KeyValuePair<Identifier, PrefabSelector<T>> kpv in this.prefabs)
			{
				T p = kpv.Value.ActivePrefab;
				if (p != null && predicate(p))
				{
					return p;
				}
			}
			return default(T);
		}

		// Token: 0x06002CD9 RID: 11481 RVA: 0x00127C78 File Offset: 0x00125E78
		public bool ContainsKey(Identifier identifier)
		{
			Prefab.DisallowCallFromConstructor();
			T t;
			return this.TryGet(identifier, out t);
		}

		// Token: 0x06002CDA RID: 11482 RVA: 0x00127C93 File Offset: 0x00125E93
		public bool ContainsKey(string k)
		{
			return this.prefabs.ContainsKey(k.ToIdentifier());
		}

		// Token: 0x06002CDB RID: 11483 RVA: 0x00127CA6 File Offset: 0x00125EA6
		public bool IsOverride(T prefab)
		{
			Prefab.DisallowCallFromConstructor();
			return this.ContainsKey(prefab.Identifier) && this.prefabs[prefab.Identifier].IsOverride(prefab);
		}

		// Token: 0x06002CDC RID: 11484 RVA: 0x00127CE0 File Offset: 0x00125EE0
		public void Add(T prefab, bool isOverride)
		{
			PrefabCollection<T>.<>c__DisplayClass33_0 CS$<>8__locals1 = new PrefabCollection<T>.<>c__DisplayClass33_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.prefab = prefab;
			Prefab.DisallowCallFromConstructor();
			if (CS$<>8__locals1.prefab.Identifier.IsEmpty)
			{
				throw new ArgumentException("Prefab has no identifier!");
			}
			PrefabSelector<T> selector;
			bool selectorExists = this.prefabs.TryGetValue(CS$<>8__locals1.prefab.Identifier, out selector);
			if (selector == null)
			{
				selector = new PrefabSelector<T>();
			}
			CS$<>8__locals1.prefabWithUintIdentifier = (CS$<>8__locals1.prefab as PrefabWithUintIdentifier);
			if (CS$<>8__locals1.prefabWithUintIdentifier != null)
			{
				if (!selector.IsEmpty)
				{
					CS$<>8__locals1.prefabWithUintIdentifier.UintIdentifier = (selector.ActivePrefab as PrefabWithUintIdentifier).UintIdentifier;
				}
				else
				{
					using (MD5 md5 = MD5.Create())
					{
						CS$<>8__locals1.prefabWithUintIdentifier.UintIdentifier = ToolBoxCore.IdentifierToUint32Hash(CS$<>8__locals1.prefab.Identifier, md5);
						for (T collision = CS$<>8__locals1.<Add>g__findCollision|0(); collision != null; collision = CS$<>8__locals1.<Add>g__findCollision|0())
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(92, 4);
							defaultInterpolatedStringHandler.AppendLiteral("Hashing collision when generating uint identifiers for ");
							defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
							defaultInterpolatedStringHandler.AppendLiteral(": ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(" has the same UintIdentifier as ");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(collision.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted<uint>(CS$<>8__locals1.prefabWithUintIdentifier.UintIdentifier);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							PrefabWithUintIdentifier prefabWithUintIdentifier = CS$<>8__locals1.prefabWithUintIdentifier;
							uint uintIdentifier = prefabWithUintIdentifier.UintIdentifier;
							prefabWithUintIdentifier.UintIdentifier = uintIdentifier + 1U;
						}
					}
				}
			}
			selector.Add(CS$<>8__locals1.prefab, isOverride);
			if (!selectorExists && !this.prefabs.TryAdd(CS$<>8__locals1.prefab.Identifier, selector))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(29, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Failed to add selector for \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(CS$<>8__locals1.prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\"");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			Action<T, bool> onAdd = this.OnAdd;
			if (onAdd != null)
			{
				onAdd(CS$<>8__locals1.prefab, isOverride);
			}
			this.HandleInheritance(CS$<>8__locals1.prefab.Identifier);
		}

		// Token: 0x06002CDD RID: 11485 RVA: 0x00127F6C File Offset: 0x0012616C
		public void Remove(T prefab)
		{
			Prefab.DisallowCallFromConstructor();
			Action<T> onRemove = this.OnRemove;
			if (onRemove != null)
			{
				onRemove(prefab);
			}
			if (!this.ContainsKey(prefab.Identifier))
			{
				return;
			}
			if (!this.prefabs[prefab.Identifier].Contains(prefab))
			{
				return;
			}
			this.prefabs[prefab.Identifier].Remove(prefab);
			if (this.prefabs[prefab.Identifier].IsEmpty)
			{
				PrefabSelector<T> prefabSelector;
				this.prefabs.TryRemove(prefab.Identifier, out prefabSelector);
			}
			this.HandleInheritance(prefab.Identifier);
		}

		// Token: 0x06002CDE RID: 11486 RVA: 0x00128028 File Offset: 0x00126228
		public void RemoveByFile(ContentFile file)
		{
			Prefab.DisallowCallFromConstructor();
			HashSet<Identifier> clearedIdentifiers = new HashSet<Identifier>();
			foreach (KeyValuePair<Identifier, PrefabSelector<T>> kpv in this.prefabs)
			{
				kpv.Value.RemoveByFile(file, this.OnRemove);
				if (kpv.Value.IsEmpty)
				{
					clearedIdentifiers.Add(kpv.Key);
				}
			}
			foreach (Identifier identifier in clearedIdentifiers)
			{
				PrefabSelector<T> prefabSelector;
				this.prefabs.TryRemove(identifier, out prefabSelector);
			}
			this.RemoveOverrideFile(file);
		}

		// Token: 0x06002CDF RID: 11487 RVA: 0x001280F8 File Offset: 0x001262F8
		public void AddOverrideFile(ContentFile file)
		{
			Prefab.DisallowCallFromConstructor();
			if (!this.overrideFiles.Contains(file))
			{
				this.overrideFiles.Add(file);
			}
			Action<ContentFile> onAddOverrideFile = this.OnAddOverrideFile;
			if (onAddOverrideFile == null)
			{
				return;
			}
			onAddOverrideFile(file);
		}

		// Token: 0x06002CE0 RID: 11488 RVA: 0x0012812B File Offset: 0x0012632B
		public void RemoveOverrideFile(ContentFile file)
		{
			Prefab.DisallowCallFromConstructor();
			if (this.overrideFiles.Contains(file))
			{
				this.overrideFiles.Remove(file);
			}
			Action<ContentFile> onRemoveOverrideFile = this.OnRemoveOverrideFile;
			if (onRemoveOverrideFile == null)
			{
				return;
			}
			onRemoveOverrideFile(file);
		}

		// Token: 0x06002CE1 RID: 11489 RVA: 0x00128160 File Offset: 0x00126360
		public void SortAll()
		{
			Prefab.DisallowCallFromConstructor();
			foreach (KeyValuePair<Identifier, PrefabSelector<T>> kvp in this.prefabs)
			{
				kvp.Value.Sort();
			}
			this.topMostOverrideFile = (this.overrideFiles.Any<ContentFile>() ? this.overrideFiles.First((ContentFile f1) => this.overrideFiles.All((ContentFile f2) => f1.ContentPackage.Index >= f2.ContentPackage.Index)) : null);
			Action onSort = this.OnSort;
			if (onSort != null)
			{
				onSort();
			}
			this.HandleInheritance(from p in this
			select p.Identifier);
			IEnumerator<T> enumerator = this.GetEnumerator(false);
			while (enumerator.MoveNext())
			{
				T p2 = enumerator.Current;
				IImplementsVariants<T> implementsVariants = p2 as IImplementsVariants<T>;
				if (implementsVariants != null && !PrefabCollection<T>.IsInheritanceValid(p2))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(102, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Error in content package \"");
					defaultInterpolatedStringHandler.AppendFormatted(p2.ContentFile.ContentPackage.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\": ");
					defaultInterpolatedStringHandler.AppendLiteral("could not find the prefab \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(implementsVariants.VariantOf);
					defaultInterpolatedStringHandler.AppendLiteral("\" the prefab \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(p2.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" is configured as a variant of.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
		}

		// Token: 0x06002CE2 RID: 11490 RVA: 0x001282F0 File Offset: 0x001264F0
		public IEnumerator<T> GetEnumerator()
		{
			return this.GetEnumerator(true);
		}

		// Token: 0x06002CE3 RID: 11491 RVA: 0x001282F9 File Offset: 0x001264F9
		private IEnumerator<T> GetEnumerator(bool requireInheritanceValid)
		{
			PrefabCollection<T>.<GetEnumerator>d__40 <GetEnumerator>d__ = new PrefabCollection<T>.<GetEnumerator>d__40(0);
			<GetEnumerator>d__.<>4__this = this;
			<GetEnumerator>d__.requireInheritanceValid = requireInheritanceValid;
			return <GetEnumerator>d__;
		}

		// Token: 0x06002CE4 RID: 11492 RVA: 0x0012830F File Offset: 0x0012650F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator(true);
		}

		// Token: 0x04001612 RID: 5650
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<T, bool> OnAdd;

		// Token: 0x04001613 RID: 5651
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<T> OnRemove;

		// Token: 0x04001614 RID: 5652
		[Nullable(2)]
		private readonly Action OnSort;

		// Token: 0x04001615 RID: 5653
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<ContentFile> OnAddOverrideFile;

		// Token: 0x04001616 RID: 5654
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<ContentFile> OnRemoveOverrideFile;

		// Token: 0x04001617 RID: 5655
		private readonly ConcurrentDictionary<Identifier, PrefabSelector<T>> prefabs = new ConcurrentDictionary<Identifier, PrefabSelector<T>>();

		// Token: 0x04001618 RID: 5656
		private readonly HashSet<ContentFile> overrideFiles = new HashSet<ContentFile>();

		// Token: 0x04001619 RID: 5657
		[Nullable(2)]
		private ContentFile topMostOverrideFile;

		// Token: 0x0400161A RID: 5658
		private readonly bool implementsVariants;

		// Token: 0x02000AE8 RID: 2792
		[Nullable(0)]
		private class InheritanceTreeCollection
		{
			// Token: 0x06005EC2 RID: 24258 RVA: 0x00205A0D File Offset: 0x00203C0D
			public InheritanceTreeCollection(PrefabCollection<T> collection)
			{
				this.prefabCollection = collection;
			}

			// Token: 0x06005EC3 RID: 24259 RVA: 0x00205A34 File Offset: 0x00203C34
			[return: Nullable(new byte[]
			{
				2,
				0
			})]
			public PrefabCollection<T>.InheritanceTreeCollection.Node AddNodeAndInheritors(Identifier id)
			{
				T t;
				if (!this.prefabCollection.TryGet(id, out t, false))
				{
					return null;
				}
				PrefabCollection<T>.InheritanceTreeCollection.Node node;
				if (!this.IdToNode.TryGetValue(id, out node))
				{
					node = new PrefabCollection<T>.InheritanceTreeCollection.Node(id);
					this.RootNodes.Add(node);
					this.IdToNode.Add(id, node);
					IEnumerator<T> enumerator = this.prefabCollection.GetEnumerator(false);
					while (enumerator.MoveNext())
					{
						T p = enumerator.Current;
						IImplementsVariants<T> implementsVariants = p as IImplementsVariants<T>;
						if (implementsVariants != null)
						{
							Identifier variantOf = implementsVariants.VariantOf;
							if (!(variantOf != id))
							{
								PrefabCollection<T>.InheritanceTreeCollection.Node inheritorNode = this.AddNodeAndInheritors(p.Identifier);
								if (inheritorNode != null)
								{
									this.RootNodes.Remove(inheritorNode);
									inheritorNode.Parent = node;
									node.Inheritors.Add(inheritorNode);
								}
							}
						}
					}
					return node;
				}
				return node;
			}

			// Token: 0x06005EC4 RID: 24260 RVA: 0x00205B08 File Offset: 0x00203D08
			private static void FindCycles([Nullable(new byte[]
			{
				1,
				0
			})] in PrefabCollection<T>.InheritanceTreeCollection.Node node, [Nullable(new byte[]
			{
				1,
				1,
				0
			})] HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node> uncheckedNodes)
			{
				HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node> checkedNodes = new HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node>();
				List<PrefabCollection<T>.InheritanceTreeCollection.Node> hierarchyPositions = new List<PrefabCollection<T>.InheritanceTreeCollection.Node>();
				PrefabCollection<T>.InheritanceTreeCollection.Node currNode = node;
				while (uncheckedNodes.Contains(currNode))
				{
					if (checkedNodes.Contains(currNode))
					{
						int index = hierarchyPositions.IndexOf(currNode);
						throw new Exception("Inheritance cycle detected: " + string.Join<Identifier>(", ", from n in hierarchyPositions.Skip(index)
						select n.Identifier));
					}
					checkedNodes.Add(currNode);
					hierarchyPositions.Add(currNode);
					currNode = currNode.Parent;
					if (currNode == null)
					{
						break;
					}
				}
				uncheckedNodes.RemoveWhere(([Nullable(new byte[]
				{
					1,
					0
				})] PrefabCollection<T>.InheritanceTreeCollection.Node i) => checkedNodes.Contains(i));
			}

			// Token: 0x06005EC5 RID: 24261 RVA: 0x00205BC7 File Offset: 0x00203DC7
			public void AddNodesAndInheritors(IEnumerable<Identifier> ids)
			{
				ids.ForEach(delegate(Identifier id)
				{
					this.AddNodeAndInheritors(id);
				});
			}

			// Token: 0x06005EC6 RID: 24262 RVA: 0x00205BDC File Offset: 0x00203DDC
			public void InvokeCallbacks()
			{
				PrefabCollection<T>.InheritanceTreeCollection.<>c__DisplayClass8_0 CS$<>8__locals1 = new PrefabCollection<T>.InheritanceTreeCollection.<>c__DisplayClass8_0();
				CS$<>8__locals1.<>4__this = this;
				CS$<>8__locals1.uncheckedNodes = this.IdToNode.Values.ToHashSet<PrefabCollection<T>.InheritanceTreeCollection.Node>();
				this.IdToNode.Values.ForEach(delegate(PrefabCollection<T>.InheritanceTreeCollection.Node v)
				{
					PrefabCollection<T>.InheritanceTreeCollection.FindCycles(v, CS$<>8__locals1.uncheckedNodes);
				});
				this.RootNodes.ForEach(new Action<PrefabCollection<T>.InheritanceTreeCollection.Node>(CS$<>8__locals1.<InvokeCallbacks>g__invokeCallbacksForNode|1));
			}

			// Token: 0x04003795 RID: 14229
			private readonly PrefabCollection<T> prefabCollection;

			// Token: 0x04003796 RID: 14230
			[Nullable(new byte[]
			{
				1,
				1,
				0
			})]
			public readonly Dictionary<Identifier, PrefabCollection<T>.InheritanceTreeCollection.Node> IdToNode = new Dictionary<Identifier, PrefabCollection<T>.InheritanceTreeCollection.Node>();

			// Token: 0x04003797 RID: 14231
			[Nullable(new byte[]
			{
				1,
				1,
				0
			})]
			public readonly HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node> RootNodes = new HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node>();

			// Token: 0x02000EBD RID: 3773
			[NullableContext(0)]
			public class Node
			{
				// Token: 0x06006AF9 RID: 27385 RVA: 0x00227659 File Offset: 0x00225859
				public Node(Identifier identifier)
				{
					this.Identifier = identifier;
				}

				// Token: 0x04004333 RID: 17203
				public readonly Identifier Identifier;

				// Token: 0x04004334 RID: 17204
				[Nullable(new byte[]
				{
					2,
					0
				})]
				public PrefabCollection<T>.InheritanceTreeCollection.Node Parent;

				// Token: 0x04004335 RID: 17205
				[Nullable(new byte[]
				{
					1,
					1,
					0
				})]
				public readonly HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node> Inheritors = new HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node>();
			}
		}
	}
}

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
	// Token: 0x02000347 RID: 839
	[NullableContext(1)]
	[Nullable(0)]
	public class PrefabCollection<T> : IEnumerable<!0>, IEnumerable where T : Prefab
	{
		// Token: 0x060041EC RID: 16876 RVA: 0x00247C80 File Offset: 0x00245E80
		public PrefabCollection()
		{
			Type[] interfaces = typeof(T).GetInterfaces();
			this.implementsVariants = interfaces.Any((Type i) => i.Name.Contains("IImplementsVariants"));
		}

		// Token: 0x060041ED RID: 16877 RVA: 0x00247CE4 File Offset: 0x00245EE4
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

		// Token: 0x060041EE RID: 16878 RVA: 0x00247D11 File Offset: 0x00245F11
		[NullableContext(2)]
		public PrefabCollection(Action onSort) : this()
		{
			this.OnSort = onSort;
		}

		// Token: 0x060041EF RID: 16879 RVA: 0x00247D20 File Offset: 0x00245F20
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

		// Token: 0x060041F0 RID: 16880 RVA: 0x00247D93 File Offset: 0x00245F93
		private bool IsPrefabOverriddenByFile(T prefab)
		{
			return this.topMostOverrideFile != null && this.topMostOverrideFile.ContentPackage.Index > prefab.ContentFile.ContentPackage.Index;
		}

		// Token: 0x060041F1 RID: 16881 RVA: 0x00247DC8 File Offset: 0x00245FC8
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

		// Token: 0x060041F2 RID: 16882 RVA: 0x00247E1C File Offset: 0x0024601C
		private void HandleInheritance(Identifier prefabIdentifier)
		{
			this.HandleInheritance(prefabIdentifier.ToEnumerable<Identifier>());
		}

		// Token: 0x060041F3 RID: 16883 RVA: 0x00247E2C File Offset: 0x0024602C
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

		// Token: 0x1700117C RID: 4476
		// (get) Token: 0x060041F4 RID: 16884 RVA: 0x00247ECC File Offset: 0x002460CC
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

		// Token: 0x1700117D RID: 4477
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

		// Token: 0x1700117E RID: 4478
		public T this[string identifier]
		{
			get
			{
				return this[identifier.ToIdentifier()];
			}
		}

		// Token: 0x060041F7 RID: 16887 RVA: 0x00247F8D File Offset: 0x0024618D
		[NullableContext(2)]
		public bool TryGet(Identifier identifier, [NotNullWhen(true)] out T result)
		{
			return this.TryGet(identifier, out result, true);
		}

		// Token: 0x060041F8 RID: 16888 RVA: 0x00247F98 File Offset: 0x00246198
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

		// Token: 0x060041F9 RID: 16889 RVA: 0x00247FEC File Offset: 0x002461EC
		public bool TryGet(string identifier, [Nullable(2)] out T result)
		{
			return this.TryGet(identifier.ToIdentifier(), out result);
		}

		// Token: 0x1700117F RID: 4479
		// (get) Token: 0x060041FA RID: 16890 RVA: 0x00247FFB File Offset: 0x002461FB
		public IEnumerable<Identifier> Keys
		{
			get
			{
				return this.prefabs.Keys;
			}
		}

		// Token: 0x060041FB RID: 16891 RVA: 0x00248008 File Offset: 0x00246208
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

		// Token: 0x060041FC RID: 16892 RVA: 0x00248084 File Offset: 0x00246284
		public bool ContainsKey(Identifier identifier)
		{
			Prefab.DisallowCallFromConstructor();
			T t;
			return this.TryGet(identifier, out t);
		}

		// Token: 0x060041FD RID: 16893 RVA: 0x0024809F File Offset: 0x0024629F
		public bool ContainsKey(string k)
		{
			return this.prefabs.ContainsKey(k.ToIdentifier());
		}

		// Token: 0x060041FE RID: 16894 RVA: 0x002480B2 File Offset: 0x002462B2
		public bool IsOverride(T prefab)
		{
			Prefab.DisallowCallFromConstructor();
			return this.ContainsKey(prefab.Identifier) && this.prefabs[prefab.Identifier].IsOverride(prefab);
		}

		// Token: 0x060041FF RID: 16895 RVA: 0x002480EC File Offset: 0x002462EC
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

		// Token: 0x06004200 RID: 16896 RVA: 0x00248378 File Offset: 0x00246578
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

		// Token: 0x06004201 RID: 16897 RVA: 0x00248434 File Offset: 0x00246634
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

		// Token: 0x06004202 RID: 16898 RVA: 0x00248504 File Offset: 0x00246704
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

		// Token: 0x06004203 RID: 16899 RVA: 0x00248537 File Offset: 0x00246737
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

		// Token: 0x06004204 RID: 16900 RVA: 0x0024856C File Offset: 0x0024676C
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

		// Token: 0x06004205 RID: 16901 RVA: 0x002486FC File Offset: 0x002468FC
		public IEnumerator<T> GetEnumerator()
		{
			return this.GetEnumerator(true);
		}

		// Token: 0x06004206 RID: 16902 RVA: 0x00248705 File Offset: 0x00246905
		private IEnumerator<T> GetEnumerator(bool requireInheritanceValid)
		{
			PrefabCollection<T>.<GetEnumerator>d__40 <GetEnumerator>d__ = new PrefabCollection<T>.<GetEnumerator>d__40(0);
			<GetEnumerator>d__.<>4__this = this;
			<GetEnumerator>d__.requireInheritanceValid = requireInheritanceValid;
			return <GetEnumerator>d__;
		}

		// Token: 0x06004207 RID: 16903 RVA: 0x0024871B File Offset: 0x0024691B
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator(true);
		}

		// Token: 0x0400225A RID: 8794
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<T, bool> OnAdd;

		// Token: 0x0400225B RID: 8795
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<T> OnRemove;

		// Token: 0x0400225C RID: 8796
		[Nullable(2)]
		private readonly Action OnSort;

		// Token: 0x0400225D RID: 8797
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<ContentFile> OnAddOverrideFile;

		// Token: 0x0400225E RID: 8798
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private readonly Action<ContentFile> OnRemoveOverrideFile;

		// Token: 0x0400225F RID: 8799
		private readonly ConcurrentDictionary<Identifier, PrefabSelector<T>> prefabs = new ConcurrentDictionary<Identifier, PrefabSelector<T>>();

		// Token: 0x04002260 RID: 8800
		private readonly HashSet<ContentFile> overrideFiles = new HashSet<ContentFile>();

		// Token: 0x04002261 RID: 8801
		[Nullable(2)]
		private ContentFile topMostOverrideFile;

		// Token: 0x04002262 RID: 8802
		private readonly bool implementsVariants;

		// Token: 0x02001060 RID: 4192
		[Nullable(0)]
		private class InheritanceTreeCollection
		{
			// Token: 0x06008C50 RID: 35920 RVA: 0x003AF225 File Offset: 0x003AD425
			public InheritanceTreeCollection(PrefabCollection<T> collection)
			{
				this.prefabCollection = collection;
			}

			// Token: 0x06008C51 RID: 35921 RVA: 0x003AF24C File Offset: 0x003AD44C
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

			// Token: 0x06008C52 RID: 35922 RVA: 0x003AF320 File Offset: 0x003AD520
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

			// Token: 0x06008C53 RID: 35923 RVA: 0x003AF3DF File Offset: 0x003AD5DF
			public void AddNodesAndInheritors(IEnumerable<Identifier> ids)
			{
				ids.ForEach(delegate(Identifier id)
				{
					this.AddNodeAndInheritors(id);
				});
			}

			// Token: 0x06008C54 RID: 35924 RVA: 0x003AF3F4 File Offset: 0x003AD5F4
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

			// Token: 0x0400583C RID: 22588
			private readonly PrefabCollection<T> prefabCollection;

			// Token: 0x0400583D RID: 22589
			[Nullable(new byte[]
			{
				1,
				1,
				0
			})]
			public readonly Dictionary<Identifier, PrefabCollection<T>.InheritanceTreeCollection.Node> IdToNode = new Dictionary<Identifier, PrefabCollection<T>.InheritanceTreeCollection.Node>();

			// Token: 0x0400583E RID: 22590
			[Nullable(new byte[]
			{
				1,
				1,
				0
			})]
			public readonly HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node> RootNodes = new HashSet<PrefabCollection<T>.InheritanceTreeCollection.Node>();

			// Token: 0x0200157D RID: 5501
			[NullableContext(0)]
			public class Node
			{
				// Token: 0x06009E15 RID: 40469 RVA: 0x003EE1BD File Offset: 0x003EC3BD
				public Node(Identifier identifier)
				{
					this.Identifier = identifier;
				}

				// Token: 0x04006897 RID: 26775
				public readonly Identifier Identifier;

				// Token: 0x04006898 RID: 26776
				[Nullable(new byte[]
				{
					2,
					0
				})]
				public PrefabCollection<T>.InheritanceTreeCollection.Node Parent;

				// Token: 0x04006899 RID: 26777
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

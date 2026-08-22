using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020003AF RID: 943
	internal class SpawnItemPerk : PerkBase
	{
		// Token: 0x060045C9 RID: 17865 RVA: 0x00269A1C File Offset: 0x00267C1C
		public SpawnItemPerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x170011FB RID: 4603
		// (get) Token: 0x060045CA RID: 17866 RVA: 0x00269A26 File Offset: 0x00267C26
		public override PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerOnly;
			}
		}

		// Token: 0x170011FC RID: 4604
		// (get) Token: 0x060045CB RID: 17867 RVA: 0x00269A29 File Offset: 0x00267C29
		// (set) Token: 0x060045CC RID: 17868 RVA: 0x00269A31 File Offset: 0x00267C31
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x170011FD RID: 4605
		// (get) Token: 0x060045CD RID: 17869 RVA: 0x00269A3A File Offset: 0x00267C3A
		// (set) Token: 0x060045CE RID: 17870 RVA: 0x00269A42 File Offset: 0x00267C42
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier Tag { get; set; }

		// Token: 0x170011FE RID: 4606
		// (get) Token: 0x060045CF RID: 17871 RVA: 0x00269A4B File Offset: 0x00267C4B
		// (set) Token: 0x060045D0 RID: 17872 RVA: 0x00269A53 File Offset: 0x00267C53
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int MinAmount { get; set; }

		// Token: 0x170011FF RID: 4607
		// (get) Token: 0x060045D1 RID: 17873 RVA: 0x00269A5C File Offset: 0x00267C5C
		// (set) Token: 0x060045D2 RID: 17874 RVA: 0x00269A64 File Offset: 0x00267C64
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float PerPlayer { get; set; }

		// Token: 0x17001200 RID: 4608
		// (get) Token: 0x060045D3 RID: 17875 RVA: 0x00269A6D File Offset: 0x00267C6D
		// (set) Token: 0x060045D4 RID: 17876 RVA: 0x00269A75 File Offset: 0x00267C75
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier PriorityContainerTag { get; set; }

		// Token: 0x060045D5 RID: 17877 RVA: 0x00269A80 File Offset: 0x00267C80
		public override void ApplyOnRoundStart(IReadOnlyCollection<Character> teamCharacters, Submarine teamSubmarine)
		{
			SpawnItemPerk.<>c__DisplayClass23_0 CS$<>8__locals1 = new SpawnItemPerk.<>c__DisplayClass23_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.teamSubmarine = teamSubmarine;
			if (CS$<>8__locals1.teamSubmarine == null)
			{
				return;
			}
			if (Entity.Spawner == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
				defaultInterpolatedStringHandler.AppendFormatted("SpawnItemPerk");
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(") failed to spawn items because EntitySpawner is null.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			int amount = Math.Max(this.MinAmount, (int)MathF.Ceiling(this.PerPlayer * (float)teamCharacters.Count));
			if (this.Identifier.IsEmpty)
			{
				if (this.Tag.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(60, 2);
					defaultInterpolatedStringHandler2.AppendFormatted("SpawnItemPerk");
					defaultInterpolatedStringHandler2.AppendLiteral(" (");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(") failed to spawn items: neither identifier or tag is set.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					return;
				}
				IEnumerable<ItemPrefab> matchingItems = from ip in ItemPrefab.Prefabs
				where ip.Tags.Contains(CS$<>8__locals1.<>4__this.Tag)
				select ip;
				if (matchingItems.None(null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(58, 3);
					defaultInterpolatedStringHandler3.AppendFormatted("SpawnItemPerk");
					defaultInterpolatedStringHandler3.AppendLiteral(" (");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler3.AppendLiteral(") failed to spawn items: no items found with the tag \"");
					defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Tag);
					defaultInterpolatedStringHandler3.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					return;
				}
				for (int i = 0; i < amount; i++)
				{
					CS$<>8__locals1.<ApplyOnRoundStart>g__SpawnItem|0(matchingItems.GetRandomUnsynced<ItemPrefab>(), 1);
				}
				return;
			}
			else
			{
				ItemPrefab prefab = ItemPrefab.Find(null, this.Identifier);
				if (prefab == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(66, 3);
					defaultInterpolatedStringHandler4.AppendFormatted("SpawnItemPerk");
					defaultInterpolatedStringHandler4.AppendLiteral(" (");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler4.AppendLiteral(") failed to spawn items because the ItemPrefab \"");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler4.AppendLiteral("\" was not found.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					return;
				}
				CS$<>8__locals1.<ApplyOnRoundStart>g__SpawnItem|0(prefab, amount);
				return;
			}
		}

		// Token: 0x060045D6 RID: 17878 RVA: 0x00269CE4 File Offset: 0x00267EE4
		private SpawnItemPerk.SuitableContainers FindSuitableContainers(ItemPrefab prefab, Submarine submarine)
		{
			HashSet<ItemContainer> priorityContainers = new HashSet<ItemContainer>();
			HashSet<ItemContainer> primaryContainers = new HashSet<ItemContainer>();
			HashSet<ItemContainer> secondaryContainers = new HashSet<ItemContainer>();
			foreach (Item item in submarine.GetItems(true))
			{
				if (item.GetComponent<Fabricator>() == null && item.GetComponent<Deconstructor>() == null && !item.NonInteractable && !item.NonPlayerTeamInteractable && !item.IsHidden)
				{
					ItemContainer container = item.GetComponent<ItemContainer>();
					if (container != null)
					{
						if (container.CanBeContained(prefab))
						{
							IReadOnlyCollection<Identifier> tags = item.GetTags();
							Identifier priorityContainerTag = this.PriorityContainerTag;
							if (!priorityContainerTag.IsEmpty)
							{
								if (!tags.Contains(this.PriorityContainerTag))
								{
									Prefab prefab2 = item.Prefab;
									priorityContainerTag = this.PriorityContainerTag;
									if (!(prefab2.Identifier == priorityContainerTag))
									{
										goto IL_E8;
									}
								}
								priorityContainers.Add(container);
								continue;
							}
							IL_E8:
							if (prefab.PreferredContainers.Any((PreferredContainer pc) => pc.Primary.Any(new Func<Identifier, bool>(tags.Contains<Identifier>))))
							{
								primaryContainers.Add(container);
							}
							else if (prefab.PreferredContainers.Any((PreferredContainer pc) => pc.Secondary.Any(new Func<Identifier, bool>(tags.Contains<Identifier>))))
							{
								secondaryContainers.Add(container);
							}
						}
					}
				}
			}
			return new SpawnItemPerk.SuitableContainers(priorityContainers, primaryContainers, secondaryContainers);
		}

		// Token: 0x060045D7 RID: 17879 RVA: 0x00269E64 File Offset: 0x00268064
		private static void SpawnItemInCrate(ItemPrefab prefab, Submarine submarine, int amount)
		{
			PurchasedItem purchasedItem = new PurchasedItem(prefab, amount, null);
			CargoManager.DeliverItemsToSub(new PurchasedItem[]
			{
				purchasedItem
			}, submarine, null, false);
		}

		// Token: 0x060045D8 RID: 17880 RVA: 0x00269E8C File Offset: 0x0026808C
		private static void SpawnInContainer(ItemPrefab prefab, int amount, SpawnItemPerk.SuitableContainers containers, Submarine submarine)
		{
			SpawnItemPerk.<>c__DisplayClass27_0 CS$<>8__locals1;
			CS$<>8__locals1.prefab = prefab;
			CS$<>8__locals1.containerAllocation = new Dictionary<ItemContainer, int>();
			CS$<>8__locals1.remaining = amount;
			SpawnItemPerk.<SpawnInContainer>g__TryAllocate|27_0(containers.PriorityContainers, ref CS$<>8__locals1);
			if (CS$<>8__locals1.remaining > 0)
			{
				SpawnItemPerk.<SpawnInContainer>g__TryAllocate|27_0(containers.PreferredContainers, ref CS$<>8__locals1);
				if (CS$<>8__locals1.remaining > 0)
				{
					SpawnItemPerk.<SpawnInContainer>g__TryAllocate|27_0(containers.SecondaryContainers, ref CS$<>8__locals1);
				}
			}
			foreach (KeyValuePair<ItemContainer, int> keyValuePair in CS$<>8__locals1.containerAllocation)
			{
				ItemContainer itemContainer;
				int num;
				keyValuePair.Deconstruct(out itemContainer, out num);
				ItemContainer container = itemContainer;
				int howManyToPut = num;
				for (int i = 0; i < howManyToPut; i++)
				{
					SpawnItemPerk.<SpawnInContainer>g__SpawnItem|27_2(CS$<>8__locals1.prefab, container);
				}
			}
			if (CS$<>8__locals1.remaining > 0)
			{
				SpawnItemPerk.SpawnItemInCrate(CS$<>8__locals1.prefab, submarine, CS$<>8__locals1.remaining);
			}
		}

		// Token: 0x060045D9 RID: 17881 RVA: 0x00269F7C File Offset: 0x0026817C
		[CompilerGenerated]
		internal static void <SpawnInContainer>g__TryAllocate|27_0(ICollection<ItemContainer> targetContainers, ref SpawnItemPerk.<>c__DisplayClass27_0 A_1)
		{
			SpawnItemPerk.<SpawnInContainer>g__AllocateContainers|27_1(A_1.prefab, targetContainers, ref A_1.remaining, ref A_1.containerAllocation);
		}

		// Token: 0x060045DA RID: 17882 RVA: 0x00269F98 File Offset: 0x00268198
		[CompilerGenerated]
		internal static void <SpawnInContainer>g__AllocateContainers|27_1(ItemPrefab prefab, ICollection<ItemContainer> containers, ref int remaining, ref Dictionary<ItemContainer, int> containerAllocation)
		{
			foreach (ItemContainer ic in containers)
			{
				int fit = ic.Inventory.HowManyCanBePut(prefab, null);
				if (fit > 0)
				{
					fit = Math.Min(fit, remaining);
					containerAllocation.Add(ic, fit);
					remaining -= fit;
					if (remaining <= 0)
					{
						break;
					}
				}
			}
		}

		// Token: 0x060045DB RID: 17883 RVA: 0x0026A014 File Offset: 0x00268214
		[CompilerGenerated]
		internal static void <SpawnInContainer>g__SpawnItem|27_2(ItemPrefab itemPrefab, ItemContainer container)
		{
			if (((container != null) ? container.Item : null) == null)
			{
				return;
			}
			Item item = new Item(itemPrefab, container.Item.Position, container.Item.Submarine, 0, true);
			container.Inventory.TryPutItem(item, null, null, true, false, true);
			CargoManager.ItemSpawned(item);
		}

		// Token: 0x020010D1 RID: 4305
		private readonly struct SuitableContainers : IEquatable<SpawnItemPerk.SuitableContainers>
		{
			// Token: 0x06008DF6 RID: 36342 RVA: 0x003B3057 File Offset: 0x003B1257
			public SuitableContainers(ICollection<ItemContainer> PriorityContainers, ICollection<ItemContainer> PreferredContainers, ICollection<ItemContainer> SecondaryContainers)
			{
				this.PriorityContainers = PriorityContainers;
				this.PreferredContainers = PreferredContainers;
				this.SecondaryContainers = SecondaryContainers;
			}

			// Token: 0x17001C90 RID: 7312
			// (get) Token: 0x06008DF7 RID: 36343 RVA: 0x003B306E File Offset: 0x003B126E
			// (set) Token: 0x06008DF8 RID: 36344 RVA: 0x003B3076 File Offset: 0x003B1276
			public ICollection<ItemContainer> PriorityContainers { get; set; }

			// Token: 0x17001C91 RID: 7313
			// (get) Token: 0x06008DF9 RID: 36345 RVA: 0x003B307F File Offset: 0x003B127F
			// (set) Token: 0x06008DFA RID: 36346 RVA: 0x003B3087 File Offset: 0x003B1287
			public ICollection<ItemContainer> PreferredContainers { get; set; }

			// Token: 0x17001C92 RID: 7314
			// (get) Token: 0x06008DFB RID: 36347 RVA: 0x003B3090 File Offset: 0x003B1290
			// (set) Token: 0x06008DFC RID: 36348 RVA: 0x003B3098 File Offset: 0x003B1298
			public ICollection<ItemContainer> SecondaryContainers { get; set; }

			// Token: 0x06008DFD RID: 36349 RVA: 0x003B30A1 File Offset: 0x003B12A1
			public bool Any()
			{
				return this.PriorityContainers.Count > 0 || this.PreferredContainers.Count > 0 || this.SecondaryContainers.Count > 0;
			}

			// Token: 0x06008DFE RID: 36350 RVA: 0x003B30D0 File Offset: 0x003B12D0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SuitableContainers");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06008DFF RID: 36351 RVA: 0x003B311C File Offset: 0x003B131C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("PriorityContainers = ");
				builder.Append(this.PriorityContainers);
				builder.Append(", PreferredContainers = ");
				builder.Append(this.PreferredContainers);
				builder.Append(", SecondaryContainers = ");
				builder.Append(this.SecondaryContainers);
				return true;
			}

			// Token: 0x06008E00 RID: 36352 RVA: 0x003B3175 File Offset: 0x003B1375
			[CompilerGenerated]
			public static bool operator !=(SpawnItemPerk.SuitableContainers left, SpawnItemPerk.SuitableContainers right)
			{
				return !(left == right);
			}

			// Token: 0x06008E01 RID: 36353 RVA: 0x003B3181 File Offset: 0x003B1381
			[CompilerGenerated]
			public static bool operator ==(SpawnItemPerk.SuitableContainers left, SpawnItemPerk.SuitableContainers right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008E02 RID: 36354 RVA: 0x003B318B File Offset: 0x003B138B
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ICollection<ItemContainer>>.Default.GetHashCode(this.<PriorityContainers>k__BackingField) * -1521134295 + EqualityComparer<ICollection<ItemContainer>>.Default.GetHashCode(this.<PreferredContainers>k__BackingField)) * -1521134295 + EqualityComparer<ICollection<ItemContainer>>.Default.GetHashCode(this.<SecondaryContainers>k__BackingField);
			}

			// Token: 0x06008E03 RID: 36355 RVA: 0x003B31CB File Offset: 0x003B13CB
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SpawnItemPerk.SuitableContainers && this.Equals((SpawnItemPerk.SuitableContainers)obj);
			}

			// Token: 0x06008E04 RID: 36356 RVA: 0x003B31E4 File Offset: 0x003B13E4
			[CompilerGenerated]
			public bool Equals(SpawnItemPerk.SuitableContainers other)
			{
				return EqualityComparer<ICollection<ItemContainer>>.Default.Equals(this.<PriorityContainers>k__BackingField, other.<PriorityContainers>k__BackingField) && EqualityComparer<ICollection<ItemContainer>>.Default.Equals(this.<PreferredContainers>k__BackingField, other.<PreferredContainers>k__BackingField) && EqualityComparer<ICollection<ItemContainer>>.Default.Equals(this.<SecondaryContainers>k__BackingField, other.<SecondaryContainers>k__BackingField);
			}

			// Token: 0x06008E05 RID: 36357 RVA: 0x003B3239 File Offset: 0x003B1439
			[CompilerGenerated]
			public void Deconstruct(out ICollection<ItemContainer> PriorityContainers, out ICollection<ItemContainer> PreferredContainers, out ICollection<ItemContainer> SecondaryContainers)
			{
				PriorityContainers = this.PriorityContainers;
				PreferredContainers = this.PreferredContainers;
				SecondaryContainers = this.SecondaryContainers;
			}
		}
	}
}

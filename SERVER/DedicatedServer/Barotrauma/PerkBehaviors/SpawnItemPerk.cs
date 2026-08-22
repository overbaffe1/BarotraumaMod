using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma.PerkBehaviors
{
	// Token: 0x020002E9 RID: 745
	internal class SpawnItemPerk : PerkBase
	{
		// Token: 0x0600318F RID: 12687 RVA: 0x00151B88 File Offset: 0x0014FD88
		public SpawnItemPerk(ContentXElement element, DisembarkPerkPrefab prefab) : base(element, prefab)
		{
		}

		// Token: 0x17000E0B RID: 3595
		// (get) Token: 0x06003190 RID: 12688 RVA: 0x00151B92 File Offset: 0x0014FD92
		public override PerkSimulation Simulation
		{
			get
			{
				return PerkSimulation.ServerOnly;
			}
		}

		// Token: 0x17000E0C RID: 3596
		// (get) Token: 0x06003191 RID: 12689 RVA: 0x00151B95 File Offset: 0x0014FD95
		// (set) Token: 0x06003192 RID: 12690 RVA: 0x00151B9D File Offset: 0x0014FD9D
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x17000E0D RID: 3597
		// (get) Token: 0x06003193 RID: 12691 RVA: 0x00151BA6 File Offset: 0x0014FDA6
		// (set) Token: 0x06003194 RID: 12692 RVA: 0x00151BAE File Offset: 0x0014FDAE
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier Tag { get; set; }

		// Token: 0x17000E0E RID: 3598
		// (get) Token: 0x06003195 RID: 12693 RVA: 0x00151BB7 File Offset: 0x0014FDB7
		// (set) Token: 0x06003196 RID: 12694 RVA: 0x00151BBF File Offset: 0x0014FDBF
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int MinAmount { get; set; }

		// Token: 0x17000E0F RID: 3599
		// (get) Token: 0x06003197 RID: 12695 RVA: 0x00151BC8 File Offset: 0x0014FDC8
		// (set) Token: 0x06003198 RID: 12696 RVA: 0x00151BD0 File Offset: 0x0014FDD0
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float PerPlayer { get; set; }

		// Token: 0x17000E10 RID: 3600
		// (get) Token: 0x06003199 RID: 12697 RVA: 0x00151BD9 File Offset: 0x0014FDD9
		// (set) Token: 0x0600319A RID: 12698 RVA: 0x00151BE1 File Offset: 0x0014FDE1
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier PriorityContainerTag { get; set; }

		// Token: 0x0600319B RID: 12699 RVA: 0x00151BEC File Offset: 0x0014FDEC
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

		// Token: 0x0600319C RID: 12700 RVA: 0x00151E50 File Offset: 0x00150050
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

		// Token: 0x0600319D RID: 12701 RVA: 0x00151FD0 File Offset: 0x001501D0
		private static void SpawnItemInCrate(ItemPrefab prefab, Submarine submarine, int amount)
		{
			PurchasedItem purchasedItem = new PurchasedItem(prefab, amount, null);
			CargoManager.DeliverItemsToSub(new PurchasedItem[]
			{
				purchasedItem
			}, submarine, null, false);
		}

		// Token: 0x0600319E RID: 12702 RVA: 0x00151FF8 File Offset: 0x001501F8
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

		// Token: 0x0600319F RID: 12703 RVA: 0x001520E8 File Offset: 0x001502E8
		[CompilerGenerated]
		internal static void <SpawnInContainer>g__TryAllocate|27_0(ICollection<ItemContainer> targetContainers, ref SpawnItemPerk.<>c__DisplayClass27_0 A_1)
		{
			SpawnItemPerk.<SpawnInContainer>g__AllocateContainers|27_1(A_1.prefab, targetContainers, ref A_1.remaining, ref A_1.containerAllocation);
		}

		// Token: 0x060031A0 RID: 12704 RVA: 0x00152104 File Offset: 0x00150304
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

		// Token: 0x060031A1 RID: 12705 RVA: 0x00152180 File Offset: 0x00150380
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
			EntitySpawner spawner = Entity.Spawner;
			if (spawner == null)
			{
				return;
			}
			spawner.CreateNetworkEvent(new EntitySpawner.SpawnEntity(item));
		}

		// Token: 0x02000B88 RID: 2952
		private readonly struct SuitableContainers : IEquatable<SpawnItemPerk.SuitableContainers>
		{
			// Token: 0x06006126 RID: 24870 RVA: 0x0020B9B0 File Offset: 0x00209BB0
			public SuitableContainers(ICollection<ItemContainer> PriorityContainers, ICollection<ItemContainer> PreferredContainers, ICollection<ItemContainer> SecondaryContainers)
			{
				this.PriorityContainers = PriorityContainers;
				this.PreferredContainers = PreferredContainers;
				this.SecondaryContainers = SecondaryContainers;
			}

			// Token: 0x170015F8 RID: 5624
			// (get) Token: 0x06006127 RID: 24871 RVA: 0x0020B9C7 File Offset: 0x00209BC7
			// (set) Token: 0x06006128 RID: 24872 RVA: 0x0020B9CF File Offset: 0x00209BCF
			public ICollection<ItemContainer> PriorityContainers { get; set; }

			// Token: 0x170015F9 RID: 5625
			// (get) Token: 0x06006129 RID: 24873 RVA: 0x0020B9D8 File Offset: 0x00209BD8
			// (set) Token: 0x0600612A RID: 24874 RVA: 0x0020B9E0 File Offset: 0x00209BE0
			public ICollection<ItemContainer> PreferredContainers { get; set; }

			// Token: 0x170015FA RID: 5626
			// (get) Token: 0x0600612B RID: 24875 RVA: 0x0020B9E9 File Offset: 0x00209BE9
			// (set) Token: 0x0600612C RID: 24876 RVA: 0x0020B9F1 File Offset: 0x00209BF1
			public ICollection<ItemContainer> SecondaryContainers { get; set; }

			// Token: 0x0600612D RID: 24877 RVA: 0x0020B9FA File Offset: 0x00209BFA
			public bool Any()
			{
				return this.PriorityContainers.Count > 0 || this.PreferredContainers.Count > 0 || this.SecondaryContainers.Count > 0;
			}

			// Token: 0x0600612E RID: 24878 RVA: 0x0020BA28 File Offset: 0x00209C28
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

			// Token: 0x0600612F RID: 24879 RVA: 0x0020BA74 File Offset: 0x00209C74
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

			// Token: 0x06006130 RID: 24880 RVA: 0x0020BACD File Offset: 0x00209CCD
			[CompilerGenerated]
			public static bool operator !=(SpawnItemPerk.SuitableContainers left, SpawnItemPerk.SuitableContainers right)
			{
				return !(left == right);
			}

			// Token: 0x06006131 RID: 24881 RVA: 0x0020BAD9 File Offset: 0x00209CD9
			[CompilerGenerated]
			public static bool operator ==(SpawnItemPerk.SuitableContainers left, SpawnItemPerk.SuitableContainers right)
			{
				return left.Equals(right);
			}

			// Token: 0x06006132 RID: 24882 RVA: 0x0020BAE3 File Offset: 0x00209CE3
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ICollection<ItemContainer>>.Default.GetHashCode(this.<PriorityContainers>k__BackingField) * -1521134295 + EqualityComparer<ICollection<ItemContainer>>.Default.GetHashCode(this.<PreferredContainers>k__BackingField)) * -1521134295 + EqualityComparer<ICollection<ItemContainer>>.Default.GetHashCode(this.<SecondaryContainers>k__BackingField);
			}

			// Token: 0x06006133 RID: 24883 RVA: 0x0020BB23 File Offset: 0x00209D23
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SpawnItemPerk.SuitableContainers && this.Equals((SpawnItemPerk.SuitableContainers)obj);
			}

			// Token: 0x06006134 RID: 24884 RVA: 0x0020BB3C File Offset: 0x00209D3C
			[CompilerGenerated]
			public bool Equals(SpawnItemPerk.SuitableContainers other)
			{
				return EqualityComparer<ICollection<ItemContainer>>.Default.Equals(this.<PriorityContainers>k__BackingField, other.<PriorityContainers>k__BackingField) && EqualityComparer<ICollection<ItemContainer>>.Default.Equals(this.<PreferredContainers>k__BackingField, other.<PreferredContainers>k__BackingField) && EqualityComparer<ICollection<ItemContainer>>.Default.Equals(this.<SecondaryContainers>k__BackingField, other.<SecondaryContainers>k__BackingField);
			}

			// Token: 0x06006135 RID: 24885 RVA: 0x0020BB91 File Offset: 0x00209D91
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

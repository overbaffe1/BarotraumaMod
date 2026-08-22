using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001FB RID: 507
	internal class ItemPrefab : MapEntityPrefab, IImplementsVariants<ItemPrefab>
	{
		// Token: 0x17000A4F RID: 2639
		// (get) Token: 0x060023D0 RID: 9168 RVA: 0x000EF998 File Offset: 0x000EDB98
		// (set) Token: 0x060023D1 RID: 9169 RVA: 0x000EF9A0 File Offset: 0x000EDBA0
		public Vector2 Size { get; private set; }

		// Token: 0x17000A50 RID: 2640
		// (get) Token: 0x060023D2 RID: 9170 RVA: 0x000EF9A9 File Offset: 0x000EDBA9
		public PriceInfo DefaultPrice
		{
			get
			{
				return this.defaultPrice;
			}
		}

		// Token: 0x17000A51 RID: 2641
		// (get) Token: 0x060023D3 RID: 9171 RVA: 0x000EF9B1 File Offset: 0x000EDBB1
		// (set) Token: 0x060023D4 RID: 9172 RVA: 0x000EF9B9 File Offset: 0x000EDBB9
		private ImmutableDictionary<Identifier, PriceInfo> StorePrices { get; set; }

		// Token: 0x17000A52 RID: 2642
		// (get) Token: 0x060023D5 RID: 9173 RVA: 0x000EF9C4 File Offset: 0x000EDBC4
		public bool CanBeBought
		{
			get
			{
				if (this.DefaultPrice != null && this.DefaultPrice.CanBeBought)
				{
					return true;
				}
				if (this.StorePrices != null)
				{
					return this.StorePrices.Any((KeyValuePair<Identifier, PriceInfo> p) => p.Value.CanBeBought);
				}
				return false;
			}
		}

		// Token: 0x17000A53 RID: 2643
		// (get) Token: 0x060023D6 RID: 9174 RVA: 0x000EFA1C File Offset: 0x000EDC1C
		public bool CanBeSold
		{
			get
			{
				return this.DefaultPrice != null;
			}
		}

		// Token: 0x17000A54 RID: 2644
		// (get) Token: 0x060023D7 RID: 9175 RVA: 0x000EFA27 File Offset: 0x000EDC27
		// (set) Token: 0x060023D8 RID: 9176 RVA: 0x000EFA2F File Offset: 0x000EDC2F
		public ImmutableArray<Rectangle> Triggers { get; private set; }

		// Token: 0x17000A55 RID: 2645
		// (get) Token: 0x060023D9 RID: 9177 RVA: 0x000EFA38 File Offset: 0x000EDC38
		public bool IsOverride
		{
			get
			{
				return ItemPrefab.Prefabs.IsOverride(this);
			}
		}

		// Token: 0x17000A56 RID: 2646
		// (get) Token: 0x060023DA RID: 9178 RVA: 0x000EFA45 File Offset: 0x000EDC45
		// (set) Token: 0x060023DB RID: 9179 RVA: 0x000EFA4D File Offset: 0x000EDC4D
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x17000A57 RID: 2647
		// (get) Token: 0x060023DC RID: 9180 RVA: 0x000EFA56 File Offset: 0x000EDC56
		// (set) Token: 0x060023DD RID: 9181 RVA: 0x000EFA5E File Offset: 0x000EDC5E
		public ImmutableArray<DeconstructItem> DeconstructItems { get; private set; }

		// Token: 0x17000A58 RID: 2648
		// (get) Token: 0x060023DE RID: 9182 RVA: 0x000EFA67 File Offset: 0x000EDC67
		// (set) Token: 0x060023DF RID: 9183 RVA: 0x000EFA6F File Offset: 0x000EDC6F
		public ImmutableDictionary<uint, FabricationRecipe> FabricationRecipes { get; private set; }

		// Token: 0x17000A59 RID: 2649
		// (get) Token: 0x060023E0 RID: 9184 RVA: 0x000EFA78 File Offset: 0x000EDC78
		// (set) Token: 0x060023E1 RID: 9185 RVA: 0x000EFA80 File Offset: 0x000EDC80
		public float DeconstructTime { get; private set; }

		// Token: 0x17000A5A RID: 2650
		// (get) Token: 0x060023E2 RID: 9186 RVA: 0x000EFA89 File Offset: 0x000EDC89
		// (set) Token: 0x060023E3 RID: 9187 RVA: 0x000EFA91 File Offset: 0x000EDC91
		public float DeconstructTimeInOutposts { get; private set; }

		// Token: 0x17000A5B RID: 2651
		// (get) Token: 0x060023E4 RID: 9188 RVA: 0x000EFA9A File Offset: 0x000EDC9A
		// (set) Token: 0x060023E5 RID: 9189 RVA: 0x000EFAA2 File Offset: 0x000EDCA2
		public bool AllowDeconstruct { get; private set; }

		// Token: 0x17000A5C RID: 2652
		// (get) Token: 0x060023E6 RID: 9190 RVA: 0x000EFAAB File Offset: 0x000EDCAB
		// (set) Token: 0x060023E7 RID: 9191 RVA: 0x000EFAB3 File Offset: 0x000EDCB3
		public ImmutableArray<PreferredContainer> PreferredContainers { get; private set; }

		// Token: 0x17000A5D RID: 2653
		// (get) Token: 0x060023E8 RID: 9192 RVA: 0x000EFABC File Offset: 0x000EDCBC
		// (set) Token: 0x060023E9 RID: 9193 RVA: 0x000EFAC4 File Offset: 0x000EDCC4
		public ImmutableArray<SkillRequirementHint> SkillRequirementHints { get; private set; }

		// Token: 0x17000A5E RID: 2654
		// (get) Token: 0x060023EA RID: 9194 RVA: 0x000EFACD File Offset: 0x000EDCCD
		// (set) Token: 0x060023EB RID: 9195 RVA: 0x000EFAD5 File Offset: 0x000EDCD5
		public SwappableItem SwappableItem { get; private set; }

		// Token: 0x17000A5F RID: 2655
		// (get) Token: 0x060023EC RID: 9196 RVA: 0x000EFADE File Offset: 0x000EDCDE
		// (set) Token: 0x060023ED RID: 9197 RVA: 0x000EFAE6 File Offset: 0x000EDCE6
		private ImmutableDictionary<Identifier, ItemPrefab.CommonnessInfo> LevelCommonness { get; set; }

		// Token: 0x17000A60 RID: 2656
		// (get) Token: 0x060023EE RID: 9198 RVA: 0x000EFAEF File Offset: 0x000EDCEF
		// (set) Token: 0x060023EF RID: 9199 RVA: 0x000EFAF7 File Offset: 0x000EDCF7
		public ImmutableDictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo> LevelQuantity { get; private set; }

		// Token: 0x17000A61 RID: 2657
		// (get) Token: 0x060023F0 RID: 9200 RVA: 0x000EFB00 File Offset: 0x000EDD00
		public override bool CanSpriteFlipX
		{
			get
			{
				return this.canSpriteFlipX;
			}
		}

		// Token: 0x17000A62 RID: 2658
		// (get) Token: 0x060023F1 RID: 9201 RVA: 0x000EFB08 File Offset: 0x000EDD08
		public override bool CanSpriteFlipY
		{
			get
			{
				return this.canSpriteFlipY;
			}
		}

		// Token: 0x17000A63 RID: 2659
		// (get) Token: 0x060023F2 RID: 9202 RVA: 0x000EFB10 File Offset: 0x000EDD10
		// (set) Token: 0x060023F3 RID: 9203 RVA: 0x000EFB18 File Offset: 0x000EDD18
		public bool? AllowAsExtraCargo { get; private set; }

		// Token: 0x17000A64 RID: 2660
		// (get) Token: 0x060023F4 RID: 9204 RVA: 0x000EFB21 File Offset: 0x000EDD21
		// (set) Token: 0x060023F5 RID: 9205 RVA: 0x000EFB29 File Offset: 0x000EDD29
		public bool RandomDeconstructionOutput { get; private set; }

		// Token: 0x17000A65 RID: 2661
		// (get) Token: 0x060023F6 RID: 9206 RVA: 0x000EFB32 File Offset: 0x000EDD32
		// (set) Token: 0x060023F7 RID: 9207 RVA: 0x000EFB3A File Offset: 0x000EDD3A
		public int RandomDeconstructionOutputAmount { get; private set; }

		// Token: 0x17000A66 RID: 2662
		// (get) Token: 0x060023F8 RID: 9208 RVA: 0x000EFB43 File Offset: 0x000EDD43
		public override Sprite Sprite
		{
			get
			{
				return this.sprite;
			}
		}

		// Token: 0x17000A67 RID: 2663
		// (get) Token: 0x060023F9 RID: 9209 RVA: 0x000EFB4B File Offset: 0x000EDD4B
		public override string OriginalName { get; }

		// Token: 0x17000A68 RID: 2664
		// (get) Token: 0x060023FA RID: 9210 RVA: 0x000EFB53 File Offset: 0x000EDD53
		public override LocalizedString Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000A69 RID: 2665
		// (get) Token: 0x060023FB RID: 9211 RVA: 0x000EFB5B File Offset: 0x000EDD5B
		public override ImmutableHashSet<Identifier> Tags
		{
			get
			{
				return this.tags;
			}
		}

		// Token: 0x17000A6A RID: 2666
		// (get) Token: 0x060023FC RID: 9212 RVA: 0x000EFB63 File Offset: 0x000EDD63
		public override ImmutableHashSet<Identifier> AllowedLinks
		{
			get
			{
				return this.allowedLinks;
			}
		}

		// Token: 0x17000A6B RID: 2667
		// (get) Token: 0x060023FD RID: 9213 RVA: 0x000EFB6B File Offset: 0x000EDD6B
		public override MapEntityCategory Category
		{
			get
			{
				return this.category;
			}
		}

		// Token: 0x17000A6C RID: 2668
		// (get) Token: 0x060023FE RID: 9214 RVA: 0x000EFB73 File Offset: 0x000EDD73
		public override ImmutableHashSet<string> Aliases
		{
			get
			{
				return this.aliases;
			}
		}

		// Token: 0x17000A6D RID: 2669
		// (get) Token: 0x060023FF RID: 9215 RVA: 0x000EFB7B File Offset: 0x000EDD7B
		// (set) Token: 0x06002400 RID: 9216 RVA: 0x000EFB83 File Offset: 0x000EDD83
		[Serialize(120f, IsPropertySaveable.No, "", "", false)]
		public float InteractDistance { get; private set; }

		// Token: 0x17000A6E RID: 2670
		// (get) Token: 0x06002401 RID: 9217 RVA: 0x000EFB8C File Offset: 0x000EDD8C
		// (set) Token: 0x06002402 RID: 9218 RVA: 0x000EFB94 File Offset: 0x000EDD94
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float InteractPriority { get; private set; }

		// Token: 0x17000A6F RID: 2671
		// (get) Token: 0x06002403 RID: 9219 RVA: 0x000EFB9D File Offset: 0x000EDD9D
		// (set) Token: 0x06002404 RID: 9220 RVA: 0x000EFBA5 File Offset: 0x000EDDA5
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool InteractThroughWalls { get; private set; }

		// Token: 0x17000A70 RID: 2672
		// (get) Token: 0x06002405 RID: 9221 RVA: 0x000EFBAE File Offset: 0x000EDDAE
		// (set) Token: 0x06002406 RID: 9222 RVA: 0x000EFBB6 File Offset: 0x000EDDB6
		[Serialize(false, IsPropertySaveable.No, "Hides the condition bar displayed at the bottom of the inventory slot the item is in.", "", false)]
		public bool HideConditionBar { get; set; }

		// Token: 0x17000A71 RID: 2673
		// (get) Token: 0x06002407 RID: 9223 RVA: 0x000EFBBF File Offset: 0x000EDDBF
		// (set) Token: 0x06002408 RID: 9224 RVA: 0x000EFBC7 File Offset: 0x000EDDC7
		[Serialize(false, IsPropertySaveable.No, "Hides the condition displayed in the item's tooltip.", "", false)]
		public bool HideConditionInTooltip { get; set; }

		// Token: 0x17000A72 RID: 2674
		// (get) Token: 0x06002409 RID: 9225 RVA: 0x000EFBD0 File Offset: 0x000EDDD0
		// (set) Token: 0x0600240A RID: 9226 RVA: 0x000EFBD8 File Offset: 0x000EDDD8
		[Serialize("", IsPropertySaveable.No, "If set, the item's tooltip displays if the given fabrication recipe has been unlocked or not. The actual unlocking of the recipe should be handled in a status effect.", "", false)]
		public Identifier[] UnlockedRecipeInToolTip { get; set; }

		// Token: 0x17000A73 RID: 2675
		// (get) Token: 0x0600240B RID: 9227 RVA: 0x000EFBE1 File Offset: 0x000EDDE1
		// (set) Token: 0x0600240C RID: 9228 RVA: 0x000EFBE9 File Offset: 0x000EDDE9
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool RequireBodyInsideTrigger { get; private set; }

		// Token: 0x17000A74 RID: 2676
		// (get) Token: 0x0600240D RID: 9229 RVA: 0x000EFBF2 File Offset: 0x000EDDF2
		// (set) Token: 0x0600240E RID: 9230 RVA: 0x000EFBFA File Offset: 0x000EDDFA
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RequireCursorInsideTrigger { get; private set; }

		// Token: 0x17000A75 RID: 2677
		// (get) Token: 0x0600240F RID: 9231 RVA: 0x000EFC03 File Offset: 0x000EDE03
		// (set) Token: 0x06002410 RID: 9232 RVA: 0x000EFC0B File Offset: 0x000EDE0B
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool RequireCampaignInteract { get; private set; }

		// Token: 0x17000A76 RID: 2678
		// (get) Token: 0x06002411 RID: 9233 RVA: 0x000EFC14 File Offset: 0x000EDE14
		// (set) Token: 0x06002412 RID: 9234 RVA: 0x000EFC1C File Offset: 0x000EDE1C
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool FocusOnSelected { get; private set; }

		// Token: 0x17000A77 RID: 2679
		// (get) Token: 0x06002413 RID: 9235 RVA: 0x000EFC25 File Offset: 0x000EDE25
		// (set) Token: 0x06002414 RID: 9236 RVA: 0x000EFC2D File Offset: 0x000EDE2D
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float OffsetOnSelected { get; private set; }

		// Token: 0x17000A78 RID: 2680
		// (get) Token: 0x06002415 RID: 9237 RVA: 0x000EFC36 File Offset: 0x000EDE36
		// (set) Token: 0x06002416 RID: 9238 RVA: 0x000EFC3E File Offset: 0x000EDE3E
		[Serialize(false, IsPropertySaveable.No, "Should the character who's selected the item grab it (hold their hand on it, the same way as they do when repairing)? Defaults to true on items that have an ItemContainer component.", "", false)]
		public bool GrabWhenSelected { get; set; }

		// Token: 0x17000A79 RID: 2681
		// (get) Token: 0x06002417 RID: 9239 RVA: 0x000EFC47 File Offset: 0x000EDE47
		// (set) Token: 0x06002418 RID: 9240 RVA: 0x000EFC4F File Offset: 0x000EDE4F
		[Serialize(true, IsPropertySaveable.No, "Are AI characters allowed to deselect the item when they're idling (and wander off?).", "", false)]
		public bool AllowDeselectWhenIdling { get; private set; }

		// Token: 0x17000A7A RID: 2682
		// (get) Token: 0x06002419 RID: 9241 RVA: 0x000EFC58 File Offset: 0x000EDE58
		// (set) Token: 0x0600241A RID: 9242 RVA: 0x000EFC60 File Offset: 0x000EDE60
		[Serialize(100f, IsPropertySaveable.No, "", "", false)]
		public float Health
		{
			get
			{
				return this.health;
			}
			private set
			{
				this.health = Math.Min(value, 1000000f);
			}
		}

		// Token: 0x17000A7B RID: 2683
		// (get) Token: 0x0600241B RID: 9243 RVA: 0x000EFC73 File Offset: 0x000EDE73
		// (set) Token: 0x0600241C RID: 9244 RVA: 0x000EFC7B File Offset: 0x000EDE7B
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowSellingWhenBroken { get; private set; }

		// Token: 0x17000A7C RID: 2684
		// (get) Token: 0x0600241D RID: 9245 RVA: 0x000EFC84 File Offset: 0x000EDE84
		// (set) Token: 0x0600241E RID: 9246 RVA: 0x000EFC8C File Offset: 0x000EDE8C
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowStealingAlways { get; private set; }

		// Token: 0x17000A7D RID: 2685
		// (get) Token: 0x0600241F RID: 9247 RVA: 0x000EFC95 File Offset: 0x000EDE95
		// (set) Token: 0x06002420 RID: 9248 RVA: 0x000EFC9D File Offset: 0x000EDE9D
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool Indestructible { get; private set; }

		// Token: 0x17000A7E RID: 2686
		// (get) Token: 0x06002421 RID: 9249 RVA: 0x000EFCA6 File Offset: 0x000EDEA6
		// (set) Token: 0x06002422 RID: 9250 RVA: 0x000EFCAE File Offset: 0x000EDEAE
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByExplosions { get; private set; }

		// Token: 0x17000A7F RID: 2687
		// (get) Token: 0x06002423 RID: 9251 RVA: 0x000EFCB7 File Offset: 0x000EDEB7
		// (set) Token: 0x06002424 RID: 9252 RVA: 0x000EFCBF File Offset: 0x000EDEBF
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByContainedItemExplosions { get; private set; }

		// Token: 0x17000A80 RID: 2688
		// (get) Token: 0x06002425 RID: 9253 RVA: 0x000EFCC8 File Offset: 0x000EDEC8
		// (set) Token: 0x06002426 RID: 9254 RVA: 0x000EFCD0 File Offset: 0x000EDED0
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float ExplosionDamageMultiplier { get; private set; }

		// Token: 0x17000A81 RID: 2689
		// (get) Token: 0x06002427 RID: 9255 RVA: 0x000EFCD9 File Offset: 0x000EDED9
		// (set) Token: 0x06002428 RID: 9256 RVA: 0x000EFCE1 File Offset: 0x000EDEE1
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		public float ItemDamageMultiplier { get; private set; }

		// Token: 0x17000A82 RID: 2690
		// (get) Token: 0x06002429 RID: 9257 RVA: 0x000EFCEA File Offset: 0x000EDEEA
		// (set) Token: 0x0600242A RID: 9258 RVA: 0x000EFCF2 File Offset: 0x000EDEF2
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByProjectiles { get; private set; }

		// Token: 0x17000A83 RID: 2691
		// (get) Token: 0x0600242B RID: 9259 RVA: 0x000EFCFB File Offset: 0x000EDEFB
		// (set) Token: 0x0600242C RID: 9260 RVA: 0x000EFD03 File Offset: 0x000EDF03
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByMeleeWeapons { get; private set; }

		// Token: 0x17000A84 RID: 2692
		// (get) Token: 0x0600242D RID: 9261 RVA: 0x000EFD0C File Offset: 0x000EDF0C
		// (set) Token: 0x0600242E RID: 9262 RVA: 0x000EFD14 File Offset: 0x000EDF14
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByRepairTools { get; private set; }

		// Token: 0x17000A85 RID: 2693
		// (get) Token: 0x0600242F RID: 9263 RVA: 0x000EFD1D File Offset: 0x000EDF1D
		// (set) Token: 0x06002430 RID: 9264 RVA: 0x000EFD25 File Offset: 0x000EDF25
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DamagedByMonsters { get; private set; }

		// Token: 0x17000A86 RID: 2694
		// (get) Token: 0x06002431 RID: 9265 RVA: 0x000EFD2E File Offset: 0x000EDF2E
		// (set) Token: 0x06002432 RID: 9266 RVA: 0x000EFD36 File Offset: 0x000EDF36
		[Serialize(false, IsPropertySaveable.No, "If true, submarine impacts will trigger OnImpact effects. Only applies to items with a null or non-dynamic physics body - items with dynamic bodies always react to impacts.", "", false)]
		public bool ReceiveSubmarineImpacts { get; set; }

		// Token: 0x17000A87 RID: 2695
		// (get) Token: 0x06002433 RID: 9267 RVA: 0x000EFD3F File Offset: 0x000EDF3F
		// (set) Token: 0x06002434 RID: 9268 RVA: 0x000EFD47 File Offset: 0x000EDF47
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float OnDamagedThreshold { get; set; }

		// Token: 0x17000A88 RID: 2696
		// (get) Token: 0x06002435 RID: 9269 RVA: 0x000EFD50 File Offset: 0x000EDF50
		// (set) Token: 0x06002436 RID: 9270 RVA: 0x000EFD58 File Offset: 0x000EDF58
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float SonarSize { get; private set; }

		// Token: 0x17000A89 RID: 2697
		// (get) Token: 0x06002437 RID: 9271 RVA: 0x000EFD61 File Offset: 0x000EDF61
		// (set) Token: 0x06002438 RID: 9272 RVA: 0x000EFD69 File Offset: 0x000EDF69
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool UseInHealthInterface { get; private set; }

		// Token: 0x17000A8A RID: 2698
		// (get) Token: 0x06002439 RID: 9273 RVA: 0x000EFD72 File Offset: 0x000EDF72
		// (set) Token: 0x0600243A RID: 9274 RVA: 0x000EFD7A File Offset: 0x000EDF7A
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DisableItemUsageWhenSelected { get; private set; }

		// Token: 0x17000A8B RID: 2699
		// (get) Token: 0x0600243B RID: 9275 RVA: 0x000EFD83 File Offset: 0x000EDF83
		// (set) Token: 0x0600243C RID: 9276 RVA: 0x000EFD8B File Offset: 0x000EDF8B
		[Serialize("metalcrate", IsPropertySaveable.No, "", "", false)]
		public string CargoContainerIdentifier { get; private set; }

		// Token: 0x17000A8C RID: 2700
		// (get) Token: 0x0600243D RID: 9277 RVA: 0x000EFD94 File Offset: 0x000EDF94
		// (set) Token: 0x0600243E RID: 9278 RVA: 0x000EFD9C File Offset: 0x000EDF9C
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool UseContainedSpriteColor { get; private set; }

		// Token: 0x17000A8D RID: 2701
		// (get) Token: 0x0600243F RID: 9279 RVA: 0x000EFDA5 File Offset: 0x000EDFA5
		// (set) Token: 0x06002440 RID: 9280 RVA: 0x000EFDAD File Offset: 0x000EDFAD
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool UseContainedInventoryIconColor { get; private set; }

		// Token: 0x17000A8E RID: 2702
		// (get) Token: 0x06002441 RID: 9281 RVA: 0x000EFDB6 File Offset: 0x000EDFB6
		// (set) Token: 0x06002442 RID: 9282 RVA: 0x000EFDBE File Offset: 0x000EDFBE
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AddedRepairSpeedMultiplier { get; private set; }

		// Token: 0x17000A8F RID: 2703
		// (get) Token: 0x06002443 RID: 9283 RVA: 0x000EFDC7 File Offset: 0x000EDFC7
		// (set) Token: 0x06002444 RID: 9284 RVA: 0x000EFDCF File Offset: 0x000EDFCF
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AddedPickingSpeedMultiplier { get; private set; }

		// Token: 0x17000A90 RID: 2704
		// (get) Token: 0x06002445 RID: 9285 RVA: 0x000EFDD8 File Offset: 0x000EDFD8
		// (set) Token: 0x06002446 RID: 9286 RVA: 0x000EFDE0 File Offset: 0x000EDFE0
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool CannotRepairFail { get; private set; }

		// Token: 0x17000A91 RID: 2705
		// (get) Token: 0x06002447 RID: 9287 RVA: 0x000EFDE9 File Offset: 0x000EDFE9
		// (set) Token: 0x06002448 RID: 9288 RVA: 0x000EFDF1 File Offset: 0x000EDFF1
		[Serialize(null, IsPropertySaveable.No, "", "", false)]
		public string EquipConfirmationText { get; set; }

		// Token: 0x17000A92 RID: 2706
		// (get) Token: 0x06002449 RID: 9289 RVA: 0x000EFDFA File Offset: 0x000EDFFA
		// (set) Token: 0x0600244A RID: 9290 RVA: 0x000EFE02 File Offset: 0x000EE002
		[Serialize(true, IsPropertySaveable.No, "Can the item be rotated in the submarine editor?", "", false)]
		public bool AllowRotatingInEditor { get; set; }

		// Token: 0x17000A93 RID: 2707
		// (get) Token: 0x0600244B RID: 9291 RVA: 0x000EFE0B File Offset: 0x000EE00B
		// (set) Token: 0x0600244C RID: 9292 RVA: 0x000EFE13 File Offset: 0x000EE013
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool ShowContentsInTooltip { get; private set; }

		// Token: 0x17000A94 RID: 2708
		// (get) Token: 0x0600244D RID: 9293 RVA: 0x000EFE1C File Offset: 0x000EE01C
		// (set) Token: 0x0600244E RID: 9294 RVA: 0x000EFE24 File Offset: 0x000EE024
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool CanFlipX { get; private set; }

		// Token: 0x17000A95 RID: 2709
		// (get) Token: 0x0600244F RID: 9295 RVA: 0x000EFE2D File Offset: 0x000EE02D
		// (set) Token: 0x06002450 RID: 9296 RVA: 0x000EFE35 File Offset: 0x000EE035
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool CanFlipY { get; private set; }

		// Token: 0x17000A96 RID: 2710
		// (get) Token: 0x06002451 RID: 9297 RVA: 0x000EFE3E File Offset: 0x000EE03E
		// (set) Token: 0x06002452 RID: 9298 RVA: 0x000EFE46 File Offset: 0x000EE046
		[Serialize(0.01f, IsPropertySaveable.No, "", "", false)]
		public float MinScale { get; private set; }

		// Token: 0x17000A97 RID: 2711
		// (get) Token: 0x06002453 RID: 9299 RVA: 0x000EFE4F File Offset: 0x000EE04F
		// (set) Token: 0x06002454 RID: 9300 RVA: 0x000EFE57 File Offset: 0x000EE057
		[Serialize(10f, IsPropertySaveable.No, "", "", false)]
		public float MaxScale { get; private set; }

		// Token: 0x17000A98 RID: 2712
		// (get) Token: 0x06002455 RID: 9301 RVA: 0x000EFE60 File Offset: 0x000EE060
		// (set) Token: 0x06002456 RID: 9302 RVA: 0x000EFE68 File Offset: 0x000EE068
		[Serialize(false, IsPropertySaveable.No, "Bots avoid rooms with dangerous items in them.", "", false)]
		public bool IsDangerous { get; private set; }

		// Token: 0x17000A99 RID: 2713
		// (get) Token: 0x06002457 RID: 9303 RVA: 0x000EFE71 File Offset: 0x000EE071
		// (set) Token: 0x06002458 RID: 9304 RVA: 0x000EFE79 File Offset: 0x000EE079
		[Serialize(1, IsPropertySaveable.No, "", "", false)]
		public int MaxStackSize
		{
			get
			{
				return this.maxStackSize;
			}
			private set
			{
				this.maxStackSize = MathHelper.Clamp(value, 1, 63);
			}
		}

		// Token: 0x17000A9A RID: 2714
		// (get) Token: 0x06002459 RID: 9305 RVA: 0x000EFE8A File Offset: 0x000EE08A
		// (set) Token: 0x0600245A RID: 9306 RVA: 0x000EFE92 File Offset: 0x000EE092
		[Serialize(-1, IsPropertySaveable.No, "Maximum stack size when the item is in a character inventory.", "", false)]
		public int MaxStackSizeCharacterInventory
		{
			get
			{
				return this.maxStackSizeCharacterInventory;
			}
			private set
			{
				this.maxStackSizeCharacterInventory = Math.Min(value, 63);
			}
		}

		// Token: 0x17000A9B RID: 2715
		// (get) Token: 0x0600245B RID: 9307 RVA: 0x000EFEA2 File Offset: 0x000EE0A2
		// (set) Token: 0x0600245C RID: 9308 RVA: 0x000EFEAA File Offset: 0x000EE0AA
		[Serialize(-1, IsPropertySaveable.No, "Maximum stack size when the item is inside a holdable or wearable item. If not set, defaults to MaxStackSizeCharacterInventory.", "", false)]
		public int MaxStackSizeHoldableOrWearableInventory
		{
			get
			{
				return this.maxStackSizeHoldableOrWearableInventory;
			}
			private set
			{
				this.maxStackSizeHoldableOrWearableInventory = Math.Min(value, 63);
			}
		}

		// Token: 0x0600245D RID: 9309 RVA: 0x000EFEBC File Offset: 0x000EE0BC
		public int GetMaxStackSize(Inventory inventory)
		{
			ItemInventory i = inventory as ItemInventory;
			int num;
			if (i != null)
			{
				Entity owner = inventory.Owner;
				Item it = owner as Item;
				if (it != null)
				{
					num = (int)it.StatManager.GetAdjustedValueAdditive(ItemTalentStats.ExtraStackSize, (float)i.ExtraStackSize);
					goto IL_B0;
				}
			}
			else
			{
				CharacterInventory j = inventory as CharacterInventory;
				if (j != null)
				{
					Entity owner = inventory.Owner;
					Character character = owner as Character;
					if (character != null)
					{
						CharacterInfo <info>5__2 = character.Info;
						if (<info>5__2 != null)
						{
							num = j.ExtraStackSize + EnumExtensions.GetIndividualFlags<MapEntityCategory>(this.Category).Sum((MapEntityCategory c) => (int)<info>5__2.GetSavedStatValueWithAll(StatTypes.InventoryExtraStackSize, c.ToIdentifier<MapEntityCategory>()));
							goto IL_B0;
						}
					}
				}
				else if (inventory == null)
				{
					num = 0;
					goto IL_B0;
				}
			}
			num = inventory.ExtraStackSize;
			IL_B0:
			int extraStackSize = num;
			if (inventory is CharacterInventory && this.maxStackSizeCharacterInventory > 0)
			{
				return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|298_0(this.maxStackSizeCharacterInventory, extraStackSize);
			}
			Item item = ((inventory != null) ? inventory.Owner : null) as Item;
			if (item != null)
			{
				Holdable component = item.GetComponent<Holdable>();
				if ((component != null && !component.Attachable) || item.GetComponent<Wearable>() != null)
				{
					if (this.maxStackSizeHoldableOrWearableInventory > 0)
					{
						return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|298_0(this.maxStackSizeHoldableOrWearableInventory, extraStackSize);
					}
					if (this.maxStackSizeCharacterInventory > 0)
					{
						return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|298_0(this.maxStackSizeCharacterInventory, extraStackSize);
					}
				}
			}
			return ItemPrefab.<GetMaxStackSize>g__MaxStackWithExtra|298_0(this.maxStackSize, extraStackSize);
		}

		// Token: 0x17000A9C RID: 2716
		// (get) Token: 0x0600245E RID: 9310 RVA: 0x000F0008 File Offset: 0x000EE208
		// (set) Token: 0x0600245F RID: 9311 RVA: 0x000F0010 File Offset: 0x000EE210
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool AllowDroppingOnSwap { get; private set; }

		// Token: 0x17000A9D RID: 2717
		// (get) Token: 0x06002460 RID: 9312 RVA: 0x000F0019 File Offset: 0x000EE219
		// (set) Token: 0x06002461 RID: 9313 RVA: 0x000F0021 File Offset: 0x000EE221
		public ImmutableHashSet<Identifier> AllowDroppingOnSwapWith { get; private set; }

		// Token: 0x17000A9E RID: 2718
		// (get) Token: 0x06002462 RID: 9314 RVA: 0x000F002A File Offset: 0x000EE22A
		// (set) Token: 0x06002463 RID: 9315 RVA: 0x000F0032 File Offset: 0x000EE232
		[Serialize(false, IsPropertySaveable.No, "If enabled, the item is not transferred when the player transfers items between subs.", "", false)]
		public bool DontTransferBetweenSubs { get; private set; }

		// Token: 0x17000A9F RID: 2719
		// (get) Token: 0x06002464 RID: 9316 RVA: 0x000F003B File Offset: 0x000EE23B
		// (set) Token: 0x06002465 RID: 9317 RVA: 0x000F0043 File Offset: 0x000EE243
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowHealthBar { get; private set; }

		// Token: 0x17000AA0 RID: 2720
		// (get) Token: 0x06002466 RID: 9318 RVA: 0x000F004C File Offset: 0x000EE24C
		// (set) Token: 0x06002467 RID: 9319 RVA: 0x000F0054 File Offset: 0x000EE254
		[Serialize(1f, IsPropertySaveable.No, "How much the bots prioritize this item when they seek for items. For example, bots prioritize less exosuit than the other diving suits. Defaults to 1. Note that there's also a specific CombatPriority for items that can be used as weapons.", "", false)]
		public float BotPriority { get; private set; }

		// Token: 0x17000AA1 RID: 2721
		// (get) Token: 0x06002468 RID: 9320 RVA: 0x000F005D File Offset: 0x000EE25D
		// (set) Token: 0x06002469 RID: 9321 RVA: 0x000F0065 File Offset: 0x000EE265
		[Serialize(true, IsPropertySaveable.No, "", "", false)]
		public bool ShowNameInHealthBar { get; private set; }

		// Token: 0x17000AA2 RID: 2722
		// (get) Token: 0x0600246A RID: 9322 RVA: 0x000F006E File Offset: 0x000EE26E
		// (set) Token: 0x0600246B RID: 9323 RVA: 0x000F0076 File Offset: 0x000EE276
		[Serialize(false, IsPropertySaveable.No, "Should the bots shoot at this item with turret or not? Disabled by default.", "", false)]
		public bool IsAITurretTarget { get; private set; }

		// Token: 0x17000AA3 RID: 2723
		// (get) Token: 0x0600246C RID: 9324 RVA: 0x000F007F File Offset: 0x000EE27F
		// (set) Token: 0x0600246D RID: 9325 RVA: 0x000F0087 File Offset: 0x000EE287
		[Serialize(1f, IsPropertySaveable.No, "How much the bots prioritize shooting this item with turrets? Defaults to 1. Distance to the target affects the decision making.", "", false)]
		public float AITurretPriority { get; private set; }

		// Token: 0x17000AA4 RID: 2724
		// (get) Token: 0x0600246E RID: 9326 RVA: 0x000F0090 File Offset: 0x000EE290
		// (set) Token: 0x0600246F RID: 9327 RVA: 0x000F0098 File Offset: 0x000EE298
		[Serialize(1f, IsPropertySaveable.No, "How much the bots prioritize shooting this item with slow turrets, like railguns? Defaults to 1. Not used if AITurretPriority is 0. Distance to the target affects the decision making.", "", false)]
		public float AISlowTurretPriority { get; private set; }

		// Token: 0x17000AA5 RID: 2725
		// (get) Token: 0x06002470 RID: 9328 RVA: 0x000F00A1 File Offset: 0x000EE2A1
		// (set) Token: 0x06002471 RID: 9329 RVA: 0x000F00A9 File Offset: 0x000EE2A9
		[Serialize(float.PositiveInfinity, IsPropertySaveable.No, "The max distance at which the bots are allowed to target the items. Defaults to infinity.", "", false)]
		public float AITurretTargetingMaxDistance { get; private set; }

		// Token: 0x17000AA6 RID: 2726
		// (get) Token: 0x06002472 RID: 9330 RVA: 0x000F00B2 File Offset: 0x000EE2B2
		// (set) Token: 0x06002473 RID: 9331 RVA: 0x000F00BA File Offset: 0x000EE2BA
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, taking items from this container is never considered stealing.", "", false)]
		public bool AllowStealingContainedItems { get; private set; }

		// Token: 0x17000AA7 RID: 2727
		// (get) Token: 0x06002474 RID: 9332 RVA: 0x000F00C3 File Offset: 0x000EE2C3
		// (set) Token: 0x06002475 RID: 9333 RVA: 0x000F00CB File Offset: 0x000EE2CB
		[Serialize("255,255,255,255", IsPropertySaveable.No, "Used in circuit box to set the color of the nodes.", "", false)]
		public Color SignalComponentColor { get; private set; }

		// Token: 0x17000AA8 RID: 2728
		// (get) Token: 0x06002476 RID: 9334 RVA: 0x000F00D4 File Offset: 0x000EE2D4
		// (set) Token: 0x06002477 RID: 9335 RVA: 0x000F00DC File Offset: 0x000EE2DC
		[Serialize(false, IsPropertySaveable.No, "If enabled, the player is unable to open the middle click menu when this item is selected.", "", false)]
		public bool DisableCommandMenuWhenSelected { get; set; }

		// Token: 0x06002478 RID: 9336 RVA: 0x000F00E8 File Offset: 0x000EE2E8
		protected override Identifier DetermineIdentifier(XElement element)
		{
			Identifier identifier = base.DetermineIdentifier(element);
			string originalName = element.GetAttributeString("name", "");
			if (identifier.IsEmpty && !string.IsNullOrEmpty(originalName))
			{
				string categoryStr = element.GetAttributeString("category", "Misc");
				MapEntityCategory category;
				if (Enum.TryParse<MapEntityCategory>(categoryStr, true, out category) && category.HasFlag(MapEntityCategory.Legacy))
				{
					identifier = ItemPrefab.GenerateLegacyIdentifier(originalName);
				}
			}
			return identifier;
		}

		// Token: 0x06002479 RID: 9337 RVA: 0x000F015A File Offset: 0x000EE35A
		public static Identifier GenerateLegacyIdentifier(string name)
		{
			return ("legacyitem_" + name.Replace(" ", "")).ToIdentifier();
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x000F017C File Offset: 0x000EE37C
		public ItemPrefab(ContentXElement element, ItemFile file) : base(element, file)
		{
			this.originalElement = element;
			this.ConfigElement = element;
			this.OriginalName = element.GetAttributeString("name", "");
			this.name = this.OriginalName;
			this.VariantOf = element.VariantOf();
			if (!this.VariantOf.IsEmpty)
			{
				return;
			}
			this.ParseConfigElement(null);
		}

		// Token: 0x0600247B RID: 9339 RVA: 0x000F01EA File Offset: 0x000EE3EA
		public string GetTexturePath(ContentXElement subElement, ItemPrefab variantOf)
		{
			if (!subElement.DoesAttributeReferenceFileNameAlone("texture"))
			{
				return "";
			}
			return Path.GetDirectoryName(((variantOf != null) ? variantOf.ContentFile.Path : null) ?? this.ContentFile.Path);
		}

		// Token: 0x0600247C RID: 9340 RVA: 0x000F0224 File Offset: 0x000EE424
		private void ParseConfigElement(ItemPrefab variantOf)
		{
			string categoryStr = this.ConfigElement.GetAttributeString("category", "Misc");
			MapEntityCategory category;
			this.category = (Enum.TryParse<MapEntityCategory>(categoryStr, true, out category) ? category : MapEntityCategory.Misc);
			Identifier nameIdentifier = this.ConfigElement.GetAttributeIdentifier("nameidentifier", "");
			string fallbackNameIdentifier = this.ConfigElement.GetAttributeString("fallbacknameidentifier", "");
			string[] array = new string[2];
			int num = 0;
			string text;
			if (!nameIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EntityName.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(nameIdentifier);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("EntityName.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				text = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			array[num] = text;
			array[1] = "EntityName." + fallbackNameIdentifier;
			this.name = TextManager.Get(array);
			if (!string.IsNullOrEmpty(this.OriginalName))
			{
				this.name = this.name.Fallback(this.OriginalName, true);
			}
			if (category == MapEntityCategory.Wrecked)
			{
				this.name = TextManager.GetWithVariable("wreckeditemformat", "[name]", this.name, FormatCapitals.No);
			}
			this.name = GeneticMaterial.TryCreateName(this, this.ConfigElement);
			this.aliases = (this.ConfigElement.GetAttributeStringArray("aliases", null, true) ?? this.ConfigElement.GetAttributeStringArray("Aliases", Array.Empty<string>(), true)).ToImmutableHashSet<string>().Add(this.OriginalName.ToLowerInvariant());
			List<Rectangle> triggers = new List<Rectangle>();
			List<DeconstructItem> deconstructItems = new List<DeconstructItem>();
			Dictionary<uint, FabricationRecipe> fabricationRecipes = new Dictionary<uint, FabricationRecipe>();
			Dictionary<Identifier, float> treatmentSuitability = new Dictionary<Identifier, float>();
			Dictionary<Identifier, PriceInfo> storePrices = new Dictionary<Identifier, PriceInfo>();
			List<PreferredContainer> preferredContainers = new List<PreferredContainer>();
			this.DeconstructTime = 1f;
			this.DeconstructTimeInOutposts = this.DeconstructTime;
			if (this.ConfigElement.GetAttribute("allowasextracargo") != null)
			{
				this.AllowAsExtraCargo = new bool?(this.ConfigElement.GetAttributeBool("allowasextracargo", false));
			}
			List<Identifier> tags = this.ConfigElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToList<Identifier>();
			if (this.ConfigElement.Descendants().Any(delegate(ContentXElement e)
			{
				Identifier identifier = e.NameAsIdentifier();
				return identifier == "lightcomponent";
			}))
			{
				tags.Add("light".ToIdentifier());
			}
			this.tags = tags.ToImmutableHashSet<Identifier>();
			if (this.ConfigElement.GetAttribute("cargocontainername") != null)
			{
				DebugConsole.ThrowError("Error in item prefab \"" + this.ToString() + "\" - cargo container should be configured using the item's identifier, not the name.", null, this.ConfigElement.ContentPackage, false, false);
			}
			SerializableProperty.DeserializeProperties(this, this.ConfigElement);
			base.LoadDescription(this.ConfigElement);
			List<SkillRequirementHint> skillRequirementHints = new List<SkillRequirementHint>();
			foreach (ContentXElement skillRequirementHintElement in this.ConfigElement.GetChildElements("SkillRequirementHint"))
			{
				skillRequirementHints.Add(new SkillRequirementHint(skillRequirementHintElement));
			}
			if (skillRequirementHints.Any<SkillRequirementHint>())
			{
				this.SkillRequirementHints = skillRequirementHints.ToImmutableArray<SkillRequirementHint>();
			}
			Identifier[] allowDroppingOnSwapWith = this.ConfigElement.GetAttributeIdentifierArray("allowdroppingonswapwith", Array.Empty<Identifier>(), true);
			this.AllowDroppingOnSwapWith = allowDroppingOnSwapWith.ToImmutableHashSet<Identifier>();
			this.AllowDroppingOnSwap = allowDroppingOnSwapWith.Any<Identifier>();
			Dictionary<Identifier, ItemPrefab.CommonnessInfo> levelCommonness = new Dictionary<Identifier, ItemPrefab.CommonnessInfo>();
			Dictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo> levelQuantity = new Dictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo>();
			List<FabricationRecipe> loadedRecipes = new List<FabricationRecipe>();
			foreach (ContentXElement subElement in this.ConfigElement.Elements())
			{
				string text2 = subElement.Name.ToString().ToLowerInvariant();
				if (text2 != null)
				{
					switch (text2.Length)
					{
					case 5:
					{
						if (!(text2 == "price"))
						{
							continue;
						}
						if (subElement.GetAttribute("baseprice") != null)
						{
							using (List<PriceInfo>.Enumerator enumerator3 = PriceInfo.CreatePriceInfos(subElement, out this.defaultPrice).GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									PriceInfo priceInfo = enumerator3.Current;
									if (!priceInfo.StoreIdentifier.IsEmpty)
									{
										if (storePrices.ContainsKey(priceInfo.StoreIdentifier))
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(71, 2);
											defaultInterpolatedStringHandler3.AppendLiteral("Error in item prefab \"");
											defaultInterpolatedStringHandler3.AppendFormatted<ItemPrefab>(this);
											defaultInterpolatedStringHandler3.AppendLiteral("\": price for the store \"");
											defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(priceInfo.StoreIdentifier);
											defaultInterpolatedStringHandler3.AppendLiteral("\" defined more than once.");
											DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), base.ContentPackage);
											storePrices[priceInfo.StoreIdentifier] = priceInfo;
										}
										else
										{
											storePrices.Add(priceInfo.StoreIdentifier, priceInfo);
										}
									}
								}
								continue;
							}
						}
						if (subElement.GetAttribute("buyprice") == null)
						{
							continue;
						}
						Identifier locationType = subElement.GetAttributeIdentifier("locationtype", "");
						if (locationType.IsEmpty)
						{
							continue;
						}
						if (storePrices.ContainsKey(locationType))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(79, 2);
							defaultInterpolatedStringHandler4.AppendLiteral("Error in item prefab \"");
							defaultInterpolatedStringHandler4.AppendFormatted<ItemPrefab>(this);
							defaultInterpolatedStringHandler4.AppendLiteral("\": price for the location type \"");
							defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(locationType);
							defaultInterpolatedStringHandler4.AppendLiteral("\" defined more than once.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler4.ToStringAndClear(), base.ContentPackage);
							storePrices[locationType] = new PriceInfo(subElement);
							continue;
						}
						storePrices.Add(locationType, new PriceInfo(subElement));
						continue;
					}
					case 6:
					{
						if (!(text2 == "sprite"))
						{
							continue;
						}
						string spriteFolder = this.GetTexturePath(subElement, variantOf);
						this.canSpriteFlipX = subElement.GetAttributeBool("canflipx", true);
						this.canSpriteFlipY = subElement.GetAttributeBool("canflipy", true);
						this.sprite = new Sprite(subElement, spriteFolder, "", true, 1f);
						if (subElement.GetAttribute("sourcerect") == null && subElement.GetAttribute("sheetindex") == null)
						{
							DebugConsole.ThrowError("Warning - sprite sourcerect not configured for item \"" + this.ToString() + "\"!", null, this.ConfigElement.ContentPackage, false, false);
						}
						this.Size = this.Sprite.size;
						if (subElement.GetAttribute("name") == null && !this.Name.IsNullOrWhiteSpace())
						{
							this.Sprite.Name = this.Name.Value;
						}
						this.Sprite.EntityIdentifier = this.Identifier;
						continue;
					}
					case 7:
					{
						if (!(text2 == "trigger"))
						{
							continue;
						}
						Rectangle trigger = new Rectangle(0, 0, 10, 10)
						{
							X = subElement.GetAttributeInt("x", 0),
							Y = subElement.GetAttributeInt("y", 0),
							Width = subElement.GetAttributeInt("width", 0),
							Height = subElement.GetAttributeInt("height", 0)
						};
						triggers.Add(trigger);
						continue;
					}
					case 8:
					case 12:
					case 15:
					case 16:
						continue;
					case 9:
						if (!(text2 == "fabricate"))
						{
							continue;
						}
						break;
					case 10:
						if (!(text2 == "fabricable"))
						{
							continue;
						}
						break;
					case 11:
						if (!(text2 == "deconstruct"))
						{
							continue;
						}
						this.DeconstructTime = subElement.GetAttributeFloat("time", 1f);
						this.DeconstructTimeInOutposts = subElement.GetAttributeFloat("timeinoutposts", this.DeconstructTime);
						this.AllowDeconstruct = true;
						this.RandomDeconstructionOutput = subElement.GetAttributeBool("chooserandom", false);
						this.RandomDeconstructionOutputAmount = subElement.GetAttributeInt("amount", 1);
						foreach (ContentXElement cxe in subElement.Elements())
						{
							XElement itemElement = cxe;
							if (itemElement.Attribute("name") != null)
							{
								DebugConsole.ThrowError("Error in item config \"" + this.ToString() + "\" - use item identifiers instead of names to configure the deconstruct items.", null, this.ConfigElement.ContentPackage, false, false);
							}
							else
							{
								DeconstructItem deconstructItem = new DeconstructItem(itemElement, this.Identifier);
								if (deconstructItem.ItemIdentifier.IsEmpty)
								{
									DebugConsole.ThrowError("Error in item config \"" + this.ToString() + "\" - deconstruction output contains an item with no identifier.", null, this.ConfigElement.ContentPackage, false, false);
								}
								else
								{
									deconstructItems.Add(deconstructItem);
								}
							}
						}
						this.RandomDeconstructionOutputAmount = Math.Min(this.RandomDeconstructionOutputAmount, deconstructItems.Count);
						continue;
					case 13:
					{
						char c = text2[0];
						if (c != 'l')
						{
							if (c != 's')
							{
								continue;
							}
							if (!(text2 == "swappableitem"))
							{
								continue;
							}
							this.SwappableItem = new SwappableItem(subElement);
							continue;
						}
						else
						{
							if (!(text2 == "levelresource"))
							{
								continue;
							}
							using (IEnumerator<ContentXElement> enumerator5 = subElement.GetChildElements("commonness").GetEnumerator())
							{
								while (enumerator5.MoveNext())
								{
									ContentXElement cxe2 = enumerator5.Current;
									XElement levelCommonnessElement = cxe2;
									Identifier levelName = levelCommonnessElement.GetAttributeIdentifier("leveltype", "");
									if (!levelCommonnessElement.GetAttributeBool("fixedquantity", false))
									{
										if (!levelCommonness.ContainsKey(levelName))
										{
											levelCommonness.Add(levelName, new ItemPrefab.CommonnessInfo(levelCommonnessElement));
										}
									}
									else if (!levelQuantity.ContainsKey(levelName))
									{
										levelQuantity.Add(levelName, new ItemPrefab.FixedQuantityResourceInfo(levelCommonnessElement.GetAttributeInt("clusterquantity", 0), levelCommonnessElement.GetAttributeInt("clustersize", 0), levelCommonnessElement.GetAttributeBool("isislandspecific", false), levelCommonnessElement.GetAttributeBool("allowatstart", true)));
									}
								}
								continue;
							}
							goto IL_BC6;
						}
						break;
					}
					case 14:
						if (!(text2 == "fabricableitem"))
						{
							continue;
						}
						break;
					case 17:
						if (!(text2 == "suitabletreatment"))
						{
							continue;
						}
						goto IL_BC6;
					case 18:
					{
						if (!(text2 == "preferredcontainer"))
						{
							continue;
						}
						PreferredContainer preferredContainer = new PreferredContainer(subElement);
						if (preferredContainer.Primary.Count != 0 || preferredContainer.Secondary.Count != 0)
						{
							preferredContainers.Add(preferredContainer);
							continue;
						}
						if (variantOf == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(75, 2);
							defaultInterpolatedStringHandler5.AppendLiteral("Error in item prefab \"");
							defaultInterpolatedStringHandler5.AppendFormatted(this.ToString());
							defaultInterpolatedStringHandler5.AppendLiteral("\": preferred container has no preferences defined (");
							defaultInterpolatedStringHandler5.AppendFormatted<ContentXElement>(subElement);
							defaultInterpolatedStringHandler5.AppendLiteral(").");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, this.ConfigElement.ContentPackage, false, false);
							continue;
						}
						continue;
					}
					default:
						continue;
					}
					FabricationRecipe newRecipe = new FabricationRecipe(subElement, this.Identifier);
					FabricationRecipe prevRecipe;
					if (fabricationRecipes.TryGetValue(newRecipe.RecipeHash, out prevRecipe))
					{
						ContentPackage packageToLog = (variantOf.ContentPackage != null && variantOf.ContentPackage != ContentPackageManager.VanillaCorePackage) ? variantOf.ContentPackage : this.GetParentModPackageOrThisPackage();
						int prevRecipeIndex = loadedRecipes.IndexOf(prevRecipe);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(177, 3);
						defaultInterpolatedStringHandler6.AppendLiteral("Error in item prefab \"");
						defaultInterpolatedStringHandler6.AppendFormatted(this.ToString());
						defaultInterpolatedStringHandler6.AppendLiteral("\": ");
						defaultInterpolatedStringHandler6.AppendLiteral("Fabrication recipe #");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(loadedRecipes.Count + 1);
						defaultInterpolatedStringHandler6.AppendLiteral(" has the same hash as recipe #");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(prevRecipeIndex + 1);
						defaultInterpolatedStringHandler6.AppendLiteral(". This is most likely caused by identical, duplicate recipes. ");
						defaultInterpolatedStringHandler6.AppendLiteral("This will cause issues with fabrication.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler6.ToStringAndClear(), packageToLog);
					}
					else
					{
						fabricationRecipes.Add(newRecipe.RecipeHash, newRecipe);
					}
					loadedRecipes.Add(newRecipe);
					continue;
					IL_BC6:
					if (subElement.GetAttribute("name") != null)
					{
						DebugConsole.ThrowError("Error in item prefab \"" + this.ToString() + "\" - suitable treatments should be defined using item identifiers, not item names.", null, this.ConfigElement.ContentPackage, false, false);
					}
					Identifier treatmentIdentifier = subElement.GetAttributeIdentifier("identifier", subElement.GetAttributeIdentifier("type", Identifier.Empty));
					float suitability = subElement.GetAttributeFloat("suitability", 0f);
					treatmentSuitability.Add(treatmentIdentifier, suitability);
				}
			}
			ContentXElement configElement = this.ConfigElement;
			string key = "Size";
			Vector2 size = this.Size;
			this.Size = configElement.GetAttributeVector2(key, size);
			this.Triggers = triggers.ToImmutableArray<Rectangle>();
			this.DeconstructItems = deconstructItems.ToImmutableArray<DeconstructItem>();
			this.FabricationRecipes = fabricationRecipes.ToImmutableDictionary<uint, FabricationRecipe>();
			this.treatmentSuitability = treatmentSuitability.ToImmutableDictionary<Identifier, float>();
			this.StorePrices = storePrices.ToImmutableDictionary<Identifier, PriceInfo>();
			this.PreferredContainers = preferredContainers.ToImmutableArray<PreferredContainer>();
			this.LevelCommonness = levelCommonness.ToImmutableDictionary<Identifier, ItemPrefab.CommonnessInfo>();
			this.LevelQuantity = levelQuantity.ToImmutableDictionary<Identifier, ItemPrefab.FixedQuantityResourceInfo>();
			ContentXElement childElement = this.ConfigElement.GetChildElement("Holdable");
			ContentXElement contentXElement = null;
			bool canFlipYByDefault = childElement == contentXElement;
			this.CanFlipY = this.ConfigElement.GetAttributeBool("CanFlipY", canFlipYByDefault);
			if (storePrices.Any<KeyValuePair<Identifier, PriceInfo>>() && this.defaultPrice == null)
			{
				this.defaultPrice = new PriceInfo(this.GetMinPrice().GetValueOrDefault(), false, 0, 0, true, 0, 1f, false, false, null);
			}
			this.HideConditionInTooltip = this.ConfigElement.GetAttributeBool("hideconditionintooltip", this.HideConditionBar);
			if (categoryStr.Equals("Thalamus", StringComparison.OrdinalIgnoreCase))
			{
				this.category = MapEntityCategory.Wrecked;
				base.Subcategory = "Thalamus";
			}
			if (this.Sprite == null)
			{
				DebugConsole.ThrowError("Item \"" + this.ToString() + "\" has no sprite!", null, this.ConfigElement.ContentPackage, false, false);
				this.sprite = new Sprite("", Vector2.Zero);
				this.sprite.SourceRect = new Rectangle(0, 0, 32, 32);
				this.Size = this.Sprite.size;
				this.Sprite.EntityIdentifier = this.Identifier;
			}
			if (this.Identifier == Identifier.Empty)
			{
				DebugConsole.ThrowError("Item prefab \"" + this.ToString() + "\" has no identifier. All item prefabs have a unique identifier string that's used to differentiate between items during saving and loading.", null, this.ConfigElement.ContentPackage, false, false);
			}
			this.allowedLinks = this.ConfigElement.GetAttributeIdentifierArray("allowedlinks", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			ContentXElement configElement2 = this.ConfigElement;
			string key2 = "GrabWhenSelected";
			childElement = this.ConfigElement.GetChildElement("ItemContainer");
			contentXElement = null;
			bool def;
			if (childElement != contentXElement)
			{
				ContentXElement childElement2 = this.ConfigElement.GetChildElement("Body");
				ContentXElement contentXElement2 = null;
				def = (childElement2 == contentXElement2);
			}
			else
			{
				def = false;
			}
			this.GrabWhenSelected = configElement2.GetAttributeBool(key2, def);
		}

		// Token: 0x0600247D RID: 9341 RVA: 0x000F1168 File Offset: 0x000EF368
		public ItemPrefab.CommonnessInfo? GetCommonnessInfo(Level level)
		{
			ItemPrefab.CommonnessInfo? levelCommonnessInfo = this.<GetCommonnessInfo>g__GetValueOrNull|356_0(level.GenerationParams.Identifier);
			ItemPrefab.CommonnessInfo? biomeCommonnessInfo = this.<GetCommonnessInfo>g__GetValueOrNull|356_0(level.LevelData.Biome.Identifier);
			ItemPrefab.CommonnessInfo? defaultCommonnessInfo = this.<GetCommonnessInfo>g__GetValueOrNull|356_0(Identifier.Empty);
			if (levelCommonnessInfo != null)
			{
				if (levelCommonnessInfo == null)
				{
					return null;
				}
				return new ItemPrefab.CommonnessInfo?(levelCommonnessInfo.GetValueOrDefault().WithInheritedCommonness(new ItemPrefab.CommonnessInfo?[]
				{
					biomeCommonnessInfo,
					defaultCommonnessInfo
				}));
			}
			else if (biomeCommonnessInfo != null)
			{
				if (biomeCommonnessInfo == null)
				{
					return null;
				}
				return new ItemPrefab.CommonnessInfo?(biomeCommonnessInfo.GetValueOrDefault().WithInheritedCommonness(defaultCommonnessInfo));
			}
			else
			{
				if (defaultCommonnessInfo != null)
				{
					return defaultCommonnessInfo;
				}
				return null;
			}
		}

		// Token: 0x0600247E RID: 9342 RVA: 0x000F123C File Offset: 0x000EF43C
		public float GetTreatmentSuitability(Identifier treatmentIdentifier)
		{
			float suitability;
			if (!this.treatmentSuitability.TryGetValue(treatmentIdentifier, out suitability))
			{
				return 0f;
			}
			return suitability;
		}

		// Token: 0x0600247F RID: 9343 RVA: 0x000F1260 File Offset: 0x000EF460
		public PriceInfo GetPriceInfo(Location.StoreInfo store)
		{
			if (store == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(60, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to get price info for \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" with a null store parameter!\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace.CleanupStackTrace());
				string message = defaultInterpolatedStringHandler.ToStringAndClear();
				DebugConsole.AddWarning(message, base.ContentPackage);
				GameAnalyticsManager.AddErrorEventOnce("ItemPrefab.GetPriceInfo:StoreParameterNull", GameAnalyticsManager.ErrorSeverity.Error, message);
				return null;
			}
			PriceInfo storePriceInfo;
			if (!store.Identifier.IsEmpty && this.StorePrices != null && this.StorePrices.TryGetValue(store.Identifier, out storePriceInfo))
			{
				return storePriceInfo;
			}
			return this.DefaultPrice;
		}

		// Token: 0x06002480 RID: 9344 RVA: 0x000F1308 File Offset: 0x000EF508
		public bool CanBeBoughtFrom(Location.StoreInfo store, out PriceInfo priceInfo)
		{
			ItemPrefab.<>c__DisplayClass359_0 CS$<>8__locals1 = new ItemPrefab.<>c__DisplayClass359_0();
			priceInfo = this.GetPriceInfo(store);
			ItemPrefab.<>c__DisplayClass359_0 CS$<>8__locals2 = CS$<>8__locals1;
			Identifier? faction;
			if (store == null)
			{
				Identifier? identifier = null;
				faction = identifier;
			}
			else
			{
				Faction faction2 = store.Location.Faction;
				if (faction2 == null)
				{
					Identifier? identifier = null;
					faction = identifier;
				}
				else
				{
					faction = new Identifier?(faction2.Prefab.Identifier);
				}
			}
			CS$<>8__locals2.faction = faction;
			ItemPrefab.<>c__DisplayClass359_0 CS$<>8__locals3 = CS$<>8__locals1;
			Identifier? secondaryFaction;
			if (store == null)
			{
				Identifier? identifier = null;
				secondaryFaction = identifier;
			}
			else
			{
				Faction secondaryFaction2 = store.Location.SecondaryFaction;
				if (secondaryFaction2 == null)
				{
					Identifier? identifier = null;
					secondaryFaction = identifier;
				}
				else
				{
					secondaryFaction = new Identifier?(secondaryFaction2.Prefab.Identifier);
				}
			}
			CS$<>8__locals3.secondaryFaction = secondaryFaction;
			PriceInfo priceInfo2 = priceInfo;
			if (priceInfo2 != null && priceInfo2.CanBeBought)
			{
				float? num;
				if (store == null)
				{
					num = null;
				}
				else
				{
					LevelData levelData = store.Location.LevelData;
					num = ((levelData != null) ? new float?(levelData.Difficulty) : null);
				}
				float? num2 = num;
				if (num2.GetValueOrDefault() >= (float)priceInfo.MinLevelDifficulty)
				{
					if (!priceInfo.RequiredFaction.IsEmpty)
					{
						ItemPrefab.<>c__DisplayClass359_0 CS$<>8__locals4 = CS$<>8__locals1;
						Identifier? identifier = new Identifier?(priceInfo.RequiredFaction);
						if (!(CS$<>8__locals4.faction == identifier))
						{
							ItemPrefab.<>c__DisplayClass359_0 CS$<>8__locals5 = CS$<>8__locals1;
							Identifier? identifier2 = new Identifier?(priceInfo.RequiredFaction);
							if (!(CS$<>8__locals5.secondaryFaction == identifier2))
							{
								return false;
							}
						}
					}
					return !priceInfo.MinReputation.Any<KeyValuePair<Identifier, float>>() || priceInfo.MinReputation.Any(delegate(KeyValuePair<Identifier, float> p)
					{
						Identifier? identifier3 = new Identifier?(p.Key);
						if (!(CS$<>8__locals1.faction == identifier3))
						{
							Identifier? identifier4 = new Identifier?(p.Key);
							return CS$<>8__locals1.secondaryFaction == identifier4;
						}
						return true;
					});
				}
			}
			return false;
		}

		// Token: 0x06002481 RID: 9345 RVA: 0x000F146C File Offset: 0x000EF66C
		public bool CanBeBoughtFrom(Location location)
		{
			Location location2 = location;
			if (((location2 != null) ? location2.Stores : null) == null)
			{
				return false;
			}
			Func<KeyValuePair<Identifier, float>, bool> <>9__0;
			foreach (KeyValuePair<Identifier, Location.StoreInfo> store in location.Stores)
			{
				PriceInfo priceInfo = this.GetPriceInfo(store.Value);
				if (priceInfo != null && priceInfo.CanBeBought && location.LevelData.Difficulty >= (float)priceInfo.MinLevelDifficulty)
				{
					if (priceInfo.MinReputation.Any<KeyValuePair<Identifier, float>>())
					{
						IEnumerable<KeyValuePair<Identifier, float>> minReputation = priceInfo.MinReputation;
						Func<KeyValuePair<Identifier, float>, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = delegate(KeyValuePair<Identifier, float> p)
							{
								Location location3 = location;
								Identifier? identifier;
								Identifier? identifier2;
								if (location3 == null)
								{
									identifier = null;
									identifier2 = identifier;
								}
								else
								{
									Faction faction = location3.Faction;
									if (faction == null)
									{
										identifier = null;
										identifier2 = identifier;
									}
									else
									{
										identifier2 = new Identifier?(faction.Prefab.Identifier);
									}
								}
								identifier = identifier2;
								Identifier? identifier3 = new Identifier?(p.Key);
								if (!(identifier == identifier3))
								{
									Location location4 = location;
									Identifier? identifier4;
									Identifier? identifier5;
									if (location4 == null)
									{
										identifier4 = null;
										identifier5 = identifier4;
									}
									else
									{
										Faction secondaryFaction = location4.SecondaryFaction;
										if (secondaryFaction == null)
										{
											identifier4 = null;
											identifier5 = identifier4;
										}
										else
										{
											identifier5 = new Identifier?(secondaryFaction.Prefab.Identifier);
										}
									}
									identifier4 = identifier5;
									Identifier? identifier6 = new Identifier?(p.Key);
									return identifier4 == identifier6;
								}
								return true;
							});
						}
						if (!minReputation.Any(predicate))
						{
							continue;
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002482 RID: 9346 RVA: 0x000F155C File Offset: 0x000EF75C
		public int? GetMinPrice()
		{
			int? minPrice = null;
			if (this.StorePrices != null && this.StorePrices.Any<KeyValuePair<Identifier, PriceInfo>>())
			{
				minPrice = new int?(this.StorePrices.Values.Min((PriceInfo p) => p.Price));
			}
			if (minPrice != null)
			{
				if (this.DefaultPrice == null)
				{
					return new int?(minPrice.Value);
				}
				int? num = minPrice;
				int price = this.DefaultPrice.Price;
				if (!(num.GetValueOrDefault() < price & num != null))
				{
					return new int?(this.DefaultPrice.Price);
				}
				return minPrice;
			}
			else
			{
				PriceInfo priceInfo = this.DefaultPrice;
				if (priceInfo == null)
				{
					return null;
				}
				return new int?(priceInfo.Price);
			}
		}

		// Token: 0x06002483 RID: 9347 RVA: 0x000F1630 File Offset: 0x000EF830
		public ImmutableDictionary<Identifier, PriceInfo> GetBuyPricesUnder(int maxCost = 0)
		{
			Dictionary<Identifier, PriceInfo> prices = new Dictionary<Identifier, PriceInfo>();
			if (this.StorePrices != null)
			{
				foreach (KeyValuePair<Identifier, PriceInfo> storePrice in this.StorePrices)
				{
					PriceInfo priceInfo = storePrice.Value;
					if (priceInfo != null && priceInfo.CanBeBought && (priceInfo.Price < maxCost || maxCost == 0))
					{
						prices.Add(storePrice.Key, priceInfo);
					}
				}
			}
			return prices.ToImmutableDictionary<Identifier, PriceInfo>();
		}

		// Token: 0x06002484 RID: 9348 RVA: 0x000F16C0 File Offset: 0x000EF8C0
		public ImmutableDictionary<Identifier, PriceInfo> GetSellPricesOver(int minCost = 0, bool sellingImportant = true)
		{
			Dictionary<Identifier, PriceInfo> prices = new Dictionary<Identifier, PriceInfo>();
			if (!this.CanBeSold && sellingImportant)
			{
				return prices.ToImmutableDictionary<Identifier, PriceInfo>();
			}
			foreach (KeyValuePair<Identifier, PriceInfo> storePrice in this.StorePrices)
			{
				PriceInfo priceInfo = storePrice.Value;
				if (priceInfo != null && priceInfo.Price > minCost)
				{
					prices.Add(storePrice.Key, priceInfo);
				}
			}
			return prices.ToImmutableDictionary<Identifier, PriceInfo>();
		}

		// Token: 0x06002485 RID: 9349 RVA: 0x000F1750 File Offset: 0x000EF950
		public static ItemPrefab Find(string name, Identifier identifier)
		{
			if (string.IsNullOrEmpty(name) && identifier.IsEmpty)
			{
				throw new ArgumentException("Both name and identifier cannot be null.");
			}
			if (identifier.IsEmpty)
			{
				identifier = ItemPrefab.GenerateLegacyIdentifier(name);
			}
			ItemPrefab prefab;
			ItemPrefab.Prefabs.TryGet(identifier, out prefab);
			if (prefab == null && !string.IsNullOrEmpty(name))
			{
				string lowerCaseName = name.ToLowerInvariant();
				prefab = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Aliases != null && me.Aliases.Contains(lowerCaseName));
			}
			if (prefab == null)
			{
				prefab = ItemPrefab.Prefabs.Find((ItemPrefab me) => me.Aliases != null && me.Aliases.Contains(identifier.Value));
			}
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error loading item - item prefab \"");
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral("\" (identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\") not found.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			return prefab;
		}

		// Token: 0x06002486 RID: 9350 RVA: 0x000F185C File Offset: 0x000EFA5C
		public bool IsContainerPreferred(Item item, ItemContainer targetContainer, out bool isPreferencesDefined, out bool isSecondary, bool requireConditionRequirement = false, bool checkTransferConditions = false)
		{
			isPreferencesDefined = this.PreferredContainers.Any<PreferredContainer>();
			isSecondary = false;
			if (!isPreferencesDefined)
			{
				return true;
			}
			if (this.PreferredContainers.Any((PreferredContainer pc) => (!requireConditionRequirement || ItemPrefab.<IsContainerPreferred>g__HasConditionRequirement|365_2(pc)) && ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Primary, targetContainer) && (!checkTransferConditions || ItemPrefab.CanBeTransferred(item.Prefab.Identifier, pc, targetContainer))))
			{
				return true;
			}
			isSecondary = true;
			return this.PreferredContainers.Any((PreferredContainer pc) => (!requireConditionRequirement || ItemPrefab.<IsContainerPreferred>g__HasConditionRequirement|365_2(pc)) && ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Secondary, targetContainer));
		}

		// Token: 0x06002487 RID: 9351 RVA: 0x000F18DC File Offset: 0x000EFADC
		public bool IsContainerPreferred(Item item, Identifier[] identifiersOrTags, out bool isPreferencesDefined, out bool isSecondary)
		{
			isPreferencesDefined = this.PreferredContainers.Any<PreferredContainer>();
			isSecondary = false;
			if (!isPreferencesDefined)
			{
				return true;
			}
			if (this.PreferredContainers.Any((PreferredContainer pc) => ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Primary, identifiersOrTags)))
			{
				return true;
			}
			isSecondary = true;
			return this.PreferredContainers.Any((PreferredContainer pc) => ItemPrefab.IsItemConditionAcceptable(item, pc) && ItemPrefab.IsContainerPreferred(pc.Secondary, identifiersOrTags));
		}

		// Token: 0x06002488 RID: 9352 RVA: 0x000F194A File Offset: 0x000EFB4A
		private static bool IsItemConditionAcceptable(Item item, PreferredContainer pc)
		{
			return item.ConditionPercentage >= pc.MinCondition && item.ConditionPercentage <= pc.MaxCondition;
		}

		// Token: 0x06002489 RID: 9353 RVA: 0x000F1970 File Offset: 0x000EFB70
		private static bool CanBeTransferred(Identifier item, PreferredContainer pc, ItemContainer targetContainer)
		{
			return pc.AllowTransfersHere && (!pc.TransferOnlyOnePerContainer || targetContainer.Inventory.AllItems.None((Item i) => i.Prefab.Identifier == item));
		}

		// Token: 0x0600248A RID: 9354 RVA: 0x000F19BC File Offset: 0x000EFBBC
		public static bool IsContainerPreferred(IEnumerable<Identifier> preferences, ItemContainer c)
		{
			return preferences.Any((Identifier id) => c.Item.Prefab.Identifier == id || c.Item.HasTag(id));
		}

		// Token: 0x0600248B RID: 9355 RVA: 0x000F19E8 File Offset: 0x000EFBE8
		public static bool IsContainerPreferred(IEnumerable<Identifier> preferences, IEnumerable<Identifier> ids)
		{
			return ids.Any((Identifier id) => preferences.Contains(id));
		}

		// Token: 0x0600248C RID: 9356 RVA: 0x000F1A14 File Offset: 0x000EFC14
		protected override void CreateInstance(Rectangle rect)
		{
			throw new InvalidOperationException("Can't call ItemPrefab.CreateInstance");
		}

		// Token: 0x0600248D RID: 9357 RVA: 0x000F1A20 File Offset: 0x000EFC20
		public override void Dispose()
		{
			Item.RemoveByPrefab(this);
		}

		// Token: 0x17000AA9 RID: 2729
		// (get) Token: 0x0600248E RID: 9358 RVA: 0x000F1A28 File Offset: 0x000EFC28
		public Identifier VariantOf { get; }

		// Token: 0x17000AAA RID: 2730
		// (get) Token: 0x0600248F RID: 9359 RVA: 0x000F1A30 File Offset: 0x000EFC30
		// (set) Token: 0x06002490 RID: 9360 RVA: 0x000F1A38 File Offset: 0x000EFC38
		public ItemPrefab ParentPrefab { get; set; }

		// Token: 0x06002491 RID: 9361 RVA: 0x000F1A44 File Offset: 0x000EFC44
		public void InheritFrom(ItemPrefab parent)
		{
			ItemPrefab.<>c__DisplayClass380_0 CS$<>8__locals1 = new ItemPrefab.<>c__DisplayClass380_0();
			CS$<>8__locals1.parent = parent;
			CS$<>8__locals1.<>4__this = this;
			this.ConfigElement = this.originalElement.CreateVariantXML(CS$<>8__locals1.parent.ConfigElement, new VariantExtensions.VariantXMLChecker(CS$<>8__locals1.<InheritFrom>g__CheckXML|0));
			this.ParseConfigElement(CS$<>8__locals1.parent);
		}

		// Token: 0x06002492 RID: 9362 RVA: 0x000F1A99 File Offset: 0x000EFC99
		public ContentPackage GetParentModPackageOrThisPackage()
		{
			if (this.ParentPrefab != null && this.ParentPrefab.ContentPackage != ContentPackageManager.VanillaCorePackage)
			{
				return this.ParentPrefab.ContentPackage;
			}
			return base.ContentPackage;
		}

		// Token: 0x06002493 RID: 9363 RVA: 0x000F1AC8 File Offset: 0x000EFCC8
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" (identifier: ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06002495 RID: 9365 RVA: 0x000F1B24 File Offset: 0x000EFD24
		[CompilerGenerated]
		internal static int <GetMaxStackSize>g__MaxStackWithExtra|298_0(int maxStackSize, int extraStackSize)
		{
			extraStackSize = Math.Max(extraStackSize, 0);
			if (maxStackSize == 1)
			{
				return Math.Min(maxStackSize, 63);
			}
			return Math.Min(maxStackSize + extraStackSize, 63);
		}

		// Token: 0x06002496 RID: 9366 RVA: 0x000F1B48 File Offset: 0x000EFD48
		[CompilerGenerated]
		private ItemPrefab.CommonnessInfo? <GetCommonnessInfo>g__GetValueOrNull|356_0(Identifier identifier)
		{
			ItemPrefab.CommonnessInfo info;
			if (this.LevelCommonness.TryGetValue(identifier, out info))
			{
				return new ItemPrefab.CommonnessInfo?(info);
			}
			return null;
		}

		// Token: 0x06002497 RID: 9367 RVA: 0x000F1B75 File Offset: 0x000EFD75
		[CompilerGenerated]
		internal static bool <IsContainerPreferred>g__HasConditionRequirement|365_2(PreferredContainer pc)
		{
			return pc.MinCondition > 0f || pc.MaxCondition < 100f;
		}

		// Token: 0x040011BE RID: 4542
		public static readonly PrefabCollection<ItemPrefab> Prefabs = new PrefabCollection<ItemPrefab>();

		// Token: 0x040011BF RID: 4543
		public const float DefaultInteractDistance = 120f;

		// Token: 0x040011C1 RID: 4545
		private PriceInfo defaultPrice;

		// Token: 0x040011C4 RID: 4548
		private ImmutableDictionary<Identifier, float> treatmentSuitability;

		// Token: 0x040011C5 RID: 4549
		private readonly ContentXElement originalElement;

		// Token: 0x040011D1 RID: 4561
		private bool canSpriteFlipX;

		// Token: 0x040011D2 RID: 4562
		private bool canSpriteFlipY;

		// Token: 0x040011D6 RID: 4566
		private Sprite sprite;

		// Token: 0x040011D8 RID: 4568
		private LocalizedString name;

		// Token: 0x040011D9 RID: 4569
		private ImmutableHashSet<Identifier> tags;

		// Token: 0x040011DA RID: 4570
		private ImmutableHashSet<Identifier> allowedLinks;

		// Token: 0x040011DB RID: 4571
		private MapEntityCategory category;

		// Token: 0x040011DC RID: 4572
		private ImmutableHashSet<string> aliases;

		// Token: 0x040011EA RID: 4586
		private float health;

		// Token: 0x04001209 RID: 4617
		private int maxStackSize;

		// Token: 0x0400120A RID: 4618
		private int maxStackSizeCharacterInventory;

		// Token: 0x0400120B RID: 4619
		private int maxStackSizeHoldableOrWearableInventory;

		// Token: 0x020009B2 RID: 2482
		public readonly struct CommonnessInfo
		{
			// Token: 0x1700156F RID: 5487
			// (get) Token: 0x06005A93 RID: 23187 RVA: 0x001FC2FA File Offset: 0x001FA4FA
			public float Commonness
			{
				get
				{
					return this.commonness;
				}
			}

			// Token: 0x17001570 RID: 5488
			// (get) Token: 0x06005A94 RID: 23188 RVA: 0x001FC302 File Offset: 0x001FA502
			public float AbyssCommonness
			{
				get
				{
					return this.abyssCommonness.GetValueOrDefault();
				}
			}

			// Token: 0x17001571 RID: 5489
			// (get) Token: 0x06005A95 RID: 23189 RVA: 0x001FC310 File Offset: 0x001FA510
			public float CaveCommonness
			{
				get
				{
					float? num = this.caveCommonness;
					if (num == null)
					{
						return this.Commonness;
					}
					return num.GetValueOrDefault();
				}
			}

			// Token: 0x17001572 RID: 5490
			// (get) Token: 0x06005A96 RID: 23190 RVA: 0x001FC33B File Offset: 0x001FA53B
			public bool CanAppear
			{
				get
				{
					return this.Commonness > 0f || this.AbyssCommonness > 0f || this.CaveCommonness > 0f;
				}
			}

			// Token: 0x06005A97 RID: 23191 RVA: 0x001FC36C File Offset: 0x001FA56C
			public CommonnessInfo(XElement element)
			{
				this.commonness = Math.Max((element != null) ? element.GetAttributeFloat("commonness", 0f) : 0f, 0f);
				float? abyssCommonness = null;
				XAttribute abyssCommonnessAttribute = ((element != null) ? element.GetAttribute("abysscommonness", StringComparison.OrdinalIgnoreCase) : null) ?? ((element != null) ? element.GetAttribute("abyss", StringComparison.OrdinalIgnoreCase) : null);
				if (abyssCommonnessAttribute != null)
				{
					abyssCommonness = new float?(Math.Max(abyssCommonnessAttribute.GetAttributeFloat(0f), 0f));
				}
				this.abyssCommonness = abyssCommonness;
				float? caveCommonness = null;
				XAttribute caveCommonnessAttribute = ((element != null) ? element.GetAttribute("cavecommonness", StringComparison.OrdinalIgnoreCase) : null) ?? ((element != null) ? element.GetAttribute("cave", StringComparison.OrdinalIgnoreCase) : null);
				if (caveCommonnessAttribute != null)
				{
					caveCommonness = new float?(Math.Max(caveCommonnessAttribute.GetAttributeFloat(0f), 0f));
				}
				this.caveCommonness = caveCommonness;
			}

			// Token: 0x06005A98 RID: 23192 RVA: 0x001FC454 File Offset: 0x001FA654
			public CommonnessInfo(float commonness, float? abyssCommonness, float? caveCommonness)
			{
				this.commonness = commonness;
				this.abyssCommonness = ((abyssCommonness != null) ? new float?(Math.Max(abyssCommonness.Value, 0f)) : null);
				this.caveCommonness = ((caveCommonness != null) ? new float?(Math.Max(caveCommonness.Value, 0f)) : null);
			}

			// Token: 0x06005A99 RID: 23193 RVA: 0x001FC4C8 File Offset: 0x001FA6C8
			public ItemPrefab.CommonnessInfo WithInheritedCommonness(ItemPrefab.CommonnessInfo? parentInfo)
			{
				float num = this.commonness;
				float? num2 = this.abyssCommonness;
				float? num3 = (num2 != null) ? num2 : ((parentInfo != null) ? parentInfo.GetValueOrDefault().abyssCommonness : null);
				num2 = this.caveCommonness;
				return new ItemPrefab.CommonnessInfo(num, num3, (num2 != null) ? num2 : ((parentInfo != null) ? parentInfo.GetValueOrDefault().caveCommonness : null));
			}

			// Token: 0x06005A9A RID: 23194 RVA: 0x001FC548 File Offset: 0x001FA748
			public ItemPrefab.CommonnessInfo WithInheritedCommonness(params ItemPrefab.CommonnessInfo?[] parentInfos)
			{
				ItemPrefab.CommonnessInfo info = this;
				foreach (ItemPrefab.CommonnessInfo parentInfo in parentInfos)
				{
					info = info.WithInheritedCommonness(parentInfo);
				}
				return info;
			}

			// Token: 0x06005A9B RID: 23195 RVA: 0x001FC57E File Offset: 0x001FA77E
			public float GetCommonness(Level.TunnelType tunnelType)
			{
				if (tunnelType == Level.TunnelType.Cave)
				{
					return this.CaveCommonness;
				}
				return this.Commonness;
			}

			// Token: 0x04003422 RID: 13346
			public readonly float commonness;

			// Token: 0x04003423 RID: 13347
			public readonly float? abyssCommonness;

			// Token: 0x04003424 RID: 13348
			public readonly float? caveCommonness;
		}

		// Token: 0x020009B3 RID: 2483
		public readonly struct FixedQuantityResourceInfo
		{
			// Token: 0x06005A9C RID: 23196 RVA: 0x001FC591 File Offset: 0x001FA791
			public FixedQuantityResourceInfo(int clusterQuantity, int clusterSize, bool isIslandSpecific, bool allowAtStart)
			{
				this.ClusterQuantity = clusterQuantity;
				this.ClusterSize = clusterSize;
				this.IsIslandSpecific = isIslandSpecific;
				this.AllowAtStart = allowAtStart;
			}

			// Token: 0x04003425 RID: 13349
			public readonly int ClusterQuantity;

			// Token: 0x04003426 RID: 13350
			public readonly int ClusterSize;

			// Token: 0x04003427 RID: 13351
			public readonly bool IsIslandSpecific;

			// Token: 0x04003428 RID: 13352
			public readonly bool AllowAtStart;
		}
	}
}

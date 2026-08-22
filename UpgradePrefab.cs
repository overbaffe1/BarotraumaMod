using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200014E RID: 334
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class UpgradePrefab : UpgradeContentPrefab
	{
		// Token: 0x17000AA9 RID: 2729
		// (get) Token: 0x060029F2 RID: 10738 RVA: 0x001D0921 File Offset: 0x001CEB21
		// (set) Token: 0x060029F3 RID: 10739 RVA: 0x001D0929 File Offset: 0x001CEB29
		[Nullable(0)]
		public Sprite Sprite { [NullableContext(0)] get; [NullableContext(0)] private set; }

		// Token: 0x17000AAA RID: 2730
		// (get) Token: 0x060029F4 RID: 10740 RVA: 0x001D0932 File Offset: 0x001CEB32
		public LocalizedString Name { get; }

		// Token: 0x17000AAB RID: 2731
		// (get) Token: 0x060029F5 RID: 10741 RVA: 0x001D093A File Offset: 0x001CEB3A
		public LocalizedString Description { get; }

		// Token: 0x17000AAC RID: 2732
		// (get) Token: 0x060029F6 RID: 10742 RVA: 0x001D0942 File Offset: 0x001CEB42
		public float IncreaseOnTooltip { get; }

		// Token: 0x17000AAD RID: 2733
		// (get) Token: 0x060029F7 RID: 10743 RVA: 0x001D094C File Offset: 0x001CEB4C
		public IEnumerable<UpgradeCategory> UpgradeCategories
		{
			get
			{
				UpgradePrefab.<get_UpgradeCategories>d__18 <get_UpgradeCategories>d__ = new UpgradePrefab.<get_UpgradeCategories>d__18(-2);
				<get_UpgradeCategories>d__.<>4__this = this;
				return <get_UpgradeCategories>d__;
			}
		}

		// Token: 0x17000AAE RID: 2734
		// (get) Token: 0x060029F8 RID: 10744 RVA: 0x001D0969 File Offset: 0x001CEB69
		public UpgradePrice Price { get; }

		// Token: 0x17000AAF RID: 2735
		// (get) Token: 0x060029F9 RID: 10745 RVA: 0x001D0971 File Offset: 0x001CEB71
		private bool isOverride
		{
			get
			{
				return UpgradePrefab.Prefabs.IsOverride(this);
			}
		}

		// Token: 0x17000AB0 RID: 2736
		// (get) Token: 0x060029FA RID: 10746 RVA: 0x001D097E File Offset: 0x001CEB7E
		public ContentXElement SourceElement { get; }

		// Token: 0x17000AB1 RID: 2737
		// (get) Token: 0x060029FB RID: 10747 RVA: 0x001D0986 File Offset: 0x001CEB86
		public bool SuppressWarnings { get; }

		// Token: 0x17000AB2 RID: 2738
		// (get) Token: 0x060029FC RID: 10748 RVA: 0x001D098E File Offset: 0x001CEB8E
		public bool HideInMenus { get; }

		// Token: 0x17000AB3 RID: 2739
		// (get) Token: 0x060029FD RID: 10749 RVA: 0x001D0996 File Offset: 0x001CEB96
		public IEnumerable<Identifier> TargetItems
		{
			get
			{
				return this.UpgradeCategories.SelectMany((UpgradeCategory u) => u.ItemTags);
			}
		}

		// Token: 0x17000AB4 RID: 2740
		// (get) Token: 0x060029FE RID: 10750 RVA: 0x001D09C2 File Offset: 0x001CEBC2
		public bool IsWallUpgrade
		{
			get
			{
				return this.UpgradeCategories.All((UpgradeCategory u) => u.IsWallUpgrade);
			}
		}

		// Token: 0x17000AB5 RID: 2741
		// (get) Token: 0x060029FF RID: 10751 RVA: 0x001D09EE File Offset: 0x001CEBEE
		private Dictionary<string, string[]> targetProperties { get; }

		// Token: 0x17000AB6 RID: 2742
		// (get) Token: 0x06002A00 RID: 10752 RVA: 0x001D09F8 File Offset: 0x001CEBF8
		public static int CrushDepthUpgradePrc
		{
			get
			{
				if (UpgradePrefab.crushDepthUpgradePrc == null)
				{
					UpgradePrefab hullUpgradePrefab = UpgradePrefab.Find("increasewallhealth".ToIdentifier());
					if (hullUpgradePrefab != null)
					{
						ContentXElement sourceElement = hullUpgradePrefab.SourceElement;
						string text;
						if (sourceElement == null)
						{
							text = null;
						}
						else
						{
							ContentXElement childElement = sourceElement.GetChildElement("Structure");
							text = ((childElement != null) ? childElement.GetAttributeString("crushdepth", null) : null);
						}
						string updateValueStr = text ?? string.Empty;
						if (!string.IsNullOrEmpty(updateValueStr))
						{
							UpgradePrefab.crushDepthUpgradePrc = new int?(UpgradePrefab.ParsePercentage(updateValueStr, Identifier.Empty, null, true));
						}
					}
				}
				return UpgradePrefab.crushDepthUpgradePrc.GetValueOrDefault(15);
			}
		}

		// Token: 0x17000AB7 RID: 2743
		// (get) Token: 0x06002A01 RID: 10753 RVA: 0x001D0A84 File Offset: 0x001CEC84
		public static int IncreaseWallHealthMaxLevel
		{
			get
			{
				if (UpgradePrefab.increaseWallHealthMaxLevel == null)
				{
					UpgradePrefab hullUpgradePrefab = UpgradePrefab.Find("increasewallhealth".ToIdentifier());
					if (hullUpgradePrefab != null)
					{
						UpgradePrefab.increaseWallHealthMaxLevel = new int?(hullUpgradePrefab.MaxLevel);
					}
				}
				return UpgradePrefab.increaseWallHealthMaxLevel.GetValueOrDefault(6);
			}
		}

		// Token: 0x06002A02 RID: 10754 RVA: 0x001D0ACC File Offset: 0x001CECCC
		public UpgradePrefab(ContentXElement element, UpgradeModulesFile file) : base(element, file)
		{
			this.Name = element.GetAttributeString("Name", string.Empty);
			this.Description = element.GetAttributeString("Description", string.Empty);
			this.MaxLevel = element.GetAttributeInt("MaxLevel", 1);
			this.SuppressWarnings = element.GetAttributeBool("SuppressWarnings", false);
			this.HideInMenus = element.GetAttributeBool("HideInMenus", false);
			this.SourceElement = element;
			Dictionary<string, string[]> targetProperties = new Dictionary<string, string[]>();
			List<UpgradeMaxLevelMod> maxLevels = new List<UpgradeMaxLevelMod>();
			HashSet<UpgradeResourceCost> resourceCosts = new HashSet<UpgradeResourceCost>();
			Identifier nameIdentifier = element.GetAttributeIdentifier("nameidentifier", "");
			if (!nameIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler.AppendLiteral("UpgradeName.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(nameIdentifier);
				this.Name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			else if (this.Name.IsNullOrWhiteSpace())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("UpgradeName.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				this.Name = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			Identifier descriptionIdentifier = element.GetAttributeIdentifier("descriptionidentifier", "");
			if (!descriptionIdentifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("UpgradeDescription.");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(descriptionIdentifier);
				this.Description = TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			else if (this.Description.IsNullOrWhiteSpace())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(19, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("UpgradeDescription.");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.Identifier);
				this.Description = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear());
			}
			this.IncreaseOnTooltip = element.GetAttributeFloat("increaseontooltip", 0f);
			DebugConsole.Log("    " + this.Name);
			List<DecorativeSprite> decorativeSprites = new List<DecorativeSprite>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "price"))
				{
					if (!(a == "maxlevel"))
					{
						if (!(a == "resourcecost"))
						{
							if (!(a == "decorativesprite"))
							{
								if (!(a == "sprite"))
								{
									IEnumerable<string> properties = from attribute in subElement.Attributes()
									select attribute.Name.ToString();
									targetProperties.Add(subElement.Name.ToString(), properties.ToArray<string>());
								}
								else
								{
									this.Sprite = new Sprite(subElement, "", "", false, 1f);
								}
							}
							else
							{
								decorativeSprites.Add(new DecorativeSprite(subElement, "", "", false));
							}
						}
						else
						{
							resourceCosts.Add(new UpgradeResourceCost(subElement));
						}
					}
					else
					{
						maxLevels.Add(new UpgradeMaxLevelMod(subElement));
					}
				}
				else
				{
					this.Price = new UpgradePrice(this, subElement);
				}
			}
			this.DecorativeSprites = decorativeSprites.ToImmutableArray<DecorativeSprite>();
			this.targetProperties = targetProperties;
			this.MaxLevelsMods = maxLevels.ToImmutableArray<UpgradeMaxLevelMod>();
			this.ResourceCosts = resourceCosts.ToImmutableHashSet<UpgradeResourceCost>();
			Identifier[] attributeIdentifierArray = element.GetAttributeIdentifierArray("categories", Array.Empty<Identifier>(), true);
			this.upgradeCategoryIdentifiers = (((attributeIdentifierArray != null) ? attributeIdentifierArray.ToImmutableHashSet<Identifier>() : null) ?? ImmutableHashSet<Identifier>.Empty);
		}

		// Token: 0x06002A03 RID: 10755 RVA: 0x001D0E78 File Offset: 0x001CF078
		public int GetMaxLevelForCurrentSub()
		{
			GameSession gameSession = GameMain.GameSession;
			Submarine sub = ((gameSession != null) ? gameSession.Submarine : null) ?? Submarine.MainSub;
			if (sub != null)
			{
				SubmarineInfo info = sub.Info;
				return this.GetMaxLevel(info);
			}
			return this.MaxLevel;
		}

		// Token: 0x06002A04 RID: 10756 RVA: 0x001D0EBC File Offset: 0x001CF0BC
		public int GetMaxLevel(SubmarineInfo info)
		{
			int level = this.MaxLevel;
			int tier = info.Tier;
			GameSession gameSession = GameMain.GameSession;
			CampaignMetadata campaignMetadata;
			if (gameSession == null)
			{
				campaignMetadata = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				campaignMetadata = ((campaign != null) ? campaign.CampaignMetadata : null);
			}
			CampaignMetadata metadata = campaignMetadata;
			if (metadata != null)
			{
				CampaignMetadata campaignMetadata2 = metadata;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("tiermodifiers.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				int modifier = campaignMetadata2.GetInt(new Identifier(defaultInterpolatedStringHandler.ToStringAndClear()), new int?(0));
				tier += modifier;
			}
			tier = Math.Clamp(tier, 1, 3);
			foreach (UpgradeMaxLevelMod mod in this.MaxLevelsMods)
			{
				if (mod.AppliesTo(info.SubmarineClass, tier))
				{
					level = mod.GetLevelAfter(level);
				}
			}
			return level;
		}

		// Token: 0x06002A05 RID: 10757 RVA: 0x001D0F7C File Offset: 0x001CF17C
		[NullableContext(2)]
		public bool IsApplicable(SubmarineInfo info)
		{
			return info != null && this.GetMaxLevel(info) > 0;
		}

		// Token: 0x06002A06 RID: 10758 RVA: 0x001D0F90 File Offset: 0x001CF190
		[NullableContext(2)]
		public bool HasResourcesToUpgrade(Character character, int currentLevel)
		{
			if (character == null)
			{
				return false;
			}
			if (!this.ResourceCosts.Any<UpgradeResourceCost>())
			{
				return true;
			}
			IReadOnlyCollection<Item> allItems = CargoManager.FindAllItemsOnPlayerAndSub(character);
			return (from cost in this.ResourceCosts
			where cost.AppliesForLevel(currentLevel)
			select cost).All((UpgradeResourceCost cost) => cost.Amount <= allItems.Count(new Func<Item, bool>(cost.MatchesItem)));
		}

		// Token: 0x06002A07 RID: 10759 RVA: 0x001D0FF4 File Offset: 0x001CF1F4
		public bool TryTakeResources(Character character, int currentLevel)
		{
			IEnumerable<UpgradeResourceCost> costs = from cost in this.ResourceCosts
			where cost.AppliesForLevel(currentLevel)
			select cost;
			if (!costs.Any<UpgradeResourceCost>())
			{
				return true;
			}
			IReadOnlyCollection<Item> inventoryItems = CargoManager.FindAllItemsOnPlayerAndSub(character);
			HashSet<Item> itemsToRemove = new HashSet<Item>();
			foreach (UpgradeResourceCost cost2 in costs)
			{
				int amountNeeded = cost2.Amount;
				foreach (Item item in inventoryItems.Where(new Func<Item, bool>(cost2.MatchesItem)))
				{
					itemsToRemove.Add(item);
					amountNeeded--;
					if (amountNeeded <= 0)
					{
						break;
					}
				}
				if (amountNeeded > 0)
				{
					return false;
				}
			}
			foreach (Item item2 in itemsToRemove)
			{
				Entity.Spawner.AddItemToRemoveQueue(item2);
			}
			if (GameMain.IsMultiplayer)
			{
				character.Inventory.CreateNetworkEvent();
			}
			return true;
		}

		// Token: 0x06002A08 RID: 10760 RVA: 0x001D1148 File Offset: 0x001CF348
		[NullableContext(0)]
		public ImmutableArray<ApplicableResourceCollection> GetApplicableResources(int level)
		{
			ImmutableHashSet<UpgradeResourceCost> applicableCosts = (from cost in this.ResourceCosts
			where cost.AppliesForLevel(level)
			select cost).ToImmutableHashSet<UpgradeResourceCost>();
			ImmutableArray<ApplicableResourceCollection> result;
			if (!applicableCosts.Any<UpgradeResourceCost>())
			{
				result = ImmutableArray<ApplicableResourceCollection>.Empty;
			}
			else
			{
				IEnumerable<UpgradeResourceCost> source = applicableCosts;
				Func<UpgradeResourceCost, ApplicableResourceCollection> selector;
				if ((selector = UpgradePrefab.<>O.<0>__CreateFor) == null)
				{
					selector = (UpgradePrefab.<>O.<0>__CreateFor = new Func<UpgradeResourceCost, ApplicableResourceCollection>(ApplicableResourceCollection.CreateFor));
				}
				result = source.Select(selector).ToImmutableArray<ApplicableResourceCollection>();
			}
			return result;
		}

		// Token: 0x06002A09 RID: 10761 RVA: 0x001D11B8 File Offset: 0x001CF3B8
		public bool IsDisallowed(MapEntity item)
		{
			return item.DisallowedUpgradeSet.Contains(this.Identifier) || this.UpgradeCategories.Any((UpgradeCategory c) => item.DisallowedUpgradeSet.Contains(c.Identifier));
		}

		// Token: 0x06002A0A RID: 10762 RVA: 0x001D1204 File Offset: 0x001CF404
		[NullableContext(2)]
		public static UpgradePrefab Find(Identifier identifier)
		{
			if (!(identifier != Identifier.Empty))
			{
				return null;
			}
			return UpgradePrefab.Prefabs.Find((UpgradePrefab prefab) => prefab.Identifier == identifier);
		}

		// Token: 0x06002A0B RID: 10763 RVA: 0x001D1248 File Offset: 0x001CF448
		public static int ParsePercentage(string value, Identifier attribute = default(Identifier), [Nullable(2)] XElement sourceElement = null, bool suppressWarnings = false)
		{
			string line = (sourceElement != null) ? sourceElement.ToString().Split('\n', StringSplitOptions.None)[0].Trim() : null;
			bool doWarnings = !suppressWarnings && !attribute.IsEmpty && sourceElement != null && line != null;
			if (string.IsNullOrWhiteSpace(value))
			{
				if (doWarnings)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(34, 3);
					defaultInterpolatedStringHandler.AppendLiteral("Attribute \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(attribute);
					defaultInterpolatedStringHandler.AppendLiteral("\" not found at ");
					XDocument document = sourceElement.Document;
					defaultInterpolatedStringHandler.AppendFormatted((document != null) ? document.ParseContentPathFromUri() : null);
					defaultInterpolatedStringHandler.AppendLiteral(" @ '");
					defaultInterpolatedStringHandler.AppendFormatted(line);
					defaultInterpolatedStringHandler.AppendLiteral("'.\n ");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "Value has been assumed to be '0'.", null);
				}
				return 1;
			}
			int price;
			if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
			{
				return price;
			}
			string str = value;
			if (str.Length > 1 && str[0] == '+')
			{
				str = str.Substring(1);
			}
			if (str.Length > 1)
			{
				string text = str;
				if (text[text.Length - 1] == '%')
				{
					str = str.Substring(0, str.Length - 1);
				}
			}
			if (int.TryParse(str, out price))
			{
				return price;
			}
			if (doWarnings)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(61, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Value in attribute \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(attribute);
				defaultInterpolatedStringHandler2.AppendLiteral("\" is not formatted correctly\n ");
				defaultInterpolatedStringHandler2.AppendLiteral("at ");
				XDocument document2 = sourceElement.Document;
				defaultInterpolatedStringHandler2.AppendFormatted((document2 != null) ? document2.ParseContentPathFromUri() : null);
				defaultInterpolatedStringHandler2.AppendLiteral(" @ '");
				defaultInterpolatedStringHandler2.AppendFormatted(line);
				defaultInterpolatedStringHandler2.AppendLiteral("'.\n ");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear() + "It should be an integer with optionally a '+' or '-' at the front and/or '%' at the end.\nThe value has been assumed to be '0'.", null);
			}
			return 1;
		}

		// Token: 0x06002A0C RID: 10764 RVA: 0x001D1410 File Offset: 0x001CF610
		public override void Dispose()
		{
			Sprite sprite2 = this.Sprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			this.Sprite = null;
			this.DecorativeSprites.ForEach(delegate(DecorativeSprite sprite)
			{
				sprite.Remove();
			});
			this.targetProperties.Clear();
		}

		// Token: 0x040015EB RID: 5611
		[Nullable(0)]
		public readonly ImmutableArray<DecorativeSprite> DecorativeSprites;

		// Token: 0x040015ED RID: 5613
		public static readonly PrefabCollection<UpgradePrefab> Prefabs = new PrefabCollection<UpgradePrefab>(delegate(UpgradePrefab prefab, bool isOverride)
		{
			if (!prefab.SuppressWarnings && !isOverride)
			{
				PrefabCollection<UpgradePrefab> prefabs = UpgradePrefab.Prefabs;
				object obj;
				if (prefabs == null)
				{
					obj = null;
				}
				else
				{
					Func<UpgradePrefab, bool> <>9__1;
					Func<UpgradePrefab, bool> predicate;
					if ((predicate = <>9__1) == null)
					{
						Func<Identifier, bool> <>9__2;
						predicate = (<>9__1 = delegate(UpgradePrefab p)
						{
							if (p != prefab)
							{
								IEnumerable<Identifier> targetItems = p.TargetItems;
								Func<Identifier, bool> predicate2;
								if ((predicate2 = <>9__2) == null)
								{
									predicate2 = (<>9__2 = ((Identifier s) => prefab.TargetItems.Contains(s)));
								}
								return targetItems.Any(predicate2);
							}
							return false;
						});
					}
					obj = prefabs.Where(predicate);
				}
				object obj2 = obj;
				if (obj2 == null)
				{
					throw new NullReferenceException("Honestly I have no clue why this could be null...");
				}
				foreach (UpgradePrefab matchingPrefab in ((IEnumerable<UpgradePrefab>)obj2))
				{
					if (!matchingPrefab.isOverride)
					{
						Dictionary<string, string[]> upgradePrefab = matchingPrefab.targetProperties;
						string key = string.Empty;
						if (upgradePrefab.Keys.Any((string s) => prefab.targetProperties.Keys.Any(delegate(string s1)
						{
							string s2 = s;
							key = s1;
							return s2 == s1;
						})) && upgradePrefab.ContainsKey(key) && upgradePrefab[key].Any((string s) => prefab.targetProperties[key].Contains(s)))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Upgrade \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" is affecting a property that is also being affected by \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(matchingPrefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\".\n");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "This is unsupported and might yield unexpected results if both upgrades are applied at the same time to the same item.\nAdd the attribute suppresswarnings=\"true\" to your XML element to disable this warning if you know what you're doing.", prefab.ContentPackage);
						}
					}
				}
			}
		}, null, null, null, null);

		// Token: 0x040015EE RID: 5614
		public readonly int MaxLevel;

		// Token: 0x040015F2 RID: 5618
		private readonly ImmutableHashSet<Identifier> upgradeCategoryIdentifiers;

		// Token: 0x040015F8 RID: 5624
		[Nullable(0)]
		private readonly ImmutableArray<UpgradeMaxLevelMod> MaxLevelsMods;

		// Token: 0x040015F9 RID: 5625
		public readonly ImmutableHashSet<UpgradeResourceCost> ResourceCosts;

		// Token: 0x040015FA RID: 5626
		public const int CrushDepthDefaultUpgradePrc = 15;

		// Token: 0x040015FB RID: 5627
		private static int? crushDepthUpgradePrc;

		// Token: 0x040015FC RID: 5628
		public const int IncreaseWallHealthDefaultMaxLevel = 6;

		// Token: 0x040015FD RID: 5629
		private static int? increaseWallHealthMaxLevel;

		// Token: 0x02000DB8 RID: 3512
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400505C RID: 20572
			[Nullable(0)]
			public static Func<UpgradeResourceCost, ApplicableResourceCollection> <0>__CreateFor;
		}
	}
}

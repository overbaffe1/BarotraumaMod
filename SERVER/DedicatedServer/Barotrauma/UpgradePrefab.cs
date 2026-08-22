using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002BA RID: 698
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class UpgradePrefab : UpgradeContentPrefab
	{
		// Token: 0x17000DC7 RID: 3527
		// (get) Token: 0x06002F92 RID: 12178 RVA: 0x0013C7E5 File Offset: 0x0013A9E5
		public LocalizedString Name { get; }

		// Token: 0x17000DC8 RID: 3528
		// (get) Token: 0x06002F93 RID: 12179 RVA: 0x0013C7ED File Offset: 0x0013A9ED
		public LocalizedString Description { get; }

		// Token: 0x17000DC9 RID: 3529
		// (get) Token: 0x06002F94 RID: 12180 RVA: 0x0013C7F5 File Offset: 0x0013A9F5
		public float IncreaseOnTooltip { get; }

		// Token: 0x17000DCA RID: 3530
		// (get) Token: 0x06002F95 RID: 12181 RVA: 0x0013C800 File Offset: 0x0013AA00
		public IEnumerable<UpgradeCategory> UpgradeCategories
		{
			get
			{
				UpgradePrefab.<get_UpgradeCategories>d__13 <get_UpgradeCategories>d__ = new UpgradePrefab.<get_UpgradeCategories>d__13(-2);
				<get_UpgradeCategories>d__.<>4__this = this;
				return <get_UpgradeCategories>d__;
			}
		}

		// Token: 0x17000DCB RID: 3531
		// (get) Token: 0x06002F96 RID: 12182 RVA: 0x0013C81D File Offset: 0x0013AA1D
		public UpgradePrice Price { get; }

		// Token: 0x17000DCC RID: 3532
		// (get) Token: 0x06002F97 RID: 12183 RVA: 0x0013C825 File Offset: 0x0013AA25
		private bool isOverride
		{
			get
			{
				return UpgradePrefab.Prefabs.IsOverride(this);
			}
		}

		// Token: 0x17000DCD RID: 3533
		// (get) Token: 0x06002F98 RID: 12184 RVA: 0x0013C832 File Offset: 0x0013AA32
		public ContentXElement SourceElement { get; }

		// Token: 0x17000DCE RID: 3534
		// (get) Token: 0x06002F99 RID: 12185 RVA: 0x0013C83A File Offset: 0x0013AA3A
		public bool SuppressWarnings { get; }

		// Token: 0x17000DCF RID: 3535
		// (get) Token: 0x06002F9A RID: 12186 RVA: 0x0013C842 File Offset: 0x0013AA42
		public bool HideInMenus { get; }

		// Token: 0x17000DD0 RID: 3536
		// (get) Token: 0x06002F9B RID: 12187 RVA: 0x0013C84A File Offset: 0x0013AA4A
		public IEnumerable<Identifier> TargetItems
		{
			get
			{
				return this.UpgradeCategories.SelectMany((UpgradeCategory u) => u.ItemTags);
			}
		}

		// Token: 0x17000DD1 RID: 3537
		// (get) Token: 0x06002F9C RID: 12188 RVA: 0x0013C876 File Offset: 0x0013AA76
		public bool IsWallUpgrade
		{
			get
			{
				return this.UpgradeCategories.All((UpgradeCategory u) => u.IsWallUpgrade);
			}
		}

		// Token: 0x17000DD2 RID: 3538
		// (get) Token: 0x06002F9D RID: 12189 RVA: 0x0013C8A2 File Offset: 0x0013AAA2
		private Dictionary<string, string[]> targetProperties { get; }

		// Token: 0x17000DD3 RID: 3539
		// (get) Token: 0x06002F9E RID: 12190 RVA: 0x0013C8AC File Offset: 0x0013AAAC
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

		// Token: 0x17000DD4 RID: 3540
		// (get) Token: 0x06002F9F RID: 12191 RVA: 0x0013C938 File Offset: 0x0013AB38
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

		// Token: 0x06002FA0 RID: 12192 RVA: 0x0013C980 File Offset: 0x0013AB80
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
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "price"))
				{
					if (!(a == "maxlevel"))
					{
						if (!(a == "resourcecost"))
						{
							if (!(a == "decorativesprite") && !(a == "sprite"))
							{
								IEnumerable<string> properties = from attribute in subElement.Attributes()
								select attribute.Name.ToString();
								targetProperties.Add(subElement.Name.ToString(), properties.ToArray<string>());
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
			this.targetProperties = targetProperties;
			this.MaxLevelsMods = maxLevels.ToImmutableArray<UpgradeMaxLevelMod>();
			this.ResourceCosts = resourceCosts.ToImmutableHashSet<UpgradeResourceCost>();
			Identifier[] attributeIdentifierArray = element.GetAttributeIdentifierArray("categories", Array.Empty<Identifier>(), true);
			this.upgradeCategoryIdentifiers = (((attributeIdentifierArray != null) ? attributeIdentifierArray.ToImmutableHashSet<Identifier>() : null) ?? ImmutableHashSet<Identifier>.Empty);
		}

		// Token: 0x06002FA1 RID: 12193 RVA: 0x0013CCCC File Offset: 0x0013AECC
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

		// Token: 0x06002FA2 RID: 12194 RVA: 0x0013CD10 File Offset: 0x0013AF10
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

		// Token: 0x06002FA3 RID: 12195 RVA: 0x0013CDD0 File Offset: 0x0013AFD0
		[NullableContext(2)]
		public bool IsApplicable(SubmarineInfo info)
		{
			return info != null && this.GetMaxLevel(info) > 0;
		}

		// Token: 0x06002FA4 RID: 12196 RVA: 0x0013CDE4 File Offset: 0x0013AFE4
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

		// Token: 0x06002FA5 RID: 12197 RVA: 0x0013CE48 File Offset: 0x0013B048
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

		// Token: 0x06002FA6 RID: 12198 RVA: 0x0013CF9C File Offset: 0x0013B19C
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

		// Token: 0x06002FA7 RID: 12199 RVA: 0x0013D00C File Offset: 0x0013B20C
		public bool IsDisallowed(MapEntity item)
		{
			return item.DisallowedUpgradeSet.Contains(this.Identifier) || this.UpgradeCategories.Any((UpgradeCategory c) => item.DisallowedUpgradeSet.Contains(c.Identifier));
		}

		// Token: 0x06002FA8 RID: 12200 RVA: 0x0013D058 File Offset: 0x0013B258
		[NullableContext(2)]
		public static UpgradePrefab Find(Identifier identifier)
		{
			if (!(identifier != Identifier.Empty))
			{
				return null;
			}
			return UpgradePrefab.Prefabs.Find((UpgradePrefab prefab) => prefab.Identifier == identifier);
		}

		// Token: 0x06002FA9 RID: 12201 RVA: 0x0013D09C File Offset: 0x0013B29C
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

		// Token: 0x06002FAA RID: 12202 RVA: 0x0013D263 File Offset: 0x0013B463
		public override void Dispose()
		{
		}

		// Token: 0x040017E4 RID: 6116
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

		// Token: 0x040017E5 RID: 6117
		public readonly int MaxLevel;

		// Token: 0x040017E9 RID: 6121
		private readonly ImmutableHashSet<Identifier> upgradeCategoryIdentifiers;

		// Token: 0x040017EF RID: 6127
		[Nullable(0)]
		private readonly ImmutableArray<UpgradeMaxLevelMod> MaxLevelsMods;

		// Token: 0x040017F0 RID: 6128
		public readonly ImmutableHashSet<UpgradeResourceCost> ResourceCosts;

		// Token: 0x040017F1 RID: 6129
		public const int CrushDepthDefaultUpgradePrc = 15;

		// Token: 0x040017F2 RID: 6130
		private static int? crushDepthUpgradePrc;

		// Token: 0x040017F3 RID: 6131
		public const int IncreaseWallHealthDefaultMaxLevel = 6;

		// Token: 0x040017F4 RID: 6132
		private static int? increaseWallHealthMaxLevel;

		// Token: 0x02000B50 RID: 2896
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400392D RID: 14637
			[Nullable(0)]
			public static Func<UpgradeResourceCost, ApplicableResourceCollection> <0>__CreateFor;
		}
	}
}

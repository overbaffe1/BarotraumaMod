using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002E2 RID: 738
	internal class FabricationRecipe
	{
		// Token: 0x17001040 RID: 4160
		// (get) Token: 0x06003D6E RID: 15726 RVA: 0x0022E644 File Offset: 0x0022C844
		public ItemPrefab TargetItem
		{
			get
			{
				return ItemPrefab.Prefabs[this.TargetItemPrefabIdentifier];
			}
		}

		// Token: 0x17001041 RID: 4161
		// (get) Token: 0x06003D6F RID: 15727 RVA: 0x0022E656 File Offset: 0x0022C856
		public LocalizedString DisplayName
		{
			get
			{
				if (!ItemPrefab.Prefabs.ContainsKey(this.TargetItemPrefabIdentifier))
				{
					return "";
				}
				return this.displayName.Value;
			}
		}

		// Token: 0x06003D70 RID: 15728 RVA: 0x0022E680 File Offset: 0x0022C880
		public FabricationRecipe(ContentXElement element, Identifier itemPrefab)
		{
			FabricationRecipe <>4__this = this;
			this.TargetItemPrefabIdentifier = itemPrefab;
			Identifier displayNameIdentifier = element.GetAttributeIdentifier("displayname", "");
			this.displayName = new Lazy<LocalizedString>(delegate()
			{
				if (!displayNameIdentifier.IsEmpty)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("DisplayName.");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(displayNameIdentifier);
					return TextManager.GetWithVariable(defaultInterpolatedStringHandler2.ToStringAndClear(), "[itemname]", <>4__this.TargetItem.Name, FormatCapitals.No);
				}
				return <>4__this.TargetItem.Name;
			});
			this.SuitableFabricatorIdentifiers = element.GetAttributeIdentifierArray("suitablefabricators", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			List<Skill> requiredSkills = new List<Skill>();
			this.RequiredTime = element.GetAttributeFloat("requiredtime", 1f);
			this.RequiredMoney = element.GetAttributeInt("requiredmoney", 0);
			this.OutCondition = element.GetAttributeFloat("outcondition", 1f);
			if (this.OutCondition > 1f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(itemPrefab);
				defaultInterpolatedStringHandler.AppendLiteral("\"'s fabrication recipe: out condition is above 100% (");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.OutCondition * 100f);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), element.ContentPackage);
			}
			List<FabricationRecipe.RequiredItem> requiredItems = new List<FabricationRecipe.RequiredItem>();
			this.RequiresRecipe = element.GetAttributeBool("requiresrecipe", false);
			this.HideIfNoRecipe = element.GetAttributeBool("hideifnorecipe", false);
			this.Amount = element.GetAttributeInt("amount", 1);
			int limitDefault = element.GetAttributeInt("fabricationlimit", -1);
			this.FabricationLimitMin = element.GetAttributeInt("FabricationLimitMin", limitDefault);
			this.FabricationLimitMax = element.GetAttributeInt("FabricationLimitMax", limitDefault);
			this.HideForNonTraitors = element.GetAttributeBool("HideForNonTraitors", false);
			string key = "MoveToSlot";
			InvSlotType invSlotType = InvSlotType.None;
			this.MoveToSlot = element.GetAttributeEnum<InvSlotType>(key, invSlotType);
			if (element.GetAttribute("Quality") != null)
			{
				this.Quality = new int?(element.GetAttributeInt("Quality", 0));
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "requiredskill"))
				{
					if (a == "item" || a == "requireditem")
					{
						Identifier requiredItemIdentifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						Identifier requiredItemTag = subElement.GetAttributeIdentifier("tag", Identifier.Empty);
						if (requiredItemIdentifier == Identifier.Empty && requiredItemTag == Identifier.Empty)
						{
							DebugConsole.ThrowError("Error in fabricable item " + itemPrefab.ToString() + "! One of the required items has no identifier or tag.", null, element.ContentPackage, false, false);
						}
						else
						{
							float minCondition = subElement.GetAttributeFloat("mincondition", 1f);
							float maxCondition = subElement.GetAttributeFloat("maxcondition", 1f);
							bool useCondition = subElement.GetAttributeBool("usecondition", true);
							int amount = subElement.GetAttributeInt("count", subElement.GetAttributeInt("amount", 1));
							LocalizedString overrideDescription = string.Empty;
							string descriptionTag = subElement.GetAttributeString("description", string.Empty);
							if (descriptionTag != null && !descriptionTag.IsNullOrEmpty())
							{
								overrideDescription = TextManager.Get(descriptionTag);
							}
							LocalizedString overrideHeader = string.Empty;
							string headerTag = subElement.GetAttributeString("header", string.Empty);
							if (headerTag != null && !headerTag.IsNullOrEmpty())
							{
								overrideHeader = TextManager.Get(headerTag);
							}
							if (requiredItemIdentifier != Identifier.Empty)
							{
								int existing = requiredItems.FindIndex(delegate(FabricationRecipe.RequiredItem r)
								{
									FabricationRecipe.RequiredItemByIdentifier ri = r as FabricationRecipe.RequiredItemByIdentifier;
									return ri != null && ri.ItemPrefabIdentifier == requiredItemIdentifier && MathUtils.NearlyEqual(r.MinCondition, minCondition, 0.0001f) && MathUtils.NearlyEqual(r.MaxCondition, maxCondition, 0.0001f);
								});
								if (existing >= 0)
								{
									amount += requiredItems[existing].Amount;
									requiredItems.RemoveAt(existing);
								}
								requiredItems.Add(new FabricationRecipe.RequiredItemByIdentifier(requiredItemIdentifier, amount, minCondition, maxCondition, useCondition, overrideDescription, overrideHeader));
							}
							else
							{
								int existing2 = requiredItems.FindIndex(delegate(FabricationRecipe.RequiredItem r)
								{
									FabricationRecipe.RequiredItemByTag rt = r as FabricationRecipe.RequiredItemByTag;
									return rt != null && rt.Tag == requiredItemTag && MathUtils.NearlyEqual(r.MinCondition, minCondition, 0.0001f) && MathUtils.NearlyEqual(r.MaxCondition, maxCondition, 0.0001f);
								});
								if (existing2 >= 0)
								{
									amount += requiredItems[existing2].Amount;
									requiredItems.RemoveAt(existing2);
								}
								Identifier defaultItem = subElement.GetAttributeIdentifier("defaultitem", Identifier.Empty);
								requiredItems.Add(new FabricationRecipe.RequiredItemByTag(requiredItemTag, amount, minCondition, maxCondition, useCondition, overrideDescription, overrideHeader, defaultItem));
							}
						}
					}
				}
				else if (subElement.GetAttribute("name") != null)
				{
					DebugConsole.ThrowError("Error in fabricable item " + itemPrefab.ToString() + "! Use skill identifiers instead of names.", null, element.ContentPackage, false, false);
				}
				else
				{
					requiredSkills.Add(new Skill(subElement.GetAttributeIdentifier("identifier", ""), (float)subElement.GetAttributeInt("level", 0)));
				}
			}
			this.RequiredSkills = requiredSkills.ToImmutableArray<Skill>();
			this.RequiredItems = (from requiredItem in requiredItems
			orderby (!(requiredItem is FabricationRecipe.RequiredItemByIdentifier)) ? 1 : 0
			select requiredItem).ToImmutableArray<FabricationRecipe.RequiredItem>();
			this.RecipeHash = this.GenerateHash();
		}

		// Token: 0x06003D71 RID: 15729 RVA: 0x0022EBD4 File Offset: 0x0022CDD4
		private uint GenerateHash()
		{
			uint result;
			using (MD5 md5 = MD5.Create())
			{
				uint outputId = ToolBoxCore.IdentifierToUint32Hash(this.TargetItemPrefabIdentifier, md5);
				string requiredItems = string.Join<string>(':', from i in this.RequiredItems.Select(delegate(FabricationRecipe.RequiredItem i)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler2.AppendFormatted<uint>(i.UintIdentifier);
					defaultInterpolatedStringHandler2.AppendLiteral(":");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(i.Amount);
					return defaultInterpolatedStringHandler2.ToStringAndClear();
				})
				select string.Join(',', new string[]
				{
					i
				}));
				string requiredSkills = string.Join<string>(':', this.RequiredSkills.Select(delegate(Skill s)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(s.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(":");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(s.Level);
					return defaultInterpolatedStringHandler2.ToStringAndClear();
				}));
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 6);
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.Amount);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(outputId);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.RequiredTime);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(this.RequiresRecipe);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				defaultInterpolatedStringHandler.AppendFormatted(requiredItems);
				defaultInterpolatedStringHandler.AppendLiteral("|");
				defaultInterpolatedStringHandler.AppendFormatted(requiredSkills);
				uint retVal = ToolBoxCore.StringToUInt32Hash(defaultInterpolatedStringHandler.ToStringAndClear(), md5);
				if (retVal == 0U)
				{
					retVal = 1U;
				}
				result = retVal;
			}
			return result;
		}

		// Token: 0x0400201F RID: 8223
		public readonly Identifier TargetItemPrefabIdentifier;

		// Token: 0x04002020 RID: 8224
		private readonly Lazy<LocalizedString> displayName;

		// Token: 0x04002021 RID: 8225
		public readonly ImmutableArray<FabricationRecipe.RequiredItem> RequiredItems;

		// Token: 0x04002022 RID: 8226
		public readonly ImmutableArray<Identifier> SuitableFabricatorIdentifiers;

		// Token: 0x04002023 RID: 8227
		public readonly float RequiredTime;

		// Token: 0x04002024 RID: 8228
		public readonly int RequiredMoney;

		// Token: 0x04002025 RID: 8229
		public readonly bool RequiresRecipe;

		// Token: 0x04002026 RID: 8230
		public readonly bool HideIfNoRecipe;

		// Token: 0x04002027 RID: 8231
		public readonly float OutCondition;

		// Token: 0x04002028 RID: 8232
		public readonly ImmutableArray<Skill> RequiredSkills;

		// Token: 0x04002029 RID: 8233
		public readonly uint RecipeHash;

		// Token: 0x0400202A RID: 8234
		public readonly int Amount;

		// Token: 0x0400202B RID: 8235
		public readonly int? Quality;

		// Token: 0x0400202C RID: 8236
		public readonly bool HideForNonTraitors;

		// Token: 0x0400202D RID: 8237
		public readonly InvSlotType MoveToSlot;

		// Token: 0x0400202E RID: 8238
		public readonly int FabricationLimitMin;

		// Token: 0x0400202F RID: 8239
		public readonly int FabricationLimitMax;

		// Token: 0x02000F89 RID: 3977
		public abstract class RequiredItem
		{
			// Token: 0x17001C2B RID: 7211
			// (get) Token: 0x0600896E RID: 35182
			public abstract IEnumerable<ItemPrefab> ItemPrefabs { get; }

			// Token: 0x17001C2C RID: 7212
			// (get) Token: 0x0600896F RID: 35183
			public abstract uint UintIdentifier { get; }

			// Token: 0x06008970 RID: 35184
			public abstract bool MatchesItem(Item item);

			// Token: 0x17001C2D RID: 7213
			// (get) Token: 0x06008971 RID: 35185
			public abstract ItemPrefab FirstMatchingPrefab { get; }

			// Token: 0x17001C2E RID: 7214
			// (get) Token: 0x06008972 RID: 35186 RVA: 0x003A801A File Offset: 0x003A621A
			public LocalizedString OverrideHeader { get; }

			// Token: 0x17001C2F RID: 7215
			// (get) Token: 0x06008973 RID: 35187 RVA: 0x003A8022 File Offset: 0x003A6222
			public LocalizedString OverrideDescription { get; }

			// Token: 0x06008974 RID: 35188 RVA: 0x003A802A File Offset: 0x003A622A
			public RequiredItem(int amount, float minCondition, float maxCondition, bool useCondition, LocalizedString overrideDescription, LocalizedString overrideHeader, Identifier defaultItem)
			{
				this.Amount = amount;
				this.MinCondition = minCondition;
				this.MaxCondition = maxCondition;
				this.UseCondition = useCondition;
				this.OverrideHeader = overrideHeader;
				this.OverrideDescription = overrideDescription;
				this.DefaultItem = defaultItem;
			}

			// Token: 0x06008975 RID: 35189 RVA: 0x003A8068 File Offset: 0x003A6268
			public bool IsConditionSuitable(float conditionPercentage)
			{
				float normalizedCondition = conditionPercentage / 100f;
				return MathUtils.NearlyEqual(normalizedCondition, this.MinCondition, 0.0001f) || MathUtils.NearlyEqual(normalizedCondition, this.MaxCondition, 0.0001f) || (normalizedCondition >= this.MinCondition && normalizedCondition <= this.MaxCondition);
			}

			// Token: 0x040055EC RID: 21996
			public readonly int Amount;

			// Token: 0x040055ED RID: 21997
			public readonly float MinCondition;

			// Token: 0x040055EE RID: 21998
			public readonly float MaxCondition;

			// Token: 0x040055EF RID: 21999
			public readonly bool UseCondition;

			// Token: 0x040055F0 RID: 22000
			public readonly Identifier DefaultItem;
		}

		// Token: 0x02000F8A RID: 3978
		public class RequiredItemByIdentifier : FabricationRecipe.RequiredItem
		{
			// Token: 0x17001C30 RID: 7216
			// (get) Token: 0x06008976 RID: 35190 RVA: 0x003A80BC File Offset: 0x003A62BC
			public ItemPrefab ItemPrefab
			{
				[return: MaybeNull]
				get
				{
					if (this.prevContentPackagesHash == null || !this.prevContentPackagesHash.Equals(ContentPackageManager.EnabledPackages.MergedHash))
					{
						ItemPrefab prefab;
						this.cachedItemPrefab = (ItemPrefab.Prefabs.TryGet(this.ItemPrefabIdentifier, out prefab) ? prefab : (MapEntityPrefab.FindByName(this.ItemPrefabIdentifier.Value) as ItemPrefab));
						this.prevContentPackagesHash = ContentPackageManager.EnabledPackages.MergedHash;
					}
					return this.cachedItemPrefab;
				}
			}

			// Token: 0x17001C31 RID: 7217
			// (get) Token: 0x06008977 RID: 35191 RVA: 0x003A812C File Offset: 0x003A632C
			public override uint UintIdentifier { get; }

			// Token: 0x17001C32 RID: 7218
			// (get) Token: 0x06008978 RID: 35192 RVA: 0x003A8134 File Offset: 0x003A6334
			public override IEnumerable<ItemPrefab> ItemPrefabs
			{
				get
				{
					if (this.ItemPrefab != null)
					{
						return this.ItemPrefab.ToEnumerable<ItemPrefab>();
					}
					return Enumerable.Empty<ItemPrefab>();
				}
			}

			// Token: 0x17001C33 RID: 7219
			// (get) Token: 0x06008979 RID: 35193 RVA: 0x003A814F File Offset: 0x003A634F
			public override ItemPrefab FirstMatchingPrefab
			{
				get
				{
					return this.ItemPrefab;
				}
			}

			// Token: 0x0600897A RID: 35194 RVA: 0x003A8158 File Offset: 0x003A6358
			public override bool MatchesItem(Item item)
			{
				Identifier? identifier;
				Identifier? identifier2;
				if (item == null)
				{
					identifier = null;
					identifier2 = identifier;
				}
				else
				{
					identifier2 = new Identifier?(item.Prefab.Identifier);
				}
				identifier = identifier2;
				ItemPrefab itemPrefab = this.ItemPrefab;
				Identifier? identifier3 = new Identifier?((itemPrefab != null) ? itemPrefab.Identifier : this.ItemPrefabIdentifier);
				return identifier == identifier3;
			}

			// Token: 0x0600897B RID: 35195 RVA: 0x003A81AC File Offset: 0x003A63AC
			public RequiredItemByIdentifier(Identifier itemPrefab, int amount, float minCondition, float maxCondition, bool useCondition, LocalizedString overrideDescription, LocalizedString overrideHeader) : base(amount, minCondition, maxCondition, useCondition, overrideDescription, overrideHeader, Identifier.Empty)
			{
				this.ItemPrefabIdentifier = itemPrefab;
				using (MD5 md5 = MD5.Create())
				{
					this.UintIdentifier = ToolBoxCore.IdentifierToUint32Hash(itemPrefab, md5);
				}
			}

			// Token: 0x0600897C RID: 35196 RVA: 0x003A8204 File Offset: 0x003A6404
			public override string ToString()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ItemPrefabIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x040055F1 RID: 22001
			public readonly Identifier ItemPrefabIdentifier;

			// Token: 0x040055F2 RID: 22002
			[MaybeNull]
			[AllowNull]
			public ItemPrefab cachedItemPrefab;

			// Token: 0x040055F3 RID: 22003
			[MaybeNull]
			[AllowNull]
			private Md5Hash prevContentPackagesHash;
		}

		// Token: 0x02000F8B RID: 3979
		public class RequiredItemByTag : FabricationRecipe.RequiredItem
		{
			// Token: 0x17001C34 RID: 7220
			// (get) Token: 0x0600897D RID: 35197 RVA: 0x003A8253 File Offset: 0x003A6453
			public override uint UintIdentifier { get; }

			// Token: 0x17001C35 RID: 7221
			// (get) Token: 0x0600897E RID: 35198 RVA: 0x003A825C File Offset: 0x003A645C
			public override IEnumerable<ItemPrefab> ItemPrefabs
			{
				get
				{
					if (this.prevContentPackagesHash == null || !this.prevContentPackagesHash.Equals(ContentPackageManager.EnabledPackages.MergedHash))
					{
						this.cachedPrefabs.Clear();
						this.cachedPrefabs.AddRange(from p in ItemPrefab.Prefabs
						where p.Tags.Contains(this.Tag)
						select p);
						this.prevContentPackagesHash = ContentPackageManager.EnabledPackages.MergedHash;
					}
					return this.cachedPrefabs;
				}
			}

			// Token: 0x17001C36 RID: 7222
			// (get) Token: 0x0600897F RID: 35199 RVA: 0x003A82C6 File Offset: 0x003A64C6
			public override ItemPrefab FirstMatchingPrefab
			{
				get
				{
					return this.ItemPrefabs.FirstOrDefault<ItemPrefab>();
				}
			}

			// Token: 0x06008980 RID: 35200 RVA: 0x003A82D3 File Offset: 0x003A64D3
			public override bool MatchesItem(Item item)
			{
				return item != null && item.HasTag(this.Tag);
			}

			// Token: 0x06008981 RID: 35201 RVA: 0x003A82E8 File Offset: 0x003A64E8
			public RequiredItemByTag(Identifier tag, int amount, float minCondition, float maxCondition, bool useCondition, LocalizedString overrideDescription, LocalizedString overrideHeader, Identifier defaultItem) : base(amount, minCondition, maxCondition, useCondition, overrideDescription, overrideHeader, defaultItem)
			{
				this.Tag = tag;
				using (MD5 md5 = MD5.Create())
				{
					this.UintIdentifier = ToolBoxCore.IdentifierToUint32Hash(("tag:" + tag.ToString()).ToIdentifier(), md5);
				}
			}

			// Token: 0x06008982 RID: 35202 RVA: 0x003A8364 File Offset: 0x003A6564
			public override string ToString()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Tag);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x040055F5 RID: 22005
			public readonly Identifier Tag;

			// Token: 0x040055F7 RID: 22007
			private readonly List<ItemPrefab> cachedPrefabs = new List<ItemPrefab>();

			// Token: 0x040055F8 RID: 22008
			private Md5Hash prevContentPackagesHash;
		}
	}
}

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
	// Token: 0x020001F8 RID: 504
	internal class FabricationRecipe
	{
		// Token: 0x17000A4C RID: 2636
		// (get) Token: 0x060023C8 RID: 9160 RVA: 0x000EEEE0 File Offset: 0x000ED0E0
		public ItemPrefab TargetItem
		{
			get
			{
				return ItemPrefab.Prefabs[this.TargetItemPrefabIdentifier];
			}
		}

		// Token: 0x17000A4D RID: 2637
		// (get) Token: 0x060023C9 RID: 9161 RVA: 0x000EEEF2 File Offset: 0x000ED0F2
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

		// Token: 0x060023CA RID: 9162 RVA: 0x000EEF1C File Offset: 0x000ED11C
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

		// Token: 0x060023CB RID: 9163 RVA: 0x000EF470 File Offset: 0x000ED670
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

		// Token: 0x04001196 RID: 4502
		public readonly Identifier TargetItemPrefabIdentifier;

		// Token: 0x04001197 RID: 4503
		private readonly Lazy<LocalizedString> displayName;

		// Token: 0x04001198 RID: 4504
		public readonly ImmutableArray<FabricationRecipe.RequiredItem> RequiredItems;

		// Token: 0x04001199 RID: 4505
		public readonly ImmutableArray<Identifier> SuitableFabricatorIdentifiers;

		// Token: 0x0400119A RID: 4506
		public readonly float RequiredTime;

		// Token: 0x0400119B RID: 4507
		public readonly int RequiredMoney;

		// Token: 0x0400119C RID: 4508
		public readonly bool RequiresRecipe;

		// Token: 0x0400119D RID: 4509
		public readonly bool HideIfNoRecipe;

		// Token: 0x0400119E RID: 4510
		public readonly float OutCondition;

		// Token: 0x0400119F RID: 4511
		public readonly ImmutableArray<Skill> RequiredSkills;

		// Token: 0x040011A0 RID: 4512
		public readonly uint RecipeHash;

		// Token: 0x040011A1 RID: 4513
		public readonly int Amount;

		// Token: 0x040011A2 RID: 4514
		public readonly int? Quality;

		// Token: 0x040011A3 RID: 4515
		public readonly bool HideForNonTraitors;

		// Token: 0x040011A4 RID: 4516
		public readonly InvSlotType MoveToSlot;

		// Token: 0x040011A5 RID: 4517
		public readonly int FabricationLimitMin;

		// Token: 0x040011A6 RID: 4518
		public readonly int FabricationLimitMax;

		// Token: 0x020009AC RID: 2476
		public abstract class RequiredItem
		{
			// Token: 0x17001563 RID: 5475
			// (get) Token: 0x06005A72 RID: 23154
			public abstract IEnumerable<ItemPrefab> ItemPrefabs { get; }

			// Token: 0x17001564 RID: 5476
			// (get) Token: 0x06005A73 RID: 23155
			public abstract uint UintIdentifier { get; }

			// Token: 0x06005A74 RID: 23156
			public abstract bool MatchesItem(Item item);

			// Token: 0x17001565 RID: 5477
			// (get) Token: 0x06005A75 RID: 23157
			public abstract ItemPrefab FirstMatchingPrefab { get; }

			// Token: 0x17001566 RID: 5478
			// (get) Token: 0x06005A76 RID: 23158 RVA: 0x001FBD5A File Offset: 0x001F9F5A
			public LocalizedString OverrideHeader { get; }

			// Token: 0x17001567 RID: 5479
			// (get) Token: 0x06005A77 RID: 23159 RVA: 0x001FBD62 File Offset: 0x001F9F62
			public LocalizedString OverrideDescription { get; }

			// Token: 0x06005A78 RID: 23160 RVA: 0x001FBD6A File Offset: 0x001F9F6A
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

			// Token: 0x06005A79 RID: 23161 RVA: 0x001FBDA8 File Offset: 0x001F9FA8
			public bool IsConditionSuitable(float conditionPercentage)
			{
				float normalizedCondition = conditionPercentage / 100f;
				return MathUtils.NearlyEqual(normalizedCondition, this.MinCondition, 0.0001f) || MathUtils.NearlyEqual(normalizedCondition, this.MaxCondition, 0.0001f) || (normalizedCondition >= this.MinCondition && normalizedCondition <= this.MaxCondition);
			}

			// Token: 0x0400340A RID: 13322
			public readonly int Amount;

			// Token: 0x0400340B RID: 13323
			public readonly float MinCondition;

			// Token: 0x0400340C RID: 13324
			public readonly float MaxCondition;

			// Token: 0x0400340D RID: 13325
			public readonly bool UseCondition;

			// Token: 0x0400340E RID: 13326
			public readonly Identifier DefaultItem;
		}

		// Token: 0x020009AD RID: 2477
		public class RequiredItemByIdentifier : FabricationRecipe.RequiredItem
		{
			// Token: 0x17001568 RID: 5480
			// (get) Token: 0x06005A7A RID: 23162 RVA: 0x001FBDFC File Offset: 0x001F9FFC
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

			// Token: 0x17001569 RID: 5481
			// (get) Token: 0x06005A7B RID: 23163 RVA: 0x001FBE6C File Offset: 0x001FA06C
			public override uint UintIdentifier { get; }

			// Token: 0x1700156A RID: 5482
			// (get) Token: 0x06005A7C RID: 23164 RVA: 0x001FBE74 File Offset: 0x001FA074
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

			// Token: 0x1700156B RID: 5483
			// (get) Token: 0x06005A7D RID: 23165 RVA: 0x001FBE8F File Offset: 0x001FA08F
			public override ItemPrefab FirstMatchingPrefab
			{
				get
				{
					return this.ItemPrefab;
				}
			}

			// Token: 0x06005A7E RID: 23166 RVA: 0x001FBE98 File Offset: 0x001FA098
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

			// Token: 0x06005A7F RID: 23167 RVA: 0x001FBEEC File Offset: 0x001FA0EC
			public RequiredItemByIdentifier(Identifier itemPrefab, int amount, float minCondition, float maxCondition, bool useCondition, LocalizedString overrideDescription, LocalizedString overrideHeader) : base(amount, minCondition, maxCondition, useCondition, overrideDescription, overrideHeader, Identifier.Empty)
			{
				this.ItemPrefabIdentifier = itemPrefab;
				using (MD5 md5 = MD5.Create())
				{
					this.UintIdentifier = ToolBoxCore.IdentifierToUint32Hash(itemPrefab, md5);
				}
			}

			// Token: 0x06005A80 RID: 23168 RVA: 0x001FBF44 File Offset: 0x001FA144
			public override string ToString()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ItemPrefabIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x0400340F RID: 13327
			public readonly Identifier ItemPrefabIdentifier;

			// Token: 0x04003410 RID: 13328
			[MaybeNull]
			[AllowNull]
			public ItemPrefab cachedItemPrefab;

			// Token: 0x04003411 RID: 13329
			[MaybeNull]
			[AllowNull]
			private Md5Hash prevContentPackagesHash;
		}

		// Token: 0x020009AE RID: 2478
		public class RequiredItemByTag : FabricationRecipe.RequiredItem
		{
			// Token: 0x1700156C RID: 5484
			// (get) Token: 0x06005A81 RID: 23169 RVA: 0x001FBF93 File Offset: 0x001FA193
			public override uint UintIdentifier { get; }

			// Token: 0x1700156D RID: 5485
			// (get) Token: 0x06005A82 RID: 23170 RVA: 0x001FBF9C File Offset: 0x001FA19C
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

			// Token: 0x1700156E RID: 5486
			// (get) Token: 0x06005A83 RID: 23171 RVA: 0x001FC006 File Offset: 0x001FA206
			public override ItemPrefab FirstMatchingPrefab
			{
				get
				{
					return this.ItemPrefabs.FirstOrDefault<ItemPrefab>();
				}
			}

			// Token: 0x06005A84 RID: 23172 RVA: 0x001FC013 File Offset: 0x001FA213
			public override bool MatchesItem(Item item)
			{
				return item != null && item.HasTag(this.Tag);
			}

			// Token: 0x06005A85 RID: 23173 RVA: 0x001FC028 File Offset: 0x001FA228
			public RequiredItemByTag(Identifier tag, int amount, float minCondition, float maxCondition, bool useCondition, LocalizedString overrideDescription, LocalizedString overrideHeader, Identifier defaultItem) : base(amount, minCondition, maxCondition, useCondition, overrideDescription, overrideHeader, defaultItem)
			{
				this.Tag = tag;
				using (MD5 md5 = MD5.Create())
				{
					this.UintIdentifier = ToolBoxCore.IdentifierToUint32Hash(("tag:" + tag.ToString()).ToIdentifier(), md5);
				}
			}

			// Token: 0x06005A86 RID: 23174 RVA: 0x001FC0A4 File Offset: 0x001FA2A4
			public override string ToString()
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted(base.ToString());
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Tag);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}

			// Token: 0x04003413 RID: 13331
			public readonly Identifier Tag;

			// Token: 0x04003415 RID: 13333
			private readonly List<ItemPrefab> cachedPrefabs = new List<ItemPrefab>();

			// Token: 0x04003416 RID: 13334
			private Md5Hash prevContentPackagesHash;
		}
	}
}

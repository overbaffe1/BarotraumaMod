using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002DE RID: 734
	[NullableContext(1)]
	[Nullable(0)]
	internal class ContainerTagPrefab : Prefab
	{
		// Token: 0x06003D5A RID: 15706 RVA: 0x0022DC7C File Offset: 0x0022BE7C
		public bool IsRecommendedForSub(Submarine sub)
		{
			SubmarineInfo info = sub.Info;
			SubmarineType type = (info != null) ? info.Type : SubmarineType.Player;
			Identifier category = ContainerTagPrefab.categoryToSubmarineType.GetValueOrDefault(this.Category, this.Category);
			Identifier identifier = type.ToIdentifier<SubmarineType>();
			return identifier == category;
		}

		// Token: 0x06003D5B RID: 15707 RVA: 0x0022DCC4 File Offset: 0x0022BEC4
		public ContainerTagPrefab(ContentXElement element, ContainerTagFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.Category = element.GetAttributeIdentifier("category", "");
			string nameOverride = element.GetAttributeString("nameidentifier", string.Empty);
			LocalizedString name;
			if (!string.IsNullOrEmpty(nameOverride))
			{
				name = TextManager.Get("tagname." + nameOverride).Fallback(this.Identifier.Value, true);
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler.AppendLiteral("tagname.");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				name = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(this.Identifier.Value, true);
			}
			this.Name = name;
			LocalizedString description;
			if (!string.IsNullOrEmpty(nameOverride))
			{
				description = TextManager.Get("tagdescription." + nameOverride);
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("tagdescription.");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
				description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			this.Description = description;
			string suffix = element.GetAttributeString("suffix", string.Empty);
			if (!string.IsNullOrEmpty(suffix))
			{
				this.Name = TextManager.GetWithVariable(suffix + ".tagnamesuffix", "[tagname]", this.Name, FormatCapitals.No);
			}
			this.RecommendedAmount = element.GetAttributeInt("recommendedamount", 0);
			this.WarnIfLess = element.GetAttributeBool("warnifless", true);
		}

		// Token: 0x06003D5C RID: 15708 RVA: 0x0022DE38 File Offset: 0x0022C038
		[NullableContext(0)]
		public ImmutableArray<ContainerTagPrefab.ItemAndProbability> GetItemsAndSpawnProbabilities()
		{
			ImmutableArray<ContainerTagPrefab.ItemAndProbability>.Builder items = ImmutableArray.CreateBuilder<ContainerTagPrefab.ItemAndProbability>();
			foreach (ItemPrefab ip in ItemPrefab.Prefabs)
			{
				bool found = false;
				float spawnProbability = 0f;
				float campaignSpawnProbability = 0f;
				foreach (PreferredContainer pc in ip.PreferredContainers)
				{
					if (pc.Primary.Contains(this.Identifier) || pc.Secondary.Contains(this.Identifier))
					{
						found = true;
						spawnProbability = Math.Max(pc.SpawnProbability, spawnProbability);
						if (!pc.NotCampaign)
						{
							campaignSpawnProbability = Math.Max(spawnProbability, campaignSpawnProbability);
						}
						if (!pc.NotCampaign || pc.CampaignOnly)
						{
							campaignSpawnProbability = Math.Max(pc.SpawnProbability, campaignSpawnProbability);
						}
					}
				}
				if (found)
				{
					items.Add(new ContainerTagPrefab.ItemAndProbability(ip, spawnProbability, campaignSpawnProbability));
				}
			}
			return items.ToImmutable();
		}

		// Token: 0x06003D5D RID: 15709 RVA: 0x0022DF4C File Offset: 0x0022C14C
		public static void CheckForContainerTagErrors()
		{
			HashSet<Identifier> allContainerTagsInTheGame = new HashSet<Identifier>();
			HashSet<Identifier> vanillaContainerTags = new HashSet<Identifier>();
			foreach (ItemPrefab prefab in ItemPrefab.Prefabs)
			{
				foreach (Identifier tag in prefab.PreferredContainers.SelectMany((PreferredContainer pc) => pc.Primary.Union(pc.Secondary)))
				{
					allContainerTagsInTheGame.Add(tag);
					if (prefab.ContentPackage == GameMain.VanillaContent && !ContainerTagPrefab.<CheckForContainerTagErrors>g__TagExistsInItemOrCharacterPrefab|11_0(tag))
					{
						vanillaContainerTags.Add(tag);
					}
				}
			}
			foreach (ContainerTagPrefab prefab2 in ContainerTagPrefab.Prefabs)
			{
				if (!allContainerTagsInTheGame.Contains(prefab2.Identifier))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(216, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Container tag \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab2.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" defined in ContainerTagPrefab is not used in any item prefabs, did you misspell it? It's also possible mods override container tags in a way that causes some of the pre-defined tags to become unused.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), prefab2.ContentPackage);
				}
			}
			using (HashSet<Identifier>.Enumerator enumerator4 = vanillaContainerTags.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					Identifier vanillaTag = enumerator4.Current;
					if (vanillaTag.StartsWith(Tags.RespawnContainer))
					{
						bool isMissionSpecificRespawnContainer = false;
						foreach (Identifier missionType in (from p in MissionPrefab.Prefabs
						select p.Type).Distinct<Identifier>())
						{
							if (vanillaTag == Tags.RespawnContainer.ToString() + "_" + missionType.ToString())
							{
								isMissionSpecificRespawnContainer = true;
								break;
							}
						}
						if (isMissionSpecificRespawnContainer)
						{
							continue;
						}
					}
					if (ContainerTagPrefab.Prefabs.None((ContainerTagPrefab p) => p.Identifier == vanillaTag))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(89, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Container tag \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(vanillaTag);
						defaultInterpolatedStringHandler2.AppendLiteral("\" is used in vanilla item prefabs but not defined in a ContainerTagPrefab.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
					}
				}
			}
		}

		// Token: 0x06003D5E RID: 15710 RVA: 0x0022E254 File Offset: 0x0022C454
		public override void Dispose()
		{
		}

		// Token: 0x06003D60 RID: 15712 RVA: 0x0022E2D4 File Offset: 0x0022C4D4
		[CompilerGenerated]
		internal static bool <CheckForContainerTagErrors>g__TagExistsInItemOrCharacterPrefab|11_0(Identifier tag)
		{
			CharacterPrefab characterPrefab;
			if (CharacterPrefab.Prefabs.TryGet(tag, out characterPrefab))
			{
				return true;
			}
			foreach (ItemPrefab prefab in ItemPrefab.Prefabs)
			{
				if (prefab.Tags.Contains(tag) || prefab.Identifier == tag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04002004 RID: 8196
		public static readonly PrefabCollection<ContainerTagPrefab> Prefabs = new PrefabCollection<ContainerTagPrefab>();

		// Token: 0x04002005 RID: 8197
		public readonly LocalizedString Name;

		// Token: 0x04002006 RID: 8198
		public readonly LocalizedString Description;

		// Token: 0x04002007 RID: 8199
		public readonly Identifier Category;

		// Token: 0x04002008 RID: 8200
		public readonly int RecommendedAmount;

		// Token: 0x04002009 RID: 8201
		public readonly bool WarnIfLess;

		// Token: 0x0400200A RID: 8202
		private static readonly Dictionary<Identifier, Identifier> categoryToSubmarineType = new Dictionary<Identifier, Identifier>
		{
			{
				new Identifier("Submarine"),
				SubmarineType.Player.ToIdentifier<SubmarineType>()
			},
			{
				new Identifier("AbandonedOutpost"),
				SubmarineType.OutpostModule.ToIdentifier<SubmarineType>()
			},
			{
				new Identifier("Ruin"),
				SubmarineType.OutpostModule.ToIdentifier<SubmarineType>()
			},
			{
				new Identifier("Enemy"),
				SubmarineType.EnemySubmarine.ToIdentifier<SubmarineType>()
			}
		};

		// Token: 0x02000F85 RID: 3973
		[Nullable(0)]
		public readonly struct ItemAndProbability : IEquatable<ContainerTagPrefab.ItemAndProbability>
		{
			// Token: 0x06008957 RID: 35159 RVA: 0x003A7DB4 File Offset: 0x003A5FB4
			public ItemAndProbability(ItemPrefab Prefab, float Probability, float CampaignProbability)
			{
				this.Prefab = Prefab;
				this.Probability = Probability;
				this.CampaignProbability = CampaignProbability;
			}

			// Token: 0x17001C28 RID: 7208
			// (get) Token: 0x06008958 RID: 35160 RVA: 0x003A7DCB File Offset: 0x003A5FCB
			// (set) Token: 0x06008959 RID: 35161 RVA: 0x003A7DD3 File Offset: 0x003A5FD3
			public ItemPrefab Prefab { get; set; }

			// Token: 0x17001C29 RID: 7209
			// (get) Token: 0x0600895A RID: 35162 RVA: 0x003A7DDC File Offset: 0x003A5FDC
			// (set) Token: 0x0600895B RID: 35163 RVA: 0x003A7DE4 File Offset: 0x003A5FE4
			public float Probability { get; set; }

			// Token: 0x17001C2A RID: 7210
			// (get) Token: 0x0600895C RID: 35164 RVA: 0x003A7DED File Offset: 0x003A5FED
			// (set) Token: 0x0600895D RID: 35165 RVA: 0x003A7DF5 File Offset: 0x003A5FF5
			public float CampaignProbability { get; set; }

			// Token: 0x0600895E RID: 35166 RVA: 0x003A7E00 File Offset: 0x003A6000
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ItemAndProbability");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x0600895F RID: 35167 RVA: 0x003A7E4C File Offset: 0x003A604C
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Prefab = ");
				builder.Append(this.Prefab);
				builder.Append(", Probability = ");
				builder.Append(this.Probability.ToString());
				builder.Append(", CampaignProbability = ");
				builder.Append(this.CampaignProbability.ToString());
				return true;
			}

			// Token: 0x06008960 RID: 35168 RVA: 0x003A7EC1 File Offset: 0x003A60C1
			[CompilerGenerated]
			public static bool operator !=(ContainerTagPrefab.ItemAndProbability left, ContainerTagPrefab.ItemAndProbability right)
			{
				return !(left == right);
			}

			// Token: 0x06008961 RID: 35169 RVA: 0x003A7ECD File Offset: 0x003A60CD
			[CompilerGenerated]
			public static bool operator ==(ContainerTagPrefab.ItemAndProbability left, ContainerTagPrefab.ItemAndProbability right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008962 RID: 35170 RVA: 0x003A7ED7 File Offset: 0x003A60D7
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ItemPrefab>.Default.GetHashCode(this.<Prefab>k__BackingField) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Probability>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<CampaignProbability>k__BackingField);
			}

			// Token: 0x06008963 RID: 35171 RVA: 0x003A7F17 File Offset: 0x003A6117
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ContainerTagPrefab.ItemAndProbability && this.Equals((ContainerTagPrefab.ItemAndProbability)obj);
			}

			// Token: 0x06008964 RID: 35172 RVA: 0x003A7F30 File Offset: 0x003A6130
			[CompilerGenerated]
			public bool Equals(ContainerTagPrefab.ItemAndProbability other)
			{
				return EqualityComparer<ItemPrefab>.Default.Equals(this.<Prefab>k__BackingField, other.<Prefab>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Probability>k__BackingField, other.<Probability>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<CampaignProbability>k__BackingField, other.<CampaignProbability>k__BackingField);
			}

			// Token: 0x06008965 RID: 35173 RVA: 0x003A7F85 File Offset: 0x003A6185
			[CompilerGenerated]
			public void Deconstruct(out ItemPrefab Prefab, out float Probability, out float CampaignProbability)
			{
				Prefab = this.Prefab;
				Probability = this.Probability;
				CampaignProbability = this.CampaignProbability;
			}
		}
	}
}

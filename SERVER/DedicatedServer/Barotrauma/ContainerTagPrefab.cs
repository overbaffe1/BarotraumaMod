using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020001F4 RID: 500
	[NullableContext(1)]
	[Nullable(0)]
	internal class ContainerTagPrefab : Prefab
	{
		// Token: 0x060023B4 RID: 9140 RVA: 0x000EE518 File Offset: 0x000EC718
		public bool IsRecommendedForSub(Submarine sub)
		{
			SubmarineInfo info = sub.Info;
			SubmarineType type = (info != null) ? info.Type : SubmarineType.Player;
			Identifier category = ContainerTagPrefab.categoryToSubmarineType.GetValueOrDefault(this.Category, this.Category);
			Identifier identifier = type.ToIdentifier<SubmarineType>();
			return identifier == category;
		}

		// Token: 0x060023B5 RID: 9141 RVA: 0x000EE560 File Offset: 0x000EC760
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

		// Token: 0x060023B6 RID: 9142 RVA: 0x000EE6D4 File Offset: 0x000EC8D4
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

		// Token: 0x060023B7 RID: 9143 RVA: 0x000EE7E8 File Offset: 0x000EC9E8
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

		// Token: 0x060023B8 RID: 9144 RVA: 0x000EEAF0 File Offset: 0x000ECCF0
		public override void Dispose()
		{
		}

		// Token: 0x060023BA RID: 9146 RVA: 0x000EEB70 File Offset: 0x000ECD70
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

		// Token: 0x0400117B RID: 4475
		public static readonly PrefabCollection<ContainerTagPrefab> Prefabs = new PrefabCollection<ContainerTagPrefab>();

		// Token: 0x0400117C RID: 4476
		public readonly LocalizedString Name;

		// Token: 0x0400117D RID: 4477
		public readonly LocalizedString Description;

		// Token: 0x0400117E RID: 4478
		public readonly Identifier Category;

		// Token: 0x0400117F RID: 4479
		public readonly int RecommendedAmount;

		// Token: 0x04001180 RID: 4480
		public readonly bool WarnIfLess;

		// Token: 0x04001181 RID: 4481
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

		// Token: 0x020009A8 RID: 2472
		[Nullable(0)]
		public readonly struct ItemAndProbability : IEquatable<ContainerTagPrefab.ItemAndProbability>
		{
			// Token: 0x06005A5B RID: 23131 RVA: 0x001FBAF4 File Offset: 0x001F9CF4
			public ItemAndProbability(ItemPrefab Prefab, float Probability, float CampaignProbability)
			{
				this.Prefab = Prefab;
				this.Probability = Probability;
				this.CampaignProbability = CampaignProbability;
			}

			// Token: 0x17001560 RID: 5472
			// (get) Token: 0x06005A5C RID: 23132 RVA: 0x001FBB0B File Offset: 0x001F9D0B
			// (set) Token: 0x06005A5D RID: 23133 RVA: 0x001FBB13 File Offset: 0x001F9D13
			public ItemPrefab Prefab { get; set; }

			// Token: 0x17001561 RID: 5473
			// (get) Token: 0x06005A5E RID: 23134 RVA: 0x001FBB1C File Offset: 0x001F9D1C
			// (set) Token: 0x06005A5F RID: 23135 RVA: 0x001FBB24 File Offset: 0x001F9D24
			public float Probability { get; set; }

			// Token: 0x17001562 RID: 5474
			// (get) Token: 0x06005A60 RID: 23136 RVA: 0x001FBB2D File Offset: 0x001F9D2D
			// (set) Token: 0x06005A61 RID: 23137 RVA: 0x001FBB35 File Offset: 0x001F9D35
			public float CampaignProbability { get; set; }

			// Token: 0x06005A62 RID: 23138 RVA: 0x001FBB40 File Offset: 0x001F9D40
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

			// Token: 0x06005A63 RID: 23139 RVA: 0x001FBB8C File Offset: 0x001F9D8C
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

			// Token: 0x06005A64 RID: 23140 RVA: 0x001FBC01 File Offset: 0x001F9E01
			[CompilerGenerated]
			public static bool operator !=(ContainerTagPrefab.ItemAndProbability left, ContainerTagPrefab.ItemAndProbability right)
			{
				return !(left == right);
			}

			// Token: 0x06005A65 RID: 23141 RVA: 0x001FBC0D File Offset: 0x001F9E0D
			[CompilerGenerated]
			public static bool operator ==(ContainerTagPrefab.ItemAndProbability left, ContainerTagPrefab.ItemAndProbability right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005A66 RID: 23142 RVA: 0x001FBC17 File Offset: 0x001F9E17
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<ItemPrefab>.Default.GetHashCode(this.<Prefab>k__BackingField) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<Probability>k__BackingField)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<CampaignProbability>k__BackingField);
			}

			// Token: 0x06005A67 RID: 23143 RVA: 0x001FBC57 File Offset: 0x001F9E57
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ContainerTagPrefab.ItemAndProbability && this.Equals((ContainerTagPrefab.ItemAndProbability)obj);
			}

			// Token: 0x06005A68 RID: 23144 RVA: 0x001FBC70 File Offset: 0x001F9E70
			[CompilerGenerated]
			public bool Equals(ContainerTagPrefab.ItemAndProbability other)
			{
				return EqualityComparer<ItemPrefab>.Default.Equals(this.<Prefab>k__BackingField, other.<Prefab>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<Probability>k__BackingField, other.<Probability>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<CampaignProbability>k__BackingField, other.<CampaignProbability>k__BackingField);
			}

			// Token: 0x06005A69 RID: 23145 RVA: 0x001FBCC5 File Offset: 0x001F9EC5
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

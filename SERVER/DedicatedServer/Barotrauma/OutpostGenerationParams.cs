using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000255 RID: 597
	internal class OutpostGenerationParams : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x17000CAB RID: 3243
		// (get) Token: 0x06002AAD RID: 10925 RVA: 0x00115D4D File Offset: 0x00113F4D
		// (set) Token: 0x06002AAE RID: 10926 RVA: 0x00115D55 File Offset: 0x00113F55
		public virtual string Name { get; private set; }

		// Token: 0x17000CAC RID: 3244
		// (get) Token: 0x06002AAF RID: 10927 RVA: 0x00115D5E File Offset: 0x00113F5E
		public IEnumerable<Identifier> AllowedLocationTypes
		{
			get
			{
				return this.allowedLocationTypes;
			}
		}

		// Token: 0x17000CAD RID: 3245
		// (get) Token: 0x06002AB0 RID: 10928 RVA: 0x00115D66 File Offset: 0x00113F66
		public IEnumerable<Identifier> AllowedGameModeIdentifiers
		{
			get
			{
				return this.allowedGameModeIdentifiers;
			}
		}

		// Token: 0x17000CAE RID: 3246
		// (get) Token: 0x06002AB1 RID: 10929 RVA: 0x00115D6E File Offset: 0x00113F6E
		// (set) Token: 0x06002AB2 RID: 10930 RVA: 0x00115D76 File Offset: 0x00113F76
		[Serialize(-1, IsPropertySaveable.Yes, "Should this type of outpost be forced to the locations at the end of the campaign map? 0 = first end level, 1 = second end level, and so on.", "", false)]
		[Editable(MinValueInt = -1, MaxValueInt = 10)]
		public int ForceToEndLocationIndex { get; set; }

		// Token: 0x17000CAF RID: 3247
		// (get) Token: 0x06002AB3 RID: 10931 RVA: 0x00115D7F File Offset: 0x00113F7F
		// (set) Token: 0x06002AB4 RID: 10932 RVA: 0x00115D87 File Offset: 0x00113F87
		[Serialize(-1, IsPropertySaveable.Yes, "The closer to the current level difficulty this value is, the higher the probability of choosing these generation params are. Defaults to -1, which means we use the current difficulty.", "", false)]
		[Editable(MinValueInt = 1, MaxValueInt = 50)]
		public int PreferredDifficulty { get; set; }

		// Token: 0x17000CB0 RID: 3248
		// (get) Token: 0x06002AB5 RID: 10933 RVA: 0x00115D90 File Offset: 0x00113F90
		// (set) Token: 0x06002AB6 RID: 10934 RVA: 0x00115D98 File Offset: 0x00113F98
		[Serialize(10, IsPropertySaveable.Yes, "Total number of modules in the outpost.", "", false)]
		[Editable(MinValueInt = 1, MaxValueInt = 50)]
		public int TotalModuleCount { get; set; }

		// Token: 0x17000CB1 RID: 3249
		// (get) Token: 0x06002AB7 RID: 10935 RVA: 0x00115DA1 File Offset: 0x00113FA1
		// (set) Token: 0x06002AB8 RID: 10936 RVA: 0x00115DA9 File Offset: 0x00113FA9
		[Serialize(true, IsPropertySaveable.Yes, "Should the generator append generic (module flag \"none\") modules to the outpost to reach the total module count.", "", false)]
		[Editable]
		public bool AppendToReachTotalModuleCount { get; set; }

		// Token: 0x17000CB2 RID: 3250
		// (get) Token: 0x06002AB9 RID: 10937 RVA: 0x00115DB2 File Offset: 0x00113FB2
		// (set) Token: 0x06002ABA RID: 10938 RVA: 0x00115DBA File Offset: 0x00113FBA
		[Serialize(200f, IsPropertySaveable.Yes, "Minimum length of the hallways between modules. If 0, the generator will place the modules directly against each other assuming it can be done without making any modules overlap.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float MinHallwayLength { get; set; }

		// Token: 0x17000CB3 RID: 3251
		// (get) Token: 0x06002ABB RID: 10939 RVA: 0x00115DC3 File Offset: 0x00113FC3
		// (set) Token: 0x06002ABC RID: 10940 RVA: 0x00115DCB File Offset: 0x00113FCB
		[Serialize(true, IsPropertySaveable.Yes, "Should hallways of the minimum hallway length be always generated between modules, even if they could be placed directly against each other with no overlaps?", "", false)]
		[Editable]
		public bool AlwaysGenerateHallways { get; set; }

		// Token: 0x17000CB4 RID: 3252
		// (get) Token: 0x06002ABD RID: 10941 RVA: 0x00115DD4 File Offset: 0x00113FD4
		// (set) Token: 0x06002ABE RID: 10942 RVA: 0x00115DDC File Offset: 0x00113FDC
		[Serialize(false, IsPropertySaveable.Yes, "Should this outpost always be destructible, regardless if damaging outposts is allowed by the server?", "", false)]
		[Editable]
		public bool AlwaysDestructible { get; set; }

		// Token: 0x17000CB5 RID: 3253
		// (get) Token: 0x06002ABF RID: 10943 RVA: 0x00115DE5 File Offset: 0x00113FE5
		// (set) Token: 0x06002AC0 RID: 10944 RVA: 0x00115DED File Offset: 0x00113FED
		[Serialize(false, IsPropertySaveable.Yes, "Should this outpost always be rewireable, regardless if rewiring is allowed by the server?", "", false)]
		[Editable]
		public bool AlwaysRewireable { get; set; }

		// Token: 0x17000CB6 RID: 3254
		// (get) Token: 0x06002AC1 RID: 10945 RVA: 0x00115DF6 File Offset: 0x00113FF6
		// (set) Token: 0x06002AC2 RID: 10946 RVA: 0x00115DFE File Offset: 0x00113FFE
		[Serialize(false, IsPropertySaveable.Yes, "Should stealing from this outpost be always allowed?", "", false)]
		[Editable]
		public bool AllowStealing { get; set; }

		// Token: 0x17000CB7 RID: 3255
		// (get) Token: 0x06002AC3 RID: 10947 RVA: 0x00115E07 File Offset: 0x00114007
		// (set) Token: 0x06002AC4 RID: 10948 RVA: 0x00115E0F File Offset: 0x0011400F
		[Serialize(true, IsPropertySaveable.Yes, "Should the crew spawn inside the outpost (if not, they'll spawn in the submarine).", "", false)]
		[Editable]
		public bool SpawnCrewInsideOutpost { get; set; }

		// Token: 0x17000CB8 RID: 3256
		// (get) Token: 0x06002AC5 RID: 10949 RVA: 0x00115E18 File Offset: 0x00114018
		// (set) Token: 0x06002AC6 RID: 10950 RVA: 0x00115E20 File Offset: 0x00114020
		[Serialize(true, IsPropertySaveable.Yes, "Should doors at the edges of an outpost module that didn't get connected to another module be locked?", "", false)]
		[Editable]
		public bool LockUnusedDoors { get; set; }

		// Token: 0x17000CB9 RID: 3257
		// (get) Token: 0x06002AC7 RID: 10951 RVA: 0x00115E29 File Offset: 0x00114029
		// (set) Token: 0x06002AC8 RID: 10952 RVA: 0x00115E31 File Offset: 0x00114031
		[Serialize(true, IsPropertySaveable.Yes, "Should gaps at the edges of an outpost module that didn't get connected to another module be removed?", "", false)]
		[Editable]
		public bool RemoveUnusedGaps { get; set; }

		// Token: 0x17000CBA RID: 3258
		// (get) Token: 0x06002AC9 RID: 10953 RVA: 0x00115E3A File Offset: 0x0011403A
		// (set) Token: 0x06002ACA RID: 10954 RVA: 0x00115E42 File Offset: 0x00114042
		[Serialize(false, IsPropertySaveable.Yes, "Should the whole outpost render behind submarines? Only set this to true if the submarine is intended to go inside the outpost.", "", false)]
		[Editable]
		public bool DrawBehindSubs { get; set; }

		// Token: 0x17000CBB RID: 3259
		// (get) Token: 0x06002ACB RID: 10955 RVA: 0x00115E4B File Offset: 0x0011404B
		// (set) Token: 0x06002ACC RID: 10956 RVA: 0x00115E53 File Offset: 0x00114053
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum amount of water in the hulls of the outpost.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MinWaterPercentage { get; set; }

		// Token: 0x17000CBC RID: 3260
		// (get) Token: 0x06002ACD RID: 10957 RVA: 0x00115E5C File Offset: 0x0011405C
		// (set) Token: 0x06002ACE RID: 10958 RVA: 0x00115E64 File Offset: 0x00114064
		[Serialize(0f, IsPropertySaveable.Yes, "Maximum amount of water in the hulls of the outpost.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MaxWaterPercentage { get; set; }

		// Token: 0x17000CBD RID: 3261
		// (get) Token: 0x06002ACF RID: 10959 RVA: 0x00115E6D File Offset: 0x0011406D
		// (set) Token: 0x06002AD0 RID: 10960 RVA: 0x00115E75 File Offset: 0x00114075
		public LevelData.LevelType? LevelType { get; set; }

		// Token: 0x17000CBE RID: 3262
		// (get) Token: 0x06002AD1 RID: 10961 RVA: 0x00115E7E File Offset: 0x0011407E
		// (set) Token: 0x06002AD2 RID: 10962 RVA: 0x00115E86 File Offset: 0x00114086
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the outpost generation parameters that should be used if this outpost has become critically irradiated.", "", false)]
		[Editable]
		public string ReplaceInRadiation { get; set; }

		// Token: 0x17000CBF RID: 3263
		// (get) Token: 0x06002AD3 RID: 10963 RVA: 0x00115E8F File Offset: 0x0011408F
		// (set) Token: 0x06002AD4 RID: 10964 RVA: 0x00115E97 File Offset: 0x00114097
		[Serialize(false, IsPropertySaveable.Yes, "By default, sonar only shows the outline of the sub/outpost from the outside. Enable this if you want to see each structure individually.", "", false)]
		[Editable]
		public bool AlwaysShowStructuresOnSonar { get; set; }

		// Token: 0x17000CC0 RID: 3264
		// (get) Token: 0x06002AD5 RID: 10965 RVA: 0x00115EA0 File Offset: 0x001140A0
		// (set) Token: 0x06002AD6 RID: 10966 RVA: 0x00115EA8 File Offset: 0x001140A8
		public ContentPath OutpostFilePath { get; set; }

		// Token: 0x17000CC1 RID: 3265
		// (get) Token: 0x06002AD7 RID: 10967 RVA: 0x00115EB1 File Offset: 0x001140B1
		// (set) Token: 0x06002AD8 RID: 10968 RVA: 0x00115EB9 File Offset: 0x001140B9
		[Serialize("", IsPropertySaveable.Yes, "If set, a fully pre-built outpost with this tag will be used instead of generating the outpost.", "", false)]
		[Editable]
		public Identifier OutpostTag { get; set; }

		// Token: 0x17000CC2 RID: 3266
		// (get) Token: 0x06002AD9 RID: 10969 RVA: 0x00115EC2 File Offset: 0x001140C2
		public IReadOnlyList<OutpostGenerationParams.ModuleCount> ModuleCounts
		{
			get
			{
				return this.moduleCounts;
			}
		}

		// Token: 0x17000CC3 RID: 3267
		// (get) Token: 0x06002ADA RID: 10970 RVA: 0x00115ECA File Offset: 0x001140CA
		// (set) Token: 0x06002ADB RID: 10971 RVA: 0x00115ED2 File Offset: 0x001140D2
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000CC4 RID: 3268
		// (get) Token: 0x06002ADC RID: 10972 RVA: 0x00115EDB File Offset: 0x001140DB
		// (set) Token: 0x06002ADD RID: 10973 RVA: 0x00115EE3 File Offset: 0x001140E3
		private ImmutableHashSet<Identifier> StoreIdentifiers { get; set; }

		// Token: 0x06002ADE RID: 10974 RVA: 0x00115EEC File Offset: 0x001140EC
		public OutpostGenerationParams(ContentXElement element, ContentFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.Name = element.GetAttributeString("name", this.Identifier.Value);
			this.allowedLocationTypes = element.GetAttributeIdentifierArray("allowedlocationtypes", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			this.allowedGameModeIdentifiers = element.GetAttributeIdentifierArray("allowedgamemodes", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element.GetAttribute("leveltype") != null)
			{
				string levelTypeStr = element.GetAttributeString("leveltype", "");
				LevelData.LevelType parsedLevelType;
				if (Enum.TryParse<LevelData.LevelType>(levelTypeStr, out parsedLevelType))
				{
					this.LevelType = new LevelData.LevelType?(parsedLevelType);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in outpost generation parameters \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". \"");
					defaultInterpolatedStringHandler.AppendFormatted(levelTypeStr);
					defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid level type.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
			}
			this.OutpostFilePath = element.GetAttributeContentPath("OutpostFilePath");
			this.OutpostTag = element.GetAttributeIdentifier("OutpostTag", Identifier.Empty);
			List<OutpostGenerationParams.NpcCollection> humanPrefabCollections = new List<OutpostGenerationParams.NpcCollection>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "modulecount"))
				{
					if (a == "npcs")
					{
						OutpostGenerationParams.NpcCollection newCollection = new OutpostGenerationParams.NpcCollection();
						foreach (ContentXElement npcElement in subElement.Elements())
						{
							Identifier from = npcElement.GetAttributeIdentifier("from", Identifier.Empty);
							Identifier faction = npcElement.GetAttributeIdentifier("faction", Identifier.Empty);
							if (from != Identifier.Empty)
							{
								newCollection.Add(from, npcElement.GetAttributeIdentifier("identifier", Identifier.Empty), faction, npcElement.ContentPackage);
							}
							else
							{
								newCollection.Add(new HumanPrefab(npcElement, file, from), faction, npcElement.ContentPackage);
							}
						}
						humanPrefabCollections.Add(newCollection);
					}
				}
				else
				{
					OutpostGenerationParams.ModuleCount newModuleCount = new OutpostGenerationParams.ModuleCount(subElement);
					if (this.moduleCounts.None(null) && newModuleCount.Probability < 1f)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(182, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Potential error in outpost generation parameters \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\".");
						defaultInterpolatedStringHandler2.AppendLiteral(" The first module is set to spawn with a probability of ");
						defaultInterpolatedStringHandler2.AppendFormatted<float>(newModuleCount.Probability);
						defaultInterpolatedStringHandler2.AppendLiteral("%. The first module must always spawn, so the probability will be ignored.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), base.ContentPackage);
						newModuleCount.Probability = 1f;
					}
					else if (newModuleCount.Probability <= 0f)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(182, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Potential error in outpost generation parameters \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\".");
						defaultInterpolatedStringHandler3.AppendLiteral(" Probability of the module ");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(newModuleCount.Identifier);
						defaultInterpolatedStringHandler3.AppendLiteral(" is 0% (the module should never spawn, so there's no reason to include it in the generation parameters.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), base.ContentPackage);
					}
					this.moduleCounts.Add(newModuleCount);
				}
			}
			this.humanPrefabCollections = humanPrefabCollections.ToImmutableArray<OutpostGenerationParams.NpcCollection>();
		}

		// Token: 0x06002ADF RID: 10975 RVA: 0x001162E0 File Offset: 0x001144E0
		public int GetModuleCount(Identifier moduleFlag)
		{
			if (moduleFlag == Identifier.Empty || moduleFlag == "none")
			{
				return int.MaxValue;
			}
			OutpostGenerationParams.ModuleCount moduleCount = this.moduleCounts.FirstOrDefault((OutpostGenerationParams.ModuleCount m) => m.Identifier == moduleFlag);
			if (moduleCount == null)
			{
				return 0;
			}
			return moduleCount.Count;
		}

		// Token: 0x06002AE0 RID: 10976 RVA: 0x00116348 File Offset: 0x00114548
		public void SetModuleCount(Identifier moduleFlag, int count, float? probability = null, float? minDifficulty = null, float? maxDifficulty = null)
		{
			if (moduleFlag == Identifier.Empty || moduleFlag == "none")
			{
				return;
			}
			if (count <= 0)
			{
				this.moduleCounts.RemoveAll((OutpostGenerationParams.ModuleCount m) => m.Identifier == moduleFlag);
				return;
			}
			OutpostGenerationParams.ModuleCount moduleCount = this.moduleCounts.FirstOrDefault((OutpostGenerationParams.ModuleCount m) => m.Identifier == moduleFlag);
			if (moduleCount == null)
			{
				moduleCount = new OutpostGenerationParams.ModuleCount(moduleFlag, count);
				if (moduleCount.Probability <= 0f)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(181, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Potential error in outpost generation parameters \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					defaultInterpolatedStringHandler.AppendLiteral(" Probability of the module ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(moduleCount.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(" is 0 (the module should never spawn, so there's no reason to include it in the generation parameters.");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), base.ContentPackage);
				}
				this.moduleCounts.Add(moduleCount);
			}
			moduleCount.Count = count;
			if (probability != null)
			{
				moduleCount.Probability = probability.Value;
			}
			if (minDifficulty != null)
			{
				moduleCount.MinDifficulty = minDifficulty.Value;
			}
			if (maxDifficulty != null)
			{
				moduleCount.MaxDifficulty = maxDifficulty.Value;
			}
		}

		// Token: 0x06002AE1 RID: 10977 RVA: 0x0011649C File Offset: 0x0011469C
		public void SetAllowedLocationTypes(IEnumerable<Identifier> allowedLocationTypes)
		{
			this.allowedLocationTypes.Clear();
			foreach (Identifier locationType in allowedLocationTypes)
			{
				if (!(locationType == "any"))
				{
					this.allowedLocationTypes.Add(locationType);
				}
			}
		}

		// Token: 0x06002AE2 RID: 10978 RVA: 0x00116504 File Offset: 0x00114704
		public IReadOnlyList<HumanPrefab> GetHumanPrefabs(IEnumerable<FactionPrefab> factions, Submarine sub, Rand.RandSync randSync)
		{
			if (!this.humanPrefabCollections.Any<OutpostGenerationParams.NpcCollection>())
			{
				return Array.Empty<HumanPrefab>();
			}
			OutpostGenerationParams.NpcCollection collection = this.humanPrefabCollections.GetRandom(randSync);
			return (from humanPrefab in collection.GetByFaction(factions)
			where !humanPrefab.RequireSpawnPointTag || WayPoint.WayPointList.Any((WayPoint wp) => wp.Submarine == sub && humanPrefab.GetSpawnPointTags().Any((Identifier tag) => wp.Tags.Contains(tag)))
			select humanPrefab).ToImmutableList<HumanPrefab>();
		}

		// Token: 0x06002AE3 RID: 10979 RVA: 0x00116560 File Offset: 0x00114760
		public bool CanHaveCampaignInteraction(CampaignMode.InteractionType interactionType)
		{
			foreach (OutpostGenerationParams.NpcCollection collection in this.humanPrefabCollections)
			{
				foreach (HumanPrefab prefab in collection)
				{
					if (prefab != null && prefab.CampaignInteractionType == interactionType)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002AE4 RID: 10980 RVA: 0x001165D8 File Offset: 0x001147D8
		public ImmutableHashSet<Identifier> GetStoreIdentifiers()
		{
			if (this.StoreIdentifiers == null)
			{
				HashSet<Identifier> storeIdentifiers = new HashSet<Identifier>();
				foreach (OutpostGenerationParams.NpcCollection collection in this.humanPrefabCollections)
				{
					foreach (HumanPrefab prefab in collection)
					{
						if (prefab != null && prefab.CampaignInteractionType == CampaignMode.InteractionType.Store)
						{
							storeIdentifiers.Add(prefab.Identifier);
						}
					}
				}
				this.StoreIdentifiers = storeIdentifiers.ToImmutableHashSet<Identifier>();
			}
			return this.StoreIdentifiers;
		}

		// Token: 0x06002AE5 RID: 10981 RVA: 0x00116678 File Offset: 0x00114878
		public override void Dispose()
		{
		}

		// Token: 0x040014F6 RID: 5366
		public static readonly PrefabCollection<OutpostGenerationParams> OutpostParams = new PrefabCollection<OutpostGenerationParams>();

		// Token: 0x040014F8 RID: 5368
		private readonly HashSet<Identifier> allowedLocationTypes = new HashSet<Identifier>();

		// Token: 0x040014F9 RID: 5369
		private readonly HashSet<Identifier> allowedGameModeIdentifiers = new HashSet<Identifier>();

		// Token: 0x0400150E RID: 5390
		private readonly List<OutpostGenerationParams.ModuleCount> moduleCounts = new List<OutpostGenerationParams.ModuleCount>();

		// Token: 0x0400150F RID: 5391
		private readonly ImmutableArray<OutpostGenerationParams.NpcCollection> humanPrefabCollections;

		// Token: 0x02000A8E RID: 2702
		public class ModuleCount : ISerializableEntity
		{
			// Token: 0x17001596 RID: 5526
			// (get) Token: 0x06005D78 RID: 23928 RVA: 0x002030A6 File Offset: 0x002012A6
			// (set) Token: 0x06005D79 RID: 23929 RVA: 0x002030AE File Offset: 0x002012AE
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public int Count { get; set; }

			// Token: 0x17001597 RID: 5527
			// (get) Token: 0x06005D7A RID: 23930 RVA: 0x002030B7 File Offset: 0x002012B7
			// (set) Token: 0x06005D7B RID: 23931 RVA: 0x002030BF File Offset: 0x002012BF
			[Serialize(0, IsPropertySaveable.Yes, "Can be used to enforce the modules to be placed in a specific order, starting from the docking module (0 = first, 1 = second, etc).", "", false)]
			[Editable]
			public int Order { get; set; }

			// Token: 0x17001598 RID: 5528
			// (get) Token: 0x06005D7C RID: 23932 RVA: 0x002030C8 File Offset: 0x002012C8
			// (set) Token: 0x06005D7D RID: 23933 RVA: 0x002030D0 File Offset: 0x002012D0
			[Serialize(0f, IsPropertySaveable.Yes, "Minimum difficulty of the current level for the module to appear in the outpost.", "", false)]
			[Editable]
			public float MinDifficulty { get; set; }

			// Token: 0x17001599 RID: 5529
			// (get) Token: 0x06005D7E RID: 23934 RVA: 0x002030D9 File Offset: 0x002012D9
			// (set) Token: 0x06005D7F RID: 23935 RVA: 0x002030E1 File Offset: 0x002012E1
			[Serialize(100f, IsPropertySaveable.Yes, "Maximum difficulty of the current level for the module to appear in the outpost.", "", false)]
			[Editable]
			public float MaxDifficulty { get; set; }

			// Token: 0x1700159A RID: 5530
			// (get) Token: 0x06005D80 RID: 23936 RVA: 0x002030EA File Offset: 0x002012EA
			// (set) Token: 0x06005D81 RID: 23937 RVA: 0x002030F2 File Offset: 0x002012F2
			[Serialize(1f, IsPropertySaveable.Yes, "Probability for this type of module to be included in the outpost.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
			public float Probability { get; set; }

			// Token: 0x1700159B RID: 5531
			// (get) Token: 0x06005D82 RID: 23938 RVA: 0x002030FB File Offset: 0x002012FB
			// (set) Token: 0x06005D83 RID: 23939 RVA: 0x00203103 File Offset: 0x00201303
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public Identifier RequiredFaction { get; set; }

			// Token: 0x1700159C RID: 5532
			// (get) Token: 0x06005D84 RID: 23940 RVA: 0x0020310C File Offset: 0x0020130C
			public string Name
			{
				get
				{
					return this.Identifier.Value;
				}
			}

			// Token: 0x1700159D RID: 5533
			// (get) Token: 0x06005D85 RID: 23941 RVA: 0x00203119 File Offset: 0x00201319
			// (set) Token: 0x06005D86 RID: 23942 RVA: 0x00203121 File Offset: 0x00201321
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x06005D87 RID: 23943 RVA: 0x0020312A File Offset: 0x0020132A
			public ModuleCount(ContentXElement element)
			{
				this.Identifier = element.GetAttributeIdentifier("flag", element.GetAttributeIdentifier("moduletype", ""));
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}

			// Token: 0x06005D88 RID: 23944 RVA: 0x00203165 File Offset: 0x00201365
			public ModuleCount(Identifier id, int count)
			{
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
				this.Identifier = id;
				this.Count = count;
				this.RequiredFaction = Identifier.Empty;
			}

			// Token: 0x04003672 RID: 13938
			public Identifier Identifier;
		}

		// Token: 0x02000A8F RID: 2703
		private class NpcCollection : IReadOnlyList<HumanPrefab>, IEnumerable<HumanPrefab>, IEnumerable, IReadOnlyCollection<HumanPrefab>
		{
			// Token: 0x06005D89 RID: 23945 RVA: 0x00203193 File Offset: 0x00201393
			public void Add(HumanPrefab humanPrefab, Identifier factionIdentifier, ContentPackage contentPackage)
			{
				this.entries.Add(new OutpostGenerationParams.NpcCollection.Entry(humanPrefab, factionIdentifier, contentPackage));
			}

			// Token: 0x06005D8A RID: 23946 RVA: 0x002031A8 File Offset: 0x002013A8
			public void Add(Identifier setIdentifier, Identifier npcIdentifier, Identifier factionIdentifier, ContentPackage contentPackage)
			{
				this.entries.Add(new OutpostGenerationParams.NpcCollection.Entry(setIdentifier, npcIdentifier, factionIdentifier, contentPackage));
			}

			// Token: 0x06005D8B RID: 23947 RVA: 0x002031BF File Offset: 0x002013BF
			public IEnumerator<HumanPrefab> GetEnumerator()
			{
				OutpostGenerationParams.NpcCollection.<GetEnumerator>d__4 <GetEnumerator>d__ = new OutpostGenerationParams.NpcCollection.<GetEnumerator>d__4(0);
				<GetEnumerator>d__.<>4__this = this;
				return <GetEnumerator>d__;
			}

			// Token: 0x06005D8C RID: 23948 RVA: 0x002031CE File Offset: 0x002013CE
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x06005D8D RID: 23949 RVA: 0x002031D6 File Offset: 0x002013D6
			public IEnumerable<HumanPrefab> GetByFaction(IEnumerable<FactionPrefab> factions)
			{
				OutpostGenerationParams.NpcCollection.<GetByFaction>d__6 <GetByFaction>d__ = new OutpostGenerationParams.NpcCollection.<GetByFaction>d__6(-2);
				<GetByFaction>d__.<>4__this = this;
				<GetByFaction>d__.<>3__factions = factions;
				return <GetByFaction>d__;
			}

			// Token: 0x1700159E RID: 5534
			// (get) Token: 0x06005D8E RID: 23950 RVA: 0x002031ED File Offset: 0x002013ED
			public int Count
			{
				get
				{
					return this.entries.Count;
				}
			}

			// Token: 0x1700159F RID: 5535
			public HumanPrefab this[int index]
			{
				get
				{
					return this.entries[index].HumanPrefab;
				}
			}

			// Token: 0x0400367A RID: 13946
			private readonly List<OutpostGenerationParams.NpcCollection.Entry> entries = new List<OutpostGenerationParams.NpcCollection.Entry>();

			// Token: 0x02000EB2 RID: 3762
			private class Entry
			{
				// Token: 0x06006AC9 RID: 27337 RVA: 0x00227218 File Offset: 0x00225418
				public Entry(HumanPrefab humanPrefab, Identifier factionIdentifier, ContentPackage contentPackage)
				{
					this.humanPrefab = humanPrefab;
					this.FactionIdentifier = factionIdentifier;
					this.ContentPackage = contentPackage;
				}

				// Token: 0x06006ACA RID: 27338 RVA: 0x00227258 File Offset: 0x00225458
				public Entry(Identifier setIdentifier, Identifier npcIdentifier, Identifier factionIdentifier, ContentPackage contentPackage)
				{
					this.SetIdentifier = setIdentifier;
					this.NpcIdentifier = npcIdentifier;
					this.FactionIdentifier = factionIdentifier;
					this.ContentPackage = contentPackage;
				}

				// Token: 0x170016B7 RID: 5815
				// (get) Token: 0x06006ACB RID: 27339 RVA: 0x002272A9 File Offset: 0x002254A9
				public HumanPrefab HumanPrefab
				{
					get
					{
						return this.humanPrefab ?? NPCSet.Get(this.SetIdentifier, this.NpcIdentifier, true, this.ContentPackage);
					}
				}

				// Token: 0x04004320 RID: 17184
				private readonly HumanPrefab humanPrefab;

				// Token: 0x04004321 RID: 17185
				public readonly Identifier SetIdentifier = Identifier.Empty;

				// Token: 0x04004322 RID: 17186
				public readonly Identifier NpcIdentifier = Identifier.Empty;

				// Token: 0x04004323 RID: 17187
				public readonly Identifier FactionIdentifier = Identifier.Empty;

				// Token: 0x04004324 RID: 17188
				public readonly ContentPackage ContentPackage;
			}
		}
	}
}

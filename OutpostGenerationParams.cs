using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200032C RID: 812
	internal class OutpostGenerationParams : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x1700112E RID: 4398
		// (get) Token: 0x060040AF RID: 16559 RVA: 0x0023E0A5 File Offset: 0x0023C2A5
		// (set) Token: 0x060040B0 RID: 16560 RVA: 0x0023E0AD File Offset: 0x0023C2AD
		public virtual string Name { get; private set; }

		// Token: 0x1700112F RID: 4399
		// (get) Token: 0x060040B1 RID: 16561 RVA: 0x0023E0B6 File Offset: 0x0023C2B6
		public IEnumerable<Identifier> AllowedLocationTypes
		{
			get
			{
				return this.allowedLocationTypes;
			}
		}

		// Token: 0x17001130 RID: 4400
		// (get) Token: 0x060040B2 RID: 16562 RVA: 0x0023E0BE File Offset: 0x0023C2BE
		public IEnumerable<Identifier> AllowedGameModeIdentifiers
		{
			get
			{
				return this.allowedGameModeIdentifiers;
			}
		}

		// Token: 0x17001131 RID: 4401
		// (get) Token: 0x060040B3 RID: 16563 RVA: 0x0023E0C6 File Offset: 0x0023C2C6
		// (set) Token: 0x060040B4 RID: 16564 RVA: 0x0023E0CE File Offset: 0x0023C2CE
		[Serialize(-1, IsPropertySaveable.Yes, "Should this type of outpost be forced to the locations at the end of the campaign map? 0 = first end level, 1 = second end level, and so on.", "", false)]
		[Editable(MinValueInt = -1, MaxValueInt = 10)]
		public int ForceToEndLocationIndex { get; set; }

		// Token: 0x17001132 RID: 4402
		// (get) Token: 0x060040B5 RID: 16565 RVA: 0x0023E0D7 File Offset: 0x0023C2D7
		// (set) Token: 0x060040B6 RID: 16566 RVA: 0x0023E0DF File Offset: 0x0023C2DF
		[Serialize(-1, IsPropertySaveable.Yes, "The closer to the current level difficulty this value is, the higher the probability of choosing these generation params are. Defaults to -1, which means we use the current difficulty.", "", false)]
		[Editable(MinValueInt = 1, MaxValueInt = 50)]
		public int PreferredDifficulty { get; set; }

		// Token: 0x17001133 RID: 4403
		// (get) Token: 0x060040B7 RID: 16567 RVA: 0x0023E0E8 File Offset: 0x0023C2E8
		// (set) Token: 0x060040B8 RID: 16568 RVA: 0x0023E0F0 File Offset: 0x0023C2F0
		[Serialize(10, IsPropertySaveable.Yes, "Total number of modules in the outpost.", "", false)]
		[Editable(MinValueInt = 1, MaxValueInt = 50)]
		public int TotalModuleCount { get; set; }

		// Token: 0x17001134 RID: 4404
		// (get) Token: 0x060040B9 RID: 16569 RVA: 0x0023E0F9 File Offset: 0x0023C2F9
		// (set) Token: 0x060040BA RID: 16570 RVA: 0x0023E101 File Offset: 0x0023C301
		[Serialize(true, IsPropertySaveable.Yes, "Should the generator append generic (module flag \"none\") modules to the outpost to reach the total module count.", "", false)]
		[Editable]
		public bool AppendToReachTotalModuleCount { get; set; }

		// Token: 0x17001135 RID: 4405
		// (get) Token: 0x060040BB RID: 16571 RVA: 0x0023E10A File Offset: 0x0023C30A
		// (set) Token: 0x060040BC RID: 16572 RVA: 0x0023E112 File Offset: 0x0023C312
		[Serialize(200f, IsPropertySaveable.Yes, "Minimum length of the hallways between modules. If 0, the generator will place the modules directly against each other assuming it can be done without making any modules overlap.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float MinHallwayLength { get; set; }

		// Token: 0x17001136 RID: 4406
		// (get) Token: 0x060040BD RID: 16573 RVA: 0x0023E11B File Offset: 0x0023C31B
		// (set) Token: 0x060040BE RID: 16574 RVA: 0x0023E123 File Offset: 0x0023C323
		[Serialize(true, IsPropertySaveable.Yes, "Should hallways of the minimum hallway length be always generated between modules, even if they could be placed directly against each other with no overlaps?", "", false)]
		[Editable]
		public bool AlwaysGenerateHallways { get; set; }

		// Token: 0x17001137 RID: 4407
		// (get) Token: 0x060040BF RID: 16575 RVA: 0x0023E12C File Offset: 0x0023C32C
		// (set) Token: 0x060040C0 RID: 16576 RVA: 0x0023E134 File Offset: 0x0023C334
		[Serialize(false, IsPropertySaveable.Yes, "Should this outpost always be destructible, regardless if damaging outposts is allowed by the server?", "", false)]
		[Editable]
		public bool AlwaysDestructible { get; set; }

		// Token: 0x17001138 RID: 4408
		// (get) Token: 0x060040C1 RID: 16577 RVA: 0x0023E13D File Offset: 0x0023C33D
		// (set) Token: 0x060040C2 RID: 16578 RVA: 0x0023E145 File Offset: 0x0023C345
		[Serialize(false, IsPropertySaveable.Yes, "Should this outpost always be rewireable, regardless if rewiring is allowed by the server?", "", false)]
		[Editable]
		public bool AlwaysRewireable { get; set; }

		// Token: 0x17001139 RID: 4409
		// (get) Token: 0x060040C3 RID: 16579 RVA: 0x0023E14E File Offset: 0x0023C34E
		// (set) Token: 0x060040C4 RID: 16580 RVA: 0x0023E156 File Offset: 0x0023C356
		[Serialize(false, IsPropertySaveable.Yes, "Should stealing from this outpost be always allowed?", "", false)]
		[Editable]
		public bool AllowStealing { get; set; }

		// Token: 0x1700113A RID: 4410
		// (get) Token: 0x060040C5 RID: 16581 RVA: 0x0023E15F File Offset: 0x0023C35F
		// (set) Token: 0x060040C6 RID: 16582 RVA: 0x0023E167 File Offset: 0x0023C367
		[Serialize(true, IsPropertySaveable.Yes, "Should the crew spawn inside the outpost (if not, they'll spawn in the submarine).", "", false)]
		[Editable]
		public bool SpawnCrewInsideOutpost { get; set; }

		// Token: 0x1700113B RID: 4411
		// (get) Token: 0x060040C7 RID: 16583 RVA: 0x0023E170 File Offset: 0x0023C370
		// (set) Token: 0x060040C8 RID: 16584 RVA: 0x0023E178 File Offset: 0x0023C378
		[Serialize(true, IsPropertySaveable.Yes, "Should doors at the edges of an outpost module that didn't get connected to another module be locked?", "", false)]
		[Editable]
		public bool LockUnusedDoors { get; set; }

		// Token: 0x1700113C RID: 4412
		// (get) Token: 0x060040C9 RID: 16585 RVA: 0x0023E181 File Offset: 0x0023C381
		// (set) Token: 0x060040CA RID: 16586 RVA: 0x0023E189 File Offset: 0x0023C389
		[Serialize(true, IsPropertySaveable.Yes, "Should gaps at the edges of an outpost module that didn't get connected to another module be removed?", "", false)]
		[Editable]
		public bool RemoveUnusedGaps { get; set; }

		// Token: 0x1700113D RID: 4413
		// (get) Token: 0x060040CB RID: 16587 RVA: 0x0023E192 File Offset: 0x0023C392
		// (set) Token: 0x060040CC RID: 16588 RVA: 0x0023E19A File Offset: 0x0023C39A
		[Serialize(false, IsPropertySaveable.Yes, "Should the whole outpost render behind submarines? Only set this to true if the submarine is intended to go inside the outpost.", "", false)]
		[Editable]
		public bool DrawBehindSubs { get; set; }

		// Token: 0x1700113E RID: 4414
		// (get) Token: 0x060040CD RID: 16589 RVA: 0x0023E1A3 File Offset: 0x0023C3A3
		// (set) Token: 0x060040CE RID: 16590 RVA: 0x0023E1AB File Offset: 0x0023C3AB
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum amount of water in the hulls of the outpost.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MinWaterPercentage { get; set; }

		// Token: 0x1700113F RID: 4415
		// (get) Token: 0x060040CF RID: 16591 RVA: 0x0023E1B4 File Offset: 0x0023C3B4
		// (set) Token: 0x060040D0 RID: 16592 RVA: 0x0023E1BC File Offset: 0x0023C3BC
		[Serialize(0f, IsPropertySaveable.Yes, "Maximum amount of water in the hulls of the outpost.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MaxWaterPercentage { get; set; }

		// Token: 0x17001140 RID: 4416
		// (get) Token: 0x060040D1 RID: 16593 RVA: 0x0023E1C5 File Offset: 0x0023C3C5
		// (set) Token: 0x060040D2 RID: 16594 RVA: 0x0023E1CD File Offset: 0x0023C3CD
		public LevelData.LevelType? LevelType { get; set; }

		// Token: 0x17001141 RID: 4417
		// (get) Token: 0x060040D3 RID: 16595 RVA: 0x0023E1D6 File Offset: 0x0023C3D6
		// (set) Token: 0x060040D4 RID: 16596 RVA: 0x0023E1DE File Offset: 0x0023C3DE
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the outpost generation parameters that should be used if this outpost has become critically irradiated.", "", false)]
		[Editable]
		public string ReplaceInRadiation { get; set; }

		// Token: 0x17001142 RID: 4418
		// (get) Token: 0x060040D5 RID: 16597 RVA: 0x0023E1E7 File Offset: 0x0023C3E7
		// (set) Token: 0x060040D6 RID: 16598 RVA: 0x0023E1EF File Offset: 0x0023C3EF
		[Serialize(false, IsPropertySaveable.Yes, "By default, sonar only shows the outline of the sub/outpost from the outside. Enable this if you want to see each structure individually.", "", false)]
		[Editable]
		public bool AlwaysShowStructuresOnSonar { get; set; }

		// Token: 0x17001143 RID: 4419
		// (get) Token: 0x060040D7 RID: 16599 RVA: 0x0023E1F8 File Offset: 0x0023C3F8
		// (set) Token: 0x060040D8 RID: 16600 RVA: 0x0023E200 File Offset: 0x0023C400
		public ContentPath OutpostFilePath { get; set; }

		// Token: 0x17001144 RID: 4420
		// (get) Token: 0x060040D9 RID: 16601 RVA: 0x0023E209 File Offset: 0x0023C409
		// (set) Token: 0x060040DA RID: 16602 RVA: 0x0023E211 File Offset: 0x0023C411
		[Serialize("", IsPropertySaveable.Yes, "If set, a fully pre-built outpost with this tag will be used instead of generating the outpost.", "", false)]
		[Editable]
		public Identifier OutpostTag { get; set; }

		// Token: 0x17001145 RID: 4421
		// (get) Token: 0x060040DB RID: 16603 RVA: 0x0023E21A File Offset: 0x0023C41A
		public IReadOnlyList<OutpostGenerationParams.ModuleCount> ModuleCounts
		{
			get
			{
				return this.moduleCounts;
			}
		}

		// Token: 0x17001146 RID: 4422
		// (get) Token: 0x060040DC RID: 16604 RVA: 0x0023E222 File Offset: 0x0023C422
		// (set) Token: 0x060040DD RID: 16605 RVA: 0x0023E22A File Offset: 0x0023C42A
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17001147 RID: 4423
		// (get) Token: 0x060040DE RID: 16606 RVA: 0x0023E233 File Offset: 0x0023C433
		// (set) Token: 0x060040DF RID: 16607 RVA: 0x0023E23B File Offset: 0x0023C43B
		private ImmutableHashSet<Identifier> StoreIdentifiers { get; set; }

		// Token: 0x060040E0 RID: 16608 RVA: 0x0023E244 File Offset: 0x0023C444
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

		// Token: 0x060040E1 RID: 16609 RVA: 0x0023E638 File Offset: 0x0023C838
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

		// Token: 0x060040E2 RID: 16610 RVA: 0x0023E6A0 File Offset: 0x0023C8A0
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

		// Token: 0x060040E3 RID: 16611 RVA: 0x0023E7F4 File Offset: 0x0023C9F4
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

		// Token: 0x060040E4 RID: 16612 RVA: 0x0023E85C File Offset: 0x0023CA5C
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

		// Token: 0x060040E5 RID: 16613 RVA: 0x0023E8B8 File Offset: 0x0023CAB8
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

		// Token: 0x060040E6 RID: 16614 RVA: 0x0023E930 File Offset: 0x0023CB30
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

		// Token: 0x060040E7 RID: 16615 RVA: 0x0023E9D0 File Offset: 0x0023CBD0
		public override void Dispose()
		{
		}

		// Token: 0x040021B9 RID: 8633
		public static readonly PrefabCollection<OutpostGenerationParams> OutpostParams = new PrefabCollection<OutpostGenerationParams>();

		// Token: 0x040021BB RID: 8635
		private readonly HashSet<Identifier> allowedLocationTypes = new HashSet<Identifier>();

		// Token: 0x040021BC RID: 8636
		private readonly HashSet<Identifier> allowedGameModeIdentifiers = new HashSet<Identifier>();

		// Token: 0x040021D1 RID: 8657
		private readonly List<OutpostGenerationParams.ModuleCount> moduleCounts = new List<OutpostGenerationParams.ModuleCount>();

		// Token: 0x040021D2 RID: 8658
		private readonly ImmutableArray<OutpostGenerationParams.NpcCollection> humanPrefabCollections;

		// Token: 0x02001025 RID: 4133
		public class ModuleCount : ISerializableEntity
		{
			// Token: 0x17001C55 RID: 7253
			// (get) Token: 0x06008B6C RID: 35692 RVA: 0x003AD172 File Offset: 0x003AB372
			// (set) Token: 0x06008B6D RID: 35693 RVA: 0x003AD17A File Offset: 0x003AB37A
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public int Count { get; set; }

			// Token: 0x17001C56 RID: 7254
			// (get) Token: 0x06008B6E RID: 35694 RVA: 0x003AD183 File Offset: 0x003AB383
			// (set) Token: 0x06008B6F RID: 35695 RVA: 0x003AD18B File Offset: 0x003AB38B
			[Serialize(0, IsPropertySaveable.Yes, "Can be used to enforce the modules to be placed in a specific order, starting from the docking module (0 = first, 1 = second, etc).", "", false)]
			[Editable]
			public int Order { get; set; }

			// Token: 0x17001C57 RID: 7255
			// (get) Token: 0x06008B70 RID: 35696 RVA: 0x003AD194 File Offset: 0x003AB394
			// (set) Token: 0x06008B71 RID: 35697 RVA: 0x003AD19C File Offset: 0x003AB39C
			[Serialize(0f, IsPropertySaveable.Yes, "Minimum difficulty of the current level for the module to appear in the outpost.", "", false)]
			[Editable]
			public float MinDifficulty { get; set; }

			// Token: 0x17001C58 RID: 7256
			// (get) Token: 0x06008B72 RID: 35698 RVA: 0x003AD1A5 File Offset: 0x003AB3A5
			// (set) Token: 0x06008B73 RID: 35699 RVA: 0x003AD1AD File Offset: 0x003AB3AD
			[Serialize(100f, IsPropertySaveable.Yes, "Maximum difficulty of the current level for the module to appear in the outpost.", "", false)]
			[Editable]
			public float MaxDifficulty { get; set; }

			// Token: 0x17001C59 RID: 7257
			// (get) Token: 0x06008B74 RID: 35700 RVA: 0x003AD1B6 File Offset: 0x003AB3B6
			// (set) Token: 0x06008B75 RID: 35701 RVA: 0x003AD1BE File Offset: 0x003AB3BE
			[Serialize(1f, IsPropertySaveable.Yes, "Probability for this type of module to be included in the outpost.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
			public float Probability { get; set; }

			// Token: 0x17001C5A RID: 7258
			// (get) Token: 0x06008B76 RID: 35702 RVA: 0x003AD1C7 File Offset: 0x003AB3C7
			// (set) Token: 0x06008B77 RID: 35703 RVA: 0x003AD1CF File Offset: 0x003AB3CF
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public Identifier RequiredFaction { get; set; }

			// Token: 0x17001C5B RID: 7259
			// (get) Token: 0x06008B78 RID: 35704 RVA: 0x003AD1D8 File Offset: 0x003AB3D8
			public string Name
			{
				get
				{
					return this.Identifier.Value;
				}
			}

			// Token: 0x17001C5C RID: 7260
			// (get) Token: 0x06008B79 RID: 35705 RVA: 0x003AD1E5 File Offset: 0x003AB3E5
			// (set) Token: 0x06008B7A RID: 35706 RVA: 0x003AD1ED File Offset: 0x003AB3ED
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

			// Token: 0x06008B7B RID: 35707 RVA: 0x003AD1F6 File Offset: 0x003AB3F6
			public ModuleCount(ContentXElement element)
			{
				this.Identifier = element.GetAttributeIdentifier("flag", element.GetAttributeIdentifier("moduletype", ""));
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}

			// Token: 0x06008B7C RID: 35708 RVA: 0x003AD231 File Offset: 0x003AB431
			public ModuleCount(Identifier id, int count)
			{
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, null);
				this.Identifier = id;
				this.Count = count;
				this.RequiredFaction = Identifier.Empty;
			}

			// Token: 0x0400577D RID: 22397
			public Identifier Identifier;
		}

		// Token: 0x02001026 RID: 4134
		private class NpcCollection : IReadOnlyList<HumanPrefab>, IEnumerable<HumanPrefab>, IEnumerable, IReadOnlyCollection<HumanPrefab>
		{
			// Token: 0x06008B7D RID: 35709 RVA: 0x003AD25F File Offset: 0x003AB45F
			public void Add(HumanPrefab humanPrefab, Identifier factionIdentifier, ContentPackage contentPackage)
			{
				this.entries.Add(new OutpostGenerationParams.NpcCollection.Entry(humanPrefab, factionIdentifier, contentPackage));
			}

			// Token: 0x06008B7E RID: 35710 RVA: 0x003AD274 File Offset: 0x003AB474
			public void Add(Identifier setIdentifier, Identifier npcIdentifier, Identifier factionIdentifier, ContentPackage contentPackage)
			{
				this.entries.Add(new OutpostGenerationParams.NpcCollection.Entry(setIdentifier, npcIdentifier, factionIdentifier, contentPackage));
			}

			// Token: 0x06008B7F RID: 35711 RVA: 0x003AD28B File Offset: 0x003AB48B
			public IEnumerator<HumanPrefab> GetEnumerator()
			{
				OutpostGenerationParams.NpcCollection.<GetEnumerator>d__4 <GetEnumerator>d__ = new OutpostGenerationParams.NpcCollection.<GetEnumerator>d__4(0);
				<GetEnumerator>d__.<>4__this = this;
				return <GetEnumerator>d__;
			}

			// Token: 0x06008B80 RID: 35712 RVA: 0x003AD29A File Offset: 0x003AB49A
			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			// Token: 0x06008B81 RID: 35713 RVA: 0x003AD2A2 File Offset: 0x003AB4A2
			public IEnumerable<HumanPrefab> GetByFaction(IEnumerable<FactionPrefab> factions)
			{
				OutpostGenerationParams.NpcCollection.<GetByFaction>d__6 <GetByFaction>d__ = new OutpostGenerationParams.NpcCollection.<GetByFaction>d__6(-2);
				<GetByFaction>d__.<>4__this = this;
				<GetByFaction>d__.<>3__factions = factions;
				return <GetByFaction>d__;
			}

			// Token: 0x17001C5D RID: 7261
			// (get) Token: 0x06008B82 RID: 35714 RVA: 0x003AD2B9 File Offset: 0x003AB4B9
			public int Count
			{
				get
				{
					return this.entries.Count;
				}
			}

			// Token: 0x17001C5E RID: 7262
			public HumanPrefab this[int index]
			{
				get
				{
					return this.entries[index].HumanPrefab;
				}
			}

			// Token: 0x04005785 RID: 22405
			private readonly List<OutpostGenerationParams.NpcCollection.Entry> entries = new List<OutpostGenerationParams.NpcCollection.Entry>();

			// Token: 0x02001572 RID: 5490
			private class Entry
			{
				// Token: 0x06009DE5 RID: 40421 RVA: 0x003EDD7C File Offset: 0x003EBF7C
				public Entry(HumanPrefab humanPrefab, Identifier factionIdentifier, ContentPackage contentPackage)
				{
					this.humanPrefab = humanPrefab;
					this.FactionIdentifier = factionIdentifier;
					this.ContentPackage = contentPackage;
				}

				// Token: 0x06009DE6 RID: 40422 RVA: 0x003EDDBC File Offset: 0x003EBFBC
				public Entry(Identifier setIdentifier, Identifier npcIdentifier, Identifier factionIdentifier, ContentPackage contentPackage)
				{
					this.SetIdentifier = setIdentifier;
					this.NpcIdentifier = npcIdentifier;
					this.FactionIdentifier = factionIdentifier;
					this.ContentPackage = contentPackage;
				}

				// Token: 0x17001DA8 RID: 7592
				// (get) Token: 0x06009DE7 RID: 40423 RVA: 0x003EDE0D File Offset: 0x003EC00D
				public HumanPrefab HumanPrefab
				{
					get
					{
						return this.humanPrefab ?? NPCSet.Get(this.SetIdentifier, this.NpcIdentifier, true, this.ContentPackage);
					}
				}

				// Token: 0x04006884 RID: 26756
				private readonly HumanPrefab humanPrefab;

				// Token: 0x04006885 RID: 26757
				public readonly Identifier SetIdentifier = Identifier.Empty;

				// Token: 0x04006886 RID: 26758
				public readonly Identifier NpcIdentifier = Identifier.Empty;

				// Token: 0x04006887 RID: 26759
				public readonly Identifier FactionIdentifier = Identifier.Empty;

				// Token: 0x04006888 RID: 26760
				public readonly ContentPackage ContentPackage;
			}
		}
	}
}

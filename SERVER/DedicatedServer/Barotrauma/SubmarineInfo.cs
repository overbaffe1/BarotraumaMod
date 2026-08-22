using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000261 RID: 609
	internal class SubmarineInfo : IDisposable
	{
		// Token: 0x17000D07 RID: 3335
		// (get) Token: 0x06002B9E RID: 11166 RVA: 0x0011F7FF File Offset: 0x0011D9FF
		// (set) Token: 0x06002B9F RID: 11167 RVA: 0x0011F806 File Offset: 0x0011DA06
		public static HashSet<string> SubmarinePathsWithRemoteStorage { get; set; } = new HashSet<string>();

		// Token: 0x17000D08 RID: 3336
		// (get) Token: 0x06002BA0 RID: 11168 RVA: 0x0011F80E File Offset: 0x0011DA0E
		// (set) Token: 0x06002BA1 RID: 11169 RVA: 0x0011F838 File Offset: 0x0011DA38
		public bool SaveToRemoteStorage
		{
			get
			{
				return this.FilePath != null && SubmarineInfo.SubmarinePathsWithRemoteStorage.Contains(this.FilePath.CleanUpPathCrossPlatform(false, ""));
			}
			set
			{
				if (this.FilePath == null)
				{
					return;
				}
				if (value)
				{
					SubmarineInfo.SubmarinePathsWithRemoteStorage.Add(this.FilePath.CleanUpPathCrossPlatform(false, ""));
					return;
				}
				SubmarineInfo.SubmarinePathsWithRemoteStorage.Remove(this.FilePath.CleanUpPathCrossPlatform(false, ""));
			}
		}

		// Token: 0x17000D09 RID: 3337
		// (get) Token: 0x06002BA2 RID: 11170 RVA: 0x0011F88A File Offset: 0x0011DA8A
		public static IEnumerable<SubmarineInfo> SavedSubmarines
		{
			get
			{
				return SubmarineInfo.savedSubmarines;
			}
		}

		// Token: 0x17000D0A RID: 3338
		// (get) Token: 0x06002BA3 RID: 11171 RVA: 0x0011F891 File Offset: 0x0011DA91
		// (set) Token: 0x06002BA4 RID: 11172 RVA: 0x0011F899 File Offset: 0x0011DA99
		public SubmarineTag Tags { get; private set; }

		// Token: 0x17000D0B RID: 3339
		// (get) Token: 0x06002BA5 RID: 11173 RVA: 0x0011F8A2 File Offset: 0x0011DAA2
		// (set) Token: 0x06002BA6 RID: 11174 RVA: 0x0011F8AA File Offset: 0x0011DAAA
		public int Tier { get; set; }

		// Token: 0x17000D0C RID: 3340
		// (get) Token: 0x06002BA7 RID: 11175 RVA: 0x0011F8B3 File Offset: 0x0011DAB3
		// (set) Token: 0x06002BA8 RID: 11176 RVA: 0x0011F8BB File Offset: 0x0011DABB
		public int EqualityCheckVal { get; private set; }

		// Token: 0x17000D0D RID: 3341
		// (get) Token: 0x06002BA9 RID: 11177 RVA: 0x0011F8C4 File Offset: 0x0011DAC4
		// (set) Token: 0x06002BAA RID: 11178 RVA: 0x0011F8CC File Offset: 0x0011DACC
		public string Name { get; set; }

		// Token: 0x17000D0E RID: 3342
		// (get) Token: 0x06002BAB RID: 11179 RVA: 0x0011F8D5 File Offset: 0x0011DAD5
		// (set) Token: 0x06002BAC RID: 11180 RVA: 0x0011F8DD File Offset: 0x0011DADD
		public LocalizedString DisplayName { get; set; }

		// Token: 0x17000D0F RID: 3343
		// (get) Token: 0x06002BAD RID: 11181 RVA: 0x0011F8E6 File Offset: 0x0011DAE6
		// (set) Token: 0x06002BAE RID: 11182 RVA: 0x0011F8EE File Offset: 0x0011DAEE
		public LocalizedString Description { get; set; }

		// Token: 0x17000D10 RID: 3344
		// (get) Token: 0x06002BAF RID: 11183 RVA: 0x0011F8F7 File Offset: 0x0011DAF7
		// (set) Token: 0x06002BB0 RID: 11184 RVA: 0x0011F8FF File Offset: 0x0011DAFF
		public int Price { get; set; }

		// Token: 0x17000D11 RID: 3345
		// (get) Token: 0x06002BB1 RID: 11185 RVA: 0x0011F908 File Offset: 0x0011DB08
		// (set) Token: 0x06002BB2 RID: 11186 RVA: 0x0011F910 File Offset: 0x0011DB10
		public bool InitialSuppliesSpawned { get; set; }

		// Token: 0x17000D12 RID: 3346
		// (get) Token: 0x06002BB3 RID: 11187 RVA: 0x0011F919 File Offset: 0x0011DB19
		// (set) Token: 0x06002BB4 RID: 11188 RVA: 0x0011F921 File Offset: 0x0011DB21
		public bool NoItems { get; set; }

		// Token: 0x17000D13 RID: 3347
		// (get) Token: 0x06002BB5 RID: 11189 RVA: 0x0011F92A File Offset: 0x0011DB2A
		// (set) Token: 0x06002BB6 RID: 11190 RVA: 0x0011F932 File Offset: 0x0011DB32
		public bool LowFuel { get; set; }

		// Token: 0x17000D14 RID: 3348
		// (get) Token: 0x06002BB7 RID: 11191 RVA: 0x0011F93B File Offset: 0x0011DB3B
		// (set) Token: 0x06002BB8 RID: 11192 RVA: 0x0011F943 File Offset: 0x0011DB43
		public Version GameVersion { get; set; }

		// Token: 0x17000D15 RID: 3349
		// (get) Token: 0x06002BB9 RID: 11193 RVA: 0x0011F94C File Offset: 0x0011DB4C
		// (set) Token: 0x06002BBA RID: 11194 RVA: 0x0011F954 File Offset: 0x0011DB54
		public SubmarineType Type { get; set; }

		// Token: 0x17000D16 RID: 3350
		// (get) Token: 0x06002BBB RID: 11195 RVA: 0x0011F95D File Offset: 0x0011DB5D
		// (set) Token: 0x06002BBC RID: 11196 RVA: 0x0011F965 File Offset: 0x0011DB65
		public bool IsManuallyOutfitted { get; set; }

		// Token: 0x17000D17 RID: 3351
		// (get) Token: 0x06002BBD RID: 11197 RVA: 0x0011F96E File Offset: 0x0011DB6E
		// (set) Token: 0x06002BBE RID: 11198 RVA: 0x0011F976 File Offset: 0x0011DB76
		public OutpostModuleInfo OutpostModuleInfo { get; set; }

		// Token: 0x17000D18 RID: 3352
		// (get) Token: 0x06002BBF RID: 11199 RVA: 0x0011F97F File Offset: 0x0011DB7F
		// (set) Token: 0x06002BC0 RID: 11200 RVA: 0x0011F987 File Offset: 0x0011DB87
		public BeaconStationInfo BeaconStationInfo { get; set; }

		// Token: 0x17000D19 RID: 3353
		// (get) Token: 0x06002BC1 RID: 11201 RVA: 0x0011F990 File Offset: 0x0011DB90
		// (set) Token: 0x06002BC2 RID: 11202 RVA: 0x0011F998 File Offset: 0x0011DB98
		public WreckInfo WreckInfo { get; set; }

		// Token: 0x17000D1A RID: 3354
		// (get) Token: 0x06002BC3 RID: 11203 RVA: 0x0011F9A1 File Offset: 0x0011DBA1
		// (set) Token: 0x06002BC4 RID: 11204 RVA: 0x0011F9A9 File Offset: 0x0011DBA9
		public EnemySubmarineInfo EnemySubmarineInfo { get; set; }

		// Token: 0x17000D1B RID: 3355
		// (get) Token: 0x06002BC5 RID: 11205 RVA: 0x0011F9B2 File Offset: 0x0011DBB2
		public ExtraSubmarineInfo GetExtraSubmarineInfo
		{
			get
			{
				return this.BeaconStationInfo ?? this.WreckInfo;
			}
		}

		// Token: 0x17000D1C RID: 3356
		// (get) Token: 0x06002BC6 RID: 11206 RVA: 0x0011F9C4 File Offset: 0x0011DBC4
		// (set) Token: 0x06002BC7 RID: 11207 RVA: 0x0011F9CC File Offset: 0x0011DBCC
		public ImmutableHashSet<Identifier> OutpostTags { get; set; } = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x17000D1D RID: 3357
		// (get) Token: 0x06002BC8 RID: 11208 RVA: 0x0011F9D5 File Offset: 0x0011DBD5
		// (set) Token: 0x06002BC9 RID: 11209 RVA: 0x0011F9DD File Offset: 0x0011DBDD
		public ImmutableHashSet<Identifier> TriggerOutpostMissionEvents { get; set; } = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x17000D1E RID: 3358
		// (get) Token: 0x06002BCA RID: 11210 RVA: 0x0011F9E8 File Offset: 0x0011DBE8
		public bool IsOutpost
		{
			get
			{
				SubmarineType type = this.Type;
				return type - SubmarineType.Outpost <= 1;
			}
		}

		// Token: 0x17000D1F RID: 3359
		// (get) Token: 0x06002BCB RID: 11211 RVA: 0x0011FA09 File Offset: 0x0011DC09
		public bool IsWreck
		{
			get
			{
				return this.Type == SubmarineType.Wreck;
			}
		}

		// Token: 0x17000D20 RID: 3360
		// (get) Token: 0x06002BCC RID: 11212 RVA: 0x0011FA14 File Offset: 0x0011DC14
		public bool IsBeacon
		{
			get
			{
				return this.Type == SubmarineType.BeaconStation;
			}
		}

		// Token: 0x17000D21 RID: 3361
		// (get) Token: 0x06002BCD RID: 11213 RVA: 0x0011FA1F File Offset: 0x0011DC1F
		public bool IsEnemySubmarine
		{
			get
			{
				return this.Type == SubmarineType.EnemySubmarine;
			}
		}

		// Token: 0x17000D22 RID: 3362
		// (get) Token: 0x06002BCE RID: 11214 RVA: 0x0011FA2A File Offset: 0x0011DC2A
		public bool IsPlayer
		{
			get
			{
				return this.Type == SubmarineType.Player;
			}
		}

		// Token: 0x17000D23 RID: 3363
		// (get) Token: 0x06002BCF RID: 11215 RVA: 0x0011FA35 File Offset: 0x0011DC35
		public bool IsRuin
		{
			get
			{
				return this.Type == SubmarineType.Ruin;
			}
		}

		// Token: 0x17000D24 RID: 3364
		// (get) Token: 0x06002BD0 RID: 11216 RVA: 0x0011FA40 File Offset: 0x0011DC40
		public bool ShouldBeRuin
		{
			get
			{
				SubmarineType type = this.Type;
				bool flag = type == SubmarineType.OutpostModule || type == SubmarineType.Ruin;
				if (flag)
				{
					return this.OutpostModuleInfo.ModuleFlags.Any((Identifier f) => f.StartsWith("ruin"));
				}
				return false;
			}
		}

		// Token: 0x17000D25 RID: 3365
		// (get) Token: 0x06002BD1 RID: 11217 RVA: 0x0011FA96 File Offset: 0x0011DC96
		public bool IsCampaignCompatible
		{
			get
			{
				return this.IsPlayer && !this.HasTag(SubmarineTag.Shuttle) && !this.HasTag(SubmarineTag.HideInMenus) && this.SubmarineClass > SubmarineClass.Undefined;
			}
		}

		// Token: 0x17000D26 RID: 3366
		// (get) Token: 0x06002BD2 RID: 11218 RVA: 0x0011FABD File Offset: 0x0011DCBD
		public bool IsCampaignCompatibleIgnoreClass
		{
			get
			{
				return this.IsPlayer && !this.HasTag(SubmarineTag.Shuttle) && !this.HasTag(SubmarineTag.HideInMenus);
			}
		}

		// Token: 0x17000D27 RID: 3367
		// (get) Token: 0x06002BD3 RID: 11219 RVA: 0x0011FADC File Offset: 0x0011DCDC
		public bool AllowPreviewImage
		{
			get
			{
				return this.Type == SubmarineType.Player;
			}
		}

		// Token: 0x17000D28 RID: 3368
		// (get) Token: 0x06002BD4 RID: 11220 RVA: 0x0011FAE8 File Offset: 0x0011DCE8
		public Md5Hash MD5Hash
		{
			get
			{
				if (this.hash == null)
				{
					if (this.hashTask == null)
					{
						XDocument doc = SubmarineInfo.OpenFile(this.FilePath);
						this.StartHashDocTask(doc);
					}
					this.hashTask.Wait();
					this.hashTask = null;
				}
				return this.hash;
			}
		}

		// Token: 0x17000D29 RID: 3369
		// (get) Token: 0x06002BD5 RID: 11221 RVA: 0x0011FB36 File Offset: 0x0011DD36
		public bool CalculatingHash
		{
			get
			{
				return this.hashTask != null && !this.hashTask.IsCompleted;
			}
		}

		// Token: 0x17000D2A RID: 3370
		// (get) Token: 0x06002BD6 RID: 11222 RVA: 0x0011FB50 File Offset: 0x0011DD50
		// (set) Token: 0x06002BD7 RID: 11223 RVA: 0x0011FB58 File Offset: 0x0011DD58
		public Vector2 Dimensions { get; private set; }

		// Token: 0x17000D2B RID: 3371
		// (get) Token: 0x06002BD8 RID: 11224 RVA: 0x0011FB61 File Offset: 0x0011DD61
		// (set) Token: 0x06002BD9 RID: 11225 RVA: 0x0011FB69 File Offset: 0x0011DD69
		public int CargoCapacity { get; private set; }

		// Token: 0x17000D2C RID: 3372
		// (get) Token: 0x06002BDA RID: 11226 RVA: 0x0011FB72 File Offset: 0x0011DD72
		// (set) Token: 0x06002BDB RID: 11227 RVA: 0x0011FB7A File Offset: 0x0011DD7A
		public string FilePath { get; set; }

		// Token: 0x17000D2D RID: 3373
		// (get) Token: 0x06002BDC RID: 11228 RVA: 0x0011FB83 File Offset: 0x0011DD83
		// (set) Token: 0x06002BDD RID: 11229 RVA: 0x0011FBA1 File Offset: 0x0011DDA1
		public XElement SubmarineElement
		{
			get
			{
				if (this.LazyLoad && this.submarineElement == null)
				{
					this.Reload();
				}
				return this.submarineElement;
			}
			private set
			{
				this.submarineElement = value;
			}
		}

		// Token: 0x06002BDE RID: 11230 RVA: 0x0011FBAA File Offset: 0x0011DDAA
		public override string ToString()
		{
			return "Barotrauma.SubmarineInfo (" + this.Name + ")";
		}

		// Token: 0x17000D2E RID: 3374
		// (get) Token: 0x06002BDF RID: 11231 RVA: 0x0011FBC1 File Offset: 0x0011DDC1
		// (set) Token: 0x06002BE0 RID: 11232 RVA: 0x0011FBC9 File Offset: 0x0011DDC9
		public bool IsFileCorrupted { get; private set; }

		// Token: 0x17000D2F RID: 3375
		// (get) Token: 0x06002BE1 RID: 11233 RVA: 0x0011FBD4 File Offset: 0x0011DDD4
		// (set) Token: 0x06002BE2 RID: 11234 RVA: 0x0011FC24 File Offset: 0x0011DE24
		public bool RequiredContentPackagesInstalled
		{
			get
			{
				if (this.requiredContentPackagesInstalled != null)
				{
					return this.requiredContentPackagesInstalled.Value;
				}
				return this.RequiredContentPackages.All((string reqName) => ContentPackageManager.EnabledPackages.All.Any((ContentPackage contentPackage) => contentPackage.NameMatches(reqName)));
			}
			set
			{
				this.requiredContentPackagesInstalled = new bool?(value);
			}
		}

		// Token: 0x17000D30 RID: 3376
		// (get) Token: 0x06002BE3 RID: 11235 RVA: 0x0011FC32 File Offset: 0x0011DE32
		public bool SubsLeftBehind
		{
			get
			{
				if (this.subsLeftBehind != null)
				{
					return this.subsLeftBehind.Value;
				}
				this.CheckSubsLeftBehind(this.SubmarineElement);
				return this.subsLeftBehind.Value;
			}
		}

		// Token: 0x17000D31 RID: 3377
		// (get) Token: 0x06002BE4 RID: 11236 RVA: 0x0011FC64 File Offset: 0x0011DE64
		// (set) Token: 0x06002BE5 RID: 11237 RVA: 0x0011FC6C File Offset: 0x0011DE6C
		public bool LeftBehindSubDockingPortOccupied { get; private set; }

		// Token: 0x17000D32 RID: 3378
		// (get) Token: 0x06002BE6 RID: 11238 RVA: 0x0011FC75 File Offset: 0x0011DE75
		// (set) Token: 0x06002BE7 RID: 11239 RVA: 0x0011FC7D File Offset: 0x0011DE7D
		public HashSet<Identifier> LayersHiddenByDefault { get; private set; } = new HashSet<Identifier>();

		// Token: 0x06002BE8 RID: 11240 RVA: 0x0011FC88 File Offset: 0x0011DE88
		public SubmarineInfo()
		{
			this.FilePath = null;
			this.DisplayName = TextManager.Get("UnspecifiedSubFileName");
			this.Name = this.DisplayName.Value;
			this.IsFileCorrupted = false;
			this.RequiredContentPackages = new HashSet<string>();
		}

		// Token: 0x06002BE9 RID: 11241 RVA: 0x0011FD30 File Offset: 0x0011DF30
		public SubmarineInfo(string filePath, string hash = "", XElement element = null, bool tryLoad = true, bool lazyLoad = false)
		{
			this.FilePath = filePath;
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
			{
				this.LastModifiedTime = File.GetLastWriteTime(filePath);
			}
			try
			{
				this.DisplayName = Path.GetFileNameWithoutExtension(filePath);
				this.Name = this.DisplayName.Value;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Error loading submarine " + filePath + "!", e, null, false, false);
			}
			if (!string.IsNullOrWhiteSpace(hash))
			{
				this.hash = Md5Hash.StringAsHash(hash);
			}
			this.IsFileCorrupted = false;
			this.RequiredContentPackages = new HashSet<string>();
			if (element == null && tryLoad)
			{
				this.Reload();
			}
			else
			{
				this.SubmarineElement = element;
			}
			this.Name = (this.SubmarineElement.GetAttributeString("name", null) ?? this.Name);
			this.Init();
			if (lazyLoad)
			{
				this.LazyLoad = true;
				this.SubmarineElement = null;
			}
		}

		// Token: 0x06002BEA RID: 11242 RVA: 0x0011FE8C File Offset: 0x0011E08C
		public SubmarineInfo(Submarine sub) : this(sub.Info)
		{
			this.GameVersion = GameMain.Version;
			this.SubmarineElement = new XElement("Submarine");
			sub.SaveToXElement(this.SubmarineElement);
			this.Init();
		}

		// Token: 0x06002BEB RID: 11243 RVA: 0x0011FECC File Offset: 0x0011E0CC
		public SubmarineInfo(SubmarineInfo original)
		{
			this.Name = original.Name;
			this.DisplayName = original.DisplayName;
			this.Description = original.Description;
			this.Price = original.Price;
			this.InitialSuppliesSpawned = original.InitialSuppliesSpawned;
			this.NoItems = original.NoItems;
			this.LowFuel = original.LowFuel;
			this.GameVersion = original.GameVersion;
			this.Type = original.Type;
			this.SubmarineClass = original.SubmarineClass;
			this.hash = ((!string.IsNullOrEmpty(original.FilePath) && File.Exists(original.FilePath)) ? original.MD5Hash : null);
			this.Dimensions = original.Dimensions;
			this.CargoCapacity = original.CargoCapacity;
			this.FilePath = original.FilePath;
			this.RequiredContentPackages = new HashSet<string>(original.RequiredContentPackages);
			this.IsFileCorrupted = original.IsFileCorrupted;
			this.SubmarineElement = original.SubmarineElement;
			this.EqualityCheckVal = original.EqualityCheckVal;
			this.RecommendedCrewExperience = original.RecommendedCrewExperience;
			this.RecommendedCrewSizeMin = original.RecommendedCrewSizeMin;
			this.RecommendedCrewSizeMax = original.RecommendedCrewSizeMax;
			this.Tier = original.Tier;
			this.IsManuallyOutfitted = original.IsManuallyOutfitted;
			this.Tags = original.Tags;
			this.OutpostGenerationParams = original.OutpostGenerationParams;
			this.LayersHiddenByDefault = original.LayersHiddenByDefault;
			this.OutpostTags = original.OutpostTags;
			this.TriggerOutpostMissionEvents = original.TriggerOutpostMissionEvents;
			if (original.OutpostModuleInfo != null)
			{
				this.OutpostModuleInfo = new OutpostModuleInfo(original.OutpostModuleInfo);
				return;
			}
			if (original.BeaconStationInfo != null)
			{
				this.BeaconStationInfo = new BeaconStationInfo(original.BeaconStationInfo);
				return;
			}
			if (original.EnemySubmarineInfo != null)
			{
				this.EnemySubmarineInfo = new EnemySubmarineInfo(original.EnemySubmarineInfo);
				return;
			}
			if (original.WreckInfo != null)
			{
				this.WreckInfo = new WreckInfo(original.WreckInfo);
			}
		}

		// Token: 0x06002BEC RID: 11244 RVA: 0x00120114 File Offset: 0x0011E314
		public void Reload()
		{
			XDocument doc = null;
			int maxLoadRetries = 4;
			for (int i = 0; i <= maxLoadRetries; i++)
			{
				Exception e;
				doc = SubmarineInfo.OpenFile(this.FilePath, out e);
				if ((e != null && !(e is IOException)) || doc != null || i == maxLoadRetries || !File.Exists(this.FilePath))
				{
					break;
				}
				DebugConsole.NewMessage("Opening submarine file \"" + this.FilePath + "\" failed, retrying in 250 ms...", null, false);
				Thread.Sleep(250);
			}
			if (((doc != null) ? doc.Root : null) == null)
			{
				this.IsFileCorrupted = true;
				return;
			}
			if (this.hash == null)
			{
				this.StartHashDocTask(doc);
			}
			this.SubmarineElement = doc.Root;
		}

		// Token: 0x06002BED RID: 11245 RVA: 0x001201C4 File Offset: 0x0011E3C4
		private void Init()
		{
			this.DisplayName = TextManager.Get("Submarine.Name." + this.Name).Fallback(this.Name, true);
			this.Description = TextManager.Get("Submarine.Description." + this.Name).Fallback(this.SubmarineElement.GetAttributeString("description", ""), true);
			this.EqualityCheckVal = this.SubmarineElement.GetAttributeInt("checkval", 0);
			this.Price = this.SubmarineElement.GetAttributeInt("price", 1000);
			this.InitialSuppliesSpawned = this.SubmarineElement.GetAttributeBool("initialsuppliesspawned", false);
			this.NoItems = this.SubmarineElement.GetAttributeBool("noitems", false);
			this.LowFuel = this.SubmarineElement.GetAttributeBool("lowfuel", false);
			this.IsManuallyOutfitted = this.SubmarineElement.GetAttributeBool("ismanuallyoutfitted", false);
			this.GameVersion = new Version(this.SubmarineElement.GetAttributeString("gameversion", "0.0.0.0"));
			SubmarineTag tags;
			if (Enum.TryParse<SubmarineTag>(this.SubmarineElement.GetAttributeString("tags", ""), out tags))
			{
				this.Tags = tags;
			}
			this.Dimensions = this.SubmarineElement.GetAttributeVector2("dimensions", Vector2.Zero);
			this.CargoCapacity = this.SubmarineElement.GetAttributeInt("cargocapacity", -1);
			this.RecommendedCrewSizeMin = this.SubmarineElement.GetAttributeInt("recommendedcrewsizemin", 0);
			this.RecommendedCrewSizeMax = this.SubmarineElement.GetAttributeInt("recommendedcrewsizemax", 0);
			Identifier recommendedCrewExperience = this.SubmarineElement.GetAttributeIdentifier("recommendedcrewexperience", SubmarineInfo.CrewExperienceLevel.Unknown.ToIdentifier<SubmarineInfo.CrewExperienceLevel>());
			foreach (Identifier hiddenLayer in this.SubmarineElement.GetAttributeIdentifierArray("layerhiddenbydefault", Array.Empty<Identifier>(), true))
			{
				this.LayersHiddenByDefault.Add(hiddenLayer);
			}
			if (recommendedCrewExperience == "Beginner")
			{
				this.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceLow;
			}
			else if (recommendedCrewExperience == "Intermediate")
			{
				this.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceMid;
			}
			else if (recommendedCrewExperience == "Experienced")
			{
				this.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceHigh;
			}
			else
			{
				Enum.TryParse<SubmarineInfo.CrewExperienceLevel>(recommendedCrewExperience.Value, true, out this.RecommendedCrewExperience);
			}
			this.Tier = this.SubmarineElement.GetAttributeInt("tier", SubmarineInfo.GetDefaultTier(this.Price));
			this.OutpostTags = this.SubmarineElement.GetAttributeIdentifierImmutableHashSet("OutpostTags", ImmutableHashSet<Identifier>.Empty, true);
			this.TriggerOutpostMissionEvents = this.SubmarineElement.GetAttributeIdentifierImmutableHashSet("TriggerOutpostMissionEvents", ImmutableHashSet<Identifier>.Empty, true);
			if (this.GameVersion < new Version(1, 8, 0, 0) && this.OutpostTags.Contains("PvPOutpost"))
			{
				this.TriggerOutpostMissionEvents = this.TriggerOutpostMissionEvents.Add("deathmatchweapondrop".ToIdentifier());
			}
			XElement xelement = this.SubmarineElement;
			SubmarineType type;
			if (((xelement != null) ? xelement.Attribute("type") : null) != null && Enum.TryParse<SubmarineType>(this.SubmarineElement.GetAttributeString("type", ""), out type))
			{
				this.Type = type;
				if (this.Type == SubmarineType.OutpostModule)
				{
					this.OutpostModuleInfo = new OutpostModuleInfo(this, this.SubmarineElement);
				}
				else if (this.Type == SubmarineType.BeaconStation)
				{
					this.BeaconStationInfo = new BeaconStationInfo(this, this.SubmarineElement);
				}
				else if (this.Type == SubmarineType.EnemySubmarine)
				{
					this.EnemySubmarineInfo = new EnemySubmarineInfo(this, this.SubmarineElement);
				}
				else if (this.Type == SubmarineType.Wreck)
				{
					this.WreckInfo = new WreckInfo(this, this.SubmarineElement);
				}
			}
			if (this.Type == SubmarineType.Player)
			{
				XElement xelement2 = this.SubmarineElement;
				if (((xelement2 != null) ? xelement2.Attribute("class") : null) != null)
				{
					string classStr = this.SubmarineElement.GetAttributeString("class", "Undefined");
					SubmarineClass submarineClass;
					if (classStr == "DeepDiver")
					{
						this.SubmarineClass = SubmarineClass.Scout;
					}
					else if (Enum.TryParse<SubmarineClass>(classStr, out submarineClass))
					{
						this.SubmarineClass = submarineClass;
					}
				}
			}
			else
			{
				this.SubmarineClass = SubmarineClass.Undefined;
			}
			this.RequiredContentPackages.Clear();
			string[] contentPackageNames = this.SubmarineElement.GetAttributeStringArray("requiredcontentpackages", Array.Empty<string>(), true, false);
			foreach (string contentPackageName in contentPackageNames)
			{
				this.RequiredContentPackages.Add(contentPackageName);
			}
		}

		// Token: 0x06002BEE RID: 11246 RVA: 0x00120634 File Offset: 0x0011E834
		public void Dispose()
		{
			if (SubmarineInfo.savedSubmarines.Contains(this))
			{
				SubmarineInfo.savedSubmarines.Remove(this);
			}
		}

		// Token: 0x06002BEF RID: 11247 RVA: 0x0012064F File Offset: 0x0011E84F
		public void UnloadSubmarineElement()
		{
			this.SubmarineElement = null;
		}

		// Token: 0x06002BF0 RID: 11248 RVA: 0x00120658 File Offset: 0x0011E858
		public bool IsVanillaSubmarine()
		{
			if (this.FilePath == null)
			{
				return false;
			}
			ContentPackage vanilla = GameMain.VanillaContent;
			if (vanilla != null)
			{
				IEnumerable<BaseSubFile> vanillaSubs = vanilla.GetFiles<BaseSubFile>();
				string pathToCompare = this.FilePath.CleanUpPath();
				if (vanillaSubs.Any((BaseSubFile sub) => sub.Path == pathToCompare))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002BF1 RID: 11249 RVA: 0x001206B0 File Offset: 0x0011E8B0
		public void StartHashDocTask(XDocument doc)
		{
			if (this.hash != null)
			{
				return;
			}
			if (this.hashTask != null)
			{
				return;
			}
			this.hashTask = new Task(delegate()
			{
				this.hash = Md5Hash.CalculateForString(doc.ToString(), Md5Hash.StringHashOptions.IgnoreWhitespace);
			});
			this.hashTask.Start();
		}

		// Token: 0x06002BF2 RID: 11250 RVA: 0x0012070B File Offset: 0x0011E90B
		public bool HasTag(SubmarineTag tag)
		{
			return this.Tags.HasFlag(tag);
		}

		// Token: 0x06002BF3 RID: 11251 RVA: 0x00120723 File Offset: 0x0011E923
		public void AddTag(SubmarineTag tag)
		{
			if (this.Tags.HasFlag(tag))
			{
				return;
			}
			this.Tags |= tag;
		}

		// Token: 0x06002BF4 RID: 11252 RVA: 0x0012074C File Offset: 0x0011E94C
		public void RemoveTag(SubmarineTag tag)
		{
			if (!this.Tags.HasFlag(tag))
			{
				return;
			}
			this.Tags &= ~tag;
		}

		// Token: 0x06002BF5 RID: 11253 RVA: 0x00120778 File Offset: 0x0011E978
		public void CheckSubsLeftBehind(XElement element = null)
		{
			if (element == null)
			{
				element = this.SubmarineElement;
			}
			this.subsLeftBehind = new bool?(false);
			this.LeftBehindSubDockingPortOccupied = false;
			this.LeftBehindDockingPortIDs.Clear();
			this.BlockedDockingPortIDs.Clear();
			foreach (XElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("linkedsubmarine", StringComparison.OrdinalIgnoreCase) && subElement.Attribute("location") != null)
				{
					this.subsLeftBehind = new bool?(true);
					ushort targetDockingPortID = (ushort)subElement.GetAttributeInt("originallinkedto", 0);
					this.LeftBehindDockingPortIDs.Add(targetDockingPortID);
					XElement targetPortElement = (targetDockingPortID == 0) ? null : element.Elements().FirstOrDefault((XElement e) => e.GetAttributeInt("ID", 0) == (int)targetDockingPortID);
					if (targetPortElement != null && targetPortElement.GetAttributeIntArray("linked", Array.Empty<int>()).Length != 0)
					{
						this.BlockedDockingPortIDs.Add(targetDockingPortID);
						this.LeftBehindSubDockingPortOccupied = true;
					}
				}
			}
		}

		// Token: 0x06002BF6 RID: 11254 RVA: 0x001208B0 File Offset: 0x0011EAB0
		public bool IsCrushDepthDefinedInStructures(out float realWorldCrushDepth)
		{
			if (this.SubmarineElement == null)
			{
				realWorldCrushDepth = 3500f;
				return false;
			}
			bool structureCrushDepthsDefined = false;
			realWorldCrushDepth = float.PositiveInfinity;
			foreach (XElement structureElement in this.SubmarineElement.GetChildElements("structure", StringComparison.OrdinalIgnoreCase))
			{
				XAttribute xattribute = structureElement.Attribute("name");
				string name = ((xattribute != null) ? xattribute.Value : null) ?? "";
				Identifier identifier = structureElement.GetAttributeIdentifier("identifier", "");
				StructurePrefab structurePrefab = Structure.FindPrefab(name, identifier);
				if (structurePrefab != null && structurePrefab.Body)
				{
					if (!structureCrushDepthsDefined && structureElement.Attribute("crushdepth") != null)
					{
						structureCrushDepthsDefined = true;
					}
					float structureCrushDepth = structureElement.GetAttributeFloat("crushdepth", float.PositiveInfinity);
					realWorldCrushDepth = Math.Min(structureCrushDepth, realWorldCrushDepth);
				}
			}
			if (!structureCrushDepthsDefined)
			{
				realWorldCrushDepth = 3500f;
			}
			return structureCrushDepthsDefined;
		}

		// Token: 0x06002BF7 RID: 11255 RVA: 0x001209B0 File Offset: 0x0011EBB0
		public void AddOutpostNPCIdentifierOrTag(Character npc, Identifier idOrTag)
		{
			if (!this.OutpostNPCs.ContainsKey(idOrTag))
			{
				this.OutpostNPCs.Add(idOrTag, new List<Character>());
			}
			this.OutpostNPCs[idOrTag].Add(npc);
		}

		// Token: 0x06002BF8 RID: 11256 RVA: 0x001209E4 File Offset: 0x0011EBE4
		public void SaveAs(string filePath, MemoryStream previewImage = null)
		{
			XName name = this.SubmarineElement.Name;
			object[] array = new object[2];
			array[0] = from a in this.SubmarineElement.Attributes()
			where !string.Equals(a.Name.LocalName, "previewimage", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(a.Name.LocalName, "name", StringComparison.InvariantCultureIgnoreCase)
			select a;
			array[1] = this.SubmarineElement.Elements();
			XElement newElement = new XElement(name, array);
			if (this.Type == SubmarineType.OutpostModule)
			{
				this.OutpostModuleInfo.Save(newElement);
				this.OutpostModuleInfo = new OutpostModuleInfo(this, newElement);
			}
			else if (this.Type == SubmarineType.BeaconStation)
			{
				this.BeaconStationInfo.Save(newElement);
				this.BeaconStationInfo = new BeaconStationInfo(this, newElement);
			}
			else if (this.Type == SubmarineType.EnemySubmarine)
			{
				this.EnemySubmarineInfo.Save(newElement);
				this.EnemySubmarineInfo = new EnemySubmarineInfo(this, newElement);
			}
			else if (this.Type == SubmarineType.Wreck)
			{
				this.WreckInfo.Save(newElement);
				this.WreckInfo = new WreckInfo(this, newElement);
			}
			XDocument doc = new XDocument(new object[]
			{
				newElement
			});
			doc.Root.Add(new XAttribute("name", this.Name));
			if (previewImage != null && this.AllowPreviewImage)
			{
				doc.Root.Add(new XAttribute("previewimage", Convert.ToBase64String(previewImage.ToArray())));
			}
			SaveUtil.CompressStringToFile(filePath, doc.ToString());
		}

		// Token: 0x06002BF9 RID: 11257 RVA: 0x00120B42 File Offset: 0x0011ED42
		public static void AddToSavedSubs(SubmarineInfo subInfo)
		{
			SubmarineInfo.savedSubmarines.Add(subInfo);
		}

		// Token: 0x06002BFA RID: 11258 RVA: 0x00120B50 File Offset: 0x0011ED50
		public static void RemoveSavedSub(string filePath)
		{
			string fullPath = Path.GetFullPath(filePath);
			for (int i = SubmarineInfo.savedSubmarines.Count - 1; i >= 0; i--)
			{
				if (Path.GetFullPath(SubmarineInfo.savedSubmarines[i].FilePath) == fullPath)
				{
					SubmarineInfo.savedSubmarines[i].Dispose();
				}
			}
		}

		// Token: 0x06002BFB RID: 11259 RVA: 0x00120BA8 File Offset: 0x0011EDA8
		public static void RefreshSavedSub(string filePath)
		{
			SubmarineInfo.RemoveSavedSub(filePath);
			if (File.Exists(filePath))
			{
				SubmarineInfo subInfo = new SubmarineInfo(filePath, "", null, true, true);
				if (!subInfo.IsFileCorrupted)
				{
					SubmarineInfo.savedSubmarines.Add(subInfo);
				}
				SubmarineInfo.savedSubmarines = (from s in SubmarineInfo.savedSubmarines
				orderby s.FilePath ?? ""
				select s).ToList<SubmarineInfo>();
			}
		}

		// Token: 0x06002BFC RID: 11260 RVA: 0x00120C18 File Offset: 0x0011EE18
		public static void RefreshSavedSubs()
		{
			IEnumerable<BaseSubFile> contentPackageSubs = ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage c) => c.GetFiles<BaseSubFile>());
			int i = SubmarineInfo.savedSubmarines.Count - 1;
			while (i >= 0)
			{
				if (!File.Exists(SubmarineInfo.savedSubmarines[i].FilePath))
				{
					goto IL_E2;
				}
				bool isDownloadedSub = Path.GetFullPath(Path.GetDirectoryName(SubmarineInfo.savedSubmarines[i].FilePath)) == Path.GetFullPath(SaveUtil.SubmarineDownloadFolder);
				bool isInContentPackage = contentPackageSubs.Any((BaseSubFile f) => f.Path == SubmarineInfo.savedSubmarines[i].FilePath);
				if (!isDownloadedSub && (!(SubmarineInfo.savedSubmarines[i].LastModifiedTime == File.GetLastWriteTime(SubmarineInfo.savedSubmarines[i].FilePath)) || !isInContentPackage))
				{
					goto IL_E2;
				}
				IL_F7:
				int j = i;
				i = j - 1;
				continue;
				IL_E2:
				SubmarineInfo.savedSubmarines[i].Dispose();
				goto IL_F7;
			}
			List<string> filePaths = new List<string>();
			using (IEnumerator<BaseSubFile> enumerator = contentPackageSubs.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BaseSubFile subFile = enumerator.Current;
					if (File.Exists(subFile.Path.Value) && !filePaths.Any((string fp) => fp == subFile.Path))
					{
						filePaths.Add(subFile.Path.Value);
					}
				}
			}
			filePaths.RemoveAll((string p) => SubmarineInfo.savedSubmarines.Any((SubmarineInfo sub) => sub.FilePath == p));
			foreach (string path in filePaths)
			{
				SubmarineInfo subInfo = new SubmarineInfo(path, "", null, true, false);
				if (!subInfo.IsFileCorrupted)
				{
					SubmarineInfo.savedSubmarines.Add(subInfo);
				}
			}
		}

		// Token: 0x06002BFD RID: 11261 RVA: 0x00120E50 File Offset: 0x0011F050
		public static XDocument OpenFile(string file)
		{
			Exception ex;
			return SubmarineInfo.OpenFile(file, out ex);
		}

		// Token: 0x06002BFE RID: 11262 RVA: 0x00120E68 File Offset: 0x0011F068
		public static XDocument OpenFile(string file, out Exception exception)
		{
			XDocument doc = null;
			string extension = "";
			exception = null;
			try
			{
				extension = Path.GetExtension(file);
			}
			catch
			{
				file += ".sub";
			}
			if (string.IsNullOrWhiteSpace(extension))
			{
				extension = ".sub";
				file += ".sub";
			}
			if (extension == ".sub")
			{
				Stream stream;
				try
				{
					stream = SaveUtil.DecompressFileToStream(file);
				}
				catch (FileNotFoundException e)
				{
					exception = e;
					DebugConsole.ThrowError("Loading submarine \"" + file + "\" failed! (File not found) " + Environment.StackTrace.CleanupStackTrace(), e, null, false, false);
					return null;
				}
				catch (Exception e2)
				{
					exception = e2;
					DebugConsole.ThrowError("Loading submarine \"" + file + "\" failed!", e2, null, false, false);
					return null;
				}
				try
				{
					stream.Position = 0L;
					using (XmlReader reader = XMLExtensions.CreateReader(stream, ""))
					{
						doc = XDocument.Load(reader);
					}
					stream.Close();
					stream.Dispose();
					return doc;
				}
				catch (Exception e3)
				{
					exception = e3;
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Loading submarine \"",
						file,
						"\" failed! (",
						e3.Message,
						")"
					}), null, null, false, false);
					return null;
				}
			}
			if (extension == ".xml")
			{
				try
				{
					ToolBox.IsProperFilenameCase(file);
					using (FileStream stream2 = File.Open(file, FileMode.Open, FileAccess.Read, null, true))
					{
						using (XmlReader reader2 = XMLExtensions.CreateReader(stream2, ""))
						{
							doc = XDocument.Load(reader2);
						}
					}
					return doc;
				}
				catch (Exception e4)
				{
					exception = e4;
					DebugConsole.ThrowError(string.Concat(new string[]
					{
						"Loading submarine \"",
						file,
						"\" failed! (",
						e4.Message,
						")"
					}), null, null, false, false);
					return null;
				}
			}
			DebugConsole.ThrowError("Couldn't load submarine \"" + file + "! (Unrecognized file extension)", null, null, false, false);
			return null;
		}

		// Token: 0x06002BFF RID: 11263 RVA: 0x001210D4 File Offset: 0x0011F2D4
		public int GetPrice(Location location = null, ImmutableHashSet<Character> characterList = null)
		{
			if (location == null)
			{
				GameSession gameSession = GameMain.GameSession;
				Location location2;
				if (gameSession == null)
				{
					location2 = null;
				}
				else
				{
					CampaignMode campaign2 = gameSession.Campaign;
					if (campaign2 == null)
					{
						location2 = null;
					}
					else
					{
						Map map = campaign2.Map;
						location2 = ((map != null) ? map.CurrentLocation : null);
					}
				}
				Location currentLocation = location2;
				if (currentLocation == null)
				{
					return this.Price;
				}
				location = currentLocation;
			}
			if (characterList == null)
			{
				characterList = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			}
			float price = (float)this.Price;
			GameSession gameSession2 = GameMain.GameSession;
			CampaignMode campaign = (gameSession2 != null) ? gameSession2.Campaign : null;
			if (campaign != null)
			{
				price *= campaign.Settings.ShipyardPriceMultiplier;
			}
			if (characterList.Any<Character>())
			{
				Faction faction = location.Faction;
				if (faction != null && Faction.GetPlayerAffiliationStatus(faction) == FactionAffiliation.Positive)
				{
					price *= 1f - characterList.Max((Character c) => c.GetStatValue(StatTypes.ShipyardBuyMultiplierAffiliated, true));
				}
				price *= 1f - characterList.Max((Character c) => c.GetStatValue(StatTypes.ShipyardBuyMultiplier, true));
			}
			return (int)price;
		}

		// Token: 0x06002C00 RID: 11264 RVA: 0x001211CB File Offset: 0x0011F3CB
		public static int GetDefaultTier(int price)
		{
			if (price > 20000)
			{
				return 3;
			}
			if (price <= 10000)
			{
				return 1;
			}
			return 2;
		}

		// Token: 0x04001581 RID: 5505
		private static List<SubmarineInfo> savedSubmarines = new List<SubmarineInfo>();

		// Token: 0x04001582 RID: 5506
		private Task hashTask;

		// Token: 0x04001583 RID: 5507
		private Md5Hash hash;

		// Token: 0x04001584 RID: 5508
		public readonly DateTime LastModifiedTime;

		// Token: 0x04001586 RID: 5510
		public int RecommendedCrewSizeMin = 1;

		// Token: 0x04001587 RID: 5511
		public int RecommendedCrewSizeMax = 2;

		// Token: 0x04001588 RID: 5512
		public SubmarineInfo.CrewExperienceLevel RecommendedCrewExperience;

		// Token: 0x0400158B RID: 5515
		public HashSet<string> RequiredContentPackages = new HashSet<string>();

		// Token: 0x0400158C RID: 5516
		public const int MaxNameLength = 30;

		// Token: 0x0400158D RID: 5517
		public const int MaxDescriptionLength = 500;

		// Token: 0x04001598 RID: 5528
		public SubmarineClass SubmarineClass;

		// Token: 0x040015A2 RID: 5538
		public bool IsFromRemoteStorage;

		// Token: 0x040015A3 RID: 5539
		public readonly bool LazyLoad;

		// Token: 0x040015A4 RID: 5540
		private XElement submarineElement;

		// Token: 0x040015A6 RID: 5542
		private bool? requiredContentPackagesInstalled;

		// Token: 0x040015A7 RID: 5543
		private bool? subsLeftBehind;

		// Token: 0x040015A8 RID: 5544
		public readonly List<ushort> LeftBehindDockingPortIDs = new List<ushort>();

		// Token: 0x040015A9 RID: 5545
		public readonly List<ushort> BlockedDockingPortIDs = new List<ushort>();

		// Token: 0x040015AB RID: 5547
		public OutpostGenerationParams OutpostGenerationParams;

		// Token: 0x040015AC RID: 5548
		public readonly Dictionary<Identifier, List<Character>> OutpostNPCs = new Dictionary<Identifier, List<Character>>();

		// Token: 0x040015AE RID: 5550
		public const int HighestTier = 3;

		// Token: 0x02000AC2 RID: 2754
		public enum CrewExperienceLevel
		{
			// Token: 0x0400371D RID: 14109
			Unknown,
			// Token: 0x0400371E RID: 14110
			CrewExperienceLow,
			// Token: 0x0400371F RID: 14111
			CrewExperienceMid,
			// Token: 0x04003720 RID: 14112
			CrewExperienceHigh
		}
	}
}

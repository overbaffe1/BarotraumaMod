using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200003C RID: 60
	internal class Level : Entity, IServerSerializable, INetSerializable
	{
		// Token: 0x06000877 RID: 2167 RVA: 0x00050194 File Offset: 0x0004E394
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Level.IEventData eventData = extraData as Level.IEventData;
			if (eventData == null)
			{
				throw new Exception("Malformed level event: expected Level.IEventData");
			}
			msg.WriteByte((byte)eventData.EventType);
			if (eventData is Level.SingleLevelWallEventData)
			{
				Level.SingleLevelWallEventData singleLevelWallEventData = (Level.SingleLevelWallEventData)eventData;
				DestructibleLevelWall destructibleWall = singleLevelWallEventData.Wall;
				int index = this.ExtraWalls.IndexOf(destructibleWall);
				msg.WriteUInt16((ushort)((index == -1) ? 65535 : index));
				msg.WriteByte((byte)MathHelper.Clamp((int)(MathUtils.InverseLerp(0f, destructibleWall.MaxHealth, destructibleWall.Damage) * 255f), 0, 255));
				return;
			}
			if (eventData is Level.GlobalLevelWallEventData)
			{
				using (List<LevelWall>.Enumerator enumerator = this.ExtraWalls.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LevelWall levelWall = enumerator.Current;
						if (levelWall.Body.BodyType != BodyType.Static)
						{
							msg.WriteSingle(levelWall.Body.Position.X);
							msg.WriteSingle(levelWall.Body.Position.Y);
							msg.WriteRangedSingle(levelWall.MoveState, 0f, 6.2831855f, 16);
						}
					}
					return;
				}
			}
			throw new Exception("Malformed level event: did not expect " + eventData.GetType().Name);
		}

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000878 RID: 2168 RVA: 0x000502EC File Offset: 0x0004E4EC
		// (set) Token: 0x06000879 RID: 2169 RVA: 0x000502F3 File Offset: 0x0004E4F3
		public static Level Loaded
		{
			get
			{
				return Level.loaded;
			}
			private set
			{
				if (Level.loaded == value)
				{
					return;
				}
				Level.loaded = value;
				Level level = Level.loaded;
				GameAnalyticsManager.SetCurrentLevel((level != null) ? level.LevelData : null);
			}
		}

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x0600087A RID: 2170 RVA: 0x0005031A File Offset: 0x0004E51A
		// (set) Token: 0x0600087B RID: 2171 RVA: 0x00050322 File Offset: 0x0004E522
		public Rectangle AbyssArea { get; private set; }

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x0600087C RID: 2172 RVA: 0x0005032B File Offset: 0x0004E52B
		public int AbyssStart
		{
			get
			{
				return this.AbyssArea.Y + this.AbyssArea.Height;
			}
		}

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x0600087D RID: 2173 RVA: 0x00050344 File Offset: 0x0004E544
		public int AbyssEnd
		{
			get
			{
				return this.AbyssArea.Y;
			}
		}

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x0600087E RID: 2174 RVA: 0x00050351 File Offset: 0x0004E551
		public Vector2 StartPosition
		{
			get
			{
				return this.startPosition.ToVector2();
			}
		}

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x0600087F RID: 2175 RVA: 0x0005035E File Offset: 0x0004E55E
		public Vector2 StartExitPosition
		{
			get
			{
				return this.startExitPosition.ToVector2();
			}
		}

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x06000880 RID: 2176 RVA: 0x0005036B File Offset: 0x0004E56B
		public Point Size
		{
			get
			{
				return this.LevelData.Size;
			}
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x06000881 RID: 2177 RVA: 0x00050378 File Offset: 0x0004E578
		public Vector2 EndPosition
		{
			get
			{
				return this.endPosition.ToVector2();
			}
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06000882 RID: 2178 RVA: 0x00050385 File Offset: 0x0004E585
		public Vector2 EndExitPosition
		{
			get
			{
				return this.endExitPosition.ToVector2();
			}
		}

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06000883 RID: 2179 RVA: 0x00050392 File Offset: 0x0004E592
		// (set) Token: 0x06000884 RID: 2180 RVA: 0x0005039A File Offset: 0x0004E59A
		public int BottomPos { get; private set; }

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000885 RID: 2181 RVA: 0x000503A3 File Offset: 0x0004E5A3
		// (set) Token: 0x06000886 RID: 2182 RVA: 0x000503AB File Offset: 0x0004E5AB
		public int SeaFloorTopPos { get; private set; }

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000887 RID: 2183 RVA: 0x000503B4 File Offset: 0x0004E5B4
		public float CrushDepth
		{
			get
			{
				return this.LevelData.CrushDepth;
			}
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000888 RID: 2184 RVA: 0x000503C1 File Offset: 0x0004E5C1
		public float RealWorldCrushDepth
		{
			get
			{
				return this.LevelData.RealWorldCrushDepth;
			}
		}

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000889 RID: 2185 RVA: 0x000503CE File Offset: 0x0004E5CE
		// (set) Token: 0x0600088A RID: 2186 RVA: 0x000503D6 File Offset: 0x0004E5D6
		public LevelWall SeaFloor { get; private set; }

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x0600088B RID: 2187 RVA: 0x000503DF File Offset: 0x0004E5DF
		// (set) Token: 0x0600088C RID: 2188 RVA: 0x000503E7 File Offset: 0x0004E5E7
		public List<Ruin> Ruins { get; private set; }

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x0600088D RID: 2189 RVA: 0x000503F0 File Offset: 0x0004E5F0
		// (set) Token: 0x0600088E RID: 2190 RVA: 0x000503F8 File Offset: 0x0004E5F8
		public List<Submarine> Wrecks { get; private set; }

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x0600088F RID: 2191 RVA: 0x00050401 File Offset: 0x0004E601
		// (set) Token: 0x06000890 RID: 2192 RVA: 0x00050409 File Offset: 0x0004E609
		public Submarine BeaconStation { get; private set; }

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000891 RID: 2193 RVA: 0x00050412 File Offset: 0x0004E612
		// (set) Token: 0x06000892 RID: 2194 RVA: 0x0005041A File Offset: 0x0004E61A
		public List<LevelWall> ExtraWalls { get; private set; }

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000893 RID: 2195 RVA: 0x00050423 File Offset: 0x0004E623
		// (set) Token: 0x06000894 RID: 2196 RVA: 0x0005042B File Offset: 0x0004E62B
		public List<LevelWall> UnsyncedExtraWalls { get; private set; }

		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06000895 RID: 2197 RVA: 0x00050434 File Offset: 0x0004E634
		// (set) Token: 0x06000896 RID: 2198 RVA: 0x0005043C File Offset: 0x0004E63C
		public List<Level.Tunnel> Tunnels { get; private set; } = new List<Level.Tunnel>();

		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06000897 RID: 2199 RVA: 0x00050445 File Offset: 0x0004E645
		// (set) Token: 0x06000898 RID: 2200 RVA: 0x0005044D File Offset: 0x0004E64D
		public List<Level.Cave> Caves { get; private set; } = new List<Level.Cave>();

		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000899 RID: 2201 RVA: 0x00050456 File Offset: 0x0004E656
		// (set) Token: 0x0600089A RID: 2202 RVA: 0x0005045E File Offset: 0x0004E65E
		public List<Level.InterestingPosition> PositionsOfInterest { get; private set; }

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x0600089B RID: 2203 RVA: 0x00050467 File Offset: 0x0004E667
		// (set) Token: 0x0600089C RID: 2204 RVA: 0x0005046F File Offset: 0x0004E66F
		public Submarine StartOutpost { get; private set; }

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x0600089D RID: 2205 RVA: 0x00050478 File Offset: 0x0004E678
		// (set) Token: 0x0600089E RID: 2206 RVA: 0x00050480 File Offset: 0x0004E680
		public Submarine EndOutpost { get; private set; }

		// Token: 0x17000243 RID: 579
		// (get) Token: 0x0600089F RID: 2207 RVA: 0x00050489 File Offset: 0x0004E689
		public IReadOnlyDictionary<Level.LevelGenStage, int> EqualityCheckValues
		{
			get
			{
				return this.equalityCheckValues;
			}
		}

		// Token: 0x060008A0 RID: 2208 RVA: 0x00050491 File Offset: 0x0004E691
		private void GenerateEqualityCheckValue(Level.LevelGenStage stage)
		{
			this.equalityCheckValues[stage] = Rand.Int(int.MaxValue, Rand.RandSync.ServerAndClient);
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x000504AA File Offset: 0x0004E6AA
		private void SetEqualityCheckValue(Level.LevelGenStage stage, int value)
		{
			this.equalityCheckValues[stage] = value;
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x000504BC File Offset: 0x0004E6BC
		private void ClearEqualityCheckValues()
		{
			foreach (object obj in Enum.GetValues(typeof(Level.LevelGenStage)))
			{
				Level.LevelGenStage stage = (Level.LevelGenStage)obj;
				this.equalityCheckValues[stage] = 0;
			}
		}

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x060008A3 RID: 2211 RVA: 0x00050524 File Offset: 0x0004E724
		// (set) Token: 0x060008A4 RID: 2212 RVA: 0x0005052C File Offset: 0x0004E72C
		public List<Entity> EntitiesBeforeGenerate { get; private set; } = new List<Entity>();

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x060008A5 RID: 2213 RVA: 0x00050535 File Offset: 0x0004E735
		// (set) Token: 0x060008A6 RID: 2214 RVA: 0x0005053D File Offset: 0x0004E73D
		public int EntityCountBeforeGenerate { get; private set; }

		// Token: 0x17000246 RID: 582
		// (get) Token: 0x060008A7 RID: 2215 RVA: 0x00050546 File Offset: 0x0004E746
		// (set) Token: 0x060008A8 RID: 2216 RVA: 0x0005054E File Offset: 0x0004E74E
		public int EntityCountAfterGenerate { get; private set; }

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x060008A9 RID: 2217 RVA: 0x00050557 File Offset: 0x0004E757
		// (set) Token: 0x060008AA RID: 2218 RVA: 0x0005055F File Offset: 0x0004E75F
		public Body TopBarrier { get; private set; }

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x060008AB RID: 2219 RVA: 0x00050568 File Offset: 0x0004E768
		// (set) Token: 0x060008AC RID: 2220 RVA: 0x00050570 File Offset: 0x0004E770
		public Body BottomBarrier { get; private set; }

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x060008AD RID: 2221 RVA: 0x00050579 File Offset: 0x0004E779
		// (set) Token: 0x060008AE RID: 2222 RVA: 0x00050581 File Offset: 0x0004E781
		public LevelObjectManager LevelObjectManager { get; private set; }

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x060008AF RID: 2223 RVA: 0x0005058A File Offset: 0x0004E78A
		// (set) Token: 0x060008B0 RID: 2224 RVA: 0x00050592 File Offset: 0x0004E792
		public bool Generating { get; private set; }

		// Token: 0x1700024B RID: 587
		// (get) Token: 0x060008B1 RID: 2225 RVA: 0x0005059B File Offset: 0x0004E79B
		// (set) Token: 0x060008B2 RID: 2226 RVA: 0x000505A3 File Offset: 0x0004E7A3
		public Location StartLocation { get; private set; }

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x060008B3 RID: 2227 RVA: 0x000505AC File Offset: 0x0004E7AC
		// (set) Token: 0x060008B4 RID: 2228 RVA: 0x000505B4 File Offset: 0x0004E7B4
		public Location EndLocation { get; private set; }

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x060008B5 RID: 2229 RVA: 0x000505BD File Offset: 0x0004E7BD
		// (set) Token: 0x060008B6 RID: 2230 RVA: 0x000505C5 File Offset: 0x0004E7C5
		public bool Mirrored { get; private set; }

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x060008B7 RID: 2231 RVA: 0x000505CE File Offset: 0x0004E7CE
		public string Seed
		{
			get
			{
				return this.LevelData.Seed;
			}
		}

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x060008B8 RID: 2232 RVA: 0x000505DC File Offset: 0x0004E7DC
		public float Difficulty
		{
			get
			{
				float? forcedDifficulty = Level.ForcedDifficulty;
				if (forcedDifficulty == null)
				{
					return this.LevelData.Difficulty;
				}
				return forcedDifficulty.GetValueOrDefault();
			}
		}

		// Token: 0x060008B9 RID: 2233 RVA: 0x0005060B File Offset: 0x0004E80B
		public bool IsAllowedDifficulty(float minDifficulty, float maxDifficulty)
		{
			return this.LevelData.IsAllowedDifficulty(minDifficulty, maxDifficulty);
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x060008BA RID: 2234 RVA: 0x0005061A File Offset: 0x0004E81A
		public LevelData.LevelType Type
		{
			get
			{
				return this.LevelData.Type;
			}
		}

		// Token: 0x17000251 RID: 593
		// (get) Token: 0x060008BB RID: 2235 RVA: 0x00050627 File Offset: 0x0004E827
		public bool IsEndBiome
		{
			get
			{
				return this.LevelData.Biome != null && this.LevelData.Biome.IsEndBiome;
			}
		}

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x060008BC RID: 2236 RVA: 0x00050648 File Offset: 0x0004E848
		public static bool IsLoadedOutpost
		{
			get
			{
				Level level = Level.Loaded;
				return level != null && level.Type == LevelData.LevelType.Outpost;
			}
		}

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x060008BD RID: 2237 RVA: 0x00050660 File Offset: 0x0004E860
		public static bool IsLoadedFriendlyOutpost
		{
			get
			{
				Level level = Level.loaded;
				if (level == null || level.Type != LevelData.LevelType.Outpost)
				{
					return false;
				}
				Level level2 = Level.loaded;
				bool flag;
				CharacterTeamType? characterTeamType2;
				if (level2 == null)
				{
					flag = false;
				}
				else
				{
					Location startLocation = level2.StartLocation;
					CharacterTeamType? characterTeamType;
					if (startLocation == null)
					{
						characterTeamType = null;
					}
					else
					{
						LocationType type = startLocation.Type;
						characterTeamType = ((type != null) ? new CharacterTeamType?(type.OutpostTeam) : null);
					}
					characterTeamType2 = characterTeamType;
					flag = (characterTeamType2.GetValueOrDefault() == CharacterTeamType.FriendlyNPC);
				}
				if (flag)
				{
					return true;
				}
				Level level3 = Level.loaded;
				if (level3 == null)
				{
					return false;
				}
				Location startLocation2 = level3.StartLocation;
				CharacterTeamType? characterTeamType3;
				if (startLocation2 == null)
				{
					characterTeamType3 = null;
				}
				else
				{
					LocationType type2 = startLocation2.Type;
					characterTeamType3 = ((type2 != null) ? new CharacterTeamType?(type2.OutpostTeam) : null);
				}
				characterTeamType2 = characterTeamType3;
				return characterTeamType2.GetValueOrDefault() == CharacterTeamType.Team1;
			}
		}

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x060008BE RID: 2238 RVA: 0x0005071D File Offset: 0x0004E91D
		public LevelGenerationParams GenerationParams
		{
			get
			{
				return this.LevelData.GenerationParams;
			}
		}

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x060008BF RID: 2239 RVA: 0x0005072A File Offset: 0x0004E92A
		public Color BackgroundTextureColor
		{
			get
			{
				return this.LevelData.GenerationParams.BackgroundTextureColor;
			}
		}

		// Token: 0x17000256 RID: 598
		// (get) Token: 0x060008C0 RID: 2240 RVA: 0x0005073C File Offset: 0x0004E93C
		public Color BackgroundColor
		{
			get
			{
				return this.LevelData.GenerationParams.BackgroundColor;
			}
		}

		// Token: 0x17000257 RID: 599
		// (get) Token: 0x060008C1 RID: 2241 RVA: 0x0005074E File Offset: 0x0004E94E
		public Color WallColor
		{
			get
			{
				return this.LevelData.GenerationParams.WallColor;
			}
		}

		// Token: 0x060008C2 RID: 2242 RVA: 0x00050760 File Offset: 0x0004E960
		private Level(LevelData levelData) : base(null, 0)
		{
			this.LevelData = levelData;
			this.borders = new Rectangle(Point.Zero, levelData.Size);
		}

		// Token: 0x060008C3 RID: 2243 RVA: 0x00050844 File Offset: 0x0004EA44
		public bool ShouldSpawnCrewInsideOutpost()
		{
			if (this.StartOutpost != null && this.Type == LevelData.LevelType.Outpost)
			{
				OutpostGenerationParams outpostGenerationParams = this.StartOutpost.Info.OutpostGenerationParams;
				if (outpostGenerationParams != null && outpostGenerationParams.SpawnCrewInsideOutpost)
				{
					if (this.StartOutpost.GetConnectedSubs().Any((Submarine s) => s.Info.Type == SubmarineType.Player))
					{
						goto IL_68;
					}
				}
				if (Submarine.MainSub != null)
				{
					return false;
				}
				IL_68:
				CampaignMode campaign = GameMain.GameSession.Campaign;
				Location location = (campaign != null) ? campaign.CurrentLocation : null;
				return location == null || !location.IsFactionHostile;
			}
			return false;
		}

		// Token: 0x060008C4 RID: 2244 RVA: 0x000508E4 File Offset: 0x0004EAE4
		public static Level Generate(LevelData levelData, bool mirror, Location startLocation, Location endLocation, SubmarineInfo startOutpost = null, SubmarineInfo endOutpost = null)
		{
			if (levelData.Biome == null)
			{
				throw new ArgumentException("Biome was null");
			}
			if (levelData.Size.X <= 0)
			{
				throw new ArgumentException("Level width needs to be larger than zero.");
			}
			if (levelData.Size.Y <= 0)
			{
				throw new ArgumentException("Level height needs to be larger than zero.");
			}
			Level level = new Level(levelData)
			{
				preSelectedStartOutpost = startOutpost,
				preSelectedEndOutpost = endOutpost
			};
			level.Generate(mirror, startLocation, endLocation);
			return level;
		}

		// Token: 0x060008C5 RID: 2245 RVA: 0x00050958 File Offset: 0x0004EB58
		private void Generate(bool mirror, Location startLocation, Location endLocation)
		{
			Level level = Level.Loaded;
			if (level != null)
			{
				level.Remove();
			}
			Level.Loaded = this;
			this.Generating = true;
			DebugConsole.NewMessage("Level identifier: " + this.GenerationParams.Identifier.ToString(), null, false);
			this.ClearEqualityCheckValues();
			this.EntitiesBeforeGenerate = Entity.GetEntities().ToList<Entity>();
			this.EntityCountBeforeGenerate = this.EntitiesBeforeGenerate.Count<Entity>();
			this.StartLocation = startLocation;
			this.EndLocation = endLocation;
			this.ResetRandomSeed();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.GenStart);
			this.SetEqualityCheckValue(Level.LevelGenStage.LevelGenParams, (int)this.GenerationParams.UintIdentifier);
			this.SetEqualityCheckValue(Level.LevelGenStage.Size, this.borders.Width ^ this.borders.Height << 16);
			this.LevelObjectManager = new LevelObjectManager();
			if (this.Type == LevelData.LevelType.Outpost)
			{
				mirror = false;
			}
			this.Mirrored = mirror;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			this.PositionsOfInterest = new List<Level.InterestingPosition>();
			this.ExtraWalls = new List<LevelWall>();
			this.UnsyncedExtraWalls = new List<LevelWall>();
			this.bodies = new List<Body>();
			List<Vector2> sites = new List<Vector2>();
			Voronoi voronoi = new Voronoi(1.0);
			this.SeaFloorTopPos = this.GenerationParams.SeaFloorDepth + this.GenerationParams.MountainHeightMax + this.GenerationParams.SeaFloorVariance;
			int minMainPathWidth = Math.Min(this.GenerationParams.MinTunnelRadius, 16000);
			int minWidth = 500;
			if (Submarine.MainSub != null)
			{
				Rectangle dockedSubBorders = Submarine.MainSub.GetDockedBorders(true);
				dockedSubBorders.Inflate(dockedSubBorders.Size.ToVector2() * 0.15f);
				minWidth = Math.Max(dockedSubBorders.Width, dockedSubBorders.Height);
				minMainPathWidth = Math.Max(minMainPathWidth, minWidth);
				minMainPathWidth = Math.Min(minMainPathWidth, 16000);
			}
			minMainPathWidth = Math.Min(minMainPathWidth, this.borders.Width / 5);
			this.LevelData.MinMainPathWidth = new int?(minMainPathWidth);
			Rectangle pathBorders = this.borders;
			pathBorders.Inflate(-Math.Min(Math.Min(minMainPathWidth * 2, 16000), this.borders.Width / 5), -Math.Min(minMainPathWidth * 2, this.borders.Height / 5));
			if (pathBorders.Width <= 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(48, 1);
				defaultInterpolatedStringHandler.AppendLiteral("The width of the level's path area is invalid (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(pathBorders.Width);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (pathBorders.Height <= 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(49, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("The height of the level's path area is invalid (");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(pathBorders.Height);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				throw new InvalidOperationException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			this.startPosition = new Point((int)MathHelper.Lerp((float)minMainPathWidth, (float)(this.borders.Width - minMainPathWidth), this.GenerationParams.StartPosition.X), (int)MathHelper.Lerp((float)this.borders.Bottom - Math.Max((float)minMainPathWidth, 9000f), (float)(this.borders.Y + minMainPathWidth), this.GenerationParams.StartPosition.Y));
			this.startExitPosition = new Point(this.startPosition.X, this.borders.Bottom);
			this.endPosition = new Point((int)MathHelper.Lerp((float)minMainPathWidth, (float)(this.borders.Width - minMainPathWidth), this.GenerationParams.EndPosition.X), (int)MathHelper.Lerp((float)this.borders.Bottom - Math.Max((float)minMainPathWidth, 9000f), (float)(this.borders.Y + minMainPathWidth), this.GenerationParams.EndPosition.Y));
			this.endExitPosition = new Point(this.endPosition.X, this.borders.Bottom);
			this.GenerateEqualityCheckValue(Level.LevelGenStage.TunnelGen1);
			Level.Tunnel mainPath = new Level.Tunnel(Level.TunnelType.MainPath, this.GeneratePathNodes(this.startPosition, this.endPosition, pathBorders, null, this.GenerationParams.MainPathVariance), minMainPathWidth, null);
			this.Tunnels.Add(mainPath);
			Level.Tunnel startPath = null;
			Level.Tunnel endPath = null;
			Level.Tunnel endHole = null;
			if (this.GenerationParams.StartPosition.Y < 0.5f && (this.Mirrored ? (!this.HasEndOutpost()) : (!this.HasStartOutpost())))
			{
				startPath = new Level.Tunnel(Level.TunnelType.SidePath, new List<Point>
				{
					this.startExitPosition,
					this.startPosition
				}, minWidth, mainPath);
				this.Tunnels.Add(startPath);
			}
			else
			{
				this.startExitPosition = this.startPosition;
			}
			if (this.GenerationParams.EndPosition.Y < 0.5f && (this.Mirrored ? (!this.HasStartOutpost()) : (!this.HasEndOutpost())))
			{
				endPath = new Level.Tunnel(Level.TunnelType.SidePath, new List<Point>
				{
					this.endPosition,
					this.endExitPosition
				}, minWidth, mainPath);
				this.Tunnels.Add(endPath);
			}
			else
			{
				this.endExitPosition = this.endPosition;
			}
			if (this.GenerationParams.CreateHoleNextToEnd)
			{
				if (this.Mirrored)
				{
					endHole = new Level.Tunnel(Level.TunnelType.SidePath, new List<Point>
					{
						this.startPosition,
						new Point(0, this.startPosition.Y)
					}, minWidth, mainPath);
				}
				else
				{
					endHole = new Level.Tunnel(Level.TunnelType.SidePath, new List<Point>
					{
						this.endPosition,
						new Point(this.Size.X, this.endPosition.Y)
					}, minWidth, mainPath);
				}
				this.Tunnels.Add(endHole);
			}
			Level.Tunnel abyssTunnel = null;
			if (this.GenerationParams.CreateHoleToAbyss)
			{
				Point lowestPoint = mainPath.Nodes.First<Point>();
				foreach (Point pathNode in mainPath.Nodes)
				{
					if (pathNode.Y < lowestPoint.Y)
					{
						lowestPoint = pathNode;
					}
				}
				abyssTunnel = new Level.Tunnel(Level.TunnelType.SidePath, new List<Point>
				{
					lowestPoint,
					new Point(lowestPoint.X, 0)
				}, minWidth, mainPath);
				this.Tunnels.Add(abyssTunnel);
			}
			int sideTunnelCount = Rand.Range(this.GenerationParams.SideTunnelCount.X, this.GenerationParams.SideTunnelCount.Y + 1, Rand.RandSync.ServerAndClient);
			int i10 = 0;
			Predicate<Level.Tunnel> <>9__4;
			while (i10 < sideTunnelCount && mainPath.Nodes.Count >= 4)
			{
				List<Level.Tunnel> tunnels = this.Tunnels;
				Predicate<Level.Tunnel> match;
				if ((match = <>9__4) == null)
				{
					match = (<>9__4 = ((Level.Tunnel t) => t.Type != Level.TunnelType.Cave && t != startPath && t != endPath && t != endHole && t != abyssTunnel));
				}
				List<Level.Tunnel> validTunnels = tunnels.FindAll(match);
				Level.Tunnel tunnelToBranchOff = validTunnels[Rand.Int(validTunnels.Count, Rand.RandSync.ServerAndClient)];
				if (tunnelToBranchOff == null)
				{
					tunnelToBranchOff = mainPath;
				}
				Point branchStart = tunnelToBranchOff.Nodes[Rand.Range(0, tunnelToBranchOff.Nodes.Count / 3, Rand.RandSync.ServerAndClient)];
				Point branchEnd = tunnelToBranchOff.Nodes[Rand.Range(tunnelToBranchOff.Nodes.Count / 3 * 2, tunnelToBranchOff.Nodes.Count - 1, Rand.RandSync.ServerAndClient)];
				List<Point> sidePathNodes = this.GeneratePathNodes(branchStart, branchEnd, pathBorders, tunnelToBranchOff, this.GenerationParams.SideTunnelVariance);
				int pathWidth = Rand.Range(this.GenerationParams.MinSideTunnelRadius.X, this.GenerationParams.MinSideTunnelRadius.Y, Rand.RandSync.ServerAndClient);
				this.Tunnels.Add(new Level.Tunnel(Level.TunnelType.SidePath, sidePathNodes, pathWidth, tunnelToBranchOff));
				i10++;
			}
			this.CalculateTunnelDistanceField(null);
			this.GenerateSeaFloorPositions();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.TunnelGen2);
			this.GenerateAbyssArea();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.AbyssGen);
			this.GenerateCaves(mainPath);
			this.GenerateEqualityCheckValue(Level.LevelGenStage.CaveGen);
			this.GenerateVoronoiSites();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.VoronoiGen);
			Stopwatch sw2 = new Stopwatch();
			sw2.Start();
			int remainingRetries = 5;
			bool voronoiGraphInvalid;
			do
			{
				remainingRetries--;
				voronoiGraphInvalid = false;
				List<GraphEdge> graphEdges = voronoi.MakeVoronoiGraph(this.siteCoordsX.ToArray(), this.siteCoordsY.ToArray(), this.borders.Width, this.borders.Height);
				this.cells = CaveGenerator.GraphEdgesToCells(graphEdges, this.borders, 2000f, out this.cellGrid);
				for (int j = 0; j < this.cells.Count; j++)
				{
					for (int k = j + 1; k < this.cells.Count; k++)
					{
						if (this.cells[k].IsPointInside(this.cells[j].Center))
						{
							voronoiGraphInvalid = true;
							break;
						}
						if (voronoiGraphInvalid)
						{
							break;
						}
					}
				}
				if (voronoiGraphInvalid)
				{
					string errorMsg = "Unknown error during level generation. Invalid voronoi graph: the same voronoi site was inside multiple cells.";
					if (remainingRetries > 0)
					{
						DebugConsole.AddWarning(errorMsg + " Retrying...", null);
						this.GenerateVoronoiSites();
					}
					else
					{
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
					}
				}
			}
			while (remainingRetries > 0 && voronoiGraphInvalid);
			this.GenerateAbyssGeometry();
			this.GenerateAbyssPositions();
			sw2.Restart();
			this.ResetRandomSeed();
			List<VoronoiCell> pathCells = new List<VoronoiCell>();
			using (List<Level.Tunnel>.Enumerator enumerator2 = this.Tunnels.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Level.Tunnel tunnel = enumerator2.Current;
					CaveGenerator.GeneratePath(tunnel, this);
					if ((tunnel.Type == Level.TunnelType.MainPath || tunnel.Type == Level.TunnelType.SidePath) && tunnel != startPath && tunnel != endPath && tunnel != endHole)
					{
						List<VoronoiCell> distinctCells = tunnel.Cells.Distinct<VoronoiCell>().ToList<VoronoiCell>();
						Predicate<Level.Cave> <>9__6;
						for (int l = 2; l < distinctCells.Count; l += 3)
						{
							List<Level.InterestingPosition> positionsOfInterest = this.PositionsOfInterest;
							Point position = new Point((int)distinctCells[l].Site.Coord.X, (int)distinctCells[l].Site.Coord.Y);
							Level.PositionType positionType = (tunnel.Type == Level.TunnelType.MainPath) ? Level.PositionType.MainPath : Level.PositionType.SidePath;
							List<Level.Cave> caves = this.Caves;
							Predicate<Level.Cave> match2;
							if ((match2 = <>9__6) == null)
							{
								match2 = (<>9__6 = ((Level.Cave cave) => cave.Tunnels.Contains(tunnel)));
							}
							positionsOfInterest.Add(new Level.InterestingPosition(position, positionType, caves.Find(match2), true));
						}
					}
					bool connectToParentTunnel = tunnel.Type != Level.TunnelType.Cave || tunnel.ParentTunnel.Type == Level.TunnelType.Cave;
					this.GenerateWaypoints(tunnel, connectToParentTunnel ? tunnel.ParentTunnel : null);
					this.EnlargePath(tunnel.Cells, (float)tunnel.MinWidth);
					foreach (VoronoiCell pathCell in tunnel.Cells)
					{
						Level.<Generate>g__MarkEdges|191_5(pathCell, tunnel.Type);
						if (!pathCells.Contains(pathCell))
						{
							pathCells.Add(pathCell);
						}
					}
				}
			}
			List<VoronoiCell> potentialIslands = new List<VoronoiCell>();
			using (List<VoronoiCell>.Enumerator enumerator4 = pathCells.GetEnumerator())
			{
				while (enumerator4.MoveNext())
				{
					VoronoiCell cell = enumerator4.Current;
					if (this.GetDistToTunnel(cell.Center, mainPath) >= (double)minMainPathWidth && (startPath == null || this.GetDistToTunnel(cell.Center, startPath) >= (double)minMainPathWidth) && (endPath == null || this.GetDistToTunnel(cell.Center, endPath) >= (double)minMainPathWidth) && (endHole == null || this.GetDistToTunnel(cell.Center, endHole) >= (double)minMainPathWidth) && !cell.Edges.Any(delegate(GraphEdge e)
					{
						VoronoiCell voronoiCell = e.AdjacentCell(cell);
						return voronoiCell == null || voronoiCell.CellType != CellType.Path || e.NextToCave;
					}) && !this.PositionsOfInterest.Any((Level.InterestingPosition p) => cell.IsPointInside(p.Position.ToVector2())))
					{
						potentialIslands.Add(cell);
					}
				}
			}
			int m2 = 0;
			while (m2 < this.GenerationParams.IslandCount && potentialIslands.Count != 0)
			{
				VoronoiCell island = potentialIslands.GetRandom(Rand.RandSync.ServerAndClient);
				island.CellType = CellType.Solid;
				island.Island = true;
				pathCells.Remove(island);
				m2++;
			}
			foreach (Level.InterestingPosition positionOfInterest in this.PositionsOfInterest)
			{
				Point position2 = positionOfInterest.Position;
				WayPoint wayPoint = new WayPoint(position2.ToVector2(), SpawnType.Enemy, null, null);
				wayPoint.Cave = positionOfInterest.Cave;
				wayPoint.Ruin = positionOfInterest.Ruin;
			}
			this.startPosition.X = (int)pathCells[0].Site.Coord.X;
			this.startExitPosition.X = this.startPosition.X;
			this.GenerateEqualityCheckValue(Level.LevelGenStage.VoronoiGen2);
			if (this.GenerationParams.NoLevelGeometry)
			{
				this.cells.ForEach(delegate(VoronoiCell c)
				{
					c.CellType = CellType.Removed;
				});
				this.cells.Clear();
			}
			this.cells = this.cells.Except(pathCells).ToList<VoronoiCell>();
			this.cells.ForEachMod(delegate(VoronoiCell c)
			{
				if (c.Edges.Any((GraphEdge e) => !MathUtils.NearlyEqual(e.Point1.Y, (float)this.Size.Y, 0.0001f) && e.AdjacentCell(c) == null))
				{
					c.CellType = CellType.Removed;
					this.cells.Remove(c);
				}
			});
			int xPadding = this.borders.Width / 5;
			pathCells.AddRange(this.CreateHoles(this.GenerationParams.BottomHoleProbability, new Rectangle(xPadding, 0, this.borders.Width - xPadding * 2, this.Size.Y / 2), minMainPathWidth));
			foreach (VoronoiCell cell10 in this.cells)
			{
				if (cell10.Site.Coord.Y >= (double)(this.borders.Height / 2))
				{
					cell10.Edges.ForEach(delegate(GraphEdge e)
					{
						e.OutsideLevel = true;
					});
				}
			}
			foreach (Level.AbyssIsland abyssIsland in this.AbyssIslands)
			{
				abyssIsland.Cells.RemoveAll((VoronoiCell c) => c.CellType == CellType.Path);
				this.cells.AddRange(abyssIsland.Cells);
			}
			this.ResetRandomSeed();
			List<Point> ruinPositions = new List<Point>();
			int ruinCount = this.GenerationParams.UseRandomRuinCount() ? Rand.Range(this.GenerationParams.MinRuinCount, this.GenerationParams.MaxRuinCount + 1, Rand.RandSync.ServerAndClient) : this.GenerationParams.RuinCount;
			GameSession gameSession = GameMain.GameSession;
			bool? flag;
			if (gameSession == null)
			{
				flag = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				if (gameMode == null)
				{
					flag = null;
				}
				else
				{
					flag = new bool?(gameMode.Missions.Any((Mission m) => m.Prefab.RequireRuin));
				}
			}
			bool? flag2 = flag;
			bool hasRuinMissions = flag2.GetValueOrDefault();
			if (hasRuinMissions)
			{
				ruinCount = Math.Max(ruinCount, 1);
			}
			for (int n = 0; n < ruinCount; n++)
			{
				if (hasRuinMissions || Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient) < this.GenerationParams.RuinSpawnProbability)
				{
					Point ruinSize = new Point(5000);
					int limitLeft = Math.Max(this.startPosition.X, ruinSize.X / 2);
					int limitRight = Math.Min(this.endPosition.X, this.Size.X - ruinSize.X / 2);
					Rectangle limits = new Rectangle(limitLeft, ruinSize.Y, limitRight - limitLeft, this.Size.Y - ruinSize.Y);
					ruinPositions.Add(this.FindPosAwayFromMainPath((double)((float)(Math.Max(ruinSize.X, ruinSize.Y) + mainPath.MinWidth) * 1.2f), true, new Rectangle?(limits)));
					this.CalculateTunnelDistanceField(ruinPositions);
				}
			}
			foreach (VoronoiCell cell2 in pathCells)
			{
				cell2.Edges.ForEach(delegate(GraphEdge e)
				{
					e.OutsideLevel = false;
				});
				cell2.CellType = CellType.Path;
				this.cells.Remove(cell2);
			}
			for (int x = 0; x < this.cellGrid.GetLength(0); x++)
			{
				for (int y = 0; y < this.cellGrid.GetLength(1); y++)
				{
					this.cellGrid[x, y].Clear();
				}
			}
			if (mirror)
			{
				HashSet<GraphEdge> mirroredEdges = new HashSet<GraphEdge>();
				HashSet<Site> mirroredSites = new HashSet<Site>();
				List<VoronoiCell> allCells = new List<VoronoiCell>(this.cells);
				allCells.AddRange(pathCells);
				foreach (VoronoiCell cell3 in allCells)
				{
					foreach (GraphEdge edge in cell3.Edges)
					{
						if (!mirroredEdges.Contains(edge))
						{
							edge.Point1.X = (float)this.borders.Width - edge.Point1.X;
							edge.Point2.X = (float)this.borders.Width - edge.Point2.X;
							if (edge.Site1 != null && !mirroredSites.Contains(edge.Site1))
							{
								if (edge.Site1.Coord.X % 2000.0 < 1.0 && edge.Site1.Coord.X % 2000.0 >= 0.0)
								{
									edge.Site1.Coord.X += 1.0;
								}
								edge.Site1.Coord.X = (double)this.borders.Width - edge.Site1.Coord.X;
								mirroredSites.Add(edge.Site1);
							}
							if (edge.Site2 != null && !mirroredSites.Contains(edge.Site2))
							{
								if (edge.Site2.Coord.X % 2000.0 < 1.0 && edge.Site2.Coord.X % 2000.0 >= 0.0)
								{
									edge.Site2.Coord.X += 1.0;
								}
								edge.Site2.Coord.X = (double)this.borders.Width - edge.Site2.Coord.X;
								mirroredSites.Add(edge.Site2);
							}
							mirroredEdges.Add(edge);
						}
					}
				}
				foreach (Level.AbyssIsland island2 in this.AbyssIslands)
				{
					island2.Area = new Rectangle(this.borders.Width - island2.Area.Right, island2.Area.Y, island2.Area.Width, island2.Area.Height);
					foreach (VoronoiCell cell4 in island2.Cells)
					{
						if (!mirroredSites.Contains(cell4.Site))
						{
							if (cell4.Site.Coord.X % 2000.0 < 1.0 && cell4.Site.Coord.X % 2000.0 >= 0.0)
							{
								cell4.Site.Coord.X += 1.0;
							}
							cell4.Site.Coord.X = (double)this.borders.Width - cell4.Site.Coord.X;
							mirroredSites.Add(cell4.Site);
						}
					}
				}
				for (int i2 = 0; i2 < ruinPositions.Count; i2++)
				{
					ruinPositions[i2] = new Point(this.borders.Width - ruinPositions[i2].X, ruinPositions[i2].Y);
				}
				foreach (Level.Cave cave3 in this.Caves)
				{
					cave3.Area = new Rectangle(this.borders.Width - cave3.Area.Right, cave3.Area.Y, cave3.Area.Width, cave3.Area.Height);
					cave3.StartPos = new Point(this.borders.Width - cave3.StartPos.X, cave3.StartPos.Y);
					cave3.EndPos = new Point(this.borders.Width - cave3.EndPos.X, cave3.EndPos.Y);
				}
				foreach (Level.Tunnel tunnel3 in this.Tunnels)
				{
					for (int i3 = 0; i3 < tunnel3.Nodes.Count; i3++)
					{
						tunnel3.Nodes[i3] = new Point(this.borders.Width - tunnel3.Nodes[i3].X, tunnel3.Nodes[i3].Y);
					}
				}
				for (int i4 = 0; i4 < this.PositionsOfInterest.Count; i4++)
				{
					this.PositionsOfInterest[i4] = new Level.InterestingPosition(new Point(this.borders.Width - this.PositionsOfInterest[i4].Position.X, this.PositionsOfInterest[i4].Position.Y), this.PositionsOfInterest[i4].PositionType, null, true)
					{
						Submarine = this.PositionsOfInterest[i4].Submarine,
						Cave = this.PositionsOfInterest[i4].Cave,
						Ruin = this.PositionsOfInterest[i4].Ruin
					};
				}
				foreach (WayPoint waypoint in WayPoint.WayPointList)
				{
					if (waypoint.Submarine == null)
					{
						waypoint.Move(new Vector2(((float)(this.borders.Width / 2) - waypoint.Position.X) * 2f, 0f), true);
					}
				}
				for (int i5 = 0; i5 < this.bottomPositions.Count; i5++)
				{
					this.bottomPositions[i5] = new Point(this.borders.Size.X - this.bottomPositions[i5].X, this.bottomPositions[i5].Y);
				}
				this.bottomPositions.Reverse();
				this.startPosition.X = this.borders.Width - this.startPosition.X;
				this.endPosition.X = this.borders.Width - this.endPosition.X;
				this.startExitPosition.X = this.borders.Width - this.startExitPosition.X;
				this.endExitPosition.X = this.borders.Width - this.endExitPosition.X;
				this.CalculateTunnelDistanceField(ruinPositions);
			}
			foreach (VoronoiCell cell5 in this.cells)
			{
				int x2 = (int)Math.Floor(cell5.Site.Coord.X / 2000.0);
				x2 = MathHelper.Clamp(x2, 0, this.cellGrid.GetLength(0) - 1);
				int y2 = (int)Math.Floor(cell5.Site.Coord.Y / 2000.0);
				y2 = MathHelper.Clamp(y2, 0, this.cellGrid.GetLength(1) - 1);
				this.cellGrid[x2, y2].Add(cell5);
			}
			float destructibleWallRatio = MathHelper.Lerp(0.2f, 1f, this.LevelData.Difficulty / 100f);
			foreach (Level.Cave cave2 in this.Caves)
			{
				if (cave2.Area.Y > 0)
				{
					List<VoronoiCell> cavePathCells = this.CreatePathToClosestTunnel(cave2.StartPos);
					Level.Tunnel mainTunnel = cave2.Tunnels.Find((Level.Tunnel t) => t.ParentTunnel.Type != Level.TunnelType.Cave);
					WayPoint prevWp = mainTunnel.WayPoints.First<WayPoint>();
					if (prevWp != null)
					{
						int i9;
						int i;
						Predicate<GraphEdge> <>9__15;
						for (i = 0; i < cavePathCells.Count; i = i9 + 1)
						{
							GraphEdge graphEdge;
							if (i <= 0)
							{
								graphEdge = null;
							}
							else
							{
								List<GraphEdge> edges = cavePathCells[i].Edges;
								Predicate<GraphEdge> match3;
								if ((match3 = <>9__15) == null)
								{
									match3 = (<>9__15 = ((GraphEdge e) => e.AdjacentCell(cavePathCells[i]) == cavePathCells[i - 1]));
								}
								graphEdge = edges.Find(match3);
							}
							GraphEdge connectingEdge = graphEdge;
							if (connectingEdge != null)
							{
								WayPoint edgeWayPoint = new WayPoint(connectingEdge.Center, SpawnType.Path, null, null)
								{
									Cave = cave2
								};
								this.ConnectWaypoints(prevWp, edgeWayPoint, 500f);
								prevWp = edgeWayPoint;
							}
							WayPoint newWaypoint = new WayPoint(cavePathCells[i].Center, SpawnType.Path, null, null)
							{
								Cave = cave2
							};
							this.ConnectWaypoints(prevWp, newWaypoint, 500f);
							prevWp = newWaypoint;
							i9 = i;
						}
						WayPoint closestPathPoint = Level.FindClosestWayPoint(prevWp.WorldPosition, mainTunnel.ParentTunnel.WayPoints, null);
						this.ConnectWaypoints(prevWp, closestPathPoint, 500f);
					}
				}
				List<VoronoiCell> caveCells = new List<VoronoiCell>();
				caveCells.AddRange(cave2.Tunnels.SelectMany((Level.Tunnel t) => t.Cells));
				using (List<VoronoiCell>.Enumerator enumerator18 = caveCells.GetEnumerator())
				{
					while (enumerator18.MoveNext())
					{
						VoronoiCell caveCell = enumerator18.Current;
						if (!this.PositionsOfInterest.Any((Level.InterestingPosition p) => caveCell.IsPointInside(p.Position.ToVector2())) && Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient) < destructibleWallRatio * cave2.CaveGenerationParams.DestructibleWallRatio)
						{
							DestructibleLevelWall chunk = this.CreateIceChunk(caveCell.Edges, caveCell.Center, new float?(50f));
							if (chunk != null)
							{
								chunk.Body.BodyType = BodyType.Static;
								this.ExtraWalls.Add(chunk);
							}
						}
					}
				}
			}
			this.GenerateEqualityCheckValue(Level.LevelGenStage.VoronoiGen3);
			this.Ruins = new List<Ruin>();
			for (int i6 = 0; i6 < ruinPositions.Count; i6++)
			{
				Rand.SetSyncedSeed(ToolBox.StringToInt(this.Seed) + i6);
				this.GenerateRuin(ruinPositions[i6], mirror, hasRuinMissions);
			}
			this.GenerateEqualityCheckValue(Level.LevelGenStage.Ruins);
			if (this.GenerationParams.FloatingIceChunkCount > 0)
			{
				List<Point> iceChunkPositions = new List<Point>();
				foreach (Level.InterestingPosition pos in this.PositionsOfInterest)
				{
					if ((pos.PositionType == Level.PositionType.MainPath || pos.PositionType == Level.PositionType.SidePath) && pos.Position.X >= pathBorders.X + minMainPathWidth && pos.Position.X <= pathBorders.Right - minMainPathWidth && Math.Abs(pos.Position.X - this.startPosition.X) >= minMainPathWidth * 2 && Math.Abs(pos.Position.X - this.endPosition.X) >= minMainPathWidth * 2)
					{
						Point position2 = pos.Position;
						if (this.GetTooCloseCells(position2.ToVector2(), (float)minMainPathWidth * 0.7f).Count <= 0)
						{
							iceChunkPositions.Add(pos.Position);
						}
					}
				}
				int i7 = 0;
				while (i7 < this.GenerationParams.FloatingIceChunkCount && iceChunkPositions.Count != 0)
				{
					Point selectedPos = iceChunkPositions[Rand.Int(iceChunkPositions.Count, Rand.RandSync.ServerAndClient)];
					float chunkRadius = Rand.Range(500f, 1000f, Rand.RandSync.ServerAndClient);
					List<Vector2> vertices = CaveGenerator.CreateRandomChunk(chunkRadius, 8, chunkRadius * 0.8f);
					DestructibleLevelWall chunk2 = this.CreateIceChunk(vertices, selectedPos.ToVector2(), null);
					chunk2.MoveAmount = new Vector2(0f, (float)minMainPathWidth * 0.7f);
					chunk2.MoveSpeed = Rand.Range(100f, 200f, Rand.RandSync.ServerAndClient);
					this.ExtraWalls.Add(chunk2);
					iceChunkPositions.Remove(selectedPos);
					i7++;
				}
			}
			this.GenerateEqualityCheckValue(Level.LevelGenStage.FloatingIce);
			foreach (VoronoiCell cell6 in this.cells)
			{
				foreach (GraphEdge ge in cell6.Edges)
				{
					VoronoiCell adjacentCell = ge.AdjacentCell(cell6);
					ge.IsSolid = (adjacentCell == null || !this.cells.Contains(adjacentCell));
				}
			}
			List<VoronoiCell> cellsWithBody = new List<VoronoiCell>(this.cells);
			if (this.GenerationParams.CellRoundingAmount > 0.01f || this.GenerationParams.CellIrregularity > 0.01f)
			{
				foreach (VoronoiCell cell7 in cellsWithBody)
				{
					CaveGenerator.RoundCell(cell7, (float)this.GenerationParams.CellSubdivisionLength, this.GenerationParams.CellRoundingAmount, this.GenerationParams.CellIrregularity, this.GenerationParams.WallTextureExpandInwardsAmount);
				}
			}
			List<Vector2[]> triangles;
			this.bodies.Add(CaveGenerator.GeneratePolygons(cellsWithBody, this, out triangles));
			foreach (VoronoiCell cell8 in this.cells)
			{
				CompareCCW compare = new CompareCCW(cell8.Center);
				foreach (GraphEdge edge2 in cell8.Edges)
				{
					if (edge2.Cell1 != null && edge2.Cell1.Body == null && edge2.Cell1.CellType != CellType.Empty)
					{
						edge2.Cell1 = null;
					}
					if (edge2.Cell2 != null && edge2.Cell2.Body == null && edge2.Cell2.CellType != CellType.Empty)
					{
						edge2.Cell2 = null;
					}
					if (compare.Compare(edge2.Point1, edge2.Point2) == -1)
					{
						Vector2 temp = edge2.Point1;
						edge2.Point1 = edge2.Point2;
						edge2.Point2 = temp;
					}
				}
			}
			this.GenerateEqualityCheckValue(Level.LevelGenStage.LevelBodies);
			this.ResetRandomSeed();
			List<GraphEdge> usedSpireEdges = new List<GraphEdge>();
			for (int i8 = 0; i8 < this.GenerationParams.IceSpireCount; i8++)
			{
				DestructibleLevelWall spire = this.CreateIceSpire(usedSpireEdges);
				if (spire != null)
				{
					this.ExtraWalls.Add(spire);
				}
			}
			this.GenerateEqualityCheckValue(Level.LevelGenStage.IceSpires);
			foreach (Ruin ruin in this.Ruins)
			{
				this.GenerateRuinWayPoints(ruin);
			}
			foreach (Level.Tunnel tunnel2 in this.Tunnels)
			{
				if (tunnel2.ParentTunnel != null && (tunnel2.Type != Level.TunnelType.Cave || tunnel2.ParentTunnel != mainPath))
				{
					this.ConnectWaypoints(tunnel2, tunnel2.ParentTunnel);
				}
			}
			this.CreateOutposts();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.Outposts);
			this.TopBarrier = GameMain.World.CreateEdge(ConvertUnits.ToSimUnits(new Vector2((float)this.borders.X, 0f)), ConvertUnits.ToSimUnits(new Vector2((float)this.borders.Right, 0f)), BodyType.Static, Category.Cat1, Category.All, true);
			this.TopBarrier.UserData = "topbarrier";
			this.TopBarrier.SetTransform(ConvertUnits.ToSimUnits(new Vector2(0f, (float)this.borders.Height)), 0f);
			this.TopBarrier.BodyType = BodyType.Static;
			this.TopBarrier.CollisionCategories = Category.Cat8;
			this.bodies.Add(this.TopBarrier);
			this.GenerateSeaFloor();
			if (mirror)
			{
				Point tempP = this.startPosition;
				this.startPosition = this.endPosition;
				this.endPosition = tempP;
				tempP = this.startExitPosition;
				this.startExitPosition = this.endExitPosition;
				this.endExitPosition = tempP;
			}
			if (this.StartOutpost != null)
			{
				this.startExitPosition = this.StartOutpost.WorldPosition.ToPoint();
				this.startPosition = this.startExitPosition;
			}
			if (this.EndOutpost != null)
			{
				this.endExitPosition = this.EndOutpost.WorldPosition.ToPoint();
				this.endPosition = this.endExitPosition;
			}
			this.CreateWrecks();
			this.CreateBeaconStation();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.TopAndBottom);
			this.ResetRandomSeed();
			this.LevelObjectManager.PlaceObjects(this, this.GenerationParams.LevelObjectAmount);
			this.GenerateEqualityCheckValue(Level.LevelGenStage.PlaceLevelObjects);
			this.GenerateItems();
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.Info.IsOutpost)
				{
					OutpostGenerator.PowerUpOutpost(sub);
				}
			}
			this.GenerateEqualityCheckValue(Level.LevelGenStage.GenerateItems);
			foreach (VoronoiCell cell9 in this.cells)
			{
				foreach (GraphEdge edge3 in cell9.Edges)
				{
					edge3.Site1 = null;
					edge3.Site2 = null;
				}
			}
			MapEntity.MapLoaded(MapEntity.MapEntityList.FindAll((MapEntity me) => me.Submarine == null), false);
			sw2.Restart();
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage(string.Concat(new string[]
				{
					"Generated level with the seed ",
					this.Seed,
					" (type: ",
					this.GenerationParams.Identifier.ToString(),
					")"
				}), new Color?(Color.White), false);
			}
			this.EntityCountAfterGenerate = Entity.GetEntities().Count<Entity>();
			if (GameMain.Server.EntityEventManager.Events.Count<ServerEntityEvent>() > 0)
			{
				DebugConsole.NewMessage("WARNING: Entity events have been created during level generation. Events should not be created until the round is fully initialized.", null, false);
			}
			GameMain.Server.EntityEventManager.Clear();
			this.GenerateEqualityCheckValue(Level.LevelGenStage.Finish);
			this.Generating = false;
		}

		// Token: 0x060008C6 RID: 2246 RVA: 0x00053240 File Offset: 0x00051440
		private void GenerateVoronoiSites()
		{
			this.ResetRandomSeed();
			Point siteInterval = this.GenerationParams.VoronoiSiteInterval;
			int siteIntervalSqr = siteInterval.X * siteInterval.X + siteInterval.Y * siteInterval.Y;
			Point siteVariance = this.GenerationParams.VoronoiSiteVariance;
			this.siteCoordsX = new List<double>(this.borders.Height / siteInterval.Y * (this.borders.Width / siteInterval.Y));
			this.siteCoordsY = new List<double>(this.borders.Height / siteInterval.Y * (this.borders.Width / siteInterval.Y));
			for (int x = siteInterval.X / 2; x < this.borders.Width - siteInterval.X / 2; x += siteInterval.X)
			{
				for (int y = siteInterval.Y / 2; y < this.borders.Height - siteInterval.Y / 2; y += siteInterval.Y)
				{
					int siteX = x + Rand.Range(-siteVariance.X, siteVariance.X + 1, Rand.RandSync.ServerAndClient);
					int siteY = y + Rand.Range(-siteVariance.Y, siteVariance.Y + 1, Rand.RandSync.ServerAndClient);
					bool closeToTunnel = false;
					bool closeToCave = false;
					foreach (Level.Tunnel tunnel in this.Tunnels)
					{
						float minDist = Math.Max((float)tunnel.MinWidth * 2f, (float)Math.Max(siteInterval.X, siteInterval.Y));
						for (int i = 1; i < tunnel.Nodes.Count; i++)
						{
							if ((float)siteX >= (float)Math.Min(tunnel.Nodes[i - 1].X, tunnel.Nodes[i].X) - minDist && (float)siteX <= (float)Math.Max(tunnel.Nodes[i - 1].X, tunnel.Nodes[i].X) + minDist && (float)siteY >= (float)Math.Min(tunnel.Nodes[i - 1].Y, tunnel.Nodes[i].Y) - minDist && (float)siteY <= (float)Math.Max(tunnel.Nodes[i - 1].Y, tunnel.Nodes[i].Y) + minDist)
							{
								double tunnelDistSqr = MathUtils.LineSegmentToPointDistanceSquared(tunnel.Nodes[i - 1], tunnel.Nodes[i], new Point(siteX, siteY));
								if (Math.Sqrt(tunnelDistSqr) < (double)minDist)
								{
									closeToTunnel = true;
									if (tunnel.Type == Level.TunnelType.Cave)
									{
										closeToCave = true;
										break;
									}
									break;
								}
							}
						}
					}
					if (closeToTunnel || Rand.Range(0, 10, Rand.RandSync.ServerAndClient) == 0)
					{
						if (!this.<GenerateVoronoiSites>g__TooCloseToOtherSites|192_0((double)siteX, (double)siteY, 10f))
						{
							this.siteCoordsX.Add((double)siteX);
							this.siteCoordsY.Add((double)siteY);
						}
						if (closeToCave)
						{
							for (int x2 = x - siteInterval.X; x2 < x + siteInterval.X; x2 += 500)
							{
								for (int y2 = y - siteInterval.Y; y2 < y + siteInterval.Y; y2 += 500)
								{
									int caveSiteX = x2 + Rand.Int(250, Rand.RandSync.ServerAndClient);
									int caveSiteY = y2 + Rand.Int(250, Rand.RandSync.ServerAndClient);
									if (!this.<GenerateVoronoiSites>g__TooCloseToOtherSites|192_0((double)caveSiteX, (double)caveSiteY, 500f))
									{
										this.siteCoordsX.Add((double)caveSiteX);
										this.siteCoordsY.Add((double)caveSiteY);
									}
								}
							}
						}
					}
				}
			}
			for (int j = 0; j < this.siteCoordsX.Count; j++)
			{
				for (int k = j + 1; k < this.siteCoordsX.Count; k++)
				{
				}
			}
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x060008C7 RID: 2247 RVA: 0x00053668 File Offset: 0x00051868
		private int RandomHash
		{
			get
			{
				if (this.Seed != this.previousSeed)
				{
					this.isRandomHashSet = false;
				}
				if (!this.isRandomHashSet)
				{
					this._randomHash = ToolBox.StringToInt(this.Seed);
					this.isRandomHashSet = true;
					this.previousSeed = this.Seed;
				}
				return this._randomHash;
			}
		}

		// Token: 0x060008C8 RID: 2248 RVA: 0x000536C1 File Offset: 0x000518C1
		private void ResetRandomSeed()
		{
			Rand.SetSyncedSeed(this.RandomHash);
		}

		// Token: 0x060008C9 RID: 2249 RVA: 0x000536D0 File Offset: 0x000518D0
		private List<Point> GeneratePathNodes(Point startPosition, Point endPosition, Rectangle pathBorders, Level.Tunnel parentTunnel, float variance)
		{
			List<Point> pathNodes = new List<Point>
			{
				startPosition
			};
			Point nodeInterval = this.GenerationParams.MainPathNodeIntervalRange;
			for (int x = startPosition.X + nodeInterval.X; x < endPosition.X - nodeInterval.X; x += Rand.Range(nodeInterval.X, nodeInterval.Y, Rand.RandSync.ServerAndClient))
			{
				Point nodePos = new Point(x, Rand.Range(pathBorders.Y, pathBorders.Bottom, Rand.RandSync.ServerAndClient));
				if (pathNodes.Count > 2 || parentTunnel != null)
				{
					nodePos.Y = (int)MathHelper.Clamp((float)nodePos.Y, (float)pathNodes.Last<Point>().Y - (float)pathBorders.Height * variance * 0.5f, (float)pathNodes.Last<Point>().Y + (float)pathBorders.Height * variance * 0.5f);
				}
				if (pathNodes.Count == 1)
				{
					nodePos.Y = startPosition.Y + Math.Abs(nodePos.Y - startPosition.Y) * -Math.Sign(nodePos.Y - pathBorders.Center.Y);
					nodePos.Y = MathHelper.Clamp(nodePos.Y, pathBorders.Y, pathBorders.Bottom);
				}
				foreach (Level.Tunnel tunnel in this.Tunnels)
				{
					int i = 1;
					while (i < tunnel.Nodes.Count)
					{
						Point node = tunnel.Nodes[i - 1];
						Point node2 = tunnel.Nodes[i];
						if (node.X < nodePos.X && node2.X > pathNodes.Last<Point>().X && !MathUtils.NearlyEqual((float)node.X, (float)pathNodes.Last<Point>().X, 0.0001f) && (Math.Abs(node.Y - nodePos.Y) <= tunnel.MinWidth || Math.Abs(node2.Y - nodePos.Y) <= tunnel.MinWidth || MathUtils.LineSegmentsIntersect(node.ToVector2(), node2.ToVector2(), pathNodes.Last<Point>().ToVector2(), nodePos.ToVector2())))
						{
							if (nodePos.Y < pathNodes.Last<Point>().Y)
							{
								nodePos.Y = Math.Min(Math.Max(node.Y, node2.Y) + tunnel.MinWidth * 2, pathBorders.Bottom);
								break;
							}
							nodePos.Y = Math.Max(Math.Min(node.Y, node2.Y) - tunnel.MinWidth * 2, pathBorders.Y);
							break;
						}
						else
						{
							i++;
						}
					}
				}
				pathNodes.Add(nodePos);
			}
			if (pathNodes.Count == 1)
			{
				pathNodes.Add(new Point(pathBorders.Center.X, pathBorders.Y));
			}
			pathNodes.Add(endPosition);
			return pathNodes;
		}

		// Token: 0x060008CA RID: 2250 RVA: 0x000539FC File Offset: 0x00051BFC
		private List<VoronoiCell> CreateHoles(float holeProbability, Rectangle limits, int submarineSize)
		{
			List<VoronoiCell> toBeRemoved = new List<VoronoiCell>();
			foreach (VoronoiCell cell in this.cells)
			{
				if (!cell.Edges.Any((GraphEdge e) => e.NextToCave) && Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient) <= holeProbability && limits.Contains(cell.Site.Coord.X, cell.Site.Coord.Y))
				{
					float closestDist = 0f;
					Point? closestTunnelNode = null;
					foreach (Level.Tunnel tunnel in this.Tunnels)
					{
						foreach (Point node in tunnel.Nodes)
						{
							float dist = Math.Abs(cell.Center.X - (float)node.X);
							if (closestTunnelNode == null || dist < closestDist)
							{
								closestDist = dist;
								closestTunnelNode = new Point?(node);
							}
						}
					}
					if (closestTunnelNode == null || (float)closestTunnelNode.Value.Y >= cell.Center.Y)
					{
						toBeRemoved.Add(cell);
					}
				}
			}
			return toBeRemoved;
		}

		// Token: 0x060008CB RID: 2251 RVA: 0x00053BD4 File Offset: 0x00051DD4
		private void EnlargePath(List<VoronoiCell> pathCells, float minWidth)
		{
			if (minWidth <= 0f)
			{
				return;
			}
			List<VoronoiCell> removedCells = this.GetTooCloseCells(pathCells, minWidth);
			foreach (VoronoiCell removedCell in removedCells)
			{
				if (removedCell.CellType != CellType.Path)
				{
					pathCells.Add(removedCell);
					removedCell.CellType = CellType.Path;
				}
			}
		}

		// Token: 0x060008CC RID: 2252 RVA: 0x00053C44 File Offset: 0x00051E44
		private void GenerateWaypoints(Level.Tunnel tunnel, Level.Tunnel parentTunnel)
		{
			if (tunnel.Cells.Count == 0)
			{
				return;
			}
			List<WayPoint> wayPoints = new List<WayPoint>();
			WayPoint prevWayPoint = null;
			int i;
			Predicate<GraphEdge> <>9__0;
			int i2;
			for (i = 0; i < tunnel.Cells.Count; i = i2 + 1)
			{
				tunnel.Cells[i].CellType = CellType.Path;
				WayPoint newWaypoint = new WayPoint(new Rectangle((int)tunnel.Cells[i].Site.Coord.X, (int)tunnel.Cells[i].Center.Y, 10, 10), null)
				{
					Tunnel = tunnel
				};
				wayPoints.Add(newWaypoint);
				if (prevWayPoint != null)
				{
					bool solidCellBetween = false;
					foreach (GraphEdge edge in tunnel.Cells[i].Edges)
					{
						VoronoiCell voronoiCell = edge.AdjacentCell(tunnel.Cells[i]);
						if (voronoiCell != null && voronoiCell.CellType == CellType.Solid && MathUtils.LineSegmentsIntersect(newWaypoint.WorldPosition, prevWayPoint.WorldPosition, edge.Point1, edge.Point2))
						{
							solidCellBetween = true;
							break;
						}
					}
					if (solidCellBetween)
					{
						List<GraphEdge> edges = tunnel.Cells[i].Edges;
						Predicate<GraphEdge> match;
						if ((match = <>9__0) == null)
						{
							match = (<>9__0 = ((GraphEdge e) => e.AdjacentCell(tunnel.Cells[i]) == tunnel.Cells[i - 1]));
						}
						GraphEdge edgeBetweenCells = edges.Find(match);
						if (edgeBetweenCells != null)
						{
							WayPoint edgeWaypoint = new WayPoint(new Rectangle((int)edgeBetweenCells.Center.X, (int)edgeBetweenCells.Center.Y, 10, 10), null)
							{
								Tunnel = tunnel
							};
							prevWayPoint.ConnectTo(edgeWaypoint);
							prevWayPoint = edgeWaypoint;
						}
					}
					prevWayPoint.ConnectTo(newWaypoint);
					int j = i - 2;
					while (j > 0 && j > i - 5)
					{
						foreach (GraphEdge edge2 in tunnel.Cells[i].Edges)
						{
							if (Vector2.DistanceSquared(edge2.Point1, edge2.Point2) >= 900f && !edge2.IsSolid && edge2.AdjacentCell(tunnel.Cells[i]) == tunnel.Cells[j])
							{
								WayPoint edgeWaypoint2 = new WayPoint(new Rectangle((int)edge2.Center.X, (int)edge2.Center.Y, 10, 10), null)
								{
									Tunnel = tunnel
								};
								wayPoints[j].ConnectTo(edgeWaypoint2);
								edgeWaypoint2.ConnectTo(newWaypoint);
								break;
							}
						}
						j--;
					}
				}
				prevWayPoint = newWaypoint;
				i2 = i;
			}
			tunnel.WayPoints.AddRange(wayPoints);
			if (parentTunnel != null)
			{
				WayPoint parentStart = Level.FindClosestWayPoint(wayPoints.First<WayPoint>().WorldPosition, parentTunnel);
				if (parentStart != null)
				{
					wayPoints.First<WayPoint>().ConnectTo(parentStart);
				}
				if (tunnel.Type != Level.TunnelType.Cave || tunnel.ParentTunnel.Type == Level.TunnelType.Cave)
				{
					WayPoint parentEnd = Level.FindClosestWayPoint(wayPoints.Last<WayPoint>().WorldPosition, parentTunnel);
					if (parentEnd != null)
					{
						wayPoints.Last<WayPoint>().ConnectTo(parentEnd);
					}
				}
			}
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x0005402C File Offset: 0x0005222C
		private void ConnectWaypoints(Level.Tunnel tunnel, Level.Tunnel parentTunnel)
		{
			Action<WayPoint> <>9__0;
			foreach (WayPoint wayPoint in tunnel.WayPoints)
			{
				WayPoint closestWaypoint = Level.FindClosestWayPoint(wayPoint.WorldPosition, parentTunnel);
				if (closestWaypoint != null && Submarine.PickBody(ConvertUnits.ToSimUnits(wayPoint.WorldPosition), ConvertUnits.ToSimUnits(closestWaypoint.WorldPosition), null, new Category?(Category.Cat1 | Category.Cat8), true, null, false) == null)
				{
					float step = ConvertUnits.ToDisplayUnits(30f) * 0.8f;
					List<WayPoint> list = this.ConnectWaypoints(wayPoint, closestWaypoint, step);
					Action<WayPoint> action;
					if ((action = <>9__0) == null)
					{
						action = (<>9__0 = delegate(WayPoint wp)
						{
							wp.Tunnel = tunnel;
						});
					}
					list.ForEach(action);
				}
			}
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x00054110 File Offset: 0x00052310
		private List<WayPoint> ConnectWaypoints(WayPoint wp1, WayPoint wp2, float interval)
		{
			List<WayPoint> newWaypoints = new List<WayPoint>();
			Vector2 diff = wp2.WorldPosition - wp1.WorldPosition;
			float dist = diff.Length();
			WayPoint prevWaypoint = wp1;
			for (float x = interval; x < dist - interval; x += interval)
			{
				WayPoint newWaypoint = new WayPoint(wp1.WorldPosition + diff / dist * x, SpawnType.Path, null, null);
				prevWaypoint.ConnectTo(newWaypoint);
				prevWaypoint = newWaypoint;
				newWaypoints.Add(newWaypoint);
			}
			prevWaypoint.ConnectTo(wp2);
			return newWaypoints;
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x0005418F File Offset: 0x0005238F
		private static WayPoint FindClosestWayPoint(Vector2 worldPosition, Level.Tunnel otherTunnel)
		{
			return Level.FindClosestWayPoint(worldPosition, otherTunnel.WayPoints, null);
		}

		// Token: 0x060008D0 RID: 2256 RVA: 0x000541A0 File Offset: 0x000523A0
		private static WayPoint FindClosestWayPoint(Vector2 worldPosition, IEnumerable<WayPoint> waypoints, Func<WayPoint, bool> filter = null)
		{
			float closestDist = float.PositiveInfinity;
			WayPoint closestWayPoint = null;
			foreach (WayPoint otherWayPoint in waypoints)
			{
				float dist = Vector2.DistanceSquared(otherWayPoint.WorldPosition, worldPosition);
				if (dist < closestDist && (filter == null || filter(otherWayPoint)))
				{
					closestDist = dist;
					closestWayPoint = otherWayPoint;
				}
			}
			return closestWayPoint;
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x00054210 File Offset: 0x00052410
		private List<VoronoiCell> GetTooCloseCells(List<VoronoiCell> emptyCells, float minDistance)
		{
			List<VoronoiCell> tooCloseCells = new List<VoronoiCell>();
			if (minDistance <= 0f)
			{
				return tooCloseCells;
			}
			foreach (VoronoiCell cell in emptyCells.Distinct<VoronoiCell>())
			{
				foreach (VoronoiCell tooCloseCell in this.GetTooCloseCells(cell.Center, minDistance))
				{
					if (!tooCloseCells.Contains(tooCloseCell))
					{
						tooCloseCells.Add(tooCloseCell);
					}
				}
			}
			return tooCloseCells;
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x000542BC File Offset: 0x000524BC
		public List<VoronoiCell> GetTooCloseCells(Vector2 position, float minDistance)
		{
			HashSet<VoronoiCell> tooCloseCells = new HashSet<VoronoiCell>();
			List<VoronoiCell> closeCells = this.GetCells(position, Math.Max((int)Math.Ceiling((double)(minDistance / 2000f)), 3));
			float minDistSqr = minDistance * minDistance;
			foreach (VoronoiCell cell in closeCells)
			{
				bool tooClose = false;
				if (cell.IsPointInside(position))
				{
					tooClose = true;
				}
				else
				{
					foreach (GraphEdge edge in cell.Edges)
					{
						if (Vector2.DistanceSquared(edge.Point1, position) < minDistSqr || Vector2.DistanceSquared(edge.Point2, position) < minDistSqr || MathUtils.LineSegmentToPointDistanceSquared(edge.Point1.ToPoint(), edge.Point2.ToPoint(), position.ToPoint()) < (double)minDistSqr)
						{
							tooClose = true;
							break;
						}
					}
				}
				if (tooClose)
				{
					tooCloseCells.Add(cell);
				}
			}
			return tooCloseCells.ToList<VoronoiCell>();
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x000543E0 File Offset: 0x000525E0
		private void GenerateAbyssPositions()
		{
			int count = 10;
			for (int i = 0; i < count; i++)
			{
				float xPos = MathHelper.Lerp((float)this.borders.X, (float)this.borders.Right, (float)i / (float)(count - 1));
				float seaFloorPos = this.GetBottomPosition(xPos).Y;
				if (seaFloorPos <= (float)this.AbyssStart)
				{
					float yPos = MathHelper.Lerp((float)this.AbyssStart, Math.Max(seaFloorPos, (float)this.AbyssArea.Y), Rand.Range(0.2f, 1f, Rand.RandSync.ServerAndClient));
					foreach (Level.AbyssIsland abyssIsland in this.AbyssIslands)
					{
						if (abyssIsland.Area.Contains(new Point((int)xPos, (int)yPos)))
						{
							xPos = (float)(abyssIsland.Area.Center.X + (int)((Rand.Int(1, Rand.RandSync.ServerAndClient) == 0) ? ((float)abyssIsland.Area.Width * -0.6f) : 0.6f));
						}
					}
					this.PositionsOfInterest.Add(new Level.InterestingPosition(new Point((int)xPos, (int)yPos), Level.PositionType.Abyss, null, true));
				}
			}
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x00054524 File Offset: 0x00052724
		private void GenerateAbyssArea()
		{
			int abyssStartY = this.borders.Y - 5000;
			int abyssEndY = Math.Max(abyssStartY - 100000, this.BottomPos + 1000);
			int abyssHeight = abyssStartY - abyssEndY;
			if (abyssHeight < 0)
			{
				abyssStartY = this.borders.Y;
				abyssEndY = this.BottomPos;
				if (abyssStartY - abyssEndY < 1000)
				{
					DebugConsole.AddWarning("Not enough space to generate Abyss in the level. You may want to move the ocean floor deeper.", null);
				}
			}
			else if (abyssHeight > 30000)
			{
				if ((float)abyssEndY + this.CrushDepth < 0f && (float)abyssStartY > -this.CrushDepth)
				{
					abyssEndY += Math.Min(-(abyssEndY + (int)this.CrushDepth), abyssHeight / 2);
				}
				if (abyssStartY - abyssEndY < 10000)
				{
					abyssStartY = this.borders.Y;
				}
			}
			this.AbyssArea = new Rectangle(this.borders.X, abyssEndY, this.borders.Width, abyssStartY - abyssEndY);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x00054604 File Offset: 0x00052804
		private void GenerateAbyssGeometry()
		{
			this.ResetRandomSeed();
			Voronoi voronoi = new Voronoi(1.0);
			Point siteInterval = new Point(500, 500);
			Point siteVariance = new Point(200, 200);
			Point islandSize = Vector2.Lerp(this.GenerationParams.AbyssIslandSizeMin.ToVector2(), this.GenerationParams.AbyssIslandSizeMax.ToVector2(), Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient)).ToPoint();
			if (this.AbyssArea.Height < islandSize.Y)
			{
				return;
			}
			int createdCaves = 0;
			int islandCount = this.GenerationParams.AbyssIslandCount;
			for (int i = 0; i < islandCount; i++)
			{
				Point islandPosition = Point.Zero;
				Rectangle islandArea = new Rectangle(islandPosition, islandSize);
				int tries = 0;
				do
				{
					islandPosition = new Point(Rand.Range(this.AbyssArea.X, this.AbyssArea.Right - islandSize.X, Rand.RandSync.ServerAndClient), Rand.Range(this.AbyssArea.Y, this.AbyssArea.Bottom - islandSize.Y, Rand.RandSync.ServerAndClient));
					islandPosition.Y = Math.Max(islandPosition.Y, (int)this.GetBottomPosition((float)islandPosition.X).Y + 500);
					islandPosition.Y = Math.Max(islandPosition.Y, (int)this.GetBottomPosition((float)(islandPosition.X + islandArea.Width)).Y + 500);
					islandArea.Location = islandPosition;
					tries++;
				}
				while ((this.AbyssIslands.Any((Level.AbyssIsland island) => island.Area.Intersects(islandArea)) || islandArea.Bottom > this.AbyssArea.Bottom) && tries < 20);
				if (tries >= 20)
				{
					break;
				}
				if ((i != islandCount - 1 || createdCaves != 0) && Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient) >= this.GenerationParams.AbyssIslandCaveProbability)
				{
					float radiusVariance = (float)Math.Min(islandArea.Width, islandArea.Height) * 0.1f;
					List<Vector2> vertices = CaveGenerator.CreateRandomChunk((float)(islandArea.Width - (int)(radiusVariance * 2f)), (float)(islandArea.Height - (int)(radiusVariance * 2f)), 16, radiusVariance);
					Vector2 position = islandArea.Center.ToVector2();
					for (int j = 0; j < vertices.Count; j++)
					{
						List<Vector2> list = vertices;
						int index = j;
						list[index] += position;
					}
					LevelWall newChunk = new LevelWall(vertices, this.GenerationParams.WallColor, this, false, false);
					this.AbyssIslands.Add(new Level.AbyssIsland(islandArea, newChunk.Cells));
				}
				else
				{
					List<double> siteCoordsX = new List<double>(islandSize.Y / siteInterval.Y * (islandSize.X / siteInterval.Y));
					List<double> siteCoordsY = new List<double>(islandSize.Y / siteInterval.Y * (islandSize.X / siteInterval.Y));
					for (int x = islandArea.X; x < islandArea.Right; x += siteInterval.X)
					{
						for (int y = islandArea.Y; y < islandArea.Bottom; y += siteInterval.Y)
						{
							siteCoordsX.Add((double)(x + Rand.Range(-siteVariance.X, siteVariance.X, Rand.RandSync.ServerAndClient)));
							siteCoordsY.Add((double)(y + Rand.Range(-siteVariance.Y, siteVariance.Y, Rand.RandSync.ServerAndClient)));
						}
					}
					List<GraphEdge> graphEdges = voronoi.MakeVoronoiGraph(siteCoordsX.ToArray(), siteCoordsY.ToArray(), islandArea);
					List<VoronoiCell>[,] cellGrid;
					List<VoronoiCell> islandCells = CaveGenerator.GraphEdgesToCells(graphEdges, islandArea, 2000f, out cellGrid);
					int k = islandCells.Count - 1;
					Func<GraphEdge, bool> <>9__1;
					Func<GraphEdge, bool> <>9__2;
					Func<GraphEdge, bool> <>9__3;
					Func<GraphEdge, bool> <>9__4;
					while (k >= 0)
					{
						VoronoiCell cell = islandCells[k];
						double xDiff = (cell.Site.Coord.X - (double)islandArea.Center.X) / ((double)islandArea.Width * 0.5);
						double yDiff = (cell.Site.Coord.Y - (double)islandArea.Center.Y) / ((double)islandArea.Height * 0.5);
						if (yDiff < 0.0)
						{
							xDiff += xDiff * Math.Abs(yDiff);
						}
						double normalizedDist = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
						if (normalizedDist > 0.95)
						{
							goto IL_5AC;
						}
						IEnumerable<GraphEdge> edges = cell.Edges;
						Func<GraphEdge, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((GraphEdge e) => MathUtils.NearlyEqual(e.Point1.X, (float)islandArea.X, 0.0001f)));
						}
						if (edges.Any(predicate))
						{
							goto IL_5AC;
						}
						IEnumerable<GraphEdge> edges2 = cell.Edges;
						Func<GraphEdge, bool> predicate2;
						if ((predicate2 = <>9__2) == null)
						{
							predicate2 = (<>9__2 = ((GraphEdge e) => MathUtils.NearlyEqual(e.Point1.X, (float)islandArea.Right, 0.0001f)));
						}
						if (edges2.Any(predicate2))
						{
							goto IL_5AC;
						}
						IEnumerable<GraphEdge> edges3 = cell.Edges;
						Func<GraphEdge, bool> predicate3;
						if ((predicate3 = <>9__3) == null)
						{
							predicate3 = (<>9__3 = ((GraphEdge e) => MathUtils.NearlyEqual(e.Point1.Y, (float)islandArea.Y, 0.0001f)));
						}
						if (edges3.Any(predicate3))
						{
							goto IL_5AC;
						}
						IEnumerable<GraphEdge> edges4 = cell.Edges;
						Func<GraphEdge, bool> predicate4;
						if ((predicate4 = <>9__4) == null)
						{
							predicate4 = (<>9__4 = ((GraphEdge e) => MathUtils.NearlyEqual(e.Point1.Y, (float)islandArea.Bottom, 0.0001f)));
						}
						if (edges4.Any(predicate4))
						{
							goto IL_5AC;
						}
						IL_5C4:
						k--;
						continue;
						IL_5AC:
						islandCells[k].CellType = CellType.Removed;
						islandCells.RemoveAt(k);
						goto IL_5C4;
					}
					CaveGenerationParams caveParams = CaveGenerationParams.GetRandom(this, true, Rand.RandSync.ServerAndClient);
					float caveScaleRelativeToIsland = 0.7f;
					this.GenerateCave(caveParams, this.Tunnels.First<Level.Tunnel>(), new Point(islandArea.Center.X, islandArea.Center.Y + (int)((float)islandArea.Size.Y * (1f - caveScaleRelativeToIsland)) / 2), new Point((int)((float)islandArea.Size.X * caveScaleRelativeToIsland), (int)((float)islandArea.Size.Y * caveScaleRelativeToIsland)));
					this.AbyssIslands.Add(new Level.AbyssIsland(islandArea, islandCells));
					createdCaves++;
				}
			}
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x00054CAC File Offset: 0x00052EAC
		private void GenerateSeaFloorPositions()
		{
			this.ResetRandomSeed();
			this.BottomPos = this.GenerationParams.SeaFloorDepth;
			this.SeaFloorTopPos = this.BottomPos;
			this.bottomPositions = new List<Point>
			{
				new Point(0, this.BottomPos)
			};
			int mountainCount = Rand.Range(this.GenerationParams.MountainCountMin, this.GenerationParams.MountainCountMax + 1, Rand.RandSync.ServerAndClient);
			for (int i = 0; i < mountainCount; i++)
			{
				this.bottomPositions.Add(new Point(this.Size.X / (mountainCount + 1) * (i + 1), this.BottomPos + Rand.Range(this.GenerationParams.MountainHeightMin, this.GenerationParams.MountainHeightMax + 1, Rand.RandSync.ServerAndClient)));
			}
			this.bottomPositions.Add(new Point(this.Size.X, this.BottomPos));
			int minVertexInterval = 5000;
			for (float currInverval = (float)(this.Size.X / 2); currInverval > (float)minVertexInterval; currInverval /= 2f)
			{
				for (int j = 0; j < this.bottomPositions.Count - 1; j++)
				{
					this.bottomPositions.Insert(j + 1, new Point((this.bottomPositions[j].X + this.bottomPositions[j + 1].X) / 2, (this.bottomPositions[j].Y + this.bottomPositions[j + 1].Y) / 2 + Rand.Range(0, this.GenerationParams.SeaFloorVariance + 1, Rand.RandSync.ServerAndClient)));
					j++;
				}
			}
			this.SeaFloorTopPos = this.bottomPositions.Max((Point p) => p.Y);
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x00054E8C File Offset: 0x0005308C
		private void GenerateSeaFloor()
		{
			this.SeaFloor = new LevelWall((from p in this.bottomPositions
			select p.ToVector2()).ToList<Vector2>(), new Vector2(0f, -2000f), this.GenerationParams.WallColor, this);
			this.ExtraWalls.Add(this.SeaFloor);
			this.BottomBarrier = GameMain.World.CreateEdge(ConvertUnits.ToSimUnits(new Vector2((float)this.borders.X, 0f)), ConvertUnits.ToSimUnits(new Vector2((float)this.borders.Right, 0f)), BodyType.Static, Category.Cat1, Category.All, true);
			this.BottomBarrier.UserData = "bottombarrier";
			this.BottomBarrier.SetTransform(ConvertUnits.ToSimUnits(new Vector2(0f, (float)this.BottomPos)), 0f);
			this.BottomBarrier.BodyType = BodyType.Static;
			this.BottomBarrier.CollisionCategories = Category.Cat8;
			this.bodies.Add(this.BottomBarrier);
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x00054FB4 File Offset: 0x000531B4
		private void GenerateCaves(Level.Tunnel parentTunnel)
		{
			this.ResetRandomSeed();
			for (int i = 0; i < this.GenerationParams.CaveCount; i++)
			{
				CaveGenerationParams caveParams = CaveGenerationParams.GetRandom(this, false, Rand.RandSync.ServerAndClient);
				Point caveSize = new Point(Rand.Range(caveParams.MinWidth, caveParams.MaxWidth, Rand.RandSync.ServerAndClient), Rand.Range(caveParams.MinHeight, caveParams.MaxHeight, Rand.RandSync.ServerAndClient));
				int padding = (int)((float)caveSize.X * 1.2f);
				Rectangle allowedArea = new Rectangle(padding, padding, this.Size.X - padding * 2, this.Size.Y - padding * 2);
				int radius = Math.Max(caveSize.X, caveSize.Y) / 2;
				Point cavePos = this.FindPosAwayFromMainPath((double)((float)(parentTunnel.MinWidth + radius) * 1.25f), true, new Rectangle?(allowedArea));
				this.GenerateCave(caveParams, parentTunnel, cavePos, caveSize);
				this.CalculateTunnelDistanceField(null);
			}
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x00055098 File Offset: 0x00053298
		private void GenerateCave(CaveGenerationParams caveParams, Level.Tunnel parentTunnel, Point cavePos, Point caveSize)
		{
			Rectangle caveArea = new Rectangle(cavePos - new Point(caveSize.X / 2, caveSize.Y / 2), caveSize);
			Point closestParentNode = parentTunnel.Nodes.First<Point>();
			double closestDist = double.PositiveInfinity;
			foreach (Point node in parentTunnel.Nodes)
			{
				if (!caveArea.Contains(node))
				{
					double dist = MathUtils.DistanceSquared((double)node.X, (double)node.Y, (double)cavePos.X, (double)cavePos.Y);
					if (dist < closestDist)
					{
						closestParentNode = node;
						closestDist = dist;
					}
				}
			}
			Vector2 caveStartPosVector;
			if (!MathUtils.GetLineWorldRectangleIntersection(closestParentNode.ToVector2(), cavePos.ToVector2(), new Rectangle(caveArea.X, caveArea.Y + caveArea.Height, caveArea.Width, caveArea.Height), out caveStartPosVector))
			{
				caveStartPosVector = caveArea.Location.ToVector2();
			}
			Point caveStartPos = caveStartPosVector.ToPoint();
			Point caveEndPos = cavePos - (caveStartPos - cavePos);
			Level.Cave cave = new Level.Cave(caveParams, caveArea, caveStartPos, caveEndPos);
			this.Caves.Add(cave);
			Vector2 start = caveStartPos.ToVector2();
			Vector2 end = caveEndPos.ToVector2();
			int iterations = 3;
			float offsetAmount = Vector2.Distance(caveStartPos.ToVector2(), caveEndPos.ToVector2()) * 0.75f;
			Rectangle? bounds = new Rectangle?(caveArea);
			List<Vector2[]> caveSegments = MathUtils.GenerateJaggedLine(start, end, iterations, offsetAmount, Rand.GetRNG(Rand.RandSync.ServerAndClient), bounds);
			if (!caveSegments.Any<Vector2[]>())
			{
				return;
			}
			List<Level.Tunnel> caveBranches = new List<Level.Tunnel>();
			Level.Tunnel tunnel = new Level.Tunnel(Level.TunnelType.Cave, Level.<GenerateCave>g__SegmentsToNodes|215_0(caveSegments), 150, parentTunnel);
			this.Tunnels.Add(tunnel);
			caveBranches.Add(tunnel);
			int branches = Rand.Range(caveParams.MinBranchCount, caveParams.MaxBranchCount + 1, Rand.RandSync.ServerAndClient);
			for (int i = 0; i < branches; i++)
			{
				Level.Tunnel parentBranch = caveBranches.GetRandom(Rand.RandSync.ServerAndClient);
				Vector2 branchStartPos = parentBranch.Nodes[Rand.Int(parentBranch.Nodes.Count / 2, Rand.RandSync.ServerAndClient)].ToVector2();
				Vector2 branchEndPos = parentBranch.Nodes[Rand.Range(parentBranch.Nodes.Count / 2, parentBranch.Nodes.Count, Rand.RandSync.ServerAndClient)].ToVector2();
				Vector2 start2 = branchStartPos;
				Vector2 end2 = branchEndPos;
				int iterations2 = 3;
				float offsetAmount2 = Vector2.Distance(branchStartPos, branchEndPos) * 0.75f;
				bounds = new Rectangle?(caveArea);
				List<Vector2[]> branchSegments = MathUtils.GenerateJaggedLine(start2, end2, iterations2, offsetAmount2, Rand.GetRNG(Rand.RandSync.ServerAndClient), bounds);
				if (branchSegments.Any<Vector2[]>())
				{
					Level.Tunnel branch = new Level.Tunnel(Level.TunnelType.Cave, Level.<GenerateCave>g__SegmentsToNodes|215_0(branchSegments), 150, parentBranch);
					this.Tunnels.Add(branch);
					caveBranches.Add(branch);
				}
			}
			foreach (Level.Tunnel branch2 in caveBranches)
			{
				Point node2 = branch2.Nodes.Last<Point>();
				this.PositionsOfInterest.Add(new Level.InterestingPosition(node2, (node2.Y < this.AbyssArea.Bottom) ? Level.PositionType.AbyssCave : Level.PositionType.Cave, cave, true));
				cave.Tunnels.Add(branch2);
			}
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x000553DC File Offset: 0x000535DC
		private void GenerateRuin(Point ruinPos, bool mirror, bool requireMissionReadyRuin)
		{
			Level.<>c__DisplayClass216_0 CS$<>8__locals1 = new Level.<>c__DisplayClass216_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.ruinGenerationParams = null;
			if (this.LevelData.ForceRuinGenerationParams != null)
			{
				CS$<>8__locals1.ruinGenerationParams = this.LevelData.ForceRuinGenerationParams;
			}
			else
			{
				IEnumerable<RuinGenerationParams> possibleRuinGenerationParams = RuinGenerationParams.RuinParams;
				if (requireMissionReadyRuin)
				{
					possibleRuinGenerationParams = from p in possibleRuinGenerationParams
					where p.IsMissionReady
					select p;
				}
				if (possibleRuinGenerationParams.Multiple(null))
				{
					possibleRuinGenerationParams = (from p in possibleRuinGenerationParams
					orderby p.UintIdentifier descending
					select p).ThenByDescending(new Func<RuinGenerationParams, float>(CS$<>8__locals1.<GenerateRuin>g__GetWeight|0)).Take((int)Math.Max(Math.Round((double)((float)possibleRuinGenerationParams.Count<RuinGenerationParams>() / 4f)), 1.0));
				}
				CS$<>8__locals1.ruinGenerationParams = possibleRuinGenerationParams.GetRandomByWeight(new Func<RuinGenerationParams, float>(CS$<>8__locals1.<GenerateRuin>g__GetWeight|0), Rand.RandSync.ServerAndClient);
				if (CS$<>8__locals1.ruinGenerationParams == null)
				{
					DebugConsole.ThrowError("Failed to generate alien ruins. Could not find any RuinGenerationParameters!", null, null, false, false);
					return;
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Creating alien ruins using ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.ruinGenerationParams.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(" (preferred difficulty: ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(CS$<>8__locals1.ruinGenerationParams.PreferredDifficulty);
			defaultInterpolatedStringHandler.AppendLiteral(", current difficulty ");
			defaultInterpolatedStringHandler.AppendFormatted<float>(this.Difficulty);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), true);
			Location startLocation = this.StartLocation;
			LocationType locationType = (startLocation != null) ? startLocation.Type : null;
			if (locationType == null)
			{
				locationType = LocationType.Prefabs.GetRandom(Rand.RandSync.ServerAndClient);
				if (CS$<>8__locals1.ruinGenerationParams.AllowedLocationTypes.Any<Identifier>())
				{
					locationType = (from lt in LocationType.Prefabs
					where CS$<>8__locals1.ruinGenerationParams.AllowedLocationTypes.Any((Identifier allowedType) => allowedType == "any" || lt.Identifier == allowedType)
					select lt).GetRandom(Rand.RandSync.ServerAndClient);
				}
			}
			Ruin ruin = new Ruin(this, CS$<>8__locals1.ruinGenerationParams, locationType, ruinPos, mirror);
			if (ruin.Submarine != null)
			{
				this.SetLinkedSubCrushDepth(ruin.Submarine);
			}
			this.Ruins.Add(ruin);
			List<VoronoiCell> tooClose = this.GetTooCloseCells(ruinPos.ToVector2(), (float)(Math.Max(ruin.Area.Width, ruin.Area.Height) * 4));
			using (List<VoronoiCell>.Enumerator enumerator = tooClose.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					VoronoiCell cell = enumerator.Current;
					if (cell.CellType != CellType.Empty && !this.ExtraWalls.Any((LevelWall w) => w.Cells.Contains(cell)))
					{
						foreach (GraphEdge e in cell.Edges)
						{
							Vector2 vector;
							if (ruin.Area.Contains(e.Point1) || ruin.Area.Contains(e.Point2) || MathUtils.GetLineRectangleIntersection(e.Point1, e.Point2, ruin.Area, out vector))
							{
								cell.CellType = CellType.Removed;
								for (int x = 0; x < this.cellGrid.GetLength(0); x++)
								{
									for (int y = 0; y < this.cellGrid.GetLength(1); y++)
									{
										this.cellGrid[x, y].Remove(cell);
									}
								}
								this.cells.Remove(cell);
								break;
							}
						}
					}
				}
			}
			ruin.PathCells = this.CreatePathToClosestTunnel(ruin.Area.Center);
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x000557D8 File Offset: 0x000539D8
		private void GenerateRuinWayPoints(Ruin ruin)
		{
			List<VoronoiCell> tooClose = this.GetTooCloseCells(ruin.Area.Center.ToVector2(), (float)(Math.Max(ruin.Area.Width, ruin.Area.Height) * 6));
			List<WayPoint> wayPoints = new List<WayPoint>();
			float outSideWaypointInterval = 500f;
			WayPoint[,] cornerWaypoint = new WayPoint[2, 2];
			Rectangle waypointArea = ruin.Area;
			waypointArea.Inflate(100, 100);
			for (int l = 0; l < 2; l++)
			{
				for (float x = (float)waypointArea.X + outSideWaypointInterval; x < (float)waypointArea.Right - outSideWaypointInterval; x += outSideWaypointInterval)
				{
					WayPoint wayPoint = new WayPoint(new Vector2(x, (float)(waypointArea.Y + waypointArea.Height * l)), SpawnType.Path, null, null)
					{
						Ruin = ruin
					};
					wayPoints.Add(wayPoint);
					if (x == (float)waypointArea.X + outSideWaypointInterval)
					{
						cornerWaypoint[l, 0] = wayPoint;
					}
					else
					{
						wayPoint.ConnectTo(wayPoints[wayPoints.Count - 2]);
					}
				}
				cornerWaypoint[l, 1] = wayPoints[wayPoints.Count - 1];
			}
			for (int j = 0; j < 2; j++)
			{
				WayPoint wayPoint2 = null;
				for (float y = (float)waypointArea.Y; y < (float)(waypointArea.Y + waypointArea.Height); y += outSideWaypointInterval)
				{
					wayPoint2 = new WayPoint(new Vector2((float)(waypointArea.X + waypointArea.Width * j), y), SpawnType.Path, null, null)
					{
						Ruin = ruin
					};
					wayPoints.Add(wayPoint2);
					if (y == (float)waypointArea.Y)
					{
						wayPoint2.ConnectTo(cornerWaypoint[0, j]);
					}
					else
					{
						wayPoint2.ConnectTo(wayPoints[wayPoints.Count - 2]);
					}
				}
				wayPoint2.ConnectTo(cornerWaypoint[1, j]);
			}
			for (int k = wayPoints.Count - 1; k >= 0; k--)
			{
				WayPoint wp = wayPoints[k];
				VoronoiCell overlappingCell = tooClose.Find((VoronoiCell c) => c.CellType != CellType.Removed && c.IsPointInside(wp.WorldPosition));
				if (overlappingCell != null)
				{
					if (wp.linkedTo.Count > 1)
					{
						WayPoint linked = wp.linkedTo[0] as WayPoint;
						WayPoint linked2 = wp.linkedTo[1] as WayPoint;
						linked.ConnectTo(linked2);
					}
					wp.Remove();
					wayPoints.RemoveAt(k);
				}
			}
			using (List<Gap>.Enumerator enumerator = Gap.GapList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Gap g = enumerator.Current;
					if (g.Submarine == ruin.Submarine && !g.IsRoomToRoom && g.linkedTo.Count != 0)
					{
						WayPoint gapWaypoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.ConnectedGap == g);
						if (gapWaypoint != null)
						{
							Vector2 entranceDir = Vector2.Zero;
							if (g.IsHorizontal)
							{
								entranceDir = Vector2.UnitX * 2f * (float)Math.Sign(g.WorldPosition.X - g.linkedTo[0].WorldPosition.X);
							}
							else
							{
								entranceDir = Vector2.UnitY * 2f * (float)Math.Sign(g.WorldPosition.Y - g.linkedTo[0].WorldPosition.Y);
							}
							WayPoint entranceWayPoint = new WayPoint(g.WorldPosition + entranceDir * 64f, SpawnType.Path, null, null)
							{
								Ruin = ruin
							};
							entranceWayPoint.ConnectTo(gapWaypoint);
							WayPoint closestWp = Level.FindClosestWayPoint(entranceWayPoint.WorldPosition, wayPoints, (WayPoint wp) => Submarine.PickBody(ConvertUnits.ToSimUnits(wp.WorldPosition), ConvertUnits.ToSimUnits(entranceWayPoint.WorldPosition), null, new Category?(Category.Cat1 | Category.Cat8), true, null, false) == null);
							if (closestWp != null)
							{
								this.ConnectWaypoints(entranceWayPoint, closestWp, outSideWaypointInterval);
							}
						}
					}
				}
			}
			WayPoint prevWp = Level.FindClosestWayPoint(ruin.PathCells.First<VoronoiCell>().Center, wayPoints, (WayPoint wp) => Submarine.PickBody(ConvertUnits.ToSimUnits(wp.WorldPosition), ConvertUnits.ToSimUnits(ruin.PathCells.First<VoronoiCell>().Center), null, new Category?(Category.Cat1 | Category.Cat8), true, null, false) == null);
			if (prevWp != null)
			{
				int i2;
				int i;
				for (i = 0; i < ruin.PathCells.Count; i = i2 + 1)
				{
					GraphEdge connectingEdge = (i > 0) ? ruin.PathCells[i].Edges.Find((GraphEdge e) => e.AdjacentCell(ruin.PathCells[i]) == ruin.PathCells[i - 1]) : null;
					if (connectingEdge != null)
					{
						WayPoint edgeWayPoint = new WayPoint(connectingEdge.Center, SpawnType.Path, null, null);
						this.ConnectWaypoints(prevWp, edgeWayPoint, outSideWaypointInterval);
						prevWp = edgeWayPoint;
					}
					WayPoint newWaypoint = new WayPoint(ruin.PathCells[i].Center, SpawnType.Path, null, null);
					this.ConnectWaypoints(prevWp, newWaypoint, outSideWaypointInterval);
					prevWp = newWaypoint;
					i2 = i;
				}
				WayPoint closestPathPoint = Level.FindClosestWayPoint(prevWp.WorldPosition, this.Tunnels.SelectMany((Level.Tunnel t) => t.WayPoints), null);
				this.ConnectWaypoints(prevWp, closestPathPoint, outSideWaypointInterval);
			}
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x00055DEC File Offset: 0x00053FEC
		private Point FindPosAwayFromMainPath(double minDistance, bool asCloseAsPossible, Rectangle? limits = null)
		{
			List<ValueTuple<Point, double>> pointsAboveBottom = this.distanceField.FindAll(([TupleElementNames(new string[]
			{
				"point",
				"distance"
			})] ValueTuple<Point, double> d) => (double)d.Item1.Y > (double)this.GetBottomPosition((float)d.Item1.X).Y + minDistance);
			if (pointsAboveBottom.Count == 0)
			{
				DebugConsole.ThrowError("Error in FindPosAwayFromMainPath: no valid positions above the bottom of the sea floor. Has the position of the sea floor been set too high up?", null, null, false, false);
				return this.distanceField[Rand.Int(this.distanceField.Count, Rand.RandSync.ServerAndClient)].Item1;
			}
			List<ValueTuple<Point, double>> validPoints = pointsAboveBottom.FindAll(([TupleElementNames(new string[]
			{
				"point",
				"distance"
			})] ValueTuple<Point, double> d) => d.Item2 >= minDistance && (limits == null || limits.Value.Contains(d.Item1)));
			if (!validPoints.Any<ValueTuple<Point, double>>())
			{
				DebugConsole.AddWarning("Failed to find a valid position far enough from the main path. Choosing the furthest possible position.\n" + Environment.StackTrace, null);
				if (limits != null)
				{
					validPoints = pointsAboveBottom.FindAll(([TupleElementNames(new string[]
					{
						"point",
						"distance"
					})] ValueTuple<Point, double> d) => limits.Value.Contains(d.Item1));
				}
				if (!validPoints.Any<ValueTuple<Point, double>>())
				{
					validPoints = pointsAboveBottom;
				}
				ValueTuple<Point, double> furthestPoint = validPoints.First<ValueTuple<Point, double>>();
				foreach (ValueTuple<Point, double> point in validPoints)
				{
					if (point.Item2 > furthestPoint.Item2)
					{
						furthestPoint = point;
					}
				}
				return furthestPoint.Item1;
			}
			if (asCloseAsPossible)
			{
				if (!validPoints.Any<ValueTuple<Point, double>>())
				{
					validPoints = this.distanceField;
				}
				ValueTuple<Point, double> closestPoint = validPoints.First<ValueTuple<Point, double>>();
				foreach (ValueTuple<Point, double> point2 in validPoints)
				{
					if (point2.Item2 < closestPoint.Item2)
					{
						closestPoint = point2;
					}
				}
				return closestPoint.Item1;
			}
			return validPoints[Rand.Int(validPoints.Count, Rand.RandSync.ServerAndClient)].Item1;
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x00055FA4 File Offset: 0x000541A4
		private void CalculateTunnelDistanceField(List<Point> ruinPositions)
		{
			Level.<>c__DisplayClass219_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.ruinPositions = ruinPositions;
			int density = 1000;
			this.distanceField = new List<ValueTuple<Point, double>>();
			if (this.Mirrored)
			{
				for (int x = this.Size.X - 1; x >= 0; x -= density)
				{
					for (int y = 0; y < this.Size.Y; y += density)
					{
						this.<CalculateTunnelDistanceField>g__addPoint|219_0(x, y, ref CS$<>8__locals1);
					}
				}
				return;
			}
			for (int x2 = 0; x2 < this.Size.X; x2 += density)
			{
				for (int y2 = 0; y2 < this.Size.Y; y2 += density)
				{
					this.<CalculateTunnelDistanceField>g__addPoint|219_0(x2, y2, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x00056054 File Offset: 0x00054254
		private double GetDistToTunnel(Vector2 position, Level.Tunnel tunnel)
		{
			Point point = position.ToPoint();
			double shortestDistSqr = double.PositiveInfinity;
			for (int i = 1; i < tunnel.Nodes.Count; i++)
			{
				shortestDistSqr = Math.Min(shortestDistSqr, MathUtils.LineSegmentToPointDistanceSquared(tunnel.Nodes[i - 1], tunnel.Nodes[i], point));
			}
			return Math.Sqrt(shortestDistSqr);
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x000560B8 File Offset: 0x000542B8
		private DestructibleLevelWall CreateIceChunk(IEnumerable<GraphEdge> edges, Vector2 position, float? health = null)
		{
			List<Vector2> vertices = new List<Vector2>();
			using (IEnumerator<GraphEdge> enumerator = edges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					GraphEdge edge = enumerator.Current;
					if (!vertices.Any<Vector2>())
					{
						vertices.Add(edge.Point1);
					}
					else if (!vertices.Any((Vector2 v) => v.NearlyEquals(edge.Point1)))
					{
						vertices.Add(edge.Point1);
					}
					else if (!vertices.Any((Vector2 v) => v.NearlyEquals(edge.Point2)))
					{
						vertices.Add(edge.Point2);
					}
				}
			}
			if (vertices.Count < 3)
			{
				return null;
			}
			return this.CreateIceChunk((from v in vertices
			select v - position).ToList<Vector2>(), position, health);
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x000561B0 File Offset: 0x000543B0
		private DestructibleLevelWall CreateIceChunk(List<Vector2> vertices, Vector2 position, float? health = null)
		{
			DestructibleLevelWall newChunk = new DestructibleLevelWall(vertices, Color.White, this, health, true);
			newChunk.Body.Position = ConvertUnits.ToSimUnits(position);
			newChunk.Cells.ForEach(delegate(VoronoiCell c)
			{
				c.Translation = position;
			});
			newChunk.Body.BodyType = BodyType.Dynamic;
			newChunk.Body.FixedRotation = true;
			newChunk.Body.LinearDamping = 0.5f;
			newChunk.Body.IgnoreGravity = true;
			newChunk.Body.Mass *= 10f;
			return newChunk;
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x00056254 File Offset: 0x00054454
		private DestructibleLevelWall CreateIceSpire(List<GraphEdge> usedSpireEdges)
		{
			float minEdgeLength = 100f;
			Level.InterestingPosition mainPathPos = this.PositionsOfInterest.GetRandom((Level.InterestingPosition pos) => pos.PositionType == Level.PositionType.MainPath, Rand.RandSync.ServerAndClient);
			double closestDistSqr = double.PositiveInfinity;
			GraphEdge closestEdge = null;
			VoronoiCell closestCell = null;
			foreach (VoronoiCell cell in this.cells)
			{
				if (cell.CellType == CellType.Solid)
				{
					foreach (GraphEdge edge in cell.Edges)
					{
						if (edge.IsSolid && !usedSpireEdges.Contains(edge) && !edge.NextToCave && (edge.Center.Y <= (float)(this.Size.Y / 2) || (edge.Center.X >= (float)this.Size.X * 0.3f && edge.Center.X <= (float)this.Size.X * 0.7f)) && Vector2.DistanceSquared(edge.Center, this.StartPosition) >= 225000000f && Vector2.DistanceSquared(edge.Center, this.EndPosition) >= 225000000f)
						{
							float edgeLengthSqr = Vector2.DistanceSquared(edge.Point1, edge.Point2);
							if (edgeLengthSqr <= 1000000f && edgeLengthSqr >= minEdgeLength * minEdgeLength && Vector2.Dot(Vector2.Normalize(mainPathPos.Position.ToVector2()) - edge.Center, edge.GetNormal(cell)) >= 0.5f)
							{
								double distSqr = MathUtils.DistanceSquared((double)edge.Center.X, (double)edge.Center.Y, (double)mainPathPos.Position.X, (double)mainPathPos.Position.Y);
								if (distSqr < closestDistSqr)
								{
									closestDistSqr = distSqr;
									closestEdge = edge;
									closestCell = cell;
								}
							}
						}
					}
				}
			}
			if (closestEdge == null)
			{
				return null;
			}
			usedSpireEdges.Add(closestEdge);
			Vector2 edgeNormal = closestEdge.GetNormal(closestCell);
			float spireLength = (float)Math.Min(Math.Sqrt(closestDistSqr), 15000.0);
			spireLength *= MathHelper.Lerp(0.3f, 1.5f, this.Difficulty / 100f);
			Vector2 extrudedPoint = closestEdge.Point1 + edgeNormal * spireLength * Rand.Range(0.8f, 1f, Rand.RandSync.ServerAndClient);
			Vector2 extrudedPoint2 = closestEdge.Point2 + edgeNormal * spireLength * Rand.Range(0.8f, 1f, Rand.RandSync.ServerAndClient);
			List<Vector2> vertices = new List<Vector2>
			{
				closestEdge.Point1,
				extrudedPoint + (extrudedPoint2 - extrudedPoint) * Rand.Range(0.3f, 0.45f, Rand.RandSync.ServerAndClient),
				extrudedPoint2 + (extrudedPoint - extrudedPoint2) * Rand.Range(0.3f, 0.45f, Rand.RandSync.ServerAndClient),
				closestEdge.Point2
			};
			Vector2 center = Vector2.Zero;
			vertices.ForEach(delegate(Vector2 v)
			{
				center += v;
			});
			center /= (float)vertices.Count;
			DestructibleLevelWall spire = new DestructibleLevelWall((from v in vertices
			select v - center).ToList<Vector2>(), Color.White, this, new float?(100f), true);
			spire.Body.Position = ConvertUnits.ToSimUnits(center);
			spire.Body.BodyType = BodyType.Static;
			spire.Body.FixedRotation = true;
			spire.Body.IgnoreGravity = true;
			spire.Body.Mass *= 10f;
			spire.Cells.ForEach(delegate(VoronoiCell c)
			{
				c.Translation = center;
			});
			spire.WallDamageOnTouch = 50f;
			return spire;
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x060008E2 RID: 2274 RVA: 0x000566BC File Offset: 0x000548BC
		public List<Level.PathPoint> PathPoints { get; } = new List<Level.PathPoint>();

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x060008E3 RID: 2275 RVA: 0x000566C4 File Offset: 0x000548C4
		public List<Level.ClusterLocation> AbyssResources { get; } = new List<Level.ClusterLocation>();

		// Token: 0x060008E4 RID: 2276 RVA: 0x000566CC File Offset: 0x000548CC
		private void GenerateItems()
		{
			Level.<>c__DisplayClass233_0 CS$<>8__locals1 = new Level.<>c__DisplayClass233_0();
			CS$<>8__locals1.<>4__this = this;
			this.ResetRandomSeed();
			CS$<>8__locals1.levelResources = new List<ValueTuple<ItemPrefab, ItemPrefab.CommonnessInfo>>();
			List<ValueTuple<ItemPrefab, ItemPrefab.FixedQuantityResourceInfo>> fixedResources = new List<ValueTuple<ItemPrefab, ItemPrefab.FixedQuantityResourceInfo>>();
			CS$<>8__locals1.commonnessRange = new Vector2(float.MaxValue, float.MinValue);
			CS$<>8__locals1.caveCommonnessRange = new Vector2(float.MaxValue, float.MinValue);
			foreach (ItemPrefab itemPrefab2 in from p in ItemPrefab.Prefabs
			orderby p.UintIdentifier
			select p)
			{
				ItemPrefab.CommonnessInfo? commonnessInfo2 = itemPrefab2.GetCommonnessInfo(this);
				if (commonnessInfo2 != null)
				{
					ItemPrefab.CommonnessInfo commonnessInfo = commonnessInfo2.GetValueOrDefault();
					if (commonnessInfo.CanAppear)
					{
						if ((double)commonnessInfo.Commonness > 0.0)
						{
							if (commonnessInfo.Commonness < CS$<>8__locals1.commonnessRange.X)
							{
								CS$<>8__locals1.commonnessRange.X = commonnessInfo.Commonness;
							}
							if (commonnessInfo.Commonness > CS$<>8__locals1.commonnessRange.Y)
							{
								CS$<>8__locals1.commonnessRange.Y = commonnessInfo.Commonness;
							}
						}
						if ((double)commonnessInfo.CaveCommonness > 0.0)
						{
							if (commonnessInfo.CaveCommonness < CS$<>8__locals1.caveCommonnessRange.X)
							{
								CS$<>8__locals1.caveCommonnessRange.X = commonnessInfo.CaveCommonness;
							}
							if (commonnessInfo.CaveCommonness > CS$<>8__locals1.caveCommonnessRange.Y)
							{
								CS$<>8__locals1.caveCommonnessRange.Y = commonnessInfo.CaveCommonness;
							}
						}
						CS$<>8__locals1.levelResources.Add(new ValueTuple<ItemPrefab, ItemPrefab.CommonnessInfo>(itemPrefab2, commonnessInfo));
						continue;
					}
				}
				ItemPrefab.FixedQuantityResourceInfo fixedQuantityResourceInfo;
				if (itemPrefab2.LevelQuantity.TryGetValue(this.GenerationParams.Identifier, out fixedQuantityResourceInfo) || itemPrefab2.LevelQuantity.TryGetValue(this.LevelData.Biome.Identifier, out fixedQuantityResourceInfo) || itemPrefab2.LevelQuantity.TryGetValue(Identifier.Empty, out fixedQuantityResourceInfo))
				{
					fixedResources.Add(new ValueTuple<ItemPrefab, ItemPrefab.FixedQuantityResourceInfo>(itemPrefab2, fixedQuantityResourceInfo));
				}
			}
			DebugConsole.Log("Generating level resources...");
			CS$<>8__locals1.allValidLocations = this.GetAllValidClusterLocations();
			CS$<>8__locals1.maxResourceOverlap = 0.4f;
			foreach (ValueTuple<ItemPrefab, ItemPrefab.FixedQuantityResourceInfo> valueTuple in fixedResources)
			{
				ItemPrefab itemPrefab = valueTuple.Item1;
				ItemPrefab.FixedQuantityResourceInfo resourceInfo = valueTuple.Item2;
				Func<Level.ClusterLocation, bool> <>9__6;
				for (int i = 0; i < resourceInfo.ClusterQuantity; i++)
				{
					Level.<>c__DisplayClass233_2 CS$<>8__locals3 = new Level.<>c__DisplayClass233_2();
					Level.<>c__DisplayClass233_2 CS$<>8__locals4 = CS$<>8__locals3;
					IReadOnlyList<Level.ClusterLocation> allValidLocations = CS$<>8__locals1.allValidLocations;
					Func<Level.ClusterLocation, bool> predicate;
					if ((predicate = <>9__6) == null)
					{
						predicate = (<>9__6 = delegate(Level.ClusterLocation l)
						{
							float num2;
							return l.Cell != null && l.Edge != null && (!resourceInfo.IsIslandSpecific || l.Cell.Island) && (resourceInfo.AllowAtStart || l.EdgeCenter.Y <= (float)CS$<>8__locals1.<>4__this.startPosition.Y || l.EdgeCenter.X >= (float)CS$<>8__locals1.<>4__this.Size.X * 0.25f) && l.Edge.Length >= itemPrefab.Size.X && l.EdgeCenter.Y >= (float)CS$<>8__locals1.<>4__this.AbyssArea.Bottom && resourceInfo.ClusterSize <= CS$<>8__locals1.<GenerateItems>g__GetMaxResourcesOnEdge|4(itemPrefab, l, out num2);
						});
					}
					CS$<>8__locals4.location = allValidLocations.GetRandom(predicate, Rand.RandSync.ServerAndClient);
					if (CS$<>8__locals3.location.Cell == null || CS$<>8__locals3.location.Edge == null)
					{
						break;
					}
					List<Item> list;
					this.PlaceResources(itemPrefab, resourceInfo.ClusterSize, CS$<>8__locals3.location, out list, null, 0.4f);
					int locationIndex = CS$<>8__locals1.allValidLocations.FindIndex((Level.ClusterLocation l) => l.Equals(CS$<>8__locals3.location));
					CS$<>8__locals1.allValidLocations.RemoveAt(locationIndex);
				}
			}
			this.AbyssResources.Clear();
			IEnumerable<ValueTuple<ItemPrefab, ItemPrefab.CommonnessInfo>> abyssResourcePrefabs = from r in CS$<>8__locals1.levelResources
			where r.Item2.AbyssCommonness > 0f
			select r;
			if (abyssResourcePrefabs.Any<ValueTuple<ItemPrefab, ItemPrefab.CommonnessInfo>>())
			{
				int abyssClusterCount = (int)MathHelper.Lerp((float)this.GenerationParams.AbyssResourceClustersMin, (float)this.GenerationParams.AbyssResourceClustersMax, MathUtils.InverseLerp(this.LevelData.Biome.MinDifficulty, this.LevelData.Biome.AdjustedMaxDifficulty, this.Difficulty));
				for (int j = 0; j < abyssClusterCount; j++)
				{
					ItemPrefab selectedPrefab = ToolBox.SelectWeightedRandom<ItemPrefab>((from r in abyssResourcePrefabs
					select r.Item1).ToList<ItemPrefab>(), (from r in abyssResourcePrefabs
					select r.Item2.AbyssCommonness).ToList<float>(), Rand.RandSync.ServerAndClient);
					Level.ClusterLocation location = CS$<>8__locals1.allValidLocations.GetRandom(delegate(Level.ClusterLocation l)
					{
						if (l.Cell == null || l.Edge == null)
						{
							return false;
						}
						if (l.EdgeCenter.Y > (float)CS$<>8__locals1.<>4__this.AbyssArea.Bottom)
						{
							return false;
						}
						if (l.Edge.Length < selectedPrefab.Size.X)
						{
							return false;
						}
						l.InitializeResources();
						float num2;
						return l.Resources.Count <= CS$<>8__locals1.<GenerateItems>g__GetMaxResourcesOnEdge|4(selectedPrefab, l, out num2);
					}, Rand.RandSync.ServerAndClient);
					if (location.Cell == null || location.Edge == null)
					{
						break;
					}
					int clusterSize = Rand.Range(this.GenerationParams.ResourceClusterSizeRange.X, this.GenerationParams.ResourceClusterSizeRange.Y + 1, Rand.RandSync.ServerAndClient);
					List<Item> placedResources;
					this.PlaceResources(selectedPrefab, clusterSize, location, out placedResources, null, 0f);
					Level.ClusterLocation abyssClusterLocation = new Level.ClusterLocation(location.Cell, location.Edge, true);
					abyssClusterLocation.Resources.AddRange(placedResources);
					this.AbyssResources.Add(abyssClusterLocation);
					int locationIndex2 = CS$<>8__locals1.allValidLocations.FindIndex((Level.ClusterLocation l) => l.Equals(location));
					CS$<>8__locals1.allValidLocations.RemoveAt(locationIndex2);
				}
			}
			this.PathPoints.Clear();
			Level.nextPathPointId = 0;
			foreach (Level.Tunnel tunnel in this.Tunnels)
			{
				Level.<>c__DisplayClass233_4 CS$<>8__locals6;
				CS$<>8__locals6.tunnel = tunnel;
				float tunnelLength = 0f;
				for (int k = 1; k < CS$<>8__locals6.tunnel.Nodes.Count; k++)
				{
					tunnelLength += Vector2.Distance(CS$<>8__locals6.tunnel.Nodes[k - 1].ToVector2(), CS$<>8__locals6.tunnel.Nodes[k].ToVector2());
				}
				Level.<>c__DisplayClass233_5 CS$<>8__locals7;
				CS$<>8__locals7.nextNodeIndex = 1;
				CS$<>8__locals7.positionOnPath = CS$<>8__locals6.tunnel.Nodes.First<Point>().ToVector2();
				Vector2 lastNodePos = CS$<>8__locals6.tunnel.Nodes.Last<Point>().ToVector2();
				Point intervalRange = (CS$<>8__locals6.tunnel.Type != Level.TunnelType.Cave) ? this.GenerationParams.ResourceIntervalRange : this.GenerationParams.CaveResourceIntervalRange;
				for (;;)
				{
					Level.<>c__DisplayClass233_6 CS$<>8__locals8;
					CS$<>8__locals8.distance = Rand.Range(intervalRange.X, intervalRange.Y, Rand.RandSync.ServerAndClient);
					bool reachedLastNode = !Level.<GenerateItems>g__CalculatePositionOnPath|233_12(0f, ref CS$<>8__locals6, ref CS$<>8__locals7, ref CS$<>8__locals8);
					string id = this.Tunnels.IndexOf(CS$<>8__locals6.tunnel).ToString() + ":" + Level.nextPathPointId++.ToString();
					if (CS$<>8__locals6.tunnel.Type == Level.TunnelType.Cave)
					{
						goto IL_737;
					}
					Level.Tunnel parentTunnel = CS$<>8__locals6.tunnel.ParentTunnel;
					if (parentTunnel != null && parentTunnel.Type == Level.TunnelType.Cave)
					{
						goto IL_737;
					}
					float num = this.GenerationParams.ResourceSpawnChance;
					IL_742:
					float spawnChance = num;
					bool containsResources = true;
					if (spawnChance < 1f)
					{
						float spawnPointRoll = Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient);
						containsResources = (spawnPointRoll <= spawnChance);
					}
					Level.TunnelType tunnelType = CS$<>8__locals6.tunnel.Type;
					if (CS$<>8__locals6.tunnel.ParentTunnel == null || CS$<>8__locals6.tunnel.ParentTunnel.Type == Level.TunnelType.Cave)
					{
					}
					this.PathPoints.Add(new Level.PathPoint(id, CS$<>8__locals7.positionOnPath, containsResources, CS$<>8__locals6.tunnel.Type));
					if (reachedLastNode || Vector2.DistanceSquared(CS$<>8__locals7.positionOnPath, lastNodePos) <= (float)(intervalRange.Y * intervalRange.Y))
					{
						break;
					}
					continue;
					IL_737:
					num = this.GenerationParams.CaveResourceSpawnChance;
					goto IL_742;
				}
			}
			CS$<>8__locals1.itemCount = 0;
			CS$<>8__locals1.exclusiveResourceTags = new Identifier[]
			{
				"ore".ToIdentifier(),
				"plant".ToIdentifier()
			};
			List<string> disabledPathPoints = new List<string>();
			foreach (Level.PathPoint pathPoint in this.PathPoints)
			{
				if (CS$<>8__locals1.itemCount >= this.GenerationParams.ItemCount)
				{
					break;
				}
				if (pathPoint.ShouldContainResources)
				{
					CS$<>8__locals1.<GenerateItems>g__GenerateFirstCluster|1(pathPoint);
					if (pathPoint.ClusterLocations.Count <= 0)
					{
						disabledPathPoints.Add(pathPoint.Id);
					}
				}
			}
			using (List<string>.Enumerator enumerator5 = disabledPathPoints.GetEnumerator())
			{
				while (enumerator5.MoveNext())
				{
					string pathPointId = enumerator5.Current;
					Level.PathPoint? pathPoint4 = this.PathPoints.FirstOrNull((Level.PathPoint p) => p.Id == pathPointId);
					if (pathPoint4 != null)
					{
						Level.PathPoint pathPoint2 = pathPoint4.GetValueOrDefault();
						this.PathPoints.RemoveAll((Level.PathPoint p) => p.Id == pathPointId);
						this.PathPoints.Add(pathPoint2.WithResources(false));
					}
				}
			}
			CS$<>8__locals1.excludedPathPointIds = new List<string>();
			while (CS$<>8__locals1.itemCount < this.GenerationParams.ItemCount)
			{
				IEnumerable<Level.PathPoint> pathPoints = this.PathPoints;
				Func<Level.PathPoint, bool> predicate2;
				if ((predicate2 = CS$<>8__locals1.<>9__15) == null)
				{
					predicate2 = (CS$<>8__locals1.<>9__15 = ((Level.PathPoint p) => p.ShouldContainResources && p.NextClusterProbability > 0f && !CS$<>8__locals1.excludedPathPointIds.Contains(p.Id)));
				}
				List<Level.PathPoint> availablePathPoints = pathPoints.Where(predicate2).ToList<Level.PathPoint>();
				if (availablePathPoints.None(null))
				{
					break;
				}
				Level.PathPoint pathPoint3 = ToolBox.SelectWeightedRandom<Level.PathPoint>(availablePathPoints, (from p in availablePathPoints
				select p.NextClusterProbability).ToList<float>(), Rand.RandSync.ServerAndClient);
				CS$<>8__locals1.<GenerateItems>g__GenerateAdditionalCluster|2(pathPoint3);
			}
			DebugConsole.Log("Level resources generated");
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x00057148 File Offset: 0x00055348
		public List<Item> GenerateMissionResources(ItemPrefab prefab, int requiredAmount, Level.PositionType positionType, IEnumerable<Level.Cave> targetCaves = null)
		{
			Level.<>c__DisplayClass234_0 CS$<>8__locals1 = new Level.<>c__DisplayClass234_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.positionType = positionType;
			CS$<>8__locals1.targetCaves = targetCaves;
			CS$<>8__locals1.requiredAmount = requiredAmount;
			CS$<>8__locals1.prefab = prefab;
			CS$<>8__locals1.allValidLocations = this.GetAllValidClusterLocations();
			List<Item> placedResources = new List<Item>();
			if (CS$<>8__locals1.allValidLocations.None(null))
			{
				return placedResources;
			}
			for (int i = CS$<>8__locals1.allValidLocations.Count - 1; i >= 0; i--)
			{
				if (CS$<>8__locals1.<GenerateMissionResources>g__HasResources|10(CS$<>8__locals1.allValidLocations[i]))
				{
					CS$<>8__locals1.allValidLocations.RemoveAt(i);
				}
			}
			if (this.PositionsOfInterest.None((Level.InterestingPosition p) => p.PositionType == CS$<>8__locals1.positionType))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(63, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to find a position of the type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Level.PositionType>(CS$<>8__locals1.positionType);
				defaultInterpolatedStringHandler.AppendLiteral("\" for mission resources.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				ImmutableArray<Level.PositionType>.Enumerator enumerator = MineralMission.ValidPositionTypes.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Level.PositionType validType = enumerator.Current;
					if (validType != CS$<>8__locals1.positionType && this.PositionsOfInterest.Any((Level.InterestingPosition p) => p.PositionType == validType))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(22, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Placing in \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Level.PositionType>(validType);
						defaultInterpolatedStringHandler2.AppendLiteral("\" instead.");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
						CS$<>8__locals1.positionType = validType;
						break;
					}
				}
			}
			try
			{
				Level.PositionType positionType2 = CS$<>8__locals1.positionType;
				Predicate<Level.ClusterLocation> match;
				switch (positionType2)
				{
				case Level.PositionType.MainPath:
				{
					Predicate<Level.ClusterLocation> predicate;
					if ((predicate = Level.<>O.<0>__IsOnMainPath) == null)
					{
						predicate = (Level.<>O.<0>__IsOnMainPath = new Predicate<Level.ClusterLocation>(Level.<GenerateMissionResources>g__IsOnMainPath|234_5));
					}
					match = predicate;
					goto IL_229;
				}
				case Level.PositionType.SidePath:
				{
					Predicate<Level.ClusterLocation> predicate2;
					if ((predicate2 = Level.<>O.<1>__IsOnSidePath) == null)
					{
						predicate2 = (Level.<>O.<1>__IsOnSidePath = new Predicate<Level.ClusterLocation>(Level.<GenerateMissionResources>g__IsOnSidePath|234_6));
					}
					match = predicate2;
					goto IL_229;
				}
				case Level.PositionType.MainPath | Level.PositionType.SidePath:
					break;
				case Level.PositionType.Cave:
				{
					Predicate<Level.ClusterLocation> predicate3;
					if ((predicate3 = Level.<>O.<2>__IsInCave) == null)
					{
						predicate3 = (Level.<>O.<2>__IsInCave = new Predicate<Level.ClusterLocation>(Level.<GenerateMissionResources>g__IsInCave|234_7));
					}
					match = predicate3;
					goto IL_229;
				}
				default:
					if (positionType2 == Level.PositionType.AbyssCave)
					{
						match = new Predicate<Level.ClusterLocation>(CS$<>8__locals1.<GenerateMissionResources>g__IsInAbyssCave|8);
						goto IL_229;
					}
					break;
				}
				throw new NotImplementedException();
				IL_229:
				CS$<>8__locals1.<GenerateMissionResources>g__RemoveInvalidLocations|9(match);
			}
			catch (NotImplementedException)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(104, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Unexpected PositionType (\"");
				defaultInterpolatedStringHandler3.AppendFormatted<Level.PositionType>(CS$<>8__locals1.positionType);
				defaultInterpolatedStringHandler3.AppendLiteral("\") for mineral mission resources: mineral spawning might not work as expected.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
			}
			if (CS$<>8__locals1.targetCaves != null && CS$<>8__locals1.targetCaves.Any<Level.Cave>())
			{
				CS$<>8__locals1.allValidLocations.RemoveAll((Level.ClusterLocation l) => CS$<>8__locals1.targetCaves.None((Level.Cave c) => c.Area.Contains(l.EdgeCenter)));
			}
			CS$<>8__locals1.poiPos = this.PositionsOfInterest.GetRandom((Level.InterestingPosition p) => p.PositionType == CS$<>8__locals1.positionType, Rand.RandSync.ServerAndClient).Position.ToVector2();
			CS$<>8__locals1.allValidLocations.Sort((Level.ClusterLocation x, Level.ClusterLocation y) => Vector2.DistanceSquared(CS$<>8__locals1.poiPos, x.EdgeCenter).CompareTo(Vector2.DistanceSquared(CS$<>8__locals1.poiPos, y.EdgeCenter)));
			CS$<>8__locals1.maxResourceOverlap = 0.4f;
			Level.ClusterLocation selectedLocation = CS$<>8__locals1.allValidLocations.FirstOrDefault(delegate(Level.ClusterLocation l)
			{
				float edgeLength2 = Vector2.Distance(l.Edge.Point1, l.Edge.Point2);
				if (!l.Edge.OutsideLevel)
				{
					VoronoiCell cell = l.Edge.Cell1;
					if (cell == null || !cell.IsDestructible)
					{
						VoronoiCell cell2 = l.Edge.Cell2;
						if (cell2 == null || !cell2.IsDestructible)
						{
							return false;
						}
					}
					return CS$<>8__locals1.requiredAmount <= (int)Math.Floor((double)(edgeLength2 / ((1f - CS$<>8__locals1.maxResourceOverlap) * CS$<>8__locals1.prefab.Size.X)));
				}
				return false;
			});
			if (selectedLocation.Edge == null)
			{
				float longestEdge = 0f;
				foreach (Level.ClusterLocation validLocation in CS$<>8__locals1.allValidLocations)
				{
					float edgeLength = Vector2.Distance(validLocation.Edge.Point1, validLocation.Edge.Point2);
					if (edgeLength > longestEdge)
					{
						selectedLocation = validLocation;
						longestEdge = edgeLength;
					}
				}
			}
			if (selectedLocation.Edge == null)
			{
				throw new Exception("Failed to find a suitable level wall edge to place level resources on.");
			}
			this.PlaceResources(CS$<>8__locals1.prefab, CS$<>8__locals1.requiredAmount, selectedLocation, out placedResources, null, 0.4f);
			Vector2 edgeNormal = selectedLocation.Edge.GetNormal(selectedLocation.Cell);
			return placedResources;
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x00057534 File Offset: 0x00055734
		private List<Level.ClusterLocation> GetAllValidClusterLocations()
		{
			Level.<>c__DisplayClass235_0 CS$<>8__locals1 = new Level.<>c__DisplayClass235_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.subBorders = new List<Rectangle>();
			this.Wrecks.ForEach(new Action<Submarine>(CS$<>8__locals1.<GetAllValidClusterLocations>g__AddBordersToList|0));
			CS$<>8__locals1.<GetAllValidClusterLocations>g__AddBordersToList|0(this.BeaconStation);
			List<Level.ClusterLocation> locations = new List<Level.ClusterLocation>();
			foreach (VoronoiCell c in this.GetAllCells())
			{
				if (c.CellType == CellType.Solid)
				{
					foreach (GraphEdge e in c.Edges)
					{
						if (CS$<>8__locals1.<GetAllValidClusterLocations>g__IsValidEdge|1(e))
						{
							locations.Add(new Level.ClusterLocation(c, e, false));
						}
					}
				}
			}
			return locations;
		}

		// Token: 0x060008E7 RID: 2279 RVA: 0x00057620 File Offset: 0x00055820
		private void PlaceResources(ItemPrefab resourcePrefab, int resourceCount, Level.ClusterLocation location, out List<Item> placedResources, float? edgeLength = null, float maxResourceOverlap = 0.4f)
		{
			float value = edgeLength.GetValueOrDefault();
			if (edgeLength == null)
			{
				value = location.Edge.Length;
				edgeLength = new float?(value);
			}
			Vector2 edgeDir = (location.Edge.Point2 - location.Edge.Point1) / edgeLength.Value;
			if (!MathUtils.IsValid(edgeDir))
			{
				edgeDir = Vector2.Zero;
			}
			float minResourceOverlap = -((edgeLength.Value - (float)resourceCount * resourcePrefab.Size.X) / ((float)resourceCount * resourcePrefab.Size.X));
			minResourceOverlap = Math.Clamp(minResourceOverlap, 0f, maxResourceOverlap);
			float[] lerpAmounts = new float[resourceCount];
			lerpAmounts[0] = 0f;
			float lerpAmount = 0f;
			for (int i = 1; i < resourceCount; i++)
			{
				float overlap = Rand.Range(minResourceOverlap, maxResourceOverlap, Rand.RandSync.ServerAndClient);
				lerpAmount = Math.Clamp(lerpAmount + (1f - overlap) * resourcePrefab.Size.X / edgeLength.Value, 0f, 1f);
				lerpAmounts[i] = lerpAmount;
			}
			float startOffset = Rand.Range(0f, 1f - lerpAmount, Rand.RandSync.ServerAndClient);
			placedResources = new List<Item>();
			for (int j = 0; j < resourceCount; j++)
			{
				Vector2 selectedPos = (location.Edge.Length < resourcePrefab.Size.X) ? location.Edge.Center : Vector2.Lerp(location.Edge.Point1 + edgeDir * resourcePrefab.Size.X / 2f, location.Edge.Point2 - edgeDir * resourcePrefab.Size.X / 2f, startOffset + lerpAmounts[j]);
				Item item = new Item(resourcePrefab, selectedPos, null, 0, true);
				Vector2 edgeNormal = location.Edge.GetNormal(location.Cell);
				float moveAmount = (item.body == null) ? ((float)(item.Rect.Height / 2)) : ConvertUnits.ToDisplayUnits(item.body.GetMaxExtent() * 0.7f);
				float num = moveAmount;
				LevelResource component = item.GetComponent<LevelResource>();
				moveAmount = num + ((component != null) ? component.RandomOffsetFromWall : 0f) * Rand.Range(-0.5f, 0.5f, Rand.RandSync.ServerAndClient);
				item.Move(edgeNormal * moveAmount, true);
				item.Rotation = MathHelper.ToDegrees(-MathUtils.VectorToAngle(edgeNormal) + 1.5707964f);
				Holdable h = item.GetComponent<Holdable>();
				if (h != null)
				{
					h.AttachToWall();
				}
				else if (item.body != null)
				{
					item.body.SetTransformIgnoreContacts(item.body.SimPosition, MathUtils.VectorToAngle(edgeNormal) - 1.5707964f, true);
				}
				placedResources.Add(item);
			}
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x000578E4 File Offset: 0x00055AE4
		public Vector2 GetRandomItemPos(Level.PositionType spawnPosType, float randomSpread, float minDistFromSubs, float offsetFromWall = 10f, Func<Level.InterestingPosition, bool> filter = null)
		{
			if (!this.PositionsOfInterest.Any<Level.InterestingPosition>())
			{
				return new Vector2((float)(this.Size.X / 2), (float)(this.Size.Y / 2));
			}
			Vector2 position = Vector2.Zero;
			int tries = 0;
			Vector2 startPos;
			Vector2 endPos;
			for (;;)
			{
				Level.InterestingPosition potentialPos;
				this.TryGetInterestingPosition(true, spawnPosType, minDistFromSubs, out potentialPos, filter, false);
				Vector2 offset = Rand.Vector(Rand.Range(0f, randomSpread, Rand.RandSync.ServerAndClient), Rand.RandSync.ServerAndClient);
				startPos = potentialPos.Position.ToVector2();
				if (!this.IsPositionInsideWall(startPos + offset))
				{
					startPos += offset;
				}
				endPos = startPos - Vector2.UnitY * (float)this.Size.Y;
				if (!potentialPos.PositionType.IsEnclosedArea())
				{
					Body body = Submarine.PickBody(ConvertUnits.ToSimUnits(startPos), ConvertUnits.ToSimUnits(endPos), (from w in this.ExtraWalls.Where(delegate(LevelWall w)
					{
						Body body2 = w.Body;
						return (body2 != null && body2.BodyType == BodyType.Dynamic) || w is DestructibleLevelWall;
					})
					select w.Body).Union(from s in Submarine.Loaded
					where s.Info.Type == SubmarineType.Player
					select s.PhysicsBody.FarseerBody), new Category?(Category.Cat1 | Category.Cat8), true, null, false);
					if (((body != null) ? body.UserData : null) is VoronoiCell)
					{
						break;
					}
				}
				tries++;
				if (tries == 10)
				{
					position = startPos;
				}
				if (tries >= 10)
				{
					return position;
				}
			}
			position = ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition) + Vector2.Normalize(startPos - endPos) * offsetFromWall;
			return position;
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x00057AB0 File Offset: 0x00055CB0
		public bool TryGetInterestingPositionAwayFromPoint(bool useSyncedRand, Level.PositionType positionType, float minDistFromSubs, out Level.InterestingPosition position, Vector2 awayPoint, float minDistFromPoint, Func<Level.InterestingPosition, bool> filter = null)
		{
			position = default(Level.InterestingPosition);
			return this.TryGetInterestingPosition(useSyncedRand, positionType, minDistFromSubs, out position, awayPoint, minDistFromPoint, filter, false);
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x00057ADC File Offset: 0x00055CDC
		public bool TryGetInterestingPosition(bool useSyncedRand, Level.PositionType positionType, float minDistFromSubs, out Level.InterestingPosition position, Func<Level.InterestingPosition, bool> filter = null, bool suppressWarning = false)
		{
			position = default(Level.InterestingPosition);
			return this.TryGetInterestingPosition(useSyncedRand, positionType, minDistFromSubs, out position, Vector2.Zero, 0f, filter, suppressWarning);
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x00057B0C File Offset: 0x00055D0C
		public bool TryGetInterestingPosition(bool useSyncedRand, Level.PositionType positionType, float minDistFromSubs, out Level.InterestingPosition position, Vector2 awayPoint, float minDistFromPoint = 0f, Func<Level.InterestingPosition, bool> filter = null, bool suppressWarning = false)
		{
			if (!this.PositionsOfInterest.Any<Level.InterestingPosition>())
			{
				position = default(Level.InterestingPosition);
				return false;
			}
			List<Level.InterestingPosition> suitablePositions = this.PositionsOfInterest.FindAll((Level.InterestingPosition p) => positionType.HasFlag(p.PositionType));
			if (filter != null)
			{
				suitablePositions.RemoveAll((Level.InterestingPosition p) => !filter(p));
			}
			if (positionType.HasFlag(Level.PositionType.MainPath) || positionType.HasFlag(Level.PositionType.SidePath) || positionType.HasFlag(Level.PositionType.Abyss) || positionType.HasFlag(Level.PositionType.Cave) || positionType.HasFlag(Level.PositionType.AbyssCave))
			{
				suitablePositions.RemoveAll((Level.InterestingPosition p) => base.<TryGetInterestingPosition>g__IsInvalid|3(p));
			}
			if (!suitablePositions.Any<Level.InterestingPosition>())
			{
				if (!suppressWarning)
				{
					string errorMsg = string.Concat(new string[]
					{
						"Could not find a suitable position of interest. (PositionType: ",
						positionType.ToString(),
						", minDistFromSubs: ",
						minDistFromSubs.ToString(),
						")\n",
						Environment.StackTrace.CleanupStackTrace()
					});
					GameAnalyticsManager.AddErrorEventOnce("Level.TryGetInterestingPosition:PositionTypeNotFound", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				}
				position = this.PositionsOfInterest[Rand.Int(this.PositionsOfInterest.Count, useSyncedRand ? Rand.RandSync.ServerAndClient : Rand.RandSync.Unsynced)];
				return false;
			}
			List<Level.InterestingPosition> farEnoughPositions = new List<Level.InterestingPosition>(suitablePositions);
			if (minDistFromSubs > 0f)
			{
				using (List<Submarine>.Enumerator enumerator = Submarine.Loaded.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Submarine sub = enumerator.Current;
						if (sub.Info.Type == SubmarineType.Player)
						{
							farEnoughPositions.RemoveAll((Level.InterestingPosition p) => Vector2.DistanceSquared(p.Position.ToVector2(), sub.WorldPosition) < minDistFromSubs * minDistFromSubs);
						}
					}
				}
			}
			if (minDistFromPoint > 0f)
			{
				farEnoughPositions.RemoveAll((Level.InterestingPosition p) => Vector2.DistanceSquared(p.Position.ToVector2(), awayPoint) < minDistFromPoint * minDistFromPoint);
			}
			if (!farEnoughPositions.Any<Level.InterestingPosition>())
			{
				string errorMsg2 = string.Concat(new string[]
				{
					"Could not find a position of interest far enough from the submarines. (PositionType: ",
					positionType.ToString(),
					", minDistFromSubs: ",
					minDistFromSubs.ToString(),
					")\n",
					Environment.StackTrace.CleanupStackTrace()
				});
				GameAnalyticsManager.AddErrorEventOnce("Level.TryGetInterestingPosition:TooCloseToSubs", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
				float maxDist = 0f;
				position = suitablePositions.First<Level.InterestingPosition>();
				using (List<Level.InterestingPosition>.Enumerator enumerator2 = suitablePositions.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Level.InterestingPosition pos = enumerator2.Current;
						float dist = Submarine.Loaded.Sum(delegate(Submarine s)
						{
							if (!Submarine.MainSubs.Contains(s))
							{
								return 0f;
							}
							Vector2 worldPosition = s.WorldPosition;
							Point position2 = pos.Position;
							return Vector2.DistanceSquared(worldPosition, position2.ToVector2());
						});
						if (dist > maxDist)
						{
							position = pos;
							maxDist = dist;
						}
					}
				}
				return false;
			}
			position = farEnoughPositions[Rand.Int(farEnoughPositions.Count, useSyncedRand ? Rand.RandSync.ServerAndClient : Rand.RandSync.Unsynced)];
			return true;
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x00057E88 File Offset: 0x00056088
		public bool IsPositionInsideWall(Vector2 worldPosition)
		{
			VoronoiCell closestCell = this.GetClosestCell(worldPosition);
			return closestCell != null && closestCell.IsPointInside(worldPosition);
		}

		// Token: 0x060008ED RID: 2285 RVA: 0x00057EAC File Offset: 0x000560AC
		public void Update(float deltaTime, Camera cam)
		{
			this.LevelObjectManager.Update(deltaTime, cam);
			foreach (LevelWall wall in this.ExtraWalls)
			{
				wall.Update(deltaTime);
			}
			for (int i = this.UnsyncedExtraWalls.Count - 1; i >= 0; i--)
			{
				this.UnsyncedExtraWalls[i].Update(deltaTime);
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				foreach (LevelWall wall2 in this.ExtraWalls)
				{
					DestructibleLevelWall destructibleWall = wall2 as DestructibleLevelWall;
					if (destructibleWall != null && destructibleWall.NetworkUpdatePending)
					{
						GameMain.NetworkMember.CreateEntityEvent(this, new Level.SingleLevelWallEventData(destructibleWall));
						destructibleWall.NetworkUpdatePending = false;
					}
				}
				this.networkUpdateTimer += deltaTime;
				if (this.networkUpdateTimer > 5f)
				{
					if (this.ExtraWalls.Any((LevelWall w) => w.Body.BodyType > BodyType.Static))
					{
						GameMain.NetworkMember.CreateEntityEvent(this, default(Level.GlobalLevelWallEventData));
					}
					this.networkUpdateTimer = 0f;
				}
			}
		}

		// Token: 0x060008EE RID: 2286 RVA: 0x0005802C File Offset: 0x0005622C
		public Vector2 GetBottomPosition(float xPosition)
		{
			float interval = (float)(this.Size.X / (this.bottomPositions.Count - 1));
			int index = (int)Math.Floor((double)(xPosition / interval));
			if (index < 0 || index >= this.bottomPositions.Count - 1)
			{
				return new Vector2(xPosition, (float)this.BottomPos);
			}
			float t = (xPosition - (float)this.bottomPositions[index].X) / interval;
			t = MathHelper.Clamp(t, 0f, 1f);
			float yPos = MathHelper.Lerp((float)this.bottomPositions[index].Y, (float)this.bottomPositions[index + 1].Y, t);
			return new Vector2(xPosition, yPos);
		}

		// Token: 0x060008EF RID: 2287 RVA: 0x000580E0 File Offset: 0x000562E0
		public List<VoronoiCell> GetAllCells()
		{
			List<VoronoiCell> cells = new List<VoronoiCell>();
			for (int x = 0; x < this.cellGrid.GetLength(0); x++)
			{
				for (int y = 0; y < this.cellGrid.GetLength(1); y++)
				{
					cells.AddRange(this.cellGrid[x, y]);
				}
			}
			return cells;
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x00058138 File Offset: 0x00056338
		public List<VoronoiCell> GetCells(Vector2 worldPos, int searchDepth = 2)
		{
			this.tempCells.Clear();
			int gridPosX = (int)Math.Floor((double)(worldPos.X / 2000f));
			int gridPosY = (int)Math.Floor((double)(worldPos.Y / 2000f));
			int startX = Math.Max(gridPosX - searchDepth, 0);
			int endX = Math.Min(gridPosX + searchDepth, this.cellGrid.GetLength(0) - 1);
			int startY = Math.Max(gridPosY - searchDepth, 0);
			int endY = Math.Min(gridPosY + searchDepth, this.cellGrid.GetLength(1) - 1);
			for (int y = startY; y <= endY; y++)
			{
				for (int x = startX; x <= endX; x++)
				{
					this.tempCells.AddRange(this.cellGrid[x, y]);
				}
			}
			foreach (LevelWall wall in this.ExtraWalls)
			{
				if (wall == this.SeaFloor)
				{
					if ((float)this.SeaFloorTopPos < worldPos.Y - (float)(searchDepth * 2000))
					{
						continue;
					}
				}
				else
				{
					DestructibleLevelWall destructibleWall = wall as DestructibleLevelWall;
					if (destructibleWall != null && destructibleWall.Destroyed)
					{
						continue;
					}
					bool closeEnough = false;
					foreach (VoronoiCell cell in wall.Cells)
					{
						if (cell.IsPointInsideAABB(worldPos, (float)((searchDepth + 1) * 2000 / 2)))
						{
							closeEnough = true;
							break;
						}
					}
					if (!closeEnough)
					{
						continue;
					}
				}
				foreach (VoronoiCell cell2 in wall.Cells)
				{
					this.tempCells.Add(cell2);
				}
			}
			foreach (Level.AbyssIsland abyssIsland in this.AbyssIslands)
			{
				if ((float)abyssIsland.Area.X <= worldPos.X + (float)(searchDepth * 2000) && (float)abyssIsland.Area.Right >= worldPos.X - (float)(searchDepth * 2000) && (float)abyssIsland.Area.Y <= worldPos.Y + (float)(searchDepth * 2000) && (float)abyssIsland.Area.Bottom >= worldPos.Y - (float)(searchDepth * 2000))
				{
					this.tempCells.AddRange(abyssIsland.Cells);
				}
			}
			return this.tempCells;
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x00058400 File Offset: 0x00056600
		public VoronoiCell GetClosestCell(Vector2 worldPos)
		{
			double closestDist = double.MaxValue;
			VoronoiCell closestCell = null;
			for (int searchDepth = 2; searchDepth < 5; searchDepth++)
			{
				foreach (VoronoiCell cell in this.GetCells(worldPos, searchDepth))
				{
					double dist = MathUtils.DistanceSquared(cell.Site.Coord.X, cell.Site.Coord.Y, (double)worldPos.X, (double)worldPos.Y);
					if (dist < closestDist)
					{
						closestDist = dist;
						closestCell = cell;
					}
				}
				if (closestCell != null)
				{
					break;
				}
			}
			return closestCell;
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x000584B0 File Offset: 0x000566B0
		private List<VoronoiCell> CreatePathToClosestTunnel(Point pos)
		{
			VoronoiCell closestPathCell = null;
			double closestDist = 0.0;
			foreach (Level.Tunnel tunnel in this.Tunnels)
			{
				if (tunnel.Type != Level.TunnelType.Cave)
				{
					foreach (VoronoiCell cell in tunnel.Cells)
					{
						double dist = MathUtils.DistanceSquared(cell.Site.Coord.X, cell.Site.Coord.Y, (double)pos.X, (double)pos.Y);
						if (closestPathCell == null || dist < closestDist)
						{
							closestPathCell = cell;
							closestDist = dist;
						}
					}
				}
			}
			List<VoronoiCell> validCells = this.cells.FindAll((VoronoiCell c) => c.CellType != CellType.Empty && c.CellType != CellType.Removed);
			List<VoronoiCell> pathCells = new List<VoronoiCell>
			{
				closestPathCell
			};
			foreach (VoronoiCell cell2 in validCells)
			{
				foreach (GraphEdge e in cell2.Edges)
				{
					if (MathUtils.LineSegmentsIntersect(closestPathCell.Center, pos.ToVector2(), e.Point1, e.Point2))
					{
						cell2.CellType = CellType.Removed;
						for (int x = 0; x < this.cellGrid.GetLength(0); x++)
						{
							for (int y = 0; y < this.cellGrid.GetLength(1); y++)
							{
								this.cellGrid[x, y].Remove(cell2);
							}
						}
						pathCells.Add(cell2);
						this.cells.Remove(cell2);
						using (List<GraphEdge>.Enumerator enumerator5 = cell2.Edges.GetEnumerator())
						{
							while (enumerator5.MoveNext())
							{
								GraphEdge otherEdge = enumerator5.Current;
								VoronoiCell otherAdjacent = otherEdge.AdjacentCell(cell2);
								if (otherAdjacent != null && otherAdjacent.CellType != CellType.Solid && Vector2.DistanceSquared(otherEdge.Point1, otherEdge.Point2) < 250000f)
								{
									foreach (GraphEdge e2 in cell2.Edges)
									{
										if (e2 != otherEdge && e2 != otherEdge && (MathUtils.NearlyEqual(otherEdge.Point1, e2.Point1, 0.0001f) || MathUtils.NearlyEqual(otherEdge.Point2, e2.Point1, 0.0001f) || MathUtils.NearlyEqual(otherEdge.Point2, e2.Point2, 0.0001f)))
										{
											VoronoiCell adjacentCell = e2.AdjacentCell(cell2);
											if (adjacentCell != null && adjacentCell.CellType != CellType.Removed)
											{
												adjacentCell.CellType = CellType.Removed;
												for (int x2 = 0; x2 < this.cellGrid.GetLength(0); x2++)
												{
													for (int y2 = 0; y2 < this.cellGrid.GetLength(1); y2++)
													{
														this.cellGrid[x2, y2].Remove(adjacentCell);
													}
												}
												this.cells.Remove(adjacentCell);
											}
										}
									}
								}
							}
							break;
						}
					}
				}
			}
			pathCells.Sort((VoronoiCell c1, VoronoiCell c2) => Vector2.DistanceSquared(c1.Center, pos.ToVector2()).CompareTo(Vector2.DistanceSquared(c2.Center, pos.ToVector2())));
			return pathCells;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x00058904 File Offset: 0x00056B04
		public bool IsCloseToStart(Vector2 position, float minDist)
		{
			return this.IsCloseToStart(position.ToPoint(), minDist);
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x00058914 File Offset: 0x00056B14
		public bool IsCloseToEnd(Vector2 position, float minDist)
		{
			return this.IsCloseToEnd(position.ToPoint(), minDist);
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x00058924 File Offset: 0x00056B24
		public bool IsCloseToStart(Point position, float minDist)
		{
			return MathUtils.LineSegmentToPointDistanceSquared(this.startPosition, this.startExitPosition, position) < (double)(minDist * minDist);
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0005893E File Offset: 0x00056B3E
		public bool IsCloseToEnd(Point position, float minDist)
		{
			return MathUtils.LineSegmentToPointDistanceSquared(this.endPosition, this.endExitPosition, position) < (double)(minDist * minDist);
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x00058958 File Offset: 0x00056B58
		private Submarine SpawnSubOnPath(string subName, ContentFile contentFile, SubmarineType type, LevelData.ThalamusSpawn thalamusSpawn = LevelData.ThalamusSpawn.Random, bool spawnInTheMiddle = false)
		{
			Level.<>c__DisplayClass253_0 CS$<>8__locals1 = new Level.<>c__DisplayClass253_0();
			CS$<>8__locals1.<>4__this = this;
			Stopwatch tempSW = Stopwatch.StartNew();
			SubmarineInfo info = new SubmarineInfo(contentFile.Path.Value, "", null, true, false)
			{
				Type = type
			};
			float distanceBetweenStartAndEnd = (float)Math.Abs(this.endPosition.X - this.startPosition.X);
			CS$<>8__locals1.spawnAwayFromStartAndEnd = (distanceBetweenStartAndEnd > 21000f);
			CS$<>8__locals1.waypoints = WayPoint.WayPointList.Where(new Func<WayPoint, bool>(CS$<>8__locals1.<SpawnSubOnPath>g__IsValidWaypoint|0)).ToList<WayPoint>();
			if (spawnInTheMiddle)
			{
				float horizontalMiddlePoint = (float)this.Size.X / 2f;
				CS$<>8__locals1.waypoints.Sort((WayPoint wp1, WayPoint wp2) => base.<SpawnSubOnPath>g__GetHorizontalDistanceToMiddlePoint|6(wp2).CompareTo(base.<SpawnSubOnPath>g__GetHorizontalDistanceToMiddlePoint|6(wp1)));
			}
			else
			{
				CS$<>8__locals1.waypoints.Shuffle(Rand.RandSync.ServerAndClient);
			}
			if (CS$<>8__locals1.waypoints.None(null))
			{
				DebugConsole.ThrowError("No valid waypoints to spawn sub: " + subName, null, null, false, false);
				return null;
			}
			XDocument subDoc = SubmarineInfo.OpenFile(contentFile.Path.Value);
			Rectangle subBorders = Submarine.GetBorders(subDoc.Root);
			int padding = 1500;
			CS$<>8__locals1.paddedBorders = new Rectangle(subBorders.X - padding, subBorders.Y + padding, subBorders.Width + padding * 2, subBorders.Height + padding * 2);
			CS$<>8__locals1.positions = new List<Vector2>();
			CS$<>8__locals1.rects = new List<Rectangle>();
			int attemptsLeft = 100;
			bool success = false;
			WayPoint wayPoint = null;
			CS$<>8__locals1.spawnPoint = Vector2.Zero;
			int attempt = 0;
			Level.Loaded.GetAllCells();
			BeaconStationInfo beaconStationInfo = info.BeaconStationInfo;
			Level.PlacementType placement = (beaconStationInfo != null) ? beaconStationInfo.Placement : Level.PlacementType.Bottom;
			bool isReshuffled = false;
			while (attemptsLeft > 0)
			{
				if (!isReshuffled && attemptsLeft < 10)
				{
					DebugConsole.AddWarning("Could not find a suitable position for " + subName + ". Reshuffling the waypoints and trying a few more times.", null);
					CS$<>8__locals1.waypoints.Shuffle(Rand.RandSync.ServerAndClient);
					isReshuffled = true;
				}
				attemptsLeft--;
				if (!CS$<>8__locals1.<SpawnSubOnPath>g__TryGetWayPoint|2(ref wayPoint))
				{
					DebugConsole.NewMessage("Failed to find any spawn point for the sub: " + subName + " (No valid waypoints left).", new Color?(Color.Red), false);
					break;
				}
				attempt++;
				CS$<>8__locals1.spawnPoint = wayPoint.WorldPosition;
				success = CS$<>8__locals1.<SpawnSubOnPath>g__TryPositionSub|1(subBorders, subName, placement, ref CS$<>8__locals1.spawnPoint);
				Dictionary<string, List<Vector2>> dictionary = this.positionHistory;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
				defaultInterpolatedStringHandler.AppendFormatted(info.Name);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(attempt);
				dictionary.TryAdd(defaultInterpolatedStringHandler.ToStringAndClear(), CS$<>8__locals1.positions.ToList<Vector2>());
				CS$<>8__locals1.positions.Clear();
				if (success)
				{
					break;
				}
			}
			tempSW.Stop();
			if (success)
			{
				tempSW.Restart();
				Submarine sub = new Submarine(info, true, null, null);
				if (type == SubmarineType.Wreck)
				{
					sub.MakeWreck();
					this.Wrecks.Add(sub);
					this.PositionsOfInterest.Add(new Level.InterestingPosition(CS$<>8__locals1.spawnPoint.ToPoint(), Level.PositionType.Wreck, sub, true));
					foreach (Hull hull in sub.GetHulls(false))
					{
						if (hull.WaterPercentage <= 0f && Rand.Value(Rand.RandSync.ServerAndClient) <= Level.Loaded.GenerationParams.WreckHullFloodingChance)
						{
							hull.WaterVolume = Math.Max(hull.WaterVolume, hull.Volume * Rand.Range(Level.Loaded.GenerationParams.WreckFloodingHullMinWaterPercentage, Level.Loaded.GenerationParams.WreckFloodingHullMaxWaterPercentage, Rand.RandSync.ServerAndClient));
						}
					}
					bool spawnThalamusByChance = Rand.Value(Rand.RandSync.ServerAndClient) <= Level.Loaded.GenerationParams.ThalamusProbability;
					bool subHasThalamusItems = sub.GetItems(false).Any((Item i) => i.Prefab.HasSubCategory("thalamus"));
					bool flag;
					switch (thalamusSpawn)
					{
					case LevelData.ThalamusSpawn.Random:
						flag = spawnThalamusByChance;
						break;
					case LevelData.ThalamusSpawn.Forced:
						flag = true;
						break;
					case LevelData.ThalamusSpawn.Disabled:
						flag = false;
						break;
					default:
						flag = false;
						break;
					}
					bool spawnThalamus = flag;
					if (spawnThalamus && subHasThalamusItems)
					{
						if (!sub.CreateWreckAI())
						{
							DebugConsole.NewMessage("Failed to create wreck AI inside " + subName + ".", new Color?(Color.Red), false);
							sub.DisableWreckAI();
						}
					}
					else
					{
						sub.DisableWreckAI();
					}
				}
				else if (type == SubmarineType.BeaconStation)
				{
					this.PositionsOfInterest.Add(new Level.InterestingPosition(CS$<>8__locals1.spawnPoint.ToPoint(), Level.PositionType.BeaconStation, sub, true));
					sub.ShowSonarMarker = false;
					sub.DockedTo.ForEach(delegate(Submarine s)
					{
						s.ShowSonarMarker = false;
					});
					sub.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
					sub.TeamID = CharacterTeamType.None;
				}
				tempSW.Stop();
				sub.SetPosition(CS$<>8__locals1.spawnPoint, null, false);
				this.blockedRects.Add(sub, CS$<>8__locals1.rects);
				return sub;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(38, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Failed to position wreck ");
			defaultInterpolatedStringHandler2.AppendFormatted(subName);
			defaultInterpolatedStringHandler2.AppendLiteral(". Used ");
			defaultInterpolatedStringHandler2.AppendFormatted<long>(tempSW.ElapsedMilliseconds);
			defaultInterpolatedStringHandler2.AppendLiteral(" (ms).");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Red), false);
			return null;
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x00058EAC File Offset: 0x000570AC
		private void CreateWrecks()
		{
			Stopwatch totalSW = new Stopwatch();
			totalSW.Start();
			this.ResetRandomSeed();
			if (LevelData.ConsoleForceWreck != null)
			{
				this.LevelData.ForceWreck = LevelData.ConsoleForceWreck;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode && !networkMember.ServerSettings.PvPSpawnWrecks)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.NewMessage("PvP setting: Skipping wreck generation", new Color?(Color.Yellow), false);
					}
					this.Wrecks = new List<Submarine>();
					return;
				}
			}
			IEnumerable<WreckFile> source = from f in ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<WreckFile>())
			orderby f.UintIdentifier
			select f;
			Func<WreckFile, Option<Level.PlaceableWreck>> selector;
			if ((selector = Level.<>O.<3>__TryCreate) == null)
			{
				selector = (Level.<>O.<3>__TryCreate = new Func<WreckFile, Option<Level.PlaceableWreck>>(Level.PlaceableWreck.TryCreate));
			}
			List<Level.PlaceableWreck> placeableWrecks = (from w in source.Select(selector)
			where w.IsSome()
			select w).Select(delegate(Option<Level.PlaceableWreck> o)
			{
				Level.PlaceableWreck w;
				if (!o.TryUnwrap(out w))
				{
					throw new InvalidOperationException();
				}
				return w;
			}).ToList<Level.PlaceableWreck>();
			if (this.LevelData.ForceWreck != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Level Generation - Forcing wreck ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.LevelData.ForceWreck.DisplayName);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			}
			else
			{
				for (int i = placeableWrecks.Count - 1; i >= 0; i--)
				{
					WreckInfo wreckInfo = placeableWrecks[i].WreckInfo;
					if (!this.IsAllowedDifficulty(wreckInfo.MinLevelDifficulty, wreckInfo.MaxLevelDifficulty))
					{
						placeableWrecks.RemoveAt(i);
					}
					else if (wreckInfo.MissionTags.Count != 0)
					{
						placeableWrecks.RemoveAt(i);
					}
				}
				if (placeableWrecks.None(null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(47, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("No wreck files found for the level difficulty ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(this.LevelData.Difficulty);
					defaultInterpolatedStringHandler2.AppendLiteral("!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					this.Wrecks = new List<Submarine>();
					return;
				}
				placeableWrecks.Shuffle(Rand.RandSync.ServerAndClient);
			}
			int minWreckCount = Math.Min(Level.Loaded.GenerationParams.MinWreckCount, placeableWrecks.Count);
			int maxWreckCount = Math.Min(Level.Loaded.GenerationParams.MaxWreckCount, placeableWrecks.Count);
			int wreckCount = Rand.Range(minWreckCount, maxWreckCount + 1, Rand.RandSync.ServerAndClient);
			bool requireThalamus = false;
			GameSession gameSession2 = GameMain.GameSession;
			bool? flag;
			if (gameSession2 == null)
			{
				flag = null;
			}
			else
			{
				GameMode gameMode = gameSession2.GameMode;
				if (gameMode == null)
				{
					flag = null;
				}
				else
				{
					flag = new bool?(gameMode.Missions.Any((Mission m) => m.Prefab.RequireWreck));
				}
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault())
			{
				wreckCount = Math.Max(wreckCount, 1);
			}
			GameSession gameSession3 = GameMain.GameSession;
			bool? flag3;
			if (gameSession3 == null)
			{
				flag3 = null;
			}
			else
			{
				GameMode gameMode2 = gameSession3.GameMode;
				if (gameMode2 == null)
				{
					flag3 = null;
				}
				else
				{
					flag3 = new bool?(gameMode2.Missions.Any((Mission m) => m.Prefab.RequireThalamusWreck));
				}
			}
			flag2 = flag3;
			if (flag2.GetValueOrDefault())
			{
				requireThalamus = true;
			}
			if (this.LevelData.ForceWreck != null)
			{
				Level.PlaceableWreck matchingFile = placeableWrecks.FirstOrDefault((Level.PlaceableWreck wreck) => wreck.WreckFile.Path == this.LevelData.ForceWreck.FilePath);
				if (matchingFile.WreckFile != null)
				{
					placeableWrecks.Remove(matchingFile);
					placeableWrecks.Insert(0, matchingFile);
					if (LevelData.ForceThalamus == LevelData.ThalamusSpawn.Forced && matchingFile.WreckInfo.WreckContainsThalamus == WreckInfo.HasThalamus.No)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(34, 1);
						defaultInterpolatedStringHandler3.AppendLiteral("Forced wreck ");
						defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(this.LevelData.ForceWreck.DisplayName);
						defaultInterpolatedStringHandler3.AppendLiteral(" can't have thalamus!");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
					}
				}
				wreckCount = Math.Max(wreckCount, 1);
			}
			else if (requireThalamus)
			{
				List<Level.PlaceableWreck> thalamusWrecks = (from w in placeableWrecks
				where w.WreckInfo.WreckContainsThalamus == WreckInfo.HasThalamus.Yes
				select w).ToList<Level.PlaceableWreck>();
				if (thalamusWrecks.Any<Level.PlaceableWreck>())
				{
					thalamusWrecks.Shuffle(Rand.RandSync.ServerAndClient);
					foreach (Level.PlaceableWreck wreck3 in thalamusWrecks)
					{
						placeableWrecks.Remove(wreck3);
						placeableWrecks.Insert(0, wreck3);
					}
				}
			}
			this.Wrecks = new List<Submarine>(wreckCount);
			for (int j = 0; j < wreckCount; j++)
			{
				int attempts = 0;
				while (placeableWrecks.Any<Level.PlaceableWreck>() && attempts < 2)
				{
					WreckFile wreckFile = placeableWrecks.First<Level.PlaceableWreck>().WreckFile;
					if (this.LevelData.ForceWreck == null)
					{
						placeableWrecks.RemoveAt(0);
					}
					LevelData.ThalamusSpawn thalamusSpawn = requireThalamus ? LevelData.ThalamusSpawn.Forced : LevelData.ThalamusSpawn.Random;
					if (this.LevelData.ForceWreck != null)
					{
						thalamusSpawn = LevelData.ForceThalamus;
					}
					NetworkMember netMember = GameMain.NetworkMember;
					if (netMember != null)
					{
						GameSession gameSession4 = GameMain.GameSession;
						if (((gameSession4 != null) ? gameSession4.GameMode : null) is PvPMode && !netMember.ServerSettings.PvPSpawnMonsters)
						{
							thalamusSpawn = LevelData.ThalamusSpawn.Disabled;
						}
					}
					if (wreckFile != null)
					{
						string wreckName = Path.GetFileNameWithoutExtension(wreckFile.Path.Value);
						if (this.SpawnSubOnPath(wreckName, wreckFile, SubmarineType.Wreck, thalamusSpawn, false) != null)
						{
							break;
						}
						attempts++;
					}
				}
			}
			foreach (Submarine wreck2 in this.Wrecks)
			{
				wreck2.SetCrushDepth(wreck2.RealWorldDepth + 1000f);
				this.SetLinkedSubCrushDepth(wreck2);
			}
			totalSW.Stop();
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x000594AC File Offset: 0x000576AC
		private bool HasStartOutpost()
		{
			return this.preSelectedStartOutpost != null || ((this.LevelData.Type == LevelData.LevelType.Outpost || Level.IsModeStartOutpostCompatible()) && (this.StartLocation == null || this.StartLocation.Type.HasOutpost));
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x000594EC File Offset: 0x000576EC
		private bool HasEndOutpost()
		{
			return this.preSelectedEndOutpost != null || (this.LevelData.Type != LevelData.LevelType.Outpost && (this.EndLocation == null || this.EndLocation.Type.HasOutpost));
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x00059528 File Offset: 0x00057728
		private void CreateOutposts()
		{
			this.ResetRandomSeed();
			List<OutpostFile> outpostFiles = (from f in ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<OutpostFile>())
			orderby f.UintIdentifier
			select f).ToList<OutpostFile>();
			if (!outpostFiles.Any<OutpostFile>() && !OutpostGenerationParams.OutpostParams.Any<OutpostGenerationParams>() && this.LevelData.ForceOutpostGenerationParams == null)
			{
				DebugConsole.ThrowError("No outpost files found in the selected content packages", null, null, false, false);
				return;
			}
			int i = 0;
			while (i < 2)
			{
				bool isStart = i == 0 == !this.Mirrored;
				if (isStart)
				{
					if (this.HasStartOutpost())
					{
						goto IL_C1;
					}
				}
				else if (this.HasEndOutpost())
				{
					goto IL_C1;
				}
				IL_B88:
				i++;
				continue;
				IL_C1:
				Submarine outpost = null;
				SubmarineInfo preSelectedOutpost = isStart ? this.preSelectedStartOutpost : this.preSelectedEndOutpost;
				if (preSelectedOutpost == null)
				{
					if (this.LevelData.OutpostGenerationParamsExist)
					{
						Location location = isStart ? this.StartLocation : this.EndLocation;
						OutpostGenerationParams outpostGenerationParams = null;
						Identifier missionForcedOutpostParamsId = Identifier.Empty;
						GameSession gameSession = GameMain.GameSession;
						IEnumerable<Mission> enumerable;
						if (gameSession == null)
						{
							enumerable = null;
						}
						else
						{
							GameMode gameMode = gameSession.GameMode;
							enumerable = ((gameMode != null) ? gameMode.Missions : null);
						}
						IEnumerable<Mission> missions = enumerable;
						if (missions != null)
						{
							foreach (Mission mission in missions)
							{
								if (!mission.Prefab.ForceOutpostGenerationParameters.IsEmpty)
								{
									missionForcedOutpostParamsId = mission.Prefab.ForceOutpostGenerationParameters;
									break;
								}
							}
						}
						bool onlyEntrance = this.LevelData.Type != LevelData.LevelType.Outpost;
						LocationType locationType = (location != null) ? location.Type : null;
						OutpostGenerationParams missionForcedOutpostParams;
						if (missionForcedOutpostParamsId != null && OutpostGenerationParams.OutpostParams.TryGet(missionForcedOutpostParamsId, out missionForcedOutpostParams))
						{
							outpostGenerationParams = missionForcedOutpostParams;
						}
						else if (this.LevelData.ForceOutpostGenerationParams != null)
						{
							outpostGenerationParams = this.LevelData.ForceOutpostGenerationParams;
						}
						else
						{
							if (locationType != null)
							{
								OutpostGenerationParams forcedOutpostGenerationParams = locationType.GetForcedOutpostGenerationParams();
								if (forcedOutpostGenerationParams != null && (!onlyEntrance || forcedOutpostGenerationParams.OutpostFilePath.IsNullOrEmpty()))
								{
									outpostGenerationParams = forcedOutpostGenerationParams;
									goto IL_244;
								}
							}
							outpostGenerationParams = (this.LevelData.ForceOutpostGenerationParams ?? LevelData.GetSuitableOutpostGenerationParams(location, this.LevelData).GetRandom(Rand.RandSync.ServerAndClient));
						}
						IL_244:
						if (locationType == null)
						{
							locationType = LocationType.Prefabs.GetRandom(Rand.RandSync.ServerAndClient);
							if (outpostGenerationParams.AllowedLocationTypes.Any<Identifier>())
							{
								locationType = LocationType.Prefabs.GetRandom((LocationType lt) => outpostGenerationParams.AllowedLocationTypes.Any((Identifier allowedType) => allowedType == "any" || lt.Identifier == allowedType), Rand.RandSync.ServerAndClient);
							}
						}
						if (location != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(73, 3);
							defaultInterpolatedStringHandler.AppendLiteral("Generating an outpost for the ");
							defaultInterpolatedStringHandler.AppendFormatted(isStart ? "start" : "end");
							defaultInterpolatedStringHandler.AppendLiteral(" of the level... (Location: ");
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(location.DisplayName);
							defaultInterpolatedStringHandler.AppendLiteral(", level type: ");
							defaultInterpolatedStringHandler.AppendFormatted<LevelData.LevelType>(this.LevelData.Type);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
							outpost = OutpostGenerator.Generate(outpostGenerationParams, location, onlyEntrance, this.LevelData.AllowInvalidOutpost);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(78, 3);
							defaultInterpolatedStringHandler2.AppendLiteral("Generating an outpost for the ");
							defaultInterpolatedStringHandler2.AppendFormatted(isStart ? "start" : "end");
							defaultInterpolatedStringHandler2.AppendLiteral(" of the level... (Location type: ");
							defaultInterpolatedStringHandler2.AppendFormatted<LocationType>(locationType);
							defaultInterpolatedStringHandler2.AppendLiteral(", level type: ");
							defaultInterpolatedStringHandler2.AppendFormatted<LevelData.LevelType>(this.LevelData.Type);
							defaultInterpolatedStringHandler2.AppendLiteral(")");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
							outpost = OutpostGenerator.Generate(outpostGenerationParams, locationType, this.LevelData.Type != LevelData.LevelType.Outpost, this.LevelData.AllowInvalidOutpost);
						}
						using (List<string>.Enumerator enumerator2 = locationType.HideEntitySubcategories.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								string categoryToHide = enumerator2.Current;
								IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
								Func<MapEntity, bool> predicate;
								Func<MapEntity, bool> <>9__4;
								if ((predicate = <>9__4) == null)
								{
									predicate = (<>9__4 = delegate(MapEntity me)
									{
										if (me.Submarine == outpost)
										{
											MapEntityPrefab prefab = me.Prefab;
											return prefab != null && prefab.HasSubCategory(categoryToHide);
										}
										return false;
									});
								}
								foreach (MapEntity entityToHide in mapEntityList.Where(predicate))
								{
									entityToHide.IsLayerHidden = true;
								}
							}
							goto IL_544;
						}
					}
					DebugConsole.NewMessage("Loading a pre-built outpost for the " + (isStart ? "start" : "end") + " of the level...", null, false);
					ContentFile outpostFile = outpostFiles.GetRandom(Rand.RandSync.ServerAndClient);
					SubmarineInfo outpostInfo = new SubmarineInfo(outpostFile.Path.Value, "", null, true, false)
					{
						Type = SubmarineType.Outpost
					};
					outpost = new Submarine(outpostInfo, true, null, null);
				}
				else
				{
					DebugConsole.NewMessage("Loading a pre-selected outpost for the " + (isStart ? "start" : "end") + " of the level...", null, false);
					SubmarineInfo outpostInfo = preSelectedOutpost;
					outpostInfo.Type = SubmarineType.Outpost;
					outpost = new Submarine(outpostInfo, true, null, null);
				}
				IL_544:
				Point? minSize = null;
				DockingPort subPort = null;
				float closestDistance = float.MaxValue;
				if (Submarine.MainSub != null)
				{
					Point subSize = Submarine.MainSub.GetDockedBorders(true).Size;
					Point outpostSize = outpost.GetDockedBorders(true).Size;
					minSize = new Point?(new Point(Math.Max(subSize.X, outpostSize.X), subSize.Y + outpostSize.Y));
					foreach (DockingPort port in DockingPort.List)
					{
						if (!port.IsHorizontal && !port.Docked && port.Item.Submarine == Submarine.MainSub && port.Item.WorldPosition.Y >= Submarine.MainSub.WorldPosition.Y)
						{
							float dist = Math.Abs(port.Item.WorldPosition.X - Submarine.MainSub.WorldPosition.X);
							if (dist < closestDistance || subPort.MainDockingPort)
							{
								subPort = port;
								closestDistance = dist;
							}
						}
					}
				}
				Vector2 spawnPos;
				if (this.GenerationParams.ForceOutpostPosition != Vector2.Zero)
				{
					spawnPos = new Vector2((float)this.Size.X * this.GenerationParams.ForceOutpostPosition.X, (float)this.Size.Y * this.GenerationParams.ForceOutpostPosition.Y);
				}
				else
				{
					DockingPort outpostPort = null;
					closestDistance = float.MaxValue;
					foreach (DockingPort port2 in DockingPort.List)
					{
						if (!port2.IsHorizontal && !port2.Docked && port2.Item.Submarine == outpost && port2.Item.WorldPosition.Y <= outpost.WorldPosition.Y)
						{
							float dist2 = Math.Abs(port2.Item.WorldPosition.X - outpost.WorldPosition.X);
							if (dist2 < closestDistance)
							{
								outpostPort = port2;
								closestDistance = dist2;
							}
						}
					}
					float subDockingPortOffset = (subPort == null) ? 0f : (subPort.Item.WorldPosition.X - Submarine.MainSub.WorldPosition.X);
					if (Math.Abs(subDockingPortOffset) > 5000f)
					{
						subDockingPortOffset = MathHelper.Clamp(subDockingPortOffset, -5000f, 5000f);
						string warningMsg = string.Concat(new string[]
						{
							"Docking port very far from the sub's center of mass (submarine: ",
							Submarine.MainSub.Info.Name,
							", dist: ",
							subDockingPortOffset.ToString(),
							"). The level generator may not be able to place the outpost so that docking is possible."
						});
						DebugConsole.NewMessage(warningMsg, new Color?(Color.Orange), false);
						GameAnalyticsManager.AddErrorEventOnce("Lever.CreateOutposts:DockingPortVeryFar" + Submarine.MainSub.Info.Name, GameAnalyticsManager.ErrorSeverity.Warning, warningMsg);
					}
					float? outpostDockingPortOffset = null;
					if (outpostPort != null)
					{
						outpostDockingPortOffset = new float?((subPort == null) ? 0f : (outpostPort.Item.WorldPosition.X - outpost.WorldPosition.X));
						if (Math.Abs(outpostDockingPortOffset.Value) > 5000f)
						{
							outpostDockingPortOffset = new float?(MathHelper.Clamp(outpostDockingPortOffset.Value, -5000f, 5000f));
							string[] array = new string[5];
							array[0] = "Docking port very far from the outpost's center of mass (outpost: ";
							array[1] = outpost.Info.Name;
							array[2] = ", dist: ";
							int num = 3;
							float? num2 = outpostDockingPortOffset;
							array[num] = num2.ToString();
							array[4] = "). The level generator may not be able to place the outpost so that docking is possible.";
							string warningMsg2 = string.Concat(array);
							DebugConsole.NewMessage(warningMsg2, new Color?(Color.Orange), false);
							GameAnalyticsManager.AddErrorEventOnce("Lever.CreateOutposts:OutpostDockingPortVeryFar" + outpost.Info.Name, GameAnalyticsManager.ErrorSeverity.Warning, warningMsg2);
						}
					}
					Vector2 preferredSpawnPos = (i == 0) ? this.StartPosition : this.EndPosition;
					if (i == 1 && this.GenerationParams.CreateHoleNextToEnd && preferredSpawnPos.X > (float)this.Size.X * 0.75f && preferredSpawnPos.Y < (float)this.Size.Y * 0.25f)
					{
						preferredSpawnPos.X = (preferredSpawnPos.X + (float)this.Size.X) / 2f;
					}
					spawnPos = outpost.FindSpawnPos(preferredSpawnPos, minSize, (outpostDockingPortOffset != null) ? (subDockingPortOffset - outpostDockingPortOffset.Value) : 0f, 1);
					if (this.Type == LevelData.LevelType.Outpost)
					{
						spawnPos.Y = Math.Min((float)this.Size.Y - (float)outpost.Borders.Height * 0.6f, spawnPos.Y + (float)(outpost.Borders.Height / 2));
					}
				}
				outpost.SetPosition(spawnPos, null, false);
				this.SetLinkedSubCrushDepth(outpost);
				foreach (WayPoint wp in WayPoint.WayPointList)
				{
					if (wp.Submarine == outpost && wp.SpawnType != SpawnType.Path)
					{
						this.PositionsOfInterest.Add(new Level.InterestingPosition(wp.WorldPosition.ToPoint(), Level.PositionType.Outpost, outpost, true));
					}
				}
				if (i == 0 == !this.Mirrored)
				{
					this.StartOutpost = outpost;
					if (this.StartLocation != null)
					{
						outpost.TeamID = this.StartLocation.Type.OutpostTeam;
						outpost.Info.Name = this.StartLocation.DisplayName.Value;
						goto IL_B88;
					}
					goto IL_B88;
				}
				else
				{
					this.EndOutpost = outpost;
					if (this.EndLocation != null)
					{
						outpost.TeamID = this.EndLocation.Type.OutpostTeam;
						outpost.Info.Name = this.EndLocation.DisplayName.Value;
						goto IL_B88;
					}
					goto IL_B88;
				}
			}
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0005A114 File Offset: 0x00058314
		private void CreateBeaconStation()
		{
			this.ResetRandomSeed();
			if (LevelData.ConsoleForceBeaconStation != null)
			{
				this.LevelData.ForceBeaconStation = LevelData.ConsoleForceBeaconStation;
			}
			GameSession gameSession = GameMain.GameSession;
			bool? flag;
			if (gameSession == null)
			{
				flag = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				if (gameMode == null)
				{
					flag = null;
				}
				else
				{
					flag = new bool?(gameMode.Missions.Any((Mission m) => m.Prefab.RequireBeaconStation));
				}
			}
			bool? flag2 = flag;
			if (!flag2.GetValueOrDefault() && !this.LevelData.HasBeaconStation && this.LevelData.ForceBeaconStation == null && string.IsNullOrEmpty(this.GenerationParams.ForceBeaconStation))
			{
				return;
			}
			GameSession gameSession2 = GameMain.GameSession;
			bool? flag3;
			if (gameSession2 == null)
			{
				flag3 = null;
			}
			else
			{
				GameMode gameMode2 = gameSession2.GameMode;
				if (gameMode2 == null)
				{
					flag3 = null;
				}
				else
				{
					flag3 = new bool?(gameMode2.Missions.Any((Mission m) => m.Prefab.RequireBeaconStation && m.Prefab.SpawnBeaconStationInMiddle));
				}
			}
			flag2 = flag3;
			bool spawnInMiddle = flag2.GetValueOrDefault();
			List<BeaconStationFile> beaconStationFiles = (from f in ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<BeaconStationFile>())
			orderby f.UintIdentifier
			select f).ToList<BeaconStationFile>();
			if (beaconStationFiles.None(null))
			{
				DebugConsole.ThrowError("No BeaconStation files found in the selected content packages!", null, null, false, false);
				return;
			}
			IEnumerable<SubmarineInfo> beaconInfos = from i in SubmarineInfo.SavedSubmarines
			where i.IsBeacon
			select i;
			ContentFile contentFile = null;
			if (!string.IsNullOrEmpty(this.GenerationParams.ForceBeaconStation))
			{
				ContentPath contentPath = ContentPath.FromRaw(this.GenerationParams.ContentPackage, this.GenerationParams.ForceBeaconStation);
				contentFile = (from b in beaconStationFiles
				orderby b.UintIdentifier
				select b).FirstOrDefault((BeaconStationFile f) => f.Path == contentPath);
				if (contentFile == null)
				{
					DebugConsole.ThrowError("Failed to find the beacon station \"" + this.GenerationParams.ForceBeaconStation + "\". Using a random one instead...", null, null, false, false);
				}
			}
			else if (this.LevelData.ForceBeaconStation != null)
			{
				contentFile = beaconStationFiles.FirstOrDefault((BeaconStationFile b) => b.Path == this.LevelData.ForceBeaconStation.FilePath);
			}
			if (contentFile == null)
			{
				for (int j = beaconStationFiles.Count - 1; j >= 0; j--)
				{
					BeaconStationFile beaconStationFile = beaconStationFiles[j];
					SubmarineInfo matchingInfo = beaconInfos.SingleOrDefault((SubmarineInfo info) => info.FilePath == beaconStationFile.Path.Value);
					BeaconStationInfo beaconInfo = (matchingInfo != null) ? matchingInfo.BeaconStationInfo : null;
					if (beaconInfo != null)
					{
						if (this.LevelData.Difficulty < beaconInfo.MinLevelDifficulty || this.LevelData.Difficulty > beaconInfo.MaxLevelDifficulty)
						{
							beaconStationFiles.RemoveAt(j);
						}
						else if (beaconInfo.MissionTags.Count != 0)
						{
							beaconStationFiles.RemoveAt(j);
						}
					}
				}
				if (beaconStationFiles.None(null))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 1);
					defaultInterpolatedStringHandler.AppendLiteral("No BeaconStation files found for the level difficulty ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(this.LevelData.Difficulty);
					defaultInterpolatedStringHandler.AppendLiteral("!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
				contentFile = beaconStationFiles.GetRandom(Rand.RandSync.ServerAndClient);
			}
			string beaconStationName = Path.GetFileNameWithoutExtension(contentFile.Path.Value);
			this.BeaconStation = this.SpawnSubOnPath(beaconStationName, contentFile, SubmarineType.BeaconStation, LevelData.ThalamusSpawn.Random, spawnInMiddle);
			if (this.BeaconStation == null)
			{
				this.LevelData.HasBeaconStation = false;
				return;
			}
			Item sonarItem = Item.ItemList.Find((Item it) => it.Submarine == this.BeaconStation && it.GetComponent<Sonar>() != null);
			if (sonarItem == null)
			{
				DebugConsole.ThrowError("No sonar found in the beacon station \"" + beaconStationName + "\"!", null, null, false, false);
				return;
			}
			this.beaconSonar = sonarItem.GetComponent<Sonar>();
			this.beaconTransducers = sonarItem.GetConnectedComponents<SonarTransducer>(false, true, null).ToImmutableArray<SonarTransducer>();
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x0005A524 File Offset: 0x00058724
		public void PrepareBeaconStation()
		{
			if (!this.LevelData.HasBeaconStation)
			{
				return;
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsClient)
			{
				return;
			}
			if (this.BeaconStation == null)
			{
				throw new InvalidOperationException("Failed to prepare beacon station (no beacon station in the level).");
			}
			List<Item> beaconItems = Item.ItemList.FindAll((Item it) => it.Submarine == this.BeaconStation);
			Item reactorItem = beaconItems.Find((Item it) => it.GetComponent<Reactor>() != null);
			Reactor reactorComponent = null;
			ItemContainer reactorContainer = null;
			if (reactorItem != null)
			{
				reactorComponent = reactorItem.GetComponent<Reactor>();
				reactorComponent.FuelConsumptionRate = 0f;
				reactorContainer = reactorItem.GetComponent<ItemContainer>();
				Repairable repairable = reactorItem.GetComponent<Repairable>();
				if (repairable != null)
				{
					repairable.DeteriorationSpeed = 0f;
				}
			}
			if (this.LevelData.IsBeaconActive)
			{
				if (reactorContainer != null && reactorContainer.Inventory.IsEmpty() && reactorContainer.ContainableItemIdentifiers.Any<Identifier>() && ItemPrefab.Prefabs.ContainsKey(reactorContainer.ContainableItemIdentifiers.FirstOrDefault<Identifier>()))
				{
					ItemPrefab fuelPrefab = ItemPrefab.Prefabs[reactorContainer.ContainableItemIdentifiers.FirstOrDefault<Identifier>()];
					Entity.Spawner.AddItemToSpawnQueue(fuelPrefab, reactorContainer.Inventory, null, null, delegate(Item it)
					{
						reactorComponent.PowerUpImmediately();
					}, true, false, InvSlotType.None);
				}
				if (this.beaconSonar == null)
				{
					DebugConsole.AddWarning("Beacon station \"" + this.BeaconStation.Info.Name + "\" has no sonar. Beacon missions might not work correctly with this beacon station.", null);
				}
				else
				{
					this.beaconSonar.CurrentMode = Sonar.Mode.Active;
					this.beaconSonar.Item.CreateServerEvent<Sonar>(this.beaconSonar);
				}
			}
			this.SetLinkedSubCrushDepth(this.BeaconStation);
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x0005A6E4 File Offset: 0x000588E4
		public void DisconnectBeaconStationWires(float disconnectWireProbability)
		{
			Submarine beaconStation = this.BeaconStation;
			BeaconStationInfo beaconStationInfo;
			if (beaconStation == null)
			{
				beaconStationInfo = null;
			}
			else
			{
				SubmarineInfo info = beaconStation.Info;
				beaconStationInfo = ((info != null) ? info.BeaconStationInfo : null);
			}
			BeaconStationInfo beaconStationInfo2 = beaconStationInfo;
			if (beaconStationInfo2 != null && !beaconStationInfo2.AllowDisconnectedWires)
			{
				return;
			}
			if (disconnectWireProbability <= 0f)
			{
				return;
			}
			List<Item> beaconItems = Item.ItemList.FindAll((Item it) => it.Submarine == this.BeaconStation);
			foreach (Item item in (from it in beaconItems
			where it.GetComponent<Wire>() != null
			select it).ToList<Item>())
			{
				if (!item.NonInteractable && !item.InvulnerableToDamage)
				{
					Wire wire = item.GetComponent<Wire>();
					if (!wire.Locked && (wire.Connections[0] == null || (!wire.Connections[0].Item.NonInteractable && !wire.Connections[0].Item.GetComponent<ConnectionPanel>().Locked)) && (wire.Connections[1] == null || (!wire.Connections[1].Item.NonInteractable && !wire.Connections[1].Item.GetComponent<ConnectionPanel>().Locked)) && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < disconnectWireProbability)
					{
						foreach (Connection connection in wire.Connections)
						{
							if (connection != null)
							{
								connection.ConnectionPanel.DisconnectedWires.Add(wire);
								wire.RemoveConnection(connection.Item);
								connection.ConnectionPanel.Item.CreateServerEvent<ConnectionPanel>(connection.ConnectionPanel);
								wire.CreateNetworkEvent();
							}
						}
					}
				}
			}
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x0005A8D4 File Offset: 0x00058AD4
		public void DamageBeaconStationDevices(float breakDeviceProbability)
		{
			Submarine beaconStation = this.BeaconStation;
			BeaconStationInfo beaconStationInfo;
			if (beaconStation == null)
			{
				beaconStationInfo = null;
			}
			else
			{
				SubmarineInfo info = beaconStation.Info;
				beaconStationInfo = ((info != null) ? info.BeaconStationInfo : null);
			}
			BeaconStationInfo beaconStationInfo2 = beaconStationInfo;
			if (beaconStationInfo2 != null && !beaconStationInfo2.AllowDamagedDevices)
			{
				return;
			}
			if (breakDeviceProbability <= 0f)
			{
				return;
			}
			List<Item> beaconItems = Item.ItemList.FindAll((Item it) => it.Submarine == this.BeaconStation);
			foreach (Item item in beaconItems.Where(delegate(Item it)
			{
				if (it.Components.Any((ItemComponent c) => c is Powered))
				{
					return it.Components.Any((ItemComponent c) => c is Repairable);
				}
				return false;
			}))
			{
				if (!item.NonInteractable && !item.InvulnerableToDamage && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < breakDeviceProbability)
				{
					item.Condition *= Rand.Range(0.6f, 0.8f, Rand.RandSync.Unsynced);
				}
			}
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x0005A9C4 File Offset: 0x00058BC4
		public void DamageBeaconStationWalls(float damageWallProbability)
		{
			Submarine beaconStation = this.BeaconStation;
			BeaconStationInfo beaconStationInfo;
			if (beaconStation == null)
			{
				beaconStationInfo = null;
			}
			else
			{
				SubmarineInfo info = beaconStation.Info;
				beaconStationInfo = ((info != null) ? info.BeaconStationInfo : null);
			}
			BeaconStationInfo beaconStationInfo2 = beaconStationInfo;
			if (beaconStationInfo2 != null && !beaconStationInfo2.AllowDamagedWalls)
			{
				return;
			}
			if (damageWallProbability <= 0f)
			{
				return;
			}
			foreach (Structure structure in from s in Structure.WallList
			where s.Submarine == this.BeaconStation
			select s)
			{
				if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < damageWallProbability)
				{
					int sectionIndex = Rand.Range(0, structure.SectionCount - 1, Rand.RandSync.Unsynced);
					structure.AddDamage(sectionIndex, Rand.Range(structure.MaxHealth * 0.2f, structure.MaxHealth, Rand.RandSync.Unsynced), null, true, false);
				}
			}
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x0005AA94 File Offset: 0x00058C94
		public bool CheckBeaconActive()
		{
			if (this.beaconSonar == null)
			{
				return false;
			}
			if (this.beaconSonar.UseTransducers)
			{
				List<SonarTransducer> connectedTransducers = this.beaconSonar.Item.GetConnectedComponents<SonarTransducer>(false, true, null);
				foreach (SonarTransducer beaconTransducer in this.beaconTransducers)
				{
					if (!beaconTransducer.HasPower || !connectedTransducers.Contains(beaconTransducer))
					{
						return false;
					}
				}
			}
			return this.beaconSonar.HasPower && this.beaconSonar.CurrentMode == Sonar.Mode.Active;
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x0005AB1C File Offset: 0x00058D1C
		private void SetLinkedSubCrushDepth(Submarine parentSub)
		{
			foreach (Submarine connectedSub in parentSub.GetConnectedSubs())
			{
				connectedSub.SetCrushDepth(Math.Max(connectedSub.RealWorldCrushDepth, this.GetRealWorldDepth(0f) + 1000f));
			}
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x0005AB84 File Offset: 0x00058D84
		private static bool IsModeStartOutpostCompatible()
		{
			GameSession gameSession = GameMain.GameSession;
			return ((gameSession != null) ? gameSession.GameMode : null) is CampaignMode;
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0005ABA0 File Offset: 0x00058DA0
		public void SpawnCorpses()
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			using (List<Submarine>.Enumerator enumerator = this.Wrecks.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Level.<>c__DisplayClass269_0 CS$<>8__locals1 = new Level.<>c__DisplayClass269_0();
					CS$<>8__locals1.wreck = enumerator.Current;
					int corpseCount = Rand.Range(Level.Loaded.GenerationParams.MinCorpseCount, Level.Loaded.GenerationParams.MaxCorpseCount + 1, Rand.RandSync.Unsynced);
					List<WayPoint> allSpawnPoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.Submarine == CS$<>8__locals1.wreck && wp.CurrentHull != null);
					List<WayPoint> humanSpawnPoints = allSpawnPoints.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Human);
					List<WayPoint> corpsePoints = allSpawnPoints.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Corpse);
					if (!corpsePoints.None(null) || !humanSpawnPoints.None(null))
					{
						humanSpawnPoints.Shuffle(Rand.RandSync.ServerAndClient);
						corpsePoints = (from p in corpsePoints
						orderby p.AssignedJob == null, Rand.Value(Rand.RandSync.Unsynced)
						select p).ToList<WayPoint>();
						HashSet<JobPrefab> usedJobs = new HashSet<JobPrefab>();
						int spawnCounter = 0;
						Func<JobPrefab, bool> <>9__9;
						for (int i = 0; i < corpseCount; i++)
						{
							Level.<>c__DisplayClass269_2 CS$<>8__locals3 = new Level.<>c__DisplayClass269_2();
							WayPoint sp2 = corpsePoints.FirstOrDefault<WayPoint>() ?? humanSpawnPoints.FirstOrDefault<WayPoint>();
							CS$<>8__locals3.job = ((sp2 != null) ? sp2.AssignedJob : null);
							CorpsePrefab selectedPrefab;
							if (CS$<>8__locals3.job == null)
							{
								selectedPrefab = Level.<SpawnCorpses>g__GetCorpsePrefab|269_10(usedJobs, null);
							}
							else
							{
								selectedPrefab = Level.<SpawnCorpses>g__GetCorpsePrefab|269_10(usedJobs, delegate(CorpsePrefab p)
								{
									Identifier job = p.Job;
									if (!(job == "any"))
									{
										Identifier job2 = p.Job;
										return job2 == CS$<>8__locals3.job.Identifier;
									}
									return true;
								});
								if (selectedPrefab == null)
								{
									corpsePoints.Remove(sp2);
									humanSpawnPoints.Remove(sp2);
									WayPoint wayPoint;
									if ((wayPoint = corpsePoints.FirstOrDefault((WayPoint sp) => sp.AssignedJob == null)) == null)
									{
										wayPoint = humanSpawnPoints.FirstOrDefault((WayPoint sp) => sp.AssignedJob == null);
									}
									sp2 = wayPoint;
									selectedPrefab = Level.<SpawnCorpses>g__GetCorpsePrefab|269_10(usedJobs, null);
									if (selectedPrefab != null)
									{
										CS$<>8__locals3.job = selectedPrefab.GetJobPrefab(Rand.RandSync.Unsynced, null);
									}
								}
							}
							if (selectedPrefab != null)
							{
								Vector2 worldPos;
								if (sp2 == null)
								{
									if (!CS$<>8__locals1.<SpawnCorpses>g__TryGetExtraSpawnPoint|5(out worldPos))
									{
										break;
									}
								}
								else
								{
									worldPos = sp2.WorldPosition;
									corpsePoints.Remove(sp2);
									humanSpawnPoints.Remove(sp2);
								}
								if (CS$<>8__locals3.job == null)
								{
									Level.<>c__DisplayClass269_2 CS$<>8__locals4 = CS$<>8__locals3;
									HumanPrefab humanPrefab = selectedPrefab;
									Rand.RandSync randSync = Rand.RandSync.Unsynced;
									Func<JobPrefab, bool> predicate;
									if ((predicate = <>9__9) == null)
									{
										predicate = (<>9__9 = ((JobPrefab p) => !usedJobs.Contains(p)));
									}
									CS$<>8__locals4.job = humanPrefab.GetJobPrefab(randSync, predicate);
								}
								if (CS$<>8__locals3.job != null)
								{
									if (CS$<>8__locals3.job.Identifier == "captain" || CS$<>8__locals3.job.Identifier == "engineer" || CS$<>8__locals3.job.Identifier == "medicaldoctor" || CS$<>8__locals3.job.Identifier == "securityofficer")
									{
										usedJobs.Add(CS$<>8__locals3.job);
									}
									CharacterInfo characterInfo = new CharacterInfo(CharacterPrefab.HumanSpeciesName, "", "", CS$<>8__locals3.job, 0, Rand.RandSync.Unsynced, default(Identifier));
									Character corpse = Character.Create(CharacterPrefab.HumanSpeciesName, worldPos, ToolBox.RandomSeed(8), characterInfo, 0, false, true, true, null, true, true);
									corpse.AnimController.FindHull(new Vector2?(worldPos), true, false);
									corpse.TeamID = CharacterTeamType.None;
									corpse.EnableDespawn = false;
									selectedPrefab.GiveItems(corpse, CS$<>8__locals1.wreck, sp2, Rand.RandSync.Unsynced, true);
									bool spawnAsHusk = Rand.Value(Rand.RandSync.Unsynced) <= Level.Loaded.GenerationParams.HuskProbability;
									if (spawnAsHusk)
									{
										corpse.TurnIntoHusk(null, new bool?(true));
									}
									else
									{
										corpse.Kill(CauseOfDeathType.Unknown, null, false, false);
										corpse.CharacterHealth.ApplyAffliction(corpse.AnimController.MainLimb, AfflictionPrefab.OxygenLow.Instantiate(AfflictionPrefab.OxygenLow.MaxStrength, null), true, false, true);
										bool applyBurns = Rand.Value(Rand.RandSync.Unsynced) < 0.1f;
										bool applyDamage = Rand.Value(Rand.RandSync.Unsynced) < 0.3f;
										foreach (Limb limb in corpse.AnimController.Limbs)
										{
											if (applyDamage && (limb.type == LimbType.Head || Rand.Value(Rand.RandSync.Unsynced) < 0.5f))
											{
												AfflictionPrefab prefab = AfflictionPrefab.BiteWounds;
												float max = prefab.MaxStrength / prefab.DamageOverlayAlpha;
												corpse.CharacterHealth.ApplyAffliction(limb, prefab.Instantiate(Level.<SpawnCorpses>g__GetStrength|269_11(limb, max), null), true, false, true);
											}
											if (applyBurns)
											{
												AfflictionPrefab prefab2 = AfflictionPrefab.Burn;
												float max2 = prefab2.MaxStrength / prefab2.BurnOverlayAlpha;
												corpse.CharacterHealth.ApplyAffliction(limb, prefab2.Instantiate(Level.<SpawnCorpses>g__GetStrength|269_11(limb, max2), null), true, false, true);
											}
										}
										corpse.CharacterHealth.ForceUpdateVisuals();
									}
									if (selectedPrefab.MinMoney >= 0 && selectedPrefab.MaxMoney > 0)
									{
										corpse.Wallet.Give(Rand.Range(selectedPrefab.MinMoney, selectedPrefab.MaxMoney, Rand.RandSync.Unsynced));
									}
									spawnCounter++;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x0005B144 File Offset: 0x00059344
		public void SpawnNPCs()
		{
			if (this.Type != LevelData.LevelType.Outpost)
			{
				return;
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				bool flag;
				if (sub == null)
				{
					flag = (null != null);
				}
				else
				{
					SubmarineInfo info = sub.Info;
					flag = (((info != null) ? info.OutpostGenerationParams : null) != null);
				}
				if (flag)
				{
					OutpostGenerator.SpawnNPCs(this.StartLocation, sub);
				}
			}
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x0005B1C0 File Offset: 0x000593C0
		public float GetRealWorldDepth(float worldPositionY)
		{
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) == null)
			{
				return (-(worldPositionY - (float)this.GenerationParams.Height) + 80000f) * Physics.DisplayToRealWorldRatio;
			}
			return (-(worldPositionY - (float)this.GenerationParams.Height) + (float)this.LevelData.InitialDepth) * Physics.DisplayToRealWorldRatio;
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x0005B21E File Offset: 0x0005941E
		public static bool IsPositionAboveLevel(Vector2 worldPosition)
		{
			return Level.Loaded != null && worldPosition.Y > (float)Level.Loaded.Size.Y;
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x0005B241 File Offset: 0x00059441
		public static bool IsPositionInAbyss(Vector2 worldPosition)
		{
			return Level.Loaded != null && worldPosition.Y < (float)Level.loaded.AbyssStart && worldPosition.Y > (float)Level.loaded.AbyssEnd;
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x0005B272 File Offset: 0x00059472
		public void DebugSetStartLocation(Location newStartLocation)
		{
			this.StartLocation = newStartLocation;
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0005B27B File Offset: 0x0005947B
		public void DebugSetEndLocation(Location newEndLocation)
		{
			this.EndLocation = newEndLocation;
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x0005B284 File Offset: 0x00059484
		public override void Remove()
		{
			base.Remove();
			if (this.LevelObjectManager != null)
			{
				this.LevelObjectManager.Remove();
				this.LevelObjectManager = null;
			}
			List<Level.AbyssIsland> abyssIslands = this.AbyssIslands;
			if (abyssIslands != null)
			{
				abyssIslands.Clear();
			}
			List<Level.ClusterLocation> abyssResources = this.AbyssResources;
			if (abyssResources != null)
			{
				abyssResources.Clear();
			}
			List<Level.Cave> caves = this.Caves;
			if (caves != null)
			{
				caves.Clear();
			}
			List<Level.Tunnel> tunnels = this.Tunnels;
			if (tunnels != null)
			{
				tunnels.Clear();
			}
			List<Level.PathPoint> pathPoints = this.PathPoints;
			if (pathPoints != null)
			{
				pathPoints.Clear();
			}
			List<Level.InterestingPosition> positionsOfInterest = this.PositionsOfInterest;
			if (positionsOfInterest != null)
			{
				positionsOfInterest.Clear();
			}
			Dictionary<string, List<Vector2>> dictionary = this.positionHistory;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			List<Submarine> wrecks = this.Wrecks;
			if (wrecks != null)
			{
				wrecks.Clear();
			}
			this.BeaconStation = null;
			this.beaconSonar = null;
			this.beaconTransducers = ImmutableArray<SonarTransducer>.Empty;
			this.StartOutpost = null;
			this.EndOutpost = null;
			Dictionary<Submarine, List<Rectangle>> dictionary2 = this.blockedRects;
			if (dictionary2 != null)
			{
				dictionary2.Clear();
			}
			List<Entity> entitiesBeforeGenerate = this.EntitiesBeforeGenerate;
			if (entitiesBeforeGenerate != null)
			{
				entitiesBeforeGenerate.Clear();
			}
			this.ClearEqualityCheckValues();
			if (this.Ruins != null)
			{
				this.Ruins.Clear();
				this.Ruins = null;
			}
			List<Point> list = this.bottomPositions;
			if (list != null)
			{
				list.Clear();
			}
			this.BottomBarrier = null;
			this.TopBarrier = null;
			this.SeaFloor = null;
			this.distanceField = null;
			if (this.ExtraWalls != null)
			{
				foreach (LevelWall w in this.ExtraWalls)
				{
					w.Dispose();
				}
				this.ExtraWalls = null;
			}
			if (this.UnsyncedExtraWalls != null)
			{
				foreach (LevelWall w2 in this.UnsyncedExtraWalls)
				{
					w2.Dispose();
				}
				this.UnsyncedExtraWalls = null;
			}
			List<VoronoiCell> list2 = this.tempCells;
			if (list2 != null)
			{
				list2.Clear();
			}
			this.cells = null;
			this.cellGrid = null;
			if (this.bodies != null)
			{
				this.bodies.Clear();
				this.bodies = null;
			}
			this.StartLocation = null;
			this.EndLocation = null;
			Level.Loaded = null;
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x0005B4C0 File Offset: 0x000596C0
		[CompilerGenerated]
		internal static void <Generate>g__MarkEdges|191_5(VoronoiCell cell, Level.TunnelType tunnelType)
		{
			foreach (GraphEdge edge in cell.Edges)
			{
				switch (tunnelType)
				{
				case Level.TunnelType.MainPath:
					edge.NextToMainPath = true;
					break;
				case Level.TunnelType.SidePath:
					edge.NextToSidePath = true;
					break;
				case Level.TunnelType.Cave:
					edge.NextToCave = true;
					break;
				}
			}
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x0005B53C File Offset: 0x0005973C
		[CompilerGenerated]
		private bool <GenerateVoronoiSites>g__TooCloseToOtherSites|192_0(double siteX, double siteY, float minDistance = 10f)
		{
			float minDistanceSqr = minDistance * minDistance;
			for (int i = 0; i < this.siteCoordsX.Count; i++)
			{
				if (MathUtils.DistanceSquared(this.siteCoordsX[i], this.siteCoordsY[i], siteX, siteY) < (double)minDistanceSqr)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0005B58C File Offset: 0x0005978C
		[CompilerGenerated]
		internal static List<Point> <GenerateCave>g__SegmentsToNodes|215_0(List<Vector2[]> segments)
		{
			List<Point> nodes = new List<Point>();
			foreach (Vector2[] segment in segments)
			{
				nodes.Add(segment[0].ToPoint());
			}
			nodes.Add(segments.Last<Vector2[]>()[1].ToPoint());
			return nodes;
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x0005B604 File Offset: 0x00059804
		[CompilerGenerated]
		private void <CalculateTunnelDistanceField>g__addPoint|219_0(int x, int y, ref Level.<>c__DisplayClass219_0 A_3)
		{
			Point point = new Point(x, y);
			double shortestDistSqr = double.PositiveInfinity;
			foreach (Level.Tunnel tunnel in this.Tunnels)
			{
				for (int i = 1; i < tunnel.Nodes.Count; i++)
				{
					shortestDistSqr = Math.Min(shortestDistSqr, MathUtils.LineSegmentToPointDistanceSquared(tunnel.Nodes[i - 1], tunnel.Nodes[i], point));
				}
			}
			if (A_3.ruinPositions != null)
			{
				int ruinSize = 10000;
				foreach (Point ruinPos in A_3.ruinPositions)
				{
					double xDiff = (double)Math.Abs(point.X - ruinPos.X);
					double yDiff = (double)Math.Abs(point.Y - ruinPos.Y);
					if (xDiff < (double)ruinSize && yDiff < (double)ruinSize)
					{
						shortestDistSqr = 0.0;
					}
					else
					{
						shortestDistSqr = Math.Min(xDiff * xDiff + yDiff * yDiff, shortestDistSqr);
					}
				}
			}
			shortestDistSqr = Math.Min(shortestDistSqr, MathUtils.DistanceSquared((double)point.X, (double)point.Y, (double)this.startPosition.X, (double)this.startPosition.Y));
			shortestDistSqr = Math.Min(shortestDistSqr, MathUtils.DistanceSquared((double)point.X, (double)point.Y, (double)this.startExitPosition.X, (double)this.borders.Bottom));
			shortestDistSqr = Math.Min(shortestDistSqr, MathUtils.DistanceSquared((double)point.X, (double)point.Y, (double)this.endPosition.X, (double)this.endPosition.Y));
			shortestDistSqr = Math.Min(shortestDistSqr, MathUtils.DistanceSquared((double)point.X, (double)point.Y, (double)this.endExitPosition.X, (double)this.borders.Bottom));
			this.distanceField.Add(new ValueTuple<Point, double>(point, Math.Sqrt(shortestDistSqr)));
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x0005B834 File Offset: 0x00059A34
		[CompilerGenerated]
		internal static bool <GenerateItems>g__CalculatePositionOnPath|233_12(float checkedDist = 0f, ref Level.<>c__DisplayClass233_4 A_1, ref Level.<>c__DisplayClass233_5 A_2, ref Level.<>c__DisplayClass233_6 A_3)
		{
			if (A_2.nextNodeIndex >= A_1.tunnel.Nodes.Count)
			{
				return false;
			}
			float distToNextNode = Vector2.Distance(A_2.positionOnPath, A_1.tunnel.Nodes[A_2.nextNodeIndex].ToVector2());
			float lerpAmount = ((float)A_3.distance - checkedDist) / distToNextNode;
			if (lerpAmount <= 1f)
			{
				A_2.positionOnPath = Vector2.Lerp(A_2.positionOnPath, A_1.tunnel.Nodes[A_2.nextNodeIndex].ToVector2(), lerpAmount);
				return true;
			}
			List<Point> nodes = A_1.tunnel.Nodes;
			int nextNodeIndex = A_2.nextNodeIndex;
			A_2.nextNodeIndex = nextNodeIndex + 1;
			A_2.positionOnPath = nodes[nextNodeIndex].ToVector2();
			return Level.<GenerateItems>g__CalculatePositionOnPath|233_12(checkedDist + distToNextNode, ref A_1, ref A_2, ref A_3);
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x0005B905 File Offset: 0x00059B05
		[CompilerGenerated]
		internal static bool <GenerateItems>g__IsNextToTunnelType|233_18(GraphEdge e, Level.TunnelType t)
		{
			return (e.NextToMainPath && t == Level.TunnelType.MainPath) || (e.NextToSidePath && t == Level.TunnelType.SidePath) || (e.NextToCave && t == Level.TunnelType.Cave);
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0005B930 File Offset: 0x00059B30
		[CompilerGenerated]
		internal static bool <GenerateItems>g__HaveConnectingEdgePoints|233_21(GraphEdge e1, GraphEdge e2)
		{
			return e1.Point1.NearlyEquals(e2.Point1) || e1.Point1.NearlyEquals(e2.Point2) || e1.Point2.NearlyEquals(e2.Point1) || e1.Point2.NearlyEquals(e2.Point2);
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0005B98C File Offset: 0x00059B8C
		[CompilerGenerated]
		internal static bool <GenerateItems>g__IsAlreadyInList|233_23(GraphEdge edge, ref Level.<>c__DisplayClass233_12 A_1)
		{
			return A_1.validLocations.Any((Level.ClusterLocation l) => l.Edge == edge);
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x0005B9BD File Offset: 0x00059BBD
		[CompilerGenerated]
		internal static bool <GenerateMissionResources>g__IsOnMainPath|234_5(Level.ClusterLocation location)
		{
			return location.Edge.NextToMainPath;
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x0005B9CB File Offset: 0x00059BCB
		[CompilerGenerated]
		internal static bool <GenerateMissionResources>g__IsOnSidePath|234_6(Level.ClusterLocation location)
		{
			return location.Edge.NextToSidePath;
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x0005B9D9 File Offset: 0x00059BD9
		[CompilerGenerated]
		internal static bool <GenerateMissionResources>g__IsInCave|234_7(Level.ClusterLocation location)
		{
			return location.Edge.NextToCave;
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x0005BA74 File Offset: 0x00059C74
		[CompilerGenerated]
		internal static float <SpawnCorpses>g__GetStrength|269_11(Limb limb, float max)
		{
			float strength = Rand.Range(0f, max, Rand.RandSync.Unsynced);
			if (limb.type != LimbType.Head)
			{
				strength = Math.Min(strength, Rand.Range(0f, max, Rand.RandSync.Unsynced));
			}
			return strength;
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0005BAAC File Offset: 0x00059CAC
		[CompilerGenerated]
		internal static CorpsePrefab <SpawnCorpses>g__GetCorpsePrefab|269_10(HashSet<JobPrefab> usedJobs, Func<CorpsePrefab, bool> predicate = null)
		{
			IEnumerable<CorpsePrefab> filteredPrefabs = from p in CorpsePrefab.Prefabs
			where usedJobs.None(delegate(JobPrefab j)
			{
				Identifier identifier = p.Job.ToIdentifier<Identifier>();
				return j.Identifier == identifier;
			}) && p.SpawnPosition == Level.PositionType.Wreck && (predicate == null || predicate(p))
			select p;
			return ToolBox.SelectWeightedRandom<CorpsePrefab>(filteredPrefabs.ToList<CorpsePrefab>(), (from p in filteredPrefabs
			select p.Commonness).ToList<float>(), Rand.RandSync.Unsynced);
		}

		// Token: 0x040003D6 RID: 982
		public const int MaxEntityDepth = -1000000;

		// Token: 0x040003D7 RID: 983
		public const float ShaftHeight = 1000f;

		// Token: 0x040003D8 RID: 984
		public const float OutsideBoundsCurrentMargin = 30000f;

		// Token: 0x040003D9 RID: 985
		public const float OutsideBoundsCurrentMarginExponential = 150000f;

		// Token: 0x040003DA RID: 986
		public const float OutsideBoundsCurrentHardLimit = 200000f;

		// Token: 0x040003DB RID: 987
		public const int MaxSubmarineWidth = 16000;

		// Token: 0x040003DC RID: 988
		private static Level loaded;

		// Token: 0x040003DD RID: 989
		public const float ExitDistance = 6000f;

		// Token: 0x040003DE RID: 990
		public const int GridCellSize = 2000;

		// Token: 0x040003DF RID: 991
		private List<VoronoiCell>[,] cellGrid;

		// Token: 0x040003E0 RID: 992
		private List<VoronoiCell> cells;

		// Token: 0x040003E2 RID: 994
		public List<Level.AbyssIsland> AbyssIslands = new List<Level.AbyssIsland>();

		// Token: 0x040003E3 RID: 995
		public List<double> siteCoordsX;

		// Token: 0x040003E4 RID: 996
		public List<double> siteCoordsY;

		// Token: 0x040003E5 RID: 997
		[TupleElementNames(new string[]
		{
			"point",
			"distance"
		})]
		public List<ValueTuple<Point, double>> distanceField;

		// Token: 0x040003E6 RID: 998
		private Point startPosition;

		// Token: 0x040003E7 RID: 999
		private Point endPosition;

		// Token: 0x040003E8 RID: 1000
		private readonly Rectangle borders;

		// Token: 0x040003E9 RID: 1001
		private List<Body> bodies;

		// Token: 0x040003EA RID: 1002
		private List<Point> bottomPositions;

		// Token: 0x040003EB RID: 1003
		private const float NetworkUpdateInterval = 5f;

		// Token: 0x040003EC RID: 1004
		private float networkUpdateTimer;

		// Token: 0x040003ED RID: 1005
		private Point startExitPosition;

		// Token: 0x040003EE RID: 1006
		private Point endExitPosition;

		// Token: 0x040003F1 RID: 1009
		public const float DefaultRealWorldCrushDepth = 3500f;

		// Token: 0x040003F6 RID: 1014
		private Sonar beaconSonar;

		// Token: 0x040003F7 RID: 1015
		private ImmutableArray<SonarTransducer> beaconTransducers = ImmutableArray<SonarTransducer>.Empty;

		// Token: 0x040003FF RID: 1023
		private SubmarineInfo preSelectedStartOutpost;

		// Token: 0x04000400 RID: 1024
		private SubmarineInfo preSelectedEndOutpost;

		// Token: 0x04000401 RID: 1025
		public readonly LevelData LevelData;

		// Token: 0x04000402 RID: 1026
		private readonly Dictionary<Level.LevelGenStage, int> equalityCheckValues = (from Level.LevelGenStage k in Enum.GetValues(typeof(Level.LevelGenStage))
		select new ValueTuple<Level.LevelGenStage, int>(k, 0)).ToDictionary<Level.LevelGenStage, int>();

		// Token: 0x0400040D RID: 1037
		public static float? ForcedDifficulty;

		// Token: 0x0400040E RID: 1038
		private bool isRandomHashSet;

		// Token: 0x0400040F RID: 1039
		private int _randomHash;

		// Token: 0x04000410 RID: 1040
		private string previousSeed;

		// Token: 0x04000411 RID: 1041
		private static int nextPathPointId;

		// Token: 0x04000414 RID: 1044
		private readonly List<VoronoiCell> tempCells = new List<VoronoiCell>();

		// Token: 0x04000415 RID: 1045
		private readonly Dictionary<string, List<Vector2>> positionHistory = new Dictionary<string, List<Vector2>>();

		// Token: 0x04000416 RID: 1046
		private readonly Dictionary<Submarine, List<Rectangle>> blockedRects = new Dictionary<Submarine, List<Rectangle>>();

		// Token: 0x020006B8 RID: 1720
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x17001406 RID: 5126
			// (get) Token: 0x06004F74 RID: 20340
			Level.EventType EventType { get; }
		}

		// Token: 0x020006B9 RID: 1721
		public readonly struct SingleLevelWallEventData : Level.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001407 RID: 5127
			// (get) Token: 0x06004F75 RID: 20341 RVA: 0x001E47CF File Offset: 0x001E29CF
			public Level.EventType EventType
			{
				get
				{
					return Level.EventType.SingleDestructibleWall;
				}
			}

			// Token: 0x06004F76 RID: 20342 RVA: 0x001E47D2 File Offset: 0x001E29D2
			public SingleLevelWallEventData(DestructibleLevelWall wall)
			{
				this.Wall = wall;
			}

			// Token: 0x04002A7B RID: 10875
			public readonly DestructibleLevelWall Wall;
		}

		// Token: 0x020006BA RID: 1722
		public readonly struct GlobalLevelWallEventData : Level.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001408 RID: 5128
			// (get) Token: 0x06004F77 RID: 20343 RVA: 0x001E47DB File Offset: 0x001E29DB
			public Level.EventType EventType
			{
				get
				{
					return Level.EventType.GlobalDestructibleWall;
				}
			}
		}

		// Token: 0x020006BB RID: 1723
		public enum PlacementType
		{
			// Token: 0x04002A7D RID: 10877
			Top,
			// Token: 0x04002A7E RID: 10878
			Bottom
		}

		// Token: 0x020006BC RID: 1724
		public enum EventType
		{
			// Token: 0x04002A80 RID: 10880
			SingleDestructibleWall,
			// Token: 0x04002A81 RID: 10881
			GlobalDestructibleWall
		}

		// Token: 0x020006BD RID: 1725
		[Flags]
		public enum PositionType
		{
			// Token: 0x04002A83 RID: 10883
			None = 0,
			// Token: 0x04002A84 RID: 10884
			MainPath = 1,
			// Token: 0x04002A85 RID: 10885
			SidePath = 2,
			// Token: 0x04002A86 RID: 10886
			Cave = 4,
			// Token: 0x04002A87 RID: 10887
			Ruin = 8,
			// Token: 0x04002A88 RID: 10888
			Wreck = 16,
			// Token: 0x04002A89 RID: 10889
			BeaconStation = 32,
			// Token: 0x04002A8A RID: 10890
			Abyss = 64,
			// Token: 0x04002A8B RID: 10891
			AbyssCave = 128,
			// Token: 0x04002A8C RID: 10892
			Outpost = 256
		}

		// Token: 0x020006BE RID: 1726
		public struct InterestingPosition
		{
			// Token: 0x06004F78 RID: 20344 RVA: 0x001E47DE File Offset: 0x001E29DE
			public InterestingPosition(Point position, Level.PositionType positionType, Submarine submarine = null, bool isValid = true)
			{
				this.Position = position;
				this.PositionType = positionType;
				this.IsValid = isValid;
				this.Submarine = submarine;
				this.Ruin = null;
				this.Cave = null;
			}

			// Token: 0x06004F79 RID: 20345 RVA: 0x001E480B File Offset: 0x001E2A0B
			public InterestingPosition(Point position, Level.PositionType positionType, Ruin ruin, bool isValid = true)
			{
				this.Position = position;
				this.PositionType = positionType;
				this.IsValid = isValid;
				this.Submarine = null;
				this.Ruin = ruin;
				this.Cave = null;
			}

			// Token: 0x06004F7A RID: 20346 RVA: 0x001E4838 File Offset: 0x001E2A38
			public InterestingPosition(Point position, Level.PositionType positionType, Level.Cave cave, bool isValid = true)
			{
				this.Position = position;
				this.PositionType = positionType;
				this.IsValid = isValid;
				this.Submarine = null;
				this.Ruin = null;
				this.Cave = cave;
			}

			// Token: 0x06004F7B RID: 20347 RVA: 0x001E4868 File Offset: 0x001E2A68
			public bool IsEnclosedArea()
			{
				return this.PositionType == Level.PositionType.Cave || this.PositionType == Level.PositionType.Ruin || this.PositionType == Level.PositionType.Outpost || this.PositionType == Level.PositionType.BeaconStation || this.PositionType == Level.PositionType.Wreck || this.PositionType == Level.PositionType.AbyssCave;
			}

			// Token: 0x04002A8D RID: 10893
			public Point Position;

			// Token: 0x04002A8E RID: 10894
			public readonly Level.PositionType PositionType;

			// Token: 0x04002A8F RID: 10895
			public bool IsValid;

			// Token: 0x04002A90 RID: 10896
			public Submarine Submarine;

			// Token: 0x04002A91 RID: 10897
			public Ruin Ruin;

			// Token: 0x04002A92 RID: 10898
			public Level.Cave Cave;
		}

		// Token: 0x020006BF RID: 1727
		public enum TunnelType
		{
			// Token: 0x04002A94 RID: 10900
			MainPath,
			// Token: 0x04002A95 RID: 10901
			SidePath,
			// Token: 0x04002A96 RID: 10902
			Cave
		}

		// Token: 0x020006C0 RID: 1728
		public class Tunnel
		{
			// Token: 0x17001409 RID: 5129
			// (get) Token: 0x06004F7C RID: 20348 RVA: 0x001E48B7 File Offset: 0x001E2AB7
			// (set) Token: 0x06004F7D RID: 20349 RVA: 0x001E48BF File Offset: 0x001E2ABF
			public List<Point> Nodes { get; private set; }

			// Token: 0x1700140A RID: 5130
			// (get) Token: 0x06004F7E RID: 20350 RVA: 0x001E48C8 File Offset: 0x001E2AC8
			// (set) Token: 0x06004F7F RID: 20351 RVA: 0x001E48D0 File Offset: 0x001E2AD0
			public List<VoronoiCell> Cells { get; private set; }

			// Token: 0x1700140B RID: 5131
			// (get) Token: 0x06004F80 RID: 20352 RVA: 0x001E48D9 File Offset: 0x001E2AD9
			// (set) Token: 0x06004F81 RID: 20353 RVA: 0x001E48E1 File Offset: 0x001E2AE1
			public List<WayPoint> WayPoints { get; private set; }

			// Token: 0x06004F82 RID: 20354 RVA: 0x001E48EA File Offset: 0x001E2AEA
			public Tunnel(Level.TunnelType type, List<Point> nodes, int minWidth, Level.Tunnel parentTunnel)
			{
				this.Type = type;
				this.MinWidth = minWidth;
				this.ParentTunnel = parentTunnel;
				this.Nodes = new List<Point>(nodes);
				this.Cells = new List<VoronoiCell>();
				this.WayPoints = new List<WayPoint>();
			}

			// Token: 0x04002A97 RID: 10903
			public readonly Level.Tunnel ParentTunnel;

			// Token: 0x04002A98 RID: 10904
			public readonly int MinWidth;

			// Token: 0x04002A99 RID: 10905
			public readonly Level.TunnelType Type;
		}

		// Token: 0x020006C1 RID: 1729
		public class Cave
		{
			// Token: 0x06004F83 RID: 20355 RVA: 0x001E492A File Offset: 0x001E2B2A
			public Cave(CaveGenerationParams caveGenerationParams, Rectangle area, Point startPos, Point endPos)
			{
				this.CaveGenerationParams = caveGenerationParams;
				this.Area = area;
				this.StartPos = startPos;
				this.EndPos = endPos;
			}

			// Token: 0x04002A9D RID: 10909
			public Rectangle Area;

			// Token: 0x04002A9E RID: 10910
			public readonly List<Level.Tunnel> Tunnels = new List<Level.Tunnel>();

			// Token: 0x04002A9F RID: 10911
			public Point StartPos;

			// Token: 0x04002AA0 RID: 10912
			public Point EndPos;

			// Token: 0x04002AA1 RID: 10913
			public readonly HashSet<Mission> MissionsToDisplayOnSonar = new HashSet<Mission>();

			// Token: 0x04002AA2 RID: 10914
			public readonly CaveGenerationParams CaveGenerationParams;
		}

		// Token: 0x020006C2 RID: 1730
		public class AbyssIsland
		{
			// Token: 0x06004F84 RID: 20356 RVA: 0x001E4965 File Offset: 0x001E2B65
			public AbyssIsland(Rectangle area, List<VoronoiCell> cells)
			{
				this.Area = area;
				this.Cells = cells;
			}

			// Token: 0x04002AA3 RID: 10915
			public Rectangle Area;

			// Token: 0x04002AA4 RID: 10916
			public readonly List<VoronoiCell> Cells;
		}

		// Token: 0x020006C3 RID: 1731
		public enum LevelGenStage
		{
			// Token: 0x04002AA6 RID: 10918
			LevelGenParams,
			// Token: 0x04002AA7 RID: 10919
			Size,
			// Token: 0x04002AA8 RID: 10920
			GenStart,
			// Token: 0x04002AA9 RID: 10921
			TunnelGen1,
			// Token: 0x04002AAA RID: 10922
			TunnelGen2,
			// Token: 0x04002AAB RID: 10923
			AbyssGen,
			// Token: 0x04002AAC RID: 10924
			CaveGen,
			// Token: 0x04002AAD RID: 10925
			VoronoiGen,
			// Token: 0x04002AAE RID: 10926
			VoronoiGen2,
			// Token: 0x04002AAF RID: 10927
			VoronoiGen3,
			// Token: 0x04002AB0 RID: 10928
			Ruins,
			// Token: 0x04002AB1 RID: 10929
			Outposts,
			// Token: 0x04002AB2 RID: 10930
			FloatingIce,
			// Token: 0x04002AB3 RID: 10931
			LevelBodies,
			// Token: 0x04002AB4 RID: 10932
			IceSpires,
			// Token: 0x04002AB5 RID: 10933
			TopAndBottom,
			// Token: 0x04002AB6 RID: 10934
			PlaceLevelObjects,
			// Token: 0x04002AB7 RID: 10935
			GenerateItems,
			// Token: 0x04002AB8 RID: 10936
			Finish
		}

		// Token: 0x020006C4 RID: 1732
		public struct PathPoint
		{
			// Token: 0x1700140C RID: 5132
			// (get) Token: 0x06004F85 RID: 20357 RVA: 0x001E497B File Offset: 0x001E2B7B
			public readonly string Id { get; }

			// Token: 0x1700140D RID: 5133
			// (get) Token: 0x06004F86 RID: 20358 RVA: 0x001E4983 File Offset: 0x001E2B83
			public readonly Vector2 Position { get; }

			// Token: 0x1700140E RID: 5134
			// (get) Token: 0x06004F87 RID: 20359 RVA: 0x001E498B File Offset: 0x001E2B8B
			// (set) Token: 0x06004F88 RID: 20360 RVA: 0x001E4993 File Offset: 0x001E2B93
			public bool ShouldContainResources { readonly get; set; }

			// Token: 0x1700140F RID: 5135
			// (get) Token: 0x06004F89 RID: 20361 RVA: 0x001E499C File Offset: 0x001E2B9C
			public float NextClusterProbability
			{
				get
				{
					float result;
					switch (this.ClusterLocations.Count)
					{
					case 1:
						result = 5f;
						break;
					case 2:
						result = 2.5f;
						break;
					case 3:
						result = 1f;
						break;
					default:
						result = 0f;
						break;
					}
					return result;
				}
			}

			// Token: 0x17001410 RID: 5136
			// (get) Token: 0x06004F8A RID: 20362 RVA: 0x001E49EA File Offset: 0x001E2BEA
			public readonly List<Identifier> ResourceTags { get; }

			// Token: 0x17001411 RID: 5137
			// (get) Token: 0x06004F8B RID: 20363 RVA: 0x001E49F2 File Offset: 0x001E2BF2
			public readonly List<Identifier> ResourceIds { get; }

			// Token: 0x17001412 RID: 5138
			// (get) Token: 0x06004F8C RID: 20364 RVA: 0x001E49FA File Offset: 0x001E2BFA
			public readonly List<Level.ClusterLocation> ClusterLocations { get; }

			// Token: 0x17001413 RID: 5139
			// (get) Token: 0x06004F8D RID: 20365 RVA: 0x001E4A02 File Offset: 0x001E2C02
			public readonly Level.TunnelType TunnelType { get; }

			// Token: 0x06004F8E RID: 20366 RVA: 0x001E4A0A File Offset: 0x001E2C0A
			private PathPoint(string id, Vector2 position, bool shouldContainResources, Level.TunnelType tunnelType, List<Identifier> resourceTags, List<Identifier> resourceIds, List<Level.ClusterLocation> clusterLocations)
			{
				this.Id = id;
				this.Position = position;
				this.ShouldContainResources = shouldContainResources;
				this.ResourceTags = resourceTags;
				this.ResourceIds = resourceIds;
				this.ClusterLocations = clusterLocations;
				this.TunnelType = tunnelType;
			}

			// Token: 0x06004F8F RID: 20367 RVA: 0x001E4A41 File Offset: 0x001E2C41
			public PathPoint(string id, Vector2 position, bool shouldContainResources, Level.TunnelType tunnelType)
			{
				this = new Level.PathPoint(id, position, shouldContainResources, tunnelType, new List<Identifier>(), new List<Identifier>(), new List<Level.ClusterLocation>());
			}

			// Token: 0x06004F90 RID: 20368 RVA: 0x001E4A5D File Offset: 0x001E2C5D
			public Level.PathPoint WithResources(bool containsResources)
			{
				return new Level.PathPoint(this.Id, this.Position, containsResources, this.TunnelType, this.ResourceTags, this.ResourceIds, this.ClusterLocations);
			}
		}

		// Token: 0x020006C5 RID: 1733
		public struct ClusterLocation
		{
			// Token: 0x17001414 RID: 5140
			// (get) Token: 0x06004F91 RID: 20369 RVA: 0x001E4A89 File Offset: 0x001E2C89
			public readonly VoronoiCell Cell { get; }

			// Token: 0x17001415 RID: 5141
			// (get) Token: 0x06004F92 RID: 20370 RVA: 0x001E4A91 File Offset: 0x001E2C91
			public readonly GraphEdge Edge { get; }

			// Token: 0x17001416 RID: 5142
			// (get) Token: 0x06004F93 RID: 20371 RVA: 0x001E4A99 File Offset: 0x001E2C99
			public readonly Vector2 EdgeCenter { get; }

			// Token: 0x17001417 RID: 5143
			// (get) Token: 0x06004F94 RID: 20372 RVA: 0x001E4AA1 File Offset: 0x001E2CA1
			// (set) Token: 0x06004F95 RID: 20373 RVA: 0x001E4AA9 File Offset: 0x001E2CA9
			public List<Item> Resources { readonly get; private set; }

			// Token: 0x06004F96 RID: 20374 RVA: 0x001E4AB2 File Offset: 0x001E2CB2
			public ClusterLocation(VoronoiCell cell, GraphEdge edge, bool initializeResourceList = false)
			{
				this.Cell = cell;
				this.Edge = edge;
				this.EdgeCenter = edge.Center;
				this.Resources = (initializeResourceList ? new List<Item>() : null);
			}

			// Token: 0x06004F97 RID: 20375 RVA: 0x001E4ADF File Offset: 0x001E2CDF
			public bool Equals(Level.ClusterLocation anotherLocation)
			{
				return this.Cell == anotherLocation.Cell && this.Edge == anotherLocation.Edge;
			}

			// Token: 0x06004F98 RID: 20376 RVA: 0x001E4B01 File Offset: 0x001E2D01
			public bool Equals(VoronoiCell cell, GraphEdge edge)
			{
				return this.Cell == cell && this.Edge == edge;
			}

			// Token: 0x06004F99 RID: 20377 RVA: 0x001E4B17 File Offset: 0x001E2D17
			public void InitializeResources()
			{
				this.Resources = new List<Item>();
			}
		}

		// Token: 0x020006C6 RID: 1734
		private readonly struct PlaceableWreck : IEquatable<Level.PlaceableWreck>
		{
			// Token: 0x06004F9A RID: 20378 RVA: 0x001E4B24 File Offset: 0x001E2D24
			public PlaceableWreck(WreckFile WreckFile, WreckInfo WreckInfo)
			{
				this.WreckFile = WreckFile;
				this.WreckInfo = WreckInfo;
			}

			// Token: 0x17001418 RID: 5144
			// (get) Token: 0x06004F9B RID: 20379 RVA: 0x001E4B34 File Offset: 0x001E2D34
			// (set) Token: 0x06004F9C RID: 20380 RVA: 0x001E4B3C File Offset: 0x001E2D3C
			public WreckFile WreckFile { get; set; }

			// Token: 0x17001419 RID: 5145
			// (get) Token: 0x06004F9D RID: 20381 RVA: 0x001E4B45 File Offset: 0x001E2D45
			// (set) Token: 0x06004F9E RID: 20382 RVA: 0x001E4B4D File Offset: 0x001E2D4D
			public WreckInfo WreckInfo { get; set; }

			// Token: 0x06004F9F RID: 20383 RVA: 0x001E4B58 File Offset: 0x001E2D58
			public static Option<Level.PlaceableWreck> TryCreate(WreckFile wreckFile)
			{
				SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo i) => i.FilePath == wreckFile.Path.Value);
				if (((matchingSub != null) ? matchingSub.WreckInfo : null) == null)
				{
					DebugConsole.ThrowError("No matching submarine info found for the wreck file " + wreckFile.Path.Value, null, null, false, false);
					Option.UnspecifiedNone none = Option.None;
					return none;
				}
				return Option.Some<Level.PlaceableWreck>(new Level.PlaceableWreck(wreckFile, matchingSub.WreckInfo));
			}

			// Token: 0x06004FA0 RID: 20384 RVA: 0x001E4BE0 File Offset: 0x001E2DE0
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("PlaceableWreck");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06004FA1 RID: 20385 RVA: 0x001E4C2C File Offset: 0x001E2E2C
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("WreckFile = ");
				builder.Append(this.WreckFile);
				builder.Append(", WreckInfo = ");
				builder.Append(this.WreckInfo);
				return true;
			}

			// Token: 0x06004FA2 RID: 20386 RVA: 0x001E4C61 File Offset: 0x001E2E61
			[CompilerGenerated]
			public static bool operator !=(Level.PlaceableWreck left, Level.PlaceableWreck right)
			{
				return !(left == right);
			}

			// Token: 0x06004FA3 RID: 20387 RVA: 0x001E4C6D File Offset: 0x001E2E6D
			[CompilerGenerated]
			public static bool operator ==(Level.PlaceableWreck left, Level.PlaceableWreck right)
			{
				return left.Equals(right);
			}

			// Token: 0x06004FA4 RID: 20388 RVA: 0x001E4C77 File Offset: 0x001E2E77
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<WreckFile>.Default.GetHashCode(this.<WreckFile>k__BackingField) * -1521134295 + EqualityComparer<WreckInfo>.Default.GetHashCode(this.<WreckInfo>k__BackingField);
			}

			// Token: 0x06004FA5 RID: 20389 RVA: 0x001E4CA0 File Offset: 0x001E2EA0
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Level.PlaceableWreck && this.Equals((Level.PlaceableWreck)obj);
			}

			// Token: 0x06004FA6 RID: 20390 RVA: 0x001E4CB8 File Offset: 0x001E2EB8
			[CompilerGenerated]
			public bool Equals(Level.PlaceableWreck other)
			{
				return EqualityComparer<WreckFile>.Default.Equals(this.<WreckFile>k__BackingField, other.<WreckFile>k__BackingField) && EqualityComparer<WreckInfo>.Default.Equals(this.<WreckInfo>k__BackingField, other.<WreckInfo>k__BackingField);
			}

			// Token: 0x06004FA7 RID: 20391 RVA: 0x001E4CEA File Offset: 0x001E2EEA
			[CompilerGenerated]
			public void Deconstruct(out WreckFile WreckFile, out WreckInfo WreckInfo)
			{
				WreckFile = this.WreckFile;
				WreckInfo = this.WreckInfo;
			}
		}

		// Token: 0x020006C7 RID: 1735
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04002AC6 RID: 10950
			public static Predicate<Level.ClusterLocation> <0>__IsOnMainPath;

			// Token: 0x04002AC7 RID: 10951
			public static Predicate<Level.ClusterLocation> <1>__IsOnSidePath;

			// Token: 0x04002AC8 RID: 10952
			public static Predicate<Level.ClusterLocation> <2>__IsInCave;

			// Token: 0x04002AC9 RID: 10953
			public static Func<WreckFile, Option<Level.PlaceableWreck>> <3>__TryCreate;
		}
	}
}

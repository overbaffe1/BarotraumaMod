using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x0200002A RID: 42
	internal class PirateMission : Mission
	{
		// Token: 0x06000510 RID: 1296 RVA: 0x0002DD8C File Offset: 0x0002BF8C
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			if (this.characters.Count == 0)
			{
				throw new InvalidOperationException("Server attempted to write escort mission data when no characters had been spawned.");
			}
			msg.WriteByte((byte)this.characters.Count);
			foreach (Character character in this.characters)
			{
				character.WriteSpawnData(msg, character.ID, false);
				msg.WriteUInt16((ushort)this.characterItems[character].Count<Item>());
				foreach (Item item in this.characterItems[character])
				{
					Item item2 = item;
					ushort id = item.ID;
					Inventory parentInventory = item.ParentInventory;
					ushort? num;
					if (parentInventory == null)
					{
						num = null;
					}
					else
					{
						Entity owner = parentInventory.Owner;
						num = ((owner != null) ? new ushort?(owner.ID) : null);
					}
					ushort? num2 = num;
					ushort valueOrDefault = num2.GetValueOrDefault();
					byte originalItemContainerIndex = 0;
					Inventory parentInventory2 = item.ParentInventory;
					item2.WriteSpawnData(msg, id, valueOrDefault, originalItemContainerIndex, (parentInventory2 != null) ? parentInventory2.FindIndex(item) : -1);
				}
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06000511 RID: 1297 RVA: 0x0002DED8 File Offset: 0x0002C0D8
		public override int TeamCount
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06000512 RID: 1298 RVA: 0x0002DEDC File Offset: 0x0002C0DC
		[TupleElementNames(new string[]
		{
			"Label",
			"Position"
		})]
		public override IEnumerable<ValueTuple<LocalizedString, Vector2>> SonarLabels
		{
			[return: TupleElementNames(new string[]
			{
				"Label",
				"Position"
			})]
			get
			{
				PirateMission.<get_SonarLabels>d__18 <get_SonarLabels>d__ = new PirateMission.<get_SonarLabels>d__18(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x0002DEF9 File Offset: 0x0002C0F9
		public override float GetBaseReward(Submarine sub)
		{
			return (float)this.alternateReward;
		}

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000514 RID: 1300 RVA: 0x0002DF02 File Offset: 0x0002C102
		public override SubmarineInfo EnemySubmarineInfo
		{
			get
			{
				return this.submarineInfo;
			}
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x0002DF0C File Offset: 0x0002C10C
		public PirateMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			this.submarineTypeConfig = prefab.ConfigElement.GetChildElement("SubmarineTypes");
			this.characterTypeConfig = prefab.ConfigElement.GetChildElement("CharacterTypes");
			this.addedMissionDifficultyPerPlayer = prefab.ConfigElement.GetAttributeFloat("addedmissiondifficultyperplayer", 0f);
			this.factionIdentifier = prefab.ConfigElement.GetAttributeIdentifier("faction", Identifier.Empty);
			foreach (ContentXElement cxe in this.characterConfig.Elements())
			{
				XElement characterElement = cxe;
				Identifier typeId = characterElement.GetAttributeIdentifier("typeidentifier", Identifier.Empty);
				if (typeId.IsEmpty)
				{
					if (characterElement.GetAttributeIdentifier("identifier", Identifier.Empty).IsEmpty)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error in mission \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\". Character element with neither a typeidentifier or identifier (");
						defaultInterpolatedStringHandler.AppendFormatted(characterElement.ToString());
						defaultInterpolatedStringHandler.AppendLiteral(").");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					}
				}
				else
				{
					ContentXElement characterTypeElement = this.characterTypeConfig.Elements().FirstOrDefault(delegate(ContentXElement e)
					{
						Identifier attributeIdentifier = e.GetAttributeIdentifier("typeidentifier", Identifier.Empty);
						return attributeIdentifier == typeId;
					});
					ContentXElement contentXElement = null;
					if (characterTypeElement == contentXElement)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(82, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Error in mission \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\". Could not find a character type element for the character \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(typeId);
						defaultInterpolatedStringHandler2.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					}
				}
			}
			foreach (ContentXElement cxe2 in this.characterTypeConfig.Elements())
			{
				XElement characterTypeElement2 = cxe2;
				foreach (XElement characterElement2 in characterTypeElement2.Elements())
				{
					Identifier characterIdentifier = characterElement2.GetAttributeIdentifier("identifier", Identifier.Empty);
					Identifier characterFrom = characterElement2.GetAttributeIdentifier("from", Identifier.Empty);
					if (NPCSet.Get(characterFrom, characterIdentifier, true, this.Prefab.ContentPackage) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(69, 3);
						defaultInterpolatedStringHandler3.AppendLiteral("Error in mission \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\". Character prefab \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(characterIdentifier);
						defaultInterpolatedStringHandler3.AppendLiteral("\" not found in the NPC set \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(characterFrom);
						defaultInterpolatedStringHandler3.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					}
				}
			}
			LocationConnection locationConnection = (from c in locations[0].Connections
			where c.Locations.Contains(locations[1])
			select c).FirstOrDefault<LocationConnection>();
			LevelData levelData2;
			if ((levelData2 = ((locationConnection != null) ? locationConnection.LevelData : null)) == null)
			{
				Location location = locations[0];
				levelData2 = ((location != null) ? location.LevelData : null);
			}
			LevelData levelData = levelData2;
			if (levelData != null)
			{
				this.SetLevel(levelData);
			}
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x0002E2FC File Offset: 0x0002C4FC
		public override void SetLevel(LevelData level)
		{
			if (this.levelData != null)
			{
				return;
			}
			this.submarineInfo = null;
			this.levelData = level;
			this.missionDifficulty = ((level != null) ? level.Difficulty : 0f);
			ContentXElement contentXElement = null;
			if (this.submarineTypeConfig == contentXElement)
			{
				this.submarineInfo = this.GetRandomDifficultyModifiedSubmarine(this.missionDifficulty, 15f);
				this.alternateReward = (int)this.submarineInfo.EnemySubmarineInfo.Reward;
			}
			else
			{
				PirateMission.<>c__DisplayClass27_0 CS$<>8__locals1 = new PirateMission.<>c__DisplayClass27_0();
				XElement submarineConfig = this.GetRandomDifficultyModifiedElement(this.submarineTypeConfig, this.missionDifficulty, 15f);
				this.alternateReward = submarineConfig.GetAttributeInt("alternatereward", this.Reward);
				this.factionIdentifier = submarineConfig.GetAttributeIdentifier("faction", this.factionIdentifier);
				CS$<>8__locals1.submarinePath = submarineConfig.GetAttributeContentPath("path", this.Prefab.ContentPackage);
				if (CS$<>8__locals1.submarinePath.IsNullOrEmpty())
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 1);
					defaultInterpolatedStringHandler.AppendLiteral("No path used for submarine for the pirate mission \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\"!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					return;
				}
				BaseSubFile contentFile = CS$<>8__locals1.<SetLevel>g__GetSubFile|0<EnemySubmarineFile>(CS$<>8__locals1.submarinePath) ?? CS$<>8__locals1.<SetLevel>g__GetSubFile|0<SubmarineFile>(CS$<>8__locals1.submarinePath);
				if (contentFile == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(39, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("No submarine file found from the path ");
					defaultInterpolatedStringHandler2.AppendFormatted<ContentPath>(CS$<>8__locals1.submarinePath);
					defaultInterpolatedStringHandler2.AppendLiteral("!");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, this.Prefab.ContentPackage, false, false);
					return;
				}
				this.submarineInfo = new SubmarineInfo(contentFile.Path.Value, "", null, true, false);
			}
			string rewardText = "‖color:gui.orange‖" + string.Format(CultureInfo.InvariantCulture, "{0:N0}", this.alternateReward) + "‖end‖";
			if (this.descriptionWithoutReward != null)
			{
				this.description = this.descriptionWithoutReward.Replace("[reward]", rewardText, StringComparison.Ordinal);
			}
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x0002E523 File Offset: 0x0002C723
		private static float GetDifficultyModifiedValue(float preferredDifficulty, float levelDifficulty, float randomnessModifier, Random rand)
		{
			return Math.Abs(levelDifficulty - preferredDifficulty + MathHelper.Lerp(-randomnessModifier, randomnessModifier, (float)rand.NextDouble()));
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x0002E53D File Offset: 0x0002C73D
		private static int GetDifficultyModifiedAmount(int minAmount, int maxAmount, float levelDifficulty, Random rand)
		{
			return Math.Max((int)Math.Round((double)((float)minAmount + (float)(maxAmount - minAmount) * (levelDifficulty + MathHelper.Lerp(-25f, 25f, (float)rand.NextDouble())) / 100f)), minAmount);
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x0002E574 File Offset: 0x0002C774
		private SubmarineInfo GetRandomDifficultyModifiedSubmarine(float levelDifficulty, float randomnessModifier)
		{
			Random rand = new MTRandom(ToolBox.StringToInt(this.levelData.Seed));
			SubmarineInfo bestSubmarine = null;
			float bestValue = float.MaxValue;
			IEnumerable<SubmarineInfo> submarineInfos = from i in SubmarineInfo.SavedSubmarines
			where i.IsEnemySubmarine
			select i;
			using (IEnumerator<SubmarineInfo> enumerator = submarineInfos.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SubmarineInfo submarineInfo = enumerator.Current;
					if (this.Prefab.Tags.Any((Identifier t) => submarineInfo.EnemySubmarineInfo.MissionTags.Contains(t)))
					{
						float applicabilityValue = PirateMission.GetDifficultyModifiedValue(submarineInfo.EnemySubmarineInfo.PreferredDifficulty, levelDifficulty, randomnessModifier, rand);
						if (applicabilityValue < bestValue)
						{
							bestSubmarine = submarineInfo;
							bestValue = applicabilityValue;
						}
					}
				}
			}
			if (bestSubmarine == null)
			{
				DebugConsole.ThrowError("No EnemySubmarine found that matches the mission's tags!", null, null, false, false);
				return SubmarineInfo.SavedSubmarines.First((SubmarineInfo i) => i.IsEnemySubmarine);
			}
			return bestSubmarine;
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0002E698 File Offset: 0x0002C898
		private XElement GetRandomDifficultyModifiedElement(XElement parentElement, float levelDifficulty, float randomnessModifier)
		{
			Random rand = new MTRandom(ToolBox.StringToInt(this.levelData.Seed));
			XElement bestElement = null;
			float bestValue = float.MaxValue;
			foreach (XElement element in parentElement.Elements())
			{
				float applicabilityValue = PirateMission.GetDifficultyModifiedValue(element.GetAttributeFloat(0f, new string[]
				{
					"preferreddifficulty"
				}), levelDifficulty, randomnessModifier, rand);
				if (applicabilityValue < bestValue)
				{
					bestElement = element;
					bestValue = applicabilityValue;
				}
			}
			return bestElement;
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x0002E730 File Offset: 0x0002C930
		private void CreateMissionPositions(out Vector2 preferredSpawnPos)
		{
			Vector2 patrolPos = Level.Loaded.EndPosition;
			Point subSize = this.enemySub.GetDockedBorders(true).Size;
			preferredSpawnPos = Level.Loaded.EndPosition;
			Level.InterestingPosition potentialSpawnPos;
			if (Level.Loaded.TryGetInterestingPosition(true, Level.PositionType.MainPath, (float)Level.Loaded.Size.X * 0.3f, out potentialSpawnPos, null, false))
			{
				preferredSpawnPos = potentialSpawnPos.Position.ToVector2();
			}
			else
			{
				DebugConsole.ThrowError("Could not spawn pirate submarine in an interesting location! " + ((this != null) ? this.ToString() : null), null, this.Prefab.ContentPackage, false, false);
			}
			Level.InterestingPosition potentialPatrolPos;
			if (Level.Loaded.TryGetInterestingPositionAwayFromPoint(true, Level.PositionType.MainPath, (float)Level.Loaded.Size.X * 0.3f, out potentialPatrolPos, preferredSpawnPos, 10000f, null))
			{
				patrolPos = potentialPatrolPos.Position.ToVector2();
			}
			else
			{
				DebugConsole.ThrowError("Could not give pirate submarine an interesting location to patrol to! " + ((this != null) ? this.ToString() : null), null, this.Prefab.ContentPackage, false, false);
			}
			patrolPos = this.enemySub.FindSpawnPos(patrolPos, new Point?(subSize), 0f, 0);
			this.patrolPositions.Add(patrolPos);
			this.patrolPositions.Add(preferredSpawnPos);
			if (!Mission.IsClient)
			{
				PathFinder pathFinder = new PathFinder(WayPoint.WayPointList, false);
				SteeringPath path = pathFinder.FindPath(ConvertUnits.ToSimUnits(patrolPos), ConvertUnits.ToSimUnits(preferredSpawnPos), null, null, 0f, null, null, null, true, 0f);
				if (!path.Unreachable)
				{
					List<WayPoint> validNodes = path.Nodes.FindAll(delegate(WayPoint n)
					{
						Func<VoronoiCell, bool> <>9__2;
						return !Level.Loaded.ExtraWalls.Any(delegate(LevelWall w)
						{
							IEnumerable<VoronoiCell> cells = w.Cells;
							Func<VoronoiCell, bool> predicate;
							if ((predicate = <>9__2) == null)
							{
								predicate = (<>9__2 = ((VoronoiCell c) => c.IsPointInside(n.WorldPosition)));
							}
							return cells.Any(predicate);
						});
					});
					if (validNodes.Any<WayPoint>())
					{
						preferredSpawnPos = validNodes.GetRandomUnsynced<WayPoint>().WorldPosition;
					}
				}
				int graceDistance = 500;
				preferredSpawnPos = this.enemySub.FindSpawnPos(preferredSpawnPos, new Point?(new Point(subSize.X + graceDistance, subSize.Y + graceDistance)), 0f, 0);
			}
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x0002E948 File Offset: 0x0002CB48
		private void InitPirateShip()
		{
			this.enemySub.NeutralizeBallast();
			Item item = this.enemySub.GetItems(false).Find((Item i) => i.HasTag(Tags.Reactor) && !i.NonInteractable);
			Reactor reactor = (item != null) ? item.GetComponent<Reactor>() : null;
			if (reactor != null)
			{
				reactor.PowerUpImmediately();
			}
			this.enemySub.EnableMaintainPosition();
			this.enemySub.TeamID = CharacterTeamType.None;
			this.enemySub.SetCrushDepth(Math.Max(this.enemySub.RealWorldCrushDepth, Submarine.MainSub.RealWorldCrushDepth));
			if (Level.Loaded != null)
			{
				foreach (Vector2 patrolPos in this.patrolPositions)
				{
					this.enemySub.SetCrushDepth(Math.Max(this.enemySub.RealWorldCrushDepth, Level.Loaded.GetRealWorldDepth(patrolPos.Y) + 1000f));
				}
			}
			this.enemySub.ImmuneToBallastFlora = true;
			this.enemySub.EnableFactionSpecificEntities(this.factionIdentifier);
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x0002EA78 File Offset: 0x0002CC78
		private void InitPirates()
		{
			this.characters.Clear();
			this.characterItems.Clear();
			ContentXElement contentXElement = null;
			if (this.characterConfig == contentXElement)
			{
				DebugConsole.ThrowError("Failed to initialize characters for escort mission (characterConfig == null)", null, this.Prefab.ContentPackage, false, false);
				return;
			}
			int playerCount = (from c in GameMain.Server.ConnectedClients
			where !c.SpectateOnly || !GameMain.Server.ServerSettings.AllowSpectating
			select c).Count<Client>();
			float enemyCreationDifficulty = this.missionDifficulty + (float)playerCount * this.addedMissionDifficultyPerPlayer;
			Random rand = new MTRandom(ToolBox.StringToInt(this.levelData.Seed));
			bool commanderAssigned = false;
			foreach (ContentXElement element in this.characterConfig.Elements())
			{
				Identifier humanPrefabId = element.GetAttributeIdentifier("identifier", Identifier.Empty);
				Identifier characterTypeId = element.GetAttributeIdentifier("typeidentifier", Identifier.Empty);
				int minAmount = element.GetAttributeInt("minamount", 0);
				int maxAmount = element.GetAttributeInt("maxamount", 0);
				int amountCreated = (minAmount == 0 && maxAmount == 0) ? 1 : PirateMission.GetDifficultyModifiedAmount(minAmount, maxAmount, enemyCreationDifficulty, rand);
				Func<ContentXElement, bool> <>9__1;
				for (int i = 0; i < amountCreated; i++)
				{
					HumanPrefab humanPrefab = null;
					bool isCommander = false;
					if (!characterTypeId.IsEmpty)
					{
						IEnumerable<ContentXElement> source = this.characterTypeConfig.Elements();
						Func<ContentXElement, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = delegate(ContentXElement e)
							{
								Identifier attributeIdentifier = e.GetAttributeIdentifier("typeidentifier", Identifier.Empty);
								return attributeIdentifier == characterTypeId;
							});
						}
						XElement characterType = source.Where(predicate).FirstOrDefault<ContentXElement>();
						if (characterType == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 1);
							defaultInterpolatedStringHandler.AppendLiteral("No character types defined in CharacterTypes for a declared type identifier in mission \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\".");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
							return;
						}
						XElement variantElement = this.GetRandomDifficultyModifiedElement(characterType, enemyCreationDifficulty, 25f);
						humanPrefab = base.GetHumanPrefabFromElement(variantElement);
						isCommander = variantElement.GetAttributeBool("iscommander", false);
					}
					else if (!humanPrefabId.IsEmpty)
					{
						humanPrefab = base.GetHumanPrefabFromElement(element);
						isCommander = element.GetAttributeBool("iscommander", false);
					}
					if (humanPrefab != null)
					{
						Character spawnedCharacter = Mission.CreateHuman(humanPrefab, this.characters, this.characterItems, this.enemySub, CharacterTeamType.None, null, Rand.RandSync.ServerAndClient);
						if (element.GetAttribute("color") != null)
						{
							Character character = spawnedCharacter;
							ContentXElement contentXElement2 = element;
							string key = "color";
							Color red = Color.Red;
							character.UniqueNameColor = new Color?(contentXElement2.GetAttributeColor(key, red));
						}
						if (!commanderAssigned && isCommander)
						{
							HumanAIController humanAIController = spawnedCharacter.AIController as HumanAIController;
							if (humanAIController != null)
							{
								humanAIController.InitShipCommandManager();
								foreach (Vector2 patrolPos in this.patrolPositions)
								{
									humanAIController.ShipCommandManager.patrolPositions.Add(patrolPos);
								}
								commanderAssigned = true;
							}
						}
						foreach (ContentXElement subElement in element.Elements())
						{
							Identifier identifier = subElement.NameAsIdentifier();
							if (identifier == "statuseffect")
							{
								StatusEffect newEffect = StatusEffect.Load(subElement, this.Prefab.Name.Value);
								if (newEffect != null)
								{
									newEffect.Apply(newEffect.type, 1f, spawnedCharacter, spawnedCharacter, null);
								}
							}
						}
						foreach (Item item in spawnedCharacter.Inventory.AllItems)
						{
							if (((item != null) ? item.GetComponent<IdCard>() : null) != null)
							{
								item.AddTag("id_pirate");
							}
						}
					}
				}
			}
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x0002EED0 File Offset: 0x0002D0D0
		protected override void StartMissionSpecific(Level level)
		{
			if (this.characters.Count > 0)
			{
				DebugConsole.AddWarning("Character list was not empty at the start of a pirate mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.characters.Clear();
			}
			if (this.patrolPositions.Count > 0)
			{
				DebugConsole.AddWarning("Patrol point list was not empty at the start of a pirate mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.patrolPositions.Clear();
			}
			this.enemySub = Submarine.MainSubs[1];
			if (this.enemySub == null)
			{
				DebugConsole.ThrowError((this.submarineInfo == null) ? "Error in PirateMission: enemy sub was not created (submarineInfo == null)." : "Error in PirateMission: enemy sub was not created.", null, this.Prefab.ContentPackage, false, false);
				return;
			}
			Vector2 spawnPos;
			this.CreateMissionPositions(out spawnPos);
			this.enemySub.SetPosition(spawnPos, null, true);
			this.InitPirateShip();
			this.enemySub.FlipX(null);
			this.enemySub.ShowSonarMarker = false;
			if (!Mission.IsClient)
			{
				this.InitPirates();
			}
		}

		// Token: 0x0600051F RID: 1311 RVA: 0x0002EFA4 File Offset: 0x0002D1A4
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (this.state >= 2 || this.enemySub == null)
			{
				return;
			}
			float sqrSonarRange = MathUtils.Pow2(10000f);
			this.outsideOfSonarRange = (Vector2.DistanceSquared(this.enemySub.WorldPosition, Submarine.MainSub.WorldPosition) > sqrSonarRange);
			if (this.CheckWinState())
			{
				this.State = 2;
				return;
			}
			int state = this.State;
			if (state != 0)
			{
				if (state != 1)
				{
					return;
				}
				if (this.outsideOfSonarRange)
				{
					if (this.lastSighting != null && Vector2.DistanceSquared(this.lastSighting.Value, Submarine.MainSub.WorldPosition) < sqrSonarRange)
					{
						this.lastSighting = null;
					}
					this.pirateSightingUpdateTimer -= deltaTime;
					if (this.pirateSightingUpdateTimer < 0f)
					{
						this.pirateSightingUpdateTimer = this.pirateSightingUpdateFrequency;
						this.lastSighting = new Vector2?(this.enemySub.WorldPosition);
						return;
					}
				}
				else
				{
					this.lastSighting = new Vector2?(this.enemySub.WorldPosition);
					this.pirateSightingUpdateTimer = 0f;
				}
			}
			else
			{
				for (int i = this.patrolPositions.Count - 1; i >= 0; i--)
				{
					if (Vector2.DistanceSquared(this.patrolPositions[i], Submarine.MainSub.WorldPosition) < sqrSonarRange)
					{
						this.patrolPositions.RemoveAt(i);
					}
				}
				if (!this.outsideOfSonarRange || this.patrolPositions.None(null))
				{
					this.State = 1;
					return;
				}
			}
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x0002F110 File Offset: 0x0002D310
		private bool CheckWinState()
		{
			if (!Mission.IsClient)
			{
				return this.characters.All((Character m) => PirateMission.DeadOrCaptured(m));
			}
			return false;
		}

		// Token: 0x06000521 RID: 1313 RVA: 0x0002F145 File Offset: 0x0002D345
		private static bool DeadOrCaptured(Character character)
		{
			return character == null || character.Removed || character.Submarine == null || (character.LockHands && character.Submarine == Submarine.MainSub) || character.IsIncapacitated;
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x0002F177 File Offset: 0x0002D377
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.state == 2;
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x0002F182 File Offset: 0x0002D382
		protected override void EndMissionSpecific(bool completed)
		{
			this.characters.Clear();
			this.characterItems.Clear();
			this.failed = !completed;
			this.submarineInfo = null;
		}

		// Token: 0x0400028E RID: 654
		private readonly ContentXElement submarineTypeConfig;

		// Token: 0x0400028F RID: 655
		private readonly ContentXElement characterTypeConfig;

		// Token: 0x04000290 RID: 656
		private readonly float addedMissionDifficultyPerPlayer;

		// Token: 0x04000291 RID: 657
		private float missionDifficulty;

		// Token: 0x04000292 RID: 658
		private int alternateReward;

		// Token: 0x04000293 RID: 659
		private Identifier factionIdentifier;

		// Token: 0x04000294 RID: 660
		private Submarine enemySub;

		// Token: 0x04000295 RID: 661
		private readonly Dictionary<HumanPrefab, List<StatusEffect>> characterStatusEffects = new Dictionary<HumanPrefab, List<StatusEffect>>();

		// Token: 0x04000296 RID: 662
		private readonly float pirateSightingUpdateFrequency = 30f;

		// Token: 0x04000297 RID: 663
		private float pirateSightingUpdateTimer;

		// Token: 0x04000298 RID: 664
		private Vector2? lastSighting;

		// Token: 0x04000299 RID: 665
		private LevelData levelData;

		// Token: 0x0400029A RID: 666
		private bool outsideOfSonarRange;

		// Token: 0x0400029B RID: 667
		private readonly List<Vector2> patrolPositions = new List<Vector2>();

		// Token: 0x0400029C RID: 668
		private SubmarineInfo submarineInfo;

		// Token: 0x0400029D RID: 669
		private const float RandomnessModifier = 25f;

		// Token: 0x0400029E RID: 670
		private const float ShipRandomnessModifier = 15f;

		// Token: 0x0400029F RID: 671
		private const float MaxDifficulty = 100f;
	}
}

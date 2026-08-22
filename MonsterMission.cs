using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005B RID: 91
	internal class MonsterMission : Mission
	{
		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06000CA1 RID: 3233 RVA: 0x0007255C File Offset: 0x0007075C
		public override bool DisplayAsCompleted
		{
			get
			{
				return this.State > 0;
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06000CA2 RID: 3234 RVA: 0x00072567 File Offset: 0x00070767
		public override bool DisplayAsFailed
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x0007256C File Offset: 0x0007076C
		public override void ClientReadInitial(IReadMessage msg)
		{
			base.ClientReadInitial(msg);
			byte monsterCount = msg.ReadByte();
			for (int i = 0; i < (int)monsterCount; i++)
			{
				Character monster = Character.ReadSpawnData(msg);
				if (monster == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(90, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in MonsterMission.ClientReadInitial: failed to create a monster (mission: ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(", index: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this.monsters.Add(monster);
			}
			if (this.monsters.Count != (int)monsterCount)
			{
				throw new Exception(string.Concat(new string[]
				{
					"Error in MonsterMission.ClientReadInitial: monster count does not match the server count (",
					monsterCount.ToString(),
					" != ",
					this.monsters.Count.ToString(),
					"mission: ",
					this.Prefab.Identifier.ToString(),
					")"
				}));
			}
			this.InitializeMonsters(this.monsters);
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06000CA4 RID: 3236 RVA: 0x00072684 File Offset: 0x00070884
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
				MonsterMission.<get_SonarLabels>d__13 <get_SonarLabels>d__ = new MonsterMission.<get_SonarLabels>d__13(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x000726A4 File Offset: 0x000708A4
		public MonsterMission(MissionPrefab prefab, Location[] locations, Submarine sub) : base(prefab, locations, sub)
		{
			Identifier speciesName = prefab.ConfigElement.GetAttributeIdentifier("monsterfile", Identifier.Empty);
			if (!speciesName.IsEmpty)
			{
				CharacterPrefab characterPrefab = CharacterPrefab.FindBySpeciesName(speciesName);
				if (characterPrefab != null)
				{
					int monsterCount = Math.Min(prefab.ConfigElement.GetAttributeInt("monstercount", 1), 255);
					this.monsterPrefabs.Add(new ValueTuple<CharacterPrefab, Point>(characterPrefab, new Point(monsterCount)));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in monster mission \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\". Could not find a character prefab with the name \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(speciesName);
					defaultInterpolatedStringHandler.AppendLiteral("\".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, prefab.ContentPackage, false, false);
				}
			}
			this.maxSonarMarkerDistance = prefab.ConfigElement.GetAttributeFloat("maxsonarmarkerdistance", 10000f);
			string spawnPosTypeStr = prefab.ConfigElement.GetAttributeString("spawntype", "");
			if (string.IsNullOrWhiteSpace(spawnPosTypeStr) || !Enum.TryParse<Level.PositionType>(spawnPosTypeStr, true, out this.spawnPosType))
			{
				this.spawnPosType = (Level.PositionType.MainPath | Level.PositionType.SidePath);
			}
			foreach (ContentXElement monsterElement in prefab.ConfigElement.GetChildElements("monster"))
			{
				if (GameMain.NetworkMember != null || !monsterElement.GetAttributeBool("multiplayeronly", false))
				{
					speciesName = monsterElement.GetAttributeIdentifier("character", Identifier.Empty);
					int defaultCount = monsterElement.GetAttributeInt("count", -1);
					if (defaultCount < 0)
					{
						defaultCount = monsterElement.GetAttributeInt("amount", 1);
					}
					int min = Math.Min(monsterElement.GetAttributeInt("min", defaultCount), 255);
					int max = Math.Min(Math.Max(min, monsterElement.GetAttributeInt("max", defaultCount)), 255);
					CharacterPrefab characterPrefab2 = CharacterPrefab.FindBySpeciesName(speciesName);
					if (characterPrefab2 != null)
					{
						this.monsterPrefabs.Add(new ValueTuple<CharacterPrefab, Point>(characterPrefab2, new Point(min, max)));
					}
					else
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(80, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Error in monster mission \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(prefab.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\". Could not find a character prefab with the name \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(speciesName);
						defaultInterpolatedStringHandler2.AppendLiteral("\".");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, prefab.ContentPackage, false, false);
					}
				}
			}
			if (this.monsterPrefabs.Any<ValueTuple<CharacterPrefab, Point>>())
			{
				CharacterParams characterParams = new CharacterParams(this.monsterPrefabs.First<ValueTuple<CharacterPrefab, Point>>().Item1.ContentFile as CharacterFile);
				this.description = this.description.Replace("[monster]", TextManager.Get("character." + characterParams.SpeciesTranslationOverride.ToString()).Fallback(TextManager.Get("character." + characterParams.SpeciesName.ToString()), true), StringComparison.Ordinal);
			}
		}

		// Token: 0x06000CA6 RID: 3238 RVA: 0x000729F8 File Offset: 0x00070BF8
		protected override void StartMissionSpecific(Level level)
		{
			if (this.monsters.Count > 0)
			{
				DebugConsole.AddWarning("Monster list was not empty at the start of a monster mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.monsters.Clear();
			}
			if (this.tempSonarPositions.Count > 0)
			{
				DebugConsole.AddWarning("Sonar position list was not empty at the start of a monster mission. The mission instance may not have been ended correctly on previous rounds.", null);
				this.tempSonarPositions.Clear();
			}
			if (!Mission.IsClient)
			{
				float minDistBetweenMonsterMissions = 10000f;
				float mindDistFromSub = (float)Level.Loaded.Size.X * 0.3f;
				IEnumerable<MonsterMission> monsterMissions = from e in GameMain.GameSession.Missions
				select e as MonsterMission into m
				where m != null && m != this && m.spawnPos != null
				select m;
				Level.InterestingPosition spawnPos;
				if (!Level.Loaded.TryGetInterestingPosition(true, this.spawnPosType, mindDistFromSub, out spawnPos, (Level.InterestingPosition p) => monsterMissions.None((MonsterMission m) => Vector2.DistanceSquared(p.Position.ToVector2(), m.spawnPos.Value) < minDistBetweenMonsterMissions * minDistBetweenMonsterMissions), true))
				{
					Level.Loaded.TryGetInterestingPosition(true, this.spawnPosType, mindDistFromSub, out spawnPos, null, false);
				}
				this.spawnPos = new Vector2?(spawnPos.Position.ToVector2());
				foreach (ValueTuple<CharacterPrefab, Point> valueTuple in this.monsterPrefabs)
				{
					CharacterPrefab character = valueTuple.Item1;
					Point amountRange = valueTuple.Item2;
					int amount = Rand.Range(amountRange.X, amountRange.Y + 1, Rand.RandSync.Unsynced);
					for (int i = 0; i < amount; i++)
					{
						this.monsters.Add(Character.Create(character.Identifier, this.spawnPos.Value, ToolBox.RandomSeed(8), null, 0, false, true, false, null, true, true));
					}
				}
				this.InitializeMonsters(this.monsters);
			}
		}

		// Token: 0x06000CA7 RID: 3239 RVA: 0x00072BC8 File Offset: 0x00070DC8
		private void InitializeMonsters(IEnumerable<Character> monsters)
		{
			foreach (Character monster in monsters)
			{
				monster.Enabled = false;
				if (monster.Params.AI != null && monster.Params.AI.EnforceAggressiveBehaviorForMissions)
				{
					monster.Params.AI.FleeHealthThreshold = 0f;
					foreach (CharacterParams.TargetParams targetParam in monster.Params.AI.Targets)
					{
						Identifier tag = targetParam.Tag;
						if (!(tag == "engine"))
						{
							switch (targetParam.State)
							{
							case AIState.Escape:
							case AIState.Flee:
							case AIState.Avoid:
							case AIState.PassiveAggressive:
								targetParam.State = AIState.Attack;
								break;
							}
						}
					}
				}
			}
			SwarmBehavior.CreateSwarm(monsters.Cast<AICharacter>());
			foreach (Character monster2 in monsters)
			{
				this.tempSonarPositions.Add(monster2.WorldPosition + Rand.Vector(this.maxSonarMarkerDistance, Rand.RandSync.Unsynced));
			}
			if (monsters.Count<Character>() != this.tempSonarPositions.Count)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
				defaultInterpolatedStringHandler.AppendLiteral("monsters.Count != tempSonarPositions.Count (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(monsters.Count<Character>());
				defaultInterpolatedStringHandler.AppendLiteral(" != ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.tempSonarPositions.Count);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x06000CA8 RID: 3240 RVA: 0x00072DA8 File Offset: 0x00070FA8
		protected override void MissionStateChanged(int previousState)
		{
			if (previousState == 0 && this.State >= 1)
			{
				SteamTimelineManager.OnMonsterMissionTargetsKilled(this);
			}
		}

		// Token: 0x06000CA9 RID: 3241 RVA: 0x00072DBC File Offset: 0x00070FBC
		protected override void UpdateMissionSpecific(float deltaTime)
		{
			if (this.State == 0)
			{
				for (int j = 0; j < this.tempSonarPositions.Count; j++)
				{
					if (this.monsters.Count != this.tempSonarPositions.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
						defaultInterpolatedStringHandler.AppendLiteral("monsters.Count != tempSonarPositions.Count (");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.monsters.Count);
						defaultInterpolatedStringHandler.AppendLiteral(" != ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(this.tempSonarPositions.Count);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					if (j < 0 || j >= this.monsters.Count)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(30, 3);
						defaultInterpolatedStringHandler2.AppendLiteral("Index ");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(j);
						defaultInterpolatedStringHandler2.AppendLiteral(" outside of bounds 0-");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(this.monsters.Count);
						defaultInterpolatedStringHandler2.AppendLiteral(" (");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(this.tempSonarPositions.Count);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
					if (!this.monsters[j].Removed && !this.monsters[j].IsDead)
					{
						Vector2 diff = this.tempSonarPositions[j] - this.monsters[j].WorldPosition;
						float maxDist = this.maxSonarMarkerDistance;
						Character controlled = Character.Controlled;
						Submarine refSub = ((controlled != null) ? controlled.Submarine : null) ?? Submarine.MainSub;
						if (refSub != null)
						{
							Vector2 refPos = (refSub == null) ? Vector2.Zero : refSub.WorldPosition;
							float subDist = Vector2.Distance(refPos, this.tempSonarPositions[j]) / maxDist;
							maxDist = Math.Min(subDist * subDist * maxDist, maxDist);
							maxDist = Math.Min(Vector2.Distance(refPos, this.monsters[j].WorldPosition), maxDist);
						}
						if (diff.LengthSquared() > maxDist * maxDist)
						{
							this.tempSonarPositions[j] = this.monsters[j].WorldPosition + Vector2.Normalize(diff) * maxDist;
						}
					}
				}
				this.sonarPositions.Clear();
				int i;
				Func<Vector2, bool> <>9__1;
				int i2;
				for (i = 0; i < this.monsters.Count; i = i2 + 1)
				{
					if (!this.monsters[i].Removed && !this.monsters[i].IsDead)
					{
						IEnumerable<Vector2> source = this.sonarPositions;
						Func<Vector2, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((Vector2 p) => Vector2.DistanceSquared(p, this.tempSonarPositions[i]) > 1000000f));
						}
						if (source.All(predicate))
						{
							this.sonarPositions.Add(this.tempSonarPositions[i]);
						}
					}
					i2 = i;
				}
				if (!Mission.IsClient)
				{
					if (this.monsters.All((Character m) => MonsterMission.IsEliminated(m)))
					{
						this.State = 1;
					}
				}
			}
		}

		// Token: 0x06000CAA RID: 3242 RVA: 0x00073100 File Offset: 0x00071300
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.state > 0;
		}

		// Token: 0x06000CAB RID: 3243 RVA: 0x0007310C File Offset: 0x0007130C
		protected override void EndMissionSpecific(bool completed)
		{
			this.tempSonarPositions.Clear();
			this.monsters.Clear();
			if (completed)
			{
				Level level = this.level;
				if (((level != null) ? level.LevelData : null) != null && this.Prefab.Tags.Contains("huntinggrounds"))
				{
					this.level.LevelData.HasHuntingGrounds = false;
				}
			}
		}

		// Token: 0x06000CAC RID: 3244 RVA: 0x00073174 File Offset: 0x00071374
		public static bool IsEliminated(Character enemy)
		{
			if (enemy != null && !enemy.Removed && !enemy.IsDead)
			{
				EnemyAIController ai = enemy.AIController as EnemyAIController;
				return ai != null && ai.State == AIState.Flee;
			}
			return true;
		}

		// Token: 0x04000681 RID: 1665
		[TupleElementNames(new string[]
		{
			"character",
			"amountRange"
		})]
		private readonly HashSet<ValueTuple<CharacterPrefab, Point>> monsterPrefabs = new HashSet<ValueTuple<CharacterPrefab, Point>>();

		// Token: 0x04000682 RID: 1666
		private readonly List<Character> monsters = new List<Character>();

		// Token: 0x04000683 RID: 1667
		private readonly List<Vector2> sonarPositions = new List<Vector2>();

		// Token: 0x04000684 RID: 1668
		private readonly List<Vector2> tempSonarPositions = new List<Vector2>();

		// Token: 0x04000685 RID: 1669
		private readonly float maxSonarMarkerDistance = 10000f;

		// Token: 0x04000686 RID: 1670
		private readonly Level.PositionType spawnPosType;

		// Token: 0x04000687 RID: 1671
		private Vector2? spawnPos;
	}
}

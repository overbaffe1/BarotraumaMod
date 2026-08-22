using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000028 RID: 40
	internal class MonsterMission : Mission
	{
		// Token: 0x060004FB RID: 1275 RVA: 0x0002C188 File Offset: 0x0002A388
		public override void ServerWriteInitial(IWriteMessage msg, Client c)
		{
			base.ServerWriteInitial(msg, c);
			if (this.monsters.Count == 0 && this.monsterPrefabs.Count > 0)
			{
				throw new InvalidOperationException("Server attempted to write monster mission data when no monsters had been spawned.");
			}
			msg.WriteByte((byte)this.monsters.Count);
			foreach (Character monster in this.monsters)
			{
				monster.WriteSpawnData(msg, monster.ID, false);
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060004FC RID: 1276 RVA: 0x0002C224 File Offset: 0x0002A424
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
				MonsterMission.<get_SonarLabels>d__9 <get_SonarLabels>d__ = new MonsterMission.<get_SonarLabels>d__9(-2);
				<get_SonarLabels>d__.<>4__this = this;
				return <get_SonarLabels>d__;
			}
		}

		// Token: 0x060004FD RID: 1277 RVA: 0x0002C244 File Offset: 0x0002A444
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

		// Token: 0x060004FE RID: 1278 RVA: 0x0002C598 File Offset: 0x0002A798
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

		// Token: 0x060004FF RID: 1279 RVA: 0x0002C768 File Offset: 0x0002A968
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

		// Token: 0x06000500 RID: 1280 RVA: 0x0002C948 File Offset: 0x0002AB48
		protected override void MissionStateChanged(int previousState)
		{
			if (previousState == 0)
			{
				int state = this.State;
			}
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x0002C958 File Offset: 0x0002AB58
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

		// Token: 0x06000502 RID: 1282 RVA: 0x0002CC9C File Offset: 0x0002AE9C
		protected override bool DetermineCompleted(CampaignMode.TransitionType transitionType)
		{
			return this.state > 0;
		}

		// Token: 0x06000503 RID: 1283 RVA: 0x0002CCA8 File Offset: 0x0002AEA8
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

		// Token: 0x06000504 RID: 1284 RVA: 0x0002CD10 File Offset: 0x0002AF10
		public static bool IsEliminated(Character enemy)
		{
			if (enemy != null && !enemy.Removed && !enemy.IsDead)
			{
				EnemyAIController ai = enemy.AIController as EnemyAIController;
				return ai != null && ai.State == AIState.Flee;
			}
			return true;
		}

		// Token: 0x0400027A RID: 634
		[TupleElementNames(new string[]
		{
			"character",
			"amountRange"
		})]
		private readonly HashSet<ValueTuple<CharacterPrefab, Point>> monsterPrefabs = new HashSet<ValueTuple<CharacterPrefab, Point>>();

		// Token: 0x0400027B RID: 635
		private readonly List<Character> monsters = new List<Character>();

		// Token: 0x0400027C RID: 636
		private readonly List<Vector2> sonarPositions = new List<Vector2>();

		// Token: 0x0400027D RID: 637
		private readonly List<Vector2> tempSonarPositions = new List<Vector2>();

		// Token: 0x0400027E RID: 638
		private readonly float maxSonarMarkerDistance = 10000f;

		// Token: 0x0400027F RID: 639
		private readonly Level.PositionType spawnPosType;

		// Token: 0x04000280 RID: 640
		private Vector2? spawnPos;
	}
}

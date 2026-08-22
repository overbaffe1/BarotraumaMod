using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002BA RID: 698
	internal class MonsterEvent : Event
	{
		// Token: 0x17000FD1 RID: 4049
		// (get) Token: 0x06003C2A RID: 15402 RVA: 0x00227004 File Offset: 0x00225204
		public IReadOnlyList<Character> Monsters
		{
			get
			{
				return this.monsters;
			}
		}

		// Token: 0x17000FD2 RID: 4050
		// (get) Token: 0x06003C2B RID: 15403 RVA: 0x0022700C File Offset: 0x0022520C
		public Vector2? SpawnPos
		{
			get
			{
				return this.spawnPos;
			}
		}

		// Token: 0x17000FD3 RID: 4051
		// (get) Token: 0x06003C2C RID: 15404 RVA: 0x00227014 File Offset: 0x00225214
		public bool SpawnPending
		{
			get
			{
				return this.spawnPending;
			}
		}

		// Token: 0x17000FD4 RID: 4052
		// (get) Token: 0x06003C2D RID: 15405 RVA: 0x0022701C File Offset: 0x0022521C
		public override Vector2 DebugDrawPos
		{
			get
			{
				Vector2? vector = this.spawnPos;
				if (vector == null)
				{
					return Vector2.Zero;
				}
				return vector.GetValueOrDefault();
			}
		}

		// Token: 0x06003C2E RID: 15406 RVA: 0x00227048 File Offset: 0x00225248
		public override string ToString()
		{
			if (this.MaxAmount <= 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 2);
				defaultInterpolatedStringHandler.AppendLiteral("MonsterEvent (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<Level.PositionType>(this.SpawnPosType);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			if (this.MinAmount < this.MaxAmount)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(20, 4);
				defaultInterpolatedStringHandler2.AppendLiteral("MonsterEvent (");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.SpeciesName);
				defaultInterpolatedStringHandler2.AppendLiteral(" x");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.MinAmount);
				defaultInterpolatedStringHandler2.AppendLiteral("-");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.MaxAmount);
				defaultInterpolatedStringHandler2.AppendLiteral(", ");
				defaultInterpolatedStringHandler2.AppendFormatted<Level.PositionType>(this.SpawnPosType);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				return defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(19, 3);
			defaultInterpolatedStringHandler3.AppendLiteral("MonsterEvent (");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.SpeciesName);
			defaultInterpolatedStringHandler3.AppendLiteral(" x");
			defaultInterpolatedStringHandler3.AppendFormatted<int>(this.MaxAmount);
			defaultInterpolatedStringHandler3.AppendLiteral(", ");
			defaultInterpolatedStringHandler3.AppendFormatted<Level.PositionType>(this.SpawnPosType);
			defaultInterpolatedStringHandler3.AppendLiteral(")");
			return defaultInterpolatedStringHandler3.ToStringAndClear();
		}

		// Token: 0x06003C2F RID: 15407 RVA: 0x002271AC File Offset: 0x002253AC
		public MonsterEvent(EventPrefab prefab, int seed) : base(prefab, seed)
		{
			string speciesFile = prefab.ConfigElement.GetAttributeString("characterfile", "");
			CharacterPrefab characterPrefab = CharacterPrefab.FindByFilePath(speciesFile);
			if (characterPrefab != null)
			{
				this.SpeciesName = characterPrefab.Identifier;
			}
			else
			{
				this.SpeciesName = speciesFile.ToIdentifier();
			}
			if (this.SpeciesName.IsEmpty)
			{
				throw new Exception("speciesname is null!");
			}
			int defaultAmount = prefab.ConfigElement.GetAttributeInt("amount", 1);
			this.MinAmount = prefab.ConfigElement.GetAttributeInt("minamount", defaultAmount);
			this.MaxAmount = Math.Max(prefab.ConfigElement.GetAttributeInt("maxamount", 1), this.MinAmount);
			this.MaxAmountPerLevel = prefab.ConfigElement.GetAttributeInt("maxamountperlevel", int.MaxValue);
			ContentXElement configElement = prefab.ConfigElement;
			string key = "spawntype";
			Level.PositionType positionType = Level.PositionType.MainPath;
			this.SpawnPosType = configElement.GetAttributeEnum<Level.PositionType>(key, positionType);
			if (prefab.ConfigElement.GetAttributeBool("spawndeep", false))
			{
				this.SpawnPosType = Level.PositionType.Abyss;
			}
			this.spawnPointTag = prefab.ConfigElement.GetAttributeString("spawnpointtag", string.Empty);
			this.SpawnDistance = prefab.ConfigElement.GetAttributeFloat("spawndistance", 0f);
			this.offset = prefab.ConfigElement.GetAttributeFloat("offset", 0f);
			this.scatter = Math.Clamp(prefab.ConfigElement.GetAttributeFloat("scatter", 500f), 0f, 3000f);
			this.delayBetweenSpawns = prefab.ConfigElement.GetAttributeFloat("delaybetweenspawns", 0.1f);
			this.resetTime = prefab.ConfigElement.GetAttributeFloat("resettime", 0f);
			float playDeadProbability = prefab.ConfigElement.GetAttributeFloat("playdeadprobability", -1f);
			if (playDeadProbability >= 0f)
			{
				this.overridePlayDeadProbability = new float?(playDeadProbability);
			}
			if (GameMain.NetworkMember != null)
			{
				List<Identifier> monsterNames = GameMain.NetworkMember.ServerSettings.MonsterEnabled.Keys.ToList<Identifier>();
				Identifier tryKey = monsterNames.Find((Identifier s) => this.SpeciesName == s);
				if (!tryKey.IsEmpty && !GameMain.NetworkMember.ServerSettings.MonsterEnabled[tryKey])
				{
					this.disallowed = true;
				}
			}
		}

		// Token: 0x06003C30 RID: 15408 RVA: 0x002273F4 File Offset: 0x002255F4
		private static Submarine GetReferenceSub(bool acceptRemoteControlledSubs)
		{
			return (EventManager.GetRefEntity(acceptRemoteControlledSubs) as Submarine) ?? Submarine.MainSub;
		}

		// Token: 0x06003C31 RID: 15409 RVA: 0x0022740A File Offset: 0x0022560A
		public override IEnumerable<ContentFile> GetFilesToPreload()
		{
			MonsterEvent.<GetFilesToPreload>d__29 <GetFilesToPreload>d__ = new MonsterEvent.<GetFilesToPreload>d__29(-2);
			<GetFilesToPreload>d__.<>4__this = this;
			return <GetFilesToPreload>d__;
		}

		// Token: 0x06003C32 RID: 15410 RVA: 0x0022741C File Offset: 0x0022561C
		protected override void InitEventSpecific(EventSet parentSet)
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode && !networkMember.ServerSettings.PvPSpawnMonsters)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
						defaultInterpolatedStringHandler.AppendLiteral("PvP setting: disabling monster event (");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpeciesName);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Yellow), false);
					}
					this.disallowed = true;
					return;
				}
			}
			if (parentSet != null && this.resetTime == 0f)
			{
				this.resetTime = parentSet.ResetTime;
			}
			if (GameSettings.CurrentConfig.VerboseLogging)
			{
				DebugConsole.NewMessage("Initialized MonsterEvent (" + this.SpeciesName.ToString() + ")", new Color?(Color.White), false);
			}
			this.monsters.Clear();
			int amount = Rand.Range(this.MinAmount, this.MaxAmount + 1, Rand.RandSync.Unsynced);
			for (int i = 0; i < amount; i++)
			{
				string seed = i.ToString() + Level.Loaded.Seed;
				Character createdCharacter = Character.Create(this.SpeciesName, Vector2.Zero, seed, null, 0, false, true, true, null, false, true);
				if (createdCharacter == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(77, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Error in MonsterEvent: failed to spawn the character \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.SpeciesName);
					defaultInterpolatedStringHandler2.AppendLiteral("\". Content package: \"");
					ContentXElement configElement = this.prefab.ConfigElement;
					string text;
					if (configElement == null)
					{
						text = null;
					}
					else
					{
						ContentPackage contentPackage = configElement.ContentPackage;
						text = ((contentPackage != null) ? contentPackage.Name : null);
					}
					defaultInterpolatedStringHandler2.AppendFormatted(text ?? "unknown");
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), base.Prefab.ContentPackage);
					this.disallowed = true;
				}
				else
				{
					if (this.overridePlayDeadProbability != null)
					{
						createdCharacter.EvaluatePlayDeadProbability(this.overridePlayDeadProbability);
					}
					createdCharacter.DisabledByEvent = true;
					this.monsters.Add(createdCharacter);
				}
			}
		}

		// Token: 0x06003C33 RID: 15411 RVA: 0x00227634 File Offset: 0x00225834
		public override string GetDebugInfo()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 5);
			defaultInterpolatedStringHandler.AppendLiteral("Finished: ");
			defaultInterpolatedStringHandler.AppendFormatted(base.IsFinished.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Amount: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.MinAmount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(" - ");
			defaultInterpolatedStringHandler.AppendFormatted(this.MaxAmount.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Spawn pending: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.SpawnPending.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral("\n");
			defaultInterpolatedStringHandler.AppendLiteral("Spawn position: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.SpawnPos.ColorizeObject());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003C34 RID: 15412 RVA: 0x00227728 File Offset: 0x00225928
		private List<Level.InterestingPosition> GetAvailableSpawnPositions()
		{
			List<Level.InterestingPosition> availablePositions = Level.Loaded.PositionsOfInterest.FindAll((Level.InterestingPosition p) => this.SpawnPosType.HasFlag(p.PositionType));
			List<Level.InterestingPosition> removals = new List<Level.InterestingPosition>();
			foreach (Level.InterestingPosition position in availablePositions)
			{
				if (this.SpawnPosFilter != null && !this.SpawnPosFilter(position))
				{
					removals.Add(position);
				}
				else
				{
					if (position.Submarine != null)
					{
						if (position.Submarine.WreckAI == null || !position.Submarine.WreckAI.IsAlive)
						{
							continue;
						}
						removals.Add(position);
					}
					if (position.PositionType == Level.PositionType.MainPath || position.PositionType == Level.PositionType.SidePath)
					{
						Level loaded = Level.Loaded;
						Point position2 = position.Position;
						if (loaded.IsPositionInsideWall(position2.ToVector2()))
						{
							removals.Add(position);
						}
						if ((float)position.Position.Y < Level.Loaded.GetBottomPosition((float)position.Position.X).Y)
						{
							removals.Add(position);
						}
					}
				}
			}
			removals.ForEach(delegate(Level.InterestingPosition r)
			{
				availablePositions.Remove(r);
			});
			return availablePositions;
		}

		// Token: 0x06003C35 RID: 15413 RVA: 0x0022787C File Offset: 0x00225A7C
		private void FindSpawnPosition(bool affectSubImmediately)
		{
			if (this.disallowed)
			{
				return;
			}
			this.spawnPos = new Vector2?(Vector2.Zero);
			List<Level.InterestingPosition> availablePositions = this.GetAvailableSpawnPositions();
			this.chosenPosition = new Level.InterestingPosition(Point.Zero, Level.PositionType.MainPath, null, false);
			bool isRuinOrWreckOrCave = this.SpawnPosType.HasFlag(Level.PositionType.Ruin) || this.SpawnPosType.HasFlag(Level.PositionType.Wreck) || this.SpawnPosType.HasFlag(Level.PositionType.Cave) || this.SpawnPosType.HasFlag(Level.PositionType.AbyssCave);
			if (affectSubImmediately && !isRuinOrWreckOrCave && !this.SpawnPosType.HasFlag(Level.PositionType.Abyss))
			{
				if (availablePositions.None(null))
				{
					this.spawnPos = null;
					this.disallowed = true;
					return;
				}
				Submarine refSub = MonsterEvent.GetReferenceSub(true);
				if (Submarine.MainSubs.Length == 2)
				{
					Submarine submarine = Submarine.MainSubs[1];
					if (submarine != null)
					{
						SubmarineInfo info = submarine.Info;
						if (info != null && info.Type == SubmarineType.Player)
						{
							refSub = Submarine.MainSubs.GetRandom(Rand.RandSync.Unsynced);
						}
					}
				}
				if (refSub != Submarine.MainSub && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.5f && refSub == null)
				{
					refSub = MonsterEvent.GetReferenceSub(false);
				}
				float closestDist = float.PositiveInfinity;
				foreach (Level.InterestingPosition position in availablePositions)
				{
					Point position3 = position.Position;
					Vector2 pos = position3.ToVector2();
					float dist = Vector2.DistanceSquared(pos, refSub.WorldPosition);
					foreach (Submarine sub in Submarine.Loaded)
					{
						if (sub.Info.Type == SubmarineType.Player || sub.Info.Type == SubmarineType.EnemySubmarine || sub.IsRespawnShuttle)
						{
							float minDistToSub = this.GetMinDistanceToSub(sub);
							if (dist >= minDistToSub * minDistToSub)
							{
								if (closestDist == float.PositiveInfinity)
								{
									closestDist = dist;
									this.chosenPosition = position;
								}
								else if ((float)this.chosenPosition.Position.X < refSub.WorldPosition.X)
								{
									if (dist < closestDist || pos.X > refSub.WorldPosition.X)
									{
										closestDist = dist;
										this.chosenPosition = position;
									}
								}
								else if ((float)this.chosenPosition.Position.X > refSub.WorldPosition.X && dist < closestDist && pos.X > refSub.WorldPosition.X)
								{
									closestDist = dist;
									this.chosenPosition = position;
								}
							}
						}
					}
				}
				if (closestDist <= 225000000f)
				{
					goto IL_3C2;
				}
				using (List<Level.InterestingPosition>.Enumerator enumerator3 = availablePositions.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Level.InterestingPosition position2 = enumerator3.Current;
						Point position3 = position2.Position;
						float dist2 = Vector2.DistanceSquared(position3.ToVector2(), refSub.WorldPosition);
						if (dist2 < closestDist)
						{
							closestDist = dist2;
							this.chosenPosition = position2;
						}
					}
					goto IL_3C2;
				}
			}
			if (!isRuinOrWreckOrCave)
			{
				float minDistance = 20000f;
				int j;
				int i;
				Predicate<Level.InterestingPosition> <>9__1;
				for (i = 0; i < Submarine.MainSubs.Length; i = j + 1)
				{
					if (Submarine.MainSubs[i] != null)
					{
						List<Level.InterestingPosition> list = availablePositions;
						Predicate<Level.InterestingPosition> match;
						if ((match = <>9__1) == null)
						{
							match = (<>9__1 = ((Level.InterestingPosition p) => Vector2.DistanceSquared(Submarine.MainSubs[i].WorldPosition, p.Position.ToVector2()) < minDistance * minDistance));
						}
						list.RemoveAll(match);
					}
					j = i;
				}
			}
			if (availablePositions.None(null))
			{
				this.spawnPos = null;
				this.disallowed = true;
				return;
			}
			this.chosenPosition = availablePositions.GetRandomUnsynced<Level.InterestingPosition>();
			IL_3C2:
			if (this.chosenPosition.IsValid)
			{
				this.spawnPos = new Vector2?(this.chosenPosition.Position.ToVector2());
				if (this.chosenPosition.Submarine != null || this.chosenPosition.Ruin != null)
				{
					SpawnType spawnType = SpawnType.Enemy;
					JobPrefab assignedJob = null;
					Submarine sub2;
					if ((sub2 = this.chosenPosition.Submarine) == null)
					{
						Ruin ruin = this.chosenPosition.Ruin;
						sub2 = ((ruin != null) ? ruin.Submarine : null);
					}
					WayPoint spawnPoint = WayPoint.GetRandom(spawnType, assignedJob, sub2, false, this.spawnPointTag, false);
					if (spawnPoint == null)
					{
						this.spawnPos = null;
						this.disallowed = true;
						return;
					}
					this.spawnPos = new Vector2?(spawnPoint.WorldPosition);
				}
				else if (this.chosenPosition.PositionType == Level.PositionType.MainPath || this.chosenPosition.PositionType == Level.PositionType.SidePath)
				{
					if (this.offset > 0f)
					{
						Level.TunnelType tunnelType = (this.chosenPosition.PositionType == Level.PositionType.MainPath) ? Level.TunnelType.MainPath : Level.TunnelType.SidePath;
						List<WayPoint> waypoints = WayPoint.WayPointList.FindAll(delegate(WayPoint wp)
						{
							if (wp.Submarine == null && wp.Ruin == null)
							{
								Level.Tunnel tunnel = wp.Tunnel;
								if (tunnel != null && tunnel.Type == tunnelType)
								{
									return wp.WorldPosition.X > this.spawnPos.Value.X;
								}
							}
							return false;
						});
						if (waypoints.None(null))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Failed to find a spawn position offset from ");
							defaultInterpolatedStringHandler.AppendFormatted<Vector2>(this.spawnPos.Value);
							defaultInterpolatedStringHandler.AppendLiteral(".");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), base.Prefab.ContentPackage);
						}
						else
						{
							float offsetSqr = this.offset * this.offset;
							WayPoint targetWaypoint = (from wp in waypoints
							orderby Math.Abs(Vector2.DistanceSquared(wp.WorldPosition, this.spawnPos.Value) - offsetSqr)
							select wp).FirstOrDefault<WayPoint>();
							if (targetWaypoint != null)
							{
								this.spawnPos = new Vector2?(targetWaypoint.WorldPosition);
							}
						}
					}
					if (Submarine.Loaded.Any((Submarine s) => ToolBox.GetWorldBounds(s.Borders.Center, s.Borders.Size).ContainsWorld(this.spawnPos.Value)))
					{
						this.spawnPos = null;
						this.disallowed = true;
						return;
					}
				}
				this.spawnPending = true;
			}
		}

		// Token: 0x06003C36 RID: 15414 RVA: 0x00227E94 File Offset: 0x00226094
		private float GetMinDistanceToSub(Submarine submarine)
		{
			float minDist = Math.Max((float)Math.Max(submarine.Borders.Width, submarine.Borders.Height), 9000f);
			if (this.SpawnPosType.HasFlag(Level.PositionType.Abyss))
			{
				minDist *= 2f;
			}
			return minDist;
		}

		// Token: 0x06003C37 RID: 15415 RVA: 0x00227EEC File Offset: 0x002260EC
		public override void Update(float deltaTime)
		{
			if (this.disallowed)
			{
				return;
			}
			if (this.resetTimer > 0f)
			{
				this.resetTimer -= deltaTime;
				if (this.resetTimer <= 0f)
				{
					EventSet parentSet = base.ParentSet;
					if (parentSet != null && parentSet.ResetTime > 0f)
					{
						this.Finish();
						return;
					}
					this.spawnReady = false;
					this.spawnPos = null;
				}
				return;
			}
			if (this.spawnPos == null)
			{
				if (this.MaxAmountPerLevel < 2147483647 && Character.CharacterList.Count(delegate(Character c)
				{
					Identifier speciesName = c.SpeciesName;
					return speciesName == this.SpeciesName;
				}) >= this.MaxAmountPerLevel)
				{
					if (this.resetTime == 0f)
					{
						this.disallowed = true;
					}
					return;
				}
				this.FindSpawnPosition(true);
				if (this.isFinished || this.disallowed)
				{
					return;
				}
				this.spawnPending = true;
			}
			if (this.spawnPending)
			{
				if (this.spawnPos == null)
				{
					this.disallowed = true;
					return;
				}
				if (this.SpawnPosType.HasFlag(Level.PositionType.MainPath) || this.SpawnPosType.HasFlag(Level.PositionType.SidePath) || this.SpawnPosType.HasFlag(Level.PositionType.Abyss))
				{
					foreach (Submarine submarine in Submarine.Loaded)
					{
						if (submarine.Info.Type == SubmarineType.Player)
						{
							float minDist = this.GetMinDistanceToSub(submarine);
							if (Vector2.DistanceSquared(submarine.WorldPosition, this.spawnPos.Value) < minDist * minDist)
							{
								return;
							}
						}
					}
				}
				float spawnDistance = this.SpawnDistance;
				if (spawnDistance <= 0f)
				{
					if (this.SpawnPosType.HasFlag(Level.PositionType.Cave))
					{
						spawnDistance = 8000f;
					}
					else if (this.SpawnPosType.HasFlag(Level.PositionType.Ruin))
					{
						spawnDistance = 5000f;
					}
					else if (this.SpawnPosType.HasFlag(Level.PositionType.Wreck) || this.SpawnPosType.HasFlag(Level.PositionType.BeaconStation))
					{
						spawnDistance = 3000f;
					}
				}
				if (spawnDistance > 0f)
				{
					bool someoneNearby = false;
					foreach (Submarine submarine2 in Submarine.Loaded)
					{
						if (submarine2.Info.Type == SubmarineType.Player)
						{
							float distanceSquared = Vector2.DistanceSquared(submarine2.WorldPosition, this.spawnPos.Value);
							if (distanceSquared < MathUtils.Pow2(spawnDistance))
							{
								someoneNearby = true;
								if (this.chosenPosition.Submarine == null)
								{
									break;
								}
								Vector2 from = Submarine.GetRelativeSimPositionFromWorldPosition(this.spawnPos.Value, this.chosenPosition.Submarine, this.chosenPosition.Submarine);
								Vector2 to = Submarine.GetRelativeSimPositionFromWorldPosition(submarine2.WorldPosition, this.chosenPosition.Submarine, submarine2);
								if (MonsterEvent.<Update>g__CheckLineOfSight|36_2(from, to, this.chosenPosition.Submarine))
								{
									return;
								}
							}
						}
					}
					foreach (Character c2 in Character.CharacterList)
					{
						if (c2 == Character.Controlled || c2.IsRemotePlayer)
						{
							float distanceSquared2 = Vector2.DistanceSquared(c2.WorldPosition, this.spawnPos.Value);
							if (distanceSquared2 < MathUtils.Pow2(spawnDistance))
							{
								someoneNearby = true;
								if (this.chosenPosition.Submarine == null)
								{
									break;
								}
								Vector2 from2 = Submarine.GetRelativeSimPositionFromWorldPosition(this.spawnPos.Value, this.chosenPosition.Submarine, this.chosenPosition.Submarine);
								Vector2 to2 = Submarine.GetRelativeSimPositionFromWorldPosition(c2.WorldPosition, this.chosenPosition.Submarine, c2.Submarine);
								if (MonsterEvent.<Update>g__CheckLineOfSight|36_2(from2, to2, this.chosenPosition.Submarine))
								{
									this.disallowed = true;
									return;
								}
							}
						}
					}
					if (!someoneNearby)
					{
						return;
					}
				}
				if (this.SpawnPosType.HasFlag(Level.PositionType.Abyss) || this.SpawnPosType.HasFlag(Level.PositionType.AbyssCave))
				{
					bool anyInAbyss = false;
					foreach (Submarine submarine3 in Submarine.Loaded)
					{
						if (submarine3.Info.Type == SubmarineType.Player && !submarine3.IsRespawnShuttle && submarine3.WorldPosition.Y < 0f)
						{
							anyInAbyss = true;
							break;
						}
					}
					if (!anyInAbyss)
					{
						return;
					}
				}
				this.spawnPending = false;
				float scatterAmount = this.scatter;
				if (this.SpawnPosType.HasFlag(Level.PositionType.SidePath))
				{
					IEnumerable<Level.Tunnel> sidePaths = from t in Level.Loaded.Tunnels
					where t.Type == Level.TunnelType.SidePath
					select t;
					if (sidePaths.Any<Level.Tunnel>())
					{
						scatterAmount = Math.Min(this.scatter, (float)(sidePaths.Min((Level.Tunnel t) => t.MinWidth) / 2));
					}
					else
					{
						scatterAmount = this.scatter;
					}
				}
				else if (this.SpawnPosType.IsIndoorsArea())
				{
					scatterAmount = 0f;
				}
				int i = 0;
				using (List<Character>.Enumerator enumerator5 = this.monsters.GetEnumerator())
				{
					while (enumerator5.MoveNext())
					{
						Character monster = enumerator5.Current;
						CoroutineManager.Invoke(delegate
						{
							if (GameMain.GameSession == null || Level.Loaded == null)
							{
								return;
							}
							if (monster.Removed)
							{
								return;
							}
							Vector2 pos = this.spawnPos.Value;
							if (scatterAmount > 0f)
							{
								int tries = 10;
								Func<Submarine, bool> <>9__6;
								Func<Ruin, bool> <>9__7;
								Func<Level.Cave, bool> <>9__8;
								for (;;)
								{
									tries--;
									pos = this.spawnPos.Value + Rand.Vector(Rand.Range(0f, scatterAmount, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
									bool isValidPos = true;
									IEnumerable<Submarine> loaded = Submarine.Loaded;
									Func<Submarine, bool> predicate;
									if ((predicate = <>9__6) == null)
									{
										predicate = (<>9__6 = ((Submarine s) => ToolBox.GetWorldBounds(s.Borders.Center, s.Borders.Size).ContainsWorld(pos)));
									}
									if (loaded.Any(predicate))
									{
										goto IL_109;
									}
									IEnumerable<Ruin> ruins = Level.Loaded.Ruins;
									Func<Ruin, bool> predicate2;
									if ((predicate2 = <>9__7) == null)
									{
										predicate2 = (<>9__7 = ((Ruin r) => ToolBox.GetWorldBounds(r.Area.Center, r.Area.Size).ContainsWorld(pos)));
									}
									if (ruins.Any(predicate2) || Level.Loaded.IsPositionInsideWall(pos))
									{
										goto IL_109;
									}
									if (this.SpawnPosType.HasFlag(Level.PositionType.Cave) || this.SpawnPosType.HasFlag(Level.PositionType.AbyssCave))
									{
										IEnumerable<Level.Cave> caves = Level.Loaded.Caves;
										Func<Level.Cave, bool> predicate3;
										if ((predicate3 = <>9__8) == null)
										{
											predicate3 = (<>9__8 = ((Level.Cave c) => c.Area.Contains(pos)));
										}
										if (caves.None(predicate3))
										{
											isValidPos = false;
										}
									}
									IL_189:
									if (isValidPos)
									{
										break;
									}
									if (tries == 0)
									{
										pos = this.spawnPos.Value;
									}
									if (tries <= 0)
									{
										break;
									}
									continue;
									IL_109:
									isValidPos = false;
									goto IL_189;
								}
							}
							monster.Enabled = true;
							monster.DisabledByEvent = false;
							monster.AnimController.SetPosition(ConvertUnits.ToSimUnits(pos), false, true, false, true);
							EventManager eventManager = GameMain.GameSession.EventManager;
							if (eventManager != null && monster.Params.AI != null)
							{
								if (this.SpawnPosType.HasFlag(Level.PositionType.MainPath) || this.SpawnPosType.HasFlag(Level.PositionType.SidePath))
								{
									eventManager.CumulativeMonsterStrengthMain += monster.Params.AI.CombatStrength;
									eventManager.AddTimeStamp(this);
								}
								else if (this.SpawnPosType.HasFlag(Level.PositionType.Ruin))
								{
									eventManager.CumulativeMonsterStrengthRuins += monster.Params.AI.CombatStrength;
								}
								else if (this.SpawnPosType.HasFlag(Level.PositionType.Wreck))
								{
									eventManager.CumulativeMonsterStrengthWrecks += monster.Params.AI.CombatStrength;
								}
								else if (this.SpawnPosType.HasFlag(Level.PositionType.Cave))
								{
									eventManager.CumulativeMonsterStrengthCaves += monster.Params.AI.CombatStrength;
								}
							}
							if (monster == this.monsters.Last<Character>())
							{
								this.spawnReady = true;
								SwarmBehavior.CreateSwarm(this.monsters.Cast<AICharacter>());
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Spawned: ");
								defaultInterpolatedStringHandler.AppendFormatted(this.ToString());
								defaultInterpolatedStringHandler.AppendLiteral(". Strength: ");
								defaultInterpolatedStringHandler.AppendFormatted(this.monsters.Sum(delegate(Character m)
								{
									CharacterParams.AIParams ai = m.Params.AI;
									if (ai == null)
									{
										return 0f;
									}
									return ai.CombatStrength;
								}).FormatZeroDecimal());
								defaultInterpolatedStringHandler.AppendLiteral(".");
								DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.LightBlue), true);
							}
							if (GameMain.GameSession != null && monster.ContentPackage == ContentPackageManager.VanillaCorePackage && GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(16, 4);
								defaultInterpolatedStringHandler2.AppendLiteral("MonsterSpawn:");
								GameMode gameMode = GameMain.GameSession.GameMode;
								string text;
								if (gameMode == null)
								{
									text = null;
								}
								else
								{
									GameModePreset preset = gameMode.Preset;
									text = ((preset != null) ? preset.Identifier.Value : null);
								}
								defaultInterpolatedStringHandler2.AppendFormatted(text ?? "none");
								defaultInterpolatedStringHandler2.AppendLiteral(":");
								Level loaded2 = Level.Loaded;
								string text2;
								if (loaded2 == null)
								{
									text2 = null;
								}
								else
								{
									LevelData levelData = loaded2.LevelData;
									if (levelData == null)
									{
										text2 = null;
									}
									else
									{
										Biome biome = levelData.Biome;
										text2 = ((biome != null) ? biome.Identifier.Value : null);
									}
								}
								defaultInterpolatedStringHandler2.AppendFormatted(text2 ?? "none");
								defaultInterpolatedStringHandler2.AppendLiteral(":");
								defaultInterpolatedStringHandler2.AppendFormatted<Level.PositionType>(this.SpawnPosType);
								defaultInterpolatedStringHandler2.AppendLiteral(":");
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.SpeciesName);
								GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler2.ToStringAndClear(), (double)GameMain.GameSession.RoundDuration);
							}
						}, this.delayBetweenSpawns * (float)i);
						i++;
					}
				}
			}
			if (this.spawnReady)
			{
				if (this.monsters.None(null))
				{
					this.Finish();
					return;
				}
				if (this.monsters.All((Character m) => m.IsDead))
				{
					if (this.resetTime > 0f)
					{
						this.resetTimer = this.resetTime;
						return;
					}
					this.Finish();
				}
			}
		}

		// Token: 0x06003C3B RID: 15419 RVA: 0x002285F0 File Offset: 0x002267F0
		[CompilerGenerated]
		internal static bool <Update>g__CheckLineOfSight|36_2(Vector2 from, Vector2 to, Submarine targetSub)
		{
			IEnumerable<Body> bodies = Submarine.PickBodies(from, to, null, new Category?(Category.Cat1), true, null, false);
			foreach (Body b in bodies)
			{
				ISpatialEntity spatialEntity = b.UserData as ISpatialEntity;
				if (spatialEntity == null || spatialEntity.Submarine == targetSub)
				{
					Structure s = b.UserData as Structure;
					if (s != null && !s.IsPlatform && s.CastShadow)
					{
						return false;
					}
					Item item = b.UserData as Item;
					if (item != null)
					{
						Door door = item.GetComponent<Door>();
						if (door != null && !door.IsBroken && !door.IsOpen)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		// Token: 0x04001EE6 RID: 7910
		public readonly Identifier SpeciesName;

		// Token: 0x04001EE7 RID: 7911
		public readonly int MinAmount;

		// Token: 0x04001EE8 RID: 7912
		public readonly int MaxAmount;

		// Token: 0x04001EE9 RID: 7913
		private readonly List<Character> monsters = new List<Character>();

		// Token: 0x04001EEA RID: 7914
		public readonly float SpawnDistance;

		// Token: 0x04001EEB RID: 7915
		private readonly float scatter;

		// Token: 0x04001EEC RID: 7916
		private readonly float offset;

		// Token: 0x04001EED RID: 7917
		private readonly float delayBetweenSpawns;

		// Token: 0x04001EEE RID: 7918
		private float resetTime;

		// Token: 0x04001EEF RID: 7919
		private float resetTimer;

		// Token: 0x04001EF0 RID: 7920
		private Vector2? spawnPos;

		// Token: 0x04001EF1 RID: 7921
		private bool disallowed;

		// Token: 0x04001EF2 RID: 7922
		public readonly Level.PositionType SpawnPosType;

		// Token: 0x04001EF3 RID: 7923
		private readonly string spawnPointTag;

		// Token: 0x04001EF4 RID: 7924
		private bool spawnPending;

		// Token: 0x04001EF5 RID: 7925
		private bool spawnReady;

		// Token: 0x04001EF6 RID: 7926
		public readonly int MaxAmountPerLevel;

		// Token: 0x04001EF7 RID: 7927
		private readonly float? overridePlayDeadProbability;

		// Token: 0x04001EF8 RID: 7928
		private Level.InterestingPosition chosenPosition;
	}
}

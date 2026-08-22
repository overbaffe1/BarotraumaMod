using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Steam;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x0200015C RID: 348
	internal static class AchievementManager
	{
		// Token: 0x06002AA5 RID: 10917 RVA: 0x001D6C74 File Offset: 0x001D4E74
		public static void OnStartRound(Biome biome = null)
		{
			SteamTimelineManager.OnRoundStarted();
			AchievementManager.roundData = new AchievementManager.RoundData();
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine != null && item.Submarine.Info.Type == SubmarineType.Player)
				{
					Reactor reactor = item.GetComponent<Reactor>();
					if (reactor != null && reactor.Item.Condition > 0f)
					{
						AchievementManager.roundData.Reactors.Add(reactor);
					}
				}
			}
			AchievementManager.pathFinder = new PathFinder(WayPoint.WayPointList, false);
			AchievementManager.cachedDistances.Clear();
			if (GameMain.Client != null)
			{
				return;
			}
			if (biome != null)
			{
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.GameMode : null) is CampaignMode)
				{
					string shortBiomeIdentifier = biome.Identifier.Value.Replace(" ", "");
					AchievementManager.UnlockAchievement(("discover" + shortBiomeIdentifier).ToIdentifier(), true, null, null);
					string str = shortBiomeIdentifier;
					Identifier identifier = "europanridge".ToIdentifier();
					if (str == identifier)
					{
						NetworkMember networkMember = GameMain.NetworkMember;
						bool flag;
						if (networkMember == null)
						{
							flag = false;
						}
						else
						{
							ServerSettings serverSettings = networkMember.ServerSettings;
							flag = (((serverSettings != null) ? new RespawnMode?(serverSettings.RespawnMode) : null).GetValueOrDefault() == RespawnMode.Permadeath);
						}
						if (flag)
						{
							AchievementManager.UnlockAchievement("getoutalive".ToIdentifier(), true, null, (Client client) => GameMain.GameSession.PermadeathCountForAccount(client.AccountId) <= 0);
						}
					}
				}
			}
		}

		// Token: 0x06002AA6 RID: 10918 RVA: 0x001D6E0C File Offset: 0x001D500C
		public static void Update(float deltaTime)
		{
			if (GameMain.GameSession == null)
			{
				return;
			}
			if (GameMain.Client != null)
			{
				return;
			}
			AchievementManager.updateTimer -= deltaTime;
			if (AchievementManager.updateTimer > 0f)
			{
				return;
			}
			AchievementManager.updateTimer = 1f;
			if (Level.Loaded != null && AchievementManager.roundData != null && Screen.Selected == GameMain.GameScreen)
			{
				if (GameMain.GameSession.EventManager.CurrentIntensity > 0.99f)
				{
					AchievementManager.UnlockAchievement("maxintensity".ToIdentifier(), true, (Character c) => c != null && !c.IsDead && !c.IsUnconscious, null);
				}
				foreach (Character c3 in Character.CharacterList)
				{
					if (!c3.IsDead && GameMain.GameSession.RoundDuration > 30f)
					{
						if ((c3.Submarine != null && c3.Submarine.AtDamageDepth) || Level.Loaded.GetRealWorldDepth(c3.WorldPosition.Y) > Level.Loaded.RealWorldCrushDepth)
						{
							AchievementManager.roundData.EnteredCrushDepth.Add(c3);
						}
						else if (Level.Loaded.GetRealWorldDepth(c3.WorldPosition.Y) < Level.Loaded.RealWorldCrushDepth - 500f && AchievementManager.roundData.EnteredCrushDepth.Contains(c3))
						{
							AchievementManager.UnlockAchievement(c3, "survivecrushdepth".ToIdentifier());
						}
					}
				}
				using (List<Submarine>.Enumerator enumerator2 = Submarine.Loaded.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Submarine sub = enumerator2.Current;
						foreach (Reactor reactor in AchievementManager.roundData.Reactors)
						{
							if (reactor.Item.Condition <= 0f && reactor.Item.Submarine == sub)
							{
								foreach (Character c2 in Character.CharacterList)
								{
									if (!c2.IsDead && c2.Submarine == sub)
									{
										AchievementManager.roundData.ReactorMeltdown.Add(c2);
									}
								}
							}
						}
						Vector2 submarineVel = Physics.DisplayToRealWorldRatio * ConvertUnits.ToDisplayUnits(sub.Velocity) * 3.6f;
						if (Math.Abs(submarineVel.X) > 50f)
						{
							AchievementManager.UnlockAchievement("subhighvelocity".ToIdentifier(), true, (Character c) => c != null && c.Submarine == sub && !c.IsDead && !c.IsUnconscious, null);
						}
						float realWorldDepth = sub.RealWorldDepth;
						if (realWorldDepth > 5000f && GameMain.GameSession.RoundDuration > 30f)
						{
							AchievementManager.UnlockAchievement("subdeep".ToIdentifier(), true, (Character c) => c != null && c.Submarine == sub && !c.IsDead && !c.IsUnconscious, null);
						}
					}
				}
				if (!AchievementManager.roundData.SubWasDamaged)
				{
					AchievementManager.roundData.SubWasDamaged = AchievementManager.SubWallsDamaged(Submarine.MainSub);
				}
			}
			if (GameMain.GameSession != null && Character.Controlled != null && !(GameMain.GameSession.GameMode is TestGameMode))
			{
				AchievementManager.CheckMidRoundAchievements(Character.Controlled);
			}
		}

		// Token: 0x06002AA7 RID: 10919 RVA: 0x001D71DC File Offset: 0x001D53DC
		private static void CheckMidRoundAchievements(Character c)
		{
			if (c == null || c.Removed)
			{
				return;
			}
			if (c.HasEquippedItem("clownmask".ToIdentifier(), true, null) && c.HasEquippedItem("clowngear".ToIdentifier(), true, null))
			{
				AchievementManager.UnlockAchievement(c, "clowncostume".ToIdentifier());
			}
			if (Submarine.MainSub != null && c.Submarine == null)
			{
				Identifier speciesName = c.SpeciesName;
				if (speciesName == CharacterPrefab.HumanSpeciesName)
				{
					float requiredDist = 500f / Physics.DisplayToRealWorldRatio;
					float distSquared = Vector2.DistanceSquared(c.WorldPosition, Submarine.MainSub.WorldPosition);
					CachedDistance cachedDistance;
					if (AchievementManager.cachedDistances.TryGetValue(c, out cachedDistance))
					{
						if (cachedDistance.ShouldUpdateDistance(c.WorldPosition, Submarine.MainSub.WorldPosition, 500f))
						{
							AchievementManager.cachedDistances.Remove(c);
							cachedDistance = AchievementManager.<CheckMidRoundAchievements>g__CalculateNewCachedDistance|12_0(c);
							if (cachedDistance != null)
							{
								AchievementManager.cachedDistances.Add(c, cachedDistance);
							}
						}
					}
					else
					{
						cachedDistance = AchievementManager.<CheckMidRoundAchievements>g__CalculateNewCachedDistance|12_0(c);
						if (cachedDistance != null)
						{
							AchievementManager.cachedDistances.Add(c, cachedDistance);
						}
					}
					if (cachedDistance != null)
					{
						distSquared = Math.Max(distSquared, cachedDistance.Distance * cachedDistance.Distance);
					}
					if (distSquared > requiredDist * requiredDist)
					{
						AchievementManager.UnlockAchievement(c, "crewaway".ToIdentifier());
					}
				}
			}
		}

		// Token: 0x06002AA8 RID: 10920 RVA: 0x001D732C File Offset: 0x001D552C
		private static bool SubWallsDamaged(Submarine sub)
		{
			foreach (Structure structure in Structure.WallList)
			{
				if (structure.Submarine == sub && !structure.HasBody)
				{
					for (int i = 0; i < structure.SectionCount; i++)
					{
						if (structure.SectionIsLeaking(i))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002AA9 RID: 10921 RVA: 0x001D73AC File Offset: 0x001D55AC
		public static void OnCampaignMetadataSet(Identifier identifier, object value, bool unlockClients = false)
		{
			if (identifier.IsEmpty || value == null)
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
			defaultInterpolatedStringHandler.AppendLiteral("campaignmetadata_");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
			defaultInterpolatedStringHandler.AppendLiteral("_");
			defaultInterpolatedStringHandler.AppendFormatted<object>(value);
			AchievementManager.UnlockAchievement(defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), unlockClients, null, null);
		}

		// Token: 0x06002AAA RID: 10922 RVA: 0x001D740C File Offset: 0x001D560C
		public static void OnItemRepaired(Item item, Character fixer)
		{
			if (GameMain.Client != null)
			{
				return;
			}
			if (fixer == null)
			{
				return;
			}
			AchievementManager.UnlockAchievement(fixer, "repairdevice".ToIdentifier());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
			defaultInterpolatedStringHandler.AppendLiteral("repair");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(item.Prefab.Identifier);
			AchievementManager.UnlockAchievement(fixer, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
		}

		// Token: 0x06002AAB RID: 10923 RVA: 0x001D7470 File Offset: 0x001D5670
		public static void OnButtonTerminalSignal(Item item, Character user)
		{
			if (item == null || user == null)
			{
				return;
			}
			if (GameMain.Client != null)
			{
				return;
			}
			if ((item.Prefab.Identifier == "alienterminal" || item.Prefab.Identifier == "alienterminal_new") && item.Condition <= 0f)
			{
				AchievementManager.UnlockAchievement(user, "ancientnovelty".ToIdentifier());
			}
		}

		// Token: 0x06002AAC RID: 10924 RVA: 0x001D74D7 File Offset: 0x001D56D7
		public static void OnAfflictionReceived(Affliction affliction, Character character)
		{
			if (affliction.Prefab.AchievementOnReceived.IsEmpty)
			{
				return;
			}
			if (GameMain.Client != null)
			{
				return;
			}
			AchievementManager.UnlockAchievement(character, affliction.Prefab.AchievementOnReceived);
		}

		// Token: 0x06002AAD RID: 10925 RVA: 0x001D7505 File Offset: 0x001D5705
		public static void OnAfflictionRemoved(Affliction affliction, Character character)
		{
			if (affliction.Prefab.AchievementOnRemoved.IsEmpty)
			{
				return;
			}
			if (GameMain.Client != null)
			{
				return;
			}
			AchievementManager.UnlockAchievement(character, affliction.Prefab.AchievementOnRemoved);
		}

		// Token: 0x06002AAE RID: 10926 RVA: 0x001D7533 File Offset: 0x001D5733
		public static void OnCharacterRevived(Character character, Character reviver)
		{
			if (GameMain.Client != null)
			{
				return;
			}
			if (reviver == null)
			{
				return;
			}
			AchievementManager.UnlockAchievement(reviver, "healcrit".ToIdentifier());
		}

		// Token: 0x06002AAF RID: 10927 RVA: 0x001D7554 File Offset: 0x001D5754
		private static void CheckSteamTimelineEvents(Character killedCharacter, CauseOfDeath causeOfDeath)
		{
			if (killedCharacter == Character.Controlled)
			{
				SteamTimelineManager.OnPlayerDied(killedCharacter, causeOfDeath);
				return;
			}
			bool flag;
			if (killedCharacter.IsHuman)
			{
				GameSession gameSession = GameMain.GameSession;
				flag = (((gameSession != null) ? gameSession.GameMode : null) is PvPMode);
			}
			else
			{
				flag = false;
			}
			bool pvpkill = flag;
			CharacterParams.AIParams ai = killedCharacter.Params.AI;
			float combatStrength = (ai != null) ? ai.CombatStrength : 0f;
			bool significantCombatStrength = combatStrength >= 300f;
			if (pvpkill || significantCombatStrength)
			{
				SteamTimelineManager.OnSignificantEnemyDied(killedCharacter, causeOfDeath);
			}
		}

		// Token: 0x06002AB0 RID: 10928 RVA: 0x001D75CC File Offset: 0x001D57CC
		public static void OnCharacterKilled(Character character, CauseOfDeath causeOfDeath)
		{
			AchievementManager.CheckSteamTimelineEvents(character, causeOfDeath);
			if (GameMain.Client != null || GameMain.GameSession == null)
			{
				return;
			}
			if (character != Character.Controlled && causeOfDeath.Killer != null && causeOfDeath.Killer == Character.Controlled)
			{
				AchievementManager.IncrementStat(causeOfDeath.Killer, character.IsHuman ? AchievementStat.HumansKilled : AchievementStat.MonstersKilled, 1);
			}
			Character killer = causeOfDeath.Killer;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
			defaultInterpolatedStringHandler.AppendLiteral("kill");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.SpeciesName);
			AchievementManager.UnlockKillAchievement(killer, character, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			if (character.CurrentHull != null)
			{
				Character killer2 = causeOfDeath.Killer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("kill");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(character.SpeciesName);
				defaultInterpolatedStringHandler2.AppendLiteral("indoors");
				AchievementManager.UnlockKillAchievement(killer2, character, defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier());
			}
			if (character.SpeciesName.EndsWith("boss"))
			{
				Character killer3 = causeOfDeath.Killer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("kill");
				defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(character.SpeciesName.Replace("boss", ""));
				AchievementManager.UnlockKillAchievement(killer3, character, defaultInterpolatedStringHandler3.ToStringAndClear().ToIdentifier());
				if (character.CurrentHull != null)
				{
					Character killer4 = causeOfDeath.Killer;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(11, 1);
					defaultInterpolatedStringHandler4.AppendLiteral("kill");
					defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(character.SpeciesName.Replace("boss", ""));
					defaultInterpolatedStringHandler4.AppendLiteral("indoors");
					AchievementManager.UnlockKillAchievement(killer4, character, defaultInterpolatedStringHandler4.ToStringAndClear().ToIdentifier());
				}
			}
			if (character.SpeciesName.EndsWith("_m"))
			{
				Character killer5 = causeOfDeath.Killer;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("kill");
				defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(character.SpeciesName.Replace("_m", ""));
				AchievementManager.UnlockKillAchievement(killer5, character, defaultInterpolatedStringHandler5.ToStringAndClear().ToIdentifier());
				if (character.CurrentHull != null)
				{
					Character killer6 = causeOfDeath.Killer;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(11, 1);
					defaultInterpolatedStringHandler6.AppendLiteral("kill");
					defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(character.SpeciesName.Replace("_m", ""));
					defaultInterpolatedStringHandler6.AppendLiteral("indoors");
					AchievementManager.UnlockKillAchievement(killer6, character, defaultInterpolatedStringHandler6.ToStringAndClear().ToIdentifier());
				}
			}
			if (character.HasEquippedItem("clownmask".ToIdentifier(), true, null) && character.HasEquippedItem("clowncostume".ToIdentifier(), true, null) && causeOfDeath.Killer != character)
			{
				AchievementManager.UnlockAchievement(causeOfDeath.Killer, "killclown".ToIdentifier());
				CharacterHealth characterHealth = character.CharacterHealth;
				if (((characterHealth != null) ? characterHealth.GetAffliction("psychosis", true) : null) != null)
				{
					AchievementManager.UnlockAchievement(causeOfDeath.Killer, "whatsmirksbelow".ToIdentifier());
				}
			}
			CharacterHealth characterHealth2 = character.CharacterHealth;
			if (((characterHealth2 != null) ? characterHealth2.GetAffliction("psychoclown", true) : null) != null)
			{
				Hull currentHull = character.CurrentHull;
				SubmarineInfo submarineInfo = (currentHull != null) ? currentHull.Submarine.Info : null;
				if (submarineInfo != null && submarineInfo.Type == SubmarineType.BeaconStation)
				{
					AchievementManager.UnlockAchievement(causeOfDeath.Killer, "whatsmirksbelow".ToIdentifier());
				}
			}
			CharacterHealth characterHealth3 = character.CharacterHealth;
			if (((characterHealth3 != null) ? characterHealth3.GetAffliction("morbusinepoisoning", true) : null) != null)
			{
				AchievementManager.UnlockAchievement(causeOfDeath.Killer, "killpoison".ToIdentifier());
			}
			Item item = causeOfDeath.DamageSource as Item;
			if (item != null)
			{
				if (item.HasTag(Tags.ToolItem))
				{
					AchievementManager.UnlockAchievement(causeOfDeath.Killer, "killtool".ToIdentifier());
					return;
				}
				if (item.Prefab.Identifier == "morbusine")
				{
					AchievementManager.UnlockAchievement(causeOfDeath.Killer, "killpoison".ToIdentifier());
					return;
				}
				if (item.Prefab.Tags.Contains("nuclearexplosive"))
				{
					AchievementManager.UnlockAchievement(causeOfDeath.Killer, "killnuke".ToIdentifier());
				}
			}
		}

		// Token: 0x06002AB1 RID: 10929 RVA: 0x001D79D8 File Offset: 0x001D5BD8
		private static void UnlockKillAchievement(Character killer, Character target, Identifier identifier)
		{
			GameSession gameSession = GameMain.GameSession;
			bool alwaysUnlockForWholeCrew = ((gameSession != null) ? gameSession.Campaign : null) is SinglePlayerCampaign;
			if (killer != null && (alwaysUnlockForWholeCrew || target.Params.UnlockKillAchievementForWholeCrew) && GameSession.GetSessionCrewCharacters(CharacterType.Both).Contains(killer))
			{
				AchievementManager.UnlockAchievement(identifier, true, (Character c) => c != null, null);
				return;
			}
			AchievementManager.UnlockAchievement(killer, identifier);
		}

		// Token: 0x06002AB2 RID: 10930 RVA: 0x001D7A51 File Offset: 0x001D5C51
		public static void OnTraitorWin(Character character)
		{
			if (GameMain.Client != null || GameMain.GameSession == null)
			{
				return;
			}
			AchievementManager.UnlockAchievement(character, "traitorwin".ToIdentifier());
		}

		// Token: 0x06002AB3 RID: 10931 RVA: 0x001D7A74 File Offset: 0x001D5C74
		public static void OnRoundEnded(GameSession gameSession, bool roundInterrupted = false)
		{
			SteamTimelineManager.OnRoundEnded();
			if (AchievementManager.CheatsEnabled)
			{
				return;
			}
			if (roundInterrupted)
			{
				return;
			}
			GameSession gameSession2 = gameSession;
			if (((gameSession2 != null) ? gameSession2.Submarine : null) != null && Level.Loaded != null && gameSession.Submarine.AtEndExit)
			{
				float levelLengthMeters = Physics.DisplayToRealWorldRatio * (float)Level.Loaded.Size.X;
				float levelLengthKilometers = levelLengthMeters / 1000f;
				if (GameMain.NetworkMember != null)
				{
					Character myCharacter = Character.Controlled;
					if (myCharacter != null && !myCharacter.IsDead)
					{
						if (myCharacter.Submarine != gameSession.Submarine)
						{
							Level loaded = Level.Loaded;
							if (((loaded != null) ? loaded.EndOutpost : null) == null || myCharacter.Submarine != Level.Loaded.EndOutpost)
							{
								goto IL_D0;
							}
						}
						AchievementManager.IncrementStat(AchievementStat.KMsTraveled, levelLengthKilometers);
					}
				}
				else
				{
					AchievementManager.IncrementStat(AchievementStat.KMsTraveled, levelLengthKilometers);
				}
			}
			IL_D0:
			SteamManager.StoreStats();
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			foreach (Mission mission in gameSession.Missions)
			{
				if (mission is CombatMission && GameMain.GameSession.WinningTeam != null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(mission.Prefab.AchievementIdentifier);
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)GameMain.GameSession.WinningTeam.Value);
					Identifier achvIdentifier = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
					AchievementManager.UnlockAchievement(achvIdentifier, true, (Character c) => c != null && !c.IsDead && !c.IsUnconscious && CombatMission.IsInWinningTeam(c), null);
					AchievementManager.UnlockAchievement(mission.Prefab.AchievementIdentifier, true, (Character c) => c != null && !c.IsDead && !c.IsUnconscious && CombatMission.IsInWinningTeam(c), null);
				}
				else if (!(mission is CombatMission) && mission.Completed)
				{
					if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
					{
						AchievementManager.UnlockAchievement(mission.Prefab.AchievementIdentifier, true, (Character c) => c != null, null);
					}
					else
					{
						AchievementManager.UnlockAchievement(mission.Prefab.AchievementIdentifier, false, null, null);
					}
				}
			}
			if (gameSession.Submarine != null && gameSession.Submarine.AtEndExit)
			{
				bool noDamageRun = !AchievementManager.roundData.SubWasDamaged && !gameSession.Casualties.Any<Character>();
				if (noDamageRun)
				{
					AchievementManager.UnlockAchievement("nodamagerun".ToIdentifier(), false, null, null);
				}
				if (AchievementManager.roundData.ReactorMeltdown.Any<Character>())
				{
					AchievementManager.UnlockAchievement("survivereactormeltdown".ToIdentifier(), false, null, null);
				}
				List<Character> charactersInSub = Character.CharacterList.FindAll(delegate(Character c)
				{
					if (c.IsDead || c.TeamID == CharacterTeamType.FriendlyNPC || c.AIController is EnemyAIController)
					{
						return false;
					}
					if (c.Submarine != gameSession.Submarine && !gameSession.Submarine.GetConnectedSubs().Contains(c.Submarine))
					{
						Level loaded2 = Level.Loaded;
						return ((loaded2 != null) ? loaded2.EndOutpost : null) != null && c.Submarine == Level.Loaded.EndOutpost;
					}
					return true;
				});
				if (charactersInSub.Count == 1)
				{
					if (gameSession.Casualties.Any<Character>())
					{
						AchievementManager.UnlockAchievement(charactersInSub[0], "lastmanstanding".ToIdentifier());
					}
					else if (GameMain.GameSession.CrewManager.GetCharacters().Count<Character>() == 1)
					{
						AchievementManager.UnlockAchievement(charactersInSub[0], "lonesailor".ToIdentifier());
					}
				}
				foreach (Character character in charactersInSub)
				{
					if (AchievementManager.roundData.EnteredCrushDepth.Contains(character))
					{
						AchievementManager.UnlockAchievement(character, "survivecrushdepth".ToIdentifier());
					}
					if (character.Info.Job != null)
					{
						Character recipient = character;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(5, 1);
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(character.Info.Job.Prefab.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral("round");
						AchievementManager.UnlockAchievement(recipient, defaultInterpolatedStringHandler2.ToStringAndClear().ToIdentifier());
					}
				}
			}
			AchievementManager.pathFinder = null;
			AchievementManager.roundData = null;
		}

		// Token: 0x06002AB4 RID: 10932 RVA: 0x001D7EB8 File Offset: 0x001D60B8
		private static void UnlockAchievement(Character recipient, Identifier identifier)
		{
			if (AchievementManager.CheatsEnabled || recipient == null)
			{
				return;
			}
			if (recipient == Character.Controlled)
			{
				AchievementManager.UnlockAchievement(identifier, false, null, null);
			}
		}

		// Token: 0x06002AB5 RID: 10933 RVA: 0x001D7ED6 File Offset: 0x001D60D6
		private static void IncrementStat(Character recipient, AchievementStat stat, int amount)
		{
			if (AchievementManager.CheatsEnabled || recipient == null)
			{
				return;
			}
			if (recipient == Character.Controlled)
			{
				AchievementManager.IncrementStat(stat, (float)amount);
			}
		}

		// Token: 0x06002AB6 RID: 10934 RVA: 0x001D7EF4 File Offset: 0x001D60F4
		public static void UnlockAchievement(Identifier identifier, bool unlockClients = false, Func<Character, bool> characterConditions = null, Func<Client, bool> clientConditions = null)
		{
			if (AchievementManager.CheatsEnabled)
			{
				return;
			}
			Screen selected = Screen.Selected;
			if (selected != null && selected.IsEditor)
			{
				return;
			}
			if (!AchievementManager.SupportedAchievements.Contains(identifier))
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.GameMode : null) is TestGameMode)
			{
				return;
			}
			if (characterConditions != null && !characterConditions(Character.Controlled))
			{
				return;
			}
			AchievementManager.UnlockAchievementsOnPlatforms(identifier);
		}

		// Token: 0x06002AB7 RID: 10935 RVA: 0x001D7F5C File Offset: 0x001D615C
		private static void UnlockAchievementsOnPlatforms(Identifier identifier)
		{
			if (AchievementManager.unlockedAchievements.Contains(identifier))
			{
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Attempting to unlock achievement ");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
			defaultInterpolatedStringHandler.AppendLiteral("...");
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			if (SteamManager.IsInitialized && SteamManager.UnlockAchievement(identifier))
			{
				AchievementManager.unlockedAchievements.Add(identifier);
			}
			if (EosInterface.Core.IsInitialized)
			{
				TaskPool.Add("Eos.UnlockAchievementsOnPlatforms", EosInterface.Achievements.UnlockAchievements(new Identifier[]
				{
					identifier
				}), delegate(Task t)
				{
					Result<uint, EosInterface.AchievementUnlockError> result;
					if (!t.TryGetResult(out result))
					{
						return;
					}
					if (result.IsSuccess)
					{
						AchievementManager.unlockedAchievements.Add(identifier);
					}
				});
			}
		}

		// Token: 0x06002AB8 RID: 10936 RVA: 0x001D8029 File Offset: 0x001D6229
		public static void IncrementStat(AchievementStat stat, float amount)
		{
			if (AchievementManager.CheatsEnabled)
			{
				return;
			}
			AchievementManager.IncrementStatOnPlatforms(stat, amount);
		}

		// Token: 0x06002AB9 RID: 10937 RVA: 0x001D803C File Offset: 0x001D623C
		private static void IncrementStatOnPlatforms(AchievementStat stat, float amount)
		{
			if (SteamManager.IsInitialized)
			{
				SteamManager.IncrementStats(new ValueTuple<AchievementStat, float>[]
				{
					stat.ToSteam(amount)
				});
			}
			if (EosInterface.Core.IsInitialized)
			{
				string name = "Eos.IncrementStat";
				Task task = EosInterface.Achievements.IngestStats(new ValueTuple<AchievementStat, int>[]
				{
					stat.ToEos(amount)
				});
				Action<Task> onCompletion;
				if ((onCompletion = AchievementManager.<>O.<0>__IgnoredCallback) == null)
				{
					onCompletion = (AchievementManager.<>O.<0>__IgnoredCallback = new Action<Task>(TaskPool.IgnoredCallback));
				}
				TaskPool.Add(name, task, onCompletion);
			}
		}

		// Token: 0x06002ABA RID: 10938 RVA: 0x001D80B0 File Offset: 0x001D62B0
		public static void SyncBetweenPlatforms()
		{
			if (!SteamManager.IsInitialized || !EosInterface.Core.IsInitialized)
			{
				return;
			}
			ImmutableDictionary<AchievementStat, float> steamStats = SteamManager.GetAllStats();
			Action<ImmutableDictionary<AchievementStat, int>> <>9__4;
			TaskPool.AddWithResult<Result<ImmutableDictionary<AchievementStat, int>, EosInterface.QueryStatsError>>("Eos.SyncBetweenPlatforms.QueryStats", EosInterface.Achievements.QueryStats(AchievementStatExtension.EosStats), delegate([Nullable(new byte[]
			{
				1,
				0
			})] Result<ImmutableDictionary<AchievementStat, int>, EosInterface.QueryStatsError> result)
			{
				Action<ImmutableDictionary<AchievementStat, int>> success;
				if ((success = <>9__4) == null)
				{
					success = (<>9__4 = delegate(ImmutableDictionary<AchievementStat, int> stats)
					{
						AchievementManager.<SyncBetweenPlatforms>g__SyncStats|31_1(stats, steamStats);
					});
				}
				result.Match(success, delegate(EosInterface.QueryStatsError error)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to query stats from EOS: ");
					defaultInterpolatedStringHandler.AppendFormatted<EosInterface.QueryStatsError>(error);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				});
			});
			List<Achievement> steamUnlockedAchievements;
			if (!SteamManager.TryGetUnlockedAchievements(out steamUnlockedAchievements))
			{
				DebugConsole.ThrowError("Failed to query unlocked achievements from Steam", null, null, false, false);
				return;
			}
			Action<ImmutableDictionary<Identifier, double>> <>9__16;
			TaskPool.AddWithResult<Result<ImmutableDictionary<Identifier, double>, EosInterface.QueryAchievementsError>>("Eos.SyncBetweenPlatforms.QueryPlayerAchievements", EosInterface.Achievements.QueryPlayerAchievements(), delegate([Nullable(new byte[]
			{
				1,
				0
			})] Result<ImmutableDictionary<Identifier, double>, EosInterface.QueryAchievementsError> t)
			{
				Action<ImmutableDictionary<Identifier, double>> success;
				if ((success = <>9__16) == null)
				{
					success = (<>9__16 = delegate(ImmutableDictionary<Identifier, double> eosAchievements)
					{
						AchievementManager.<SyncBetweenPlatforms>g__SyncAchievements|31_3(eosAchievements, steamUnlockedAchievements);
					});
				}
				t.Match(success, delegate(EosInterface.QueryAchievementsError error)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to query achievements from EOS: ");
					defaultInterpolatedStringHandler.AppendFormatted<EosInterface.QueryAchievementsError>(error);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				});
			});
		}

		// Token: 0x06002ABB RID: 10939 RVA: 0x001D8138 File Offset: 0x001D6338
		[CompilerGenerated]
		internal static CachedDistance <CheckMidRoundAchievements>g__CalculateNewCachedDistance|12_0(Character c)
		{
			if (AchievementManager.pathFinder == null)
			{
				AchievementManager.pathFinder = new PathFinder(WayPoint.WayPointList, false);
			}
			SteeringPath path = AchievementManager.pathFinder.FindPath(ConvertUnits.ToSimUnits(c.WorldPosition), ConvertUnits.ToSimUnits(Submarine.MainSub.WorldPosition), null, null, 0f, null, null, null, true, 0f);
			if (path.Unreachable)
			{
				return null;
			}
			return new CachedDistance(c.WorldPosition, Submarine.MainSub.WorldPosition, path.TotalLength, Timing.TotalTime + (double)Rand.Range(1f, 5f, Rand.RandSync.Unsynced));
		}

		// Token: 0x06002ABC RID: 10940 RVA: 0x001D81D0 File Offset: 0x001D63D0
		[CompilerGenerated]
		internal static void <SyncBetweenPlatforms>g__SyncStats|31_1(ImmutableDictionary<AchievementStat, int> eosStats, ImmutableDictionary<AchievementStat, float> steamStats)
		{
			ImmutableDictionary<AchievementStat, int> steamStatsConverted = (from s in steamStats
			select s.Key.ToEos(s.Value)).ToImmutableDictionary(([TupleElementNames(new string[]
			{
				"Stat",
				"Value"
			})] ValueTuple<AchievementStat, int> s) => s.Item1, ([TupleElementNames(new string[]
			{
				"Stat",
				"Value"
			})] ValueTuple<AchievementStat, int> s) => s.Item2);
			ImmutableDictionary<AchievementStat, int> eosStatsConverted = (from s in eosStats
			select s.Key.ToEos((float)s.Value)).ToImmutableDictionary(([TupleElementNames(new string[]
			{
				"Stat",
				"Value"
			})] ValueTuple<AchievementStat, int> s) => s.Item1, ([TupleElementNames(new string[]
			{
				"Stat",
				"Value"
			})] ValueTuple<AchievementStat, int> s) => s.Item2);
			Dictionary<AchievementStat, int> highestStats = AchievementStatExtension.EosStats.ToDictionary((AchievementStat key) => key, (AchievementStat value) => Math.Max(AchievementManager.<SyncBetweenPlatforms>g__GetStatValue|31_12(value, steamStatsConverted), AchievementManager.<SyncBetweenPlatforms>g__GetStatValue|31_12(value, eosStatsConverted)));
			List<ValueTuple<AchievementStat, int>> eosStatsToIngest = new List<ValueTuple<AchievementStat, int>>();
			List<ValueTuple<AchievementStat, int>> steamStatsToIncrement = new List<ValueTuple<AchievementStat, int>>();
			foreach (KeyValuePair<AchievementStat, int> keyValuePair in highestStats)
			{
				AchievementStat achievementStat;
				int num;
				keyValuePair.Deconstruct(out achievementStat, out num);
				AchievementStat stat = achievementStat;
				int value2 = num;
				int steamDiff = value2 - AchievementManager.<SyncBetweenPlatforms>g__GetStatValue|31_12(stat, steamStatsConverted);
				int eosDiff = value2 - AchievementManager.<SyncBetweenPlatforms>g__GetStatValue|31_12(stat, eosStatsConverted);
				if (steamDiff > 0)
				{
					steamStatsToIncrement.Add(new ValueTuple<AchievementStat, int>(stat, steamDiff));
				}
				if (eosDiff > 0)
				{
					eosStatsToIngest.Add(new ValueTuple<AchievementStat, int>(stat, eosDiff));
				}
			}
			if (steamStatsToIncrement.Any<ValueTuple<AchievementStat, int>>())
			{
				SteamManager.IncrementStats((from s in steamStatsToIncrement
				select s.Item1.ToSteam((float)s.Item2)).ToArray<ValueTuple<AchievementStat, float>>());
				SteamManager.StoreStats();
			}
			if (eosStatsToIngest.Any<ValueTuple<AchievementStat, int>>())
			{
				string name = "Eos.SyncBetweenPlatforms.IngestStats";
				Task task = EosInterface.Achievements.IngestStats(eosStatsToIngest.ToArray());
				Action<Task> onCompletion;
				if ((onCompletion = AchievementManager.<>O.<0>__IgnoredCallback) == null)
				{
					onCompletion = (AchievementManager.<>O.<0>__IgnoredCallback = new Action<Task>(TaskPool.IgnoredCallback));
				}
				TaskPool.Add(name, task, onCompletion);
			}
		}

		// Token: 0x06002ABD RID: 10941 RVA: 0x001D8414 File Offset: 0x001D6614
		[CompilerGenerated]
		internal static int <SyncBetweenPlatforms>g__GetStatValue|31_12(AchievementStat stat, ImmutableDictionary<AchievementStat, int> stats)
		{
			int value;
			if (!stats.TryGetValue(stat, out value))
			{
				return 0;
			}
			return value;
		}

		// Token: 0x06002ABE RID: 10942 RVA: 0x001D8430 File Offset: 0x001D6630
		[CompilerGenerated]
		internal static void <SyncBetweenPlatforms>g__SyncAchievements|31_3(ImmutableDictionary<Identifier, double> eosAchievements, List<Achievement> steamUnlockedAchievements)
		{
			foreach (KeyValuePair<Identifier, double> keyValuePair in eosAchievements)
			{
				Identifier identifier2;
				double num;
				keyValuePair.Deconstruct(out identifier2, out num);
				Identifier identifier = identifier2;
				double progress = num;
				if (AchievementManager.<SyncBetweenPlatforms>g__IsUnlocked|31_18(progress) && !steamUnlockedAchievements.Any(delegate(Achievement a)
				{
					Identifier identifier3 = a.Identifier.ToIdentifier();
					return identifier3 == identifier;
				}))
				{
					SteamManager.UnlockAchievement(identifier);
				}
			}
			List<Identifier> eosAchievementsToUnlock = new List<Identifier>();
			foreach (Achievement achievement in steamUnlockedAchievements)
			{
				Identifier identifier4 = achievement.Identifier.ToIdentifier();
				double progress2;
				if (!eosAchievements.TryGetValue(identifier4, out progress2) || !AchievementManager.<SyncBetweenPlatforms>g__IsUnlocked|31_18(progress2))
				{
					eosAchievementsToUnlock.Add(achievement.Identifier.ToIdentifier());
				}
			}
			if (eosAchievementsToUnlock.Any<Identifier>())
			{
				string name = "Eos.SyncBetweenPlatforms.UnlockAchievements";
				Task task = EosInterface.Achievements.UnlockAchievements(eosAchievementsToUnlock.ToArray());
				Action<Task> onCompletion;
				if ((onCompletion = AchievementManager.<>O.<0>__IgnoredCallback) == null)
				{
					onCompletion = (AchievementManager.<>O.<0>__IgnoredCallback = new Action<Task>(TaskPool.IgnoredCallback));
				}
				TaskPool.Add(name, task, onCompletion);
			}
		}

		// Token: 0x06002ABF RID: 10943 RVA: 0x001D8568 File Offset: 0x001D6768
		[CompilerGenerated]
		internal static bool <SyncBetweenPlatforms>g__IsUnlocked|31_18(double progress)
		{
			return progress >= 100.0;
		}

		// Token: 0x04001622 RID: 5666
		public static readonly ImmutableHashSet<Identifier> SupportedAchievements = ImmutableHashSet.Create<Identifier>(new Identifier[]
		{
			"killmoloch".ToIdentifier(),
			"killhammerhead".ToIdentifier(),
			"killendworm".ToIdentifier(),
			"artifactmission".ToIdentifier(),
			"combatmission1".ToIdentifier(),
			"combatmission2".ToIdentifier(),
			"healcrit".ToIdentifier(),
			"repairdevice".ToIdentifier(),
			"traitorwin".ToIdentifier(),
			"killtraitor".ToIdentifier(),
			"killclown".ToIdentifier(),
			"healopiateaddiction".ToIdentifier(),
			"survivecrushdepth".ToIdentifier(),
			"survivereactormeltdown".ToIdentifier(),
			"healhusk".ToIdentifier(),
			"killpoison".ToIdentifier(),
			"killnuke".ToIdentifier(),
			"killtool".ToIdentifier(),
			"clowncostume".ToIdentifier(),
			"lastmanstanding".ToIdentifier(),
			"lonesailor".ToIdentifier(),
			"subhighvelocity".ToIdentifier(),
			"nodamagerun".ToIdentifier(),
			"subdeep".ToIdentifier(),
			"maxintensity".ToIdentifier(),
			"discovercoldcaverns".ToIdentifier(),
			"discovereuropanridge".ToIdentifier(),
			"discoverhydrothermalwastes".ToIdentifier(),
			"discovertheaphoticplateau".ToIdentifier(),
			"discoverthegreatsea".ToIdentifier(),
			"travel10".ToIdentifier(),
			"travel100".ToIdentifier(),
			"xenocide".ToIdentifier(),
			"genocide".ToIdentifier(),
			"cargomission".ToIdentifier(),
			"subeditor24h".ToIdentifier(),
			"crewaway".ToIdentifier(),
			"captainround".ToIdentifier(),
			"securityofficerround".ToIdentifier(),
			"engineerround".ToIdentifier(),
			"mechanicround".ToIdentifier(),
			"medicaldoctorround".ToIdentifier(),
			"assistantround".ToIdentifier(),
			"campaigncompleted".ToIdentifier(),
			"salvagewreckmission".ToIdentifier(),
			"escortmission".ToIdentifier(),
			"killcharybdis".ToIdentifier(),
			"killlatcher".ToIdentifier(),
			"killspineling_giant".ToIdentifier(),
			"killcrawlerbroodmother".ToIdentifier(),
			"ascension".ToIdentifier(),
			"campaignmetadata_pathofthebikehorn_7".ToIdentifier(),
			"campaignmetadata_coalitionspecialhire1_hired_true".ToIdentifier(),
			"campaignmetadata_coalitionspecialhire2_hired_true".ToIdentifier(),
			"campaignmetadata_separatistspecialhire1_hired_true".ToIdentifier(),
			"campaignmetadata_separatistspecialhire2_hired_true".ToIdentifier(),
			"campaignmetadata_huskcultspecialhire1_hired_true".ToIdentifier(),
			"campaignmetadata_clownspecialhire1_hired_true".ToIdentifier(),
			"scanruin".ToIdentifier(),
			"clearruin".ToIdentifier(),
			"beaconmission".ToIdentifier(),
			"abandonedoutpostrescue".ToIdentifier(),
			"abandonedoutpostassassinate".ToIdentifier(),
			"abandonedoutpostdestroyhumans".ToIdentifier(),
			"abandonedoutpostdestroymonsters".ToIdentifier(),
			"nestmission".ToIdentifier(),
			"miningmission".ToIdentifier(),
			"combatmissionseparatistsvscoalition".ToIdentifier(),
			"combatmissioncoalitionvsseparatists".ToIdentifier(),
			"getoutalive".ToIdentifier(),
			"abyssbeckons".ToIdentifier(),
			"europasfinest".ToIdentifier(),
			"kingofthehull".ToIdentifier(),
			"killmantis".ToIdentifier(),
			"ancientnovelty".ToIdentifier(),
			"whatsmirksbelow".ToIdentifier()
		});

		// Token: 0x04001623 RID: 5667
		private const float UpdateInterval = 1f;

		// Token: 0x04001624 RID: 5668
		private static readonly HashSet<Identifier> unlockedAchievements = new HashSet<Identifier>();

		// Token: 0x04001625 RID: 5669
		public static bool CheatsEnabled = false;

		// Token: 0x04001626 RID: 5670
		private static float updateTimer;

		// Token: 0x04001627 RID: 5671
		private static AchievementManager.RoundData roundData;

		// Token: 0x04001628 RID: 5672
		private static PathFinder pathFinder;

		// Token: 0x04001629 RID: 5673
		private static readonly Dictionary<Character, CachedDistance> cachedDistances = new Dictionary<Character, CachedDistance>();

		// Token: 0x02000DDB RID: 3547
		private sealed class RoundData
		{
			// Token: 0x040050C4 RID: 20676
			public readonly List<Reactor> Reactors = new List<Reactor>();

			// Token: 0x040050C5 RID: 20677
			public readonly HashSet<Character> EnteredCrushDepth = new HashSet<Character>();

			// Token: 0x040050C6 RID: 20678
			public readonly HashSet<Character> ReactorMeltdown = new HashSet<Character>();

			// Token: 0x040050C7 RID: 20679
			public bool SubWasDamaged;
		}

		// Token: 0x02000DDC RID: 3548
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040050C8 RID: 20680
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Task> <0>__IgnoredCallback;
		}
	}
}

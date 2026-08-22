using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.RuinGeneration;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200032D RID: 813
	internal static class OutpostGenerator
	{
		// Token: 0x060040E9 RID: 16617 RVA: 0x0023E9DE File Offset: 0x0023CBDE
		public static Submarine Generate(OutpostGenerationParams generationParams, LocationType locationType, bool onlyEntrance = false, bool allowInvalidOutpost = false)
		{
			return OutpostGenerator.Generate(generationParams, locationType, null, onlyEntrance, allowInvalidOutpost);
		}

		// Token: 0x060040EA RID: 16618 RVA: 0x0023E9EA File Offset: 0x0023CBEA
		public static Submarine Generate(OutpostGenerationParams generationParams, Location location, bool onlyEntrance = false, bool allowInvalidOutpost = false)
		{
			return OutpostGenerator.Generate(generationParams, location.Type, location, onlyEntrance, allowInvalidOutpost);
		}

		// Token: 0x060040EB RID: 16619 RVA: 0x0023E9FC File Offset: 0x0023CBFC
		private static Submarine Generate(OutpostGenerationParams generationParams, LocationType locationType, Location location, bool onlyEntrance = false, bool allowInvalidOutpost = false)
		{
			OutpostModuleFile[] outpostModuleFiles = (from f in ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<OutpostModuleFile>())
			orderby f.UintIdentifier
			select f).ToArray<OutpostModuleFile>();
			OutpostModuleFile[] uintIdDupes = (from f1 in outpostModuleFiles
			where outpostModuleFiles.Any((OutpostModuleFile f2) => f1 != f2 && f1.UintIdentifier == f2.UintIdentifier)
			select f1).ToArray<OutpostModuleFile>();
			if (uintIdDupes.Any<OutpostModuleFile>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral("OutpostModuleFile UintIdentifier duplicates found: ");
				defaultInterpolatedStringHandler.AppendFormatted<IEnumerable<ContentPath>>(from f in uintIdDupes
				select f.Path);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (location != null)
			{
				if (location.IsCriticallyRadiated())
				{
					OutpostGenerationParams newParams = OutpostGenerationParams.OutpostParams.FirstOrDefault((OutpostGenerationParams p) => p.Identifier == generationParams.ReplaceInRadiation);
					if (newParams != null)
					{
						generationParams = newParams;
					}
				}
				locationType = location.Type;
			}
			Submarine sub = null;
			if (generationParams.OutpostTag.IsEmpty)
			{
				GameSession gameSession = GameMain.GameSession;
				SubmarineInfo forceOutpostModule = (gameSession != null) ? gameSession.ForceOutpostModule : null;
				sub = OutpostGenerator.GenerateFromModules(generationParams, outpostModuleFiles, sub, locationType, location, onlyEntrance, allowInvalidOutpost);
				if (sub != null)
				{
					return sub;
				}
				if (forceOutpostModule != null)
				{
					return null;
				}
			}
			SubmarineInfo prebuiltOutpostInfo = OutpostGenerator.ChooseOutpost(generationParams);
			prebuiltOutpostInfo.Type = SubmarineType.Outpost;
			sub = new Submarine(prebuiltOutpostInfo, true, null, null);
			sub.Info.OutpostGenerationParams = generationParams;
			if (location != null)
			{
				location.RemoveTakenItems();
			}
			OutpostGenerator.EnableFactionSpecificEntities(sub, location);
			return sub;
		}

		// Token: 0x060040EC RID: 16620 RVA: 0x0023EBA8 File Offset: 0x0023CDA8
		private static SubmarineInfo ChooseOutpost(OutpostGenerationParams generationParams)
		{
			OutpostGenerator.<>c__DisplayClass6_0 CS$<>8__locals1 = new OutpostGenerator.<>c__DisplayClass6_0();
			CS$<>8__locals1.generationParams = generationParams;
			List<OutpostFile> outpostFiles = (from f in ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<OutpostFile>())
			where !TutorialPrefab.Prefabs.Any((TutorialPrefab tp) => tp.OutpostPath == f.Path)
			orderby f.UintIdentifier
			select f).ToList<OutpostFile>();
			List<SubmarineInfo> outpostInfos = new List<SubmarineInfo>();
			foreach (OutpostFile outpostFile in outpostFiles)
			{
				outpostInfos.Add(new SubmarineInfo(outpostFile.Path.Value, "", null, true, false));
			}
			List<SubmarineInfo> outpostInfosSuitableForMission = new List<SubmarineInfo>();
			GameSession gameSession = GameMain.GameSession;
			GameMode gameMode = (gameSession != null) ? gameSession.GameMode : null;
			Identifier identifier;
			if (gameMode != null)
			{
				foreach (Mission mission in gameMode.Missions)
				{
					identifier = mission.Prefab.AllowOutpostSelectionFromTag;
					if (!identifier.IsEmpty)
					{
						foreach (SubmarineInfo outpostInfo in outpostInfos)
						{
							if (outpostInfo.OutpostTags.Contains(mission.Prefab.AllowOutpostSelectionFromTag) && !outpostInfosSuitableForMission.Contains(outpostInfo))
							{
								outpostInfosSuitableForMission.Add(outpostInfo);
							}
						}
					}
				}
			}
			OutpostGenerator.<>c__DisplayClass6_0 CS$<>8__locals2 = CS$<>8__locals1;
			NetworkMember networkMember = GameMain.NetworkMember;
			CS$<>8__locals2.serverSettings = ((networkMember != null) ? networkMember.ServerSettings : null);
			if (CS$<>8__locals1.serverSettings != null)
			{
				identifier = CS$<>8__locals1.serverSettings.SelectedOutpostName;
				if (identifier != "Random")
				{
					SubmarineInfo matchingOutpost = outpostInfos.FirstOrDefault(delegate(SubmarineInfo o)
					{
						string name = o.Name;
						Identifier selectedOutpostName = CS$<>8__locals1.serverSettings.SelectedOutpostName;
						return name == selectedOutpostName;
					});
					if ((outpostInfosSuitableForMission.Contains(matchingOutpost) || outpostInfosSuitableForMission.None(null)) && matchingOutpost != null)
					{
						return matchingOutpost;
					}
				}
			}
			if (outpostInfosSuitableForMission.Any<SubmarineInfo>())
			{
				return outpostInfosSuitableForMission.GetRandom(Rand.RandSync.ServerAndClient);
			}
			identifier = CS$<>8__locals1.generationParams.OutpostTag;
			if (identifier.IsEmpty)
			{
				outpostInfos = outpostInfos.FindAll((SubmarineInfo o) => o.OutpostTags.None(null));
			}
			else if (outpostInfos.Any((SubmarineInfo o) => o.OutpostTags.Contains(CS$<>8__locals1.generationParams.OutpostTag)))
			{
				outpostInfos = outpostInfos.FindAll((SubmarineInfo o) => o.OutpostTags.Contains(CS$<>8__locals1.generationParams.OutpostTag));
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(75, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find any outposts with the tag ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.generationParams.OutpostTag);
				defaultInterpolatedStringHandler.AppendLiteral(". Choosing a random one instead...");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (!outpostInfos.Any<SubmarineInfo>())
			{
				throw new Exception("Failed to generate an outpost. Could not generate an outpost from the available outpost modules and there are no pre-built outposts available.");
			}
			return outpostInfos.GetRandom(Rand.RandSync.ServerAndClient);
		}

		// Token: 0x060040ED RID: 16621 RVA: 0x0023EEB4 File Offset: 0x0023D0B4
		private static Submarine GenerateFromModules(OutpostGenerationParams generationParams, OutpostModuleFile[] outpostModuleFiles, Submarine sub, LocationType locationType, Location location, bool onlyEntrance = false, bool allowInvalidOutpost = false)
		{
			OutpostGenerator.<>c__DisplayClass7_0 CS$<>8__locals1 = new OutpostGenerator.<>c__DisplayClass7_0();
			CS$<>8__locals1.generationParams = generationParams;
			CS$<>8__locals1.locationType = locationType;
			CS$<>8__locals1.outpostModules = new List<SubmarineInfo>();
			for (int j = 0; j < outpostModuleFiles.Length; j++)
			{
				OutpostModuleFile outpostModuleFile = outpostModuleFiles[j];
				SubmarineInfo subInfo = new SubmarineInfo(outpostModuleFile.Path.Value, "", null, true, false);
				if (subInfo.OutpostModuleInfo != null)
				{
					if (CS$<>8__locals1.generationParams is RuinGenerationParams)
					{
						if (!subInfo.OutpostModuleInfo.ModuleFlags.Contains("ruin".ToIdentifier()) && !CS$<>8__locals1.generationParams.ModuleCounts.Any((OutpostGenerationParams.ModuleCount m) => subInfo.OutpostModuleInfo.ModuleFlags.Contains(m.Identifier)))
						{
							goto IL_E7;
						}
					}
					else if (subInfo.OutpostModuleInfo.ModuleFlags.Contains("ruin".ToIdentifier()))
					{
						goto IL_E7;
					}
					CS$<>8__locals1.outpostModules.Add(subInfo);
				}
				IL_E7:;
			}
			CS$<>8__locals1.selectedModules = new List<OutpostGenerator.PlacedModule>();
			CS$<>8__locals1.generationFailed = false;
			CS$<>8__locals1.remainingOutpostGenerationTries = 6;
			while (CS$<>8__locals1.remainingOutpostGenerationTries > -1 && CS$<>8__locals1.outpostModules.Any<SubmarineInfo>())
			{
				if (sub != null)
				{
					HashSet<Submarine> connectedSubs = new HashSet<Submarine>
					{
						sub
					};
					foreach (Submarine otherSub in Submarine.Loaded)
					{
						if (otherSub.Submarine == sub)
						{
							connectedSubs.Add(otherSub);
						}
					}
					List<MapEntity> entities = MapEntity.MapEntityList.FindAll((MapEntity e) => connectedSubs.Contains(e.Submarine));
					entities.ForEach(delegate(MapEntity e)
					{
						e.Remove();
					});
					foreach (Submarine otherSub2 in connectedSubs)
					{
						otherSub2.Remove();
					}
					if (CS$<>8__locals1.remainingOutpostGenerationTries <= 0)
					{
						CS$<>8__locals1.generationFailed = true;
						break;
					}
				}
				CS$<>8__locals1.selectedModules.Clear();
				List<Identifier> pendingModuleFlags = new List<Identifier>();
				if (CS$<>8__locals1.generationParams.ModuleCounts.Any<OutpostGenerationParams.ModuleCount>())
				{
					pendingModuleFlags = (onlyEntrance ? CS$<>8__locals1.generationParams.ModuleCounts[0].Identifier.ToEnumerable<Identifier>().ToList<Identifier>() : OutpostGenerator.SelectModules(CS$<>8__locals1.outpostModules, location, CS$<>8__locals1.generationParams));
				}
				using (List<Identifier>.Enumerator enumerator3 = pendingModuleFlags.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Identifier flag = enumerator3.Current;
						if (!(flag == "none"))
						{
							int pendingCount = pendingModuleFlags.Count((Identifier f) => f == flag);
							Func<Identifier, bool> <>9__8;
							int availableModuleCount = (from m in CS$<>8__locals1.outpostModules.Where(delegate(SubmarineInfo m)
							{
								IEnumerable<Identifier> moduleFlags = m.OutpostModuleInfo.ModuleFlags;
								Func<Identifier, bool> predicate;
								if ((predicate = <>9__8) == null)
								{
									predicate = (<>9__8 = ((Identifier f) => f == flag));
								}
								return moduleFlags.Any(predicate);
							})
							select m.OutpostModuleInfo.MaxCount).DefaultIfEmpty(0).Sum();
							if (availableModuleCount < pendingCount)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(270, 2);
								defaultInterpolatedStringHandler.AppendLiteral("Error in outpost generation parameters. Trying to place ");
								defaultInterpolatedStringHandler.AppendFormatted<int>(pendingCount);
								defaultInterpolatedStringHandler.AppendLiteral(" modules of the type \"");
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(flag);
								defaultInterpolatedStringHandler.AppendLiteral("\", but there aren't enough suitable modules available. You may need to increase the \"max count\" value of some of the modules in the sub editor or decrease the number of modules in the outpost.");
								DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
								for (int i = 0; i < pendingCount - availableModuleCount; i++)
								{
									pendingModuleFlags.Remove(flag);
								}
							}
						}
					}
				}
				Identifier identifier = pendingModuleFlags.FirstOrDefault<Identifier>();
				Identifier identifier2 = "airlock".ToIdentifier();
				Identifier initialModuleFlag = identifier.IfEmpty(identifier2);
				pendingModuleFlags.Remove(initialModuleFlag);
				GameSession gameSession = GameMain.GameSession;
				bool hasForceOutpostWithInitialFlag = ((gameSession != null) ? gameSession.ForceOutpostModule : null) != null && GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.ModuleFlags.Contains(initialModuleFlag);
				SubmarineInfo initialModule = hasForceOutpostWithInitialFlag ? GameMain.GameSession.ForceOutpostModule : OutpostGenerator.GetRandomModule(CS$<>8__locals1.outpostModules, initialModuleFlag, CS$<>8__locals1.locationType);
				if (hasForceOutpostWithInitialFlag)
				{
					DebugConsole.NewMessage("Forcing module \"" + GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.Name + "\" as the initial module...", new Color?(Color.Yellow), false);
					OutpostGenerator.usedForceOutpostModule = GameMain.GameSession.ForceOutpostModule;
				}
				if (initialModule == null)
				{
					GameMain.GameSession.ForceOutpostModule = null;
					throw new Exception("Failed to generate an outpost (no airlock modules found).");
				}
				foreach (Identifier initialFlag in initialModule.OutpostModuleInfo.ModuleFlags)
				{
					if (pendingModuleFlags.Contains("initialFlag".ToIdentifier()))
					{
						pendingModuleFlags.Remove(initialFlag);
					}
				}
				if (CS$<>8__locals1.remainingOutpostGenerationTries == 1)
				{
					pendingModuleFlags = pendingModuleFlags.Distinct<Identifier>().ToList<Identifier>();
				}
				CS$<>8__locals1.selectedModules.Add(new OutpostGenerator.PlacedModule(initialModule, null, OutpostModuleInfo.GapPosition.None));
				CS$<>8__locals1.selectedModules.Last<OutpostGenerator.PlacedModule>().FulfilledModuleTypes.Add(initialModuleFlag);
				OutpostGenerator.AppendToModule(CS$<>8__locals1.selectedModules.Last<OutpostGenerator.PlacedModule>(), CS$<>8__locals1.outpostModules.ToList<SubmarineInfo>(), pendingModuleFlags, CS$<>8__locals1.selectedModules, CS$<>8__locals1.locationType, true, CS$<>8__locals1.generationParams is RuinGenerationParams, CS$<>8__locals1.remainingOutpostGenerationTries == 1);
				GameSession gameSession2 = GameMain.GameSession;
				if (((gameSession2 != null) ? gameSession2.ForceOutpostModule : null) != null)
				{
					if (CS$<>8__locals1.remainingOutpostGenerationTries <= 0)
					{
						DebugConsole.ThrowError("Could not force the outpost module \"" + GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.Name + "\" to the outpost. Loading the module as-is...", null, null, false, false);
						return null;
					}
					int remainingOutpostGenerationTries = CS$<>8__locals1.remainingOutpostGenerationTries;
					CS$<>8__locals1.remainingOutpostGenerationTries = remainingOutpostGenerationTries - 1;
				}
				else
				{
					if (GameMain.GameSession != null)
					{
						GameMain.GameSession.ForceOutpostModule = null;
					}
					int remainingOutpostGenerationTries;
					if (pendingModuleFlags.Any((Identifier flag) => flag != "none"))
					{
						if (!allowInvalidOutpost)
						{
							remainingOutpostGenerationTries = CS$<>8__locals1.remainingOutpostGenerationTries;
							CS$<>8__locals1.remainingOutpostGenerationTries = remainingOutpostGenerationTries - 1;
							if (CS$<>8__locals1.remainingOutpostGenerationTries > 0)
							{
								continue;
							}
							DebugConsole.AddSafeError("Could not generate an outpost with all of the required modules. Some modules may not have enough connections at the edges to generate a valid layout. Pending modules: " + string.Join<Identifier>(", ", pendingModuleFlags));
						}
						else
						{
							DebugConsole.AddSafeError("Could not generate an outpost with all of the required modules. Some modules may not have enough connections at the edges to generate a valid layout. Pending modules: " + string.Join<Identifier>(", ", pendingModuleFlags) + ". Won't retry because invalid outposts are allowed.");
						}
					}
					SubmarineInfo outpostInfo = new SubmarineInfo
					{
						Type = SubmarineType.Outpost
					};
					CS$<>8__locals1.generationFailed = false;
					outpostInfo.OutpostGenerationParams = CS$<>8__locals1.generationParams;
					sub = new Submarine(outpostInfo, true, new Func<Submarine, List<MapEntity>>(CS$<>8__locals1.<GenerateFromModules>g__loadEntities|0), null);
					sub.Info.OutpostGenerationParams = CS$<>8__locals1.generationParams;
					if (!CS$<>8__locals1.generationFailed)
					{
						foreach (Hull hull in Hull.HullList)
						{
							if (hull.Submarine == sub && string.IsNullOrEmpty(hull.RoomName))
							{
								hull.RoomName = hull.CreateRoomName();
							}
						}
						if (Level.IsLoadedOutpost && location != null)
						{
							location.RemoveTakenItems();
						}
						foreach (WayPoint wp in WayPoint.WayPointList)
						{
							if (wp.CurrentHull == null && wp.Submarine == sub)
							{
								wp.FindHull();
							}
						}
						OutpostGenerator.EnableFactionSpecificEntities(sub, location);
						return sub;
					}
					remainingOutpostGenerationTries = CS$<>8__locals1.remainingOutpostGenerationTries;
					CS$<>8__locals1.remainingOutpostGenerationTries = remainingOutpostGenerationTries - 1;
				}
			}
			DebugConsole.AddSafeError("Failed to generate an outpost with a valid layout and all the required modules. Trying to use a pre-built outpost instead...");
			return null;
		}

		// Token: 0x060040EE RID: 16622 RVA: 0x0023F6EC File Offset: 0x0023D8EC
		private static List<Identifier> SelectModules(IEnumerable<SubmarineInfo> modules, Location location, OutpostGenerationParams generationParams)
		{
			int totalModuleCount = generationParams.TotalModuleCount;
			int totalModuleCountExcludingOptional = totalModuleCount - generationParams.ModuleCounts.Count((OutpostGenerationParams.ModuleCount m) => m.Probability < 1f);
			List<Identifier> pendingModuleFlags = new List<Identifier>();
			bool availableModulesFound = true;
			Identifier initialModuleFlag = generationParams.ModuleCounts.FirstOrDefault<OutpostGenerationParams.ModuleCount>().Identifier;
			pendingModuleFlags.Add(initialModuleFlag);
			while (pendingModuleFlags.Count < totalModuleCountExcludingOptional && availableModulesFound)
			{
				availableModulesFound = false;
				using (IEnumerator<OutpostGenerationParams.ModuleCount> enumerator = generationParams.ModuleCounts.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						OutpostGenerationParams.ModuleCount moduleCount = enumerator.Current;
						float? forcedDifficulty = Level.ForcedDifficulty;
						float? num;
						if (forcedDifficulty == null)
						{
							if (location == null)
							{
								num = null;
							}
							else
							{
								LevelData levelData = location.LevelData;
								num = ((levelData != null) ? new float?(levelData.Difficulty) : null);
							}
						}
						else
						{
							num = forcedDifficulty;
						}
						float? difficulty = num;
						if (difficulty == null || (difficulty.Value >= moduleCount.MinDifficulty && difficulty.Value <= moduleCount.MaxDifficulty))
						{
							GameSession gameSession = GameMain.GameSession;
							if (((gameSession != null) ? gameSession.ForceOutpostModule : null) == null || !GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.ModuleFlags.Contains(moduleCount.Identifier))
							{
								if (moduleCount.Probability < 1f && Rand.Range(0f, 1f, Rand.RandSync.ServerAndClient) > moduleCount.Probability)
								{
									continue;
								}
								if (!moduleCount.RequiredFaction.IsEmpty)
								{
									Identifier? identifier;
									Identifier? identifier2;
									if (location == null)
									{
										identifier = null;
										identifier2 = identifier;
									}
									else
									{
										Faction faction = location.Faction;
										if (faction == null)
										{
											identifier = null;
											identifier2 = identifier;
										}
										else
										{
											identifier2 = new Identifier?(faction.Prefab.Identifier);
										}
									}
									identifier = identifier2;
									Identifier? identifier3 = new Identifier?(moduleCount.RequiredFaction);
									if (identifier != identifier3)
									{
										Identifier? identifier4;
										Identifier? identifier5;
										if (location == null)
										{
											identifier4 = null;
											identifier5 = identifier4;
										}
										else
										{
											Faction secondaryFaction = location.SecondaryFaction;
											if (secondaryFaction == null)
											{
												identifier4 = null;
												identifier5 = identifier4;
											}
											else
											{
												identifier5 = new Identifier?(secondaryFaction.Prefab.Identifier);
											}
										}
										identifier4 = identifier5;
										Identifier? identifier6 = new Identifier?(moduleCount.RequiredFaction);
										if (identifier4 != identifier6)
										{
											continue;
										}
									}
								}
							}
							if (pendingModuleFlags.Count((Identifier m) => m == moduleCount.Identifier) < generationParams.GetModuleCount(moduleCount.Identifier))
							{
								if (!modules.Any((SubmarineInfo m) => m.OutpostModuleInfo.ModuleFlags.Contains(moduleCount.Identifier)))
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 1);
									defaultInterpolatedStringHandler.AppendLiteral("Failed to add a module to the outpost (no modules with the flag \"");
									defaultInterpolatedStringHandler.AppendFormatted<Identifier>(moduleCount.Identifier);
									defaultInterpolatedStringHandler.AppendLiteral("\" found).");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
								}
								else
								{
									availableModulesFound = true;
									pendingModuleFlags.Add(moduleCount.Identifier);
								}
							}
						}
					}
				}
			}
			from f in pendingModuleFlags
			orderby generationParams.ModuleCounts.First((OutpostGenerationParams.ModuleCount m) => m.Identifier == f).Order, Rand.Value(Rand.RandSync.ServerAndClient)
			select f;
			while (pendingModuleFlags.Count < totalModuleCount && generationParams.AppendToReachTotalModuleCount)
			{
				pendingModuleFlags.Insert(Rand.Int(pendingModuleFlags.Count - 1, Rand.RandSync.ServerAndClient), "none".ToIdentifier());
			}
			pendingModuleFlags.Remove(initialModuleFlag);
			pendingModuleFlags.Insert(0, initialModuleFlag);
			if (pendingModuleFlags.Count > totalModuleCount)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(137, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error during outpost generation. ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(pendingModuleFlags.Count);
				defaultInterpolatedStringHandler2.AppendLiteral(" modules set to be used the outpost, but total module count is only ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(totalModuleCount);
				defaultInterpolatedStringHandler2.AppendLiteral(". Leaving out some of the modules...");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				int removeCount = pendingModuleFlags.Count - totalModuleCount;
				for (int i = 0; i < removeCount; i++)
				{
					pendingModuleFlags.Remove(pendingModuleFlags.Last<Identifier>());
				}
			}
			return pendingModuleFlags;
		}

		// Token: 0x060040EF RID: 16623 RVA: 0x0023FB40 File Offset: 0x0023DD40
		private static bool AppendToModule(OutpostGenerator.PlacedModule currentModule, List<SubmarineInfo> availableModules, List<Identifier> pendingModuleFlags, List<OutpostGenerator.PlacedModule> selectedModules, LocationType locationType, bool tryReplacingCurrentModule = true, bool allowExtendBelowInitialModule = false, bool allowDifferentLocationType = false)
		{
			if (pendingModuleFlags.Count == 0)
			{
				return true;
			}
			List<OutpostGenerator.PlacedModule> placedModules = new List<OutpostGenerator.PlacedModule>();
			foreach (OutpostModuleInfo.GapPosition gapPosition in OutpostGenerator.GapPositions.Randomize(Rand.RandSync.ServerAndClient))
			{
				if (!currentModule.UsedGapPositions.HasFlag(gapPosition) && !OutpostGenerator.<AppendToModule>g__DisallowBelowAirlock|9_2(allowExtendBelowInitialModule, gapPosition, currentModule))
				{
					OutpostGenerator.PlacedModule newModule = null;
					if (currentModule.Info.OutpostModuleInfo.GapPositions.HasFlag(gapPosition))
					{
						newModule = OutpostGenerator.AppendModule(currentModule, OutpostGenerator.GetOpposingGapPosition(gapPosition), availableModules, pendingModuleFlags, selectedModules, locationType, allowDifferentLocationType);
					}
					if (newModule != null)
					{
						placedModules.Add(newModule);
					}
					else
					{
						using (List<OutpostGenerator.PlacedModule>.Enumerator enumerator = selectedModules.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								OutpostGenerator.PlacedModule otherModule = enumerator.Current;
								if (otherModule != currentModule)
								{
									IEnumerable<OutpostModuleInfo.GapPosition> gapPositions = OutpostGenerator.GapPositions;
									Func<OutpostModuleInfo.GapPosition, bool> predicate;
									Func<OutpostModuleInfo.GapPosition, bool> <>9__3;
									if ((predicate = <>9__3) == null)
									{
										predicate = (<>9__3 = ((OutpostModuleInfo.GapPosition g) => !otherModule.UsedGapPositions.HasFlag(g) && otherModule.Info.OutpostModuleInfo.GapPositions.HasFlag(g)));
									}
									foreach (OutpostModuleInfo.GapPosition otherGapPosition in gapPositions.Where(predicate))
									{
										if (!OutpostGenerator.<AppendToModule>g__DisallowBelowAirlock|9_2(allowExtendBelowInitialModule, otherGapPosition, otherModule))
										{
											newModule = OutpostGenerator.AppendModule(otherModule, OutpostGenerator.GetOpposingGapPosition(otherGapPosition), availableModules, pendingModuleFlags, selectedModules, locationType, allowDifferentLocationType);
											if (newModule != null)
											{
												placedModules.Add(newModule);
												break;
											}
										}
									}
									if (newModule != null)
									{
										break;
									}
								}
							}
						}
					}
					if (pendingModuleFlags.Count == 0)
					{
						return true;
					}
				}
			}
			if (placedModules.Count == 0 && tryReplacingCurrentModule && currentModule.PreviousModule != null && !selectedModules.Any((OutpostGenerator.PlacedModule m) => m != currentModule && m.PreviousModule == currentModule))
			{
				for (int i = 0; i < 10; i++)
				{
					selectedModules.Remove(currentModule);
					OutpostGenerator.<AppendToModule>g__assertAllPreviousModulesPresent|9_1();
					pendingModuleFlags.AddRange(currentModule.FulfilledModuleTypes);
					if (!availableModules.Contains(currentModule.Info))
					{
						availableModules.Add(currentModule.Info);
					}
					currentModule = OutpostGenerator.AppendModule(currentModule.PreviousModule, currentModule.ThisGapPosition, availableModules, pendingModuleFlags, selectedModules, locationType, true);
					OutpostGenerator.<AppendToModule>g__assertAllPreviousModulesPresent|9_1();
					if (currentModule == null)
					{
						break;
					}
					if (OutpostGenerator.AppendToModule(currentModule, availableModules, pendingModuleFlags, selectedModules, locationType, false, allowExtendBelowInitialModule, allowDifferentLocationType))
					{
						OutpostGenerator.<AppendToModule>g__assertAllPreviousModulesPresent|9_1();
						return true;
					}
				}
				return false;
			}
			foreach (OutpostGenerator.PlacedModule placedModule in placedModules)
			{
				OutpostGenerator.AppendToModule(placedModule, availableModules, pendingModuleFlags, selectedModules, locationType, true, allowExtendBelowInitialModule, allowDifferentLocationType);
			}
			return placedModules.Count > 0;
		}

		// Token: 0x060040F0 RID: 16624 RVA: 0x0023FE68 File Offset: 0x0023E068
		private static OutpostGenerator.PlacedModule AppendModule(OutpostGenerator.PlacedModule currentModule, OutpostModuleInfo.GapPosition gapPosition, List<SubmarineInfo> availableModules, List<Identifier> pendingModuleFlags, List<OutpostGenerator.PlacedModule> selectedModules, LocationType locationType, bool allowDifferentLocationType)
		{
			OutpostGenerator.<>c__DisplayClass10_0 CS$<>8__locals1 = new OutpostGenerator.<>c__DisplayClass10_0();
			CS$<>8__locals1.currentModule = currentModule;
			if (pendingModuleFlags.Count == 0)
			{
				return null;
			}
			Identifier flagToPlace = "none".ToIdentifier();
			CS$<>8__locals1.nextModule = null;
			Func<Identifier, bool> keySelector;
			if ((keySelector = CS$<>8__locals1.<>9__0) == null)
			{
				keySelector = (CS$<>8__locals1.<>9__0 = delegate(Identifier f)
				{
					OutpostGenerator.PlacedModule currentModule3 = CS$<>8__locals1.currentModule;
					bool? flag;
					if (currentModule3 == null)
					{
						flag = null;
					}
					else
					{
						SubmarineInfo info2 = currentModule3.Info;
						flag = ((info2 != null) ? new bool?(info2.OutpostModuleInfo.AllowAttachToModules.Contains(f)) : null);
					}
					bool? flag2 = flag;
					return flag2.GetValueOrDefault();
				});
			}
			foreach (Identifier moduleFlag in pendingModuleFlags.OrderByDescending(keySelector))
			{
				flagToPlace = moduleFlag;
				OutpostGenerator.<>c__DisplayClass10_0 CS$<>8__locals2 = CS$<>8__locals1;
				OutpostGenerator.PlacedModule currentModule2 = CS$<>8__locals1.currentModule;
				OutpostModuleInfo prevModule;
				if (currentModule2 == null)
				{
					prevModule = null;
				}
				else
				{
					SubmarineInfo info = currentModule2.Info;
					prevModule = ((info != null) ? info.OutpostModuleInfo : null);
				}
				CS$<>8__locals2.nextModule = OutpostGenerator.GetRandomModule(prevModule, availableModules, flagToPlace, gapPosition, locationType, allowDifferentLocationType);
				if (CS$<>8__locals1.nextModule != null)
				{
					break;
				}
			}
			if (CS$<>8__locals1.nextModule != null)
			{
				OutpostGenerator.PlacedModule newModule = new OutpostGenerator.PlacedModule(CS$<>8__locals1.nextModule, CS$<>8__locals1.currentModule, gapPosition)
				{
					Offset = CS$<>8__locals1.currentModule.Offset + OutpostGenerator.GetMoveDir(gapPosition)
				};
				foreach (Identifier moduleFlag2 in CS$<>8__locals1.nextModule.OutpostModuleInfo.ModuleFlags)
				{
					if (pendingModuleFlags.Contains(moduleFlag2) && (moduleFlag2 != "none" || flagToPlace == "none"))
					{
						newModule.FulfilledModuleTypes.Add(moduleFlag2);
						pendingModuleFlags.Remove(moduleFlag2);
					}
				}
				selectedModules.Add(newModule);
				if (selectedModules.Count((OutpostGenerator.PlacedModule m) => m.Info == CS$<>8__locals1.nextModule) >= CS$<>8__locals1.nextModule.OutpostModuleInfo.MaxCount)
				{
					availableModules.Remove(CS$<>8__locals1.nextModule);
				}
				return newModule;
			}
			return null;
		}

		// Token: 0x060040F1 RID: 16625 RVA: 0x00240034 File Offset: 0x0023E234
		private static bool FindOverlap(IEnumerable<OutpostGenerator.PlacedModule> modules1, IEnumerable<OutpostGenerator.PlacedModule> modules2, out OutpostGenerator.PlacedModule module1, out OutpostGenerator.PlacedModule module2)
		{
			module1 = null;
			module2 = null;
			foreach (OutpostGenerator.PlacedModule module3 in modules1)
			{
				foreach (OutpostGenerator.PlacedModule otherModule in modules2)
				{
					if (module3 != otherModule && (module3.PreviousModule != otherModule || module3.PreviousGap.ConnectedDoor != null || module3.ThisGap.ConnectedDoor != null) && OutpostGenerator.ModulesOverlap(module3, otherModule))
					{
						module1 = module3;
						module2 = otherModule;
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060040F2 RID: 16626 RVA: 0x002400EC File Offset: 0x0023E2EC
		private static bool ModulesOverlap(OutpostGenerator.PlacedModule module1, OutpostGenerator.PlacedModule module2)
		{
			Rectangle bounds = module1.Bounds;
			bounds.Location += (module1.Offset + module1.MoveOffset).ToPoint();
			Rectangle bounds2 = module2.Bounds;
			bounds2.Location += (module2.Offset + module2.MoveOffset).ToPoint();
			if (module1.PreviousModule == module2 || module2.PreviousModule == module1)
			{
				bounds.Inflate(-16, -16);
				bounds2.Inflate(-16, -16);
			}
			Rectangle hullBounds = module1.HullBounds;
			hullBounds.Location += (module1.Offset + module1.MoveOffset).ToPoint();
			Rectangle hullBounds2 = module2.HullBounds;
			hullBounds2.Location += (module2.Offset + module2.MoveOffset).ToPoint();
			hullBounds.Inflate(-32, -32);
			hullBounds2.Inflate(-32, -32);
			return hullBounds.Intersects(hullBounds2) || hullBounds.Intersects(bounds2) || hullBounds2.Intersects(bounds);
		}

		// Token: 0x060040F3 RID: 16627 RVA: 0x00240224 File Offset: 0x0023E424
		private static bool ModuleOverlapsWithModuleConnections(IEnumerable<OutpostGenerator.PlacedModule> modules)
		{
			foreach (OutpostGenerator.PlacedModule module in modules)
			{
				Rectangle rect = module.Bounds;
				Point location = rect.Location;
				Vector2 vector = module.Offset + module.MoveOffset;
				rect.Location = location + vector.ToPoint();
				rect.Y += module.Bounds.Height;
				Vector2? selfGapPos = null;
				Vector2? selfGapPos2 = null;
				if (module.PreviousModule != null)
				{
					selfGapPos = new Vector2?(module.Offset + module.ThisGap.Position + module.MoveOffset);
					selfGapPos2 = new Vector2?(module.PreviousModule.Offset + module.PreviousGap.Position + module.PreviousModule.MoveOffset);
				}
				foreach (OutpostGenerator.PlacedModule otherModule in modules)
				{
					if (otherModule != module && otherModule.PreviousModule != null && otherModule.PreviousModule != module)
					{
						for (int i = -1; i <= 1; i += 2)
						{
							Vector2 gapEdgeOffset = otherModule.ThisGap.IsHorizontal ? (Vector2.UnitY * (float)otherModule.ThisGap.Rect.Height / 2f * (float)i * 0.9f) : (Vector2.UnitX * (float)otherModule.ThisGap.Rect.Width / 2f * (float)i * 0.9f);
							Vector2 gapPos = otherModule.Offset + otherModule.ThisGap.Position + gapEdgeOffset + otherModule.MoveOffset;
							Vector2 gapPos2 = otherModule.PreviousModule.Offset + otherModule.PreviousGap.Position + gapEdgeOffset + otherModule.PreviousModule.MoveOffset;
							if (Submarine.RectContains(rect, gapPos, false) || Submarine.RectContains(rect, gapPos2, false) || MathUtils.GetLineWorldRectangleIntersection(gapPos, gapPos2, rect, out vector))
							{
								return true;
							}
							if (selfGapPos != null && selfGapPos2 != null && !gapPos.NearlyEquals(gapPos2) && !selfGapPos.Value.NearlyEquals(selfGapPos2.Value) && MathUtils.LineSegmentsIntersect(gapPos, gapPos2, selfGapPos.Value, selfGapPos2.Value))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060040F4 RID: 16628 RVA: 0x00240518 File Offset: 0x0023E718
		private static bool ModuleBelowInitialModule(OutpostGenerator.PlacedModule module, OutpostGenerator.PlacedModule initialModule)
		{
			Rectangle bounds = module.Bounds;
			bounds.Location += (module.Offset + module.MoveOffset).ToPoint();
			Rectangle initialModuleBounds = initialModule.Bounds;
			initialModuleBounds.Location += (initialModule.Offset + initialModule.MoveOffset).ToPoint();
			return bounds.Y < initialModuleBounds.Y;
		}

		// Token: 0x060040F5 RID: 16629 RVA: 0x00240598 File Offset: 0x0023E798
		private static bool FindOverlapSolution(IEnumerable<OutpostGenerator.PlacedModule> movableModules, OutpostGenerator.PlacedModule module1, OutpostGenerator.PlacedModule module2, IEnumerable<OutpostGenerator.PlacedModule> allmodules, float minMoveAmount, int maxMoveAmount, out Dictionary<OutpostGenerator.PlacedModule, Vector2> solution)
		{
			solution = new Dictionary<OutpostGenerator.PlacedModule, Vector2>();
			foreach (OutpostGenerator.PlacedModule module3 in movableModules)
			{
				solution[module3] = Vector2.Zero;
			}
			Vector2 shortestMove = new Vector2(float.MaxValue, float.MaxValue);
			bool solutionFound = false;
			foreach (OutpostGenerator.PlacedModule module4 in movableModules)
			{
				if (module4.ThisGap.ConnectedDoor != null || module4.PreviousGap.ConnectedDoor != null)
				{
					Vector2 moveDir = OutpostGenerator.GetMoveDir(module4.ThisGapPosition);
					Vector2 currentMove = moveDir * Math.Max(minMoveAmount, 50f);
					List<OutpostGenerator.PlacedModule> subsequentModules2 = new List<OutpostGenerator.PlacedModule>();
					OutpostGenerator.GetSubsequentModules(module4, movableModules, ref subsequentModules2);
					while (currentMove.LengthSquared() < (float)(maxMoveAmount * maxMoveAmount))
					{
						foreach (OutpostGenerator.PlacedModule movedModule in subsequentModules2)
						{
							movedModule.MoveOffset = currentMove;
						}
						if (!OutpostGenerator.ModulesOverlap(module1, module2) && !OutpostGenerator.ModuleOverlapsWithModuleConnections(allmodules) && currentMove.LengthSquared() < shortestMove.LengthSquared())
						{
							shortestMove = currentMove;
							using (IEnumerator<OutpostGenerator.PlacedModule> enumerator4 = allmodules.GetEnumerator())
							{
								while (enumerator4.MoveNext())
								{
									OutpostGenerator.PlacedModule movedModule2 = enumerator4.Current;
									solution[movedModule2] = (subsequentModules2.Contains(movedModule2) ? currentMove : Vector2.Zero);
									solutionFound = true;
								}
								break;
							}
						}
						currentMove += moveDir * 50f;
					}
					foreach (OutpostGenerator.PlacedModule movedModule3 in allmodules)
					{
						movedModule3.MoveOffset = Vector2.Zero;
					}
				}
			}
			return solutionFound;
		}

		// Token: 0x060040F6 RID: 16630 RVA: 0x002407F8 File Offset: 0x0023E9F8
		private static SubmarineInfo GetRandomModule(IEnumerable<SubmarineInfo> modules, Identifier moduleFlag, LocationType locationType)
		{
			IEnumerable<SubmarineInfo> availableModules;
			if (moduleFlag.IsEmpty || moduleFlag == "none")
			{
				availableModules = from m in modules
				where !m.OutpostModuleInfo.ModuleFlags.Any<Identifier>() || m.OutpostModuleInfo.ModuleFlags.Contains("none".ToIdentifier())
				select m;
			}
			else
			{
				availableModules = from m in modules
				where m.OutpostModuleInfo.ModuleFlags.Contains(moduleFlag)
				select m;
				if (moduleFlag != "hallwayhorizontal" && moduleFlag != "hallwayvertical")
				{
					availableModules = from m in availableModules
					where !m.OutpostModuleInfo.ModuleFlags.Contains("hallwayhorizontal".ToIdentifier()) && !m.OutpostModuleInfo.ModuleFlags.Contains("hallwayvertical".ToIdentifier())
					select m;
				}
			}
			if (!availableModules.Any<SubmarineInfo>())
			{
				return null;
			}
			IEnumerable<SubmarineInfo> modulesSuitableForLocationType = from m in availableModules
			where m.OutpostModuleInfo.IsAllowedInLocationType(locationType, false)
			select m;
			if (!modulesSuitableForLocationType.Any<SubmarineInfo>())
			{
				modulesSuitableForLocationType = from m in availableModules
				where m.OutpostModuleInfo.IsAllowedInAnyLocationType()
				select m;
			}
			if (!modulesSuitableForLocationType.Any<SubmarineInfo>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a suitable module for the location type ");
				defaultInterpolatedStringHandler.AppendFormatted<LocationType>(locationType);
				defaultInterpolatedStringHandler.AppendLiteral(". Module flag: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(moduleFlag);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Orange), false);
				return ToolBox.SelectWeightedRandom<SubmarineInfo>(availableModules.ToList<SubmarineInfo>(), (from m in availableModules
				select m.OutpostModuleInfo.Commonness).ToList<float>(), Rand.RandSync.ServerAndClient);
			}
			return ToolBox.SelectWeightedRandom<SubmarineInfo>(modulesSuitableForLocationType.ToList<SubmarineInfo>(), (from m in modulesSuitableForLocationType
			select m.OutpostModuleInfo.Commonness).ToList<float>(), Rand.RandSync.ServerAndClient);
		}

		// Token: 0x060040F7 RID: 16631 RVA: 0x002409E4 File Offset: 0x0023EBE4
		private static SubmarineInfo GetRandomModule(OutpostModuleInfo prevModule, IEnumerable<SubmarineInfo> modules, Identifier moduleFlag, OutpostModuleInfo.GapPosition gapPosition, LocationType locationType, bool allowDifferentLocationType)
		{
			OutpostGenerator.<>c__DisplayClass17_0 CS$<>8__locals1 = new OutpostGenerator.<>c__DisplayClass17_0();
			CS$<>8__locals1.moduleFlag = moduleFlag;
			CS$<>8__locals1.gapPosition = gapPosition;
			CS$<>8__locals1.locationType = locationType;
			CS$<>8__locals1.prevModule = prevModule;
			IEnumerable<SubmarineInfo> modulesWithCorrectFlags;
			if (CS$<>8__locals1.moduleFlag.IsEmpty || CS$<>8__locals1.moduleFlag.Equals("none"))
			{
				modulesWithCorrectFlags = from m in modules
				where !m.OutpostModuleInfo.ModuleFlags.Any<Identifier>() || (m.OutpostModuleInfo.ModuleFlags.Count<Identifier>() == 1 && m.OutpostModuleInfo.ModuleFlags.Contains("none".ToIdentifier()))
				select m;
			}
			else
			{
				modulesWithCorrectFlags = from m in modules
				where m.OutpostModuleInfo.ModuleFlags.Contains(CS$<>8__locals1.moduleFlag)
				select m;
			}
			modulesWithCorrectFlags = from m in modulesWithCorrectFlags
			where m.OutpostModuleInfo.GapPositions.HasFlag(CS$<>8__locals1.gapPosition) && m.OutpostModuleInfo.CanAttachToPrevious.HasFlag(CS$<>8__locals1.gapPosition)
			select m;
			IEnumerable<SubmarineInfo> suitableModules = CS$<>8__locals1.<GetRandomModule>g__GetSuitableModules|4(modulesWithCorrectFlags, true, true, true);
			IEnumerable<SubmarineInfo> suitableModulesForAnyOutpost = CS$<>8__locals1.<GetRandomModule>g__GetSuitableModules|4(modulesWithCorrectFlags, true, true, false);
			if (!suitableModules.Any<SubmarineInfo>())
			{
				suitableModules = suitableModulesForAnyOutpost;
				if (!suitableModules.Any<SubmarineInfo>())
				{
					suitableModules = CS$<>8__locals1.<GetRandomModule>g__GetSuitableModules|4(modulesWithCorrectFlags, false, true, true);
				}
				if (!suitableModules.Any<SubmarineInfo>())
				{
					suitableModules = CS$<>8__locals1.<GetRandomModule>g__GetSuitableModules|4(modulesWithCorrectFlags, false, true, false);
				}
			}
			if (suitableModules.Any<SubmarineInfo>())
			{
				SubmarineInfo suitableModule = ToolBox.SelectWeightedRandom<SubmarineInfo>(suitableModules.ToList<SubmarineInfo>(), (from m in suitableModules
				select m.OutpostModuleInfo.Commonness).ToList<float>(), Rand.RandSync.ServerAndClient);
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.ForceOutpostModule : null) != null)
				{
					if (!suitableModules.Any((SubmarineInfo module) => module.OutpostModuleInfo.Name == GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.Name))
					{
						if (!suitableModulesForAnyOutpost.Any((SubmarineInfo module) => module.OutpostModuleInfo.Name == GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.Name))
						{
							if (GameMain.GameSession.ForceOutpostModule.OutpostModuleInfo.ModuleFlags.Contains(CS$<>8__locals1.moduleFlag))
							{
								return null;
							}
							return suitableModule;
						}
					}
					SubmarineInfo forceOutpostModule = GameMain.GameSession.ForceOutpostModule;
					GameMain.GameSession.ForceOutpostModule = null;
					OutpostGenerator.usedForceOutpostModule = forceOutpostModule;
					return forceOutpostModule;
				}
				return suitableModule;
			}
			if (allowDifferentLocationType && modulesWithCorrectFlags.Any<SubmarineInfo>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(71, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find a suitable module for the location type ");
				defaultInterpolatedStringHandler.AppendFormatted<LocationType>(CS$<>8__locals1.locationType);
				defaultInterpolatedStringHandler.AppendLiteral(". Module flag: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals1.moduleFlag);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Orange), false);
				return ToolBox.SelectWeightedRandom<SubmarineInfo>(modulesWithCorrectFlags.ToList<SubmarineInfo>(), (from m in modulesWithCorrectFlags
				select m.OutpostModuleInfo.Commonness).ToList<float>(), Rand.RandSync.ServerAndClient);
			}
			return null;
		}

		// Token: 0x060040F8 RID: 16632 RVA: 0x00240C68 File Offset: 0x0023EE68
		private static void GetSubsequentModules(OutpostGenerator.PlacedModule startModule, IEnumerable<OutpostGenerator.PlacedModule> allModules, ref List<OutpostGenerator.PlacedModule> subsequentModules)
		{
			subsequentModules.Add(startModule);
			foreach (OutpostGenerator.PlacedModule module in allModules)
			{
				if (module.PreviousModule == startModule)
				{
					OutpostGenerator.GetSubsequentModules(module, allModules, ref subsequentModules);
				}
			}
		}

		// Token: 0x060040F9 RID: 16633 RVA: 0x00240CC4 File Offset: 0x0023EEC4
		private static OutpostModuleInfo.GapPosition GetOpposingGapPosition(OutpostModuleInfo.GapPosition thisGapPosition)
		{
			switch (thisGapPosition)
			{
			case OutpostModuleInfo.GapPosition.None:
				return OutpostModuleInfo.GapPosition.None;
			case OutpostModuleInfo.GapPosition.Right:
				return OutpostModuleInfo.GapPosition.Left;
			case OutpostModuleInfo.GapPosition.Left:
				return OutpostModuleInfo.GapPosition.Right;
			case OutpostModuleInfo.GapPosition.Top:
				return OutpostModuleInfo.GapPosition.Bottom;
			case OutpostModuleInfo.GapPosition.Bottom:
				return OutpostModuleInfo.GapPosition.Top;
			}
			throw new ArgumentException();
		}

		// Token: 0x060040FA RID: 16634 RVA: 0x00240D18 File Offset: 0x0023EF18
		private static Vector2 GetMoveDir(OutpostModuleInfo.GapPosition thisGapPosition)
		{
			switch (thisGapPosition)
			{
			case OutpostModuleInfo.GapPosition.None:
				return Vector2.Zero;
			case OutpostModuleInfo.GapPosition.Right:
				return -Vector2.UnitX;
			case OutpostModuleInfo.GapPosition.Left:
				return Vector2.UnitX;
			case OutpostModuleInfo.GapPosition.Top:
				return -Vector2.UnitY;
			case OutpostModuleInfo.GapPosition.Bottom:
				return Vector2.UnitY;
			}
			throw new ArgumentException();
		}

		// Token: 0x060040FB RID: 16635 RVA: 0x00240D8C File Offset: 0x0023EF8C
		private static Gap GetGap(IEnumerable<MapEntity> entities, OutpostModuleInfo.GapPosition gapPosition)
		{
			Gap selectedGap = null;
			foreach (MapEntity entity in entities)
			{
				Gap gap = entity as Gap;
				if (gap != null && (gap.ConnectedDoor == null || gap.ConnectedDoor.UseBetweenOutpostModules))
				{
					switch (gapPosition)
					{
					case OutpostModuleInfo.GapPosition.Right:
						if (gap.IsHorizontal && (selectedGap == null || gap.WorldPosition.X > selectedGap.WorldPosition.X) && !entities.Any((MapEntity e) => e is Hull && e.WorldPosition.X > gap.WorldPosition.X && gap.WorldRect.Y - gap.WorldRect.Height <= e.WorldRect.Y && gap.WorldRect.Y >= e.WorldRect.Y - e.WorldRect.Height))
						{
							selectedGap = gap;
						}
						break;
					case OutpostModuleInfo.GapPosition.Left:
						if (gap.IsHorizontal && (selectedGap == null || gap.WorldPosition.X < selectedGap.WorldPosition.X) && !entities.Any((MapEntity e) => e is Hull && e.WorldPosition.X < gap.WorldPosition.X && gap.WorldRect.Y - gap.WorldRect.Height <= e.WorldRect.Y && gap.WorldRect.Y >= e.WorldRect.Y - e.WorldRect.Height))
						{
							selectedGap = gap;
						}
						break;
					case OutpostModuleInfo.GapPosition.Right | OutpostModuleInfo.GapPosition.Left:
						break;
					case OutpostModuleInfo.GapPosition.Top:
						if (!gap.IsHorizontal && (selectedGap == null || gap.WorldPosition.Y > selectedGap.WorldPosition.Y) && !entities.Any((MapEntity e) => e is Hull && e.WorldPosition.Y > gap.WorldPosition.Y && gap.WorldRect.Right >= e.WorldRect.X && gap.WorldRect.X <= e.WorldRect.Right))
						{
							selectedGap = gap;
						}
						break;
					default:
						if (gapPosition == OutpostModuleInfo.GapPosition.Bottom)
						{
							if (!gap.IsHorizontal && (selectedGap == null || gap.WorldPosition.Y < selectedGap.WorldPosition.Y) && !entities.Any((MapEntity e) => e is Hull && e.WorldPosition.Y < gap.WorldPosition.Y && gap.WorldRect.Right >= e.WorldRect.X && gap.WorldRect.X <= e.WorldRect.Right))
							{
								selectedGap = gap;
							}
						}
						break;
					}
				}
			}
			return selectedGap;
		}

		// Token: 0x060040FC RID: 16636 RVA: 0x00240F88 File Offset: 0x0023F188
		private static bool CanAttachTo(OutpostModuleInfo from, OutpostModuleInfo to)
		{
			if (from.AllowAttachToModules.Any<Identifier>())
			{
				if (!from.AllowAttachToModules.All((Identifier s) => s == "any"))
				{
					return from.AllowAttachToModules.Any((Identifier s) => to.ModuleFlags.Contains(s));
				}
			}
			return true;
		}

		// Token: 0x060040FD RID: 16637 RVA: 0x00240FF4 File Offset: 0x0023F1F4
		private static List<MapEntity> GenerateHallways(Submarine sub, LocationType locationType, IEnumerable<OutpostGenerator.PlacedModule> placedModules, IEnumerable<SubmarineInfo> availableModules, Dictionary<OutpostGenerator.PlacedModule, List<MapEntity>> allEntities, bool isRuin)
		{
			List<MapEntity> placedEntities = new List<MapEntity>();
			using (IEnumerator<OutpostGenerator.PlacedModule> enumerator = placedModules.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					OutpostGenerator.PlacedModule module = enumerator.Current;
					if (module.PreviousModule != null)
					{
						Powered powered = Powered.PoweredList.FirstOrDefault(delegate(Powered p)
						{
							PowerTransfer pt = p as PowerTransfer;
							return pt != null && OutpostGenerator.<GenerateHallways>g__IsLinked|24_2(module.ThisGap, pt);
						});
						ConnectionPanel connectionPanel;
						if (powered == null)
						{
							connectionPanel = null;
						}
						else
						{
							Item item2 = powered.Item;
							connectionPanel = ((item2 != null) ? item2.GetComponent<ConnectionPanel>() : null);
						}
						ConnectionPanel thisJunctionBox = connectionPanel;
						Powered powered2 = Powered.PoweredList.FirstOrDefault(delegate(Powered p)
						{
							PowerTransfer pt = p as PowerTransfer;
							return pt != null && OutpostGenerator.<GenerateHallways>g__IsLinked|24_2(module.PreviousGap, pt);
						});
						ConnectionPanel connectionPanel2;
						if (powered2 == null)
						{
							connectionPanel2 = null;
						}
						else
						{
							Item item3 = powered2.Item;
							connectionPanel2 = ((item3 != null) ? item3.GetComponent<ConnectionPanel>() : null);
						}
						ConnectionPanel previousJunctionBox = connectionPanel2;
						if (thisJunctionBox != null && previousJunctionBox != null)
						{
							int i = 0;
							while (i < thisJunctionBox.Connections.Count && i < previousJunctionBox.Connections.Count)
							{
								ItemPrefab wirePrefab = MapEntityPrefab.FindByIdentifier((thisJunctionBox.Connections[i].IsPower ? "redwire" : "bluewire").ToIdentifier()) as ItemPrefab;
								Wire wire = new Item(wirePrefab, thisJunctionBox.Item.Position, sub, 0, true).GetComponent<Wire>();
								if (!thisJunctionBox.Connections[i].TryAddLink(wire))
								{
									DebugConsole.AddWarning("Failed to connect junction boxes between outpost modules (not enough free connections in module \"" + module.Info.Name + "\")", null);
								}
								else if (!previousJunctionBox.Connections[i].TryAddLink(wire))
								{
									DebugConsole.AddWarning("Failed to connect junction boxes between outpost modules (not enough free connections in module \"" + module.PreviousModule.Info.Name + "\")", null);
								}
								else
								{
									wire.TryConnect(thisJunctionBox.Connections[i], false, false);
									wire.TryConnect(previousJunctionBox.Connections[i], false, false);
									wire.SetNodes(new List<Vector2>());
								}
								i++;
							}
						}
						bool isHorizontal = module.ThisGapPosition == OutpostModuleInfo.GapPosition.Left || module.ThisGapPosition == OutpostModuleInfo.GapPosition.Right;
						if (!module.ThisGap.linkedTo.Any<MapEntity>())
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 2);
							defaultInterpolatedStringHandler.AppendLiteral("Error during outpost generation: ");
							defaultInterpolatedStringHandler.AppendFormatted<OutpostModuleInfo.GapPosition>(module.ThisGapPosition);
							defaultInterpolatedStringHandler.AppendLiteral(" gap in module \"");
							defaultInterpolatedStringHandler.AppendFormatted(module.Info.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\" was not linked to any hulls.");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
						}
						else if (!module.PreviousGap.linkedTo.Any<MapEntity>())
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(79, 2);
							defaultInterpolatedStringHandler2.AppendLiteral("Error during outpost generation: ");
							defaultInterpolatedStringHandler2.AppendFormatted<OutpostModuleInfo.GapPosition>(OutpostGenerator.GetOpposingGapPosition(module.ThisGapPosition));
							defaultInterpolatedStringHandler2.AppendLiteral(" gap in module \"");
							defaultInterpolatedStringHandler2.AppendFormatted(module.PreviousModule.Info.Name);
							defaultInterpolatedStringHandler2.AppendLiteral("\" was not linked to any hulls.");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
						}
						else
						{
							MapEntity leftHull = (module.ThisGap.Position.X < module.PreviousGap.Position.X) ? module.ThisGap.linkedTo[0] : module.PreviousGap.linkedTo[0];
							MapEntity rightHull = (module.ThisGap.Position.X > module.PreviousGap.Position.X) ? ((module.ThisGap.linkedTo.Count == 1) ? module.ThisGap.linkedTo[0] : module.ThisGap.linkedTo[1]) : ((module.PreviousGap.linkedTo.Count == 1) ? module.PreviousGap.linkedTo[0] : module.PreviousGap.linkedTo[1]);
							MapEntity topHull = (module.ThisGap.Position.Y > module.PreviousGap.Position.Y) ? module.ThisGap.linkedTo[0] : module.PreviousGap.linkedTo[0];
							MapEntity bottomHull = (module.ThisGap.Position.Y < module.PreviousGap.Position.Y) ? ((module.ThisGap.linkedTo.Count == 1) ? module.ThisGap.linkedTo[0] : module.ThisGap.linkedTo[1]) : ((module.PreviousGap.linkedTo.Count == 1) ? module.PreviousGap.linkedTo[0] : module.PreviousGap.linkedTo[1]);
							float hallwayLength = (float)(isHorizontal ? (rightHull.WorldRect.X - leftHull.WorldRect.Right) : (topHull.WorldRect.Y - topHull.RectHeight - bottomHull.WorldRect.Y));
							if (module.ThisGap != null && module.ThisGap.ConnectedDoor == null)
							{
								foreach (MapEntity otherEntity in allEntities[module])
								{
									Structure structure = otherEntity as Structure;
									if (structure != null && structure.HasBody && !structure.IsPlatform && structure.RemoveIfLinkedOutpostDoorInUse && Submarine.RectContains(structure.WorldRect, module.ThisGap.WorldPosition, false))
									{
										structure.Remove();
									}
								}
							}
							if (module.PreviousGap != null && module.PreviousGap.ConnectedDoor == null)
							{
								foreach (MapEntity otherEntity2 in allEntities[module.PreviousModule])
								{
									Structure structure2 = otherEntity2 as Structure;
									if (structure2 != null && structure2.HasBody && !structure2.IsPlatform && structure2.RemoveIfLinkedOutpostDoorInUse && Submarine.RectContains(structure2.WorldRect, module.PreviousGap.WorldPosition, false))
									{
										structure2.Remove();
									}
								}
							}
							if (hallwayLength <= 32f && module.ThisGap != null && module.PreviousGap != null)
							{
								Gap gapToRemove = (module.ThisGap.ConnectedDoor == null) ? module.ThisGap : module.PreviousGap;
								Gap otherGap = (gapToRemove == module.ThisGap) ? module.PreviousGap : module.ThisGap;
								Door connectedDoor = gapToRemove.ConnectedDoor;
								if (connectedDoor != null)
								{
									connectedDoor.Item.linkedTo.ForEachMod(delegate(MapEntity lt)
									{
										Structure structure3 = lt as Structure;
										if (structure3 == null)
										{
											return;
										}
										structure3.Remove();
									});
								}
								Door connectedDoor2 = gapToRemove.ConnectedDoor;
								if (((connectedDoor2 != null) ? connectedDoor2.Item.Connections : null) != null)
								{
									foreach (Connection c in gapToRemove.ConnectedDoor.Item.Connections)
									{
										c.Wires.ToArray<Wire>().ForEach(delegate(Wire w)
										{
											if (w != null)
											{
												w.Item.Remove();
											}
										});
									}
								}
								WayPoint thisWayPoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.ConnectedGap == gapToRemove);
								WayPoint previousWayPoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.ConnectedGap == otherGap);
								if (thisWayPoint != null && previousWayPoint != null)
								{
									foreach (MapEntity me in thisWayPoint.linkedTo)
									{
										WayPoint wayPoint = me as WayPoint;
										if (wayPoint != null && !previousWayPoint.linkedTo.Contains(wayPoint))
										{
											previousWayPoint.linkedTo.Add(wayPoint);
										}
									}
									thisWayPoint.Remove();
								}
								else
								{
									if (thisWayPoint == null)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(94, 2);
										defaultInterpolatedStringHandler3.AppendLiteral("Failed to connect waypoints between outpost modules. No waypoint in the ");
										defaultInterpolatedStringHandler3.AppendFormatted(module.ThisGapPosition.ToString().ToLower());
										defaultInterpolatedStringHandler3.AppendLiteral(" gap of the module \"");
										defaultInterpolatedStringHandler3.AppendFormatted(module.Info.Name);
										defaultInterpolatedStringHandler3.AppendLiteral("\".");
										DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, null, false, false);
									}
									if (previousWayPoint == null)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(94, 2);
										defaultInterpolatedStringHandler4.AppendLiteral("Failed to connect waypoints between outpost modules. No waypoint in the ");
										defaultInterpolatedStringHandler4.AppendFormatted(OutpostGenerator.GetOpposingGapPosition(module.ThisGapPosition).ToString().ToLower());
										defaultInterpolatedStringHandler4.AppendLiteral(" gap of the module \"");
										defaultInterpolatedStringHandler4.AppendFormatted(module.PreviousModule.Info.Name);
										defaultInterpolatedStringHandler4.AppendLiteral("\".");
										DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
									}
								}
								Door connectedDoor3 = gapToRemove.ConnectedDoor;
								if (connectedDoor3 != null)
								{
									connectedDoor3.Item.Remove();
								}
								if (hallwayLength <= 1f)
								{
									Gap gapToRemove2 = gapToRemove;
									if (gapToRemove2 != null)
									{
										gapToRemove2.Remove();
									}
								}
							}
							if (hallwayLength > 1f)
							{
								Identifier moduleFlag = (isHorizontal ? "hallwayhorizontal" : "hallwayvertical").ToIdentifier();
								IEnumerable<SubmarineInfo> hallwayModules = from m in availableModules
								where m.OutpostModuleInfo.ModuleFlags.Contains(moduleFlag)
								select m;
								Func<Identifier, bool> <>9__12;
								Func<Identifier, bool> <>9__13;
								IEnumerable<SubmarineInfo> suitableHallwayModules = hallwayModules.Where(delegate(SubmarineInfo m)
								{
									IEnumerable<Identifier> allowAttachToModules = m.OutpostModuleInfo.AllowAttachToModules;
									Func<Identifier, bool> predicate;
									if ((predicate = <>9__12) == null)
									{
										predicate = (<>9__12 = ((Identifier s) => module.Info.OutpostModuleInfo.ModuleFlags.Contains(s)));
									}
									if (allowAttachToModules.Any(predicate))
									{
										IEnumerable<Identifier> allowAttachToModules2 = m.OutpostModuleInfo.AllowAttachToModules;
										Func<Identifier, bool> predicate2;
										if ((predicate2 = <>9__13) == null)
										{
											predicate2 = (<>9__13 = ((Identifier s) => module.PreviousModule.Info.OutpostModuleInfo.ModuleFlags.Contains(s)));
										}
										return allowAttachToModules2.Any(predicate2);
									}
									return false;
								});
								if (suitableHallwayModules.None(null))
								{
									suitableHallwayModules = hallwayModules.Where(delegate(SubmarineInfo m)
									{
										if (m.OutpostModuleInfo.AllowAttachToModules.Any<Identifier>())
										{
											return m.OutpostModuleInfo.AllowAttachToModules.All((Identifier s) => s == "any");
										}
										return true;
									});
								}
								SubmarineInfo hallwayInfo = OutpostGenerator.GetRandomModule(suitableHallwayModules, moduleFlag, locationType);
								if (hallwayInfo == null)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(119, 3);
									defaultInterpolatedStringHandler5.AppendLiteral("Generating hallways between outpost modules failed. No ");
									defaultInterpolatedStringHandler5.AppendFormatted(isHorizontal ? "horizontal" : "vertical");
									defaultInterpolatedStringHandler5.AppendLiteral(" hallway modules suitable for use between the modules \"");
									defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(module.Info.DisplayName);
									defaultInterpolatedStringHandler5.AppendLiteral("\" and \"");
									defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(module.PreviousModule.Info.DisplayName);
									defaultInterpolatedStringHandler5.AppendLiteral("\".");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler5.ToStringAndClear(), null, null, false, false);
									return placedEntities;
								}
								List<MapEntity> moduleEntities = MapEntity.LoadAll(sub, hallwayInfo.SubmarineElement, hallwayInfo.FilePath, -1);
								moduleEntities.Where(delegate(MapEntity e)
								{
									Item item4 = e as Item;
									return item4 != null && item4.GetComponent<Door>() == null && (float)(isHorizontal ? e.Rect.Width : e.Rect.Height) > hallwayLength;
								}).ForEach(delegate(MapEntity e)
								{
									e.Remove();
								});
								Vector2 hullCenter = Vector2.Zero;
								Rectangle hullBounds = Rectangle.Empty;
								float largestHullVolume = 0f;
								foreach (MapEntity me2 in moduleEntities)
								{
									Hull hull = me2 as Hull;
									if (hull != null)
									{
										if (hull.Volume > largestHullVolume)
										{
											largestHullVolume = hull.Volume;
											hullCenter = hull.WorldPosition;
										}
										hullBounds = new Rectangle(Math.Min(hullBounds.X, me2.WorldRect.X), Math.Min(hullBounds.Y, me2.WorldRect.Y - me2.WorldRect.Height), Math.Max(hullBounds.Width, me2.WorldRect.Right), Math.Max(hullBounds.Height, me2.WorldRect.Y));
									}
								}
								hullBounds.Width -= hullBounds.X;
								hullBounds.Height -= hullBounds.Y;
								float scaleFactor = isHorizontal ? (hallwayLength / (float)hullBounds.Width) : (hallwayLength / (float)hullBounds.Height);
								placedEntities.AddRange(moduleEntities);
								MapEntity.InitializeLoadedLinks(moduleEntities);
								Vector2 moveAmount = (module.ThisGap.Position + module.PreviousGap.Position) / 2f - hullCenter;
								Submarine.RepositionEntities(moveAmount, moduleEntities);
								hullBounds.Location += moveAmount.ToPoint();
								foreach (MapEntity me3 in moduleEntities)
								{
									if (me3 is Hull)
									{
										if (hallwayLength <= 32f)
										{
											if (isHorizontal)
											{
												int midX = (leftHull.Rect.Right + rightHull.Rect.X) / 2;
												leftHull.Rect = new Rectangle(leftHull.Rect.X, leftHull.Rect.Y, midX - leftHull.Rect.X, leftHull.Rect.Height);
												rightHull.Rect = new Rectangle(midX, rightHull.Rect.Y, rightHull.Rect.Right - midX, rightHull.Rect.Height);
											}
											else
											{
												int midY = (topHull.Rect.Y - topHull.Rect.Height + bottomHull.Rect.Y) / 2;
												topHull.Rect = new Rectangle(topHull.Rect.X, topHull.Rect.Y, topHull.Rect.Width, topHull.Rect.Y - midY);
												bottomHull.Rect = new Rectangle(bottomHull.Rect.X, midY, bottomHull.Rect.Width, midY - (bottomHull.Rect.Y - bottomHull.Rect.Height));
											}
											me3.Remove();
										}
										else if (isHorizontal)
										{
											me3.Rect = new Rectangle(leftHull.Rect.Right, me3.Rect.Y, rightHull.Rect.X - leftHull.Rect.Right, me3.Rect.Height);
										}
										else
										{
											me3.Rect = new Rectangle(me3.Rect.X, topHull.Rect.Y - topHull.Rect.Height, me3.Rect.Width, topHull.Rect.Y - topHull.Rect.Height - bottomHull.Rect.Y);
										}
									}
									else
									{
										if (!(me3 is Structure))
										{
											Item item = me3 as Item;
											if (item == null || item.GetComponent<Door>() != null)
											{
												continue;
											}
										}
										if (isHorizontal)
										{
											if (!me3.ResizeHorizontal)
											{
												int xPos = (int)((float)leftHull.WorldRect.Right + (me3.WorldPosition.X - (float)hullBounds.X) * scaleFactor);
												me3.Rect = new Rectangle(xPos - me3.RectWidth / 2, me3.Rect.Y, me3.Rect.Width, me3.Rect.Height);
											}
											else
											{
												int minX = (int)((float)leftHull.WorldRect.Right + (float)(me3.WorldRect.X - hullBounds.X) * scaleFactor);
												int maxX = (int)((float)leftHull.WorldRect.Right + (float)(me3.WorldRect.Right - hullBounds.X) * scaleFactor);
												me3.Rect = new Rectangle(minX, me3.Rect.Y, Math.Max(maxX - minX, 16), me3.Rect.Height);
											}
										}
										else if (!me3.ResizeVertical)
										{
											int yPos = (int)((float)(topHull.WorldRect.Y - topHull.RectHeight) + (me3.WorldPosition.Y - (float)hullBounds.Bottom) * scaleFactor);
											me3.Rect = new Rectangle(me3.Rect.X, yPos + me3.RectHeight / 2, me3.Rect.Width, me3.Rect.Height);
										}
										else
										{
											int minY = (int)((float)bottomHull.WorldRect.Y + (float)(me3.WorldRect.Y - me3.RectHeight - hullBounds.Y) * scaleFactor);
											int maxY = (int)((float)bottomHull.WorldRect.Y + (float)(me3.WorldRect.Y - hullBounds.Y) * scaleFactor);
											me3.Rect = new Rectangle(me3.Rect.X, maxY, me3.Rect.Width, Math.Max(maxY - minY, 16));
										}
									}
								}
								if (hallwayLength > 32f)
								{
									WayPoint startWaypoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.ConnectedGap == module.ThisGap);
									if (startWaypoint == null)
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(94, 2);
										defaultInterpolatedStringHandler6.AppendLiteral("Failed to connect waypoints between outpost modules. No waypoint in the ");
										defaultInterpolatedStringHandler6.AppendFormatted(module.ThisGapPosition.ToString().ToLower());
										defaultInterpolatedStringHandler6.AppendLiteral(" gap of the module \"");
										defaultInterpolatedStringHandler6.AppendFormatted(module.Info.Name);
										defaultInterpolatedStringHandler6.AppendLiteral("\".");
										DebugConsole.ThrowError(defaultInterpolatedStringHandler6.ToStringAndClear(), null, null, false, false);
									}
									else
									{
										WayPoint endWaypoint = WayPoint.WayPointList.Find((WayPoint wp) => wp.ConnectedGap == module.PreviousGap);
										if (endWaypoint == null)
										{
											DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(94, 2);
											defaultInterpolatedStringHandler7.AppendLiteral("Failed to connect waypoints between outpost modules. No waypoint in the ");
											defaultInterpolatedStringHandler7.AppendFormatted(OutpostGenerator.GetOpposingGapPosition(module.ThisGapPosition).ToString().ToLower());
											defaultInterpolatedStringHandler7.AppendLiteral(" gap of the module \"");
											defaultInterpolatedStringHandler7.AppendFormatted(module.PreviousModule.Info.Name);
											defaultInterpolatedStringHandler7.AppendLiteral("\".");
											DebugConsole.ThrowError(defaultInterpolatedStringHandler7.ToStringAndClear(), null, null, false, false);
										}
										else
										{
											if (startWaypoint.WorldPosition.X > endWaypoint.WorldPosition.X)
											{
												WayPoint wayPoint2 = startWaypoint;
												startWaypoint = endWaypoint;
												endWaypoint = wayPoint2;
											}
											if (hallwayLength > 100f)
											{
												WayPoint prevWayPoint = startWaypoint;
												WayPoint firstWayPoint = null;
												if (isHorizontal)
												{
													for (float x = (float)leftHull.Rect.Right + 50f; x < (float)rightHull.Rect.X - 50f; x += 100f)
													{
														WayPoint newWayPoint = new WayPoint(new Vector2(x, (float)hullBounds.Y + 110f), SpawnType.Path, sub, null);
														if (firstWayPoint == null)
														{
															firstWayPoint = newWayPoint;
														}
														prevWayPoint.linkedTo.Add(newWayPoint);
														newWayPoint.linkedTo.Add(prevWayPoint);
														prevWayPoint = newWayPoint;
													}
												}
												else if (startWaypoint.Ladders == null)
												{
													float bottom = (float)bottomHull.Rect.Y;
													float top = (float)(topHull.Rect.Y - topHull.Rect.Height);
													for (float y = bottom + 100f; y < top - 100f; y += 100f)
													{
														WayPoint newWayPoint2 = new WayPoint(new Vector2(startWaypoint.Position.X, y), SpawnType.Path, sub, null);
														if (firstWayPoint == null)
														{
															firstWayPoint = newWayPoint2;
														}
														prevWayPoint.linkedTo.Add(newWayPoint2);
														newWayPoint2.linkedTo.Add(prevWayPoint);
														prevWayPoint = newWayPoint2;
													}
												}
												else
												{
													startWaypoint.linkedTo.Add(endWaypoint);
													endWaypoint.linkedTo.Add(startWaypoint);
												}
												if (firstWayPoint != null)
												{
													firstWayPoint.linkedTo.Add(startWaypoint);
													startWaypoint.linkedTo.Add(firstWayPoint);
												}
												if (prevWayPoint != null)
												{
													prevWayPoint.linkedTo.Add(endWaypoint);
													endWaypoint.linkedTo.Add(prevWayPoint);
												}
											}
											else
											{
												startWaypoint.linkedTo.Add(endWaypoint);
												endWaypoint.linkedTo.Add(startWaypoint);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return placedEntities;
		}

		// Token: 0x060040FE RID: 16638 RVA: 0x0024263C File Offset: 0x0024083C
		private static void LinkOxygenGenerators(IEnumerable<MapEntity> entities)
		{
			List<OxygenGenerator> oxygenGenerators = new List<OxygenGenerator>();
			List<Vent> vents = new List<Vent>();
			foreach (MapEntity e in entities)
			{
				Item item = e as Item;
				if (item != null)
				{
					OxygenGenerator oxygenGenerator = item.GetComponent<OxygenGenerator>();
					if (oxygenGenerator != null)
					{
						oxygenGenerators.Add(oxygenGenerator);
					}
					Vent vent = item.GetComponent<Vent>();
					if (vent != null)
					{
						vents.Add(vent);
					}
				}
			}
			foreach (Vent vent2 in vents)
			{
				OxygenGenerator closestOxygenGenerator = null;
				float closestDist = float.MaxValue;
				foreach (OxygenGenerator oxygenGenerator2 in oxygenGenerators)
				{
					float dist = Vector2.DistanceSquared(oxygenGenerator2.Item.WorldPosition, vent2.Item.WorldPosition);
					if (dist < closestDist)
					{
						closestOxygenGenerator = oxygenGenerator2;
						closestDist = dist;
					}
				}
				if (closestOxygenGenerator != null && !closestOxygenGenerator.Item.linkedTo.Contains(vent2.Item))
				{
					closestOxygenGenerator.Item.linkedTo.Add(vent2.Item);
				}
			}
		}

		// Token: 0x060040FF RID: 16639 RVA: 0x002427A4 File Offset: 0x002409A4
		private static void EnableFactionSpecificEntities(Submarine sub, Location location)
		{
			Identifier? identifier;
			if (location == null)
			{
				identifier = null;
			}
			else
			{
				Faction faction = location.Faction;
				identifier = ((faction != null) ? new Identifier?(faction.Prefab.Identifier) : null);
			}
			sub.EnableFactionSpecificEntities(identifier ?? Identifier.Empty);
		}

		// Token: 0x06004100 RID: 16640 RVA: 0x00242804 File Offset: 0x00240A04
		private static void LockUnusedDoors(IEnumerable<OutpostGenerator.PlacedModule> placedModules, Dictionary<OutpostGenerator.PlacedModule, List<MapEntity>> entities, bool removeUnusedGaps)
		{
			using (IEnumerator<OutpostGenerator.PlacedModule> enumerator = placedModules.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					OutpostGenerator.PlacedModule module = enumerator.Current;
					Func<MapEntity, bool> <>9__4;
					Func<MapEntity, bool> <>9__8;
					foreach (MapEntity me in entities[module])
					{
						Gap gap = me as Gap;
						if (gap != null)
						{
							Door door = gap.ConnectedDoor;
							if (door == null || door.UseBetweenOutpostModules)
							{
								if (placedModules.Any((OutpostGenerator.PlacedModule m) => m.PreviousGap == gap || m.ThisGap == gap))
								{
									if (gap.ConnectedDoor == null)
									{
										foreach (MapEntity otherEntity in entities[module])
										{
											Structure structure = otherEntity as Structure;
											if (structure != null && structure.HasBody && !structure.IsPlatform && structure.RemoveIfLinkedOutpostDoorInUse && Submarine.RectContains(structure.WorldRect, gap.WorldPosition, false))
											{
												OutpostGenerator.<LockUnusedDoors>g__RemoveLinkedEntity|27_1(otherEntity);
											}
										}
									}
									Door door2 = door;
									if (door2 != null)
									{
										IEnumerable<MapEntity> linkedTo = door2.Item.linkedTo;
										Func<MapEntity, bool> predicate;
										if ((predicate = <>9__4) == null)
										{
											predicate = (<>9__4 = ((MapEntity lt) => OutpostGenerator.<LockUnusedDoors>g__ShouldRemoveLinkedEntity|27_0(lt, true, module)));
										}
										linkedTo.Where(predicate).ForEachMod(delegate(MapEntity lt)
										{
											OutpostGenerator.<LockUnusedDoors>g__RemoveLinkedEntity|27_1(lt);
										});
									}
								}
								else if ((door == null || !DockingPort.List.Any((DockingPort d) => Submarine.RectContains(d.Item.WorldRect, door.Item.WorldPosition, false))) && (gap.linkedTo.Count != 2 || !entities[module].Contains(gap.linkedTo[0]) || !entities[module].Contains(gap.linkedTo[1])))
								{
									if (door != null)
									{
										if (door.Item.linkedTo.Any((MapEntity lt) => lt is Structure))
										{
											IEnumerable<MapEntity> linkedTo2 = door.Item.linkedTo;
											Func<MapEntity, bool> predicate2;
											if ((predicate2 = <>9__8) == null)
											{
												predicate2 = (<>9__8 = ((MapEntity lt) => OutpostGenerator.<LockUnusedDoors>g__ShouldRemoveLinkedEntity|27_0(lt, false, module)));
											}
											linkedTo2.Where(predicate2).ForEachMod(delegate(MapEntity lt)
											{
												OutpostGenerator.<LockUnusedDoors>g__RemoveLinkedEntity|27_1(lt);
											});
											(from wp in WayPoint.WayPointList
											where wp.ConnectedDoor == door
											select wp).ForEachMod(delegate(WayPoint wp)
											{
												wp.Remove();
											});
											OutpostGenerator.<LockUnusedDoors>g__RemoveLinkedEntity|27_1(door.Item);
										}
										else
										{
											door.Stuck = 100f;
											door.Item.NonInteractable = true;
											ConnectionPanel connectionPanel = door.Item.GetComponent<ConnectionPanel>();
											if (connectionPanel != null)
											{
												connectionPanel.Locked = true;
											}
										}
									}
									else if (removeUnusedGaps)
									{
										gap.Remove();
										(from wp in WayPoint.WayPointList
										where wp.ConnectedGap == gap
										select wp).ForEachMod(delegate(WayPoint wp)
										{
											wp.Remove();
										});
									}
								}
							}
						}
					}
					entities[module].RemoveAll((MapEntity e) => e.Removed);
				}
			}
		}

		// Token: 0x06004101 RID: 16641 RVA: 0x00242C64 File Offset: 0x00240E64
		private static void AlignLadders(IEnumerable<OutpostGenerator.PlacedModule> placedModules, Dictionary<OutpostGenerator.PlacedModule, List<MapEntity>> entities)
		{
			OutpostGenerator.<>c__DisplayClass28_0 CS$<>8__locals1 = new OutpostGenerator.<>c__DisplayClass28_0();
			CS$<>8__locals1.horizontalTolerance = 30f;
			using (IEnumerator<OutpostGenerator.PlacedModule> enumerator = placedModules.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					OutpostGenerator.<>c__DisplayClass28_1 CS$<>8__locals2 = new OutpostGenerator.<>c__DisplayClass28_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.module = enumerator.Current;
					OutpostGenerator.PlacedModule topModule = (CS$<>8__locals2.module.ThisGapPosition == OutpostModuleInfo.GapPosition.Top) ? CS$<>8__locals2.module.PreviousModule : placedModules.FirstOrDefault((OutpostGenerator.PlacedModule m) => m.PreviousModule == CS$<>8__locals2.module && m.ThisGapPosition == OutpostModuleInfo.GapPosition.Bottom);
					if (topModule != null)
					{
						Gap topGap = (CS$<>8__locals2.module.ThisGapPosition == OutpostModuleInfo.GapPosition.Top) ? CS$<>8__locals2.module.ThisGap : topModule.ThisGap;
						Gap bottomGap = (CS$<>8__locals2.module.ThisGapPosition == OutpostModuleInfo.GapPosition.Top) ? CS$<>8__locals2.module.PreviousGap : topModule.PreviousGap;
						using (List<MapEntity>.Enumerator enumerator2 = entities[CS$<>8__locals2.module].GetEnumerator())
						{
							Predicate<WayPoint> <>9__3;
							Predicate<WayPoint> <>9__4;
							while (enumerator2.MoveNext())
							{
								MapEntity me = enumerator2.Current;
								Item item = me as Item;
								Ladder ladder = (item != null) ? item.GetComponent<Ladder>() : null;
								if (ladder != null && ladder.Item.WorldRect.Right >= topGap.WorldRect.X && ladder.Item.WorldPosition.X <= (float)topGap.WorldRect.Right)
								{
									MapEntity topLadder = entities[topModule].Find(delegate(MapEntity e)
									{
										Item item2 = e as Item;
										return ((item2 != null) ? item2.GetComponent<Ladder>() : null) != null && Math.Abs(e.WorldPosition.X - me.WorldPosition.X) < CS$<>8__locals2.CS$<>8__locals1.horizontalTolerance;
									});
									int topLadderDiff = 0;
									int topLadderBottom = (int)((float)topModule.HullBounds.Y + topModule.Offset.Y + topModule.MoveOffset.Y + ladder.Item.Submarine.HiddenSubPosition.Y);
									if (topLadder != null)
									{
										topLadderBottom = topLadder.WorldRect.Y - topLadder.WorldRect.Height;
									}
									Rectangle newLadderRect = new Rectangle(ladder.Item.Rect.X + topLadderDiff, topLadderBottom, ladder.Item.Rect.Width, topLadderBottom - (ladder.Item.WorldRect.Y - ladder.Item.WorldRect.Height));
									Rectangle testOverlapRect = new Rectangle(newLadderRect.X, newLadderRect.Y + 30, newLadderRect.Width, newLadderRect.Height - 60);
									if (testOverlapRect.Height > 0 && !entities[CS$<>8__locals2.module].Any(delegate(MapEntity e)
									{
										Structure structure = e as Structure;
										return structure != null && structure.HasBody && !structure.IsPlatform && Submarine.RectsOverlap(testOverlapRect, structure.Rect, true);
									}))
									{
										ladder.Item.Rect = newLadderRect;
										if (topGap != null && bottomGap != null)
										{
											List<WayPoint> wayPointList = WayPoint.WayPointList;
											Predicate<WayPoint> match;
											if ((match = <>9__3) == null)
											{
												match = (<>9__3 = ((WayPoint wp) => wp.ConnectedGap == bottomGap));
											}
											WayPoint startWaypoint = wayPointList.Find(match);
											List<WayPoint> wayPointList2 = WayPoint.WayPointList;
											Predicate<WayPoint> match2;
											if ((match2 = <>9__4) == null)
											{
												match2 = (<>9__4 = ((WayPoint wp) => wp.ConnectedGap == topGap));
											}
											WayPoint endWaypoint = wayPointList2.Find(match2);
											float margin = 100f;
											if (startWaypoint != null && endWaypoint != null)
											{
												WayPoint prevWaypoint = startWaypoint;
												for (float y = bottomGap.Position.Y + margin; y <= topGap.Position.Y - margin; y += 75f)
												{
													WayPoint wayPoint = new WayPoint(new Vector2(startWaypoint.Position.X, y), SpawnType.Path, ladder.Item.Submarine, null)
													{
														Ladders = ladder
													};
													prevWaypoint.ConnectTo(wayPoint);
													prevWaypoint = wayPoint;
												}
												prevWaypoint.ConnectTo(endWaypoint);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06004102 RID: 16642 RVA: 0x0024311C File Offset: 0x0024131C
		public static void PowerUpOutpost(Submarine sub)
		{
			List<MapEntity> entities = (from me in MapEntity.MapEntityList
			where me.Submarine == sub
			select me).ToList<MapEntity>();
			foreach (MapEntity e in entities)
			{
				Item item = e as Item;
				if (item != null)
				{
					Reactor reactor = item.GetComponent<Reactor>();
					if (reactor != null)
					{
						reactor.PowerOn = true;
						reactor.AutoTemp = true;
					}
				}
			}
			for (int i = 0; i < 600; i++)
			{
				Powered.UpdatePower(0.016666668f);
				foreach (MapEntity e2 in entities)
				{
					Item item2 = e2 as Item;
					if (item2 != null && item2.GetComponent<Powered>() != null)
					{
						item2.Update(0.016666668f, GameMain.GameScreen.Cam);
					}
				}
			}
		}

		// Token: 0x06004103 RID: 16643 RVA: 0x00243238 File Offset: 0x00241438
		public static void SpawnNPCs(Location location, Submarine outpost)
		{
			bool flag;
			if (outpost == null)
			{
				flag = (null != null);
			}
			else
			{
				SubmarineInfo info = outpost.Info;
				flag = (((info != null) ? info.OutpostGenerationParams : null) != null);
			}
			if (!flag)
			{
				return;
			}
			List<HumanPrefab> killedCharacters = new List<HumanPrefab>();
			List<ValueTuple<HumanPrefab, CharacterInfo>> selectedCharacters = new List<ValueTuple<HumanPrefab, CharacterInfo>>();
			List<FactionPrefab> factions = new List<FactionPrefab>();
			if (((location != null) ? location.Faction : null) != null)
			{
				factions.Add(location.Faction.Prefab);
			}
			if (((location != null) ? location.SecondaryFaction : null) != null)
			{
				factions.Add(location.SecondaryFaction.Prefab);
			}
			IReadOnlyList<HumanPrefab> humanPrefabs = outpost.Info.OutpostGenerationParams.GetHumanPrefabs(factions, outpost, Rand.RandSync.ServerAndClient);
			foreach (HumanPrefab humanPrefab in humanPrefabs)
			{
				if (humanPrefab != null)
				{
					CharacterInfo characterInfo = humanPrefab.CreateCharacterInfo(Rand.RandSync.ServerAndClient);
					if (location != null && location.KilledCharacterIdentifiers.Contains(characterInfo.GetIdentifier()))
					{
						killedCharacters.Add(humanPrefab);
					}
					else
					{
						selectedCharacters.Add(new ValueTuple<HumanPrefab, CharacterInfo>(humanPrefab, characterInfo));
					}
				}
			}
			foreach (HumanPrefab killedCharacter in killedCharacters)
			{
				for (int tries = 0; tries < 100; tries++)
				{
					CharacterInfo characterInfo2 = killedCharacter.CreateCharacterInfo(Rand.RandSync.ServerAndClient);
					if (location != null && !location.KilledCharacterIdentifiers.Contains(characterInfo2.GetIdentifier()))
					{
						selectedCharacters.Add(new ValueTuple<HumanPrefab, CharacterInfo>(killedCharacter, characterInfo2));
						break;
					}
				}
			}
			foreach (ValueTuple<HumanPrefab, CharacterInfo> valueTuple in selectedCharacters)
			{
				HumanPrefab humanPrefab2 = valueTuple.Item1;
				CharacterInfo characterInfo3 = valueTuple.Item2;
				Rand.SetSyncedSeed(ToolBox.StringToInt(characterInfo3.Name));
				ISpatialEntity gotoTarget = SpawnAction.GetSpawnPos(SpawnAction.SpawnLocationType.Outpost, new SpawnType?(SpawnType.Human), humanPrefab2.GetModuleFlags(), humanPrefab2.GetSpawnPointTags(), false, false, true);
				if (gotoTarget == null)
				{
					gotoTarget = outpost.GetHulls(true).GetRandom(Rand.RandSync.ServerAndClient);
				}
				characterInfo3.TeamID = CharacterTeamType.FriendlyNPC;
				Character npc = Character.Create(characterInfo3.SpeciesName, SpawnAction.OffsetSpawnPos(gotoTarget.WorldPosition, 100f), ToolBox.RandomSeed(8), characterInfo3, 0, false, true, true, null, true, true);
				npc.AnimController.FindHull(new Vector2?(gotoTarget.WorldPosition), true, false);
				npc.TeamID = CharacterTeamType.FriendlyNPC;
				npc.HumanPrefab = humanPrefab2;
				outpost.Info.AddOutpostNPCIdentifierOrTag(npc, humanPrefab2.Identifier);
				foreach (Identifier tag in humanPrefab2.GetTags())
				{
					outpost.Info.AddOutpostNPCIdentifierOrTag(npc, tag);
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null && !GameMain.NetworkMember.ServerSettings.KillableNPCs)
				{
					npc.CharacterHealth.Unkillable = true;
				}
				humanPrefab2.GiveItems(npc, outpost, gotoTarget as WayPoint, Rand.RandSync.ServerAndClient, true);
				foreach (Item item in npc.Inventory.FindAllItems((Item it) => it != null, true, null))
				{
					item.AllowStealing = outpost.Info.OutpostGenerationParams.AllowStealing;
					item.SpawnedInCurrentOutpost = true;
				}
				humanPrefab2.InitializeCharacter(npc, gotoTarget);
			}
		}

		// Token: 0x06004105 RID: 16645 RVA: 0x0024362C File Offset: 0x0024182C
		[CompilerGenerated]
		internal static void <AppendToModule>g__assertAllPreviousModulesPresent|9_1()
		{
		}

		// Token: 0x06004106 RID: 16646 RVA: 0x0024362E File Offset: 0x0024182E
		[CompilerGenerated]
		internal static bool <AppendToModule>g__DisallowBelowAirlock|9_2(bool allowExtendBelowInitialModule, OutpostModuleInfo.GapPosition gapPosition, OutpostGenerator.PlacedModule currentModule)
		{
			return !allowExtendBelowInitialModule && gapPosition == OutpostModuleInfo.GapPosition.Bottom && currentModule.Offset.Y <= 1f;
		}

		// Token: 0x06004107 RID: 16647 RVA: 0x0024364C File Offset: 0x0024184C
		[CompilerGenerated]
		internal static bool <GenerateHallways>g__IsLinked|24_2(Gap gap, PowerTransfer junctionBox)
		{
			return junctionBox.Item.linkedTo.Contains(gap) || (gap.ConnectedDoor != null && junctionBox.Item.linkedTo.Contains(gap.ConnectedDoor.Item)) || gap.linkedTo.Contains(junctionBox.Item) || (gap.ConnectedDoor != null && gap.ConnectedDoor.Item.linkedTo.Contains(junctionBox.Item));
		}

		// Token: 0x06004108 RID: 16648 RVA: 0x002436D4 File Offset: 0x002418D4
		[CompilerGenerated]
		internal static bool <LockUnusedDoors>g__ShouldRemoveLinkedEntity|27_0(MapEntity e, bool doorInUse, OutpostGenerator.PlacedModule module)
		{
			Item ladderItem = e as Item;
			if (ladderItem != null && ladderItem.IsLadder)
			{
				int linkedToLadderCount = Door.DoorList.Count((Door otherDoor) => otherDoor.Item.linkedTo.Contains(ladderItem));
				return linkedToLadderCount <= 1 && ladderItem.RemoveIfLinkedOutpostDoorInUse == doorInUse;
			}
			Structure structure = e as Structure;
			if (structure != null)
			{
				return structure.RemoveIfLinkedOutpostDoorInUse == doorInUse;
			}
			Item item = e as Item;
			return item != null && item.GetComponent<PowerTransfer>() == null && item.RemoveIfLinkedOutpostDoorInUse == doorInUse;
		}

		// Token: 0x06004109 RID: 16649 RVA: 0x00243768 File Offset: 0x00241968
		[CompilerGenerated]
		internal static void <LockUnusedDoors>g__RemoveLinkedEntity|27_1(MapEntity linked)
		{
			Item linkedItem = linked as Item;
			if (linkedItem != null)
			{
				if (linkedItem.Connections != null)
				{
					foreach (Connection connection in linkedItem.Connections)
					{
						foreach (Wire w in connection.Wires.ToArray<Wire>())
						{
							if (w != null)
							{
								w.Item.Remove();
							}
						}
					}
				}
				Ladder ladder = linkedItem.GetComponent<Ladder>();
				if (ladder != null)
				{
					List<WayPoint> ladderWaypoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.Ladders == ladder);
					foreach (WayPoint ladderWaypoint in ladderWaypoints)
					{
						for (int i = 0; i < ladderWaypoint.linkedTo.Count; i++)
						{
							WayPoint waypoint = ladderWaypoint.linkedTo[i] as WayPoint;
							if (waypoint != null && waypoint.Ladders != ladder)
							{
								for (int j = i + 1; j < ladderWaypoint.linkedTo.Count; j++)
								{
									WayPoint waypoint2 = ladderWaypoint.linkedTo[j] as WayPoint;
									if (waypoint2 != null && waypoint2.Ladders != ladder)
									{
										waypoint.ConnectTo(waypoint2);
									}
								}
							}
						}
					}
					ladderWaypoints.ForEach(delegate(WayPoint wp)
					{
						wp.Remove();
					});
				}
			}
			linked.Remove();
		}

		// Token: 0x040021D5 RID: 8661
		private const int MaxOutpostGenerationRetries = 6;

		// Token: 0x040021D6 RID: 8662
		private static SubmarineInfo usedForceOutpostModule;

		// Token: 0x040021D7 RID: 8663
		private static readonly OutpostModuleInfo.GapPosition[] GapPositions = new OutpostModuleInfo.GapPosition[]
		{
			OutpostModuleInfo.GapPosition.Right,
			OutpostModuleInfo.GapPosition.Left,
			OutpostModuleInfo.GapPosition.Top,
			OutpostModuleInfo.GapPosition.Bottom
		};

		// Token: 0x0200102C RID: 4140
		private class PlacedModule
		{
			// Token: 0x06008B90 RID: 35728 RVA: 0x003AD3FC File Offset: 0x003AB5FC
			public PlacedModule(SubmarineInfo thisModule, OutpostGenerator.PlacedModule previousModule, OutpostModuleInfo.GapPosition thisGapPosition)
			{
				this.Info = thisModule;
				this.PreviousModule = previousModule;
				this.ThisGapPosition = thisGapPosition;
				this.UsedGapPositions = thisGapPosition;
				if (this.PreviousModule != null)
				{
					previousModule.UsedGapPositions |= OutpostGenerator.GetOpposingGapPosition(thisGapPosition);
				}
			}

			// Token: 0x06008B91 RID: 35729 RVA: 0x003AD451 File Offset: 0x003AB651
			public override string ToString()
			{
				return "OutpostGenerator.PlacedModule (" + this.Info.Name + ")";
			}

			// Token: 0x0400578C RID: 22412
			public readonly SubmarineInfo Info;

			// Token: 0x0400578D RID: 22413
			public readonly OutpostGenerator.PlacedModule PreviousModule;

			// Token: 0x0400578E RID: 22414
			public readonly OutpostModuleInfo.GapPosition ThisGapPosition;

			// Token: 0x0400578F RID: 22415
			public OutpostModuleInfo.GapPosition UsedGapPositions;

			// Token: 0x04005790 RID: 22416
			public readonly HashSet<Identifier> FulfilledModuleTypes = new HashSet<Identifier>();

			// Token: 0x04005791 RID: 22417
			public Vector2 Offset;

			// Token: 0x04005792 RID: 22418
			public Vector2 MoveOffset;

			// Token: 0x04005793 RID: 22419
			public Gap ThisGap;

			// Token: 0x04005794 RID: 22420
			public Gap PreviousGap;

			// Token: 0x04005795 RID: 22421
			public Rectangle Bounds;

			// Token: 0x04005796 RID: 22422
			public Rectangle HullBounds;
		}
	}
}

using System;
using System.Runtime.CompilerServices;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma
{
	// Token: 0x02000140 RID: 320
	[NullableContext(1)]
	[Nullable(0)]
	internal static class SteamTimelineManager
	{
		// Token: 0x06002997 RID: 10647 RVA: 0x001CE15B File Offset: 0x001CC35B
		public static void Initialize()
		{
			SteamTimelineManager.SetTimelineGameMode(SteamTimelineManager.TimelineGameMode.LoadingScreen);
		}

		// Token: 0x06002998 RID: 10648 RVA: 0x001CE163 File Offset: 0x001CC363
		public static void Update(float deltaTime)
		{
			SteamTimelineManager.PollScreenChange();
			SteamTimelineManager.PollCharacterChange(deltaTime);
			SteamTimelineManager.PollSubmarineChange(deltaTime);
		}

		// Token: 0x06002999 RID: 10649 RVA: 0x001CE178 File Offset: 0x001CC378
		private static void PollScreenChange()
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			if (Screen.Selected == SteamTimelineManager.prevScreen)
			{
				return;
			}
			Screen selected = Screen.Selected;
			SteamTimelineManager.TimelineGameMode timelineGameMode;
			if (!(selected is GameScreen))
			{
				if (!(selected is NetLobbyScreen))
				{
					if (!(selected is EditorScreen))
					{
						if (!(selected is MainMenuScreen))
						{
							timelineGameMode = SteamTimelineManager.TimelineGameMode.LoadingScreen;
						}
						else
						{
							timelineGameMode = SteamTimelineManager.TimelineGameMode.Menus;
						}
					}
					else
					{
						timelineGameMode = SteamTimelineManager.TimelineGameMode.Playing;
					}
				}
				else
				{
					timelineGameMode = SteamTimelineManager.TimelineGameMode.Staging;
				}
			}
			else
			{
				timelineGameMode = SteamTimelineManager.TimelineGameMode.Playing;
			}
			SteamTimelineManager.TimelineGameMode newMode = timelineGameMode;
			if (GameMain.Instance != null && GameMain.Instance.LoadingScreenOpen)
			{
				newMode = SteamTimelineManager.TimelineGameMode.LoadingScreen;
			}
			if (newMode == SteamTimelineManager.gameMode)
			{
				return;
			}
			SteamTimelineManager.SetTimelineGameMode(newMode);
			SteamTimelineManager.gameMode = newMode;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Timeline game mode set to ");
			defaultInterpolatedStringHandler.AppendFormatted<SteamTimelineManager.TimelineGameMode>(newMode);
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			SteamTimelineManager.prevScreen = Screen.Selected;
		}

		// Token: 0x0600299A RID: 10650 RVA: 0x001CE240 File Offset: 0x001CC440
		private static void PollCharacterChange(float deltaTime)
		{
			Character controlledCharacter = Character.Controlled;
			if (controlledCharacter != SteamTimelineManager.trackedCharacter)
			{
				SteamTimelineManager.InstantlySetCurrentSubmarine(((controlledCharacter != null) ? controlledCharacter.Submarine : null) ?? null);
				SteamTimelineManager.trackedCharacter = controlledCharacter;
			}
		}

		// Token: 0x0600299B RID: 10651 RVA: 0x001CE278 File Offset: 0x001CC478
		private static void PollSubmarineChange(float deltaTime)
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			if (SteamTimelineManager.trackedCharacter == null)
			{
				return;
			}
			if (!(Screen.Selected is GameScreen))
			{
				return;
			}
			Submarine trackedCharacterSubmarine = SteamTimelineManager.trackedCharacter.Submarine;
			if (SteamTimelineManager.submarineStateChangeTimer > 0f)
			{
				SteamTimelineManager.submarineStateChangeTimer -= deltaTime;
				if (SteamTimelineManager.submarineStateChangeTimer <= 0f)
				{
					SteamTimelineManager.CharacterSubChanged(SteamTimelineManager.trackedCharacter, trackedCharacterSubmarine);
				}
			}
			if (SteamTimelineManager.previousTrackedSubmarine != trackedCharacterSubmarine)
			{
				SteamTimelineManager.submarineStateChangeTimer = 2f;
			}
			SteamTimelineManager.previousTrackedSubmarine = trackedCharacterSubmarine;
		}

		// Token: 0x0600299C RID: 10652 RVA: 0x001CE2F4 File Offset: 0x001CC4F4
		[NullableContext(2)]
		private static void InstantlySetCurrentSubmarine(Submarine submarine)
		{
			SteamTimelineManager.currentSubmarine = submarine;
			SteamTimelineManager.previousTrackedSubmarine = submarine;
			SteamTimelineManager.submarineStateChangeTimer = 0f;
		}

		// Token: 0x0600299D RID: 10653 RVA: 0x001CE30C File Offset: 0x001CC50C
		private static void CharacterSubChanged(Character character, Submarine newSubmarine)
		{
			if (newSubmarine == SteamTimelineManager.currentSubmarine)
			{
				return;
			}
			if (SteamTimelineManager.currentSubmarine != null && newSubmarine == null)
			{
				SteamTimelineManager.OnCharacterLeftSubmarine(character, SteamTimelineManager.currentSubmarine);
			}
			else if (SteamTimelineManager.currentSubmarine != null && newSubmarine != null)
			{
				SteamTimelineManager.OnCharacterMovedBetweenSubmarines(character, SteamTimelineManager.currentSubmarine, newSubmarine);
			}
			else if (SteamTimelineManager.currentSubmarine == null && newSubmarine != null)
			{
				SteamTimelineManager.OnCharacterEnteredSubmarine(character, newSubmarine);
			}
			SteamTimelineManager.currentSubmarine = newSubmarine;
		}

		// Token: 0x0600299E RID: 10654 RVA: 0x001CE368 File Offset: 0x001CC568
		public static void SetTimelineGameMode(SteamTimelineManager.TimelineGameMode mode)
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			Steamworks.TimelineGameMode timelineGameMode;
			switch (mode)
			{
			case SteamTimelineManager.TimelineGameMode.Playing:
				timelineGameMode = Steamworks.TimelineGameMode.Playing;
				break;
			case SteamTimelineManager.TimelineGameMode.Staging:
				timelineGameMode = Steamworks.TimelineGameMode.Staging;
				break;
			case SteamTimelineManager.TimelineGameMode.Menus:
				timelineGameMode = Steamworks.TimelineGameMode.Menus;
				break;
			case SteamTimelineManager.TimelineGameMode.LoadingScreen:
				timelineGameMode = Steamworks.TimelineGameMode.LoadingScreen;
				break;
			default:
				throw new ArgumentOutOfRangeException("mode", mode, null);
			}
			Steamworks.TimelineGameMode steamMode = timelineGameMode;
			try
			{
				SteamTimeline.SetTimelineGameMode(steamMode);
			}
			catch (Exception e)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to set timeline game mode to ");
				defaultInterpolatedStringHandler.AppendFormatted<SteamTimelineManager.TimelineGameMode>(mode);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
			}
		}

		// Token: 0x0600299F RID: 10655 RVA: 0x001CE404 File Offset: 0x001CC604
		public static void OnPlayerDied(Character victim, CauseOfDeath causeOfDeath)
		{
			if (victim == null || causeOfDeath == null)
			{
				return;
			}
			string eventTitle = victim.DisplayName + " died";
			string causeOfDeathText = (causeOfDeath.Affliction != null) ? causeOfDeath.Affliction.CauseOfDeathDescription.Value : causeOfDeath.Type.ToString();
			string eventDescription = victim.DisplayName + " died: " + causeOfDeathText;
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_death", 1U, null);
		}

		// Token: 0x060029A0 RID: 10656 RVA: 0x001CE478 File Offset: 0x001CC678
		public static void OnSignificantEnemyDied(Character victim, CauseOfDeath causeOfDeath)
		{
			string eventTitle = victim.DisplayName + " has died!";
			string causeOfDeathText = (causeOfDeath.Affliction != null) ? causeOfDeath.Affliction.CauseOfDeathDescription.Value : causeOfDeath.Type.ToString();
			string eventDescription = victim.DisplayName + " died: " + causeOfDeathText;
			if (causeOfDeath.Killer != null)
			{
				eventDescription = victim.DisplayName + " was killed by " + causeOfDeath.Killer.DisplayName;
			}
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_attack", 2U, null);
		}

		// Token: 0x060029A1 RID: 10657 RVA: 0x001CE50C File Offset: 0x001CC70C
		public static void OnRoundStarted()
		{
			string eventTitle = "Round Started";
			string eventDescription = "The round has started";
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_marker", 0U, null);
		}

		// Token: 0x060029A2 RID: 10658 RVA: 0x001CE534 File Offset: 0x001CC734
		public static void OnRoundEnded()
		{
			string eventTitle = "Round Ended";
			string eventDescription = "The round has ended";
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_completed", 0U, null);
		}

		// Token: 0x060029A3 RID: 10659 RVA: 0x001CE55C File Offset: 0x001CC75C
		public static void OnCharacterLeftSubmarine(Character character, Submarine submarine)
		{
			string eventTitle = character.Name + " Went Diving Outside";
			string eventDescription = character.Name + " left " + submarine.Info.Name;
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_transfer", 1U, null);
		}

		// Token: 0x060029A4 RID: 10660 RVA: 0x001CE5A4 File Offset: 0x001CC7A4
		public static void OnCharacterMovedBetweenSubmarines(Character character, Submarine oldSubmarine, Submarine newSubmarine)
		{
			string eventTitle = character.Name + " Moved Between Locations";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 3);
			defaultInterpolatedStringHandler.AppendFormatted(character.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" moved from ");
			defaultInterpolatedStringHandler.AppendFormatted(oldSubmarine.Info.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" to ");
			defaultInterpolatedStringHandler.AppendFormatted(newSubmarine.Info.Name);
			string eventDescription = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_transfer", 1U, null);
		}

		// Token: 0x060029A5 RID: 10661 RVA: 0x001CE62C File Offset: 0x001CC82C
		public static void OnCharacterEnteredSubmarine(Character character, Submarine submarine)
		{
			string eventTitle = character.Name + " Entered Hull";
			string eventDescription = character.Name + " has entered " + submarine.Info.Name;
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_transfer", 1U, null);
		}

		// Token: 0x060029A6 RID: 10662 RVA: 0x001CE674 File Offset: 0x001CC874
		public static void OnError(string errorMessage, [Nullable(2)] Exception e = null)
		{
			string eventTitle = "Error Occurred";
			string eventDescription = "An error was logged: " + errorMessage;
			if (e != null)
			{
				eventDescription = eventDescription + "\n" + e.GetType().Name;
			}
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_bug", 3U, null);
		}

		// Token: 0x060029A7 RID: 10663 RVA: 0x001CE6BC File Offset: 0x001CC8BC
		public static void OnClientDisconnect(string disconnectInfo)
		{
			string eventTitle = "Client Disconnected";
			string eventDescription = disconnectInfo ?? "";
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_bug", 2U, null);
		}

		// Token: 0x060029A8 RID: 10664 RVA: 0x001CE6E8 File Offset: 0x001CC8E8
		public static void OnMonsterMissionTargetsKilled(MonsterMission mission)
		{
			string eventTitle = "Monsters Dispatched";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
			defaultInterpolatedStringHandler.AppendLiteral(": All targets were eliminated.");
			string eventDescription = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_attack", 2U, null);
		}

		// Token: 0x060029A9 RID: 10665 RVA: 0x001CE734 File Offset: 0x001CC934
		public static void OnScanSuccessful(ScanMission mission)
		{
			string eventTitle = "Scan Successful";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
			defaultInterpolatedStringHandler.AppendLiteral(": A scanner has successfully scanned a target.");
			string eventDescription = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_marker", 1U, null);
		}

		// Token: 0x060029AA RID: 10666 RVA: 0x001CE780 File Offset: 0x001CC980
		public static void OnOutpostTargetEliminated(AbandonedOutpostMission mission)
		{
			string eventTitle = "Target Character Eliminated";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
			defaultInterpolatedStringHandler.AppendLiteral(": A target was eliminated.");
			string eventDescription = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_attack", 2U, null);
		}

		// Token: 0x060029AB RID: 10667 RVA: 0x001CE7CC File Offset: 0x001CC9CC
		public static void OnHullBreached(Structure structure)
		{
			if (SteamTimelineManager.LastHullBreachTime > Timing.TotalTime - 10.0)
			{
				return;
			}
			Submarine submarine = structure.Submarine;
			SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
			if (submarineInfo == null || !submarineInfo.IsPlayer)
			{
				return;
			}
			string eventTitle = "Major Hull Breach";
			string str = "The hull of ";
			Submarine submarine2 = structure.Submarine;
			string eventDescription = str + (((submarine2 != null) ? submarine2.Info.Name : null) ?? "Unknown Submarine") + " suffered a major breach.";
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_caution", 2U, null);
			SteamTimelineManager.LastHullBreachTime = Timing.TotalTime;
		}

		// Token: 0x060029AC RID: 10668 RVA: 0x001CE860 File Offset: 0x001CCA60
		public static void OnMissionTargetRetrieved(Item item, Mission mission)
		{
			string eventTitle = "Target Retrieved: " + item.Name;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
			defaultInterpolatedStringHandler.AppendLiteral(": A target item ");
			defaultInterpolatedStringHandler.AppendFormatted(item.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" was retrieved.");
			string eventDescription = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_checkmark", 1U, null);
		}

		// Token: 0x060029AD RID: 10669 RVA: 0x001CE8D0 File Offset: 0x001CCAD0
		public static void OnMissionTargetPickedUp(Item item, Mission mission)
		{
			string eventTitle = "Target Picked Up: " + item.Name;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(mission.Name);
			defaultInterpolatedStringHandler.AppendLiteral(": A target item ");
			defaultInterpolatedStringHandler.AppendFormatted(item.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" was picked up.");
			string eventDescription = defaultInterpolatedStringHandler.ToStringAndClear();
			SteamTimelineManager.AddTimelineEvent(eventTitle, eventDescription, "steam_checkmark", 1U, null);
		}

		// Token: 0x060029AE RID: 10670 RVA: 0x001CE940 File Offset: 0x001CCB40
		public static void AddTimelineEvent(string title, string description, string icon, uint priority = 1U, [Nullable(2)] Submarine submarine = null)
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(icon))
			{
				DebugConsole.ThrowError("Failed to add timeline event: title, description or icon is empty", null, null, false, false);
				return;
			}
			if (submarine != null)
			{
				SubmarineInfo info = submarine.Info;
				string submarineName = ((info != null) ? info.DisplayName.Value : null) ?? "Unknown Submarine";
				title = title.Replace("[sub]", submarineName);
				description = description.Replace("[sub]", submarineName);
			}
			try
			{
				TimelineEventHandle eventHandle = SteamTimeline.AddInstantaneousTimelineEvent(title, description, icon, priority, 0f, TimelineEventClipPriority.Standard);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to add timeline event", e, null, false, false);
			}
		}

		// Token: 0x040015B4 RID: 5556
		[Nullable(2)]
		private static Screen prevScreen;

		// Token: 0x040015B5 RID: 5557
		private static SteamTimelineManager.TimelineGameMode gameMode = SteamTimelineManager.TimelineGameMode.LoadingScreen;

		// Token: 0x040015B6 RID: 5558
		[Nullable(2)]
		private static Submarine currentSubmarine = null;

		// Token: 0x040015B7 RID: 5559
		[Nullable(2)]
		private static Submarine previousTrackedSubmarine;

		// Token: 0x040015B8 RID: 5560
		[Nullable(2)]
		private static Character trackedCharacter = null;

		// Token: 0x040015B9 RID: 5561
		private const float SubmarineStateChangeDelay = 2f;

		// Token: 0x040015BA RID: 5562
		private static float submarineStateChangeTimer = 0f;

		// Token: 0x040015BB RID: 5563
		private const float HullBreachEventInterval = 10f;

		// Token: 0x040015BC RID: 5564
		private static double LastHullBreachTime;

		// Token: 0x02000DAA RID: 3498
		[NullableContext(0)]
		public enum TimelineGameMode
		{
			// Token: 0x04005033 RID: 20531
			Playing,
			// Token: 0x04005034 RID: 20532
			Staging,
			// Token: 0x04005035 RID: 20533
			Menus,
			// Token: 0x04005036 RID: 20534
			LoadingScreen
		}
	}
}

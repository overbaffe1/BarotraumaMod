using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200001C RID: 28
	internal class MissionAction : EventAction
	{
		// Token: 0x060003EA RID: 1002 RVA: 0x000203D4 File Offset: 0x0001E5D4
		public static void ResetMissionsUnlockedThisRound()
		{
			MissionAction.missionsUnlockedThisRound.Clear();
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x000203E0 File Offset: 0x0001E5E0
		public static void NotifyMissionsUnlockedThisRound(Client client)
		{
			foreach (Mission mission in MissionAction.missionsUnlockedThisRound)
			{
				MissionAction.NotifyMissionUnlock(mission, client);
			}
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00020434 File Offset: 0x0001E634
		private static void NotifyMissionUnlock(Mission mission)
		{
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				MissionAction.NotifyMissionUnlock(mission, client);
			}
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x00020488 File Offset: 0x0001E688
		private static void NotifyMissionUnlock(Mission mission, Client client)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(3);
			outmsg.WriteIdentifier(mission.Prefab.Identifier);
			IWriteMessage writeMessage = outmsg;
			GameSession gameSession = GameMain.GameSession;
			int? num;
			if (gameSession == null)
			{
				num = null;
			}
			else
			{
				Map map = gameSession.Map;
				num = ((map != null) ? new int?(map.Locations.IndexOf(mission.Locations[0])) : null);
			}
			int? num2 = num;
			writeMessage.WriteInt32(num2.GetValueOrDefault(-1));
			IWriteMessage writeMessage2 = outmsg;
			GameSession gameSession2 = GameMain.GameSession;
			int? num3;
			if (gameSession2 == null)
			{
				num3 = null;
			}
			else
			{
				Map map2 = gameSession2.Map;
				num3 = ((map2 != null) ? new int?(map2.Locations.IndexOf(mission.Locations[1])) : null);
			}
			num2 = num3;
			writeMessage2.WriteInt32(num2.GetValueOrDefault(-1));
			outmsg.WriteString(mission.Name.Value);
			GameMain.Server.ServerPeer.Send(outmsg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060003EE RID: 1006 RVA: 0x00020580 File Offset: 0x0001E780
		// (set) Token: 0x060003EF RID: 1007 RVA: 0x00020588 File Offset: 0x0001E788
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the mission to unlock.", "", false)]
		public Identifier MissionIdentifier { get; set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060003F0 RID: 1008 RVA: 0x00020591 File Offset: 0x0001E791
		// (set) Token: 0x060003F1 RID: 1009 RVA: 0x00020599 File Offset: 0x0001E799
		[Serialize("", IsPropertySaveable.Yes, "Tag of the mission to unlock. If there are multiple missions with the tag, one is chosen randomly.", "", false)]
		public Identifier MissionTag { get; set; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060003F2 RID: 1010 RVA: 0x000205A2 File Offset: 0x0001E7A2
		// (set) Token: 0x060003F3 RID: 1011 RVA: 0x000205AA File Offset: 0x0001E7AA
		[Serialize("", IsPropertySaveable.Yes, "The mission can only be unlocked in a location that's occupied by this faction.", "", false)]
		public Identifier RequiredFaction { get; set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060003F4 RID: 1012 RVA: 0x000205B3 File Offset: 0x0001E7B3
		public ImmutableArray<Identifier> LocationTypes { get; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x000205BB File Offset: 0x0001E7BB
		// (set) Token: 0x060003F6 RID: 1014 RVA: 0x000205C3 File Offset: 0x0001E7C3
		[Serialize(0, IsPropertySaveable.Yes, "Minimum distance to the location the mission is unlocked in (1 = one path between locations).", "", false)]
		public int MinLocationDistance { get; set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x060003F7 RID: 1015 RVA: 0x000205CC File Offset: 0x0001E7CC
		// (set) Token: 0x060003F8 RID: 1016 RVA: 0x000205D4 File Offset: 0x0001E7D4
		[Serialize(true, IsPropertySaveable.Yes, "If true, the mission has to be unlocked in a location further on the campaign map.", "", false)]
		public bool UnlockFurtherOnMap { get; set; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x060003F9 RID: 1017 RVA: 0x000205DD File Offset: 0x0001E7DD
		// (set) Token: 0x060003FA RID: 1018 RVA: 0x000205E5 File Offset: 0x0001E7E5
		[Serialize(false, IsPropertySaveable.Yes, "If true, a suitable location is forced on the map if one isn't found.", "", false)]
		public bool CreateLocationIfNotFound { get; set; }

		// Token: 0x060003FB RID: 1019 RVA: 0x000205F0 File Offset: 0x0001E7F0
		public MissionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.MissionIdentifier.IsEmpty && this.MissionTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(79, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": neither MissionIdentifier or MissionTag has been configured.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			if (!this.MissionIdentifier.IsEmpty && !this.MissionTag.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(102, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler2.AppendLiteral("\": both MissionIdentifier or MissionTag have been configured. The tag will be ignored.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
			this.LocationTypes = element.GetAttributeIdentifierArray("locationtype", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.random = new MTRandom(parentEvent.RandomSeed + ToolBox.StringToInt(this.ParentEvent.Prefab.Identifier.Value) + this.ParentEvent.Actions.Count);
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00020735 File Offset: 0x0001E935
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x0002073D File Offset: 0x0001E93D
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00020748 File Offset: 0x0001E948
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			Identifier missionDebugId = this.MissionIdentifier.IsEmpty ? this.MissionTag : this.MissionIdentifier;
			CampaignMode campaign = GameMain.GameSession.GameMode as CampaignMode;
			if (campaign != null)
			{
				Mission unlockedMission = null;
				Location unlockLocation = this.FindUnlockLocation(this.MinLocationDistance, this.UnlockFurtherOnMap, this.LocationTypes, false, true);
				if (unlockLocation == null && this.UnlockFurtherOnMap)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(131, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to find a suitable location to unlock the mission \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(missionDebugId);
					defaultInterpolatedStringHandler.AppendLiteral("\" further on the map. Attempting to find a location earlier on the map...");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					unlockLocation = this.FindUnlockLocation(this.MinLocationDistance, false, this.LocationTypes, false, true);
				}
				if (unlockLocation == null && this.CreateLocationIfNotFound)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(144, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Failed to find a suitable location to unlock the mission \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(missionDebugId);
					defaultInterpolatedStringHandler2.AppendLiteral("\". Attempting to change the type of an empty location to create a suitable location...");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), null, false);
					Location emptyLocation = this.FindUnlockLocation(Math.Max(this.MinLocationDistance, 3), true, "none".ToIdentifier().ToEnumerable<Identifier>(), true, false);
					if (emptyLocation == null)
					{
						DebugConsole.NewMessage("Failed to find a suitable empty location further on the map. Attempting to find a location earlier on the map...", null, false);
						emptyLocation = this.FindUnlockLocation(Math.Max(this.MinLocationDistance, 3), false, "none".ToIdentifier().ToEnumerable<Identifier>(), true, false);
					}
					if (emptyLocation != null)
					{
						emptyLocation.ChangeType(campaign, LocationType.Prefabs[this.LocationTypes[0]], true, true);
						unlockLocation = emptyLocation;
						if (!this.RequiredFaction.IsEmpty)
						{
							emptyLocation.Faction = campaign.Factions.Find(delegate(Faction f)
							{
								Prefab prefab = f.Prefab;
								Identifier requiredFaction = this.RequiredFaction;
								return prefab.Identifier == requiredFaction;
							});
						}
					}
				}
				if (unlockLocation != null)
				{
					if (!this.MissionIdentifier.IsEmpty)
					{
						unlockedMission = unlockLocation.UnlockMissionByIdentifier(this.MissionIdentifier, this.ParentEvent.Prefab.ContentPackage);
					}
					else if (!this.MissionTag.IsEmpty)
					{
						unlockedMission = unlockLocation.UnlockMissionByTag(this.MissionTag, this.random, this.ParentEvent.Prefab.ContentPackage);
					}
					MultiPlayerCampaign mpCampaign = campaign as MultiPlayerCampaign;
					if (mpCampaign != null)
					{
						mpCampaign.IncrementLastUpdateIdForFlag(MultiPlayerCampaign.NetFlags.MapAndMissions);
					}
					if (unlockedMission != null)
					{
						unlockedMission.OriginLocation = campaign.Map.CurrentLocation;
						campaign.Map.Discover(unlockLocation, false);
						if (unlockedMission.Locations[0] == unlockedMission.Locations[1] || unlockedMission.Locations[1] == null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(39, 2);
							defaultInterpolatedStringHandler3.AppendLiteral("Unlocked mission \"");
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(unlockedMission.Name);
							defaultInterpolatedStringHandler3.AppendLiteral("\" in the location \"");
							defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(unlockLocation.DisplayName);
							defaultInterpolatedStringHandler3.AppendLiteral("\".");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler3.ToStringAndClear(), null, false);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(52, 3);
							defaultInterpolatedStringHandler4.AppendLiteral("Unlocked mission \"");
							defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(unlockedMission.Name);
							defaultInterpolatedStringHandler4.AppendLiteral("\" in the connection from \"");
							defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(unlockedMission.Locations[0].DisplayName);
							defaultInterpolatedStringHandler4.AppendLiteral("\" to \"");
							defaultInterpolatedStringHandler4.AppendFormatted<LocalizedString>(unlockedMission.Locations[1].DisplayName);
							defaultInterpolatedStringHandler4.AppendLiteral("\".");
							DebugConsole.NewMessage(defaultInterpolatedStringHandler4.ToStringAndClear(), null, false);
						}
						MissionAction.missionsUnlockedThisRound.Add(unlockedMission);
						MissionAction.NotifyMissionUnlock(unlockedMission);
					}
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(121, 4);
					defaultInterpolatedStringHandler5.AppendLiteral("Failed to find a suitable location to unlock the mission \"");
					defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(missionDebugId);
					defaultInterpolatedStringHandler5.AppendLiteral("\" (LocationType: ");
					defaultInterpolatedStringHandler5.AppendFormatted(string.Join<Identifier>(", ", this.LocationTypes));
					defaultInterpolatedStringHandler5.AppendLiteral(", MinLocationDistance: ");
					defaultInterpolatedStringHandler5.AppendFormatted<int>(this.MinLocationDistance);
					defaultInterpolatedStringHandler5.AppendLiteral(", UnlockFurtherOnMap: ");
					defaultInterpolatedStringHandler5.AppendFormatted<bool>(this.UnlockFurtherOnMap);
					defaultInterpolatedStringHandler5.AppendLiteral(")");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler5.ToStringAndClear(), this.ParentEvent.Prefab.ContentPackage);
				}
			}
			this.isFinished = true;
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00020BA0 File Offset: 0x0001EDA0
		private Location FindUnlockLocation(int minDistance, bool unlockFurtherOnMap, IEnumerable<Identifier> locationTypes, bool mustAllowLocationTypeChanges, bool requireCorrectFaction = true)
		{
			CampaignMode campaign = GameMain.GameSession.GameMode as CampaignMode;
			if (this.LocationTypes.Length == 0 && minDistance <= 1)
			{
				return campaign.Map.CurrentLocation;
			}
			Location currentLocation = campaign.Map.CurrentLocation;
			int distance = 0;
			HashSet<Location> checkedLocations = new HashSet<Location>();
			HashSet<Location> pendingLocations = new HashSet<Location>
			{
				currentLocation
			};
			do
			{
				List<Location> currentLocations = pendingLocations.ToList<Location>();
				pendingLocations.Clear();
				foreach (Location location in currentLocations)
				{
					checkedLocations.Add(location);
					if (this.IsLocationValid(currentLocation, location, unlockFurtherOnMap, distance, minDistance, locationTypes, mustAllowLocationTypeChanges, requireCorrectFaction))
					{
						return location;
					}
					foreach (LocationConnection connection in location.Connections)
					{
						Location otherLocation = connection.OtherLocation(location);
						if (!checkedLocations.Contains(otherLocation))
						{
							pendingLocations.Add(otherLocation);
						}
					}
				}
				distance++;
			}
			while (pendingLocations.Any<Location>());
			return null;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00020CE4 File Offset: 0x0001EEE4
		private bool IsLocationValid(Location currLocation, Location location, bool unlockFurtherOnMap, int distance, int minDistance, IEnumerable<Identifier> locationTypes, bool mustAllowLocationTypeChanges, bool requireCorrectFaction)
		{
			if (mustAllowLocationTypeChanges && location.LocationTypeChangesBlocked)
			{
				return false;
			}
			if (requireCorrectFaction && !this.RequiredFaction.IsEmpty)
			{
				Faction faction = location.Faction;
				Identifier? identifier;
				Identifier? identifier2;
				if (faction == null)
				{
					identifier = null;
					identifier2 = identifier;
				}
				else
				{
					identifier2 = new Identifier?(faction.Prefab.Identifier);
				}
				identifier = identifier2;
				Identifier? identifier3 = new Identifier?(this.RequiredFaction);
				if (identifier != identifier3)
				{
					Faction secondaryFaction = location.SecondaryFaction;
					Identifier? identifier4;
					Identifier? identifier5;
					if (secondaryFaction == null)
					{
						identifier4 = null;
						identifier5 = identifier4;
					}
					else
					{
						identifier5 = new Identifier?(secondaryFaction.Prefab.Identifier);
					}
					identifier4 = identifier5;
					Identifier? identifier6 = new Identifier?(this.RequiredFaction);
					if (identifier4 != identifier6)
					{
						return false;
					}
				}
			}
			return (locationTypes.Contains(location.Type.Identifier) || (location.HasOutpost() && locationTypes.Contains(Tags.AnyOutpost))) && distance >= minDistance && (!unlockFurtherOnMap || location.MapPosition.X >= currLocation.MapPosition.X);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x00020DE4 File Offset: 0x0001EFE4
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("MissionAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.MissionIdentifier.IsEmpty ? this.MissionTag : this.MissionIdentifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x040001CE RID: 462
		private static readonly HashSet<Mission> missionsUnlockedThisRound = new HashSet<Mission>();

		// Token: 0x040001D6 RID: 470
		private bool isFinished;

		// Token: 0x040001D7 RID: 471
		private readonly Random random;
	}
}

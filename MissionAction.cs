using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000296 RID: 662
	internal class MissionAction : EventAction
	{
		// Token: 0x17000F45 RID: 3909
		// (get) Token: 0x06003A0C RID: 14860 RVA: 0x0021D9FA File Offset: 0x0021BBFA
		// (set) Token: 0x06003A0D RID: 14861 RVA: 0x0021DA02 File Offset: 0x0021BC02
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the mission to unlock.", "", false)]
		public Identifier MissionIdentifier { get; set; }

		// Token: 0x17000F46 RID: 3910
		// (get) Token: 0x06003A0E RID: 14862 RVA: 0x0021DA0B File Offset: 0x0021BC0B
		// (set) Token: 0x06003A0F RID: 14863 RVA: 0x0021DA13 File Offset: 0x0021BC13
		[Serialize("", IsPropertySaveable.Yes, "Tag of the mission to unlock. If there are multiple missions with the tag, one is chosen randomly.", "", false)]
		public Identifier MissionTag { get; set; }

		// Token: 0x17000F47 RID: 3911
		// (get) Token: 0x06003A10 RID: 14864 RVA: 0x0021DA1C File Offset: 0x0021BC1C
		// (set) Token: 0x06003A11 RID: 14865 RVA: 0x0021DA24 File Offset: 0x0021BC24
		[Serialize("", IsPropertySaveable.Yes, "The mission can only be unlocked in a location that's occupied by this faction.", "", false)]
		public Identifier RequiredFaction { get; set; }

		// Token: 0x17000F48 RID: 3912
		// (get) Token: 0x06003A12 RID: 14866 RVA: 0x0021DA2D File Offset: 0x0021BC2D
		public ImmutableArray<Identifier> LocationTypes { get; }

		// Token: 0x17000F49 RID: 3913
		// (get) Token: 0x06003A13 RID: 14867 RVA: 0x0021DA35 File Offset: 0x0021BC35
		// (set) Token: 0x06003A14 RID: 14868 RVA: 0x0021DA3D File Offset: 0x0021BC3D
		[Serialize(0, IsPropertySaveable.Yes, "Minimum distance to the location the mission is unlocked in (1 = one path between locations).", "", false)]
		public int MinLocationDistance { get; set; }

		// Token: 0x17000F4A RID: 3914
		// (get) Token: 0x06003A15 RID: 14869 RVA: 0x0021DA46 File Offset: 0x0021BC46
		// (set) Token: 0x06003A16 RID: 14870 RVA: 0x0021DA4E File Offset: 0x0021BC4E
		[Serialize(true, IsPropertySaveable.Yes, "If true, the mission has to be unlocked in a location further on the campaign map.", "", false)]
		public bool UnlockFurtherOnMap { get; set; }

		// Token: 0x17000F4B RID: 3915
		// (get) Token: 0x06003A17 RID: 14871 RVA: 0x0021DA57 File Offset: 0x0021BC57
		// (set) Token: 0x06003A18 RID: 14872 RVA: 0x0021DA5F File Offset: 0x0021BC5F
		[Serialize(false, IsPropertySaveable.Yes, "If true, a suitable location is forced on the map if one isn't found.", "", false)]
		public bool CreateLocationIfNotFound { get; set; }

		// Token: 0x06003A19 RID: 14873 RVA: 0x0021DA68 File Offset: 0x0021BC68
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

		// Token: 0x06003A1A RID: 14874 RVA: 0x0021DBAD File Offset: 0x0021BDAD
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003A1B RID: 14875 RVA: 0x0021DBB5 File Offset: 0x0021BDB5
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003A1C RID: 14876 RVA: 0x0021DBC0 File Offset: 0x0021BDC0
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
						RichString headerText = string.Empty;
						RichString text = TextManager.GetWithVariable("missionunlocked", "[missionname]", unlockedMission.Name, FormatCapitals.No);
						LocalizedString[] buttons = Array.Empty<LocalizedString>();
						Sprite icon = unlockedMission.Prefab.Icon;
						new GUIMessageBox(headerText, text, buttons, new Vector2?(new Vector2(0.3f, 0.15f)), new Point?(new Point(512, 128)), Alignment.TopLeft, GUIMessageBox.Type.InGame, "", icon, "", null, null, false).IconColor = unlockedMission.Prefab.IconColor;
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

		// Token: 0x06003A1D RID: 14877 RVA: 0x0021E08C File Offset: 0x0021C28C
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

		// Token: 0x06003A1E RID: 14878 RVA: 0x0021E1D0 File Offset: 0x0021C3D0
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

		// Token: 0x06003A1F RID: 14879 RVA: 0x0021E2D0 File Offset: 0x0021C4D0
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

		// Token: 0x04001DE5 RID: 7653
		private bool isFinished;

		// Token: 0x04001DE6 RID: 7654
		private readonly Random random;
	}
}

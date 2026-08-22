using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200027F RID: 639
	internal class CheckMissionAction : BinaryOptionAction
	{
		// Token: 0x17000EFC RID: 3836
		// (get) Token: 0x0600390E RID: 14606 RVA: 0x0021A777 File Offset: 0x00218977
		// (set) Token: 0x0600390F RID: 14607 RVA: 0x0021A77F File Offset: 0x0021897F
		[Serialize(CheckMissionAction.MissionType.Current, IsPropertySaveable.Yes, "Does the mission need to be currently active, selected for the next round or available.", "", false)]
		public CheckMissionAction.MissionType Type { get; set; }

		// Token: 0x17000EFD RID: 3837
		// (get) Token: 0x06003910 RID: 14608 RVA: 0x0021A788 File Offset: 0x00218988
		// (set) Token: 0x06003911 RID: 14609 RVA: 0x0021A790 File Offset: 0x00218990
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the mission.", "", false)]
		public Identifier MissionIdentifier { get; set; }

		// Token: 0x17000EFE RID: 3838
		// (get) Token: 0x06003912 RID: 14610 RVA: 0x0021A799 File Offset: 0x00218999
		// (set) Token: 0x06003913 RID: 14611 RVA: 0x0021A7A1 File Offset: 0x002189A1
		[Serialize("", IsPropertySaveable.Yes, "Tag of the mission. Ignored if MissionIdentifier is set.", "", false)]
		public Identifier MissionTag { get; set; }

		// Token: 0x17000EFF RID: 3839
		// (get) Token: 0x06003914 RID: 14612 RVA: 0x0021A7AA File Offset: 0x002189AA
		// (set) Token: 0x06003915 RID: 14613 RVA: 0x0021A7B2 File Offset: 0x002189B2
		[Serialize(1, IsPropertySaveable.Yes, "Minimum number of matching missions for the check to succeed.", "", false)]
		public int MissionCount { get; set; }

		// Token: 0x06003916 RID: 14614 RVA: 0x0021A7BB File Offset: 0x002189BB
		public CheckMissionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.MissionCount = Math.Max(this.MissionCount, 0);
		}

		// Token: 0x06003917 RID: 14615 RVA: 0x0021A7D8 File Offset: 0x002189D8
		protected override bool? DetermineSuccess()
		{
			IEnumerable<Mission> enumerable;
			switch (this.Type)
			{
			case CheckMissionAction.MissionType.Current:
			{
				GameSession gameSession = GameMain.GameSession;
				enumerable = ((gameSession != null) ? gameSession.Missions : null);
				break;
			}
			case CheckMissionAction.MissionType.Selected:
			{
				GameSession gameSession2 = GameMain.GameSession;
				IEnumerable<Mission> enumerable2;
				if (gameSession2 == null)
				{
					enumerable2 = null;
				}
				else
				{
					CampaignMode campaign = gameSession2.Campaign;
					enumerable2 = ((campaign != null) ? campaign.Missions : null);
				}
				enumerable = enumerable2;
				break;
			}
			case CheckMissionAction.MissionType.Available:
			{
				GameSession gameSession3 = GameMain.GameSession;
				IEnumerable<Mission> enumerable3;
				if (gameSession3 == null)
				{
					enumerable3 = null;
				}
				else
				{
					Map map = gameSession3.Map;
					if (map == null)
					{
						enumerable3 = null;
					}
					else
					{
						Location currentLocation = map.CurrentLocation;
						enumerable3 = ((currentLocation != null) ? currentLocation.AvailableMissions : null);
					}
				}
				enumerable = enumerable3;
				break;
			}
			default:
				enumerable = null;
				break;
			}
			IEnumerable<Mission> missions = enumerable;
			if (missions == null)
			{
				return new bool?(this.MissionIdentifier.IsEmpty && this.MissionTag.IsEmpty && this.MissionCount == 0);
			}
			if (!this.MissionIdentifier.IsEmpty)
			{
				return new bool?(missions.Any(delegate(Mission m)
				{
					Prefab prefab = m.Prefab;
					Identifier missionIdentifier = this.MissionIdentifier;
					return prefab.Identifier == missionIdentifier;
				}));
			}
			if (!this.MissionTag.IsEmpty)
			{
				return new bool?(missions.Count((Mission m) => m.Prefab.Tags.Contains(this.MissionTag.Value)) >= this.MissionCount);
			}
			return new bool?(missions.Count<Mission>() >= this.MissionCount);
		}

		// Token: 0x02000F1B RID: 3867
		public enum MissionType
		{
			// Token: 0x040054A5 RID: 21669
			Current,
			// Token: 0x040054A6 RID: 21670
			Selected,
			// Token: 0x040054A7 RID: 21671
			Available
		}
	}
}

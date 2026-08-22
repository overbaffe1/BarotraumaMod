using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x0200018B RID: 395
	internal class CheckMissionAction : BinaryOptionAction
	{
		// Token: 0x170008A8 RID: 2216
		// (get) Token: 0x06001E42 RID: 7746 RVA: 0x000D51C3 File Offset: 0x000D33C3
		// (set) Token: 0x06001E43 RID: 7747 RVA: 0x000D51CB File Offset: 0x000D33CB
		[Serialize(CheckMissionAction.MissionType.Current, IsPropertySaveable.Yes, "Does the mission need to be currently active, selected for the next round or available.", "", false)]
		public CheckMissionAction.MissionType Type { get; set; }

		// Token: 0x170008A9 RID: 2217
		// (get) Token: 0x06001E44 RID: 7748 RVA: 0x000D51D4 File Offset: 0x000D33D4
		// (set) Token: 0x06001E45 RID: 7749 RVA: 0x000D51DC File Offset: 0x000D33DC
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the mission.", "", false)]
		public Identifier MissionIdentifier { get; set; }

		// Token: 0x170008AA RID: 2218
		// (get) Token: 0x06001E46 RID: 7750 RVA: 0x000D51E5 File Offset: 0x000D33E5
		// (set) Token: 0x06001E47 RID: 7751 RVA: 0x000D51ED File Offset: 0x000D33ED
		[Serialize("", IsPropertySaveable.Yes, "Tag of the mission. Ignored if MissionIdentifier is set.", "", false)]
		public Identifier MissionTag { get; set; }

		// Token: 0x170008AB RID: 2219
		// (get) Token: 0x06001E48 RID: 7752 RVA: 0x000D51F6 File Offset: 0x000D33F6
		// (set) Token: 0x06001E49 RID: 7753 RVA: 0x000D51FE File Offset: 0x000D33FE
		[Serialize(1, IsPropertySaveable.Yes, "Minimum number of matching missions for the check to succeed.", "", false)]
		public int MissionCount { get; set; }

		// Token: 0x06001E4A RID: 7754 RVA: 0x000D5207 File Offset: 0x000D3407
		public CheckMissionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.MissionCount = Math.Max(this.MissionCount, 0);
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x000D5224 File Offset: 0x000D3424
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

		// Token: 0x02000909 RID: 2313
		public enum MissionType
		{
			// Token: 0x040031D7 RID: 12759
			Current,
			// Token: 0x040031D8 RID: 12760
			Selected,
			// Token: 0x040031D9 RID: 12761
			Available
		}
	}
}

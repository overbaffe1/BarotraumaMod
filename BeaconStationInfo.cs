using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000328 RID: 808
	internal class BeaconStationInfo : ExtraSubmarineInfo
	{
		// Token: 0x17001127 RID: 4391
		// (get) Token: 0x06004093 RID: 16531 RVA: 0x0023DD54 File Offset: 0x0023BF54
		// (set) Token: 0x06004094 RID: 16532 RVA: 0x0023DD5C File Offset: 0x0023BF5C
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool AllowDamagedWalls { get; set; }

		// Token: 0x17001128 RID: 4392
		// (get) Token: 0x06004095 RID: 16533 RVA: 0x0023DD65 File Offset: 0x0023BF65
		// (set) Token: 0x06004096 RID: 16534 RVA: 0x0023DD6D File Offset: 0x0023BF6D
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool AllowDamagedDevices { get; set; }

		// Token: 0x17001129 RID: 4393
		// (get) Token: 0x06004097 RID: 16535 RVA: 0x0023DD76 File Offset: 0x0023BF76
		// (set) Token: 0x06004098 RID: 16536 RVA: 0x0023DD7E File Offset: 0x0023BF7E
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool AllowDisconnectedWires { get; set; }

		// Token: 0x1700112A RID: 4394
		// (get) Token: 0x06004099 RID: 16537 RVA: 0x0023DD87 File Offset: 0x0023BF87
		// (set) Token: 0x0600409A RID: 16538 RVA: 0x0023DD8F File Offset: 0x0023BF8F
		[Serialize(Level.PlacementType.Bottom, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Level.PlacementType Placement { get; set; }

		// Token: 0x0600409B RID: 16539 RVA: 0x0023DD98 File Offset: 0x0023BF98
		public BeaconStationInfo(SubmarineInfo submarineInfo, XElement element) : base(submarineInfo, element)
		{
			base.Name = "BeaconStationInfo (" + submarineInfo.Name + ")";
		}

		// Token: 0x0600409C RID: 16540 RVA: 0x0023DDBD File Offset: 0x0023BFBD
		public BeaconStationInfo(SubmarineInfo submarineInfo) : base(submarineInfo)
		{
			base.Name = "BeaconStationInfo (" + submarineInfo.Name + ")";
		}

		// Token: 0x0600409D RID: 16541 RVA: 0x0023DDE1 File Offset: 0x0023BFE1
		public BeaconStationInfo(BeaconStationInfo original) : base(original)
		{
		}
	}
}

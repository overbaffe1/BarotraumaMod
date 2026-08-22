using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000251 RID: 593
	internal class BeaconStationInfo : ExtraSubmarineInfo
	{
		// Token: 0x17000CA4 RID: 3236
		// (get) Token: 0x06002A91 RID: 10897 RVA: 0x001159FC File Offset: 0x00113BFC
		// (set) Token: 0x06002A92 RID: 10898 RVA: 0x00115A04 File Offset: 0x00113C04
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool AllowDamagedWalls { get; set; }

		// Token: 0x17000CA5 RID: 3237
		// (get) Token: 0x06002A93 RID: 10899 RVA: 0x00115A0D File Offset: 0x00113C0D
		// (set) Token: 0x06002A94 RID: 10900 RVA: 0x00115A15 File Offset: 0x00113C15
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool AllowDamagedDevices { get; set; }

		// Token: 0x17000CA6 RID: 3238
		// (get) Token: 0x06002A95 RID: 10901 RVA: 0x00115A1E File Offset: 0x00113C1E
		// (set) Token: 0x06002A96 RID: 10902 RVA: 0x00115A26 File Offset: 0x00113C26
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool AllowDisconnectedWires { get; set; }

		// Token: 0x17000CA7 RID: 3239
		// (get) Token: 0x06002A97 RID: 10903 RVA: 0x00115A2F File Offset: 0x00113C2F
		// (set) Token: 0x06002A98 RID: 10904 RVA: 0x00115A37 File Offset: 0x00113C37
		[Serialize(Level.PlacementType.Bottom, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Level.PlacementType Placement { get; set; }

		// Token: 0x06002A99 RID: 10905 RVA: 0x00115A40 File Offset: 0x00113C40
		public BeaconStationInfo(SubmarineInfo submarineInfo, XElement element) : base(submarineInfo, element)
		{
			base.Name = "BeaconStationInfo (" + submarineInfo.Name + ")";
		}

		// Token: 0x06002A9A RID: 10906 RVA: 0x00115A65 File Offset: 0x00113C65
		public BeaconStationInfo(SubmarineInfo submarineInfo) : base(submarineInfo)
		{
			base.Name = "BeaconStationInfo (" + submarineInfo.Name + ")";
		}

		// Token: 0x06002A9B RID: 10907 RVA: 0x00115A89 File Offset: 0x00113C89
		public BeaconStationInfo(BeaconStationInfo original) : base(original)
		{
		}
	}
}

using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000253 RID: 595
	internal class EnemySubmarineInfo : ExtraSubmarineInfo
	{
		// Token: 0x17000CA9 RID: 3241
		// (get) Token: 0x06002AA2 RID: 10914 RVA: 0x00115BAC File Offset: 0x00113DAC
		// (set) Token: 0x06002AA3 RID: 10915 RVA: 0x00115BB4 File Offset: 0x00113DB4
		[Serialize(4000f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float Reward { get; set; }

		// Token: 0x17000CAA RID: 3242
		// (get) Token: 0x06002AA4 RID: 10916 RVA: 0x00115BBD File Offset: 0x00113DBD
		// (set) Token: 0x06002AA5 RID: 10917 RVA: 0x00115BC5 File Offset: 0x00113DC5
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float PreferredDifficulty { get; set; }

		// Token: 0x06002AA6 RID: 10918 RVA: 0x00115BCE File Offset: 0x00113DCE
		public EnemySubmarineInfo(SubmarineInfo submarineInfo, XElement element) : base(submarineInfo, element)
		{
			base.Name = "EnemySubmarineInfo (" + submarineInfo.Name + ")";
			base.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x06002AA7 RID: 10919 RVA: 0x00115C00 File Offset: 0x00113E00
		public EnemySubmarineInfo(SubmarineInfo submarineInfo) : base(submarineInfo)
		{
			base.Name = "EnemySubmarineInfo (" + submarineInfo.Name + ")";
		}

		// Token: 0x06002AA8 RID: 10920 RVA: 0x00115C24 File Offset: 0x00113E24
		public EnemySubmarineInfo(EnemySubmarineInfo original) : base(original)
		{
		}
	}
}

using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200032A RID: 810
	internal class EnemySubmarineInfo : ExtraSubmarineInfo
	{
		// Token: 0x1700112C RID: 4396
		// (get) Token: 0x060040A4 RID: 16548 RVA: 0x0023DF04 File Offset: 0x0023C104
		// (set) Token: 0x060040A5 RID: 16549 RVA: 0x0023DF0C File Offset: 0x0023C10C
		[Serialize(4000f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float Reward { get; set; }

		// Token: 0x1700112D RID: 4397
		// (get) Token: 0x060040A6 RID: 16550 RVA: 0x0023DF15 File Offset: 0x0023C115
		// (set) Token: 0x060040A7 RID: 16551 RVA: 0x0023DF1D File Offset: 0x0023C11D
		[Serialize(50f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float PreferredDifficulty { get; set; }

		// Token: 0x060040A8 RID: 16552 RVA: 0x0023DF26 File Offset: 0x0023C126
		public EnemySubmarineInfo(SubmarineInfo submarineInfo, XElement element) : base(submarineInfo, element)
		{
			base.Name = "EnemySubmarineInfo (" + submarineInfo.Name + ")";
			base.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060040A9 RID: 16553 RVA: 0x0023DF58 File Offset: 0x0023C158
		public EnemySubmarineInfo(SubmarineInfo submarineInfo) : base(submarineInfo)
		{
			base.Name = "EnemySubmarineInfo (" + submarineInfo.Name + ")";
		}

		// Token: 0x060040AA RID: 16554 RVA: 0x0023DF7C File Offset: 0x0023C17C
		public EnemySubmarineInfo(EnemySubmarineInfo original) : base(original)
		{
		}
	}
}

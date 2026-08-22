using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004CD RID: 1229
	internal class MiniMap : Powered
	{
		// Token: 0x170012CD RID: 4813
		// (get) Token: 0x06004617 RID: 17943 RVA: 0x001C0368 File Offset: 0x001BE568
		// (set) Token: 0x06004618 RID: 17944 RVA: 0x001C0370 File Offset: 0x001BE570
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Does the machine require inputs from water detectors in order to show the water levels inside rooms.", "", false)]
		public bool RequireWaterDetectors { get; set; }

		// Token: 0x170012CE RID: 4814
		// (get) Token: 0x06004619 RID: 17945 RVA: 0x001C0379 File Offset: 0x001BE579
		// (set) Token: 0x0600461A RID: 17946 RVA: 0x001C0381 File Offset: 0x001BE581
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Does the machine require inputs from oxygen detectors in order to show the oxygen levels inside rooms.", "", false)]
		public bool RequireOxygenDetectors { get; set; }

		// Token: 0x170012CF RID: 4815
		// (get) Token: 0x0600461B RID: 17947 RVA: 0x001C038A File Offset: 0x001BE58A
		// (set) Token: 0x0600461C RID: 17948 RVA: 0x001C0392 File Offset: 0x001BE592
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should damaged walls be displayed by the machine.", "", false)]
		public bool ShowHullIntegrity { get; set; }

		// Token: 0x170012D0 RID: 4816
		// (get) Token: 0x0600461D RID: 17949 RVA: 0x001C039B File Offset: 0x001BE59B
		// (set) Token: 0x0600461E RID: 17950 RVA: 0x001C03A3 File Offset: 0x001BE5A3
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Enable hull status mode.", "", false)]
		public bool EnableHullStatus { get; set; }

		// Token: 0x170012D1 RID: 4817
		// (get) Token: 0x0600461F RID: 17951 RVA: 0x001C03AC File Offset: 0x001BE5AC
		// (set) Token: 0x06004620 RID: 17952 RVA: 0x001C03B4 File Offset: 0x001BE5B4
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Enable electrical view mode.", "", false)]
		public bool EnableElectricalView { get; set; }

		// Token: 0x170012D2 RID: 4818
		// (get) Token: 0x06004621 RID: 17953 RVA: 0x001C03BD File Offset: 0x001BE5BD
		// (set) Token: 0x06004622 RID: 17954 RVA: 0x001C03C5 File Offset: 0x001BE5C5
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Enable item finder mode.", "", false)]
		public bool EnableItemFinder { get; set; }

		// Token: 0x06004623 RID: 17955 RVA: 0x001C03CE File Offset: 0x001BE5CE
		public MiniMap(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004624 RID: 17956 RVA: 0x001C03E0 File Offset: 0x001BE5E0
		public override void Update(float deltaTime, Camera cam)
		{
			this.hasPower = this.HasPower;
			if (this.hasPower)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			}
		}

		// Token: 0x06004625 RID: 17957 RVA: 0x001C041C File Offset: 0x001BE61C
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			return base.PowerConsumption * MathHelper.Lerp(1.5f, 1f, this.item.Condition / this.item.MaxCondition);
		}

		// Token: 0x06004626 RID: 17958 RVA: 0x001C046D File Offset: 0x001BE66D
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x040021AC RID: 8620
		private bool hasPower;

		// Token: 0x02000E20 RID: 3616
		internal class HullData
		{
			// Token: 0x040041C8 RID: 16840
			public float? HullOxygenAmount;

			// Token: 0x040041C9 RID: 16841
			public float? HullWaterAmount;

			// Token: 0x040041CA RID: 16842
			public float? ReceivedOxygenAmount;

			// Token: 0x040041CB RID: 16843
			public float? ReceivedWaterAmount;

			// Token: 0x040041CC RID: 16844
			public double LastOxygenDataTime;

			// Token: 0x040041CD RID: 16845
			public double LastWaterDataTime;

			// Token: 0x040041CE RID: 16846
			public readonly HashSet<IdCard> Cards = new HashSet<IdCard>();

			// Token: 0x040041CF RID: 16847
			public bool Distort;

			// Token: 0x040041D0 RID: 16848
			public float DistortionTimer;

			// Token: 0x040041D1 RID: 16849
			public List<Hull> LinkedHulls = new List<Hull>();
		}
	}
}

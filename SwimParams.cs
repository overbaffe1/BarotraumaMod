using System;

namespace Barotrauma
{
	// Token: 0x020001D9 RID: 473
	internal abstract class SwimParams : AnimationParams
	{
		// Token: 0x17000D60 RID: 3424
		// (get) Token: 0x060032AD RID: 12973 RVA: 0x00209FC0 File Offset: 0x002081C0
		// (set) Token: 0x060032AE RID: 12974 RVA: 0x00209FC8 File Offset: 0x002081C8
		[Serialize(25f, IsPropertySaveable.Yes, "Turning speed (or rather a force applied on the main collider to make it turn). Note that you can set a limb-specific steering forces too (additional).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float SteerTorque { get; set; }

		// Token: 0x17000D61 RID: 3425
		// (get) Token: 0x060032AF RID: 12975 RVA: 0x00209FD1 File Offset: 0x002081D1
		// (set) Token: 0x060032B0 RID: 12976 RVA: 0x00209FD9 File Offset: 0x002081D9
		[Serialize(25f, IsPropertySaveable.Yes, "How much torque is used to move the legs.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float LegTorque { get; set; }
	}
}

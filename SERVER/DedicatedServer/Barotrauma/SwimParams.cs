using System;

namespace Barotrauma
{
	// Token: 0x020000DD RID: 221
	internal abstract class SwimParams : AnimationParams
	{
		// Token: 0x170006F9 RID: 1785
		// (get) Token: 0x060017B1 RID: 6065 RVA: 0x000C37CC File Offset: 0x000C19CC
		// (set) Token: 0x060017B2 RID: 6066 RVA: 0x000C37D4 File Offset: 0x000C19D4
		[Serialize(25f, IsPropertySaveable.Yes, "Turning speed (or rather a force applied on the main collider to make it turn). Note that you can set a limb-specific steering forces too (additional).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float SteerTorque { get; set; }

		// Token: 0x170006FA RID: 1786
		// (get) Token: 0x060017B3 RID: 6067 RVA: 0x000C37DD File Offset: 0x000C19DD
		// (set) Token: 0x060017B4 RID: 6068 RVA: 0x000C37E5 File Offset: 0x000C19E5
		[Serialize(25f, IsPropertySaveable.Yes, "How much torque is used to move the legs.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float LegTorque { get; set; }
	}
}

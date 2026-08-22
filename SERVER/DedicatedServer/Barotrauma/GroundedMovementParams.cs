using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000DC RID: 220
	internal abstract class GroundedMovementParams : AnimationParams
	{
		// Token: 0x170006EB RID: 1771
		// (get) Token: 0x06001793 RID: 6035 RVA: 0x000C3649 File Offset: 0x000C1849
		// (set) Token: 0x06001794 RID: 6036 RVA: 0x000C3651 File Offset: 0x000C1851
		[Header("Legs", null)]
		[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "How big steps the character takes.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.01f)]
		public Vector2 StepSize { get; set; }

		// Token: 0x170006EC RID: 1772
		// (get) Token: 0x06001795 RID: 6037 RVA: 0x000C365A File Offset: 0x000C185A
		// (set) Token: 0x06001796 RID: 6038 RVA: 0x000C3662 File Offset: 0x000C1862
		[Header("Standing", null)]
		[Serialize(0f, IsPropertySaveable.Yes, "How high above the ground the character's head is positioned.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.1f)]
		public float HeadPosition { get; set; }

		// Token: 0x170006ED RID: 1773
		// (get) Token: 0x06001797 RID: 6039 RVA: 0x000C366B File Offset: 0x000C186B
		// (set) Token: 0x06001798 RID: 6040 RVA: 0x000C3673 File Offset: 0x000C1873
		[Serialize(0f, IsPropertySaveable.Yes, "How high above the ground the character's torso is positioned.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.1f)]
		public float TorsoPosition { get; set; }

		// Token: 0x170006EE RID: 1774
		// (get) Token: 0x06001799 RID: 6041 RVA: 0x000C367C File Offset: 0x000C187C
		// (set) Token: 0x0600179A RID: 6042 RVA: 0x000C3684 File Offset: 0x000C1884
		[Header("Step lift", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "Separate multiplier for the head lift", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2f, ValueStep = 0.1f)]
		public float StepLiftHeadMultiplier { get; set; }

		// Token: 0x170006EF RID: 1775
		// (get) Token: 0x0600179B RID: 6043 RVA: 0x000C368D File Offset: 0x000C188D
		// (set) Token: 0x0600179C RID: 6044 RVA: 0x000C3695 File Offset: 0x000C1895
		[Serialize(0f, IsPropertySaveable.Yes, "How much the body raises when taking a step.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, ValueStep = 0.1f)]
		public float StepLiftAmount { get; set; }

		// Token: 0x170006F0 RID: 1776
		// (get) Token: 0x0600179D RID: 6045 RVA: 0x000C369E File Offset: 0x000C189E
		// (set) Token: 0x0600179E RID: 6046 RVA: 0x000C36A6 File Offset: 0x000C18A6
		[Serialize(0.5f, IsPropertySaveable.Yes, "When does the body raise when taking a step. The default (0.5) is in the middle of the step.", "", false)]
		[Editable(MinValueFloat = -1f, MaxValueFloat = 1f, DecimalCount = 2, ValueStep = 0.1f)]
		public float StepLiftOffset { get; set; }

		// Token: 0x170006F1 RID: 1777
		// (get) Token: 0x0600179F RID: 6047 RVA: 0x000C36AF File Offset: 0x000C18AF
		// (set) Token: 0x060017A0 RID: 6048 RVA: 0x000C36B7 File Offset: 0x000C18B7
		[Serialize(2f, IsPropertySaveable.Yes, "How frequently the body raises when taking a step. The default is 2 (after every step).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, ValueStep = 0.1f)]
		public float StepLiftFrequency { get; set; }

		// Token: 0x170006F2 RID: 1778
		// (get) Token: 0x060017A1 RID: 6049 RVA: 0x000C36C0 File Offset: 0x000C18C0
		// (set) Token: 0x060017A2 RID: 6050 RVA: 0x000C36C8 File Offset: 0x000C18C8
		[Header("Movement", null)]
		[Serialize(0.75f, IsPropertySaveable.Yes, "The character's movement speed is multiplied with this value when moving backwards.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 0.99f, DecimalCount = 2)]
		public float BackwardsMovementMultiplier { get; set; }

		// Token: 0x170006F3 RID: 1779
		// (get) Token: 0x060017A3 RID: 6051 RVA: 0x000C36D1 File Offset: 0x000C18D1
		// (set) Token: 0x060017A4 RID: 6052 RVA: 0x000C36D9 File Offset: 0x000C18D9
		[Serialize(1f, IsPropertySaveable.Yes, "Adjusts the maximum speed while climbing. The actual speed is affected by the MovementSpeed.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float ClimbSpeed { get; set; }

		// Token: 0x170006F4 RID: 1780
		// (get) Token: 0x060017A5 RID: 6053 RVA: 0x000C36E2 File Offset: 0x000C18E2
		// (set) Token: 0x060017A6 RID: 6054 RVA: 0x000C36EA File Offset: 0x000C18EA
		[Serialize(2f, IsPropertySaveable.Yes, "Used instead of ClimbSpeed when descending ladders while moving fast (running). Not used if lower than ClimbSpeed.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float SlideSpeed { get; set; }

		// Token: 0x170006F5 RID: 1781
		// (get) Token: 0x060017A7 RID: 6055 RVA: 0x000C36F3 File Offset: 0x000C18F3
		// (set) Token: 0x060017A8 RID: 6056 RVA: 0x000C36FB File Offset: 0x000C18FB
		[Serialize(10.5f, IsPropertySaveable.Yes, "Force applied to the main collider, torso and head, when climbing ladders.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbBodyMoveForce { get; set; }

		// Token: 0x170006F6 RID: 1782
		// (get) Token: 0x060017A9 RID: 6057 RVA: 0x000C3704 File Offset: 0x000C1904
		// (set) Token: 0x060017AA RID: 6058 RVA: 0x000C370C File Offset: 0x000C190C
		[Serialize(5.2f, IsPropertySaveable.Yes, "Force applied to the hands when climbing ladders.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbHandMoveForce { get; set; }

		// Token: 0x170006F7 RID: 1783
		// (get) Token: 0x060017AB RID: 6059 RVA: 0x000C3715 File Offset: 0x000C1915
		// (set) Token: 0x060017AC RID: 6060 RVA: 0x000C371D File Offset: 0x000C191D
		[Serialize(10f, IsPropertySaveable.Yes, "Force applied to the feet when climbing ladders.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbFootMoveForce { get; set; }

		// Token: 0x170006F8 RID: 1784
		// (get) Token: 0x060017AD RID: 6061 RVA: 0x000C3726 File Offset: 0x000C1926
		// (set) Token: 0x060017AE RID: 6062 RVA: 0x000C372E File Offset: 0x000C192E
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbStepHeight { get; set; }

		// Token: 0x060017AF RID: 6063 RVA: 0x000C3738 File Offset: 0x000C1938
		protected override bool Deserialize(XElement element = null)
		{
			if (element.GetAttributeEnum("AnimationType", AnimationType.NotDefined) == AnimationType.Run)
			{
				if (element.GetAttribute("ClimbSpeed", StringComparison.OrdinalIgnoreCase) == null)
				{
					element.SetAttribute("ClimbSpeed", 2f);
				}
				if (element.GetAttribute("ClimbStepHeight", StringComparison.OrdinalIgnoreCase) == null)
				{
					element.SetAttribute("ClimbStepHeight", 60f);
				}
				if (element.GetAttribute("SlideSpeed", StringComparison.OrdinalIgnoreCase) == null)
				{
					element.SetAttribute("SlideSpeed", 4f);
				}
			}
			return base.Deserialize(element);
		}
	}
}

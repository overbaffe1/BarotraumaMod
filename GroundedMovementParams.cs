using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001D8 RID: 472
	internal abstract class GroundedMovementParams : AnimationParams
	{
		// Token: 0x17000D52 RID: 3410
		// (get) Token: 0x0600328F RID: 12943 RVA: 0x00209E3D File Offset: 0x0020803D
		// (set) Token: 0x06003290 RID: 12944 RVA: 0x00209E45 File Offset: 0x00208045
		[Header("Legs", null)]
		[Serialize("1.0, 1.0", IsPropertySaveable.Yes, "How big steps the character takes.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.01f)]
		public Vector2 StepSize { get; set; }

		// Token: 0x17000D53 RID: 3411
		// (get) Token: 0x06003291 RID: 12945 RVA: 0x00209E4E File Offset: 0x0020804E
		// (set) Token: 0x06003292 RID: 12946 RVA: 0x00209E56 File Offset: 0x00208056
		[Header("Standing", null)]
		[Serialize(0f, IsPropertySaveable.Yes, "How high above the ground the character's head is positioned.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.1f)]
		public float HeadPosition { get; set; }

		// Token: 0x17000D54 RID: 3412
		// (get) Token: 0x06003293 RID: 12947 RVA: 0x00209E5F File Offset: 0x0020805F
		// (set) Token: 0x06003294 RID: 12948 RVA: 0x00209E67 File Offset: 0x00208067
		[Serialize(0f, IsPropertySaveable.Yes, "How high above the ground the character's torso is positioned.", "", false)]
		[Editable(DecimalCount = 2, ValueStep = 0.1f)]
		public float TorsoPosition { get; set; }

		// Token: 0x17000D55 RID: 3413
		// (get) Token: 0x06003295 RID: 12949 RVA: 0x00209E70 File Offset: 0x00208070
		// (set) Token: 0x06003296 RID: 12950 RVA: 0x00209E78 File Offset: 0x00208078
		[Header("Step lift", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "Separate multiplier for the head lift", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2f, ValueStep = 0.1f)]
		public float StepLiftHeadMultiplier { get; set; }

		// Token: 0x17000D56 RID: 3414
		// (get) Token: 0x06003297 RID: 12951 RVA: 0x00209E81 File Offset: 0x00208081
		// (set) Token: 0x06003298 RID: 12952 RVA: 0x00209E89 File Offset: 0x00208089
		[Serialize(0f, IsPropertySaveable.Yes, "How much the body raises when taking a step.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, ValueStep = 0.1f)]
		public float StepLiftAmount { get; set; }

		// Token: 0x17000D57 RID: 3415
		// (get) Token: 0x06003299 RID: 12953 RVA: 0x00209E92 File Offset: 0x00208092
		// (set) Token: 0x0600329A RID: 12954 RVA: 0x00209E9A File Offset: 0x0020809A
		[Serialize(0.5f, IsPropertySaveable.Yes, "When does the body raise when taking a step. The default (0.5) is in the middle of the step.", "", false)]
		[Editable(MinValueFloat = -1f, MaxValueFloat = 1f, DecimalCount = 2, ValueStep = 0.1f)]
		public float StepLiftOffset { get; set; }

		// Token: 0x17000D58 RID: 3416
		// (get) Token: 0x0600329B RID: 12955 RVA: 0x00209EA3 File Offset: 0x002080A3
		// (set) Token: 0x0600329C RID: 12956 RVA: 0x00209EAB File Offset: 0x002080AB
		[Serialize(2f, IsPropertySaveable.Yes, "How frequently the body raises when taking a step. The default is 2 (after every step).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, ValueStep = 0.1f)]
		public float StepLiftFrequency { get; set; }

		// Token: 0x17000D59 RID: 3417
		// (get) Token: 0x0600329D RID: 12957 RVA: 0x00209EB4 File Offset: 0x002080B4
		// (set) Token: 0x0600329E RID: 12958 RVA: 0x00209EBC File Offset: 0x002080BC
		[Header("Movement", null)]
		[Serialize(0.75f, IsPropertySaveable.Yes, "The character's movement speed is multiplied with this value when moving backwards.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 0.99f, DecimalCount = 2)]
		public float BackwardsMovementMultiplier { get; set; }

		// Token: 0x17000D5A RID: 3418
		// (get) Token: 0x0600329F RID: 12959 RVA: 0x00209EC5 File Offset: 0x002080C5
		// (set) Token: 0x060032A0 RID: 12960 RVA: 0x00209ECD File Offset: 0x002080CD
		[Serialize(1f, IsPropertySaveable.Yes, "Adjusts the maximum speed while climbing. The actual speed is affected by the MovementSpeed.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float ClimbSpeed { get; set; }

		// Token: 0x17000D5B RID: 3419
		// (get) Token: 0x060032A1 RID: 12961 RVA: 0x00209ED6 File Offset: 0x002080D6
		// (set) Token: 0x060032A2 RID: 12962 RVA: 0x00209EDE File Offset: 0x002080DE
		[Serialize(2f, IsPropertySaveable.Yes, "Used instead of ClimbSpeed when descending ladders while moving fast (running). Not used if lower than ClimbSpeed.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float SlideSpeed { get; set; }

		// Token: 0x17000D5C RID: 3420
		// (get) Token: 0x060032A3 RID: 12963 RVA: 0x00209EE7 File Offset: 0x002080E7
		// (set) Token: 0x060032A4 RID: 12964 RVA: 0x00209EEF File Offset: 0x002080EF
		[Serialize(10.5f, IsPropertySaveable.Yes, "Force applied to the main collider, torso and head, when climbing ladders.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbBodyMoveForce { get; set; }

		// Token: 0x17000D5D RID: 3421
		// (get) Token: 0x060032A5 RID: 12965 RVA: 0x00209EF8 File Offset: 0x002080F8
		// (set) Token: 0x060032A6 RID: 12966 RVA: 0x00209F00 File Offset: 0x00208100
		[Serialize(5.2f, IsPropertySaveable.Yes, "Force applied to the hands when climbing ladders.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbHandMoveForce { get; set; }

		// Token: 0x17000D5E RID: 3422
		// (get) Token: 0x060032A7 RID: 12967 RVA: 0x00209F09 File Offset: 0x00208109
		// (set) Token: 0x060032A8 RID: 12968 RVA: 0x00209F11 File Offset: 0x00208111
		[Serialize(10f, IsPropertySaveable.Yes, "Force applied to the feet when climbing ladders.", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbFootMoveForce { get; set; }

		// Token: 0x17000D5F RID: 3423
		// (get) Token: 0x060032A9 RID: 12969 RVA: 0x00209F1A File Offset: 0x0020811A
		// (set) Token: 0x060032AA RID: 12970 RVA: 0x00209F22 File Offset: 0x00208122
		[Serialize(30f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0.1f, MaxValueFloat = 100f, DecimalCount = 1)]
		public float ClimbStepHeight { get; set; }

		// Token: 0x060032AB RID: 12971 RVA: 0x00209F2C File Offset: 0x0020812C
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

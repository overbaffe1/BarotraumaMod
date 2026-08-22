using System;

namespace Barotrauma
{
	// Token: 0x020000E8 RID: 232
	internal class HumanCrouchParams : HumanGroundedParams
	{
		// Token: 0x17000731 RID: 1841
		// (get) Token: 0x06001857 RID: 6231 RVA: 0x000C4B20 File Offset: 0x000C2D20
		// (set) Token: 0x06001858 RID: 6232 RVA: 0x000C4B28 File Offset: 0x000C2D28
		[Serialize(0f, IsPropertySaveable.Yes, "How much lower the character's head and torso move when stationary.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2f, DecimalCount = 2)]
		public float MoveDownAmountWhenStationary { get; set; }

		// Token: 0x17000732 RID: 1842
		// (get) Token: 0x06001859 RID: 6233 RVA: 0x000C4B31 File Offset: 0x000C2D31
		// (set) Token: 0x0600185A RID: 6234 RVA: 0x000C4B39 File Offset: 0x000C2D39
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float ExtraHeadAngleWhenStationary { get; set; }

		// Token: 0x17000733 RID: 1843
		// (get) Token: 0x0600185B RID: 6235 RVA: 0x000C4B42 File Offset: 0x000C2D42
		// (set) Token: 0x0600185C RID: 6236 RVA: 0x000C4B4A File Offset: 0x000C2D4A
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float ExtraTorsoAngleWhenStationary { get; set; }

		// Token: 0x0600185D RID: 6237 RVA: 0x000C4B53 File Offset: 0x000C2D53
		public static HumanCrouchParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanCrouchParams>(character, AnimationType.Crouch);
		}

		// Token: 0x0600185E RID: 6238 RVA: 0x000C4B5C File Offset: 0x000C2D5C
		public static HumanCrouchParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanCrouchParams>(character, AnimationType.Crouch, file, throwErrors);
		}

		// Token: 0x0600185F RID: 6239 RVA: 0x000C4B67 File Offset: 0x000C2D67
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanCrouchParams>();
		}
	}
}

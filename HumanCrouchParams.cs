using System;

namespace Barotrauma
{
	// Token: 0x020001E4 RID: 484
	internal class HumanCrouchParams : HumanGroundedParams
	{
		// Token: 0x17000D98 RID: 3480
		// (get) Token: 0x06003353 RID: 13139 RVA: 0x0020B314 File Offset: 0x00209514
		// (set) Token: 0x06003354 RID: 13140 RVA: 0x0020B31C File Offset: 0x0020951C
		[Serialize(0f, IsPropertySaveable.Yes, "How much lower the character's head and torso move when stationary.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2f, DecimalCount = 2)]
		public float MoveDownAmountWhenStationary { get; set; }

		// Token: 0x17000D99 RID: 3481
		// (get) Token: 0x06003355 RID: 13141 RVA: 0x0020B325 File Offset: 0x00209525
		// (set) Token: 0x06003356 RID: 13142 RVA: 0x0020B32D File Offset: 0x0020952D
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float ExtraHeadAngleWhenStationary { get; set; }

		// Token: 0x17000D9A RID: 3482
		// (get) Token: 0x06003357 RID: 13143 RVA: 0x0020B336 File Offset: 0x00209536
		// (set) Token: 0x06003358 RID: 13144 RVA: 0x0020B33E File Offset: 0x0020953E
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(-360f, 360f, 1)]
		public float ExtraTorsoAngleWhenStationary { get; set; }

		// Token: 0x06003359 RID: 13145 RVA: 0x0020B347 File Offset: 0x00209547
		public static HumanCrouchParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanCrouchParams>(character, AnimationType.Crouch);
		}

		// Token: 0x0600335A RID: 13146 RVA: 0x0020B350 File Offset: 0x00209550
		public static HumanCrouchParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanCrouchParams>(character, AnimationType.Crouch, file, throwErrors);
		}

		// Token: 0x0600335B RID: 13147 RVA: 0x0020B35B File Offset: 0x0020955B
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanCrouchParams>();
		}
	}
}

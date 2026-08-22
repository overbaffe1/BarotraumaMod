using System;

namespace Barotrauma
{
	// Token: 0x020001DD RID: 477
	internal class FishSwimFastParams : FishSwimParams
	{
		// Token: 0x060032F5 RID: 13045 RVA: 0x0020AF88 File Offset: 0x00209188
		public static FishSwimFastParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<FishSwimFastParams>(character, AnimationType.SwimFast);
		}

		// Token: 0x060032F6 RID: 13046 RVA: 0x0020AF91 File Offset: 0x00209191
		public static FishSwimFastParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<FishSwimFastParams>(character, AnimationType.SwimFast, file, throwErrors);
		}

		// Token: 0x060032F7 RID: 13047 RVA: 0x0020AF9C File Offset: 0x0020919C
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishSwimFastParams>();
		}
	}
}

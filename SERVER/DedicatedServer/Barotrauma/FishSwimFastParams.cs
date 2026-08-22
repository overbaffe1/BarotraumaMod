using System;

namespace Barotrauma
{
	// Token: 0x020000E1 RID: 225
	internal class FishSwimFastParams : FishSwimParams
	{
		// Token: 0x060017F9 RID: 6137 RVA: 0x000C4794 File Offset: 0x000C2994
		public static FishSwimFastParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<FishSwimFastParams>(character, AnimationType.SwimFast);
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x000C479D File Offset: 0x000C299D
		public static FishSwimFastParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<FishSwimFastParams>(character, AnimationType.SwimFast, file, throwErrors);
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x000C47A8 File Offset: 0x000C29A8
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishSwimFastParams>();
		}
	}
}

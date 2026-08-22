using System;

namespace Barotrauma
{
	// Token: 0x020001DE RID: 478
	internal class FishSwimSlowParams : FishSwimParams
	{
		// Token: 0x060032F9 RID: 13049 RVA: 0x0020AFAC File Offset: 0x002091AC
		public static FishSwimSlowParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<FishSwimSlowParams>(character, AnimationType.SwimSlow);
		}

		// Token: 0x060032FA RID: 13050 RVA: 0x0020AFB5 File Offset: 0x002091B5
		public static FishSwimSlowParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<FishSwimSlowParams>(character, AnimationType.SwimSlow, file, throwErrors);
		}

		// Token: 0x060032FB RID: 13051 RVA: 0x0020AFC0 File Offset: 0x002091C0
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishSwimSlowParams>();
		}
	}
}

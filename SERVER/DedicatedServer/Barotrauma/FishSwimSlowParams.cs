using System;

namespace Barotrauma
{
	// Token: 0x020000E2 RID: 226
	internal class FishSwimSlowParams : FishSwimParams
	{
		// Token: 0x060017FD RID: 6141 RVA: 0x000C47B8 File Offset: 0x000C29B8
		public static FishSwimSlowParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<FishSwimSlowParams>(character, AnimationType.SwimSlow);
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x000C47C1 File Offset: 0x000C29C1
		public static FishSwimSlowParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<FishSwimSlowParams>(character, AnimationType.SwimSlow, file, throwErrors);
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x000C47CC File Offset: 0x000C29CC
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishSwimSlowParams>();
		}
	}
}

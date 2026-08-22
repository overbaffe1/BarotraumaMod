using System;

namespace Barotrauma
{
	// Token: 0x020000E0 RID: 224
	internal class FishRunParams : FishGroundedParams
	{
		// Token: 0x060017F4 RID: 6132 RVA: 0x000C474C File Offset: 0x000C294C
		public static FishRunParams GetDefaultAnimParams(Character character)
		{
			if (!FishGroundedParams.Check(character))
			{
				return FishRunParams.Empty;
			}
			return AnimationParams.GetDefaultAnimParams<FishRunParams>(character, AnimationType.Run);
		}

		// Token: 0x060017F5 RID: 6133 RVA: 0x000C4763 File Offset: 0x000C2963
		public static FishRunParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			if (!FishGroundedParams.Check(character))
			{
				return null;
			}
			return AnimationParams.GetAnimParams<FishRunParams>(character, AnimationType.Run, file, throwErrors);
		}

		// Token: 0x060017F6 RID: 6134 RVA: 0x000C4778 File Offset: 0x000C2978
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishRunParams>();
		}

		// Token: 0x04000BA1 RID: 2977
		protected static readonly FishRunParams Empty = new FishRunParams();
	}
}

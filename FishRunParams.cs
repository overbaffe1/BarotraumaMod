using System;

namespace Barotrauma
{
	// Token: 0x020001DC RID: 476
	internal class FishRunParams : FishGroundedParams
	{
		// Token: 0x060032F0 RID: 13040 RVA: 0x0020AF40 File Offset: 0x00209140
		public static FishRunParams GetDefaultAnimParams(Character character)
		{
			if (!FishGroundedParams.Check(character))
			{
				return FishRunParams.Empty;
			}
			return AnimationParams.GetDefaultAnimParams<FishRunParams>(character, AnimationType.Run);
		}

		// Token: 0x060032F1 RID: 13041 RVA: 0x0020AF57 File Offset: 0x00209157
		public static FishRunParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			if (!FishGroundedParams.Check(character))
			{
				return null;
			}
			return AnimationParams.GetAnimParams<FishRunParams>(character, AnimationType.Run, file, throwErrors);
		}

		// Token: 0x060032F2 RID: 13042 RVA: 0x0020AF6C File Offset: 0x0020916C
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishRunParams>();
		}

		// Token: 0x04001AC6 RID: 6854
		protected static readonly FishRunParams Empty = new FishRunParams();
	}
}

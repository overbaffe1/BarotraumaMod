using System;

namespace Barotrauma
{
	// Token: 0x020001DB RID: 475
	internal class FishWalkParams : FishGroundedParams
	{
		// Token: 0x060032EB RID: 13035 RVA: 0x0020AEF8 File Offset: 0x002090F8
		public static FishWalkParams GetDefaultAnimParams(Character character)
		{
			if (!FishGroundedParams.Check(character))
			{
				return FishWalkParams.Empty;
			}
			return AnimationParams.GetDefaultAnimParams<FishWalkParams>(character, AnimationType.Walk);
		}

		// Token: 0x060032EC RID: 13036 RVA: 0x0020AF0F File Offset: 0x0020910F
		public static FishWalkParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			if (!FishGroundedParams.Check(character))
			{
				return null;
			}
			return AnimationParams.GetAnimParams<FishWalkParams>(character, AnimationType.Walk, file, throwErrors);
		}

		// Token: 0x060032ED RID: 13037 RVA: 0x0020AF24 File Offset: 0x00209124
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishWalkParams>();
		}

		// Token: 0x04001AC5 RID: 6853
		protected static readonly FishWalkParams Empty = new FishWalkParams();
	}
}

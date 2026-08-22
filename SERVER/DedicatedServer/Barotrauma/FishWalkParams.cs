using System;

namespace Barotrauma
{
	// Token: 0x020000DF RID: 223
	internal class FishWalkParams : FishGroundedParams
	{
		// Token: 0x060017EF RID: 6127 RVA: 0x000C4704 File Offset: 0x000C2904
		public static FishWalkParams GetDefaultAnimParams(Character character)
		{
			if (!FishGroundedParams.Check(character))
			{
				return FishWalkParams.Empty;
			}
			return AnimationParams.GetDefaultAnimParams<FishWalkParams>(character, AnimationType.Walk);
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x000C471B File Offset: 0x000C291B
		public static FishWalkParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			if (!FishGroundedParams.Check(character))
			{
				return null;
			}
			return AnimationParams.GetAnimParams<FishWalkParams>(character, AnimationType.Walk, file, throwErrors);
		}

		// Token: 0x060017F1 RID: 6129 RVA: 0x000C4730 File Offset: 0x000C2930
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<FishWalkParams>();
		}

		// Token: 0x04000BA0 RID: 2976
		protected static readonly FishWalkParams Empty = new FishWalkParams();
	}
}

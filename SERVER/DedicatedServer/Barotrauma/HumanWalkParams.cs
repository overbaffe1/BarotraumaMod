using System;

namespace Barotrauma
{
	// Token: 0x020000E6 RID: 230
	internal class HumanWalkParams : HumanGroundedParams
	{
		// Token: 0x0600184F RID: 6223 RVA: 0x000C4AD8 File Offset: 0x000C2CD8
		public static HumanWalkParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanWalkParams>(character, AnimationType.Walk);
		}

		// Token: 0x06001850 RID: 6224 RVA: 0x000C4AE1 File Offset: 0x000C2CE1
		public static HumanWalkParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanWalkParams>(character, AnimationType.Walk, file, throwErrors);
		}

		// Token: 0x06001851 RID: 6225 RVA: 0x000C4AEC File Offset: 0x000C2CEC
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanWalkParams>();
		}
	}
}

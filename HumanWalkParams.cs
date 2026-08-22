using System;

namespace Barotrauma
{
	// Token: 0x020001E2 RID: 482
	internal class HumanWalkParams : HumanGroundedParams
	{
		// Token: 0x0600334B RID: 13131 RVA: 0x0020B2CC File Offset: 0x002094CC
		public static HumanWalkParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanWalkParams>(character, AnimationType.Walk);
		}

		// Token: 0x0600334C RID: 13132 RVA: 0x0020B2D5 File Offset: 0x002094D5
		public static HumanWalkParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanWalkParams>(character, AnimationType.Walk, file, throwErrors);
		}

		// Token: 0x0600334D RID: 13133 RVA: 0x0020B2E0 File Offset: 0x002094E0
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanWalkParams>();
		}
	}
}

using System;

namespace Barotrauma
{
	// Token: 0x020001E3 RID: 483
	internal class HumanRunParams : HumanGroundedParams
	{
		// Token: 0x0600334F RID: 13135 RVA: 0x0020B2F0 File Offset: 0x002094F0
		public static HumanRunParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanRunParams>(character, AnimationType.Run);
		}

		// Token: 0x06003350 RID: 13136 RVA: 0x0020B2F9 File Offset: 0x002094F9
		public static HumanRunParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanRunParams>(character, AnimationType.Run, file, throwErrors);
		}

		// Token: 0x06003351 RID: 13137 RVA: 0x0020B304 File Offset: 0x00209504
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanRunParams>();
		}
	}
}

using System;

namespace Barotrauma
{
	// Token: 0x020001E5 RID: 485
	internal class HumanSwimFastParams : HumanSwimParams
	{
		// Token: 0x0600335D RID: 13149 RVA: 0x0020B36B File Offset: 0x0020956B
		public static HumanSwimFastParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanSwimFastParams>(character, AnimationType.SwimFast);
		}

		// Token: 0x0600335E RID: 13150 RVA: 0x0020B374 File Offset: 0x00209574
		public static HumanSwimFastParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanSwimFastParams>(character, AnimationType.SwimFast, file, throwErrors);
		}

		// Token: 0x0600335F RID: 13151 RVA: 0x0020B37F File Offset: 0x0020957F
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanSwimFastParams>();
		}
	}
}

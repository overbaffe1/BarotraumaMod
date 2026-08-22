using System;

namespace Barotrauma
{
	// Token: 0x020001E6 RID: 486
	internal class HumanSwimSlowParams : HumanSwimParams
	{
		// Token: 0x06003361 RID: 13153 RVA: 0x0020B38F File Offset: 0x0020958F
		public static HumanSwimSlowParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanSwimSlowParams>(character, AnimationType.SwimSlow);
		}

		// Token: 0x06003362 RID: 13154 RVA: 0x0020B398 File Offset: 0x00209598
		public static HumanSwimSlowParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanSwimSlowParams>(character, AnimationType.SwimSlow, file, throwErrors);
		}

		// Token: 0x06003363 RID: 13155 RVA: 0x0020B3A3 File Offset: 0x002095A3
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanSwimSlowParams>();
		}
	}
}

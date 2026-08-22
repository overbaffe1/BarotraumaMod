using System;

namespace Barotrauma
{
	// Token: 0x020000EA RID: 234
	internal class HumanSwimSlowParams : HumanSwimParams
	{
		// Token: 0x06001865 RID: 6245 RVA: 0x000C4B9B File Offset: 0x000C2D9B
		public static HumanSwimSlowParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanSwimSlowParams>(character, AnimationType.SwimSlow);
		}

		// Token: 0x06001866 RID: 6246 RVA: 0x000C4BA4 File Offset: 0x000C2DA4
		public static HumanSwimSlowParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanSwimSlowParams>(character, AnimationType.SwimSlow, file, throwErrors);
		}

		// Token: 0x06001867 RID: 6247 RVA: 0x000C4BAF File Offset: 0x000C2DAF
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanSwimSlowParams>();
		}
	}
}

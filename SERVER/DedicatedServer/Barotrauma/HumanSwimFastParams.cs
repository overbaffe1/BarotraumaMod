using System;

namespace Barotrauma
{
	// Token: 0x020000E9 RID: 233
	internal class HumanSwimFastParams : HumanSwimParams
	{
		// Token: 0x06001861 RID: 6241 RVA: 0x000C4B77 File Offset: 0x000C2D77
		public static HumanSwimFastParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanSwimFastParams>(character, AnimationType.SwimFast);
		}

		// Token: 0x06001862 RID: 6242 RVA: 0x000C4B80 File Offset: 0x000C2D80
		public static HumanSwimFastParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanSwimFastParams>(character, AnimationType.SwimFast, file, throwErrors);
		}

		// Token: 0x06001863 RID: 6243 RVA: 0x000C4B8B File Offset: 0x000C2D8B
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanSwimFastParams>();
		}
	}
}

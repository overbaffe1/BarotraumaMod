using System;

namespace Barotrauma
{
	// Token: 0x020000E7 RID: 231
	internal class HumanRunParams : HumanGroundedParams
	{
		// Token: 0x06001853 RID: 6227 RVA: 0x000C4AFC File Offset: 0x000C2CFC
		public static HumanRunParams GetDefaultAnimParams(Character character)
		{
			return AnimationParams.GetDefaultAnimParams<HumanRunParams>(character, AnimationType.Run);
		}

		// Token: 0x06001854 RID: 6228 RVA: 0x000C4B05 File Offset: 0x000C2D05
		public static HumanRunParams GetAnimParams(Character character, Either<string, ContentPath> file, bool throwErrors = true)
		{
			return AnimationParams.GetAnimParams<HumanRunParams>(character, AnimationType.Run, file, throwErrors);
		}

		// Token: 0x06001855 RID: 6229 RVA: 0x000C4B10 File Offset: 0x000C2D10
		public override void StoreSnapshot()
		{
			base.StoreSnapshot<HumanRunParams>();
		}
	}
}

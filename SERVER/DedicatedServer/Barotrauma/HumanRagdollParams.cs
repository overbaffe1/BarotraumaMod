using System;

namespace Barotrauma
{
	// Token: 0x020000F1 RID: 241
	internal class HumanRagdollParams : RagdollParams
	{
		// Token: 0x0600193E RID: 6462 RVA: 0x000C5FD2 File Offset: 0x000C41D2
		public static HumanRagdollParams GetDefaultRagdollParams(Character character)
		{
			return RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(character);
		}
	}
}

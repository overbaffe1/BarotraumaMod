using System;

namespace Barotrauma
{
	// Token: 0x020000F2 RID: 242
	internal class FishRagdollParams : RagdollParams
	{
		// Token: 0x06001940 RID: 6464 RVA: 0x000C5FE2 File Offset: 0x000C41E2
		public static FishRagdollParams GetDefaultRagdollParams(Character character)
		{
			return RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(character);
		}
	}
}

using System;

namespace Barotrauma
{
	// Token: 0x020001EE RID: 494
	internal class FishRagdollParams : RagdollParams
	{
		// Token: 0x06003440 RID: 13376 RVA: 0x0020C937 File Offset: 0x0020AB37
		public static FishRagdollParams GetDefaultRagdollParams(Character character)
		{
			return RagdollParams.GetDefaultRagdollParams<FishRagdollParams>(character);
		}
	}
}

using System;

namespace Barotrauma
{
	// Token: 0x020001ED RID: 493
	internal class HumanRagdollParams : RagdollParams
	{
		// Token: 0x0600343E RID: 13374 RVA: 0x0020C927 File Offset: 0x0020AB27
		public static HumanRagdollParams GetDefaultRagdollParams(Character character)
		{
			return RagdollParams.GetDefaultRagdollParams<HumanRagdollParams>(character);
		}
	}
}

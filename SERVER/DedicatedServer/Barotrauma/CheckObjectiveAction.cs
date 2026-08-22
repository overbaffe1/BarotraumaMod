using System;

namespace Barotrauma
{
	// Token: 0x0200018D RID: 397
	internal class CheckObjectiveAction : BinaryOptionAction
	{
		// Token: 0x06001E55 RID: 7765 RVA: 0x000D55EB File Offset: 0x000D37EB
		public CheckObjectiveAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x000D55F8 File Offset: 0x000D37F8
		protected override bool? DetermineSuccess()
		{
			bool success = false;
			return new bool?(success);
		}
	}
}

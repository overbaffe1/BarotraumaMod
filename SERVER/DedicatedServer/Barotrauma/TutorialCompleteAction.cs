using System;

namespace Barotrauma
{
	// Token: 0x020001B9 RID: 441
	internal class TutorialCompleteAction : EventAction
	{
		// Token: 0x060020F0 RID: 8432 RVA: 0x000DE4DC File Offset: 0x000DC6DC
		public TutorialCompleteAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060020F1 RID: 8433 RVA: 0x000DE4E6 File Offset: 0x000DC6E6
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			this.isFinished = true;
		}

		// Token: 0x060020F2 RID: 8434 RVA: 0x000DE4F8 File Offset: 0x000DC6F8
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x060020F3 RID: 8435 RVA: 0x000DE500 File Offset: 0x000DC700
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000F8C RID: 3980
		private bool isFinished;
	}
}

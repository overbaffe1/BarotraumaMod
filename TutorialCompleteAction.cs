using System;
using Barotrauma.Tutorials;

namespace Barotrauma
{
	// Token: 0x020002AC RID: 684
	internal class TutorialCompleteAction : EventAction
	{
		// Token: 0x06003BA4 RID: 15268 RVA: 0x00224328 File Offset: 0x00222528
		public TutorialCompleteAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003BA5 RID: 15269 RVA: 0x00224334 File Offset: 0x00222534
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			TutorialMode tutorialMode = ((gameSession != null) ? gameSession.GameMode : null) as TutorialMode;
			if (tutorialMode != null)
			{
				Tutorial tutorial = tutorialMode.Tutorial;
				if (tutorial != null)
				{
					tutorial.Complete();
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003BA6 RID: 15270 RVA: 0x0022437C File Offset: 0x0022257C
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06003BA7 RID: 15271 RVA: 0x00224384 File Offset: 0x00222584
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04001E79 RID: 7801
		private bool isFinished;
	}
}

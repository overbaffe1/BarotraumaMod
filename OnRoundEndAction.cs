using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029E RID: 670
	[NullableContext(1)]
	[Nullable(0)]
	internal class OnRoundEndAction : EventAction
	{
		// Token: 0x06003A90 RID: 14992 RVA: 0x0021FC2F File Offset: 0x0021DE2F
		public OnRoundEndAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.subActions = new EventAction.SubactionGroup(parentEvent, element);
		}

		// Token: 0x06003A91 RID: 14993 RVA: 0x0021FC46 File Offset: 0x0021DE46
		public override bool IsFinished(ref string goToLabel)
		{
			return false;
		}

		// Token: 0x06003A92 RID: 14994 RVA: 0x0021FC4C File Offset: 0x0021DE4C
		public override void Update(float deltaTime)
		{
			int remainingTries = 100;
			string throwaway = null;
			while (remainingTries > 0 && !this.subActions.IsFinished(ref throwaway))
			{
				this.subActions.Update(deltaTime);
				EntitySpawner spawner = Entity.Spawner;
				if (spawner != null)
				{
					spawner.Update(false);
				}
				remainingTries--;
			}
		}

		// Token: 0x06003A93 RID: 14995 RVA: 0x0021FC94 File Offset: 0x0021DE94
		public override void Reset()
		{
		}

		// Token: 0x06003A94 RID: 14996 RVA: 0x0021FC96 File Offset: 0x0021DE96
		public override string ToDebugString()
		{
			return "OnRoundEndAction";
		}

		// Token: 0x04001E16 RID: 7702
		private readonly EventAction.SubactionGroup subActions;
	}
}

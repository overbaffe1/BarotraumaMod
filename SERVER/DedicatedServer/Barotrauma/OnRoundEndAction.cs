using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001AC RID: 428
	[NullableContext(1)]
	[Nullable(0)]
	internal class OnRoundEndAction : EventAction
	{
		// Token: 0x06001FE4 RID: 8164 RVA: 0x000DA113 File Offset: 0x000D8313
		public OnRoundEndAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.subActions = new EventAction.SubactionGroup(parentEvent, element);
		}

		// Token: 0x06001FE5 RID: 8165 RVA: 0x000DA12A File Offset: 0x000D832A
		public override bool IsFinished(ref string goToLabel)
		{
			return false;
		}

		// Token: 0x06001FE6 RID: 8166 RVA: 0x000DA130 File Offset: 0x000D8330
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

		// Token: 0x06001FE7 RID: 8167 RVA: 0x000DA178 File Offset: 0x000D8378
		public override void Reset()
		{
		}

		// Token: 0x06001FE8 RID: 8168 RVA: 0x000DA17A File Offset: 0x000D837A
		public override string ToDebugString()
		{
			return "OnRoundEndAction";
		}

		// Token: 0x04000F2D RID: 3885
		private readonly EventAction.SubactionGroup subActions;
	}
}

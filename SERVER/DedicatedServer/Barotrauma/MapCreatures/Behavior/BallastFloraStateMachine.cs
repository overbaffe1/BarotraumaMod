using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D4 RID: 980
	[NullableContext(1)]
	[Nullable(0)]
	internal class BallastFloraStateMachine
	{
		// Token: 0x060038AA RID: 14506 RVA: 0x0017A9C7 File Offset: 0x00178BC7
		public BallastFloraStateMachine(BallastFloraBehavior parent)
		{
			this.parent = parent;
		}

		// Token: 0x060038AB RID: 14507 RVA: 0x0017A9D6 File Offset: 0x00178BD6
		public void EnterState(IBallastFloraState newState)
		{
			this.lastState = this.State;
			IBallastFloraState state = this.State;
			if (state != null)
			{
				state.Exit();
			}
			this.State = null;
			newState.Enter();
			this.State = newState;
		}

		// Token: 0x060038AC RID: 14508 RVA: 0x0017AA0C File Offset: 0x00178C0C
		public void Update(float deltaTime)
		{
			if (this.State == null)
			{
				this.EnterState(new GrowIdleState(this.parent));
				return;
			}
			this.State.Update(deltaTime);
			ExitState state = this.State.GetState();
			if (state != ExitState.Running)
			{
				if (state == ExitState.ReturnLast && this.lastState != null && this.lastState.GetState() == ExitState.Running)
				{
					this.EnterState(this.lastState);
					return;
				}
				this.EnterState(new GrowIdleState(this.parent));
			}
		}

		// Token: 0x04001C8B RID: 7307
		private readonly BallastFloraBehavior parent;

		// Token: 0x04001C8C RID: 7308
		[Nullable(2)]
		private IBallastFloraState lastState;

		// Token: 0x04001C8D RID: 7309
		[Nullable(2)]
		public IBallastFloraState State;
	}
}

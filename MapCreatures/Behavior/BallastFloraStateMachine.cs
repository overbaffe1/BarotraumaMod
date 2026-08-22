using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020004DC RID: 1244
	[NullableContext(1)]
	[Nullable(0)]
	internal class BallastFloraStateMachine
	{
		// Token: 0x06005156 RID: 20822 RVA: 0x002BC64F File Offset: 0x002BA84F
		public BallastFloraStateMachine(BallastFloraBehavior parent)
		{
			this.parent = parent;
		}

		// Token: 0x06005157 RID: 20823 RVA: 0x002BC65E File Offset: 0x002BA85E
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

		// Token: 0x06005158 RID: 20824 RVA: 0x002BC694 File Offset: 0x002BA894
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

		// Token: 0x04002B24 RID: 11044
		private readonly BallastFloraBehavior parent;

		// Token: 0x04002B25 RID: 11045
		[Nullable(2)]
		private IBallastFloraState lastState;

		// Token: 0x04002B26 RID: 11046
		[Nullable(2)]
		public IBallastFloraState State;
	}
}

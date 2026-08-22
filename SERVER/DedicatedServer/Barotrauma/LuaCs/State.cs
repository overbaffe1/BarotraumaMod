using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003DE RID: 990
	public class State<T> where T : Enum
	{
		// Token: 0x06003914 RID: 14612 RVA: 0x0017D008 File Offset: 0x0017B208
		public State(T stateId, Action<State<T>> onEnterState, Action<State<T>> onExitState)
		{
			this.StateId = stateId;
			this._onEnter = onEnterState;
			this._onExit = onExitState;
		}

		// Token: 0x06003915 RID: 14613 RVA: 0x0017D025 File Offset: 0x0017B225
		public virtual void OnEnter()
		{
			Action<State<T>> onEnter = this._onEnter;
			if (onEnter == null)
			{
				return;
			}
			onEnter(this);
		}

		// Token: 0x06003916 RID: 14614 RVA: 0x0017D038 File Offset: 0x0017B238
		public virtual void OnExit()
		{
			Action<State<T>> onExit = this._onExit;
			if (onExit == null)
			{
				return;
			}
			onExit(this);
		}

		// Token: 0x04001CB7 RID: 7351
		public T StateId;

		// Token: 0x04001CB8 RID: 7352
		private Action<State<T>> _onEnter;

		// Token: 0x04001CB9 RID: 7353
		private Action<State<T>> _onExit;
	}
}

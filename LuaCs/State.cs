using System;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F5 RID: 1269
	public class State<T> where T : Enum
	{
		// Token: 0x06005269 RID: 21097 RVA: 0x002C3348 File Offset: 0x002C1548
		public State(T stateId, Action<State<T>> onEnterState, Action<State<T>> onExitState)
		{
			this.StateId = stateId;
			this._onEnter = onEnterState;
			this._onExit = onExitState;
		}

		// Token: 0x0600526A RID: 21098 RVA: 0x002C3365 File Offset: 0x002C1565
		public virtual void OnEnter()
		{
			Action<State<T>> onEnter = this._onEnter;
			if (onEnter == null)
			{
				return;
			}
			onEnter(this);
		}

		// Token: 0x0600526B RID: 21099 RVA: 0x002C3378 File Offset: 0x002C1578
		public virtual void OnExit()
		{
			Action<State<T>> onExit = this._onExit;
			if (onExit == null)
			{
				return;
			}
			onExit(this);
		}

		// Token: 0x04002BB2 RID: 11186
		public T StateId;

		// Token: 0x04002BB3 RID: 11187
		private Action<State<T>> _onEnter;

		// Token: 0x04002BB4 RID: 11188
		private Action<State<T>> _onExit;
	}
}

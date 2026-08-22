using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003DD RID: 989
	public class StateMachine<T> where T : Enum
	{
		// Token: 0x17000F93 RID: 3987
		// (get) Token: 0x0600390E RID: 14606 RVA: 0x0017CC6E File Offset: 0x0017AE6E
		public T CurrentState
		{
			get
			{
				return this._currentState.StateId;
			}
		}

		// Token: 0x0600390F RID: 14607 RVA: 0x0017CC7C File Offset: 0x0017AE7C
		public StateMachine(bool errorOnSameState, T defaultState, Action<State<T>> onEnter, Action<State<T>> onExit)
		{
			this._errorOnSameStateSelected = errorOnSameState;
			this._states = new ConcurrentDictionary<T, State<T>>();
			State<T> defState = new State<T>(defaultState, onEnter, onExit);
			this._currentState = defState;
			this._states[defaultState] = defState;
		}

		// Token: 0x06003910 RID: 14608 RVA: 0x0017CCCC File Offset: 0x0017AECC
		public StateMachine<T> AddState(T stateId, Action<State<T>> onEnter, Action<State<T>> onExit)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				State<T> state;
				if (this._states.TryGetValue(stateId, out state))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
					defaultInterpolatedStringHandler.AppendLiteral("State with id ");
					defaultInterpolatedStringHandler.AppendFormatted<T>(stateId);
					defaultInterpolatedStringHandler.AppendLiteral(" already exists.");
					ThrowHelper.ThrowArgumentException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this._states[stateId] = new State<T>(stateId, onEnter, onExit);
			}
			return this;
		}

		// Token: 0x06003911 RID: 14609 RVA: 0x0017CD80 File Offset: 0x0017AF80
		public StateMachine<T> RemoveState(T stateId)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (EqualityComparer<T>.Default.Equals(stateId, this.CurrentState))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
					defaultInterpolatedStringHandler.AppendLiteral("State with id ");
					defaultInterpolatedStringHandler.AppendFormatted<T>(this.CurrentState);
					defaultInterpolatedStringHandler.AppendLiteral(" is active. Cannot remove.");
					ThrowHelper.ThrowInvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				State<T> state;
				this._states.TryRemove(stateId, out state);
			}
			return this;
		}

		// Token: 0x06003912 RID: 14610 RVA: 0x0017CE38 File Offset: 0x0017B038
		public StateMachine<T> AddOrReplaceState(T oldStateId, T newStateId, Action<State<T>> onEnter, Action<State<T>> onExit)
		{
			using (this._operationsLock.AcquireReaderLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (EqualityComparer<T>.Default.Equals(oldStateId, this.CurrentState))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
					defaultInterpolatedStringHandler.AppendLiteral("State with id ");
					defaultInterpolatedStringHandler.AppendFormatted<T>(this.CurrentState);
					defaultInterpolatedStringHandler.AppendLiteral(" is active. Cannot replace.");
					ThrowHelper.ThrowInvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				this._states[oldStateId] = new State<T>(newStateId, onEnter, onExit);
			}
			return this;
		}

		// Token: 0x06003913 RID: 14611 RVA: 0x0017CEF4 File Offset: 0x0017B0F4
		public StateMachine<T> GotoState(T stateId)
		{
			StateMachine<T> result;
			using (this._operationsLock.AcquireWriterLock(default(CancellationToken)).ConfigureAwait(false).GetAwaiter().GetResult())
			{
				if (EqualityComparer<T>.Default.Equals(stateId, this.CurrentState))
				{
					if (this._errorOnSameStateSelected)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
						defaultInterpolatedStringHandler.AppendLiteral("State with id ");
						defaultInterpolatedStringHandler.AppendFormatted<T>(stateId);
						defaultInterpolatedStringHandler.AppendLiteral(" is already selected.");
						ThrowHelper.ThrowInvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					result = this;
				}
				else
				{
					State<T> newState;
					if (!this._states.TryGetValue(stateId, out newState))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(37, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Target state with id ");
						defaultInterpolatedStringHandler2.AppendFormatted<T>(stateId);
						defaultInterpolatedStringHandler2.AppendLiteral(" does not exist.");
						ThrowHelper.ThrowArgumentNullException(defaultInterpolatedStringHandler2.ToStringAndClear());
					}
					this._currentState.OnExit();
					this._currentState = newState;
					this._currentState.OnEnter();
					result = this;
				}
			}
			return result;
		}

		// Token: 0x04001CB3 RID: 7347
		private readonly ConcurrentDictionary<T, State<T>> _states;

		// Token: 0x04001CB4 RID: 7348
		private State<T> _currentState;

		// Token: 0x04001CB5 RID: 7349
		private bool _errorOnSameStateSelected;

		// Token: 0x04001CB6 RID: 7350
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();
	}
}

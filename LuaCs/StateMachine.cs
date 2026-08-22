using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F4 RID: 1268
	public class StateMachine<T> where T : Enum
	{
		// Token: 0x170014DD RID: 5341
		// (get) Token: 0x06005263 RID: 21091 RVA: 0x002C2FAE File Offset: 0x002C11AE
		public T CurrentState
		{
			get
			{
				return this._currentState.StateId;
			}
		}

		// Token: 0x06005264 RID: 21092 RVA: 0x002C2FBC File Offset: 0x002C11BC
		public StateMachine(bool errorOnSameState, T defaultState, Action<State<T>> onEnter, Action<State<T>> onExit)
		{
			this._errorOnSameStateSelected = errorOnSameState;
			this._states = new ConcurrentDictionary<T, State<T>>();
			State<T> defState = new State<T>(defaultState, onEnter, onExit);
			this._currentState = defState;
			this._states[defaultState] = defState;
		}

		// Token: 0x06005265 RID: 21093 RVA: 0x002C300C File Offset: 0x002C120C
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

		// Token: 0x06005266 RID: 21094 RVA: 0x002C30C0 File Offset: 0x002C12C0
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

		// Token: 0x06005267 RID: 21095 RVA: 0x002C3178 File Offset: 0x002C1378
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

		// Token: 0x06005268 RID: 21096 RVA: 0x002C3234 File Offset: 0x002C1434
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

		// Token: 0x04002BAE RID: 11182
		private readonly ConcurrentDictionary<T, State<T>> _states;

		// Token: 0x04002BAF RID: 11183
		private State<T> _currentState;

		// Token: 0x04002BB0 RID: 11184
		private bool _errorOnSameStateSelected;

		// Token: 0x04002BB1 RID: 11185
		private readonly AsyncReaderWriterLock _operationsLock = new AsyncReaderWriterLock();
	}
}

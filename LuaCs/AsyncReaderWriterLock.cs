using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004F2 RID: 1266
	public sealed class AsyncReaderWriterLock : IDisposable
	{
		// Token: 0x0600524F RID: 21071 RVA: 0x002C24D0 File Offset: 0x002C06D0
		public Task<IDisposable> AcquireWriterLock(CancellationToken token = default(CancellationToken))
		{
			AsyncReaderWriterLock.<AcquireWriterLock>d__3 <AcquireWriterLock>d__;
			<AcquireWriterLock>d__.<>t__builder = AsyncTaskMethodBuilder<IDisposable>.Create();
			<AcquireWriterLock>d__.<>4__this = this;
			<AcquireWriterLock>d__.token = token;
			<AcquireWriterLock>d__.<>1__state = -1;
			<AcquireWriterLock>d__.<>t__builder.Start<AsyncReaderWriterLock.<AcquireWriterLock>d__3>(ref <AcquireWriterLock>d__);
			return <AcquireWriterLock>d__.<>t__builder.Task;
		}

		// Token: 0x06005250 RID: 21072 RVA: 0x002C251B File Offset: 0x002C071B
		private void ReleaseWriterLock()
		{
			this._readSemaphore.Release();
			this._writeSemaphore.Release();
		}

		// Token: 0x06005251 RID: 21073 RVA: 0x002C2538 File Offset: 0x002C0738
		public Task<IDisposable> AcquireReaderLock(CancellationToken token = default(CancellationToken))
		{
			AsyncReaderWriterLock.<AcquireReaderLock>d__5 <AcquireReaderLock>d__;
			<AcquireReaderLock>d__.<>t__builder = AsyncTaskMethodBuilder<IDisposable>.Create();
			<AcquireReaderLock>d__.<>4__this = this;
			<AcquireReaderLock>d__.token = token;
			<AcquireReaderLock>d__.<>1__state = -1;
			<AcquireReaderLock>d__.<>t__builder.Start<AsyncReaderWriterLock.<AcquireReaderLock>d__5>(ref <AcquireReaderLock>d__);
			return <AcquireReaderLock>d__.<>t__builder.Task;
		}

		// Token: 0x06005252 RID: 21074 RVA: 0x002C2583 File Offset: 0x002C0783
		private void ReleaseReaderLock()
		{
			if (Interlocked.Decrement(ref this._readerCount) == 0)
			{
				this._readSemaphore.Release();
			}
		}

		// Token: 0x06005253 RID: 21075 RVA: 0x002C259E File Offset: 0x002C079E
		public void Dispose()
		{
			this._writeSemaphore.Dispose();
			this._readSemaphore.Dispose();
		}

		// Token: 0x04002BAB RID: 11179
		private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(1, 1);

		// Token: 0x04002BAC RID: 11180
		private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);

		// Token: 0x04002BAD RID: 11181
		private int _readerCount;

		// Token: 0x020012B5 RID: 4789
		private sealed class LockToken : IDisposable
		{
			// Token: 0x0600950B RID: 38155 RVA: 0x003D23BD File Offset: 0x003D05BD
			public LockToken(Action action)
			{
				this._action = action;
			}

			// Token: 0x0600950C RID: 38156 RVA: 0x003D23CC File Offset: 0x003D05CC
			public void Dispose()
			{
				Action action = this._action;
				if (action == null)
				{
					return;
				}
				action();
			}

			// Token: 0x0400600E RID: 24590
			private readonly Action _action;
		}
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003DB RID: 987
	public sealed class AsyncReaderWriterLock : IDisposable
	{
		// Token: 0x060038FA RID: 14586 RVA: 0x0017C190 File Offset: 0x0017A390
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

		// Token: 0x060038FB RID: 14587 RVA: 0x0017C1DB File Offset: 0x0017A3DB
		private void ReleaseWriterLock()
		{
			this._readSemaphore.Release();
			this._writeSemaphore.Release();
		}

		// Token: 0x060038FC RID: 14588 RVA: 0x0017C1F8 File Offset: 0x0017A3F8
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

		// Token: 0x060038FD RID: 14589 RVA: 0x0017C243 File Offset: 0x0017A443
		private void ReleaseReaderLock()
		{
			if (Interlocked.Decrement(ref this._readerCount) == 0)
			{
				this._readSemaphore.Release();
			}
		}

		// Token: 0x060038FE RID: 14590 RVA: 0x0017C25E File Offset: 0x0017A45E
		public void Dispose()
		{
			this._writeSemaphore.Dispose();
			this._readSemaphore.Dispose();
		}

		// Token: 0x04001CB0 RID: 7344
		private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(1, 1);

		// Token: 0x04001CB1 RID: 7345
		private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1);

		// Token: 0x04001CB2 RID: 7346
		private int _readerCount;

		// Token: 0x02000C7A RID: 3194
		private sealed class LockToken : IDisposable
		{
			// Token: 0x06006433 RID: 25651 RVA: 0x00213ECE File Offset: 0x002120CE
			public LockToken(Action action)
			{
				this._action = action;
			}

			// Token: 0x06006434 RID: 25652 RVA: 0x00213EDD File Offset: 0x002120DD
			public void Dispose()
			{
				Action action = this._action;
				if (action == null)
				{
					return;
				}
				action();
			}

			// Token: 0x04003C7F RID: 15487
			private readonly Action _action;
		}
	}
}

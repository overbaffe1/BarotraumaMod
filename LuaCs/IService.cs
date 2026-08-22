using System;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000524 RID: 1316
	public interface IService : IDisposable
	{
		// Token: 0x17001510 RID: 5392
		// (get) Token: 0x0600545B RID: 21595
		bool IsDisposed { get; }

		// Token: 0x0600545C RID: 21596 RVA: 0x002CE0F2 File Offset: 0x002CC2F2
		void CheckDisposed()
		{
			if (this.IsDisposed)
			{
				ThrowHelper.ThrowObjectDisposedException("Tried to call method on disposed object '" + base.GetType().Name + "'!");
			}
		}

		// Token: 0x0600545D RID: 21597 RVA: 0x002CE11B File Offset: 0x002CC31B
		public static void CheckDisposed(IService service)
		{
			if (service.IsDisposed)
			{
				ThrowHelper.ThrowObjectDisposedException("Tried to call method on disposed object '" + service.GetType().Name + "'!");
			}
		}
	}
}

using System;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000411 RID: 1041
	public interface IService : IDisposable
	{
		// Token: 0x17000FC9 RID: 4041
		// (get) Token: 0x06003B3F RID: 15167
		bool IsDisposed { get; }

		// Token: 0x06003B40 RID: 15168 RVA: 0x0018995A File Offset: 0x00187B5A
		void CheckDisposed()
		{
			if (this.IsDisposed)
			{
				ThrowHelper.ThrowObjectDisposedException("Tried to call method on disposed object '" + base.GetType().Name + "'!");
			}
		}

		// Token: 0x06003B41 RID: 15169 RVA: 0x00189983 File Offset: 0x00187B83
		public static void CheckDisposed(IService service)
		{
			if (service.IsDisposed)
			{
				ThrowHelper.ThrowObjectDisposedException("Tried to call method on disposed object '" + service.GetType().Name + "'!");
			}
		}
	}
}

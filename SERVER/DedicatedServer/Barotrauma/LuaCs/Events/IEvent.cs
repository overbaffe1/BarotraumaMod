using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x02000428 RID: 1064
	public interface IEvent
	{
		// Token: 0x06003C6D RID: 15469 RVA: 0x0018CBE3 File Offset: 0x0018ADE3
		bool IsLuaRunner()
		{
			return false;
		}

		// Token: 0x02000D3B RID: 3387
		public abstract class LuaWrapperBase : IEvent
		{
			// Token: 0x060066BD RID: 26301 RVA: 0x0021EC37 File Offset: 0x0021CE37
			protected LuaWrapperBase(IDictionary<string, LuaCsFunc> luaFuncs)
			{
				this.LuaFuncs = luaFuncs;
			}

			// Token: 0x060066BE RID: 26302 RVA: 0x0021EC46 File Offset: 0x0021CE46
			public bool IsLuaRunner()
			{
				return true;
			}

			// Token: 0x04003FAD RID: 16301
			protected readonly IDictionary<string, LuaCsFunc> LuaFuncs;
		}
	}
}

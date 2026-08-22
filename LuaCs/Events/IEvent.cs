using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Events
{
	// Token: 0x0200053B RID: 1339
	public interface IEvent
	{
		// Token: 0x06005588 RID: 21896 RVA: 0x002D13A3 File Offset: 0x002CF5A3
		bool IsLuaRunner()
		{
			return false;
		}

		// Token: 0x0200135A RID: 4954
		public abstract class LuaWrapperBase : IEvent
		{
			// Token: 0x0600974F RID: 38735 RVA: 0x003DB29B File Offset: 0x003D949B
			protected LuaWrapperBase(IDictionary<string, LuaCsFunc> luaFuncs)
			{
				this.LuaFuncs = luaFuncs;
			}

			// Token: 0x06009750 RID: 38736 RVA: 0x003DB2AA File Offset: 0x003D94AA
			public bool IsLuaRunner()
			{
				return true;
			}

			// Token: 0x040062D0 RID: 25296
			protected readonly IDictionary<string, LuaCsFunc> LuaFuncs;
		}
	}
}

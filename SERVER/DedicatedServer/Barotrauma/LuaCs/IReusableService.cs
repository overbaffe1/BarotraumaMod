using System;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000410 RID: 1040
	public interface IReusableService : IService, IDisposable
	{
		// Token: 0x06003B3E RID: 15166
		Result Reset();
	}
}

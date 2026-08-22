using System;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000523 RID: 1315
	public interface IReusableService : IService, IDisposable
	{
		// Token: 0x0600545A RID: 21594
		Result Reset();
	}
}

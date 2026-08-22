using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200050F RID: 1295
	public interface IParserService<in TSrc, TOut> : IService, IDisposable
	{
		// Token: 0x060053F3 RID: 21491
		Result<TOut> TryParseResource(TSrc src);

		// Token: 0x060053F4 RID: 21492
		ImmutableArray<Result<TOut>> TryParseResources(IEnumerable<TSrc> sources);
	}
}

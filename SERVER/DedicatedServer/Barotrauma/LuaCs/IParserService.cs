using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003FC RID: 1020
	public interface IParserService<in TSrc, TOut> : IService, IDisposable
	{
		// Token: 0x06003AD7 RID: 15063
		Result<TOut> TryParseResource(TSrc src);

		// Token: 0x06003AD8 RID: 15064
		ImmutableArray<Result<TOut>> TryParseResources(IEnumerable<TSrc> sources);
	}
}

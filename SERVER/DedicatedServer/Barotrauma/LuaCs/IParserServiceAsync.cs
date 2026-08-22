using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003FD RID: 1021
	public interface IParserServiceAsync<in TSrc, TOut> : IService, IDisposable
	{
		// Token: 0x06003AD9 RID: 15065
		Task<Result<TOut>> TryParseResourceAsync(TSrc src);

		// Token: 0x06003ADA RID: 15066
		Task<ImmutableArray<Result<TOut>>> TryParseResourcesAsync(IEnumerable<TSrc> sources);
	}
}

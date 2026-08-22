using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000510 RID: 1296
	public interface IParserServiceAsync<in TSrc, TOut> : IService, IDisposable
	{
		// Token: 0x060053F5 RID: 21493
		Task<Result<TOut>> TryParseResourceAsync(TSrc src);

		// Token: 0x060053F6 RID: 21494
		Task<ImmutableArray<Result<TOut>>> TryParseResourcesAsync(IEnumerable<TSrc> sources);
	}
}

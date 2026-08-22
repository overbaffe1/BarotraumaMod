using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000511 RID: 1297
	public interface IParserServiceOneToManyAsync<in TSrc, TOut> : IService, IDisposable
	{
		// Token: 0x060053F7 RID: 21495
		Task<Result<ImmutableArray<TOut>>> TryParseResourcesAsync(TSrc src);
	}
}

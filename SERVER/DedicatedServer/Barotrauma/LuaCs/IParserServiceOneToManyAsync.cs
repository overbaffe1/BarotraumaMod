using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003FE RID: 1022
	public interface IParserServiceOneToManyAsync<in TSrc, TOut> : IService, IDisposable
	{
		// Token: 0x06003ADB RID: 15067
		Task<Result<ImmutableArray<TOut>>> TryParseResourcesAsync(TSrc src);
	}
}

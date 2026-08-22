using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;
using MoonSharp.Interpreter.Loaders;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000532 RID: 1330
	public interface ILuaScriptLoader : IService, IDisposable, IScriptLoader, ISafeStorageValidation
	{
		// Token: 0x060054B0 RID: 21680
		void ClearCaches();

		// Token: 0x060054B1 RID: 21681
		void SetCachingPolicy(bool useCaching);

		// Token: 0x060054B2 RID: 21682
		[return: TupleElementNames(new string[]
		{
			"Path",
			null
		})]
		Task<Result<ImmutableArray<ValueTuple<ContentPath, Result<string>>>>> CacheResourcesAsync(ImmutableArray<ILuaScriptResourceInfo> resourceInfos);
	}
}

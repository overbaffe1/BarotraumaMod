using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;
using MoonSharp.Interpreter.Loaders;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200041F RID: 1055
	public interface ILuaScriptLoader : IService, IDisposable, IScriptLoader, ISafeStorageValidation
	{
		// Token: 0x06003B94 RID: 15252
		void ClearCaches();

		// Token: 0x06003B95 RID: 15253
		void SetCachingPolicy(bool useCaching);

		// Token: 0x06003B96 RID: 15254
		[return: TupleElementNames(new string[]
		{
			"Path",
			null
		})]
		Task<Result<ImmutableArray<ValueTuple<ContentPath, Result<string>>>>> CacheResourcesAsync(ImmutableArray<ILuaScriptResourceInfo> resourceInfos);
	}
}

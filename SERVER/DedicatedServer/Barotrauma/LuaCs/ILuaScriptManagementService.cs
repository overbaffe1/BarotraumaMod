using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000404 RID: 1028
	[NullableContext(1)]
	public interface ILuaScriptManagementService : IReusableService, IService, IDisposable
	{
		// Token: 0x17000FC6 RID: 4038
		// (get) Token: 0x06003AFF RID: 15103
		[Nullable(2)]
		Script InternalScript { [NullableContext(2)] get; }

		// Token: 0x06003B00 RID: 15104
		[return: Nullable(2)]
		object GetGlobalTableValue(string tableName);

		// Token: 0x06003B01 RID: 15105
		Result<DynValue> DoString(string code);

		// Token: 0x06003B02 RID: 15106
		[return: Nullable(2)]
		DynValue CallFunctionSafe(object luaFunction, params object[] args);

		// Token: 0x06003B03 RID: 15107
		void SetCachingPolicy(bool useCaching);

		// Token: 0x06003B04 RID: 15108
		Task<Result> LoadScriptResourcesAsync([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<ILuaScriptResourceInfo> resourcesInfo);

		// Token: 0x06003B05 RID: 15109
		Result ExecuteLoadedScripts([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<ILuaScriptResourceInfo> executionOrder, bool enableSandbox);

		// Token: 0x06003B06 RID: 15110
		Result DisposePackageResources(ContentPackage package);

		// Token: 0x06003B07 RID: 15111
		Result UnloadActiveScripts();

		// Token: 0x06003B08 RID: 15112
		Result DisposeAllPackageResources();
	}
}

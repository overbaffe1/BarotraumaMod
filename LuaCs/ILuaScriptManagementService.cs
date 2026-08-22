using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000517 RID: 1303
	[NullableContext(1)]
	public interface ILuaScriptManagementService : IReusableService, IService, IDisposable
	{
		// Token: 0x1700150D RID: 5389
		// (get) Token: 0x0600541B RID: 21531
		[Nullable(2)]
		Script InternalScript { [NullableContext(2)] get; }

		// Token: 0x0600541C RID: 21532
		[return: Nullable(2)]
		object GetGlobalTableValue(string tableName);

		// Token: 0x0600541D RID: 21533
		Result<DynValue> DoString(string code);

		// Token: 0x0600541E RID: 21534
		[return: Nullable(2)]
		DynValue CallFunctionSafe(object luaFunction, params object[] args);

		// Token: 0x0600541F RID: 21535
		void SetCachingPolicy(bool useCaching);

		// Token: 0x06005420 RID: 21536
		Task<Result> LoadScriptResourcesAsync([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<ILuaScriptResourceInfo> resourcesInfo);

		// Token: 0x06005421 RID: 21537
		Result ExecuteLoadedScripts([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<ILuaScriptResourceInfo> executionOrder, bool enableSandbox);

		// Token: 0x06005422 RID: 21538
		Result DisposePackageResources(ContentPackage package);

		// Token: 0x06005423 RID: 21539
		Result UnloadActiveScripts();

		// Token: 0x06005424 RID: 21540
		Result DisposeAllPackageResources();
	}
}

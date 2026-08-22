using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000518 RID: 1304
	public interface IModConfigService : IService, IDisposable
	{
		// Token: 0x06005425 RID: 21541
		Task<Result<IModConfigInfo>> CreateConfigAsync([NotNull] ContentPackage src);

		// Token: 0x06005426 RID: 21542
		[return: TupleElementNames(new string[]
		{
			"Source",
			"Config"
		})]
		Task<ImmutableArray<ValueTuple<ContentPackage, Result<IModConfigInfo>>>> CreateConfigsAsync(ImmutableArray<ContentPackage> src);
	}
}

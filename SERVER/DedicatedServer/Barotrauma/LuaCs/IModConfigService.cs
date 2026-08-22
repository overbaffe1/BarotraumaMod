using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000405 RID: 1029
	public interface IModConfigService : IService, IDisposable
	{
		// Token: 0x06003B09 RID: 15113
		Task<Result<IModConfigInfo>> CreateConfigAsync([NotNull] ContentPackage src);

		// Token: 0x06003B0A RID: 15114
		[return: TupleElementNames(new string[]
		{
			"Source",
			"Config"
		})]
		Task<ImmutableArray<ValueTuple<ContentPackage, Result<IModConfigInfo>>>> CreateConfigsAsync(ImmutableArray<ContentPackage> src);
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000589 RID: 1417
	public interface IConfigProfileInfo : IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x1700157C RID: 5500
		// (get) Token: 0x060056CF RID: 22223
		[TupleElementNames(new string[]
		{
			"SettingName",
			"Element"
		})]
		IReadOnlyList<ValueTuple<string, XElement>> ProfileValues { [return: TupleElementNames(new string[]
		{
			"SettingName",
			"Element"
		})] get; }
	}
}

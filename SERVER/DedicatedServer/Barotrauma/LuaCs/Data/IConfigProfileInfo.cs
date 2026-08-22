using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200046B RID: 1131
	public interface IConfigProfileInfo : IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>
	{
		// Token: 0x17001016 RID: 4118
		// (get) Token: 0x06003D5F RID: 15711
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

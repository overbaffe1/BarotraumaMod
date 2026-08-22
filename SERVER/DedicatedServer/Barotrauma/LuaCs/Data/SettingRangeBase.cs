using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000486 RID: 1158
	public abstract class SettingRangeBase<T> : SettingEntry<T>, ISettingRangeBase<T>, ISettingBase<T>, ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x06003E10 RID: 15888 RVA: 0x0018F798 File Offset: 0x0018D998
		public SettingRangeBase(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
		}

		// Token: 0x1700105E RID: 4190
		// (get) Token: 0x06003E11 RID: 15889 RVA: 0x0018F7A2 File Offset: 0x0018D9A2
		// (set) Token: 0x06003E12 RID: 15890 RVA: 0x0018F7AA File Offset: 0x0018D9AA
		public T MinValue { get; protected set; }

		// Token: 0x1700105F RID: 4191
		// (get) Token: 0x06003E13 RID: 15891 RVA: 0x0018F7B3 File Offset: 0x0018D9B3
		// (set) Token: 0x06003E14 RID: 15892 RVA: 0x0018F7BB File Offset: 0x0018D9BB
		public T MaxValue { get; protected set; }

		// Token: 0x17001060 RID: 4192
		// (get) Token: 0x06003E15 RID: 15893 RVA: 0x0018F7C4 File Offset: 0x0018D9C4
		// (set) Token: 0x06003E16 RID: 15894 RVA: 0x0018F7CC File Offset: 0x0018D9CC
		public int IncrementalSteps { get; protected set; }
	}
}

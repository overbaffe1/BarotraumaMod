using System;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000488 RID: 1160
	public class SettingRangeInt : SettingRangeBase<int>
	{
		// Token: 0x06003E19 RID: 15897 RVA: 0x0018F858 File Offset: 0x0018DA58
		public SettingRangeInt(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
			base.MinValue = configInfo.Element.GetAttributeInt("Min", int.MinValue);
			base.MaxValue = configInfo.Element.GetAttributeInt("Max", int.MaxValue);
			base.IncrementalSteps = configInfo.Element.GetAttributeInt("Steps", 3);
		}

		// Token: 0x06003E1A RID: 15898 RVA: 0x0018F8BA File Offset: 0x0018DABA
		public override bool TrySetValue(int value)
		{
			return value <= base.MaxValue && value >= base.MinValue && base.TrySetValue(value);
		}

		// Token: 0x02000D6A RID: 3434
		public class RangeFactory : ISettingBase.IFactory<SettingRangeInt>
		{
			// Token: 0x0600671C RID: 26396 RVA: 0x0021F9E7 File Offset: 0x0021DBE7
			public SettingRangeInt CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingRangeInt(configInfo, valueChangePredicate);
			}
		}
	}
}

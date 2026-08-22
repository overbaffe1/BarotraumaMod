using System;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000487 RID: 1159
	public class SettingRangeFloat : SettingRangeBase<float>
	{
		// Token: 0x06003E17 RID: 15895 RVA: 0x0018F7D8 File Offset: 0x0018D9D8
		public SettingRangeFloat(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
			base.MinValue = configInfo.Element.GetAttributeFloat("Min", float.MinValue);
			base.MaxValue = configInfo.Element.GetAttributeFloat("Max", float.MaxValue);
			base.IncrementalSteps = configInfo.Element.GetAttributeInt("Steps", 3);
		}

		// Token: 0x06003E18 RID: 15896 RVA: 0x0018F83A File Offset: 0x0018DA3A
		public override bool TrySetValue(float value)
		{
			return value <= base.MaxValue && value >= base.MinValue && base.TrySetValue(value);
		}

		// Token: 0x02000D69 RID: 3433
		public class RangeFactory : ISettingBase.IFactory<SettingRangeFloat>
		{
			// Token: 0x0600671A RID: 26394 RVA: 0x0021F9CB File Offset: 0x0021DBCB
			public SettingRangeFloat CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingRangeFloat(configInfo, valueChangePredicate);
			}
		}
	}
}

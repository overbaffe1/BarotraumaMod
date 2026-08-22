using System;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A4 RID: 1444
	public class SettingRangeInt : SettingRangeBase<int>
	{
		// Token: 0x06005787 RID: 22407 RVA: 0x002D4C08 File Offset: 0x002D2E08
		public SettingRangeInt(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
			base.MinValue = configInfo.Element.GetAttributeInt("Min", int.MinValue);
			base.MaxValue = configInfo.Element.GetAttributeInt("Max", int.MaxValue);
			base.IncrementalSteps = configInfo.Element.GetAttributeInt("Steps", 3);
		}

		// Token: 0x06005788 RID: 22408 RVA: 0x002D4C6A File Offset: 0x002D2E6A
		public override bool TrySetValue(int value)
		{
			return value <= base.MaxValue && value >= base.MinValue && base.TrySetValue(value);
		}

		// Token: 0x06005789 RID: 22409 RVA: 0x002D4C88 File Offset: 0x002D2E88
		public override void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
		{
			GUIUtil.Slider(layoutGroup, new Vector2((float)base.MinValue, (float)base.MaxValue), base.IncrementalSteps, (float val) => ((int)val).ToString(), (float)base.Value, delegate(float val)
			{
				Action<string> onSerializedValue2 = onSerializedValue;
				if (onSerializedValue2 == null)
				{
					return;
				}
				onSerializedValue2(((int)val).ToString());
			}, TextManager.Get(base.GetDisplayInfo().Tooltip), relativeSize);
		}

		// Token: 0x02001391 RID: 5009
		public class RangeFactory : ISettingBase.IFactory<SettingRangeInt>
		{
			// Token: 0x060097C5 RID: 38853 RVA: 0x003DC3BA File Offset: 0x003DA5BA
			public SettingRangeInt CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingRangeInt(configInfo, valueChangePredicate);
			}
		}
	}
}

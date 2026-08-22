using System;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A3 RID: 1443
	public class SettingRangeFloat : SettingRangeBase<float>
	{
		// Token: 0x06005784 RID: 22404 RVA: 0x002D4B0C File Offset: 0x002D2D0C
		public SettingRangeFloat(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
			base.MinValue = configInfo.Element.GetAttributeFloat("Min", float.MinValue);
			base.MaxValue = configInfo.Element.GetAttributeFloat("Max", float.MaxValue);
			base.IncrementalSteps = configInfo.Element.GetAttributeInt("Steps", 3);
		}

		// Token: 0x06005785 RID: 22405 RVA: 0x002D4B6E File Offset: 0x002D2D6E
		public override bool TrySetValue(float value)
		{
			return value <= base.MaxValue && value >= base.MinValue && base.TrySetValue(value);
		}

		// Token: 0x06005786 RID: 22406 RVA: 0x002D4B8C File Offset: 0x002D2D8C
		public override void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
		{
			GUIUtil.Slider(layoutGroup, new Vector2(base.MinValue, base.MaxValue), base.IncrementalSteps, (float val) => val.ToString("G4", CultureInfo.InvariantCulture), base.Value, delegate(float val)
			{
				Action<string> onSerializedValue2 = onSerializedValue;
				if (onSerializedValue2 == null)
				{
					return;
				}
				onSerializedValue2(val.ToString());
			}, TextManager.Get(base.GetDisplayInfo().Tooltip), relativeSize);
		}

		// Token: 0x0200138E RID: 5006
		public class RangeFactory : ISettingBase.IFactory<SettingRangeFloat>
		{
			// Token: 0x060097BE RID: 38846 RVA: 0x003DC356 File Offset: 0x003DA556
			public SettingRangeFloat CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingRangeFloat(configInfo, valueChangePredicate);
			}
		}
	}
}

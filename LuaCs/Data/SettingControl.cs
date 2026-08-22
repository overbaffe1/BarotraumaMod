using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000579 RID: 1401
	public sealed class SettingControl : SettingBase, ISettingControl, ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable
	{
		// Token: 0x06005613 RID: 22035 RVA: 0x002D150B File Offset: 0x002CF70B
		public SettingControl(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo)
		{
			this._valueChangePredicate = valueChangePredicate;
			this.TrySetSerializedValue(configInfo.Element);
		}

		// Token: 0x06005614 RID: 22036 RVA: 0x002D153D File Offset: 0x002CF73D
		protected override void OnDispose()
		{
			this.OnValueChanged = null;
		}

		// Token: 0x06005615 RID: 22037 RVA: 0x002D1546 File Offset: 0x002CF746
		public override Type GetValueType()
		{
			return typeof(KeyOrMouse);
		}

		// Token: 0x06005616 RID: 22038 RVA: 0x002D1552 File Offset: 0x002CF752
		public override string GetStringValue()
		{
			return this.Value.ToString();
		}

		// Token: 0x06005617 RID: 22039 RVA: 0x002D155F File Offset: 0x002CF75F
		public override string GetDefaultStringValue()
		{
			return new KeyOrMouse(Keys.NumLock).ToString();
		}

		// Token: 0x06005618 RID: 22040 RVA: 0x002D1570 File Offset: 0x002CF770
		public override bool TrySetSerializedValue(OneOf<string, XElement> value)
		{
			KeyOrMouse newVal = value.Match<KeyOrMouse>((string v) => SettingControl.<TrySetSerializedValue>g__GetKeyOrMouse|7_2(v), (XElement e) => e.GetAttributeKeyOrMouse("Value", null));
			if (newVal == null)
			{
				return false;
			}
			if (this._valueChangePredicate != null && !this._valueChangePredicate(newVal))
			{
				return false;
			}
			this.Value = newVal;
			Action<ISettingBase> onValueChanged = this.OnValueChanged;
			if (onValueChanged != null)
			{
				onValueChanged(this);
			}
			return true;
		}

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x06005619 RID: 22041 RVA: 0x002D1600 File Offset: 0x002CF800
		// (remove) Token: 0x0600561A RID: 22042 RVA: 0x002D1638 File Offset: 0x002CF838
		public override event Action<ISettingBase> OnValueChanged;

		// Token: 0x0600561B RID: 22043 RVA: 0x002D166D File Offset: 0x002CF86D
		public override OneOf<string, XElement> GetSerializableValue()
		{
			return this.Value.ToString();
		}

		// Token: 0x17001547 RID: 5447
		// (get) Token: 0x0600561C RID: 22044 RVA: 0x002D167F File Offset: 0x002CF87F
		// (set) Token: 0x0600561D RID: 22045 RVA: 0x002D1687 File Offset: 0x002CF887
		public KeyOrMouse Value { get; private set; } = new KeyOrMouse(Keys.NumLock);

		// Token: 0x0600561E RID: 22046 RVA: 0x002D1690 File Offset: 0x002CF890
		public bool TrySetValue(KeyOrMouse value)
		{
			this.Value = value;
			Action<ISettingBase> onValueChanged = this.OnValueChanged;
			if (onValueChanged != null)
			{
				onValueChanged(this);
			}
			return true;
		}

		// Token: 0x0600561F RID: 22047 RVA: 0x002D16AC File Offset: 0x002CF8AC
		public bool IsDown()
		{
			if (this.Value == null)
			{
				return false;
			}
			switch (this.Value.MouseButton)
			{
			case MouseButton.None:
				return PlayerInput.KeyDown(this.Value.Key);
			case MouseButton.PrimaryMouse:
				return PlayerInput.PrimaryMouseButtonHeld();
			case MouseButton.SecondaryMouse:
				return PlayerInput.SecondaryMouseButtonHeld();
			case MouseButton.MiddleMouse:
				return PlayerInput.MidButtonHeld();
			case MouseButton.MouseButton4:
				return PlayerInput.Mouse4ButtonHeld();
			case MouseButton.MouseButton5:
				return PlayerInput.Mouse5ButtonHeld();
			case MouseButton.MouseWheelUp:
				return PlayerInput.MouseWheelUpClicked();
			case MouseButton.MouseWheelDown:
				return PlayerInput.MouseWheelDownClicked();
			default:
				return false;
			}
		}

		// Token: 0x06005620 RID: 22048 RVA: 0x002D1738 File Offset: 0x002CF938
		public bool IsHit()
		{
			if (this.Value == null)
			{
				return false;
			}
			switch (this.Value.MouseButton)
			{
			case MouseButton.None:
				return PlayerInput.KeyHit(this.Value.Key);
			case MouseButton.PrimaryMouse:
				return PlayerInput.PrimaryMouseButtonClicked();
			case MouseButton.SecondaryMouse:
				return PlayerInput.SecondaryMouseButtonClicked();
			case MouseButton.MiddleMouse:
				return PlayerInput.MidButtonClicked();
			case MouseButton.MouseButton4:
				return PlayerInput.Mouse4ButtonClicked();
			case MouseButton.MouseButton5:
				return PlayerInput.Mouse5ButtonClicked();
			case MouseButton.MouseWheelUp:
				return PlayerInput.MouseWheelUpClicked();
			case MouseButton.MouseWheelDown:
				return PlayerInput.MouseWheelDownClicked();
			default:
				return false;
			}
		}

		// Token: 0x06005621 RID: 22049 RVA: 0x002D17C4 File Offset: 0x002CF9C4
		public override void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
		{
			SettingControl.<>c__DisplayClass20_0 CS$<>8__locals1 = new SettingControl.<>c__DisplayClass20_0();
			CS$<>8__locals1.layoutGroup = layoutGroup;
			CS$<>8__locals1.onSerializedValue = onSerializedValue;
			GUIButton inputButton = new GUIButton(new RectTransform(relativeSize, CS$<>8__locals1.layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "GUITextBoxNoIcon", null)
			{
				Text = this.Value.ToString(),
				OnClicked = delegate(GUIButton btn, object obj)
				{
					if (SettingControl.InputListener != null)
					{
						return true;
					}
					CoroutineManager.Invoke(delegate
					{
						CS$<>8__locals1.<AddDisplayComponent>g__CreateListener|1(btn);
					}, 0f);
					return true;
				}
			};
			inputButton.OutlineColor = Color.PeachPuff;
			inputButton.TextColor = Color.White;
		}

		// Token: 0x06005622 RID: 22050 RVA: 0x002D1868 File Offset: 0x002CFA68
		[CompilerGenerated]
		internal static KeyOrMouse <TrySetSerializedValue>g__GetKeyOrMouse|7_2(string strValue)
		{
			if (strValue == null)
			{
				strValue = string.Empty;
			}
			Keys key;
			if (Enum.TryParse<Keys>(strValue, true, out key))
			{
				return key;
			}
			MouseButton mouseButton;
			if (Enum.TryParse<MouseButton>(strValue, out mouseButton))
			{
				return mouseButton;
			}
			int mouseButtonInt;
			if (int.TryParse(strValue, NumberStyles.Any, CultureInfo.InvariantCulture, out mouseButtonInt) && Enum.GetValues<MouseButton>().Contains((MouseButton)mouseButtonInt))
			{
				return (MouseButton)mouseButtonInt;
			}
			if (string.Equals(strValue, "LeftMouse", StringComparison.OrdinalIgnoreCase))
			{
				return (!PlayerInput.MouseButtonsSwapped()) ? MouseButton.PrimaryMouse : MouseButton.SecondaryMouse;
			}
			if (string.Equals(strValue, "RightMouse", StringComparison.OrdinalIgnoreCase))
			{
				return (!PlayerInput.MouseButtonsSwapped()) ? MouseButton.SecondaryMouse : MouseButton.PrimaryMouse;
			}
			return null;
		}

		// Token: 0x06005623 RID: 22051 RVA: 0x002D1909 File Offset: 0x002CFB09
		[CompilerGenerated]
		internal static void <AddDisplayComponent>g__ClearListener|20_0()
		{
			GUICustomComponent inputListener = SettingControl.InputListener;
			if (inputListener != null)
			{
				inputListener.Parent.RemoveChild(SettingControl.InputListener);
			}
			SettingControl.InputListener = null;
		}

		// Token: 0x04002C6E RID: 11374
		private Func<OneOf<string, XElement, object>, bool> _valueChangePredicate;

		// Token: 0x04002C71 RID: 11377
		private static GUICustomComponent InputListener;

		// Token: 0x02001382 RID: 4994
		public class Factory : ISettingBase.IFactory<ISettingBase>
		{
			// Token: 0x0600979C RID: 38812 RVA: 0x003DBE18 File Offset: 0x003DA018
			public ISettingBase CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingControl(configInfo, valueChangePredicate);
			}
		}
	}
}

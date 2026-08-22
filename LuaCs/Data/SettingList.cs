using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A1 RID: 1441
	public class SettingList<T> : SettingEntry<T>, ISettingList<T>, ISettingBase<T>, ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x06005776 RID: 22390 RVA: 0x002D47D4 File Offset: 0x002D29D4
		public SettingList(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
			if (!typeof(T).IsEnum && !typeof(T).IsPrimitive && !(typeof(T) == typeof(string)))
			{
				ThrowHelper.ThrowArgumentException("ISettingBase: The type of T is not an allowed type.");
			}
			this.ValueChangePredicate = valueChangePredicate;
			XElement childElement = base.ConfigInfo.Element.GetChildElement("Values", StringComparison.OrdinalIgnoreCase);
			ImmutableArray<XElement>? immutableArray;
			if (childElement == null)
			{
				immutableArray = null;
			}
			else
			{
				IEnumerable<XElement> childElements = childElement.GetChildElements("Value", StringComparison.OrdinalIgnoreCase);
				immutableArray = ((childElements != null) ? new ImmutableArray<XElement>?(childElements.ToImmutableArray<XElement>()) : null);
			}
			ImmutableArray<XElement>? valuesElements = immutableArray;
			Guard.IsNotNull<ImmutableArray<XElement>>(valuesElements, base.InternalName);
			if (valuesElements.Value.IsEmpty)
			{
				ThrowHelper.ThrowArgumentNullException(base.InternalName + ": Could not find any values in list!");
			}
			foreach (XElement element in valuesElements.Value)
			{
				T v;
				if (!SettingList<T>.<.ctor>g__TryConvert|1_0(element, out v))
				{
					ThrowHelper.ThrowArgumentException(base.InternalName + ": Error while parsing list values");
				}
				this._valuesList.Add(v);
			}
			T v2;
			if (SettingList<T>.<.ctor>g__TryConvert|1_0(base.ConfigInfo.Element, out v2) && this._valuesList.Contains(v2))
			{
				base.Value = v2;
				base.DefaultValue = v2;
				return;
			}
			base.Value = this._valuesList[0];
			base.DefaultValue = this._valuesList[0];
		}

		// Token: 0x06005777 RID: 22391 RVA: 0x002D4965 File Offset: 0x002D2B65
		public override bool TrySetValue(T value)
		{
			return this._valuesList.Contains(value) && base.TrySetValue(value);
		}

		// Token: 0x06005778 RID: 22392 RVA: 0x002D497E File Offset: 0x002D2B7E
		public bool TrySetValueByIndex(int index)
		{
			return this._valuesList.Count > index && base.TrySetValue(this._valuesList[index]);
		}

		// Token: 0x170015C2 RID: 5570
		// (get) Token: 0x06005779 RID: 22393 RVA: 0x002D49A2 File Offset: 0x002D2BA2
		public IReadOnlyList<T> Options
		{
			get
			{
				return this._valuesList.AsReadOnly();
			}
		}

		// Token: 0x170015C3 RID: 5571
		// (get) Token: 0x0600577A RID: 22394 RVA: 0x002D49AF File Offset: 0x002D2BAF
		public IReadOnlyList<string> StringOptions
		{
			get
			{
				return (from e in this._valuesList
				select e.ToString()).ToImmutableArray<string>();
			}
		}

		// Token: 0x0600577B RID: 22395 RVA: 0x002D49E8 File Offset: 0x002D2BE8
		public override void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
		{
			GUIUtil.Dropdown<T>(layoutGroup, (T val) => base.<AddDisplayComponent>g__GetLocalizedString|2(val.ToString(), val.ToString()), null, this.Options, base.Value, delegate(T val)
			{
				Action<string> onSerializedValue2 = onSerializedValue;
				if (onSerializedValue2 == null)
				{
					return;
				}
				onSerializedValue2(val.ToString());
			}, new Vector2(relativeSize.X, 1f), 1f);
		}

		// Token: 0x0600577C RID: 22396 RVA: 0x002D4A4C File Offset: 0x002D2C4C
		[CompilerGenerated]
		internal static bool <.ctor>g__TryConvert|1_0(XElement element, out T value)
		{
			bool result;
			try
			{
				value = (T)((object)Convert.ChangeType(element.GetAttributeString("Value", null), typeof(T)));
				result = true;
			}
			catch (Exception e) when (e is InvalidCastException || e is ArgumentNullException)
			{
				value = default(T);
				result = false;
			}
			return result;
		}

		// Token: 0x04002CAE RID: 11438
		private readonly List<T> _valuesList = new List<T>();

		// Token: 0x0200138B RID: 5003
		public class LFactory : ISettingBase.IFactory<ISettingList<T>>
		{
			// Token: 0x060097B5 RID: 38837 RVA: 0x003DC23B File Offset: 0x003DA43B
			public ISettingList<T> CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingList<T>(configInfo, valueChangePredicate);
			}
		}
	}
}

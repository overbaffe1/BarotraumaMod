using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000485 RID: 1157
	public class SettingList<T> : SettingEntry<T>, ISettingList<T>, ISettingBase<T>, ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x06003E0A RID: 15882 RVA: 0x0018F504 File Offset: 0x0018D704
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

		// Token: 0x06003E0B RID: 15883 RVA: 0x0018F695 File Offset: 0x0018D895
		public override bool TrySetValue(T value)
		{
			return this._valuesList.Contains(value) && base.TrySetValue(value);
		}

		// Token: 0x06003E0C RID: 15884 RVA: 0x0018F6AE File Offset: 0x0018D8AE
		public bool TrySetValueByIndex(int index)
		{
			return this._valuesList.Count > index && base.TrySetValue(this._valuesList[index]);
		}

		// Token: 0x1700105C RID: 4188
		// (get) Token: 0x06003E0D RID: 15885 RVA: 0x0018F6D2 File Offset: 0x0018D8D2
		public IReadOnlyList<T> Options
		{
			get
			{
				return this._valuesList.AsReadOnly();
			}
		}

		// Token: 0x1700105D RID: 4189
		// (get) Token: 0x06003E0E RID: 15886 RVA: 0x0018F6DF File Offset: 0x0018D8DF
		public IReadOnlyList<string> StringOptions
		{
			get
			{
				return (from e in this._valuesList
				select e.ToString()).ToImmutableArray<string>();
			}
		}

		// Token: 0x06003E0F RID: 15887 RVA: 0x0018F718 File Offset: 0x0018D918
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

		// Token: 0x04001DBD RID: 7613
		private readonly List<T> _valuesList = new List<T>();

		// Token: 0x02000D67 RID: 3431
		public class LFactory : ISettingBase.IFactory<ISettingList<T>>
		{
			// Token: 0x06006715 RID: 26389 RVA: 0x0021F98C File Offset: 0x0021DB8C
			public ISettingList<T> CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingList<T>(configInfo, valueChangePredicate);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000577 RID: 1399
	public interface ISettingBase : IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable
	{
		// Token: 0x06005605 RID: 22021
		IConfigInfo GetConfigInfo();

		// Token: 0x06005606 RID: 22022
		IConfigDisplayInfo GetDisplayInfo();

		// Token: 0x17001545 RID: 5445
		// (get) Token: 0x06005607 RID: 22023
		bool IsDisposed { get; }

		// Token: 0x06005608 RID: 22024
		Type GetValueType();

		// Token: 0x06005609 RID: 22025
		string GetStringValue();

		// Token: 0x0600560A RID: 22026
		string GetDefaultStringValue();

		// Token: 0x0600560B RID: 22027
		bool TrySetSerializedValue(OneOf<string, XElement> value);

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x0600560C RID: 22028
		// (remove) Token: 0x0600560D RID: 22029
		event Action<ISettingBase> OnValueChanged;

		// Token: 0x0600560E RID: 22030
		OneOf<string, XElement> GetSerializableValue();

		// Token: 0x02001381 RID: 4993
		public interface IFactory<out T> where T : ISettingBase
		{
			// Token: 0x0600979B RID: 38811
			T CreateInstance([NotNull] IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000477 RID: 1143
	public interface ISettingBase : IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable
	{
		// Token: 0x06003D8B RID: 15755
		IConfigInfo GetConfigInfo();

		// Token: 0x1700102D RID: 4141
		// (get) Token: 0x06003D8C RID: 15756
		bool IsDisposed { get; }

		// Token: 0x06003D8D RID: 15757
		Type GetValueType();

		// Token: 0x06003D8E RID: 15758
		string GetStringValue();

		// Token: 0x06003D8F RID: 15759
		string GetDefaultStringValue();

		// Token: 0x06003D90 RID: 15760
		bool TrySetSerializedValue(OneOf<string, XElement> value);

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x06003D91 RID: 15761
		// (remove) Token: 0x06003D92 RID: 15762
		event Action<ISettingBase> OnValueChanged;

		// Token: 0x06003D93 RID: 15763
		OneOf<string, XElement> GetSerializableValue();

		// Token: 0x02000D64 RID: 3428
		public interface IFactory<out T> where T : ISettingBase
		{
			// Token: 0x0600670F RID: 26383
			T CreateInstance([NotNull] IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate);
		}
	}
}

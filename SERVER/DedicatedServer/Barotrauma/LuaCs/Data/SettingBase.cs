using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000483 RID: 1155
	public abstract class SettingBase : ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable
	{
		// Token: 0x06003DE2 RID: 15842 RVA: 0x0018EB5A File Offset: 0x0018CD5A
		protected SettingBase(IConfigInfo configInfo)
		{
			Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
			this.ConfigInfo = configInfo;
		}

		// Token: 0x17001053 RID: 4179
		// (get) Token: 0x06003DE3 RID: 15843 RVA: 0x0018EB74 File Offset: 0x0018CD74
		// (set) Token: 0x06003DE4 RID: 15844 RVA: 0x0018EB7C File Offset: 0x0018CD7C
		private protected IConfigInfo ConfigInfo { protected get; private set; }

		// Token: 0x17001054 RID: 4180
		// (get) Token: 0x06003DE5 RID: 15845 RVA: 0x0018EB85 File Offset: 0x0018CD85
		public string InternalName
		{
			get
			{
				return this.ConfigInfo.InternalName;
			}
		}

		// Token: 0x17001055 RID: 4181
		// (get) Token: 0x06003DE6 RID: 15846 RVA: 0x0018EB92 File Offset: 0x0018CD92
		public ContentPackage OwnerPackage
		{
			get
			{
				return this.ConfigInfo.OwnerPackage;
			}
		}

		// Token: 0x06003DE7 RID: 15847 RVA: 0x0018EB9F File Offset: 0x0018CD9F
		public IConfigInfo GetConfigInfo()
		{
			return this.ConfigInfo;
		}

		// Token: 0x06003DE8 RID: 15848 RVA: 0x0018EBA7 File Offset: 0x0018CDA7
		public virtual bool Equals(ISettingBase other)
		{
			return other != null && (this == other || (!this.IsDisposed && this.OwnerPackage == other.OwnerPackage && this.InternalName.Equals(other.InternalName)));
		}

		// Token: 0x17001056 RID: 4182
		// (get) Token: 0x06003DE9 RID: 15849 RVA: 0x0018EBDD File Offset: 0x0018CDDD
		// (set) Token: 0x06003DEA RID: 15850 RVA: 0x0018EBEA File Offset: 0x0018CDEA
		public virtual bool IsDisposed
		{
			get
			{
				return ModUtils.Threading.GetBool(ref this._isDisposed);
			}
			private set
			{
				ModUtils.Threading.SetBool(ref this._isDisposed, value);
			}
		}

		// Token: 0x06003DEB RID: 15851
		protected abstract void OnDispose();

		// Token: 0x06003DEC RID: 15852 RVA: 0x0018EBF8 File Offset: 0x0018CDF8
		public virtual void Dispose()
		{
			if (!ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
			{
				return;
			}
			this.OnDispose();
			this.ConfigInfo = null;
			GC.SuppressFinalize(this);
		}

		// Token: 0x06003DED RID: 15853
		public abstract Type GetValueType();

		// Token: 0x06003DEE RID: 15854
		public abstract string GetStringValue();

		// Token: 0x06003DEF RID: 15855
		public abstract string GetDefaultStringValue();

		// Token: 0x06003DF0 RID: 15856
		public abstract bool TrySetSerializedValue(OneOf<string, XElement> value);

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x06003DF1 RID: 15857
		// (remove) Token: 0x06003DF2 RID: 15858
		public abstract event Action<ISettingBase> OnValueChanged;

		// Token: 0x06003DF3 RID: 15859
		public abstract OneOf<string, XElement> GetSerializableValue();

		// Token: 0x04001DB7 RID: 7607
		private int _isDisposed;
	}
}

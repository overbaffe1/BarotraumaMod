using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200059F RID: 1439
	public abstract class SettingBase : ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable
	{
		// Token: 0x0600574B RID: 22347 RVA: 0x002D3B7E File Offset: 0x002D1D7E
		protected SettingBase(IConfigInfo configInfo)
		{
			Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
			this.ConfigInfo = configInfo;
		}

		// Token: 0x170015B9 RID: 5561
		// (get) Token: 0x0600574C RID: 22348 RVA: 0x002D3B98 File Offset: 0x002D1D98
		// (set) Token: 0x0600574D RID: 22349 RVA: 0x002D3BA0 File Offset: 0x002D1DA0
		private protected IConfigInfo ConfigInfo { protected get; private set; }

		// Token: 0x170015BA RID: 5562
		// (get) Token: 0x0600574E RID: 22350 RVA: 0x002D3BA9 File Offset: 0x002D1DA9
		public string InternalName
		{
			get
			{
				return this.ConfigInfo.InternalName;
			}
		}

		// Token: 0x170015BB RID: 5563
		// (get) Token: 0x0600574F RID: 22351 RVA: 0x002D3BB6 File Offset: 0x002D1DB6
		public ContentPackage OwnerPackage
		{
			get
			{
				return this.ConfigInfo.OwnerPackage;
			}
		}

		// Token: 0x06005750 RID: 22352 RVA: 0x002D3BC3 File Offset: 0x002D1DC3
		public IConfigInfo GetConfigInfo()
		{
			return this.ConfigInfo;
		}

		// Token: 0x06005751 RID: 22353 RVA: 0x002D3BCB File Offset: 0x002D1DCB
		public IConfigDisplayInfo GetDisplayInfo()
		{
			return this.ConfigInfo;
		}

		// Token: 0x06005752 RID: 22354 RVA: 0x002D3BD3 File Offset: 0x002D1DD3
		public virtual bool Equals(ISettingBase other)
		{
			return other != null && (this == other || (!this.IsDisposed && this.OwnerPackage == other.OwnerPackage && this.InternalName.Equals(other.InternalName)));
		}

		// Token: 0x170015BC RID: 5564
		// (get) Token: 0x06005753 RID: 22355 RVA: 0x002D3C09 File Offset: 0x002D1E09
		// (set) Token: 0x06005754 RID: 22356 RVA: 0x002D3C16 File Offset: 0x002D1E16
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

		// Token: 0x06005755 RID: 22357
		protected abstract void OnDispose();

		// Token: 0x06005756 RID: 22358 RVA: 0x002D3C24 File Offset: 0x002D1E24
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

		// Token: 0x06005757 RID: 22359
		public abstract Type GetValueType();

		// Token: 0x06005758 RID: 22360
		public abstract string GetStringValue();

		// Token: 0x06005759 RID: 22361
		public abstract string GetDefaultStringValue();

		// Token: 0x0600575A RID: 22362
		public abstract bool TrySetSerializedValue(OneOf<string, XElement> value);

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x0600575B RID: 22363
		// (remove) Token: 0x0600575C RID: 22364
		public abstract event Action<ISettingBase> OnValueChanged;

		// Token: 0x0600575D RID: 22365
		public abstract OneOf<string, XElement> GetSerializableValue();

		// Token: 0x0600575E RID: 22366 RVA: 0x002D3C48 File Offset: 0x002D1E48
		public virtual void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
		{
			RectTransform rectT = new RectTransform(relativeSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text = "";
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBox guitextBox = new GUITextBox(rectT, text, null, smallFont, Alignment.Left, false, "", null, false, true);
			guitextBox.Text = this.GetStringValue();
			guitextBox.OnTextChangedDelegate = delegate(GUITextBox box, string txt)
			{
				Action<string> onSerializedValue2 = onSerializedValue;
				if (onSerializedValue2 != null)
				{
					onSerializedValue2(txt);
				}
				return true;
			};
		}

		// Token: 0x04002CA8 RID: 11432
		private int _isDisposed;
	}
}

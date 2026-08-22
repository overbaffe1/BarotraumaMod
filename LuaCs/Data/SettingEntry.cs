using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Xna.Framework;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A0 RID: 1440
	public class SettingEntry<T> : SettingBase, ISettingBase<T>, ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable, INetworkSyncVar where T : IEquatable<T>, IConvertible
	{
		// Token: 0x0600575F RID: 22367 RVA: 0x002D3CD8 File Offset: 0x002D1ED8
		public SettingEntry(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo)
		{
			if (!typeof(T).IsEnum && !typeof(T).IsPrimitive && !(typeof(T) == typeof(string)))
			{
				ThrowHelper.ThrowArgumentException("ISettingBase: The type of T is not an allowed type.");
			}
			this.ValueChangePredicate = valueChangePredicate;
			try
			{
				this.Value = (T)((object)Convert.ChangeType(base.ConfigInfo.Element.GetAttributeString("Value", null), typeof(T)));
				this.DefaultValue = this.Value;
			}
			catch (Exception e) when (e is InvalidCastException || e is ArgumentNullException)
			{
				this.Value = default(T);
				this.DefaultValue = default(T);
			}
		}

		// Token: 0x170015BD RID: 5565
		// (get) Token: 0x06005760 RID: 22368 RVA: 0x002D3DD4 File Offset: 0x002D1FD4
		// (set) Token: 0x06005761 RID: 22369 RVA: 0x002D3DDC File Offset: 0x002D1FDC
		public T Value { get; protected set; }

		// Token: 0x170015BE RID: 5566
		// (get) Token: 0x06005762 RID: 22370 RVA: 0x002D3DE5 File Offset: 0x002D1FE5
		// (set) Token: 0x06005763 RID: 22371 RVA: 0x002D3DED File Offset: 0x002D1FED
		public T DefaultValue { get; protected set; }

		// Token: 0x06005764 RID: 22372 RVA: 0x002D3DF8 File Offset: 0x002D1FF8
		public virtual bool TrySetValue(T value)
		{
			if (value != null)
			{
				ref T ptr = ref value;
				if (default(T) == null)
				{
					T t = value;
					ptr = ref t;
				}
				if (!ptr.Equals(this.Value))
				{
					if (this.SyncType == NetSync.ServerAuthority && this.NetworkingService != null && GameMain.IsMultiplayer && GameMain.Client != null && !GameMain.Client.HasPermission(this.WritePermissions))
					{
						return false;
					}
					if (!this.TrySetValueInternal(value))
					{
						return false;
					}
					Action<ISettingBase> onValueChanged = this.OnValueChanged;
					if (onValueChanged != null)
					{
						onValueChanged(this);
					}
					bool isMultiplayer = GameMain.IsMultiplayer;
					bool flag = isMultiplayer;
					if (flag)
					{
						NetSync syncType = this.SyncType;
						bool flag2 = syncType == NetSync.TwoWay || syncType == NetSync.ClientOneWay;
						flag = flag2;
					}
					if (flag)
					{
						IEntityNetworkingService networkingService = this.NetworkingService;
						if (networkingService != null)
						{
							networkingService.SendNetVar(this);
						}
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06005765 RID: 22373 RVA: 0x002D3EC7 File Offset: 0x002D20C7
		private bool TrySetValueInternal(T value)
		{
			if (value == null)
			{
				return false;
			}
			if (this.ValueChangePredicate != null && !this.ValueChangePredicate(value))
			{
				return false;
			}
			this.Value = value;
			return true;
		}

		// Token: 0x06005766 RID: 22374 RVA: 0x002D3F00 File Offset: 0x002D2100
		private bool TrySetValueNetwork(T value)
		{
			if (this.NetworkingService == null)
			{
				return false;
			}
			NetSync syncType = this.SyncType;
			bool flag = syncType == NetSync.None || syncType == NetSync.ClientOneWay;
			if (flag)
			{
				return false;
			}
			if (!this.TrySetValueInternal(value))
			{
				return false;
			}
			Action<ISettingBase> onValueChanged = this.OnValueChanged;
			if (onValueChanged != null)
			{
				onValueChanged(this);
			}
			return true;
		}

		// Token: 0x06005767 RID: 22375 RVA: 0x002D3F4E File Offset: 0x002D214E
		protected override void OnDispose()
		{
			this.ValueChangePredicate = null;
			IEntityNetworkingService networkingService = this.NetworkingService;
			if (networkingService == null)
			{
				return;
			}
			networkingService.DeregisterNetVar(this);
		}

		// Token: 0x06005768 RID: 22376 RVA: 0x002D3F68 File Offset: 0x002D2168
		public override Type GetValueType()
		{
			return typeof(T);
		}

		// Token: 0x06005769 RID: 22377 RVA: 0x002D3F74 File Offset: 0x002D2174
		public override string GetStringValue()
		{
			T value = this.Value;
			return ((value != null) ? value.ToString() : null) ?? string.Empty;
		}

		// Token: 0x0600576A RID: 22378 RVA: 0x002D3FB0 File Offset: 0x002D21B0
		public override string GetDefaultStringValue()
		{
			T defaultValue = this.DefaultValue;
			return ((defaultValue != null) ? defaultValue.ToString() : null) ?? string.Empty;
		}

		// Token: 0x0600576B RID: 22379 RVA: 0x002D3FEC File Offset: 0x002D21EC
		public override bool TrySetSerializedValue(OneOf<string, XElement> value)
		{
			bool isFailed = false;
			T typeConvertedValue = value.Match<T>(delegate(string val)
			{
				T result;
				try
				{
					result = (T)((object)Convert.ChangeType(val, typeof(T)));
				}
				catch (Exception e)
				{
					isFailed = true;
					result = default(T);
				}
				return result;
			}, delegate(XElement val)
			{
				T result;
				try
				{
					result = (T)((object)Convert.ChangeType(val.GetAttributeString("Value", null), typeof(T)));
				}
				catch (Exception e)
				{
					isFailed = true;
					result = default(T);
				}
				return result;
			});
			return !isFailed && this.TrySetValue(typeConvertedValue);
		}

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x0600576C RID: 22380 RVA: 0x002D4038 File Offset: 0x002D2238
		// (remove) Token: 0x0600576D RID: 22381 RVA: 0x002D4070 File Offset: 0x002D2270
		public override event Action<ISettingBase> OnValueChanged;

		// Token: 0x0600576E RID: 22382 RVA: 0x002D40A8 File Offset: 0x002D22A8
		public override OneOf<string, XElement> GetSerializableValue()
		{
			T value = this.Value;
			return value.ToString();
		}

		// Token: 0x170015BF RID: 5567
		// (get) Token: 0x0600576F RID: 22383 RVA: 0x002D40CE File Offset: 0x002D22CE
		public Guid InstanceId
		{
			get
			{
				IEntityNetworkingService networkingService = this.NetworkingService;
				if (networkingService == null)
				{
					return Guid.Empty;
				}
				return networkingService.GetNetworkIdForInstance(this);
			}
		}

		// Token: 0x06005770 RID: 22384 RVA: 0x002D40E6 File Offset: 0x002D22E6
		public void SetNetworkOwner(IEntityNetworkingService networkingService)
		{
			this.NetworkingService = networkingService;
		}

		// Token: 0x170015C0 RID: 5568
		// (get) Token: 0x06005771 RID: 22385 RVA: 0x002D40EF File Offset: 0x002D22EF
		public NetSync SyncType
		{
			get
			{
				IConfigInfo configInfo = base.ConfigInfo;
				if (configInfo == null)
				{
					return NetSync.None;
				}
				return configInfo.NetSync;
			}
		}

		// Token: 0x170015C1 RID: 5569
		// (get) Token: 0x06005772 RID: 22386 RVA: 0x002D4102 File Offset: 0x002D2302
		public ClientPermissions WritePermissions
		{
			get
			{
				return ClientPermissions.ManageSettings;
			}
		}

		// Token: 0x06005773 RID: 22387 RVA: 0x002D410C File Offset: 0x002D230C
		public void ReadNetMessage(IReadMessage message)
		{
			if (this.SyncType == NetSync.None || this.NetworkingService == null)
			{
				return;
			}
			try
			{
				if (typeof(T).IsEnum)
				{
					this.TrySetValueInternal((T)((object)message.ReadInt32()));
				}
				TypeCode typeCode = Type.GetTypeCode(typeof(T));
				switch (typeCode)
				{
				case TypeCode.Boolean:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadBoolean(), typeCode)));
					return;
				case TypeCode.Char:
				case TypeCode.UInt16:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadUInt16(), typeCode)));
					return;
				case TypeCode.SByte:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadInt16(), typeCode)));
					return;
				case TypeCode.Byte:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadByte(), typeCode)));
					return;
				case TypeCode.Int16:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadInt16(), typeCode)));
					return;
				case TypeCode.Int32:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadInt32(), typeCode)));
					return;
				case TypeCode.UInt32:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadUInt32(), typeCode)));
					return;
				case TypeCode.Int64:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadInt64(), typeCode)));
					return;
				case TypeCode.UInt64:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadUInt64(), typeCode)));
					return;
				case TypeCode.Single:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadSingle(), typeCode)));
					return;
				case TypeCode.Double:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadDouble(), typeCode)));
					return;
				case TypeCode.String:
					this.TrySetValueNetwork((T)((object)Convert.ChangeType(message.ReadString(), typeCode)));
					return;
				}
				ThrowHelper.ThrowNotSupportedException("SettingEntry: The type " + typeof(T).Name + " is not supported.");
			}
			catch (Exception e)
			{
			}
		}

		// Token: 0x06005774 RID: 22388 RVA: 0x002D4380 File Offset: 0x002D2580
		public void WriteNetMessage(IWriteMessage message)
		{
			if (this.SyncType == NetSync.None || this.NetworkingService == null)
			{
				return;
			}
			try
			{
				if (typeof(T).IsEnum)
				{
					message.WriteInt32((int)((object)this.Value));
				}
				TypeCode typeCode = Type.GetTypeCode(typeof(T));
				switch (typeCode)
				{
				case TypeCode.Boolean:
					message.WriteBoolean((bool)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Char:
				case TypeCode.UInt16:
					message.WriteUInt16((ushort)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.SByte:
					message.WriteInt16((short)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Byte:
					message.WriteByte((byte)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Int16:
					message.WriteInt16((short)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Int32:
					message.WriteInt32((int)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.UInt32:
					message.WriteUInt32((uint)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Int64:
					message.WriteInt64((long)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.UInt64:
					message.WriteUInt64((ulong)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Single:
					message.WriteSingle((float)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.Double:
					message.WriteDouble((double)Convert.ChangeType(this.Value, typeCode));
					return;
				case TypeCode.String:
					message.WriteString((string)Convert.ChangeType(this.Value, typeCode));
					return;
				}
				ThrowHelper.ThrowNotSupportedException("SettingEntry: The type " + typeof(T).Name + " is not supported.");
			}
			catch (Exception e)
			{
			}
		}

		// Token: 0x06005775 RID: 22389 RVA: 0x002D45EC File Offset: 0x002D27EC
		public override void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue)
		{
			switch (Type.GetTypeCode(typeof(T)))
			{
			case TypeCode.Boolean:
			{
				GUITickBox guitickBox = new GUITickBox(new RectTransform(relativeSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, "");
				guitickBox.Selected = (bool)Convert.ChangeType(this.Value, TypeCode.Boolean);
				guitickBox.OnSelected = delegate(GUITickBox box)
				{
					Action<string> onSerializedValue2 = onSerializedValue;
					if (onSerializedValue2 != null)
					{
						onSerializedValue2(box.Selected.ToString());
					}
					return true;
				};
				return;
			}
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			{
				GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(relativeSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
				guinumberInput.IntValue = (int)Convert.ChangeType(this.Value, TypeCode.Int32);
				guinumberInput.OnValueChanged = delegate(GUINumberInput num)
				{
					Action<string> onSerializedValue2 = onSerializedValue;
					if (onSerializedValue2 == null)
					{
						return;
					}
					onSerializedValue2(num.IntValue.ToString());
				};
				return;
			}
			case TypeCode.Single:
			case TypeCode.Double:
			{
				GUINumberInput guinumberInput2 = new GUINumberInput(new RectTransform(relativeSize, layoutGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
				guinumberInput2.FloatValue = (float)Convert.ChangeType(this.Value, TypeCode.Single);
				guinumberInput2.OnValueChanged = delegate(GUINumberInput num)
				{
					Action<string> onSerializedValue2 = onSerializedValue;
					if (onSerializedValue2 == null)
					{
						return;
					}
					onSerializedValue2(num.FloatValue.ToString());
				};
				return;
			}
			}
			base.AddDisplayComponent(layoutGroup, relativeSize, onSerializedValue);
		}

		// Token: 0x04002CA9 RID: 11433
		protected Func<OneOf<string, XElement, object>, bool> ValueChangePredicate;

		// Token: 0x04002CAD RID: 11437
		protected IEntityNetworkingService NetworkingService;

		// Token: 0x02001388 RID: 5000
		public class Factory : ISettingBase.IFactory<ISettingBase<T>>
		{
			// Token: 0x060097AC RID: 38828 RVA: 0x003DC0E4 File Offset: 0x003DA2E4
			public ISettingBase<T> CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingEntry<T>(configInfo, valueChangePredicate);
			}
		}
	}
}

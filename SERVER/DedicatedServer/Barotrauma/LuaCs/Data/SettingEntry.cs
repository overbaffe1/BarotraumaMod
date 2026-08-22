using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Toolkit.Diagnostics;
using OneOf;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000484 RID: 1156
	public class SettingEntry<T> : SettingBase, ISettingBase<T>, ISettingBase, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable, INetworkSyncVar where T : IEquatable<T>, IConvertible
	{
		// Token: 0x06003DF4 RID: 15860 RVA: 0x0018EC1C File Offset: 0x0018CE1C
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

		// Token: 0x17001057 RID: 4183
		// (get) Token: 0x06003DF5 RID: 15861 RVA: 0x0018ED18 File Offset: 0x0018CF18
		// (set) Token: 0x06003DF6 RID: 15862 RVA: 0x0018ED20 File Offset: 0x0018CF20
		public T Value { get; protected set; }

		// Token: 0x17001058 RID: 4184
		// (get) Token: 0x06003DF7 RID: 15863 RVA: 0x0018ED29 File Offset: 0x0018CF29
		// (set) Token: 0x06003DF8 RID: 15864 RVA: 0x0018ED31 File Offset: 0x0018CF31
		public T DefaultValue { get; protected set; }

		// Token: 0x06003DF9 RID: 15865 RVA: 0x0018ED3C File Offset: 0x0018CF3C
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
					if (!this.TrySetValueInternal(value))
					{
						return false;
					}
					Action<ISettingBase> onValueChanged = this.OnValueChanged;
					if (onValueChanged != null)
					{
						onValueChanged(this);
					}
					NetSync syncType = this.SyncType;
					bool flag = syncType - NetSync.TwoWay <= 1;
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

		// Token: 0x06003DFA RID: 15866 RVA: 0x0018EDC6 File Offset: 0x0018CFC6
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

		// Token: 0x06003DFB RID: 15867 RVA: 0x0018EDFC File Offset: 0x0018CFFC
		private bool TrySetValueNetwork(T value)
		{
			if (this.NetworkingService == null)
			{
				return false;
			}
			NetSync syncType = this.SyncType;
			bool flag = syncType == NetSync.None || syncType == NetSync.ServerAuthority;
			if (flag)
			{
				return false;
			}
			if (!this.TrySetValueInternal(value))
			{
				return false;
			}
			if (this.SyncType == NetSync.TwoWay)
			{
				IEntityNetworkingService networkingService = this.NetworkingService;
				if (networkingService != null)
				{
					networkingService.SendNetVar(this);
				}
			}
			Action<ISettingBase> onValueChanged = this.OnValueChanged;
			if (onValueChanged != null)
			{
				onValueChanged(this);
			}
			return true;
		}

		// Token: 0x06003DFC RID: 15868 RVA: 0x0018EE65 File Offset: 0x0018D065
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

		// Token: 0x06003DFD RID: 15869 RVA: 0x0018EE7F File Offset: 0x0018D07F
		public override Type GetValueType()
		{
			return typeof(T);
		}

		// Token: 0x06003DFE RID: 15870 RVA: 0x0018EE8C File Offset: 0x0018D08C
		public override string GetStringValue()
		{
			T value = this.Value;
			return ((value != null) ? value.ToString() : null) ?? string.Empty;
		}

		// Token: 0x06003DFF RID: 15871 RVA: 0x0018EEC8 File Offset: 0x0018D0C8
		public override string GetDefaultStringValue()
		{
			T defaultValue = this.DefaultValue;
			return ((defaultValue != null) ? defaultValue.ToString() : null) ?? string.Empty;
		}

		// Token: 0x06003E00 RID: 15872 RVA: 0x0018EF04 File Offset: 0x0018D104
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

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x06003E01 RID: 15873 RVA: 0x0018EF50 File Offset: 0x0018D150
		// (remove) Token: 0x06003E02 RID: 15874 RVA: 0x0018EF88 File Offset: 0x0018D188
		public override event Action<ISettingBase> OnValueChanged;

		// Token: 0x06003E03 RID: 15875 RVA: 0x0018EFC0 File Offset: 0x0018D1C0
		public override OneOf<string, XElement> GetSerializableValue()
		{
			T value = this.Value;
			return value.ToString();
		}

		// Token: 0x17001059 RID: 4185
		// (get) Token: 0x06003E04 RID: 15876 RVA: 0x0018EFE6 File Offset: 0x0018D1E6
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

		// Token: 0x06003E05 RID: 15877 RVA: 0x0018EFFE File Offset: 0x0018D1FE
		public void SetNetworkOwner(IEntityNetworkingService networkingService)
		{
			this.NetworkingService = networkingService;
		}

		// Token: 0x1700105A RID: 4186
		// (get) Token: 0x06003E06 RID: 15878 RVA: 0x0018F007 File Offset: 0x0018D207
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

		// Token: 0x1700105B RID: 4187
		// (get) Token: 0x06003E07 RID: 15879 RVA: 0x0018F01A File Offset: 0x0018D21A
		public ClientPermissions WritePermissions
		{
			get
			{
				return ClientPermissions.ManageSettings;
			}
		}

		// Token: 0x06003E08 RID: 15880 RVA: 0x0018F024 File Offset: 0x0018D224
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

		// Token: 0x06003E09 RID: 15881 RVA: 0x0018F298 File Offset: 0x0018D498
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

		// Token: 0x04001DB8 RID: 7608
		protected Func<OneOf<string, XElement, object>, bool> ValueChangePredicate;

		// Token: 0x04001DBC RID: 7612
		protected IEntityNetworkingService NetworkingService;

		// Token: 0x02000D65 RID: 3429
		public class Factory : ISettingBase.IFactory<ISettingBase<T>>
		{
			// Token: 0x06006710 RID: 26384 RVA: 0x0021F8C1 File Offset: 0x0021DAC1
			public ISettingBase<T> CreateInstance(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
			{
				Guard.IsNotNull<IConfigInfo>(configInfo, "configInfo");
				return new SettingEntry<T>(configInfo, valueChangePredicate);
			}
		}
	}
}

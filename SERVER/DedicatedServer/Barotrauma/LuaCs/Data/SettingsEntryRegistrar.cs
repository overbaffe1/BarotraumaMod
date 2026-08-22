using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200048A RID: 1162
	public class SettingsEntryRegistrar : ISettingsRegistrationProvider, IService, IDisposable
	{
		// Token: 0x06003E1C RID: 15900 RVA: 0x0018F8D7 File Offset: 0x0018DAD7
		public SettingsEntryRegistrar(ILuaCsInfoProvider infoProvider)
		{
			this._infoProvider = infoProvider;
		}

		// Token: 0x06003E1D RID: 15901 RVA: 0x0018F8E8 File Offset: 0x0018DAE8
		public void RegisterTypeProviders(IConfigService configService, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
		{
			this.RegisterSettingEntry<bool>(configService, "bool", valueChangePredicate);
			this.RegisterSettingEntry<byte>(configService, "byte", valueChangePredicate);
			this.RegisterSettingEntry<sbyte>(configService, "sbyte", valueChangePredicate);
			this.RegisterSettingEntry<short>(configService, "short", valueChangePredicate);
			this.RegisterSettingEntry<ushort>(configService, "ushort", valueChangePredicate);
			this.RegisterSettingEntry<int>(configService, "int", valueChangePredicate);
			this.RegisterSettingEntry<uint>(configService, "uint", valueChangePredicate);
			this.RegisterSettingEntry<long>(configService, "long", valueChangePredicate);
			this.RegisterSettingEntry<ulong>(configService, "ulong", valueChangePredicate);
			this.RegisterSettingEntry<string>(configService, "string", valueChangePredicate);
			this.RegisterSettingEntry<float>(configService, "float", valueChangePredicate);
			this.RegisterSettingEntry<float>(configService, "single", valueChangePredicate);
			this.RegisterSettingEntry<double>(configService, "double", valueChangePredicate);
			configService.RegisterSettingTypeInitializer<SettingRangeInt>("rangeInt", ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingRangeInt.RangeFactory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
			configService.RegisterSettingTypeInitializer<SettingRangeFloat>("rangeFloat", ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingRangeFloat.RangeFactory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
			this.RegisterSettingList<bool>(configService, "listBool", valueChangePredicate);
			this.RegisterSettingList<byte>(configService, "listByte", valueChangePredicate);
			this.RegisterSettingList<sbyte>(configService, "listSbyte", valueChangePredicate);
			this.RegisterSettingList<short>(configService, "listShort", valueChangePredicate);
			this.RegisterSettingList<ushort>(configService, "listUshort", valueChangePredicate);
			this.RegisterSettingList<int>(configService, "listInt", valueChangePredicate);
			this.RegisterSettingList<uint>(configService, "listUint", valueChangePredicate);
			this.RegisterSettingList<long>(configService, "listLong", valueChangePredicate);
			this.RegisterSettingList<ulong>(configService, "listUlong", valueChangePredicate);
			this.RegisterSettingList<string>(configService, "listString", valueChangePredicate);
			this.RegisterSettingList<float>(configService, "listFloat", valueChangePredicate);
			this.RegisterSettingList<float>(configService, "listSingle", valueChangePredicate);
			this.RegisterSettingList<double>(configService, "listDouble", valueChangePredicate);
		}

		// Token: 0x06003E1E RID: 15902 RVA: 0x0018FB0C File Offset: 0x0018DD0C
		private void RegisterSettingList<T>(IConfigService configService, string typeName, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) where T : IEquatable<T>, IConvertible
		{
			configService.RegisterSettingTypeInitializer<ISettingList<T>>(typeName, ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingList<T>.LFactory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
		}

		// Token: 0x06003E1F RID: 15903 RVA: 0x0018FB40 File Offset: 0x0018DD40
		private void RegisterSettingEntry<T>(IConfigService configService, string typeName, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) where T : IEquatable<T>, IConvertible
		{
			configService.RegisterSettingTypeInitializer<ISettingBase<T>>(typeName, ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingEntry<T>.Factory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
		}

		// Token: 0x06003E20 RID: 15904 RVA: 0x0018FB74 File Offset: 0x0018DD74
		private bool IsValueChangeAllowed(IConfigInfo info, OneOf<string, XElement, object> newValue, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
		{
			return !info.Element.GetAttributeBool("ReadOnly", false);
		}

		// Token: 0x06003E21 RID: 15905 RVA: 0x0018FB8A File Offset: 0x0018DD8A
		public void Dispose()
		{
			if (!ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
			{
				return;
			}
			this._infoProvider.Dispose();
			this._infoProvider = null;
		}

		// Token: 0x17001061 RID: 4193
		// (get) Token: 0x06003E22 RID: 15906 RVA: 0x0018FBAC File Offset: 0x0018DDAC
		// (set) Token: 0x06003E23 RID: 15907 RVA: 0x0018FBB9 File Offset: 0x0018DDB9
		public bool IsDisposed
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

		// Token: 0x04001DC1 RID: 7617
		private ILuaCsInfoProvider _infoProvider;

		// Token: 0x04001DC2 RID: 7618
		private int _isDisposed;
	}
}

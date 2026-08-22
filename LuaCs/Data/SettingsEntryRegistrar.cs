using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A6 RID: 1446
	public class SettingsEntryRegistrar : ISettingsRegistrationProvider, IService, IDisposable
	{
		// Token: 0x0600578B RID: 22411 RVA: 0x002D4D05 File Offset: 0x002D2F05
		public SettingsEntryRegistrar(ILuaCsInfoProvider infoProvider)
		{
			this._infoProvider = infoProvider;
		}

		// Token: 0x0600578C RID: 22412 RVA: 0x002D4D14 File Offset: 0x002D2F14
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
			configService.RegisterSettingTypeInitializer<ISettingBase>("control", ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingControl.Factory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
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

		// Token: 0x0600578D RID: 22413 RVA: 0x002D4F50 File Offset: 0x002D3150
		private void RegisterSettingList<T>(IConfigService configService, string typeName, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) where T : IEquatable<T>, IConvertible
		{
			configService.RegisterSettingTypeInitializer<ISettingList<T>>(typeName, ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingList<T>.LFactory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
		}

		// Token: 0x0600578E RID: 22414 RVA: 0x002D4F84 File Offset: 0x002D3184
		private void RegisterSettingEntry<T>(IConfigService configService, string typeName, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) where T : IEquatable<T>, IConvertible
		{
			configService.RegisterSettingTypeInitializer<ISettingBase<T>>(typeName, ([TupleElementNames(new string[]
			{
				"ConfigService",
				"Info"
			})] ValueTuple<IConfigService, IConfigInfo> cfgInfo) => new SettingEntry<T>.Factory().CreateInstance(cfgInfo.Item2, (OneOf<string, XElement, object> val) => this.IsValueChangeAllowed(cfgInfo.Item2, val, valueChangePredicate)));
		}

		// Token: 0x0600578F RID: 22415 RVA: 0x002D4FB8 File Offset: 0x002D31B8
		private bool IsValueChangeAllowed(IConfigInfo info, OneOf<string, XElement, object> newValue, Func<OneOf<string, XElement, object>, bool> valueChangePredicate)
		{
			return !info.Element.GetAttributeBool("ReadOnly", false) || info.EditableStates < this._infoProvider.CurrentRunState || valueChangePredicate == null || valueChangePredicate(newValue);
		}

		// Token: 0x06005790 RID: 22416 RVA: 0x002D4FEC File Offset: 0x002D31EC
		public void Dispose()
		{
			if (!ModUtils.Threading.CheckIfClearAndSetBool(ref this._isDisposed))
			{
				return;
			}
			this._infoProvider.Dispose();
			this._infoProvider = null;
		}

		// Token: 0x170015C7 RID: 5575
		// (get) Token: 0x06005791 RID: 22417 RVA: 0x002D500E File Offset: 0x002D320E
		// (set) Token: 0x06005792 RID: 22418 RVA: 0x002D501B File Offset: 0x002D321B
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

		// Token: 0x04002CB2 RID: 11442
		private ILuaCsInfoProvider _infoProvider;

		// Token: 0x04002CB3 RID: 11443
		private int _isDisposed;
	}
}

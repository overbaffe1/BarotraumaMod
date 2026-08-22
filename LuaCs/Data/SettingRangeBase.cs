using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A2 RID: 1442
	public abstract class SettingRangeBase<T> : SettingEntry<T>, ISettingRangeBase<T>, ISettingBase<T>, ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable where T : IEquatable<T>, IConvertible
	{
		// Token: 0x0600577D RID: 22397 RVA: 0x002D4ACC File Offset: 0x002D2CCC
		public SettingRangeBase(IConfigInfo configInfo, Func<OneOf<string, XElement, object>, bool> valueChangePredicate) : base(configInfo, valueChangePredicate)
		{
		}

		// Token: 0x170015C4 RID: 5572
		// (get) Token: 0x0600577E RID: 22398 RVA: 0x002D4AD6 File Offset: 0x002D2CD6
		// (set) Token: 0x0600577F RID: 22399 RVA: 0x002D4ADE File Offset: 0x002D2CDE
		public T MinValue { get; protected set; }

		// Token: 0x170015C5 RID: 5573
		// (get) Token: 0x06005780 RID: 22400 RVA: 0x002D4AE7 File Offset: 0x002D2CE7
		// (set) Token: 0x06005781 RID: 22401 RVA: 0x002D4AEF File Offset: 0x002D2CEF
		public T MaxValue { get; protected set; }

		// Token: 0x170015C6 RID: 5574
		// (get) Token: 0x06005782 RID: 22402 RVA: 0x002D4AF8 File Offset: 0x002D2CF8
		// (set) Token: 0x06005783 RID: 22403 RVA: 0x002D4B00 File Offset: 0x002D2D00
		public int IncrementalSteps { get; protected set; }
	}
}

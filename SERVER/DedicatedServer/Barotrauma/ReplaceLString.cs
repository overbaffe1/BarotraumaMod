using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x020002A1 RID: 673
	[NullableContext(1)]
	[Nullable(0)]
	public class ReplaceLString : LocalizedString
	{
		// Token: 0x06002EC1 RID: 11969 RVA: 0x001386D0 File Offset: 0x001368D0
		public ReplaceLString(LocalizedString nStr, StringComparison sc, [TupleElementNames(new string[]
		{
			"Key",
			"Value",
			"FormatCapitals"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] IEnumerable<ValueTuple<LocalizedString, LocalizedString, FormatCapitals>> r)
		{
			this.nestedStr = nStr;
			this.replacements = (from kvf in r
			select new ValueTuple<LocalizedString, ValueTuple<LocalizedString, FormatCapitals>>(kvf.Item1, new ValueTuple<LocalizedString, FormatCapitals>(kvf.Item2, kvf.Item3))).ToImmutableDictionary<LocalizedString, ValueTuple<LocalizedString, FormatCapitals>>();
			this.stringComparison = sc;
		}

		// Token: 0x06002EC2 RID: 11970 RVA: 0x00138721 File Offset: 0x00136921
		public ReplaceLString(LocalizedString nStr, StringComparison sc, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<LocalizedString, LocalizedString>[] r) : this(nStr, sc, from kv in r
		select new ValueTuple<LocalizedString, LocalizedString, FormatCapitals>(kv.Item1, kv.Item2, FormatCapitals.No))
		{
		}

		// Token: 0x06002EC3 RID: 11971 RVA: 0x00138750 File Offset: 0x00136950
		public ReplaceLString(LocalizedString nStr, StringComparison sc, [TupleElementNames(new string[]
		{
			"Key",
			"Value",
			"FormatCapitals"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1
		})] IEnumerable<ValueTuple<Identifier, LocalizedString, FormatCapitals>> r) : this(nStr, sc, from p in r
		select new ValueTuple<LocalizedString, LocalizedString, FormatCapitals>(p.Item1.Value, p.Item2, p.Item3))
		{
		}

		// Token: 0x06002EC4 RID: 11972 RVA: 0x0013877F File Offset: 0x0013697F
		public ReplaceLString(LocalizedString nStr, StringComparison sc, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1
		})] params ValueTuple<Identifier, LocalizedString>[] r) : this(nStr, sc, from kv in r
		select new ValueTuple<LocalizedString, LocalizedString, FormatCapitals>(kv.Item1.Value, kv.Item2, FormatCapitals.No))
		{
		}

		// Token: 0x06002EC5 RID: 11973 RVA: 0x001387B0 File Offset: 0x001369B0
		private static string HandleVariableCapitalization(string text, string variableTag, string variableValue)
		{
			int index = text.IndexOf(variableTag, StringComparison.InvariantCulture) - 1;
			if (index == -1)
			{
				return variableValue;
			}
			for (int i = index; i >= 0; i--)
			{
				if (!char.IsWhiteSpace(text[i]))
				{
					if (text[i] == '.')
					{
						variableValue = variableValue.Capitalize().Value;
						break;
					}
					variableValue = variableValue.ToLowerInvariant();
				}
			}
			return variableValue;
		}

		// Token: 0x17000DA3 RID: 3491
		// (get) Token: 0x06002EC6 RID: 11974 RVA: 0x00138812 File Offset: 0x00136A12
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x06002EC7 RID: 11975 RVA: 0x00138820 File Offset: 0x00136A20
		public override void RetrieveValue()
		{
			this.cachedValue = this.nestedStr.Value;
			foreach (LocalizedString varName in this.replacements.Keys)
			{
				string key = varName.Value;
				string value = this.replacements[varName].Item1.Value;
				if (this.replacements[varName].Item2 == FormatCapitals.Yes)
				{
					value = ReplaceLString.HandleVariableCapitalization(this.cachedValue, key, value);
				}
				this.cachedValue = this.cachedValue.Replace(key, value, this.stringComparison);
			}
			base.UpdateLanguage();
		}

		// Token: 0x0400176C RID: 5996
		private readonly LocalizedString nestedStr;

		// Token: 0x0400176D RID: 5997
		[TupleElementNames(new string[]
		{
			"Value",
			"FormatCapitals"
		})]
		[Nullable(new byte[]
		{
			1,
			1,
			0,
			1
		})]
		private readonly ImmutableDictionary<LocalizedString, ValueTuple<LocalizedString, FormatCapitals>> replacements;

		// Token: 0x0400176E RID: 5998
		private readonly StringComparison stringComparison;
	}
}

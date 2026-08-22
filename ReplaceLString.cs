using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x0200036D RID: 877
	[NullableContext(1)]
	[Nullable(0)]
	public class ReplaceLString : LocalizedString
	{
		// Token: 0x06004356 RID: 17238 RVA: 0x00252B10 File Offset: 0x00250D10
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

		// Token: 0x06004357 RID: 17239 RVA: 0x00252B61 File Offset: 0x00250D61
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

		// Token: 0x06004358 RID: 17240 RVA: 0x00252B90 File Offset: 0x00250D90
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

		// Token: 0x06004359 RID: 17241 RVA: 0x00252BBF File Offset: 0x00250DBF
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

		// Token: 0x0600435A RID: 17242 RVA: 0x00252BF0 File Offset: 0x00250DF0
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

		// Token: 0x170011A5 RID: 4517
		// (get) Token: 0x0600435B RID: 17243 RVA: 0x00252C52 File Offset: 0x00250E52
		public override bool Loaded
		{
			get
			{
				return this.nestedStr.Loaded;
			}
		}

		// Token: 0x0600435C RID: 17244 RVA: 0x00252C60 File Offset: 0x00250E60
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

		// Token: 0x0400234C RID: 9036
		private readonly LocalizedString nestedStr;

		// Token: 0x0400234D RID: 9037
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

		// Token: 0x0400234E RID: 9038
		private readonly StringComparison stringComparison;
	}
}

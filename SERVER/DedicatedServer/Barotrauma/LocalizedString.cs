using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200029E RID: 670
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class LocalizedString : IComparable
	{
		// Token: 0x17000D9D RID: 3485
		// (get) Token: 0x06002E93 RID: 11923 RVA: 0x00138361 File Offset: 0x00136561
		// (set) Token: 0x06002E94 RID: 11924 RVA: 0x00138369 File Offset: 0x00136569
		public LanguageIdentifier Language { get; private set; } = LanguageIdentifier.None;

		// Token: 0x17000D9E RID: 3486
		// (get) Token: 0x06002E95 RID: 11925 RVA: 0x00138372 File Offset: 0x00136572
		public string Value
		{
			get
			{
				if (this.MustRetrieveValue())
				{
					this.RetrieveValue();
				}
				return this.cachedValue;
			}
		}

		// Token: 0x17000D9F RID: 3487
		// (get) Token: 0x06002E96 RID: 11926 RVA: 0x00138388 File Offset: 0x00136588
		public int Length
		{
			get
			{
				return this.Value.Length;
			}
		}

		// Token: 0x17000DA0 RID: 3488
		// (get) Token: 0x06002E97 RID: 11927
		public abstract bool Loaded { get; }

		// Token: 0x06002E98 RID: 11928 RVA: 0x00138395 File Offset: 0x00136595
		protected void UpdateLanguage()
		{
			this.Language = GameSettings.CurrentConfig.Language;
			this.languageVersion = TextManager.LanguageVersion;
		}

		// Token: 0x06002E99 RID: 11929 RVA: 0x001383B2 File Offset: 0x001365B2
		protected virtual bool MustRetrieveValue()
		{
			return this.Language != GameSettings.CurrentConfig.Language || this.languageVersion != TextManager.LanguageVersion;
		}

		// Token: 0x06002E9A RID: 11930 RVA: 0x001383DD File Offset: 0x001365DD
		protected static bool MustRetrieveValue(LocalizedString str)
		{
			return str.MustRetrieveValue();
		}

		// Token: 0x06002E9B RID: 11931
		public abstract void RetrieveValue();

		// Token: 0x06002E9C RID: 11932 RVA: 0x001383E5 File Offset: 0x001365E5
		public static implicit operator LocalizedString(string value)
		{
			if (value.IsNullOrEmpty())
			{
				return LocalizedString.EmptyString;
			}
			return new RawLString(value);
		}

		// Token: 0x06002E9D RID: 11933 RVA: 0x001383FB File Offset: 0x001365FB
		public static implicit operator LocalizedString(char value)
		{
			return new RawLString(value.ToString());
		}

		// Token: 0x06002E9E RID: 11934 RVA: 0x0013840C File Offset: 0x0013660C
		public static LocalizedString operator +(LocalizedString left, LocalizedString right)
		{
			if (left is RawLString)
			{
				string value = left.Value;
				if (value != null && value.Length == 0)
				{
					return right;
				}
			}
			if (right is RawLString)
			{
				string value = right.Value;
				if (value != null && value.Length == 0)
				{
					return left;
				}
			}
			return new ConcatLString(left, right);
		}

		// Token: 0x06002E9F RID: 11935 RVA: 0x00138458 File Offset: 0x00136658
		public static LocalizedString operator +(LocalizedString left, object right)
		{
			return left + (right.ToString() ?? "");
		}

		// Token: 0x06002EA0 RID: 11936 RVA: 0x00138474 File Offset: 0x00136674
		public static LocalizedString operator +(object left, LocalizedString right)
		{
			return (left.ToString() ?? "") + right;
		}

		// Token: 0x06002EA1 RID: 11937 RVA: 0x00138490 File Offset: 0x00136690
		[NullableContext(2)]
		public static bool operator ==(LocalizedString left, LocalizedString right)
		{
			return ((left != null) ? left.Value : null) == ((right != null) ? right.Value : null);
		}

		// Token: 0x06002EA2 RID: 11938 RVA: 0x001384AF File Offset: 0x001366AF
		[NullableContext(2)]
		public static bool operator !=(LocalizedString left, LocalizedString right)
		{
			return !(left == right);
		}

		// Token: 0x06002EA3 RID: 11939 RVA: 0x001384BB File Offset: 0x001366BB
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x06002EA4 RID: 11940 RVA: 0x001384C3 File Offset: 0x001366C3
		public bool Contains(string subStr, StringComparison comparison = StringComparison.Ordinal)
		{
			return !this.Value.IsNullOrEmpty() && this.Value.Contains(subStr, comparison);
		}

		// Token: 0x06002EA5 RID: 11941 RVA: 0x001384E1 File Offset: 0x001366E1
		public bool Contains(char chr, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.Value.Contains(chr, comparison);
		}

		// Token: 0x06002EA6 RID: 11942 RVA: 0x001384F0 File Offset: 0x001366F0
		public virtual LocalizedString ToUpper()
		{
			return new UpperLString(this);
		}

		// Token: 0x06002EA7 RID: 11943 RVA: 0x001384F8 File Offset: 0x001366F8
		public static LocalizedString Join(string separator, params LocalizedString[] subStrs)
		{
			return LocalizedString.Join(separator, subStrs);
		}

		// Token: 0x06002EA8 RID: 11944 RVA: 0x00138501 File Offset: 0x00136701
		public static LocalizedString Join(string separator, IEnumerable<LocalizedString> subStrs)
		{
			return new JoinLString(separator, subStrs);
		}

		// Token: 0x06002EA9 RID: 11945 RVA: 0x0013850A File Offset: 0x0013670A
		public LocalizedString Fallback(LocalizedString fallback, bool useDefaultLanguageIfFound = true)
		{
			return new FallbackLString(this, fallback, useDefaultLanguageIfFound);
		}

		// Token: 0x06002EAA RID: 11946 RVA: 0x00138514 File Offset: 0x00136714
		public IReadOnlyList<LocalizedString> Split(params char[] separators)
		{
			LStringSplitter splitter = new LStringSplitter(this, separators);
			return splitter.Substrings;
		}

		// Token: 0x06002EAB RID: 11947 RVA: 0x0013852F File Offset: 0x0013672F
		public LocalizedString Replace(Identifier find, LocalizedString replace, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new ReplaceLString(this, stringComparison, new ValueTuple<Identifier, LocalizedString>[]
			{
				new ValueTuple<Identifier, LocalizedString>(find, replace)
			});
		}

		// Token: 0x06002EAC RID: 11948 RVA: 0x0013854C File Offset: 0x0013674C
		public LocalizedString Replace(string find, LocalizedString replace, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new ReplaceLString(this, stringComparison, new ValueTuple<Identifier, LocalizedString>[]
			{
				new ValueTuple<Identifier, LocalizedString>(find.ToIdentifier(), replace)
			});
		}

		// Token: 0x06002EAD RID: 11949 RVA: 0x0013856E File Offset: 0x0013676E
		public LocalizedString Replace(LocalizedString find, LocalizedString replace, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new ReplaceLString(this, stringComparison, new ValueTuple<LocalizedString, LocalizedString>[]
			{
				new ValueTuple<LocalizedString, LocalizedString>(find, replace)
			});
		}

		// Token: 0x06002EAE RID: 11950 RVA: 0x0013858B File Offset: 0x0013678B
		public LocalizedString TrimStart()
		{
			return new TrimLString(this, TrimLString.Mode.Start, null);
		}

		// Token: 0x06002EAF RID: 11951 RVA: 0x00138595 File Offset: 0x00136795
		public LocalizedString TrimEnd()
		{
			return new TrimLString(this, TrimLString.Mode.End, null);
		}

		// Token: 0x06002EB0 RID: 11952 RVA: 0x0013859F File Offset: 0x0013679F
		public LocalizedString ToLower()
		{
			return new LowerLString(this);
		}

		// Token: 0x06002EB1 RID: 11953 RVA: 0x001385A8 File Offset: 0x001367A8
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			LocalizedString lStr = obj as LocalizedString;
			if (lStr != null)
			{
				return this.Equals(lStr, StringComparison.Ordinal);
			}
			string str = obj as string;
			if (str != null)
			{
				return this.Equals(str, StringComparison.Ordinal);
			}
			return base.Equals(obj);
		}

		// Token: 0x06002EB2 RID: 11954 RVA: 0x001385E2 File Offset: 0x001367E2
		public bool Equals(LocalizedString other, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.Equals(other.Value, comparison);
		}

		// Token: 0x06002EB3 RID: 11955 RVA: 0x001385F1 File Offset: 0x001367F1
		public bool Equals(string other, StringComparison comparison = StringComparison.Ordinal)
		{
			return string.Equals(this.Value, other, comparison);
		}

		// Token: 0x06002EB4 RID: 11956 RVA: 0x00138600 File Offset: 0x00136800
		public bool StartsWith(LocalizedString other, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.StartsWith(other.Value, comparison);
		}

		// Token: 0x06002EB5 RID: 11957 RVA: 0x0013860F File Offset: 0x0013680F
		public bool StartsWith(string other, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.Value.StartsWith(other, comparison);
		}

		// Token: 0x06002EB6 RID: 11958 RVA: 0x0013861E File Offset: 0x0013681E
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}

		// Token: 0x06002EB7 RID: 11959 RVA: 0x0013862B File Offset: 0x0013682B
		[NullableContext(2)]
		public int CompareTo(object obj)
		{
			return this.Value.CompareTo(((obj != null) ? obj.ToString() : null) ?? "");
		}

		// Token: 0x04001768 RID: 5992
		private int languageVersion;

		// Token: 0x04001769 RID: 5993
		protected string cachedValue = "";

		// Token: 0x0400176A RID: 5994
		public static readonly RawLString EmptyString = new RawLString("");

		// Token: 0x02000B29 RID: 2857
		[NullableContext(0)]
		protected enum LoadedSuccessfully
		{
			// Token: 0x040038BE RID: 14526
			Unknown,
			// Token: 0x040038BF RID: 14527
			No,
			// Token: 0x040038C0 RID: 14528
			Yes
		}
	}
}

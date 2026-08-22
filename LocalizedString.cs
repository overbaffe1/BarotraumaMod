using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200036A RID: 874
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class LocalizedString : IComparable
	{
		// Token: 0x1700119F RID: 4511
		// (get) Token: 0x06004328 RID: 17192 RVA: 0x002527A1 File Offset: 0x002509A1
		// (set) Token: 0x06004329 RID: 17193 RVA: 0x002527A9 File Offset: 0x002509A9
		public LanguageIdentifier Language { get; private set; } = LanguageIdentifier.None;

		// Token: 0x170011A0 RID: 4512
		// (get) Token: 0x0600432A RID: 17194 RVA: 0x002527B2 File Offset: 0x002509B2
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

		// Token: 0x170011A1 RID: 4513
		// (get) Token: 0x0600432B RID: 17195 RVA: 0x002527C8 File Offset: 0x002509C8
		public int Length
		{
			get
			{
				return this.Value.Length;
			}
		}

		// Token: 0x170011A2 RID: 4514
		// (get) Token: 0x0600432C RID: 17196
		public abstract bool Loaded { get; }

		// Token: 0x0600432D RID: 17197 RVA: 0x002527D5 File Offset: 0x002509D5
		protected void UpdateLanguage()
		{
			this.Language = GameSettings.CurrentConfig.Language;
			this.languageVersion = TextManager.LanguageVersion;
		}

		// Token: 0x0600432E RID: 17198 RVA: 0x002527F2 File Offset: 0x002509F2
		protected virtual bool MustRetrieveValue()
		{
			return this.Language != GameSettings.CurrentConfig.Language || this.languageVersion != TextManager.LanguageVersion;
		}

		// Token: 0x0600432F RID: 17199 RVA: 0x0025281D File Offset: 0x00250A1D
		protected static bool MustRetrieveValue(LocalizedString str)
		{
			return str.MustRetrieveValue();
		}

		// Token: 0x06004330 RID: 17200
		public abstract void RetrieveValue();

		// Token: 0x06004331 RID: 17201 RVA: 0x00252825 File Offset: 0x00250A25
		public static implicit operator LocalizedString(string value)
		{
			if (value.IsNullOrEmpty())
			{
				return LocalizedString.EmptyString;
			}
			return new RawLString(value);
		}

		// Token: 0x06004332 RID: 17202 RVA: 0x0025283B File Offset: 0x00250A3B
		public static implicit operator LocalizedString(char value)
		{
			return new RawLString(value.ToString());
		}

		// Token: 0x06004333 RID: 17203 RVA: 0x0025284C File Offset: 0x00250A4C
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

		// Token: 0x06004334 RID: 17204 RVA: 0x00252898 File Offset: 0x00250A98
		public static LocalizedString operator +(LocalizedString left, object right)
		{
			return left + (right.ToString() ?? "");
		}

		// Token: 0x06004335 RID: 17205 RVA: 0x002528B4 File Offset: 0x00250AB4
		public static LocalizedString operator +(object left, LocalizedString right)
		{
			return (left.ToString() ?? "") + right;
		}

		// Token: 0x06004336 RID: 17206 RVA: 0x002528D0 File Offset: 0x00250AD0
		[NullableContext(2)]
		public static bool operator ==(LocalizedString left, LocalizedString right)
		{
			return ((left != null) ? left.Value : null) == ((right != null) ? right.Value : null);
		}

		// Token: 0x06004337 RID: 17207 RVA: 0x002528EF File Offset: 0x00250AEF
		[NullableContext(2)]
		public static bool operator !=(LocalizedString left, LocalizedString right)
		{
			return !(left == right);
		}

		// Token: 0x06004338 RID: 17208 RVA: 0x002528FB File Offset: 0x00250AFB
		public override string ToString()
		{
			return this.Value;
		}

		// Token: 0x06004339 RID: 17209 RVA: 0x00252903 File Offset: 0x00250B03
		public bool Contains(string subStr, StringComparison comparison = StringComparison.Ordinal)
		{
			return !this.Value.IsNullOrEmpty() && this.Value.Contains(subStr, comparison);
		}

		// Token: 0x0600433A RID: 17210 RVA: 0x00252921 File Offset: 0x00250B21
		public bool Contains(char chr, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.Value.Contains(chr, comparison);
		}

		// Token: 0x0600433B RID: 17211 RVA: 0x00252930 File Offset: 0x00250B30
		public virtual LocalizedString ToUpper()
		{
			return new UpperLString(this);
		}

		// Token: 0x0600433C RID: 17212 RVA: 0x00252938 File Offset: 0x00250B38
		public static LocalizedString Join(string separator, params LocalizedString[] subStrs)
		{
			return LocalizedString.Join(separator, subStrs);
		}

		// Token: 0x0600433D RID: 17213 RVA: 0x00252941 File Offset: 0x00250B41
		public static LocalizedString Join(string separator, IEnumerable<LocalizedString> subStrs)
		{
			return new JoinLString(separator, subStrs);
		}

		// Token: 0x0600433E RID: 17214 RVA: 0x0025294A File Offset: 0x00250B4A
		public LocalizedString Fallback(LocalizedString fallback, bool useDefaultLanguageIfFound = true)
		{
			return new FallbackLString(this, fallback, useDefaultLanguageIfFound);
		}

		// Token: 0x0600433F RID: 17215 RVA: 0x00252954 File Offset: 0x00250B54
		public IReadOnlyList<LocalizedString> Split(params char[] separators)
		{
			LStringSplitter splitter = new LStringSplitter(this, separators);
			return splitter.Substrings;
		}

		// Token: 0x06004340 RID: 17216 RVA: 0x0025296F File Offset: 0x00250B6F
		public LocalizedString Replace(Identifier find, LocalizedString replace, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new ReplaceLString(this, stringComparison, new ValueTuple<Identifier, LocalizedString>[]
			{
				new ValueTuple<Identifier, LocalizedString>(find, replace)
			});
		}

		// Token: 0x06004341 RID: 17217 RVA: 0x0025298C File Offset: 0x00250B8C
		public LocalizedString Replace(string find, LocalizedString replace, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new ReplaceLString(this, stringComparison, new ValueTuple<Identifier, LocalizedString>[]
			{
				new ValueTuple<Identifier, LocalizedString>(find.ToIdentifier(), replace)
			});
		}

		// Token: 0x06004342 RID: 17218 RVA: 0x002529AE File Offset: 0x00250BAE
		public LocalizedString Replace(LocalizedString find, LocalizedString replace, StringComparison stringComparison = StringComparison.Ordinal)
		{
			return new ReplaceLString(this, stringComparison, new ValueTuple<LocalizedString, LocalizedString>[]
			{
				new ValueTuple<LocalizedString, LocalizedString>(find, replace)
			});
		}

		// Token: 0x06004343 RID: 17219 RVA: 0x002529CB File Offset: 0x00250BCB
		public LocalizedString TrimStart()
		{
			return new TrimLString(this, TrimLString.Mode.Start, null);
		}

		// Token: 0x06004344 RID: 17220 RVA: 0x002529D5 File Offset: 0x00250BD5
		public LocalizedString TrimEnd()
		{
			return new TrimLString(this, TrimLString.Mode.End, null);
		}

		// Token: 0x06004345 RID: 17221 RVA: 0x002529DF File Offset: 0x00250BDF
		public LocalizedString ToLower()
		{
			return new LowerLString(this);
		}

		// Token: 0x06004346 RID: 17222 RVA: 0x002529E8 File Offset: 0x00250BE8
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

		// Token: 0x06004347 RID: 17223 RVA: 0x00252A22 File Offset: 0x00250C22
		public bool Equals(LocalizedString other, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.Equals(other.Value, comparison);
		}

		// Token: 0x06004348 RID: 17224 RVA: 0x00252A31 File Offset: 0x00250C31
		public bool Equals(string other, StringComparison comparison = StringComparison.Ordinal)
		{
			return string.Equals(this.Value, other, comparison);
		}

		// Token: 0x06004349 RID: 17225 RVA: 0x00252A40 File Offset: 0x00250C40
		public bool StartsWith(LocalizedString other, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.StartsWith(other.Value, comparison);
		}

		// Token: 0x0600434A RID: 17226 RVA: 0x00252A4F File Offset: 0x00250C4F
		public bool StartsWith(string other, StringComparison comparison = StringComparison.Ordinal)
		{
			return this.Value.StartsWith(other, comparison);
		}

		// Token: 0x0600434B RID: 17227 RVA: 0x00252A5E File Offset: 0x00250C5E
		public override int GetHashCode()
		{
			return this.Value.GetHashCode();
		}

		// Token: 0x0600434C RID: 17228 RVA: 0x00252A6B File Offset: 0x00250C6B
		[NullableContext(2)]
		public int CompareTo(object obj)
		{
			return this.Value.CompareTo(((obj != null) ? obj.ToString() : null) ?? "");
		}

		// Token: 0x04002348 RID: 9032
		private int languageVersion;

		// Token: 0x04002349 RID: 9033
		protected string cachedValue = "";

		// Token: 0x0400234A RID: 9034
		public static readonly RawLString EmptyString = new RawLString("");

		// Token: 0x02001085 RID: 4229
		[NullableContext(0)]
		protected enum LoadedSuccessfully
		{
			// Token: 0x040058F2 RID: 22770
			Unknown,
			// Token: 0x040058F3 RID: 22771
			No,
			// Token: 0x040058F4 RID: 22772
			Yes
		}
	}
}

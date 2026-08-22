using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Unicode;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x020002AB RID: 683
	[NullableContext(1)]
	[Nullable(0)]
	public static class TextManager
	{
		// Token: 0x17000DB2 RID: 3506
		// (get) Token: 0x06002F06 RID: 12038 RVA: 0x001394B4 File Offset: 0x001376B4
		public static IEnumerable<LanguageIdentifier> AvailableLanguages
		{
			get
			{
				return TextManager.TextPacks.Keys;
			}
		}

		// Token: 0x17000DB3 RID: 3507
		// (get) Token: 0x06002F07 RID: 12039 RVA: 0x001394C0 File Offset: 0x001376C0
		// (set) Token: 0x06002F08 RID: 12040 RVA: 0x001394C7 File Offset: 0x001376C7
		public static int LanguageVersion { get; private set; } = 0;

		// Token: 0x06002F09 RID: 12041 RVA: 0x001394D0 File Offset: 0x001376D0
		[NullableContext(0)]
		private static ImmutableArray<Range<int>> UnicodeToIntRanges([Nullable(1)] params UnicodeRange[] ranges)
		{
			return (from r in ranges
			select new Range<int>(r.FirstCodePoint, r.FirstCodePoint + r.Length - 1) into r
			orderby r.Start
			select r).ToImmutableArray<Range<int>>();
		}

		// Token: 0x06002F0A RID: 12042 RVA: 0x0013952B File Offset: 0x0013772B
		public static TextManager.SpeciallyHandledCharCategory GetSpeciallyHandledCategories(LocalizedString text)
		{
			return TextManager.GetSpeciallyHandledCategories(text.Value);
		}

		// Token: 0x06002F0B RID: 12043 RVA: 0x00139538 File Offset: 0x00137738
		public static TextManager.SpeciallyHandledCharCategory GetSpeciallyHandledCategories(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return TextManager.SpeciallyHandledCharCategory.None;
			}
			object obj = TextManager.mutex;
			lock (obj)
			{
				TextManager.CachedCategory cachedCategory;
				if (TextManager.SpeciallyHandledCategoriesCache.TryGetValue(text, out cachedCategory))
				{
					TextManager.SpeciallyHandledCategoriesCache[text] = new TextManager.CachedCategory(cachedCategory.Category);
					return cachedCategory.Category;
				}
			}
			TextManager.SpeciallyHandledCharCategory retVal = TextManager.SpeciallyHandledCharCategory.None;
			foreach (char chr in text)
			{
				foreach (TextManager.SpeciallyHandledCharCategory category in TextManager.SpeciallyHandledCharCategories)
				{
					if (!retVal.AlreadyHasCategoryFlag(category))
					{
						for (int j = 0; j < TextManager.SpeciallyHandledCharacterRanges[category].Length; j++)
						{
							Range<int> range = TextManager.SpeciallyHandledCharacterRanges[category][j];
							if ((int)chr < range.Start)
							{
								break;
							}
							int num = (int)chr;
							if (range.Contains(num))
							{
								retVal |= category;
								break;
							}
						}
					}
				}
				if (retVal == TextManager.SpeciallyHandledCharCategory.All)
				{
					break;
				}
			}
			object obj2 = TextManager.mutex;
			lock (obj2)
			{
				TextManager.SpeciallyHandledCategoriesCache[text] = new TextManager.CachedCategory(retVal);
				TextManager.TrimSpeciallyHandledCategoriesCache();
			}
			return retVal;
		}

		// Token: 0x06002F0C RID: 12044 RVA: 0x001396A8 File Offset: 0x001378A8
		private static void TrimSpeciallyHandledCategoriesCache()
		{
			if (TextManager.SpeciallyHandledCategoriesCache.Count > 5000)
			{
				foreach (KeyValuePair<string, TextManager.CachedCategory> cachedVal in (from c in TextManager.SpeciallyHandledCategoriesCache
				orderby c.Value.LastAccessed
				select c).Take(2500).ToList<KeyValuePair<string, TextManager.CachedCategory>>())
				{
					TextManager.SpeciallyHandledCategoriesCache.Remove(cachedVal.Key);
				}
			}
		}

		// Token: 0x06002F0D RID: 12045 RVA: 0x0013974C File Offset: 0x0013794C
		public static bool IsCJK(LocalizedString text)
		{
			return TextManager.IsCJK(text.Value);
		}

		// Token: 0x06002F0E RID: 12046 RVA: 0x00139759 File Offset: 0x00137959
		public static bool IsCJK(string text)
		{
			return TextManager.GetSpeciallyHandledCategories(text).AlreadyHasCategoryFlag(TextManager.SpeciallyHandledCharCategory.CJK);
		}

		// Token: 0x06002F0F RID: 12047 RVA: 0x00139767 File Offset: 0x00137967
		private static bool AlreadyHasCategoryFlag(this TextManager.SpeciallyHandledCharCategory existingFlags, TextManager.SpeciallyHandledCharCategory categoryFlag)
		{
			return (existingFlags & categoryFlag) > TextManager.SpeciallyHandledCharCategory.None;
		}

		// Token: 0x06002F10 RID: 12048 RVA: 0x00139770 File Offset: 0x00137970
		public unsafe static void VerifyLanguageAvailable()
		{
			if (!TextManager.TextPacks.ContainsKey(GameSettings.CurrentConfig.Language))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find the language \"");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(GameSettings.CurrentConfig.Language);
				defaultInterpolatedStringHandler.AppendLiteral("\". Trying to switch to English...");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.Language = "English".ToLanguageIdentifier();
				GameSettings.SetCurrentConfig(config);
			}
		}

		// Token: 0x06002F11 RID: 12049 RVA: 0x001397F9 File Offset: 0x001379F9
		public static bool ContainsTag(string tag)
		{
			return TextManager.ContainsTag(tag.ToIdentifier());
		}

		// Token: 0x06002F12 RID: 12050 RVA: 0x00139808 File Offset: 0x00137A08
		public static bool ContainsTag(Identifier tag)
		{
			return TextManager.TextPacks[GameSettings.CurrentConfig.Language].Any((TextPack p) => p.Texts.ContainsKey(tag));
		}

		// Token: 0x06002F13 RID: 12051 RVA: 0x00139848 File Offset: 0x00137A48
		public static bool ContainsTag(Identifier tag, LanguageIdentifier language)
		{
			return TextManager.TextPacks[language].Any((TextPack p) => p.Texts.ContainsKey(tag));
		}

		// Token: 0x06002F14 RID: 12052 RVA: 0x0013987E File Offset: 0x00137A7E
		public static IEnumerable<string> GetAll(string tag)
		{
			return TextManager.GetAll(tag.ToIdentifier());
		}

		// Token: 0x06002F15 RID: 12053 RVA: 0x0013988C File Offset: 0x00137A8C
		public static IEnumerable<string> GetAll(Identifier tag)
		{
			List<TextPack.Text> allTexts = TextManager.TextPacks[GameSettings.CurrentConfig.Language].SelectMany(delegate(TextPack p)
			{
				ImmutableArray<TextPack.Text> value;
				if (!p.Texts.TryGetValue(tag, out value))
				{
					return Array.Empty<TextPack.Text>();
				}
				return value;
			}).ToList<TextPack.Text>();
			TextPack.Text firstOverride = allTexts.FirstOrDefault((TextPack.Text t) => t.IsOverride);
			if (firstOverride != default(TextPack.Text))
			{
				return from t in allTexts
				where t.IsOverride && t.TextPack == firstOverride.TextPack
				select t.String;
			}
			return from t in allTexts
			select t.String;
		}

		// Token: 0x06002F16 RID: 12054 RVA: 0x0013996E File Offset: 0x00137B6E
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public static IEnumerable<KeyValuePair<Identifier, string>> GetAllTagTextPairs()
		{
			return new TextManager.<GetAllTagTextPairs>d__31(-2);
		}

		// Token: 0x06002F17 RID: 12055 RVA: 0x00139977 File Offset: 0x00137B77
		public static IEnumerable<string> GetTextFiles()
		{
			return TextManager.GetTextFilesRecursive(Path.Combine(new string[]
			{
				"Content",
				"Texts"
			}));
		}

		// Token: 0x06002F18 RID: 12056 RVA: 0x00139999 File Offset: 0x00137B99
		private static IEnumerable<string> GetTextFilesRecursive(string directory)
		{
			TextManager.<GetTextFilesRecursive>d__33 <GetTextFilesRecursive>d__ = new TextManager.<GetTextFilesRecursive>d__33(-2);
			<GetTextFilesRecursive>d__.<>3__directory = directory;
			return <GetTextFilesRecursive>d__;
		}

		// Token: 0x06002F19 RID: 12057 RVA: 0x001399A9 File Offset: 0x00137BA9
		public static string GetTranslatedLanguageName(LanguageIdentifier languageIdentifier)
		{
			return TextManager.TextPacks[languageIdentifier].First<TextPack>().TranslatedName;
		}

		// Token: 0x06002F1A RID: 12058 RVA: 0x001399C0 File Offset: 0x00137BC0
		public static void LanguageChanged()
		{
			foreach (KeyValuePair<LanguageIdentifier, ImmutableList<TextPack>> keyValuePair in TextManager.TextPacks)
			{
				LanguageIdentifier languageIdentifier;
				ImmutableList<TextPack> immutableList;
				keyValuePair.Deconstruct(out languageIdentifier, out immutableList);
				LanguageIdentifier language = languageIdentifier;
				ImmutableList<TextPack> textPacks = immutableList;
				foreach (TextPack textPack in textPacks)
				{
					if (GameSettings.CurrentConfig.Language == language)
					{
						textPack.VerifyLoaded();
					}
					else
					{
						textPack.Unload();
					}
				}
			}
			TextManager.ClearCache();
		}

		// Token: 0x06002F1B RID: 12059 RVA: 0x00139A78 File Offset: 0x00137C78
		public static void ClearCache()
		{
			Dictionary<Identifier, WeakReference<TagLString>> obj = TextManager.cachedStrings;
			lock (obj)
			{
				TextManager.cachedStrings.Clear();
				TextManager.nonCacheableTags.Clear();
			}
		}

		// Token: 0x06002F1C RID: 12060 RVA: 0x00139AC8 File Offset: 0x00137CC8
		public static LocalizedString Get(params Identifier[] tags)
		{
			if (tags.Length == 1)
			{
				return TextManager.Get(tags[0]);
			}
			return new TagLString(tags);
		}

		// Token: 0x06002F1D RID: 12061 RVA: 0x00139AE4 File Offset: 0x00137CE4
		public static LocalizedString Get(Identifier tag)
		{
			TagLString str = null;
			Dictionary<Identifier, WeakReference<TagLString>> obj = TextManager.cachedStrings;
			lock (obj)
			{
				if (!TextManager.nonCacheableTags.Contains(tag))
				{
					WeakReference<TagLString> strRef;
					if (TextManager.cachedStrings.TryGetValue(tag, out strRef) && !strRef.TryGetTarget(out str))
					{
						TextManager.cachedStrings.Remove(tag);
					}
					if (str == null && TextManager.TextPacks.ContainsKey(GameSettings.CurrentConfig.Language))
					{
						int count = 0;
						foreach (TextPack pack in TextManager.TextPacks[GameSettings.CurrentConfig.Language])
						{
							ImmutableArray<TextPack.Text> texts;
							if (pack.Texts.TryGetValue(tag, out texts))
							{
								count += texts.Length;
								if (count > 1)
								{
									break;
								}
							}
						}
						if (count > 1)
						{
							TextManager.nonCacheableTags = TextManager.nonCacheableTags.Add(tag);
						}
						else
						{
							str = new TagLString(new Identifier[]
							{
								tag
							});
							TextManager.cachedStrings.Add(tag, new WeakReference<TagLString>(str));
						}
					}
				}
			}
			TagLString result;
			if ((result = str) == null)
			{
				result = new TagLString(new Identifier[]
				{
					tag
				});
			}
			return result;
		}

		// Token: 0x06002F1E RID: 12062 RVA: 0x00139C54 File Offset: 0x00137E54
		public static LocalizedString Get(string tag)
		{
			return TextManager.Get(tag.ToIdentifier());
		}

		// Token: 0x06002F1F RID: 12063 RVA: 0x00139C61 File Offset: 0x00137E61
		public static LocalizedString Get(params string[] tags)
		{
			return TextManager.Get(tags.ToIdentifiers());
		}

		// Token: 0x06002F20 RID: 12064 RVA: 0x00139C6E File Offset: 0x00137E6E
		public static LocalizedString AddPunctuation(char punctuationSymbol, params LocalizedString[] texts)
		{
			return new AddedPunctuationLString(punctuationSymbol, texts);
		}

		// Token: 0x06002F21 RID: 12065 RVA: 0x00139C77 File Offset: 0x00137E77
		public static LocalizedString GetFormatted(Identifier tag, params object[] args)
		{
			return TextManager.GetFormatted(new TagLString(new Identifier[]
			{
				tag
			}), args);
		}

		// Token: 0x06002F22 RID: 12066 RVA: 0x00139C94 File Offset: 0x00137E94
		public static LocalizedString GetFormatted(LocalizedString str, params object[] args)
		{
			LocalizedString[] argStrs = new LocalizedString[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				LocalizedString ls = args[i] as LocalizedString;
				if (ls != null)
				{
					argStrs[i] = ls;
				}
				else
				{
					argStrs[i] = new RawLString(args[i].ToString() ?? "");
				}
			}
			return new FormattedLString(str, argStrs);
		}

		// Token: 0x06002F23 RID: 12067 RVA: 0x00139CEA File Offset: 0x00137EEA
		public static string FormatServerMessage(string str)
		{
			return str + "~";
		}

		// Token: 0x06002F24 RID: 12068 RVA: 0x00139CF8 File Offset: 0x00137EF8
		public static string FormatServerMessage(string message, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<string, string>[] keysWithValues)
		{
			if (keysWithValues.Length == 0)
			{
				return TextManager.FormatServerMessage(message);
			}
			int startIndex = message.LastIndexOf('/') + 1;
			int endIndex = message.IndexOf('~', startIndex);
			if (endIndex == -1)
			{
				endIndex = message.Length - 1;
			}
			string textId = message.Substring(startIndex, endIndex - startIndex + 1);
			string[] prefixEntries = (from e in keysWithValues.Select(delegate([TupleElementNames(new string[]
			{
				"Key",
				"Value"
			})] ValueTuple<string, string> kv, int index)
			{
				if (kv.Item2.IndexOfAny(new char[]
				{
					'~',
					'/'
				}) != -1)
				{
					int kvStartIndex = kv.Item2.LastIndexOf('/') + 1;
					string str = kv.Item2.Substring(0, kvStartIndex);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 3);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted(textId);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendFormatted<int>(index);
					defaultInterpolatedStringHandler.AppendLiteral("]=");
					defaultInterpolatedStringHandler.AppendFormatted(kv.Item2.Substring(kvStartIndex));
					return str + defaultInterpolatedStringHandler.ToStringAndClear();
				}
				return null;
			})
			where e != null
			select e).ToArray<string>();
			return string.Join("", new string[]
			{
				(prefixEntries.Length != 0) ? (string.Join("/", prefixEntries) + "/") : "",
				message,
				string.Join("", keysWithValues.Select(delegate([TupleElementNames(new string[]
				{
					"Key",
					"Value"
				})] ValueTuple<string, string> kv, int index)
				{
					if (kv.Item2.IndexOfAny(new char[]
					{
						'~',
						'/'
					}) == -1)
					{
						return "~" + kv.Item1 + "=" + kv.Item2;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(5, 3);
					defaultInterpolatedStringHandler.AppendLiteral("~");
					defaultInterpolatedStringHandler.AppendFormatted(kv.Item1);
					defaultInterpolatedStringHandler.AppendLiteral("=[");
					defaultInterpolatedStringHandler.AppendFormatted(textId);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendFormatted<int>(index);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}))
			});
		}

		// Token: 0x06002F25 RID: 12069 RVA: 0x00139DDC File Offset: 0x00137FDC
		internal static string FormatServerMessageWithPronouns(CharacterInfo charInfo, string message, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<string, string>[] keysWithValues)
		{
			Identifier pronounCategory = charInfo.Prefab.Pronouns;
			ValueTuple<string, string>[] array = new ValueTuple<string, string>[6];
			int num = 0;
			string item = "[PronounLowercase]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Pronoun[");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(pronounCategory);
			defaultInterpolatedStringHandler.AppendLiteral("]Lowercase");
			array[num] = new ValueTuple<string, string>(item, charInfo.ReplaceVars(defaultInterpolatedStringHandler.ToStringAndClear()));
			int num2 = 1;
			string item2 = "[PronounUppercase]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Pronoun[");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(pronounCategory);
			defaultInterpolatedStringHandler2.AppendLiteral("]");
			array[num2] = new ValueTuple<string, string>(item2, charInfo.ReplaceVars(defaultInterpolatedStringHandler2.ToStringAndClear()));
			int num3 = 2;
			string item3 = "[PronounPossessiveLowercase]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(28, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("PronounPossessive[");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(pronounCategory);
			defaultInterpolatedStringHandler3.AppendLiteral("]Lowercase");
			array[num3] = new ValueTuple<string, string>(item3, charInfo.ReplaceVars(defaultInterpolatedStringHandler3.ToStringAndClear()));
			int num4 = 3;
			string item4 = "[PronounPossessiveUppercase]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(19, 1);
			defaultInterpolatedStringHandler4.AppendLiteral("PronounPossessive[");
			defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(pronounCategory);
			defaultInterpolatedStringHandler4.AppendLiteral("]");
			array[num4] = new ValueTuple<string, string>(item4, charInfo.ReplaceVars(defaultInterpolatedStringHandler4.ToStringAndClear()));
			int num5 = 4;
			string item5 = "[PronounReflexiveLowercase]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(27, 1);
			defaultInterpolatedStringHandler5.AppendLiteral("PronounReflexive[");
			defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(pronounCategory);
			defaultInterpolatedStringHandler5.AppendLiteral("]Lowercase");
			array[num5] = new ValueTuple<string, string>(item5, charInfo.ReplaceVars(defaultInterpolatedStringHandler5.ToStringAndClear()));
			int num6 = 5;
			string item6 = "[PronounReflexiveUppercase]";
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler6.AppendLiteral("PronounReflexive[");
			defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(pronounCategory);
			defaultInterpolatedStringHandler6.AppendLiteral("]");
			array[num6] = new ValueTuple<string, string>(item6, charInfo.ReplaceVars(defaultInterpolatedStringHandler6.ToStringAndClear()));
			ValueTuple<string, string>[] pronounKwv = array;
			return TextManager.FormatServerMessage(message, keysWithValues.Concat(pronounKwv).ToArray<ValueTuple<string, string>>());
		}

		// Token: 0x06002F26 RID: 12070 RVA: 0x00139FC0 File Offset: 0x001381C0
		public static string JoinServerMessages(string separator, string[] parts, string namePrefix = "part.")
		{
			return string.Join("/", new string[]
			{
				string.Join("/", parts.Select(delegate(string part, int index)
				{
					int partStart = part.LastIndexOf('/') + 1;
					if (partStart <= 0)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 3);
						defaultInterpolatedStringHandler.AppendLiteral("[");
						defaultInterpolatedStringHandler.AppendFormatted(namePrefix);
						defaultInterpolatedStringHandler.AppendFormatted<int>(index);
						defaultInterpolatedStringHandler.AppendLiteral("]=");
						defaultInterpolatedStringHandler.AppendFormatted(part.Substring(partStart));
						return defaultInterpolatedStringHandler.ToStringAndClear();
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(4, 4);
					defaultInterpolatedStringHandler2.AppendFormatted(part.Substring(0, partStart));
					defaultInterpolatedStringHandler2.AppendLiteral("/[");
					defaultInterpolatedStringHandler2.AppendFormatted(namePrefix);
					defaultInterpolatedStringHandler2.AppendFormatted<int>(index);
					defaultInterpolatedStringHandler2.AppendLiteral("]=");
					defaultInterpolatedStringHandler2.AppendFormatted(part.Substring(partStart));
					return defaultInterpolatedStringHandler2.ToStringAndClear();
				})),
				string.Join(separator, parts.Select(delegate(string part, int index)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
					defaultInterpolatedStringHandler.AppendLiteral("[");
					defaultInterpolatedStringHandler.AppendFormatted(namePrefix);
					defaultInterpolatedStringHandler.AppendFormatted<int>(index);
					defaultInterpolatedStringHandler.AppendLiteral("]");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}))
			});
		}

		// Token: 0x06002F27 RID: 12071 RVA: 0x0013A024 File Offset: 0x00138224
		public static LocalizedString ParseInputTypes(LocalizedString str, bool useColorHighlight = false)
		{
			return new InputTypeLString(str, useColorHighlight);
		}

		// Token: 0x06002F28 RID: 12072 RVA: 0x0013A02D File Offset: 0x0013822D
		public static LocalizedString GetWithVariable(string tag, string varName, LocalizedString value, FormatCapitals formatCapitals = FormatCapitals.No)
		{
			return TextManager.GetWithVariable(tag.ToIdentifier(), varName.ToIdentifier(), value, formatCapitals);
		}

		// Token: 0x06002F29 RID: 12073 RVA: 0x0013A042 File Offset: 0x00138242
		public static LocalizedString GetWithVariable(Identifier tag, Identifier varName, LocalizedString value, FormatCapitals formatCapitals = FormatCapitals.No)
		{
			return TextManager.GetWithVariables(tag, new ValueTuple<Identifier, LocalizedString>[]
			{
				new ValueTuple<Identifier, LocalizedString>(varName, value)
			});
		}

		// Token: 0x06002F2A RID: 12074 RVA: 0x0013A05E File Offset: 0x0013825E
		public static LocalizedString GetWithVariables(string tag, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<string, string>[] replacements)
		{
			return TextManager.GetWithVariables(tag.ToIdentifier(), from kv in replacements
			select new ValueTuple<Identifier, LocalizedString, FormatCapitals>(kv.Item1.ToIdentifier(), new RawLString(kv.Item2), FormatCapitals.No));
		}

		// Token: 0x06002F2B RID: 12075 RVA: 0x0013A090 File Offset: 0x00138290
		public static LocalizedString GetWithVariables(string tag, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] params ValueTuple<string, LocalizedString>[] replacements)
		{
			return TextManager.GetWithVariables(tag.ToIdentifier(), from kv in replacements
			select new ValueTuple<Identifier, LocalizedString, FormatCapitals>(kv.Item1.ToIdentifier(), kv.Item2, FormatCapitals.No));
		}

		// Token: 0x06002F2C RID: 12076 RVA: 0x0013A0C2 File Offset: 0x001382C2
		public static LocalizedString GetWithVariables(string tag, [TupleElementNames(new string[]
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
		})] params ValueTuple<string, LocalizedString, FormatCapitals>[] replacements)
		{
			return TextManager.GetWithVariables(tag.ToIdentifier(), from kv in replacements
			select new ValueTuple<Identifier, LocalizedString, FormatCapitals>(kv.Item1.ToIdentifier(), kv.Item2, kv.Item3));
		}

		// Token: 0x06002F2D RID: 12077 RVA: 0x0013A0F4 File Offset: 0x001382F4
		public static LocalizedString GetWithVariables(string tag, [TupleElementNames(new string[]
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
		})] params ValueTuple<string, string, FormatCapitals>[] replacements)
		{
			return TextManager.GetWithVariables(tag.ToIdentifier(), from kv in replacements
			select new ValueTuple<Identifier, LocalizedString, FormatCapitals>(kv.Item1.ToIdentifier(), new RawLString(kv.Item2), kv.Item3));
		}

		// Token: 0x06002F2E RID: 12078 RVA: 0x0013A126 File Offset: 0x00138326
		public static LocalizedString GetWithVariables(Identifier tag, [TupleElementNames(new string[]
		{
			"Key",
			"Value"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1
		})] params ValueTuple<Identifier, LocalizedString>[] replacements)
		{
			return TextManager.GetWithVariables(tag, from kv in replacements
			select new ValueTuple<Identifier, LocalizedString, FormatCapitals>(kv.Item1, kv.Item2, FormatCapitals.No));
		}

		// Token: 0x06002F2F RID: 12079 RVA: 0x0013A153 File Offset: 0x00138353
		public static LocalizedString GetWithVariables(Identifier tag, [Nullable(new byte[]
		{
			1,
			0,
			1
		})] IEnumerable<ValueTuple<Identifier, LocalizedString, FormatCapitals>> replacements)
		{
			return new ReplaceLString(new TagLString(new Identifier[]
			{
				tag
			}), StringComparison.OrdinalIgnoreCase, replacements);
		}

		// Token: 0x06002F30 RID: 12080 RVA: 0x0013A170 File Offset: 0x00138370
		public static void ConstructDescription(ref LocalizedString description, XElement descriptionElement, [Nullable(new byte[]
		{
			2,
			1,
			1
		})] Func<string, string> customTagReplacer = null)
		{
			Identifier tag = descriptionElement.GetAttributeIdentifier("tag", Identifier.Empty);
			LocalizedString extraDescriptionLine = TextManager.Get(tag).Fallback(tag.Value, true);
			foreach (XElement replaceElement in descriptionElement.Elements())
			{
				Identifier identifier = replaceElement.NameAsIdentifier();
				if (!(identifier != "replace"))
				{
					Identifier variableTag = replaceElement.GetAttributeIdentifier("tag", Identifier.Empty);
					LocalizedString replacementValue = string.Empty;
					if (customTagReplacer != null)
					{
						replacementValue = customTagReplacer(replaceElement.GetAttributeString("value", string.Empty));
					}
					if (replacementValue.IsNullOrWhiteSpace())
					{
						string[] replacementValues = replaceElement.GetAttributeStringArray("value", Array.Empty<string>(), true, false);
						for (int i = 0; i < replacementValues.Length; i++)
						{
							replacementValue += TextManager.Get(replacementValues[i]).Fallback(replacementValues[i], true);
							if (i < replacementValues.Length - 1)
							{
								replacementValue += ", ";
							}
						}
					}
					if (replaceElement.Attribute("color") != null)
					{
						string colorStr = replaceElement.GetAttributeString("color", "255,255,255,255");
						replacementValue = "‖color:" + colorStr + "‖" + replacementValue + "‖color:end‖";
					}
					extraDescriptionLine = extraDescriptionLine.Replace(variableTag, replacementValue, StringComparison.Ordinal);
				}
			}
			LocalizedString localizedString = description;
			if (!(localizedString is RawLString) || !(localizedString.Value == ""))
			{
				description += "\n";
			}
			description += extraDescriptionLine;
		}

		// Token: 0x06002F31 RID: 12081 RVA: 0x0013A354 File Offset: 0x00138554
		public static LocalizedString FormatCurrency(int amount, bool includeCurrencySymbol = true)
		{
			string valueString = string.Format(CultureInfo.InvariantCulture, "{0:N0}", amount);
			if (!includeCurrencySymbol)
			{
				return valueString;
			}
			return TextManager.GetWithVariable("currencyformat", "[credits]", valueString, FormatCapitals.No);
		}

		// Token: 0x06002F32 RID: 12082 RVA: 0x0013A397 File Offset: 0x00138597
		public static LocalizedString GetServerMessage(string serverMessage)
		{
			return new ServerMsgLString(serverMessage);
		}

		// Token: 0x06002F33 RID: 12083 RVA: 0x0013A39F File Offset: 0x0013859F
		public static LocalizedString Capitalize(this LocalizedString str)
		{
			return new CapitalizeLString(str);
		}

		// Token: 0x06002F34 RID: 12084 RVA: 0x0013A3A7 File Offset: 0x001385A7
		public static void IncrementLanguageVersion()
		{
			TextManager.LanguageVersion++;
			TextManager.ClearCache();
		}

		// Token: 0x04001793 RID: 6035
		public static bool DebugDraw;

		// Token: 0x04001794 RID: 6036
		public static readonly LanguageIdentifier DefaultLanguage = "English".ToLanguageIdentifier();

		// Token: 0x04001795 RID: 6037
		public static readonly ConcurrentDictionary<LanguageIdentifier, ImmutableList<TextPack>> TextPacks = new ConcurrentDictionary<LanguageIdentifier, ImmutableList<TextPack>>();

		// Token: 0x04001796 RID: 6038
		private static readonly Dictionary<Identifier, WeakReference<TagLString>> cachedStrings = new Dictionary<Identifier, WeakReference<TagLString>>();

		// Token: 0x04001797 RID: 6039
		private static ImmutableHashSet<Identifier> nonCacheableTags = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x04001799 RID: 6041
		private static readonly object mutex = new object();

		// Token: 0x0400179A RID: 6042
		[Nullable(0)]
		public static readonly ImmutableArray<TextManager.SpeciallyHandledCharCategory> SpeciallyHandledCharCategories = Enum.GetValues<TextManager.SpeciallyHandledCharCategory>().Where(delegate(TextManager.SpeciallyHandledCharCategory c)
		{
			bool flag = c == TextManager.SpeciallyHandledCharCategory.None || c == TextManager.SpeciallyHandledCharCategory.All;
			return !flag;
		}).ToImmutableArray<TextManager.SpeciallyHandledCharCategory>();

		// Token: 0x0400179B RID: 6043
		[Nullable(new byte[]
		{
			1,
			0,
			0
		})]
		private static readonly ImmutableDictionary<TextManager.SpeciallyHandledCharCategory, ImmutableArray<Range<int>>> SpeciallyHandledCharacterRanges = new ValueTuple<TextManager.SpeciallyHandledCharCategory, ImmutableArray<Range<int>>>[]
		{
			new ValueTuple<TextManager.SpeciallyHandledCharCategory, ImmutableArray<Range<int>>>(TextManager.SpeciallyHandledCharCategory.CJK, TextManager.UnicodeToIntRanges(new UnicodeRange[]
			{
				UnicodeRanges.HalfwidthandFullwidthForms,
				UnicodeRanges.HangulJamo,
				UnicodeRanges.HangulCompatibilityJamo,
				UnicodeRanges.CjkRadicalsSupplement,
				UnicodeRanges.CjkSymbolsandPunctuation,
				UnicodeRanges.EnclosedCjkLettersandMonths,
				UnicodeRanges.CjkCompatibility,
				UnicodeRanges.CjkUnifiedIdeographsExtensionA,
				UnicodeRanges.CjkUnifiedIdeographs,
				UnicodeRanges.HangulSyllables,
				UnicodeRanges.CjkCompatibilityForms,
				UnicodeRanges.BlockElements
			})),
			new ValueTuple<TextManager.SpeciallyHandledCharCategory, ImmutableArray<Range<int>>>(TextManager.SpeciallyHandledCharCategory.Japanese, TextManager.UnicodeToIntRanges(new UnicodeRange[]
			{
				UnicodeRanges.Hiragana,
				UnicodeRanges.Katakana
			})),
			new ValueTuple<TextManager.SpeciallyHandledCharCategory, ImmutableArray<Range<int>>>(TextManager.SpeciallyHandledCharCategory.Cyrillic, TextManager.UnicodeToIntRanges(new UnicodeRange[]
			{
				UnicodeRanges.Cyrillic,
				UnicodeRanges.CyrillicSupplement,
				UnicodeRanges.CyrillicExtendedA,
				UnicodeRanges.CyrillicExtendedB,
				UnicodeRanges.CyrillicExtendedC
			}))
		}.ToImmutableDictionary<TextManager.SpeciallyHandledCharCategory, ImmutableArray<Range<int>>>();

		// Token: 0x0400179C RID: 6044
		private const int SpeciallyHandledCategoriesCacheSize = 5000;

		// Token: 0x0400179D RID: 6045
		private static readonly Dictionary<string, TextManager.CachedCategory> SpeciallyHandledCategoriesCache = new Dictionary<string, TextManager.CachedCategory>();

		// Token: 0x02000B31 RID: 2865
		[NullableContext(0)]
		[Flags]
		public enum SpeciallyHandledCharCategory
		{
			// Token: 0x040038D4 RID: 14548
			None = 0,
			// Token: 0x040038D5 RID: 14549
			CJK = 1,
			// Token: 0x040038D6 RID: 14550
			Cyrillic = 2,
			// Token: 0x040038D7 RID: 14551
			Japanese = 4,
			// Token: 0x040038D8 RID: 14552
			All = 7
		}

		// Token: 0x02000B32 RID: 2866
		[NullableContext(0)]
		private readonly struct CachedCategory
		{
			// Token: 0x06005FFE RID: 24574 RVA: 0x00208EDD File Offset: 0x002070DD
			public CachedCategory(TextManager.SpeciallyHandledCharCategory category)
			{
				this.Category = category;
				this.LastAccessed = Timing.TotalTime;
			}

			// Token: 0x040038D9 RID: 14553
			public readonly TextManager.SpeciallyHandledCharCategory Category;

			// Token: 0x040038DA RID: 14554
			public readonly double LastAccessed;
		}
	}
}

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
	// Token: 0x02000377 RID: 887
	[NullableContext(1)]
	[Nullable(0)]
	public static class TextManager
	{
		// Token: 0x170011B5 RID: 4533
		// (get) Token: 0x0600439E RID: 17310 RVA: 0x002539F8 File Offset: 0x00251BF8
		public static IEnumerable<LanguageIdentifier> AvailableLanguages
		{
			get
			{
				return TextManager.TextPacks.Keys;
			}
		}

		// Token: 0x170011B6 RID: 4534
		// (get) Token: 0x0600439F RID: 17311 RVA: 0x00253A04 File Offset: 0x00251C04
		// (set) Token: 0x060043A0 RID: 17312 RVA: 0x00253A0B File Offset: 0x00251C0B
		public static int LanguageVersion { get; private set; } = 0;

		// Token: 0x060043A1 RID: 17313 RVA: 0x00253A14 File Offset: 0x00251C14
		[NullableContext(0)]
		private static ImmutableArray<Range<int>> UnicodeToIntRanges([Nullable(1)] params UnicodeRange[] ranges)
		{
			return (from r in ranges
			select new Range<int>(r.FirstCodePoint, r.FirstCodePoint + r.Length - 1) into r
			orderby r.Start
			select r).ToImmutableArray<Range<int>>();
		}

		// Token: 0x060043A2 RID: 17314 RVA: 0x00253A6F File Offset: 0x00251C6F
		public static TextManager.SpeciallyHandledCharCategory GetSpeciallyHandledCategories(LocalizedString text)
		{
			return TextManager.GetSpeciallyHandledCategories(text.Value);
		}

		// Token: 0x060043A3 RID: 17315 RVA: 0x00253A7C File Offset: 0x00251C7C
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

		// Token: 0x060043A4 RID: 17316 RVA: 0x00253BEC File Offset: 0x00251DEC
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

		// Token: 0x060043A5 RID: 17317 RVA: 0x00253C90 File Offset: 0x00251E90
		public static bool IsCJK(LocalizedString text)
		{
			return TextManager.IsCJK(text.Value);
		}

		// Token: 0x060043A6 RID: 17318 RVA: 0x00253C9D File Offset: 0x00251E9D
		public static bool IsCJK(string text)
		{
			return TextManager.GetSpeciallyHandledCategories(text).AlreadyHasCategoryFlag(TextManager.SpeciallyHandledCharCategory.CJK);
		}

		// Token: 0x060043A7 RID: 17319 RVA: 0x00253CAB File Offset: 0x00251EAB
		private static bool AlreadyHasCategoryFlag(this TextManager.SpeciallyHandledCharCategory existingFlags, TextManager.SpeciallyHandledCharCategory categoryFlag)
		{
			return (existingFlags & categoryFlag) > TextManager.SpeciallyHandledCharCategory.None;
		}

		// Token: 0x060043A8 RID: 17320 RVA: 0x00253CB4 File Offset: 0x00251EB4
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

		// Token: 0x060043A9 RID: 17321 RVA: 0x00253D3D File Offset: 0x00251F3D
		public static bool ContainsTag(string tag)
		{
			return TextManager.ContainsTag(tag.ToIdentifier());
		}

		// Token: 0x060043AA RID: 17322 RVA: 0x00253D4C File Offset: 0x00251F4C
		public static bool ContainsTag(Identifier tag)
		{
			return TextManager.TextPacks[GameSettings.CurrentConfig.Language].Any((TextPack p) => p.Texts.ContainsKey(tag));
		}

		// Token: 0x060043AB RID: 17323 RVA: 0x00253D8C File Offset: 0x00251F8C
		public static bool ContainsTag(Identifier tag, LanguageIdentifier language)
		{
			return TextManager.TextPacks[language].Any((TextPack p) => p.Texts.ContainsKey(tag));
		}

		// Token: 0x060043AC RID: 17324 RVA: 0x00253DC2 File Offset: 0x00251FC2
		public static IEnumerable<string> GetAll(string tag)
		{
			return TextManager.GetAll(tag.ToIdentifier());
		}

		// Token: 0x060043AD RID: 17325 RVA: 0x00253DD0 File Offset: 0x00251FD0
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

		// Token: 0x060043AE RID: 17326 RVA: 0x00253EB2 File Offset: 0x002520B2
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

		// Token: 0x060043AF RID: 17327 RVA: 0x00253EBB File Offset: 0x002520BB
		public static IEnumerable<string> GetTextFiles()
		{
			return TextManager.GetTextFilesRecursive(Path.Combine(new string[]
			{
				"Content",
				"Texts"
			}));
		}

		// Token: 0x060043B0 RID: 17328 RVA: 0x00253EDD File Offset: 0x002520DD
		private static IEnumerable<string> GetTextFilesRecursive(string directory)
		{
			TextManager.<GetTextFilesRecursive>d__33 <GetTextFilesRecursive>d__ = new TextManager.<GetTextFilesRecursive>d__33(-2);
			<GetTextFilesRecursive>d__.<>3__directory = directory;
			return <GetTextFilesRecursive>d__;
		}

		// Token: 0x060043B1 RID: 17329 RVA: 0x00253EED File Offset: 0x002520ED
		public static string GetTranslatedLanguageName(LanguageIdentifier languageIdentifier)
		{
			return TextManager.TextPacks[languageIdentifier].First<TextPack>().TranslatedName;
		}

		// Token: 0x060043B2 RID: 17330 RVA: 0x00253F04 File Offset: 0x00252104
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

		// Token: 0x060043B3 RID: 17331 RVA: 0x00253FBC File Offset: 0x002521BC
		public static void ClearCache()
		{
			Dictionary<Identifier, WeakReference<TagLString>> obj = TextManager.cachedStrings;
			lock (obj)
			{
				TextManager.cachedStrings.Clear();
				TextManager.nonCacheableTags.Clear();
			}
		}

		// Token: 0x060043B4 RID: 17332 RVA: 0x0025400C File Offset: 0x0025220C
		public static LocalizedString Get(params Identifier[] tags)
		{
			if (tags.Length == 1)
			{
				return TextManager.Get(tags[0]);
			}
			return new TagLString(tags);
		}

		// Token: 0x060043B5 RID: 17333 RVA: 0x00254028 File Offset: 0x00252228
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

		// Token: 0x060043B6 RID: 17334 RVA: 0x00254198 File Offset: 0x00252398
		public static LocalizedString Get(string tag)
		{
			return TextManager.Get(tag.ToIdentifier());
		}

		// Token: 0x060043B7 RID: 17335 RVA: 0x002541A5 File Offset: 0x002523A5
		public static LocalizedString Get(params string[] tags)
		{
			return TextManager.Get(tags.ToIdentifiers());
		}

		// Token: 0x060043B8 RID: 17336 RVA: 0x002541B2 File Offset: 0x002523B2
		public static LocalizedString AddPunctuation(char punctuationSymbol, params LocalizedString[] texts)
		{
			return new AddedPunctuationLString(punctuationSymbol, texts);
		}

		// Token: 0x060043B9 RID: 17337 RVA: 0x002541BB File Offset: 0x002523BB
		public static LocalizedString GetFormatted(Identifier tag, params object[] args)
		{
			return TextManager.GetFormatted(new TagLString(new Identifier[]
			{
				tag
			}), args);
		}

		// Token: 0x060043BA RID: 17338 RVA: 0x002541D8 File Offset: 0x002523D8
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

		// Token: 0x060043BB RID: 17339 RVA: 0x0025422E File Offset: 0x0025242E
		public static string FormatServerMessage(string str)
		{
			return str + "~";
		}

		// Token: 0x060043BC RID: 17340 RVA: 0x0025423C File Offset: 0x0025243C
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

		// Token: 0x060043BD RID: 17341 RVA: 0x00254320 File Offset: 0x00252520
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

		// Token: 0x060043BE RID: 17342 RVA: 0x00254504 File Offset: 0x00252704
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

		// Token: 0x060043BF RID: 17343 RVA: 0x00254568 File Offset: 0x00252768
		public static LocalizedString ParseInputTypes(LocalizedString str, bool useColorHighlight = false)
		{
			return new InputTypeLString(str, useColorHighlight);
		}

		// Token: 0x060043C0 RID: 17344 RVA: 0x00254571 File Offset: 0x00252771
		public static LocalizedString GetWithVariable(string tag, string varName, LocalizedString value, FormatCapitals formatCapitals = FormatCapitals.No)
		{
			return TextManager.GetWithVariable(tag.ToIdentifier(), varName.ToIdentifier(), value, formatCapitals);
		}

		// Token: 0x060043C1 RID: 17345 RVA: 0x00254586 File Offset: 0x00252786
		public static LocalizedString GetWithVariable(Identifier tag, Identifier varName, LocalizedString value, FormatCapitals formatCapitals = FormatCapitals.No)
		{
			return TextManager.GetWithVariables(tag, new ValueTuple<Identifier, LocalizedString>[]
			{
				new ValueTuple<Identifier, LocalizedString>(varName, value)
			});
		}

		// Token: 0x060043C2 RID: 17346 RVA: 0x002545A2 File Offset: 0x002527A2
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

		// Token: 0x060043C3 RID: 17347 RVA: 0x002545D4 File Offset: 0x002527D4
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

		// Token: 0x060043C4 RID: 17348 RVA: 0x00254606 File Offset: 0x00252806
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

		// Token: 0x060043C5 RID: 17349 RVA: 0x00254638 File Offset: 0x00252838
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

		// Token: 0x060043C6 RID: 17350 RVA: 0x0025466A File Offset: 0x0025286A
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

		// Token: 0x060043C7 RID: 17351 RVA: 0x00254697 File Offset: 0x00252897
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

		// Token: 0x060043C8 RID: 17352 RVA: 0x002546B4 File Offset: 0x002528B4
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

		// Token: 0x060043C9 RID: 17353 RVA: 0x00254898 File Offset: 0x00252A98
		public static LocalizedString FormatCurrency(int amount, bool includeCurrencySymbol = true)
		{
			string valueString = string.Format(CultureInfo.InvariantCulture, "{0:N0}", amount);
			if (!includeCurrencySymbol)
			{
				return valueString;
			}
			return TextManager.GetWithVariable("currencyformat", "[credits]", valueString, FormatCapitals.No);
		}

		// Token: 0x060043CA RID: 17354 RVA: 0x002548DB File Offset: 0x00252ADB
		public static LocalizedString GetServerMessage(string serverMessage)
		{
			return new ServerMsgLString(serverMessage);
		}

		// Token: 0x060043CB RID: 17355 RVA: 0x002548E3 File Offset: 0x00252AE3
		public static LocalizedString Capitalize(this LocalizedString str)
		{
			return new CapitalizeLString(str);
		}

		// Token: 0x060043CC RID: 17356 RVA: 0x002548EB File Offset: 0x00252AEB
		public static void IncrementLanguageVersion()
		{
			TextManager.LanguageVersion++;
			TextManager.ClearCache();
		}

		// Token: 0x04002376 RID: 9078
		public static bool DebugDraw;

		// Token: 0x04002377 RID: 9079
		public static readonly LanguageIdentifier DefaultLanguage = "English".ToLanguageIdentifier();

		// Token: 0x04002378 RID: 9080
		public static readonly ConcurrentDictionary<LanguageIdentifier, ImmutableList<TextPack>> TextPacks = new ConcurrentDictionary<LanguageIdentifier, ImmutableList<TextPack>>();

		// Token: 0x04002379 RID: 9081
		private static readonly Dictionary<Identifier, WeakReference<TagLString>> cachedStrings = new Dictionary<Identifier, WeakReference<TagLString>>();

		// Token: 0x0400237A RID: 9082
		private static ImmutableHashSet<Identifier> nonCacheableTags = ImmutableHashSet<Identifier>.Empty;

		// Token: 0x0400237C RID: 9084
		private static readonly object mutex = new object();

		// Token: 0x0400237D RID: 9085
		[Nullable(0)]
		public static readonly ImmutableArray<TextManager.SpeciallyHandledCharCategory> SpeciallyHandledCharCategories = Enum.GetValues<TextManager.SpeciallyHandledCharCategory>().Where(delegate(TextManager.SpeciallyHandledCharCategory c)
		{
			bool flag = c == TextManager.SpeciallyHandledCharCategory.None || c == TextManager.SpeciallyHandledCharCategory.All;
			return !flag;
		}).ToImmutableArray<TextManager.SpeciallyHandledCharCategory>();

		// Token: 0x0400237E RID: 9086
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

		// Token: 0x0400237F RID: 9087
		private const int SpeciallyHandledCategoriesCacheSize = 5000;

		// Token: 0x04002380 RID: 9088
		private static readonly Dictionary<string, TextManager.CachedCategory> SpeciallyHandledCategoriesCache = new Dictionary<string, TextManager.CachedCategory>();

		// Token: 0x0200108D RID: 4237
		[NullableContext(0)]
		[Flags]
		public enum SpeciallyHandledCharCategory
		{
			// Token: 0x04005908 RID: 22792
			None = 0,
			// Token: 0x04005909 RID: 22793
			CJK = 1,
			// Token: 0x0400590A RID: 22794
			Cyrillic = 2,
			// Token: 0x0400590B RID: 22795
			Japanese = 4,
			// Token: 0x0400590C RID: 22796
			All = 7
		}

		// Token: 0x0200108E RID: 4238
		[NullableContext(0)]
		private readonly struct CachedCategory
		{
			// Token: 0x06008D06 RID: 36102 RVA: 0x003B0CAD File Offset: 0x003AEEAD
			public CachedCategory(TextManager.SpeciallyHandledCharCategory category)
			{
				this.Category = category;
				this.LastAccessed = Timing.TotalTime;
			}

			// Token: 0x0400590D RID: 22797
			public readonly TextManager.SpeciallyHandledCharCategory Category;

			// Token: 0x0400590E RID: 22798
			public readonly double LastAccessed;
		}
	}
}

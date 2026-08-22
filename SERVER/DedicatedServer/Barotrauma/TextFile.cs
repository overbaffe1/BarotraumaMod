using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000154 RID: 340
	[NotSyncedInMultiplayer]
	public sealed class TextFile : ContentFile
	{
		// Token: 0x06001C76 RID: 7286 RVA: 0x000CF063 File Offset: 0x000CD263
		public TextFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C77 RID: 7287 RVA: 0x000CF070 File Offset: 0x000CD270
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			ContentXElement mainElement = doc.Root.FromPackage(this.ContentPackage);
			Identifier languageName = mainElement.GetAttributeIdentifier("language", TextManager.DefaultLanguage.Value);
			LanguageIdentifier language = languageName.ToLanguageIdentifier();
			if (!TextManager.TextPacks.ContainsKey(language))
			{
				TextManager.TextPacks.TryAdd(language, ImmutableList<TextPack>.Empty);
			}
			TextPack newPack = new TextPack(this, mainElement, language, language == GameSettings.CurrentConfig.Language);
			ImmutableList<TextPack> newList = TextManager.TextPacks[language].Add(newPack);
			ImmutableList<TextPack> immutableList;
			TextManager.TextPacks.TryRemove(language, out immutableList);
			TextManager.TextPacks.TryAdd(language, newList);
			TextManager.IncrementLanguageVersion();
		}

		// Token: 0x06001C78 RID: 7288 RVA: 0x000CF128 File Offset: 0x000CD328
		public unsafe override void UnloadFile()
		{
			foreach (KeyValuePair<LanguageIdentifier, ImmutableList<TextPack>> kvp in TextManager.TextPacks.ToArray())
			{
				ImmutableList<TextPack> newList = (from p in kvp.Value
				where p.ContentFile != this
				select p).ToImmutableList<TextPack>();
				ImmutableList<TextPack> immutableList;
				TextManager.TextPacks.TryRemove(kvp.Key, out immutableList);
				if (newList.Count != 0)
				{
					TextManager.TextPacks.TryAdd(kvp.Key, newList);
				}
			}
			TextManager.IncrementLanguageVersion();
			if (!TextManager.TextPacks.ContainsKey(GameSettings.CurrentConfig.Language) && GameSettings.CurrentConfig.Language != TextManager.DefaultLanguage)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 2);
				defaultInterpolatedStringHandler.AppendLiteral("The language ");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(GameSettings.CurrentConfig.Language);
				defaultInterpolatedStringHandler.AppendLiteral(" is no longer available. Switching to ");
				defaultInterpolatedStringHandler.AppendFormatted<LanguageIdentifier>(TextManager.DefaultLanguage);
				defaultInterpolatedStringHandler.AppendLiteral("...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.Language = TextManager.DefaultLanguage;
				GameSettings.SetCurrentConfig(config);
			}
		}

		// Token: 0x06001C79 RID: 7289 RVA: 0x000CF250 File Offset: 0x000CD450
		public override void Sort()
		{
			foreach (LanguageIdentifier language in TextManager.TextPacks.Keys.ToList<LanguageIdentifier>())
			{
				TextManager.TextPacks[language] = TextManager.TextPacks[language].Sort(delegate(TextPack t1, TextPack t2)
				{
					ContentPackage contentPackage = t1.ContentFile.ContentPackage;
					int num = (contentPackage != null) ? contentPackage.Index : int.MaxValue;
					ContentPackage contentPackage2 = t2.ContentFile.ContentPackage;
					return num - ((contentPackage2 != null) ? contentPackage2.Index : int.MaxValue);
				});
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200024A RID: 586
	[NotSyncedInMultiplayer]
	public sealed class TextFile : ContentFile
	{
		// Token: 0x0600375F RID: 14175 RVA: 0x00214C27 File Offset: 0x00212E27
		public TextFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003760 RID: 14176 RVA: 0x00214C34 File Offset: 0x00212E34
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

		// Token: 0x06003761 RID: 14177 RVA: 0x00214CEC File Offset: 0x00212EEC
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

		// Token: 0x06003762 RID: 14178 RVA: 0x00214E14 File Offset: 0x00213014
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

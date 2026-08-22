using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000356 RID: 854
	[NullableContext(1)]
	[Nullable(0)]
	public static class CreatureMetrics
	{
		// Token: 0x1700118D RID: 4493
		// (get) Token: 0x060042D2 RID: 17106 RVA: 0x0024F701 File Offset: 0x0024D901
		// (set) Token: 0x060042D3 RID: 17107 RVA: 0x0024F708 File Offset: 0x0024D908
		public static HashSet<Identifier> RecentlyEncountered { get; private set; } = new HashSet<Identifier>();

		// Token: 0x1700118E RID: 4494
		// (get) Token: 0x060042D4 RID: 17108 RVA: 0x0024F710 File Offset: 0x0024D910
		// (set) Token: 0x060042D5 RID: 17109 RVA: 0x0024F717 File Offset: 0x0024D917
		public static HashSet<Identifier> Encountered { get; private set; } = new HashSet<Identifier>();

		// Token: 0x1700118F RID: 4495
		// (get) Token: 0x060042D6 RID: 17110 RVA: 0x0024F71F File Offset: 0x0024D91F
		// (set) Token: 0x060042D7 RID: 17111 RVA: 0x0024F726 File Offset: 0x0024D926
		public static HashSet<Identifier> Unlocked { get; private set; } = new HashSet<Identifier>();

		// Token: 0x17001190 RID: 4496
		// (get) Token: 0x060042D8 RID: 17112 RVA: 0x0024F72E File Offset: 0x0024D92E
		// (set) Token: 0x060042D9 RID: 17113 RVA: 0x0024F735 File Offset: 0x0024D935
		public static HashSet<Identifier> Killed { get; private set; } = new HashSet<Identifier>();

		// Token: 0x17001191 RID: 4497
		// (get) Token: 0x060042DA RID: 17114 RVA: 0x0024F73D File Offset: 0x0024D93D
		// (set) Token: 0x060042DB RID: 17115 RVA: 0x0024F744 File Offset: 0x0024D944
		public static bool IsInitialized { get; private set; }

		// Token: 0x17001192 RID: 4498
		// (get) Token: 0x060042DC RID: 17116 RVA: 0x0024F74C File Offset: 0x0024D94C
		// (set) Token: 0x060042DD RID: 17117 RVA: 0x0024F753 File Offset: 0x0024D953
		public static bool UnlockAll { get; set; }

		// Token: 0x060042DE RID: 17118 RVA: 0x0024F75B File Offset: 0x0024D95B
		public static void Init()
		{
			CreatureMetrics.IsInitialized = true;
			if (File.Exists("creature_metrics.xml"))
			{
				CreatureMetrics.Load();
			}
			CreatureMetrics.Save();
		}

		// Token: 0x060042DF RID: 17119 RVA: 0x0024F77C File Offset: 0x0024D97C
		private static void Load()
		{
			XDocument doc = XMLExtensions.TryLoadXml("creature_metrics.xml");
			XElement root = (doc != null) ? doc.Root : null;
			if (root == null)
			{
				DebugConsole.AddWarning("Failed to load creature metrics from creature_metrics.xml!", null);
				return;
			}
			CreatureMetrics.UnlockAll = root.GetAttributeBool("UnlockAll", CreatureMetrics.UnlockAll);
			CreatureMetrics.Unlocked = new HashSet<Identifier>(root.GetAttributeIdentifierArray("Unlocked", Array.Empty<Identifier>(), true));
			CreatureMetrics.Encountered = new HashSet<Identifier>(root.GetAttributeIdentifierArray("Encountered", Array.Empty<Identifier>(), true));
			CreatureMetrics.Killed = new HashSet<Identifier>(root.GetAttributeIdentifierArray("Killed", Array.Empty<Identifier>(), true));
			CreatureMetrics.SyncSets();
		}

		// Token: 0x060042E0 RID: 17120 RVA: 0x0024F81C File Offset: 0x0024DA1C
		public static void Save()
		{
			if (!CreatureMetrics.IsInitialized)
			{
				throw new Exception("Creature Metrics not yet initialized!");
			}
			CreatureMetrics.SyncSets();
			XDocument configDoc = new XDocument();
			XElement root = new XElement("CreatureMetrics");
			configDoc.Add(root);
			root.SetAttributeValue("UnlockAll", CreatureMetrics.UnlockAll);
			root.SetAttributeValue("Unlocked", string.Join<Identifier>(",", CreatureMetrics.Unlocked).Trim().ToLowerInvariant());
			root.SetAttributeValue("Encountered", string.Join<Identifier>(",", CreatureMetrics.Encountered).Trim().ToLowerInvariant());
			root.SetAttributeValue("Killed", string.Join<Identifier>(",", CreatureMetrics.Killed).Trim().ToLowerInvariant());
			configDoc.SaveSafe("creature_metrics.xml", SaveOptions.None, false, 0);
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				NewLineOnAttributes = true
			};
			try
			{
				using (XmlWriter writer = XmlWriter.Create("creature_metrics.xml", settings))
				{
					configDoc.WriteTo(writer);
					writer.Flush();
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving creature metrics failed.", e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("CreatureMetrics.Save:SaveFailed", GameAnalyticsManager.ErrorSeverity.Error, "Saving creature metrics failed.\n" + e.Message + "\n" + e.StackTrace.CleanupStackTrace());
			}
		}

		// Token: 0x060042E1 RID: 17121 RVA: 0x0024F9A0 File Offset: 0x0024DBA0
		public static void RecordKill(Identifier species)
		{
			CreatureMetrics.AddEncounter(species);
			if (!CreatureMetrics.Killed.Contains(species))
			{
				CreatureMetrics.Killed.Add(species);
			}
		}

		// Token: 0x060042E2 RID: 17122 RVA: 0x0024F9C1 File Offset: 0x0024DBC1
		public static void AddEncounter(Identifier species)
		{
			if (species == CharacterPrefab.HumanSpeciesName)
			{
				return;
			}
			if (CreatureMetrics.Encountered.Contains(species))
			{
				return;
			}
			CreatureMetrics.Encountered.Add(species);
			CreatureMetrics.RecentlyEncountered.Add(species);
			CreatureMetrics.UnlockInEditor(species);
		}

		// Token: 0x060042E3 RID: 17123 RVA: 0x0024FA00 File Offset: 0x0024DC00
		public static void UnlockInEditor(Identifier species)
		{
			if (species == CharacterPrefab.HumanSpeciesName)
			{
				return;
			}
			if (CreatureMetrics.Unlocked.Contains(species))
			{
				return;
			}
			if (CreatureMetrics.vanillaCharacters == null)
			{
				CreatureMetrics.vanillaCharacters = GameMain.VanillaContent.GetFiles<CharacterFile>();
			}
			CharacterPrefab contentFile = CharacterPrefab.FindBySpeciesName(species);
			if (contentFile == null)
			{
				return;
			}
			if (!CreatureMetrics.vanillaCharacters.Contains(contentFile.ContentFile))
			{
				return;
			}
			CreatureMetrics.Unlocked.Add(species);
		}

		// Token: 0x060042E4 RID: 17124 RVA: 0x0024FA6C File Offset: 0x0024DC6C
		private static void SyncSets()
		{
			foreach (Identifier species in CreatureMetrics.Killed)
			{
				CreatureMetrics.Encountered.Add(species);
			}
			foreach (Identifier species2 in CreatureMetrics.Encountered)
			{
				CreatureMetrics.Unlocked.Add(species2);
			}
		}

		// Token: 0x04002294 RID: 8852
		private const string path = "creature_metrics.xml";

		// Token: 0x0400229B RID: 8859
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private static IEnumerable<CharacterFile> vanillaCharacters;
	}
}

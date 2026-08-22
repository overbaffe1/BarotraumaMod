using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x02000285 RID: 645
	[NullableContext(1)]
	[Nullable(0)]
	public static class CreatureMetrics
	{
		// Token: 0x17000D6B RID: 3435
		// (get) Token: 0x06002DBF RID: 11711 RVA: 0x0012F535 File Offset: 0x0012D735
		// (set) Token: 0x06002DC0 RID: 11712 RVA: 0x0012F53C File Offset: 0x0012D73C
		public static HashSet<Identifier> RecentlyEncountered { get; private set; } = new HashSet<Identifier>();

		// Token: 0x17000D6C RID: 3436
		// (get) Token: 0x06002DC1 RID: 11713 RVA: 0x0012F544 File Offset: 0x0012D744
		// (set) Token: 0x06002DC2 RID: 11714 RVA: 0x0012F54B File Offset: 0x0012D74B
		public static HashSet<Identifier> Encountered { get; private set; } = new HashSet<Identifier>();

		// Token: 0x17000D6D RID: 3437
		// (get) Token: 0x06002DC3 RID: 11715 RVA: 0x0012F553 File Offset: 0x0012D753
		// (set) Token: 0x06002DC4 RID: 11716 RVA: 0x0012F55A File Offset: 0x0012D75A
		public static HashSet<Identifier> Unlocked { get; private set; } = new HashSet<Identifier>();

		// Token: 0x17000D6E RID: 3438
		// (get) Token: 0x06002DC5 RID: 11717 RVA: 0x0012F562 File Offset: 0x0012D762
		// (set) Token: 0x06002DC6 RID: 11718 RVA: 0x0012F569 File Offset: 0x0012D769
		public static HashSet<Identifier> Killed { get; private set; } = new HashSet<Identifier>();

		// Token: 0x17000D6F RID: 3439
		// (get) Token: 0x06002DC7 RID: 11719 RVA: 0x0012F571 File Offset: 0x0012D771
		// (set) Token: 0x06002DC8 RID: 11720 RVA: 0x0012F578 File Offset: 0x0012D778
		public static bool IsInitialized { get; private set; }

		// Token: 0x17000D70 RID: 3440
		// (get) Token: 0x06002DC9 RID: 11721 RVA: 0x0012F580 File Offset: 0x0012D780
		// (set) Token: 0x06002DCA RID: 11722 RVA: 0x0012F587 File Offset: 0x0012D787
		public static bool UnlockAll { get; set; }

		// Token: 0x06002DCB RID: 11723 RVA: 0x0012F58F File Offset: 0x0012D78F
		public static void Init()
		{
			CreatureMetrics.IsInitialized = true;
			if (File.Exists("creature_metrics.xml"))
			{
				CreatureMetrics.Load();
			}
			CreatureMetrics.Save();
		}

		// Token: 0x06002DCC RID: 11724 RVA: 0x0012F5B0 File Offset: 0x0012D7B0
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

		// Token: 0x06002DCD RID: 11725 RVA: 0x0012F650 File Offset: 0x0012D850
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

		// Token: 0x06002DCE RID: 11726 RVA: 0x0012F7D4 File Offset: 0x0012D9D4
		public static void RecordKill(Identifier species)
		{
			CreatureMetrics.AddEncounter(species);
			if (!CreatureMetrics.Killed.Contains(species))
			{
				CreatureMetrics.Killed.Add(species);
			}
		}

		// Token: 0x06002DCF RID: 11727 RVA: 0x0012F7F5 File Offset: 0x0012D9F5
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

		// Token: 0x06002DD0 RID: 11728 RVA: 0x0012F834 File Offset: 0x0012DA34
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

		// Token: 0x06002DD1 RID: 11729 RVA: 0x0012F8A0 File Offset: 0x0012DAA0
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

		// Token: 0x04001650 RID: 5712
		private const string path = "creature_metrics.xml";

		// Token: 0x04001657 RID: 5719
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private static IEnumerable<CharacterFile> vanillaCharacters;
	}
}

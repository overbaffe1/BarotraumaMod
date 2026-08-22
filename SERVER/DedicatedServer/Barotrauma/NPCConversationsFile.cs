using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200013F RID: 319
	[NotSyncedInMultiplayer]
	internal sealed class NPCConversationsFile : ContentFile
	{
		// Token: 0x06001C26 RID: 7206 RVA: 0x000CE621 File Offset: 0x000CC821
		public NPCConversationsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001C27 RID: 7207 RVA: 0x000CE62C File Offset: 0x000CC82C
		public override void LoadFile()
		{
			XDocument doc = XMLExtensions.TryLoadXml(this.Path);
			if (doc == null)
			{
				return;
			}
			ContentXElement mainElement = doc.Root.FromPackage(this.ContentPackage);
			bool allowOverriding = doc.Root.IsOverride();
			if (allowOverriding)
			{
				mainElement = mainElement.FirstElement();
			}
			NPCConversationCollection npcConversationCollection = new NPCConversationCollection(this, mainElement);
			if (!NPCConversationCollection.Collections.ContainsKey(npcConversationCollection.Language))
			{
				NPCConversationCollection.Collections.Add(npcConversationCollection.Language, new PrefabCollection<NPCConversationCollection>());
			}
			NPCConversationCollection.Collections[npcConversationCollection.Language].Add(npcConversationCollection, allowOverriding);
		}

		// Token: 0x06001C28 RID: 7208 RVA: 0x000CE6B8 File Offset: 0x000CC8B8
		public override void UnloadFile()
		{
			foreach (PrefabCollection<NPCConversationCollection> collection in NPCConversationCollection.Collections.Values)
			{
				collection.RemoveByFile(this);
			}
		}

		// Token: 0x06001C29 RID: 7209 RVA: 0x000CE710 File Offset: 0x000CC910
		public override void Sort()
		{
			foreach (PrefabCollection<NPCConversationCollection> collection in NPCConversationCollection.Collections.Values)
			{
				collection.SortAll();
			}
		}
	}
}

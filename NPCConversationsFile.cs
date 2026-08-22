using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000235 RID: 565
	[NotSyncedInMultiplayer]
	internal sealed class NPCConversationsFile : ContentFile
	{
		// Token: 0x06003705 RID: 14085 RVA: 0x002140B9 File Offset: 0x002122B9
		public NPCConversationsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06003706 RID: 14086 RVA: 0x002140C4 File Offset: 0x002122C4
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

		// Token: 0x06003707 RID: 14087 RVA: 0x00214150 File Offset: 0x00212350
		public override void UnloadFile()
		{
			foreach (PrefabCollection<NPCConversationCollection> collection in NPCConversationCollection.Collections.Values)
			{
				collection.RemoveByFile(this);
			}
		}

		// Token: 0x06003708 RID: 14088 RVA: 0x002141A8 File Offset: 0x002123A8
		public override void Sort()
		{
			foreach (PrefabCollection<NPCConversationCollection> collection in NPCConversationCollection.Collections.Values)
			{
				collection.SortAll();
			}
		}
	}
}

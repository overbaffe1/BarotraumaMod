using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x02000062 RID: 98
	internal class NPCConversationCollection : Prefab
	{
		// Token: 0x06000DC4 RID: 3524 RVA: 0x000883F0 File Offset: 0x000865F0
		public NPCConversationCollection(NPCConversationsFile file, ContentXElement element) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.Language = element.GetAttributeIdentifier("language", "English").ToLanguageIdentifier();
			this.Conversations = new List<NPCConversation>();
			foreach (ContentXElement subElement in element.Elements())
			{
				Identifier elemName = new Identifier(subElement.Name.LocalName);
				if (elemName == "Conversation")
				{
					this.Conversations.Add(new NPCConversation(subElement));
				}
			}
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x000884AC File Offset: 0x000866AC
		public override void Dispose()
		{
		}

		// Token: 0x04000678 RID: 1656
		public static readonly Dictionary<LanguageIdentifier, PrefabCollection<NPCConversationCollection>> Collections = new Dictionary<LanguageIdentifier, PrefabCollection<NPCConversationCollection>>();

		// Token: 0x04000679 RID: 1657
		public readonly LanguageIdentifier Language;

		// Token: 0x0400067A RID: 1658
		public readonly List<NPCConversation> Conversations;
	}
}

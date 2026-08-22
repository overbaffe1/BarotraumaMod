using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x02000169 RID: 361
	internal class NPCConversationCollection : Prefab
	{
		// Token: 0x06002B22 RID: 11042 RVA: 0x001DC054 File Offset: 0x001DA254
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

		// Token: 0x06002B23 RID: 11043 RVA: 0x001DC110 File Offset: 0x001DA310
		public override void Dispose()
		{
		}

		// Token: 0x0400168B RID: 5771
		public static readonly Dictionary<LanguageIdentifier, PrefabCollection<NPCConversationCollection>> Collections = new Dictionary<LanguageIdentifier, PrefabCollection<NPCConversationCollection>>();

		// Token: 0x0400168C RID: 5772
		public readonly LanguageIdentifier Language;

		// Token: 0x0400168D RID: 5773
		public readonly List<NPCConversation> Conversations;
	}
}

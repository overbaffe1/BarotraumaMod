using System;

namespace Barotrauma
{
	// Token: 0x02000133 RID: 307
	internal sealed class EventManagerSettingsFile : GenericPrefabFile<EventManagerSettings>
	{
		// Token: 0x06001BE8 RID: 7144 RVA: 0x000CDF19 File Offset: 0x000CC119
		public EventManagerSettingsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x000CDF23 File Offset: 0x000CC123
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x000CDF2F File Offset: 0x000CC12F
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "EventManagerSettings";
		}

		// Token: 0x17000825 RID: 2085
		// (get) Token: 0x06001BEB RID: 7147 RVA: 0x000CDF3D File Offset: 0x000CC13D
		protected override PrefabCollection<EventManagerSettings> Prefabs
		{
			get
			{
				return EventManagerSettings.Prefabs;
			}
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x000CDF44 File Offset: 0x000CC144
		protected override EventManagerSettings CreatePrefab(ContentXElement element)
		{
			return new EventManagerSettings(element, this);
		}
	}
}

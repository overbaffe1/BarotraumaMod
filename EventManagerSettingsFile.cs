using System;

namespace Barotrauma
{
	// Token: 0x02000229 RID: 553
	internal sealed class EventManagerSettingsFile : GenericPrefabFile<EventManagerSettings>
	{
		// Token: 0x060036C7 RID: 14023 RVA: 0x002139B1 File Offset: 0x00211BB1
		public EventManagerSettingsFile(ContentPackage contentPackage, ContentPath path) : base(contentPackage, path)
		{
		}

		// Token: 0x060036C8 RID: 14024 RVA: 0x002139BB File Offset: 0x00211BBB
		protected override bool MatchesSingular(Identifier identifier)
		{
			return !this.MatchesPlural(identifier);
		}

		// Token: 0x060036C9 RID: 14025 RVA: 0x002139C7 File Offset: 0x00211BC7
		protected override bool MatchesPlural(Identifier identifier)
		{
			return identifier == "EventManagerSettings";
		}

		// Token: 0x17000E86 RID: 3718
		// (get) Token: 0x060036CA RID: 14026 RVA: 0x002139D5 File Offset: 0x00211BD5
		protected override PrefabCollection<EventManagerSettings> Prefabs
		{
			get
			{
				return EventManagerSettings.Prefabs;
			}
		}

		// Token: 0x060036CB RID: 14027 RVA: 0x002139DC File Offset: 0x00211BDC
		protected override EventManagerSettings CreatePrefab(ContentXElement element)
		{
			return new EventManagerSettings(element, this);
		}
	}
}

using System;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001C0 RID: 448
	internal class EventManagerSettings : PrefabWithUintIdentifier
	{
		// Token: 0x17000988 RID: 2440
		// (get) Token: 0x0600214F RID: 8527 RVA: 0x000DF0E0 File Offset: 0x000DD2E0
		public static IOrderedEnumerable<EventManagerSettings> OrderedByDifficulty
		{
			get
			{
				return from p in EventManagerSettings.Prefabs
				orderby (p.MinLevelDifficulty + p.MaxLevelDifficulty) * 0.5f, p.UintIdentifier
				select p;
			}
		}

		// Token: 0x06002150 RID: 8528 RVA: 0x000DF13C File Offset: 0x000DD33C
		public static EventManagerSettings GetByDifficultyPercentile(float p)
		{
			EventManagerSettings[] settings = EventManagerSettings.OrderedByDifficulty.ToArray<EventManagerSettings>();
			return settings[Math.Clamp((int)((float)settings.Length * p), 0, settings.Length - 1)];
		}

		// Token: 0x06002151 RID: 8529 RVA: 0x000DF168 File Offset: 0x000DD368
		public override void Dispose()
		{
		}

		// Token: 0x06002152 RID: 8530 RVA: 0x000DF16C File Offset: 0x000DD36C
		public EventManagerSettings(XElement element, EventManagerSettingsFile file) : base(file, element.NameAsIdentifier())
		{
			this.Name = TextManager.Get("difficulty." + this.Identifier.ToString()).Fallback(this.Identifier.Value, true);
			this.EventThresholdIncrease = element.GetAttributeFloat("EventThresholdIncrease", this.EventThresholdIncrease);
			this.DefaultEventThreshold = element.GetAttributeFloat("DefaultEventThreshold", this.DefaultEventThreshold);
			this.EventCooldown = element.GetAttributeFloat("EventCooldown", this.EventCooldown);
			this.MinLevelDifficulty = element.GetAttributeFloat("MinLevelDifficulty", this.MinLevelDifficulty);
			this.MaxLevelDifficulty = element.GetAttributeFloat("MaxLevelDifficulty", this.MaxLevelDifficulty);
			this.FreezeDurationWhenCrewAway = element.GetAttributeFloat("FreezeDurationWhenCrewAway", this.FreezeDurationWhenCrewAway);
		}

		// Token: 0x04000FB2 RID: 4018
		public static readonly PrefabCollection<EventManagerSettings> Prefabs = new PrefabCollection<EventManagerSettings>();

		// Token: 0x04000FB3 RID: 4019
		public readonly LocalizedString Name;

		// Token: 0x04000FB4 RID: 4020
		public readonly float EventThresholdIncrease = 0.0005f;

		// Token: 0x04000FB5 RID: 4021
		public readonly float DefaultEventThreshold = 0.2f;

		// Token: 0x04000FB6 RID: 4022
		public readonly float EventCooldown = 360f;

		// Token: 0x04000FB7 RID: 4023
		public readonly float MinLevelDifficulty;

		// Token: 0x04000FB8 RID: 4024
		public readonly float MaxLevelDifficulty = 100f;

		// Token: 0x04000FB9 RID: 4025
		public readonly float FreezeDurationWhenCrewAway = 600f;
	}
}

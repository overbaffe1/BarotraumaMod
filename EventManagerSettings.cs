using System;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002B2 RID: 690
	internal class EventManagerSettings : PrefabWithUintIdentifier
	{
		// Token: 0x17000FC7 RID: 4039
		// (get) Token: 0x06003BED RID: 15341 RVA: 0x00224F74 File Offset: 0x00223174
		public static IOrderedEnumerable<EventManagerSettings> OrderedByDifficulty
		{
			get
			{
				return from p in EventManagerSettings.Prefabs
				orderby (p.MinLevelDifficulty + p.MaxLevelDifficulty) * 0.5f, p.UintIdentifier
				select p;
			}
		}

		// Token: 0x06003BEE RID: 15342 RVA: 0x00224FD0 File Offset: 0x002231D0
		public static EventManagerSettings GetByDifficultyPercentile(float p)
		{
			EventManagerSettings[] settings = EventManagerSettings.OrderedByDifficulty.ToArray<EventManagerSettings>();
			return settings[Math.Clamp((int)((float)settings.Length * p), 0, settings.Length - 1)];
		}

		// Token: 0x06003BEF RID: 15343 RVA: 0x00224FFC File Offset: 0x002231FC
		public override void Dispose()
		{
		}

		// Token: 0x06003BF0 RID: 15344 RVA: 0x00225000 File Offset: 0x00223200
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

		// Token: 0x04001E96 RID: 7830
		public static readonly PrefabCollection<EventManagerSettings> Prefabs = new PrefabCollection<EventManagerSettings>();

		// Token: 0x04001E97 RID: 7831
		public readonly LocalizedString Name;

		// Token: 0x04001E98 RID: 7832
		public readonly float EventThresholdIncrease = 0.0005f;

		// Token: 0x04001E99 RID: 7833
		public readonly float DefaultEventThreshold = 0.2f;

		// Token: 0x04001E9A RID: 7834
		public readonly float EventCooldown = 360f;

		// Token: 0x04001E9B RID: 7835
		public readonly float MinLevelDifficulty;

		// Token: 0x04001E9C RID: 7836
		public readonly float MaxLevelDifficulty = 100f;

		// Token: 0x04001E9D RID: 7837
		public readonly float FreezeDurationWhenCrewAway = 600f;
	}
}

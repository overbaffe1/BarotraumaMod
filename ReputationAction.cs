using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A0 RID: 672
	internal class ReputationAction : EventAction
	{
		// Token: 0x06003A9F RID: 15007 RVA: 0x0021FF00 File Offset: 0x0021E100
		public ReputationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x17000F71 RID: 3953
		// (get) Token: 0x06003AA0 RID: 15008 RVA: 0x0021FF0A File Offset: 0x0021E10A
		// (set) Token: 0x06003AA1 RID: 15009 RVA: 0x0021FF12 File Offset: 0x0021E112
		[Serialize(0f, IsPropertySaveable.Yes, "Amount of reputation to add or remove.", "", false)]
		public float Increase { get; set; }

		// Token: 0x17000F72 RID: 3954
		// (get) Token: 0x06003AA2 RID: 15010 RVA: 0x0021FF1B File Offset: 0x0021E11B
		// (set) Token: 0x06003AA3 RID: 15011 RVA: 0x0021FF23 File Offset: 0x0021E123
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the faction you want to adjust the reputation for. Ignored if TargetType is set to Location.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x17000F73 RID: 3955
		// (get) Token: 0x06003AA4 RID: 15012 RVA: 0x0021FF2C File Offset: 0x0021E12C
		// (set) Token: 0x06003AA5 RID: 15013 RVA: 0x0021FF34 File Offset: 0x0021E134
		[Serialize(ReputationAction.ReputationType.None, IsPropertySaveable.Yes, "Do you want to adjust the reputation for a specific faction, or whichever faction controls the current location?", "", false)]
		public ReputationAction.ReputationType TargetType { get; set; }

		// Token: 0x06003AA6 RID: 15014 RVA: 0x0021FF3D File Offset: 0x0021E13D
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003AA7 RID: 15015 RVA: 0x0021FF45 File Offset: 0x0021E145
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003AA8 RID: 15016 RVA: 0x0021FF50 File Offset: 0x0021E150
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = ((gameSession != null) ? gameSession.GameMode : null) as CampaignMode;
			if (campaign != null)
			{
				ReputationAction.ReputationType targetType = this.TargetType;
				if (targetType != ReputationAction.ReputationType.Location)
				{
					if (targetType == ReputationAction.ReputationType.Faction)
					{
						Faction faction = campaign.Factions.Find(delegate(Faction faction1)
						{
							Prefab prefab = faction1.Prefab;
							Identifier identifier = this.Identifier;
							return prefab.Identifier == identifier;
						});
						if (faction != null)
						{
							faction.Reputation.AddReputation(this.Increase, float.MaxValue);
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 1);
							defaultInterpolatedStringHandler.AppendLiteral("Faction with the identifier \"");
							defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
							defaultInterpolatedStringHandler.AppendLiteral("\" was not found.");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.ParentEvent.Prefab.ContentPackage, false, false);
						}
					}
					else
					{
						DebugConsole.ThrowError("ReputationAction requires a \"TargetType\" but none were specified.", null, this.ParentEvent.Prefab.ContentPackage, false, false);
					}
				}
				else
				{
					Location currentLocation = campaign.Map.CurrentLocation;
					if (currentLocation != null)
					{
						Reputation reputation = currentLocation.Reputation;
						if (reputation != null)
						{
							reputation.AddReputation(this.Increase, float.MaxValue);
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003AA9 RID: 15017 RVA: 0x00220070 File Offset: 0x0021E270
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("ReputationAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (FactionIdentifier: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Identifier.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", TargetType: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetType.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Increase: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Increase.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E1F RID: 7711
		private bool isFinished;

		// Token: 0x02000F2B RID: 3883
		public enum ReputationType
		{
			// Token: 0x040054D1 RID: 21713
			None,
			// Token: 0x040054D2 RID: 21714
			Location,
			// Token: 0x040054D3 RID: 21715
			Faction
		}
	}
}

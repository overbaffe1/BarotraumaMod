using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001AE RID: 430
	internal class ReputationAction : EventAction
	{
		// Token: 0x06001FF3 RID: 8179 RVA: 0x000DA3E4 File Offset: 0x000D85E4
		public ReputationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x1700092B RID: 2347
		// (get) Token: 0x06001FF4 RID: 8180 RVA: 0x000DA3EE File Offset: 0x000D85EE
		// (set) Token: 0x06001FF5 RID: 8181 RVA: 0x000DA3F6 File Offset: 0x000D85F6
		[Serialize(0f, IsPropertySaveable.Yes, "Amount of reputation to add or remove.", "", false)]
		public float Increase { get; set; }

		// Token: 0x1700092C RID: 2348
		// (get) Token: 0x06001FF6 RID: 8182 RVA: 0x000DA3FF File Offset: 0x000D85FF
		// (set) Token: 0x06001FF7 RID: 8183 RVA: 0x000DA407 File Offset: 0x000D8607
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the faction you want to adjust the reputation for. Ignored if TargetType is set to Location.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x1700092D RID: 2349
		// (get) Token: 0x06001FF8 RID: 8184 RVA: 0x000DA410 File Offset: 0x000D8610
		// (set) Token: 0x06001FF9 RID: 8185 RVA: 0x000DA418 File Offset: 0x000D8618
		[Serialize(ReputationAction.ReputationType.None, IsPropertySaveable.Yes, "Do you want to adjust the reputation for a specific faction, or whichever faction controls the current location?", "", false)]
		public ReputationAction.ReputationType TargetType { get; set; }

		// Token: 0x06001FFA RID: 8186 RVA: 0x000DA421 File Offset: 0x000D8621
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001FFB RID: 8187 RVA: 0x000DA429 File Offset: 0x000D8629
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001FFC RID: 8188 RVA: 0x000DA434 File Offset: 0x000D8634
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

		// Token: 0x06001FFD RID: 8189 RVA: 0x000DA554 File Offset: 0x000D8754
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

		// Token: 0x04000F36 RID: 3894
		private bool isFinished;

		// Token: 0x0200091D RID: 2333
		public enum ReputationType
		{
			// Token: 0x0400320B RID: 12811
			None,
			// Token: 0x0400320C RID: 12812
			Location,
			// Token: 0x0400320D RID: 12813
			Faction
		}
	}
}

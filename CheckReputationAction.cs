using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000283 RID: 643
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckReputationAction : CheckDataAction
	{
		// Token: 0x17000F0B RID: 3851
		// (get) Token: 0x0600393C RID: 14652 RVA: 0x0021B132 File Offset: 0x00219332
		// (set) Token: 0x0600393D RID: 14653 RVA: 0x0021B13A File Offset: 0x0021933A
		[Serialize(ReputationAction.ReputationType.None, IsPropertySaveable.Yes, "Should the action check the reputation for a given faction, or whichever faction owns the current location.", "", false)]
		public ReputationAction.ReputationType TargetType { get; set; }

		// Token: 0x0600393E RID: 14654 RVA: 0x0021B143 File Offset: 0x00219343
		public CheckReputationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x0600393F RID: 14655 RVA: 0x0021B150 File Offset: 0x00219350
		protected override float GetFloat(CampaignMode campaignMode)
		{
			ReputationAction.ReputationType targetType = this.TargetType;
			if (targetType != ReputationAction.ReputationType.Location)
			{
				if (targetType == ReputationAction.ReputationType.Faction)
				{
					Faction faction = campaignMode.Factions.Find(delegate(Faction f)
					{
						Prefab prefab = f.Prefab;
						Identifier identifier = base.Identifier;
						return prefab.Identifier == identifier;
					});
					if (faction != null)
					{
						return faction.Reputation.Value;
					}
				}
				else
				{
					DebugConsole.ThrowError("CheckReputationAction requires a \"TargetType\" but none were specified.", null, this.ParentEvent.Prefab.ContentPackage, false, false);
				}
			}
			else
			{
				Location location = campaignMode.Map.CurrentLocation;
				if (((location != null) ? location.Reputation : null) != null)
				{
					return location.Reputation.Value;
				}
			}
			return 0f;
		}

		// Token: 0x06003940 RID: 14656 RVA: 0x0021B1DB File Offset: 0x002193DB
		protected override bool GetBool(CampaignMode campaignMode)
		{
			DebugConsole.ThrowError("Boolean comparison cannot be applied to reputations.", null, this.ParentEvent.Prefab.ContentPackage, false, false);
			return false;
		}

		// Token: 0x06003941 RID: 14657 RVA: 0x0021B1FC File Offset: 0x002193FC
		public override string ToDebugString()
		{
			string condition = "?";
			if (this.value2 != null && this.value1 != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler.AppendFormatted(this.value1.ColorizeObject());
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(base.Operator.ColorizeObject());
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(this.value2.ColorizeObject());
				condition = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(38, 6);
			defaultInterpolatedStringHandler2.AppendFormatted(ToolBox.GetDebugSymbol(this.succeeded != null, false));
			defaultInterpolatedStringHandler2.AppendLiteral(" ");
			defaultInterpolatedStringHandler2.AppendFormatted("CheckReputationAction");
			defaultInterpolatedStringHandler2.AppendLiteral(" -> (Type: ");
			defaultInterpolatedStringHandler2.AppendFormatted(this.TargetType.ColorizeObject());
			defaultInterpolatedStringHandler2.AppendLiteral(", ");
			defaultInterpolatedStringHandler2.AppendFormatted(base.Identifier.IsEmpty ? string.Empty : ("Identifier: " + base.Identifier.ColorizeObject() + ", "));
			defaultInterpolatedStringHandler2.AppendLiteral("Success: ");
			defaultInterpolatedStringHandler2.AppendFormatted(this.succeeded.ColorizeObject());
			defaultInterpolatedStringHandler2.AppendLiteral(", Expression: ");
			defaultInterpolatedStringHandler2.AppendFormatted(condition);
			defaultInterpolatedStringHandler2.AppendLiteral(")");
			return defaultInterpolatedStringHandler2.ToStringAndClear();
		}
	}
}

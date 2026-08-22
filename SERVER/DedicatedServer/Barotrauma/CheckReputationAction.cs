using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000190 RID: 400
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckReputationAction : CheckDataAction
	{
		// Token: 0x170008B7 RID: 2231
		// (get) Token: 0x06001E72 RID: 7794 RVA: 0x000D5C36 File Offset: 0x000D3E36
		// (set) Token: 0x06001E73 RID: 7795 RVA: 0x000D5C3E File Offset: 0x000D3E3E
		[Serialize(ReputationAction.ReputationType.None, IsPropertySaveable.Yes, "Should the action check the reputation for a given faction, or whichever faction owns the current location.", "", false)]
		public ReputationAction.ReputationType TargetType { get; set; }

		// Token: 0x06001E74 RID: 7796 RVA: 0x000D5C47 File Offset: 0x000D3E47
		public CheckReputationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x000D5C54 File Offset: 0x000D3E54
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

		// Token: 0x06001E76 RID: 7798 RVA: 0x000D5CDF File Offset: 0x000D3EDF
		protected override bool GetBool(CampaignMode campaignMode)
		{
			DebugConsole.ThrowError("Boolean comparison cannot be applied to reputations.", null, this.ParentEvent.Prefab.ContentPackage, false, false);
			return false;
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x000D5D00 File Offset: 0x000D3F00
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

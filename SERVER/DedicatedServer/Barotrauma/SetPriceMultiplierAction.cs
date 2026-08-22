using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001B1 RID: 433
	internal class SetPriceMultiplierAction : EventAction
	{
		// Token: 0x17000932 RID: 2354
		// (get) Token: 0x06002012 RID: 8210 RVA: 0x000DAB1A File Offset: 0x000D8D1A
		// (set) Token: 0x06002013 RID: 8211 RVA: 0x000DAB22 File Offset: 0x000D8D22
		[Serialize(1f, IsPropertySaveable.Yes, "Value to set as the multiplier, or to multiply, min or max the current multiplier with.", "", false)]
		public float Multiplier { get; set; }

		// Token: 0x17000933 RID: 2355
		// (get) Token: 0x06002014 RID: 8212 RVA: 0x000DAB2B File Offset: 0x000D8D2B
		// (set) Token: 0x06002015 RID: 8213 RVA: 0x000DAB33 File Offset: 0x000D8D33
		[Serialize(SetPriceMultiplierAction.OperationType.Set, IsPropertySaveable.Yes, "Do you want to set the value as the multiplier, multiply the existing multiplier with it, or take the smaller or larger of the values.", "", false)]
		public SetPriceMultiplierAction.OperationType Operation { get; set; }

		// Token: 0x17000934 RID: 2356
		// (get) Token: 0x06002016 RID: 8214 RVA: 0x000DAB3C File Offset: 0x000D8D3C
		// (set) Token: 0x06002017 RID: 8215 RVA: 0x000DAB44 File Offset: 0x000D8D44
		[Serialize(SetPriceMultiplierAction.PriceMultiplierType.Store, IsPropertySaveable.Yes, "Do you want to set the price multiplier for stores or for mechanical services (hull and item repairs and restoring lost shuttles)?", "", false)]
		public SetPriceMultiplierAction.PriceMultiplierType TargetMultiplier { get; set; }

		// Token: 0x06002018 RID: 8216 RVA: 0x000DAB4D File Offset: 0x000D8D4D
		public SetPriceMultiplierAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06002019 RID: 8217 RVA: 0x000DAB57 File Offset: 0x000D8D57
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x0600201A RID: 8218 RVA: 0x000DAB5F File Offset: 0x000D8D5F
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x0600201B RID: 8219 RVA: 0x000DAB68 File Offset: 0x000D8D68
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
				Map map = campaign.Map;
				if (((map != null) ? map.CurrentLocation : null) != null)
				{
					float newMultiplier = this.GetCurrentMultiplier(campaign.Map.CurrentLocation);
					switch (this.Operation)
					{
					case SetPriceMultiplierAction.OperationType.Set:
						newMultiplier = this.Multiplier;
						break;
					case SetPriceMultiplierAction.OperationType.Multiply:
						newMultiplier *= this.Multiplier;
						break;
					case SetPriceMultiplierAction.OperationType.Min:
						newMultiplier = Math.Min(this.Multiplier, campaign.Map.CurrentLocation.PriceMultiplier);
						break;
					case SetPriceMultiplierAction.OperationType.Max:
						newMultiplier = Math.Max(this.Multiplier, campaign.Map.CurrentLocation.PriceMultiplier);
						break;
					default:
						throw new NotImplementedException();
					}
					this.SetCurrentMultiplier(campaign.Map.CurrentLocation, newMultiplier);
				}
			}
			this.isFinished = true;
		}

		// Token: 0x0600201C RID: 8220 RVA: 0x000DAC54 File Offset: 0x000D8E54
		private float GetCurrentMultiplier(Location location)
		{
			SetPriceMultiplierAction.PriceMultiplierType targetMultiplier = this.TargetMultiplier;
			float result;
			if (targetMultiplier != SetPriceMultiplierAction.PriceMultiplierType.Store)
			{
				if (targetMultiplier != SetPriceMultiplierAction.PriceMultiplierType.Mechanical)
				{
					throw new NotImplementedException();
				}
				result = location.MechanicalPriceMultiplier;
			}
			else
			{
				result = location.PriceMultiplier;
			}
			return result;
		}

		// Token: 0x0600201D RID: 8221 RVA: 0x000DAC8C File Offset: 0x000D8E8C
		private void SetCurrentMultiplier(Location location, float value)
		{
			SetPriceMultiplierAction.PriceMultiplierType targetMultiplier = this.TargetMultiplier;
			if (targetMultiplier == SetPriceMultiplierAction.PriceMultiplierType.Store)
			{
				location.PriceMultiplier = value;
				return;
			}
			if (targetMultiplier != SetPriceMultiplierAction.PriceMultiplierType.Mechanical)
			{
				throw new NotImplementedException();
			}
			location.MechanicalPriceMultiplier = value;
		}

		// Token: 0x0600201E RID: 8222 RVA: 0x000DACC0 File Offset: 0x000D8EC0
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(42, 5);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("SetPriceMultiplierAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (Multiplier: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Multiplier.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Operation: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Operation.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Target: ");
			defaultInterpolatedStringHandler.AppendFormatted<SetPriceMultiplierAction.PriceMultiplierType>(this.TargetMultiplier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000F40 RID: 3904
		private bool isFinished;

		// Token: 0x0200091F RID: 2335
		public enum OperationType
		{
			// Token: 0x04003213 RID: 12819
			Set,
			// Token: 0x04003214 RID: 12820
			Multiply,
			// Token: 0x04003215 RID: 12821
			Min,
			// Token: 0x04003216 RID: 12822
			Max
		}

		// Token: 0x02000920 RID: 2336
		public enum PriceMultiplierType
		{
			// Token: 0x04003218 RID: 12824
			Store,
			// Token: 0x04003219 RID: 12825
			Mechanical
		}
	}
}

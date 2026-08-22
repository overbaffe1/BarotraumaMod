using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A3 RID: 675
	internal class SetPriceMultiplierAction : EventAction
	{
		// Token: 0x17000F78 RID: 3960
		// (get) Token: 0x06003ABE RID: 15038 RVA: 0x00220636 File Offset: 0x0021E836
		// (set) Token: 0x06003ABF RID: 15039 RVA: 0x0022063E File Offset: 0x0021E83E
		[Serialize(1f, IsPropertySaveable.Yes, "Value to set as the multiplier, or to multiply, min or max the current multiplier with.", "", false)]
		public float Multiplier { get; set; }

		// Token: 0x17000F79 RID: 3961
		// (get) Token: 0x06003AC0 RID: 15040 RVA: 0x00220647 File Offset: 0x0021E847
		// (set) Token: 0x06003AC1 RID: 15041 RVA: 0x0022064F File Offset: 0x0021E84F
		[Serialize(SetPriceMultiplierAction.OperationType.Set, IsPropertySaveable.Yes, "Do you want to set the value as the multiplier, multiply the existing multiplier with it, or take the smaller or larger of the values.", "", false)]
		public SetPriceMultiplierAction.OperationType Operation { get; set; }

		// Token: 0x17000F7A RID: 3962
		// (get) Token: 0x06003AC2 RID: 15042 RVA: 0x00220658 File Offset: 0x0021E858
		// (set) Token: 0x06003AC3 RID: 15043 RVA: 0x00220660 File Offset: 0x0021E860
		[Serialize(SetPriceMultiplierAction.PriceMultiplierType.Store, IsPropertySaveable.Yes, "Do you want to set the price multiplier for stores or for mechanical services (hull and item repairs and restoring lost shuttles)?", "", false)]
		public SetPriceMultiplierAction.PriceMultiplierType TargetMultiplier { get; set; }

		// Token: 0x06003AC4 RID: 15044 RVA: 0x00220669 File Offset: 0x0021E869
		public SetPriceMultiplierAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003AC5 RID: 15045 RVA: 0x00220673 File Offset: 0x0021E873
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003AC6 RID: 15046 RVA: 0x0022067B File Offset: 0x0021E87B
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003AC7 RID: 15047 RVA: 0x00220684 File Offset: 0x0021E884
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

		// Token: 0x06003AC8 RID: 15048 RVA: 0x00220770 File Offset: 0x0021E970
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

		// Token: 0x06003AC9 RID: 15049 RVA: 0x002207A8 File Offset: 0x0021E9A8
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

		// Token: 0x06003ACA RID: 15050 RVA: 0x002207DC File Offset: 0x0021E9DC
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

		// Token: 0x04001E29 RID: 7721
		private bool isFinished;

		// Token: 0x02000F2D RID: 3885
		public enum OperationType
		{
			// Token: 0x040054D9 RID: 21721
			Set,
			// Token: 0x040054DA RID: 21722
			Multiply,
			// Token: 0x040054DB RID: 21723
			Min,
			// Token: 0x040054DC RID: 21724
			Max
		}

		// Token: 0x02000F2E RID: 3886
		public enum PriceMultiplierType
		{
			// Token: 0x040054DE RID: 21726
			Store,
			// Token: 0x040054DF RID: 21727
			Mechanical
		}
	}
}

using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005FB RID: 1531
	internal class SonarTransducer : Powered
	{
		// Token: 0x060063C8 RID: 25544 RVA: 0x0033E978 File Offset: 0x0033CB78
		public SonarTransducer(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060063C9 RID: 25545 RVA: 0x0033E98C File Offset: 0x0033CB8C
		public override void Update(float deltaTime, Camera cam)
		{
			base.UpdateOnActiveEffects(deltaTime);
			if (this.HasPower)
			{
				this.sendSignalTimer += deltaTime;
				if (this.sendSignalTimer > 0.5f)
				{
					this.item.SendSignal("0101101101101011010", "data_out");
					this.sendSignalTimer = 0.5f;
				}
			}
		}

		// Token: 0x060063CA RID: 25546 RVA: 0x0033E9E4 File Offset: 0x0033CBE4
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			float powerConsumption = base.PowerConsumption;
			Sonar connectedSonar = this.ConnectedSonar;
			return powerConsumption * ((connectedSonar != null && connectedSonar.CurrentMode == Sonar.Mode.Active) ? 1f : 0.1f);
		}

		// Token: 0x040033B9 RID: 13241
		private const float SendSignalInterval = 0.5f;

		// Token: 0x040033BA RID: 13242
		private float sendSignalTimer;

		// Token: 0x040033BB RID: 13243
		public Sonar ConnectedSonar;
	}
}

using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D1 RID: 1233
	internal class SonarTransducer : Powered
	{
		// Token: 0x06004651 RID: 18001 RVA: 0x001C14E5 File Offset: 0x001BF6E5
		public SonarTransducer(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06004652 RID: 18002 RVA: 0x001C14F8 File Offset: 0x001BF6F8
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

		// Token: 0x06004653 RID: 18003 RVA: 0x001C1550 File Offset: 0x001BF750
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

		// Token: 0x040021D1 RID: 8657
		private const float SendSignalInterval = 0.5f;

		// Token: 0x040021D2 RID: 8658
		private float sendSignalTimer;

		// Token: 0x040021D3 RID: 8659
		public Sonar ConnectedSonar;
	}
}

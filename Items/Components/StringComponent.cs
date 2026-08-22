using System;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000621 RID: 1569
	internal abstract class StringComponent : ItemComponent
	{
		// Token: 0x17001977 RID: 6519
		// (get) Token: 0x06006496 RID: 25750 RVA: 0x003418AB File Offset: 0x0033FAAB
		// (set) Token: 0x06006497 RID: 25751 RVA: 0x003418B4 File Offset: 0x0033FAB4
		[InGameEditable(DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "The item must have received signals to both inputs within this timeframe to output the result. If set to 0, the inputs must be received at the same time.", "", true)]
		public float TimeFrame
		{
			get
			{
				return this.timeFrame;
			}
			set
			{
				if (value > this.timeFrame)
				{
					this.timeSinceReceived[0] = (this.timeSinceReceived[1] = Math.Max(value * 2f, 0.1f));
				}
				this.timeFrame = Math.Max(0f, value);
			}
		}

		// Token: 0x06006498 RID: 25752 RVA: 0x00341900 File Offset: 0x0033FB00
		public StringComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.receivedSignal = new string[2];
		}

		// Token: 0x06006499 RID: 25753 RVA: 0x00341960 File Offset: 0x0033FB60
		public sealed override void Update(float deltaTime, Camera cam)
		{
			bool deactivate = true;
			bool earlyReturn = false;
			for (int i = 0; i < this.timeSinceReceived.Length; i++)
			{
				deactivate &= (this.timeSinceReceived[i] > this.timeFrame);
				earlyReturn |= (this.timeSinceReceived[i] > this.timeFrame);
				this.timeSinceReceived[i] += deltaTime;
			}
			this.IsActive = !deactivate;
			if (earlyReturn)
			{
				return;
			}
			string output = this.Calculate(this.receivedSignal[0], this.receivedSignal[1]);
			this.item.SendSignal(output, "signal_out");
		}

		// Token: 0x0600649A RID: 25754
		protected abstract string Calculate(string signal1, string signal2);

		// Token: 0x0600649B RID: 25755 RVA: 0x003419F4 File Offset: 0x0033FBF4
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in1")
			{
				this.receivedSignal[0] = signal.value;
				this.timeSinceReceived[0] = 0f;
				this.IsActive = true;
				return;
			}
			if (!(name == "signal_in2"))
			{
				return;
			}
			this.receivedSignal[1] = signal.value;
			this.timeSinceReceived[1] = 0f;
			this.IsActive = true;
		}

		// Token: 0x04003433 RID: 13363
		protected float[] timeSinceReceived;

		// Token: 0x04003434 RID: 13364
		protected string[] receivedSignal;

		// Token: 0x04003435 RID: 13365
		protected float timeFrame;
	}
}

using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000608 RID: 1544
	internal abstract class ArithmeticComponent : ItemComponent
	{
		// Token: 0x17001947 RID: 6471
		// (get) Token: 0x060063E6 RID: 25574 RVA: 0x0033ED13 File Offset: 0x0033CF13
		// (set) Token: 0x060063E7 RID: 25575 RVA: 0x0033ED1B File Offset: 0x0033CF1B
		[Serialize(999999f, IsPropertySaveable.Yes, "The output of the item is restricted below this value.", "", true)]
		[InGameEditable(MinValueFloat = -999999f, MaxValueFloat = 999999f)]
		public float ClampMax { get; set; }

		// Token: 0x17001948 RID: 6472
		// (get) Token: 0x060063E8 RID: 25576 RVA: 0x0033ED24 File Offset: 0x0033CF24
		// (set) Token: 0x060063E9 RID: 25577 RVA: 0x0033ED2C File Offset: 0x0033CF2C
		[Serialize(-999999f, IsPropertySaveable.Yes, "The output of the item is restricted above this value.", "", true)]
		[InGameEditable(MinValueFloat = -999999f, MaxValueFloat = 999999f)]
		public float ClampMin { get; set; }

		// Token: 0x17001949 RID: 6473
		// (get) Token: 0x060063EA RID: 25578 RVA: 0x0033ED35 File Offset: 0x0033CF35
		// (set) Token: 0x060063EB RID: 25579 RVA: 0x0033ED40 File Offset: 0x0033CF40
		[InGameEditable(DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "The item must have received signals to both inputs within this timeframe to output the result. If set to 0, the inputs must be received at the same time.", "sp.", true)]
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

		// Token: 0x060063EC RID: 25580 RVA: 0x0033ED8C File Offset: 0x0033CF8C
		public ArithmeticComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.timeSinceReceived = new float[]
			{
				Math.Max(this.timeFrame * 2f, 0.1f),
				Math.Max(this.timeFrame * 2f, 0.1f)
			};
			this.receivedSignal = new float[2];
		}

		// Token: 0x060063ED RID: 25581 RVA: 0x0033EDF8 File Offset: 0x0033CFF8
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
			float output = this.Calculate(this.receivedSignal[0], this.receivedSignal[1]);
			if (MathUtils.IsValid(output))
			{
				this.item.SendSignal(new Signal(MathHelper.Clamp(output, this.ClampMin, this.ClampMax).ToString("G", CultureInfo.InvariantCulture), 0, this.signalSender[0] ?? this.signalSender[1], null, 0f, 1f), "signal_out");
			}
		}

		// Token: 0x060063EE RID: 25582
		protected abstract float Calculate(float signal1, float signal2);

		// Token: 0x060063EF RID: 25583 RVA: 0x0033EEDC File Offset: 0x0033D0DC
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "signal_in1")
			{
				float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[0]);
				this.signalSender[0] = signal.sender;
				this.timeSinceReceived[0] = 0f;
				this.IsActive = true;
				return;
			}
			if (!(name == "signal_in2"))
			{
				return;
			}
			float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[1]);
			this.signalSender[1] = signal.sender;
			this.timeSinceReceived[1] = 0f;
			this.IsActive = true;
		}

		// Token: 0x040033D8 RID: 13272
		protected float[] timeSinceReceived;

		// Token: 0x040033D9 RID: 13273
		protected float[] receivedSignal;

		// Token: 0x040033DA RID: 13274
		protected float timeFrame;

		// Token: 0x040033DB RID: 13275
		protected readonly Character[] signalSender = new Character[2];
	}
}

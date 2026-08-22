using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004F7 RID: 1271
	internal class OscillatorComponent : ItemComponent
	{
		// Token: 0x17001331 RID: 4913
		// (get) Token: 0x0600477A RID: 18298 RVA: 0x001C76FE File Offset: 0x001C58FE
		// (set) Token: 0x0600477B RID: 18299 RVA: 0x001C7706 File Offset: 0x001C5906
		[InGameEditable]
		[Serialize(OscillatorComponent.WaveType.Pulse, IsPropertySaveable.Yes, "What kind of a signal the item outputs. Pulse: periodically sends out a signal of 1. Sawtooth: sends out a periodic wave that increases linearly from 0 to 1. Sine: sends out a sine wave oscillating between -1 and 1. Square: sends out a signal that alternates between 0 and 1. Triangle: sends out a wave that alternates between increasing linearly from -1 to 1 and decreasing from 1 to -1.", "", true)]
		public OscillatorComponent.WaveType OutputType { get; set; }

		// Token: 0x17001332 RID: 4914
		// (get) Token: 0x0600477C RID: 18300 RVA: 0x001C770F File Offset: 0x001C590F
		// (set) Token: 0x0600477D RID: 18301 RVA: 0x001C7717 File Offset: 0x001C5917
		[InGameEditable(DecimalCount = 2)]
		[Serialize(1f, IsPropertySaveable.Yes, "How fast the signal oscillates, or how fast the pulses are sent (in Hz).", "", true)]
		public float Frequency
		{
			get
			{
				return this.frequency;
			}
			set
			{
				this.frequency = MathHelper.Clamp(value, 0f, 240f);
			}
		}

		// Token: 0x0600477E RID: 18302 RVA: 0x001C772F File Offset: 0x001C592F
		public OscillatorComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600477F RID: 18303 RVA: 0x001C7740 File Offset: 0x001C5940
		public override void Update(float deltaTime, Camera cam)
		{
			switch (this.OutputType)
			{
			case OscillatorComponent.WaveType.Pulse:
			{
				if (this.frequency <= 0f)
				{
					return;
				}
				this.phase += deltaTime;
				float pulseInterval = 1f / this.frequency;
				while (this.phase >= pulseInterval)
				{
					this.item.SendSignal("1", "signal_out");
					this.phase -= pulseInterval;
				}
				return;
			}
			case OscillatorComponent.WaveType.Sawtooth:
				this.phase = (this.phase + deltaTime * this.frequency) % 1f;
				this.item.SendSignal(this.phase.ToString(CultureInfo.InvariantCulture), "signal_out");
				return;
			case OscillatorComponent.WaveType.Sine:
				this.phase = (this.phase + deltaTime * this.frequency) % 1f;
				this.item.SendSignal(Math.Sin((double)(this.phase * 6.2831855f)).ToString(CultureInfo.InvariantCulture), "signal_out");
				return;
			case OscillatorComponent.WaveType.Square:
				this.phase = (this.phase + deltaTime * this.frequency) % 1f;
				this.item.SendSignal((this.phase < 0.5f) ? "0" : "1", "signal_out");
				return;
			case OscillatorComponent.WaveType.Triangle:
			{
				this.phase = (this.phase + deltaTime * this.frequency) % 1f;
				float output = 4f * MathF.Abs(MathUtils.PositiveModulo(this.phase - 0.25f, 1f) - 0.5f) - 1f;
				this.item.SendSignal(output.ToString(CultureInfo.InvariantCulture), "signal_out");
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x06004780 RID: 18304 RVA: 0x001C78F8 File Offset: 0x001C5AF8
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (name == "set_frequency" || name == "frequency_in")
			{
				float newFrequency;
				if (float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newFrequency))
				{
					this.Frequency = newFrequency;
				}
				this.IsActive = true;
				return;
			}
			if (!(name == "set_outputtype") && !(name == "set_wavetype"))
			{
				return;
			}
			OscillatorComponent.WaveType newOutputType;
			if (Enum.TryParse<OscillatorComponent.WaveType>(signal.value, out newOutputType))
			{
				this.OutputType = newOutputType;
			}
		}

		// Token: 0x0400227A RID: 8826
		private float frequency;

		// Token: 0x0400227B RID: 8827
		private float phase;

		// Token: 0x02000E37 RID: 3639
		public enum WaveType
		{
			// Token: 0x04004226 RID: 16934
			Pulse,
			// Token: 0x04004227 RID: 16935
			Sawtooth,
			// Token: 0x04004228 RID: 16936
			Sine,
			// Token: 0x04004229 RID: 16937
			Square,
			// Token: 0x0400422A RID: 16938
			Triangle
		}
	}
}

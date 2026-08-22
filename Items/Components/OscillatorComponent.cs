using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200061A RID: 1562
	internal class OscillatorComponent : ItemComponent
	{
		// Token: 0x17001960 RID: 6496
		// (get) Token: 0x0600644A RID: 25674 RVA: 0x003403CA File Offset: 0x0033E5CA
		// (set) Token: 0x0600644B RID: 25675 RVA: 0x003403D2 File Offset: 0x0033E5D2
		[InGameEditable]
		[Serialize(OscillatorComponent.WaveType.Pulse, IsPropertySaveable.Yes, "What kind of a signal the item outputs. Pulse: periodically sends out a signal of 1. Sawtooth: sends out a periodic wave that increases linearly from 0 to 1. Sine: sends out a sine wave oscillating between -1 and 1. Square: sends out a signal that alternates between 0 and 1. Triangle: sends out a wave that alternates between increasing linearly from -1 to 1 and decreasing from 1 to -1.", "", true)]
		public OscillatorComponent.WaveType OutputType { get; set; }

		// Token: 0x17001961 RID: 6497
		// (get) Token: 0x0600644C RID: 25676 RVA: 0x003403DB File Offset: 0x0033E5DB
		// (set) Token: 0x0600644D RID: 25677 RVA: 0x003403E3 File Offset: 0x0033E5E3
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

		// Token: 0x0600644E RID: 25678 RVA: 0x003403FB File Offset: 0x0033E5FB
		public OscillatorComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600644F RID: 25679 RVA: 0x0034040C File Offset: 0x0033E60C
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

		// Token: 0x06006450 RID: 25680 RVA: 0x003405C4 File Offset: 0x0033E7C4
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

		// Token: 0x040033FE RID: 13310
		private float frequency;

		// Token: 0x040033FF RID: 13311
		private float phase;

		// Token: 0x020014A0 RID: 5280
		public enum WaveType
		{
			// Token: 0x04006669 RID: 26217
			Pulse,
			// Token: 0x0400666A RID: 26218
			Sawtooth,
			// Token: 0x0400666B RID: 26219
			Sine,
			// Token: 0x0400666C RID: 26220
			Square,
			// Token: 0x0400666D RID: 26221
			Triangle
		}
	}
}

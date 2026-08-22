using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000624 RID: 1572
	internal class TrigonometricFunctionComponent : ItemComponent
	{
		// Token: 0x17001978 RID: 6520
		// (get) Token: 0x060064A0 RID: 25760 RVA: 0x00341AA4 File Offset: 0x0033FCA4
		// (set) Token: 0x060064A1 RID: 25761 RVA: 0x00341AAC File Offset: 0x0033FCAC
		[Serialize(TrigonometricFunctionComponent.FunctionType.Sin, IsPropertySaveable.No, "Which kind of function to run the input through.", "", true)]
		public TrigonometricFunctionComponent.FunctionType Function { get; set; }

		// Token: 0x17001979 RID: 6521
		// (get) Token: 0x060064A2 RID: 25762 RVA: 0x00341AB5 File Offset: 0x0033FCB5
		// (set) Token: 0x060064A3 RID: 25763 RVA: 0x00341ABD File Offset: 0x0033FCBD
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the trigonometric function uses radians instead of degrees.", "", true)]
		public bool UseRadians { get; set; }

		// Token: 0x060064A4 RID: 25764 RVA: 0x00341AC6 File Offset: 0x0033FCC6
		public TrigonometricFunctionComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060064A5 RID: 25765 RVA: 0x00341AF0 File Offset: 0x0033FCF0
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.Function == TrigonometricFunctionComponent.FunctionType.Atan)
			{
				for (int i = 0; i < 2; i++)
				{
					this.timeSinceReceived[i] += deltaTime;
					if (this.timeSinceReceived[i] > 0.1f)
					{
						this.receivedSignal[i] = float.NaN;
					}
				}
				if (!float.IsNaN(this.receivedSignal[0]) && !float.IsNaN(this.receivedSignal[1]))
				{
					float angle = (float)Math.Atan2((double)this.receivedSignal[1], (double)this.receivedSignal[0]);
					if (!this.UseRadians)
					{
						angle = MathHelper.ToDegrees(angle);
					}
					this.item.SendSignal(new Signal(angle.ToString("G", CultureInfo.InvariantCulture), 0, this.signalSender, null, 0f, 1f), "signal_out");
				}
			}
		}

		// Token: 0x060064A6 RID: 25766 RVA: 0x00341BC0 File Offset: 0x0033FDC0
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			float value;
			float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
			bool sendOutputImmediately = true;
			this.signalSender = signal.sender;
			switch (this.Function)
			{
			case TrigonometricFunctionComponent.FunctionType.Sin:
				if (!this.UseRadians)
				{
					value = MathHelper.ToRadians(value);
				}
				value = MathF.Sin(value);
				break;
			case TrigonometricFunctionComponent.FunctionType.Cos:
				if (!this.UseRadians)
				{
					value = MathHelper.ToRadians(value);
				}
				value = MathF.Cos(value);
				break;
			case TrigonometricFunctionComponent.FunctionType.Tan:
				if (!this.UseRadians)
				{
					value = MathHelper.ToRadians(value);
				}
				if (!MathUtils.NearlyEqual(value % 3.1415927f, 1.5707964f, 0.0001f))
				{
					value = MathF.Tan(value);
				}
				break;
			case TrigonometricFunctionComponent.FunctionType.Asin:
				if (value >= -1f && value <= 1f)
				{
					float angle = MathF.Asin(value);
					if (!this.UseRadians)
					{
						angle = MathHelper.ToDegrees(angle);
					}
					value = angle;
				}
				break;
			case TrigonometricFunctionComponent.FunctionType.Acos:
				if (value >= -1f && value <= 1f)
				{
					float angle2 = MathF.Acos(value);
					if (!this.UseRadians)
					{
						angle2 = MathHelper.ToDegrees(angle2);
					}
					value = angle2;
				}
				break;
			case TrigonometricFunctionComponent.FunctionType.Atan:
				if (connection.Name == "signal_in_x")
				{
					this.timeSinceReceived[0] = 0f;
					float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[0]);
					sendOutputImmediately = false;
				}
				else if (connection.Name == "signal_in_y")
				{
					this.timeSinceReceived[1] = 0f;
					float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out this.receivedSignal[1]);
					sendOutputImmediately = false;
				}
				else
				{
					float angle3 = MathF.Atan(value);
					if (!this.UseRadians)
					{
						angle3 = MathHelper.ToDegrees(angle3);
					}
					value = angle3;
				}
				break;
			default:
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Function ");
				defaultInterpolatedStringHandler.AppendFormatted<TrigonometricFunctionComponent.FunctionType>(this.Function);
				defaultInterpolatedStringHandler.AppendLiteral(" has not been implemented.");
				throw new NotImplementedException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			}
			if (sendOutputImmediately)
			{
				signal.value = value.ToString("G", CultureInfo.InvariantCulture);
				this.item.SendSignal(signal, "signal_out");
			}
		}

		// Token: 0x04003439 RID: 13369
		private readonly float[] receivedSignal = new float[2];

		// Token: 0x0400343A RID: 13370
		private readonly float[] timeSinceReceived = new float[2];

		// Token: 0x0400343B RID: 13371
		protected Character signalSender;

		// Token: 0x020014A2 RID: 5282
		public enum FunctionType
		{
			// Token: 0x04006670 RID: 26224
			Sin,
			// Token: 0x04006671 RID: 26225
			Cos,
			// Token: 0x04006672 RID: 26226
			Tan,
			// Token: 0x04006673 RID: 26227
			Asin,
			// Token: 0x04006674 RID: 26228
			Acos,
			// Token: 0x04006675 RID: 26229
			Atan
		}
	}
}

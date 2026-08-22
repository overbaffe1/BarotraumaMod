using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000501 RID: 1281
	internal class TrigonometricFunctionComponent : ItemComponent
	{
		// Token: 0x17001349 RID: 4937
		// (get) Token: 0x060047D0 RID: 18384 RVA: 0x001C8DE8 File Offset: 0x001C6FE8
		// (set) Token: 0x060047D1 RID: 18385 RVA: 0x001C8DF0 File Offset: 0x001C6FF0
		[Serialize(TrigonometricFunctionComponent.FunctionType.Sin, IsPropertySaveable.No, "Which kind of function to run the input through.", "", true)]
		public TrigonometricFunctionComponent.FunctionType Function { get; set; }

		// Token: 0x1700134A RID: 4938
		// (get) Token: 0x060047D2 RID: 18386 RVA: 0x001C8DF9 File Offset: 0x001C6FF9
		// (set) Token: 0x060047D3 RID: 18387 RVA: 0x001C8E01 File Offset: 0x001C7001
		[InGameEditable]
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the trigonometric function uses radians instead of degrees.", "", true)]
		public bool UseRadians { get; set; }

		// Token: 0x060047D4 RID: 18388 RVA: 0x001C8E0A File Offset: 0x001C700A
		public TrigonometricFunctionComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060047D5 RID: 18389 RVA: 0x001C8E34 File Offset: 0x001C7034
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

		// Token: 0x060047D6 RID: 18390 RVA: 0x001C8F04 File Offset: 0x001C7104
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

		// Token: 0x040022B5 RID: 8885
		private readonly float[] receivedSignal = new float[2];

		// Token: 0x040022B6 RID: 8886
		private readonly float[] timeSinceReceived = new float[2];

		// Token: 0x040022B7 RID: 8887
		protected Character signalSender;

		// Token: 0x02000E39 RID: 3641
		public enum FunctionType
		{
			// Token: 0x0400422D RID: 16941
			Sin,
			// Token: 0x0400422E RID: 16942
			Cos,
			// Token: 0x0400422F RID: 16943
			Tan,
			// Token: 0x04004230 RID: 16944
			Asin,
			// Token: 0x04004231 RID: 16945
			Acos,
			// Token: 0x04004232 RID: 16946
			Atan
		}
	}
}

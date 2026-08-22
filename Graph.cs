using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x0200007A RID: 122
	internal class Graph
	{
		// Token: 0x06001135 RID: 4405 RVA: 0x000A9EB0 File Offset: 0x000A80B0
		public Graph(int arraySize = 100)
		{
			this.values = new float[arraySize];
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x000A9EC4 File Offset: 0x000A80C4
		public float LargestValue()
		{
			float maxValue = 0f;
			for (int i = 0; i < this.values.Length; i++)
			{
				if (this.values[i] > maxValue)
				{
					maxValue = this.values[i];
				}
			}
			return maxValue;
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x000A9EFF File Offset: 0x000A80FF
		public float Average()
		{
			if (this.values.Length != 0)
			{
				return this.values.Average();
			}
			return 0f;
		}

		// Token: 0x06001138 RID: 4408 RVA: 0x000A9F1C File Offset: 0x000A811C
		public void Update(float newValue)
		{
			for (int i = this.values.Length - 1; i > 0; i--)
			{
				this.values[i] = this.values[i - 1];
			}
			this.values[0] = newValue;
		}

		// Token: 0x06001139 RID: 4409 RVA: 0x000A9F5C File Offset: 0x000A815C
		public void Draw(SpriteBatch spriteBatch, Rectangle rect, float? maxValue = null, float xOffset = 0f, Color? color = null, Graph.GraphDelegate doForEachValue = null)
		{
			Color value2 = color.GetValueOrDefault();
			if (color == null)
			{
				value2 = Color.White;
				color = new Color?(value2);
			}
			float graphMaxVal = 1f;
			if (maxValue == null)
			{
				graphMaxVal = this.LargestValue();
			}
			else
			{
				float? num = maxValue;
				float num2 = 0f;
				if (num.GetValueOrDefault() > num2 & num != null)
				{
					graphMaxVal = maxValue.Value;
				}
			}
			GUI.DrawRectangle(spriteBatch, rect, Color.White, false, 0f, 1f);
			if (this.values.Length == 0)
			{
				return;
			}
			float lineWidth = (float)rect.Width / (float)(this.values.Length - 2);
			float yScale = (float)rect.Height / graphMaxVal;
			Vector2 prevPoint = new Vector2((float)rect.Right, (float)rect.Bottom - (this.values[1] + (this.values[0] - this.values[1]) * xOffset) * yScale);
			float currX = (float)rect.Right - (xOffset - 1f) * lineWidth;
			for (int i = 1; i < this.values.Length - 1; i++)
			{
				float value = this.values[i];
				currX -= lineWidth;
				Vector2 newPoint = new Vector2(currX, (float)rect.Bottom - value * yScale);
				GUI.DrawLine(spriteBatch, prevPoint, newPoint - new Vector2(1f, 0f), color.Value, 0f, 1f);
				prevPoint = newPoint;
				if (doForEachValue != null)
				{
					doForEachValue(spriteBatch, value, i, newPoint);
				}
			}
			int lastIndex = this.values.Length - 1;
			float lastValue = this.values[lastIndex];
			Vector2 lastPoint = new Vector2((float)rect.X, (float)rect.Bottom - (lastValue + (this.values[this.values.Length - 2] - lastValue) * xOffset) * yScale);
			GUI.DrawLine(spriteBatch, prevPoint, lastPoint, color.Value, 0f, 1f);
			if (doForEachValue != null)
			{
				doForEachValue(spriteBatch, lastValue, lastIndex, lastPoint);
			}
		}

		// Token: 0x0400089E RID: 2206
		private float[] values;

		// Token: 0x0200091B RID: 2331
		// (Invoke) Token: 0x060070D4 RID: 28884
		public delegate void GraphDelegate(SpriteBatch spriteBatch, float value, int order, Vector2 position);
	}
}

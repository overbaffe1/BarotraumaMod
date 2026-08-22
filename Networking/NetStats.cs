using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Networking
{
	// Token: 0x02000461 RID: 1121
	internal class NetStats
	{
		// Token: 0x06004BA8 RID: 19368 RVA: 0x0029BD54 File Offset: 0x00299F54
		public NetStats()
		{
			this.graphs = new Graph[3];
			this.totalValue = new float[3];
			this.lastValue = new float[3];
			for (int i = 0; i < 3; i++)
			{
				this.graphs[i] = new Graph(100);
			}
		}

		// Token: 0x06004BA9 RID: 19369 RVA: 0x0029BDA8 File Offset: 0x00299FA8
		public void AddValue(NetStats.NetStatType statType, float value)
		{
			float valueChange = value - this.lastValue[(int)statType];
			this.totalValue[(int)statType] += valueChange;
			this.lastValue[(int)statType] = value;
		}

		// Token: 0x06004BAA RID: 19370 RVA: 0x0029BDDC File Offset: 0x00299FDC
		public void Update(float deltaTime)
		{
			this.updateTimer -= deltaTime;
			if (this.updateTimer > 0f)
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				this.graphs[i].Update(this.totalValue[i] / 0.1f);
				this.totalValue[i] = 0f;
			}
			this.updateTimer = 0.1f;
		}

		// Token: 0x06004BAB RID: 19371 RVA: 0x0029BE44 File Offset: 0x0029A044
		public void Draw(SpriteBatch spriteBatch, Rectangle rect)
		{
			GUI.DrawRectangle(spriteBatch, rect, Color.Black * 0.4f, true, 0f, 1f);
			Graph graph = this.graphs[1];
			Rectangle rect2 = rect;
			Color? color = new Color?(Color.Cyan);
			graph.Draw(spriteBatch, rect2, null, 0f, color, null);
			this.graphs[0].Draw(spriteBatch, rect, null, 0f, new Color?(GUIStyle.Orange), null);
			if (this.graphs[2].Average() > 0f)
			{
				Graph graph2 = this.graphs[2];
				Rectangle rect3 = rect;
				color = new Color?(GUIStyle.Red);
				graph2.Draw(spriteBatch, rect3, null, 0f, color, null);
				GUIStyle.SmallFont.DrawString(spriteBatch, "Peak resent: " + this.graphs[2].LargestValue().ToString() + " messages/s", new Vector2((float)(rect.Right + 10), (float)(rect.Y + 50)), GUIStyle.Red, ForceUpperCase.Inherit, false);
			}
			GUIStyle.SmallFont.DrawString(spriteBatch, string.Concat(new string[]
			{
				"Peak received: ",
				MathUtils.GetBytesReadable((long)((int)this.graphs[1].LargestValue())),
				"/s      Avg received: ",
				MathUtils.GetBytesReadable((long)((int)this.graphs[1].Average())),
				"/s"
			}), new Vector2((float)(rect.Right + 10), (float)(rect.Y + 10)), Color.Cyan, ForceUpperCase.Inherit, false);
			GUIStyle.SmallFont.DrawString(spriteBatch, string.Concat(new string[]
			{
				"Peak sent: ",
				MathUtils.GetBytesReadable((long)((int)this.graphs[0].LargestValue())),
				"/s      Avg sent: ",
				MathUtils.GetBytesReadable((long)((int)this.graphs[0].Average())),
				"/s"
			}), new Vector2((float)(rect.Right + 10), (float)(rect.Y + 30)), GUIStyle.Orange, ForceUpperCase.Inherit, false);
		}

		// Token: 0x04002797 RID: 10135
		private readonly Graph[] graphs;

		// Token: 0x04002798 RID: 10136
		private readonly float[] totalValue;

		// Token: 0x04002799 RID: 10137
		private readonly float[] lastValue;

		// Token: 0x0400279A RID: 10138
		private const float UpdateInterval = 0.1f;

		// Token: 0x0400279B RID: 10139
		private float updateTimer;

		// Token: 0x020011EA RID: 4586
		public enum NetStatType
		{
			// Token: 0x04005D95 RID: 23957
			SentBytes,
			// Token: 0x04005D96 RID: 23958
			ReceivedBytes,
			// Token: 0x04005D97 RID: 23959
			ResentMessages
		}
	}
}

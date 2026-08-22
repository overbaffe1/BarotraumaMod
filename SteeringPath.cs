using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001A2 RID: 418
	internal class SteeringPath
	{
		// Token: 0x17000C58 RID: 3160
		// (get) Token: 0x06002FE2 RID: 12258 RVA: 0x001F87BD File Offset: 0x001F69BD
		// (set) Token: 0x06002FE3 RID: 12259 RVA: 0x001F87C5 File Offset: 0x001F69C5
		public bool Unreachable { get; set; }

		// Token: 0x17000C59 RID: 3161
		// (get) Token: 0x06002FE4 RID: 12260 RVA: 0x001F87CE File Offset: 0x001F69CE
		public float TotalLength
		{
			get
			{
				if (this.Unreachable)
				{
					return float.PositiveInfinity;
				}
				if (this.totalLength == null)
				{
					this.CalculateTotalLength();
				}
				return this.totalLength.Value;
			}
		}

		// Token: 0x06002FE5 RID: 12261 RVA: 0x001F87FC File Offset: 0x001F69FC
		public float GetLength(int? startIndex = null, int? endIndex = null)
		{
			if (this.Unreachable)
			{
				return float.PositiveInfinity;
			}
			int num = startIndex.GetValueOrDefault();
			if (startIndex == null)
			{
				num = 0;
				startIndex = new int?(num);
			}
			num = endIndex.GetValueOrDefault();
			if (endIndex == null)
			{
				num = this.Nodes.Count - 1;
				endIndex = new int?(num);
			}
			int? num2 = startIndex;
			num = 0;
			if (num2.GetValueOrDefault() == num & num2 != null)
			{
				num2 = endIndex;
				num = this.Nodes.Count - 1;
				if (num2.GetValueOrDefault() == num & num2 != null)
				{
					return this.TotalLength;
				}
			}
			if (this.totalLength == null)
			{
				this.CalculateTotalLength();
			}
			float length = 0f;
			for (int i = startIndex.Value; i < endIndex.Value; i++)
			{
				length += this.nodeDistances[i];
			}
			return length;
		}

		// Token: 0x06002FE6 RID: 12262 RVA: 0x001F88E0 File Offset: 0x001F6AE0
		private void CalculateTotalLength()
		{
			this.totalLength = new float?(0f);
			this.nodeDistances.Clear();
			for (int i = 0; i < this.nodes.Count - 1; i++)
			{
				float distance = Vector2.Distance(this.nodes[i].WorldPosition, this.nodes[i + 1].WorldPosition);
				this.totalLength += distance;
				this.nodeDistances.Add(distance);
			}
		}

		// Token: 0x06002FE7 RID: 12263 RVA: 0x001F8988 File Offset: 0x001F6B88
		public SteeringPath(bool unreachable = false)
		{
			this.nodes = new List<WayPoint>();
			this.Unreachable = unreachable;
		}

		// Token: 0x06002FE8 RID: 12264 RVA: 0x001F89AD File Offset: 0x001F6BAD
		public void AddNode(WayPoint node)
		{
			if (node == null)
			{
				return;
			}
			this.nodes.Add(node);
			if (node.CurrentHull == null)
			{
				this.HasOutdoorsNodes = true;
			}
		}

		// Token: 0x17000C5A RID: 3162
		// (get) Token: 0x06002FE9 RID: 12265 RVA: 0x001F89CE File Offset: 0x001F6BCE
		// (set) Token: 0x06002FEA RID: 12266 RVA: 0x001F89D6 File Offset: 0x001F6BD6
		public bool HasOutdoorsNodes { get; private set; }

		// Token: 0x17000C5B RID: 3163
		// (get) Token: 0x06002FEB RID: 12267 RVA: 0x001F89DF File Offset: 0x001F6BDF
		public int CurrentIndex
		{
			get
			{
				return this.currentIndex;
			}
		}

		// Token: 0x17000C5C RID: 3164
		// (get) Token: 0x06002FEC RID: 12268 RVA: 0x001F89E7 File Offset: 0x001F6BE7
		// (set) Token: 0x06002FED RID: 12269 RVA: 0x001F89EF File Offset: 0x001F6BEF
		public float Cost { get; set; }

		// Token: 0x17000C5D RID: 3165
		// (get) Token: 0x06002FEE RID: 12270 RVA: 0x001F89F8 File Offset: 0x001F6BF8
		public WayPoint PrevNode
		{
			get
			{
				if (this.currentIndex - 1 < 0 || this.currentIndex - 1 > this.nodes.Count - 1)
				{
					return null;
				}
				return this.nodes[this.currentIndex - 1];
			}
		}

		// Token: 0x17000C5E RID: 3166
		// (get) Token: 0x06002FEF RID: 12271 RVA: 0x001F8A31 File Offset: 0x001F6C31
		public WayPoint CurrentNode
		{
			get
			{
				if (this.currentIndex < 0 || this.currentIndex > this.nodes.Count - 1)
				{
					return null;
				}
				return this.nodes[this.currentIndex];
			}
		}

		// Token: 0x17000C5F RID: 3167
		// (get) Token: 0x06002FF0 RID: 12272 RVA: 0x001F8A64 File Offset: 0x001F6C64
		public bool IsAtEndNode
		{
			get
			{
				return this.currentIndex >= this.nodes.Count - 1;
			}
		}

		// Token: 0x17000C60 RID: 3168
		// (get) Token: 0x06002FF1 RID: 12273 RVA: 0x001F8A7E File Offset: 0x001F6C7E
		public List<WayPoint> Nodes
		{
			get
			{
				return this.nodes;
			}
		}

		// Token: 0x17000C61 RID: 3169
		// (get) Token: 0x06002FF2 RID: 12274 RVA: 0x001F8A86 File Offset: 0x001F6C86
		public WayPoint NextNode
		{
			get
			{
				if (this.currentIndex + 1 < 0 || this.currentIndex + 1 > this.nodes.Count - 1)
				{
					return null;
				}
				return this.nodes[this.currentIndex + 1];
			}
		}

		// Token: 0x17000C62 RID: 3170
		// (get) Token: 0x06002FF3 RID: 12275 RVA: 0x001F8ABF File Offset: 0x001F6CBF
		public bool Finished
		{
			get
			{
				return this.currentIndex >= this.nodes.Count;
			}
		}

		// Token: 0x06002FF4 RID: 12276 RVA: 0x001F8AD7 File Offset: 0x001F6CD7
		public void SkipToNextNode()
		{
			this.currentIndex++;
		}

		// Token: 0x06002FF5 RID: 12277 RVA: 0x001F8AE7 File Offset: 0x001F6CE7
		public void SkipToNode(int nodeIndex)
		{
			this.currentIndex = nodeIndex;
		}

		// Token: 0x06002FF6 RID: 12278 RVA: 0x001F8AF0 File Offset: 0x001F6CF0
		public WayPoint CheckProgress(Vector2 simPosition, float minSimDistance = 0.1f)
		{
			if (this.nodes.Count == 0 || this.currentIndex > this.nodes.Count - 1)
			{
				return null;
			}
			if (Vector2.Distance(simPosition, this.nodes[this.currentIndex].SimPosition) < minSimDistance)
			{
				this.currentIndex++;
			}
			return this.CurrentNode;
		}

		// Token: 0x06002FF7 RID: 12279 RVA: 0x001F8B54 File Offset: 0x001F6D54
		public void ClearPath()
		{
			this.nodes.Clear();
		}

		// Token: 0x040018F4 RID: 6388
		private List<WayPoint> nodes;

		// Token: 0x040018F5 RID: 6389
		private int currentIndex;

		// Token: 0x040018F6 RID: 6390
		private float? totalLength;

		// Token: 0x040018F8 RID: 6392
		private readonly List<float> nodeDistances = new List<float>();
	}
}

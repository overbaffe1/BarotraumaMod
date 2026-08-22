using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200009C RID: 156
	internal class SteeringPath
	{
		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x060012DA RID: 4826 RVA: 0x000A58DD File Offset: 0x000A3ADD
		// (set) Token: 0x060012DB RID: 4827 RVA: 0x000A58E5 File Offset: 0x000A3AE5
		public bool Unreachable { get; set; }

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x060012DC RID: 4828 RVA: 0x000A58EE File Offset: 0x000A3AEE
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

		// Token: 0x060012DD RID: 4829 RVA: 0x000A591C File Offset: 0x000A3B1C
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

		// Token: 0x060012DE RID: 4830 RVA: 0x000A5A00 File Offset: 0x000A3C00
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

		// Token: 0x060012DF RID: 4831 RVA: 0x000A5AA8 File Offset: 0x000A3CA8
		public SteeringPath(bool unreachable = false)
		{
			this.nodes = new List<WayPoint>();
			this.Unreachable = unreachable;
		}

		// Token: 0x060012E0 RID: 4832 RVA: 0x000A5ACD File Offset: 0x000A3CCD
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

		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x060012E1 RID: 4833 RVA: 0x000A5AEE File Offset: 0x000A3CEE
		// (set) Token: 0x060012E2 RID: 4834 RVA: 0x000A5AF6 File Offset: 0x000A3CF6
		public bool HasOutdoorsNodes { get; private set; }

		// Token: 0x17000541 RID: 1345
		// (get) Token: 0x060012E3 RID: 4835 RVA: 0x000A5AFF File Offset: 0x000A3CFF
		public int CurrentIndex
		{
			get
			{
				return this.currentIndex;
			}
		}

		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x060012E4 RID: 4836 RVA: 0x000A5B07 File Offset: 0x000A3D07
		// (set) Token: 0x060012E5 RID: 4837 RVA: 0x000A5B0F File Offset: 0x000A3D0F
		public float Cost { get; set; }

		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x060012E6 RID: 4838 RVA: 0x000A5B18 File Offset: 0x000A3D18
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

		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x060012E7 RID: 4839 RVA: 0x000A5B51 File Offset: 0x000A3D51
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

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x060012E8 RID: 4840 RVA: 0x000A5B84 File Offset: 0x000A3D84
		public bool IsAtEndNode
		{
			get
			{
				return this.currentIndex >= this.nodes.Count - 1;
			}
		}

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x060012E9 RID: 4841 RVA: 0x000A5B9E File Offset: 0x000A3D9E
		public List<WayPoint> Nodes
		{
			get
			{
				return this.nodes;
			}
		}

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x060012EA RID: 4842 RVA: 0x000A5BA6 File Offset: 0x000A3DA6
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

		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x060012EB RID: 4843 RVA: 0x000A5BDF File Offset: 0x000A3DDF
		public bool Finished
		{
			get
			{
				return this.currentIndex >= this.nodes.Count;
			}
		}

		// Token: 0x060012EC RID: 4844 RVA: 0x000A5BF7 File Offset: 0x000A3DF7
		public void SkipToNextNode()
		{
			this.currentIndex++;
		}

		// Token: 0x060012ED RID: 4845 RVA: 0x000A5C07 File Offset: 0x000A3E07
		public void SkipToNode(int nodeIndex)
		{
			this.currentIndex = nodeIndex;
		}

		// Token: 0x060012EE RID: 4846 RVA: 0x000A5C10 File Offset: 0x000A3E10
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

		// Token: 0x060012EF RID: 4847 RVA: 0x000A5C74 File Offset: 0x000A3E74
		public void ClearPath()
		{
			this.nodes.Clear();
		}

		// Token: 0x040008FA RID: 2298
		private List<WayPoint> nodes;

		// Token: 0x040008FB RID: 2299
		private int currentIndex;

		// Token: 0x040008FC RID: 2300
		private float? totalLength;

		// Token: 0x040008FE RID: 2302
		private readonly List<float> nodeDistances = new List<float>();
	}
}

using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000110 RID: 272
	[NullableContext(1)]
	[Nullable(0)]
	internal class NodeConnectionType
	{
		// Token: 0x170009EB RID: 2539
		// (get) Token: 0x060024E4 RID: 9444 RVA: 0x00174DF6 File Offset: 0x00172FF6
		public NodeConnectionType.Side NodeSide { get; }

		// Token: 0x170009EC RID: 2540
		// (get) Token: 0x060024E5 RID: 9445 RVA: 0x00174DFE File Offset: 0x00172FFE
		public string Label { get; }

		// Token: 0x170009ED RID: 2541
		// (get) Token: 0x060024E6 RID: 9446 RVA: 0x00174E06 File Offset: 0x00173006
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public NodeConnectionType[] AllowedConnections { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; }

		// Token: 0x060024E7 RID: 9447 RVA: 0x00174E0E File Offset: 0x0017300E
		private NodeConnectionType(NodeConnectionType.Side side, string name, [Nullable(new byte[]
		{
			2,
			1
		})] NodeConnectionType[] allowedConnections = null)
		{
			this.NodeSide = side;
			this.Label = name;
			this.AllowedConnections = allowedConnections;
		}

		// Token: 0x0400124E RID: 4686
		public static readonly NodeConnectionType Activate = new NodeConnectionType(NodeConnectionType.Side.Left, "Activate", null);

		// Token: 0x0400124F RID: 4687
		public static readonly NodeConnectionType Value = new NodeConnectionType(NodeConnectionType.Side.Left, "Value", null);

		// Token: 0x04001250 RID: 4688
		public static readonly NodeConnectionType Option = new NodeConnectionType(NodeConnectionType.Side.Right, "Option", new NodeConnectionType[]
		{
			NodeConnectionType.Activate
		});

		// Token: 0x04001251 RID: 4689
		public static readonly NodeConnectionType Add = new NodeConnectionType(NodeConnectionType.Side.Right, "Add", new NodeConnectionType[]
		{
			NodeConnectionType.Activate
		});

		// Token: 0x04001252 RID: 4690
		public static readonly NodeConnectionType Success = new NodeConnectionType(NodeConnectionType.Side.Right, "Success", new NodeConnectionType[]
		{
			NodeConnectionType.Activate
		});

		// Token: 0x04001253 RID: 4691
		public static readonly NodeConnectionType Failure = new NodeConnectionType(NodeConnectionType.Side.Right, "Failure", new NodeConnectionType[]
		{
			NodeConnectionType.Activate
		});

		// Token: 0x04001254 RID: 4692
		public static readonly NodeConnectionType Next = new NodeConnectionType(NodeConnectionType.Side.Right, "Next", new NodeConnectionType[]
		{
			NodeConnectionType.Activate
		});

		// Token: 0x04001255 RID: 4693
		public static readonly NodeConnectionType Out = new NodeConnectionType(NodeConnectionType.Side.Right, "Out", new NodeConnectionType[]
		{
			NodeConnectionType.Value
		});

		// Token: 0x02000C31 RID: 3121
		[NullableContext(0)]
		public enum Side
		{
			// Token: 0x04004A40 RID: 19008
			Left,
			// Token: 0x04004A41 RID: 19009
			Right
		}
	}
}

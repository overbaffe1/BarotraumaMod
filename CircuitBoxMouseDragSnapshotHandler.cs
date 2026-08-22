using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200003A RID: 58
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBoxMouseDragSnapshotHandler
	{
		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x0600091C RID: 2332 RVA: 0x00052684 File Offset: 0x00050884
		public IEnumerable<CircuitBoxNode> Nodes
		{
			get
			{
				CircuitBoxMouseDragSnapshotHandler.<get_Nodes>d__1 <get_Nodes>d__ = new CircuitBoxMouseDragSnapshotHandler.<get_Nodes>d__1(-2);
				<get_Nodes>d__.<>4__this = this;
				return <get_Nodes>d__;
			}
		}

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x0600091D RID: 2333 RVA: 0x000526A1 File Offset: 0x000508A1
		private IReadOnlyList<CircuitBoxWire> Wires
		{
			get
			{
				return this.circuitBoxUi.CircuitBox.Wires;
			}
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x000526B3 File Offset: 0x000508B3
		public ImmutableHashSet<CircuitBoxNode> GetLastComponentsUnderCursor()
		{
			return this.lastNodesUnderCursor;
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x000526BB File Offset: 0x000508BB
		public ImmutableHashSet<CircuitBoxNode> GetMoveAffectedComponents()
		{
			return this.moveAffectedComponents;
		}

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000920 RID: 2336 RVA: 0x000526C3 File Offset: 0x000508C3
		// (set) Token: 0x06000921 RID: 2337 RVA: 0x000526CB File Offset: 0x000508CB
		public bool IsDragging { get; private set; }

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000922 RID: 2338 RVA: 0x000526D4 File Offset: 0x000508D4
		// (set) Token: 0x06000923 RID: 2339 RVA: 0x000526DC File Offset: 0x000508DC
		public bool IsWiring { get; private set; }

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06000924 RID: 2340 RVA: 0x000526E5 File Offset: 0x000508E5
		// (set) Token: 0x06000925 RID: 2341 RVA: 0x000526ED File Offset: 0x000508ED
		public bool IsResizing { get; private set; }

		// Token: 0x06000926 RID: 2342 RVA: 0x000526F8 File Offset: 0x000508F8
		public CircuitBoxMouseDragSnapshotHandler(CircuitBoxUI ui)
		{
			Option.UnspecifiedNone none = Option.None;
			this.LastResizeAffectedNode = none;
			none = Option.None;
			this.LastConnectorUnderCursor = none;
			none = Option.None;
			this.LastWireUnderCursor = none;
			this.startClick = Vector2.Zero;
			base..ctor();
			this.circuitBoxUi = ui;
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x00052784 File Offset: 0x00050984
		public void StartDragging()
		{
			Vector2 cursorPos = this.circuitBoxUi.GetCursorPosition();
			this.SnapshotNodesUnderCursor(cursorPos);
			this.SnapshotSelectedNodes();
			this.SnapshotMoveAffectedNodes();
			this.startClick = cursorPos;
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x000527B8 File Offset: 0x000509B8
		public void ClearSnapshot()
		{
			this.lastNodesUnderCursor = ImmutableHashSet<CircuitBoxNode>.Empty;
			this.lastSelectedComponents = ImmutableHashSet<CircuitBoxNode>.Empty;
			this.moveAffectedComponents = ImmutableHashSet<CircuitBoxNode>.Empty;
			Option.UnspecifiedNone none = Option.None;
			this.LastConnectorUnderCursor = none;
			none = Option.None;
			this.LastWireUnderCursor = none;
			none = Option.None;
			this.LastResizeAffectedNode = none;
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x00052820 File Offset: 0x00050A20
		public void UpdateConnections()
		{
			ImmutableArray<CircuitBoxConnection>.Builder builder = ImmutableArray.CreateBuilder<CircuitBoxConnection>();
			builder.AddRange<CircuitBoxInputConnection>(this.circuitBoxUi.CircuitBox.Inputs);
			builder.AddRange<CircuitBoxOutputConnection>(this.circuitBoxUi.CircuitBox.Outputs);
			foreach (CircuitBoxNode node in this.Nodes)
			{
				builder.AddRange(node.Connectors);
			}
			this.connections = builder.ToImmutable();
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x000528B0 File Offset: 0x00050AB0
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxConnection> FindConnectorUnderCursor(Vector2 cursorPos)
		{
			foreach (CircuitBoxConnection connection in this.connections)
			{
				if (connection.Contains(cursorPos))
				{
					return Option.Some<CircuitBoxConnection>(connection);
				}
			}
			Option.UnspecifiedNone none = Option.None;
			return none;
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x000528FC File Offset: 0x00050AFC
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxWire> FindWireUnderCursor(Vector2 cursorPos)
		{
			foreach (CircuitBoxWire wire in this.Wires)
			{
				if ((wire == null || !wire.IsSelected || wire.IsSelectedByMe) && wire.Renderer.Contains(cursorPos))
				{
					return Option.Some<CircuitBoxWire>(wire);
				}
			}
			Option.UnspecifiedNone none = Option.None;
			return none;
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x0005297C File Offset: 0x00050B7C
		public ImmutableHashSet<CircuitBoxNode> FindNodesUnderCursor(Vector2 cursorPos)
		{
			ImmutableHashSet<CircuitBoxNode>.Builder builder = ImmutableHashSet.CreateBuilder<CircuitBoxNode>();
			foreach (CircuitBoxNode node in this.Nodes)
			{
				if ((node == null || !node.IsSelected || node.IsSelectedByMe) && node.Rect.Contains(cursorPos))
				{
					builder.Add(node);
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x000529F8 File Offset: 0x00050BF8
		private void SnapshotNodesUnderCursor(Vector2 cursorPos)
		{
			this.lastNodesUnderCursor = this.FindNodesUnderCursor(cursorPos);
			this.LastConnectorUnderCursor = this.FindConnectorUnderCursor(cursorPos);
			this.LastWireUnderCursor = this.FindWireUnderCursor(cursorPos);
			this.LastResizeAffectedNode = this.FindResizeBorderUnderCursor(this.lastNodesUnderCursor, cursorPos);
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x00052A34 File Offset: 0x00050C34
		[return: Nullable(new byte[]
		{
			0,
			0,
			1
		})]
		private Option<ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode>> FindResizeBorderUnderCursor(ImmutableHashSet<CircuitBoxNode> nodes, Vector2 cursorPos)
		{
			if (!nodes.Any<CircuitBoxNode>())
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			CircuitBoxNode node = this.circuitBoxUi.GetTopmostNode(nodes);
			if (node == null || !node.IsResizable)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			RectangleF rect = node.Rect;
			RectangleF bottomBorder = new RectangleF(rect.X, rect.Top, rect.Width, 32f);
			RectangleF rightBorder = new RectangleF(rect.Right - 32f, rect.Y, 32f, rect.Height);
			RectangleF leftBorder = new RectangleF(rect.X, rect.Y, 32f, rect.Height);
			bool hoverBottom = bottomBorder.Contains(cursorPos);
			bool hoverRight = rightBorder.Contains(cursorPos);
			bool hoverLeft = leftBorder.Contains(cursorPos);
			CircuitBoxResizeDirection dir = CircuitBoxResizeDirection.None;
			if (hoverBottom)
			{
				dir |= CircuitBoxResizeDirection.Down;
			}
			if (hoverRight)
			{
				dir |= CircuitBoxResizeDirection.Right;
			}
			if (hoverLeft)
			{
				dir |= CircuitBoxResizeDirection.Left;
			}
			if (dir == CircuitBoxResizeDirection.None)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			return Option.Some<ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode>>(new ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode>(dir, node));
		}

		// Token: 0x0600092F RID: 2351 RVA: 0x00052B47 File Offset: 0x00050D47
		private void SnapshotSelectedNodes()
		{
			this.lastSelectedComponents = (from n in this.Nodes
			where n != null && n.IsSelected && n.IsSelectedByMe
			select n).ToImmutableHashSet<CircuitBoxNode>();
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x00052B80 File Offset: 0x00050D80
		private void SnapshotMoveAffectedNodes()
		{
			bool moveSelection = this.lastNodesUnderCursor.Any((CircuitBoxNode node) => this.lastSelectedComponents.Contains(node));
			ImmutableHashSet<CircuitBoxNode> immutableHashSet;
			if (moveSelection)
			{
				immutableHashSet = this.lastSelectedComponents;
			}
			else
			{
				CircuitBoxNode node2 = this.circuitBoxUi.GetTopmostNode(this.lastNodesUnderCursor);
				ImmutableHashSet<CircuitBoxNode> immutableHashSet2;
				if (node2 == null)
				{
					immutableHashSet2 = ImmutableHashSet<CircuitBoxNode>.Empty;
				}
				else
				{
					immutableHashSet2 = ImmutableHashSet.Create<CircuitBoxNode>(node2);
				}
				immutableHashSet = immutableHashSet2;
			}
			this.moveAffectedComponents = immutableHashSet;
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x00052BDE File Offset: 0x00050DDE
		public Vector2 GetDragAmount(Vector2 mousePos)
		{
			return mousePos - this.startClick;
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x00052BEC File Offset: 0x00050DEC
		public void EndDragging()
		{
			this.startClick = Vector2.Zero;
			this.IsDragging = false;
			this.IsWiring = false;
			this.IsResizing = false;
			this.lastNodesUnderCursor = ImmutableHashSet<CircuitBoxNode>.Empty;
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x00052C1C File Offset: 0x00050E1C
		public void UpdateDrag(Vector2 cursorPos)
		{
			if (this.LastConnectorUnderCursor.IsNone())
			{
				this.IsWiring = false;
			}
			if (this.lastNodesUnderCursor.IsEmpty)
			{
				this.IsDragging = false;
			}
			if (this.LastResizeAffectedNode.IsNone())
			{
				this.IsResizing = false;
			}
			if (this.startClick == Vector2.Zero)
			{
				this.IsDragging = false;
				this.IsWiring = false;
				this.IsResizing = false;
				return;
			}
			if (this.circuitBoxUi.Locked)
			{
				return;
			}
			bool isDragThresholdExceeded = Vector2.DistanceSquared(this.startClick, cursorPos) > 256f;
			if (this.LastConnectorUnderCursor.IsSome())
			{
				this.IsWiring = (this.IsWiring || isDragThresholdExceeded);
				return;
			}
			if (this.LastResizeAffectedNode.IsSome())
			{
				this.IsResizing = (this.IsResizing || isDragThresholdExceeded);
				return;
			}
			this.IsDragging = (this.IsDragging || isDragThresholdExceeded);
		}

		// Token: 0x040004B9 RID: 1209
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private ImmutableArray<CircuitBoxConnection> connections = ImmutableArray<CircuitBoxConnection>.Empty;

		// Token: 0x040004BA RID: 1210
		private ImmutableHashSet<CircuitBoxNode> lastNodesUnderCursor = ImmutableHashSet<CircuitBoxNode>.Empty;

		// Token: 0x040004BB RID: 1211
		private ImmutableHashSet<CircuitBoxNode> lastSelectedComponents = ImmutableHashSet<CircuitBoxNode>.Empty;

		// Token: 0x040004BC RID: 1212
		private ImmutableHashSet<CircuitBoxNode> moveAffectedComponents = ImmutableHashSet<CircuitBoxNode>.Empty;

		// Token: 0x040004BD RID: 1213
		[Nullable(new byte[]
		{
			0,
			0,
			1
		})]
		public Option<ValueTuple<CircuitBoxResizeDirection, CircuitBoxNode>> LastResizeAffectedNode;

		// Token: 0x040004BE RID: 1214
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxConnection> LastConnectorUnderCursor;

		// Token: 0x040004BF RID: 1215
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxWire> LastWireUnderCursor;

		// Token: 0x040004C3 RID: 1219
		private Vector2 startClick;

		// Token: 0x040004C4 RID: 1220
		private readonly CircuitBoxUI circuitBoxUi;

		// Token: 0x040004C5 RID: 1221
		private const float dragTreshold = 16f;
	}
}

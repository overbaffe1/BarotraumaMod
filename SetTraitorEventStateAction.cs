using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A4 RID: 676
	[NullableContext(1)]
	[Nullable(0)]
	internal class SetTraitorEventStateAction : EventAction
	{
		// Token: 0x06003ACB RID: 15051 RVA: 0x0022089C File Offset: 0x0021EA9C
		public SetTraitorEventStateAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			TraitorEvent traitorEvent = parentEvent as TraitorEvent;
			if (traitorEvent != null)
			{
				this.traitorEvent = traitorEvent;
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(72, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Cannot use the action ");
			defaultInterpolatedStringHandler.AppendFormatted("SetTraitorEventStateAction");
			defaultInterpolatedStringHandler.AppendLiteral(" in the event \"");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral("\" because it's not a traitor event.");
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
		}

		// Token: 0x17000F7B RID: 3963
		// (get) Token: 0x06003ACC RID: 15052 RVA: 0x00220924 File Offset: 0x0021EB24
		// (set) Token: 0x06003ACD RID: 15053 RVA: 0x0022092C File Offset: 0x0021EB2C
		[Serialize(TraitorEvent.State.Completed, IsPropertySaveable.Yes, "The state to set the traitor event to (Incomplete, Completed or Failed).", "", false)]
		public TraitorEvent.State State { get; set; }

		// Token: 0x06003ACE RID: 15054 RVA: 0x00220935 File Offset: 0x0021EB35
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003ACF RID: 15055 RVA: 0x0022093D File Offset: 0x0021EB3D
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003AD0 RID: 15056 RVA: 0x00220946 File Offset: 0x0021EB46
		public override void Update(float deltaTime)
		{
			if (this.isFinished || this.traitorEvent == null)
			{
				return;
			}
			this.traitorEvent.CurrentState = this.State;
			this.isFinished = true;
		}

		// Token: 0x06003AD1 RID: 15057 RVA: 0x00220974 File Offset: 0x0021EB74
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("SetTraitorEventStateAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (State: ");
			defaultInterpolatedStringHandler.AppendFormatted<TraitorEvent.State>(this.State);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E2A RID: 7722
		[Nullable(2)]
		private readonly TraitorEvent traitorEvent;

		// Token: 0x04001E2C RID: 7724
		private bool isFinished;
	}
}

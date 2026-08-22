using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001B2 RID: 434
	[NullableContext(1)]
	[Nullable(0)]
	internal class SetTraitorEventStateAction : EventAction
	{
		// Token: 0x0600201F RID: 8223 RVA: 0x000DAD80 File Offset: 0x000D8F80
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

		// Token: 0x17000935 RID: 2357
		// (get) Token: 0x06002020 RID: 8224 RVA: 0x000DAE08 File Offset: 0x000D9008
		// (set) Token: 0x06002021 RID: 8225 RVA: 0x000DAE10 File Offset: 0x000D9010
		[Serialize(TraitorEvent.State.Completed, IsPropertySaveable.Yes, "The state to set the traitor event to (Incomplete, Completed or Failed).", "", false)]
		public TraitorEvent.State State { get; set; }

		// Token: 0x06002022 RID: 8226 RVA: 0x000DAE19 File Offset: 0x000D9019
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06002023 RID: 8227 RVA: 0x000DAE21 File Offset: 0x000D9021
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06002024 RID: 8228 RVA: 0x000DAE2A File Offset: 0x000D902A
		public override void Update(float deltaTime)
		{
			if (this.isFinished || this.traitorEvent == null)
			{
				return;
			}
			this.traitorEvent.CurrentState = this.State;
			this.isFinished = true;
		}

		// Token: 0x06002025 RID: 8229 RVA: 0x000DAE58 File Offset: 0x000D9058
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

		// Token: 0x04000F41 RID: 3905
		[Nullable(2)]
		private readonly TraitorEvent traitorEvent;

		// Token: 0x04000F43 RID: 3907
		private bool isFinished;
	}
}

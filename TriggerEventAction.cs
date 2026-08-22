using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002AB RID: 683
	internal class TriggerEventAction : EventAction
	{
		// Token: 0x17000FB2 RID: 4018
		// (get) Token: 0x06003B99 RID: 15257 RVA: 0x002241B8 File Offset: 0x002223B8
		// (set) Token: 0x06003B9A RID: 15258 RVA: 0x002241C0 File Offset: 0x002223C0
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the event to trigger.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x17000FB3 RID: 4019
		// (get) Token: 0x06003B9B RID: 15259 RVA: 0x002241C9 File Offset: 0x002223C9
		// (set) Token: 0x06003B9C RID: 15260 RVA: 0x002241D1 File Offset: 0x002223D1
		[Serialize("", IsPropertySaveable.Yes, "Tag of the event to trigger.", "", false)]
		public Identifier EventTag { get; set; }

		// Token: 0x17000FB4 RID: 4020
		// (get) Token: 0x06003B9D RID: 15261 RVA: 0x002241DA File Offset: 0x002223DA
		// (set) Token: 0x06003B9E RID: 15262 RVA: 0x002241E2 File Offset: 0x002223E2
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the event will trigger at the beginning of the next round. Useful for e.g. triggering some scripted event in the outpost after you finish a mission.", "", false)]
		public bool NextRound { get; set; }

		// Token: 0x06003B9F RID: 15263 RVA: 0x002241EB File Offset: 0x002223EB
		public TriggerEventAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003BA0 RID: 15264 RVA: 0x002241F5 File Offset: 0x002223F5
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003BA1 RID: 15265 RVA: 0x002241FD File Offset: 0x002223FD
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003BA2 RID: 15266 RVA: 0x00224208 File Offset: 0x00222408
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.EventManager : null) != null)
			{
				if (this.NextRound)
				{
					GameMain.GameSession.EventManager.QueuedEventsForNextRound.Enqueue(this.Identifier);
				}
				else
				{
					EventPrefab eventPrefab = EventPrefab.FindEventPrefab(this.Identifier, this.EventTag, this.ParentEvent.Prefab.ContentPackage);
					if (eventPrefab != null)
					{
						Event ev = eventPrefab.CreateInstance(GameMain.GameSession.EventManager.RandomSeed);
						if (ev != null)
						{
							GameMain.GameSession.EventManager.QueuedEvents.Enqueue(ev);
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003BA3 RID: 15267 RVA: 0x002242B0 File Offset: 0x002224B0
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("TriggerEventAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (EventPrefab: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Identifier.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04001E78 RID: 7800
		private bool isFinished;
	}
}

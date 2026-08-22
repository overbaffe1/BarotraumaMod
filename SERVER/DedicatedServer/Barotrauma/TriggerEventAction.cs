using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001B8 RID: 440
	internal class TriggerEventAction : EventAction
	{
		// Token: 0x1700096B RID: 2411
		// (get) Token: 0x060020E5 RID: 8421 RVA: 0x000DE36C File Offset: 0x000DC56C
		// (set) Token: 0x060020E6 RID: 8422 RVA: 0x000DE374 File Offset: 0x000DC574
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the event to trigger.", "", false)]
		public Identifier Identifier { get; set; }

		// Token: 0x1700096C RID: 2412
		// (get) Token: 0x060020E7 RID: 8423 RVA: 0x000DE37D File Offset: 0x000DC57D
		// (set) Token: 0x060020E8 RID: 8424 RVA: 0x000DE385 File Offset: 0x000DC585
		[Serialize("", IsPropertySaveable.Yes, "Tag of the event to trigger.", "", false)]
		public Identifier EventTag { get; set; }

		// Token: 0x1700096D RID: 2413
		// (get) Token: 0x060020E9 RID: 8425 RVA: 0x000DE38E File Offset: 0x000DC58E
		// (set) Token: 0x060020EA RID: 8426 RVA: 0x000DE396 File Offset: 0x000DC596
		[Serialize(false, IsPropertySaveable.Yes, "If set to true, the event will trigger at the beginning of the next round. Useful for e.g. triggering some scripted event in the outpost after you finish a mission.", "", false)]
		public bool NextRound { get; set; }

		// Token: 0x060020EB RID: 8427 RVA: 0x000DE39F File Offset: 0x000DC59F
		public TriggerEventAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060020EC RID: 8428 RVA: 0x000DE3A9 File Offset: 0x000DC5A9
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x060020ED RID: 8429 RVA: 0x000DE3B1 File Offset: 0x000DC5B1
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060020EE RID: 8430 RVA: 0x000DE3BC File Offset: 0x000DC5BC
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

		// Token: 0x060020EF RID: 8431 RVA: 0x000DE464 File Offset: 0x000DC664
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

		// Token: 0x04000F8B RID: 3979
		private bool isFinished;
	}
}

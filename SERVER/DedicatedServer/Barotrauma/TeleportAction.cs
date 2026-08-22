using System;

namespace Barotrauma
{
	// Token: 0x020001B6 RID: 438
	internal class TeleportAction : EventAction
	{
		// Token: 0x1700095A RID: 2394
		// (get) Token: 0x060020B6 RID: 8374 RVA: 0x000DD480 File Offset: 0x000DB680
		// (set) Token: 0x060020B7 RID: 8375 RVA: 0x000DD488 File Offset: 0x000DB688
		[Serialize(TeleportAction.TeleportPosition.MainSub, IsPropertySaveable.Yes, "Should the entity be teleported to the main submarine or the outpost?", "", false)]
		public TeleportAction.TeleportPosition Position { get; set; }

		// Token: 0x1700095B RID: 2395
		// (get) Token: 0x060020B8 RID: 8376 RVA: 0x000DD491 File Offset: 0x000DB691
		// (set) Token: 0x060020B9 RID: 8377 RVA: 0x000DD499 File Offset: 0x000DB699
		[Serialize(SpawnType.Human, IsPropertySaveable.Yes, "The type of the spawnpoint to teleport the character to.", "", false)]
		public SpawnType SpawnType { get; set; }

		// Token: 0x1700095C RID: 2396
		// (get) Token: 0x060020BA RID: 8378 RVA: 0x000DD4A2 File Offset: 0x000DB6A2
		// (set) Token: 0x060020BB RID: 8379 RVA: 0x000DD4AA File Offset: 0x000DB6AA
		[Serialize("", IsPropertySaveable.Yes, "Optional tag of the spawnpoint.", "", false)]
		public string SpawnPointTag { get; set; }

		// Token: 0x1700095D RID: 2397
		// (get) Token: 0x060020BC RID: 8380 RVA: 0x000DD4B3 File Offset: 0x000DB6B3
		// (set) Token: 0x060020BD RID: 8381 RVA: 0x000DD4BB File Offset: 0x000DB6BB
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target(s) to teleport.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x060020BE RID: 8382 RVA: 0x000DD4C4 File Offset: 0x000DB6C4
		public TeleportAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060020BF RID: 8383 RVA: 0x000DD4D0 File Offset: 0x000DB6D0
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			TeleportAction.TeleportPosition position = this.Position;
			Submarine submarine;
			if (position != TeleportAction.TeleportPosition.MainSub)
			{
				if (position != TeleportAction.TeleportPosition.Outpost)
				{
					submarine = null;
				}
				else
				{
					GameSession gameSession = GameMain.GameSession;
					Submarine submarine2;
					if (gameSession == null)
					{
						submarine2 = null;
					}
					else
					{
						Level level = gameSession.Level;
						submarine2 = ((level != null) ? level.StartOutpost : null);
					}
					submarine = submarine2;
				}
			}
			else
			{
				submarine = Submarine.MainSub;
			}
			Submarine sub = submarine;
			WayPoint wp = WayPoint.GetRandom(this.SpawnType, null, sub, false, this.SpawnPointTag, false);
			if (wp != null)
			{
				foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
				{
					Character c = target as Character;
					if (c != null)
					{
						c.TeleportTo(wp.WorldPosition);
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x060020C0 RID: 8384 RVA: 0x000DD5A8 File Offset: 0x000DB7A8
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x060020C1 RID: 8385 RVA: 0x000DD5B0 File Offset: 0x000DB7B0
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000F75 RID: 3957
		private bool isFinished;

		// Token: 0x02000938 RID: 2360
		public enum TeleportPosition
		{
			// Token: 0x0400326C RID: 12908
			MainSub,
			// Token: 0x0400326D RID: 12909
			Outpost
		}
	}
}

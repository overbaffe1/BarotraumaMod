using System;

namespace Barotrauma
{
	// Token: 0x020002A9 RID: 681
	internal class TeleportAction : EventAction
	{
		// Token: 0x17000FA1 RID: 4001
		// (get) Token: 0x06003B6A RID: 15210 RVA: 0x002232F0 File Offset: 0x002214F0
		// (set) Token: 0x06003B6B RID: 15211 RVA: 0x002232F8 File Offset: 0x002214F8
		[Serialize(TeleportAction.TeleportPosition.MainSub, IsPropertySaveable.Yes, "Should the entity be teleported to the main submarine or the outpost?", "", false)]
		public TeleportAction.TeleportPosition Position { get; set; }

		// Token: 0x17000FA2 RID: 4002
		// (get) Token: 0x06003B6C RID: 15212 RVA: 0x00223301 File Offset: 0x00221501
		// (set) Token: 0x06003B6D RID: 15213 RVA: 0x00223309 File Offset: 0x00221509
		[Serialize(SpawnType.Human, IsPropertySaveable.Yes, "The type of the spawnpoint to teleport the character to.", "", false)]
		public SpawnType SpawnType { get; set; }

		// Token: 0x17000FA3 RID: 4003
		// (get) Token: 0x06003B6E RID: 15214 RVA: 0x00223312 File Offset: 0x00221512
		// (set) Token: 0x06003B6F RID: 15215 RVA: 0x0022331A File Offset: 0x0022151A
		[Serialize("", IsPropertySaveable.Yes, "Optional tag of the spawnpoint.", "", false)]
		public string SpawnPointTag { get; set; }

		// Token: 0x17000FA4 RID: 4004
		// (get) Token: 0x06003B70 RID: 15216 RVA: 0x00223323 File Offset: 0x00221523
		// (set) Token: 0x06003B71 RID: 15217 RVA: 0x0022332B File Offset: 0x0022152B
		[Serialize("", IsPropertySaveable.Yes, "Tag of the target(s) to teleport.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06003B72 RID: 15218 RVA: 0x00223334 File Offset: 0x00221534
		public TeleportAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003B73 RID: 15219 RVA: 0x00223340 File Offset: 0x00221540
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

		// Token: 0x06003B74 RID: 15220 RVA: 0x00223418 File Offset: 0x00221618
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06003B75 RID: 15221 RVA: 0x00223420 File Offset: 0x00221620
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04001E62 RID: 7778
		private bool isFinished;

		// Token: 0x02000F47 RID: 3911
		public enum TeleportPosition
		{
			// Token: 0x04005533 RID: 21811
			MainSub,
			// Token: 0x04005534 RID: 21812
			Outpost
		}
	}
}

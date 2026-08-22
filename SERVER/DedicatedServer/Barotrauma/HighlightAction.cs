using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200001B RID: 27
	[NullableContext(1)]
	[Nullable(0)]
	internal class HighlightAction : EventAction
	{
		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060003DE RID: 990 RVA: 0x00020251 File Offset: 0x0001E451
		// (set) Token: 0x060003DF RID: 991 RVA: 0x00020259 File Offset: 0x0001E459
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to highlight.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060003E0 RID: 992 RVA: 0x00020262 File Offset: 0x0001E462
		// (set) Token: 0x060003E1 RID: 993 RVA: 0x0002026A File Offset: 0x0001E46A
		[Serialize("", IsPropertySaveable.Yes, "Only the player controlling this character will see the highlight. If empty, all players will see it.", "", false)]
		public Identifier TargetCharacter { get; set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060003E2 RID: 994 RVA: 0x00020273 File Offset: 0x0001E473
		// (set) Token: 0x060003E3 RID: 995 RVA: 0x0002027B File Offset: 0x0001E47B
		[Serialize(true, IsPropertySaveable.Yes, "Should the highlight be turned on or off?", "", false)]
		public bool State { get; set; }

		// Token: 0x060003E4 RID: 996 RVA: 0x00020284 File Offset: 0x0001E484
		public HighlightAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00020290 File Offset: 0x0001E490
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Character> targetCharacters = this.TargetCharacter.IsEmpty ? null : this.ParentEvent.GetTargets(this.TargetCharacter).OfType<Character>();
			foreach (Entity target in this.ParentEvent.GetTargets(this.TargetTag))
			{
				this.SetHighlightProjSpecific(target, targetCharacters);
			}
			this.isFinished = true;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00020324 File Offset: 0x0001E524
		private void SetHighlightProjSpecific(Entity entity, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<Character> targetCharacters)
		{
			Item item = entity as Item;
			if (item != null && GameMain.Server != null)
			{
				IEnumerable<Client> targetClients = null;
				if (targetCharacters != null)
				{
					targetClients = from c in targetCharacters
					select GameMain.Server.ConnectedClients.FirstOrDefault((Client client) => client.Character == c) into c
					where c != null
					select c;
				}
				GameServer server = GameMain.Server;
				if (server == null)
				{
					return;
				}
				server.CreateEntityEvent(item, new Item.SetHighlightEventData(this.State, HighlightAction.highlightColor, targetClients));
			}
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x000203B7 File Offset: 0x0001E5B7
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x060003E8 RID: 1000 RVA: 0x000203BF File Offset: 0x0001E5BF
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x040001C9 RID: 457
		private static readonly Color highlightColor = Color.Orange;

		// Token: 0x040001CD RID: 461
		private bool isFinished;
	}
}

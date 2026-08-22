using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000048 RID: 72
	[NullableContext(1)]
	[Nullable(0)]
	internal class HighlightAction : EventAction
	{
		// Token: 0x06000A85 RID: 2693 RVA: 0x000622D8 File Offset: 0x000604D8
		private void SetItemHighlight(Item item)
		{
			if (item.ExternalHighlight == this.State)
			{
				return;
			}
			item.HighlightColor = (this.State ? new Color?(HighlightAction.highlightColor) : null);
			item.ExternalHighlight = this.State;
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x00062323 File Offset: 0x00060523
		private void SetStructureHighlight(Structure structure)
		{
			structure.SpriteColor = (this.State ? HighlightAction.highlightColor : Color.White);
			structure.ExternalHighlight = this.State;
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x0006234B File Offset: 0x0006054B
		private void SetCharacterHighlight(Character character)
		{
			character.ExternalHighlight = this.State;
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000A88 RID: 2696 RVA: 0x00062359 File Offset: 0x00060559
		// (set) Token: 0x06000A89 RID: 2697 RVA: 0x00062361 File Offset: 0x00060561
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity to highlight.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000A8A RID: 2698 RVA: 0x0006236A File Offset: 0x0006056A
		// (set) Token: 0x06000A8B RID: 2699 RVA: 0x00062372 File Offset: 0x00060572
		[Serialize("", IsPropertySaveable.Yes, "Only the player controlling this character will see the highlight. If empty, all players will see it.", "", false)]
		public Identifier TargetCharacter { get; set; }

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000A8C RID: 2700 RVA: 0x0006237B File Offset: 0x0006057B
		// (set) Token: 0x06000A8D RID: 2701 RVA: 0x00062383 File Offset: 0x00060583
		[Serialize(true, IsPropertySaveable.Yes, "Should the highlight be turned on or off?", "", false)]
		public bool State { get; set; }

		// Token: 0x06000A8E RID: 2702 RVA: 0x0006238C File Offset: 0x0006058C
		public HighlightAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x00062398 File Offset: 0x00060598
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

		// Token: 0x06000A90 RID: 2704 RVA: 0x0006242C File Offset: 0x0006062C
		private void SetHighlightProjSpecific(Entity entity, [Nullable(new byte[]
		{
			2,
			1
		})] IEnumerable<Character> targetCharacters)
		{
			if (targetCharacters != null && !targetCharacters.Contains(Character.Controlled))
			{
				return;
			}
			Item i = entity as Item;
			if (i != null)
			{
				this.SetItemHighlight(i);
				return;
			}
			Structure s = entity as Structure;
			if (s != null)
			{
				this.SetStructureHighlight(s);
				return;
			}
			Character c = entity as Character;
			if (c != null)
			{
				this.SetCharacterHighlight(c);
			}
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x0006247F File Offset: 0x0006067F
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x00062487 File Offset: 0x00060687
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x04000564 RID: 1380
		private static readonly Color highlightColor = Color.Orange;

		// Token: 0x04000568 RID: 1384
		private bool isFinished;
	}
}

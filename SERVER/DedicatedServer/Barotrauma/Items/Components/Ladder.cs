using System;
using System.Collections.Generic;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004C6 RID: 1222
	internal class Ladder : ItemComponent
	{
		// Token: 0x170012BD RID: 4797
		// (get) Token: 0x060045EA RID: 17898 RVA: 0x001BFE72 File Offset: 0x001BE072
		public static List<Ladder> List { get; } = new List<Ladder>();

		// Token: 0x060045EB RID: 17899 RVA: 0x001BFE79 File Offset: 0x001BE079
		public Ladder(Item item, ContentXElement element) : base(item, element)
		{
			Ladder.List.Add(this);
		}

		// Token: 0x060045EC RID: 17900 RVA: 0x001BFE8E File Offset: 0x001BE08E
		public override bool Select(Character character)
		{
			if (character == null || character.LockHands || character.Removed)
			{
				return false;
			}
			if (!character.CanClimb)
			{
				return false;
			}
			character.AnimController.StartClimbing();
			return true;
		}

		// Token: 0x060045ED RID: 17901 RVA: 0x001BFEBB File Offset: 0x001BE0BB
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Ladder.List.Remove(this);
		}
	}
}

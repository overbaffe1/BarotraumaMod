using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004DF RID: 1247
	internal class RemoteController : ItemComponent
	{
		// Token: 0x170012FB RID: 4859
		// (get) Token: 0x060046C5 RID: 18117 RVA: 0x001C4425 File Offset: 0x001C2625
		// (set) Token: 0x060046C6 RID: 18118 RVA: 0x001C442D File Offset: 0x001C262D
		[Serialize("", IsPropertySaveable.No, "Tag or identifier of the item that should be controlled.", "", false)]
		public Identifier Target { get; private set; }

		// Token: 0x170012FC RID: 4860
		// (get) Token: 0x060046C7 RID: 18119 RVA: 0x001C4436 File Offset: 0x001C2636
		// (set) Token: 0x060046C8 RID: 18120 RVA: 0x001C443E File Offset: 0x001C263E
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool OnlyInOwnSub { get; private set; }

		// Token: 0x170012FD RID: 4861
		// (get) Token: 0x060046C9 RID: 18121 RVA: 0x001C4447 File Offset: 0x001C2647
		// (set) Token: 0x060046CA RID: 18122 RVA: 0x001C444F File Offset: 0x001C264F
		[Serialize(10000f, IsPropertySaveable.No, "", "", false)]
		public float Range { get; private set; }

		// Token: 0x170012FE RID: 4862
		// (get) Token: 0x060046CB RID: 18123 RVA: 0x001C4458 File Offset: 0x001C2658
		public Item TargetItem
		{
			get
			{
				return this.currentTarget;
			}
		}

		// Token: 0x060046CC RID: 18124 RVA: 0x001C4460 File Offset: 0x001C2660
		public RemoteController(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x060046CD RID: 18125 RVA: 0x001C446A File Offset: 0x001C266A
		public override bool Select(Character character)
		{
			if (base.Select(character))
			{
				this.FindTarget(character);
				return true;
			}
			return false;
		}

		// Token: 0x060046CE RID: 18126 RVA: 0x001C447F File Offset: 0x001C267F
		public override void Equip(Character character)
		{
			this.FindTarget(character);
		}

		// Token: 0x060046CF RID: 18127 RVA: 0x001C4488 File Offset: 0x001C2688
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (this.currentTarget.Removed || this.item.Submarine != this.currentSub || Vector2.DistanceSquared(this.currentTarget.WorldPosition, this.item.WorldPosition) > this.Range * this.Range)
			{
				this.FindTarget(this.currentUser);
			}
		}

		// Token: 0x060046D0 RID: 18128 RVA: 0x001C44F4 File Offset: 0x001C26F4
		private void FindTarget(Character user)
		{
			this.currentTarget = null;
			if (user == null || (this.item.Submarine == null && this.OnlyInOwnSub))
			{
				this.IsActive = false;
				return;
			}
			float closestDist = float.PositiveInfinity;
			foreach (Item targetItem in Item.ItemList)
			{
				if (!targetItem.NonInteractable && !targetItem.NonPlayerTeamInteractable && !targetItem.IsHidden && (!this.OnlyInOwnSub || (targetItem.Submarine == this.item.Submarine && targetItem.Submarine.TeamID == user.TeamID)))
				{
					if (!targetItem.HasTag(this.Target))
					{
						Prefab prefab = targetItem.Prefab;
						Identifier target = this.Target;
						if (prefab.Identifier != target)
						{
							continue;
						}
					}
					float distSqr = Vector2.DistanceSquared(this.item.WorldPosition, targetItem.WorldPosition);
					if (distSqr <= this.Range * this.Range && distSqr <= closestDist)
					{
						this.currentTarget = targetItem;
						this.currentSub = this.item.Submarine;
						closestDist = distSqr;
						this.currentUser = user;
					}
				}
			}
			this.IsActive = (this.currentTarget != null);
		}

		// Token: 0x04002228 RID: 8744
		private Item currentTarget;

		// Token: 0x04002229 RID: 8745
		private Character currentUser;

		// Token: 0x0400222A RID: 8746
		private Submarine currentSub;
	}
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D1 RID: 1489
	internal class RemoteController : ItemComponent
	{
		// Token: 0x06005EF9 RID: 24313 RVA: 0x00317E69 File Offset: 0x00316069
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			base.DrawHUD(spriteBatch, character);
			Item item = this.currentTarget;
			if (item == null)
			{
				return;
			}
			item.DrawHUD(spriteBatch, Screen.Selected.Cam, character);
		}

		// Token: 0x06005EFA RID: 24314 RVA: 0x00317E8F File Offset: 0x0031608F
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			Item item = this.currentTarget;
			if (item == null)
			{
				return;
			}
			item.UpdateHUD(cam, character, deltaTime);
		}

		// Token: 0x06005EFB RID: 24315 RVA: 0x00317EA4 File Offset: 0x003160A4
		public override void AddToGUIUpdateList(int order = 0)
		{
			Item item = this.currentTarget;
			if (item == null)
			{
				return;
			}
			item.AddToGUIUpdateList(-1);
		}

		// Token: 0x170017F2 RID: 6130
		// (get) Token: 0x06005EFC RID: 24316 RVA: 0x00317EB7 File Offset: 0x003160B7
		// (set) Token: 0x06005EFD RID: 24317 RVA: 0x00317EBF File Offset: 0x003160BF
		[Serialize("", IsPropertySaveable.No, "Tag or identifier of the item that should be controlled.", "", false)]
		public Identifier Target { get; private set; }

		// Token: 0x170017F3 RID: 6131
		// (get) Token: 0x06005EFE RID: 24318 RVA: 0x00317EC8 File Offset: 0x003160C8
		// (set) Token: 0x06005EFF RID: 24319 RVA: 0x00317ED0 File Offset: 0x003160D0
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool OnlyInOwnSub { get; private set; }

		// Token: 0x170017F4 RID: 6132
		// (get) Token: 0x06005F00 RID: 24320 RVA: 0x00317ED9 File Offset: 0x003160D9
		// (set) Token: 0x06005F01 RID: 24321 RVA: 0x00317EE1 File Offset: 0x003160E1
		[Serialize(10000f, IsPropertySaveable.No, "", "", false)]
		public float Range { get; private set; }

		// Token: 0x170017F5 RID: 6133
		// (get) Token: 0x06005F02 RID: 24322 RVA: 0x00317EEA File Offset: 0x003160EA
		public Item TargetItem
		{
			get
			{
				return this.currentTarget;
			}
		}

		// Token: 0x06005F03 RID: 24323 RVA: 0x00317EF2 File Offset: 0x003160F2
		public RemoteController(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06005F04 RID: 24324 RVA: 0x00317EFC File Offset: 0x003160FC
		public override bool Select(Character character)
		{
			if (base.Select(character))
			{
				this.FindTarget(character);
				return true;
			}
			return false;
		}

		// Token: 0x06005F05 RID: 24325 RVA: 0x00317F11 File Offset: 0x00316111
		public override void Equip(Character character)
		{
			this.FindTarget(character);
		}

		// Token: 0x06005F06 RID: 24326 RVA: 0x00317F1C File Offset: 0x0031611C
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (this.currentTarget.Removed || this.item.Submarine != this.currentSub || Vector2.DistanceSquared(this.currentTarget.WorldPosition, this.item.WorldPosition) > this.Range * this.Range)
			{
				this.FindTarget(this.currentUser);
			}
		}

		// Token: 0x06005F07 RID: 24327 RVA: 0x00317F88 File Offset: 0x00316188
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

		// Token: 0x04003114 RID: 12564
		private Item currentTarget;

		// Token: 0x04003115 RID: 12565
		private Character currentUser;

		// Token: 0x04003116 RID: 12566
		private Submarine currentSub;
	}
}

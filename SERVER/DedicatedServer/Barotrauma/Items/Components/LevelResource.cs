using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000496 RID: 1174
	internal class LevelResource : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06003F9E RID: 16286 RVA: 0x001993F7 File Offset: 0x001975F7
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteSingle(this.deattachTimer);
		}

		// Token: 0x170010D7 RID: 4311
		// (get) Token: 0x06003F9F RID: 16287 RVA: 0x00199405 File Offset: 0x00197605
		// (set) Token: 0x06003FA0 RID: 16288 RVA: 0x0019940D File Offset: 0x0019760D
		[Serialize(1f, IsPropertySaveable.No, "How long it takes to deattach the item from the level walls (in seconds).", "", false)]
		public float DeattachDuration { get; set; }

		// Token: 0x170010D8 RID: 4312
		// (get) Token: 0x06003FA1 RID: 16289 RVA: 0x00199416 File Offset: 0x00197616
		// (set) Token: 0x06003FA2 RID: 16290 RVA: 0x00199420 File Offset: 0x00197620
		[Serialize(0f, IsPropertySaveable.No, "How far along the item is to being deattached. When the timer goes above DeattachDuration, the item is deattached.", "", false)]
		public float DeattachTimer
		{
			get
			{
				return this.deattachTimer;
			}
			set
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				if (this.holdable == null)
				{
					return;
				}
				this.deattachTimer = Math.Max(0f, value);
				if (this.deattachTimer >= this.DeattachDuration)
				{
					if (this.holdable.Attached)
					{
						this.item.CreateServerEvent<LevelResource>(this);
					}
					this.holdable.DeattachFromWall();
					return;
				}
				if (Math.Abs(this.lastSentDeattachTimer - this.deattachTimer) > 0.1f)
				{
					this.item.CreateServerEvent<LevelResource>(this);
					this.lastSentDeattachTimer = this.deattachTimer;
				}
			}
		}

		// Token: 0x170010D9 RID: 4313
		// (get) Token: 0x06003FA3 RID: 16291 RVA: 0x001994BF File Offset: 0x001976BF
		// (set) Token: 0x06003FA4 RID: 16292 RVA: 0x001994C7 File Offset: 0x001976C7
		[Serialize(1f, IsPropertySaveable.No, "How much the position of the item can vary from the wall the item spawns on.", "", false)]
		public float RandomOffsetFromWall { get; set; }

		// Token: 0x170010DA RID: 4314
		// (get) Token: 0x06003FA5 RID: 16293 RVA: 0x001994D0 File Offset: 0x001976D0
		public bool Attached
		{
			get
			{
				return this.holdable != null && this.holdable.Attached;
			}
		}

		// Token: 0x06003FA6 RID: 16294 RVA: 0x001994E7 File Offset: 0x001976E7
		public LevelResource(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06003FA7 RID: 16295 RVA: 0x001994F8 File Offset: 0x001976F8
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (this.trigger != null && amount.LengthSquared() > 1E-05f)
			{
				if (ignoreContacts)
				{
					this.trigger.SetTransformIgnoreContacts(this.item.SimPosition, 0f, true);
					return;
				}
				this.trigger.SetTransform(this.item.SimPosition, 0f, true);
			}
		}

		// Token: 0x06003FA8 RID: 16296 RVA: 0x0019955C File Offset: 0x0019775C
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.holdable != null && !this.holdable.Attached)
			{
				if (this.trigger != null)
				{
					this.trigger.Enabled = false;
				}
				this.IsActive = false;
				return;
			}
			if (this.trigger == null)
			{
				this.CreateTriggerBody();
			}
			if (this.trigger != null && Vector2.DistanceSquared(this.item.SimPosition, this.trigger.SimPosition) > 0.01f)
			{
				this.trigger.SetTransform(this.item.SimPosition, 0f, true);
			}
			this.IsActive = false;
		}

		// Token: 0x06003FA9 RID: 16297 RVA: 0x001995F8 File Offset: 0x001977F8
		public override void OnItemLoaded()
		{
			this.holdable = this.item.GetComponent<Holdable>();
			if (this.holdable == null)
			{
				this.IsActive = false;
				return;
			}
			this.holdable.Reattachable = false;
			if (this.RequiredItems.Any<KeyValuePair<RelatedItem.RelationType, List<RelatedItem>>>())
			{
				this.holdable.PickingTime = float.MaxValue;
			}
		}

		// Token: 0x06003FAA RID: 16298 RVA: 0x00199650 File Offset: 0x00197850
		private void CreateTriggerBody()
		{
			PhysicsBody physicsBody;
			if ((physicsBody = this.item.body) == null)
			{
				Holdable holdable = this.holdable;
				physicsBody = ((holdable != null) ? holdable.Body : null);
			}
			PhysicsBody body = physicsBody;
			if (body != null && this.Attached)
			{
				this.trigger = new PhysicsBody(body.Width, body.Height, body.Radius, body.Density, BodyType.Static, Category.Cat1, Category.None, false)
				{
					UserData = this.item
				};
				this.trigger.FarseerBody.SetIsSensor(true);
			}
		}

		// Token: 0x06003FAB RID: 16299 RVA: 0x001996CE File Offset: 0x001978CE
		protected override void RemoveComponentSpecific()
		{
			if (this.trigger != null)
			{
				this.trigger.Remove();
				this.trigger = null;
			}
		}

		// Token: 0x04001E6D RID: 7789
		private float lastSentDeattachTimer;

		// Token: 0x04001E6E RID: 7790
		private PhysicsBody trigger;

		// Token: 0x04001E6F RID: 7791
		private Holdable holdable;

		// Token: 0x04001E70 RID: 7792
		private float deattachTimer;
	}
}

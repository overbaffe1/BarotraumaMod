using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B7 RID: 1463
	internal class LevelResource : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06005B03 RID: 23299 RVA: 0x002EA537 File Offset: 0x002E8737
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.deattachTimer = msg.ReadSingle();
			if (this.deattachTimer >= this.DeattachDuration)
			{
				this.holdable.DeattachFromWall();
				this.trigger.Enabled = false;
			}
		}

		// Token: 0x170016E9 RID: 5865
		// (get) Token: 0x06005B04 RID: 23300 RVA: 0x002EA56A File Offset: 0x002E876A
		// (set) Token: 0x06005B05 RID: 23301 RVA: 0x002EA572 File Offset: 0x002E8772
		[Serialize(1f, IsPropertySaveable.No, "How long it takes to deattach the item from the level walls (in seconds).", "", false)]
		public float DeattachDuration { get; set; }

		// Token: 0x170016EA RID: 5866
		// (get) Token: 0x06005B06 RID: 23302 RVA: 0x002EA57B File Offset: 0x002E877B
		// (set) Token: 0x06005B07 RID: 23303 RVA: 0x002EA584 File Offset: 0x002E8784
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
						if (GameAnalyticsManager.ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize.Small))
						{
							string str = "ResourceCollected:";
							GameSession gameSession = GameMain.GameSession;
							string text;
							if (gameSession == null)
							{
								text = null;
							}
							else
							{
								GameMode gameMode = gameSession.GameMode;
								text = ((gameMode != null) ? gameMode.Preset.Identifier.Value : null);
							}
							GameAnalyticsManager.AddDesignEvent(str + (text ?? "none") + ":" + this.item.Prefab.Identifier.ToString());
						}
						this.holdable.DeattachFromWall();
					}
					this.trigger.Enabled = false;
				}
			}
		}

		// Token: 0x170016EB RID: 5867
		// (get) Token: 0x06005B08 RID: 23304 RVA: 0x002EA65B File Offset: 0x002E885B
		// (set) Token: 0x06005B09 RID: 23305 RVA: 0x002EA663 File Offset: 0x002E8863
		[Serialize(1f, IsPropertySaveable.No, "How much the position of the item can vary from the wall the item spawns on.", "", false)]
		public float RandomOffsetFromWall { get; set; }

		// Token: 0x170016EC RID: 5868
		// (get) Token: 0x06005B0A RID: 23306 RVA: 0x002EA66C File Offset: 0x002E886C
		public bool Attached
		{
			get
			{
				return this.holdable != null && this.holdable.Attached;
			}
		}

		// Token: 0x06005B0B RID: 23307 RVA: 0x002EA683 File Offset: 0x002E8883
		public LevelResource(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06005B0C RID: 23308 RVA: 0x002EA694 File Offset: 0x002E8894
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

		// Token: 0x06005B0D RID: 23309 RVA: 0x002EA6F8 File Offset: 0x002E88F8
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

		// Token: 0x06005B0E RID: 23310 RVA: 0x002EA794 File Offset: 0x002E8994
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

		// Token: 0x06005B0F RID: 23311 RVA: 0x002EA7EC File Offset: 0x002E89EC
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

		// Token: 0x06005B10 RID: 23312 RVA: 0x002EA86A File Offset: 0x002E8A6A
		protected override void RemoveComponentSpecific()
		{
			if (this.trigger != null)
			{
				this.trigger.Remove();
				this.trigger = null;
			}
		}

		// Token: 0x04002E65 RID: 11877
		private PhysicsBody trigger;

		// Token: 0x04002E66 RID: 11878
		private Holdable holdable;

		// Token: 0x04002E67 RID: 11879
		private float deattachTimer;
	}
}

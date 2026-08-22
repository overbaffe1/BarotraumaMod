using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005AD RID: 1453
	internal class Holdable : Pickable, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x1700162E RID: 5678
		// (get) Token: 0x060058C9 RID: 22729 RVA: 0x002DD278 File Offset: 0x002DB478
		public Vector2 DrawSize
		{
			get
			{
				return this.item.Rect.Size.ToVector2();
			}
		}

		// Token: 0x060058CA RID: 22730 RVA: 0x002DD2A0 File Offset: 0x002DB4A0
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (!this.IsActive || this.picker == null || !this.picker.IsKeyDown(InputType.Aim) || this.picker != Character.Controlled || !this.attachable)
			{
				base.Drawable = false;
				return;
			}
			Color indicatorColor = Color.White;
			IEnumerable<Item> overlappingItems;
			if (!this.CanBeAttached(this.picker, out overlappingItems))
			{
				foreach (Item overlappingItem in overlappingItems)
				{
					overlappingItem.Draw(spriteBatch, false, true, new Color?(Color.Red * 0.7f), new float?(0f));
				}
				indicatorColor = Color.Red;
			}
			Vector2 attachPos = this.GetAttachPosition(this.picker, false);
			Vector2 gridPos = this.picker.Position;
			if (this.AttachesToFloor)
			{
				gridPos.Y = attachPos.Y - (float)(this.item.Rect.Height / 2);
			}
			Vector2 roundedGridPos = new Vector2(MathUtils.RoundTowardsClosest(gridPos.X, Submarine.GridSize.X), MathUtils.RoundTowardsClosest(gridPos.Y, Submarine.GridSize.Y));
			if (this.item.Submarine == null)
			{
				Structure attachTarget = Structure.GetAttachTarget(this.item.WorldPosition);
				if (attachTarget != null && attachTarget.Submarine != null)
				{
					gridPos += attachTarget.Submarine.DrawPosition;
					roundedGridPos += attachTarget.Submarine.DrawPosition;
					attachPos += attachTarget.Submarine.DrawPosition;
				}
			}
			else
			{
				gridPos += this.item.Submarine.DrawPosition;
				roundedGridPos += this.item.Submarine.DrawPosition;
				attachPos += this.item.Submarine.DrawPosition;
			}
			Submarine.DrawGrid(spriteBatch, 14, gridPos, roundedGridPos, 0.4f, new Color?(indicatorColor));
			Sprite sprite = this.item.Sprite;
			foreach (ContainedItemSprite containedSprite in this.item.Prefab.ContainedSprites)
			{
				if (containedSprite.UseWhenAttached)
				{
					sprite = containedSprite.Sprite;
					break;
				}
			}
			sprite.Draw(spriteBatch, new Vector2(attachPos.X, -attachPos.Y), this.item.SpriteColor.Multiply(indicatorColor) * 0.5f, this.item.RotationRad, this.item.Scale, SpriteEffects.None, new float?(0f));
			GUI.DrawRectangle(spriteBatch, new Vector2(attachPos.X - 2f, -attachPos.Y - 2f), Vector2.One * 5f, GUIStyle.Red, false, 0f, 3f);
		}

		// Token: 0x060058CB RID: 22731 RVA: 0x002DD594 File Offset: 0x002DB794
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			Holdable.AttachEventData attachEventData;
			return base.TryExtractEventData<Holdable.AttachEventData>(data, out attachEventData);
		}

		// Token: 0x060058CC RID: 22732 RVA: 0x002DD5AC File Offset: 0x002DB7AC
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			if (!this.attachable || this.originalBody == null)
			{
				return;
			}
			Holdable.AttachEventData eventData = base.ExtractEventData<Holdable.AttachEventData>(extraData);
			Vector2 attachPos = eventData.AttachPos;
			msg.WriteSingle(attachPos.X);
			msg.WriteSingle(attachPos.Y);
		}

		// Token: 0x060058CD RID: 22733 RVA: 0x002DD5F4 File Offset: 0x002DB7F4
		public override void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			base.ClientEventRead(msg, sendingTime);
			if (!msg.ReadBoolean())
			{
				return;
			}
			bool shouldBeAttached = msg.ReadBoolean();
			Vector2 simPosition = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			ushort submarineID = msg.ReadUInt16();
			ushort attacherID = msg.ReadUInt16();
			Submarine sub = Entity.FindEntityByID(submarineID) as Submarine;
			Character attacher = Entity.FindEntityByID(attacherID) as Character;
			if (shouldBeAttached)
			{
				if (!this.attached)
				{
					this.Drop(false, null, true);
					this.item.SetTransform(simPosition, 0f, true, true, sub);
					this.AttachToWall();
					base.PlaySound(ActionType.OnUse, attacher);
					base.ApplyStatusEffects(ActionType.OnUse, 0.016666668f, attacher, null, null, attacher, null, 1f);
					return;
				}
			}
			else
			{
				if (this.attached)
				{
					base.DropConnectedWires(null);
					if (this.originalBody != null)
					{
						this.item.body = this.originalBody;
						this.item.body.Enabled = true;
					}
					this.IsActive = false;
					this.DeattachFromWall();
					return;
				}
				this.item.SetTransform(simPosition, 0f, true, true, sub);
			}
		}

		// Token: 0x1700162F RID: 5679
		// (get) Token: 0x060058CE RID: 22734 RVA: 0x002DD711 File Offset: 0x002DB911
		public override bool IsAttached
		{
			get
			{
				return this.Attached;
			}
		}

		// Token: 0x17001630 RID: 5680
		// (get) Token: 0x060058CF RID: 22735 RVA: 0x002DD719 File Offset: 0x002DB919
		// (set) Token: 0x060058D0 RID: 22736 RVA: 0x002DD721 File Offset: 0x002DB921
		public PhysicsBody Pusher { get; private set; }

		// Token: 0x17001631 RID: 5681
		// (get) Token: 0x060058D1 RID: 22737 RVA: 0x002DD72A File Offset: 0x002DB92A
		// (set) Token: 0x060058D2 RID: 22738 RVA: 0x002DD732 File Offset: 0x002DB932
		[Serialize(true, IsPropertySaveable.Yes, "Is the item currently able to push characters around? True by default. Only valid if blocksplayers is set to true.", "", false)]
		public bool CanPush { get; set; }

		// Token: 0x17001632 RID: 5682
		// (get) Token: 0x060058D3 RID: 22739 RVA: 0x002DD73B File Offset: 0x002DB93B
		public PhysicsBody Body
		{
			get
			{
				return this.item.body ?? this.originalBody;
			}
		}

		// Token: 0x17001633 RID: 5683
		// (get) Token: 0x060058D4 RID: 22740 RVA: 0x002DD752 File Offset: 0x002DB952
		// (set) Token: 0x060058D5 RID: 22741 RVA: 0x002DD76C File Offset: 0x002DB96C
		[Serialize(false, IsPropertySaveable.Yes, "Is the item currently attached to a wall (only valid if Attachable is set to true).", "", false)]
		public bool Attached
		{
			get
			{
				return this.attached && this.item.ParentInventory == null;
			}
			set
			{
				this.attached = value;
				this.item.CheckCleanable();
				this.item.SetActiveSprite();
			}
		}

		// Token: 0x17001634 RID: 5684
		// (get) Token: 0x060058D6 RID: 22742 RVA: 0x002DD78B File Offset: 0x002DB98B
		// (set) Token: 0x060058D7 RID: 22743 RVA: 0x002DD793 File Offset: 0x002DB993
		[Serialize(true, IsPropertySaveable.Yes, "Can the item be pointed to a specific direction or do the characters always hold it in a static pose.", "", false)]
		public bool Aimable { get; set; }

		// Token: 0x17001635 RID: 5685
		// (get) Token: 0x060058D8 RID: 22744 RVA: 0x002DD79C File Offset: 0x002DB99C
		// (set) Token: 0x060058D9 RID: 22745 RVA: 0x002DD7A4 File Offset: 0x002DB9A4
		[Serialize(0f, IsPropertySaveable.Yes, "Camera offset to apply when aiming this item. Only valid if Aimable is set to true.", "", false)]
		public float CameraAimOffset { get; set; }

		// Token: 0x17001636 RID: 5686
		// (get) Token: 0x060058DA RID: 22746 RVA: 0x002DD7AD File Offset: 0x002DB9AD
		// (set) Token: 0x060058DB RID: 22747 RVA: 0x002DD7B5 File Offset: 0x002DB9B5
		[Serialize(false, IsPropertySaveable.No, "Should the character adjust its pose when aiming with the item. Most noticeable underwater, where the character will rotate its entire body to face the direction the item is aimed at.", "", false)]
		public bool ControlPose { get; set; }

		// Token: 0x17001637 RID: 5687
		// (get) Token: 0x060058DC RID: 22748 RVA: 0x002DD7BE File Offset: 0x002DB9BE
		// (set) Token: 0x060058DD RID: 22749 RVA: 0x002DD7C6 File Offset: 0x002DB9C6
		[Serialize(false, IsPropertySaveable.No, "Use the hand rotation instead of torso rotation for the item hold angle. Enable this if you want the item just to follow with the arm when not aiming instead of forcing the arm to a hold pose.", "", false)]
		public bool UseHandRotationForHoldAngle { get; set; }

		// Token: 0x17001638 RID: 5688
		// (get) Token: 0x060058DE RID: 22750 RVA: 0x002DD7CF File Offset: 0x002DB9CF
		// (set) Token: 0x060058DF RID: 22751 RVA: 0x002DD7D7 File Offset: 0x002DB9D7
		[Serialize(false, IsPropertySaveable.No, "Can the item be attached to walls.", "", false)]
		public bool Attachable
		{
			get
			{
				return this.attachable;
			}
			set
			{
				this.attachable = value;
			}
		}

		// Token: 0x17001639 RID: 5689
		// (get) Token: 0x060058E0 RID: 22752 RVA: 0x002DD7E0 File Offset: 0x002DB9E0
		// (set) Token: 0x060058E1 RID: 22753 RVA: 0x002DD7E8 File Offset: 0x002DB9E8
		[Serialize(true, IsPropertySaveable.No, "Can the item be reattached to walls after it has been deattached (only valid if Attachable is set to true).", "", false)]
		public bool Reattachable { get; set; }

		// Token: 0x1700163A RID: 5690
		// (get) Token: 0x060058E2 RID: 22754 RVA: 0x002DD7F1 File Offset: 0x002DB9F1
		// (set) Token: 0x060058E3 RID: 22755 RVA: 0x002DD7F9 File Offset: 0x002DB9F9
		[Serialize(false, IsPropertySaveable.No, "Can the item only be attached in limited amount? Uses permanent stat values to check for legibility.", "", false)]
		public bool LimitedAttachable { get; set; }

		// Token: 0x1700163B RID: 5691
		// (get) Token: 0x060058E4 RID: 22756 RVA: 0x002DD802 File Offset: 0x002DBA02
		// (set) Token: 0x060058E5 RID: 22757 RVA: 0x002DD80A File Offset: 0x002DBA0A
		[Serialize(false, IsPropertySaveable.No, "When enabled, the item can only be attached to a position where it touches the floor.", "", false)]
		public bool AttachesToFloor { get; set; }

		// Token: 0x1700163C RID: 5692
		// (get) Token: 0x060058E6 RID: 22758 RVA: 0x002DD813 File Offset: 0x002DBA13
		// (set) Token: 0x060058E7 RID: 22759 RVA: 0x002DD81B File Offset: 0x002DBA1B
		[Serialize(true, IsPropertySaveable.No, "Can the item be attached inside doors?", "", false)]
		public bool AllowAttachInsideDoors { get; set; }

		// Token: 0x1700163D RID: 5693
		// (get) Token: 0x060058E8 RID: 22760 RVA: 0x002DD824 File Offset: 0x002DBA24
		// (set) Token: 0x060058E9 RID: 22761 RVA: 0x002DD836 File Offset: 0x002DBA36
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public string DisallowAttachingOverTags
		{
			get
			{
				return this.disallowAttachingOverTags.ConvertToString(",");
			}
			set
			{
				this.disallowAttachingOverTags = value.ToIdentifiers(",").ToHashSet<Identifier>();
			}
		}

		// Token: 0x1700163E RID: 5694
		// (get) Token: 0x060058EA RID: 22762 RVA: 0x002DD84E File Offset: 0x002DBA4E
		// (set) Token: 0x060058EB RID: 22763 RVA: 0x002DD856 File Offset: 0x002DBA56
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		public Point DisallowAttachingOverSize { get; set; }

		// Token: 0x1700163F RID: 5695
		// (get) Token: 0x060058EC RID: 22764 RVA: 0x002DD85F File Offset: 0x002DBA5F
		// (set) Token: 0x060058ED RID: 22765 RVA: 0x002DD867 File Offset: 0x002DBA67
		[Serialize(false, IsPropertySaveable.No, "Should the item be attached to a wall by default when it's placed in the submarine editor.", "", false)]
		public bool AttachedByDefault
		{
			get
			{
				return this.attachedByDefault;
			}
			set
			{
				this.attachedByDefault = value;
			}
		}

		// Token: 0x17001640 RID: 5696
		// (get) Token: 0x060058EE RID: 22766 RVA: 0x002DD870 File Offset: 0x002DBA70
		// (set) Token: 0x060058EF RID: 22767 RVA: 0x002DD87D File Offset: 0x002DBA7D
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position the character holds the item at (in pixels, as an offset from the character's shoulder). For example, a value of 10,-100 would make the character hold the item 100 pixels below the shoulder and 10 pixels forwards.", "", false)]
		public Vector2 HoldPos
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.holdPos);
			}
			set
			{
				this.holdPos = ConvertUnits.ToSimUnits(value);
			}
		}

		// Token: 0x17001641 RID: 5697
		// (get) Token: 0x060058F0 RID: 22768 RVA: 0x002DD88B File Offset: 0x002DBA8B
		// (set) Token: 0x060058F1 RID: 22769 RVA: 0x002DD898 File Offset: 0x002DBA98
		[Serialize("0.0,0.0", IsPropertySaveable.No, "The position the character holds the item at when aiming (in pixels, as an offset from the character's shoulder). Works similarly as HoldPos, except that the position is rotated according to the direction the player is aiming at. For example, a value of 10,-100 would make the character hold the item 100 pixels below the shoulder and 10 pixels forwards when aiming directly to the right.", "", false)]
		public Vector2 AimPos
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.aimPos);
			}
			set
			{
				this.aimPos = ConvertUnits.ToSimUnits(value);
			}
		}

		// Token: 0x17001642 RID: 5698
		// (get) Token: 0x060058F2 RID: 22770 RVA: 0x002DD8A6 File Offset: 0x002DBAA6
		// (set) Token: 0x060058F3 RID: 22771 RVA: 0x002DD8B3 File Offset: 0x002DBAB3
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float HoldAngle
		{
			get
			{
				return MathHelper.ToDegrees(this.holdAngle);
			}
			set
			{
				this.holdAngle = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17001643 RID: 5699
		// (get) Token: 0x060058F4 RID: 22772 RVA: 0x002DD8C1 File Offset: 0x002DBAC1
		// (set) Token: 0x060058F5 RID: 22773 RVA: 0x002DD8CE File Offset: 0x002DBACE
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float AimAngle
		{
			get
			{
				return MathHelper.ToDegrees(this.aimAngle);
			}
			set
			{
				this.aimAngle = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17001644 RID: 5700
		// (get) Token: 0x060058F6 RID: 22774 RVA: 0x002DD8DC File Offset: 0x002DBADC
		// (set) Token: 0x060058F7 RID: 22775 RVA: 0x002DD8E9 File Offset: 0x002DBAE9
		[Serialize("0.0,0.0", IsPropertySaveable.No, "", "", false)]
		public Vector2 SwingAmount
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.swingAmount);
			}
			set
			{
				this.swingAmount = ConvertUnits.ToSimUnits(value);
			}
		}

		// Token: 0x17001645 RID: 5701
		// (get) Token: 0x060058F8 RID: 22776 RVA: 0x002DD8F7 File Offset: 0x002DBAF7
		// (set) Token: 0x060058F9 RID: 22777 RVA: 0x002DD8FF File Offset: 0x002DBAFF
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float SwingSpeed { get; set; }

		// Token: 0x17001646 RID: 5702
		// (get) Token: 0x060058FA RID: 22778 RVA: 0x002DD908 File Offset: 0x002DBB08
		// (set) Token: 0x060058FB RID: 22779 RVA: 0x002DD910 File Offset: 0x002DBB10
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool SwingWhenHolding { get; set; }

		// Token: 0x17001647 RID: 5703
		// (get) Token: 0x060058FC RID: 22780 RVA: 0x002DD919 File Offset: 0x002DBB19
		// (set) Token: 0x060058FD RID: 22781 RVA: 0x002DD921 File Offset: 0x002DBB21
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool SwingWhenAiming { get; set; }

		// Token: 0x17001648 RID: 5704
		// (get) Token: 0x060058FE RID: 22782 RVA: 0x002DD92A File Offset: 0x002DBB2A
		// (set) Token: 0x060058FF RID: 22783 RVA: 0x002DD932 File Offset: 0x002DBB32
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool SwingWhenUsing { get; set; }

		// Token: 0x17001649 RID: 5705
		// (get) Token: 0x06005900 RID: 22784 RVA: 0x002DD93B File Offset: 0x002DBB3B
		// (set) Token: 0x06005901 RID: 22785 RVA: 0x002DD943 File Offset: 0x002DBB43
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DisableHeadRotation { get; set; }

		// Token: 0x1700164A RID: 5706
		// (get) Token: 0x06005902 RID: 22786 RVA: 0x002DD94C File Offset: 0x002DBB4C
		// (set) Token: 0x06005903 RID: 22787 RVA: 0x002DD954 File Offset: 0x002DBB54
		[Serialize(false, IsPropertySaveable.No, "If true, this item can't be used if the character is also holding a ranged weapon.", "", false)]
		public bool DisableWhenRangedWeaponEquipped { get; set; }

		// Token: 0x1700164B RID: 5707
		// (get) Token: 0x06005904 RID: 22788 RVA: 0x002DD95D File Offset: 0x002DBB5D
		// (set) Token: 0x06005905 RID: 22789 RVA: 0x002DD965 File Offset: 0x002DBB65
		[ConditionallyEditable(ConditionallyEditable.ConditionType.Attachable, true, MinValueFloat = 0f, MaxValueFloat = 0.999f, DecimalCount = 3)]
		[Serialize(0.55f, IsPropertySaveable.No, "Sprite depth that's used when the item is NOT attached to a wall.", "", false)]
		public float SpriteDepthWhenDropped { get; set; }

		// Token: 0x1700164C RID: 5708
		// (get) Token: 0x06005906 RID: 22790 RVA: 0x002DD96E File Offset: 0x002DBB6E
		// (set) Token: 0x06005907 RID: 22791 RVA: 0x002DD976 File Offset: 0x002DBB76
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "A text displayed next to the item when it's been dropped on the floor (not attached to a wall).", "ItemMsg", false)]
		public string MsgWhenDropped { get; set; }

		// Token: 0x1700164D RID: 5709
		// (get) Token: 0x06005908 RID: 22792 RVA: 0x002DD97F File Offset: 0x002DBB7F
		// (set) Token: 0x06005909 RID: 22793 RVA: 0x002DD994 File Offset: 0x002DBB94
		public Vector2 Handle1
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.handlePos[0]);
			}
			set
			{
				this.handlePos[0] = ConvertUnits.ToSimUnits(value);
				if (this.item.FlippedX)
				{
					this.handlePos[0].X = -this.handlePos[0].X;
				}
				if (!this.secondHandlePosDefined)
				{
					this.Handle2 = value;
				}
			}
		}

		// Token: 0x1700164E RID: 5710
		// (get) Token: 0x0600590A RID: 22794 RVA: 0x002DD9F2 File Offset: 0x002DBBF2
		// (set) Token: 0x0600590B RID: 22795 RVA: 0x002DDA08 File Offset: 0x002DBC08
		public Vector2 Handle2
		{
			get
			{
				return ConvertUnits.ToDisplayUnits(this.handlePos[1]);
			}
			set
			{
				this.handlePos[1] = ConvertUnits.ToSimUnits(value);
				if (this.item.FlippedX)
				{
					this.handlePos[1].X = -this.handlePos[1].X;
				}
			}
		}

		// Token: 0x0600590C RID: 22796 RVA: 0x002DDA58 File Offset: 0x002DBC58
		public Holdable(Item item, ContentXElement element) : base(item, element)
		{
			this.originalBody = item.body;
			this.Pusher = null;
			if (element.GetAttributeBool("blocksplayers", false))
			{
				this.Pusher = new PhysicsBody(item.body.Width, item.body.Height, item.body.Radius, item.body.Density, BodyType.Dynamic, Category.Cat6, Category.Cat2 | Category.Cat7, true)
				{
					Enabled = false,
					UserData = this
				};
				this.Pusher.FarseerBody.OnCollision += this.OnPusherCollision;
				this.Pusher.FarseerBody.FixedRotation = false;
				this.Pusher.FarseerBody.IgnoreGravity = true;
			}
			this.handlePos = new Vector2[2];
			this.scaledHandlePos = new Vector2[2];
			Vector2 previousValue = Vector2.Zero;
			for (int i = 1; i < 3; i++)
			{
				int index = i - 1;
				string attributeName = "handle" + i.ToString();
				Vector2 value = previousValue;
				XAttribute attribute = element.GetAttribute(attributeName);
				if (attribute != null)
				{
					this.secondHandlePosDefined = (i > 1);
					value = ConvertUnits.ToSimUnits(XMLExtensions.ParseVector2(attribute.Value, true));
				}
				this.handlePos[index] = value;
				previousValue = value;
			}
			this.canBePicked = true;
			this.prevRequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>(this.RequiredItems);
			if (this.attachable)
			{
				this.prevMsg = base.DisplayMsg;
				this.prevPickKey = base.PickKey;
				if (item.Submarine != null)
				{
					if (item.Submarine.Loading)
					{
						this.AttachToWall();
						this.Attached = false;
					}
					else if (Screen.Selected == GameMain.SubEditorScreen)
					{
						this.AttachToWall();
					}
					else
					{
						this.DeattachFromWall();
					}
				}
			}
			this.characterUsable = element.GetAttributeBool("characterusable", true);
			Dictionary<StatTypes, float> statValues = new Dictionary<StatTypes, float>();
			foreach (ContentXElement subElement in element.GetChildElements("statvalue"))
			{
				StatTypes statType = CharacterAbilityGroup.ParseStatType(subElement.GetAttributeString("stattype", ""), base.Name);
				float statValue = subElement.GetAttributeFloat("value", 0f);
				if (statValues.ContainsKey(statType))
				{
					Dictionary<StatTypes, float> dictionary = statValues;
					StatTypes key = statType;
					dictionary[key] += statValue;
				}
				else
				{
					statValues.TryAdd(statType, statValue);
				}
			}
			this.HoldableStatValues = statValues.ToImmutableDictionary<StatTypes, float>();
		}

		// Token: 0x0600590D RID: 22797 RVA: 0x002DDCE8 File Offset: 0x002DBEE8
		private bool OnPusherCollision(Fixture sender, Fixture other, Contact contact)
		{
			Character character = other.Body.UserData as Character;
			return character == null || (this.IsActive && this.CanPush && character != this.picker);
		}

		// Token: 0x0600590E RID: 22798 RVA: 0x002DDD2C File Offset: 0x002DBF2C
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.loadedFromInstance = true;
			if (usePrefabValues)
			{
				this.Attached = componentElement.GetAttributeBool("attached", this.attached);
			}
			if (this.attachable)
			{
				this.prevMsg = base.DisplayMsg;
				this.prevRequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>(this.RequiredItems);
			}
		}

		// Token: 0x0600590F RID: 22799 RVA: 0x002DDD8A File Offset: 0x002DBF8A
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.Drop(true, dropper, setTransform);
		}

		// Token: 0x06005910 RID: 22800 RVA: 0x002DDD98 File Offset: 0x002DBF98
		private void Drop(bool dropConnectedWires, Character dropper, bool setTransform = true)
		{
			Rope rope = this.GetRope();
			if (rope != null)
			{
				rope.Snap();
			}
			if (dropConnectedWires)
			{
				base.DropConnectedWires(dropper);
			}
			if (this.attachable)
			{
				if (this.originalBody != null)
				{
					this.item.body = this.originalBody;
				}
				this.DeattachFromWall();
			}
			if (this.Pusher != null)
			{
				this.Pusher.Enabled = false;
			}
			if (this.item.body != null)
			{
				this.item.body.Enabled = true;
			}
			this.IsActive = false;
			this.attachTargetCell = null;
			if (this.picker == null || this.picker.Removed)
			{
				if (dropper == null || dropper.Removed)
				{
					return;
				}
				this.picker = dropper;
			}
			if (this.picker.Inventory == null)
			{
				return;
			}
			this.item.Submarine = this.picker.Submarine;
			if (this.item.body != null && setTransform)
			{
				if (this.item.body.Removed)
				{
					DebugConsole.ThrowError("Failed to drop the Holdable component of the item \"" + this.item.Name + "\" (body has been removed" + (this.item.Removed ? ", item has been removed)" : ")"), null, null, false, false);
				}
				else
				{
					this.item.body.ResetDynamics();
					Limb heldHand;
					Limb arm;
					if (this.picker.Inventory.IsInLimbSlot(this.item, InvSlotType.LeftHand))
					{
						heldHand = this.picker.AnimController.GetLimb(LimbType.LeftHand, true, false, false);
						arm = this.picker.AnimController.GetLimb(LimbType.LeftArm, true, false, false);
					}
					else
					{
						heldHand = this.picker.AnimController.GetLimb(LimbType.RightHand, true, false, false);
						arm = this.picker.AnimController.GetLimb(LimbType.RightArm, true, false, false);
					}
					if (heldHand != null && !heldHand.Removed && arm != null && !arm.Removed)
					{
						Vector2 diff = new Vector2((heldHand.SimPosition.X - arm.SimPosition.X) / 2f, (heldHand.SimPosition.Y - arm.SimPosition.Y) / 2.5f);
						this.item.SetTransform(heldHand.SimPosition + diff, 0f, true, true, this.picker.Submarine);
					}
					else
					{
						this.item.SetTransform(this.picker.SimPosition, 0f, true, true, this.picker.Submarine);
					}
				}
			}
			this.picker.Inventory.RemoveItem(this.item);
			this.picker = null;
		}

		// Token: 0x06005911 RID: 22801 RVA: 0x002DE024 File Offset: 0x002DC224
		public override void Equip(Character character)
		{
			if (this.item.GetComponents<Pickable>().Count<Pickable>() > 0)
			{
				bool inSuitableSlot = false;
				int i;
				Func<InvSlotType, bool> <>9__0;
				int j;
				for (i = 0; i < character.Inventory.Capacity; i = j + 1)
				{
					if (character.Inventory.GetItemsAt(i).Contains(this.item) && character.Inventory.SlotTypes[i] != InvSlotType.Any)
					{
						IEnumerable<InvSlotType> allowedSlots = this.allowedSlots;
						Func<InvSlotType, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((InvSlotType a) => a.HasFlag(character.Inventory.SlotTypes[i])));
						}
						if (allowedSlots.Any(predicate))
						{
							inSuitableSlot = true;
							break;
						}
					}
					j = i;
				}
				if (!inSuitableSlot)
				{
					return;
				}
			}
			this.picker = character;
			if (this.item.Removed)
			{
				DebugConsole.ThrowError("Attempted to equip a removed item (" + this.item.Name + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return;
			}
			Wearable wearable = this.item.GetComponent<Wearable>();
			if (wearable != null && !wearable.AllowedSlots.SequenceEqual(this.allowedSlots))
			{
				wearable.Unequip(character);
			}
			if (character != null)
			{
				this.item.Submarine = character.Submarine;
			}
			if (this.item.body == null)
			{
				if (this.originalBody == null)
				{
					return;
				}
				this.item.body = this.originalBody;
			}
			if (!this.item.body.Enabled)
			{
				Limb hand = this.picker.AnimController.GetLimb(LimbType.RightHand, true, false, false) ?? this.picker.AnimController.GetLimb(LimbType.LeftHand, true, false, false);
				this.item.SetTransform((hand != null) ? hand.SimPosition : character.SimPosition, 0f, true, true, null);
			}
			bool alreadyEquipped = character.HasEquippedItem(this.item, null, null);
			if (this.picker.HasEquippedItem(this.item, null, null))
			{
				this.item.body.Enabled = true;
				this.item.body.PhysEnabled = false;
				this.IsActive = true;
				this.prevEquipper = this.picker;
				return;
			}
			this.prevEquipper = null;
		}

		// Token: 0x06005912 RID: 22802 RVA: 0x002DE2A1 File Offset: 0x002DC4A1
		public override void Unequip(Character character)
		{
			this.prevEquipper = null;
			if (this.picker == null)
			{
				return;
			}
			this.item.body.PhysEnabled = true;
			this.item.body.Enabled = false;
			this.IsActive = false;
		}

		// Token: 0x06005913 RID: 22803 RVA: 0x002DE2DC File Offset: 0x002DC4DC
		public bool CanBeAttached(Character user)
		{
			IEnumerable<Item> enumerable;
			return this.CanBeAttached(user, out enumerable);
		}

		// Token: 0x06005914 RID: 22804 RVA: 0x002DE2F4 File Offset: 0x002DC4F4
		private bool CanBeAttached(Character user, out IEnumerable<Item> overlappingItems)
		{
			Holdable.tempOverlappingItems.Clear();
			overlappingItems = Holdable.tempOverlappingItems;
			if (!this.attachable || !this.Reattachable)
			{
				return false;
			}
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				return true;
			}
			if (this.AttachesToFloor && this.item.CurrentHull == null)
			{
				return false;
			}
			Vector2 attachPos = (user == null) ? this.item.WorldPosition : this.GetAttachPosition(user, true);
			if (this.disallowAttachingOverTags.Any<Identifier>() || !this.AllowAttachInsideDoors)
			{
				Hull currentHull = this.item.CurrentHull;
				IEnumerable<Hull> connectedHulls = (currentHull != null) ? currentHull.GetConnectedHulls(true, new int?(5), true) : null;
				Vector2 size = (this.DisallowAttachingOverSize == Point.Zero) ? this.item.Rect.Size.ToVector2() : (this.DisallowAttachingOverSize.ToVector2() * this.item.Scale);
				size /= 2f;
				foreach (Item otherItem in Item.ItemList)
				{
					if (otherItem != this.item)
					{
						PhysicsBody body = otherItem.body;
						if ((body == null || body.BodyType != BodyType.Dynamic || !body.Enabled) && (connectedHulls == null || connectedHulls.Contains(otherItem.CurrentHull)) && (!this.disallowAttachingOverTags.None(new Func<Identifier, bool>(otherItem.HasTag)) || (otherItem.GetComponent<Door>() != null && !this.AllowAttachInsideDoors)))
						{
							Rectangle worldRect = otherItem.WorldRect;
							Holdable otherHoldable = otherItem.GetComponent<Holdable>();
							if (otherHoldable != null)
							{
								if (!otherHoldable.attached)
								{
									continue;
								}
								if (otherHoldable.DisallowAttachingOverSize != Point.Zero)
								{
									Vector2 scaledSize = otherHoldable.DisallowAttachingOverSize.ToVector2() * this.item.Scale;
									worldRect = new Rectangle(otherItem.WorldPosition.ToPoint() - new Point((int)(scaledSize.X / 2f), (int)(-scaledSize.Y / 2f)), scaledSize.ToPoint());
								}
							}
							if (attachPos.X + size.X >= (float)worldRect.X && attachPos.X - size.X <= (float)worldRect.Right && attachPos.Y - size.Y <= (float)worldRect.Y && attachPos.Y + size.Y >= (float)(worldRect.Y - worldRect.Height))
							{
								Holdable.tempOverlappingItems.Add(otherItem);
							}
						}
					}
				}
				if (Holdable.tempOverlappingItems.Any<Item>())
				{
					return false;
				}
			}
			return (this.item.CurrentHull != null && Submarine.RectContains(this.item.CurrentHull.WorldRect, attachPos, false)) || Structure.GetAttachTarget(attachPos) != null || this.GetAttachTargetCell(100f) != null;
		}

		// Token: 0x06005915 RID: 22805 RVA: 0x002DE61C File Offset: 0x002DC81C
		public bool CanBeDeattached()
		{
			if (!this.attachable || !this.attached)
			{
				return true;
			}
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				return true;
			}
			if (this.item.GetComponent<LevelResource>() != null)
			{
				return true;
			}
			Planter planter = this.item.GetComponent<Planter>();
			if (planter != null)
			{
				if (planter.GrowableSeeds.Any((Growable seed) => seed != null))
				{
					return false;
				}
			}
			ConnectionPanel connectionPanel = this.item.GetComponent<ConnectionPanel>();
			if (connectionPanel != null && !connectionPanel.AlwaysAllowRewiring)
			{
				if (!connectionPanel.Locked)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					bool? flag;
					if (networkMember == null)
					{
						flag = null;
					}
					else
					{
						ServerSettings serverSettings = networkMember.ServerSettings;
						flag = ((serverSettings != null) ? new bool?(serverSettings.AllowRewiring) : null);
					}
					bool? flag2 = flag;
					if (flag2.GetValueOrDefault(true))
					{
						goto IL_CA;
					}
				}
				return false;
			}
			IL_CA:
			return this.item.CurrentHull != null || this.attachTargetCell != null || Structure.GetAttachTarget(this.item.WorldPosition) != null;
		}

		// Token: 0x06005916 RID: 22806 RVA: 0x002DE720 File Offset: 0x002DC920
		public override bool Pick(Character picker)
		{
			if (this.item.Removed)
			{
				DebugConsole.ThrowError("Attempted to pick up a removed item (" + this.item.Name + ")\n" + Environment.StackTrace.CleanupStackTrace(), null, null, false, false);
				return false;
			}
			if (!this.attachable)
			{
				return base.Pick(picker);
			}
			if (!this.CanBeDeattached())
			{
				return false;
			}
			if (this.Attached)
			{
				return base.Pick(picker);
			}
			return this.OnPicked(picker);
		}

		// Token: 0x06005917 RID: 22807 RVA: 0x002DE79C File Offset: 0x002DC99C
		public override bool OnPicked(Character picker)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				if (!picker.Inventory.CanBeAutoMovedToCorrectSlots(this.item))
				{
					picker.Inventory.FlashAllowedSlots(this.item, Color.Red);
				}
				else
				{
					SoundPlayer.PlayUISound(GUISoundType.PickItem);
				}
				return false;
			}
			bool wasAttached = this.IsAttached;
			if (base.OnPicked(picker))
			{
				this.DeattachFromWall();
				return true;
			}
			return false;
		}

		// Token: 0x06005918 RID: 22808 RVA: 0x002DE808 File Offset: 0x002DCA08
		public void AttachToWall()
		{
			if (!this.attachable)
			{
				return;
			}
			if (this.originalBody == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Tried to attach an item with no physics body to a wall (");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral(").");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.originalBody.Enabled = false;
			this.originalBody.SetTransformIgnoreContacts(this.originalBody.SimPosition, 0f, true);
			if (this.item.body != null)
			{
				this.item.body.Dir = 1f;
				this.item.body = null;
			}
			this.item.GetComponents<LightComponent>().ForEach(delegate(LightComponent light)
			{
				light.SetLightSourceTransform();
			});
			if (this.item.CurrentHull == null && this.item.Submarine == null)
			{
				Structure attachTarget = Structure.GetAttachTarget(this.item.WorldPosition);
				if (attachTarget != null)
				{
					if (attachTarget.Submarine != null)
					{
						this.item.SetTransform(ConvertUnits.ToSimUnits(this.item.WorldPosition - attachTarget.Submarine.Position), 0f, false, true, null);
						this.originalBody.SetTransformIgnoreContacts(this.item.SimPosition, 0f, true);
					}
					this.item.Submarine = attachTarget.Submarine;
				}
				else
				{
					this.attachTargetCell = this.GetAttachTargetCell(150f);
					if (this.attachTargetCell != null && this.attachTargetCell.IsDestructible)
					{
						VoronoiCell voronoiCell = this.attachTargetCell;
						voronoiCell.OnDestroyed = (Action)Delegate.Combine(voronoiCell.OnDestroyed, new Action(delegate()
						{
							if (this.attachTargetCell != null && this.attachTargetCell.CellType != CellType.Solid)
							{
								this.Drop(true, null, true);
							}
						}));
					}
				}
			}
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null)
			{
				foreach (Item contained in containedItems)
				{
					if (((contained != null) ? contained.body : null) != null)
					{
						contained.SetTransform(this.item.SimPosition, contained.body.Rotation, true, true, null);
					}
				}
			}
			base.DisplayMsg = this.prevMsg;
			base.PickKey = this.prevPickKey;
			this.RequiredItems = new Dictionary<RelatedItem.RelationType, List<RelatedItem>>(this.prevRequiredItems);
			this.Attached = true;
			this.item.DrawDepthOffset = 0f;
		}

		// Token: 0x06005919 RID: 22809 RVA: 0x002DEAA0 File Offset: 0x002DCCA0
		public void DeattachFromWall()
		{
			if (!this.attachable)
			{
				return;
			}
			this.Attached = false;
			this.attachTargetCell = null;
			this.item.DrawDepthOffset = 0f;
			this.RequiredItems.Clear();
			if (this.MsgWhenDropped.IsNullOrEmpty())
			{
				base.DisplayMsg = "";
			}
			else
			{
				base.DisplayMsg = TextManager.Get(this.MsgWhenDropped);
				base.DisplayMsg = (base.DisplayMsg.Loaded ? TextManager.ParseInputTypes(base.DisplayMsg, false) : this.MsgWhenDropped);
			}
			base.PickKey = InputType.Select;
			this.item.DrawDepthOffset = this.SpriteDepthWhenDropped - this.item.SpriteDepth;
			foreach (LightComponent light in this.item.GetComponents<LightComponent>())
			{
				light.CheckIfNeedsUpdate();
			}
		}

		// Token: 0x0600591A RID: 22810 RVA: 0x002DEBA4 File Offset: 0x002DCDA4
		public override void ParseMsg()
		{
			base.ParseMsg();
			if (this.Attachable)
			{
				this.prevMsg = base.DisplayMsg;
			}
		}

		// Token: 0x0600591B RID: 22811 RVA: 0x002DEBC0 File Offset: 0x002DCDC0
		public override bool Use(float deltaTime, Character character = null)
		{
			Holdable.<>c__DisplayClass158_0 CS$<>8__locals1 = new Holdable.<>c__DisplayClass158_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.UsageDisabledByRangedWeapon(character))
			{
				return false;
			}
			if (!this.attachable || this.item.body == null)
			{
				return character == null || (character.IsKeyDown(InputType.Aim) && this.characterUsable);
			}
			if (character != null)
			{
				if (!this.characterUsable && !this.attachable)
				{
					return false;
				}
				if (!character.IsKeyDown(InputType.Aim))
				{
					return false;
				}
				if (!this.CanBeAttached(character))
				{
					return false;
				}
				if (this.LimitedAttachable)
				{
					if (((character != null) ? character.Info : null) == null)
					{
						DebugConsole.AddWarning("Character without CharacterInfo attempting to attach a limited attachable item!", null);
						return false;
					}
					Vector2 attachPos = this.GetAttachPosition(character, true);
					Holdable.<>c__DisplayClass158_0 CS$<>8__locals2 = CS$<>8__locals1;
					Structure attachTarget = Structure.GetAttachTarget(attachPos);
					CS$<>8__locals2.attachSubmarine = (((attachTarget != null) ? attachTarget.Submarine : null) ?? this.item.Submarine);
					int maxAttachableCount = (int)character.Info.GetSavedStatValueWithBotsInMp(StatTypes.MaxAttachableCount, this.item.Prefab.Identifier);
					int currentlyAttachedCount = Item.ItemList.Count(delegate(Item i)
					{
						if (i.Submarine == CS$<>8__locals1.attachSubmarine)
						{
							Holdable holdable = i.GetComponent<Holdable>();
							if (holdable != null && holdable.Attached)
							{
								return i.Prefab.Identifier == CS$<>8__locals1.<>4__this.item.Prefab.Identifier;
							}
						}
						return false;
					});
					if (maxAttachableCount == 0)
					{
						if (character == Character.Controlled)
						{
							GUI.AddMessage(TextManager.Get("itemmsgrequiretraining"), Color.Red, null, true, null);
						}
						return false;
					}
					if (currentlyAttachedCount >= maxAttachableCount)
					{
						if (character == Character.Controlled)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 3);
							defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("itemmsgtotalnumberlimited"));
							defaultInterpolatedStringHandler.AppendLiteral(" (");
							defaultInterpolatedStringHandler.AppendFormatted<int>(currentlyAttachedCount);
							defaultInterpolatedStringHandler.AppendLiteral("/");
							defaultInterpolatedStringHandler.AppendFormatted<int>(maxAttachableCount);
							defaultInterpolatedStringHandler.AppendLiteral(")");
							GUI.AddMessage(defaultInterpolatedStringHandler.ToStringAndClear(), Color.Red, null, true, null);
						}
						return false;
					}
				}
				if (GameMain.NetworkMember != null)
				{
					if (character == Character.Controlled)
					{
						Vector2 attachPos2 = ConvertUnits.ToSimUnits(this.GetAttachPosition(character, false));
						this.item.CreateClientEvent<Holdable>(this, new Holdable.AttachEventData(attachPos2, character));
					}
					return false;
				}
				this.item.Drop(character, true, true);
				this.item.SetTransform(ConvertUnits.ToSimUnits(this.GetAttachPosition(character, false)), 0f, false, true, null);
				this.item.CurrentHull = Hull.FindHull(this.item.WorldPosition, this.item.CurrentHull, true, true);
				Holdable.<Use>g__RefreshLightSources|158_0(this.item);
				this.AttachToWall();
			}
			return true;
		}

		// Token: 0x0600591C RID: 22812 RVA: 0x002DEE12 File Offset: 0x002DD012
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return true;
		}

		// Token: 0x0600591D RID: 22813 RVA: 0x002DEE18 File Offset: 0x002DD018
		private Vector2 GetAttachPosition(Character user, bool useWorldCoordinates = false)
		{
			if (user != null)
			{
				Vector2 mouseDiff = user.CursorWorldPosition - user.WorldPosition;
				mouseDiff = mouseDiff.ClampLength(114f);
				Vector2 submarinePos = (useWorldCoordinates && user.Submarine != null) ? user.Submarine.Position : Vector2.Zero;
				Vector2 userPos = useWorldCoordinates ? user.WorldPosition : user.Position;
				Vector2 attachPos = userPos + mouseDiff;
				Vector2 halfSize = new Vector2((float)this.item.Rect.Width, (float)this.item.Rect.Height) / 2f;
				Vector2 offset = new Vector2(-halfSize.X % Submarine.GridSize.X, halfSize.Y % Submarine.GridSize.Y);
				if (user.Submarine != null)
				{
					Vector2 padding = halfSize * new Vector2((float)Math.Sign(mouseDiff.X), (float)Math.Sign(mouseDiff.Y));
					if (Submarine.PickBody(ConvertUnits.ToSimUnits(user.Position), ConvertUnits.ToSimUnits(user.Position + mouseDiff + padding), null, new Category?(Category.Cat1), this.AllowAttachInsideDoors, (Fixture fixture) => !(fixture.UserData is Door), false) != null)
					{
						Vector2 pickedPos = userPos + mouseDiff * Submarine.LastPickedFraction + offset - submarinePos;
						attachPos = new Vector2(Holdable.<GetAttachPosition>g__RoundToGrid|160_0(pickedPos.X, Submarine.GridSize.X, -Math.Sign(mouseDiff.X)), Holdable.<GetAttachPosition>g__RoundToGrid|160_0(pickedPos.Y, Submarine.GridSize.Y, -Math.Sign(mouseDiff.Y))) - offset + submarinePos;
					}
					if (this.AttachesToFloor)
					{
						float size = (float)this.item.Rect.Height / 2f;
						Vector2 rayStart = attachPos - submarinePos;
						Vector2 rayEnd = rayStart - Vector2.UnitY * 114f * 2f;
						if (Submarine.PickBody(ConvertUnits.ToSimUnits(rayStart), ConvertUnits.ToSimUnits(rayEnd), null, new Category?(Category.Cat1 | Category.Cat3), true, null, false) == null)
						{
							return Vector2.Zero;
						}
						attachPos = ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition) + Vector2.UnitY * size + submarinePos;
					}
				}
				else if (Level.Loaded != null)
				{
					bool edgeFound = false;
					foreach (VoronoiCell cell in Level.Loaded.GetCells(attachPos, 2))
					{
						if (cell.CellType == CellType.Solid)
						{
							foreach (GraphEdge edge in cell.Edges)
							{
								Vector2 intersection;
								if (edge.IsSolid && MathUtils.GetLineSegmentIntersection(edge.Point1, edge.Point2, user.WorldPosition, attachPos, out intersection))
								{
									attachPos = intersection;
									edgeFound = true;
									break;
								}
							}
							if (edgeFound)
							{
								break;
							}
						}
					}
				}
				Vector2 offsetAttachPos = attachPos + offset - submarinePos;
				return new Vector2(Holdable.<GetAttachPosition>g__RoundToGrid|160_0(offsetAttachPos.X, Submarine.GridSize.X, 0), this.AttachesToFloor ? offsetAttachPos.Y : Holdable.<GetAttachPosition>g__RoundToGrid|160_0(offsetAttachPos.Y, Submarine.GridSize.Y, 0)) - offset + submarinePos;
			}
			if (!useWorldCoordinates)
			{
				return this.item.Position;
			}
			return this.item.WorldPosition;
		}

		// Token: 0x0600591E RID: 22814 RVA: 0x002DF1C8 File Offset: 0x002DD3C8
		private VoronoiCell GetAttachTargetCell(float maxDist)
		{
			if (Level.Loaded == null)
			{
				return null;
			}
			foreach (VoronoiCell cell in Level.Loaded.GetCells(this.item.WorldPosition, 1))
			{
				if (cell.CellType == CellType.Solid)
				{
					Vector2 diff = cell.Center - this.item.WorldPosition;
					if (diff.LengthSquared() > 0.0001f)
					{
						diff = Vector2.Normalize(diff);
					}
					if (cell.IsPointInside(this.item.WorldPosition + diff * maxDist))
					{
						return cell;
					}
				}
			}
			return null;
		}

		// Token: 0x0600591F RID: 22815 RVA: 0x002DF288 File Offset: 0x002DD488
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06005920 RID: 22816 RVA: 0x002DF294 File Offset: 0x002DD494
		public Rope GetRope()
		{
			RangedWeapon rangedWeapon = base.Item.GetComponent<RangedWeapon>();
			if (rangedWeapon != null)
			{
				Projectile lastProjectile = rangedWeapon.LastProjectile;
				if (lastProjectile != null)
				{
					return lastProjectile.Item.GetComponent<Rope>();
				}
			}
			return null;
		}

		// Token: 0x06005921 RID: 22817 RVA: 0x002DF2C8 File Offset: 0x002DD4C8
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.item.body == null || !this.item.body.Enabled)
			{
				return;
			}
			Character owner = this.picker ?? (this.item.GetRootInventoryOwner() as Character);
			if (owner != null)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, owner, null, null, null, null, 1f);
			}
			if (this.picker == null || !this.picker.HasEquippedItem(this.item, null, null))
			{
				if (this.Pusher != null)
				{
					this.Pusher.Enabled = false;
				}
				if (this.attachTargetCell == null && owner == null)
				{
					this.IsActive = false;
				}
				return;
			}
			if (this.picker == Character.Controlled && this.picker.IsKeyDown(InputType.Aim) && this.attachable && this.Reattachable)
			{
				base.Drawable = true;
			}
			Vector2 swingPos;
			this.UpdateSwingPos(deltaTime, out swingPos);
			if (this.item.body.Dir != this.picker.AnimController.Dir)
			{
				this.item.FlipX(false, false);
			}
			this.item.Submarine = this.picker.Submarine;
			if (this.picker.HeldItems.Contains(this.item))
			{
				this.scaledHandlePos[0] = this.handlePos[0] * this.item.Scale;
				this.scaledHandlePos[1] = this.handlePos[1] * this.item.Scale;
				bool aim = this.picker.IsKeyDown(InputType.Aim) && this.aimPos != Vector2.Zero && this.picker.CanAim && !this.UsageDisabledByRangedWeapon(this.picker);
				if (aim)
				{
					if (this.picker.AnimController.IsHoldingToRope)
					{
						Rope rope = this.GetRope();
						if (rope != null && !rope.Snapped)
						{
							Vector2 targetPos = Submarine.GetRelativeSimPosition(this.picker, rope.Item, null);
							this.picker.AnimController.HoldItem(deltaTime, this.item, this.scaledHandlePos, this.aimPos, true, this.holdAngle, this.aimAngle, false, new Vector2?(targetPos));
							return;
						}
					}
					this.picker.AnimController.HoldItem(deltaTime, this.item, this.scaledHandlePos, this.aimPos + swingPos, true, this.holdAngle, this.aimAngle, false, null);
					return;
				}
				this.picker.AnimController.HoldItem(deltaTime, this.item, this.scaledHandlePos, this.holdPos + swingPos, false, this.holdAngle, 0f, false, null);
				Rope rope2 = this.GetRope();
				if (rope2 != null && rope2.SnapWhenNotAimed && rope2.Item.ParentInventory == null)
				{
					rope2.Snap();
					return;
				}
			}
			else
			{
				Rope rope3 = this.GetRope();
				if (rope3 != null)
				{
					rope3.Snap();
				}
				Limb equipLimb = null;
				if (this.picker.Inventory.IsInLimbSlot(this.item, InvSlotType.Headset) || this.picker.Inventory.IsInLimbSlot(this.item, InvSlotType.Head))
				{
					equipLimb = this.picker.AnimController.GetLimb(LimbType.Head, true, false, false);
				}
				else if (this.picker.Inventory.IsInLimbSlot(this.item, InvSlotType.InnerClothes) || this.picker.Inventory.IsInLimbSlot(this.item, InvSlotType.OuterClothes))
				{
					equipLimb = this.picker.AnimController.GetLimb(LimbType.Torso, true, false, false);
				}
				if (equipLimb != null && !equipLimb.Removed)
				{
					float itemAngle = equipLimb.Rotation + this.holdAngle * this.picker.AnimController.Dir;
					Matrix itemTransfrom = Matrix.CreateRotationZ(equipLimb.Rotation);
					Vector2 transformedHandlePos = Vector2.Transform(this.handlePos[0] * this.item.Scale, itemTransfrom);
					this.item.body.ResetDynamics();
					this.item.SetTransform(equipLimb.SimPosition - transformedHandlePos, itemAngle, true, true, null);
				}
			}
		}

		// Token: 0x06005922 RID: 22818 RVA: 0x002DF720 File Offset: 0x002DD920
		public void UpdateSwingPos(float deltaTime, out Vector2 swingPos)
		{
			swingPos = Vector2.Zero;
			if (this.swingAmount != Vector2.Zero && !this.picker.IsUnconscious && this.picker.Stun <= 0f)
			{
				this.swingState += deltaTime;
				this.swingState %= 1f;
				if (this.SwingWhenHolding || (this.SwingWhenAiming && this.picker.IsKeyDown(InputType.Aim)) || (this.SwingWhenUsing && this.picker.IsKeyDown(InputType.Aim) && this.picker.IsKeyDown(InputType.Shoot)))
				{
					swingPos = this.swingAmount * new Vector2(PerlinNoise.GetPerlin(this.swingState * this.SwingSpeed * 0.1f, this.swingState * this.SwingSpeed * 0.1f) - 0.5f, PerlinNoise.GetPerlin(this.swingState * this.SwingSpeed * 0.1f + 0.5f, this.swingState * this.SwingSpeed * 0.1f + 0.5f) - 0.5f);
				}
			}
		}

		// Token: 0x06005923 RID: 22819 RVA: 0x002DF862 File Offset: 0x002DDA62
		protected bool UsageDisabledByRangedWeapon(Character character)
		{
			if (this.DisableWhenRangedWeaponEquipped && character != null)
			{
				if (character.HeldItems.Any((Item it) => it.GetComponent<RangedWeapon>() != null))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06005924 RID: 22820 RVA: 0x002DF89E File Offset: 0x002DDA9E
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
		}

		// Token: 0x06005925 RID: 22821 RVA: 0x002DF8A0 File Offset: 0x002DDAA0
		public override void FlipX(bool relativeToSub)
		{
			this.handlePos[0].X = -this.handlePos[0].X;
			this.handlePos[1].X = -this.handlePos[1].X;
			if (this.item.body != null)
			{
				this.item.body.Dir = -this.item.body.Dir;
			}
		}

		// Token: 0x06005926 RID: 22822 RVA: 0x002DF921 File Offset: 0x002DDB21
		public override void OnItemLoaded()
		{
			if (this.item.Submarine != null && this.item.Submarine.Loading)
			{
				return;
			}
			this.OnMapLoaded();
			this.item.SetActiveSprite();
		}

		// Token: 0x06005927 RID: 22823 RVA: 0x002DF954 File Offset: 0x002DDB54
		public override void OnMapLoaded()
		{
			if (!this.attachable)
			{
				return;
			}
			if (!this.loadedFromInstance && this.attachedByDefault)
			{
				this.AttachToWall();
				return;
			}
			if (this.Attached)
			{
				this.AttachToWall();
				return;
			}
			if (this.originalBody != null)
			{
				this.originalBody.SetTransformIgnoreContacts(this.item.SimPosition, this.item.Rotation, true);
				this.item.body = this.originalBody;
				this.originalBody.Enabled = (this.item.ParentInventory == null);
			}
			this.DeattachFromWall();
		}

		// Token: 0x06005928 RID: 22824 RVA: 0x002DF9EB File Offset: 0x002DDBEB
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.attachTargetCell = null;
			if (this.Pusher != null)
			{
				this.Pusher.Remove();
				this.Pusher = null;
			}
			this.originalBody = null;
		}

		// Token: 0x06005929 RID: 22825 RVA: 0x002DFA1C File Offset: 0x002DDC1C
		public override XElement Save(XElement parentElement)
		{
			if (!this.attachable)
			{
				return base.Save(parentElement);
			}
			LocalizedString tempMsg = base.DisplayMsg;
			Dictionary<RelatedItem.RelationType, List<RelatedItem>> tempRequiredItems = this.RequiredItems;
			base.DisplayMsg = this.prevMsg;
			this.RequiredItems = this.prevRequiredItems;
			XElement saveElement = base.Save(parentElement);
			base.DisplayMsg = tempMsg;
			this.RequiredItems = tempRequiredItems;
			return saveElement;
		}

		// Token: 0x0600592C RID: 22828 RVA: 0x002DFAA4 File Offset: 0x002DDCA4
		[CompilerGenerated]
		internal static void <Use>g__RefreshLightSources|158_0(Item item)
		{
			PhysicsBody body = item.body;
			if (body != null)
			{
				body.UpdateDrawPosition(true);
			}
			foreach (LightComponent light in item.GetComponents<LightComponent>())
			{
				light.SetLightSourceTransform();
			}
			ItemContainer component = item.GetComponent<ItemContainer>();
			if (component != null)
			{
				component.SetContainedItemPositions();
			}
			foreach (Item containedItem in item.ContainedItems)
			{
				Holdable.<Use>g__RefreshLightSources|158_0(containedItem);
			}
		}

		// Token: 0x0600592D RID: 22829 RVA: 0x002DFB50 File Offset: 0x002DDD50
		[CompilerGenerated]
		internal static float <GetAttachPosition>g__RoundToGrid|160_0(float position, float gridSize, int roundingDir = 0)
		{
			if (roundingDir < 0)
			{
				return MathF.Floor(position / gridSize) * gridSize;
			}
			if (roundingDir > 0)
			{
				return MathF.Ceiling(position / gridSize) * gridSize;
			}
			return MathUtils.RoundTowardsClosest(position, gridSize);
		}

		// Token: 0x04002D51 RID: 11601
		private const float MaxAttachDistance = 114f;

		// Token: 0x04002D52 RID: 11602
		protected Vector2[] handlePos;

		// Token: 0x04002D53 RID: 11603
		private readonly Vector2[] scaledHandlePos;

		// Token: 0x04002D54 RID: 11604
		private readonly InputType prevPickKey;

		// Token: 0x04002D55 RID: 11605
		private LocalizedString prevMsg;

		// Token: 0x04002D56 RID: 11606
		private Dictionary<RelatedItem.RelationType, List<RelatedItem>> prevRequiredItems;

		// Token: 0x04002D57 RID: 11607
		private float swingState;

		// Token: 0x04002D58 RID: 11608
		private Character prevEquipper;

		// Token: 0x04002D59 RID: 11609
		private bool attachable;

		// Token: 0x04002D5A RID: 11610
		private bool attached;

		// Token: 0x04002D5B RID: 11611
		private bool attachedByDefault;

		// Token: 0x04002D5C RID: 11612
		private VoronoiCell attachTargetCell;

		// Token: 0x04002D5D RID: 11613
		private PhysicsBody originalBody;

		// Token: 0x04002D5E RID: 11614
		public readonly ImmutableDictionary<StatTypes, float> HoldableStatValues;

		// Token: 0x04002D69 RID: 11625
		private HashSet<Identifier> disallowAttachingOverTags = new HashSet<Identifier>();

		// Token: 0x04002D6B RID: 11627
		protected Vector2 holdPos;

		// Token: 0x04002D6C RID: 11628
		protected Vector2 aimPos;

		// Token: 0x04002D6D RID: 11629
		protected float holdAngle;

		// Token: 0x04002D6E RID: 11630
		protected float aimAngle;

		// Token: 0x04002D6F RID: 11631
		private Vector2 swingAmount;

		// Token: 0x04002D78 RID: 11640
		private bool secondHandlePosDefined;

		// Token: 0x04002D79 RID: 11641
		private bool loadedFromInstance;

		// Token: 0x04002D7A RID: 11642
		private static List<Item> tempOverlappingItems = new List<Item>();

		// Token: 0x020013AE RID: 5038
		private readonly struct AttachEventData : ItemComponent.IEventData
		{
			// Token: 0x06009810 RID: 38928 RVA: 0x003DCA4B File Offset: 0x003DAC4B
			public AttachEventData(Vector2 attachPos, Character attacher)
			{
				this.AttachPos = attachPos;
				this.Attacher = attacher;
			}

			// Token: 0x04006328 RID: 25384
			public readonly Vector2 AttachPos;

			// Token: 0x04006329 RID: 25385
			public readonly Character Attacher;
		}
	}
}

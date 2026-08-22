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
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000495 RID: 1173
	internal class Holdable : Pickable, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06003F3C RID: 16188 RVA: 0x00196E38 File Offset: 0x00195038
		public override void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			base.ServerEventWrite(msg, c, extraData);
			bool writeAttachData = this.attachable && this.originalBody != null;
			msg.WriteBoolean(writeAttachData);
			if (!writeAttachData)
			{
				return;
			}
			ushort attacherId = 0;
			Holdable.AttachEventData attachEventData;
			if (base.TryExtractEventData<Holdable.AttachEventData>(extraData, out attachEventData) && attachEventData.Attacher != null)
			{
				attacherId = attachEventData.Attacher.ID;
			}
			msg.WriteBoolean(this.Attached);
			msg.WriteSingle(this.originalBody.SimPosition.X);
			msg.WriteSingle(this.originalBody.SimPosition.Y);
			Submarine submarine = this.item.Submarine;
			msg.WriteUInt16((submarine != null) ? submarine.ID : 0);
			msg.WriteUInt16(attacherId);
		}

		// Token: 0x06003F3D RID: 16189 RVA: 0x00196EEC File Offset: 0x001950EC
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			Vector2 simPosition = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			if (!this.item.CanClientAccess(c) || !this.Attachable || this.attached || !MathUtils.IsValid(simPosition))
			{
				return;
			}
			Vector2 offset = simPosition - c.Character.SimPosition;
			offset = offset.ClampLength(171f);
			simPosition = c.Character.SimPosition + offset;
			this.Drop(false, null, true);
			this.item.SetTransform(simPosition, 0f, false, true, null);
			this.item.CurrentHull = Hull.FindHull(this.item.WorldPosition, this.item.CurrentHull, true, true);
			this.AttachToWall();
			this.OnUsed.Invoke(new ItemComponent.ItemUseInfo(this.item, c.Character));
			this.item.CreateServerEvent<Holdable>(this, new Holdable.AttachEventData(simPosition, c.Character));
			CharacterInventory inventory = c.Character.Inventory;
			if (inventory != null)
			{
				inventory.CreateNetworkEvent();
			}
			GameServer.Log(GameServer.CharacterLogName(c.Character) + " attached " + this.item.Name + " to a wall", ServerLog.MessageType.ItemInteraction);
		}

		// Token: 0x170010B7 RID: 4279
		// (get) Token: 0x06003F3E RID: 16190 RVA: 0x00197028 File Offset: 0x00195228
		public override bool IsAttached
		{
			get
			{
				return this.Attached;
			}
		}

		// Token: 0x170010B8 RID: 4280
		// (get) Token: 0x06003F3F RID: 16191 RVA: 0x00197030 File Offset: 0x00195230
		// (set) Token: 0x06003F40 RID: 16192 RVA: 0x00197038 File Offset: 0x00195238
		public PhysicsBody Pusher { get; private set; }

		// Token: 0x170010B9 RID: 4281
		// (get) Token: 0x06003F41 RID: 16193 RVA: 0x00197041 File Offset: 0x00195241
		// (set) Token: 0x06003F42 RID: 16194 RVA: 0x00197049 File Offset: 0x00195249
		[Serialize(true, IsPropertySaveable.Yes, "Is the item currently able to push characters around? True by default. Only valid if blocksplayers is set to true.", "", false)]
		public bool CanPush { get; set; }

		// Token: 0x170010BA RID: 4282
		// (get) Token: 0x06003F43 RID: 16195 RVA: 0x00197052 File Offset: 0x00195252
		public PhysicsBody Body
		{
			get
			{
				return this.item.body ?? this.originalBody;
			}
		}

		// Token: 0x170010BB RID: 4283
		// (get) Token: 0x06003F44 RID: 16196 RVA: 0x00197069 File Offset: 0x00195269
		// (set) Token: 0x06003F45 RID: 16197 RVA: 0x00197083 File Offset: 0x00195283
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

		// Token: 0x170010BC RID: 4284
		// (get) Token: 0x06003F46 RID: 16198 RVA: 0x001970A2 File Offset: 0x001952A2
		// (set) Token: 0x06003F47 RID: 16199 RVA: 0x001970AA File Offset: 0x001952AA
		[Serialize(true, IsPropertySaveable.Yes, "Can the item be pointed to a specific direction or do the characters always hold it in a static pose.", "", false)]
		public bool Aimable { get; set; }

		// Token: 0x170010BD RID: 4285
		// (get) Token: 0x06003F48 RID: 16200 RVA: 0x001970B3 File Offset: 0x001952B3
		// (set) Token: 0x06003F49 RID: 16201 RVA: 0x001970BB File Offset: 0x001952BB
		[Serialize(0f, IsPropertySaveable.Yes, "Camera offset to apply when aiming this item. Only valid if Aimable is set to true.", "", false)]
		public float CameraAimOffset { get; set; }

		// Token: 0x170010BE RID: 4286
		// (get) Token: 0x06003F4A RID: 16202 RVA: 0x001970C4 File Offset: 0x001952C4
		// (set) Token: 0x06003F4B RID: 16203 RVA: 0x001970CC File Offset: 0x001952CC
		[Serialize(false, IsPropertySaveable.No, "Should the character adjust its pose when aiming with the item. Most noticeable underwater, where the character will rotate its entire body to face the direction the item is aimed at.", "", false)]
		public bool ControlPose { get; set; }

		// Token: 0x170010BF RID: 4287
		// (get) Token: 0x06003F4C RID: 16204 RVA: 0x001970D5 File Offset: 0x001952D5
		// (set) Token: 0x06003F4D RID: 16205 RVA: 0x001970DD File Offset: 0x001952DD
		[Serialize(false, IsPropertySaveable.No, "Use the hand rotation instead of torso rotation for the item hold angle. Enable this if you want the item just to follow with the arm when not aiming instead of forcing the arm to a hold pose.", "", false)]
		public bool UseHandRotationForHoldAngle { get; set; }

		// Token: 0x170010C0 RID: 4288
		// (get) Token: 0x06003F4E RID: 16206 RVA: 0x001970E6 File Offset: 0x001952E6
		// (set) Token: 0x06003F4F RID: 16207 RVA: 0x001970EE File Offset: 0x001952EE
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

		// Token: 0x170010C1 RID: 4289
		// (get) Token: 0x06003F50 RID: 16208 RVA: 0x001970F7 File Offset: 0x001952F7
		// (set) Token: 0x06003F51 RID: 16209 RVA: 0x001970FF File Offset: 0x001952FF
		[Serialize(true, IsPropertySaveable.No, "Can the item be reattached to walls after it has been deattached (only valid if Attachable is set to true).", "", false)]
		public bool Reattachable { get; set; }

		// Token: 0x170010C2 RID: 4290
		// (get) Token: 0x06003F52 RID: 16210 RVA: 0x00197108 File Offset: 0x00195308
		// (set) Token: 0x06003F53 RID: 16211 RVA: 0x00197110 File Offset: 0x00195310
		[Serialize(false, IsPropertySaveable.No, "Can the item only be attached in limited amount? Uses permanent stat values to check for legibility.", "", false)]
		public bool LimitedAttachable { get; set; }

		// Token: 0x170010C3 RID: 4291
		// (get) Token: 0x06003F54 RID: 16212 RVA: 0x00197119 File Offset: 0x00195319
		// (set) Token: 0x06003F55 RID: 16213 RVA: 0x00197121 File Offset: 0x00195321
		[Serialize(false, IsPropertySaveable.No, "When enabled, the item can only be attached to a position where it touches the floor.", "", false)]
		public bool AttachesToFloor { get; set; }

		// Token: 0x170010C4 RID: 4292
		// (get) Token: 0x06003F56 RID: 16214 RVA: 0x0019712A File Offset: 0x0019532A
		// (set) Token: 0x06003F57 RID: 16215 RVA: 0x00197132 File Offset: 0x00195332
		[Serialize(true, IsPropertySaveable.No, "Can the item be attached inside doors?", "", false)]
		public bool AllowAttachInsideDoors { get; set; }

		// Token: 0x170010C5 RID: 4293
		// (get) Token: 0x06003F58 RID: 16216 RVA: 0x0019713B File Offset: 0x0019533B
		// (set) Token: 0x06003F59 RID: 16217 RVA: 0x0019714D File Offset: 0x0019534D
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

		// Token: 0x170010C6 RID: 4294
		// (get) Token: 0x06003F5A RID: 16218 RVA: 0x00197165 File Offset: 0x00195365
		// (set) Token: 0x06003F5B RID: 16219 RVA: 0x0019716D File Offset: 0x0019536D
		[Serialize("0,0", IsPropertySaveable.Yes, "", "", false)]
		public Point DisallowAttachingOverSize { get; set; }

		// Token: 0x170010C7 RID: 4295
		// (get) Token: 0x06003F5C RID: 16220 RVA: 0x00197176 File Offset: 0x00195376
		// (set) Token: 0x06003F5D RID: 16221 RVA: 0x0019717E File Offset: 0x0019537E
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

		// Token: 0x170010C8 RID: 4296
		// (get) Token: 0x06003F5E RID: 16222 RVA: 0x00197187 File Offset: 0x00195387
		// (set) Token: 0x06003F5F RID: 16223 RVA: 0x00197194 File Offset: 0x00195394
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

		// Token: 0x170010C9 RID: 4297
		// (get) Token: 0x06003F60 RID: 16224 RVA: 0x001971A2 File Offset: 0x001953A2
		// (set) Token: 0x06003F61 RID: 16225 RVA: 0x001971AF File Offset: 0x001953AF
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

		// Token: 0x170010CA RID: 4298
		// (get) Token: 0x06003F62 RID: 16226 RVA: 0x001971BD File Offset: 0x001953BD
		// (set) Token: 0x06003F63 RID: 16227 RVA: 0x001971CA File Offset: 0x001953CA
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

		// Token: 0x170010CB RID: 4299
		// (get) Token: 0x06003F64 RID: 16228 RVA: 0x001971D8 File Offset: 0x001953D8
		// (set) Token: 0x06003F65 RID: 16229 RVA: 0x001971E5 File Offset: 0x001953E5
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

		// Token: 0x170010CC RID: 4300
		// (get) Token: 0x06003F66 RID: 16230 RVA: 0x001971F3 File Offset: 0x001953F3
		// (set) Token: 0x06003F67 RID: 16231 RVA: 0x00197200 File Offset: 0x00195400
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

		// Token: 0x170010CD RID: 4301
		// (get) Token: 0x06003F68 RID: 16232 RVA: 0x0019720E File Offset: 0x0019540E
		// (set) Token: 0x06003F69 RID: 16233 RVA: 0x00197216 File Offset: 0x00195416
		[Serialize(0f, IsPropertySaveable.No, "", "", false)]
		public float SwingSpeed { get; set; }

		// Token: 0x170010CE RID: 4302
		// (get) Token: 0x06003F6A RID: 16234 RVA: 0x0019721F File Offset: 0x0019541F
		// (set) Token: 0x06003F6B RID: 16235 RVA: 0x00197227 File Offset: 0x00195427
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool SwingWhenHolding { get; set; }

		// Token: 0x170010CF RID: 4303
		// (get) Token: 0x06003F6C RID: 16236 RVA: 0x00197230 File Offset: 0x00195430
		// (set) Token: 0x06003F6D RID: 16237 RVA: 0x00197238 File Offset: 0x00195438
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool SwingWhenAiming { get; set; }

		// Token: 0x170010D0 RID: 4304
		// (get) Token: 0x06003F6E RID: 16238 RVA: 0x00197241 File Offset: 0x00195441
		// (set) Token: 0x06003F6F RID: 16239 RVA: 0x00197249 File Offset: 0x00195449
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool SwingWhenUsing { get; set; }

		// Token: 0x170010D1 RID: 4305
		// (get) Token: 0x06003F70 RID: 16240 RVA: 0x00197252 File Offset: 0x00195452
		// (set) Token: 0x06003F71 RID: 16241 RVA: 0x0019725A File Offset: 0x0019545A
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool DisableHeadRotation { get; set; }

		// Token: 0x170010D2 RID: 4306
		// (get) Token: 0x06003F72 RID: 16242 RVA: 0x00197263 File Offset: 0x00195463
		// (set) Token: 0x06003F73 RID: 16243 RVA: 0x0019726B File Offset: 0x0019546B
		[Serialize(false, IsPropertySaveable.No, "If true, this item can't be used if the character is also holding a ranged weapon.", "", false)]
		public bool DisableWhenRangedWeaponEquipped { get; set; }

		// Token: 0x170010D3 RID: 4307
		// (get) Token: 0x06003F74 RID: 16244 RVA: 0x00197274 File Offset: 0x00195474
		// (set) Token: 0x06003F75 RID: 16245 RVA: 0x0019727C File Offset: 0x0019547C
		[ConditionallyEditable(ConditionallyEditable.ConditionType.Attachable, true, MinValueFloat = 0f, MaxValueFloat = 0.999f, DecimalCount = 3)]
		[Serialize(0.55f, IsPropertySaveable.No, "Sprite depth that's used when the item is NOT attached to a wall.", "", false)]
		public float SpriteDepthWhenDropped { get; set; }

		// Token: 0x170010D4 RID: 4308
		// (get) Token: 0x06003F76 RID: 16246 RVA: 0x00197285 File Offset: 0x00195485
		// (set) Token: 0x06003F77 RID: 16247 RVA: 0x0019728D File Offset: 0x0019548D
		[Editable]
		[Serialize("", IsPropertySaveable.Yes, "A text displayed next to the item when it's been dropped on the floor (not attached to a wall).", "ItemMsg", false)]
		public string MsgWhenDropped { get; set; }

		// Token: 0x170010D5 RID: 4309
		// (get) Token: 0x06003F78 RID: 16248 RVA: 0x00197296 File Offset: 0x00195496
		// (set) Token: 0x06003F79 RID: 16249 RVA: 0x001972AC File Offset: 0x001954AC
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

		// Token: 0x170010D6 RID: 4310
		// (get) Token: 0x06003F7A RID: 16250 RVA: 0x0019730A File Offset: 0x0019550A
		// (set) Token: 0x06003F7B RID: 16251 RVA: 0x00197320 File Offset: 0x00195520
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

		// Token: 0x06003F7C RID: 16252 RVA: 0x00197370 File Offset: 0x00195570
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

		// Token: 0x06003F7D RID: 16253 RVA: 0x00197600 File Offset: 0x00195800
		private bool OnPusherCollision(Fixture sender, Fixture other, Contact contact)
		{
			Character character = other.Body.UserData as Character;
			return character == null || (this.IsActive && this.CanPush && character != this.picker);
		}

		// Token: 0x06003F7E RID: 16254 RVA: 0x00197644 File Offset: 0x00195844
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

		// Token: 0x06003F7F RID: 16255 RVA: 0x001976A2 File Offset: 0x001958A2
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.Drop(true, dropper, setTransform);
		}

		// Token: 0x06003F80 RID: 16256 RVA: 0x001976B0 File Offset: 0x001958B0
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

		// Token: 0x06003F81 RID: 16257 RVA: 0x0019793C File Offset: 0x00195B3C
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
				if (this.picker != this.prevEquipper)
				{
					GameServer.Log(GameServer.CharacterLogName(character) + " equipped " + this.item.Name, ServerLog.MessageType.ItemInteraction);
				}
				this.prevEquipper = this.picker;
				return;
			}
			this.prevEquipper = null;
		}

		// Token: 0x06003F82 RID: 16258 RVA: 0x00197BF0 File Offset: 0x00195DF0
		public override void Unequip(Character character)
		{
			if (this.prevEquipper != null)
			{
				GameServer.Log(GameServer.CharacterLogName(character) + " unequipped " + this.item.Name, ServerLog.MessageType.ItemInteraction);
			}
			this.prevEquipper = null;
			if (this.picker == null)
			{
				return;
			}
			this.item.body.PhysEnabled = true;
			this.item.body.Enabled = false;
			this.IsActive = false;
		}

		// Token: 0x06003F83 RID: 16259 RVA: 0x00197C60 File Offset: 0x00195E60
		public bool CanBeAttached(Character user)
		{
			IEnumerable<Item> enumerable;
			return this.CanBeAttached(user, out enumerable);
		}

		// Token: 0x06003F84 RID: 16260 RVA: 0x00197C78 File Offset: 0x00195E78
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

		// Token: 0x06003F85 RID: 16261 RVA: 0x00197FA0 File Offset: 0x001961A0
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

		// Token: 0x06003F86 RID: 16262 RVA: 0x001980A4 File Offset: 0x001962A4
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

		// Token: 0x06003F87 RID: 16263 RVA: 0x00198120 File Offset: 0x00196320
		public override bool OnPicked(Character picker)
		{
			bool wasAttached = this.IsAttached;
			if (base.OnPicked(picker))
			{
				this.DeattachFromWall();
				if (GameMain.Server != null && this.attachable)
				{
					this.item.CreateServerEvent<Holdable>(this);
					if (picker != null && wasAttached)
					{
						GameServer.Log(GameServer.CharacterLogName(picker) + " detached " + this.item.Name + " from a wall", ServerLog.MessageType.ItemInteraction);
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06003F88 RID: 16264 RVA: 0x00198190 File Offset: 0x00196390
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
		}

		// Token: 0x06003F89 RID: 16265 RVA: 0x00198418 File Offset: 0x00196618
		public void DeattachFromWall()
		{
			if (!this.attachable)
			{
				return;
			}
			this.Attached = false;
			this.attachTargetCell = null;
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
			foreach (LightComponent light in this.item.GetComponents<LightComponent>())
			{
				light.CheckIfNeedsUpdate();
			}
		}

		// Token: 0x06003F8A RID: 16266 RVA: 0x001984F0 File Offset: 0x001966F0
		public override void ParseMsg()
		{
			base.ParseMsg();
			if (this.Attachable)
			{
				this.prevMsg = base.DisplayMsg;
			}
		}

		// Token: 0x06003F8B RID: 16267 RVA: 0x0019850C File Offset: 0x0019670C
		public override bool Use(float deltaTime, Character character = null)
		{
			Holdable.<>c__DisplayClass154_0 CS$<>8__locals1 = new Holdable.<>c__DisplayClass154_0();
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
					Holdable.<>c__DisplayClass154_0 CS$<>8__locals2 = CS$<>8__locals1;
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
						return false;
					}
					if (currentlyAttachedCount >= maxAttachableCount)
					{
						return false;
					}
				}
				if (GameMain.NetworkMember != null)
				{
					return false;
				}
				this.item.Drop(character, true, true);
				this.item.SetTransform(ConvertUnits.ToSimUnits(this.GetAttachPosition(character, false)), 0f, false, true, null);
				this.item.CurrentHull = Hull.FindHull(this.item.WorldPosition, this.item.CurrentHull, true, true);
				Holdable.<Use>g__RefreshLightSources|154_0(this.item);
				this.AttachToWall();
			}
			return true;
		}

		// Token: 0x06003F8C RID: 16268 RVA: 0x00198693 File Offset: 0x00196893
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			return true;
		}

		// Token: 0x06003F8D RID: 16269 RVA: 0x00198698 File Offset: 0x00196898
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
						attachPos = new Vector2(Holdable.<GetAttachPosition>g__RoundToGrid|156_0(pickedPos.X, Submarine.GridSize.X, -Math.Sign(mouseDiff.X)), Holdable.<GetAttachPosition>g__RoundToGrid|156_0(pickedPos.Y, Submarine.GridSize.Y, -Math.Sign(mouseDiff.Y))) - offset + submarinePos;
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
				return new Vector2(Holdable.<GetAttachPosition>g__RoundToGrid|156_0(offsetAttachPos.X, Submarine.GridSize.X, 0), this.AttachesToFloor ? offsetAttachPos.Y : Holdable.<GetAttachPosition>g__RoundToGrid|156_0(offsetAttachPos.Y, Submarine.GridSize.Y, 0)) - offset + submarinePos;
			}
			if (!useWorldCoordinates)
			{
				return this.item.Position;
			}
			return this.item.WorldPosition;
		}

		// Token: 0x06003F8E RID: 16270 RVA: 0x00198A48 File Offset: 0x00196C48
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

		// Token: 0x06003F8F RID: 16271 RVA: 0x00198B08 File Offset: 0x00196D08
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06003F90 RID: 16272 RVA: 0x00198B14 File Offset: 0x00196D14
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

		// Token: 0x06003F91 RID: 16273 RVA: 0x00198B48 File Offset: 0x00196D48
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

		// Token: 0x06003F92 RID: 16274 RVA: 0x00198FA0 File Offset: 0x001971A0
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

		// Token: 0x06003F93 RID: 16275 RVA: 0x001990E2 File Offset: 0x001972E2
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

		// Token: 0x06003F94 RID: 16276 RVA: 0x0019911E File Offset: 0x0019731E
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
		}

		// Token: 0x06003F95 RID: 16277 RVA: 0x00199120 File Offset: 0x00197320
		public override void FlipX(bool relativeToSub)
		{
			this.handlePos[0].X = -this.handlePos[0].X;
			this.handlePos[1].X = -this.handlePos[1].X;
			if (this.item.body != null)
			{
				this.item.body.Dir = -this.item.body.Dir;
			}
		}

		// Token: 0x06003F96 RID: 16278 RVA: 0x001991A1 File Offset: 0x001973A1
		public override void OnItemLoaded()
		{
			if (this.item.Submarine != null && this.item.Submarine.Loading)
			{
				return;
			}
			this.OnMapLoaded();
			this.item.SetActiveSprite();
		}

		// Token: 0x06003F97 RID: 16279 RVA: 0x001991D4 File Offset: 0x001973D4
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

		// Token: 0x06003F98 RID: 16280 RVA: 0x0019926B File Offset: 0x0019746B
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

		// Token: 0x06003F99 RID: 16281 RVA: 0x0019929C File Offset: 0x0019749C
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

		// Token: 0x06003F9C RID: 16284 RVA: 0x00199324 File Offset: 0x00197524
		[CompilerGenerated]
		internal static void <Use>g__RefreshLightSources|154_0(Item item)
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
				Holdable.<Use>g__RefreshLightSources|154_0(containedItem);
			}
		}

		// Token: 0x06003F9D RID: 16285 RVA: 0x001993D0 File Offset: 0x001975D0
		[CompilerGenerated]
		internal static float <GetAttachPosition>g__RoundToGrid|156_0(float position, float gridSize, int roundingDir = 0)
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

		// Token: 0x04001E43 RID: 7747
		private const float MaxAttachDistance = 114f;

		// Token: 0x04001E44 RID: 7748
		protected Vector2[] handlePos;

		// Token: 0x04001E45 RID: 7749
		private readonly Vector2[] scaledHandlePos;

		// Token: 0x04001E46 RID: 7750
		private readonly InputType prevPickKey;

		// Token: 0x04001E47 RID: 7751
		private LocalizedString prevMsg;

		// Token: 0x04001E48 RID: 7752
		private Dictionary<RelatedItem.RelationType, List<RelatedItem>> prevRequiredItems;

		// Token: 0x04001E49 RID: 7753
		private float swingState;

		// Token: 0x04001E4A RID: 7754
		private Character prevEquipper;

		// Token: 0x04001E4B RID: 7755
		private bool attachable;

		// Token: 0x04001E4C RID: 7756
		private bool attached;

		// Token: 0x04001E4D RID: 7757
		private bool attachedByDefault;

		// Token: 0x04001E4E RID: 7758
		private VoronoiCell attachTargetCell;

		// Token: 0x04001E4F RID: 7759
		private PhysicsBody originalBody;

		// Token: 0x04001E50 RID: 7760
		public readonly ImmutableDictionary<StatTypes, float> HoldableStatValues;

		// Token: 0x04001E5B RID: 7771
		private HashSet<Identifier> disallowAttachingOverTags = new HashSet<Identifier>();

		// Token: 0x04001E5D RID: 7773
		protected Vector2 holdPos;

		// Token: 0x04001E5E RID: 7774
		protected Vector2 aimPos;

		// Token: 0x04001E5F RID: 7775
		protected float holdAngle;

		// Token: 0x04001E60 RID: 7776
		protected float aimAngle;

		// Token: 0x04001E61 RID: 7777
		private Vector2 swingAmount;

		// Token: 0x04001E6A RID: 7786
		private bool secondHandlePosDefined;

		// Token: 0x04001E6B RID: 7787
		private bool loadedFromInstance;

		// Token: 0x04001E6C RID: 7788
		private static List<Item> tempOverlappingItems = new List<Item>();

		// Token: 0x02000D82 RID: 3458
		private readonly struct AttachEventData : ItemComponent.IEventData
		{
			// Token: 0x06006757 RID: 26455 RVA: 0x0021FEF3 File Offset: 0x0021E0F3
			public AttachEventData(Vector2 attachPos, Character attacher)
			{
				this.AttachPos = attachPos;
				this.Attacher = attacher;
			}

			// Token: 0x04003FED RID: 16365
			public readonly Vector2 AttachPos;

			// Token: 0x04003FEE RID: 16366
			public readonly Character Attacher;
		}
	}
}

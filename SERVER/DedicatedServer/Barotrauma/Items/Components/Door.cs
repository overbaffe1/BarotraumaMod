using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000492 RID: 1170
	internal class Door : Pickable, IDrawableComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06003E7C RID: 15996 RVA: 0x00192E08 File Offset: 0x00191008
		public override void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Door.EventData eventData;
			bool forcedOpen = base.TryExtractEventData<Door.EventData>(extraData, out eventData) && eventData.ForcedOpen;
			base.ServerEventWrite(msg, c, extraData);
			msg.WriteBoolean(this.isOpen);
			msg.WriteBoolean(this.isBroken);
			msg.WriteBoolean(forcedOpen);
			msg.WriteBoolean(this.isStuck);
			msg.WriteBoolean(this.isJammed);
			msg.WriteRangedSingle(this.stuck, 0f, 100f, 8);
			msg.WriteUInt16((this.lastUser == null) ? 0 : this.lastUser.ID);
		}

		// Token: 0x17001076 RID: 4214
		// (get) Token: 0x06003E7D RID: 15997 RVA: 0x00192E9D File Offset: 0x0019109D
		public static IReadOnlyCollection<Door> DoorList
		{
			get
			{
				return Door.doorList;
			}
		}

		// Token: 0x17001077 RID: 4215
		// (get) Token: 0x06003E7E RID: 15998 RVA: 0x00192EA4 File Offset: 0x001910A4
		// (set) Token: 0x06003E7F RID: 15999 RVA: 0x00192EAC File Offset: 0x001910AC
		public bool IsJammed
		{
			get
			{
				return this.isJammed;
			}
			set
			{
				if (this.isJammed == value)
				{
					return;
				}
				this.isJammed = value;
				this.item.CreateServerEvent<Door>(this);
			}
		}

		// Token: 0x17001078 RID: 4216
		// (get) Token: 0x06003E80 RID: 16000 RVA: 0x00192ECB File Offset: 0x001910CB
		// (set) Token: 0x06003E81 RID: 16001 RVA: 0x00192ED3 File Offset: 0x001910D3
		[Serialize(false, IsPropertySaveable.Yes, "", "", true)]
		public bool IsStuck
		{
			get
			{
				return this.isStuck;
			}
			private set
			{
				if (this.isStuck == value)
				{
					return;
				}
				this.isStuck = value;
				if (this.item.FullyInitialized)
				{
					this.item.CreateServerEvent<Door>(this);
				}
			}
		}

		// Token: 0x17001079 RID: 4217
		// (get) Token: 0x06003E82 RID: 16002 RVA: 0x00192EFF File Offset: 0x001910FF
		// (set) Token: 0x06003E83 RID: 16003 RVA: 0x00192F07 File Offset: 0x00191107
		public bool IgnoreSignals { get; private set; }

		// Token: 0x1700107A RID: 4218
		// (get) Token: 0x06003E84 RID: 16004 RVA: 0x00192F10 File Offset: 0x00191110
		public bool CanBeTraversed
		{
			get
			{
				return !this.Impassable && (this.IsBroken || this.IsOpen);
			}
		}

		// Token: 0x1700107B RID: 4219
		// (get) Token: 0x06003E85 RID: 16005 RVA: 0x00192F2C File Offset: 0x0019112C
		// (set) Token: 0x06003E86 RID: 16006 RVA: 0x00192F34 File Offset: 0x00191134
		public bool IsBroken
		{
			get
			{
				return this.isBroken;
			}
			set
			{
				if (this.isBroken == value)
				{
					return;
				}
				this.isBroken = value;
				if (this.isBroken)
				{
					this.DisableBody();
				}
				else
				{
					this.EnableBody();
				}
				this.item.CreateServerEvent<Door>(this);
			}
		}

		// Token: 0x1700107C RID: 4220
		// (get) Token: 0x06003E87 RID: 16007 RVA: 0x00192F69 File Offset: 0x00191169
		// (set) Token: 0x06003E88 RID: 16008 RVA: 0x00192F71 File Offset: 0x00191171
		public PhysicsBody Body { get; private set; }

		// Token: 0x1700107D RID: 4221
		// (get) Token: 0x06003E89 RID: 16009 RVA: 0x00192F7A File Offset: 0x0019117A
		private float RepairThreshold
		{
			get
			{
				if (this.item.GetComponent<Repairable>() != null)
				{
					return this.item.MaxCondition;
				}
				return 0f;
			}
		}

		// Token: 0x1700107E RID: 4222
		// (get) Token: 0x06003E8A RID: 16010 RVA: 0x00192F9A File Offset: 0x0019119A
		// (set) Token: 0x06003E8B RID: 16011 RVA: 0x00192FA4 File Offset: 0x001911A4
		[Serialize(0f, IsPropertySaveable.Yes, "How badly stuck the door is (in percentages). If the percentage reaches 100, the door needs to be cut open to make it usable again.", "", false)]
		public float Stuck
		{
			get
			{
				return this.stuck;
			}
			set
			{
				if (this.isOpen || this.isBroken || !this.CanBeWelded)
				{
					return;
				}
				this.stuck = MathHelper.Clamp(value, 0f, 100f);
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				if (this.stuck <= 0f)
				{
					this.IsStuck = false;
				}
				if (this.stuck >= 99f)
				{
					this.IsStuck = true;
				}
			}
		}

		// Token: 0x1700107F RID: 4223
		// (get) Token: 0x06003E8C RID: 16012 RVA: 0x0019301C File Offset: 0x0019121C
		// (set) Token: 0x06003E8D RID: 16013 RVA: 0x00193024 File Offset: 0x00191224
		[Serialize(3f, IsPropertySaveable.Yes, "How quickly the door opens.", "", false)]
		[Editable]
		public float OpeningSpeed { get; private set; }

		// Token: 0x17001080 RID: 4224
		// (get) Token: 0x06003E8E RID: 16014 RVA: 0x0019302D File Offset: 0x0019122D
		// (set) Token: 0x06003E8F RID: 16015 RVA: 0x00193035 File Offset: 0x00191235
		[Serialize(3f, IsPropertySaveable.Yes, "How quickly the door closes.", "", false)]
		[Editable]
		public float ClosingSpeed { get; private set; }

		// Token: 0x17001081 RID: 4225
		// (get) Token: 0x06003E90 RID: 16016 RVA: 0x0019303E File Offset: 0x0019123E
		// (set) Token: 0x06003E91 RID: 16017 RVA: 0x00193046 File Offset: 0x00191246
		[Serialize(1f, IsPropertySaveable.Yes, "The door cannot be opened/closed during this time after it has been opened/closed by another character.", "", false)]
		[Editable]
		public float ToggleCoolDown { get; private set; }

		// Token: 0x17001082 RID: 4226
		// (get) Token: 0x06003E92 RID: 16018 RVA: 0x0019304F File Offset: 0x0019124F
		// (set) Token: 0x06003E93 RID: 16019 RVA: 0x00193057 File Offset: 0x00191257
		public bool? PredictedState { get; private set; }

		// Token: 0x17001083 RID: 4227
		// (get) Token: 0x06003E94 RID: 16020 RVA: 0x00193060 File Offset: 0x00191260
		public Gap LinkedGap
		{
			get
			{
				if (this.linkedGap == null)
				{
					this.GetLinkedGap();
				}
				return this.linkedGap;
			}
		}

		// Token: 0x06003E95 RID: 16021 RVA: 0x00193078 File Offset: 0x00191278
		private void GetLinkedGap()
		{
			this.linkedGap = (this.item.linkedTo.FirstOrDefault((MapEntity e) => e is Gap) as Gap);
			if (this.linkedGap == null)
			{
				Rectangle rect = this.item.Rect;
				this.linkedGap = new Gap(rect, !this.IsHorizontal, base.Item.Submarine, false, 0)
				{
					Submarine = this.item.Submarine
				};
				this.item.linkedTo.Add(this.linkedGap);
			}
			this.RefreshLinkedGap();
		}

		// Token: 0x17001084 RID: 4228
		// (get) Token: 0x06003E96 RID: 16022 RVA: 0x00193122 File Offset: 0x00191322
		// (set) Token: 0x06003E97 RID: 16023 RVA: 0x0019312A File Offset: 0x0019132A
		public bool IsHorizontal { get; private set; }

		// Token: 0x17001085 RID: 4229
		// (get) Token: 0x06003E98 RID: 16024 RVA: 0x00193133 File Offset: 0x00191333
		public bool IsConvexHullHorizontal
		{
			get
			{
				if (!this.autoOrientGap || this.linkedGap == null)
				{
					return this.IsHorizontal;
				}
				return !this.linkedGap.IsHorizontal;
			}
		}

		// Token: 0x17001086 RID: 4230
		// (get) Token: 0x06003E99 RID: 16025 RVA: 0x0019315A File Offset: 0x0019135A
		// (set) Token: 0x06003E9A RID: 16026 RVA: 0x00193162 File Offset: 0x00191362
		[Serialize("0.0,0.0,0.0,0.0", IsPropertySaveable.No, "Position and size of the window on the door. The upper left corner is 0,0. Set the width and height to 0 if you don't want the door to have a window.", "", false)]
		public Rectangle Window { get; set; }

		// Token: 0x17001087 RID: 4231
		// (get) Token: 0x06003E9B RID: 16027 RVA: 0x0019316B File Offset: 0x0019136B
		// (set) Token: 0x06003E9C RID: 16028 RVA: 0x00193173 File Offset: 0x00191373
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Is the door currently open.", "", false)]
		public bool IsOpen
		{
			get
			{
				return this.isOpen;
			}
			set
			{
				this.isOpen = value;
				this.OpenState = (this.isOpen ? 1f : 0f);
			}
		}

		// Token: 0x17001088 RID: 4232
		// (get) Token: 0x06003E9D RID: 16029 RVA: 0x00193196 File Offset: 0x00191396
		// (set) Token: 0x06003E9E RID: 16030 RVA: 0x0019319E File Offset: 0x0019139E
		public bool ShouldBeOpen
		{
			get
			{
				return this.isOpen;
			}
			set
			{
				if (this.isOpen != value)
				{
					this.ToggleState(ActionType.OnUse, null);
				}
			}
		}

		// Token: 0x17001089 RID: 4233
		// (get) Token: 0x06003E9F RID: 16031 RVA: 0x001931B1 File Offset: 0x001913B1
		public bool IsClosed
		{
			get
			{
				return !this.IsOpen;
			}
		}

		// Token: 0x1700108A RID: 4234
		// (get) Token: 0x06003EA0 RID: 16032 RVA: 0x001931BC File Offset: 0x001913BC
		public bool IsFullyOpen
		{
			get
			{
				return this.IsOpen && this.OpenState >= 1f;
			}
		}

		// Token: 0x1700108B RID: 4235
		// (get) Token: 0x06003EA1 RID: 16033 RVA: 0x001931D8 File Offset: 0x001913D8
		public bool IsFullyClosed
		{
			get
			{
				return this.IsClosed && this.OpenState <= 0f;
			}
		}

		// Token: 0x1700108C RID: 4236
		// (get) Token: 0x06003EA2 RID: 16034 RVA: 0x001931F4 File Offset: 0x001913F4
		public bool HasWindow
		{
			get
			{
				return this.Window != Rectangle.Empty;
			}
		}

		// Token: 0x1700108D RID: 4237
		// (get) Token: 0x06003EA3 RID: 16035 RVA: 0x00193206 File Offset: 0x00191406
		// (set) Token: 0x06003EA4 RID: 16036 RVA: 0x0019320E File Offset: 0x0019140E
		[Serialize(false, IsPropertySaveable.No, "If the door has integrated buttons, it can be opened by interacting with it directly (instead of using buttons wired to it).", "", false)]
		public bool HasIntegratedButtons { get; private set; }

		// Token: 0x1700108E RID: 4238
		// (get) Token: 0x06003EA5 RID: 16037 RVA: 0x00193217 File Offset: 0x00191417
		// (set) Token: 0x06003EA6 RID: 16038 RVA: 0x0019321F File Offset: 0x0019141F
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasIntegratedButtons, true)]
		[Serialize(true, IsPropertySaveable.No, "If the door has integrated buttons, should clicking on it perform the default action of opening the door? Can be used in conjunction with the \"activate_out\" output to pass a signal to a circuit without toggling the door when someone tries to open/close the door.", "", false)]
		public bool ToggleWhenClicked { get; private set; }

		// Token: 0x1700108F RID: 4239
		// (get) Token: 0x06003EA7 RID: 16039 RVA: 0x00193228 File Offset: 0x00191428
		// (set) Token: 0x06003EA8 RID: 16040 RVA: 0x00193230 File Offset: 0x00191430
		public float OpenState
		{
			get
			{
				return this.openState;
			}
			set
			{
				this.lastOpenState = this.openState;
				this.openState = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17001090 RID: 4240
		// (get) Token: 0x06003EA9 RID: 16041 RVA: 0x00193254 File Offset: 0x00191454
		// (set) Token: 0x06003EAA RID: 16042 RVA: 0x0019325C File Offset: 0x0019145C
		[Serialize(false, IsPropertySaveable.No, "Characters and items cannot pass through impassable doors. Useful for things such as ducts that should only let water and air through.", "", false)]
		public bool Impassable { get; set; }

		// Token: 0x17001091 RID: 4241
		// (get) Token: 0x06003EAB RID: 16043 RVA: 0x00193265 File Offset: 0x00191465
		// (set) Token: 0x06003EAC RID: 16044 RVA: 0x0019326D File Offset: 0x0019146D
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool UseBetweenOutpostModules { get; private set; }

		// Token: 0x17001092 RID: 4242
		// (get) Token: 0x06003EAD RID: 16045 RVA: 0x00193276 File Offset: 0x00191476
		// (set) Token: 0x06003EAE RID: 16046 RVA: 0x0019327E File Offset: 0x0019147E
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "If true, bots won't try to close this door behind them.", "", true)]
		public bool BotsShouldKeepOpen { get; private set; }

		// Token: 0x06003EAF RID: 16047 RVA: 0x00193288 File Offset: 0x00191488
		public Door(Item item, ContentXElement element) : base(item, element)
		{
			this.IsHorizontal = element.GetAttributeBool("horizontal", false);
			this.canBePicked = element.GetAttributeBool("canbepicked", false);
			this.autoOrientGap = element.GetAttributeBool("autoorientgap", false);
			this.allowedSlots.Clear();
			foreach (ContentXElement subElement in element.Elements())
			{
				string textureDir = base.GetTextureDirectory(subElement);
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "weldedsprite"))
					{
						if (a == "brokensprite")
						{
							this.brokenSprite = new Sprite(subElement, textureDir, "", false, 1f);
							this.scaleBrokenSprite = subElement.GetAttributeBool("scale", false);
							this.fadeBrokenSprite = subElement.GetAttributeBool("fade", false);
						}
					}
					else
					{
						this.weldedSprite = new Sprite(subElement, textureDir, "", false, 1f);
					}
				}
				else
				{
					this.doorSprite = new Sprite(subElement, textureDir, "", false, 1f);
				}
			}
			this.IsActive = true;
			Door.doorList.Add(this);
		}

		// Token: 0x06003EB0 RID: 16048 RVA: 0x00193414 File Offset: 0x00191614
		public override void OnItemLoaded()
		{
			this.doorRect = new Rectangle(this.item.Rect.Center.X - (int)(this.doorSprite.size.X / 2f * this.item.Scale), this.item.Rect.Y - this.item.Rect.Height / 2 + (int)(this.doorSprite.size.Y / 2f * this.item.Scale), (int)(this.doorSprite.size.X * this.item.Scale), (int)(this.doorSprite.size.Y * this.item.Scale));
			this.Body = new PhysicsBody(ConvertUnits.ToSimUnits(Math.Max(this.doorRect.Width, 1)), ConvertUnits.ToSimUnits(Math.Max(this.doorRect.Height, 1)), 0f, 1.5f, BodyType.Static, Category.Cat1, Category.Cat2 | Category.Cat5 | Category.Cat6 | Category.Cat7, false)
			{
				UserData = this.item,
				Friction = 0.5f
			};
			this.Body.SetTransformIgnoreContacts(ConvertUnits.ToSimUnits(new Vector2((float)this.doorRect.Center.X, (float)(this.doorRect.Y - this.doorRect.Height / 2))), 0f, true);
			if (this.isBroken)
			{
				this.DisableBody();
			}
		}

		// Token: 0x06003EB1 RID: 16049 RVA: 0x0019359C File Offset: 0x0019179C
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (ignoreContacts)
			{
				PhysicsBody body = this.Body;
				if (body == null)
				{
					return;
				}
				body.SetTransformIgnoreContacts(this.Body.SimPosition + ConvertUnits.ToSimUnits(amount), 0f, true);
				return;
			}
			else
			{
				PhysicsBody body2 = this.Body;
				if (body2 == null)
				{
					return;
				}
				body2.SetTransform(this.Body.SimPosition + ConvertUnits.ToSimUnits(amount), 0f, true);
				return;
			}
		}

		// Token: 0x06003EB2 RID: 16050 RVA: 0x00193608 File Offset: 0x00191808
		public override bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			if (this.IsBroken)
			{
				return false;
			}
			if (this.isOpen)
			{
				base.Msg = (this.HasAccess(character) ? "ItemMsgClose" : "ItemMsgForceCloseCrowbar");
			}
			else
			{
				base.Msg = (this.HasAccess(character) ? "ItemMsgOpen" : "ItemMsgForceOpenCrowbar");
			}
			this.ParseMsg();
			if (addMessage && msg == null)
			{
				msg = (this.HasIntegratedButtons ? this.accessDeniedTxt : this.cannotOpenText).Value;
			}
			return base.HasRequiredItems(character, addMessage, msg);
		}

		// Token: 0x06003EB3 RID: 16051 RVA: 0x00193698 File Offset: 0x00191898
		public override bool Pick(Character picker)
		{
			return (this.item.Condition < this.RepairThreshold && this.item.GetComponent<Repairable>().HasRequiredItems(picker, false, null)) || (!this.RequiredItems.None(null) && (!this.HasAccess(picker) || !this.HasRequiredItems(picker, false, null)) && base.Pick(picker));
		}

		// Token: 0x06003EB4 RID: 16052 RVA: 0x001936FC File Offset: 0x001918FC
		public override bool OnPicked(Character picker)
		{
			if (this.item.Condition < this.RepairThreshold && this.item.GetComponent<Repairable>().HasRequiredItems(picker, false, null))
			{
				return true;
			}
			if (!this.HasAccess(picker))
			{
				this.ToggleState(ActionType.OnPicked, picker);
				base.ApplyStatusEffects(ActionType.OnPicked, 1f, picker, null, null, null, null, 1f);
			}
			return false;
		}

		// Token: 0x06003EB5 RID: 16053 RVA: 0x00193764 File Offset: 0x00191964
		private void ToggleState(ActionType actionType, Character user)
		{
			if (this.toggleCooldownTimer > 0f && user != this.lastUser)
			{
				return;
			}
			if (this.ToggleWhenClicked)
			{
				this.toggleCooldownTimer = this.ToggleCoolDown;
			}
			if (this.IsStuck || this.IsJammed)
			{
				this.toggleCooldownTimer = 1f;
				return;
			}
			this.item.SendSignal("1", "activate_out");
			this.lastUser = user;
			if (this.ToggleWhenClicked)
			{
				this.SetState((this.PredictedState == null) ? (!this.isOpen) : (!this.PredictedState.Value), false, true, actionType == ActionType.OnPicked);
			}
		}

		// Token: 0x06003EB6 RID: 16054 RVA: 0x00193814 File Offset: 0x00191A14
		public override bool Select(Character character)
		{
			if (this.isBroken)
			{
				return true;
			}
			bool hasRequiredItems = this.HasRequiredItems(character, false, null);
			if (this.HasAccess(character))
			{
				float originalPickingTime = base.PickingTime;
				base.PickingTime = 0f;
				this.ToggleState(ActionType.OnUse, character);
				base.PickingTime = originalPickingTime;
				base.StopPicking(this.picker);
				return true;
			}
			return false;
		}

		// Token: 0x06003EB7 RID: 16055 RVA: 0x00193870 File Offset: 0x00191A70
		public bool IsPositionOnWindow(Vector2 position, float maxPerpendicularDistance = 10f)
		{
			if (this.IsHorizontal)
			{
				return position.X >= (float)(this.item.Rect.X + this.Window.X) && position.X <= (float)(this.item.Rect.X + this.Window.X + this.Window.Width) && position.Y >= (float)this.item.Rect.Y - maxPerpendicularDistance && position.Y <= (float)(this.item.Rect.Y - this.item.Rect.Height) - maxPerpendicularDistance;
			}
			return position.Y >= (float)(this.item.Rect.Y + this.Window.Y) && position.Y <= (float)(this.item.Rect.Y + this.Window.Y + this.Window.Height) && position.X >= (float)this.item.Rect.X - maxPerpendicularDistance && position.X <= (float)this.item.Rect.Right + maxPerpendicularDistance;
		}

		// Token: 0x06003EB8 RID: 16056 RVA: 0x001939C0 File Offset: 0x00191BC0
		public override void Update(float deltaTime, Camera cam)
		{
			this.toggleCooldownTimer -= deltaTime;
			this.damageSoundCooldown -= deltaTime;
			if (this.isBroken)
			{
				this.lastBrokenTime = Timing.TotalTime;
				if (this.item.ConditionPercentage * Math.Max(this.item.MaxRepairConditionMultiplier, 1f) > 50f && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer))
				{
					this.IsBroken = false;
				}
				return;
			}
			bool isClosing = false;
			if ((!this.IsStuck && !this.IsJammed) || !this.isOpen)
			{
				if (this.PredictedState == null)
				{
					this.OpenState += deltaTime * (this.isOpen ? this.OpeningSpeed : (-this.ClosingSpeed));
					float num = this.openState;
					isClosing = (num > 0f && num < 1f && !this.isOpen);
				}
				else
				{
					this.OpenState += deltaTime * (this.PredictedState.Value ? this.OpeningSpeed : (-this.ClosingSpeed));
					float num = this.openState;
					isClosing = (num > 0f && num < 1f && !this.PredictedState.Value);
					this.resetPredictionTimer -= deltaTime;
					if (this.resetPredictionTimer <= 0f)
					{
						this.PredictedState = null;
					}
				}
				this.LinkedGap.Open = (this.isBroken ? 1f : this.openState);
			}
			if (isClosing)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				float pushCharactersAwayThreshold = (networkMember != null && networkMember.IsServer) ? 0.1f : 0.9f;
				if (this.OpenState < pushCharactersAwayThreshold)
				{
					this.PushCharactersAway();
				}
				if (this.CheckSubmarinesInDoorWay())
				{
					this.PredictedState = null;
					this.isOpen = true;
				}
			}
			else
			{
				bool wasEnabled = this.Body.Enabled;
				this.Body.Enabled = (this.Impassable || this.openState < 1f);
				if (this.OutsideSubmarineFixture != null)
				{
					this.OutsideSubmarineFixture.CollidesWith = (this.Body.Enabled ? (Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8) : Category.None);
				}
				if (wasEnabled && !this.Body.Enabled && this.IsHorizontal)
				{
					foreach (Character c in Character.CharacterList)
					{
						if (c.WorldPosition.Y >= this.item.WorldPosition.Y && c.WorldPosition.X >= (float)this.item.WorldRect.X && c.WorldPosition.X <= (float)this.item.WorldRect.Right)
						{
							AnimController animController = c.AnimController;
							if (animController != null)
							{
								animController.ForceRefreshFloorY();
							}
						}
					}
				}
			}
			this.item.SendSignal(this.isOpen ? "1" : "0", "state_out");
		}

		// Token: 0x06003EB9 RID: 16057 RVA: 0x00193D0C File Offset: 0x00191F0C
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				this.IsBroken = true;
			}
		}

		// Token: 0x06003EBA RID: 16058 RVA: 0x00193D30 File Offset: 0x00191F30
		private void EnableBody()
		{
			if (!this.Impassable)
			{
				this.Body.FarseerBody.SetIsSensor(false);
				ContactEdge ce = this.Body.FarseerBody.ContactList;
				while (ce != null && ce.Contact != null)
				{
					ce.Contact.Enabled = false;
					ce = ce.Next;
				}
				this.PushCharactersAway();
			}
			if (this.OutsideSubmarineFixture != null && this.Body.Enabled)
			{
				this.OutsideSubmarineFixture.CollidesWith = (Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat7 | Category.Cat8);
			}
			this.isBroken = false;
		}

		// Token: 0x06003EBB RID: 16059 RVA: 0x00193DBC File Offset: 0x00191FBC
		private void DisableBody()
		{
			if (!this.Impassable)
			{
				this.Body.FarseerBody.SetIsSensor(true);
				ContactEdge ce = this.Body.FarseerBody.ContactList;
				while (ce != null && ce.Contact != null)
				{
					ce.Contact.Enabled = false;
					ce = ce.Next;
				}
			}
			if (this.OutsideSubmarineFixture != null)
			{
				this.OutsideSubmarineFixture.CollidesWith = Category.None;
			}
			if (this.linkedGap != null)
			{
				this.linkedGap.Open = 1f;
			}
			this.IsOpen = false;
		}

		// Token: 0x06003EBC RID: 16060 RVA: 0x00193E48 File Offset: 0x00192048
		public void RefreshLinkedGap()
		{
			this.LinkedGap.Layer = this.item.Layer;
			this.LinkedGap.ConnectedDoor = this;
			if (this.autoOrientGap)
			{
				this.LinkedGap.AutoOrient();
			}
			this.LinkedGap.Open = (this.isBroken ? 1f : this.openState);
			this.LinkedGap.PassAmbientLight = (this.Window != Rectangle.Empty);
		}

		// Token: 0x06003EBD RID: 16061 RVA: 0x00193EC5 File Offset: 0x001920C5
		public override void OnMapLoaded()
		{
			this.RefreshLinkedGap();
		}

		// Token: 0x06003EBE RID: 16062 RVA: 0x00193ECD File Offset: 0x001920CD
		public override void OnScaleChanged()
		{
			if (this.linkedGap != null)
			{
				this.RefreshLinkedGap();
				this.linkedGap.Rect = this.item.Rect;
			}
		}

		// Token: 0x06003EBF RID: 16063 RVA: 0x00193EF4 File Offset: 0x001920F4
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			if (this.Body != null)
			{
				this.Body.Remove();
				this.Body = null;
			}
			foreach (Gap gap in Gap.GapList)
			{
				if (gap.ConnectedDoor == this)
				{
					gap.ConnectedDoor = null;
				}
			}
			if (this.OutsideSubmarineFixture != null)
			{
				this.OutsideSubmarineFixture.Body.Remove(this.OutsideSubmarineFixture);
				this.OutsideSubmarineFixture = null;
			}
			if (!Submarine.Unloading)
			{
				Gap gap2 = this.linkedGap;
				if (gap2 != null)
				{
					gap2.Remove();
				}
			}
			Sprite sprite = this.doorSprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Sprite sprite2 = this.weldedSprite;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			Door.doorList.Remove(this);
		}

		// Token: 0x06003EC0 RID: 16064 RVA: 0x00193FDC File Offset: 0x001921DC
		private bool CheckSubmarinesInDoorWay()
		{
			if (this.linkedGap != null && this.linkedGap.IsRoomToRoom)
			{
				return false;
			}
			Rectangle doorRect = this.item.WorldRect;
			if (this.IsHorizontal)
			{
				doorRect.Width = (int)((float)this.item.Rect.Width * (1f - this.openState));
			}
			else
			{
				doorRect.Height = (int)((float)this.item.Rect.Height * (1f - this.openState));
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub != this.item.Submarine && !sub.DockedTo.Contains(this.item.Submarine))
				{
					Rectangle worldBorders = sub.Borders;
					worldBorders.Location += sub.WorldPosition.ToPoint();
					if (Submarine.RectsOverlap(worldBorders, doorRect, true))
					{
						foreach (Hull hull in sub.GetHulls(false))
						{
							if (Submarine.RectsOverlap(hull.WorldRect, doorRect, true))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06003EC1 RID: 16065 RVA: 0x00194158 File Offset: 0x00192358
		private void PushCharactersAway()
		{
			if (!MathUtils.IsValid(this.item.SimPosition))
			{
				if (!this.itemPosErrorShown)
				{
					DebugConsole.ThrowError("Failed to push a character out of a doorway - position of the door is not valid (" + this.item.SimPosition.ToString() + ")", null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("PushCharactersAway:DoorPosInvalid", GameAnalyticsManager.ErrorSeverity.Error, "Failed to push a character out of a doorway - position of the door is not valid (" + this.item.SimPosition.ToString() + ").");
					this.itemPosErrorShown = true;
				}
				return;
			}
			Vector2 simPos = ConvertUnits.ToSimUnits(new Vector2((float)this.item.Rect.X, (float)this.item.Rect.Y));
			Vector2 currSize = this.IsHorizontal ? new Vector2((float)this.item.Rect.Width * (1f - this.openState), this.doorSprite.size.Y * this.item.Scale) : new Vector2(this.doorSprite.size.X * this.item.Scale, (float)this.item.Rect.Height * (1f - this.openState));
			Vector2 simSize = ConvertUnits.ToSimUnits(currSize);
			foreach (Character c in Character.CharacterList)
			{
				if (c.Enabled)
				{
					Item selectedItem = c.SelectedItem;
					Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
					if (controller == null || !controller.IsAttachedUser(c))
					{
						if (!MathUtils.IsValid(c.SimPosition))
						{
							if (!this.characterPosErrorShown.Contains(c))
							{
								if (GameSettings.CurrentConfig.VerboseLogging)
								{
									DebugConsole.ThrowError(string.Concat(new string[]
									{
										"Failed to push a character out of a doorway - position of the character \"",
										c.Name,
										"\" is not valid (",
										c.SimPosition.ToString(),
										")"
									}), null, null, false, false);
								}
								GameAnalyticsManager.AddErrorEventOnce("PushCharactersAway:CharacterPosInvalid", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
								{
									"Failed to push a character out of a doorway - position of the character \"",
									c.SpeciesName.ToString(),
									"\" is not valid (",
									c.SimPosition.ToString(),
									"). Removed: ",
									c.Removed.ToString(),
									" Remoteplayer: ",
									c.IsRemotePlayer.ToString()
								}));
								this.characterPosErrorShown.Add(c);
							}
						}
						else
						{
							int dir = this.IsHorizontal ? Math.Sign(c.SimPosition.Y - this.item.SimPosition.Y) : Math.Sign(c.SimPosition.X - this.item.SimPosition.X);
							foreach (Limb limb in c.AnimController.Limbs)
							{
								if (!limb.IsSevered && this.PushBodyOutOfDoorway(c, limb.body, dir, simPos, simSize) && this.damageSoundCooldown <= 0f)
								{
									this.damageSoundCooldown = 0.5f;
								}
							}
							this.PushBodyOutOfDoorway(c, c.AnimController.Collider, dir, simPos, simSize);
						}
					}
				}
			}
		}

		// Token: 0x06003EC2 RID: 16066 RVA: 0x00194514 File Offset: 0x00192714
		private bool PushBodyOutOfDoorway(Character c, PhysicsBody body, int dir, Vector2 doorRectSimPos, Vector2 doorRectSimSize)
		{
			if (!MathUtils.IsValid(body.SimPosition))
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Failed to push a limb out of a doorway - position of the body (character \"",
					c.Name,
					"\") is not valid (",
					body.SimPosition.ToString(),
					")"
				}), null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("PushCharactersAway:LimbPosInvalid", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
				{
					"Failed to push a character out of a doorway - position of the character \"",
					c.SpeciesName.ToString(),
					"\" is not valid (",
					body.SimPosition.ToString(),
					"). Removed: ",
					c.Removed.ToString(),
					" Remoteplayer: ",
					c.IsRemotePlayer.ToString()
				}));
				return false;
			}
			float diff;
			if (this.IsHorizontal)
			{
				if (body.SimPosition.X < doorRectSimPos.X || body.SimPosition.X > doorRectSimPos.X + doorRectSimSize.X)
				{
					return false;
				}
				diff = body.SimPosition.Y - this.item.SimPosition.Y;
			}
			else
			{
				if (body.SimPosition.Y > doorRectSimPos.Y || body.SimPosition.Y < doorRectSimPos.Y - doorRectSimSize.Y)
				{
					return false;
				}
				diff = body.SimPosition.X - this.item.SimPosition.X;
			}
			if (Math.Sign(diff) != dir)
			{
				if (this.IsHorizontal)
				{
					body.SetTransformIgnoreContacts(new Vector2(body.SimPosition.X, this.item.SimPosition.Y + (float)dir * doorRectSimSize.Y * 2f), body.Rotation, true);
				}
				else
				{
					body.SetTransformIgnoreContacts(new Vector2(this.item.SimPosition.X + (float)dir * doorRectSimSize.X * 1.2f, body.SimPosition.Y), body.Rotation, true);
				}
			}
			if (this.IsHorizontal)
			{
				if (Math.Abs(body.SimPosition.Y - this.item.SimPosition.Y) > doorRectSimSize.Y * 0.5f)
				{
					return false;
				}
				body.ApplyLinearImpulse(new Vector2(this.isOpen ? 0f : 1f, (float)dir * 2f), 64f);
			}
			else
			{
				if (Math.Abs(body.SimPosition.X - this.item.SimPosition.X) > doorRectSimSize.X * 0.5f)
				{
					return false;
				}
				body.ApplyLinearImpulse(new Vector2((float)dir * 2f, this.isOpen ? 0f : -1f), 64f);
			}
			if (this.lastBrokenTime < Timing.TotalTime - 1.0)
			{
				c.SetStun(0.2f, false, false);
			}
			return true;
		}

		// Token: 0x06003EC3 RID: 16067 RVA: 0x0019482C File Offset: 0x00192A2C
		public override bool HasAccess(Character character)
		{
			if (!this.item.IsInteractable(character))
			{
				return false;
			}
			if (!base.HasAccess(character))
			{
				return false;
			}
			if (this.HasIntegratedButtons)
			{
				return true;
			}
			List<Controller> buttons = base.Item.GetConnectedComponents<Controller>(true, true, null);
			return buttons.None(null) || buttons.Any((Controller b) => b.HasAccess(character));
		}

		// Token: 0x06003EC4 RID: 16068 RVA: 0x001948A4 File Offset: 0x00192AA4
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.IsStuck || this.IsJammed || this.IgnoreSignals)
			{
				return;
			}
			bool wasOpen = (this.PredictedState == null) ? this.isOpen : this.PredictedState.Value;
			if (connection.Name == "toggle")
			{
				if (signal.value == "0")
				{
					return;
				}
				if (this.toggleCooldownTimer > 0f && signal.sender != this.lastUser)
				{
					return;
				}
				if (this.IsStuck)
				{
					this.toggleCooldownTimer = 1f;
					return;
				}
				this.toggleCooldownTimer = this.ToggleCoolDown;
				this.lastUser = signal.sender;
				this.SetState(!wasOpen, false, true, false);
			}
			else if (connection.Name == "set_state")
			{
				bool signalOpen = signal.value != "0";
				if (this.IsStuck && signalOpen != wasOpen)
				{
					this.toggleCooldownTimer = 1f;
					return;
				}
				this.SetState(signalOpen, false, true, false);
			}
			if (signal.sender != null && wasOpen != this.isOpen)
			{
				GameServer.Log(GameServer.CharacterLogName(signal.sender) + (this.isOpen ? " opened " : " closed ") + this.item.Name, ServerLog.MessageType.ItemInteraction);
			}
		}

		// Token: 0x06003EC5 RID: 16069 RVA: 0x001949F8 File Offset: 0x00192BF8
		public void TrySetState(bool open, bool isNetworkMessage, bool sendNetworkMessage = false)
		{
			this.SetState(open, isNetworkMessage, sendNetworkMessage, false);
		}

		// Token: 0x06003EC6 RID: 16070 RVA: 0x00194A04 File Offset: 0x00192C04
		private void SetState(bool open, bool isNetworkMessage, bool sendNetworkMessage, bool forcedOpen)
		{
			if (this.IsStuck || this.isOpen == open)
			{
				return;
			}
			this.isOpen = open;
			if (this.isOpen)
			{
				this.stuck = MathHelper.Clamp(this.stuck - 30f, 0f, 100f);
			}
			ActionType actionType = open ? ActionType.OnOpen : ActionType.OnClose;
			this.item.ApplyStatusEffects(actionType, 1f, null, null, null, false, null);
			if (sendNetworkMessage)
			{
				this.item.CreateServerEvent<Door>(this, new Door.EventData(forcedOpen));
			}
		}

		// Token: 0x04001DE1 RID: 7649
		private static readonly HashSet<Door> doorList = new HashSet<Door>();

		// Token: 0x04001DE2 RID: 7650
		private Gap linkedGap;

		// Token: 0x04001DE3 RID: 7651
		private bool isOpen;

		// Token: 0x04001DE4 RID: 7652
		private float openState;

		// Token: 0x04001DE5 RID: 7653
		private float lastOpenState;

		// Token: 0x04001DE6 RID: 7654
		private readonly Sprite doorSprite;

		// Token: 0x04001DE7 RID: 7655
		private readonly Sprite weldedSprite;

		// Token: 0x04001DE8 RID: 7656
		private readonly Sprite brokenSprite;

		// Token: 0x04001DE9 RID: 7657
		private readonly bool scaleBrokenSprite;

		// Token: 0x04001DEA RID: 7658
		private readonly bool fadeBrokenSprite;

		// Token: 0x04001DEB RID: 7659
		private readonly bool autoOrientGap;

		// Token: 0x04001DEC RID: 7660
		private bool isJammed;

		// Token: 0x04001DED RID: 7661
		private bool isStuck;

		// Token: 0x04001DEF RID: 7663
		private const float StuckReductionOnOpen = 30f;

		// Token: 0x04001DF0 RID: 7664
		private float resetPredictionTimer;

		// Token: 0x04001DF1 RID: 7665
		private float toggleCooldownTimer;

		// Token: 0x04001DF2 RID: 7666
		private Character lastUser;

		// Token: 0x04001DF3 RID: 7667
		private float damageSoundCooldown;

		// Token: 0x04001DF4 RID: 7668
		private double lastBrokenTime;

		// Token: 0x04001DF5 RID: 7669
		private Rectangle doorRect;

		// Token: 0x04001DF6 RID: 7670
		private bool isBroken;

		// Token: 0x04001DF8 RID: 7672
		public Fixture OutsideSubmarineFixture;

		// Token: 0x04001DF9 RID: 7673
		public bool CanBeWelded = true;

		// Token: 0x04001DFA RID: 7674
		private float stuck;

		// Token: 0x04001E06 RID: 7686
		private readonly LocalizedString accessDeniedTxt = TextManager.Get("AccessDenied");

		// Token: 0x04001E07 RID: 7687
		private readonly LocalizedString cannotOpenText = TextManager.Get("DoorMsgCannotOpen");

		// Token: 0x04001E08 RID: 7688
		private bool itemPosErrorShown;

		// Token: 0x04001E09 RID: 7689
		private readonly HashSet<Character> characterPosErrorShown = new HashSet<Character>();

		// Token: 0x02000D76 RID: 3446
		private readonly struct EventData : ItemComponent.IEventData
		{
			// Token: 0x06006735 RID: 26421 RVA: 0x0021FC35 File Offset: 0x0021DE35
			public EventData(bool forcedOpen)
			{
				this.ForcedOpen = forcedOpen;
			}

			// Token: 0x04003FD3 RID: 16339
			public readonly bool ForcedOpen;
		}
	}
}

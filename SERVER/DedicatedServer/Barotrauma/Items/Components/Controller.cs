using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200049A RID: 1178
	internal class Controller : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x0600405B RID: 16475 RVA: 0x0019BEBD File Offset: 0x0019A0BD
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.State);
			msg.WriteUInt16((this.User == null || this.User.Removed) ? 0 : this.User.ID);
		}

		// Token: 0x17001112 RID: 4370
		// (get) Token: 0x0600405C RID: 16476 RVA: 0x0019BEF4 File Offset: 0x0019A0F4
		public Direction Direction
		{
			get
			{
				return this.dir;
			}
		}

		// Token: 0x17001113 RID: 4371
		// (get) Token: 0x0600405D RID: 16477 RVA: 0x0019BEFC File Offset: 0x0019A0FC
		// (set) Token: 0x0600405E RID: 16478 RVA: 0x0019BF04 File Offset: 0x0019A104
		public Character User
		{
			get
			{
				return this.user;
			}
			private set
			{
				if (this.user == value)
				{
					return;
				}
				this.user = value;
				if (this.user != null)
				{
					this.teleportTransition = 0f;
					this.teleportStartPosition = this.user.WorldPosition;
				}
				this.item.CreateServerEvent<Controller>(this);
			}
		}

		// Token: 0x17001114 RID: 4372
		// (get) Token: 0x0600405F RID: 16479 RVA: 0x0019BF52 File Offset: 0x0019A152
		// (set) Token: 0x06004060 RID: 16480 RVA: 0x0019BF5A File Offset: 0x0019A15A
		public Vector2 UserPos
		{
			get
			{
				return this.userPos;
			}
			set
			{
				this.userPos = value;
			}
		}

		// Token: 0x17001115 RID: 4373
		// (get) Token: 0x06004061 RID: 16481 RVA: 0x0019BF63 File Offset: 0x0019A163
		public IEnumerable<LimbPos> LimbPositions
		{
			get
			{
				return this.limbPositions;
			}
		}

		// Token: 0x17001116 RID: 4374
		// (get) Token: 0x06004062 RID: 16482 RVA: 0x0019BF6B File Offset: 0x0019A16B
		// (set) Token: 0x06004063 RID: 16483 RVA: 0x0019BF73 File Offset: 0x0019A173
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "When enabled, the item will continuously send out a signal and interacting with it will flip the signal (making the item behave like a switch). When disabled, the item will simply send out a signal when interacted with.", "", true)]
		public bool IsToggle { get; set; }

		// Token: 0x17001117 RID: 4375
		// (get) Token: 0x06004064 RID: 16484 RVA: 0x0019BF7C File Offset: 0x0019A17C
		// (set) Token: 0x06004065 RID: 16485 RVA: 0x0019BF84 File Offset: 0x0019A184
		[ConditionallyEditable(ConditionallyEditable.ConditionType.HasConnectionPanel, false)]
		[Serialize("1", IsPropertySaveable.Yes, "The signal sent when the controller is being activated or is toggled on. If empty, no signal is sent.", "", true)]
		public string Output
		{
			get
			{
				return this.output;
			}
			set
			{
				if (value == null || value == this.output)
				{
					return;
				}
				this.output = value;
				if (!value.IsNullOrEmpty())
				{
					this.IsActive = true;
				}
			}
		}

		// Token: 0x17001118 RID: 4376
		// (get) Token: 0x06004066 RID: 16486 RVA: 0x0019BFAE File Offset: 0x0019A1AE
		// (set) Token: 0x06004067 RID: 16487 RVA: 0x0019BFB6 File Offset: 0x0019A1B6
		[ConditionallyEditable(ConditionallyEditable.ConditionType.IsToggleableController, false)]
		[Serialize("0", IsPropertySaveable.Yes, "The signal sent when the controller is toggled off. If empty, no signal is sent. Only valid if IsToggle is true.", "", true)]
		public string FalseOutput
		{
			get
			{
				return this.falseOutput;
			}
			set
			{
				if (value == null || value == this.falseOutput)
				{
					return;
				}
				this.falseOutput = value;
				if (!value.IsNullOrEmpty())
				{
					this.IsActive = true;
				}
			}
		}

		// Token: 0x17001119 RID: 4377
		// (get) Token: 0x06004068 RID: 16488 RVA: 0x0019BFE0 File Offset: 0x0019A1E0
		// (set) Token: 0x06004069 RID: 16489 RVA: 0x0019BFE8 File Offset: 0x0019A1E8
		[ConditionallyEditable(ConditionallyEditable.ConditionType.IsToggleableController, true)]
		[Serialize(false, IsPropertySaveable.No, "Whether the item is toggled on/off. Only valid if IsToggle is set to true.", "", true)]
		public bool State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state != value)
				{
					this.state = value;
					string newOutput = this.state ? this.output : this.falseOutput;
					this.IsActive = !string.IsNullOrEmpty(newOutput);
				}
			}
		}

		// Token: 0x1700111A RID: 4378
		// (get) Token: 0x0600406A RID: 16490 RVA: 0x0019C02B File Offset: 0x0019A22B
		// (set) Token: 0x0600406B RID: 16491 RVA: 0x0019C033 File Offset: 0x0019A233
		[Serialize(true, IsPropertySaveable.No, "Should the HUD (inventory, health bar, etc) be hidden when this item is selected.", "", false)]
		public bool HideHUD { get; set; }

		// Token: 0x1700111B RID: 4379
		// (get) Token: 0x0600406C RID: 16492 RVA: 0x0019C03C File Offset: 0x0019A23C
		// (set) Token: 0x0600406D RID: 16493 RVA: 0x0019C044 File Offset: 0x0019A244
		[Serialize(false, IsPropertySaveable.No, "Should the HUDs of all item components in this item be hidden when a character is using this controller.", "", false)]
		public bool HideAllItemComponentHUDs { get; set; }

		// Token: 0x1700111C RID: 4380
		// (get) Token: 0x0600406E RID: 16494 RVA: 0x0019C04D File Offset: 0x0019A24D
		// (set) Token: 0x0600406F RID: 16495 RVA: 0x0019C055 File Offset: 0x0019A255
		[Serialize(Controller.UseEnvironment.Both, IsPropertySaveable.No, "Can the item be selected in air, underwater or both.", "", false)]
		public Controller.UseEnvironment UsableIn { get; set; }

		// Token: 0x1700111D RID: 4381
		// (get) Token: 0x06004070 RID: 16496 RVA: 0x0019C05E File Offset: 0x0019A25E
		// (set) Token: 0x06004071 RID: 16497 RVA: 0x0019C066 File Offset: 0x0019A266
		[Serialize(false, IsPropertySaveable.No, "Should the character using the item be drawn behind the item.", "", false)]
		public bool DrawUserBehind { get; set; }

		// Token: 0x1700111E RID: 4382
		// (get) Token: 0x06004072 RID: 16498 RVA: 0x0019C06F File Offset: 0x0019A26F
		// (set) Token: 0x06004073 RID: 16499 RVA: 0x0019C077 File Offset: 0x0019A277
		[Serialize(true, IsPropertySaveable.No, "Can another character select this controller when another character has already selected it?", "", false)]
		public bool AllowSelectingWhenSelectedByOther { get; set; }

		// Token: 0x1700111F RID: 4383
		// (get) Token: 0x06004074 RID: 16500 RVA: 0x0019C080 File Offset: 0x0019A280
		// (set) Token: 0x06004075 RID: 16501 RVA: 0x0019C088 File Offset: 0x0019A288
		[Serialize(true, IsPropertySaveable.No, "Can another character select this controller when a bot has already selected it?", "", false)]
		public bool AllowSelectingWhenSelectedByBot { get; set; }

		// Token: 0x17001120 RID: 4384
		// (get) Token: 0x06004076 RID: 16502 RVA: 0x0019C091 File Offset: 0x0019A291
		// (set) Token: 0x06004077 RID: 16503 RVA: 0x0019C099 File Offset: 0x0019A299
		[Serialize(false, IsPropertySaveable.No, "Can a character put another character into this controller by dragging them and selecting this controller?", "", false)]
		public bool AllowPuttingInOtherCharacters { get; set; }

		// Token: 0x17001121 RID: 4385
		// (get) Token: 0x06004078 RID: 16504 RVA: 0x0019C0A2 File Offset: 0x0019A2A2
		// (set) Token: 0x06004079 RID: 16505 RVA: 0x0019C0AA File Offset: 0x0019A2AA
		[Serialize(true, IsPropertySaveable.No, "Can a character select this controller by themselves?", "", false)]
		public bool CanBeSelectedByCharacters { get; set; }

		// Token: 0x17001122 RID: 4386
		// (get) Token: 0x0600407A RID: 16506 RVA: 0x0019C0B3 File Offset: 0x0019A2B3
		// (set) Token: 0x0600407B RID: 16507 RVA: 0x0019C0BB File Offset: 0x0019A2BB
		[Serialize(false, IsPropertySaveable.No, "If a character selects this controller, but another character already has it selected, should it be kicked out?", "", false)]
		public bool SelectingKicksCharacterOut { get; set; }

		// Token: 0x17001123 RID: 4387
		// (get) Token: 0x0600407C RID: 16508 RVA: 0x0019C0C4 File Offset: 0x0019A2C4
		// (set) Token: 0x0600407D RID: 16509 RVA: 0x0019C0CC File Offset: 0x0019A2CC
		[Serialize("", IsPropertySaveable.No, "Message displayed when there's a character inside this controller.", "", false)]
		public string KickOutCharacterMsg { get; set; }

		// Token: 0x17001124 RID: 4388
		// (get) Token: 0x0600407E RID: 16510 RVA: 0x0019C0D5 File Offset: 0x0019A2D5
		// (set) Token: 0x0600407F RID: 16511 RVA: 0x0019C0DD File Offset: 0x0019A2DD
		[Serialize("", IsPropertySaveable.No, "Message displayed when you are putting a character into the controller.", "", false)]
		public string PutOtherCharacterMsg { get; set; }

		// Token: 0x17001125 RID: 4389
		// (get) Token: 0x06004080 RID: 16512 RVA: 0x0019C0E6 File Offset: 0x0019A2E6
		// (set) Token: 0x06004081 RID: 16513 RVA: 0x0019C0EE File Offset: 0x0019A2EE
		[Serialize("", IsPropertySaveable.No, "Spawns this item in the first available item container slot when a character selects this controller, if the item container is full, the character will not be able to select the controller.", "", false)]
		public Identifier SpawnItemOnSelected { get; private set; }

		// Token: 0x17001126 RID: 4390
		// (get) Token: 0x06004082 RID: 16514 RVA: 0x0019C0F7 File Offset: 0x0019A2F7
		public bool ControlCharacterPose
		{
			get
			{
				return this.limbPositions.Count > 0;
			}
		}

		// Token: 0x17001127 RID: 4391
		// (get) Token: 0x06004083 RID: 16515 RVA: 0x0019C107 File Offset: 0x0019A307
		// (set) Token: 0x06004084 RID: 16516 RVA: 0x0019C10F File Offset: 0x0019A30F
		public bool UserInCorrectPosition { get; private set; }

		// Token: 0x17001128 RID: 4392
		// (get) Token: 0x06004085 RID: 16517 RVA: 0x0019C118 File Offset: 0x0019A318
		// (set) Token: 0x06004086 RID: 16518 RVA: 0x0019C120 File Offset: 0x0019A320
		public bool AllowAiming { get; private set; } = true;

		// Token: 0x17001129 RID: 4393
		// (get) Token: 0x06004087 RID: 16519 RVA: 0x0019C129 File Offset: 0x0019A329
		// (set) Token: 0x06004088 RID: 16520 RVA: 0x0019C131 File Offset: 0x0019A331
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool NonInteractableWhenFlippedX { get; set; }

		// Token: 0x1700112A RID: 4394
		// (get) Token: 0x06004089 RID: 16521 RVA: 0x0019C13A File Offset: 0x0019A33A
		// (set) Token: 0x0600408A RID: 16522 RVA: 0x0019C142 File Offset: 0x0019A342
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool NonInteractableWhenFlippedY { get; set; }

		// Token: 0x1700112B RID: 4395
		// (get) Token: 0x0600408B RID: 16523 RVA: 0x0019C14B File Offset: 0x0019A34B
		// (set) Token: 0x0600408C RID: 16524 RVA: 0x0019C153 File Offset: 0x0019A353
		[Serialize(false, IsPropertySaveable.No, "Does the Controller require power to function (= to send signals and move the camera focus to a connected item)?", "", false)]
		public bool RequirePower { get; set; }

		// Token: 0x1700112C RID: 4396
		// (get) Token: 0x0600408D RID: 16525 RVA: 0x0019C15C File Offset: 0x0019A35C
		// (set) Token: 0x0600408E RID: 16526 RVA: 0x0019C164 File Offset: 0x0019A364
		[Serialize(false, IsPropertySaveable.No, "If true, other items can be used simultaneously.", "", false)]
		public bool IsSecondaryItem { get; private set; }

		// Token: 0x1700112D RID: 4397
		// (get) Token: 0x0600408F RID: 16527 RVA: 0x0019C16D File Offset: 0x0019A36D
		// (set) Token: 0x06004090 RID: 16528 RVA: 0x0019C175 File Offset: 0x0019A375
		[Serialize(false, IsPropertySaveable.No, "If enabled, the user sticks to the position of this item even if the item moves.", "", false)]
		public bool ForceUserToStayAttached { get; set; }

		// Token: 0x06004091 RID: 16529 RVA: 0x0019C180 File Offset: 0x0019A380
		public Controller(Item item, ContentXElement element) : base(item, element)
		{
			string key = "UserPos";
			Vector2 zero = Vector2.Zero;
			this.userPos = element.GetAttributeVector2(key, zero);
			Enum.TryParse<Direction>(element.GetAttributeString("direction", "None"), out this.dir);
			this.LoadLimbPositions(element);
			this.IsActive = true;
			this.containerToSpawnOnSelectedItem = item.GetComponent<ItemContainer>();
			if (!this.SpawnItemOnSelected.IsEmpty && !ItemPrefab.Prefabs.TryGet(this.SpawnItemOnSelected, out this.spawnItemOnSelectedPrefab))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to find item prefab \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpawnItemOnSelected);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			if (this.containerToSpawnOnSelectedItem == null && !this.SpawnItemOnSelected.IsEmpty)
			{
				DebugConsole.ThrowError("Error - Controller has a SpawnItemOnSelected but no ItemContainer defined", null, null, false, false);
			}
		}

		// Token: 0x06004092 RID: 16530 RVA: 0x0019C284 File Offset: 0x0019A484
		public override void Update(float deltaTime, Camera cam)
		{
			this.cam = cam;
			if (!this.ForceUserToStayAttached)
			{
				this.UserInCorrectPosition = false;
			}
			string signal = (this.IsToggle && this.State) ? this.output : this.falseOutput;
			if (this.item.Connections != null && this.IsToggle && !string.IsNullOrEmpty(signal) && !this.IsOutOfPower())
			{
				this.item.SendSignal(signal, "signal_out");
				this.item.SendSignal(signal, "trigger_out");
			}
			if (this.forceSelectNextFrame && this.User != null)
			{
				this.User.SelectedItem = this.item;
			}
			this.forceSelectNextFrame = false;
			this.userCanInteractCheckTimer -= deltaTime;
			if (this.User == null || this.User.Removed || (((this.User.Stun <= 0f && !this.User.IsKnockedDownOrRagdolled && !this.User.LockHands) || !this.ForceUserToStayAttached) && (!this.User.IsAnySelectedItem(this.item) || !this.CheckUserCanInteract())) || (this.item.ParentInventory != null && !this.IsAttachedUser(this.User)) || (this.UsableIn == Controller.UseEnvironment.Water && !this.User.AnimController.InWater) || (this.UsableIn == Controller.UseEnvironment.Air && this.User.AnimController.InWater) || !this.CheckSpawnItem())
			{
				if (this.User != null)
				{
					this.CancelUsing(this.User);
					this.User = null;
				}
				if (this.item.Connections == null || !this.IsToggle || string.IsNullOrEmpty(signal))
				{
					this.IsActive = false;
				}
				return;
			}
			if (this.ForceUserToStayAttached)
			{
				this.teleportTransition = MathF.Min(this.teleportTransition + deltaTime * 8f, 1f);
				if (this.teleportTransition >= 1f && this.User.SelectedBy != null)
				{
					this.User.SelectedBy.SelectedCharacter = null;
				}
				if (this.User == Character.Controlled || this.teleportTransition < 1f || Vector2.DistanceSquared(this.item.WorldPosition, this.User.WorldPosition) > 0.1f)
				{
					Vector2 targetPosition = Vector2.Lerp(this.teleportStartPosition, this.item.WorldPosition, this.teleportTransition);
					this.User.TeleportTo(targetPosition);
					this.User.AnimController.Collider.ResetDynamics();
					foreach (Limb limb in this.User.AnimController.Limbs)
					{
						if (!limb.Removed && !limb.IsSevered)
						{
							PhysicsBody body = limb.body;
							if (body != null)
							{
								body.ResetDynamics();
							}
						}
					}
				}
			}
			this.User.AnimController.StartUsingItem();
			if (this.userPos != Vector2.Zero)
			{
				Vector2 diff = this.item.WorldPosition + this.userPos - this.User.WorldPosition;
				if (this.User.AnimController.InWater)
				{
					if (diff.LengthSquared() > 900f)
					{
						this.User.AnimController.TargetMovement = Vector2.Clamp(diff * 0.01f, -Vector2.One, Vector2.One);
						this.User.AnimController.TargetDir = ((diff.X > 0f) ? Direction.Right : Direction.Left);
					}
					else
					{
						this.User.AnimController.TargetMovement = Vector2.Zero;
						this.UserInCorrectPosition = true;
					}
				}
				else
				{
					if (!this.User.HasSelectedAnotherSecondaryItem(base.Item))
					{
						diff.Y = 0f;
						if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient && this.User != Character.Controlled)
						{
							if (Math.Abs(diff.X) > 20f)
							{
								return;
							}
							if (Math.Abs(diff.X) > 0.1f)
							{
								this.User.AnimController.Collider.LinearVelocity = new Vector2(diff.X * 0.1f, this.User.AnimController.Collider.LinearVelocity.Y);
							}
						}
						else if (Math.Abs(diff.X) > 10f)
						{
							this.User.AnimController.TargetMovement = Vector2.Normalize(diff);
							this.User.AnimController.TargetDir = ((diff.X > 0f) ? Direction.Right : Direction.Left);
							return;
						}
						this.User.AnimController.TargetMovement = Vector2.Zero;
					}
					this.UserInCorrectPosition = true;
				}
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, this.User, null, null, null, null, 1f);
			if (this.limbPositions.Count == 0)
			{
				return;
			}
			this.User.AnimController.StartUsingItem();
			if (this.User.SelectedItem != null)
			{
				this.User.AnimController.ResetPullJoints((Limb l) => l.IsLowerBody);
			}
			else
			{
				this.User.AnimController.ResetPullJoints(null);
			}
			if (this.dir != Direction.None)
			{
				this.User.AnimController.TargetDir = this.dir;
			}
			foreach (LimbPos lb in this.limbPositions)
			{
				Limb limb2 = this.User.AnimController.GetLimb(lb.LimbType, true, false, false);
				if (limb2 != null && limb2.body.Enabled && (!limb2.IsLowerBody || !this.User.HasSelectedAnotherSecondaryItem(base.Item)) && (!limb2.IsArm || base.Item != this.User.SelectedSecondaryItem || this.User.SelectedItem == null))
				{
					if (lb.AllowUsingLimb)
					{
						switch (lb.LimbType)
						{
						case LimbType.LeftHand:
						case LimbType.LeftArm:
						case LimbType.LeftForearm:
							if (this.User.Inventory.GetItemInLimbSlot(InvSlotType.LeftHand) != null)
							{
								continue;
							}
							break;
						case LimbType.RightHand:
						case LimbType.RightArm:
						case LimbType.RightForearm:
							if (this.User.Inventory.GetItemInLimbSlot(InvSlotType.RightHand) != null)
							{
								continue;
							}
							break;
						}
					}
					limb2.Disabled = true;
					Vector2 worldPosition = new Vector2((float)this.item.WorldRect.X, (float)this.item.WorldRect.Y) + lb.Position * this.item.Scale;
					Vector2 diff2 = worldPosition - limb2.WorldPosition;
					limb2.PullJointEnabled = true;
					limb2.PullJointWorldAnchorB = limb2.SimPosition + ConvertUnits.ToSimUnits(diff2);
				}
			}
		}

		// Token: 0x06004093 RID: 16531 RVA: 0x0019C9C0 File Offset: 0x0019ABC0
		private bool IsSpawnContainerFull()
		{
			return this.spawnItemOnSelectedPrefab != null && this.containerToSpawnOnSelectedItem != null && this.containerToSpawnOnSelectedItem.Inventory.IsFull(false);
		}

		// Token: 0x06004094 RID: 16532 RVA: 0x0019C9EC File Offset: 0x0019ABEC
		private bool CheckSpawnItem()
		{
			if (this.spawnItemOnSelectedPrefab == null || this.containerToSpawnOnSelectedItem == null)
			{
				return true;
			}
			if (this.containerToSpawnOnSelectedItem.Inventory.AllItems.Any((Item item) => item == this.spawnedItemOnSelected))
			{
				return true;
			}
			if (this.spawnedItemOnSelected != null && !this.spawnedItemOnSelected.Removed)
			{
				return this.spawnedItemOnSelected.ParentInventory == this.containerToSpawnOnSelectedItem.Inventory;
			}
			if (this.IsSpawnContainerFull())
			{
				return false;
			}
			if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
			{
				Entity.Spawner.AddItemToSpawnQueue(this.spawnItemOnSelectedPrefab, this.containerToSpawnOnSelectedItem.Inventory, null, null, delegate(Item spawnedItem)
				{
					this.spawnedItemOnSelected = spawnedItem;
					LinkedControllerCharacterComponent linkedCharacterComponent = spawnedItem.GetComponent<LinkedControllerCharacterComponent>();
					if (linkedCharacterComponent != null)
					{
						linkedCharacterComponent.UpdateLinkedCharacter(this.User);
					}
				}, true, false, InvSlotType.None);
			}
			return true;
		}

		// Token: 0x06004095 RID: 16533 RVA: 0x0019CABB File Offset: 0x0019ACBB
		private bool CheckUserCanInteract()
		{
			if (this.User != null && this.userCanInteractCheckTimer <= 0f)
			{
				this.userCanInteractCheckTimer = 1f;
				return this.User.CanInteractWith(this.item, true);
			}
			return true;
		}

		// Token: 0x06004096 RID: 16534 RVA: 0x0019CAF4 File Offset: 0x0019ACF4
		public override bool Use(float deltaTime, Character activator = null)
		{
			if (activator != this.User)
			{
				return false;
			}
			if (this.User == null || this.User.Removed || !this.User.IsAnySelectedItem(this.item) || !this.User.CanInteractWith(this.item, true))
			{
				this.User = null;
				return false;
			}
			if (this.IsOutOfPower())
			{
				return false;
			}
			base.ApplyStatusEffects(ActionType.OnUse, 1f, activator, null, null, null, null, 1f);
			if (this.IsToggle && (activator == null || this.lastUsed < Timing.TotalTime - 0.1))
			{
				if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
				{
					this.State = !this.State;
					this.item.CreateServerEvent<Controller>(this);
				}
			}
			else if (!string.IsNullOrEmpty(this.output))
			{
				this.item.SendSignal(new Signal(this.output, 0, this.User, null, 0f, 1f), "trigger_out");
			}
			this.lastUsed = Timing.TotalTime;
			return true;
		}

		// Token: 0x06004097 RID: 16535 RVA: 0x0019CC14 File Offset: 0x0019AE14
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			if (this.User != character)
			{
				return false;
			}
			if (this.User == null || character.Removed || !this.User.IsAnySelectedItem(this.item) || !character.CanInteractWith(this.item, true))
			{
				this.User = null;
				return false;
			}
			if (character == null)
			{
				return false;
			}
			if (this.IsOutOfPower())
			{
				return false;
			}
			this.focusTarget = this.GetFocusTarget();
			if (this.focusTarget == null)
			{
				Vector2 centerPos = new Vector2((float)this.item.WorldRect.Center.X, (float)this.item.WorldRect.Center.Y);
				Vector2 offset = character.CursorWorldPosition - centerPos;
				offset.Y = -offset.Y;
				this.targetRotation = MathUtils.WrapAngleTwoPi(MathUtils.VectorToAngle(offset));
				return false;
			}
			character.ViewTarget = this.focusTarget;
			if (!character.IsRemotePlayer || character.ViewTarget == this.focusTarget)
			{
				Vector2 centerPos2 = new Vector2((float)this.focusTarget.WorldRect.Center.X, (float)this.focusTarget.WorldRect.Center.Y);
				Turret turret = this.focusTarget.GetComponent<Turret>();
				if (turret != null)
				{
					centerPos2 = new Vector2((float)this.focusTarget.WorldRect.X + turret.TransformedBarrelPos.X, (float)this.focusTarget.WorldRect.Y - turret.TransformedBarrelPos.Y);
				}
				Vector2 offset2 = character.CursorWorldPosition - centerPos2;
				offset2.Y = -offset2.Y;
				this.targetRotation = MathUtils.WrapAngleTwoPi(MathUtils.VectorToAngle(offset2));
			}
			return true;
		}

		// Token: 0x06004098 RID: 16536 RVA: 0x0019CDD4 File Offset: 0x0019AFD4
		public bool IsOutOfPower()
		{
			if (!this.RequirePower)
			{
				return false;
			}
			Powered powered = this.item.GetComponent<Powered>();
			return powered == null || powered.Voltage < powered.MinVoltage;
		}

		// Token: 0x06004099 RID: 16537 RVA: 0x0019CE0C File Offset: 0x0019B00C
		public Item GetFocusTarget()
		{
			List<Connection> connections = this.item.Connections;
			Connection connection;
			if (connections == null)
			{
				connection = null;
			}
			else
			{
				connection = connections.Find((Connection c) => c.Name == "position_out");
			}
			Connection positionOut = connection;
			if (positionOut == null)
			{
				return null;
			}
			if (this.IsOutOfPower())
			{
				return null;
			}
			this.item.SendSignal(new Signal(MathHelper.ToDegrees(this.targetRotation).ToString("G", CultureInfo.InvariantCulture), 0, this.User, null, 0f, 1f), positionOut);
			for (int i = this.item.LastSentSignalRecipients.Count - 1; i >= 0; i--)
			{
				if (this.item.LastSentSignalRecipients[i].Item.Condition > 0f && !this.item.LastSentSignalRecipients[i].IsPower && this.item.LastSentSignalRecipients[i].Item.Prefab.FocusOnSelected)
				{
					return this.item.LastSentSignalRecipients[i].Item;
				}
			}
			foreach (ConnectionPanel recipientPanel in this.item.GetConnectedComponentsRecursive<ConnectionPanel>(positionOut, false, false))
			{
				if (recipientPanel.Item.Condition > 0f && recipientPanel.Item.Prefab.FocusOnSelected)
				{
					return recipientPanel.Item;
				}
			}
			return null;
		}

		// Token: 0x0600409A RID: 16538 RVA: 0x0019CFAC File Offset: 0x0019B1AC
		public override bool Pick(Character picker)
		{
			if (this.IsOutOfPower())
			{
				return false;
			}
			if (this.IsToggle)
			{
				if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
				{
					this.State = !this.State;
					this.item.CreateServerEvent<Controller>(this);
				}
			}
			else if (!string.IsNullOrEmpty(this.output))
			{
				this.item.SendSignal(new Signal(this.output, 0, picker, null, 0f, 1f), "signal_out");
			}
			base.ApplyStatusEffects(ActionType.OnUse, 1f, picker, null, null, null, null, 1f);
			return true;
		}

		// Token: 0x0600409B RID: 16539 RVA: 0x0019D050 File Offset: 0x0019B250
		private void CancelUsing(Character character)
		{
			if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && this.spawnedItemOnSelected != null)
			{
				Entity.Spawner.AddEntityToRemoveQueue(this.spawnedItemOnSelected);
				this.spawnedItemOnSelected = null;
			}
			if (character == null || character.Removed)
			{
				return;
			}
			character.AnimController.BodyInRest = false;
			foreach (LimbPos lb in this.limbPositions)
			{
				Limb limb = character.AnimController.GetLimb(lb.LimbType, true, false, false);
				if (limb != null)
				{
					limb.Disabled = false;
					limb.PullJointEnabled = false;
				}
			}
			HumanoidAnimController humanoidAnim = character.AnimController as HumanoidAnimController;
			if (humanoidAnim != null)
			{
				humanoidAnim.LockFlipping(0.5f);
			}
			if (character.SelectedItem == this.item)
			{
				character.SelectedItem = null;
			}
			if (character.SelectedSecondaryItem == this.item)
			{
				character.SelectedSecondaryItem = null;
			}
			character.AnimController.StopUsingItem();
			Character controlled = Character.Controlled;
			this.item.CreateServerEvent<Controller>(this);
		}

		// Token: 0x0600409C RID: 16540 RVA: 0x0019D170 File Offset: 0x0019B370
		public override bool Select(Character activator)
		{
			if (activator == null || activator.Removed)
			{
				return false;
			}
			if (base.Item.Condition <= 0f && !this.UpdateWhenInactive)
			{
				return false;
			}
			if ((this.UsableIn == Controller.UseEnvironment.Water && !activator.AnimController.InWater) || (this.UsableIn == Controller.UseEnvironment.Air && activator.AnimController.InWater))
			{
				return false;
			}
			if (this.User != null && !this.User.Removed)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsClient)
				{
					return false;
				}
				if (this.AllowPuttingInOtherCharacters && this.CanPutSelectedCharacter(activator.SelectedCharacter, false))
				{
					return false;
				}
				if (this.User == activator || this.SelectingKicksCharacterOut)
				{
					if (this.User != activator)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 3);
						defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(activator));
						defaultInterpolatedStringHandler.AppendLiteral(" removed ");
						defaultInterpolatedStringHandler.AppendFormatted(GameServer.CharacterLogName(this.User));
						defaultInterpolatedStringHandler.AppendLiteral(" from ");
						defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
						GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Attack);
					}
					this.IsActive = false;
					this.CancelUsing(this.User);
					this.User = null;
					return false;
				}
				if (this.User.IsBot && !activator.IsBot && this.AllowSelectingWhenSelectedByBot)
				{
					this.CancelUsing(this.User);
					this.User = activator;
					this.IsActive = true;
					return true;
				}
				return this.AllowSelectingWhenSelectedByOther;
			}
			else
			{
				if (this.AllowPuttingInOtherCharacters && this.CanPutSelectedCharacter(activator.SelectedCharacter, false))
				{
					if (activator.SelectedCharacter.IsPet)
					{
						activator.SelectedCharacter.SetStun(MathF.Max(activator.SelectedCharacter.Stun, 4f), false, true);
					}
					else
					{
						activator.SelectedCharacter.SetStun(MathF.Max(activator.SelectedCharacter.Stun, 1f), false, true);
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(14, 3);
					defaultInterpolatedStringHandler2.AppendFormatted(GameServer.CharacterLogName(activator));
					defaultInterpolatedStringHandler2.AppendLiteral(" forced ");
					defaultInterpolatedStringHandler2.AppendFormatted(GameServer.CharacterLogName(activator.SelectedCharacter));
					defaultInterpolatedStringHandler2.AppendLiteral(" into ");
					defaultInterpolatedStringHandler2.AppendFormatted(this.item.Name);
					GameServer.Log(defaultInterpolatedStringHandler2.ToStringAndClear(), ServerLog.MessageType.Attack);
					this.User = activator.SelectedCharacter;
					this.User.SelectedItem = base.Item;
					this.IsActive = true;
					if (this.ForceUserToStayAttached && this.item.Container != null)
					{
						this.forceSelectNextFrame = true;
					}
					return false;
				}
				if (this.CanBeSelectedByCharacters)
				{
					activator.DeselectCharacter();
					this.User = activator;
					this.IsActive = true;
					if (this.ForceUserToStayAttached && this.item.Container != null)
					{
						this.forceSelectNextFrame = true;
						return false;
					}
				}
				if (this.IsOutOfPower())
				{
					return false;
				}
				if (!string.IsNullOrEmpty(this.output))
				{
					this.item.SendSignal(new Signal(this.output, 0, this.User, null, 0f, 1f), "signal_out");
				}
				return true;
			}
		}

		// Token: 0x0600409D RID: 16541 RVA: 0x0019D484 File Offset: 0x0019B684
		public bool IsAttachedUser(Character character)
		{
			return character != null && character == this.User && this.ForceUserToStayAttached;
		}

		// Token: 0x0600409E RID: 16542 RVA: 0x0019D49A File Offset: 0x0019B69A
		public override void FlipX(bool relativeToSub)
		{
			if (this.dir != Direction.None)
			{
				this.dir = ((this.dir == Direction.Left) ? Direction.Right : Direction.Left);
			}
			this.userPos.X = -this.UserPos.X;
			this.FlipLimbPositions();
		}

		// Token: 0x0600409F RID: 16543 RVA: 0x0019D4D4 File Offset: 0x0019B6D4
		public override void FlipY(bool relativeToSub)
		{
			this.userPos.Y = -this.UserPos.Y;
			for (int i = 0; i < this.limbPositions.Count; i++)
			{
				float diff = (float)this.item.Rect.Y + this.limbPositions[i].Position.Y - (float)this.item.Rect.Center.Y;
				Vector2 flippedPos = new Vector2(this.limbPositions[i].Position.X, (float)this.item.Rect.Center.Y - diff - (float)this.item.Rect.Y);
				this.limbPositions[i] = new LimbPos(this.limbPositions[i].LimbType, flippedPos, this.limbPositions[i].AllowUsingLimb);
			}
		}

		// Token: 0x060040A0 RID: 16544 RVA: 0x0019D5D8 File Offset: 0x0019B7D8
		public override bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			bool canPutCharacter = this.AllowPuttingInOtherCharacters && this.CanPutSelectedCharacter(character.SelectedCharacter, addMessage);
			bool canKickCharacter = this.SelectingKicksCharacterOut && this.User != null && !this.User.Removed;
			bool canUseController = this.CanBeSelectedByCharacters;
			return (!canPutCharacter || !canKickCharacter) && (canKickCharacter || canPutCharacter || canUseController) && !this.IsSpawnContainerFull() && base.HasRequiredItems(character, addMessage, msg);
		}

		// Token: 0x060040A1 RID: 16545 RVA: 0x0019D64C File Offset: 0x0019B84C
		public override bool HasAccess(Character character)
		{
			return this.item.IsInteractable(character) && base.HasAccess(character);
		}

		// Token: 0x060040A2 RID: 16546 RVA: 0x0019D665 File Offset: 0x0019B865
		private bool CanPutSelectedCharacter(Character character, bool showMessage = false)
		{
			return character != null && character.IsContainable && (character.IsKnockedDownOrRagdolled || character.LockHands || character.IsPet);
		}

		// Token: 0x060040A3 RID: 16547 RVA: 0x0019D695 File Offset: 0x0019B895
		public override XElement Save(XElement parentElement)
		{
			return this.SaveLimbPositions(base.Save(parentElement));
		}

		// Token: 0x060040A4 RID: 16548 RVA: 0x0019D6A4 File Offset: 0x0019B8A4
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			GameSession gameSession = GameMain.GameSession;
			GameModePreset gameModePreset;
			if (gameSession == null)
			{
				gameModePreset = null;
			}
			else
			{
				GameMode gameMode = gameSession.GameMode;
				gameModePreset = ((gameMode != null) ? gameMode.Preset : null);
			}
			if (gameModePreset == GameModePreset.TestMode)
			{
				this.LoadLimbPositions(componentElement);
			}
		}

		// Token: 0x060040A5 RID: 16549 RVA: 0x0019D6DC File Offset: 0x0019B8DC
		private XElement SaveLimbPositions(XElement element)
		{
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				if (this.item.FlippedX)
				{
					this.FlipLimbPositions();
				}
				foreach (LimbPos limbPos in this.limbPositions)
				{
					element.Add(new XElement("limbposition", new object[]
					{
						new XAttribute("limb", limbPos.LimbType),
						new XAttribute("position", XMLExtensions.Vector2ToString(limbPos.Position)),
						new XAttribute("allowusinglimb", limbPos.AllowUsingLimb)
					}));
				}
				if (this.item.FlippedX)
				{
					this.FlipLimbPositions();
				}
			}
			return element;
		}

		// Token: 0x060040A6 RID: 16550 RVA: 0x0019D7D4 File Offset: 0x0019B9D4
		private void LoadLimbPositions(ContentXElement element)
		{
			this.limbPositions.Clear();
			foreach (ContentXElement subElement in element.Elements())
			{
				if (!(subElement.Name != "limbposition"))
				{
					string limbStr = subElement.GetAttributeString("limb", "");
					LimbType limbType;
					if (!Enum.TryParse<LimbType>(subElement.GetAttribute("limb").Value, out limbType))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(45, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Error in item \"");
						defaultInterpolatedStringHandler.AppendFormatted(this.item.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\" - ");
						defaultInterpolatedStringHandler.AppendFormatted(limbStr);
						defaultInterpolatedStringHandler.AppendLiteral(" is not a valid limb type.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					}
					else
					{
						LimbType limbType2 = limbType;
						ContentXElement contentXElement = subElement;
						string key = "position";
						Vector2 zero = Vector2.Zero;
						LimbPos limbPos = new LimbPos(limbType2, contentXElement.GetAttributeVector2(key, zero), subElement.GetAttributeBool("allowusinglimb", false));
						this.limbPositions.Add(limbPos);
						if (!limbPos.AllowUsingLimb && (limbType == LimbType.RightHand || limbType == LimbType.RightForearm || limbType == LimbType.RightArm || limbType == LimbType.LeftHand || limbType == LimbType.LeftForearm || limbType == LimbType.LeftArm))
						{
							this.AllowAiming = false;
						}
					}
				}
			}
		}

		// Token: 0x060040A7 RID: 16551 RVA: 0x0019D938 File Offset: 0x0019BB38
		private void FlipLimbPositions()
		{
			for (int i = 0; i < this.limbPositions.Count; i++)
			{
				float diff = (float)this.item.Rect.X + this.limbPositions[i].Position.X * this.item.Scale - (float)this.item.Rect.Center.X;
				Vector2 flippedPos = new Vector2(((float)this.item.Rect.Center.X - diff - (float)this.item.Rect.X) / this.item.Scale, this.limbPositions[i].Position.Y);
				this.limbPositions[i] = new LimbPos(this.limbPositions[i].LimbType, flippedPos, this.limbPositions[i].AllowUsingLimb);
			}
		}

		// Token: 0x060040A8 RID: 16552 RVA: 0x0019DA3C File Offset: 0x0019BC3C
		public override void OnItemLoaded()
		{
			if (this.item.FlippedX && this.NonInteractableWhenFlippedX)
			{
				this.item.NonInteractable = true;
				return;
			}
			if (this.item.FlippedY && this.NonInteractableWhenFlippedY)
			{
				this.item.NonInteractable = true;
			}
		}

		// Token: 0x060040A9 RID: 16553 RVA: 0x0019DA8C File Offset: 0x0019BC8C
		public override void Reset()
		{
			base.Reset();
			this.LoadLimbPositions(this.originalElement);
			if (this.item.FlippedX)
			{
				this.FlipLimbPositions();
			}
		}

		// Token: 0x04001EBD RID: 7869
		private readonly List<LimbPos> limbPositions = new List<LimbPos>();

		// Token: 0x04001EBE RID: 7870
		private Direction dir;

		// Token: 0x04001EBF RID: 7871
		private Vector2 userPos;

		// Token: 0x04001EC0 RID: 7872
		private Camera cam;

		// Token: 0x04001EC1 RID: 7873
		private Character user;

		// Token: 0x04001EC2 RID: 7874
		private Item focusTarget;

		// Token: 0x04001EC3 RID: 7875
		private float targetRotation;

		// Token: 0x04001EC5 RID: 7877
		private string output;

		// Token: 0x04001EC6 RID: 7878
		private string falseOutput;

		// Token: 0x04001EC7 RID: 7879
		private bool state;

		// Token: 0x04001EDB RID: 7899
		private const float TeleportTransitionSpeed = 8f;

		// Token: 0x04001EDC RID: 7900
		private float teleportTransition;

		// Token: 0x04001EDD RID: 7901
		private Vector2 teleportStartPosition;

		// Token: 0x04001EDE RID: 7902
		private readonly ItemPrefab spawnItemOnSelectedPrefab;

		// Token: 0x04001EDF RID: 7903
		private readonly ItemContainer containerToSpawnOnSelectedItem;

		// Token: 0x04001EE0 RID: 7904
		private Item spawnedItemOnSelected;

		// Token: 0x04001EE1 RID: 7905
		private bool forceSelectNextFrame;

		// Token: 0x04001EE2 RID: 7906
		private float userCanInteractCheckTimer;

		// Token: 0x04001EE3 RID: 7907
		private const float UserCanInteractCheckInterval = 1f;

		// Token: 0x04001EE4 RID: 7908
		private double lastUsed;

		// Token: 0x02000D93 RID: 3475
		public enum UseEnvironment
		{
			// Token: 0x04004018 RID: 16408
			Air,
			// Token: 0x04004019 RID: 16409
			Water,
			// Token: 0x0400401A RID: 16410
			Both
		}
	}
}

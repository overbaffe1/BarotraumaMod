using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Lights;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B9 RID: 1465
	internal class Controller : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06005B4D RID: 23373 RVA: 0x002EB918 File Offset: 0x002E9B18
		public void UpdateMsg()
		{
			if (Character.Controlled == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(this.KickOutCharacterMsg) && this.SelectingKicksCharacterOut && this.User != null && !this.User.Removed)
			{
				base.DisplayMsg = TextManager.ParseInputTypes(TextManager.Get(this.KickOutCharacterMsg), false);
			}
			else if (!string.IsNullOrEmpty(this.PutOtherCharacterMsg) && this.AllowPuttingInOtherCharacters && this.CanPutSelectedCharacter(Character.Controlled.SelectedCharacter, false))
			{
				base.DisplayMsg = TextManager.ParseInputTypes(TextManager.Get(this.PutOtherCharacterMsg), false);
			}
			else
			{
				base.DisplayMsg = TextManager.ParseInputTypes(TextManager.Get(base.Msg), false).Fallback(base.Msg, true);
			}
			CharacterHUD.RecreateHudTextsIfControlling(Character.Controlled);
		}

		// Token: 0x06005B4E RID: 23374 RVA: 0x002EB9E4 File Offset: 0x002E9BE4
		public override void DrawHUD(SpriteBatch spriteBatch, Character character)
		{
			base.DrawHUD(spriteBatch, character);
			if (this.focusTarget != null && character.ViewTarget == this.focusTarget)
			{
				foreach (ItemComponent ic in this.focusTarget.Components)
				{
					if (ic.ShouldDrawHUD(character))
					{
						ic.DrawHUD(spriteBatch, character);
					}
				}
			}
		}

		// Token: 0x06005B4F RID: 23375 RVA: 0x002EBA64 File Offset: 0x002E9C64
		public override void AddToGUIUpdateList(int order = 0)
		{
			base.AddToGUIUpdateList(order);
			if (this.focusTarget != null && Character.Controlled.ViewTarget == this.focusTarget)
			{
				this.focusTarget.AddToGUIUpdateList(order);
			}
		}

		// Token: 0x06005B50 RID: 23376 RVA: 0x002EBA94 File Offset: 0x002E9C94
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.State = msg.ReadBoolean();
			ushort userID = msg.ReadUInt16();
			if (userID == 0)
			{
				if (this.User != null)
				{
					this.IsActive = false;
					this.CancelUsing(this.User);
					this.User = null;
					return;
				}
			}
			else
			{
				Character newUser = Entity.FindEntityByID(userID) as Character;
				if (newUser != this.User)
				{
					this.CancelUsing(this.User);
				}
				this.User = newUser;
				if (this.ForceUserToStayAttached && this.user != null && !this.user.IsAnySelectedItem(base.Item))
				{
					this.user.SelectedItem = base.Item;
				}
				this.IsActive = true;
			}
		}

		// Token: 0x17001702 RID: 5890
		// (get) Token: 0x06005B51 RID: 23377 RVA: 0x002EBB3D File Offset: 0x002E9D3D
		public Direction Direction
		{
			get
			{
				return this.dir;
			}
		}

		// Token: 0x17001703 RID: 5891
		// (get) Token: 0x06005B52 RID: 23378 RVA: 0x002EBB45 File Offset: 0x002E9D45
		// (set) Token: 0x06005B53 RID: 23379 RVA: 0x002EBB50 File Offset: 0x002E9D50
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
				this.UpdateMsg();
				if (this.HideAllItemComponentHUDs && Character.Controlled == this.user)
				{
					base.Item.ClearActiveHUDs();
				}
			}
		}

		// Token: 0x17001704 RID: 5892
		// (get) Token: 0x06005B54 RID: 23380 RVA: 0x002EBBB8 File Offset: 0x002E9DB8
		// (set) Token: 0x06005B55 RID: 23381 RVA: 0x002EBBC0 File Offset: 0x002E9DC0
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

		// Token: 0x17001705 RID: 5893
		// (get) Token: 0x06005B56 RID: 23382 RVA: 0x002EBBC9 File Offset: 0x002E9DC9
		public IEnumerable<LimbPos> LimbPositions
		{
			get
			{
				return this.limbPositions;
			}
		}

		// Token: 0x17001706 RID: 5894
		// (get) Token: 0x06005B57 RID: 23383 RVA: 0x002EBBD1 File Offset: 0x002E9DD1
		// (set) Token: 0x06005B58 RID: 23384 RVA: 0x002EBBD9 File Offset: 0x002E9DD9
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "When enabled, the item will continuously send out a signal and interacting with it will flip the signal (making the item behave like a switch). When disabled, the item will simply send out a signal when interacted with.", "", true)]
		public bool IsToggle { get; set; }

		// Token: 0x17001707 RID: 5895
		// (get) Token: 0x06005B59 RID: 23385 RVA: 0x002EBBE2 File Offset: 0x002E9DE2
		// (set) Token: 0x06005B5A RID: 23386 RVA: 0x002EBBEA File Offset: 0x002E9DEA
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

		// Token: 0x17001708 RID: 5896
		// (get) Token: 0x06005B5B RID: 23387 RVA: 0x002EBC14 File Offset: 0x002E9E14
		// (set) Token: 0x06005B5C RID: 23388 RVA: 0x002EBC1C File Offset: 0x002E9E1C
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

		// Token: 0x17001709 RID: 5897
		// (get) Token: 0x06005B5D RID: 23389 RVA: 0x002EBC46 File Offset: 0x002E9E46
		// (set) Token: 0x06005B5E RID: 23390 RVA: 0x002EBC50 File Offset: 0x002E9E50
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

		// Token: 0x1700170A RID: 5898
		// (get) Token: 0x06005B5F RID: 23391 RVA: 0x002EBC93 File Offset: 0x002E9E93
		// (set) Token: 0x06005B60 RID: 23392 RVA: 0x002EBC9B File Offset: 0x002E9E9B
		[Serialize(true, IsPropertySaveable.No, "Should the HUD (inventory, health bar, etc) be hidden when this item is selected.", "", false)]
		public bool HideHUD { get; set; }

		// Token: 0x1700170B RID: 5899
		// (get) Token: 0x06005B61 RID: 23393 RVA: 0x002EBCA4 File Offset: 0x002E9EA4
		// (set) Token: 0x06005B62 RID: 23394 RVA: 0x002EBCAC File Offset: 0x002E9EAC
		[Serialize(false, IsPropertySaveable.No, "Should the HUDs of all item components in this item be hidden when a character is using this controller.", "", false)]
		public bool HideAllItemComponentHUDs { get; set; }

		// Token: 0x1700170C RID: 5900
		// (get) Token: 0x06005B63 RID: 23395 RVA: 0x002EBCB5 File Offset: 0x002E9EB5
		// (set) Token: 0x06005B64 RID: 23396 RVA: 0x002EBCBD File Offset: 0x002E9EBD
		[Serialize(Controller.UseEnvironment.Both, IsPropertySaveable.No, "Can the item be selected in air, underwater or both.", "", false)]
		public Controller.UseEnvironment UsableIn { get; set; }

		// Token: 0x1700170D RID: 5901
		// (get) Token: 0x06005B65 RID: 23397 RVA: 0x002EBCC6 File Offset: 0x002E9EC6
		// (set) Token: 0x06005B66 RID: 23398 RVA: 0x002EBCCE File Offset: 0x002E9ECE
		[Serialize(false, IsPropertySaveable.No, "Should the character using the item be drawn behind the item.", "", false)]
		public bool DrawUserBehind { get; set; }

		// Token: 0x1700170E RID: 5902
		// (get) Token: 0x06005B67 RID: 23399 RVA: 0x002EBCD7 File Offset: 0x002E9ED7
		// (set) Token: 0x06005B68 RID: 23400 RVA: 0x002EBCDF File Offset: 0x002E9EDF
		[Serialize(true, IsPropertySaveable.No, "Can another character select this controller when another character has already selected it?", "", false)]
		public bool AllowSelectingWhenSelectedByOther { get; set; }

		// Token: 0x1700170F RID: 5903
		// (get) Token: 0x06005B69 RID: 23401 RVA: 0x002EBCE8 File Offset: 0x002E9EE8
		// (set) Token: 0x06005B6A RID: 23402 RVA: 0x002EBCF0 File Offset: 0x002E9EF0
		[Serialize(true, IsPropertySaveable.No, "Can another character select this controller when a bot has already selected it?", "", false)]
		public bool AllowSelectingWhenSelectedByBot { get; set; }

		// Token: 0x17001710 RID: 5904
		// (get) Token: 0x06005B6B RID: 23403 RVA: 0x002EBCF9 File Offset: 0x002E9EF9
		// (set) Token: 0x06005B6C RID: 23404 RVA: 0x002EBD01 File Offset: 0x002E9F01
		[Serialize(false, IsPropertySaveable.No, "Can a character put another character into this controller by dragging them and selecting this controller?", "", false)]
		public bool AllowPuttingInOtherCharacters { get; set; }

		// Token: 0x17001711 RID: 5905
		// (get) Token: 0x06005B6D RID: 23405 RVA: 0x002EBD0A File Offset: 0x002E9F0A
		// (set) Token: 0x06005B6E RID: 23406 RVA: 0x002EBD12 File Offset: 0x002E9F12
		[Serialize(true, IsPropertySaveable.No, "Can a character select this controller by themselves?", "", false)]
		public bool CanBeSelectedByCharacters { get; set; }

		// Token: 0x17001712 RID: 5906
		// (get) Token: 0x06005B6F RID: 23407 RVA: 0x002EBD1B File Offset: 0x002E9F1B
		// (set) Token: 0x06005B70 RID: 23408 RVA: 0x002EBD23 File Offset: 0x002E9F23
		[Serialize(false, IsPropertySaveable.No, "If a character selects this controller, but another character already has it selected, should it be kicked out?", "", false)]
		public bool SelectingKicksCharacterOut { get; set; }

		// Token: 0x17001713 RID: 5907
		// (get) Token: 0x06005B71 RID: 23409 RVA: 0x002EBD2C File Offset: 0x002E9F2C
		// (set) Token: 0x06005B72 RID: 23410 RVA: 0x002EBD34 File Offset: 0x002E9F34
		[Serialize("", IsPropertySaveable.No, "Message displayed when there's a character inside this controller.", "", false)]
		public string KickOutCharacterMsg { get; set; }

		// Token: 0x17001714 RID: 5908
		// (get) Token: 0x06005B73 RID: 23411 RVA: 0x002EBD3D File Offset: 0x002E9F3D
		// (set) Token: 0x06005B74 RID: 23412 RVA: 0x002EBD45 File Offset: 0x002E9F45
		[Serialize("", IsPropertySaveable.No, "Message displayed when you are putting a character into the controller.", "", false)]
		public string PutOtherCharacterMsg { get; set; }

		// Token: 0x17001715 RID: 5909
		// (get) Token: 0x06005B75 RID: 23413 RVA: 0x002EBD4E File Offset: 0x002E9F4E
		// (set) Token: 0x06005B76 RID: 23414 RVA: 0x002EBD56 File Offset: 0x002E9F56
		[Serialize("", IsPropertySaveable.No, "Spawns this item in the first available item container slot when a character selects this controller, if the item container is full, the character will not be able to select the controller.", "", false)]
		public Identifier SpawnItemOnSelected { get; private set; }

		// Token: 0x17001716 RID: 5910
		// (get) Token: 0x06005B77 RID: 23415 RVA: 0x002EBD5F File Offset: 0x002E9F5F
		public bool ControlCharacterPose
		{
			get
			{
				return this.limbPositions.Count > 0;
			}
		}

		// Token: 0x17001717 RID: 5911
		// (get) Token: 0x06005B78 RID: 23416 RVA: 0x002EBD6F File Offset: 0x002E9F6F
		// (set) Token: 0x06005B79 RID: 23417 RVA: 0x002EBD77 File Offset: 0x002E9F77
		public bool UserInCorrectPosition { get; private set; }

		// Token: 0x17001718 RID: 5912
		// (get) Token: 0x06005B7A RID: 23418 RVA: 0x002EBD80 File Offset: 0x002E9F80
		// (set) Token: 0x06005B7B RID: 23419 RVA: 0x002EBD88 File Offset: 0x002E9F88
		public bool AllowAiming { get; private set; } = true;

		// Token: 0x17001719 RID: 5913
		// (get) Token: 0x06005B7C RID: 23420 RVA: 0x002EBD91 File Offset: 0x002E9F91
		// (set) Token: 0x06005B7D RID: 23421 RVA: 0x002EBD99 File Offset: 0x002E9F99
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool NonInteractableWhenFlippedX { get; set; }

		// Token: 0x1700171A RID: 5914
		// (get) Token: 0x06005B7E RID: 23422 RVA: 0x002EBDA2 File Offset: 0x002E9FA2
		// (set) Token: 0x06005B7F RID: 23423 RVA: 0x002EBDAA File Offset: 0x002E9FAA
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool NonInteractableWhenFlippedY { get; set; }

		// Token: 0x1700171B RID: 5915
		// (get) Token: 0x06005B80 RID: 23424 RVA: 0x002EBDB3 File Offset: 0x002E9FB3
		// (set) Token: 0x06005B81 RID: 23425 RVA: 0x002EBDBB File Offset: 0x002E9FBB
		[Serialize(false, IsPropertySaveable.No, "Does the Controller require power to function (= to send signals and move the camera focus to a connected item)?", "", false)]
		public bool RequirePower { get; set; }

		// Token: 0x1700171C RID: 5916
		// (get) Token: 0x06005B82 RID: 23426 RVA: 0x002EBDC4 File Offset: 0x002E9FC4
		// (set) Token: 0x06005B83 RID: 23427 RVA: 0x002EBDCC File Offset: 0x002E9FCC
		[Serialize(false, IsPropertySaveable.No, "If true, other items can be used simultaneously.", "", false)]
		public bool IsSecondaryItem { get; private set; }

		// Token: 0x1700171D RID: 5917
		// (get) Token: 0x06005B84 RID: 23428 RVA: 0x002EBDD5 File Offset: 0x002E9FD5
		// (set) Token: 0x06005B85 RID: 23429 RVA: 0x002EBDDD File Offset: 0x002E9FDD
		[Serialize(false, IsPropertySaveable.No, "If enabled, the user sticks to the position of this item even if the item moves.", "", false)]
		public bool ForceUserToStayAttached { get; set; }

		// Token: 0x06005B86 RID: 23430 RVA: 0x002EBDE8 File Offset: 0x002E9FE8
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

		// Token: 0x06005B87 RID: 23431 RVA: 0x002EBEEC File Offset: 0x002EA0EC
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

		// Token: 0x06005B88 RID: 23432 RVA: 0x002EC628 File Offset: 0x002EA828
		private bool IsSpawnContainerFull()
		{
			return this.spawnItemOnSelectedPrefab != null && this.containerToSpawnOnSelectedItem != null && this.containerToSpawnOnSelectedItem.Inventory.IsFull(false);
		}

		// Token: 0x06005B89 RID: 23433 RVA: 0x002EC654 File Offset: 0x002EA854
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

		// Token: 0x06005B8A RID: 23434 RVA: 0x002EC723 File Offset: 0x002EA923
		private bool CheckUserCanInteract()
		{
			if (this.User != null && this.userCanInteractCheckTimer <= 0f)
			{
				this.userCanInteractCheckTimer = 1f;
				return this.User.CanInteractWith(this.item, true);
			}
			return true;
		}

		// Token: 0x06005B8B RID: 23435 RVA: 0x002EC75C File Offset: 0x002EA95C
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
				}
			}
			else if (!string.IsNullOrEmpty(this.output))
			{
				this.item.SendSignal(new Signal(this.output, 0, this.User, null, 0f, 1f), "trigger_out");
			}
			this.lastUsed = Timing.TotalTime;
			return true;
		}

		// Token: 0x06005B8C RID: 23436 RVA: 0x002EC870 File Offset: 0x002EAA70
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
			if (character == Character.Controlled && this.cam != null)
			{
				LightManager.ViewTarget = this.focusTarget;
				this.cam.TargetPos = this.focusTarget.WorldPosition;
				this.cam.OffsetAmount = MathHelper.Lerp(this.cam.OffsetAmount, this.focusTarget.Prefab.OffsetOnSelected * this.focusTarget.OffsetOnSelectedMultiplier, deltaTime * 10f);
				this.HideHUDs(true);
			}
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

		// Token: 0x06005B8D RID: 23437 RVA: 0x002ECAA8 File Offset: 0x002EACA8
		public bool IsOutOfPower()
		{
			if (!this.RequirePower)
			{
				return false;
			}
			Powered powered = this.item.GetComponent<Powered>();
			return powered == null || powered.Voltage < powered.MinVoltage;
		}

		// Token: 0x06005B8E RID: 23438 RVA: 0x002ECAE0 File Offset: 0x002EACE0
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

		// Token: 0x06005B8F RID: 23439 RVA: 0x002ECC80 File Offset: 0x002EAE80
		public override bool Pick(Character picker)
		{
			if (this.IsOutOfPower())
			{
				return false;
			}
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				return false;
			}
			if (this.IsToggle)
			{
				if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
				{
					this.State = !this.State;
				}
			}
			else if (!string.IsNullOrEmpty(this.output))
			{
				this.item.SendSignal(new Signal(this.output, 0, picker, null, 0f, 1f), "signal_out");
			}
			base.PlaySound(ActionType.OnUse, picker);
			base.ApplyStatusEffects(ActionType.OnUse, 1f, picker, null, null, null, null, 1f);
			return true;
		}

		// Token: 0x06005B90 RID: 23440 RVA: 0x002ECD30 File Offset: 0x002EAF30
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
			if (character == Character.Controlled)
			{
				this.HideHUDs(false);
			}
		}

		// Token: 0x06005B91 RID: 23441 RVA: 0x002ECE4C File Offset: 0x002EB04C
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

		// Token: 0x06005B92 RID: 23442 RVA: 0x002ED094 File Offset: 0x002EB294
		public bool IsAttachedUser(Character character)
		{
			return character != null && character == this.User && this.ForceUserToStayAttached;
		}

		// Token: 0x06005B93 RID: 23443 RVA: 0x002ED0AA File Offset: 0x002EB2AA
		public override void FlipX(bool relativeToSub)
		{
			if (this.dir != Direction.None)
			{
				this.dir = ((this.dir == Direction.Left) ? Direction.Right : Direction.Left);
			}
			this.userPos.X = -this.UserPos.X;
			this.FlipLimbPositions();
		}

		// Token: 0x06005B94 RID: 23444 RVA: 0x002ED0E4 File Offset: 0x002EB2E4
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

		// Token: 0x06005B95 RID: 23445 RVA: 0x002ED1E8 File Offset: 0x002EB3E8
		public override bool HasRequiredItems(Character character, bool addMessage, LocalizedString msg = null)
		{
			this.UpdateMsg();
			bool canPutCharacter = this.AllowPuttingInOtherCharacters && this.CanPutSelectedCharacter(character.SelectedCharacter, addMessage);
			bool canKickCharacter = this.SelectingKicksCharacterOut && this.User != null && !this.User.Removed;
			bool canUseController = this.CanBeSelectedByCharacters;
			if (canPutCharacter && canKickCharacter)
			{
				if (addMessage)
				{
					GUI.AddMessage(TextManager.Get("ItemMsgAlreadyHasCharacterFail"), Color.Red, null, false, null);
					SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
				}
				return false;
			}
			if (!canKickCharacter && !canPutCharacter && !canUseController)
			{
				return false;
			}
			if (this.IsSpawnContainerFull())
			{
				if (addMessage)
				{
					GUI.AddMessage(TextManager.Get("ItemMsgNotEnoughSpaceCharacterFail"), Color.Red, null, false, null);
					SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
				}
				return false;
			}
			return base.HasRequiredItems(character, addMessage, msg);
		}

		// Token: 0x06005B96 RID: 23446 RVA: 0x002ED2B2 File Offset: 0x002EB4B2
		public override bool HasAccess(Character character)
		{
			return this.item.IsInteractable(character) && base.HasAccess(character);
		}

		// Token: 0x06005B97 RID: 23447 RVA: 0x002ED2CC File Offset: 0x002EB4CC
		private bool CanPutSelectedCharacter(Character character, bool showMessage = false)
		{
			if (character == null)
			{
				return false;
			}
			if (!character.IsContainable)
			{
				if (showMessage)
				{
					GUI.AddMessage(TextManager.Get("ItemMsgPutCharacterFail"), Color.Red, null, true, null);
				}
				return false;
			}
			return character.IsKnockedDownOrRagdolled || character.LockHands || character.IsPet;
		}

		// Token: 0x06005B98 RID: 23448 RVA: 0x002ED32C File Offset: 0x002EB52C
		private void HideHUDs(bool value)
		{
			if (this.isHUDsHidden == value)
			{
				return;
			}
			if (value)
			{
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					CrewManager crewManager = gameSession.CrewManager;
					if (crewManager != null)
					{
						crewManager.AutoHideCrewList();
					}
				}
				ChatBox.AutoHideChatBox();
			}
			else
			{
				GameSession gameSession2 = GameMain.GameSession;
				if (gameSession2 != null)
				{
					CrewManager crewManager2 = gameSession2.CrewManager;
					if (crewManager2 != null)
					{
						crewManager2.ResetCrewListOpenState();
					}
				}
				ChatBox.ResetChatBoxOpenState();
			}
			this.isHUDsHidden = value;
		}

		// Token: 0x06005B99 RID: 23449 RVA: 0x002ED38F File Offset: 0x002EB58F
		public override XElement Save(XElement parentElement)
		{
			return this.SaveLimbPositions(base.Save(parentElement));
		}

		// Token: 0x06005B9A RID: 23450 RVA: 0x002ED39E File Offset: 0x002EB59E
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

		// Token: 0x06005B9B RID: 23451 RVA: 0x002ED3D8 File Offset: 0x002EB5D8
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

		// Token: 0x06005B9C RID: 23452 RVA: 0x002ED4D0 File Offset: 0x002EB6D0
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

		// Token: 0x06005B9D RID: 23453 RVA: 0x002ED634 File Offset: 0x002EB834
		private void FlipLimbPositions()
		{
			for (int i = 0; i < this.limbPositions.Count; i++)
			{
				float diff = (float)this.item.Rect.X + this.limbPositions[i].Position.X * this.item.Scale - (float)this.item.Rect.Center.X;
				Vector2 flippedPos = new Vector2(((float)this.item.Rect.Center.X - diff - (float)this.item.Rect.X) / this.item.Scale, this.limbPositions[i].Position.Y);
				this.limbPositions[i] = new LimbPos(this.limbPositions[i].LimbType, flippedPos, this.limbPositions[i].AllowUsingLimb);
			}
		}

		// Token: 0x06005B9E RID: 23454 RVA: 0x002ED738 File Offset: 0x002EB938
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

		// Token: 0x06005B9F RID: 23455 RVA: 0x002ED788 File Offset: 0x002EB988
		public override void Reset()
		{
			base.Reset();
			this.LoadLimbPositions(this.originalElement);
			if (this.item.FlippedX)
			{
				this.FlipLimbPositions();
			}
		}

		// Token: 0x04002E84 RID: 11908
		private bool isHUDsHidden;

		// Token: 0x04002E85 RID: 11909
		private readonly List<LimbPos> limbPositions = new List<LimbPos>();

		// Token: 0x04002E86 RID: 11910
		private Direction dir;

		// Token: 0x04002E87 RID: 11911
		private Vector2 userPos;

		// Token: 0x04002E88 RID: 11912
		private Camera cam;

		// Token: 0x04002E89 RID: 11913
		private Character user;

		// Token: 0x04002E8A RID: 11914
		private Item focusTarget;

		// Token: 0x04002E8B RID: 11915
		private float targetRotation;

		// Token: 0x04002E8D RID: 11917
		private string output;

		// Token: 0x04002E8E RID: 11918
		private string falseOutput;

		// Token: 0x04002E8F RID: 11919
		private bool state;

		// Token: 0x04002EA3 RID: 11939
		private const float TeleportTransitionSpeed = 8f;

		// Token: 0x04002EA4 RID: 11940
		private float teleportTransition;

		// Token: 0x04002EA5 RID: 11941
		private Vector2 teleportStartPosition;

		// Token: 0x04002EA6 RID: 11942
		private readonly ItemPrefab spawnItemOnSelectedPrefab;

		// Token: 0x04002EA7 RID: 11943
		private readonly ItemContainer containerToSpawnOnSelectedItem;

		// Token: 0x04002EA8 RID: 11944
		private Item spawnedItemOnSelected;

		// Token: 0x04002EA9 RID: 11945
		private bool forceSelectNextFrame;

		// Token: 0x04002EAA RID: 11946
		private float userCanInteractCheckTimer;

		// Token: 0x04002EAB RID: 11947
		private const float UserCanInteractCheckInterval = 1f;

		// Token: 0x04002EAC RID: 11948
		private double lastUsed;

		// Token: 0x020013D7 RID: 5079
		public enum UseEnvironment
		{
			// Token: 0x040063A0 RID: 25504
			Air,
			// Token: 0x040063A1 RID: 25505
			Water,
			// Token: 0x040063A2 RID: 25506
			Both
		}
	}
}

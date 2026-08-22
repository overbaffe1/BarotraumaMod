using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000015 RID: 21
	internal class Limb : ISerializableEntity, ISpatialEntity
	{
		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060002BA RID: 698 RVA: 0x00015EF8 File Offset: 0x000140F8
		// (set) Token: 0x060002BB RID: 699 RVA: 0x00015F00 File Offset: 0x00014100
		public PhysicsBody LagCompensatedBody { get; private set; }

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060002BC RID: 700 RVA: 0x00015F09 File Offset: 0x00014109
		public Queue<PosInfo> MemState { get; } = new Queue<PosInfo>();

		// Token: 0x060002BD RID: 701 RVA: 0x00015F14 File Offset: 0x00014114
		public static void SetLagCompensatedBodyPositions(Client client)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			float latency = (float)client.Ping / 1000f / 2f;
			float time = (float)Timing.TotalTime - MathUtils.Min(new float[]
			{
				latency,
				GameMain.Server.ServerSettings.MaxLagCompensationSeconds
			});
			Func<PosInfo, bool> <>9__0;
			foreach (Character character in Character.CharacterList)
			{
				foreach (Limb limb in character.AnimController.Limbs)
				{
					if (limb.body.Enabled && !limb.IgnoreCollisions)
					{
						IEnumerable<PosInfo> memState = limb.MemState;
						Func<PosInfo, bool> predicate;
						if ((predicate = <>9__0) == null)
						{
							predicate = (<>9__0 = ((PosInfo l) => l.Timestamp <= time));
						}
						PosInfo matchingState = memState.FirstOrDefault(predicate);
						if (matchingState != null)
						{
							limb.LagCompensatedBody.SetTransformIgnoreContacts(matchingState.Position, matchingState.Rotation.GetValueOrDefault(), true);
						}
					}
				}
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x060002BE RID: 702 RVA: 0x00016050 File Offset: 0x00014250
		// (set) Token: 0x060002BF RID: 703 RVA: 0x00016058 File Offset: 0x00014258
		public float SeveredFadeOutTime { get; private set; } = 10f;

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x060002C0 RID: 704 RVA: 0x00016061 File Offset: 0x00014261
		public Vector2 StepOffset
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Params.StepOffset) * this.ragdoll.RagdollParams.JointScale;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x060002C1 RID: 705 RVA: 0x00016088 File Offset: 0x00014288
		// (set) Token: 0x060002C2 RID: 706 RVA: 0x00016090 File Offset: 0x00014290
		public bool InWater { get; set; }

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x060002C3 RID: 707 RVA: 0x00016099 File Offset: 0x00014299
		// (set) Token: 0x060002C4 RID: 708 RVA: 0x000160A4 File Offset: 0x000142A4
		public bool IgnoreCollisions
		{
			get
			{
				return this.ignoreCollisions;
			}
			set
			{
				this.ignoreCollisions = value;
				if (this.body != null)
				{
					if (this.ignoreCollisions)
					{
						this.body.CollisionCategories = Category.None;
						this.body.CollidesWith = Category.None;
						return;
					}
					this.body.CollisionCategories = Category.Cat2;
					this.body.CollidesWith = (Category.Cat1 | Category.Cat3 | Category.Cat4 | Category.Cat7 | Category.Cat8 | Category.Cat9 | Category.Cat10 | Category.Cat11 | Category.Cat12 | Category.Cat13 | Category.Cat14 | Category.Cat15 | Category.Cat16 | Category.Cat17 | Category.Cat18 | Category.Cat19 | Category.Cat20 | Category.Cat21 | Category.Cat22 | Category.Cat23 | Category.Cat24 | Category.Cat25 | Category.Cat26 | Category.Cat27 | Category.Cat28 | Category.Cat29 | Category.Cat30 | Category.Cat31);
				}
			}
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x060002C5 RID: 709 RVA: 0x00016100 File Offset: 0x00014300
		// (set) Token: 0x060002C6 RID: 710 RVA: 0x00016149 File Offset: 0x00014349
		public Vector2 MouthPos
		{
			get
			{
				Vector2 valueOrDefault = this.mouthPos.GetValueOrDefault();
				if (this.mouthPos == null)
				{
					valueOrDefault = this.Params.MouthPos;
					this.mouthPos = new Vector2?(valueOrDefault);
				}
				return this.mouthPos.Value;
			}
			set
			{
				this.mouthPos = new Vector2?(value);
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x060002C7 RID: 711 RVA: 0x00016157 File Offset: 0x00014357
		// (set) Token: 0x060002C8 RID: 712 RVA: 0x0001615F File Offset: 0x0001435F
		public List<DamageModifier> DamageModifiers { get; private set; } = new List<DamageModifier>();

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x060002C9 RID: 713 RVA: 0x00016168 File Offset: 0x00014368
		public int HealthIndex
		{
			get
			{
				return this.Params.HealthIndex;
			}
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060002CA RID: 714 RVA: 0x00016175 File Offset: 0x00014375
		public float Scale
		{
			get
			{
				return this.Params.Scale * this.Params.Ragdoll.LimbScale;
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060002CB RID: 715 RVA: 0x00016193 File Offset: 0x00014393
		public float AttackPriority
		{
			get
			{
				return this.Params.AttackPriority;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060002CC RID: 716 RVA: 0x000161A0 File Offset: 0x000143A0
		public bool DoesFlip
		{
			get
			{
				Character character = this.character;
				return (((character != null) ? character.AnimController.CurrentAnimationParams : null) is GroundedMovementParams && this.IsLeg) || this.Params.Flip;
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060002CD RID: 717 RVA: 0x000161D5 File Offset: 0x000143D5
		public bool DoesMirror
		{
			get
			{
				return this.IsLeg || this.DoesFlip;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060002CE RID: 718 RVA: 0x000161E7 File Offset: 0x000143E7
		public float SteerForce
		{
			get
			{
				return this.Params.SteerForce;
			}
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x060002CF RID: 719 RVA: 0x000161F4 File Offset: 0x000143F4
		public bool IsLowerBody
		{
			get
			{
				LimbType limbType = this.type;
				return limbType - LimbType.LeftLeg <= 3 || limbType - LimbType.Tail <= 4;
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x060002D0 RID: 720 RVA: 0x00016218 File Offset: 0x00014418
		public bool IsLeg
		{
			get
			{
				LimbType limbType = this.type;
				return limbType - LimbType.LeftLeg <= 3 || limbType - LimbType.RightThigh <= 1;
			}
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060002D1 RID: 721 RVA: 0x00016240 File Offset: 0x00014440
		public bool IsArm
		{
			get
			{
				LimbType limbType = this.type;
				return limbType - LimbType.LeftHand <= 5;
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060002D2 RID: 722 RVA: 0x00016261 File Offset: 0x00014461
		// (set) Token: 0x060002D3 RID: 723 RVA: 0x0001626C File Offset: 0x0001446C
		public bool IsSevered
		{
			get
			{
				return this.isSevered;
			}
			set
			{
				if (this.isSevered == value)
				{
					return;
				}
				if (value)
				{
					IEnumerable<Limb> connectedLimbs = this.GetConnectedLimbs();
					float severedFadeOutTime = this.Params.SeveredFadeOutTime;
					float val;
					if (!connectedLimbs.Any<Limb>())
					{
						val = 0f;
					}
					else
					{
						val = connectedLimbs.Max((Limb l) => l.SeveredFadeOutTime);
					}
					this.SeveredFadeOutTime = Math.Max(severedFadeOutTime, val);
				}
				this.isSevered = value;
				if (this.isSevered)
				{
					this.ragdoll.SubtractMass(this);
					if (this.type == LimbType.Head && this.character.Params.Health.DieFromBeheading)
					{
						this.character.Kill(CauseOfDeathType.Unknown, null, false, true);
						return;
					}
				}
				else
				{
					this.severedFadeOutTimer = 0f;
				}
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060002D4 RID: 724 RVA: 0x0001632E File Offset: 0x0001452E
		public Submarine Submarine
		{
			get
			{
				Character character = this.character;
				if (character == null)
				{
					return null;
				}
				return character.Submarine;
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060002D5 RID: 725 RVA: 0x00016341 File Offset: 0x00014541
		// (set) Token: 0x060002D6 RID: 726 RVA: 0x00016358 File Offset: 0x00014558
		public bool Hidden
		{
			get
			{
				return this._hidden || this.Params.Hide;
			}
			set
			{
				this._hidden = value;
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060002D7 RID: 727 RVA: 0x00016361 File Offset: 0x00014561
		// (set) Token: 0x060002D8 RID: 728 RVA: 0x00016369 File Offset: 0x00014569
		public bool Hide
		{
			get
			{
				return this.Hidden;
			}
			set
			{
				this.Hidden = value;
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060002D9 RID: 729 RVA: 0x00016372 File Offset: 0x00014572
		public Vector2 WorldPosition
		{
			get
			{
				Character character = this.character;
				if (((character != null) ? character.Submarine : null) != null)
				{
					return this.Position + this.character.Submarine.Position;
				}
				return this.Position;
			}
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060002DA RID: 730 RVA: 0x000163AA File Offset: 0x000145AA
		public Vector2 Position
		{
			get
			{
				PhysicsBody physicsBody = this.body;
				return ConvertUnits.ToDisplayUnits((physicsBody != null) ? physicsBody.SimPosition : Vector2.Zero);
			}
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060002DB RID: 731 RVA: 0x000163C7 File Offset: 0x000145C7
		public Vector2 SimPosition
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:SimPosition", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.body.SimPosition;
			}
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060002DC RID: 732 RVA: 0x00016401 File Offset: 0x00014601
		public Vector2 DrawPosition
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:DrawPosition", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.body.DrawPosition;
			}
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060002DD RID: 733 RVA: 0x0001643B File Offset: 0x0001463B
		public float Rotation
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:SimPosition", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return 0f;
				}
				return this.body.Rotation;
			}
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060002DE RID: 734 RVA: 0x00016475 File Offset: 0x00014675
		// (set) Token: 0x060002DF RID: 735 RVA: 0x0001647D File Offset: 0x0001467D
		public Vector2 AnimTargetPos { get; private set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060002E0 RID: 736 RVA: 0x00016486 File Offset: 0x00014686
		public float Mass
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.Mass:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return 1f;
				}
				return this.body.Mass;
			}
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060002E1 RID: 737 RVA: 0x000164C0 File Offset: 0x000146C0
		// (set) Token: 0x060002E2 RID: 738 RVA: 0x000164C8 File Offset: 0x000146C8
		public bool Disabled { get; set; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060002E3 RID: 739 RVA: 0x000164D1 File Offset: 0x000146D1
		public Vector2 LinearVelocity
		{
			get
			{
				if (this.Removed)
				{
					GameAnalyticsManager.AddErrorEventOnce("Limb.LinearVelocity:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, "Attempted to access a removed limb.\n" + Environment.StackTrace.CleanupStackTrace());
					return Vector2.Zero;
				}
				return this.body.LinearVelocity;
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060002E4 RID: 740 RVA: 0x0001650B File Offset: 0x0001470B
		// (set) Token: 0x060002E5 RID: 741 RVA: 0x00016521 File Offset: 0x00014721
		public float Dir
		{
			get
			{
				if (this.dir != Direction.Left)
				{
					return 1f;
				}
				return -1f;
			}
			set
			{
				this.dir = ((value == -1f) ? Direction.Left : Direction.Right);
				if (this.body != null)
				{
					this.body.Dir = this.Dir;
				}
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x0001654E File Offset: 0x0001474E
		// (set) Token: 0x060002E7 RID: 743 RVA: 0x00016556 File Offset: 0x00014756
		public float Alpha
		{
			get
			{
				return this._alpha;
			}
			set
			{
				this._alpha = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060002E8 RID: 744 RVA: 0x0001656E File Offset: 0x0001476E
		public int RefJointIndex
		{
			get
			{
				return this.Params.RefJoint;
			}
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060002E9 RID: 745 RVA: 0x0001657B File Offset: 0x0001477B
		// (set) Token: 0x060002EA RID: 746 RVA: 0x00016588 File Offset: 0x00014788
		public bool PullJointEnabled
		{
			get
			{
				return this.pullJoint.Enabled;
			}
			set
			{
				this.pullJoint.Enabled = value;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060002EB RID: 747 RVA: 0x00016596 File Offset: 0x00014796
		// (set) Token: 0x060002EC RID: 748 RVA: 0x000165A3 File Offset: 0x000147A3
		public float PullJointMaxForce
		{
			get
			{
				return this.pullJoint.MaxForce;
			}
			set
			{
				this.pullJoint.MaxForce = value;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060002ED RID: 749 RVA: 0x000165B1 File Offset: 0x000147B1
		// (set) Token: 0x060002EE RID: 750 RVA: 0x000165C0 File Offset: 0x000147C0
		public Vector2 PullJointWorldAnchorA
		{
			get
			{
				return this.pullJoint.WorldAnchorA;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					string str = "Attempted to set the anchor A of a limb's pull joint to an invalid value (";
					Vector2 vector = value;
					string errorMsg = str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorA:InvalidValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					return;
				}
				if (Vector2.DistanceSquared(this.SimPosition, value) > 2500f)
				{
					Vector2 diff = value - this.SimPosition;
					string[] array = new string[8];
					array[0] = "Attempted to move the anchor A of a limb's pull joint extremely far from the limb (diff: ";
					int num = 1;
					Vector2 vector = diff;
					array[num] = vector.ToString();
					array[2] = ", limb enabled: ";
					array[3] = this.body.Enabled.ToString();
					array[4] = ", simple physics enabled: ";
					array[5] = this.character.AnimController.SimplePhysicsEnabled.ToString();
					array[6] = ")\n";
					array[7] = Environment.StackTrace.CleanupStackTrace();
					string errorMsg2 = string.Concat(array);
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorA:ExcessiveValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
					return;
				}
				this.pullJoint.WorldAnchorA = value;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060002EF RID: 751 RVA: 0x000166C5 File Offset: 0x000148C5
		// (set) Token: 0x060002F0 RID: 752 RVA: 0x000166D4 File Offset: 0x000148D4
		public Vector2 PullJointWorldAnchorB
		{
			get
			{
				return this.pullJoint.WorldAnchorB;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					string str = "Attempted to set the anchor B of a limb's pull joint to an invalid value (";
					Vector2 vector = value;
					string errorMsg = str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace();
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorB:InvalidValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
					return;
				}
				if (Vector2.DistanceSquared(this.pullJoint.WorldAnchorA, value) > 2500f)
				{
					Vector2 diff = value - this.pullJoint.WorldAnchorA;
					string[] array = new string[8];
					array[0] = "Attempted to move the anchor B of a limb's pull joint extremely far from the limb (diff: ";
					int num = 1;
					Vector2 vector = diff;
					array[num] = vector.ToString();
					array[2] = ", limb enabled: ";
					array[3] = this.body.Enabled.ToString();
					array[4] = ", simple physics enabled: ";
					array[5] = this.character.AnimController.SimplePhysicsEnabled.ToString();
					array[6] = ")\n";
					array[7] = Environment.StackTrace.CleanupStackTrace();
					string errorMsg2 = string.Concat(array);
					GameAnalyticsManager.AddErrorEventOnce("Limb.SetPullJointAnchorB:ExcessiveValue", GameAnalyticsManager.ErrorSeverity.Error, errorMsg2);
					return;
				}
				this.pullJoint.WorldAnchorB = value;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x000167E3 File Offset: 0x000149E3
		public Vector2 PullJointLocalAnchorA
		{
			get
			{
				return this.pullJoint.LocalAnchorA;
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x000167F0 File Offset: 0x000149F0
		// (set) Token: 0x060002F3 RID: 755 RVA: 0x000167F8 File Offset: 0x000149F8
		public bool Removed { get; private set; }

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x00016801 File Offset: 0x00014A01
		// (set) Token: 0x060002F5 RID: 757 RVA: 0x00016809 File Offset: 0x00014A09
		public Rope AttachedRope { get; set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x00016812 File Offset: 0x00014A12
		public string Name
		{
			get
			{
				return this.Params.Name;
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060002F7 RID: 759 RVA: 0x0001681F File Offset: 0x00014A1F
		public bool IsDead
		{
			get
			{
				return this.character.IsDead;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060002F8 RID: 760 RVA: 0x0001682C File Offset: 0x00014A2C
		public float Health
		{
			get
			{
				return this.character.Health;
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x00016839 File Offset: 0x00014A39
		public float HealthPercentage
		{
			get
			{
				return this.character.HealthPercentage;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060002FA RID: 762 RVA: 0x00016846 File Offset: 0x00014A46
		public bool IsHuman
		{
			get
			{
				return this.character.IsHuman;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060002FB RID: 763 RVA: 0x00016854 File Offset: 0x00014A54
		public AIState AIState
		{
			get
			{
				EnemyAIController enemyAI = this.character.AIController as EnemyAIController;
				if (enemyAI == null)
				{
					return AIState.Idle;
				}
				return enemyAI.State;
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060002FC RID: 764 RVA: 0x0001687D File Offset: 0x00014A7D
		public bool IsFlipped
		{
			get
			{
				return this.character.AnimController.IsFlipped;
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060002FD RID: 765 RVA: 0x00016890 File Offset: 0x00014A90
		public bool CanBeSeveredAlive
		{
			get
			{
				if (this.character.IsHumanoid)
				{
					return false;
				}
				if (this == this.character.AnimController.MainLimb)
				{
					return false;
				}
				bool canBeSevered = this.Params.CanBeSeveredAlive;
				if (this.character.AnimController.CanWalk && !this.character.Params.Health.AllowSeveringLegs)
				{
					LimbType limbType = this.type;
					if (limbType - LimbType.LeftLeg <= 3 || limbType - LimbType.Legs <= 3)
					{
						return false;
					}
				}
				return canBeSevered;
			}
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060002FE RID: 766 RVA: 0x0001690D File Offset: 0x00014B0D
		// (set) Token: 0x060002FF RID: 767 RVA: 0x00016915 File Offset: 0x00014B15
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000300 RID: 768 RVA: 0x0001691E File Offset: 0x00014B1E
		public Dictionary<ActionType, List<StatusEffect>> StatusEffects
		{
			get
			{
				return this.statusEffects;
			}
		}

		// Token: 0x06000301 RID: 769 RVA: 0x00016928 File Offset: 0x00014B28
		public Limb(Ragdoll ragdoll, Character character, RagdollParams.LimbParams limbParams)
		{
			this.ragdoll = ragdoll;
			this.character = character;
			this.Params = limbParams;
			this.dir = Direction.Right;
			this.body = new PhysicsBody(limbParams, false);
			this.type = limbParams.Type;
			this.IgnoreCollisions = limbParams.IgnoreCollisions;
			this.body.UserData = this;
			this.pullJoint = new FixedMouseJoint(this.body.FarseerBody, ConvertUnits.ToSimUnits(limbParams.PullPos * this.Scale))
			{
				Enabled = false,
				MaxForce = 1000f * this.Mass
			};
			GameMain.World.Add(this.pullJoint);
			ContentXElement element = limbParams.Element;
			this.body.BodyType = BodyType.Dynamic;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "attack"))
				{
					if (!(a == "damagemodifier"))
					{
						if (a == "statuseffect")
						{
							StatusEffect statusEffect = StatusEffect.Load(subElement, character.Name + ", " + this.Name);
							if (statusEffect != null)
							{
								if (!this.statusEffects.ContainsKey(statusEffect.type))
								{
									this.statusEffects.Add(statusEffect.type, new List<StatusEffect>());
								}
								this.statusEffects[statusEffect.type].Add(statusEffect);
							}
						}
					}
					else
					{
						this.DamageModifiers.Add(new DamageModifier(subElement, character.Name, true));
					}
				}
				else
				{
					this.attack = new Attack(subElement, ((character == null) ? "null" : character.Name) + ", limb " + this.type.ToString());
					if (this.attack.DamageRange <= 0f)
					{
						switch (this.body.BodyShape)
						{
						case PhysicsBody.Shape.Circle:
							this.attack.DamageRange = this.body.Radius;
							break;
						case PhysicsBody.Shape.Rectangle:
							this.attack.DamageRange = new Vector2(this.body.Width / 2f, this.body.Height / 2f).Length();
							break;
						case PhysicsBody.Shape.Capsule:
							this.attack.DamageRange = this.body.Height / 2f + this.body.Radius;
							break;
						}
						this.attack.DamageRange = ConvertUnits.ToDisplayUnits(this.attack.DamageRange);
					}
					if (character != null && !character.VariantOf.IsEmpty)
					{
						XElement attackElement = character.Params.VariantFile.GetRootExcludingOverride().GetChildElement("attack", StringComparison.OrdinalIgnoreCase);
						if (attackElement != null)
						{
							this.attack.SetInitialDamageMultiplier(attackElement.GetAttributeFloat("damagemultiplier", 1f));
							this.attack.RangeMultiplier = attackElement.GetAttributeFloat("rangemultiplier", 1f);
							this.attack.ImpactMultiplier = attackElement.GetAttributeFloat("impactmultiplier", 1f);
						}
					}
				}
			}
			this.SerializableProperties = SerializableProperty.GetProperties(this);
			this.InitProjSpecific(element);
		}

		// Token: 0x06000302 RID: 770 RVA: 0x00016D38 File Offset: 0x00014F38
		private void InitProjSpecific(ContentXElement element)
		{
			this.LagCompensatedBody = new PhysicsBody(this.Params, false)
			{
				BodyType = BodyType.Static,
				CollisionCategories = Category.Cat10,
				CollidesWith = Category.None,
				UserData = this
			};
		}

		// Token: 0x06000303 RID: 771 RVA: 0x00016D6C File Offset: 0x00014F6C
		public void MoveToPos(Vector2 pos, float force, bool pullFromCenter = false)
		{
			Vector2 pullPos = this.body.SimPosition;
			if (!pullFromCenter)
			{
				pullPos = this.pullJoint.WorldAnchorA;
			}
			this.AnimTargetPos = pos;
			this.body.MoveToPos(pos, force, new Vector2?(pullPos));
		}

		// Token: 0x06000304 RID: 772 RVA: 0x00016DAE File Offset: 0x00014FAE
		public void MirrorPullJoint()
		{
			this.pullJoint.LocalAnchorA = new Vector2(-this.pullJoint.LocalAnchorA.X, this.pullJoint.LocalAnchorA.Y);
		}

		// Token: 0x06000305 RID: 773 RVA: 0x00016DE4 File Offset: 0x00014FE4
		public AttackResult AddDamage(Vector2 simPosition, float damage, float bleedingDamage, float burnDamage, bool playSound)
		{
			List<Affliction> afflictions = new List<Affliction>();
			if (damage > 0f)
			{
				afflictions.Add(AfflictionPrefab.InternalDamage.Instantiate(damage, null));
			}
			if (bleedingDamage > 0f)
			{
				afflictions.Add(AfflictionPrefab.Bleeding.Instantiate(bleedingDamage, null));
			}
			if (burnDamage > 0f)
			{
				afflictions.Add(AfflictionPrefab.Burn.Instantiate(burnDamage, null));
			}
			return this.AddDamage(simPosition, afflictions, playSound, 1f, 0f, null);
		}

		// Token: 0x06000306 RID: 774 RVA: 0x00016E5C File Offset: 0x0001505C
		public AttackResult AddDamage(Vector2 simPosition, IEnumerable<Affliction> afflictions, bool playSound, float damageMultiplier = 1f, float penetration = 0f, Character attacker = null)
		{
			this.appliedDamageModifiers.Clear();
			this.afflictionsCopy.Clear();
			foreach (Affliction affliction in afflictions)
			{
				this.tempModifiers.Clear();
				Affliction newAffliction = affliction;
				float random = Rand.Value(Rand.RandSync.Unsynced);
				bool foundMatchingModifier = false;
				bool applyAffliction = true;
				foreach (DamageModifier damageModifier in this.DamageModifiers)
				{
					if (damageModifier.MatchesAffliction(affliction))
					{
						foundMatchingModifier = true;
						if (random > affliction.Probability * damageModifier.ProbabilityMultiplier)
						{
							applyAffliction = false;
						}
						else if (this.SectorHit(damageModifier.ArmorSectorInRadians, simPosition))
						{
							this.tempModifiers.Add(damageModifier);
						}
					}
				}
				foreach (WearableSprite wearable in this.WearingItems)
				{
					foreach (DamageModifier damageModifier2 in wearable.WearableComponent.DamageModifiers)
					{
						if (damageModifier2.MatchesAffliction(affliction))
						{
							foundMatchingModifier = true;
							if (random > affliction.Probability * damageModifier2.ProbabilityMultiplier)
							{
								applyAffliction = false;
							}
							else if (this.SectorHit(damageModifier2.ArmorSectorInRadians, simPosition))
							{
								this.tempModifiers.Add(damageModifier2);
							}
						}
					}
				}
				if (foundMatchingModifier || random <= affliction.Probability)
				{
					float finalDamageModifier = affliction.AffectedByAttackMultipliers ? damageMultiplier : 1f;
					if (this.character.EmpVulnerability > 0f && affliction.Prefab.AfflictionType == AfflictionPrefab.EMPType)
					{
						finalDamageModifier *= this.character.EmpVulnerability;
					}
					if (!this.character.Params.Health.PoisonImmunity && (affliction.Prefab.AfflictionType == AfflictionPrefab.PoisonType || affliction.Prefab.AfflictionType == AfflictionPrefab.ParalysisType))
					{
						finalDamageModifier *= this.character.PoisonVulnerability;
					}
					foreach (DamageModifier damageModifier3 in this.tempModifiers)
					{
						float damageModifierValue = damageModifier3.DamageMultiplier;
						if (damageModifier3.DeflectProjectiles && damageModifierValue < 1f)
						{
							damageModifierValue = MathHelper.Lerp(damageModifierValue, 1f, penetration);
						}
						finalDamageModifier *= damageModifierValue;
					}
					if (affliction.MultiplyByMaxVitality)
					{
						finalDamageModifier *= this.character.MaxVitality / 100f;
					}
					if (!MathUtils.NearlyEqual(finalDamageModifier, 1f, 0.0001f))
					{
						newAffliction = affliction.CreateMultiplied(finalDamageModifier, affliction);
					}
					else
					{
						newAffliction.SetStrength(affliction.NonClampedStrength);
					}
					if (attacker != null)
					{
						AbilityAfflictionCharacter abilityAfflictionCharacter = new AbilityAfflictionCharacter(newAffliction, this.character);
						attacker.CheckTalents(AbilityEffectType.OnAddDamageAffliction, abilityAfflictionCharacter);
						newAffliction = abilityAfflictionCharacter.Affliction;
					}
					if (applyAffliction)
					{
						this.afflictionsCopy.Add(newAffliction);
						Affliction affliction3 = newAffliction;
						if (affliction3.Source == null)
						{
							affliction3.Source = attacker;
						}
					}
					this.appliedDamageModifiers.AddRange(this.tempModifiers);
				}
			}
			AttackResult result = new AttackResult(this.afflictionsCopy, this, this.appliedDamageModifiers);
			if (result.Afflictions.None(null))
			{
			}
			float bleedingDamage = 0f;
			if (this.character.CharacterHealth.DoesBleed)
			{
				foreach (Affliction affliction2 in result.Afflictions)
				{
					if (affliction2 is AfflictionBleeding)
					{
						bleedingDamage += affliction2.GetVitalityDecrease(this.character.CharacterHealth);
					}
				}
				if (bleedingDamage > 0f)
				{
					float bloodDecalSize = MathHelper.Clamp(bleedingDamage / 5f, 0.1f, 1f);
					if (this.character.CurrentHull != null && !string.IsNullOrEmpty(this.character.BloodDecalName))
					{
						this.character.CurrentHull.AddDecal(this.character.BloodDecalName, this.WorldPosition, MathHelper.Clamp(bloodDecalSize, 0.5f, 1f), false, null);
					}
				}
			}
			return result;
		}

		// Token: 0x06000307 RID: 775 RVA: 0x00017344 File Offset: 0x00015544
		public bool SectorHit(Vector2 armorSector, Vector2 simPosition)
		{
			if (armorSector == Vector2.Zero)
			{
				return false;
			}
			if (Math.Abs(armorSector.Y - armorSector.X) >= 6.2831855f)
			{
				return true;
			}
			float rotation = this.body.TransformedRotation;
			float offset = (1.5707964f - MathUtils.GetMidAngle(armorSector.X, armorSector.Y)) * this.Dir;
			float hitAngle = VectorExtensions.Forward(rotation + offset, 1f).Angle(this.SimPosition - simPosition);
			float sectorSize = this.GetArmorSectorSize(armorSector);
			return hitAngle < sectorSize / 2f;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x000173D7 File Offset: 0x000155D7
		protected float GetArmorSectorSize(Vector2 armorSector)
		{
			return Math.Abs(armorSector.X - armorSector.Y);
		}

		// Token: 0x06000309 RID: 777 RVA: 0x000173EC File Offset: 0x000155EC
		public void Update(float deltaTime)
		{
			this.UpdateProjSpecific(deltaTime);
			this.ApplyStatusEffects(ActionType.Always, deltaTime);
			this.ApplyStatusEffects(ActionType.OnActive, deltaTime);
			if (this.InWater)
			{
				this.body.ApplyWaterForces();
			}
			if (this.isSevered)
			{
				this.severedFadeOutTimer += deltaTime;
				if (this.severedFadeOutTimer >= this.SeveredFadeOutTime)
				{
					this.body.Enabled = false;
				}
				else if (this.character.CurrentHull == null && Hull.FindHull(this.WorldPosition, null, true, true) != null)
				{
					this.severedFadeOutTimer = this.SeveredFadeOutTime;
				}
			}
			else if (!this.IsDead && (this.character.IsPlayer || this.character.AIState != AIState.PlayDead))
			{
				if (this.Params.BlinkFrequency > 0f)
				{
					if (this.BlinkTimer > -this.TotalBlinkDurationOut)
					{
						this.BlinkTimer -= deltaTime;
					}
					else
					{
						this.BlinkTimer = this.Params.BlinkFrequency;
					}
				}
				if (this.reEnableTimer > 0f)
				{
					this.reEnableTimer -= deltaTime;
				}
				else if (this.reEnableTimer > -1f)
				{
					this.ReEnable();
				}
			}
			Attack attack = this.attack;
			if (attack == null)
			{
				return;
			}
			attack.UpdateCoolDown(deltaTime);
		}

		// Token: 0x0600030A RID: 778 RVA: 0x00017538 File Offset: 0x00015738
		public void HideAndDisable(float duration = 0f, bool ignoreCollisions = true)
		{
			if (this.Hidden || this.Disabled)
			{
				return;
			}
			this.temporarilyDisabled = true;
			this.Hidden = true;
			this.Disabled = true;
			this.originalIgnoreCollisions = this.IgnoreCollisions;
			this.IgnoreCollisions = ignoreCollisions;
			if (duration > 0f)
			{
				this.reEnableTimer = duration;
			}
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0001758D File Offset: 0x0001578D
		public void ReEnable()
		{
			if (!this.temporarilyDisabled)
			{
				return;
			}
			this.temporarilyDisabled = false;
			this.Hidden = false;
			this.Disabled = false;
			this.IgnoreCollisions = this.originalIgnoreCollisions;
			this.reEnableTimer = -1f;
		}

		// Token: 0x0600030C RID: 780 RVA: 0x000175C4 File Offset: 0x000157C4
		private void UpdateProjSpecific(float deltaTime)
		{
			if (GameMain.Server == null)
			{
				return;
			}
			this.MemState.Enqueue(new PosInfo(this.body.SimPosition, new float?(this.body.Rotation), this.body.LinearVelocity, new float?(this.body.AngularVelocity), (float)Timing.TotalTime));
			while (this.MemState.Any<PosInfo>() && (double)this.MemState.Peek().Timestamp < Timing.TotalTime - (double)GameMain.Server.ServerSettings.MaxLagCompensationSeconds)
			{
				this.MemState.Dequeue();
			}
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0001766C File Offset: 0x0001586C
		public bool UpdateAttack(float deltaTime, Vector2 attackSimPos, IDamageable damageTarget, out AttackResult attackResult, float distance = -1f, Limb targetLimb = null)
		{
			attackResult = default(AttackResult);
			Vector2 simPos = this.ragdoll.SimplePhysicsEnabled ? this.character.SimPosition : this.SimPosition;
			float dist = (distance > -1f) ? distance : ConvertUnits.ToDisplayUnits(Vector2.Distance(simPos, attackSimPos));
			bool wasRunning = this.attack.IsRunning;
			this.attack.UpdateAttackTimer(deltaTime, this.character);
			if (this.attack.Blink)
			{
				if (this.attack.ForceOnLimbIndices != null && this.attack.ForceOnLimbIndices.Any<int>())
				{
					using (List<int>.Enumerator enumerator = this.attack.ForceOnLimbIndices.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int limbIndex = enumerator.Current;
							if (limbIndex >= 0 && limbIndex < this.character.AnimController.Limbs.Length)
							{
								Limb limb = this.character.AnimController.Limbs[limbIndex];
								if (!limb.IsSevered)
								{
									limb.Blink();
								}
							}
						}
						goto IL_10F;
					}
				}
				this.Blink();
			}
			IL_10F:
			bool wasHit = false;
			if (damageTarget != null)
			{
				HitDetection hitDetectionType = this.attack.HitDetectionType;
				if (hitDetectionType != HitDetection.Distance)
				{
					if (hitDetectionType == HitDetection.Contact)
					{
						this.contactBodies.Clear();
						Character targetCharacter = damageTarget as Character;
						if (targetCharacter != null)
						{
							foreach (Limb limb2 in targetCharacter.AnimController.Limbs)
							{
								if (!limb2.IsSevered)
								{
									PhysicsBody physicsBody = limb2.body;
									if (((physicsBody != null) ? physicsBody.FarseerBody : null) != null)
									{
										this.contactBodies.Add(limb2.body.FarseerBody);
									}
								}
							}
						}
						else
						{
							Structure targetStructure = damageTarget as Structure;
							if (targetStructure != null)
							{
								if (this.character.Submarine == null && targetStructure.Submarine != null)
								{
									this.contactBodies.Add(targetStructure.Submarine.PhysicsBody.FarseerBody);
								}
								else
								{
									this.contactBodies.AddRange(targetStructure.Bodies);
								}
							}
							else if (damageTarget is Item)
							{
								Item targetItem = damageTarget as Item;
								PhysicsBody physicsBody2 = targetItem.body;
								if (((physicsBody2 != null) ? physicsBody2.FarseerBody : null) != null)
								{
									this.contactBodies.Add(targetItem.body.FarseerBody);
								}
							}
						}
						ContactEdge contactEdge;
						for (contactEdge = this.body.FarseerBody.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
						{
							if (contactEdge.Contact != null && contactEdge.Contact.IsTouching && this.contactBodies.Any(delegate(Body b)
							{
								Fixture fixtureA = contactEdge.Contact.FixtureA;
								if (b != ((fixtureA != null) ? fixtureA.Body : null))
								{
									Fixture fixtureB = contactEdge.Contact.FixtureB;
									return b == ((fixtureB != null) ? fixtureB.Body : null);
								}
								return true;
							}))
							{
								Body structureBody = this.contactBodies.LastOrDefault<Body>();
								wasHit = true;
								break;
							}
						}
					}
				}
				else if (dist < this.attack.DamageRange)
				{
					Vector2 rayStart = simPos;
					Vector2 rayEnd = attackSimPos;
					if (this.Submarine == null)
					{
						ISpatialEntity spatialEntity = damageTarget as ISpatialEntity;
						if (spatialEntity != null && spatialEntity.Submarine != null)
						{
							rayStart -= spatialEntity.Submarine.SimPosition;
							rayEnd -= spatialEntity.Submarine.SimPosition;
						}
					}
					Body structureBody = Submarine.CheckVisibility(rayStart, rayEnd, false, false, true, true, true, null);
					Item i = damageTarget as Item;
					if (i != null && i.GetComponent<Door>() != null)
					{
						wasHit = true;
					}
					else
					{
						Structure wall = damageTarget as Structure;
						if (wall != null && structureBody != null)
						{
							if (!(structureBody.UserData is Structure))
							{
								Submarine sub = structureBody.UserData as Submarine;
								if (sub == null || sub != wall.Submarine)
								{
									goto IL_20C;
								}
							}
							wasHit = true;
							goto IL_3A1;
						}
						IL_20C:
						wasHit = (structureBody == null);
					}
				}
			}
			IL_3A1:
			if (wasHit)
			{
				wasHit = (damageTarget != null);
			}
			if (wasHit || this.attack.HitDetectionType == HitDetection.None)
			{
				if (this.character == Character.Controlled || GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
				{
					this.ExecuteAttack(damageTarget, targetLimb, out attackResult);
				}
				if (Timing.TotalTime > this.lastExecuteAttackEventTime + 0.5)
				{
					GameMain.NetworkMember.CreateEntityEvent(this.character, new Character.ExecuteAttackEventData(this, damageTarget, targetLimb, attackSimPos));
					this.lastExecuteAttackEventTime = Timing.TotalTime;
				}
			}
			Vector2 diff = attackSimPos - this.SimPosition;
			bool applyForces = !this.attack.ApplyForcesOnlyOnce || !wasRunning;
			if (applyForces)
			{
				if (this.attack.ForceOnLimbIndices != null && this.attack.ForceOnLimbIndices.Count > 0)
				{
					using (List<int>.Enumerator enumerator2 = this.attack.ForceOnLimbIndices.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							int limbIndex2 = enumerator2.Current;
							if (limbIndex2 >= 0 && limbIndex2 < this.character.AnimController.Limbs.Length)
							{
								Limb limb3 = this.character.AnimController.Limbs[limbIndex2];
								if (!limb3.IsSevered)
								{
									diff = attackSimPos - limb3.SimPosition;
									if (!(diff == Vector2.Zero))
									{
										limb3.body.ApplyTorque(limb3.Mass * this.character.AnimController.Dir * this.attack.Torque * limb3.Params.AttackForceMultiplier);
										Vector2 forcePos = (limb3.pullJoint == null) ? limb3.body.SimPosition : limb3.pullJoint.WorldAnchorA;
										limb3.body.ApplyLinearImpulse(limb3.Mass * this.attack.Force * limb3.Params.AttackForceMultiplier * Vector2.Normalize(diff), forcePos, 64f);
									}
								}
							}
						}
						goto IL_662;
					}
				}
				if (diff != Vector2.Zero)
				{
					this.body.ApplyTorque(this.Mass * this.character.AnimController.Dir * this.attack.Torque * this.Params.AttackForceMultiplier);
					Vector2 forcePos2 = (this.pullJoint == null) ? this.body.SimPosition : this.pullJoint.WorldAnchorA;
					this.body.ApplyLinearImpulse(this.Mass * this.attack.Force * this.Params.AttackForceMultiplier * Vector2.Normalize(diff), forcePos2, 64f);
				}
			}
			IL_662:
			Vector2 forceWorld = this.attack.CalculateAttackPhase(this.attack.RootTransitionEasing);
			forceWorld.X *= this.character.AnimController.Dir;
			this.character.AnimController.MainLimb.body.ApplyLinearImpulse(this.character.Mass * forceWorld, this.character.SimPosition, 64f);
			if (!this.attack.IsRunning && !this.attack.Ranged && Vector2.DistanceSquared(this.character.AnimController.Collider.SimPosition, this.character.AnimController.MainLimb.body.SimPosition) > 0.010000001f)
			{
				this.character.AnimController.Collider.SetTransformIgnoreContacts(this.character.AnimController.MainLimb.body.SimPosition, this.character.AnimController.Collider.Rotation, true);
			}
			return wasHit;
		}

		// Token: 0x0600030E RID: 782 RVA: 0x00017E20 File Offset: 0x00016020
		public void ExecuteAttack(IDamageable damageTarget, Limb targetLimb, out AttackResult attackResult)
		{
			bool playSound = false;
			this.attack.ResetDamageMultiplier();
			this.attack.DamageMultiplier *= 1f + this.character.GetStatValue(this.attack.Ranged ? StatTypes.NaturalRangedAttackMultiplier : StatTypes.NaturalMeleeAttackMultiplier, true);
			if (damageTarget is Character && targetLimb != null)
			{
				attackResult = this.attack.DoDamageToLimb(this.character, targetLimb, this.WorldPosition, 1f, playSound, this.body, this);
			}
			else
			{
				Item targetItem = damageTarget as Item;
				if (targetItem != null && !targetItem.Prefab.DamagedByMonsters)
				{
					attackResult = default(AttackResult);
				}
				else
				{
					attackResult = this.attack.DoDamage(this.character, damageTarget, this.WorldPosition, 1f, playSound, this.body, this);
				}
			}
			this.attack.ResetAttackTimer();
			this.attack.SetCoolDown(!this.character.IsPlayer);
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x0600030F RID: 783 RVA: 0x00017F18 File Offset: 0x00016118
		public bool IsStuck
		{
			get
			{
				return this.attachJoint != null;
			}
		}

		// Token: 0x06000310 RID: 784 RVA: 0x00017F24 File Offset: 0x00016124
		private void StickTo(Body target, Vector2 from, Vector2 to)
		{
			if (this.attachJoint != null)
			{
				if (this.attachJoint.BodyB == target)
				{
					return;
				}
				this.Release();
			}
			if (!this.ragdoll.IsStuck)
			{
				PhysicsBody mainLimbBody = this.ragdoll.MainLimb.body;
				Body colliderBody = this.ragdoll.Collider.FarseerBody;
				Vector2 mainLimbLocalFront = mainLimbBody.GetLocalFront(new float?(this.ragdoll.MainLimb.Params.GetSpriteOrientation()));
				if (this.Dir < 0f)
				{
					mainLimbLocalFront.X = -mainLimbLocalFront.X;
				}
				Vector2 mainLimbFront = mainLimbBody.FarseerBody.GetWorldPoint(mainLimbLocalFront);
				colliderBody.SetTransform(mainLimbBody.SimPosition, mainLimbBody.Rotation);
				this.colliderJoint = new WeldJoint(colliderBody, mainLimbBody.FarseerBody, mainLimbFront, mainLimbFront, true)
				{
					KinematicBodyB = true,
					CollideConnected = false
				};
				GameMain.World.Add(this.colliderJoint);
			}
			this.attachJoint = new WeldJoint(this.body.FarseerBody, target, from, to, true)
			{
				FrequencyHz = 1f,
				DampingRatio = 0.5f,
				KinematicBodyB = true,
				CollideConnected = false
			};
			GameMain.World.Add(this.attachJoint);
		}

		// Token: 0x06000311 RID: 785 RVA: 0x0001805C File Offset: 0x0001625C
		public void Release()
		{
			if (!this.IsStuck)
			{
				return;
			}
			GameMain.World.Remove(this.attachJoint);
			this.attachJoint = null;
			if (this.colliderJoint != null)
			{
				GameMain.World.Remove(this.colliderJoint);
				this.colliderJoint = null;
			}
		}

		// Token: 0x06000312 RID: 786 RVA: 0x000180A8 File Offset: 0x000162A8
		public void ApplyStatusEffects(ActionType actionType, float deltaTime)
		{
			List<StatusEffect> statusEffectList;
			if (!this.statusEffects.TryGetValue(actionType, out statusEffectList))
			{
				return;
			}
			foreach (StatusEffect statusEffect in statusEffectList)
			{
				if (!statusEffect.ShouldWaitForInterval(this.character, deltaTime))
				{
					statusEffect.sourceBody = this.body;
					if (statusEffect.type != ActionType.OnDamaged || (statusEffect.HasRequiredAfflictions(this.character.LastDamage) && (!statusEffect.OnlyWhenDamagedByPlayer || (this.character.LastAttacker != null && this.character.LastAttacker.IsPlayer))))
					{
						if (statusEffect.HasTargetType(StatusEffect.TargetType.NearbyItems) || statusEffect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
						{
							this.targets.Clear();
							statusEffect.AddNearbyTargets(this.WorldPosition, this.targets);
							statusEffect.Apply(actionType, deltaTime, this.character, this.targets, null);
						}
						else if (statusEffect.targetLimbs != null)
						{
							LimbType[] targetLimbs = statusEffect.targetLimbs;
							for (int i = 0; i < targetLimbs.Length; i++)
							{
								LimbType limbType = targetLimbs[i];
								if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
								{
									foreach (Limb limb in this.ragdoll.Limbs)
									{
										if (!limb.IsSevered && limb.type == limbType)
										{
											Limb.<ApplyStatusEffects>g__ApplyToLimb|182_0(actionType, deltaTime, statusEffect, this.character, limb);
										}
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.Limb) || statusEffect.HasTargetType(StatusEffect.TargetType.Character) || statusEffect.HasTargetType(StatusEffect.TargetType.This))
								{
									Limb limb2 = this.ragdoll.GetLimb(limbType, true, false, false);
									if (limb2 != null)
									{
										Limb.<ApplyStatusEffects>g__ApplyToLimb|182_0(actionType, deltaTime, statusEffect, this.character, limb2);
									}
								}
								else if (statusEffect.HasTargetType(StatusEffect.TargetType.LastLimb))
								{
									Limb limb3 = this.ragdoll.Limbs.LastOrDefault((Limb l) => l.type == limbType && !l.IsSevered && !l.Hidden);
									if (limb3 != null)
									{
										Limb.<ApplyStatusEffects>g__ApplyToLimb|182_0(actionType, deltaTime, statusEffect, this.character, limb3);
									}
								}
							}
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.AllLimbs))
						{
							foreach (Limb limb4 in this.ragdoll.Limbs)
							{
								if (!limb4.IsSevered)
								{
									Limb.<ApplyStatusEffects>g__ApplyToLimb|182_0(actionType, deltaTime, statusEffect, this.character, limb4);
								}
							}
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.Character))
						{
							statusEffect.Apply(actionType, deltaTime, this.character, this.character, new Vector2?(this.WorldPosition));
						}
						else if (statusEffect.HasTargetType(StatusEffect.TargetType.This) || statusEffect.HasTargetType(StatusEffect.TargetType.Limb))
						{
							Limb.<ApplyStatusEffects>g__ApplyToLimb|182_0(actionType, deltaTime, statusEffect, this.character, this);
						}
					}
				}
			}
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000313 RID: 787 RVA: 0x000183A0 File Offset: 0x000165A0
		// (set) Token: 0x06000314 RID: 788 RVA: 0x000183A8 File Offset: 0x000165A8
		public float BlinkTimer { get; private set; }

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000315 RID: 789 RVA: 0x000183B1 File Offset: 0x000165B1
		// (set) Token: 0x06000316 RID: 790 RVA: 0x000183B9 File Offset: 0x000165B9
		public float BlinkPhase { get; set; }

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000317 RID: 791 RVA: 0x000183C2 File Offset: 0x000165C2
		private float TotalBlinkDurationOut
		{
			get
			{
				return this.Params.BlinkDurationOut + this.Params.BlinkHoldTime;
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x000183DB File Offset: 0x000165DB
		public void Blink()
		{
			this.BlinkTimer = -this.TotalBlinkDurationOut;
		}

		// Token: 0x06000319 RID: 793 RVA: 0x000183EC File Offset: 0x000165EC
		public void UpdateBlink(float deltaTime, float referenceRotation)
		{
			if (this.BlinkTimer <= -this.TotalBlinkDurationOut)
			{
				if (!this.FreezeBlinkState)
				{
					this.BlinkPhase = this.Params.BlinkDurationIn;
				}
				this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationOut) * this.Dir, this.Mass * this.Params.BlinkForce, true);
				return;
			}
			if (!this.FreezeBlinkState)
			{
				this.BlinkPhase -= deltaTime;
			}
			if (this.BlinkPhase > 0f)
			{
				float t = ToolBox.GetEasing(this.Params.BlinkTransitionIn, MathUtils.InverseLerp(1f, 0f, this.BlinkPhase / this.Params.BlinkDurationIn));
				this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationIn) * this.Dir, this.Mass * this.Params.BlinkForce * t, true);
				bool useTextureOffsetForBlinking = this.Params.UseTextureOffsetForBlinking;
				return;
			}
			if (Math.Abs(this.BlinkPhase) < this.Params.BlinkHoldTime)
			{
				this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationIn) * this.Dir, this.Mass * this.Params.BlinkForce, true);
				return;
			}
			float t2 = ToolBox.GetEasing(this.Params.BlinkTransitionOut, MathUtils.InverseLerp(0f, 1f, (-this.BlinkPhase - this.Params.BlinkHoldTime) / this.Params.BlinkDurationOut));
			this.body.SmoothRotate(referenceRotation + MathHelper.ToRadians(this.Params.BlinkRotationOut) * this.Dir, this.Mass * this.Params.BlinkForce * t2, true);
			bool useTextureOffsetForBlinking2 = this.Params.UseTextureOffsetForBlinking;
		}

		// Token: 0x0600031A RID: 794 RVA: 0x000185CB File Offset: 0x000167CB
		public IEnumerable<LimbJoint> GetConnectedJoints()
		{
			return from j in this.ragdoll.LimbJoints
			where !j.IsSevered && (j.LimbA == this || j.LimbB == this)
			select j;
		}

		// Token: 0x0600031B RID: 795 RVA: 0x000185EC File Offset: 0x000167EC
		public IEnumerable<Limb> GetConnectedLimbs()
		{
			IEnumerable<LimbJoint> connectedJoints = this.GetConnectedJoints();
			HashSet<Limb> connectedLimbs = new HashSet<Limb>();
			foreach (Limb limb in this.ragdoll.Limbs)
			{
				IEnumerable<LimbJoint> otherJoints = limb.GetConnectedJoints();
				foreach (LimbJoint connectedJoint in connectedJoints)
				{
					if (otherJoints.Contains(connectedJoint))
					{
						connectedLimbs.Add(limb);
					}
				}
			}
			return connectedLimbs;
		}

		// Token: 0x0600031C RID: 796 RVA: 0x00018680 File Offset: 0x00016880
		public void Remove()
		{
			this.ragdoll.SubtractMass(this);
			PhysicsBody physicsBody = this.body;
			if (physicsBody != null)
			{
				physicsBody.Remove();
			}
			this.body = null;
			if (this.pullJoint != null)
			{
				if (GameMain.World.JointList.Contains(this.pullJoint))
				{
					GameMain.World.Remove(this.pullJoint);
				}
				this.pullJoint = null;
			}
			this.Release();
			this.RemoveProjSpecific();
			this.Removed = true;
		}

		// Token: 0x0600031D RID: 797 RVA: 0x000186FA File Offset: 0x000168FA
		private void RemoveProjSpecific()
		{
			this.LagCompensatedBody.Remove();
		}

		// Token: 0x0600031E RID: 798 RVA: 0x00018707 File Offset: 0x00016907
		public void LoadParams()
		{
			this.pullJoint.LocalAnchorA = ConvertUnits.ToSimUnits(this.Params.PullPos * this.Scale);
		}

		// Token: 0x0600031F RID: 799 RVA: 0x00018730 File Offset: 0x00016930
		[CompilerGenerated]
		internal static void <ApplyStatusEffects>g__ApplyToLimb|182_0(ActionType actionType, float deltaTime, StatusEffect statusEffect, Character character, Limb limb)
		{
			statusEffect.sourceBody = limb.body;
			statusEffect.Apply(actionType, deltaTime, character, limb, null);
		}

		// Token: 0x0400014F RID: 335
		public readonly Character character;

		// Token: 0x04000150 RID: 336
		public readonly Ragdoll ragdoll;

		// Token: 0x04000151 RID: 337
		public readonly RagdollParams.LimbParams Params;

		// Token: 0x04000152 RID: 338
		public PhysicsBody body;

		// Token: 0x04000153 RID: 339
		public Hull Hull;

		// Token: 0x04000155 RID: 341
		private FixedMouseJoint pullJoint;

		// Token: 0x04000156 RID: 342
		public readonly LimbType type;

		// Token: 0x04000157 RID: 343
		private bool ignoreCollisions;

		// Token: 0x04000158 RID: 344
		private bool isSevered;

		// Token: 0x04000159 RID: 345
		private float severedFadeOutTimer;

		// Token: 0x0400015A RID: 346
		private Vector2? mouthPos;

		// Token: 0x0400015B RID: 347
		public readonly Attack attack;

		// Token: 0x0400015D RID: 349
		private Direction dir;

		// Token: 0x0400015E RID: 350
		public Vector2 DebugTargetPos;

		// Token: 0x0400015F RID: 351
		public Vector2 DebugRefPos;

		// Token: 0x04000160 RID: 352
		private bool _hidden;

		// Token: 0x04000163 RID: 355
		private float _alpha = 1f;

		// Token: 0x04000164 RID: 356
		public readonly List<WearableSprite> WearingItems = new List<WearableSprite>();

		// Token: 0x04000165 RID: 357
		public readonly List<WearableSprite> OtherWearables = new List<WearableSprite>();

		// Token: 0x04000169 RID: 361
		private readonly Dictionary<ActionType, List<StatusEffect>> statusEffects = new Dictionary<ActionType, List<StatusEffect>>();

		// Token: 0x0400016A RID: 362
		private readonly List<DamageModifier> appliedDamageModifiers = new List<DamageModifier>();

		// Token: 0x0400016B RID: 363
		private readonly List<DamageModifier> tempModifiers = new List<DamageModifier>();

		// Token: 0x0400016C RID: 364
		private readonly List<Affliction> afflictionsCopy = new List<Affliction>();

		// Token: 0x0400016D RID: 365
		private bool temporarilyDisabled;

		// Token: 0x0400016E RID: 366
		private float reEnableTimer = -1f;

		// Token: 0x0400016F RID: 367
		private bool originalIgnoreCollisions;

		// Token: 0x04000170 RID: 368
		private const double MinExecuteAttackEventInterval = 0.5;

		// Token: 0x04000171 RID: 369
		private double lastExecuteAttackEventTime;

		// Token: 0x04000172 RID: 370
		private readonly List<Body> contactBodies = new List<Body>();

		// Token: 0x04000173 RID: 371
		private WeldJoint attachJoint;

		// Token: 0x04000174 RID: 372
		private WeldJoint colliderJoint;

		// Token: 0x04000175 RID: 373
		private readonly List<ISerializableEntity> targets = new List<ISerializableEntity>();

		// Token: 0x04000178 RID: 376
		public bool FreezeBlinkState;
	}
}

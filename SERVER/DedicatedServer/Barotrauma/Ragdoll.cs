using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000A6 RID: 166
	internal abstract class Ragdoll
	{
		// Token: 0x170005C0 RID: 1472
		// (get) Token: 0x0600141E RID: 5150
		// (set) Token: 0x0600141F RID: 5151
		public abstract RagdollParams RagdollParams { get; protected set; }

		// Token: 0x170005C1 RID: 1473
		// (get) Token: 0x06001420 RID: 5152 RVA: 0x000B1BBE File Offset: 0x000AFDBE
		public Limb[] Limbs
		{
			get
			{
				if (this.limbs == null)
				{
					this.LogAccessedRemovedCharacterError();
					return Array.Empty<Limb>();
				}
				return this.limbs;
			}
		}

		// Token: 0x170005C2 RID: 1474
		// (get) Token: 0x06001421 RID: 5153 RVA: 0x000B1BDA File Offset: 0x000AFDDA
		public IEnumerable<Body> LimbBodies
		{
			get
			{
				return this.limbBodies;
			}
		}

		// Token: 0x170005C3 RID: 1475
		// (get) Token: 0x06001422 RID: 5154 RVA: 0x000B1BE2 File Offset: 0x000AFDE2
		public bool HasMultipleLimbsOfSameType
		{
			get
			{
				return this.limbs != null && this.limbs.Length > this.limbDictionary.Count;
			}
		}

		// Token: 0x170005C4 RID: 1476
		// (get) Token: 0x06001423 RID: 5155 RVA: 0x000B1C03 File Offset: 0x000AFE03
		// (set) Token: 0x06001424 RID: 5156 RVA: 0x000B1C0C File Offset: 0x000AFE0C
		public bool Frozen
		{
			get
			{
				return this.frozen;
			}
			set
			{
				if (this.frozen == value)
				{
					return;
				}
				this.frozen = value;
				this.Collider.FarseerBody.LinearDamping = (this.frozen ? 89.99999f : 0f);
				this.Collider.FarseerBody.AngularDamping = (this.frozen ? 89.99999f : 5f);
				this.Collider.FarseerBody.IgnoreGravity = this.frozen;
				if (this.frozen && this.MainLimb != null)
				{
					this.MainLimb.PullJointWorldAnchorB = this.MainLimb.SimPosition;
				}
			}
		}

		// Token: 0x170005C5 RID: 1477
		// (get) Token: 0x06001425 RID: 5157 RVA: 0x000B1CAE File Offset: 0x000AFEAE
		public Character Character
		{
			get
			{
				return this.character;
			}
		}

		// Token: 0x170005C6 RID: 1478
		// (get) Token: 0x06001426 RID: 5158 RVA: 0x000B1CB6 File Offset: 0x000AFEB6
		public bool OnGround
		{
			get
			{
				return this.onGround;
			}
		}

		// Token: 0x170005C7 RID: 1479
		// (get) Token: 0x06001427 RID: 5159 RVA: 0x000B1CBE File Offset: 0x000AFEBE
		public float ColliderHeightFromFloor
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.RagdollParams.ColliderHeightFromFloor) * this.RagdollParams.JointScale;
			}
		}

		// Token: 0x170005C8 RID: 1480
		// (get) Token: 0x06001428 RID: 5160 RVA: 0x000B1CDC File Offset: 0x000AFEDC
		public bool ColliderControlsMovement
		{
			get
			{
				return this.character.CanMove;
			}
		}

		// Token: 0x170005C9 RID: 1481
		// (get) Token: 0x06001429 RID: 5161 RVA: 0x000B1CE9 File Offset: 0x000AFEE9
		public bool IsStuck
		{
			get
			{
				return this.Limbs.Any((Limb l) => l.IsStuck);
			}
		}

		// Token: 0x170005CA RID: 1482
		// (get) Token: 0x0600142A RID: 5162 RVA: 0x000B1D15 File Offset: 0x000AFF15
		public PhysicsBody Collider
		{
			get
			{
				List<PhysicsBody> list = this.collider;
				if (list == null)
				{
					return null;
				}
				return list[this.colliderIndex];
			}
		}

		// Token: 0x0600142B RID: 5163 RVA: 0x000B1D30 File Offset: 0x000AFF30
		public bool TryGetCollider(int index, out PhysicsBody collider)
		{
			collider = null;
			bool result;
			try
			{
				List<PhysicsBody> list = this.collider;
				collider = ((list != null) ? list[index] : null);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x170005CB RID: 1483
		// (get) Token: 0x0600142C RID: 5164 RVA: 0x000B1D70 File Offset: 0x000AFF70
		// (set) Token: 0x0600142D RID: 5165 RVA: 0x000B1D78 File Offset: 0x000AFF78
		public int ColliderIndex
		{
			get
			{
				return this.colliderIndex;
			}
			set
			{
				if (value == this.colliderIndex || this.collider == null)
				{
					return;
				}
				if (value >= this.collider.Count || value < 0)
				{
					return;
				}
				if (this.collider[this.colliderIndex].Height < this.collider[value].Height)
				{
					Vector2 pos = this.collider[this.colliderIndex].SimPosition;
					pos.Y -= this.collider[this.colliderIndex].Height * this.ColliderHeightFromFloor;
					Vector2 pos2 = pos;
					pos2.Y += this.collider[value].Height * 1.1f;
					if (GameMain.World.RayCast(pos, pos2).Any((Fixture f) => f.CollisionCategories.HasFlag(Category.Cat1) && !(f.Body.UserData is Submarine)))
					{
						return;
					}
				}
				Vector2 pos3 = this.collider[this.colliderIndex].SimPosition;
				pos3.Y -= this.collider[this.colliderIndex].Height * 0.5f;
				pos3.Y += this.collider[value].Height * 0.5f;
				this.collider[value].SetTransformIgnoreContacts(pos3, this.collider[this.colliderIndex].Rotation, true);
				this.collider[value].LinearVelocity = this.collider[this.colliderIndex].LinearVelocity;
				this.collider[value].AngularVelocity = this.collider[this.colliderIndex].AngularVelocity;
				this.collider[value].Submarine = this.collider[this.colliderIndex].Submarine;
				this.collider[value].PhysEnabled = !this.frozen;
				this.collider[value].Enabled = !this.simplePhysicsEnabled;
				this.collider[this.colliderIndex].PhysEnabled = false;
				this.colliderIndex = value;
			}
		}

		// Token: 0x170005CC RID: 1484
		// (get) Token: 0x0600142E RID: 5166 RVA: 0x000B1FBD File Offset: 0x000B01BD
		public float FloorY
		{
			get
			{
				return this.floorY;
			}
		}

		// Token: 0x170005CD RID: 1485
		// (get) Token: 0x0600142F RID: 5167 RVA: 0x000B1FC5 File Offset: 0x000B01C5
		// (set) Token: 0x06001430 RID: 5168 RVA: 0x000B1FCD File Offset: 0x000B01CD
		public float Mass { get; private set; }

		// Token: 0x06001431 RID: 5169 RVA: 0x000B1FD6 File Offset: 0x000B01D6
		public void SubtractMass(Limb limb)
		{
			if (this.limbs.Contains(limb))
			{
				this.Mass -= limb.Mass;
			}
		}

		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x06001432 RID: 5170 RVA: 0x000B1FFC File Offset: 0x000B01FC
		public Limb MainLimb
		{
			get
			{
				Limb mainLimb = this.GetLimb(this.RagdollParams.MainLimb, true, false, false);
				if (!Ragdoll.<get_MainLimb>g__IsValid|74_0(mainLimb))
				{
					Limb torso = this.GetLimb(LimbType.Torso, true, false, false);
					Limb head = this.GetLimb(LimbType.Head, true, false, false);
					mainLimb = (torso ?? head);
					if (!Ragdoll.<get_MainLimb>g__IsValid|74_0(mainLimb))
					{
						mainLimb = this.Limbs.FirstOrDefault((Limb l) => Ragdoll.<get_MainLimb>g__IsValid|74_0(l));
					}
					if (mainLimb == null)
					{
						DebugConsole.ThrowError("Couldn't find a valid main limb. The limb can't be hidden nor be set to ignore collisions!", null, null, false, false);
						mainLimb = this.Limbs.FirstOrDefault<Limb>();
					}
				}
				return mainLimb;
			}
		}

		// Token: 0x170005CF RID: 1487
		// (get) Token: 0x06001433 RID: 5171 RVA: 0x000B2098 File Offset: 0x000B0298
		public Vector2 WorldPosition
		{
			get
			{
				if (this.character.Submarine != null)
				{
					return ConvertUnits.ToDisplayUnits(this.Collider.SimPosition) + this.character.Submarine.Position;
				}
				return ConvertUnits.ToDisplayUnits(this.Collider.SimPosition);
			}
		}

		// Token: 0x170005D0 RID: 1488
		// (get) Token: 0x06001434 RID: 5172 RVA: 0x000B20E8 File Offset: 0x000B02E8
		// (set) Token: 0x06001435 RID: 5173 RVA: 0x000B20F0 File Offset: 0x000B02F0
		public bool SimplePhysicsEnabled
		{
			get
			{
				return this.simplePhysicsEnabled;
			}
			set
			{
				if (value == this.simplePhysicsEnabled)
				{
					return;
				}
				this.simplePhysicsEnabled = value;
				foreach (Limb limb in this.Limbs)
				{
					if (!limb.IsSevered)
					{
						if (limb.body == null)
						{
							DebugConsole.ThrowError("Limb has no body! (" + ((this.character != null) ? this.character.Name : "Unknown character") + ", " + limb.type.ToString(), null, null, false, false);
						}
						else
						{
							limb.body.Enabled = !this.simplePhysicsEnabled;
						}
					}
				}
				foreach (LimbJoint joint in this.LimbJoints)
				{
					joint.Enabled = (!joint.IsSevered && !this.simplePhysicsEnabled);
				}
				if (!this.simplePhysicsEnabled)
				{
					foreach (Limb limb2 in this.Limbs)
					{
						if (!limb2.IsSevered && limb2.body.PhysEnabled)
						{
							limb2.body.SetTransformIgnoreContacts(this.Collider.SimPosition, this.Collider.Rotation, true);
							limb2.PullJointEnabled = false;
							limb2.PullJointWorldAnchorB = limb2.SimPosition;
						}
					}
				}
			}
		}

		// Token: 0x170005D1 RID: 1489
		// (get) Token: 0x06001436 RID: 5174 RVA: 0x000B224C File Offset: 0x000B044C
		// (set) Token: 0x06001437 RID: 5175 RVA: 0x000B2278 File Offset: 0x000B0478
		public Vector2 TargetMovement
		{
			get
			{
				Vector2? vector = this.overrideTargetMovement;
				if (vector == null)
				{
					return this.targetMovement;
				}
				return vector.GetValueOrDefault();
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.targetMovement.X = MathHelper.Clamp(value.X, -20f, 20f);
				this.targetMovement.Y = MathHelper.Clamp(value.Y, -20f, 20f);
			}
		}

		// Token: 0x170005D2 RID: 1490
		// (get) Token: 0x06001438 RID: 5176
		public abstract float? HeadPosition { get; }

		// Token: 0x170005D3 RID: 1491
		// (get) Token: 0x06001439 RID: 5177
		public abstract float? HeadAngle { get; }

		// Token: 0x170005D4 RID: 1492
		// (get) Token: 0x0600143A RID: 5178
		public abstract float? TorsoPosition { get; }

		// Token: 0x170005D5 RID: 1493
		// (get) Token: 0x0600143B RID: 5179
		public abstract float? TorsoAngle { get; }

		// Token: 0x170005D6 RID: 1494
		// (get) Token: 0x0600143C RID: 5180 RVA: 0x000B22D0 File Offset: 0x000B04D0
		public float ImpactTolerance
		{
			get
			{
				if (this.impactTolerance == null)
				{
					this.impactTolerance = new float?(this.RagdollParams.ImpactTolerance);
					if (this.character.Params.VariantFile != null)
					{
						XElement childElement = this.character.Params.VariantFile.GetRootExcludingOverride().GetChildElement("ragdoll", StringComparison.OrdinalIgnoreCase);
						float? tolerance = (childElement != null) ? new float?(childElement.GetAttributeFloat("impacttolerance", this.impactTolerance.Value)) : null;
						if (tolerance != null)
						{
							this.impactTolerance = tolerance;
						}
					}
				}
				return this.impactTolerance.Value;
			}
		}

		// Token: 0x170005D7 RID: 1495
		// (get) Token: 0x0600143D RID: 5181 RVA: 0x000B237A File Offset: 0x000B057A
		public bool Draggable
		{
			get
			{
				return this.RagdollParams.Draggable;
			}
		}

		// Token: 0x170005D8 RID: 1496
		// (get) Token: 0x0600143E RID: 5182 RVA: 0x000B2387 File Offset: 0x000B0587
		public CanEnterSubmarine CanEnterSubmarine
		{
			get
			{
				return this.RagdollParams.CanEnterSubmarine;
			}
		}

		// Token: 0x170005D9 RID: 1497
		// (get) Token: 0x0600143F RID: 5183 RVA: 0x000B2394 File Offset: 0x000B0594
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
		}

		// Token: 0x170005DA RID: 1498
		// (get) Token: 0x06001440 RID: 5184 RVA: 0x000B23AA File Offset: 0x000B05AA
		public Direction Direction
		{
			get
			{
				return this.dir;
			}
		}

		// Token: 0x170005DB RID: 1499
		// (get) Token: 0x06001441 RID: 5185 RVA: 0x000B23B2 File Offset: 0x000B05B2
		public bool InWater
		{
			get
			{
				return this.inWater;
			}
		}

		// Token: 0x170005DC RID: 1500
		// (get) Token: 0x06001442 RID: 5186 RVA: 0x000B23BA File Offset: 0x000B05BA
		public bool HeadInWater
		{
			get
			{
				return this.headInWater;
			}
		}

		// Token: 0x170005DD RID: 1501
		// (get) Token: 0x06001443 RID: 5187 RVA: 0x000B23C2 File Offset: 0x000B05C2
		// (set) Token: 0x06001444 RID: 5188 RVA: 0x000B23CC File Offset: 0x000B05CC
		public Hull CurrentHull
		{
			get
			{
				return this.currentHull;
			}
			set
			{
				if (value == this.currentHull)
				{
					return;
				}
				this.currentHull = value;
				Hull hull = this.currentHull;
				Submarine currSubmarine = (hull != null) ? hull.Submarine : null;
				foreach (Limb limb in this.Limbs)
				{
					if (!limb.IsSevered)
					{
						limb.body.Submarine = currSubmarine;
					}
				}
				this.Collider.Submarine = currSubmarine;
			}
		}

		// Token: 0x170005DE RID: 1502
		// (get) Token: 0x06001445 RID: 5189 RVA: 0x000B2436 File Offset: 0x000B0636
		// (set) Token: 0x06001446 RID: 5190 RVA: 0x000B243E File Offset: 0x000B063E
		public bool IgnorePlatforms { get; set; }

		// Token: 0x06001447 RID: 5191 RVA: 0x000B2448 File Offset: 0x000B0648
		public virtual void Recreate(RagdollParams ragdollParams = null)
		{
			if (this.IsFlipped)
			{
				this.Flip();
			}
			this.dir = Direction.Right;
			Dictionary<RagdollParams.LimbParams, List<WearableSprite>> items = null;
			if (ragdollParams != null)
			{
				this.RagdollParams = ragdollParams;
			}
			else
			{
				Limb[] array = this.limbs;
				Dictionary<RagdollParams.LimbParams, List<WearableSprite>> dictionary;
				if (array == null)
				{
					dictionary = null;
				}
				else
				{
					dictionary = array.ToDictionary((Limb l) => l.Params, (Limb l) => l.WearingItems);
				}
				items = dictionary;
			}
			XDocument variantFile = this.character.Params.VariantFile;
			if (variantFile != null)
			{
				this.RagdollParams.TryApplyVariantScale(variantFile);
			}
			foreach (RagdollParams.LimbParams limbParams in this.RagdollParams.Limbs)
			{
				if (!PhysicsBody.IsValidShape(limbParams.Radius, limbParams.Height, limbParams.Width))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 4);
					defaultInterpolatedStringHandler.AppendLiteral("Invalid collider dimensions (r: ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(limbParams.Radius);
					defaultInterpolatedStringHandler.AppendLiteral(", h: ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(limbParams.Height);
					defaultInterpolatedStringHandler.AppendLiteral(", w: ");
					defaultInterpolatedStringHandler.AppendFormatted<float>(limbParams.Width);
					defaultInterpolatedStringHandler.AppendLiteral(") on limb: ");
					defaultInterpolatedStringHandler.AppendFormatted(limbParams.Name);
					defaultInterpolatedStringHandler.AppendLiteral(". Fixing.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					limbParams.Radius = 10f;
				}
			}
			foreach (RagdollParams.ColliderParams colliderParams in this.RagdollParams.Colliders)
			{
				if (!PhysicsBody.IsValidShape(colliderParams.Radius, colliderParams.Height, colliderParams.Width))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(66, 4);
					defaultInterpolatedStringHandler2.AppendLiteral("Invalid collider dimensions (r: ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(colliderParams.Radius);
					defaultInterpolatedStringHandler2.AppendLiteral(", h: ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(colliderParams.Height);
					defaultInterpolatedStringHandler2.AppendLiteral(", w: ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(colliderParams.Width);
					defaultInterpolatedStringHandler2.AppendLiteral(") on collider: ");
					defaultInterpolatedStringHandler2.AppendFormatted(colliderParams.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(". Fixing.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					colliderParams.Radius = 10f;
				}
			}
			this.CreateColliders();
			this.CreateLimbs();
			this.CreateJoints();
			this.UpdateCollisionCategories();
			this.character.LoadHeadAttachments();
			if (items != null)
			{
				foreach (KeyValuePair<RagdollParams.LimbParams, List<WearableSprite>> kvp in items)
				{
					int id = kvp.Key.ID;
					if (id <= this.limbs.Length - 1)
					{
						Limb limb = this.limbs[id];
						List<WearableSprite> itemList = kvp.Value;
						limb.WearingItems.AddRange(itemList);
					}
				}
			}
			if (this.character.IsHusk && this.character.Params.UseHuskAppendage)
			{
				CharacterPrefab characterPrefab = CharacterPrefab.FindByFilePath(this.character.ConfigPath);
				ContentXElement contentXElement = (characterPrefab != null) ? characterPrefab.ConfigElement : null;
				ContentXElement contentXElement2 = null;
				if (contentXElement != contentXElement2)
				{
					ContentXElement mainElement = characterPrefab.ConfigElement;
					foreach (ContentXElement huskAppendage in mainElement.GetChildElements("huskappendage"))
					{
						if (!huskAppendage.GetAttributeBool("onlyfromafflictions", false))
						{
							Identifier afflictionIdentifier = huskAppendage.GetAttributeIdentifier("affliction", Identifier.Empty);
							AfflictionPrefab affliction;
							if (AfflictionPrefab.Prefabs.TryGet(afflictionIdentifier, out affliction))
							{
								AfflictionPrefabHusk matchingAffliction = affliction as AfflictionPrefabHusk;
								if (matchingAffliction != null)
								{
									AfflictionHusk.AttachHuskAppendage(this.character, matchingAffliction, this.character.SpeciesName, huskAppendage, this);
									continue;
								}
							}
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(84, 1);
							defaultInterpolatedStringHandler3.AppendLiteral("Could not find an affliction of type 'huskinfection' that matches the affliction '");
							defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(afflictionIdentifier);
							defaultInterpolatedStringHandler3.AppendLiteral("'!");
							DebugConsole.ThrowError(defaultInterpolatedStringHandler3.ToStringAndClear(), null, huskAppendage.ContentPackage, false, false);
						}
					}
				}
			}
		}

		// Token: 0x06001448 RID: 5192 RVA: 0x000B28CC File Offset: 0x000B0ACC
		public Ragdoll(Character character, string seed, RagdollParams ragdollParams = null)
		{
			Ragdoll.list.Add(this);
			this.character = character;
			this.Recreate(ragdollParams ?? this.RagdollParams);
		}

		// Token: 0x06001449 RID: 5193 RVA: 0x000B294C File Offset: 0x000B0B4C
		protected void CreateColliders()
		{
			List<PhysicsBody> list = this.collider;
			if (list != null)
			{
				list.ForEach(delegate(PhysicsBody c)
				{
					c.Remove();
				});
			}
			DebugConsole.Log("Creating colliders from " + this.RagdollParams.Name + ".");
			this.collider = new List<PhysicsBody>();
			foreach (RagdollParams.ColliderParams cParams in this.RagdollParams.Colliders)
			{
				if (!PhysicsBody.IsValidShape(cParams.Radius, cParams.Height, cParams.Width))
				{
					DebugConsole.ThrowError("Invalid collider dimensions: " + cParams.Name, null, null, false, false);
					break;
				}
				PhysicsBody body = new PhysicsBody(cParams, false);
				this.collider.Add(body);
				body.UserData = this.character;
				body.FarseerBody.OnCollision += this.OnLimbCollision;
				if (this.collider.Count > 1)
				{
					body.PhysEnabled = false;
				}
			}
		}

		// Token: 0x0600144A RID: 5194 RVA: 0x000B2A80 File Offset: 0x000B0C80
		protected void CreateJoints()
		{
			if (this.LimbJoints != null)
			{
				foreach (LimbJoint joint in this.LimbJoints)
				{
					if (GameMain.World.JointList.Contains(joint.Joint))
					{
						GameMain.World.Remove(joint.Joint);
					}
				}
			}
			DebugConsole.Log("Creating joints from " + this.RagdollParams.Name + ".");
			this.LimbJoints = new LimbJoint[this.RagdollParams.Joints.Count];
			this.RagdollParams.Joints.ForEach(delegate(RagdollParams.JointParams j)
			{
				this.AddJoint(j);
			});
			for (int i = 0; i < this.LimbJoints.Length; i++)
			{
				if (this.LimbJoints[i] == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Joint ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					defaultInterpolatedStringHandler.AppendLiteral(" null.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
			}
			this.UpdateCollisionCategories();
			this.SetInitialLimbPositions();
		}

		// Token: 0x0600144B RID: 5195 RVA: 0x000B2B90 File Offset: 0x000B0D90
		private void SetInitialLimbPositions()
		{
			foreach (LimbJoint joint in this.LimbJoints)
			{
				if (joint != null)
				{
					float angle = (joint.LowerLimit + joint.UpperLimit) / 2f;
					Limb limbB = joint.LimbB;
					if (limbB != null)
					{
						PhysicsBody body = limbB.body;
						if (body != null)
						{
							body.SetTransformIgnoreContacts(joint.WorldAnchorA - MathUtils.RotatePointAroundTarget(joint.LocalAnchorB, Vector2.Zero, joint.BodyA.Rotation + angle, true), joint.BodyA.Rotation + angle, true);
						}
					}
				}
			}
		}

		// Token: 0x0600144C RID: 5196 RVA: 0x000B2C24 File Offset: 0x000B0E24
		protected void CreateLimbs()
		{
			this.limbBodies.Clear();
			Limb[] array = this.limbs;
			if (array != null)
			{
				array.ForEach(delegate(Limb l)
				{
					l.Remove();
				});
			}
			this.Mass = 0f;
			DebugConsole.Log("Creating limbs from " + this.RagdollParams.Name + ".");
			this.limbDictionary = new Dictionary<LimbType, Limb>();
			this.limbs = new Limb[this.RagdollParams.Limbs.Count];
			this.RagdollParams.Limbs.ForEach(new Action<RagdollParams.LimbParams>(this.AddLimb));
			this.limbs.Contains(null);
		}

		// Token: 0x0600144D RID: 5197 RVA: 0x000B2CE5 File Offset: 0x000B0EE5
		public void SaveRagdoll(string fileNameWithoutExtension = null)
		{
			this.RagdollParams.Save(fileNameWithoutExtension);
		}

		// Token: 0x0600144E RID: 5198 RVA: 0x000B2CF4 File Offset: 0x000B0EF4
		public void ResetRagdoll()
		{
			this.RagdollParams.Reset(true);
			this.ResetJoints();
			this.ResetLimbs();
		}

		// Token: 0x0600144F RID: 5199 RVA: 0x000B2D0F File Offset: 0x000B0F0F
		public void ResetJoints()
		{
			this.LimbJoints.ForEach(delegate(LimbJoint j)
			{
				j.LoadParams();
			});
		}

		// Token: 0x06001450 RID: 5200 RVA: 0x000B2D3B File Offset: 0x000B0F3B
		public void ResetLimbs()
		{
			this.Limbs.ForEach(delegate(Limb l)
			{
				l.LoadParams();
			});
		}

		// Token: 0x06001451 RID: 5201 RVA: 0x000B2D68 File Offset: 0x000B0F68
		public void AddJoint(RagdollParams.JointParams jointParams)
		{
			Ragdoll.<>c__DisplayClass125_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.jointParams = jointParams;
			if (!this.<AddJoint>g__checkLimbIndex|125_0(CS$<>8__locals1.jointParams.Limb1, "Limb1", ref CS$<>8__locals1) || !this.<AddJoint>g__checkLimbIndex|125_0(CS$<>8__locals1.jointParams.Limb2, "Limb2", ref CS$<>8__locals1))
			{
				return;
			}
			LimbJoint joint = new LimbJoint(this.Limbs[CS$<>8__locals1.jointParams.Limb1], this.Limbs[CS$<>8__locals1.jointParams.Limb2], CS$<>8__locals1.jointParams, this);
			GameMain.World.Add(joint.Joint);
			for (int i = 0; i < this.LimbJoints.Length; i++)
			{
				if (this.LimbJoints[i] == null)
				{
					this.LimbJoints[i] = joint;
					return;
				}
			}
			Array.Resize<LimbJoint>(ref this.LimbJoints, this.LimbJoints.Length + 1);
			this.LimbJoints[this.LimbJoints.Length - 1] = joint;
		}

		// Token: 0x06001452 RID: 5202 RVA: 0x000B2E4C File Offset: 0x000B104C
		protected void AddLimb(RagdollParams.LimbParams limbParams)
		{
			if (limbParams.ID < 0 || limbParams.ID > 255)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(58, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Invalid limb params in limb \"");
				defaultInterpolatedStringHandler.AppendFormatted<LimbType>(limbParams.Type);
				defaultInterpolatedStringHandler.AppendLiteral("\". \"");
				defaultInterpolatedStringHandler.AppendFormatted<int>(limbParams.ID);
				defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid limb ID.");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			byte ID = Convert.ToByte(limbParams.ID);
			Limb limb = new Limb(this, this.character, limbParams);
			limb.body.FarseerBody.OnCollision += this.OnLimbCollision;
			if ((int)ID >= this.Limbs.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(117, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Failed to add a limb to the character \"");
				Character character = this.Character;
				defaultInterpolatedStringHandler2.AppendFormatted(((character != null) ? character.ConfigPath : null) ?? "null");
				defaultInterpolatedStringHandler2.AppendLiteral("\" (limb index ");
				defaultInterpolatedStringHandler2.AppendFormatted<byte>(ID);
				defaultInterpolatedStringHandler2.AppendLiteral(" out of bounds). The ragdoll file may be configured incorrectly.");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			this.limbBodies.Add(limb.body.FarseerBody);
			this.Limbs[(int)ID] = limb;
			this.Mass += limb.Mass;
			if (!this.limbDictionary.ContainsKey(limb.type))
			{
				this.limbDictionary.Add(limb.type, limb);
			}
		}

		// Token: 0x06001453 RID: 5203 RVA: 0x000B2FC4 File Offset: 0x000B11C4
		public void AddLimb(Limb limb)
		{
			if (this.Limbs.Contains(limb))
			{
				return;
			}
			limb.body.FarseerBody.OnCollision += this.OnLimbCollision;
			Array.Resize<Limb>(ref this.limbs, this.Limbs.Length + 1);
			this.Limbs[this.Limbs.Length - 1] = limb;
			this.limbBodies.Add(limb.body.FarseerBody);
			this.Mass += limb.Mass;
			if (!this.limbDictionary.ContainsKey(limb.type))
			{
				this.limbDictionary.Add(limb.type, limb);
			}
		}

		// Token: 0x06001454 RID: 5204 RVA: 0x000B3074 File Offset: 0x000B1274
		public void RemoveLimb(Limb limb)
		{
			if (!this.Limbs.Contains(limb))
			{
				return;
			}
			Limb[] newLimbs = new Limb[this.Limbs.Length - 1];
			int i = 0;
			foreach (Limb existingLimb in this.Limbs)
			{
				if (existingLimb != limb)
				{
					newLimbs[i] = existingLimb;
					i++;
				}
			}
			this.limbs = newLimbs;
			if (this.limbDictionary.ContainsKey(limb.type))
			{
				this.limbDictionary.Remove(limb.type);
				if (this.HasMultipleLimbsOfSameType)
				{
					Limb otherLimb = this.Limbs.FirstOrDefault((Limb l) => l != limb && l.type == limb.type);
					if (otherLimb != null)
					{
						this.limbDictionary.Add(otherLimb.type, otherLimb);
					}
				}
			}
			LimbJoint[] attachedJoints = Array.FindAll<LimbJoint>(this.LimbJoints, (LimbJoint lj) => lj.LimbA == limb || lj.LimbB == limb);
			if (attachedJoints.Length != 0)
			{
				LimbJoint[] newJoints = new LimbJoint[this.LimbJoints.Length - attachedJoints.Length];
				i = 0;
				foreach (LimbJoint limbJoint in this.LimbJoints)
				{
					if (!attachedJoints.Contains(limbJoint))
					{
						newJoints[i] = limbJoint;
						i++;
					}
				}
				this.LimbJoints = newJoints;
			}
			this.limbBodies.Remove(limb.body.FarseerBody);
			limb.Remove();
			foreach (LimbJoint limbJoint2 in attachedJoints)
			{
				GameMain.World.Remove(limbJoint2.Joint);
			}
		}

		// Token: 0x06001455 RID: 5205 RVA: 0x000B321C File Offset: 0x000B141C
		public bool OnLimbCollision(Fixture f1, Fixture f2, Contact contact)
		{
			Ragdoll.<>c__DisplayClass130_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.contact = contact;
			Submarine submarine = f2.Body.UserData as Submarine;
			if (submarine != null && this.character.Submarine == submarine)
			{
				return false;
			}
			if (f2.UserData is Hull)
			{
				if (this.character.Submarine != null)
				{
					return false;
				}
				if (this.CanEnterSubmarine == CanEnterSubmarine.Partial)
				{
					if (f1.Body != this.Collider.FarseerBody)
					{
						Limb limb = f1.Body.UserData as Limb;
						return limb != null && !limb.Params.CanEnterSubmarine;
					}
					return true;
				}
			}
			Vector2 velocity = this.Collider.LinearVelocity;
			if (this.character.Submarine == null)
			{
				Submarine sub = f2.Body.UserData as Submarine;
				if (sub != null)
				{
					velocity -= sub.Velocity;
				}
			}
			object userData = f2.Body.UserData;
			CS$<>8__locals1.structure = (userData as Structure);
			if (CS$<>8__locals1.structure == null)
			{
				if (!f2.IsSensor)
				{
					Queue<Ragdoll.Impact> obj = this.impactQueue;
					lock (obj)
					{
						this.impactQueue.Enqueue(new Ragdoll.Impact(f1, f2, CS$<>8__locals1.contact, velocity));
					}
				}
				return true;
			}
			if (this.character.Submarine != null && CS$<>8__locals1.structure.Submarine != null && this.character.Submarine != CS$<>8__locals1.structure.Submarine)
			{
				return false;
			}
			CS$<>8__locals1.colliderBottom = this.GetColliderBottom();
			if (CS$<>8__locals1.structure.IsPlatform)
			{
				if (this.IgnorePlatforms || this.currentHull == null)
				{
					return false;
				}
				if (CS$<>8__locals1.colliderBottom.Y < ConvertUnits.ToSimUnits(CS$<>8__locals1.structure.Rect.Y - 5))
				{
					return false;
				}
				if (f1.Body.Position.Y < ConvertUnits.ToSimUnits(CS$<>8__locals1.structure.Rect.Y - 5))
				{
					return false;
				}
			}
			else if (CS$<>8__locals1.structure.StairDirection != Direction.None)
			{
				if (this.character.SelectedBy != null)
				{
					this.Stairs = this.character.SelectedBy.AnimController.Stairs;
				}
				Ragdoll.LimbStairCollisionResponse collisionResponse = this.<OnLimbCollision>g__getStairCollisionResponse|130_0(ref CS$<>8__locals1);
				if (collisionResponse != Ragdoll.LimbStairCollisionResponse.ClimbWithLimbCollision)
				{
					if (collisionResponse == Ragdoll.LimbStairCollisionResponse.DontClimbStairs)
					{
						this.Stairs = null;
					}
					return false;
				}
				this.Stairs = CS$<>8__locals1.structure;
			}
			Queue<Ragdoll.Impact> obj2 = this.impactQueue;
			lock (obj2)
			{
				this.impactQueue.Enqueue(new Ragdoll.Impact(f1, f2, CS$<>8__locals1.contact, velocity));
			}
			return true;
		}

		// Token: 0x06001456 RID: 5206 RVA: 0x000B34CC File Offset: 0x000B16CC
		private void ApplyImpact(Fixture f1, Fixture f2, Vector2 worldNormal, Vector2 impactPos, Vector2 velocity)
		{
			if (this.character.DisableImpactDamageTimer > 0f)
			{
				return;
			}
			Body body = f2.Body;
			if (((body != null) ? body.UserData : null) is Item && f2.Body.BodyType != BodyType.Static)
			{
				return;
			}
			float impact = Vector2.Dot(velocity, -worldNormal);
			if (f1.Body == this.Collider.FarseerBody || !this.Collider.Enabled)
			{
				bool isNotRemote = true;
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					isNotRemote = !this.character.IsRemotelyControlled;
				}
				if (isNotRemote)
				{
					float impactTolerance = this.ImpactTolerance;
					if (this.character.Stun > 0f)
					{
						impactTolerance *= 0.5f;
					}
					if (impact > impactTolerance)
					{
						impactPos = ConvertUnits.ToDisplayUnits(impactPos);
						if (this.character.Submarine != null)
						{
							impactPos += this.character.Submarine.Position;
						}
						float impactDamage = this.GetImpactDamage(impact, new float?(impactTolerance));
						float? should = null;
						LuaCsSetup.Instance.EventService.PublishEvent<IEventChangeFallDamage>(delegate(IEventChangeFallDamage x)
						{
							float? num = x.OnChangeFallDamage(impactDamage, this.character, impactPos, velocity);
							should = ((num != null) ? num : should);
						});
						if (should != null)
						{
							impactDamage = should.Value;
						}
						this.character.LastDamageSource = null;
						this.character.AddDamage(impactPos, AfflictionPrefab.ImpactDamage.Instantiate(impactDamage, null).ToEnumerable<Affliction>(), 0f, true, null, null, 1f);
						this.strongestImpact = Math.Max(this.strongestImpact, impact - impactTolerance);
						this.character.ApplyStatusEffects(ActionType.OnImpact, 1f);
						this.character.DisableImpactDamageTimer = 0.25f;
					}
				}
			}
		}

		// Token: 0x06001457 RID: 5207 RVA: 0x000B36D8 File Offset: 0x000B18D8
		public float GetImpactDamage(float impact, float? impactTolerance = null)
		{
			float tolerance = impactTolerance ?? this.ImpactTolerance;
			return Math.Min((impact - tolerance) * 10f, this.character.MaxVitality * 0.1f);
		}

		// Token: 0x06001458 RID: 5208 RVA: 0x000B3720 File Offset: 0x000B1920
		public bool SeverLimbJoint(LimbJoint limbJoint)
		{
			if (!limbJoint.CanBeSevered || limbJoint.IsSevered)
			{
				return false;
			}
			limbJoint.IsSevered = true;
			limbJoint.Enabled = false;
			Vector2 limbDiff = limbJoint.LimbA.SimPosition - limbJoint.LimbB.SimPosition;
			if (limbDiff.LengthSquared() < 0.0001f)
			{
				limbDiff = Rand.Vector(1f, Rand.RandSync.Unsynced);
			}
			limbDiff = Vector2.Normalize(limbDiff);
			float mass = limbJoint.BodyA.Mass + limbJoint.BodyB.Mass;
			limbJoint.LimbA.body.ApplyLinearImpulse(limbDiff * Math.Min(mass, limbJoint.BodyA.Mass * 500f), (limbJoint.LimbA.SimPosition + limbJoint.LimbB.SimPosition) / 2f);
			limbJoint.LimbB.body.ApplyLinearImpulse(-limbDiff * Math.Min(mass, limbJoint.BodyB.Mass * 500f), (limbJoint.LimbA.SimPosition + limbJoint.LimbB.SimPosition) / 2f);
			this.connectedLimbs.Clear();
			this.checkedJoints.Clear();
			this.GetConnectedLimbs(this.connectedLimbs, this.checkedJoints, this.MainLimb);
			foreach (Limb limb in this.Limbs)
			{
				if (!this.connectedLimbs.Contains(limb))
				{
					limb.IsSevered = true;
					if (limb.type == LimbType.RightHand)
					{
						CharacterInventory inventory = this.character.Inventory;
						if (inventory != null)
						{
							Item itemInLimbSlot = inventory.GetItemInLimbSlot(InvSlotType.RightHand);
							if (itemInLimbSlot != null)
							{
								itemInLimbSlot.Drop(this.character, true, true);
							}
						}
					}
					else if (limb.type == LimbType.LeftHand)
					{
						CharacterInventory inventory2 = this.character.Inventory;
						if (inventory2 != null)
						{
							Item itemInLimbSlot2 = inventory2.GetItemInLimbSlot(InvSlotType.LeftHand);
							if (itemInLimbSlot2 != null)
							{
								itemInLimbSlot2.Drop(this.character, true, true);
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(this.character.BloodDecalName))
			{
				Hull hull = this.character.CurrentHull;
				if (hull != null)
				{
					hull.AddDecal(this.character.BloodDecalName, (limbJoint.LimbA.WorldPosition + limbJoint.LimbB.WorldPosition) / 2f, MathHelper.Clamp(Math.Min(limbJoint.LimbA.Mass, limbJoint.LimbB.Mass), 0.5f, 2f), false, null);
				}
			}
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				GameMain.NetworkMember.CreateEntityEvent(this.character, default(Character.CharacterStatusEventData));
			}
			return true;
		}

		// Token: 0x06001459 RID: 5209 RVA: 0x000B39DC File Offset: 0x000B1BDC
		protected List<Limb> GetConnectedLimbs(Limb limb)
		{
			this.connectedLimbs.Clear();
			this.checkedJoints.Clear();
			this.GetConnectedLimbs(this.connectedLimbs, this.checkedJoints, limb);
			return this.connectedLimbs;
		}

		// Token: 0x0600145A RID: 5210 RVA: 0x000B3A10 File Offset: 0x000B1C10
		private void GetConnectedLimbs(List<Limb> connectedLimbs, List<LimbJoint> checkedJoints, Limb limb)
		{
			connectedLimbs.Add(limb);
			foreach (LimbJoint joint in this.LimbJoints)
			{
				if (!joint.IsSevered && !checkedJoints.Contains(joint))
				{
					if (joint.LimbA == limb)
					{
						if (!connectedLimbs.Contains(joint.LimbB))
						{
							checkedJoints.Add(joint);
							this.GetConnectedLimbs(connectedLimbs, checkedJoints, joint.LimbB);
						}
					}
					else if (joint.LimbB == limb && !connectedLimbs.Contains(joint.LimbA))
					{
						checkedJoints.Add(joint);
						this.GetConnectedLimbs(connectedLimbs, checkedJoints, joint.LimbA);
					}
				}
			}
		}

		// Token: 0x170005DF RID: 1503
		// (get) Token: 0x0600145B RID: 5211 RVA: 0x000B3AA8 File Offset: 0x000B1CA8
		// (set) Token: 0x0600145C RID: 5212 RVA: 0x000B3AB0 File Offset: 0x000B1CB0
		public bool IsFlipped { get; private set; }

		// Token: 0x0600145D RID: 5213 RVA: 0x000B3ABC File Offset: 0x000B1CBC
		public virtual void Flip()
		{
			this.IsFlipped = !this.IsFlipped;
			this.dir = ((this.dir == Direction.Left) ? Direction.Right : Direction.Left);
			for (int i = 0; i < this.LimbJoints.Length; i++)
			{
				float lowerLimit = -this.LimbJoints[i].UpperLimit;
				float upperLimit = -this.LimbJoints[i].LowerLimit;
				this.LimbJoints[i].LowerLimit = lowerLimit;
				this.LimbJoints[i].UpperLimit = upperLimit;
				this.LimbJoints[i].LocalAnchorA = new Vector2(-this.LimbJoints[i].LocalAnchorA.X, this.LimbJoints[i].LocalAnchorA.Y);
				this.LimbJoints[i].LocalAnchorB = new Vector2(-this.LimbJoints[i].LocalAnchorB.X, this.LimbJoints[i].LocalAnchorB.Y);
			}
			foreach (Limb limb in this.Limbs)
			{
				if (limb != null && !limb.IsSevered && limb.DoesMirror)
				{
					limb.Dir = this.Dir;
					limb.MouthPos = new Vector2(-limb.MouthPos.X, limb.MouthPos.Y);
					limb.MirrorPullJoint();
				}
			}
		}

		// Token: 0x0600145E RID: 5214 RVA: 0x000B3C1C File Offset: 0x000B1E1C
		public Vector2 GetCenterOfMass()
		{
			if (!this.Limbs.Any((Limb l) => !l.IsSevered && l.body.Enabled))
			{
				return this.Collider.SimPosition;
			}
			Vector2 centerOfMass = Vector2.Zero;
			float totalMass = 0f;
			foreach (Limb limb in this.Limbs)
			{
				if (!limb.IsSevered && limb.body.Enabled)
				{
					centerOfMass += limb.Mass * limb.SimPosition;
					totalMass += limb.Mass;
				}
			}
			if (totalMass <= 0f)
			{
				return this.Collider.SimPosition;
			}
			centerOfMass /= totalMass;
			if (!MathUtils.IsValid(centerOfMass))
			{
				string[] array2 = new string[7];
				array2[0] = "Ragdoll.GetCenterOfMass returned an invalid value (";
				int num = 1;
				Vector2 vector = centerOfMass;
				array2[num] = vector.ToString();
				array2[2] = "). Limb positions: {";
				array2[3] = string.Join<Vector2>(", ", from l in this.limbs
				select l.SimPosition);
				array2[4] = "}, total mass: ";
				array2[5] = totalMass.ToString();
				array2[6] = ".";
				string errorMsg = string.Concat(array2);
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Ragdoll.GetCenterOfMass", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				return this.Collider.SimPosition;
			}
			return centerOfMass;
		}

		// Token: 0x0600145F RID: 5215 RVA: 0x000B3D8F File Offset: 0x000B1F8F
		public void MoveLimb(Limb limb, Vector2 pos, float amount, bool pullFromCenter = false)
		{
			limb.MoveToPos(pos, amount, pullFromCenter);
		}

		// Token: 0x06001460 RID: 5216 RVA: 0x000B3D9C File Offset: 0x000B1F9C
		public void ResetPullJoints(Func<Limb, bool> condition = null)
		{
			for (int i = 0; i < this.Limbs.Length; i++)
			{
				if (this.Limbs[i] != null && (condition == null || condition(this.Limbs[i])))
				{
					this.Limbs[i].PullJointEnabled = false;
				}
			}
		}

		// Token: 0x06001461 RID: 5217 RVA: 0x000B3DE8 File Offset: 0x000B1FE8
		public static void UpdateAll(float deltaTime, Camera cam)
		{
			foreach (Ragdoll r in Ragdoll.list)
			{
				r.UpdateRagdoll(deltaTime, cam);
			}
		}

		// Token: 0x06001462 RID: 5218 RVA: 0x000B3E3C File Offset: 0x000B203C
		public void FindHull(Vector2? worldPosition = null, bool setSubmarine = true, bool setInWater = false)
		{
			Vector2 findPos = (worldPosition == null) ? this.WorldPosition : worldPosition.Value;
			if (!MathUtils.IsValid(findPos))
			{
				string identifier = "Ragdoll.FindHull:InvalidPosition";
				GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
				string str = "Attempted to find a hull at an invalid position (";
				Vector2 vector = findPos;
				GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, str + vector.ToString() + ")\n" + Environment.StackTrace.CleanupStackTrace());
				return;
			}
			Hull newHull = Hull.FindHull(findPos, this.currentHull, true, true);
			if (setInWater && (newHull == null || findPos.Y < newHull.WorldSurface))
			{
				this.inWater = true;
			}
			if (newHull == this.currentHull)
			{
				return;
			}
			if ((this.CanEnterSubmarine == CanEnterSubmarine.False || (this.character.AIController != null && this.character.AIController.CanEnterSubmarine == CanEnterSubmarine.False)) && ((newHull != null) ? newHull.Submarine : null) != null)
			{
				Vector2 hullDiff = this.WorldPosition - newHull.WorldPosition;
				Vector2 moveDir = (hullDiff.LengthSquared() < 0.001f) ? Vector2.UnitY : Vector2.Normalize(hullDiff);
				Vector2 intersection;
				if (MathUtils.GetLineWorldRectangleIntersection(newHull.WorldPosition, newHull.WorldPosition + moveDir * (float)Math.Max(newHull.Rect.Width, newHull.Rect.Height), new Rectangle(newHull.WorldRect.X - 32, newHull.WorldRect.Y + 32, newHull.WorldRect.Width + 64, newHull.Rect.Height + 64), out intersection))
				{
					this.Collider.SetTransform(ConvertUnits.ToSimUnits(intersection), this.Collider.Rotation, true);
				}
				return;
			}
			if (this.CanEnterSubmarine != CanEnterSubmarine.True)
			{
				return;
			}
			if (setSubmarine)
			{
				if (((newHull != null) ? newHull.Submarine : null) == null)
				{
					Hull hull = this.currentHull;
					if (((hull != null) ? hull.Submarine : null) != null)
					{
						if (Gap.FindAdjacent(from g in Gap.GapList
						where g.Submarine == this.currentHull.Submarine
						select g, findPos, 150f, true) != null)
						{
							return;
						}
						if (this.Limbs.Any((Limb l) => !l.IsSevered && Gap.FindAdjacent(this.currentHull.ConnectedGaps, l.WorldPosition, ConvertUnits.ToDisplayUnits(l.body.GetSize().Combine()), true) != null))
						{
							return;
						}
						List<CharacterStateInfo> memLocalState = this.character.MemLocalState;
						if (memLocalState != null)
						{
							memLocalState.Clear();
						}
						this.Teleport(ConvertUnits.ToSimUnits(this.currentHull.Submarine.Position), this.currentHull.Submarine.Velocity, true);
						goto IL_33B;
					}
				}
				if (this.currentHull == null && newHull.Submarine != null)
				{
					List<CharacterStateInfo> memLocalState2 = this.character.MemLocalState;
					if (memLocalState2 != null)
					{
						memLocalState2.Clear();
					}
					this.Teleport(-ConvertUnits.ToSimUnits(newHull.Submarine.Position), -newHull.Submarine.Velocity, true);
				}
				else if (newHull != null && this.currentHull != null && newHull.Submarine != this.currentHull.Submarine)
				{
					List<CharacterStateInfo> memLocalState3 = this.character.MemLocalState;
					if (memLocalState3 != null)
					{
						memLocalState3.Clear();
					}
					Vector2 newSubPos = (newHull.Submarine == null) ? Vector2.Zero : newHull.Submarine.Position;
					Vector2 prevSubPos = (this.currentHull.Submarine == null) ? Vector2.Zero : this.currentHull.Submarine.Position;
					this.Teleport(ConvertUnits.ToSimUnits(prevSubPos - newSubPos), Vector2.Zero, true);
				}
			}
			IL_33B:
			this.CurrentHull = newHull;
			Entity entity = this.character;
			Hull hull2 = this.currentHull;
			entity.Submarine = ((hull2 != null) ? hull2.Submarine : null);
			foreach (Projectile attachedProjectile in this.character.AttachedProjectiles)
			{
				attachedProjectile.Item.CurrentHull = this.currentHull;
				attachedProjectile.Item.Submarine = this.character.Submarine;
				attachedProjectile.Item.UpdateTransform();
			}
		}

		// Token: 0x06001463 RID: 5219 RVA: 0x000B4224 File Offset: 0x000B2424
		private void PreventOutsideCollision()
		{
			Hull hull = this.currentHull;
			if (((hull != null) ? hull.Submarine : null) == null)
			{
				return;
			}
			IEnumerable<Gap> connectedGaps = from g in this.currentHull.ConnectedGaps
			where !g.IsRoomToRoom
			select g;
			foreach (Gap gap in connectedGaps)
			{
				if (gap.IsHorizontal)
				{
					if (this.character.Position.Y > (float)gap.Rect.Y || this.character.Position.Y < (float)(gap.Rect.Y - gap.Rect.Height))
					{
						continue;
					}
					if (Math.Sign(gap.Rect.Center.X - this.currentHull.Rect.Center.X) != Math.Sign(this.character.Position.X - (float)this.currentHull.Rect.Center.X))
					{
						continue;
					}
				}
				else if (this.character.Position.X < (float)gap.Rect.X || this.character.Position.X > (float)gap.Rect.Right || Math.Sign(gap.Rect.Y - gap.Rect.Height / 2 - (this.currentHull.Rect.Y - this.currentHull.Rect.Height / 2)) != Math.Sign(this.character.Position.Y - (float)(this.currentHull.Rect.Y - this.currentHull.Rect.Height / 2)))
				{
					continue;
				}
				gap.RefreshOutsideCollider();
			}
		}

		// Token: 0x06001464 RID: 5220 RVA: 0x000B444C File Offset: 0x000B264C
		public void Teleport(Vector2 moveAmount, Vector2 velocityChange, bool detachProjectiles = true)
		{
			foreach (Limb limb in this.Limbs)
			{
				if (!limb.IsSevered && limb.body.FarseerBody.ContactList != null)
				{
					ContactEdge ce = limb.body.FarseerBody.ContactList;
					while (ce != null && ce.Contact != null)
					{
						ce.Contact.Enabled = false;
						ce = ce.Next;
					}
				}
			}
			foreach (Limb limb2 in this.Limbs)
			{
				if (!limb2.IsSevered)
				{
					limb2.body.LinearVelocity += velocityChange;
				}
			}
			this.character.DisableImpactDamageTimer = 0.25f;
			this.SetPosition(this.Collider.SimPosition + moveAmount, false, true, false, true);
			this.character.CursorPosition += moveAmount;
			PhysicsBody physicsBody = this.Collider;
			if (physicsBody != null)
			{
				physicsBody.UpdateDrawPosition(true);
			}
			foreach (Limb limb3 in this.Limbs)
			{
				limb3.body.UpdateDrawPosition(true);
			}
		}

		// Token: 0x06001465 RID: 5221 RVA: 0x000B4584 File Offset: 0x000B2784
		private void UpdateCollisionCategories()
		{
			Hull hull = this.currentHull;
			Category wall = (((hull != null) ? hull.Submarine : null) == null) ? (Category.Cat1 | Category.Cat8) : Category.Cat1;
			Category collisionCategory = this.IgnorePlatforms ? (wall | Category.Cat7 | Category.Cat4) : (wall | Category.Cat7 | Category.Cat3 | Category.Cat4);
			if (collisionCategory == this.prevCollisionCategory)
			{
				return;
			}
			this.prevCollisionCategory = collisionCategory;
			this.Collider.CollidesWith = (collisionCategory | Category.Cat6);
			foreach (Limb limb in this.Limbs)
			{
				if (!limb.IgnoreCollisions && !limb.IsSevered)
				{
					try
					{
						limb.body.CollidesWith = collisionCategory;
					}
					catch (Exception e)
					{
						DebugConsole.ThrowError("Failed to update ragdoll limb collisioncategories", e, null, false, false);
					}
				}
			}
		}

		// Token: 0x170005E0 RID: 1504
		// (get) Token: 0x06001466 RID: 5222 RVA: 0x000B4648 File Offset: 0x000B2848
		// (set) Token: 0x06001467 RID: 5223 RVA: 0x000B4658 File Offset: 0x000B2858
		public bool BodyInRest
		{
			get
			{
				return this.bodyInRestTimer > this.BodyInRestDelay;
			}
			set
			{
				foreach (Limb limb in this.Limbs)
				{
					limb.body.PhysEnabled = !value;
				}
				this.bodyInRestTimer = (value ? this.BodyInRestDelay : 0f);
			}
		}

		// Token: 0x06001468 RID: 5224 RVA: 0x000B46A4 File Offset: 0x000B28A4
		public void UpdateRagdoll(float deltaTime, Camera cam)
		{
			if (!this.character.Enabled || this.character.Removed || this.Frozen || this.Invalid || this.Collider == null || this.Collider.Removed)
			{
				return;
			}
			while (this.impactQueue.Count > 0)
			{
				Ragdoll.Impact impact = this.impactQueue.Dequeue();
				this.ApplyImpact(impact.F1, impact.F2, impact.WorldNormal, impact.ImpactPos, impact.Velocity);
			}
			this.CheckValidity();
			this.UpdateNetPlayerPosition(deltaTime);
			this.CheckDistFromCollider();
			this.UpdateCollisionCategories();
			this.FindHull(null, true, false);
			this.PreventOutsideCollision();
			this.CheckBodyInRest(deltaTime);
			this.splashSoundTimer -= deltaTime;
			if (this.character.Submarine == null && Level.Loaded != null)
			{
				if (this.Collider.SimPosition.Y > Level.Loaded.TopBarrier.Position.Y)
				{
					this.Collider.LinearVelocity = new Vector2(this.Collider.LinearVelocity.X, Math.Min(this.Collider.LinearVelocity.Y, -1f));
				}
				else if (this.Collider.SimPosition.Y < Level.Loaded.BottomBarrier.Position.Y)
				{
					this.Collider.LinearVelocity = new Vector2(this.Collider.LinearVelocity.X, MathHelper.Clamp(this.Collider.LinearVelocity.Y, Level.Loaded.BottomBarrier.Position.Y - this.Collider.SimPosition.Y, 10f));
				}
				foreach (Limb limb in this.Limbs)
				{
					if (limb.SimPosition.Y > Level.Loaded.TopBarrier.Position.Y)
					{
						limb.body.LinearVelocity = new Vector2(limb.LinearVelocity.X, Math.Min(limb.LinearVelocity.Y, -1f));
					}
					else if (limb.SimPosition.Y < Level.Loaded.BottomBarrier.Position.Y)
					{
						limb.body.LinearVelocity = new Vector2(limb.LinearVelocity.X, MathHelper.Clamp(limb.LinearVelocity.Y, Level.Loaded.BottomBarrier.Position.Y - limb.SimPosition.Y, 10f));
					}
				}
			}
			float MaxVel = 64f;
			if (GameMain.NetworkMember != null)
			{
				this.Collider.LinearVelocity = new Vector2(NetConfig.Quantize(this.Collider.LinearVelocity.X, -MaxVel, MaxVel, 12), NetConfig.Quantize(this.Collider.LinearVelocity.Y, -MaxVel, MaxVel, 12));
			}
			else
			{
				this.Collider.LinearVelocity = new Vector2(MathHelper.Clamp(this.Collider.LinearVelocity.X, -MaxVel, MaxVel), MathHelper.Clamp(this.Collider.LinearVelocity.Y, -MaxVel, MaxVel));
			}
			if (this.forceStanding)
			{
				this.inWater = false;
				this.headInWater = false;
				this.RefreshFloorY(deltaTime, this.Stairs == null);
			}
			else if (this.currentHull == null)
			{
				this.inWater = true;
				this.headInWater = true;
			}
			else
			{
				this.headInWater = false;
				this.inWater = false;
				this.RefreshFloorY(deltaTime, this.Stairs == null);
				if (this.currentHull.WaterPercentage > 0.001f)
				{
					ValueTuple<float, float> waterSurfaceAndCeilingY = this.GetWaterSurfaceAndCeilingY();
					float waterSurfaceDisplayUnits = waterSurfaceAndCeilingY.Item1;
					float ceilingDisplayUnits = waterSurfaceAndCeilingY.Item2;
					float waterSurfaceY = ConvertUnits.ToSimUnits(waterSurfaceDisplayUnits);
					float ceilingY = ConvertUnits.ToSimUnits(ceilingDisplayUnits);
					if (this.targetMovement.Y < 0f)
					{
						Vector2 colliderBottom = this.GetColliderBottom();
						this.floorY = Math.Min(colliderBottom.Y, this.floorY);
						if (this.floorY < ConvertUnits.ToSimUnits(this.currentHull.Rect.Y - this.currentHull.Rect.Height))
						{
							Hull lowerHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(colliderBottom), null, false, true);
							if (lowerHull != null)
							{
								this.floorY = ConvertUnits.ToSimUnits(lowerHull.Rect.Y - lowerHull.Rect.Height);
							}
						}
					}
					float standHeight = this.HeadPosition ?? (this.TorsoPosition ?? (this.Collider.GetMaxExtent() * 0.5f));
					if (this.Collider.SimPosition.Y < waterSurfaceY && (waterSurfaceY - this.floorY > standHeight * 0.8f || ceilingY - this.floorY < standHeight * 0.8f))
					{
						this.inWater = true;
					}
				}
			}
			this.UpdateHullFlowForces(deltaTime);
			bool applyWaterForces = this.currentHull == null || this.currentHull.WaterVolume > this.currentHull.Volume * 0.95f || ConvertUnits.ToSimUnits(this.currentHull.Surface) > this.Collider.SimPosition.Y;
			if (applyWaterForces)
			{
				this.Collider.ApplyWaterForces();
			}
			foreach (Limb limb2 in this.Limbs)
			{
				Hull newHull = (this.currentHull == null) ? null : Hull.FindHull(limb2.WorldPosition, this.currentHull, true, true);
				bool prevInWater = limb2.InWater;
				limb2.InWater = false;
				if (this.forceStanding)
				{
					limb2.InWater = false;
				}
				else if (newHull == null)
				{
					limb2.InWater = true;
					if (limb2.type == LimbType.Head)
					{
						this.headInWater = true;
					}
				}
				else if (newHull.WaterVolume > 0f && Submarine.RectContains(newHull.Rect, limb2.Position, false))
				{
					if (limb2.Position.Y < newHull.Surface)
					{
						limb2.InWater = true;
						this.surfaceY = newHull.Surface;
						if (limb2.type == LimbType.Head)
						{
							this.headInWater = true;
						}
					}
					if (Math.Abs(limb2.LinearVelocity.Y) > 5f && limb2.InWater != prevInWater && newHull == limb2.Hull && limb2.LinearVelocity.Y < 0f)
					{
						Vector2 impulse = limb2.LinearVelocity * limb2.Mass;
						int i = (int)((limb2.Position.X - (float)newHull.Rect.X) / 32f);
						newHull.WaveVel[i] += MathHelper.Clamp(impulse.Y, -5f, 5f);
					}
				}
				limb2.Hull = newHull;
				limb2.Update(deltaTime);
			}
			Item selectedItem = this.character.SelectedItem;
			Controller controller = (selectedItem != null) ? selectedItem.GetComponent<Controller>() : null;
			bool isAttachedToController = controller != null && controller.User == this.character && controller.IsAttachedUser(controller.User);
			if (!this.inWater && this.character.AllowInput && this.levitatingCollider && !isAttachedToController)
			{
				if (this.onGround && this.Collider.LinearVelocity.Y > -this.ImpactTolerance)
				{
					float targetY = this.standOnFloorY + (float)Math.Abs(Math.Cos((double)this.Collider.Rotation)) * this.Collider.Height * 0.5f + this.Collider.Radius + this.ColliderHeightFromFloor;
					float slopePull = 0f;
					float y = this.floorNormal.Y;
					if (y > 0f && y < 1f && Math.Sign(this.movement.X) == Math.Sign(this.floorNormal.X))
					{
						float steepness = Math.Abs(this.floorNormal.X);
						slopePull = Math.Abs(this.movement.X * steepness) / 5f;
					}
					if (Math.Abs(this.Collider.SimPosition.Y - targetY - slopePull) > 0.01f)
					{
						float yVelocity = (targetY - this.Collider.SimPosition.Y) * 5f;
						if (this.Stairs != null && targetY < this.Collider.SimPosition.Y)
						{
							yVelocity = (float)Math.Sign(yVelocity);
						}
						yVelocity -= slopePull * 5f;
						this.Collider.LinearVelocity = new Vector2(this.Collider.LinearVelocity.X, yVelocity);
					}
				}
				else if (this.Collider.LinearVelocity == Vector2.Zero)
				{
					NetworkMember networkMember = GameMain.NetworkMember;
					if (networkMember == null || !networkMember.IsClient)
					{
						this.character.IsRagdolled = true;
						if (!this.character.IsPlayer)
						{
							this.character.SetInput(InputType.Ragdoll, false, true);
						}
					}
				}
			}
			this.forceNotStanding = false;
		}

		// Token: 0x06001469 RID: 5225 RVA: 0x000B5014 File Offset: 0x000B3214
		protected void UpdateRagdollControlsMovement()
		{
			this.levitatingCollider = false;
			this.Collider.FarseerBody.FixedRotation = false;
			if (this.Collider.Enabled)
			{
				this.MainLimb.body.LinearVelocity = this.Collider.LinearVelocity;
				this.Collider.Enabled = false;
			}
			this.Collider.LinearVelocity = this.MainLimb.LinearVelocity;
			this.Collider.SetTransformIgnoreContacts(this.MainLimb.SimPosition, this.MainLimb.Rotation, true);
			if (!this.Draggable || this.character.SelectedBy == null)
			{
				this.ResetPullJoints(null);
			}
		}

		// Token: 0x0600146A RID: 5226 RVA: 0x000B50C4 File Offset: 0x000B32C4
		private void CheckBodyInRest(float deltaTime)
		{
			if (this.SimplePhysicsEnabled)
			{
				return;
			}
			if (this.InWater || this.Collider.LinearVelocity.LengthSquared() > 0.01f || this.character.SelectedBy != null || !this.character.IsDead)
			{
				this.bodyInRestTimer = 0f;
				foreach (Limb limb in this.Limbs)
				{
					limb.body.PhysEnabled = true;
				}
				return;
			}
			if (this.Limbs.All((Limb l) => (l != null && !l.body.Enabled) || l.LinearVelocity.LengthSquared() < 0.001f))
			{
				this.bodyInRestTimer += deltaTime;
				if (this.bodyInRestTimer > this.BodyInRestDelay)
				{
					foreach (Limb limb2 in this.Limbs)
					{
						limb2.body.PhysEnabled = false;
					}
				}
			}
		}

		// Token: 0x170005E1 RID: 1505
		// (get) Token: 0x0600146B RID: 5227 RVA: 0x000B51BE File Offset: 0x000B33BE
		// (set) Token: 0x0600146C RID: 5228 RVA: 0x000B51C6 File Offset: 0x000B33C6
		public bool Invalid { get; private set; }

		// Token: 0x0600146D RID: 5229 RVA: 0x000B51D0 File Offset: 0x000B33D0
		private bool CheckValidity()
		{
			if (this.limbs == null)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Attempted to check the validity of a potentially removed ragdoll. Character: ",
					this.character.Name,
					", id: ",
					this.character.ID.ToString(),
					", removed: ",
					this.character.Removed.ToString(),
					", ragdoll removed: ",
					(!Ragdoll.list.Contains(this)).ToString()
				}), null, null, false, false);
				this.Invalid = true;
				return false;
			}
			bool isColliderValid = this.CheckValidity(this.Collider);
			if (!isColliderValid)
			{
				this.Collider.ResetDynamics();
			}
			bool limbsValid = true;
			foreach (Limb limb in this.limbs)
			{
				if (((limb != null) ? limb.body : null) != null && limb.body.Enabled && !this.CheckValidity(limb.body))
				{
					limbsValid = false;
					limb.body.ResetDynamics();
					break;
				}
			}
			bool isValid = isColliderValid && limbsValid;
			if (!isValid)
			{
				this.validityResets++;
				if (this.validityResets > 3)
				{
					this.Invalid = true;
					DebugConsole.ThrowError("Invalid ragdoll physics. Ragdoll frozen to prevent crashes.", null, null, false, false);
					this.Collider.SetTransform(Vector2.Zero, 0f, true);
					this.Collider.ResetDynamics();
					foreach (Limb limb2 in this.Limbs)
					{
						PhysicsBody body = limb2.body;
						if (body != null)
						{
							body.SetTransform(this.Collider.SimPosition, 0f, true);
						}
						PhysicsBody body2 = limb2.body;
						if (body2 != null)
						{
							body2.ResetDynamics();
						}
					}
					this.Frozen = true;
				}
			}
			return isValid;
		}

		// Token: 0x0600146E RID: 5230 RVA: 0x000B53AC File Offset: 0x000B35AC
		private bool CheckValidity(PhysicsBody body)
		{
			Ragdoll.<>c__DisplayClass171_0 CS$<>8__locals1;
			CS$<>8__locals1.body = body;
			string errorMsg = null;
			if (!MathUtils.IsValid(CS$<>8__locals1.body.SimPosition) || Math.Abs(CS$<>8__locals1.body.SimPosition.X) > 1E+10f || Math.Abs(CS$<>8__locals1.body.SimPosition.Y) > 1E+10f)
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|171_0(ref CS$<>8__locals1) + " position invalid (" + CS$<>8__locals1.body.SimPosition.ToString() + ", character: [name]).";
			}
			else if (!MathUtils.IsValid(CS$<>8__locals1.body.LinearVelocity) || Math.Abs(CS$<>8__locals1.body.LinearVelocity.X) > 1000f || Math.Abs(CS$<>8__locals1.body.LinearVelocity.Y) > 1000f)
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|171_0(ref CS$<>8__locals1) + " velocity invalid (" + CS$<>8__locals1.body.LinearVelocity.ToString() + ", character: [name]).";
			}
			else if (!MathUtils.IsValid(CS$<>8__locals1.body.Rotation))
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|171_0(ref CS$<>8__locals1) + " rotation invalid (" + CS$<>8__locals1.body.Rotation.ToString() + ", character: [name]).";
			}
			else if (!MathUtils.IsValid(CS$<>8__locals1.body.AngularVelocity) || Math.Abs(CS$<>8__locals1.body.AngularVelocity) > 1000f)
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|171_0(ref CS$<>8__locals1) + " angular velocity invalid (" + CS$<>8__locals1.body.AngularVelocity.ToString() + ", character: [name]).";
			}
			if (errorMsg != null)
			{
				if (this.character.IsRemotelyControlled)
				{
					errorMsg += " Ragdoll controlled remotely.";
				}
				if (this.SimplePhysicsEnabled)
				{
					errorMsg += " Simple physics enabled.";
				}
				if (GameMain.NetworkMember != null)
				{
					errorMsg += (GameMain.NetworkMember.IsClient ? " Playing as a client." : " Hosting a server.");
				}
				DebugConsole.NewMessage(errorMsg.Replace("[name]", this.Character.Name), new Color?(Color.Red), false);
				GameAnalyticsManager.AddErrorEventOnce("Ragdoll.CheckValidity:" + this.character.ID.ToString(), GameAnalyticsManager.ErrorSeverity.Error, errorMsg.Replace("[name]", this.Character.SpeciesName.Value));
				if (!MathUtils.IsValid(this.Collider.SimPosition) || Math.Abs(this.Collider.SimPosition.X) > 1E+10f || Math.Abs(this.Collider.SimPosition.Y) > 1E+10f)
				{
					this.Collider.SetTransform(Vector2.Zero, 0f, true);
				}
				foreach (Limb otherLimb in this.Limbs)
				{
					otherLimb.body.SetTransform(this.Collider.SimPosition, 0f, true);
					otherLimb.body.ResetDynamics();
				}
				this.SetInitialLimbPositions();
				return false;
			}
			return true;
		}

		// Token: 0x0600146F RID: 5231 RVA: 0x000B56C8 File Offset: 0x000B38C8
		protected void LogAccessedRemovedCharacterError()
		{
			if (!this.accessRemovedCharacterErrorShown)
			{
				string errorMsg = string.Concat(new string[]
				{
					"Attempted to access a potentially removed ragdoll. Character: ",
					this.character.Name,
					", id: ",
					this.character.ID.ToString(),
					", removed: ",
					this.character.Removed.ToString(),
					", ragdoll removed: ",
					(!Ragdoll.list.Contains(this)).ToString()
				});
				errorMsg = errorMsg + "\n" + Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(errorMsg, null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Ragdoll:AccessRemoved", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
				{
					"Attempted to access a potentially removed ragdoll. Character: ",
					this.character.SpeciesName.ToString(),
					", id: ",
					this.character.ID.ToString(),
					", removed: ",
					this.character.Removed.ToString(),
					", ragdoll removed: ",
					(!Ragdoll.list.Contains(this)).ToString(),
					"\n",
					Environment.StackTrace.CleanupStackTrace()
				}));
				this.accessRemovedCharacterErrorShown = true;
			}
		}

		// Token: 0x06001470 RID: 5232 RVA: 0x000B5830 File Offset: 0x000B3A30
		private void UpdateHullFlowForces(float deltaTime)
		{
			if (this.currentHull == null)
			{
				return;
			}
			Vector2 flowForce = Vector2.Zero;
			foreach (Gap gap in Gap.GapList)
			{
				if (gap.Open > 0f && gap.linkedTo.Contains(this.currentHull) && gap.LerpedFlowForce.LengthSquared() >= 0.01f)
				{
					float dist = Vector2.Distance(this.MainLimb.WorldPosition, gap.WorldPosition) * 0.5f;
					flowForce += Vector2.Normalize(gap.LerpedFlowForce) * (Math.Max(gap.LerpedFlowForce.Length() - dist, 0f) * 0.035f);
				}
			}
			if (this.character.CanMove)
			{
				flowForce *= 2f;
			}
			flowForce *= 1f - Math.Clamp(this.character.GetStatValue(StatTypes.FlowResistance, true), 0f, 1f);
			float flowForceMagnitude = flowForce.Length();
			float limbMultipier = (float)this.limbs.Count((Limb l) => l.InWater) / (float)this.limbs.Length;
			if (flowForceMagnitude * limbMultipier - this.flowStunTolerance > 5f)
			{
				this.character.Stun = Math.Max(this.character.Stun, 0.5f);
				this.flowStunTolerance = Math.Max(this.flowStunTolerance, flowForceMagnitude);
			}
			if (this.character == Character.Controlled && this.inWater)
			{
				Screen selected = Screen.Selected;
				if (((selected != null) ? selected.Cam : null) != null)
				{
					float shakeStrength = Math.Min(flowForceMagnitude / 10f, 5f) * limbMultipier;
					Screen.Selected.Cam.Shake = Math.Max(Screen.Selected.Cam.Shake, shakeStrength);
				}
			}
			if (flowForceMagnitude > 0.0001f)
			{
				flowForce = Vector2.Normalize(flowForce) * Math.Max(flowForceMagnitude - this.flowForceTolerance, 0f);
			}
			if (this.flowForceTolerance <= flowForceMagnitude * 1.5f && this.inWater)
			{
				this.flowForceTolerance += deltaTime * 5f;
				this.flowStunTolerance = Math.Max(this.flowStunTolerance, this.flowForceTolerance);
			}
			else
			{
				this.flowForceTolerance = Math.Max(this.flowForceTolerance - deltaTime * 1f, 0f);
				this.flowStunTolerance = Math.Max(this.flowStunTolerance - deltaTime * 1f, 0f);
			}
			if (flowForce.LengthSquared() > 0.001f)
			{
				this.Collider.ApplyForce(flowForce * (this.Collider.Mass / this.Mass), 64f);
				foreach (Limb limb in this.limbs)
				{
					if (limb.InWater)
					{
						limb.body.ApplyForce(flowForce * (limb.Mass / this.Mass * (float)this.limbs.Length), 64f);
					}
				}
			}
		}

		// Token: 0x06001471 RID: 5233 RVA: 0x000B5B88 File Offset: 0x000B3D88
		public void ForceRefreshFloorY()
		{
			this.lastFloorCheckPos = Vector2.Zero;
		}

		// Token: 0x06001472 RID: 5234 RVA: 0x000B5B98 File Offset: 0x000B3D98
		private void RefreshFloorY(float deltaTime, bool ignoreStairs = false)
		{
			this.floorYCheckTimer -= deltaTime;
			PhysicsBody refBody = this.Collider;
			if (this.floorYCheckTimer < 0f || this.lastFloorCheckIgnoreStairs != ignoreStairs || this.lastFloorCheckIgnorePlatforms != this.IgnorePlatforms || Vector2.DistanceSquared(this.lastFloorCheckPos, refBody.SimPosition) > 0.010000001f)
			{
				this.floorY = this.GetFloorY(refBody.SimPosition, ignoreStairs);
				this.lastFloorCheckPos = refBody.SimPosition;
				this.lastFloorCheckIgnoreStairs = ignoreStairs;
				this.lastFloorCheckIgnorePlatforms = this.IgnorePlatforms;
				this.floorYCheckTimer = 1f * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
			}
		}

		// Token: 0x06001473 RID: 5235 RVA: 0x000B5C44 File Offset: 0x000B3E44
		private float GetFloorY(Vector2 simPosition, bool ignoreStairs = false)
		{
			this.onGround = false;
			this.Stairs = null;
			this.floorFixture = null;
			float height = this.ColliderHeightFromFloor;
			if (this.HeadPosition != null && MathUtils.IsValid(this.HeadPosition.Value))
			{
				height = Math.Max(height, this.HeadPosition.Value);
			}
			if (this.TorsoPosition != null && MathUtils.IsValid(this.TorsoPosition.Value))
			{
				height = Math.Max(height, this.TorsoPosition.Value);
			}
			Vector2 rayEnd = simPosition - new Vector2(0f, height * 2f);
			Vector2 colliderBottomDisplay = ConvertUnits.ToDisplayUnits(this.GetColliderBottom());
			Fixture standOnFloorFixture = null;
			float standOnFloorFraction = 1f;
			float closestFraction = 1f;
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				Category collisionCategories = fixture.CollisionCategories;
				if (collisionCategories <= Category.Cat3)
				{
					if (collisionCategories != Category.Cat1)
					{
						if (collisionCategories != Category.Cat3)
						{
							goto IL_270;
						}
						Structure platform = fixture.Body.UserData as Structure;
						if (!this.IgnorePlatforms && fraction < standOnFloorFraction && (colliderBottomDisplay.Y >= (float)(platform.Rect.Y - 16) || (this.targetMovement.Y > 0f && this.Stairs == null)))
						{
							standOnFloorFraction = fraction;
							standOnFloorFixture = fixture;
						}
						if (colliderBottomDisplay.Y < (float)(platform.Rect.Y - 16) && (this.targetMovement.Y <= 0f || this.Stairs != null))
						{
							return -1f;
						}
						if ((this.IgnorePlatforms && this.TargetMovement.Y < -0.5f) || this.Collider.Position.Y < (float)platform.Rect.Y)
						{
							return -1f;
						}
						goto IL_276;
					}
				}
				else if (collisionCategories != Category.Cat4)
				{
					if (collisionCategories != Category.Cat8)
					{
						goto IL_270;
					}
				}
				else
				{
					if (this.inWater && this.TargetMovement.Y < 0.5f)
					{
						return -1f;
					}
					if (this.character.SelectedBy == null && fraction < standOnFloorFraction)
					{
						Structure structure = fixture.Body.UserData as Structure;
						if (colliderBottomDisplay.Y >= (float)(structure.Rect.Y - structure.Rect.Height + 30) || this.TargetMovement.Y > 0.5f || this.Stairs != null)
						{
							standOnFloorFraction = fraction;
							standOnFloorFixture = fixture;
						}
					}
					if (ignoreStairs)
					{
						return -1f;
					}
					goto IL_276;
				}
				if (!fixture.CollidesWith.HasFlag(Category.Cat2))
				{
					return -1f;
				}
				if (fixture.Body.UserData is Submarine && this.character.Submarine != null)
				{
					return -1f;
				}
				if (fixture.IsSensor)
				{
					return -1f;
				}
				if (fraction < standOnFloorFraction)
				{
					standOnFloorFraction = fraction;
					standOnFloorFixture = fixture;
					goto IL_276;
				}
				goto IL_276;
				IL_270:
				return -1f;
				IL_276:
				if (fraction < closestFraction)
				{
					this.floorNormal = normal;
					closestFraction = fraction;
				}
				return closestFraction;
			}, simPosition, rayEnd, Category.Cat1 | Category.Cat3 | Category.Cat4 | Category.Cat8);
			if (standOnFloorFixture != null && !this.IsHangingWithRope)
			{
				this.floorFixture = standOnFloorFixture;
				this.standOnFloorY = simPosition.Y + (rayEnd.Y - simPosition.Y) * standOnFloorFraction;
				float standHeight = this.Collider.Height * 0.5f + this.Collider.Radius + this.ColliderHeightFromFloor;
				if (simPosition.Y - this.standOnFloorY <= standHeight + 0.1f)
				{
					this.onGround = true;
					if (standOnFloorFixture.CollisionCategories == Category.Cat4)
					{
						this.Stairs = (standOnFloorFixture.Body.UserData as Structure);
					}
				}
			}
			if (closestFraction < 1f)
			{
				return simPosition.Y + (rayEnd.Y - simPosition.Y) * closestFraction;
			}
			this.floorNormal = Vector2.UnitY;
			if (this.CurrentHull == null)
			{
				return -1000f;
			}
			float hullBottom = (float)(this.currentHull.Rect.Y - this.currentHull.Rect.Height);
			foreach (Gap gap in this.currentHull.ConnectedGaps)
			{
				if (gap.IsRoomToRoom && gap.Open >= 1f && gap.ConnectedDoor == null && !gap.IsHorizontal && this.WorldPosition.X > (float)gap.WorldRect.X && this.WorldPosition.X < (float)gap.WorldRect.Right && gap.WorldPosition.Y < this.WorldPosition.Y)
				{
					MapEntity lowerHull = (gap.linkedTo[0] == this.currentHull) ? gap.linkedTo[1] : gap.linkedTo[0];
					hullBottom = Math.Min(hullBottom, (float)(lowerHull.Rect.Y - lowerHull.Rect.Height));
				}
			}
			return ConvertUnits.ToSimUnits(hullBottom);
		}

		// Token: 0x06001474 RID: 5236 RVA: 0x000B5FAC File Offset: 0x000B41AC
		public float GetSurfaceY()
		{
			return this.GetWaterSurfaceAndCeilingY().Item1;
		}

		// Token: 0x06001475 RID: 5237 RVA: 0x000B5FBC File Offset: 0x000B41BC
		[return: TupleElementNames(new string[]
		{
			"WaterSurfaceY",
			"CeilingY"
		})]
		private ValueTuple<float, float> GetWaterSurfaceAndCeilingY()
		{
			Ragdoll.<>c__DisplayClass182_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.currentHull == null || this.character.CurrentHull == null)
			{
				return new ValueTuple<float, float>(float.PositiveInfinity, float.PositiveInfinity);
			}
			CS$<>8__locals1.surfaceY = this.currentHull.Surface;
			float ceilingY = (float)this.currentHull.Rect.Y;
			CS$<>8__locals1.surfaceThreshold = ConvertUnits.ToDisplayUnits(this.Collider.SimPosition.Y + 1f);
			if ((float)this.currentHull.Rect.Y - this.currentHull.Surface < 5f)
			{
				this.<GetWaterSurfaceAndCeilingY>g__GetSurfacePos|182_0(this.currentHull, ref CS$<>8__locals1.surfaceY, ref ceilingY, ref CS$<>8__locals1);
			}
			return new ValueTuple<float, float>(CS$<>8__locals1.surfaceY, ceilingY);
		}

		// Token: 0x06001476 RID: 5238 RVA: 0x000B6084 File Offset: 0x000B4284
		public void SetPosition(Vector2 simPosition, bool lerp = false, bool ignorePlatforms = true, bool forceMainLimbToCollider = false, bool moveLatchers = true)
		{
			if (!MathUtils.IsValid(simPosition))
			{
				string[] array = new string[6];
				array[0] = "Attempted to move a ragdoll (";
				array[1] = this.character.Name;
				array[2] = ") to an invalid position (";
				int num = 3;
				Vector2 vector = simPosition;
				array[num] = vector.ToString();
				array[4] = "). ";
				array[5] = Environment.StackTrace.CleanupStackTrace();
				DebugConsole.ThrowError(string.Concat(array), null, null, false, false);
				string identifier = "Ragdoll.SetPosition:InvalidPosition";
				GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
				string[] array2 = new string[6];
				array2[0] = "Attempted to move a ragdoll (";
				array2[1] = this.character.SpeciesName.ToString();
				array2[2] = ") to an invalid position (";
				int num2 = 3;
				vector = simPosition;
				array2[num2] = vector.ToString();
				array2[4] = "). ";
				array2[5] = Environment.StackTrace.CleanupStackTrace();
				GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, string.Concat(array2));
				return;
			}
			if (this.MainLimb == null)
			{
				return;
			}
			Vector2 limbMoveAmount = forceMainLimbToCollider ? (simPosition - this.MainLimb.SimPosition) : (simPosition - this.Collider.SimPosition);
			if (limbMoveAmount.LengthSquared() > 100f)
			{
				EnemyAIController enemyAI = this.Character.AIController as EnemyAIController;
				if (enemyAI != null && enemyAI.LatchOntoAI != null && enemyAI.LatchOntoAI.IsAttached)
				{
					Character target = enemyAI.LatchOntoAI.TargetCharacter;
					if (target != null)
					{
						target.Latchers.ForEachMod(delegate(LatchOntoAI l)
						{
							if (l != null)
							{
								l.DeattachFromBody(true, 0f);
							}
						});
						target.Latchers.Clear();
					}
					enemyAI.LatchOntoAI.DeattachFromBody(true, 0f);
				}
			}
			this.Character.Latchers.ForEachMod(delegate(LatchOntoAI l)
			{
				if (l != null)
				{
					l.DeattachFromBody(true, 0f);
				}
			});
			this.Character.Latchers.Clear();
			if (lerp)
			{
				this.Collider.TargetPosition = new Vector2?(simPosition);
				this.Collider.MoveToTargetPosition(true);
			}
			else
			{
				this.Collider.SetTransformIgnoreContacts(simPosition, this.Collider.Rotation, true);
			}
			if (!MathUtils.NearlyEqual(limbMoveAmount, Vector2.Zero, 0.0001f))
			{
				foreach (Limb limb in this.Limbs)
				{
					if (!limb.IsSevered)
					{
						Vector2 movePos = limb.SimPosition + limbMoveAmount;
						this.TrySetLimbPosition(limb, simPosition, movePos, limb.Rotation, lerp, ignorePlatforms);
					}
				}
			}
		}

		// Token: 0x170005E2 RID: 1506
		// (get) Token: 0x06001477 RID: 5239 RVA: 0x000B62FE File Offset: 0x000B44FE
		// (set) Token: 0x06001478 RID: 5240 RVA: 0x000B6306 File Offset: 0x000B4506
		public bool IsHoldingToRope { get; private set; }

		// Token: 0x170005E3 RID: 1507
		// (get) Token: 0x06001479 RID: 5241 RVA: 0x000B630F File Offset: 0x000B450F
		// (set) Token: 0x0600147A RID: 5242 RVA: 0x000B6317 File Offset: 0x000B4517
		public bool IsHangingWithRope { get; private set; }

		// Token: 0x170005E4 RID: 1508
		// (get) Token: 0x0600147B RID: 5243 RVA: 0x000B6320 File Offset: 0x000B4520
		// (set) Token: 0x0600147C RID: 5244 RVA: 0x000B6328 File Offset: 0x000B4528
		public bool IsDraggedWithRope { get; private set; }

		// Token: 0x0600147D RID: 5245 RVA: 0x000B6331 File Offset: 0x000B4531
		public void HangWithRope()
		{
			this.shouldHangWithRope = true;
			this.IsHangingWithRope = true;
			this.ResetPullJoints(null);
			this.onGround = false;
			this.levitatingCollider = false;
		}

		// Token: 0x0600147E RID: 5246 RVA: 0x000B6356 File Offset: 0x000B4556
		public void HoldToRope()
		{
			this.shouldHoldToRope = true;
			this.IsHoldingToRope = true;
		}

		// Token: 0x0600147F RID: 5247 RVA: 0x000B6366 File Offset: 0x000B4566
		public void DragWithRope()
		{
			this.shouldBeDraggedWithRope = true;
			this.IsDraggedWithRope = true;
		}

		// Token: 0x06001480 RID: 5248 RVA: 0x000B6376 File Offset: 0x000B4576
		protected void StopHangingWithRope()
		{
			this.shouldHangWithRope = false;
			this.IsHangingWithRope = false;
		}

		// Token: 0x06001481 RID: 5249 RVA: 0x000B6386 File Offset: 0x000B4586
		protected void StopHoldingToRope()
		{
			this.shouldHoldToRope = false;
			this.IsHoldingToRope = false;
		}

		// Token: 0x06001482 RID: 5250 RVA: 0x000B6396 File Offset: 0x000B4596
		protected void StopGettingDraggedWithRope()
		{
			this.shouldBeDraggedWithRope = false;
			this.IsDraggedWithRope = false;
		}

		// Token: 0x06001483 RID: 5251 RVA: 0x000B63A8 File Offset: 0x000B45A8
		protected void TrySetLimbPosition(Limb limb, Vector2 original, Vector2 simPosition, float rotation, bool lerp = false, bool ignorePlatforms = true)
		{
			Vector2 movePos = simPosition;
			Vector2 prevPosition = limb.body.SimPosition;
			if (Vector2.DistanceSquared(original, simPosition) > 0.0001f)
			{
				Category collisionCategory = Category.Cat1 | Category.Cat8;
				if (!ignorePlatforms)
				{
					collisionCategory |= Category.Cat3;
				}
				Body body = Submarine.PickBody(original, simPosition, null, new Category?(collisionCategory), true, null, false);
				if (body != null)
				{
					movePos = original + (simPosition - original) * Submarine.LastPickedFraction * 0.9f;
				}
			}
			if (lerp)
			{
				limb.body.TargetPosition = new Vector2?(movePos);
				limb.body.TargetRotation = new float?(rotation);
				limb.body.MoveToTargetPosition(true);
			}
			else
			{
				limb.body.SetTransformIgnoreContacts(movePos, rotation, true);
				limb.PullJointWorldAnchorB = limb.PullJointWorldAnchorA;
				limb.PullJointEnabled = false;
			}
			foreach (Projectile attachedProjectile in this.character.AttachedProjectiles)
			{
				if (attachedProjectile.IsAttachedTo(limb.body))
				{
					attachedProjectile.Item.SetTransform(attachedProjectile.Item.SimPosition + (movePos - prevPosition), attachedProjectile.Item.body.Rotation, false, true, null);
				}
			}
		}

		// Token: 0x06001484 RID: 5252 RVA: 0x000B64FC File Offset: 0x000B46FC
		protected void CheckDistFromCollider()
		{
			float allowedDist = Math.Max(Math.Max(this.Collider.Radius, this.Collider.Width), this.Collider.Height) * 2f;
			allowedDist = Math.Max(allowedDist, 1f);
			float resetDist = allowedDist * 5f;
			float obstacleCheckDist = 0.3f;
			Vector2 diff = this.Collider.SimPosition - this.MainLimb.SimPosition;
			float distSqrd = diff.LengthSquared();
			bool shouldReset = distSqrd > resetDist * resetDist;
			if (!shouldReset && distSqrd > obstacleCheckDist * obstacleCheckDist && Timing.TotalTime > this.lastObstacleRayCastTime + 1.0 && Submarine.PickBody(this.Collider.SimPosition, this.MainLimb.SimPosition, null, new Category?(Category.Cat1), true, null, false) != null)
			{
				shouldReset = true;
				this.lastObstacleRayCastTime = Timing.TotalTime;
			}
			if (shouldReset)
			{
				this.SetPosition(this.Collider.SimPosition, true, true, true, true);
				return;
			}
			if (distSqrd > allowedDist * allowedDist)
			{
				Vector2 forceDir = diff / (float)Math.Sqrt((double)distSqrd);
				foreach (Limb limb in this.Limbs)
				{
					if (!limb.IsSevered)
					{
						limb.body.CollidesWith = Category.None;
						limb.body.ApplyForce(forceDir * limb.Mass * 10f, 10f);
					}
				}
				this.collisionsDisabled = true;
				return;
			}
			if (this.collisionsDisabled)
			{
				this.SetPosition(this.Collider.SimPosition, true, true, false, true);
				this.collisionsDisabled = false;
				this.prevCollisionCategory = Category.None;
			}
		}

		// Token: 0x06001485 RID: 5253 RVA: 0x000B66A0 File Offset: 0x000B48A0
		private void UpdateNetPlayerPosition(float deltaTime)
		{
			if (GameMain.NetworkMember == null)
			{
				return;
			}
			if (Submarine.Loaded.Any<Submarine>())
			{
				float lowestSubPos = ConvertUnits.ToSimUnits(Submarine.Loaded.Min((Submarine s) => s.HiddenSubPosition.Y - (float)s.Borders.Height - 128f));
				for (int i = 0; i < this.character.MemState.Count; i++)
				{
					if (this.character.Submarine == null)
					{
						if (this.character.MemState[i].Position.Y > lowestSubPos)
						{
							this.character.MemState[i].TransformInToOutside();
						}
					}
					else
					{
						Hull hull = this.currentHull;
						if (((hull != null) ? hull.Submarine : null) != null && this.character.MemState[i].Position.Y < lowestSubPos)
						{
							this.character.MemState[i].TransformOutToInside(this.currentHull.Submarine);
						}
					}
				}
			}
		}

		// Token: 0x06001486 RID: 5254 RVA: 0x000B67B0 File Offset: 0x000B49B0
		public Limb GetLimb(LimbType limbType, bool excludeSevered = true, bool excludeLimbsWithSecondaryType = false, bool useSecondaryType = false)
		{
			Limb limb = null;
			if (!this.HasMultipleLimbsOfSameType && !useSecondaryType && !excludeLimbsWithSecondaryType && this.limbDictionary.TryGetValue(limbType, out limb))
			{
				if (limb.Removed)
				{
					limb = null;
				}
				if (excludeSevered && limb != null && limb.IsSevered)
				{
					limb = null;
				}
			}
			if (limb == null)
			{
				foreach (Limb i in this.limbs)
				{
					if (!i.Removed)
					{
						if (useSecondaryType)
						{
							if (i.Params.SecondaryType != limbType)
							{
								goto IL_91;
							}
						}
						else if (i.type != limbType)
						{
							goto IL_91;
						}
						if ((!excludeSevered || !i.IsSevered) && (!excludeLimbsWithSecondaryType || i.Params.SecondaryType == LimbType.None))
						{
							limb = i;
							break;
						}
					}
					IL_91:;
				}
			}
			return limb;
		}

		// Token: 0x06001487 RID: 5255 RVA: 0x000B685C File Offset: 0x000B4A5C
		public Vector2? GetMouthPosition()
		{
			Limb mouthLimb = this.GetLimb(LimbType.Head, true, false, false);
			if (mouthLimb == null)
			{
				return null;
			}
			float cos = (float)Math.Cos((double)mouthLimb.Rotation);
			float sin = (float)Math.Sin((double)mouthLimb.Rotation);
			Vector2 bodySize = mouthLimb.body.GetSize();
			Vector2 offset = new Vector2(mouthLimb.MouthPos.X * bodySize.X / 2f, mouthLimb.MouthPos.Y * bodySize.Y / 2f);
			return new Vector2?(mouthLimb.SimPosition + new Vector2(offset.X * cos - offset.Y * sin, offset.X * sin + offset.Y * cos));
		}

		// Token: 0x06001488 RID: 5256 RVA: 0x000B6920 File Offset: 0x000B4B20
		public Vector2 GetColliderBottom()
		{
			float offset = 0f;
			if (!this.character.IsDead && this.character.Stun <= 0f && !this.character.IsIncapacitated)
			{
				offset = -this.ColliderHeightFromFloor;
			}
			float lowestBound = this.Collider.SimPosition.Y;
			if (this.Collider.FarseerBody.FixtureList != null)
			{
				for (int i = 0; i < this.Collider.FarseerBody.FixtureList.Count; i++)
				{
					Transform transform;
					this.Collider.FarseerBody.GetTransform(out transform);
					AABB aabb;
					this.Collider.FarseerBody.FixtureList[i].Shape.ComputeAABB(out aabb, ref transform, i);
					lowestBound = Math.Min(aabb.LowerBound.Y, lowestBound);
				}
			}
			return new Vector2(this.Collider.SimPosition.X, lowestBound + offset);
		}

		// Token: 0x06001489 RID: 5257 RVA: 0x000B6A0C File Offset: 0x000B4C0C
		public Limb FindLowestLimb()
		{
			Limb lowestLimb = null;
			foreach (Limb limb in this.Limbs)
			{
				if (!limb.IsSevered)
				{
					if (lowestLimb == null)
					{
						lowestLimb = limb;
					}
					else if (limb.SimPosition.Y < lowestLimb.SimPosition.Y)
					{
						lowestLimb = limb;
					}
				}
			}
			return lowestLimb;
		}

		// Token: 0x0600148A RID: 5258 RVA: 0x000B6A5E File Offset: 0x000B4C5E
		public void ReleaseStuckLimbs()
		{
		}

		// Token: 0x0600148B RID: 5259 RVA: 0x000B6A60 File Offset: 0x000B4C60
		public void HideAndDisable(LimbType limbType, float duration = 0f, bool ignoreCollisions = true)
		{
			foreach (Limb limb in this.Limbs)
			{
				if (limb.type == limbType)
				{
					limb.HideAndDisable(duration, ignoreCollisions);
				}
			}
		}

		// Token: 0x0600148C RID: 5260 RVA: 0x000B6A97 File Offset: 0x000B4C97
		public void RestoreTemporarilyDisabled()
		{
			this.Limbs.ForEach(delegate(Limb l)
			{
				l.ReEnable();
			});
		}

		// Token: 0x0600148D RID: 5261 RVA: 0x000B6AC4 File Offset: 0x000B4CC4
		public void Remove()
		{
			if (this.Limbs != null)
			{
				foreach (Limb i in this.Limbs)
				{
					if (i != null)
					{
						i.Remove();
					}
				}
				this.limbs = null;
			}
			this.limbBodies.Clear();
			if (this.collider != null)
			{
				foreach (PhysicsBody b in this.collider)
				{
					if (b != null)
					{
						b.Remove();
					}
				}
				this.collider = null;
			}
			if (this.LimbJoints != null)
			{
				foreach (LimbJoint joint in this.LimbJoints)
				{
					Joint j = (joint != null) ? joint.Joint : null;
					if (GameMain.World.JointList.Contains(j))
					{
						GameMain.World.Remove(j);
					}
				}
				this.LimbJoints = null;
			}
			Ragdoll.list.Remove(this);
		}

		// Token: 0x0600148E RID: 5262 RVA: 0x000B6BD4 File Offset: 0x000B4DD4
		public static void RemoveAll()
		{
			for (int i = Ragdoll.list.Count - 1; i >= 0; i--)
			{
				Ragdoll.list[i].Remove();
			}
		}

		// Token: 0x06001490 RID: 5264 RVA: 0x000B6C14 File Offset: 0x000B4E14
		[CompilerGenerated]
		internal static bool <get_MainLimb>g__IsValid|74_0(Limb limb)
		{
			return limb != null && !limb.IsSevered && !limb.IgnoreCollisions && !limb.Hidden;
		}

		// Token: 0x06001492 RID: 5266 RVA: 0x000B6C40 File Offset: 0x000B4E40
		[CompilerGenerated]
		private bool <AddJoint>g__checkLimbIndex|125_0(int index, string debugName, ref Ragdoll.<>c__DisplayClass125_0 A_3)
		{
			if (index < 0 || index >= this.limbs.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 4);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to add a joint to character ");
				defaultInterpolatedStringHandler.AppendFormatted(this.character.Name);
				defaultInterpolatedStringHandler.AppendLiteral(". ");
				defaultInterpolatedStringHandler.AppendFormatted(debugName);
				defaultInterpolatedStringHandler.AppendLiteral(" out of bounds (index: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(index);
				defaultInterpolatedStringHandler.AppendLiteral(", limbs: ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.limbs.Length);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
				string error = errorMsg;
				Exception e = null;
				ContentXElement element = A_3.jointParams.Element;
				DebugConsole.ThrowError(error, e, (element != null) ? element.ContentPackage : null, false, false);
				ContentXElement element2 = A_3.jointParams.Element;
				if (((element2 != null) ? element2.ContentPackage : null) == GameMain.VanillaContent)
				{
					GameAnalyticsManager.AddErrorEventOnce("Ragdoll.AddJoint:IndexOutOfRange", GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				}
				return false;
			}
			return true;
		}

		// Token: 0x06001493 RID: 5267 RVA: 0x000B6D2C File Offset: 0x000B4F2C
		[CompilerGenerated]
		private Ragdoll.LimbStairCollisionResponse <OnLimbCollision>g__getStairCollisionResponse|130_0(ref Ragdoll.<>c__DisplayClass130_0 A_1)
		{
			float stairBottomPos = ConvertUnits.ToSimUnits(A_1.structure.Rect.Y - A_1.structure.Rect.Height + 10);
			if (A_1.colliderBottom.Y < stairBottomPos && this.targetMovement.Y < 0.5f)
			{
				return Ragdoll.LimbStairCollisionResponse.DontClimbStairs;
			}
			if (this.character.SelectedBy != null && this.character.SelectedBy.AnimController.GetColliderBottom().Y < stairBottomPos && this.character.SelectedBy.AnimController.targetMovement.Y < 0.5f)
			{
				return Ragdoll.LimbStairCollisionResponse.DontClimbStairs;
			}
			if (this.targetMovement.Y >= 0f && A_1.colliderBottom.Y >= ConvertUnits.ToSimUnits((float)A_1.structure.Rect.Y - Submarine.GridSize.Y * 5f))
			{
				return Ragdoll.LimbStairCollisionResponse.DontClimbStairs;
			}
			if (A_1.contact.Manifold.LocalNormal.Y < 0f)
			{
				if (this.Stairs == A_1.structure)
				{
					return Ragdoll.LimbStairCollisionResponse.ClimbWithoutLimbCollision;
				}
				return Ragdoll.LimbStairCollisionResponse.DontClimbStairs;
			}
			else
			{
				Vector2 vector;
				FixedArray2<Vector2> points;
				A_1.contact.GetWorldManifold(out vector, out points);
				if (points[0].Y > this.Collider.SimPosition.Y)
				{
					return Ragdoll.LimbStairCollisionResponse.DontClimbStairs;
				}
				if (this.inWater && this.targetMovement.Y < 0.5f)
				{
					return Ragdoll.LimbStairCollisionResponse.DontClimbStairs;
				}
				return Ragdoll.LimbStairCollisionResponse.ClimbWithLimbCollision;
			}
		}

		// Token: 0x06001496 RID: 5270 RVA: 0x000B6EE8 File Offset: 0x000B50E8
		[CompilerGenerated]
		internal static string <CheckValidity>g__GetBodyName|171_0(ref Ragdoll.<>c__DisplayClass171_0 A_0)
		{
			Limb limb = A_0.body.UserData as Limb;
			if (limb == null)
			{
				return "Collider";
			}
			return "Limb (" + limb.type.ToString() + ")";
		}

		// Token: 0x06001497 RID: 5271 RVA: 0x000B6F34 File Offset: 0x000B5134
		[CompilerGenerated]
		private void <GetWaterSurfaceAndCeilingY>g__GetSurfacePos|182_0(Hull hull, ref float prevSurfacePos, ref float ceilingPos, ref Ragdoll.<>c__DisplayClass182_0 A_4)
		{
			if (prevSurfacePos > A_4.surfaceThreshold)
			{
				return;
			}
			foreach (Gap gap in hull.ConnectedGaps)
			{
				if (!gap.IsHorizontal && gap.Open > 0f && gap.WorldPosition.Y >= hull.WorldPosition.Y && this.Collider.SimPosition.X >= ConvertUnits.ToSimUnits(gap.Rect.X) && this.Collider.SimPosition.X <= ConvertUnits.ToSimUnits(gap.Rect.Right))
				{
					if (!gap.IsRoomToRoom && gap.Position.Y > hull.Position.Y)
					{
						ceilingPos += 100000f;
						prevSurfacePos += 100000f;
						break;
					}
					foreach (MapEntity linkedTo in gap.linkedTo)
					{
						Hull otherHull = linkedTo as Hull;
						if (otherHull != null && otherHull != hull && otherHull != this.currentHull)
						{
							prevSurfacePos = Math.Max(A_4.surfaceY, otherHull.Surface);
							ceilingPos = Math.Max(ceilingPos, (float)otherHull.Rect.Y);
							this.<GetWaterSurfaceAndCeilingY>g__GetSurfacePos|182_0(otherHull, ref prevSurfacePos, ref ceilingPos, ref A_4);
							break;
						}
					}
				}
			}
		}

		// Token: 0x04000975 RID: 2421
		private const float ImpactDamageMultiplayer = 10f;

		// Token: 0x04000976 RID: 2422
		private const float MaxImpactDamage = 0.1f;

		// Token: 0x04000977 RID: 2423
		private static readonly List<Ragdoll> list = new List<Ragdoll>();

		// Token: 0x04000978 RID: 2424
		private readonly Queue<Ragdoll.Impact> impactQueue = new Queue<Ragdoll.Impact>();

		// Token: 0x04000979 RID: 2425
		protected Hull currentHull;

		// Token: 0x0400097A RID: 2426
		private bool accessRemovedCharacterErrorShown;

		// Token: 0x0400097B RID: 2427
		private Limb[] limbs;

		// Token: 0x0400097C RID: 2428
		private readonly List<Body> limbBodies = new List<Body>();

		// Token: 0x0400097D RID: 2429
		private bool frozen;

		// Token: 0x0400097E RID: 2430
		private Dictionary<LimbType, Limb> limbDictionary;

		// Token: 0x0400097F RID: 2431
		public LimbJoint[] LimbJoints;

		// Token: 0x04000980 RID: 2432
		private bool simplePhysicsEnabled;

		// Token: 0x04000981 RID: 2433
		protected readonly Character character;

		// Token: 0x04000982 RID: 2434
		protected float strongestImpact;

		// Token: 0x04000983 RID: 2435
		private float splashSoundTimer;

		// Token: 0x04000984 RID: 2436
		private float flowForceTolerance;

		// Token: 0x04000985 RID: 2437
		private float flowStunTolerance;

		// Token: 0x04000986 RID: 2438
		public Vector2 movement;

		// Token: 0x04000987 RID: 2439
		protected Vector2 targetMovement;

		// Token: 0x04000988 RID: 2440
		protected Vector2? overrideTargetMovement;

		// Token: 0x04000989 RID: 2441
		protected float floorY;

		// Token: 0x0400098A RID: 2442
		protected float standOnFloorY;

		// Token: 0x0400098B RID: 2443
		protected Fixture floorFixture;

		// Token: 0x0400098C RID: 2444
		protected Vector2 floorNormal = Vector2.UnitY;

		// Token: 0x0400098D RID: 2445
		protected float surfaceY;

		// Token: 0x0400098E RID: 2446
		protected bool inWater;

		// Token: 0x0400098F RID: 2447
		protected bool headInWater;

		// Token: 0x04000990 RID: 2448
		protected bool onGround;

		// Token: 0x04000991 RID: 2449
		private Vector2 lastFloorCheckPos;

		// Token: 0x04000992 RID: 2450
		private bool lastFloorCheckIgnoreStairs;

		// Token: 0x04000993 RID: 2451
		private bool lastFloorCheckIgnorePlatforms;

		// Token: 0x04000994 RID: 2452
		public Structure Stairs;

		// Token: 0x04000995 RID: 2453
		protected Direction dir;

		// Token: 0x04000996 RID: 2454
		public Direction TargetDir;

		// Token: 0x04000997 RID: 2455
		protected List<PhysicsBody> collider;

		// Token: 0x04000998 RID: 2456
		protected int colliderIndex;

		// Token: 0x04000999 RID: 2457
		private Category prevCollisionCategory;

		// Token: 0x0400099B RID: 2459
		public const float MAX_SPEED = 20f;

		// Token: 0x0400099C RID: 2460
		private float? impactTolerance;

		// Token: 0x0400099E RID: 2462
		private readonly List<Limb> connectedLimbs = new List<Limb>();

		// Token: 0x0400099F RID: 2463
		private readonly List<LimbJoint> checkedJoints = new List<LimbJoint>();

		// Token: 0x040009A1 RID: 2465
		protected bool levitatingCollider = true;

		// Token: 0x040009A2 RID: 2466
		private float bodyInRestTimer;

		// Token: 0x040009A3 RID: 2467
		private float BodyInRestDelay = 1f;

		// Token: 0x040009A4 RID: 2468
		public bool forceStanding;

		// Token: 0x040009A5 RID: 2469
		public bool forceNotStanding;

		// Token: 0x040009A7 RID: 2471
		private int validityResets;

		// Token: 0x040009A8 RID: 2472
		private const float FloorYStaleTime = 1f;

		// Token: 0x040009A9 RID: 2473
		private float floorYCheckTimer;

		// Token: 0x040009AB RID: 2475
		protected bool shouldHoldToRope;

		// Token: 0x040009AD RID: 2477
		protected bool shouldHangWithRope;

		// Token: 0x040009AF RID: 2479
		protected bool shouldBeDraggedWithRope;

		// Token: 0x040009B0 RID: 2480
		private bool collisionsDisabled;

		// Token: 0x040009B1 RID: 2481
		private double lastObstacleRayCastTime;

		// Token: 0x0200085F RID: 2143
		private struct Impact
		{
			// Token: 0x0600549B RID: 21659 RVA: 0x001F0CA0 File Offset: 0x001EEEA0
			public Impact(Fixture f1, Fixture f2, Contact contact, Vector2 velocity)
			{
				this.F1 = f1;
				this.F2 = f2;
				this.Velocity = velocity;
				this.LocalNormal = contact.Manifold.LocalNormal;
				FixedArray2<Vector2> points;
				contact.GetWorldManifold(out this.WorldNormal, out points);
				this.ImpactPos = points[0];
			}

			// Token: 0x04002F6D RID: 12141
			public Fixture F1;

			// Token: 0x04002F6E RID: 12142
			public Fixture F2;

			// Token: 0x04002F6F RID: 12143
			public Vector2 LocalNormal;

			// Token: 0x04002F70 RID: 12144
			public Vector2 WorldNormal;

			// Token: 0x04002F71 RID: 12145
			public Vector2 Velocity;

			// Token: 0x04002F72 RID: 12146
			public Vector2 ImpactPos;
		}

		// Token: 0x02000860 RID: 2144
		private enum LimbStairCollisionResponse
		{
			// Token: 0x04002F74 RID: 12148
			DontClimbStairs,
			// Token: 0x04002F75 RID: 12149
			ClimbWithoutLimbCollision,
			// Token: 0x04002F76 RID: 12150
			ClimbWithLimbCollision
		}
	}
}

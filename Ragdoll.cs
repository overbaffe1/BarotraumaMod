using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.CharacterEditor;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.SpriteDeformations;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000025 RID: 37
	internal abstract class Ragdoll
	{
		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x0600030A RID: 778 RVA: 0x0001B99C File Offset: 0x00019B9C
		// (set) Token: 0x0600030B RID: 779 RVA: 0x0001B9A4 File Offset: 0x00019BA4
		public HashSet<SpriteDeformation> SpriteDeformations { get; protected set; } = new HashSet<SpriteDeformation>();

		// Token: 0x0600030C RID: 780 RVA: 0x0001B9B0 File Offset: 0x00019BB0
		private void TryPlatformCorrection(Vector2 serverPos)
		{
			float highestPos = (from l in this.limbs
			where !l.IsSevered
			select l).Max((Limb l) => l.SimPosition.Y);
			highestPos = Math.Max(serverPos.Y, highestPos);
			float lowestPos = (from l in this.limbs
			where !l.IsSevered
			select l).Min((Limb l) => l.SimPosition.Y);
			lowestPos = Math.Min(serverPos.Y, lowestPos);
			Body platform = Submarine.PickBody(new Vector2(serverPos.X, highestPos), new Vector2(serverPos.X, lowestPos), null, new Category?(Category.Cat3), true, null, true);
			if (platform == null)
			{
				return;
			}
			int serverDir = Math.Sign(serverPos.Y - platform.Position.Y);
			foreach (Limb limb in this.limbs)
			{
				if (!limb.IsSevered)
				{
					int limbDir = Math.Sign(limb.SimPosition.Y - platform.Position.Y);
					if (limbDir != serverDir)
					{
						limb.body.SetTransformIgnoreContacts(new Vector2(limb.SimPosition.X, (serverDir > 0) ? Math.Max(serverPos.Y + 0.01f + limb.body.GetMaxExtent(), limb.SimPosition.Y) : Math.Min(serverPos.Y - 0.01f - limb.body.GetMaxExtent(), limb.SimPosition.Y)), limb.Rotation, true);
					}
				}
			}
		}

		// Token: 0x0600030D RID: 781 RVA: 0x0001BB94 File Offset: 0x00019D94
		public void PlayImpactSound(Limb limb)
		{
			limb.LastImpactSoundTime = (float)Timing.TotalTime;
			if (!string.IsNullOrWhiteSpace(limb.HitSoundTag))
			{
				bool inWater = limb.InWater;
				if (this.character.CurrentHull != null && this.character.CurrentHull.Surface > (float)(this.character.CurrentHull.Rect.Y - this.character.CurrentHull.Rect.Height) + 5f && limb.SimPosition.Y < ConvertUnits.ToSimUnits(this.character.CurrentHull.Rect.Y - this.character.CurrentHull.Rect.Height) + limb.body.GetMaxExtent())
				{
					inWater = true;
				}
				string soundTag = inWater ? "footstep_water" : limb.HitSoundTag;
				Vector2 worldPosition = limb.WorldPosition;
				Hull hullGuess = this.character.CurrentHull;
				SoundPlayer.PlaySound(soundTag, worldPosition, null, null, hullGuess);
			}
			foreach (WearableSprite wearable in limb.WearingItems)
			{
				if (limb.type == wearable.Limb && !string.IsNullOrWhiteSpace(wearable.Sound))
				{
					string sound = wearable.Sound;
					Vector2 worldPosition2 = limb.WorldPosition;
					Hull hullGuess = this.character.CurrentHull;
					SoundPlayer.PlaySound(sound, worldPosition2, null, null, hullGuess);
				}
			}
		}

		// Token: 0x0600030E RID: 782 RVA: 0x0001BD30 File Offset: 0x00019F30
		public void Draw(SpriteBatch spriteBatch, Camera cam, bool onlyDrawSeveredLimbs)
		{
			if (this.simplePhysicsEnabled)
			{
				return;
			}
			this.Collider.UpdateDrawPosition(true);
			if (this.Limbs == null)
			{
				DebugConsole.ThrowError(string.Concat(new string[]
				{
					"Failed to draw a ragdoll, limbs have been removed. Character: \"",
					this.character.Name,
					"\", removed: ",
					this.character.Removed.ToString(),
					"\n",
					Environment.StackTrace.CleanupStackTrace()
				}), null, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("Ragdoll.Draw:LimbsRemoved", GameAnalyticsManager.ErrorSeverity.Error, string.Concat(new string[]
				{
					"Failed to draw a ragdoll, limbs have been removed. Character: \"",
					this.character.SpeciesName.ToString(),
					"\", removed: ",
					this.character.Removed.ToString(),
					"\n",
					Environment.StackTrace.CleanupStackTrace()
				}));
				return;
			}
			Color? color = null;
			if (this.character.ExternalHighlight)
			{
				color = new Color?(Color.Lerp(Color.White, GUIStyle.Orange, (float)Math.Sin(Timing.TotalTime * 3.5)));
			}
			float depthOffset = this.GetDepthOffset();
			if (!MathUtils.NearlyEqual(depthOffset, 0f, 0.0001f))
			{
				foreach (Limb limb in this.limbs)
				{
					limb.ActiveSprite.Depth += depthOffset;
				}
			}
			for (int i = 0; i < this.inversedLimbDrawOrder.Length; i++)
			{
				if (!onlyDrawSeveredLimbs || this.inversedLimbDrawOrder[i].IsSevered)
				{
					this.inversedLimbDrawOrder[i].Draw(spriteBatch, cam, color, false);
				}
			}
			if (!MathUtils.NearlyEqual(depthOffset, 0f, 0.0001f))
			{
				foreach (Limb limb2 in this.limbs)
				{
					limb2.ActiveSprite.Depth -= depthOffset;
				}
			}
			this.LimbJoints.ForEach(delegate(LimbJoint j)
			{
				j.Draw(spriteBatch);
			});
		}

		// Token: 0x0600030F RID: 783 RVA: 0x0001BF6C File Offset: 0x0001A16C
		public float GetDepthOffset()
		{
			Ragdoll.<>c__DisplayClass8_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.maxDepth = 0f;
			CS$<>8__locals1.minDepth = 1f;
			CS$<>8__locals1.depthOffset = 0f;
			Item selectedSecondaryItem = this.character.SelectedSecondaryItem;
			Ladder ladder = (selectedSecondaryItem != null) ? selectedSecondaryItem.GetComponent<Ladder>() : null;
			if (ladder != null)
			{
				this.<GetDepthOffset>g__CalculateLimbDepths|8_1(ref CS$<>8__locals1);
				if (this.character.WorldPosition.X < this.character.SelectedSecondaryItem.WorldPosition.X)
				{
					if (CS$<>8__locals1.maxDepth > ladder.BackgroundSpriteDepth)
					{
						CS$<>8__locals1.depthOffset = Math.Max(ladder.BackgroundSpriteDepth - 0.01f - CS$<>8__locals1.maxDepth, 0f);
					}
					else
					{
						CS$<>8__locals1.depthOffset = Math.Max(ladder.Item.GetDrawDepth() + 0.0001f - CS$<>8__locals1.minDepth, -CS$<>8__locals1.minDepth);
					}
				}
				else
				{
					CS$<>8__locals1.depthOffset = Math.Max(ladder.BackgroundSpriteDepth + 0.01f - CS$<>8__locals1.minDepth, 0f);
				}
			}
			else
			{
				this.<GetDepthOffset>g__CalculateLimbDepths|8_1(ref CS$<>8__locals1);
				this.<GetDepthOffset>g__AdjustDepthOffset|8_0(this.character.SelectedItem, ref CS$<>8__locals1);
				this.<GetDepthOffset>g__AdjustDepthOffset|8_0(this.character.SelectedSecondaryItem, ref CS$<>8__locals1);
			}
			return CS$<>8__locals1.depthOffset;
		}

		// Token: 0x06000310 RID: 784 RVA: 0x0001C0B0 File Offset: 0x0001A2B0
		public void DebugDraw(SpriteBatch spriteBatch)
		{
			if (GameMain.DebugDraw && this.character.Enabled)
			{
				Screen selected = Screen.Selected;
				Camera camera = (selected != null) ? selected.Cam : null;
				if (camera == null || camera.Zoom >= 0.2f)
				{
					if (this.simplePhysicsEnabled)
					{
						return;
					}
					foreach (Limb limb in this.Limbs)
					{
						if (limb.PullJointEnabled)
						{
							Vector2 pos = ConvertUnits.ToDisplayUnits(limb.PullJointWorldAnchorB);
							Hull hull = this.currentHull;
							if (((hull != null) ? hull.Submarine : null) != null)
							{
								pos += this.currentHull.Submarine.DrawPosition;
							}
							pos.Y = -pos.Y;
							GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, 5, 5), GUIStyle.Red, true, 0.01f, 1f);
							pos = ConvertUnits.ToDisplayUnits(limb.PullJointWorldAnchorA);
							Hull hull2 = this.currentHull;
							if (((hull2 != null) ? hull2.Submarine : null) != null)
							{
								pos += this.currentHull.Submarine.DrawPosition;
							}
							pos.Y = -pos.Y;
							GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos.X, (int)pos.Y, 5, 5), Color.Cyan, true, 0.01f, 1f);
						}
						limb.body.DebugDraw(spriteBatch, this.inWater ? ((this.currentHull == null) ? Color.Blue : Color.Cyan) : Color.White, false);
					}
					this.Collider.DebugDraw(spriteBatch, this.frozen ? GUIStyle.Red : (this.inWater ? Color.SkyBlue : Color.Gray), false);
					GUIStyle.Font.DrawString(spriteBatch, this.Collider.LinearVelocity.X.FormatSingleDecimal(), new Vector2(this.Collider.DrawPosition.X, -this.Collider.DrawPosition.Y), Color.Orange, ForceUpperCase.Inherit, false);
					foreach (LimbJoint joint in this.LimbJoints)
					{
						Vector2 pos2 = ConvertUnits.ToDisplayUnits(joint.WorldAnchorA);
						GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos2.X, (int)(-(int)pos2.Y), 5, 5), Color.White, true, 0f, 1f);
						pos2 = ConvertUnits.ToDisplayUnits(joint.WorldAnchorB);
						GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos2.X, (int)(-(int)pos2.Y), 5, 5), Color.White, true, 0f, 1f);
					}
					foreach (Limb limb2 in this.Limbs)
					{
						if (limb2.body.TargetPosition != null)
						{
							Vector2 pos3 = ConvertUnits.ToDisplayUnits(limb2.body.TargetPosition.Value);
							Hull hull3 = this.currentHull;
							if (((hull3 != null) ? hull3.Submarine : null) != null)
							{
								pos3 += this.currentHull.Submarine.DrawPosition;
							}
							pos3.Y = -pos3.Y;
							GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos3.X - 10, (int)pos3.Y - 10, 20, 20), Color.Cyan, false, 0.01f, 1f);
							GUI.DrawLine(spriteBatch, pos3, new Vector2(limb2.WorldPosition.X, -limb2.WorldPosition.Y), Color.Cyan, 0f, 1f);
						}
					}
					HumanoidAnimController humanoid = this as HumanoidAnimController;
					if (humanoid != null)
					{
						Vector2 pos4 = ConvertUnits.ToDisplayUnits(humanoid.RightHandIKPos);
						if (humanoid.character.Submarine != null)
						{
							pos4 += humanoid.character.Submarine.DrawPosition;
						}
						GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos4.X, (int)(-(int)pos4.Y), 4, 4), GUIStyle.Green, true, 0f, 1f);
						pos4 = ConvertUnits.ToDisplayUnits(humanoid.LeftHandIKPos);
						if (humanoid.character.Submarine != null)
						{
							pos4 += humanoid.character.Submarine.DrawPosition;
						}
						GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos4.X, (int)(-(int)pos4.Y), 4, 4), GUIStyle.Green, true, 0f, 1f);
						Vector2 aimPos = humanoid.AimSourceWorldPos;
						aimPos.Y = -aimPos.Y;
						GUI.DrawLine(spriteBatch, aimPos - Vector2.UnitY * 3f, aimPos + Vector2.UnitY * 3f, Color.Red, 0f, 1f);
						GUI.DrawLine(spriteBatch, aimPos - Vector2.UnitX * 3f, aimPos + Vector2.UnitX * 3f, Color.Red, 0f, 1f);
					}
					if (this.character.MemState.Count > 1)
					{
						Vector2 prevPos = ConvertUnits.ToDisplayUnits(this.character.MemState[0].Position);
						Hull hull4 = this.currentHull;
						if (((hull4 != null) ? hull4.Submarine : null) != null)
						{
							prevPos += this.currentHull.Submarine.DrawPosition;
						}
						prevPos.Y = -prevPos.Y;
						for (int i = 1; i < this.character.MemState.Count; i++)
						{
							Vector2 currPos = ConvertUnits.ToDisplayUnits(this.character.MemState[i].Position);
							Hull hull5 = this.currentHull;
							if (((hull5 != null) ? hull5.Submarine : null) != null)
							{
								currPos += this.currentHull.Submarine.DrawPosition;
							}
							currPos.Y = -currPos.Y;
							GUI.DrawRectangle(spriteBatch, new Rectangle((int)currPos.X - 3, (int)currPos.Y - 3, 6, 6), Color.Cyan * 0.6f, true, 0.01f, 1f);
							GUI.DrawLine(spriteBatch, prevPos, currPos, Color.Cyan * 0.6f, 0f, 3f);
							prevPos = currPos;
						}
					}
					if (this.currentHull != null)
					{
						Vector2 displayFloorPos = ConvertUnits.ToDisplayUnits(new Vector2(this.Collider.SimPosition.X, this.floorY));
						Hull hull6 = this.currentHull;
						if (((hull6 != null) ? hull6.Submarine : null) != null)
						{
							displayFloorPos += this.currentHull.Submarine.DrawPosition;
						}
						displayFloorPos.Y = -displayFloorPos.Y;
						GUI.DrawLine(spriteBatch, displayFloorPos, displayFloorPos + new Vector2(this.floorNormal.X, -this.floorNormal.Y) * 50f, Color.Cyan * 0.5f, 0f, 2f);
					}
					if (this.IgnorePlatforms)
					{
						GUI.DrawLine(spriteBatch, new Vector2(this.Collider.DrawPosition.X, -this.Collider.DrawPosition.Y), new Vector2(this.Collider.DrawPosition.X, -this.Collider.DrawPosition.Y + 50f), Color.Orange, 0f, 5f);
					}
					return;
				}
			}
		}

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000311 RID: 785
		// (set) Token: 0x06000312 RID: 786
		public abstract RagdollParams RagdollParams { get; protected set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000313 RID: 787 RVA: 0x0001C85B File Offset: 0x0001AA5B
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

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000314 RID: 788 RVA: 0x0001C877 File Offset: 0x0001AA77
		public IEnumerable<Body> LimbBodies
		{
			get
			{
				return this.limbBodies;
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000315 RID: 789 RVA: 0x0001C87F File Offset: 0x0001AA7F
		public bool HasMultipleLimbsOfSameType
		{
			get
			{
				return this.limbs != null && this.limbs.Length > this.limbDictionary.Count;
			}
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000316 RID: 790 RVA: 0x0001C8A0 File Offset: 0x0001AAA0
		// (set) Token: 0x06000317 RID: 791 RVA: 0x0001C8A8 File Offset: 0x0001AAA8
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

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000318 RID: 792 RVA: 0x0001C94A File Offset: 0x0001AB4A
		public Character Character
		{
			get
			{
				return this.character;
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000319 RID: 793 RVA: 0x0001C952 File Offset: 0x0001AB52
		public bool OnGround
		{
			get
			{
				return this.onGround;
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x0600031A RID: 794 RVA: 0x0001C95A File Offset: 0x0001AB5A
		public float ColliderHeightFromFloor
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.RagdollParams.ColliderHeightFromFloor) * this.RagdollParams.JointScale;
			}
		}

		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x0600031B RID: 795 RVA: 0x0001C978 File Offset: 0x0001AB78
		public bool ColliderControlsMovement
		{
			get
			{
				return this.character.CanMove;
			}
		}

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x0600031C RID: 796 RVA: 0x0001C985 File Offset: 0x0001AB85
		public bool IsStuck
		{
			get
			{
				return this.Limbs.Any((Limb l) => l.IsStuck);
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x0600031D RID: 797 RVA: 0x0001C9B1 File Offset: 0x0001ABB1
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

		// Token: 0x0600031E RID: 798 RVA: 0x0001C9CC File Offset: 0x0001ABCC
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

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600031F RID: 799 RVA: 0x0001CA0C File Offset: 0x0001AC0C
		// (set) Token: 0x06000320 RID: 800 RVA: 0x0001CA14 File Offset: 0x0001AC14
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

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0001CC59 File Offset: 0x0001AE59
		public float FloorY
		{
			get
			{
				return this.floorY;
			}
		}

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x06000322 RID: 802 RVA: 0x0001CC61 File Offset: 0x0001AE61
		// (set) Token: 0x06000323 RID: 803 RVA: 0x0001CC69 File Offset: 0x0001AE69
		public float Mass { get; private set; }

		// Token: 0x06000324 RID: 804 RVA: 0x0001CC72 File Offset: 0x0001AE72
		public void SubtractMass(Limb limb)
		{
			if (this.limbs.Contains(limb))
			{
				this.Mass -= limb.Mass;
			}
		}

		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x06000325 RID: 805 RVA: 0x0001CC98 File Offset: 0x0001AE98
		public Limb MainLimb
		{
			get
			{
				Limb mainLimb = this.GetLimb(this.RagdollParams.MainLimb, true, false, false);
				if (!Ragdoll.<get_MainLimb>g__IsValid|84_0(mainLimb))
				{
					Limb torso = this.GetLimb(LimbType.Torso, true, false, false);
					Limb head = this.GetLimb(LimbType.Head, true, false, false);
					mainLimb = (torso ?? head);
					if (!Ragdoll.<get_MainLimb>g__IsValid|84_0(mainLimb))
					{
						mainLimb = this.Limbs.FirstOrDefault((Limb l) => Ragdoll.<get_MainLimb>g__IsValid|84_0(l));
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

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000326 RID: 806 RVA: 0x0001CD34 File Offset: 0x0001AF34
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

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x06000327 RID: 807 RVA: 0x0001CD84 File Offset: 0x0001AF84
		// (set) Token: 0x06000328 RID: 808 RVA: 0x0001CD8C File Offset: 0x0001AF8C
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

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x06000329 RID: 809 RVA: 0x0001CEE8 File Offset: 0x0001B0E8
		// (set) Token: 0x0600032A RID: 810 RVA: 0x0001CF14 File Offset: 0x0001B114
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

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x0600032B RID: 811
		public abstract float? HeadPosition { get; }

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x0600032C RID: 812
		public abstract float? HeadAngle { get; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x0600032D RID: 813
		public abstract float? TorsoPosition { get; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x0600032E RID: 814
		public abstract float? TorsoAngle { get; }

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x0600032F RID: 815 RVA: 0x0001CF6C File Offset: 0x0001B16C
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

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000330 RID: 816 RVA: 0x0001D016 File Offset: 0x0001B216
		public bool Draggable
		{
			get
			{
				return this.RagdollParams.Draggable;
			}
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000331 RID: 817 RVA: 0x0001D023 File Offset: 0x0001B223
		public CanEnterSubmarine CanEnterSubmarine
		{
			get
			{
				return this.RagdollParams.CanEnterSubmarine;
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000332 RID: 818 RVA: 0x0001D030 File Offset: 0x0001B230
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

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x06000333 RID: 819 RVA: 0x0001D046 File Offset: 0x0001B246
		public Direction Direction
		{
			get
			{
				return this.dir;
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0001D04E File Offset: 0x0001B24E
		public bool InWater
		{
			get
			{
				return this.inWater;
			}
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000335 RID: 821 RVA: 0x0001D056 File Offset: 0x0001B256
		public bool HeadInWater
		{
			get
			{
				return this.headInWater;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000336 RID: 822 RVA: 0x0001D05E File Offset: 0x0001B25E
		// (set) Token: 0x06000337 RID: 823 RVA: 0x0001D068 File Offset: 0x0001B268
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

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000338 RID: 824 RVA: 0x0001D0D2 File Offset: 0x0001B2D2
		// (set) Token: 0x06000339 RID: 825 RVA: 0x0001D0DA File Offset: 0x0001B2DA
		public bool IgnorePlatforms { get; set; }

		// Token: 0x0600033A RID: 826 RVA: 0x0001D0E4 File Offset: 0x0001B2E4
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

		// Token: 0x0600033B RID: 827 RVA: 0x0001D568 File Offset: 0x0001B768
		public Ragdoll(Character character, string seed, RagdollParams ragdollParams = null)
		{
			Ragdoll.list.Add(this);
			this.character = character;
			this.Recreate(ragdollParams ?? this.RagdollParams);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0001D5F4 File Offset: 0x0001B7F4
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

		// Token: 0x0600033D RID: 829 RVA: 0x0001D728 File Offset: 0x0001B928
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

		// Token: 0x0600033E RID: 830 RVA: 0x0001D838 File Offset: 0x0001BA38
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

		// Token: 0x0600033F RID: 831 RVA: 0x0001D8CC File Offset: 0x0001BACC
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
			if (this.limbs.Contains(null))
			{
				return;
			}
			this.SetupDrawOrder();
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0001D998 File Offset: 0x0001BB98
		private void SetupDrawOrder()
		{
			float startDepth = 0.1f;
			float increment = 0.001f;
			foreach (Character otherCharacter in Character.CharacterList)
			{
				if (otherCharacter != this.character)
				{
					startDepth += increment;
				}
			}
			List<Limb> depthSortedLimbs = (from l in this.Limbs
			orderby l.DefaultSpriteDepth
			select l).ToList<Limb>();
			foreach (Limb limb in this.Limbs)
			{
				Sprite sprite = limb.GetActiveSprite(true);
				if (sprite != null)
				{
					sprite.Depth = startDepth + (float)depthSortedLimbs.IndexOf(limb) * 1E-05f;
					foreach (ConditionalSprite conditionalSprite in limb.ConditionalSprites)
					{
						if (conditionalSprite.Exclusive)
						{
							conditionalSprite.ActiveSprite.Depth = sprite.Depth;
						}
					}
				}
			}
			foreach (Limb limb2 in this.Limbs)
			{
				if (limb2.ActiveSprite != null && limb2.Params.InheritLimbDepth != LimbType.None)
				{
					Limb matchingLimb = this.GetLimb(limb2.Params.InheritLimbDepth, true, false, false);
					if (matchingLimb != null)
					{
						limb2.ActiveSprite.Depth = matchingLimb.ActiveSprite.Depth - 1E-07f;
					}
				}
			}
			depthSortedLimbs.Reverse();
			this.inversedLimbDrawOrder = depthSortedLimbs.ToArray();
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0001DB5C File Offset: 0x0001BD5C
		public void SaveRagdoll(string fileNameWithoutExtension = null)
		{
			this.RagdollParams.Save(fileNameWithoutExtension);
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0001DB6B File Offset: 0x0001BD6B
		public void ResetRagdoll()
		{
			this.RagdollParams.Reset(true);
			this.ResetJoints();
			this.ResetLimbs();
		}

		// Token: 0x06000343 RID: 835 RVA: 0x0001DB86 File Offset: 0x0001BD86
		public void ResetJoints()
		{
			this.LimbJoints.ForEach(delegate(LimbJoint j)
			{
				j.LoadParams();
			});
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0001DBB2 File Offset: 0x0001BDB2
		public void ResetLimbs()
		{
			this.Limbs.ForEach(delegate(Limb l)
			{
				l.LoadParams();
			});
			this.SetupDrawOrder();
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0001DBE4 File Offset: 0x0001BDE4
		public void AddJoint(RagdollParams.JointParams jointParams)
		{
			Ragdoll.<>c__DisplayClass135_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.jointParams = jointParams;
			if (!this.<AddJoint>g__checkLimbIndex|135_0(CS$<>8__locals1.jointParams.Limb1, "Limb1", ref CS$<>8__locals1) || !this.<AddJoint>g__checkLimbIndex|135_0(CS$<>8__locals1.jointParams.Limb2, "Limb2", ref CS$<>8__locals1))
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

		// Token: 0x06000346 RID: 838 RVA: 0x0001DCC8 File Offset: 0x0001BEC8
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

		// Token: 0x06000347 RID: 839 RVA: 0x0001DE40 File Offset: 0x0001C040
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
			this.SetupDrawOrder();
		}

		// Token: 0x06000348 RID: 840 RVA: 0x0001DEF4 File Offset: 0x0001C0F4
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
			this.SetupDrawOrder();
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

		// Token: 0x06000349 RID: 841 RVA: 0x0001E0A4 File Offset: 0x0001C2A4
		public bool OnLimbCollision(Fixture f1, Fixture f2, Contact contact)
		{
			Ragdoll.<>c__DisplayClass140_0 CS$<>8__locals1;
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
				Ragdoll.LimbStairCollisionResponse collisionResponse = this.<OnLimbCollision>g__getStairCollisionResponse|140_0(ref CS$<>8__locals1);
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

		// Token: 0x0600034A RID: 842 RVA: 0x0001E354 File Offset: 0x0001C554
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
			this.ImpactProjSpecific(impact, f1.Body);
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0001E56C File Offset: 0x0001C76C
		public float GetImpactDamage(float impact, float? impactTolerance = null)
		{
			float tolerance = impactTolerance ?? this.ImpactTolerance;
			return Math.Min((impact - tolerance) * 10f, this.character.MaxVitality * 0.1f);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0001E5B4 File Offset: 0x0001C7B4
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
			this.SeverLimbJointProjSpecific(limbJoint, true);
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				GameMain.NetworkMember.CreateEntityEvent(this.character, default(Character.CharacterStatusEventData));
			}
			return true;
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0001E878 File Offset: 0x0001CA78
		private void SeverLimbJointProjSpecific(LimbJoint limbJoint, bool playSound)
		{
			foreach (Limb limb in new Limb[]
			{
				limbJoint.LimbA,
				limbJoint.LimbB
			})
			{
				float gibParticleAmount = MathHelper.Clamp(limb.Mass / this.character.AnimController.Mass, 0.1f, 1f);
				foreach (ParticleEmitter emitter in this.character.GibEmitters)
				{
					if (((emitter != null) ? emitter.Prefab : null) != null && (!this.inWater || emitter.Prefab.ParticlePrefab.DrawTarget != ParticlePrefab.DrawTargetType.Air) && (this.inWater || emitter.Prefab.ParticlePrefab.DrawTarget != ParticlePrefab.DrawTargetType.Water))
					{
						emitter.Emit(1f, limb.WorldPosition, this.character.CurrentHull, 0f, 0f, 1f, 1f, gibParticleAmount, null, null, false, null);
					}
				}
			}
			if (playSound)
			{
				CharacterSound damageSound = this.character.GetSound((CharacterSound s) => s.Type == CharacterSound.SoundType.Damage, false);
				float range = (damageSound != null) ? (damageSound.Range * 2f) : ConvertUnits.ToDisplayUnits(this.character.AnimController.Collider.GetSize().Length() * 10f);
				if (!limbJoint.Params.BreakSound.IsNullOrEmpty() && !limbJoint.Params.BreakSound.Equals("none", StringComparison.OrdinalIgnoreCase))
				{
					SoundPlayer.PlayDamageSound(limbJoint.Params.BreakSound, 1f, limbJoint.LimbA.body.DrawPosition, range, null, 1f);
				}
			}
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0001EA78 File Offset: 0x0001CC78
		protected List<Limb> GetConnectedLimbs(Limb limb)
		{
			this.connectedLimbs.Clear();
			this.checkedJoints.Clear();
			this.GetConnectedLimbs(this.connectedLimbs, this.checkedJoints, limb);
			return this.connectedLimbs;
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0001EAAC File Offset: 0x0001CCAC
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

		// Token: 0x06000350 RID: 848 RVA: 0x0001EB44 File Offset: 0x0001CD44
		private void ImpactProjSpecific(float impact, Body body)
		{
			float volume = MathHelper.Clamp(impact - 3f, 0.5f, 1f);
			Limb limb = body.UserData as Limb;
			if (limb != null && this.character.Stun <= 0f)
			{
				if (impact > 3f)
				{
					this.PlayImpactSound(limb);
				}
			}
			else if ((body.UserData is Limb || body == this.Collider.FarseerBody) && !this.character.IsRemotelyControlled && impact > this.ImpactTolerance)
			{
				SoundPlayer.PlayDamageSound("LimbBlunt", this.strongestImpact, this.Collider);
			}
			if (Character.Controlled == this.character)
			{
				GameMain.GameScreen.Cam.Shake = Math.Min(Math.Max(this.strongestImpact, GameMain.GameScreen.Cam.Shake), 3f);
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x06000351 RID: 849 RVA: 0x0001EC21 File Offset: 0x0001CE21
		// (set) Token: 0x06000352 RID: 850 RVA: 0x0001EC29 File Offset: 0x0001CE29
		public bool IsFlipped { get; private set; }

		// Token: 0x06000353 RID: 851 RVA: 0x0001EC34 File Offset: 0x0001CE34
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
			this.FlipProjSpecific();
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0001ED98 File Offset: 0x0001CF98
		private void FlipProjSpecific()
		{
			foreach (Limb limb in this.Limbs)
			{
				if (limb != null && !limb.IsSevered && limb.DoesMirror)
				{
					DeformableSprite deformSprite = limb.DeformSprite;
					Ragdoll.<FlipProjSpecific>g__FlipSprite|155_0(((deformSprite != null) ? deformSprite.Sprite : null) ?? limb.Sprite);
					foreach (ConditionalSprite conditionalSprite in limb.ConditionalSprites)
					{
						DeformableSprite deformableSprite = conditionalSprite.DeformableSprite;
						Ragdoll.<FlipProjSpecific>g__FlipSprite|155_0(((deformableSprite != null) ? deformableSprite.Sprite : null) ?? conditionalSprite.Sprite);
					}
				}
			}
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0001EE60 File Offset: 0x0001D060
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

		// Token: 0x06000356 RID: 854 RVA: 0x0001EFD3 File Offset: 0x0001D1D3
		public void MoveLimb(Limb limb, Vector2 pos, float amount, bool pullFromCenter = false)
		{
			limb.MoveToPos(pos, amount, pullFromCenter);
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0001EFE0 File Offset: 0x0001D1E0
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

		// Token: 0x06000358 RID: 856 RVA: 0x0001F02C File Offset: 0x0001D22C
		public static void UpdateAll(float deltaTime, Camera cam)
		{
			foreach (Ragdoll r in Ragdoll.list)
			{
				r.UpdateRagdoll(deltaTime, cam);
			}
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0001F080 File Offset: 0x0001D280
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

		// Token: 0x0600035A RID: 858 RVA: 0x0001F468 File Offset: 0x0001D668
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

		// Token: 0x0600035B RID: 859 RVA: 0x0001F690 File Offset: 0x0001D890
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

		// Token: 0x0600035C RID: 860 RVA: 0x0001F7C8 File Offset: 0x0001D9C8
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

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x0600035D RID: 861 RVA: 0x0001F88C File Offset: 0x0001DA8C
		// (set) Token: 0x0600035E RID: 862 RVA: 0x0001F89C File Offset: 0x0001DA9C
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

		// Token: 0x0600035F RID: 863 RVA: 0x0001F8E8 File Offset: 0x0001DAE8
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
			if (Screen.Selected is CharacterEditorScreen)
			{
				AnimController animController = this as AnimController;
				if (animController != null)
				{
					applyWaterForces = (animController.CurrentAnimationParams is SwimParams);
				}
			}
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
					if (Math.Abs(limb2.LinearVelocity.Y) > 5f && limb2.InWater != prevInWater && newHull == limb2.Hull)
					{
						this.Splash(limb2, newHull);
						if (limb2.LinearVelocity.Y < 0f)
						{
							Vector2 impulse = limb2.LinearVelocity * limb2.Mass;
							int i = (int)((limb2.Position.X - (float)newHull.Rect.X) / 32f);
							newHull.WaveVel[i] += MathHelper.Clamp(impulse.Y, -5f, 5f);
						}
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
			this.UpdateProjSpecific(deltaTime, cam);
			this.forceNotStanding = false;
		}

		// Token: 0x06000360 RID: 864 RVA: 0x00020298 File Offset: 0x0001E498
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

		// Token: 0x06000361 RID: 865 RVA: 0x00020348 File Offset: 0x0001E548
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

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000362 RID: 866 RVA: 0x00020442 File Offset: 0x0001E642
		// (set) Token: 0x06000363 RID: 867 RVA: 0x0002044A File Offset: 0x0001E64A
		public bool Invalid { get; private set; }

		// Token: 0x06000364 RID: 868 RVA: 0x00020454 File Offset: 0x0001E654
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

		// Token: 0x06000365 RID: 869 RVA: 0x00020630 File Offset: 0x0001E830
		private bool CheckValidity(PhysicsBody body)
		{
			Ragdoll.<>c__DisplayClass181_0 CS$<>8__locals1;
			CS$<>8__locals1.body = body;
			string errorMsg = null;
			if (!MathUtils.IsValid(CS$<>8__locals1.body.SimPosition) || Math.Abs(CS$<>8__locals1.body.SimPosition.X) > 1E+10f || Math.Abs(CS$<>8__locals1.body.SimPosition.Y) > 1E+10f)
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|181_0(ref CS$<>8__locals1) + " position invalid (" + CS$<>8__locals1.body.SimPosition.ToString() + ", character: [name]).";
			}
			else if (!MathUtils.IsValid(CS$<>8__locals1.body.LinearVelocity) || Math.Abs(CS$<>8__locals1.body.LinearVelocity.X) > 1000f || Math.Abs(CS$<>8__locals1.body.LinearVelocity.Y) > 1000f)
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|181_0(ref CS$<>8__locals1) + " velocity invalid (" + CS$<>8__locals1.body.LinearVelocity.ToString() + ", character: [name]).";
			}
			else if (!MathUtils.IsValid(CS$<>8__locals1.body.Rotation))
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|181_0(ref CS$<>8__locals1) + " rotation invalid (" + CS$<>8__locals1.body.Rotation.ToString() + ", character: [name]).";
			}
			else if (!MathUtils.IsValid(CS$<>8__locals1.body.AngularVelocity) || Math.Abs(CS$<>8__locals1.body.AngularVelocity) > 1000f)
			{
				errorMsg = Ragdoll.<CheckValidity>g__GetBodyName|181_0(ref CS$<>8__locals1) + " angular velocity invalid (" + CS$<>8__locals1.body.AngularVelocity.ToString() + ", character: [name]).";
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

		// Token: 0x06000366 RID: 870 RVA: 0x0002094C File Offset: 0x0001EB4C
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

		// Token: 0x06000367 RID: 871 RVA: 0x00020AB4 File Offset: 0x0001ECB4
		private void UpdateProjSpecific(float deltaTime, Camera cam)
		{
			if (!this.character.IsVisible)
			{
				return;
			}
			this.LimbJoints.ForEach(delegate(LimbJoint j)
			{
				j.UpdateDeformations(deltaTime);
			});
			foreach (SpriteDeformation deformation in this.SpriteDeformations)
			{
				if ((!this.character.IsDead || !deformation.Params.StopWhenHostIsDead) && (this.character.AnimController.InWater || !deformation.Params.OnlyInWater))
				{
					if (deformation.Params.UseMovementSine)
					{
						AnimController animator = this as AnimController;
						if (animator != null)
						{
							deformation.Phase = MathUtils.WrapAngleTwoPi(animator.WalkPos * deformation.Params.Frequency + 3.1415927f * deformation.Params.SineOffset);
						}
					}
					else
					{
						deformation.Update(deltaTime);
					}
				}
			}
		}

		// Token: 0x06000368 RID: 872 RVA: 0x00020BC8 File Offset: 0x0001EDC8
		private void Splash(Limb limb, Hull limbHull)
		{
			int i = 0;
			while ((float)i < MathHelper.Clamp(Math.Abs(limb.LinearVelocity.Y), 1f, 5f))
			{
				Particle splash = GameMain.ParticleManager.CreateParticle("watersplash", new Vector2(limb.WorldPosition.X, limbHull.WorldSurface), new Vector2(0f, Math.Abs(-limb.LinearVelocity.Y * 20f)) + Rand.Vector(Math.Abs(limb.LinearVelocity.Y * 10f), Rand.RandSync.Unsynced), Rand.Range(0f, 6.2831855f, Rand.RandSync.Unsynced), limbHull, 0f, null);
				if (splash != null)
				{
					splash.Size *= MathHelper.Clamp(Math.Abs(limb.LinearVelocity.Y) * 0.1f, 1f, 2f);
				}
				i++;
			}
			GameMain.ParticleManager.CreateParticle("bubbles", new Vector2(limb.WorldPosition.X, limbHull.WorldSurface), limb.LinearVelocity * 0.001f, 0f, limbHull, 0f, null);
			if (limb.LinearVelocity.Y < 0f)
			{
				if (this.splashSoundTimer <= 0f)
				{
					SoundPlayer.PlaySplashSound(limb.WorldPosition, Math.Abs(limb.LinearVelocity.Y) + Rand.Range(-5f, 0f, Rand.RandSync.Unsynced));
					this.splashSoundTimer = 0.5f;
				}
				GameMain.ParticleManager.CreateParticle("bubbles", new Vector2(limb.WorldPosition.X, limbHull.WorldSurface), limb.LinearVelocity * 10f, 0f, limbHull, 0f, null);
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x00020D9C File Offset: 0x0001EF9C
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

		// Token: 0x0600036A RID: 874 RVA: 0x000210F4 File Offset: 0x0001F2F4
		public void ForceRefreshFloorY()
		{
			this.lastFloorCheckPos = Vector2.Zero;
		}

		// Token: 0x0600036B RID: 875 RVA: 0x00021104 File Offset: 0x0001F304
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

		// Token: 0x0600036C RID: 876 RVA: 0x000211B0 File Offset: 0x0001F3B0
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

		// Token: 0x0600036D RID: 877 RVA: 0x00021518 File Offset: 0x0001F718
		public float GetSurfaceY()
		{
			return this.GetWaterSurfaceAndCeilingY().Item1;
		}

		// Token: 0x0600036E RID: 878 RVA: 0x00021528 File Offset: 0x0001F728
		[return: TupleElementNames(new string[]
		{
			"WaterSurfaceY",
			"CeilingY"
		})]
		private ValueTuple<float, float> GetWaterSurfaceAndCeilingY()
		{
			Ragdoll.<>c__DisplayClass192_0 CS$<>8__locals1;
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
				this.<GetWaterSurfaceAndCeilingY>g__GetSurfacePos|192_0(this.currentHull, ref CS$<>8__locals1.surfaceY, ref ceilingY, ref CS$<>8__locals1);
			}
			return new ValueTuple<float, float>(CS$<>8__locals1.surfaceY, ceilingY);
		}

		// Token: 0x0600036F RID: 879 RVA: 0x000215F0 File Offset: 0x0001F7F0
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

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000370 RID: 880 RVA: 0x0002186A File Offset: 0x0001FA6A
		// (set) Token: 0x06000371 RID: 881 RVA: 0x00021872 File Offset: 0x0001FA72
		public bool IsHoldingToRope { get; private set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000372 RID: 882 RVA: 0x0002187B File Offset: 0x0001FA7B
		// (set) Token: 0x06000373 RID: 883 RVA: 0x00021883 File Offset: 0x0001FA83
		public bool IsHangingWithRope { get; private set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000374 RID: 884 RVA: 0x0002188C File Offset: 0x0001FA8C
		// (set) Token: 0x06000375 RID: 885 RVA: 0x00021894 File Offset: 0x0001FA94
		public bool IsDraggedWithRope { get; private set; }

		// Token: 0x06000376 RID: 886 RVA: 0x0002189D File Offset: 0x0001FA9D
		public void HangWithRope()
		{
			this.shouldHangWithRope = true;
			this.IsHangingWithRope = true;
			this.ResetPullJoints(null);
			this.onGround = false;
			this.levitatingCollider = false;
		}

		// Token: 0x06000377 RID: 887 RVA: 0x000218C2 File Offset: 0x0001FAC2
		public void HoldToRope()
		{
			this.shouldHoldToRope = true;
			this.IsHoldingToRope = true;
		}

		// Token: 0x06000378 RID: 888 RVA: 0x000218D2 File Offset: 0x0001FAD2
		public void DragWithRope()
		{
			this.shouldBeDraggedWithRope = true;
			this.IsDraggedWithRope = true;
		}

		// Token: 0x06000379 RID: 889 RVA: 0x000218E2 File Offset: 0x0001FAE2
		protected void StopHangingWithRope()
		{
			this.shouldHangWithRope = false;
			this.IsHangingWithRope = false;
		}

		// Token: 0x0600037A RID: 890 RVA: 0x000218F2 File Offset: 0x0001FAF2
		protected void StopHoldingToRope()
		{
			this.shouldHoldToRope = false;
			this.IsHoldingToRope = false;
		}

		// Token: 0x0600037B RID: 891 RVA: 0x00021902 File Offset: 0x0001FB02
		protected void StopGettingDraggedWithRope()
		{
			this.shouldBeDraggedWithRope = false;
			this.IsDraggedWithRope = false;
		}

		// Token: 0x0600037C RID: 892 RVA: 0x00021914 File Offset: 0x0001FB14
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

		// Token: 0x0600037D RID: 893 RVA: 0x00021A68 File Offset: 0x0001FC68
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

		// Token: 0x0600037E RID: 894 RVA: 0x00021C0C File Offset: 0x0001FE0C
		private void UpdateNetPlayerPositionProjSpecific(float deltaTime, float lowestSubPos)
		{
			if (this.character != GameMain.Client.Character)
			{
				this.character.MemState.RemoveAll((CharacterStateInfo m) => m.Timestamp == 0f);
				if (this.character.MemState.Count > 0)
				{
					CharacterStateInfo serverPos2 = this.character.MemState.Last<CharacterStateInfo>();
					if (!this.character.isSynced)
					{
						this.<UpdateNetPlayerPositionProjSpecific>g__SyncPosition|219_1(serverPos2);
						return;
					}
					if (this.character.MemState[0].SelectedCharacter == null || this.character.MemState[0].SelectedCharacter.Removed)
					{
						this.character.DeselectCharacter();
					}
					else if (this.character.MemState[0].SelectedCharacter != null)
					{
						this.character.SelectCharacter(this.character.MemState[0].SelectedCharacter);
					}
					if (this.character.MemState[0].SelectedItem == null || this.character.MemState[0].SelectedItem.Removed)
					{
						this.character.SelectedItem = null;
					}
					else if (this.character.SelectedItem != this.character.MemState[0].SelectedItem)
					{
						foreach (ItemComponent ic in this.character.MemState[0].SelectedItem.Components)
						{
							if (ic.CanBeSelected)
							{
								ic.Select(this.character);
							}
						}
						this.character.SelectedItem = this.character.MemState[0].SelectedItem;
					}
					if (this.character.MemState[0].SelectedSecondaryItem == null || this.character.MemState[0].SelectedSecondaryItem.Removed)
					{
						this.character.SelectedSecondaryItem = null;
					}
					else if (this.character.SelectedSecondaryItem != this.character.MemState[0].SelectedSecondaryItem)
					{
						foreach (ItemComponent ic2 in this.character.MemState[0].SelectedSecondaryItem.Components)
						{
							if (ic2.CanBeSelected)
							{
								ic2.Select(this.character);
							}
						}
						this.character.SelectedSecondaryItem = this.character.MemState[0].SelectedSecondaryItem;
					}
					if (this.character.MemState[0].Animation == AnimController.Animation.CPR)
					{
						this.character.AnimController.Anim = AnimController.Animation.CPR;
					}
					else if (this.character.AnimController.Anim == AnimController.Animation.CPR)
					{
						this.character.AnimController.Anim = AnimController.Animation.None;
					}
					this.character.AnimController.IgnorePlatforms = this.character.MemState[0].IgnorePlatforms;
					this.character.AnimController.overrideTargetMovement = new Vector2?(this.character.MemState[0].TargetMovement);
					Vector2 newVelocity = this.Collider.LinearVelocity;
					Vector2 newPosition = this.Collider.SimPosition;
					float newRotation = this.Collider.Rotation;
					float newAngularVelocity = this.Collider.AngularVelocity;
					this.Collider.CorrectPosition<CharacterStateInfo>(this.character.MemState, out newPosition, out newVelocity, out newRotation, out newAngularVelocity);
					if (this.Collider.BodyType == BodyType.Dynamic)
					{
						newVelocity = newVelocity.ClampLength(100f);
						if (!MathUtils.IsValid(newVelocity))
						{
							newVelocity = Vector2.Zero;
						}
						this.Collider.LinearVelocity = newVelocity;
						this.Collider.AngularVelocity = newAngularVelocity;
					}
					float distSqrd = Vector2.DistanceSquared(newPosition, this.Collider.SimPosition);
					float errorTolerance = (this.ColliderControlsMovement && (!this.character.IsRagdolled || this.character.AnimController.IsHangingWithRope)) ? 0.01f : 0.2f;
					if (distSqrd > errorTolerance)
					{
						this.character.AnimController.BodyInRest = false;
						if (distSqrd > 10f)
						{
							this.Collider.TargetRotation = new float?(newRotation);
							if (distSqrd > 10f)
							{
								Hull serverHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(newPosition), this.CurrentHull, newPosition.Y < lowestSubPos, true);
								if (this.currentHull != null && serverHull != null && serverHull.Submarine != this.currentHull.Submarine)
								{
									this.character.Submarine = serverHull.Submarine;
									this.character.CurrentHull = (this.CurrentHull = serverHull);
								}
							}
							this.SetPosition(newPosition, distSqrd < 5f, false, false, true);
							if (!this.ColliderControlsMovement && newVelocity.LengthSquared() < 0.01f)
							{
								this.TryPlatformCorrection(newPosition);
							}
						}
						else if (this.ColliderControlsMovement)
						{
							this.Collider.TargetRotation = new float?(newRotation);
							this.Collider.TargetPosition = new Vector2?(newPosition);
							this.Collider.MoveToTargetPosition(true);
						}
						else
						{
							float mainLimbDistSqrd = Vector2.DistanceSquared(this.MainLimb.PullJointWorldAnchorA, newPosition);
							float mainLimbErrorTolerance = (this.character == GameMain.Client.Character) ? 0.25f : 0.1f;
							this.MainLimb.body.LinearVelocity = newVelocity;
							if (mainLimbDistSqrd > mainLimbErrorTolerance)
							{
								this.MainLimb.PullJointWorldAnchorB = newPosition;
								this.MainLimb.PullJointEnabled = true;
								if (!this.ColliderControlsMovement && newVelocity.LengthSquared() < 0.01f)
								{
									this.TryPlatformCorrection(newPosition);
								}
							}
						}
					}
					else if (!this.ColliderControlsMovement)
					{
						this.MainLimb.body.LinearVelocity = newVelocity;
					}
				}
				this.character.MemLocalState.Clear();
				return;
			}
			this.character.MemState.RemoveAll((CharacterStateInfo m) => m.Timestamp > 0f);
			for (int i = 0; i < this.character.MemLocalState.Count; i++)
			{
				if (this.character.Submarine == null)
				{
					if (this.character.MemLocalState[i].Position.Y > lowestSubPos)
					{
						this.character.MemLocalState[i].TransformInToOutside();
					}
				}
				else
				{
					Hull hull = this.currentHull;
					if (((hull != null) ? hull.Submarine : null) != null && this.character.MemLocalState[i].Position.Y < lowestSubPos)
					{
						this.character.MemLocalState[i].TransformOutToInside(this.currentHull.Submarine);
					}
				}
			}
			if (this.character.MemState.Count < 1)
			{
				return;
			}
			this.overrideTargetMovement = null;
			CharacterStateInfo serverPos = this.character.MemState.Last<CharacterStateInfo>();
			this.Collider.LastServerState = serverPos;
			if (!this.character.isSynced)
			{
				this.<UpdateNetPlayerPositionProjSpecific>g__SyncPosition|219_1(serverPos);
				return;
			}
			int localPosIndex = this.character.MemLocalState.FindIndex((CharacterStateInfo m) => m.ID == serverPos.ID);
			if (localPosIndex > -1)
			{
				CharacterStateInfo localPos = this.character.MemLocalState[localPosIndex];
				if (localPos.SelectedCharacter != serverPos.SelectedCharacter)
				{
					if (serverPos.SelectedCharacter == null || serverPos.SelectedCharacter.Removed)
					{
						this.character.DeselectCharacter();
					}
					else if (serverPos.SelectedCharacter != null)
					{
						this.character.SelectCharacter(serverPos.SelectedCharacter);
					}
				}
				if (localPos.SelectedItem != serverPos.SelectedItem)
				{
					if (serverPos.SelectedItem == null || serverPos.SelectedItem.Removed)
					{
						this.character.SelectedItem = null;
					}
					else if (this.character.SelectedItem != serverPos.SelectedItem)
					{
						serverPos.SelectedItem.TryInteract(this.character, true, true, false);
						this.character.SelectedItem = serverPos.SelectedItem;
					}
				}
				if (localPos.SelectedSecondaryItem != serverPos.SelectedSecondaryItem)
				{
					if (serverPos.SelectedSecondaryItem == null || serverPos.SelectedSecondaryItem.Removed)
					{
						this.character.SelectedSecondaryItem = null;
					}
					else if (this.character.SelectedSecondaryItem != serverPos.SelectedSecondaryItem)
					{
						serverPos.SelectedSecondaryItem.TryInteract(this.character, true, true, false);
						this.character.SelectedSecondaryItem = serverPos.SelectedSecondaryItem;
					}
				}
				if (localPos.Animation != serverPos.Animation)
				{
					if (serverPos.Animation == AnimController.Animation.CPR)
					{
						this.character.AnimController.Anim = AnimController.Animation.CPR;
					}
					else if (this.character.AnimController.Anim == AnimController.Animation.CPR)
					{
						this.character.AnimController.Anim = AnimController.Animation.None;
					}
				}
				Hull serverHull2 = Hull.FindHull(ConvertUnits.ToDisplayUnits(serverPos.Position), this.character.CurrentHull, serverPos.Position.Y < lowestSubPos, true);
				Hull clientHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(localPos.Position), serverHull2, localPos.Position.Y < lowestSubPos, true);
				if (serverHull2 != null && clientHull != null && serverHull2.Submarine != clientHull.Submarine)
				{
					this.character.Submarine = serverHull2.Submarine;
					this.character.CurrentHull = (this.CurrentHull = serverHull2);
					this.SetPosition(serverPos.Position, false, true, false, true);
					this.character.MemLocalState.Clear();
				}
				else
				{
					Vector2 positionError = serverPos.Position - localPos.Position;
					float rotationError = (serverPos.Rotation != null && localPos.Rotation != null) ? (serverPos.Rotation.Value - localPos.Rotation.Value) : 0f;
					for (int j = localPosIndex; j < this.character.MemLocalState.Count; j++)
					{
						Hull pointHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(this.character.MemLocalState[j].Position), clientHull, this.character.MemLocalState[j].Position.Y < lowestSubPos, true);
						if (pointHull != clientHull && (pointHull == null || clientHull == null || pointHull.Submarine == clientHull.Submarine))
						{
							break;
						}
						this.character.MemLocalState[j].Translate(positionError, rotationError);
					}
					float errorMagnitude = positionError.Length();
					if (errorMagnitude > 0.5f)
					{
						this.character.MemLocalState.Clear();
						this.SetPosition(serverPos.Position, true, false, false, true);
					}
					else if (errorMagnitude > 0.01f)
					{
						if (this.ColliderControlsMovement)
						{
							this.Collider.TargetPosition = new Vector2?(this.Collider.SimPosition + positionError);
							this.Collider.TargetRotation = new float?(this.Collider.Rotation + rotationError);
							this.Collider.MoveToTargetPosition(true);
						}
						else
						{
							float mainLimbErrorTolerance2 = (this.character == GameMain.Client.Character) ? 0.25f : 0.1f;
							if (errorMagnitude > mainLimbErrorTolerance2)
							{
								this.MainLimb.PullJointWorldAnchorB = this.MainLimb.SimPosition + positionError;
								this.MainLimb.PullJointEnabled = true;
								if (serverPos.LinearVelocity.LengthSquared() < 0.01f)
								{
									this.TryPlatformCorrection(this.MainLimb.SimPosition + positionError);
								}
							}
						}
					}
				}
			}
			if (this.character.MemLocalState.Count > 120)
			{
				this.character.MemLocalState.RemoveRange(0, this.character.MemLocalState.Count - 120);
			}
			this.character.MemState.Clear();
		}

		// Token: 0x0600037F RID: 895 RVA: 0x00022904 File Offset: 0x00020B04
		private void UpdateNetPlayerPosition(float deltaTime)
		{
			if (GameMain.NetworkMember == null)
			{
				return;
			}
			float lowestSubPos = float.MaxValue;
			if (Submarine.Loaded.Any<Submarine>())
			{
				lowestSubPos = ConvertUnits.ToSimUnits(Submarine.Loaded.Min((Submarine s) => s.HiddenSubPosition.Y - (float)s.Borders.Height - 128f));
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
			this.UpdateNetPlayerPositionProjSpecific(deltaTime, lowestSubPos);
		}

		// Token: 0x06000380 RID: 896 RVA: 0x00022A1C File Offset: 0x00020C1C
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

		// Token: 0x06000381 RID: 897 RVA: 0x00022AC8 File Offset: 0x00020CC8
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

		// Token: 0x06000382 RID: 898 RVA: 0x00022B8C File Offset: 0x00020D8C
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

		// Token: 0x06000383 RID: 899 RVA: 0x00022C78 File Offset: 0x00020E78
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

		// Token: 0x06000384 RID: 900 RVA: 0x00022CCA File Offset: 0x00020ECA
		public void ReleaseStuckLimbs()
		{
		}

		// Token: 0x06000385 RID: 901 RVA: 0x00022CCC File Offset: 0x00020ECC
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

		// Token: 0x06000386 RID: 902 RVA: 0x00022D03 File Offset: 0x00020F03
		public void RestoreTemporarilyDisabled()
		{
			this.Limbs.ForEach(delegate(Limb l)
			{
				l.ReEnable();
			});
		}

		// Token: 0x06000387 RID: 903 RVA: 0x00022D30 File Offset: 0x00020F30
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

		// Token: 0x06000388 RID: 904 RVA: 0x00022E40 File Offset: 0x00021040
		public static void RemoveAll()
		{
			for (int i = Ragdoll.list.Count - 1; i >= 0; i--)
			{
				Ragdoll.list[i].Remove();
			}
		}

		// Token: 0x0600038A RID: 906 RVA: 0x00022E80 File Offset: 0x00021080
		[CompilerGenerated]
		private void <GetDepthOffset>g__AdjustDepthOffset|8_0(Item item, ref Ragdoll.<>c__DisplayClass8_0 A_2)
		{
			if (item == null)
			{
				return;
			}
			foreach (Controller controller in item.GetComponents<Controller>())
			{
				if (controller != null && controller.ControlCharacterPose && controller.UserInCorrectPosition && controller.User == this.character)
				{
					if (controller.Item.SpriteDepth <= A_2.maxDepth || controller.DrawUserBehind)
					{
						A_2.depthOffset = Math.Max(controller.Item.GetDrawDepth() + 0.0001f - A_2.minDepth, -A_2.minDepth);
					}
					else
					{
						A_2.depthOffset = Math.Max(controller.Item.GetDrawDepth() - 0.0001f - A_2.maxDepth, 0f);
					}
				}
			}
		}

		// Token: 0x0600038B RID: 907 RVA: 0x00022F68 File Offset: 0x00021168
		[CompilerGenerated]
		private void <GetDepthOffset>g__CalculateLimbDepths|8_1(ref Ragdoll.<>c__DisplayClass8_0 A_1)
		{
			foreach (Limb limb in this.Limbs)
			{
				Sprite activeSprite = limb.ActiveSprite;
				if (activeSprite != null)
				{
					A_1.maxDepth = Math.Max(activeSprite.Depth, A_1.maxDepth);
					A_1.minDepth = Math.Min(activeSprite.Depth, A_1.minDepth);
				}
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x00022FC6 File Offset: 0x000211C6
		[CompilerGenerated]
		internal static bool <get_MainLimb>g__IsValid|84_0(Limb limb)
		{
			return limb != null && !limb.IsSevered && !limb.IgnoreCollisions && !limb.Hidden;
		}

		// Token: 0x0600038E RID: 910 RVA: 0x00022FF0 File Offset: 0x000211F0
		[CompilerGenerated]
		private bool <AddJoint>g__checkLimbIndex|135_0(int index, string debugName, ref Ragdoll.<>c__DisplayClass135_0 A_3)
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

		// Token: 0x0600038F RID: 911 RVA: 0x000230DC File Offset: 0x000212DC
		[CompilerGenerated]
		private Ragdoll.LimbStairCollisionResponse <OnLimbCollision>g__getStairCollisionResponse|140_0(ref Ragdoll.<>c__DisplayClass140_0 A_1)
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

		// Token: 0x06000390 RID: 912 RVA: 0x00023248 File Offset: 0x00021448
		[CompilerGenerated]
		internal static void <FlipProjSpecific>g__FlipSprite|155_0(Sprite sprite)
		{
			if (sprite == null)
			{
				return;
			}
			Vector2 spriteOrigin = sprite.Origin;
			spriteOrigin.X = (float)sprite.SourceRect.Width - spriteOrigin.X;
			sprite.Origin = spriteOrigin;
		}

		// Token: 0x06000393 RID: 915 RVA: 0x000232D4 File Offset: 0x000214D4
		[CompilerGenerated]
		internal static string <CheckValidity>g__GetBodyName|181_0(ref Ragdoll.<>c__DisplayClass181_0 A_0)
		{
			Limb limb = A_0.body.UserData as Limb;
			if (limb == null)
			{
				return "Collider";
			}
			return "Limb (" + limb.type.ToString() + ")";
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00023320 File Offset: 0x00021520
		[CompilerGenerated]
		private void <GetWaterSurfaceAndCeilingY>g__GetSurfacePos|192_0(Hull hull, ref float prevSurfacePos, ref float ceilingPos, ref Ragdoll.<>c__DisplayClass192_0 A_4)
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
							this.<GetWaterSurfaceAndCeilingY>g__GetSurfacePos|192_0(otherHull, ref prevSurfacePos, ref ceilingPos, ref A_4);
							break;
						}
					}
				}
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x000234E4 File Offset: 0x000216E4
		[CompilerGenerated]
		private void <UpdateNetPlayerPositionProjSpecific>g__SyncPosition|219_1(CharacterStateInfo serverPos)
		{
			this.SetPosition(serverPos.Position, false, true, false, true);
			this.Collider.LinearVelocity = Vector2.Zero;
			this.character.MemLocalState.Clear();
			this.character.LastNetworkUpdateID = serverPos.ID;
			this.character.isSynced = true;
		}

		// Token: 0x040001FA RID: 506
		protected Limb[] inversedLimbDrawOrder;

		// Token: 0x040001FB RID: 507
		private const float ImpactDamageMultiplayer = 10f;

		// Token: 0x040001FC RID: 508
		private const float MaxImpactDamage = 0.1f;

		// Token: 0x040001FD RID: 509
		private static readonly List<Ragdoll> list = new List<Ragdoll>();

		// Token: 0x040001FE RID: 510
		private readonly Queue<Ragdoll.Impact> impactQueue = new Queue<Ragdoll.Impact>();

		// Token: 0x040001FF RID: 511
		protected Hull currentHull;

		// Token: 0x04000200 RID: 512
		private bool accessRemovedCharacterErrorShown;

		// Token: 0x04000201 RID: 513
		private Limb[] limbs;

		// Token: 0x04000202 RID: 514
		private readonly List<Body> limbBodies = new List<Body>();

		// Token: 0x04000203 RID: 515
		private bool frozen;

		// Token: 0x04000204 RID: 516
		private Dictionary<LimbType, Limb> limbDictionary;

		// Token: 0x04000205 RID: 517
		public LimbJoint[] LimbJoints;

		// Token: 0x04000206 RID: 518
		private bool simplePhysicsEnabled;

		// Token: 0x04000207 RID: 519
		protected readonly Character character;

		// Token: 0x04000208 RID: 520
		protected float strongestImpact;

		// Token: 0x04000209 RID: 521
		private float splashSoundTimer;

		// Token: 0x0400020A RID: 522
		private float flowForceTolerance;

		// Token: 0x0400020B RID: 523
		private float flowStunTolerance;

		// Token: 0x0400020C RID: 524
		public Vector2 movement;

		// Token: 0x0400020D RID: 525
		protected Vector2 targetMovement;

		// Token: 0x0400020E RID: 526
		protected Vector2? overrideTargetMovement;

		// Token: 0x0400020F RID: 527
		protected float floorY;

		// Token: 0x04000210 RID: 528
		protected float standOnFloorY;

		// Token: 0x04000211 RID: 529
		protected Fixture floorFixture;

		// Token: 0x04000212 RID: 530
		protected Vector2 floorNormal = Vector2.UnitY;

		// Token: 0x04000213 RID: 531
		protected float surfaceY;

		// Token: 0x04000214 RID: 532
		protected bool inWater;

		// Token: 0x04000215 RID: 533
		protected bool headInWater;

		// Token: 0x04000216 RID: 534
		protected bool onGround;

		// Token: 0x04000217 RID: 535
		private Vector2 lastFloorCheckPos;

		// Token: 0x04000218 RID: 536
		private bool lastFloorCheckIgnoreStairs;

		// Token: 0x04000219 RID: 537
		private bool lastFloorCheckIgnorePlatforms;

		// Token: 0x0400021A RID: 538
		public Structure Stairs;

		// Token: 0x0400021B RID: 539
		protected Direction dir;

		// Token: 0x0400021C RID: 540
		public Direction TargetDir;

		// Token: 0x0400021D RID: 541
		protected List<PhysicsBody> collider;

		// Token: 0x0400021E RID: 542
		protected int colliderIndex;

		// Token: 0x0400021F RID: 543
		private Category prevCollisionCategory;

		// Token: 0x04000221 RID: 545
		public const float MAX_SPEED = 20f;

		// Token: 0x04000222 RID: 546
		private float? impactTolerance;

		// Token: 0x04000224 RID: 548
		private readonly List<Limb> connectedLimbs = new List<Limb>();

		// Token: 0x04000225 RID: 549
		private readonly List<LimbJoint> checkedJoints = new List<LimbJoint>();

		// Token: 0x04000227 RID: 551
		protected bool levitatingCollider = true;

		// Token: 0x04000228 RID: 552
		private float bodyInRestTimer;

		// Token: 0x04000229 RID: 553
		private float BodyInRestDelay = 1f;

		// Token: 0x0400022A RID: 554
		public bool forceStanding;

		// Token: 0x0400022B RID: 555
		public bool forceNotStanding;

		// Token: 0x0400022D RID: 557
		private int validityResets;

		// Token: 0x0400022E RID: 558
		private const float FloorYStaleTime = 1f;

		// Token: 0x0400022F RID: 559
		private float floorYCheckTimer;

		// Token: 0x04000231 RID: 561
		protected bool shouldHoldToRope;

		// Token: 0x04000233 RID: 563
		protected bool shouldHangWithRope;

		// Token: 0x04000235 RID: 565
		protected bool shouldBeDraggedWithRope;

		// Token: 0x04000236 RID: 566
		private bool collisionsDisabled;

		// Token: 0x04000237 RID: 567
		private double lastObstacleRayCastTime;

		// Token: 0x02000677 RID: 1655
		private struct Impact
		{
			// Token: 0x060065CC RID: 26060 RVA: 0x00346294 File Offset: 0x00344494
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

			// Token: 0x040036E7 RID: 14055
			public Fixture F1;

			// Token: 0x040036E8 RID: 14056
			public Fixture F2;

			// Token: 0x040036E9 RID: 14057
			public Vector2 LocalNormal;

			// Token: 0x040036EA RID: 14058
			public Vector2 WorldNormal;

			// Token: 0x040036EB RID: 14059
			public Vector2 Velocity;

			// Token: 0x040036EC RID: 14060
			public Vector2 ImpactPos;
		}

		// Token: 0x02000678 RID: 1656
		private enum LimbStairCollisionResponse
		{
			// Token: 0x040036EE RID: 14062
			DontClimbStairs,
			// Token: 0x040036EF RID: 14063
			ClimbWithoutLimbCollision,
			// Token: 0x040036F0 RID: 14064
			ClimbWithLimbCollision
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Particles;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020004DA RID: 1242
	[NullableContext(1)]
	[Nullable(0)]
	internal class BallastFloraBehavior : ISerializableEntity
	{
		// Token: 0x060050DA RID: 20698 RVA: 0x002B8A34 File Offset: 0x002B6C34
		private void CreateShapnel(Vector2 pos)
		{
			float particleAmount = (float)Rand.Range(16, 32, Rand.RandSync.Unsynced);
			int i = 0;
			while ((float)i < particleAmount)
			{
				GameMain.ParticleManager.CreateParticle("shrapnel", pos, Rand.Vector(Rand.Range(0f, 250f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced), Rand.Range(0f, 360f, Rand.RandSync.Unsynced), null, 0f, null);
				i++;
			}
		}

		// Token: 0x060050DB RID: 20699 RVA: 0x002B8A98 File Offset: 0x002B6C98
		private void CreateDamageParticle(BallastFloraBranch branch, float deltaTime)
		{
			Vector2 pos = this.GetWorldPosition() + branch.Position;
			foreach (ParticleEmitter particleEmitter in this.DamageParticles)
			{
				particleEmitter.Emit(deltaTime, pos, branch.CurrentHull, 0f, 0f, 1f, 1f, 1f, null, null, false, null);
			}
		}

		// Token: 0x060050DC RID: 20700 RVA: 0x002B8B2C File Offset: 0x002B6D2C
		private void CreateDeathParticle(BallastFloraBranch branch, float deltaTime)
		{
			Vector2 pos = this.GetWorldPosition() + branch.Position;
			foreach (ParticleEmitter particleEmitter in this.DeathParticles)
			{
				particleEmitter.Emit(deltaTime, pos, branch.CurrentHull, 0f, 0f, 1f, 1f, 1f, null, null, false, null);
			}
		}

		// Token: 0x060050DD RID: 20701 RVA: 0x002B8BC0 File Offset: 0x002B6DC0
		public void Draw(SpriteBatch spriteBatch)
		{
			float leafDepth = 1E-06f;
			float flowerDepth = 1E-06f;
			if (GameMain.DebugDraw)
			{
				foreach (Body body in this.bodies)
				{
					Vector2 pos = this.Parent.Submarine.DrawPosition + ConvertUnits.ToDisplayUnits(body.Position);
					pos.Y = -pos.Y;
					Vector2 center = pos;
					float width = 32f;
					float height = 32f;
					float rotation = 0f;
					BallastFloraBranch ballastFloraBranch = body.UserData as BallastFloraBranch;
					GUI.DrawRectangle(spriteBatch, center, width, height, rotation, (ballastFloraBranch != null && ballastFloraBranch.IsRoot) ? Color.Magenta : Color.Cyan, 0.1f, 1f);
				}
				foreach (KeyValuePair<Item, int> keyValuePair in this.IgnoredTargets)
				{
					Item item;
					int num;
					keyValuePair.Deconstruct(out item, out num);
					Item key = item;
					int steps = num;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Ignored \"");
					defaultInterpolatedStringHandler.AppendFormatted(key.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\" for ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(steps);
					defaultInterpolatedStringHandler.AppendLiteral(" steps");
					string label = defaultInterpolatedStringHandler.ToStringAndClear();
					float num2;
					float num3;
					GUIStyle.SubHeadingFont.MeasureString(label, false).Deconstruct(out num2, out num3);
					float sizeX = num2;
					float sizeY = num3;
					Vector2 targetPos = key.WorldPosition;
					targetPos.Y = -targetPos.Y;
					Vector2 pos3 = targetPos - new Vector2(sizeX / 2f, sizeY);
					string text = label;
					Color color = GUIStyle.Red;
					GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
					GUI.DrawString(spriteBatch, pos3, text, color, null, 0, subHeadingFont, ForceUpperCase.Inherit);
				}
			}
			foreach (BallastFloraBranch branch in this.Branches)
			{
				Vector2 pos2 = this.Parent.DrawPosition + this.Offset + branch.Position + branch.ShakeAmount;
				pos2.Y = -pos2.Y;
				float depth = branch.IsRootGrowth ? 0.2f : this.BranchDepth;
				float layer = depth + 0.01f;
				float layer2 = depth + 0.02f;
				float layer3 = depth + 0.03f;
				VineSprite branchSprite = this.BranchSprites[branch.Type];
				Color branchColor = (branch.IsRoot || branch.IsRootGrowth) ? this.RootColor : Color.White;
				if (GameMain.DebugDraw)
				{
					if (branch.DisconnectedFromRoot && branch.ParentBranch == null)
					{
						branchColor = Color.Yellow;
					}
					else if (branch.DisconnectedFromRoot)
					{
						branchColor = Color.Cyan;
					}
					else if (branch.ParentBranch == null)
					{
						branchColor = Color.Magenta;
					}
					string label2 = "";
					BallastFloraBranch ballastFloraBranch2 = branch;
					List<BallastFloraBranch> branches = this.Branches;
					if (ballastFloraBranch2 == branches[branches.Count - 1])
					{
						string str = label2;
						string str2 = "Current State: ";
						IBallastFloraState state = this.StateMachine.State;
						label2 = str + str2 + (((state != null) ? state.GetType().Name : null) ?? "null!") + "\n";
					}
					GrowToTargetState targetState = this.StateMachine.State as GrowToTargetState;
					if (targetState != null)
					{
						if (targetState.TargetBranches.Contains(branch))
						{
							GUI.DrawRectangle(spriteBatch, pos2, (float)branch.Rect.Width, (float)branch.Rect.Height, 0f, Color.Red, 0f, 4f);
						}
						List<BallastFloraBranch> targetBranches = targetState.TargetBranches;
						if (targetBranches[targetBranches.Count - 1] == branch)
						{
							label2 = label2 + "Target: " + targetState.Target.Name + "\n";
							Vector2 targetPos2 = targetState.Target.WorldPosition;
							targetPos2.Y = -targetPos2.Y;
							GUI.DrawLine(spriteBatch, pos2, targetPos2, Color.Red, 0f, 4f);
						}
					}
					float num2;
					float num3;
					GUIStyle.SubHeadingFont.MeasureString(label2, false).Deconstruct(out num3, out num2);
					float sizeX2 = num3;
					float sizeY2 = num2;
					Vector2 pos4 = pos2 - new Vector2(sizeX2 / 2f, (float)branch.Rect.Height + sizeY2);
					string text2 = label2;
					Color white = Color.White;
					GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
					GUI.DrawString(spriteBatch, pos4, text2, white, null, 0, subHeadingFont, ForceUpperCase.Inherit);
				}
				bool isDamaged = branch.Health < branch.MaxHealth;
				if (this.HasBrokenThrough)
				{
					if (this.branchAtlas != null && this.branchAtlas.Loaded)
					{
						spriteBatch.Draw(this.branchAtlas.Texture, pos2 + branch.offset, new Rectangle?(branchSprite.SourceRect), branchColor, 0f, branchSprite.AbsoluteOrigin, this.BaseBranchScale * branch.VineStep, SpriteEffects.None, layer2);
					}
					if (this.decayAtlas != null && isDamaged && this.decayAtlas.Loaded)
					{
						spriteBatch.Draw(this.decayAtlas.Texture, pos2 + branch.offset, new Rectangle?(branchSprite.SourceRect), branch.HealthColor, 0f, branchSprite.AbsoluteOrigin, this.BaseBranchScale * branch.VineStep, SpriteEffects.None, layer2 - 1E-06f);
					}
				}
				if (branch.FlowerConfig.Variant >= 0)
				{
					int variant = branch.FlowerConfig.Variant;
					Sprite flowerSprite = this.HasBrokenThrough ? this.FlowerSprites[variant] : this.HiddenFlowerSprites[variant];
					float flowerScale = this.BaseFlowerScale * branch.FlowerConfig.Scale * branch.FlowerStep;
					if (this.HasBrokenThrough)
					{
						flowerScale *= branch.Pulse;
					}
					Sprite sprite = flowerSprite;
					Vector2 pos5 = pos2;
					Color color2 = branchColor;
					Vector2 origin = flowerSprite.Origin;
					float num2 = flowerScale;
					sprite.Draw(spriteBatch, pos5, color2, origin, branch.FlowerConfig.Rotation, num2, SpriteEffects.None, new float?(layer - flowerDepth));
					if (isDamaged && this.HasBrokenThrough && this.DamagedFlowerSprites.Count > variant)
					{
						Sprite sprite2 = this.DamagedFlowerSprites[variant];
						Vector2 pos6 = pos2;
						Color healthColor = branch.HealthColor;
						Vector2 origin2 = flowerSprite.Origin;
						num2 = flowerScale;
						sprite2.Draw(spriteBatch, pos6, healthColor, origin2, branch.FlowerConfig.Rotation, num2, SpriteEffects.None, new float?(layer - flowerDepth - 1E-06f));
					}
					flowerDepth -= 1E-06f;
					if (flowerDepth > 0.01f)
					{
						flowerDepth = 1E-06f;
					}
				}
				if (branch.LeafConfig.Variant >= 0 && this.HasBrokenThrough)
				{
					int variant2 = branch.LeafConfig.Variant;
					Sprite leafSprite = this.LeafSprites[variant2];
					Sprite sprite3 = leafSprite;
					Vector2 pos7 = pos2;
					Color color3 = branchColor;
					Vector2 origin3 = leafSprite.Origin;
					float num2 = this.BaseLeafScale * branch.LeafConfig.Scale * branch.FlowerStep;
					sprite3.Draw(spriteBatch, pos7, color3, origin3, branch.LeafConfig.Rotation, num2, SpriteEffects.None, new float?(layer3 + leafDepth));
					if (isDamaged && this.DamagedLeafSprites.Count > variant2)
					{
						Sprite sprite4 = this.DamagedLeafSprites[variant2];
						Vector2 pos8 = pos2;
						Color healthColor2 = branch.HealthColor;
						Vector2 origin4 = leafSprite.Origin;
						num2 = this.BaseLeafScale * branch.LeafConfig.Scale * branch.FlowerStep;
						sprite4.Draw(spriteBatch, pos8, healthColor2, origin4, branch.LeafConfig.Rotation, num2, SpriteEffects.None, new float?(layer3 + leafDepth - 1E-06f));
					}
					leafDepth += 1E-06f;
					if (leafDepth > 0.01f)
					{
						flowerDepth = 1E-06f;
					}
				}
			}
		}

		// Token: 0x060050DE RID: 20702 RVA: 0x002B93B8 File Offset: 0x002B75B8
		public void ClientRead(IReadMessage msg, BallastFloraBehavior.NetworkHeader header)
		{
			switch (header)
			{
			case BallastFloraBehavior.NetworkHeader.Kill:
				this.Kill();
				break;
			case BallastFloraBehavior.NetworkHeader.BranchCreate:
			{
				int parentId = msg.ReadInt32();
				BallastFloraBranch branch = this.ReadBranch(msg);
				BallastFloraBranch parent = this.Branches.FirstOrDefault((BallastFloraBranch b) => b.ID == parentId);
				if (parent == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Received BranchCreate with an invalid parent ID: ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(parentId);
					defaultInterpolatedStringHandler.AppendLiteral(", Maximum ID is ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(this.Branches.Max((BallastFloraBranch b) => b.ID));
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				this.UpdateConnections(branch, parent);
				this.Branches.Add(branch);
				this.OnBranchGrowthSuccess(branch);
				break;
			}
			case BallastFloraBehavior.NetworkHeader.BranchRemove:
			{
				int removedBranchId = msg.ReadInt32();
				BallastFloraBranch removedBranch = this.Branches.FirstOrDefault((BallastFloraBranch b) => b.ID == removedBranchId);
				if (removedBranch != null)
				{
					this.RemoveBranch(removedBranch);
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Received BranchRemove for a branch that doesn't exist. ID: ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(removedBranchId);
					defaultInterpolatedStringHandler2.AppendLiteral(", Maximum ID is ");
					defaultInterpolatedStringHandler2.AppendFormatted<int>(this.Branches.Max((BallastFloraBranch b) => b.ID));
					DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
				break;
			}
			case BallastFloraBehavior.NetworkHeader.BranchDamage:
			{
				int damageBranchId = msg.ReadInt32();
				float health = msg.ReadSingle();
				BallastFloraBranch damagedBranch = this.Branches.FirstOrDefault((BallastFloraBranch b) => b.ID == damageBranchId);
				if (damagedBranch != null)
				{
					damagedBranch.Health = health;
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(75, 2);
					defaultInterpolatedStringHandler3.AppendLiteral("Received BranchDamage for a branch that doesn't exist. ID: ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(damageBranchId);
					defaultInterpolatedStringHandler3.AppendLiteral(", Maximum ID is ");
					defaultInterpolatedStringHandler3.AppendFormatted<int>(this.Branches.Max((BallastFloraBranch b) => b.ID));
					DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
				}
				break;
			}
			case BallastFloraBehavior.NetworkHeader.Infect:
			{
				int infectBranch = -1;
				ushort itemId = msg.ReadUInt16();
				bool infect = msg.ReadBoolean();
				if (infect)
				{
					infectBranch = msg.ReadInt32();
				}
				Entity entity = Entity.FindEntityByID(itemId);
				Item item = entity as Item;
				if (item != null)
				{
					if (infect)
					{
						this.ClaimTarget(item, this.Branches.FirstOrDefault((BallastFloraBranch b) => b.ID == infectBranch), false);
					}
					else
					{
						this.RemoveClaim(item);
					}
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(73, 3);
					defaultInterpolatedStringHandler4.AppendLiteral("Received Infect.");
					defaultInterpolatedStringHandler4.AppendFormatted<bool>(infect);
					defaultInterpolatedStringHandler4.AppendLiteral(" Network Header with invalid item ID: ");
					defaultInterpolatedStringHandler4.AppendFormatted<ushort>(itemId);
					defaultInterpolatedStringHandler4.AppendLiteral(", which belongs to ");
					defaultInterpolatedStringHandler4.AppendFormatted(((entity != null) ? entity.ToString() : null) ?? "null!");
					DebugConsole.AddWarning(defaultInterpolatedStringHandler4.ToStringAndClear(), null);
				}
				break;
			}
			case BallastFloraBehavior.NetworkHeader.Remove:
				this.Remove();
				break;
			}
			this.PowerConsumptionTimer = msg.ReadSingle();
		}

		// Token: 0x060050DF RID: 20703 RVA: 0x002B96F8 File Offset: 0x002B78F8
		private BallastFloraBranch ReadBranch(IReadMessage msg)
		{
			int id = msg.ReadInt32();
			bool isRootGrowth = msg.ReadBoolean();
			byte type = (byte)msg.ReadRangedInteger(0, 15);
			byte sides = (byte)msg.ReadRangedInteger(0, 15);
			int flowerConfig = msg.ReadRangedInteger(0, 4095);
			int leafConfig = msg.ReadRangedInteger(0, 4095);
			int maxHealth = (int)msg.ReadUInt16();
			int posX = msg.ReadInt32();
			int posY = msg.ReadInt32();
			int parentBranchIndex = msg.ReadInt32();
			Vector2 pos = new Vector2((float)(posX * VineTile.Size), (float)(posY * VineTile.Size));
			BallastFloraBranch parentBranch = (parentBranchIndex < 0 || parentBranchIndex >= this.Branches.Count) ? null : this.Branches[parentBranchIndex];
			return new BallastFloraBranch(this, parentBranch, pos, (VineTileType)type, new FoliageConfig?(FoliageConfig.Deserialize(flowerConfig)), new FoliageConfig?(FoliageConfig.Deserialize(leafConfig)), null)
			{
				ID = id,
				MaxHealth = (float)maxHealth,
				Sides = (TileSide)sides,
				IsRootGrowth = isRootGrowth
			};
		}

		// Token: 0x170014A5 RID: 5285
		// (get) Token: 0x060050E0 RID: 20704 RVA: 0x002B97EF File Offset: 0x002B79EF
		public static IEnumerable<BallastFloraBehavior> EntityList
		{
			get
			{
				return BallastFloraBehavior._entityList;
			}
		}

		// Token: 0x170014A6 RID: 5286
		// (get) Token: 0x060050E1 RID: 20705 RVA: 0x002B97F6 File Offset: 0x002B79F6
		// (set) Token: 0x060050E2 RID: 20706 RVA: 0x002B97FE File Offset: 0x002B79FE
		[Serialize(0.25f, IsPropertySaveable.Yes, "Scale of the branches.", "", false)]
		public float BaseBranchScale { get; set; }

		// Token: 0x170014A7 RID: 5287
		// (get) Token: 0x060050E3 RID: 20707 RVA: 0x002B9807 File Offset: 0x002B7A07
		// (set) Token: 0x060050E4 RID: 20708 RVA: 0x002B980F File Offset: 0x002B7A0F
		[Serialize(0.25f, IsPropertySaveable.Yes, "Scale of the flowers.", "", false)]
		public float BaseFlowerScale { get; set; }

		// Token: 0x170014A8 RID: 5288
		// (get) Token: 0x060050E5 RID: 20709 RVA: 0x002B9818 File Offset: 0x002B7A18
		// (set) Token: 0x060050E6 RID: 20710 RVA: 0x002B9820 File Offset: 0x002B7A20
		[Serialize(0.5f, IsPropertySaveable.Yes, "Scale of the leaves.", "", false)]
		public float BaseLeafScale { get; set; }

		// Token: 0x170014A9 RID: 5289
		// (get) Token: 0x060050E7 RID: 20711 RVA: 0x002B9829 File Offset: 0x002B7A29
		// (set) Token: 0x060050E8 RID: 20712 RVA: 0x002B9831 File Offset: 0x002B7A31
		[Serialize(0.33f, IsPropertySaveable.Yes, "Chance for a flower to appear on a branch.", "", false)]
		public float FlowerProbability { get; set; }

		// Token: 0x170014AA RID: 5290
		// (get) Token: 0x060050E9 RID: 20713 RVA: 0x002B983A File Offset: 0x002B7A3A
		// (set) Token: 0x060050EA RID: 20714 RVA: 0x002B9842 File Offset: 0x002B7A42
		[Serialize(0.7f, IsPropertySaveable.Yes, "Chance for leaves to appear on a branch.", "", false)]
		public float LeafProbability { get; set; }

		// Token: 0x170014AB RID: 5291
		// (get) Token: 0x060050EB RID: 20715 RVA: 0x002B984B File Offset: 0x002B7A4B
		// (set) Token: 0x060050EC RID: 20716 RVA: 0x002B9853 File Offset: 0x002B7A53
		[Serialize(3f, IsPropertySaveable.Yes, "Delay between pulses.", "", false)]
		public float PulseDelay { get; set; }

		// Token: 0x170014AC RID: 5292
		// (get) Token: 0x060050ED RID: 20717 RVA: 0x002B985C File Offset: 0x002B7A5C
		// (set) Token: 0x060050EE RID: 20718 RVA: 0x002B9864 File Offset: 0x002B7A64
		[Serialize(3f, IsPropertySaveable.Yes, "How fast the flower inflates during a pulse.", "", false)]
		public float PulseInflateSpeed { get; set; }

		// Token: 0x170014AD RID: 5293
		// (get) Token: 0x060050EF RID: 20719 RVA: 0x002B986D File Offset: 0x002B7A6D
		// (set) Token: 0x060050F0 RID: 20720 RVA: 0x002B9875 File Offset: 0x002B7A75
		[Serialize(1f, IsPropertySaveable.Yes, "How fast the flower deflates.", "", false)]
		public float PulseDeflateSpeed { get; set; }

		// Token: 0x170014AE RID: 5294
		// (get) Token: 0x060050F1 RID: 20721 RVA: 0x002B987E File Offset: 0x002B7A7E
		// (set) Token: 0x060050F2 RID: 20722 RVA: 0x002B9886 File Offset: 0x002B7A86
		[Serialize(32, IsPropertySaveable.Yes, "How many vines must grow before the plant breaks through the wall.", "", false)]
		public int BreakthroughPoint { get; set; }

		// Token: 0x170014AF RID: 5295
		// (get) Token: 0x060050F3 RID: 20723 RVA: 0x002B988F File Offset: 0x002B7A8F
		// (set) Token: 0x060050F4 RID: 20724 RVA: 0x002B9897 File Offset: 0x002B7A97
		[Serialize(false, IsPropertySaveable.Yes, "Has the plant grown large enough to expose itself.", "", false)]
		public bool HasBrokenThrough { get; set; }

		// Token: 0x170014B0 RID: 5296
		// (get) Token: 0x060050F5 RID: 20725 RVA: 0x002B98A0 File Offset: 0x002B7AA0
		// (set) Token: 0x060050F6 RID: 20726 RVA: 0x002B98A8 File Offset: 0x002B7AA8
		[Serialize(300, IsPropertySaveable.Yes, "How far the ballast flora can detect items from.", "", false)]
		public int Sight { get; set; }

		// Token: 0x170014B1 RID: 5297
		// (get) Token: 0x060050F7 RID: 20727 RVA: 0x002B98B1 File Offset: 0x002B7AB1
		// (set) Token: 0x060050F8 RID: 20728 RVA: 0x002B98B9 File Offset: 0x002B7AB9
		[Serialize(100, IsPropertySaveable.Yes, "How much health the branches have.", "", false)]
		public int BranchHealth { get; set; }

		// Token: 0x170014B2 RID: 5298
		// (get) Token: 0x060050F9 RID: 20729 RVA: 0x002B98C2 File Offset: 0x002B7AC2
		// (set) Token: 0x060050FA RID: 20730 RVA: 0x002B98CA File Offset: 0x002B7ACA
		[Serialize(400, IsPropertySaveable.Yes, "How much health the root has.", "", false)]
		public int RootHealth { get; set; }

		// Token: 0x170014B3 RID: 5299
		// (get) Token: 0x060050FB RID: 20731 RVA: 0x002B98D3 File Offset: 0x002B7AD3
		// (set) Token: 0x060050FC RID: 20732 RVA: 0x002B98DB File Offset: 0x002B7ADB
		[Serialize(0.00025f, IsPropertySaveable.Yes, "How fast the root's health regenerates per each grown branch.", "", false)]
		public float HealthRegenPerBranch { get; set; }

		// Token: 0x170014B4 RID: 5300
		// (get) Token: 0x060050FD RID: 20733 RVA: 0x002B98E4 File Offset: 0x002B7AE4
		// (set) Token: 0x060050FE RID: 20734 RVA: 0x002B98EC File Offset: 0x002B7AEC
		[Serialize(30, IsPropertySaveable.Yes, "How far away from the root branches can regenerate health (in number of branches). The amount of regen decreases lineary further from the root.", "", false)]
		public int MaxBranchHealthRegenDistance { get; set; }

		// Token: 0x170014B5 RID: 5301
		// (get) Token: 0x060050FF RID: 20735 RVA: 0x002B98F5 File Offset: 0x002B7AF5
		// (set) Token: 0x06005100 RID: 20736 RVA: 0x002B98FD File Offset: 0x002B7AFD
		[Serialize("255,255,255,255", IsPropertySaveable.Yes, "", "", false)]
		public Color RootColor { get; set; }

		// Token: 0x170014B6 RID: 5302
		// (get) Token: 0x06005101 RID: 20737 RVA: 0x002B9906 File Offset: 0x002B7B06
		// (set) Token: 0x06005102 RID: 20738 RVA: 0x002B990E File Offset: 0x002B7B0E
		[Serialize(300f, IsPropertySaveable.Yes, "How much power the ballast flora takes from junction boxes.", "", false)]
		public float PowerConsumptionMin { get; set; }

		// Token: 0x170014B7 RID: 5303
		// (get) Token: 0x06005103 RID: 20739 RVA: 0x002B9917 File Offset: 0x002B7B17
		// (set) Token: 0x06005104 RID: 20740 RVA: 0x002B991F File Offset: 0x002B7B1F
		[Serialize(3000f, IsPropertySaveable.Yes, "How much the power drain spikes.", "", false)]
		public float PowerConsumptionMax { get; set; }

		// Token: 0x170014B8 RID: 5304
		// (get) Token: 0x06005105 RID: 20741 RVA: 0x002B9928 File Offset: 0x002B7B28
		// (set) Token: 0x06005106 RID: 20742 RVA: 0x002B9930 File Offset: 0x002B7B30
		[Serialize(10f, IsPropertySaveable.Yes, "How long it takes for power drain to wind down.", "", false)]
		public float PowerConsumptionDuration { get; set; }

		// Token: 0x170014B9 RID: 5305
		// (get) Token: 0x06005107 RID: 20743 RVA: 0x002B9939 File Offset: 0x002B7B39
		// (set) Token: 0x06005108 RID: 20744 RVA: 0x002B9941 File Offset: 0x002B7B41
		[Serialize(250f, IsPropertySaveable.Yes, "How much power does it take to accelerate growth.", "", false)]
		public float PowerRequirement { get; set; }

		// Token: 0x170014BA RID: 5306
		// (get) Token: 0x06005109 RID: 20745 RVA: 0x002B994A File Offset: 0x002B7B4A
		// (set) Token: 0x0600510A RID: 20746 RVA: 0x002B9952 File Offset: 0x002B7B52
		[Serialize(5f, IsPropertySaveable.Yes, "Maximum anger, anger increases when the plant gets damaged and increases growth speed.", "", false)]
		public float MaxAnger { get; set; }

		// Token: 0x170014BB RID: 5307
		// (get) Token: 0x0600510B RID: 20747 RVA: 0x002B995B File Offset: 0x002B7B5B
		// (set) Token: 0x0600510C RID: 20748 RVA: 0x002B9963 File Offset: 0x002B7B63
		[Serialize(10000f, IsPropertySaveable.Yes, "Maximum power buffer.", "", false)]
		public float MaxPowerCapacity { get; set; }

		// Token: 0x170014BC RID: 5308
		// (get) Token: 0x0600510D RID: 20749 RVA: 0x002B996C File Offset: 0x002B7B6C
		// (set) Token: 0x0600510E RID: 20750 RVA: 0x002B9974 File Offset: 0x002B7B74
		[Serialize("", IsPropertySaveable.Yes, "Item prefab that is spawned when threatened.", "", false)]
		public Identifier AttackItemPrefab { get; set; } = Identifier.Empty;

		// Token: 0x170014BD RID: 5309
		// (get) Token: 0x0600510F RID: 20751 RVA: 0x002B997D File Offset: 0x002B7B7D
		// (set) Token: 0x06005110 RID: 20752 RVA: 0x002B9985 File Offset: 0x002B7B85
		[Serialize(0.8f, IsPropertySaveable.Yes, "How resistant the ballast flora is to explosives before it blooms.", "", false)]
		public float ExplosionResistance { get; set; }

		// Token: 0x170014BE RID: 5310
		// (get) Token: 0x06005111 RID: 20753 RVA: 0x002B998E File Offset: 0x002B7B8E
		// (set) Token: 0x06005112 RID: 20754 RVA: 0x002B9996 File Offset: 0x002B7B96
		[Serialize(5f, IsPropertySaveable.Yes, "How much damage is taken from open fires.", "", false)]
		public float FireVulnerability { get; set; }

		// Token: 0x170014BF RID: 5311
		// (get) Token: 0x06005113 RID: 20755 RVA: 0x002B999F File Offset: 0x002B7B9F
		// (set) Token: 0x06005114 RID: 20756 RVA: 0x002B99A7 File Offset: 0x002B7BA7
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much resistance against fire is gained while submerged.", "", false)]
		public float SubmergedWaterResistance { get; set; }

		// Token: 0x170014C0 RID: 5312
		// (get) Token: 0x06005115 RID: 20757 RVA: 0x002B99B0 File Offset: 0x002B7BB0
		// (set) Token: 0x06005116 RID: 20758 RVA: 0x002B99B8 File Offset: 0x002B7BB8
		[Serialize(0.8f, IsPropertySaveable.Yes, "What depth the branches will be drawn on.", "", false)]
		public float BranchDepth { get; set; }

		// Token: 0x170014C1 RID: 5313
		// (get) Token: 0x06005117 RID: 20759 RVA: 0x002B99C1 File Offset: 0x002B7BC1
		// (set) Token: 0x06005118 RID: 20760 RVA: 0x002B99C9 File Offset: 0x002B7BC9
		[Serialize("", IsPropertySaveable.Yes, "What sound to play when the ballast flora bursts through walls.", "", false)]
		public string BurstSound { get; set; } = "";

		// Token: 0x170014C2 RID: 5314
		// (get) Token: 0x06005119 RID: 20761 RVA: 0x002B99D2 File Offset: 0x002B7BD2
		// (set) Token: 0x0600511A RID: 20762 RVA: 0x002B99DA File Offset: 0x002B7BDA
		[Serialize(0f, IsPropertySaveable.Yes, "How much power the ballast flora has stored.", "", false)]
		public float AvailablePower
		{
			get
			{
				return this.availablePower;
			}
			set
			{
				this.availablePower = Math.Max(value, this.MaxPowerCapacity);
			}
		}

		// Token: 0x170014C3 RID: 5315
		// (get) Token: 0x0600511B RID: 20763 RVA: 0x002B99EE File Offset: 0x002B7BEE
		// (set) Token: 0x0600511C RID: 20764 RVA: 0x002B99F6 File Offset: 0x002B7BF6
		[Serialize(1f, IsPropertySaveable.Yes, "How enraged the flora is, affects how fast it grows.", "", false)]
		public float Anger
		{
			get
			{
				return this.anger;
			}
			set
			{
				this.anger = Math.Clamp(value, 1f, this.MaxAnger);
			}
		}

		// Token: 0x170014C4 RID: 5316
		// (get) Token: 0x0600511D RID: 20765 RVA: 0x002B9A0F File Offset: 0x002B7C0F
		public string Name { get; } = "";

		// Token: 0x170014C5 RID: 5317
		// (get) Token: 0x0600511E RID: 20766 RVA: 0x002B9A17 File Offset: 0x002B7C17
		// (set) Token: 0x0600511F RID: 20767 RVA: 0x002B9A1F File Offset: 0x002B7C1F
		public Hull Parent { get; private set; }

		// Token: 0x170014C6 RID: 5318
		// (get) Token: 0x06005120 RID: 20768 RVA: 0x002B9A28 File Offset: 0x002B7C28
		// (set) Token: 0x06005121 RID: 20769 RVA: 0x002B9A30 File Offset: 0x002B7C30
		public BallastFloraPrefab Prefab { get; private set; }

		// Token: 0x170014C7 RID: 5319
		// (get) Token: 0x06005122 RID: 20770 RVA: 0x002B9A39 File Offset: 0x002B7C39
		// (set) Token: 0x06005123 RID: 20771 RVA: 0x002B9A41 File Offset: 0x002B7C41
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06005124 RID: 20772 RVA: 0x002B9A4C File Offset: 0x002B7C4C
		public void OnMapLoaded()
		{
			using (List<Tuple<ushort, int>>.Enumerator enumerator = this.tempClaimedTargets.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ushort num;
					int branchid2;
					enumerator.Current.Deconstruct(out num, out branchid2);
					ushort itemId = num;
					int branchid = branchid2;
					Item item = Entity.FindEntityByID(itemId) as Item;
					if (item != null)
					{
						this.ClaimTarget(item, this.Branches.FirstOrDefault((BallastFloraBranch b) => b.ID == branchid), true);
					}
					else
					{
						string errorMsg = "Error in BallastFloraBehavior.OnMapLoaded: could not find the item claimed by the ballast flora.";
						DebugConsole.ThrowError(errorMsg, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("BallastFloraBehavior.OnMapLoaded:ClaimedItemNotFound", GameAnalyticsManager.ErrorSeverity.Warning, errorMsg);
					}
				}
			}
			foreach (BallastFloraBranch branch in this.Branches)
			{
				this.SetHull(branch);
				if (branch.ClaimedItemId > -1)
				{
					Item item2 = Entity.FindEntityByID((ushort)branch.ClaimedItemId) as Item;
					if (item2 != null)
					{
						branch.ClaimedItem = item2;
					}
					else
					{
						string errorMsg2 = "Error in BallastFloraBehavior.OnMapLoaded: could not find the item claimed by a branch.";
						DebugConsole.ThrowError(errorMsg2, null, null, false, false);
						GameAnalyticsManager.AddErrorEventOnce("BallastFloraBehavior.OnMapLoaded:BranchClaimedItemNotFound", GameAnalyticsManager.ErrorSeverity.Warning, errorMsg2);
					}
				}
				this.UpdateConnections(branch, null);
				this.CreateBody(branch);
			}
		}

		// Token: 0x06005125 RID: 20773 RVA: 0x002B9BA4 File Offset: 0x002B7DA4
		private int CreateID()
		{
			int num;
			if (!this.Branches.Any<BallastFloraBranch>())
			{
				num = 0;
			}
			else
			{
				num = this.Branches.Max((BallastFloraBranch b) => b.ID);
			}
			int maxId = num;
			return maxId + 1;
		}

		// Token: 0x06005126 RID: 20774 RVA: 0x002B9BF1 File Offset: 0x002B7DF1
		public Vector2 GetWorldPosition()
		{
			return this.Parent.WorldPosition + this.Offset;
		}

		// Token: 0x06005127 RID: 20775 RVA: 0x002B9C0C File Offset: 0x002B7E0C
		public BallastFloraBehavior(Hull parent, BallastFloraPrefab prefab, Vector2 offset, bool firstGrowth = false)
		{
			this.Prefab = prefab;
			this.Offset = offset;
			this.Parent = parent;
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, prefab.Element);
			this.LoadPrefab(prefab.Element);
			this.StateMachine = new BallastFloraStateMachine(this);
			if (firstGrowth)
			{
				this.GenerateRoot();
			}
			BallastFloraBehavior._entityList.Add(this);
		}

		// Token: 0x06005128 RID: 20776 RVA: 0x002B9D60 File Offset: 0x002B7F60
		private void LoadPrefab(ContentXElement element)
		{
			ContentPath branchAtlasPath = element.GetAttributeContentPath("branchatlas");
			if (branchAtlasPath != null)
			{
				this.branchAtlas = new Sprite(branchAtlasPath.Value, new Rectangle?(Rectangle.Empty), null, 0f);
			}
			ContentPath decayAtlasPath = element.GetAttributeContentPath("decayatlas");
			if (decayAtlasPath != null)
			{
				this.decayAtlas = new Sprite(decayAtlasPath.Value, new Rectangle?(Rectangle.Empty), null, 0f);
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 7:
						if (text == "targets")
						{
							this.LoadTargets(subElement);
						}
						break;
					case 10:
						if (text == "leafsprite")
						{
							this.LeafSprites.Add(new Sprite(subElement, "", "", false, 1f));
						}
						break;
					case 12:
					{
						char c = text[0];
						if (c != 'b')
						{
							if (c == 'f')
							{
								if (text == "flowersprite")
								{
									this.FlowerSprites.Add(new Sprite(subElement, "", "", false, 1f));
								}
							}
						}
						else if (text == "branchsprite")
						{
							ContentXElement contentXElement = subElement;
							string key = "type";
							VineTileType vineTileType = VineTileType.Stem;
							VineTileType type = contentXElement.GetAttributeEnum<VineTileType>(key, vineTileType);
							this.BranchSprites.Add(type, new VineSprite(subElement));
						}
						break;
					}
					case 13:
						if (text == "deathparticle")
						{
							this.DeathParticles.Add(new ParticleEmitter(subElement));
						}
						break;
					case 14:
						if (text == "damageparticle")
						{
							this.DamageParticles.Add(new ParticleEmitter(subElement));
						}
						break;
					case 17:
						if (text == "damagedleafsprite")
						{
							this.DamagedLeafSprites.Add(new Sprite(subElement, "", "", false, 1f));
						}
						break;
					case 18:
						if (text == "hiddenflowersprite")
						{
							this.HiddenFlowerSprites.Add(new Sprite(subElement, "", "", false, 1f));
						}
						break;
					case 19:
						if (text == "damagedflowersprite")
						{
							this.DamagedFlowerSprites.Add(new Sprite(subElement, "", "", false, 1f));
						}
						break;
					}
				}
				this.flowerVariants = this.FlowerSprites.Count;
				this.leafVariants = this.LeafSprites.Count;
			}
		}

		// Token: 0x06005129 RID: 20777 RVA: 0x002BA0A8 File Offset: 0x002B82A8
		public void LoadTargets(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				this.Targets.Add(new BallastFloraBehavior.AITarget(subElement));
			}
		}

		// Token: 0x0600512A RID: 20778 RVA: 0x002BA100 File Offset: 0x002B8300
		public void Save(XElement element)
		{
			XElement saveElement = new XElement("BallastFloraBehavior", new object[]
			{
				new XAttribute("identifier", this.Prefab.Identifier),
				new XAttribute("offset", XMLExtensions.Vector2ToString(this.Offset))
			});
			SerializableProperty.SerializeProperties(this, saveElement, false, false);
			foreach (BallastFloraBranch branch in this.Branches)
			{
				XElement be = new XElement("Branch", new object[]
				{
					new XAttribute("flowerconfig", branch.FlowerConfig.Serialize()),
					new XAttribute("leafconfig", branch.LeafConfig.Serialize()),
					new XAttribute("pos", XMLExtensions.Vector2ToString(branch.Position)),
					new XAttribute("ID", branch.ID),
					new XAttribute("isroot", branch.IsRoot),
					new XAttribute("isrootgrowth", branch.IsRootGrowth),
					new XAttribute("health", branch.Health.ToString("G", CultureInfo.InvariantCulture)),
					new XAttribute("maxhealth", branch.MaxHealth.ToString("G", CultureInfo.InvariantCulture)),
					new XAttribute("sides", (int)branch.Sides),
					new XAttribute("blockedsides", (int)branch.BlockedSides),
					new XAttribute("tile", (int)branch.Type)
				});
				if (branch.ClaimedItem != null)
				{
					XContainer xcontainer = be;
					XName name = "claimed";
					Item claimedItem = branch.ClaimedItem;
					xcontainer.Add(new XAttribute(name, ((int)((claimedItem != null) ? new ushort?(claimedItem.ID) : null)) ?? -1));
				}
				if (branch.ParentBranch != null && !branch.ParentBranch.Removed)
				{
					XContainer xcontainer2 = be;
					XName name2 = "parentbranch";
					BallastFloraBranch parentBranch = branch.ParentBranch;
					xcontainer2.Add(new XAttribute(name2, (parentBranch != null) ? parentBranch.ID : -1));
				}
				saveElement.Add(be);
			}
			foreach (Item target in this.ClaimedTargets)
			{
				if (target.Infector == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(74, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Error in BallastFloraBehavior.Save: claimed target \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(target.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\" had no infector set.");
					string errorMsg = defaultInterpolatedStringHandler.ToStringAndClear();
					DebugConsole.ThrowError(errorMsg, null, null, false, false);
					GameAnalyticsManager.AddErrorEventOnce("BallastFloraBehavior.Save:InfectorNull", GameAnalyticsManager.ErrorSeverity.Warning, errorMsg);
				}
				else
				{
					XElement te = new XElement("ClaimedTarget", new object[]
					{
						new XAttribute("id", target.ID),
						new XAttribute("branchId", target.Infector.ID)
					});
					saveElement.Add(te);
				}
			}
			element.Add(saveElement);
		}

		// Token: 0x0600512B RID: 20779 RVA: 0x002BA4EC File Offset: 0x002B86EC
		public void LoadSave(XElement element, IdRemap idRemap)
		{
			BallastFloraBehavior.<>c__DisplayClass188_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.branches = new List<ValueTuple<BallastFloraBranch, int>>();
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			this.Offset = element.GetAttributeVector2("offset", Vector2.Zero);
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "branch"))
				{
					if (a == "claimedtarget")
					{
						int id = subElement.GetAttributeInt("id", -1);
						int branchId = subElement.GetAttributeInt("branchId", -1);
						if (id > 0)
						{
							this.tempClaimedTargets.Add(Tuple.Create<ushort, int>(idRemap.GetOffsetId(id), branchId));
						}
					}
				}
				else
				{
					this.<LoadSave>g__LoadBranch|188_1(subElement, idRemap, ref CS$<>8__locals1);
				}
			}
			foreach (ValueTuple<BallastFloraBranch, int> valueTuple in CS$<>8__locals1.branches)
			{
				BallastFloraBranch branch = valueTuple.Item1;
				int parentBranchId = valueTuple.Item2;
				if (parentBranchId > -1)
				{
					BallastFloraBranch parentBranch = this.Branches.Find((BallastFloraBranch b) => b.ID == parentBranchId);
					if (parentBranch == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Error while loading ballast flora: couldn't find a parent branch with the ID ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(parentBranchId);
						DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					}
					else
					{
						branch.ParentBranch = parentBranch;
					}
				}
			}
			if (this.root == null)
			{
				this.Branches.ForEach(delegate(BallastFloraBranch b)
				{
					b.DisconnectedFromRoot = true;
				});
				return;
			}
			this.CheckDisconnectedFromRoot();
		}

		// Token: 0x0600512C RID: 20780 RVA: 0x002BA6E0 File Offset: 0x002B88E0
		public void Update(float deltaTime)
		{
			if ((GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) && this.Branches.Count == 0)
			{
				this.Remove();
				return;
			}
			foreach (BallastFloraBranch branch in this.Branches)
			{
				branch.UpdateScale(deltaTime);
				branch.UpdatePulse(deltaTime, this.PulseInflateSpeed, this.PulseDeflateSpeed, this.PulseDelay);
				branch.UpdateHealth();
			}
			this.UpdateDamage(deltaTime);
			this.UpdatePowerDrain(deltaTime);
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.root != null && this.HealthRegenPerBranch > 0f)
			{
				float healAmount = (float)this.Branches.Count((BallastFloraBranch b) => !b.IsRoot && !b.IsRootGrowth && !b.DisconnectedFromRoot) * this.HealthRegenPerBranch;
				foreach (BallastFloraBranch branch2 in this.Branches)
				{
					if (branch2.Health <= branch2.MaxHealth * 0.9f && !branch2.DisconnectedFromRoot)
					{
						float branchHealAmount = (float)(this.MaxBranchHealthRegenDistance - branch2.BranchDepth) / (float)this.MaxBranchHealthRegenDistance * healAmount;
						if (branchHealAmount > 0f)
						{
							float prevHealth = branch2.Health;
							branch2.Health += branchHealAmount;
							branch2.AccumulatedDamage += prevHealth - branch2.Health;
						}
					}
				}
			}
			this.StateMachine.Update(deltaTime);
			if (this.HasBrokenThrough)
			{
				if (this.fireCheckCooldown <= 0f)
				{
					this.UpdateFireSources();
					this.fireCheckCooldown = 10f;
				}
				else
				{
					this.fireCheckCooldown -= deltaTime;
				}
				foreach (BallastFloraBranch branch3 in this.branchesVulnerableToFire)
				{
					if (!branch3.Removed)
					{
						this.DamageBranch(branch3, this.FireVulnerability * deltaTime, BallastFloraBehavior.AttackType.Fire, null);
					}
				}
			}
			this.UpdateSelfDamage(deltaTime);
			if (this.Anger > 1f)
			{
				this.Anger -= deltaTime;
			}
			if (this.toxinsTimer > 0.1f)
			{
				this.toxinsSpawnTimer -= deltaTime;
				if (!this.AttackItemPrefab.IsEmpty && this.toxinsSpawnTimer <= 0f)
				{
					this.toxinsSpawnTimer = 1f;
					Dictionary<Hull, List<BallastFloraBranch>> branches = new Dictionary<Hull, List<BallastFloraBranch>>();
					foreach (BallastFloraBranch branch4 in this.Branches)
					{
						if (branch4.CurrentHull != null && branch4.FlowerConfig.Variant >= 0 && !branch4.DisconnectedFromRoot)
						{
							List<BallastFloraBranch> list;
							if (branches.TryGetValue(branch4.CurrentHull, out list))
							{
								list.Add(branch4);
							}
							else
							{
								branches.Add(branch4.CurrentHull, new List<BallastFloraBranch>
								{
									branch4
								});
							}
						}
					}
					foreach (Hull hull in branches.Keys)
					{
						List<BallastFloraBranch> list2 = branches[hull];
						IEnumerable<BallastFloraBranch> source = list2;
						Func<BallastFloraBranch, bool> predicate;
						if ((predicate = BallastFloraBehavior.<>O.<0>__HasAcidEmitter) == null)
						{
							predicate = (BallastFloraBehavior.<>O.<0>__HasAcidEmitter = new Func<BallastFloraBranch, bool>(BallastFloraBehavior.<Update>g__HasAcidEmitter|189_1));
						}
						if (!source.Any(predicate))
						{
							BallastFloraBranch randomBranch = branches[hull].GetRandomUnsynced<BallastFloraBranch>();
							if (randomBranch != null)
							{
								randomBranch.SpawningItem = true;
								ItemPrefab prefab = ItemPrefab.Find(null, this.AttackItemPrefab);
								EntitySpawner spawner = Entity.Spawner;
								if (spawner != null)
								{
									spawner.AddItemToSpawnQueue(prefab, this.Parent.Position + this.Offset + randomBranch.Position, this.Parent.Submarine, null, null, delegate(Item item)
									{
										randomBranch.AttackItem = item;
										randomBranch.SpawningItem = false;
									});
								}
							}
						}
					}
				}
				this.toxinsTimer -= deltaTime;
			}
			if (this.defenseCooldown >= 0f)
			{
				this.defenseCooldown -= deltaTime;
			}
			if (this.toxinsCooldown >= 0f)
			{
				this.toxinsCooldown -= deltaTime;
			}
		}

		// Token: 0x0600512D RID: 20781 RVA: 0x002BABA4 File Offset: 0x002B8DA4
		private void UpdateDamage(float deltaTime)
		{
			foreach (BallastFloraBranch branch in this.Branches)
			{
				if (!branch.IsRoot || !this.isDead)
				{
					if (branch.AccumulatedDamage > 0f)
					{
						this.CreateDamageParticle(branch, branch.AccumulatedDamage);
						if (GameMain.DebugDraw)
						{
							Hull parent = this.Parent;
							Vector2 pos = ((parent != null) ? parent.Position : Vector2.Zero) + this.Offset + branch.Position;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
							defaultInterpolatedStringHandler.AppendFormatted<int>((int)branch.AccumulatedDamage);
							string message = defaultInterpolatedStringHandler.ToStringAndClear();
							Color color = GUIStyle.Red;
							Vector2 pos2 = pos;
							Vector2 velocity = Vector2.UnitY * 10f;
							float lifeTime = 3f;
							bool playSound = false;
							GUISoundType soundType = GUISoundType.UIMessage;
							Hull parent2 = this.Parent;
							int? num;
							if (parent2 == null)
							{
								num = null;
							}
							else
							{
								Submarine submarine = parent2.Submarine;
								num = ((submarine != null) ? new ushort?(submarine.ID) : null);
							}
							GUI.AddMessage(message, color, pos2, velocity, lifeTime, playSound, soundType, num ?? -1);
						}
					}
					if (Character.Controlled != null && Character.Controlled.CurrentHull == branch.CurrentHull && branch.IsRoot && (branch.AccumulatedDamage > 0f || branch.AccumulatedDamage < -0.1f))
					{
						Character.Controlled.UpdateHUDProgressBar(this, this.GetWorldPosition() + branch.Position, branch.Health / branch.MaxHealth, GUIStyle.HealthBarColorLow, GUIStyle.HealthBarColorHigh, this.Prefab.DisplayName.Value);
					}
					branch.AccumulatedDamage = 0f;
					if (branch.DamageVisualizationTimer > 0f)
					{
						branch.DamageVisualizationTimer -= deltaTime;
						float t = (float)Timing.TotalTime * 0.2f + branch.Position.X / 100f;
						float t2 = (float)Timing.TotalTime * 0.5f + branch.Position.Y / 100f;
						branch.ShakeAmount = new Vector2(PerlinNoise.GetPerlin(t, t2) - 0.5f, PerlinNoise.GetPerlin(t2, t) - 0.5f) * 10f * branch.DamageVisualizationTimer;
					}
				}
			}
		}

		// Token: 0x0600512E RID: 20782 RVA: 0x002BAE2C File Offset: 0x002B902C
		private void UpdateSelfDamage(float deltaTime)
		{
			if (this.selfDamageTimer <= 0f)
			{
				if (!this.HasBrokenThrough && !this.CanGrowMore())
				{
					this.Branches.ForEachMod(delegate(BallastFloraBranch branch)
					{
						float maxHealth = (float)(branch.IsRoot ? this.RootHealth : this.BranchHealth);
						this.DamageBranch(branch, Rand.Range(1f, maxHealth, Rand.RandSync.Unsynced), BallastFloraBehavior.AttackType.Other, null);
					});
				}
				this.selfDamageTimer = 1f;
			}
			this.toBeRemoved.Clear();
			foreach (BallastFloraBranch branch3 in this.Branches)
			{
				if (!branch3.IsRoot && (branch3.ParentBranch == null || branch3.ParentBranch.DisconnectedFromRoot || branch3.ParentBranch.Health <= 0f))
				{
					float parentHealth = (branch3.ParentBranch == null) ? 0f : (branch3.ParentBranch.Health / branch3.ParentBranch.MaxHealth);
					float speed = MathHelper.Lerp(5f, 0.1f, parentHealth);
					this.DamageBranch(branch3, speed * speed * deltaTime, BallastFloraBehavior.AttackType.CutFromRoot, null);
				}
				if (branch3.Health <= 0f)
				{
					if (branch3.ClaimedItem != null)
					{
						this.RemoveClaim(branch3.ClaimedItem);
					}
					branch3.RemoveTimer -= deltaTime;
					if (branch3.RemoveTimer <= 0f)
					{
						this.toBeRemoved.Add(branch3);
					}
				}
			}
			foreach (BallastFloraBranch branch2 in this.toBeRemoved)
			{
				this.RemoveBranch(branch2);
			}
			this.selfDamageTimer -= deltaTime;
		}

		// Token: 0x0600512F RID: 20783 RVA: 0x002BAFD8 File Offset: 0x002B91D8
		private void UpdatePowerDrain(float deltaTime)
		{
			this.PowerConsumptionTimer += deltaTime;
			if (this.PowerConsumptionTimer > this.PowerConsumptionDuration)
			{
				this.PowerConsumptionTimer = 0f;
			}
			float powerConsumption = MathHelper.Lerp(this.PowerConsumptionMax, this.PowerConsumptionMin, this.PowerConsumptionTimer / this.PowerConsumptionDuration);
			float powerDelta = powerConsumption * deltaTime;
			foreach (PowerTransfer jb in this.ClaimedJunctionBoxes)
			{
				if (jb.ExtraLoad <= Math.Max(this.PowerConsumptionMin, this.PowerConsumptionMax))
				{
					jb.ExtraLoad = powerConsumption;
					float currPowerConsumption = -jb.CurrPowerConsumption;
					if (currPowerConsumption > powerDelta)
					{
						this.AvailablePower += powerDelta;
					}
					else
					{
						this.AvailablePower += currPowerConsumption * deltaTime;
					}
				}
			}
			float batteryDrain = powerDelta * 0.1f;
			foreach (PowerContainer battery in this.ClaimedBatteries)
			{
				float amount = Math.Min(battery.MaxOutPut, batteryDrain);
				if (battery.Charge > amount)
				{
					battery.Charge -= amount;
					this.AvailablePower += amount;
				}
			}
		}

		// Token: 0x06005130 RID: 20784 RVA: 0x002BB140 File Offset: 0x002B9340
		private void UpdateFireSources()
		{
			this.branchesVulnerableToFire.Clear();
			foreach (BallastFloraBranch branch in this.Branches)
			{
				if (branch.CurrentHull != null)
				{
					foreach (FireSource source in branch.CurrentHull.FireSources)
					{
						if (source.IsInDamageRange(this.GetWorldPosition() + branch.Position, source.DamageRange))
						{
							this.branchesVulnerableToFire.Add(branch);
						}
					}
				}
			}
		}

		// Token: 0x06005131 RID: 20785 RVA: 0x002BB20C File Offset: 0x002B940C
		private bool IsInWater(BallastFloraBranch branch)
		{
			if (branch.CurrentHull == null)
			{
				return false;
			}
			float surfaceY = branch.CurrentHull.Surface;
			Vector2 pos = this.Parent.Position + this.Offset + branch.Position;
			return this.Parent.WaterVolume > 0f && pos.Y < surfaceY;
		}

		// Token: 0x06005132 RID: 20786 RVA: 0x002BB26E File Offset: 0x002B946E
		public void SetHull(BallastFloraBranch branch)
		{
			branch.CurrentHull = Hull.FindHull(this.GetWorldPosition() + branch.Position, this.Parent, true, true);
		}

		// Token: 0x06005133 RID: 20787 RVA: 0x002BB294 File Offset: 0x002B9494
		private void GenerateRoot()
		{
			if (this.root != null)
			{
				DebugConsole.ThrowError("Error in ballast flora: tried to grow a root even though root has already been created.\n" + Environment.StackTrace, null, null, false, false);
			}
			this.root = new BallastFloraBranch(this, null, Vector2.Zero, VineTileType.Stem, new FoliageConfig?(FoliageConfig.EmptyConfig), new FoliageConfig?(FoliageConfig.EmptyConfig), null)
			{
				BlockedSides = (TileSide.Left | TileSide.Bottom | TileSide.Right),
				GrowthStep = 1f,
				MaxHealth = (float)this.RootHealth,
				Health = (float)this.RootHealth,
				IsRoot = true,
				CurrentHull = this.Parent,
				ID = this.CreateID()
			};
			this.Branches.Add(this.root);
			this.CreateBody(this.root);
		}

		// Token: 0x06005134 RID: 20788 RVA: 0x002BB35C File Offset: 0x002B955C
		public float GetGrowthSpeed(float deltaTime)
		{
			float load = this.PowerRequirement * this.Anger * deltaTime;
			if (this.AvailablePower > load)
			{
				this.AvailablePower -= load;
				return this.Anger * 2f * deltaTime;
			}
			return deltaTime;
		}

		// Token: 0x06005135 RID: 20789 RVA: 0x002BB3A0 File Offset: 0x002B95A0
		public bool TryGrowBranch(BallastFloraBranch parent, TileSide side, out List<BallastFloraBranch> result, bool isRootGrowth = false, Vector2? forcePosition = null)
		{
			result = new List<BallastFloraBranch>();
			if (!isRootGrowth && parent.IsSideBlocked(side))
			{
				return false;
			}
			Vector2 pos = forcePosition ?? parent.AdjacentPositions[side];
			Rectangle rect = VineTile.CreatePlantRect(pos);
			if (this.CollidesWithWorld(rect, !isRootGrowth))
			{
				parent.BlockedSides |= side;
				parent.FailedGrowthAttempts++;
				return false;
			}
			FoliageConfig flowerConfig = FoliageConfig.EmptyConfig;
			FoliageConfig leafConfig = FoliageConfig.EmptyConfig;
			if ((double)this.FlowerProbability > Rand.Range(0.0, 1.0, Rand.RandSync.Unsynced))
			{
				flowerConfig = FoliageConfig.CreateRandomConfig(this.flowerVariants, 0.5f, 1f, null);
			}
			if ((double)this.LeafProbability > Rand.Range(0.0, 1.0, Rand.RandSync.Unsynced))
			{
				leafConfig = FoliageConfig.CreateRandomConfig(this.leafVariants, 0.5f, 1f, null);
			}
			BallastFloraBranch newBranch = new BallastFloraBranch(this, parent, pos, VineTileType.CrossJunction, new FoliageConfig?(flowerConfig), new FoliageConfig?(leafConfig), new Rectangle?(rect))
			{
				ID = this.CreateID(),
				MaxHealth = (float)this.BranchHealth,
				Health = (float)this.BranchHealth,
				IsRootGrowth = isRootGrowth
			};
			this.SetHull(newBranch);
			if (newBranch.CurrentHull == null || newBranch.CurrentHull.Submarine != this.Parent.Submarine)
			{
				if (!isRootGrowth)
				{
					parent.BlockedSides |= side;
				}
				parent.FailedGrowthAttempts++;
				return false;
			}
			this.UpdateConnections(newBranch, parent);
			this.Branches.Add(newBranch);
			result.Add(newBranch);
			this.OnBranchGrowthSuccess(newBranch);
			if (this.GrowthWarps > 0)
			{
				this.GrowthWarps--;
			}
			int rootGrowthCount = this.Branches.Count((BallastFloraBranch b) => b.IsRootGrowth);
			if (rootGrowthCount < this.GetDesiredRootGrowthAmount() && this.root != null)
			{
				Vector2 rootGrowthPos = Rand.Vector((float)Math.Max(rootGrowthCount, 1) * Rand.Range(3f, 5f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
				List<BallastFloraBranch> newRootGrowth;
				this.TryGrowBranch(this.root, TileSide.None, out newRootGrowth, true, new Vector2?(rootGrowthPos));
			}
			return true;
		}

		// Token: 0x06005136 RID: 20790 RVA: 0x002BB5E4 File Offset: 0x002B97E4
		private int GetDesiredRootGrowthAmount()
		{
			if (this.root == null)
			{
				return 0;
			}
			return MathHelper.Clamp(this.Branches.Count((BallastFloraBranch b) => !b.IsRootGrowth && b.Health > 0f) / 20, 3, 30);
		}

		// Token: 0x06005137 RID: 20791 RVA: 0x002BB630 File Offset: 0x002B9830
		public bool BranchContainsTarget(BallastFloraBranch branch, Item target)
		{
			Rectangle worldRect = branch.Rect;
			worldRect.Location = this.GetWorldPosition().ToPoint() + worldRect.Location;
			return worldRect.IntersectsWorld(target.WorldRect);
		}

		// Token: 0x06005138 RID: 20792 RVA: 0x002BB674 File Offset: 0x002B9874
		public void ClaimTarget(Item target, [Nullable(2)] BallastFloraBranch branch, bool load = false)
		{
			target.Infector = branch;
			PowerTransfer powerTransfer = target.GetComponent<PowerTransfer>();
			if (powerTransfer != null)
			{
				this.ClaimedJunctionBoxes.Add(powerTransfer);
			}
			PowerContainer powerContainer = target.GetComponent<PowerContainer>();
			if (powerContainer != null)
			{
				this.ClaimedBatteries.Add(powerContainer);
			}
			this.ClaimedTargets.Add(target);
			if (branch != null)
			{
				branch.ClaimedItem = target;
			}
		}

		// Token: 0x06005139 RID: 20793 RVA: 0x002BB6D0 File Offset: 0x002B98D0
		private void UpdateConnections(BallastFloraBranch branch, [Nullable(2)] BallastFloraBranch parent = null)
		{
			foreach (BallastFloraBranch otherBranch in this.Branches)
			{
				float num;
				float num2;
				(branch.Position - otherBranch.Position).Deconstruct(out num, out num2);
				float distX = num;
				float distY = num2;
				int absDistX = (int)Math.Abs(distX);
				int absDistY = (int)Math.Abs(distY);
				if (absDistX <= branch.Rect.Width && absDistY <= branch.Rect.Height && (absDistX <= 0 || absDistY <= 0))
				{
					TileSide connectingSide = (absDistX > absDistY) ? ((distX > 0f) ? TileSide.Right : TileSide.Left) : ((distY > 0f) ? TileSide.Top : TileSide.Bottom);
					TileSide oppositeSide = connectingSide.GetOppositeSide();
					if (parent != null)
					{
						if (otherBranch.BlockedSides.HasFlag(connectingSide))
						{
							branch.BlockedSides |= oppositeSide;
							continue;
						}
						if (otherBranch != parent)
						{
							otherBranch.BlockedSides |= connectingSide;
							branch.BlockedSides |= oppositeSide;
						}
						else
						{
							otherBranch.Sides |= connectingSide;
							branch.Sides |= oppositeSide;
						}
					}
					branch.Connections.TryAdd(oppositeSide, otherBranch);
					otherBranch.Connections.TryAdd(connectingSide, branch);
				}
			}
		}

		// Token: 0x0600513A RID: 20794 RVA: 0x002BB850 File Offset: 0x002B9A50
		private void OnBranchGrowthSuccess(BallastFloraBranch newBranch)
		{
			if (!this.HasBrokenThrough)
			{
				if (this.Branches.Count > this.BreakthroughPoint)
				{
					this.BreakThrough();
				}
				if (newBranch.FlowerConfig.Variant > -1)
				{
					Vector2 flowerPos = this.GetWorldPosition() + newBranch.Position;
					this.CreateShapnel(flowerPos);
					newBranch.GrowthStep = 2f;
					SoundPlayer.PlayDamageSound(this.BurstSound, 1f, flowerPos, 800f, null, 1f);
				}
			}
			this.CreateBody(newBranch);
			foreach (BallastFloraBranch vine in this.Branches)
			{
				vine.UpdateType();
			}
		}

		// Token: 0x0600513B RID: 20795 RVA: 0x002BB918 File Offset: 0x002B9B18
		private void CreateBody(BallastFloraBranch branch)
		{
			Rectangle rect = branch.Rect;
			Vector2 pos = this.Parent.Position + this.Offset + branch.Position;
			float scale = branch.IsRoot ? 3f : 1f;
			Body branchBody = GameMain.World.CreateRectangle(ConvertUnits.ToSimUnits((float)rect.Width * scale), ConvertUnits.ToSimUnits((float)rect.Height * scale), 1.5f, default(Vector2), 0f, BodyType.Static, Category.Cat1, Category.All, true);
			branchBody.BodyType = BodyType.Static;
			branchBody.UserData = branch;
			branchBody.SetCollidesWith(Category.Cat9);
			branchBody.SetCollisionCategories(Category.Cat9);
			branchBody.Position = ConvertUnits.ToSimUnits(pos);
			branchBody.Enabled = this.HasBrokenThrough;
			this.bodies.Add(branchBody);
		}

		// Token: 0x0600513C RID: 20796 RVA: 0x002BB9F0 File Offset: 0x002B9BF0
		public void DamageBranch(BallastFloraBranch branch, float amount, BallastFloraBehavior.AttackType type, [Nullable(2)] Character attacker = null)
		{
			float damage = amount;
			if (type != BallastFloraBehavior.AttackType.Other && type != BallastFloraBehavior.AttackType.CutFromRoot)
			{
				branch.DamageVisualizationTimer = 1f;
			}
			if (branch.IsRootGrowth)
			{
				BallastFloraBranch ballastFloraBranch = this.root;
				if (ballastFloraBranch != null && ballastFloraBranch.Health > 0f)
				{
					return;
				}
			}
			if (type != BallastFloraBehavior.AttackType.Other && type != BallastFloraBehavior.AttackType.CutFromRoot)
			{
				branch.AccumulatedDamage += damage;
				this.Anger += damage * 0.001f;
			}
			if (GameMain.NetworkMember != null)
			{
				if (GameMain.NetworkMember.IsClient)
				{
					return;
				}
				if (type == BallastFloraBehavior.AttackType.Other || type == BallastFloraBehavior.AttackType.CutFromRoot)
				{
					branch.AccumulatedDamage += damage;
				}
			}
			if (attacker != null && this.toxinsCooldown <= 0f)
			{
				this.toxinsTimer = 25f;
				this.toxinsCooldown = 60f;
			}
			if (type == BallastFloraBehavior.AttackType.Fire)
			{
				if (attacker != null)
				{
					damage *= 1f + attacker.GetStatValue(StatTypes.BallastFloraDamageMultiplier, true);
				}
				if (this.IsInWater(branch))
				{
					damage *= 1f - this.SubmergedWaterResistance;
				}
				if (this.defenseCooldown <= 0f)
				{
					if (!(this.StateMachine.State is DefendWithPumpState))
					{
						this.StateMachine.EnterState(new DefendWithPumpState(branch, this.ClaimedTargets, attacker));
						this.defenseCooldown = 180f;
					}
					else
					{
						this.defenseCooldown = 10f;
					}
				}
			}
			if (damage > 0f)
			{
				damage = Math.Min(damage, branch.Health);
			}
			else
			{
				damage = Math.Max(damage, branch.Health - branch.MaxHealth);
			}
			branch.Health -= damage;
			if (branch.Health <= 0f && type != BallastFloraBehavior.AttackType.CutFromRoot)
			{
				this.RemoveBranch(branch);
				if (branch.IsRoot)
				{
					this.Kill();
				}
			}
		}

		// Token: 0x0600513D RID: 20797 RVA: 0x002BBB90 File Offset: 0x002B9D90
		private void CheckDisconnectedFromRoot()
		{
			bool foundDisconnected;
			do
			{
				foundDisconnected = false;
				foreach (BallastFloraBranch branch in this.Branches)
				{
					if (branch.ParentBranch != null && !branch.DisconnectedFromRoot && (branch.ParentBranch.Removed || branch.ParentBranch.DisconnectedFromRoot))
					{
						branch.DisconnectedFromRoot = true;
						foundDisconnected = true;
					}
				}
			}
			while (foundDisconnected);
		}

		// Token: 0x0600513E RID: 20798 RVA: 0x002BBC14 File Offset: 0x002B9E14
		public void RemoveBranch(BallastFloraBranch branch)
		{
			bool isClient = GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient;
			this.Anger += 0.01f;
			bool wasRemoved = branch.Removed;
			this.Branches.Remove(branch);
			branch.Removed = true;
			this.CheckDisconnectedFromRoot();
			this.bodies.ForEachMod(delegate(Body body)
			{
				if (body.UserData == branch)
				{
					GameMain.World.Remove(body);
					this.bodies.Remove(body);
					foreach (KeyValuePair<TileSide, BallastFloraBranch> keyValuePair in branch.Connections)
					{
						TileSide tileSide2;
						BallastFloraBranch ballastFloraBranch;
						keyValuePair.Deconstruct(out tileSide2, out ballastFloraBranch);
						TileSide tileSide = tileSide2;
						BallastFloraBranch otherBranch = ballastFloraBranch;
						TileSide opposite = tileSide.GetOppositeSide();
						otherBranch.BlockedSides &= ~opposite;
						otherBranch.Sides &= ~opposite;
						otherBranch.UpdateType();
						if (!isClient && (otherBranch.Type == VineTileType.Stem || otherBranch.Sides == TileSide.None) && !otherBranch.IsRoot)
						{
							this.RemoveBranch(otherBranch);
						}
					}
				}
			});
			this.CreateDeathParticle(branch, 1f);
			if (isClient)
			{
				return;
			}
			int rootGrowthCount = this.Branches.Count((BallastFloraBranch b) => b.IsRootGrowth);
			if (rootGrowthCount > this.GetDesiredRootGrowthAmount())
			{
				BallastFloraBranch rootGrowth = this.Branches.LastOrDefault((BallastFloraBranch b) => b.IsRootGrowth);
				if (rootGrowth != null)
				{
					this.RemoveBranch(rootGrowth);
				}
			}
			if (branch.ClaimedItem != null)
			{
				this.RemoveClaim(branch.ClaimedItem);
			}
			if (branch.IsRoot)
			{
				this.Kill();
				return;
			}
		}

		// Token: 0x0600513F RID: 20799 RVA: 0x002BBD60 File Offset: 0x002B9F60
		public void RemoveClaim(Item item)
		{
			if (!this.IgnoredTargets.ContainsKey(item))
			{
				this.IgnoredTargets.Add(item, 10);
			}
			this.ClaimedTargets.Remove(item);
			item.Infector = null;
			foreach (BallastFloraBranch branch in this.Branches)
			{
				if (branch.ClaimedItem == item)
				{
					branch.ClaimedItem = null;
				}
			}
			this.ClaimedJunctionBoxes.ForEachMod(delegate(PowerTransfer jb)
			{
				if (jb.Item == item)
				{
					this.ClaimedJunctionBoxes.Remove(jb);
				}
			});
			this.ClaimedBatteries.ForEachMod(delegate(PowerContainer bat)
			{
				if (bat.Item == item)
				{
					this.ClaimedBatteries.Remove(bat);
				}
			});
		}

		// Token: 0x06005140 RID: 20800 RVA: 0x002BBE48 File Offset: 0x002BA048
		public void Kill()
		{
			this.isDead = true;
			foreach (BallastFloraBranch branch in this.Branches)
			{
				branch.DisconnectedFromRoot = true;
			}
			foreach (Item target in this.ClaimedTargets.ToList<Item>())
			{
				this.RemoveClaim(target);
				target.Infector = null;
			}
			BallastFloraStateMachine stateMachine = this.StateMachine;
			if (stateMachine == null)
			{
				return;
			}
			IBallastFloraState state = stateMachine.State;
			if (state == null)
			{
				return;
			}
			state.Exit();
		}

		// Token: 0x06005141 RID: 20801 RVA: 0x002BBF0C File Offset: 0x002BA10C
		public void Remove()
		{
			this.Kill();
			this.Branches.ForEachMod(new Action<BallastFloraBranch>(this.RemoveBranch));
			this.Branches.Clear();
			this.toBeRemoved.Clear();
			this.Parent.BallastFlora = null;
			foreach (Body body in this.bodies)
			{
				GameMain.World.Remove(body);
			}
			BallastFloraBehavior._entityList.Remove(this);
		}

		// Token: 0x06005142 RID: 20802 RVA: 0x002BBFB0 File Offset: 0x002BA1B0
		private void BreakThrough()
		{
			this.HasBrokenThrough = true;
			foreach (Body body in this.bodies)
			{
				body.Enabled = true;
			}
			foreach (BallastFloraBranch branch in this.Branches)
			{
				this.CreateShapnel(this.GetWorldPosition() + branch.Position);
			}
			SoundPlayer.PlayDamageSound(this.BurstSound, (float)this.BreakthroughPoint, this.GetWorldPosition(), 800f, null, 1f);
		}

		// Token: 0x06005143 RID: 20803 RVA: 0x002BC080 File Offset: 0x002BA280
		private bool CanGrowMore()
		{
			return this.Branches.Any((BallastFloraBranch b) => b.CanGrowMore());
		}

		// Token: 0x06005144 RID: 20804 RVA: 0x002BC0AC File Offset: 0x002BA2AC
		private bool CollidesWithWorld(Rectangle rect, bool checkOtherBranches = true)
		{
			if (checkOtherBranches && this.Branches.Any((BallastFloraBranch g) => g.Rect.Contains(rect)))
			{
				return true;
			}
			Rectangle worldRect = rect;
			worldRect.Location = (this.Parent.Position + this.Offset).ToPoint() + worldRect.Location;
			worldRect.Y -= worldRect.Height;
			Vector2 topLeft = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Left, (float)worldRect.Top));
			Vector2 topRight = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Right, (float)worldRect.Top));
			Vector2 bottomLeft = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Left, (float)worldRect.Bottom));
			Vector2 bottomRight = ConvertUnits.ToSimUnits(new Vector2((float)worldRect.Right, (float)worldRect.Bottom));
			return BallastFloraBehavior.LineCollides(topLeft, topRight) || BallastFloraBehavior.LineCollides(topRight, bottomRight) || BallastFloraBehavior.LineCollides(bottomRight, bottomLeft) || BallastFloraBehavior.LineCollides(bottomLeft, topLeft);
		}

		// Token: 0x06005145 RID: 20805 RVA: 0x002BC1C9 File Offset: 0x002BA3C9
		private static bool LineCollides(Vector2 point1, Vector2 point2)
		{
			IEnumerable<Body> ignoredBodies = null;
			Category? collisionCategory = new Category?(Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat8);
			bool ignoreSensors = true;
			Predicate<Fixture> customPredicate;
			if ((customPredicate = BallastFloraBehavior.<>O.<1>__CustomPredicate) == null)
			{
				customPredicate = (BallastFloraBehavior.<>O.<1>__CustomPredicate = new Predicate<Fixture>(BallastFloraBehavior.<LineCollides>g__CustomPredicate|215_0));
			}
			return Submarine.PickBody(point1, point2, ignoredBodies, collisionCategory, ignoreSensors, customPredicate, false) != null;
		}

		// Token: 0x06005147 RID: 20807 RVA: 0x002BC210 File Offset: 0x002BA410
		[CompilerGenerated]
		private void <LoadSave>g__LoadBranch|188_1(XElement branchElement, IdRemap idRemap, ref BallastFloraBehavior.<>c__DisplayClass188_0 A_3)
		{
			BallastFloraBehavior.<>c__DisplayClass188_2 CS$<>8__locals1;
			CS$<>8__locals1.branchElement = branchElement;
			Vector2 pos = CS$<>8__locals1.branchElement.GetAttributeVector2("pos", Vector2.Zero);
			bool isRoot = CS$<>8__locals1.branchElement.GetAttributeBool("isroot", false);
			bool isRootGrowth = CS$<>8__locals1.branchElement.GetAttributeBool("isrootgrowth", false);
			int flowerConfig = BallastFloraBehavior.<LoadSave>g__getInt|188_3("flowerconfig", ref CS$<>8__locals1);
			int leafconfig = BallastFloraBehavior.<LoadSave>g__getInt|188_3("leafconfig", ref CS$<>8__locals1);
			int id = BallastFloraBehavior.<LoadSave>g__getInt|188_3("ID", ref CS$<>8__locals1);
			float health = BallastFloraBehavior.<LoadSave>g__getFloat|188_4("health", ref CS$<>8__locals1);
			float maxhealth = BallastFloraBehavior.<LoadSave>g__getFloat|188_4("maxhealth", ref CS$<>8__locals1);
			int sides = BallastFloraBehavior.<LoadSave>g__getInt|188_3("sides", ref CS$<>8__locals1);
			int blockedSides = BallastFloraBehavior.<LoadSave>g__getInt|188_3("blockedsides", ref CS$<>8__locals1);
			int claimedId = CS$<>8__locals1.branchElement.GetAttributeInt("claimed", -1);
			int parentBranchId = CS$<>8__locals1.branchElement.GetAttributeInt("parentbranch", -1);
			VineTileType type = (VineTileType)CS$<>8__locals1.branchElement.GetAttributeInt("tile", 0);
			BallastFloraBranch newBranch = new BallastFloraBranch(this, null, pos, type, new FoliageConfig?(FoliageConfig.Deserialize(flowerConfig)), new FoliageConfig?(FoliageConfig.Deserialize(leafconfig)), null)
			{
				ID = id,
				Health = health,
				MaxHealth = maxhealth,
				Sides = (TileSide)sides,
				BlockedSides = (TileSide)blockedSides,
				IsRoot = isRoot,
				IsRootGrowth = isRootGrowth
			};
			A_3.branches.Add(new ValueTuple<BallastFloraBranch, int>(newBranch, parentBranchId));
			if (newBranch.IsRoot)
			{
				this.root = newBranch;
			}
			if (claimedId > -1)
			{
				newBranch.ClaimedItemId = (int)idRemap.GetOffsetId((int)((ushort)claimedId));
			}
			this.Branches.Add(newBranch);
		}

		// Token: 0x06005148 RID: 20808 RVA: 0x002BC3A5 File Offset: 0x002BA5A5
		[CompilerGenerated]
		internal static int <LoadSave>g__getInt|188_3(string name, ref BallastFloraBehavior.<>c__DisplayClass188_2 A_1)
		{
			return A_1.branchElement.GetAttributeInt(name, 0);
		}

		// Token: 0x06005149 RID: 20809 RVA: 0x002BC3B4 File Offset: 0x002BA5B4
		[CompilerGenerated]
		internal static float <LoadSave>g__getFloat|188_4(string name, ref BallastFloraBehavior.<>c__DisplayClass188_2 A_1)
		{
			return A_1.branchElement.GetAttributeFloat(name, 0f);
		}

		// Token: 0x0600514A RID: 20810 RVA: 0x002BC3C7 File Offset: 0x002BA5C7
		[CompilerGenerated]
		internal static bool <Update>g__HasAcidEmitter|189_1(BallastFloraBranch b)
		{
			return b.SpawningItem || (b.AttackItem != null && !b.AttackItem.Removed);
		}

		// Token: 0x0600514C RID: 20812 RVA: 0x002BC428 File Offset: 0x002BA628
		[CompilerGenerated]
		internal static bool <LineCollides>g__CustomPredicate|215_0(Fixture f)
		{
			bool hasCollision = f.CollidesWith.HasFlag(Category.Cat5);
			Body body = f.Body;
			if (body.UserData == null)
			{
				return false;
			}
			object userData = body.UserData;
			return (userData is Submarine || userData is Structure) && hasCollision;
		}

		// Token: 0x04002AC7 RID: 10951
		[Nullable(2)]
		public Sprite branchAtlas;

		// Token: 0x04002AC8 RID: 10952
		[Nullable(2)]
		public Sprite decayAtlas;

		// Token: 0x04002AC9 RID: 10953
		public readonly Dictionary<VineTileType, VineSprite> BranchSprites = new Dictionary<VineTileType, VineSprite>();

		// Token: 0x04002ACA RID: 10954
		public readonly List<Sprite> FlowerSprites = new List<Sprite>();

		// Token: 0x04002ACB RID: 10955
		public readonly List<Sprite> DamagedFlowerSprites = new List<Sprite>();

		// Token: 0x04002ACC RID: 10956
		public readonly List<Sprite> HiddenFlowerSprites = new List<Sprite>();

		// Token: 0x04002ACD RID: 10957
		public readonly List<Sprite> LeafSprites = new List<Sprite>();

		// Token: 0x04002ACE RID: 10958
		public readonly List<Sprite> DamagedLeafSprites = new List<Sprite>();

		// Token: 0x04002ACF RID: 10959
		public readonly List<ParticleEmitter> DamageParticles = new List<ParticleEmitter>();

		// Token: 0x04002AD0 RID: 10960
		public readonly List<ParticleEmitter> DeathParticles = new List<ParticleEmitter>();

		// Token: 0x04002AD1 RID: 10961
		public static bool AlwaysShowBallastFloraSprite = false;

		// Token: 0x04002AD2 RID: 10962
		private static readonly List<BallastFloraBehavior> _entityList = new List<BallastFloraBehavior>();

		// Token: 0x04002AEF RID: 10991
		private float availablePower;

		// Token: 0x04002AF0 RID: 10992
		private float anger;

		// Token: 0x04002AF5 RID: 10997
		public Vector2 Offset;

		// Token: 0x04002AF6 RID: 10998
		public readonly HashSet<Item> ClaimedTargets = new HashSet<Item>();

		// Token: 0x04002AF7 RID: 10999
		public readonly HashSet<PowerTransfer> ClaimedJunctionBoxes = new HashSet<PowerTransfer>();

		// Token: 0x04002AF8 RID: 11000
		public readonly HashSet<PowerContainer> ClaimedBatteries = new HashSet<PowerContainer>();

		// Token: 0x04002AF9 RID: 11001
		public readonly Dictionary<Item, int> IgnoredTargets = new Dictionary<Item, int>();

		// Token: 0x04002AFA RID: 11002
		private readonly List<Tuple<ushort, int>> tempClaimedTargets = new List<Tuple<ushort, int>>();

		// Token: 0x04002AFB RID: 11003
		private int flowerVariants;

		// Token: 0x04002AFC RID: 11004
		private int leafVariants;

		// Token: 0x04002AFD RID: 11005
		public readonly List<BallastFloraBehavior.AITarget> Targets = new List<BallastFloraBehavior.AITarget>();

		// Token: 0x04002AFE RID: 11006
		public float PowerConsumptionTimer;

		// Token: 0x04002AFF RID: 11007
		private float defenseCooldown;

		// Token: 0x04002B00 RID: 11008
		private float toxinsCooldown;

		// Token: 0x04002B01 RID: 11009
		private float fireCheckCooldown;

		// Token: 0x04002B02 RID: 11010
		private float selfDamageTimer;

		// Token: 0x04002B03 RID: 11011
		private float toxinsTimer;

		// Token: 0x04002B04 RID: 11012
		private float toxinsSpawnTimer;

		// Token: 0x04002B05 RID: 11013
		private readonly List<BallastFloraBranch> branchesVulnerableToFire = new List<BallastFloraBranch>();

		// Token: 0x04002B06 RID: 11014
		public readonly List<BallastFloraBranch> Branches = new List<BallastFloraBranch>();

		// Token: 0x04002B07 RID: 11015
		[Nullable(2)]
		private BallastFloraBranch root;

		// Token: 0x04002B08 RID: 11016
		private readonly List<Body> bodies = new List<Body>();

		// Token: 0x04002B09 RID: 11017
		private bool isDead;

		// Token: 0x04002B0A RID: 11018
		public readonly BallastFloraStateMachine StateMachine;

		// Token: 0x04002B0B RID: 11019
		public int GrowthWarps;

		// Token: 0x04002B0C RID: 11020
		private readonly List<BallastFloraBranch> toBeRemoved = new List<BallastFloraBranch>();

		// Token: 0x02001267 RID: 4711
		[NullableContext(0)]
		public enum NetworkHeader
		{
			// Token: 0x04005F04 RID: 24324
			Spawn,
			// Token: 0x04005F05 RID: 24325
			Kill,
			// Token: 0x04005F06 RID: 24326
			BranchCreate,
			// Token: 0x04005F07 RID: 24327
			BranchRemove,
			// Token: 0x04005F08 RID: 24328
			BranchDamage,
			// Token: 0x04005F09 RID: 24329
			Infect,
			// Token: 0x04005F0A RID: 24330
			Remove
		}

		// Token: 0x02001268 RID: 4712
		[NullableContext(0)]
		public enum AttackType
		{
			// Token: 0x04005F0C RID: 24332
			Fire,
			// Token: 0x04005F0D RID: 24333
			Explosives,
			// Token: 0x04005F0E RID: 24334
			Other,
			// Token: 0x04005F0F RID: 24335
			CutFromRoot
		}

		// Token: 0x02001269 RID: 4713
		[Nullable(0)]
		public struct AITarget
		{
			// Token: 0x0600942C RID: 37932 RVA: 0x003CE5C7 File Offset: 0x003CC7C7
			public AITarget(ContentXElement element)
			{
				this.Tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
				this.Priority = element.GetAttributeInt("priority", 0);
			}

			// Token: 0x0600942D RID: 37933 RVA: 0x003CE5F4 File Offset: 0x003CC7F4
			public bool Matches(Item item)
			{
				foreach (Identifier targetTag in this.Tags)
				{
					if (item.HasTag(targetTag))
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x04005F10 RID: 24336
			public Identifier[] Tags;

			// Token: 0x04005F11 RID: 24337
			public int Priority;
		}

		// Token: 0x0200126A RID: 4714
		[NullableContext(0)]
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x17001CED RID: 7405
			// (get) Token: 0x0600942E RID: 37934
			BallastFloraBehavior.NetworkHeader NetworkHeader { get; }
		}

		// Token: 0x0200126B RID: 4715
		[NullableContext(0)]
		public readonly struct SpawnEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CEE RID: 7406
			// (get) Token: 0x0600942F RID: 37935 RVA: 0x003CE62A File Offset: 0x003CC82A
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Spawn;
				}
			}
		}

		// Token: 0x0200126C RID: 4716
		[NullableContext(0)]
		private readonly struct KillEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CEF RID: 7407
			// (get) Token: 0x06009430 RID: 37936 RVA: 0x003CE62D File Offset: 0x003CC82D
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Kill;
				}
			}
		}

		// Token: 0x0200126D RID: 4717
		[NullableContext(0)]
		private readonly struct RemoveEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CF0 RID: 7408
			// (get) Token: 0x06009431 RID: 37937 RVA: 0x003CE630 File Offset: 0x003CC830
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Remove;
				}
			}
		}

		// Token: 0x0200126E RID: 4718
		[NullableContext(0)]
		private readonly struct BranchCreateEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CF1 RID: 7409
			// (get) Token: 0x06009432 RID: 37938 RVA: 0x003CE633 File Offset: 0x003CC833
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.BranchCreate;
				}
			}

			// Token: 0x06009433 RID: 37939 RVA: 0x003CE636 File Offset: 0x003CC836
			public BranchCreateEventData(BallastFloraBranch newBranch, BallastFloraBranch parent)
			{
				this.NewBranch = newBranch;
				this.Parent = parent;
			}

			// Token: 0x04005F12 RID: 24338
			public readonly BallastFloraBranch NewBranch;

			// Token: 0x04005F13 RID: 24339
			public readonly BallastFloraBranch Parent;
		}

		// Token: 0x0200126F RID: 4719
		[NullableContext(0)]
		private readonly struct BranchRemoveEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CF2 RID: 7410
			// (get) Token: 0x06009434 RID: 37940 RVA: 0x003CE646 File Offset: 0x003CC846
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.BranchRemove;
				}
			}

			// Token: 0x06009435 RID: 37941 RVA: 0x003CE649 File Offset: 0x003CC849
			public BranchRemoveEventData(BallastFloraBranch branch)
			{
				this.Branch = branch;
			}

			// Token: 0x04005F14 RID: 24340
			public readonly BallastFloraBranch Branch;
		}

		// Token: 0x02001270 RID: 4720
		[NullableContext(0)]
		private readonly struct BranchDamageEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CF3 RID: 7411
			// (get) Token: 0x06009436 RID: 37942 RVA: 0x003CE652 File Offset: 0x003CC852
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.BranchDamage;
				}
			}

			// Token: 0x06009437 RID: 37943 RVA: 0x003CE655 File Offset: 0x003CC855
			public BranchDamageEventData(BallastFloraBranch branch)
			{
				this.Branch = branch;
			}

			// Token: 0x04005F15 RID: 24341
			public readonly BallastFloraBranch Branch;
		}

		// Token: 0x02001271 RID: 4721
		[NullableContext(0)]
		private readonly struct InfectEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001CF4 RID: 7412
			// (get) Token: 0x06009438 RID: 37944 RVA: 0x003CE65E File Offset: 0x003CC85E
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Infect;
				}
			}

			// Token: 0x06009439 RID: 37945 RVA: 0x003CE661 File Offset: 0x003CC861
			public InfectEventData(Item item, BallastFloraBehavior.InfectEventData.InfectState infect, BallastFloraBranch infector)
			{
				this.Item = item;
				this.Infect = infect;
				this.Infector = infector;
			}

			// Token: 0x04005F16 RID: 24342
			public readonly Item Item;

			// Token: 0x04005F17 RID: 24343
			public readonly BallastFloraBehavior.InfectEventData.InfectState Infect;

			// Token: 0x04005F18 RID: 24344
			public readonly BallastFloraBranch Infector;

			// Token: 0x020015C8 RID: 5576
			public enum InfectState
			{
				// Token: 0x040069C9 RID: 27081
				Yes,
				// Token: 0x040069CA RID: 27082
				No
			}
		}

		// Token: 0x02001272 RID: 4722
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005F19 RID: 24345
			[Nullable(0)]
			public static Func<BallastFloraBranch, bool> <0>__HasAcidEmitter;

			// Token: 0x04005F1A RID: 24346
			[Nullable(0)]
			public static Predicate<Fixture> <1>__CustomPredicate;
		}
	}
}

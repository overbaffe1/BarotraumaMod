using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.PerkBehaviors;
using Barotrauma.RuinGeneration;
using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma
{
	// Token: 0x02000041 RID: 65
	internal class Submarine : Entity, IServerPositionSync, IServerSerializable, INetSerializable
	{
		// Token: 0x06000A30 RID: 2608 RVA: 0x000645A5 File Offset: 0x000627A5
		public void ServerWritePosition(ReadWriteMessage tempBuffer, Client c)
		{
			this.subBody.Body.ServerWrite(tempBuffer);
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x000645B8 File Offset: 0x000627B8
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			if (extraData is Submarine.SetLayerEnabledEventData)
			{
				Submarine.SetLayerEnabledEventData setLayerEnabledEventData = (Submarine.SetLayerEnabledEventData)extraData;
				msg.WriteIdentifier(setLayerEnabledEventData.Layer);
				msg.WriteBoolean(setLayerEnabledEventData.Enabled);
				return;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Error while writing a network event for the submarine \"");
			defaultInterpolatedStringHandler.AppendFormatted(this.Info.Name);
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
			defaultInterpolatedStringHandler.AppendLiteral(")\". Unrecognized event data: ");
			defaultInterpolatedStringHandler.AppendFormatted(((extraData != null) ? extraData.GetType().Name : null) ?? "null");
			throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000A32 RID: 2610 RVA: 0x00064667 File Offset: 0x00062867
		// (set) Token: 0x06000A33 RID: 2611 RVA: 0x0006466F File Offset: 0x0006286F
		public SubmarineInfo Info { get; private set; }

		// Token: 0x06000A34 RID: 2612 RVA: 0x00064678 File Offset: 0x00062878
		public static ImmutableArray<SubItemSwapPerk> GetSubItemSwapPerksFromTeamPerks(ImmutableArray<DisembarkPerkPrefab> teamPerks)
		{
			ImmutableArray<SubItemSwapPerk>.Builder builder = ImmutableArray.CreateBuilder<SubItemSwapPerk>();
			foreach (DisembarkPerkPrefab prefab in teamPerks)
			{
				foreach (PerkBase perk in prefab.PerkBehaviors)
				{
					SubItemSwapPerk subSwapPerk = perk as SubItemSwapPerk;
					if (subSwapPerk != null)
					{
						builder.Add(subSwapPerk);
					}
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x170002BE RID: 702
		// (get) Token: 0x06000A35 RID: 2613 RVA: 0x000646E5 File Offset: 0x000628E5
		// (set) Token: 0x06000A36 RID: 2614 RVA: 0x000646ED File Offset: 0x000628ED
		public Vector2 HiddenSubPosition { get; private set; }

		// Token: 0x170002BF RID: 703
		// (get) Token: 0x06000A37 RID: 2615 RVA: 0x000646F6 File Offset: 0x000628F6
		// (set) Token: 0x06000A38 RID: 2616 RVA: 0x000646FE File Offset: 0x000628FE
		public ushort IdOffset { get; private set; }

		// Token: 0x170002C0 RID: 704
		// (get) Token: 0x06000A39 RID: 2617 RVA: 0x00064707 File Offset: 0x00062907
		// (set) Token: 0x06000A3A RID: 2618 RVA: 0x00064710 File Offset: 0x00062910
		public static Submarine MainSub
		{
			get
			{
				return Submarine.MainSubs[0];
			}
			set
			{
				Submarine.MainSubs[0] = value;
			}
		}

		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000A3B RID: 2619 RVA: 0x0006471A File Offset: 0x0006291A
		public static IEnumerable<MapEntity> VisibleEntities
		{
			get
			{
				return Submarine.visibleEntities;
			}
		}

		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000A3C RID: 2620 RVA: 0x00064724 File Offset: 0x00062924
		public IEnumerable<Submarine> DockedTo
		{
			get
			{
				Submarine.<get_DockedTo>d__33 <get_DockedTo>d__ = new Submarine.<get_DockedTo>d__33(-2);
				<get_DockedTo>d__.<>4__this = this;
				return <get_DockedTo>d__;
			}
		}

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000A3D RID: 2621 RVA: 0x00064741 File Offset: 0x00062941
		public static Vector2 LastPickedPosition
		{
			get
			{
				return Submarine.lastPickedPosition;
			}
		}

		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000A3E RID: 2622 RVA: 0x00064748 File Offset: 0x00062948
		public static float LastPickedFraction
		{
			get
			{
				return Submarine.lastPickedFraction;
			}
		}

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000A3F RID: 2623 RVA: 0x0006474F File Offset: 0x0006294F
		public static Fixture LastPickedFixture
		{
			get
			{
				return Submarine.lastPickedFixture;
			}
		}

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000A40 RID: 2624 RVA: 0x00064756 File Offset: 0x00062956
		public static Vector2 LastPickedNormal
		{
			get
			{
				return Submarine.lastPickedNormal;
			}
		}

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000A41 RID: 2625 RVA: 0x0006475D File Offset: 0x0006295D
		// (set) Token: 0x06000A42 RID: 2626 RVA: 0x00064765 File Offset: 0x00062965
		public bool Loading { get; private set; }

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x0006476E File Offset: 0x0006296E
		// (set) Token: 0x06000A44 RID: 2628 RVA: 0x00064776 File Offset: 0x00062976
		public bool GodMode { get; set; }

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000A45 RID: 2629 RVA: 0x0006477F File Offset: 0x0006297F
		public static List<Submarine> Loaded
		{
			get
			{
				return Submarine.loaded;
			}
		}

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000A46 RID: 2630 RVA: 0x00064786 File Offset: 0x00062986
		public SubmarineBody SubBody
		{
			get
			{
				return this.subBody;
			}
		}

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x0006478E File Offset: 0x0006298E
		public PhysicsBody PhysicsBody
		{
			get
			{
				SubmarineBody submarineBody = this.subBody;
				if (submarineBody == null)
				{
					return null;
				}
				return submarineBody.Body;
			}
		}

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000A48 RID: 2632 RVA: 0x000647A1 File Offset: 0x000629A1
		public Rectangle Borders
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Borders;
				}
				return Rectangle.Empty;
			}
		}

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000A49 RID: 2633 RVA: 0x000647BC File Offset: 0x000629BC
		public Rectangle VisibleBorders
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.VisibleBorders;
				}
				return Rectangle.Empty;
			}
		}

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000A4A RID: 2634 RVA: 0x000647D7 File Offset: 0x000629D7
		public override Vector2 Position
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Position - this.HiddenSubPosition;
				}
				return Vector2.Zero;
			}
		}

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000A4B RID: 2635 RVA: 0x000647FD File Offset: 0x000629FD
		public override Vector2 WorldPosition
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Position;
				}
				return Vector2.Zero;
			}
		}

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000A4C RID: 2636 RVA: 0x00064818 File Offset: 0x00062A18
		public float RealWorldCrushDepth
		{
			get
			{
				if (this.realWorldCrushDepth == null)
				{
					this.realWorldCrushDepth = new float?(float.PositiveInfinity);
					foreach (Structure structure in Structure.WallList)
					{
						if (structure.Submarine == this && structure.HasBody && !structure.Indestructible)
						{
							this.realWorldCrushDepth = new float?(Math.Min(structure.CrushDepth, this.realWorldCrushDepth.Value));
						}
					}
				}
				return this.realWorldCrushDepth.Value;
			}
		}

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000A4D RID: 2637 RVA: 0x000648C8 File Offset: 0x00062AC8
		public float RealWorldDepth
		{
			get
			{
				Level level = Level.Loaded;
				if (((level != null) ? level.GenerationParams : null) == null)
				{
					return -this.WorldPosition.Y * Physics.DisplayToRealWorldRatio;
				}
				return Level.Loaded.GetRealWorldDepth(this.WorldPosition.Y);
			}
		}

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000A4E RID: 2638 RVA: 0x00064905 File Offset: 0x00062B05
		public bool IsAboveLevel
		{
			get
			{
				return Level.IsPositionAboveLevel(this.WorldPosition);
			}
		}

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000A4F RID: 2639 RVA: 0x00064914 File Offset: 0x00062B14
		public bool AtEndExit
		{
			get
			{
				if (Level.Loaded == null)
				{
					return false;
				}
				if (Level.Loaded.EndOutpost != null)
				{
					if (this.DockedTo.Contains(Level.Loaded.EndOutpost))
					{
						return true;
					}
					if (Level.Loaded.EndOutpost.exitPoints.Any<WayPoint>())
					{
						return this.IsAtOutpostExit(Level.Loaded.EndOutpost);
					}
				}
				else if (Level.Loaded.Type == LevelData.LevelType.Outpost && Level.Loaded.StartOutpost != null)
				{
					return this.IsAtOutpostExit(Level.Loaded.StartOutpost);
				}
				return Vector2.DistanceSquared(this.Position + this.HiddenSubPosition, Level.Loaded.EndExitPosition) < 36000000f;
			}
		}

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000A50 RID: 2640 RVA: 0x000649C8 File Offset: 0x00062BC8
		public bool AtStartExit
		{
			get
			{
				if (Level.Loaded == null)
				{
					return false;
				}
				if (Level.Loaded.StartOutpost != null)
				{
					if (this.DockedTo.Contains(Level.Loaded.StartOutpost))
					{
						return true;
					}
					if (Level.Loaded.StartOutpost.exitPoints.Any<WayPoint>())
					{
						return this.IsAtOutpostExit(Level.Loaded.StartOutpost);
					}
				}
				return Vector2.DistanceSquared(this.Position + this.HiddenSubPosition, Level.Loaded.StartExitPosition) < 36000000f;
			}
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000A51 RID: 2641 RVA: 0x00064A51 File Offset: 0x00062C51
		public bool AtEitherExit
		{
			get
			{
				return this.AtStartExit || this.AtEndExit;
			}
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x00064A64 File Offset: 0x00062C64
		private bool IsAtOutpostExit(Submarine outpost)
		{
			if (outpost.exitPoints.Any<WayPoint>())
			{
				Rectangle worldBorders = this.GetDockedBorders(true);
				worldBorders.Location += this.WorldPosition.ToPoint();
				foreach (WayPoint exitPoint in outpost.exitPoints)
				{
					if (exitPoint.ExitPointSize != Point.Zero)
					{
						if (Submarine.RectsOverlap(worldBorders, exitPoint.ExitPointWorldRect, true))
						{
							return true;
						}
					}
					else if (Submarine.RectContains(worldBorders, exitPoint.WorldPosition, false))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000A53 RID: 2643 RVA: 0x00064B28 File Offset: 0x00062D28
		// (set) Token: 0x06000A54 RID: 2644 RVA: 0x00064B30 File Offset: 0x00062D30
		public new Vector2 DrawPosition { get; private set; }

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000A55 RID: 2645 RVA: 0x00064B39 File Offset: 0x00062D39
		public override Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000A56 RID: 2646 RVA: 0x00064B46 File Offset: 0x00062D46
		// (set) Token: 0x06000A57 RID: 2647 RVA: 0x00064B61 File Offset: 0x00062D61
		public Vector2 Velocity
		{
			get
			{
				if (this.subBody != null)
				{
					return this.subBody.Velocity;
				}
				return Vector2.Zero;
			}
			set
			{
				if (this.subBody == null)
				{
					return;
				}
				this.subBody.Velocity = value;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000A58 RID: 2648 RVA: 0x00064B78 File Offset: 0x00062D78
		public List<Vector2> HullVertices
		{
			get
			{
				SubmarineBody submarineBody = this.subBody;
				if (submarineBody == null)
				{
					return null;
				}
				return submarineBody.HullVertices;
			}
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06000A59 RID: 2649 RVA: 0x00064B8C File Offset: 0x00062D8C
		public int SubmarineSpecificIDTag
		{
			get
			{
				int value = this.submarineSpecificIDTag.GetValueOrDefault();
				if (this.submarineSpecificIDTag == null)
				{
					Level level = Level.Loaded;
					value = ToolBox.StringToInt(((level != null) ? level.Seed : null) + this.Info.Name);
					this.submarineSpecificIDTag = new int?(value);
				}
				return this.submarineSpecificIDTag.Value;
			}
		}

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000A5A RID: 2650 RVA: 0x00064BF0 File Offset: 0x00062DF0
		public bool AtDamageDepth
		{
			get
			{
				return Level.Loaded != null && this.subBody != null && this.RealWorldDepth > Level.Loaded.RealWorldCrushDepth && this.RealWorldDepth > this.RealWorldCrushDepth;
			}
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000A5B RID: 2651 RVA: 0x00064C28 File Offset: 0x00062E28
		public bool AtCosmeticDamageDepth
		{
			get
			{
				return Level.Loaded != null && this.subBody != null && this.RealWorldDepth > Level.Loaded.RealWorldCrushDepth + -500f && this.RealWorldDepth > this.RealWorldCrushDepth + -500f;
			}
		}

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000A5C RID: 2652 RVA: 0x00064C74 File Offset: 0x00062E74
		public bool IsRespawnShuttle
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				RespawnManager respawnManager = (networkMember != null) ? networkMember.RespawnManager : null;
				return respawnManager != null && respawnManager.RespawnShuttles.Contains(this);
			}
		}

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000A5D RID: 2653 RVA: 0x00064CA4 File Offset: 0x00062EA4
		public IReadOnlyList<WayPoint> ExitPoints
		{
			get
			{
				return this.exitPoints;
			}
		}

		// Token: 0x06000A5E RID: 2654 RVA: 0x00064CAC File Offset: 0x00062EAC
		public override string ToString()
		{
			string[] array = new string[5];
			array[0] = "Barotrauma.Submarine (";
			int num = 1;
			SubmarineInfo info = this.Info;
			array[num] = (((info != null) ? info.Name : null) ?? "[NULL INFO]");
			array[2] = ", ";
			array[3] = this.IdOffset.ToString();
			array[4] = ")";
			return string.Concat(array);
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x00064D0C File Offset: 0x00062F0C
		public int CalculateBasePrice()
		{
			int minPrice = 1000;
			float volume = (from h in Hull.HullList
			where h.Submarine == this
			select h).Sum((Hull h) => h.Volume);
			float itemValue = (float)(from it in Item.ItemList
			where it.Submarine == this
			select it).Sum((Item it) => it.Prefab.GetMinPrice().GetValueOrDefault());
			float price = volume / 500f + itemValue / 100f;
			return Math.Max(minPrice, (int)price);
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000A60 RID: 2656 RVA: 0x00064DAE File Offset: 0x00062FAE
		// (set) Token: 0x06000A61 RID: 2657 RVA: 0x00064DB6 File Offset: 0x00062FB6
		public bool ImmuneToBallastFlora { get; set; }

		// Token: 0x06000A62 RID: 2658 RVA: 0x00064DC0 File Offset: 0x00062FC0
		public void AttemptBallastFloraInfection(Identifier identifier, float deltaTime, float probability)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (this.ImmuneToBallastFlora)
			{
				return;
			}
			if (this.ballastFloraTimer < 1f)
			{
				this.ballastFloraTimer += deltaTime;
				return;
			}
			this.ballastFloraTimer = 0f;
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) >= probability)
			{
				return;
			}
			List<Pump> pumps = new List<Pump>();
			List<Item> allItems = this.GetItems(true);
			bool anyHasTag = allItems.Any((Item i) => i.HasTag(Tags.Ballast));
			foreach (Item item in allItems)
			{
				if (!anyHasTag || item.HasTag(Tags.Ballast))
				{
					Pump pump = item.GetComponent<Pump>();
					if (pump != null)
					{
						pumps.Add(pump);
					}
				}
			}
			if (!pumps.Any<Pump>())
			{
				return;
			}
			Pump randomPump = pumps.GetRandom(Rand.RandSync.Unsynced);
			if (randomPump.IsOn && randomPump.HasPower && randomPump.FlowPercentage > 0f && randomPump.Item.Condition > 0f)
			{
				randomPump.InfectBallast(identifier, false);
				randomPump.Item.CreateServerEvent<Pump>(randomPump);
			}
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x00064F10 File Offset: 0x00063110
		public void MakeWreck()
		{
			this.Info.Type = SubmarineType.Wreck;
			this.ShowSonarMarker = false;
			this.DockedTo.ForEach(delegate(Submarine s)
			{
				s.ShowSonarMarker = false;
			});
			this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
			this.TeamID = CharacterTeamType.None;
		}

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06000A64 RID: 2660 RVA: 0x00064F72 File Offset: 0x00063172
		// (set) Token: 0x06000A65 RID: 2661 RVA: 0x00064F7A File Offset: 0x0006317A
		public WreckAI WreckAI { get; private set; }

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06000A66 RID: 2662 RVA: 0x00064F83 File Offset: 0x00063183
		// (set) Token: 0x06000A67 RID: 2663 RVA: 0x00064F8B File Offset: 0x0006318B
		public SubmarineTurretAI TurretAI { get; private set; }

		// Token: 0x06000A68 RID: 2664 RVA: 0x00064F94 File Offset: 0x00063194
		public bool CreateWreckAI()
		{
			this.WreckAI = WreckAI.Create(this);
			return this.WreckAI != null;
		}

		// Token: 0x06000A69 RID: 2665 RVA: 0x00064FAC File Offset: 0x000631AC
		public bool CreateTurretAI()
		{
			this.TurretAI = new SubmarineTurretAI(this, default(Identifier));
			return this.TurretAI != null;
		}

		// Token: 0x06000A6A RID: 2666 RVA: 0x00064FD7 File Offset: 0x000631D7
		public void DisableWreckAI()
		{
			if (this.WreckAI == null)
			{
				WreckAI.RemoveThalamusItems(this);
				return;
			}
			WreckAI wreckAI = this.WreckAI;
			if (wreckAI != null)
			{
				wreckAI.Remove();
			}
			this.WreckAI = null;
		}

		// Token: 0x06000A6B RID: 2667 RVA: 0x00065000 File Offset: 0x00063200
		public Rectangle GetDockedBorders(bool allowDifferentTeam = true)
		{
			Submarine.checkSubmarineBorders.Clear();
			return this.GetDockedBordersRecursive(allowDifferentTeam);
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x00065014 File Offset: 0x00063214
		private Rectangle GetDockedBordersRecursive(bool allowDifferentTeam)
		{
			Rectangle dockedBorders = this.Borders;
			Submarine.checkSubmarineBorders.Add(this);
			IEnumerable<Submarine> connectedSubs = from s in this.DockedTo
			where !Submarine.checkSubmarineBorders.Contains(s) && !s.Info.IsOutpost && (allowDifferentTeam || s.TeamID == this.TeamID)
			select s;
			foreach (Submarine dockedSub in connectedSubs)
			{
				Vector2? expectedLocation = Submarine.CalculateDockOffset(this, dockedSub);
				if (expectedLocation != null)
				{
					Rectangle dockedSubBorders = dockedSub.GetDockedBordersRecursive(allowDifferentTeam);
					dockedSubBorders.Location += MathUtils.ToPoint(expectedLocation.Value);
					dockedBorders.Y = -dockedBorders.Y;
					dockedSubBorders.Y = -dockedSubBorders.Y;
					dockedBorders = Rectangle.Union(dockedBorders, dockedSubBorders);
					dockedBorders.Y = -dockedBorders.Y;
				}
			}
			return dockedBorders;
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00065110 File Offset: 0x00063310
		public IEnumerable<Submarine> GetConnectedSubs()
		{
			return this.connectedSubs;
		}

		// Token: 0x06000A6E RID: 2670 RVA: 0x00065118 File Offset: 0x00063318
		public void RefreshConnectedSubs()
		{
			this.connectedSubs.Clear();
			this.connectedSubs.Add(this);
			this.GetConnectedSubsRecursive(this.connectedSubs);
		}

		// Token: 0x06000A6F RID: 2671 RVA: 0x00065140 File Offset: 0x00063340
		private void GetConnectedSubsRecursive(HashSet<Submarine> subs)
		{
			foreach (Submarine dockedSub in this.DockedTo)
			{
				if (!subs.Contains(dockedSub))
				{
					subs.Add(dockedSub);
					dockedSub.GetConnectedSubsRecursive(subs);
				}
			}
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x000651A0 File Offset: 0x000633A0
		public Vector2 FindSpawnPos(Vector2 spawnPos, Point? submarineSize = null, float subDockingPortOffset = 0f, int verticalMoveDir = 0)
		{
			Submarine.<>c__DisplayClass137_0 CS$<>8__locals1;
			CS$<>8__locals1.subDockingPortOffset = subDockingPortOffset;
			Rectangle dockedBorders = this.GetDockedBorders(true);
			Vector2 diffFromDockedBorders = new Vector2((float)dockedBorders.Center.X, (float)(dockedBorders.Y - dockedBorders.Height / 2)) - new Vector2((float)this.Borders.Center.X, (float)(this.Borders.Y - this.Borders.Height / 2));
			CS$<>8__locals1.minWidth = Math.Max((submarineSize != null) ? submarineSize.Value.X : dockedBorders.Width, 500);
			int minHeight = Math.Max((submarineSize != null) ? submarineSize.Value.Y : dockedBorders.Height, 1000);
			int padding = 100;
			CS$<>8__locals1.minWidth += padding;
			minHeight += padding;
			int iterations = 0;
			do
			{
				Vector2 potentialPos = spawnPos;
				if (verticalMoveDir != 0)
				{
					verticalMoveDir = Math.Sign(verticalMoveDir);
					Vector2 rayEnd = new Vector2(potentialPos.X, (float)((verticalMoveDir > 0) ? Level.Loaded.Size.Y : 0));
					Vector2 closestPickedPos = rayEnd;
					for (float x = -1f; x <= 1f; x += 0.2f)
					{
						Vector2 xOffset = Vector2.UnitX * (float)CS$<>8__locals1.minWidth / 2f * x;
						xOffset.X += CS$<>8__locals1.subDockingPortOffset;
						if (Submarine.PickBody(ConvertUnits.ToSimUnits(potentialPos + xOffset), ConvertUnits.ToSimUnits(rayEnd + xOffset), null, new Category?(Category.Cat1 | Category.Cat8), true, delegate(Fixture f)
						{
							VoronoiCell voronoiCell = f.UserData as VoronoiCell;
							return voronoiCell == null || !voronoiCell.IsDestructible;
						}, false) != null)
						{
							int offsetFromWall = 10 * -verticalMoveDir;
							float pickedPos = ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition.Y) + (float)offsetFromWall;
							closestPickedPos.Y = ((verticalMoveDir > 0) ? Math.Min(closestPickedPos.Y, pickedPos) : Math.Max(closestPickedPos.Y, pickedPos));
						}
					}
					potentialPos.Y = closestPickedPos.Y;
				}
				Vector2 limits = Submarine.<FindSpawnPos>g__GetHorizontalLimits|137_0(new Vector2(potentialPos.X, potentialPos.Y - (float)dockedBorders.Height * 0.5f * (float)verticalMoveDir), (float)CS$<>8__locals1.minWidth, (float)minHeight, verticalMoveDir, padding, ref CS$<>8__locals1);
				if (limits.Y - limits.X >= (float)CS$<>8__locals1.minWidth)
				{
					Vector2 newSpawnPos = new Vector2(spawnPos.X, potentialPos.Y - (float)dockedBorders.Height * 0.5f * (float)verticalMoveDir);
					bool couldMoveInVerticalMoveDir = Math.Sign(newSpawnPos.Y - spawnPos.Y) == Math.Sign(verticalMoveDir);
					if (!couldMoveInVerticalMoveDir)
					{
						break;
					}
					spawnPos = Submarine.<FindSpawnPos>g__ClampToHorizontalLimits|137_1(newSpawnPos, limits, ref CS$<>8__locals1);
				}
				iterations++;
			}
			while (iterations < 5);
			spawnPos.Y = MathHelper.Clamp(spawnPos.Y, (float)(dockedBorders.Height / 2 + 10), (float)(Level.Loaded.Size.Y - dockedBorders.Height / 2 - padding * 2));
			return spawnPos - diffFromDockedBorders;
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x000654BC File Offset: 0x000636BC
		public void UpdateTransform(bool interpolate = true)
		{
			this.DrawPosition = (interpolate ? Timing.Interpolate(this.prevPosition, this.Position) : this.Position);
			if (!interpolate)
			{
				this.prevPosition = this.Position;
			}
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x000654F0 File Offset: 0x000636F0
		public static Vector2 VectorToWorldGrid(Vector2 position, Submarine sub = null, bool round = false)
		{
			if (round)
			{
				position.X = MathF.Round(position.X / Submarine.GridSize.X) * Submarine.GridSize.X;
				position.Y = MathF.Round(position.Y / Submarine.GridSize.Y) * Submarine.GridSize.Y;
			}
			else
			{
				position.X = MathF.Floor(position.X / Submarine.GridSize.X) * Submarine.GridSize.X;
				position.Y = MathF.Ceiling(position.Y / Submarine.GridSize.Y) * Submarine.GridSize.Y;
			}
			if (sub != null)
			{
				position.X += sub.Position.X % Submarine.GridSize.X;
				position.Y += sub.Position.Y % Submarine.GridSize.Y;
			}
			return position;
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x000655E8 File Offset: 0x000637E8
		public Rectangle CalculateDimensions(bool onlyHulls = true)
		{
			List<MapEntity> entities = onlyHulls ? Hull.HullList.FindAll((Hull h) => h.Submarine == this).Cast<MapEntity>().ToList<MapEntity>() : MapEntity.MapEntityList.FindAll((MapEntity me) => me.Submarine == this);
			entities.RemoveAll(delegate(MapEntity e)
			{
				Item item2 = e as Item;
				if (item2 != null)
				{
					if (item2.GetComponent<Turret>() != null)
					{
						return false;
					}
					if (item2.body != null && !item2.body.Enabled)
					{
						return true;
					}
				}
				return e.IsHidden;
			});
			if (entities.Count == 0)
			{
				return Rectangle.Empty;
			}
			float minX = (float)entities[0].Rect.X;
			float minY = (float)(entities[0].Rect.Y - entities[0].Rect.Height);
			float maxX = (float)entities[0].Rect.Right;
			float maxY = (float)entities[0].Rect.Y;
			for (int i = 1; i < entities.Count; i++)
			{
				Item item = entities[i] as Item;
				if (item != null)
				{
					Turret turret = item.GetComponent<Turret>();
					if (turret != null)
					{
						minX = Math.Min(minX, (float)entities[i].Rect.X + turret.TransformedBarrelPos.X * 2f);
						minY = Math.Min(minY, (float)(entities[i].Rect.Y - entities[i].Rect.Height) - turret.TransformedBarrelPos.Y * 2f);
						maxX = Math.Max(maxX, (float)entities[i].Rect.Right + turret.TransformedBarrelPos.X * 2f);
						maxY = Math.Max(maxY, (float)entities[i].Rect.Y - turret.TransformedBarrelPos.Y * 2f);
					}
				}
				minX = Math.Min(minX, (float)entities[i].Rect.X);
				minY = Math.Min(minY, (float)(entities[i].Rect.Y - entities[i].Rect.Height));
				maxX = Math.Max(maxX, (float)entities[i].Rect.Right);
				maxY = Math.Max(maxY, (float)entities[i].Rect.Y);
			}
			return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x0006586C File Offset: 0x00063A6C
		public static Rectangle AbsRect(Vector2 pos, Vector2 size)
		{
			if (size.X < 0f)
			{
				pos.X += size.X;
				size.X = -size.X;
			}
			if (size.Y < 0f)
			{
				pos.Y -= size.Y;
				size.Y = -size.Y;
			}
			return new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x000658F4 File Offset: 0x00063AF4
		public static RectangleF AbsRectF(Vector2 pos, Vector2 size)
		{
			if (size.X < 0f)
			{
				pos.X += size.X;
				size.X = -size.X;
			}
			if (size.Y < 0f)
			{
				pos.Y += size.Y;
				size.Y = -size.Y;
			}
			return new RectangleF(pos.X, pos.Y, size.X, size.Y);
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x00065978 File Offset: 0x00063B78
		public static bool RectContains(Rectangle rect, Vector2 pos, bool inclusive = false)
		{
			if (inclusive)
			{
				return pos.X >= (float)rect.X && pos.X <= (float)(rect.X + rect.Width) && pos.Y <= (float)rect.Y && pos.Y >= (float)(rect.Y - rect.Height);
			}
			return pos.X > (float)rect.X && pos.X < (float)(rect.X + rect.Width) && pos.Y < (float)rect.Y && pos.Y > (float)(rect.Y - rect.Height);
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x00065A24 File Offset: 0x00063C24
		public static bool RectsOverlap(Rectangle rect1, Rectangle rect2, bool inclusive = true)
		{
			if (inclusive)
			{
				return rect1.X <= rect2.X + rect2.Width && rect1.X + rect1.Width >= rect2.X && rect1.Y >= rect2.Y - rect2.Height && rect1.Y - rect1.Height <= rect2.Y;
			}
			return rect1.X < rect2.X + rect2.Width && rect1.X + rect1.Width > rect2.X && rect1.Y > rect2.Y - rect2.Height && rect1.Y - rect1.Height < rect2.Y;
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00065AE4 File Offset: 0x00063CE4
		public static bool RectsOverlap(RectangleF rect1, RectangleF rect2, bool inclusive = true)
		{
			if (inclusive)
			{
				return rect1.X <= rect2.X + rect2.Width && rect1.X + rect1.Width >= rect2.X && rect1.Y >= rect2.Y - rect2.Height && rect1.Y - rect1.Height <= rect2.Y;
			}
			return rect1.X < rect2.X + rect2.Width && rect1.X + rect1.Width > rect2.X && rect1.Y > rect2.Y - rect2.Height && rect1.Y - rect1.Height < rect2.Y;
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x00065BA4 File Offset: 0x00063DA4
		public static Body PickBody(Vector2 rayStart, Vector2 rayEnd, IEnumerable<Body> ignoredBodies = null, Category? collisionCategory = null, bool ignoreSensors = true, Predicate<Fixture> customPredicate = null, bool allowInsideFixture = false)
		{
			if (Vector2.DistanceSquared(rayStart, rayEnd) < 0.0001f)
			{
				return null;
			}
			float closestFraction = 1f;
			Vector2 closestNormal = Vector2.Zero;
			Fixture closestFixture = null;
			Body closestBody = null;
			if (allowInsideFixture)
			{
				AABB aabb = new AABB(rayStart - Vector2.One * 0.001f, rayStart + Vector2.One * 0.001f);
				GameMain.World.QueryAABB(delegate(Fixture fixture)
				{
					if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
					{
						return true;
					}
					Transform transform;
					fixture.Body.GetTransform(out transform);
					if (!fixture.Shape.TestPoint(ref transform, ref rayStart))
					{
						return true;
					}
					closestFraction = 0f;
					closestNormal = Vector2.Normalize(rayEnd - rayStart);
					closestFixture = fixture;
					if (fixture.Body != null)
					{
						closestBody = fixture.Body;
					}
					return false;
				}, ref aabb);
				if (closestFraction <= 0f)
				{
					Submarine.lastPickedPosition = rayStart;
					Submarine.lastPickedFraction = closestFraction;
					Submarine.lastPickedFixture = closestFixture;
					Submarine.lastPickedNormal = closestNormal;
					return closestBody;
				}
			}
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
				{
					return -1f;
				}
				if (fraction < closestFraction)
				{
					closestFraction = fraction;
					closestNormal = normal;
					closestFixture = fixture;
					if (fixture.Body != null)
					{
						closestBody = fixture.Body;
					}
				}
				return fraction;
			}, rayStart, rayEnd, collisionCategory.GetValueOrDefault(Category.All));
			Submarine.lastPickedPosition = rayStart + (rayEnd - rayStart) * closestFraction;
			Submarine.lastPickedFraction = closestFraction;
			Submarine.lastPickedFixture = closestFixture;
			Submarine.lastPickedNormal = closestNormal;
			return closestBody;
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00065D40 File Offset: 0x00063F40
		public static float LastPickedBodyDist(Body body)
		{
			if (!Submarine.bodyDist.ContainsKey(body))
			{
				return 0f;
			}
			return Submarine.bodyDist[body];
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x00065D60 File Offset: 0x00063F60
		public static IEnumerable<Body> PickBodies(Vector2 rayStart, Vector2 rayEnd, IEnumerable<Body> ignoredBodies = null, Category? collisionCategory = null, bool ignoreSensors = true, Predicate<Fixture> customPredicate = null, bool allowInsideFixture = false)
		{
			if (Vector2.DistanceSquared(rayStart, rayEnd) < 1E-05f)
			{
				rayEnd += Vector2.UnitX * 0.001f;
			}
			float closestFraction = 1f;
			Submarine.bodies.Clear();
			Submarine.bodyDist.Clear();
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
				{
					return -1f;
				}
				if (fixture.Body != null)
				{
					Submarine.bodies.Add(fixture.Body);
					Submarine.bodyDist[fixture.Body] = fraction;
				}
				if (fraction < closestFraction)
				{
					Submarine.lastPickedPosition = rayStart + (rayEnd - rayStart) * fraction;
					Submarine.lastPickedFraction = fraction;
					Submarine.lastPickedNormal = normal;
					Submarine.lastPickedFixture = fixture;
				}
				return -1f;
			}, rayStart, rayEnd, collisionCategory.GetValueOrDefault(Category.All));
			if (allowInsideFixture)
			{
				AABB aabb = new AABB(rayStart - Vector2.One * 0.001f, rayStart + Vector2.One * 0.001f);
				GameMain.World.QueryAABB(delegate(Fixture fixture)
				{
					if (Submarine.bodies.Contains(fixture.Body) || fixture.Body == null)
					{
						return true;
					}
					if (!Submarine.CheckFixtureCollision(fixture, ignoredBodies, collisionCategory, ignoreSensors, customPredicate))
					{
						return true;
					}
					Transform transform;
					fixture.Body.GetTransform(out transform);
					if (!fixture.Shape.TestPoint(ref transform, ref rayStart))
					{
						return true;
					}
					closestFraction = 0f;
					Submarine.lastPickedPosition = rayStart;
					Submarine.lastPickedFraction = 0f;
					Submarine.lastPickedNormal = Vector2.Normalize(rayEnd - rayStart);
					Submarine.lastPickedFixture = fixture;
					Submarine.bodies.Add(fixture.Body);
					Submarine.bodyDist[fixture.Body] = 0f;
					return false;
				}, ref aabb);
			}
			Submarine.bodies.Sort((Body b1, Body b2) => Submarine.bodyDist[b1].CompareTo(Submarine.bodyDist[b2]));
			return Submarine.bodies;
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00065EB0 File Offset: 0x000640B0
		private static bool CheckFixtureCollision(Fixture fixture, IEnumerable<Body> ignoredBodies = null, Category? collisionCategory = null, bool ignoreSensors = true, Predicate<Fixture> customPredicate = null)
		{
			if (fixture == null || (ignoreSensors && fixture.IsSensor) || fixture.CollisionCategories == Category.None || fixture.CollisionCategories == Category.Cat5)
			{
				return false;
			}
			if (customPredicate != null && !customPredicate(fixture))
			{
				return false;
			}
			if (collisionCategory != null && !fixture.CollisionCategories.HasFlag(collisionCategory.Value) && !collisionCategory.Value.HasFlag(fixture.CollisionCategories))
			{
				return false;
			}
			if (ignoredBodies != null && ignoredBodies.Contains(fixture.Body))
			{
				return false;
			}
			Structure structure = fixture.Body.UserData as Structure;
			return structure == null || !structure.IsPlatform || collisionCategory == null || collisionCategory.Value.HasFlag(Category.Cat3);
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x00065F8C File Offset: 0x0006418C
		public static Body CheckVisibility(Vector2 rayStart, Vector2 rayEnd, bool ignoreLevel = false, bool ignoreSubs = false, bool ignoreSensors = true, bool ignoreDisabledWalls = true, bool ignoreBranches = true, Predicate<Fixture> blocksVisibilityPredicate = null)
		{
			Body closestBody = null;
			float closestFraction = 1f;
			Fixture closestFixture = null;
			Vector2 closestNormal = Vector2.Zero;
			if (Vector2.DistanceSquared(rayStart, rayEnd) < 0.01f)
			{
				Submarine.lastPickedPosition = rayEnd;
				return null;
			}
			GameMain.World.RayCast(delegate(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
			{
				if (fixture == null)
				{
					return -1f;
				}
				if (ignoreSensors && fixture.IsSensor)
				{
					return -1f;
				}
				if (ignoreLevel && fixture.CollisionCategories.HasFlag(Category.Cat8))
				{
					return -1f;
				}
				if (!fixture.CollisionCategories.HasFlag(Category.Cat8) && !fixture.CollisionCategories.HasFlag(Category.Cat1) && !fixture.CollisionCategories.HasFlag(Category.Cat9))
				{
					return -1f;
				}
				if (ignoreSubs && fixture.Body.UserData is Submarine)
				{
					return -1f;
				}
				if (ignoreBranches && fixture.Body.UserData is VineTile)
				{
					return -1f;
				}
				if (fixture.Body.UserData as string == "ruinroom")
				{
					return -1f;
				}
				if (fixture.UserData is Hull)
				{
					return -1f;
				}
				Structure structure = fixture.Body.UserData as Structure;
				if (structure != null)
				{
					if (structure.IsPlatform || structure.StairDirection != Direction.None)
					{
						return -1f;
					}
					if (ignoreDisabledWalls)
					{
						int sectionIndex = structure.FindSectionIndex(ConvertUnits.ToDisplayUnits(point), false, false);
						if (sectionIndex > -1 && structure.SectionBodyDisabled(sectionIndex))
						{
							return -1f;
						}
					}
				}
				if (blocksVisibilityPredicate != null && !blocksVisibilityPredicate(fixture))
				{
					return -1f;
				}
				if (fraction < closestFraction)
				{
					closestBody = fixture.Body;
					closestFraction = fraction;
					closestFixture = fixture;
					closestNormal = normal;
				}
				return closestFraction;
			}, rayStart, rayEnd, Category.All);
			Submarine.lastPickedPosition = rayStart + (rayEnd - rayStart) * closestFraction;
			Submarine.lastPickedFraction = closestFraction;
			Submarine.lastPickedFixture = closestFixture;
			Submarine.lastPickedNormal = closestNormal;
			return closestBody;
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000A7E RID: 2686 RVA: 0x00066068 File Offset: 0x00064268
		public bool FlippedX
		{
			get
			{
				return this.flippedX;
			}
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x00066070 File Offset: 0x00064270
		public void FlipX(List<Submarine> parents = null)
		{
			if (parents == null)
			{
				parents = new List<Submarine>();
			}
			parents.Add(this);
			this.flippedX = !this.flippedX;
			Item.UpdateHulls();
			List<Item> bodyItems = Item.ItemList.FindAll((Item it) => it.Submarine == this && it.body != null);
			List<MapEntity> subEntities = MapEntity.MapEntityList.FindAll((MapEntity me) => me.Submarine == this);
			foreach (MapEntity e in subEntities)
			{
				if (!(e is Item))
				{
					LinkedSubmarine linkedSub = e as LinkedSubmarine;
					if (linkedSub != null)
					{
						Submarine sub = linkedSub.Sub;
						if (sub == null)
						{
							Vector2 relative = linkedSub.Position - this.SubBody.Position;
							relative.X = -relative.X;
							linkedSub.Rect = new Rectangle((relative + this.SubBody.Position).ToPoint(), linkedSub.Rect.Size);
						}
						else if (!parents.Contains(sub))
						{
							Vector2 relative2 = sub.SubBody.Position - this.SubBody.Position;
							relative2.X = -relative2.X;
							sub.SetPosition(relative2 + this.SubBody.Position, new List<Submarine>(parents), true);
							sub.FlipX(parents);
						}
					}
					else
					{
						e.FlipX(true, false);
					}
				}
			}
			foreach (MapEntity mapEntity in subEntities)
			{
				mapEntity.Move(-this.HiddenSubPosition, true);
			}
			BodyType prevBodyType = this.subBody.Body.BodyType;
			Vector2 pos = new Vector2(this.subBody.Position.X, this.subBody.Position.Y);
			this.subBody.Body.Remove();
			this.subBody = new SubmarineBody(this, true);
			this.subBody.Body.BodyType = prevBodyType;
			this.SetPosition(pos, new List<Submarine>(from p in parents
			where p != this
			select p), true);
			if (this.entityGrid != null)
			{
				Hull.EntityGrids.Remove(this.entityGrid);
				this.entityGrid = null;
			}
			this.entityGrid = Hull.GenerateEntityGrid(this);
			this.SubBody.FlipX();
			foreach (MapEntity mapEntity2 in subEntities)
			{
				mapEntity2.Move(this.HiddenSubPosition, true);
			}
			for (int i = 0; i < 2; i++)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item.GetComponent<DockingPort>() != null != (i == 0))
					{
						if (bodyItems.Contains(item))
						{
							item.Submarine = this;
							if (this.Position == Vector2.Zero)
							{
								item.Move(-this.HiddenSubPosition, true);
							}
						}
						else if (item.Submarine != this)
						{
							continue;
						}
						item.FlipX(true, false);
						if (!item.Prefab.CanFlipX && item.Prefab.AllowRotatingInEditor)
						{
							item.Rotation = -item.Rotation;
						}
					}
				}
			}
			foreach (DockingPort dockingPort in DockingPort.List)
			{
				DockingPort dockingTarget = dockingPort.DockingTarget;
				if (dockingTarget != null)
				{
					dockingPort.Undock(true);
					dockingPort.Dock(dockingTarget);
				}
			}
			Item.UpdateHulls();
			Gap.UpdateHulls();
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x000664D4 File Offset: 0x000646D4
		public void EnableFactionSpecificEntities(Identifier factionIdentifier)
		{
			foreach (FactionPrefab faction in FactionPrefab.Prefabs)
			{
				this.SetLayerEnabled(faction.Identifier, faction.Identifier == factionIdentifier, false);
			}
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x00066534 File Offset: 0x00064734
		public static bool LayerExistsInAnySub(Identifier layer)
		{
			foreach (MapEntity me in MapEntity.MapEntityList)
			{
				if (me.Layer == layer)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x00066598 File Offset: 0x00064798
		public bool LayerExists(Identifier layer)
		{
			foreach (MapEntity me in MapEntity.MapEntityList)
			{
				if (me.Submarine == this || me.Layer == layer)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x00066604 File Offset: 0x00064804
		public void SetLayerEnabled(Identifier layer, bool enabled, bool sendNetworkEvent = false)
		{
			Submarine.SetLayerEnabled(layer, enabled, from m in MapEntity.MapEntityList
			where m.Submarine == this
			select m);
			if (sendNetworkEvent)
			{
				GameMain.Server.CreateEntityEvent(this, new Submarine.SetLayerEnabledEventData(layer, enabled));
			}
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x00066640 File Offset: 0x00064840
		public static void SetLayerEnabled(Identifier layer, bool enabled, IEnumerable<MapEntity> entities)
		{
			foreach (MapEntity entity in MapEntity.MapEntityList)
			{
				if (!string.IsNullOrEmpty(entity.Layer) && !(entity.Layer != layer))
				{
					entity.IsLayerHidden = !enabled;
					WayPoint wp = entity as WayPoint;
					if (wp != null)
					{
						if (enabled)
						{
							wp.SpawnType = wp.SpawnType.RemoveFlag(SpawnType.Disabled);
						}
						else
						{
							wp.SpawnType = wp.SpawnType.AddFlag(SpawnType.Disabled);
						}
					}
					else
					{
						Item item = entity as Item;
						if (item != null)
						{
							Submarine.<SetLayerEnabled>g__SetItemHidden|161_0(item, entity.IsLayerHidden);
						}
					}
				}
			}
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x00066704 File Offset: 0x00064904
		public void Update(float deltaTime)
		{
			this.RefreshConnectedSubs();
			if (this.Info.IsWreck)
			{
				WreckAI wreckAI = this.WreckAI;
				if (wreckAI != null)
				{
					wreckAI.Update(deltaTime);
				}
			}
			SubmarineTurretAI turretAI = this.TurretAI;
			if (turretAI != null)
			{
				turretAI.Update(deltaTime);
			}
			SubmarineBody submarineBody = this.subBody;
			if (((submarineBody != null) ? submarineBody.Body : null) == null)
			{
				return;
			}
			if (Level.Loaded != null && this.WorldPosition.Y < -1000000f && this.subBody.Body.Enabled && !this.IsRespawnShuttle)
			{
				this.subBody.Body.ResetDynamics();
				this.subBody.Body.Enabled = false;
				foreach (Character c in Character.CharacterList)
				{
					if (c.Submarine == this)
					{
						c.Kill(CauseOfDeathType.Pressure, null, false, true);
						c.Enabled = false;
					}
				}
				return;
			}
			this.subBody.Body.LinearVelocity = new Vector2(Submarine.LockX ? 0f : this.subBody.Body.LinearVelocity.X, Submarine.LockY ? 0f : this.subBody.Body.LinearVelocity.Y);
			this.subBody.Update(deltaTime);
			for (int i = 0; i < 2; i++)
			{
				if (Submarine.MainSubs[i] != null && this != Submarine.MainSubs[i] && Submarine.MainSubs[i].DockedTo.Contains(this))
				{
					return;
				}
			}
			this.networkUpdateTimer -= MathHelper.Clamp(this.Velocity.Length() * 10f, 0.1f, 5f) * deltaTime;
			if (this.networkUpdateTimer < 0f)
			{
				this.networkUpdateTimer = 1f;
			}
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x000668F8 File Offset: 0x00064AF8
		public void ApplyForce(Vector2 force)
		{
			if (this.subBody != null)
			{
				this.subBody.ApplyForce(force);
			}
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x00066910 File Offset: 0x00064B10
		public void EnableMaintainPosition()
		{
			using (List<Item>.Enumerator enumerator = Item.ItemList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					if (item.Submarine == this)
					{
						Steering steering = item.GetComponent<Steering>();
						if (steering != null && item.Connections != null)
						{
							List<Item> connectedItems = new List<Item>();
							foreach (Connection c in item.Connections)
							{
								if (!c.IsPower)
								{
									connectedItems.AddRange(from engine in item.GetConnectedComponentsRecursive<Engine>(c, false, true)
									select engine.Item);
									connectedItems.AddRange(from pump in item.GetConnectedComponentsRecursive<Pump>(c, false, true)
									select pump.Item);
								}
							}
							if (connectedItems.Count((Item it) => it.Submarine != item.Submarine) <= connectedItems.Count / 2)
							{
								steering.MaintainPos = true;
								steering.PosToMaintain = new Vector2?(this.WorldPosition);
								steering.AutoPilot = true;
								steering.UnsentChanges = true;
							}
						}
					}
				}
			}
		}

		// Token: 0x06000A88 RID: 2696 RVA: 0x00066AC4 File Offset: 0x00064CC4
		public void NeutralizeBallast()
		{
			if (this.PhysicsBody.BodyType != BodyType.Dynamic)
			{
				return;
			}
			float neutralBallastLevel = 0.5f;
			int selectedSteeringValue = 0;
			foreach (Item item in Item.ItemList)
			{
				if (item.Submarine == this)
				{
					Steering steering = item.GetComponent<Steering>();
					if (steering != null)
					{
						int steeringValue = 1;
						ConnectionPanel component = item.GetComponent<ConnectionPanel>();
						Connection connection;
						if (component == null)
						{
							connection = null;
						}
						else
						{
							connection = component.Connections.Find((Connection c) => c.Name == "velocity_x_out");
						}
						Connection connectionX = connection;
						ConnectionPanel component2 = item.GetComponent<ConnectionPanel>();
						Connection connection2;
						if (component2 == null)
						{
							connection2 = null;
						}
						else
						{
							connection2 = component2.Connections.Find((Connection c) => c.Name == "velocity_y_out");
						}
						Connection connectionY = connection2;
						if (connectionX != null)
						{
							foreach (Engine engine in steering.Item.GetConnectedComponentsRecursive<Engine>(connectionX, false, true))
							{
								if (engine.Item.Submarine == this)
								{
									steeringValue++;
								}
							}
						}
						if (connectionY != null)
						{
							foreach (Pump pump in steering.Item.GetConnectedComponentsRecursive<Pump>(connectionY, false, true))
							{
								if (pump.Item.Submarine == this)
								{
									steeringValue++;
								}
							}
						}
						if (steeringValue > selectedSteeringValue)
						{
							neutralBallastLevel = steering.NeutralBallastLevel;
						}
					}
				}
			}
			HashSet<Hull> ballastHulls = new HashSet<Hull>();
			foreach (Item item2 in Item.ItemList)
			{
				if (item2.Submarine == this)
				{
					Pump pump2 = item2.GetComponent<Pump>();
					if (pump2 != null && item2.CurrentHull != null && item2.GetComponent<ConnectionPanel>() != null && (item2.HasTag(Tags.Ballast) || item2.CurrentHull.RoomName.Contains("ballast", StringComparison.OrdinalIgnoreCase)))
					{
						pump2.FlowPercentage = 0f;
						ballastHulls.Add(item2.CurrentHull);
					}
				}
			}
			float waterVolume = 0f;
			float volume = 0f;
			float excessWater = 0f;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine == this)
				{
					waterVolume += hull.WaterVolume;
					volume += hull.Volume;
					if (!ballastHulls.Contains(hull))
					{
						excessWater += hull.WaterVolume;
					}
				}
			}
			neutralBallastLevel -= excessWater / volume;
			neutralBallastLevel *= 0.9f;
			foreach (Hull hull2 in ballastHulls)
			{
				hull2.WaterVolume = hull2.Volume * neutralBallastLevel;
			}
		}

		// Token: 0x06000A89 RID: 2697 RVA: 0x00066E60 File Offset: 0x00065060
		public void SetPrevTransform(Vector2 position)
		{
			this.prevPosition = position;
		}

		// Token: 0x06000A8A RID: 2698 RVA: 0x00066E6C File Offset: 0x0006506C
		public void SetPosition(Vector2 position, List<Submarine> checkd = null, bool forceUndockFromStaticSubmarines = true)
		{
			if (!MathUtils.IsValid(position))
			{
				return;
			}
			if (checkd == null)
			{
				checkd = new List<Submarine>();
			}
			if (checkd.Contains(this))
			{
				return;
			}
			checkd.Add(this);
			this.subBody.SetPosition(position);
			this.UpdateTransform(false);
			foreach (Submarine dockedSub in this.DockedTo)
			{
				DockingPort port;
				if (dockedSub.PhysicsBody.BodyType == BodyType.Static && forceUndockFromStaticSubmarines && this.ConnectedDockingPorts.TryGetValue(dockedSub, out port))
				{
					port.Undock(false);
				}
				else
				{
					Vector2? expectedLocation = Submarine.CalculateDockOffset(this, dockedSub);
					if (expectedLocation != null)
					{
						dockedSub.SetPosition(position + expectedLocation.Value, checkd, forceUndockFromStaticSubmarines);
						dockedSub.UpdateTransform(false);
					}
				}
			}
		}

		// Token: 0x06000A8B RID: 2699 RVA: 0x00066F44 File Offset: 0x00065144
		public static Vector2? CalculateDockOffset(Submarine sub, Submarine dockedSub)
		{
			Item myPort = sub.ConnectedDockingPorts.ContainsKey(dockedSub) ? sub.ConnectedDockingPorts[dockedSub].Item : null;
			if (myPort == null)
			{
				return null;
			}
			Item theirPort = dockedSub.ConnectedDockingPorts.ContainsKey(sub) ? dockedSub.ConnectedDockingPorts[sub].Item : null;
			if (theirPort == null)
			{
				return null;
			}
			return new Vector2?(myPort.Position - sub.HiddenSubPosition - (theirPort.Position - dockedSub.HiddenSubPosition));
		}

		// Token: 0x06000A8C RID: 2700 RVA: 0x00066FDD File Offset: 0x000651DD
		public void Translate(Vector2 amount)
		{
			if (amount == Vector2.Zero || !MathUtils.IsValid(amount))
			{
				return;
			}
			this.subBody.SetPosition(this.subBody.Position + amount);
		}

		// Token: 0x06000A8D RID: 2701 RVA: 0x00067014 File Offset: 0x00065214
		public static Submarine FindClosest(Vector2 worldPosition, bool ignoreOutposts = false, bool ignoreOutsideLevel = true, bool ignoreRespawnShuttle = false, CharacterTeamType? teamType = null)
		{
			Submarine closest = null;
			float closestDist = 0f;
			foreach (Submarine sub in Submarine.loaded)
			{
				if ((!ignoreOutposts || !sub.Info.IsOutpost) && (!ignoreOutsideLevel || Level.Loaded == null || !sub.IsAboveLevel) && (!ignoreRespawnShuttle || !sub.IsRespawnShuttle))
				{
					if (teamType != null)
					{
						CharacterTeamType teamID = sub.TeamID;
						CharacterTeamType? characterTeamType = teamType;
						if (!(teamID == characterTeamType.GetValueOrDefault() & characterTeamType != null))
						{
							continue;
						}
					}
					float dist = Vector2.DistanceSquared(worldPosition, sub.WorldPosition);
					if (closest == null || dist < closestDist)
					{
						closest = sub;
						closestDist = dist;
					}
				}
			}
			return closest;
		}

		// Token: 0x06000A8E RID: 2702 RVA: 0x000670D8 File Offset: 0x000652D8
		public bool IsConnectedTo(Submarine otherSub)
		{
			return this == otherSub || this.GetConnectedSubs().Contains(otherSub);
		}

		// Token: 0x06000A8F RID: 2703 RVA: 0x000670EC File Offset: 0x000652EC
		public List<Hull> GetHulls(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Hull>(alsoFromConnectedSubs, Hull.HullList);
		}

		// Token: 0x06000A90 RID: 2704 RVA: 0x000670FA File Offset: 0x000652FA
		public List<Gap> GetGaps(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Gap>(alsoFromConnectedSubs, Gap.GapList);
		}

		// Token: 0x06000A91 RID: 2705 RVA: 0x00067108 File Offset: 0x00065308
		public List<Item> GetItems(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Item>(alsoFromConnectedSubs, Item.ItemList);
		}

		// Token: 0x06000A92 RID: 2706 RVA: 0x00067116 File Offset: 0x00065316
		public List<WayPoint> GetWaypoints(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<WayPoint>(alsoFromConnectedSubs, WayPoint.WayPointList);
		}

		// Token: 0x06000A93 RID: 2707 RVA: 0x00067124 File Offset: 0x00065324
		public List<Structure> GetWalls(bool alsoFromConnectedSubs)
		{
			return this.GetEntities<Structure>(alsoFromConnectedSubs, Structure.WallList);
		}

		// Token: 0x06000A94 RID: 2708 RVA: 0x00067134 File Offset: 0x00065334
		public List<T> GetEntities<T>(bool includingConnectedSubs, List<T> list) where T : MapEntity
		{
			return list.FindAll((T e) => this.IsEntityFoundOnThisSub(e, includingConnectedSubs, false, false));
		}

		// Token: 0x06000A95 RID: 2709 RVA: 0x00067168 File Offset: 0x00065368
		[return: TupleElementNames(new string[]
		{
			"container",
			"freeSlots"
		})]
		public List<ValueTuple<ItemContainer, int>> GetCargoContainers()
		{
			List<ValueTuple<ItemContainer, int>> containers = new List<ValueTuple<ItemContainer, int>>();
			IEnumerable<Submarine> connectedSubs = this.GetConnectedSubs().Where(delegate(Submarine sub)
			{
				SubmarineInfo info = sub.Info;
				SubmarineType? submarineType = (info != null) ? new SubmarineType?(info.Type) : null;
				SubmarineType type = this.Info.Type;
				return submarineType.GetValueOrDefault() == type & submarineType != null;
			});
			foreach (Item item in Item.ItemList.ToList<Item>())
			{
				if (connectedSubs.Contains(item.Submarine) && item.HasTag(Tags.CargoContainer) && !item.HasTag(Tags.DisallowCargo) && !item.NonInteractable && !item.IsHidden)
				{
					ItemContainer itemContainer = item.GetComponent<ItemContainer>();
					if (itemContainer != null)
					{
						int emptySlots = 0;
						for (int i = 0; i < itemContainer.Inventory.Capacity; i++)
						{
							if (itemContainer.Inventory.GetItemAt(i) == null)
							{
								emptySlots++;
							}
						}
						containers.Add(new ValueTuple<ItemContainer, int>(itemContainer, emptySlots));
					}
				}
			}
			return containers;
		}

		// Token: 0x06000A96 RID: 2710 RVA: 0x00067264 File Offset: 0x00065464
		public IEnumerable<T> GetEntities<T>(bool includingConnectedSubs, IEnumerable<T> list) where T : MapEntity
		{
			return from e in list
			where this.IsEntityFoundOnThisSub(e, includingConnectedSubs, false, false)
			select e;
		}

		// Token: 0x06000A97 RID: 2711 RVA: 0x00067298 File Offset: 0x00065498
		public bool IsEntityFoundOnThisSub(MapEntity entity, bool includingConnectedSubs, bool allowDifferentTeam = false, bool allowDifferentType = false)
		{
			if (entity == null)
			{
				return false;
			}
			if (entity.Submarine == this)
			{
				return true;
			}
			if (entity.Submarine == null)
			{
				return false;
			}
			if (includingConnectedSubs)
			{
				foreach (Submarine s in this.connectedSubs)
				{
					if (s == entity.Submarine && (allowDifferentTeam || entity.Submarine.TeamID == this.TeamID) && (allowDifferentType || entity.Submarine.Info.Type == this.Info.Type))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000A98 RID: 2712 RVA: 0x00067348 File Offset: 0x00065548
		public static Submarine FindContainingInLocalCoordinates(Vector2 position, float inflate = 500f)
		{
			foreach (Submarine sub in Submarine.Loaded)
			{
				Rectangle subBorders = sub.Borders;
				subBorders.Location += MathUtils.ToPoint(sub.HiddenSubPosition) - new Point(0, sub.Borders.Height);
				subBorders.Inflate(inflate, inflate);
				if (subBorders.Contains(position))
				{
					return sub;
				}
			}
			return null;
		}

		// Token: 0x06000A99 RID: 2713 RVA: 0x000673E8 File Offset: 0x000655E8
		public static Submarine FindContaining(Vector2 worldPosition, float inflate = 500f)
		{
			foreach (Submarine sub in Submarine.Loaded)
			{
				Rectangle worldBorders = sub.Borders;
				worldBorders.Location += sub.WorldPosition.ToPoint() - new Point(0, sub.Borders.Height);
				worldBorders.Inflate(inflate, inflate);
				if (worldBorders.Contains(worldPosition))
				{
					return sub;
				}
			}
			return null;
		}

		// Token: 0x06000A9A RID: 2714 RVA: 0x0006748C File Offset: 0x0006568C
		public static Rectangle GetBorders(XElement submarineElement)
		{
			Vector4 bounds = new Vector4(float.MaxValue, float.MinValue, float.MinValue, float.MaxValue);
			foreach (XElement element in submarineElement.Elements())
			{
				if (element.Name == "Structure")
				{
					string name = element.GetAttributeString("name", "");
					Identifier identifier = element.GetAttributeIdentifier("identifier", "");
					StructurePrefab prefab = Structure.FindPrefab(name, identifier);
					if (prefab != null && prefab.Body)
					{
						Rectangle rect = element.GetAttributeRect("rect", Rectangle.Empty);
						bounds = new Vector4(Math.Min((float)rect.X, bounds.X), Math.Max((float)rect.Y, bounds.Y), Math.Max((float)rect.Right, bounds.Z), Math.Min((float)(rect.Y - rect.Height), bounds.W));
					}
				}
				else if (element.Name == "LinkedSubmarine")
				{
					Point dimensions = element.GetAttributePoint("dimensions", Point.Zero);
					Point pos = element.GetAttributeVector2("pos", Vector2.Zero).ToPoint();
					bounds = new Vector4(Math.Min((float)(pos.X - dimensions.X / 2), bounds.X), Math.Max((float)(pos.Y + dimensions.Y / 2), bounds.Y), Math.Max((float)(pos.X + dimensions.X / 2), bounds.Z), Math.Min((float)(pos.Y - dimensions.Y / 2), bounds.W));
				}
			}
			if (bounds.X == 3.4028235E+38f || bounds.Y == -3.4028235E+38f || bounds.Z == -3.4028235E+38f || bounds.W == 3.4028235E+38f)
			{
				return Rectangle.Empty;
			}
			return new Rectangle((int)bounds.X, (int)bounds.Y, (int)(bounds.Z - bounds.X), (int)(bounds.Y - bounds.W));
		}

		// Token: 0x06000A9B RID: 2715 RVA: 0x000676F4 File Offset: 0x000658F4
		public Submarine(SubmarineInfo info, bool showErrorMessages = true, Func<Submarine, List<MapEntity>> loadEntities = null, IdRemap linkedRemap = null) : base(null, 0)
		{
			Stopwatch sw = Stopwatch.StartNew();
			this.connectedSubs = new HashSet<Submarine>(2)
			{
				this
			};
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(9, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Submarine");
			defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.ID);
			this.upgradeEventIdentifier = new Identifier(defaultInterpolatedStringHandler.ToStringAndClear());
			this.Loading = true;
			GameMain.World.Enabled = false;
			try
			{
				Submarine.loaded.Add(this);
				this.Info = new SubmarineInfo(info);
				this.ConnectedDockingPorts = new Dictionary<Submarine, DockingPort>();
				this.HiddenSubPosition = Submarine.HiddenSubStartPosition;
				GameSession gameSession = GameMain.GameSession;
				if (((gameSession != null) ? gameSession.LevelData : null) != null)
				{
					this.HiddenSubPosition += Vector2.UnitY * (float)GameMain.GameSession.LevelData.Size.Y;
				}
				for (int i = 0; i < Submarine.loaded.Count; i++)
				{
					Submarine sub = Submarine.loaded[i];
					this.HiddenSubPosition = new Vector2(-this.HiddenSubPosition.X, this.HiddenSubPosition.Y + (float)sub.Borders.Height + 5000f);
				}
				this.IdOffset = IdRemap.DetermineNewOffset();
				List<MapEntity> newEntities = new List<MapEntity>();
				if (loadEntities == null)
				{
					if (this.Info.SubmarineElement != null)
					{
						newEntities = MapEntity.LoadAll(this, this.Info.SubmarineElement, this.Info.FilePath, (int)this.IdOffset);
					}
				}
				else
				{
					newEntities = loadEntities(this);
					newEntities.ForEach(delegate(MapEntity me)
					{
						me.Submarine = this;
					});
				}
				if (newEntities != null)
				{
					foreach (MapEntity e in newEntities)
					{
						if (linkedRemap != null)
						{
							e.ResolveLinks(linkedRemap);
						}
						e.unresolvedLinkedToID = null;
					}
				}
				Vector2 center = Vector2.Zero;
				List<Hull> matchingHulls = Hull.HullList.FindAll((Hull h) => h.Submarine == this);
				if (matchingHulls.Any<Hull>())
				{
					Vector2 topLeft = new Vector2((float)matchingHulls[0].Rect.X, (float)matchingHulls[0].Rect.Y);
					Vector2 bottomRight = new Vector2((float)matchingHulls[0].Rect.X, (float)matchingHulls[0].Rect.Y);
					foreach (Hull hull in matchingHulls)
					{
						if ((float)hull.Rect.X < topLeft.X)
						{
							topLeft.X = (float)hull.Rect.X;
						}
						if ((float)hull.Rect.Y > topLeft.Y)
						{
							topLeft.Y = (float)hull.Rect.Y;
						}
						if ((float)hull.Rect.Right > bottomRight.X)
						{
							bottomRight.X = (float)hull.Rect.Right;
						}
						if ((float)(hull.Rect.Y - hull.Rect.Height) < bottomRight.Y)
						{
							bottomRight.Y = (float)(hull.Rect.Y - hull.Rect.Height);
						}
					}
					center = (topLeft + bottomRight) / 2f;
					center.X -= center.X % Submarine.GridSize.X;
					center.Y -= center.Y % Submarine.GridSize.Y;
					Submarine.RepositionEntities(-center, from me in MapEntity.MapEntityList
					where me.Submarine == this
					select me);
				}
				this.subBody = new SubmarineBody(this, showErrorMessages);
				Vector2 pos = ConvertUnits.ToSimUnits(this.HiddenSubPosition);
				this.subBody.Body.FarseerBody.SetTransformIgnoreContacts(ref pos, 0f);
				if (info.IsOutpost)
				{
					this.ShowSonarMarker = false;
					this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
					this.TeamID = CharacterTeamType.FriendlyNPC;
					foreach (Submarine dockedSub in this.DockedTo)
					{
						dockedSub.TeamID = CharacterTeamType.FriendlyNPC;
					}
					bool flag;
					if (GameMain.NetworkMember != null && !GameMain.NetworkMember.ServerSettings.DestructibleOutposts)
					{
						OutpostGenerationParams outpostGenerationParams = info.OutpostGenerationParams;
						flag = (outpostGenerationParams == null || !outpostGenerationParams.AlwaysDestructible);
					}
					else
					{
						flag = false;
					}
					bool indestructible = flag;
					using (List<MapEntity>.Enumerator enumerator4 = MapEntity.MapEntityList.GetEnumerator())
					{
						while (enumerator4.MoveNext())
						{
							MapEntity me3 = enumerator4.Current;
							if (me3.Submarine == this)
							{
								Item item = me3 as Item;
								if (item != null)
								{
									item.AllowStealing = true;
									if (info.OutpostGenerationParams != null)
									{
										item.SpawnedInCurrentOutpost = true;
										Item item3 = item;
										bool allowStealing;
										if (!info.OutpostGenerationParams.AllowStealing)
										{
											Item rootContainer = item.RootContainer;
											if (rootContainer != null)
											{
												ItemPrefab prefab = rootContainer.Prefab;
												if (prefab != null)
												{
													allowStealing = prefab.AllowStealingContainedItems;
													goto IL_543;
												}
											}
											allowStealing = false;
										}
										else
										{
											allowStealing = true;
										}
										IL_543:
										item3.AllowStealing = allowStealing;
									}
									if (item.GetComponent<Repairable>() != null && indestructible)
									{
										item.Indestructible = true;
									}
									using (List<ItemComponent>.Enumerator enumerator5 = item.Components.GetEnumerator())
									{
										while (enumerator5.MoveNext())
										{
											ItemComponent ic = enumerator5.Current;
											ConnectionPanel connectionPanel = ic as ConnectionPanel;
											if (connectionPanel != null)
											{
												if (info.OutpostGenerationParams != null && !info.OutpostGenerationParams.AlwaysRewireable)
												{
													connectionPanel.Locked = true;
												}
											}
											else
											{
												Holdable holdable = ic as Holdable;
												if (holdable != null && holdable.Attached && item.GetComponent<LevelResource>() == null)
												{
													holdable.CanBePicked = false;
													holdable.CanBeSelected = false;
												}
											}
										}
										continue;
									}
								}
								Structure structure = me3 as Structure;
								if (structure != null && structure.Prefab.IndestructibleInOutposts && indestructible)
								{
									structure.Indestructible = true;
								}
							}
						}
						goto IL_651;
					}
				}
				if (info.IsRuin)
				{
					this.ShowSonarMarker = false;
					this.PhysicsBody.FarseerBody.BodyType = BodyType.Static;
				}
				IL_651:
				if (this.entityGrid != null)
				{
					Hull.EntityGrids.Remove(this.entityGrid);
					this.entityGrid = null;
				}
				this.entityGrid = Hull.GenerateEntityGrid(this);
				for (int j = 0; j < MapEntity.MapEntityList.Count; j++)
				{
					if (MapEntity.MapEntityList[j].Submarine == this)
					{
						MapEntity.MapEntityList[j].Move(this.HiddenSubPosition, true);
					}
				}
				this.Loading = false;
				MapEntity.MapLoaded(newEntities, true);
				foreach (MapEntity me2 in MapEntity.MapEntityList)
				{
					if (me2.Submarine == this)
					{
						LinkedSubmarine linkedSub = me2 as LinkedSubmarine;
						if (linkedSub != null)
						{
							linkedSub.LinkDummyToMainSubmarine();
						}
						else
						{
							WayPoint wayPoint = me2 as WayPoint;
							if (wayPoint != null && wayPoint.SpawnType.HasFlag(SpawnType.ExitPoint))
							{
								this.exitPoints.Add(wayPoint);
							}
						}
					}
				}
				foreach (Hull hull2 in matchingHulls)
				{
					if (string.IsNullOrEmpty(hull2.RoomName))
					{
						hull2.RoomName = hull2.CreateRoomName();
					}
				}
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					foreach (Identifier layer in this.Info.LayersHiddenByDefault)
					{
						this.SetLayerEnabled(layer, false, false);
					}
				}
				GameSession gameSession2 = GameMain.GameSession;
				if (gameSession2 != null)
				{
					CampaignMode campaign = gameSession2.Campaign;
					if (campaign != null)
					{
						UpgradeManager upgradeManager = campaign.UpgradeManager;
						if (upgradeManager != null)
						{
							upgradeManager.OnUpgradesChanged.Register(this.upgradeEventIdentifier, delegate(UpgradeManager _)
							{
								this.ResetCrushDepth();
							});
						}
					}
				}
				if (showErrorMessages && !string.IsNullOrEmpty(this.Info.FilePath) && Screen.Selected != GameMain.SubEditorScreen && (this.Info.GameVersion == null || this.Info.GameVersion < new Version("0.8.9.0")))
				{
					DebugConsole.ThrowError("The submarine \"" + this.Info.Name + "\" was made using an older version of the Barotrauma that used a different formula to calculate the lighting. The game automatically adjusts the lights make them look better with the new formula, but it's recommended to open the submarine in the submarine editor and make sure everything looks right after the automatic conversion.", null, null, false, false);
					foreach (Item item2 in Item.ItemList)
					{
						if (item2.Submarine == this && item2.ParentInventory == null && item2.body == null)
						{
							foreach (LightComponent light in item2.GetComponents<LightComponent>())
							{
								light.LightColor = new Color(light.LightColor, (float)light.LightColor.A / 255f * 0.5f);
							}
						}
					}
				}
				this.GenerateOutdoorNodes();
			}
			finally
			{
				this.Loading = false;
				GameMain.World.Enabled = true;
			}
			sw.Stop();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(18, 2);
			defaultInterpolatedStringHandler2.AppendLiteral("Loading ");
			SubmarineInfo info2 = this.Info;
			defaultInterpolatedStringHandler2.AppendFormatted(((info2 != null) ? info2.Name : null) ?? "unknown");
			defaultInterpolatedStringHandler2.AppendLiteral(" took ");
			defaultInterpolatedStringHandler2.AppendFormatted<long>(sw.ElapsedMilliseconds);
			defaultInterpolatedStringHandler2.AppendLiteral(" ms.");
			string debugMsg = defaultInterpolatedStringHandler2.ToStringAndClear();
			DebugConsole.Log(debugMsg);
		}

		// Token: 0x06000A9C RID: 2716 RVA: 0x000681F8 File Offset: 0x000663F8
		protected override ushort DetermineID(ushort id, Submarine submarine)
		{
			return (ushort)(65532 - Submarine.loaded.Count);
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x0006820B File Offset: 0x0006640B
		public static Submarine Load(SubmarineInfo info, bool unloadPrevious, IdRemap linkedRemap = null)
		{
			if (unloadPrevious)
			{
				Submarine.Unload();
			}
			return new Submarine(info, false, null, linkedRemap);
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x0006821E File Offset: 0x0006641E
		private void ResetCrushDepth()
		{
			this.realWorldCrushDepth = null;
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x0006822C File Offset: 0x0006642C
		public void SetCrushDepth(float realWorldCrushDepth)
		{
			foreach (Structure structure in Structure.WallList)
			{
				if (structure.Submarine == this && structure.HasBody && !structure.Indestructible)
				{
					structure.CrushDepth = realWorldCrushDepth;
				}
			}
			this.realWorldCrushDepth = new float?(realWorldCrushDepth);
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x000682A4 File Offset: 0x000664A4
		public static void RepositionEntities(Vector2 moveAmount, IEnumerable<MapEntity> entities)
		{
			if (moveAmount.LengthSquared() < 1E-05f)
			{
				return;
			}
			foreach (MapEntity entity in entities)
			{
				Item item = entity as Item;
				if (item != null)
				{
					Wire component = item.GetComponent<Wire>();
					if (component != null)
					{
						component.MoveNodes(moveAmount);
					}
				}
				entity.Move(moveAmount, true);
			}
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x00068318 File Offset: 0x00066518
		public bool CheckFuel()
		{
			float fuel = (from i in this.GetItems(true)
			where i.HasTag(Tags.ReactorFuel)
			select i).Sum((Item i) => i.Condition);
			this.Info.LowFuel = (fuel < 200f);
			return !this.Info.LowFuel;
		}

		// Token: 0x06000AA2 RID: 2722 RVA: 0x00068398 File Offset: 0x00066598
		public void SaveToXElement(XElement element)
		{
			element.Add(new XAttribute("name", this.Info.Name));
			element.Add(new XAttribute("description", this.Info.Description ?? ""));
			element.Add(new XAttribute("checkval", Rand.Int(int.MaxValue, Rand.RandSync.Unsynced)));
			element.Add(new XAttribute("price", this.Info.Price));
			element.Add(new XAttribute("tier", this.Info.Tier));
			element.Add(new XAttribute("initialsuppliesspawned", this.Info.InitialSuppliesSpawned));
			element.Add(new XAttribute("noitems", this.Info.NoItems));
			element.Add(new XAttribute("lowfuel", !this.CheckFuel()));
			element.Add(new XAttribute("type", this.Info.Type.ToString()));
			element.Add(new XAttribute("ismanuallyoutfitted", this.Info.IsManuallyOutfitted));
			if (this.Info.IsPlayer && !this.Info.HasTag(SubmarineTag.Shuttle))
			{
				element.Add(new XAttribute("class", this.Info.SubmarineClass.ToString()));
			}
			element.Add(new XAttribute("tags", this.Info.Tags.ToString()));
			element.Add(new XAttribute("outposttags", this.Info.OutpostTags.ConvertToString(",")));
			element.Add(new XAttribute("triggeroutpostmissionevents", this.Info.TriggerOutpostMissionEvents.ConvertToString(",")));
			element.Add(new XAttribute("gameversion", GameMain.Version.ToString()));
			Rectangle dimensions = this.VisibleBorders;
			element.Add(new XAttribute("dimensions", XMLExtensions.Vector2ToString(dimensions.Size.ToVector2())));
			List<ValueTuple<ItemContainer, int>> cargoContainers = this.GetCargoContainers();
			int cargoCapacity = cargoContainers.Sum(([TupleElementNames(new string[]
			{
				"container",
				"freeSlots"
			})] ValueTuple<ItemContainer, int> c) => c.Item1.Capacity);
			foreach (MapEntity me in MapEntity.MapEntityList)
			{
				LinkedSubmarine linkedSub = me as LinkedSubmarine;
				if (linkedSub != null && linkedSub.Submarine == this)
				{
					cargoCapacity += linkedSub.CargoCapacity;
				}
			}
			element.Add(new XAttribute("cargocapacity", cargoCapacity));
			element.Add(new XAttribute("recommendedcrewsizemin", this.Info.RecommendedCrewSizeMin));
			element.Add(new XAttribute("recommendedcrewsizemax", this.Info.RecommendedCrewSizeMax));
			element.Add(new XAttribute("recommendedcrewexperience", this.Info.RecommendedCrewExperience.ToString()));
			element.Add(new XAttribute("requiredcontentpackages", string.Join(", ", this.Info.RequiredContentPackages)));
			if (this.Info.LayersHiddenByDefault.Any<Identifier>())
			{
				element.Add(new XAttribute("layerhiddenbydefault", string.Join<Identifier>(", ", this.Info.LayersHiddenByDefault)));
			}
			if (this.Info.WreckInfo != null)
			{
				bool hasThalamus = false;
				IEnumerable<Identifier> wreckAiEntities = from p in WreckAIConfig.Prefabs
				select p.Entity;
				IEnumerable<ItemPrefab> prefabsOnSub = (from i in this.GetItems(true)
				select i.Prefab).Distinct<ItemPrefab>();
				foreach (ItemPrefab prefab in prefabsOnSub)
				{
					foreach (Identifier entity in wreckAiEntities)
					{
						if (WreckAI.IsThalamus(prefab, entity))
						{
							hasThalamus = true;
							break;
						}
					}
					if (hasThalamus)
					{
						break;
					}
				}
				element.Add(new XAttribute("WreckContainsThalamus", hasThalamus ? WreckInfo.HasThalamus.Yes : WreckInfo.HasThalamus.No));
			}
			if (this.Info.Type == SubmarineType.OutpostModule)
			{
				OutpostModuleInfo outpostModuleInfo = this.Info.OutpostModuleInfo;
				if (outpostModuleInfo != null)
				{
					outpostModuleInfo.Save(element);
				}
			}
			ExtraSubmarineInfo extraSubInfo = this.Info.GetExtraSubmarineInfo;
			if (extraSubInfo != null)
			{
				extraSubInfo.Save(element);
			}
			foreach (Item item in Item.ItemList)
			{
				ItemPrefab pendingItemSwap = item.PendingItemSwap;
				if (((pendingItemSwap != null) ? pendingItemSwap.SwappableItem : null) != null)
				{
					Dictionary<Item, ItemPrefab> connectedItemsToSwap = item.GetConnectedItemsToSwap(item.PendingItemSwap.SwappableItem);
					foreach (KeyValuePair<Item, ItemPrefab> kvp in connectedItemsToSwap)
					{
						Item itemToSwap = kvp.Key;
						ItemPrefab swapTo = kvp.Value;
						itemToSwap.PurchasedNewSwap = item.PurchasedNewSwap;
						if (itemToSwap.Prefab != swapTo)
						{
							itemToSwap.PendingItemSwap = swapTo;
						}
					}
				}
			}
			Dictionary<int, MapEntity> savedEntities = new Dictionary<int, MapEntity>();
			foreach (MapEntity e2 in from e in MapEntity.MapEntityList
			orderby e.ID
			select e)
			{
				if (e2.ShouldBeSaved)
				{
					MapEntity duplicateEntity;
					if (e2.Removed)
					{
						string identifier = "Submarine.SaveToXElement:Removed" + e2.Name;
						GameAnalyticsManager.ErrorSeverity errorSeverity = GameAnalyticsManager.ErrorSeverity.Error;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Attempted to save a removed entity (\"");
						defaultInterpolatedStringHandler.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler.AppendLiteral("\"). Duplicate ID: ");
						defaultInterpolatedStringHandler.AppendFormatted<bool>(savedEntities.ContainsKey((int)e2.ID));
						GameAnalyticsManager.AddErrorEventOnce(identifier, errorSeverity, defaultInterpolatedStringHandler.ToStringAndClear());
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(146, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Error while saving the submarine. Attempted to save a removed entity (\"");
						defaultInterpolatedStringHandler2.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler2.AppendLiteral(" (");
						defaultInterpolatedStringHandler2.AppendFormatted<ushort>(e2.ID);
						defaultInterpolatedStringHandler2.AppendLiteral(")\"). The entity will not be saved to avoid corrupting the submarine file.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					}
					else if (savedEntities.TryGetValue((int)e2.ID, out duplicateEntity))
					{
						string identifier2 = "Submarine.SaveToXElement:DuplicateId" + e2.Name;
						GameAnalyticsManager.ErrorSeverity errorSeverity2 = GameAnalyticsManager.ErrorSeverity.Error;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(53, 2);
						defaultInterpolatedStringHandler3.AppendLiteral("Attempted to save an entity with a duplicate ID (");
						defaultInterpolatedStringHandler3.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler3.AppendLiteral(", ");
						defaultInterpolatedStringHandler3.AppendFormatted(duplicateEntity.Name);
						defaultInterpolatedStringHandler3.AppendLiteral(").");
						GameAnalyticsManager.AddErrorEventOnce(identifier2, errorSeverity2, defaultInterpolatedStringHandler3.ToStringAndClear());
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(142, 3);
						defaultInterpolatedStringHandler4.AppendLiteral("Error while saving the submarine. The entity \"");
						defaultInterpolatedStringHandler4.AppendFormatted(e2.Name);
						defaultInterpolatedStringHandler4.AppendLiteral("\" has the same ID as \"");
						defaultInterpolatedStringHandler4.AppendFormatted(duplicateEntity.Name);
						defaultInterpolatedStringHandler4.AppendLiteral("\" (");
						defaultInterpolatedStringHandler4.AppendFormatted<ushort>(e2.ID);
						defaultInterpolatedStringHandler4.AppendLiteral("). The entity will not be saved to avoid corrupting the submarine file.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						Item item2 = e2 as Item;
						if (item2 != null)
						{
							if (item2.FindParentInventory((Inventory inv) => inv is CharacterInventory) != null || e2.Submarine != this)
							{
								continue;
							}
							if (item2.RootContainer != null && item2.RootContainer.Submarine != this)
							{
								continue;
							}
						}
						else if (e2.Submarine != this)
						{
							continue;
						}
						e2.Save(element);
						savedEntities.Add((int)e2.ID, e2);
					}
				}
			}
			this.Info.CheckSubsLeftBehind(element);
		}

		// Token: 0x06000AA3 RID: 2723 RVA: 0x00068CF8 File Offset: 0x00066EF8
		public bool TrySaveAs(string filePath, MemoryStream previewImage = null)
		{
			SubmarineInfo newInfo = new SubmarineInfo(this)
			{
				Type = this.Info.Type,
				FilePath = filePath,
				OutpostModuleInfo = ((this.Info.OutpostModuleInfo != null) ? new OutpostModuleInfo(this.Info.OutpostModuleInfo) : null),
				BeaconStationInfo = ((this.Info.BeaconStationInfo != null) ? new BeaconStationInfo(this.Info.BeaconStationInfo) : null),
				EnemySubmarineInfo = ((this.Info.EnemySubmarineInfo != null) ? new EnemySubmarineInfo(this.Info.EnemySubmarineInfo) : null),
				WreckInfo = ((this.Info.WreckInfo != null) ? new WreckInfo(this.Info.WreckInfo) : null),
				Name = Barotrauma.IO.Path.GetFileNameWithoutExtension(filePath)
			};
			this.Info.Dispose();
			this.Info = newInfo;
			try
			{
				newInfo.SaveAs(filePath, previewImage);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving submarine \"" + filePath + "\" failed!", e, null, false, false);
				return false;
			}
			return true;
		}

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000AA4 RID: 2724 RVA: 0x00068E18 File Offset: 0x00067018
		// (set) Token: 0x06000AA5 RID: 2725 RVA: 0x00068E1F File Offset: 0x0006701F
		public static bool Unloading { get; private set; }

		// Token: 0x06000AA6 RID: 2726 RVA: 0x00068E28 File Offset: 0x00067028
		public static void Unload()
		{
			if (Submarine.Unloading)
			{
				DebugConsole.AddWarning("Called Unload when already unloading.", null);
				return;
			}
			Submarine.Unloading = true;
			try
			{
				List<Submarine> _loaded = new List<Submarine>(Submarine.loaded);
				foreach (Submarine sub in _loaded)
				{
					sub.Remove();
					if (sub.Info.LazyLoad)
					{
						sub.Info.UnloadSubmarineElement();
					}
				}
				Submarine.loaded.Clear();
				Submarine.visibleEntities = null;
				if (GameMain.GameScreen.Cam != null)
				{
					GameMain.GameScreen.Cam.TargetPos = Vector2.Zero;
				}
				Entity.RemoveAll();
				if (Item.ItemList.Count > 0)
				{
					List<Item> items = new List<Item>(Item.ItemList);
					foreach (Item item in items)
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Error while unloading submarines - item \"",
							item.Name,
							"\" (ID:",
							item.ID.ToString(),
							") not removed"
						}), null, null, false, false);
						try
						{
							item.Remove();
						}
						catch (Exception e)
						{
							DebugConsole.ThrowError("Error while removing \"" + item.Name + "\"!", e, null, false, false);
						}
					}
					Item.ItemList.Clear();
				}
				Ragdoll.RemoveAll();
				PhysicsBody.RemoveAll();
				StatusEffect.StopAll();
				GameMain.World = null;
				Powered.Grids.Clear();
				Powered.ChangedConnections.Clear();
				GC.Collect();
			}
			finally
			{
				Submarine.Unloading = false;
			}
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x00069038 File Offset: 0x00067238
		public override void Remove()
		{
			base.Remove();
			SubmarineBody submarineBody = this.subBody;
			if (submarineBody != null)
			{
				submarineBody.Remove();
			}
			this.subBody = null;
			List<PathNode> list = this.outdoorNodes;
			if (list != null)
			{
				list.Clear();
			}
			this.outdoorNodes = null;
			this.obstructedNodes.Clear();
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				CampaignMode campaign = gameSession.Campaign;
				if (campaign != null)
				{
					UpgradeManager upgradeManager = campaign.UpgradeManager;
					if (upgradeManager != null)
					{
						NamedEvent<UpgradeManager> onUpgradesChanged = upgradeManager.OnUpgradesChanged;
						if (onUpgradesChanged != null)
						{
							onUpgradesChanged.TryDeregister(this.upgradeEventIdentifier);
						}
					}
				}
			}
			if (this.entityGrid != null)
			{
				Hull.EntityGrids.Remove(this.entityGrid);
				this.entityGrid = null;
			}
			Submarine.visibleEntities = null;
			Submarine.bodyDist.Clear();
			Submarine.bodies.Clear();
			if (Submarine.MainSub == this)
			{
				Submarine.MainSub = null;
			}
			if (Submarine.MainSubs[1] == this)
			{
				Submarine.MainSubs[1] = null;
			}
			Dictionary<Submarine, DockingPort> connectedDockingPorts = this.ConnectedDockingPorts;
			if (connectedDockingPorts != null)
			{
				connectedDockingPorts.Clear();
			}
			Powered.ChangedConnections.Clear();
			Powered.Grids.Clear();
			Submarine.loaded.Remove(this);
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x00069148 File Offset: 0x00067348
		public void Dispose()
		{
			this.Remove();
		}

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000AA9 RID: 2729 RVA: 0x00069150 File Offset: 0x00067350
		private List<PathNode> OutdoorNodes
		{
			get
			{
				if (this.outdoorNodes == null)
				{
					this.GenerateOutdoorNodes();
				}
				return this.outdoorNodes;
			}
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x00069168 File Offset: 0x00067368
		private void GenerateOutdoorNodes()
		{
			List<WayPoint> waypoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.SpawnType == SpawnType.Path && wp.Submarine == this && wp.CurrentHull == null);
			this.outdoorNodes = PathNode.GenerateNodes(waypoints, false);
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x0006919C File Offset: 0x0006739C
		public void DisableObstructedWayPoints()
		{
			foreach (PathNode node in this.OutdoorNodes)
			{
				if (node != null && node.Waypoint != null)
				{
					WayPoint wp = node.Waypoint;
					if (!wp.IsObstructed)
					{
						foreach (PathNode connection in node.connections)
						{
							WayPoint connectedWp = connection.Waypoint;
							if (!connectedWp.IsObstructed)
							{
								Vector2 start = ConvertUnits.ToSimUnits(wp.WorldPosition);
								Vector2 end = ConvertUnits.ToSimUnits(connectedWp.WorldPosition);
								Body body = Submarine.PickBody(start, end, null, new Category?(Category.Cat8), true, null, false);
								if (body != null)
								{
									connectedWp.IsObstructed = true;
									wp.IsObstructed = true;
									break;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x000692A8 File Offset: 0x000674A8
		public void DisableObstructedWayPoints(Submarine otherSub)
		{
			if (otherSub == null)
			{
				return;
			}
			if (otherSub == this)
			{
				return;
			}
			foreach (PathNode node in this.OutdoorNodes)
			{
				if (node != null && node.Waypoint != null)
				{
					WayPoint wp = node.Waypoint;
					if (!wp.IsObstructed)
					{
						foreach (PathNode connection in node.connections)
						{
							WayPoint connectedWp = connection.Waypoint;
							if (!connectedWp.IsObstructed && connectedWp.Ladders == null)
							{
								Hull h = wp.CurrentHull;
								bool isObstructed = h != null && h.Submarine != this;
								if (!isObstructed)
								{
									Vector2 start = ConvertUnits.ToSimUnits(wp.WorldPosition) - otherSub.SimPosition;
									Vector2 end = ConvertUnits.ToSimUnits(connectedWp.WorldPosition) - otherSub.SimPosition;
									Body body = Submarine.PickBody(start, end, null, new Category?(Category.Cat1), true, null, true);
									if (body != null)
									{
										Structure wall = body.UserData as Structure;
										if ((wall != null && !wall.IsPlatform) || (body.UserData is Item && body.FixtureList[0].CollisionCategories.HasFlag(Category.Cat1)))
										{
											isObstructed = true;
										}
									}
								}
								if (isObstructed)
								{
									connectedWp.IsObstructed = true;
									wp.IsObstructed = true;
									HashSet<PathNode> nodes;
									if (!this.obstructedNodes.TryGetValue(otherSub, out nodes))
									{
										nodes = new HashSet<PathNode>();
										this.obstructedNodes.Add(otherSub, nodes);
									}
									nodes.Add(node);
									nodes.Add(connection);
									break;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000AAD RID: 2733 RVA: 0x000694B0 File Offset: 0x000676B0
		public void EnableObstructedWaypoints(Submarine otherSub)
		{
			HashSet<PathNode> nodes;
			if (this.obstructedNodes.TryGetValue(otherSub, out nodes))
			{
				nodes.ForEach(delegate(PathNode n)
				{
					n.Waypoint.IsObstructed = false;
				});
				nodes.Clear();
				this.obstructedNodes.Remove(otherSub);
			}
		}

		// Token: 0x06000AAE RID: 2734 RVA: 0x00069505 File Offset: 0x00067705
		public void RefreshOutdoorNodes()
		{
			this.OutdoorNodes.ForEach(delegate(PathNode n)
			{
				if (n != null)
				{
					WayPoint waypoint = n.Waypoint;
					if (waypoint == null)
					{
						return;
					}
					waypoint.FindHull();
				}
			});
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x00069534 File Offset: 0x00067734
		public Item FindContainerFor(Item item, bool onlyPrimary, bool checkTransferConditions = false, bool allowConnectedSubs = false)
		{
			HashSet<Submarine> connectedSubs = (from s in this.GetConnectedSubs()
			where s.Info.Type == SubmarineType.Player
			select s).ToHashSet<Submarine>();
			Item selectedContainer = null;
			foreach (Item potentialContainer in Item.ItemList)
			{
				if (!potentialContainer.Removed && !potentialContainer.NonInteractable && !potentialContainer.IsHidden)
				{
					if (allowConnectedSubs)
					{
						if (!connectedSubs.Contains(potentialContainer.Submarine))
						{
							continue;
						}
					}
					else if (potentialContainer.Submarine != this)
					{
						continue;
					}
					if (potentialContainer != item && potentialContainer.Condition > 0f && potentialContainer.OwnInventory != null && potentialContainer.GetRootInventoryOwner() == potentialContainer)
					{
						ItemContainer container = potentialContainer.GetComponent<ItemContainer>();
						bool flag;
						bool isPreferencesDefined;
						bool isSecondary;
						if (container != null && potentialContainer.OwnInventory.CanBePut(item) && container.ShouldBeContained(item, out flag) && item.Prefab.IsContainerPreferred(item, container, out isPreferencesDefined, out isSecondary, false, checkTransferConditions) && isPreferencesDefined && (!onlyPrimary || !isSecondary))
						{
							if (potentialContainer.Submarine == this && !isSecondary)
							{
								return potentialContainer;
							}
							selectedContainer = potentialContainer;
						}
					}
				}
			}
			return selectedContainer;
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x00069678 File Offset: 0x00067878
		public static Vector2 GetRelativeSimPosition(ISpatialEntity from, ISpatialEntity to, Vector2? targetWorldPos = null)
		{
			if (targetWorldPos == null)
			{
				return Submarine.GetRelativeSimPosition(to.SimPosition, from.Submarine, to.Submarine);
			}
			return Submarine.GetRelativeSimPositionFromWorldPosition(targetWorldPos.Value, from.Submarine, to.Submarine);
		}

		// Token: 0x06000AB1 RID: 2737 RVA: 0x000696B4 File Offset: 0x000678B4
		public static Vector2 GetRelativeSimPositionFromWorldPosition(Vector2 targetWorldPos, Submarine fromSub, Submarine toSub)
		{
			Vector2 worldPos = targetWorldPos;
			if (toSub != null)
			{
				worldPos -= toSub.Position;
			}
			return Submarine.GetRelativeSimPosition(ConvertUnits.ToSimUnits(worldPos), fromSub, toSub);
		}

		// Token: 0x06000AB2 RID: 2738 RVA: 0x000696E0 File Offset: 0x000678E0
		public static Vector2 GetRelativeSimPosition(Vector2 targetSimPos, Submarine fromSub, Submarine toSub)
		{
			Vector2 targetPos = targetSimPos;
			if (fromSub == null && toSub != null)
			{
				targetPos += toSub.SimPosition;
			}
			else if (fromSub != null && toSub == null)
			{
				targetPos -= fromSub.SimPosition;
			}
			else if (fromSub != toSub && fromSub != null && toSub != null)
			{
				Vector2 diff = fromSub.SimPosition - toSub.SimPosition;
				targetPos -= diff;
			}
			return targetPos;
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x000697C0 File Offset: 0x000679C0
		[CompilerGenerated]
		internal static Vector2 <FindSpawnPos>g__GetHorizontalLimits|137_0(Vector2 spawnPos, float maxHorizontalMoveAmount, float minHeight, int verticalMoveDir, int padding, ref Submarine.<>c__DisplayClass137_0 A_5)
		{
			Vector2 refPos = spawnPos - Vector2.UnitY * minHeight * 0.5f * (float)Math.Sign(verticalMoveDir);
			float minX = float.MinValue;
			float maxX = float.MaxValue;
			foreach (VoronoiCell cell in Level.Loaded.GetAllCells())
			{
				foreach (GraphEdge e in cell.Edges)
				{
					if ((e.Point1.Y >= refPos.Y - minHeight * 0.5f || e.Point2.Y >= refPos.Y - minHeight * 0.5f) && (e.Point1.Y <= refPos.Y + minHeight * 0.5f || e.Point2.Y <= refPos.Y + minHeight * 0.5f))
					{
						if (cell.Site.Coord.X < (double)refPos.X)
						{
							minX = Math.Max(minX, Math.Max(e.Point1.X, e.Point2.X));
						}
						else
						{
							maxX = Math.Min(maxX, Math.Min(e.Point1.X, e.Point2.X));
						}
					}
				}
			}
			foreach (Ruin ruin in Level.Loaded.Ruins)
			{
				if (Math.Abs((float)ruin.Area.Center.Y - refPos.Y) <= (minHeight + (float)ruin.Area.Height) * 0.5f)
				{
					if ((float)ruin.Area.Center.X < refPos.X)
					{
						minX = Math.Max(minX, (float)(ruin.Area.Right + padding));
					}
					else
					{
						maxX = Math.Min(maxX, (float)(ruin.Area.X - padding));
					}
				}
			}
			minX += A_5.subDockingPortOffset;
			maxX += A_5.subDockingPortOffset;
			return new Vector2(Math.Max(Math.Max(minX, spawnPos.X - maxHorizontalMoveAmount - (float)padding), 0f), Math.Min(Math.Min(maxX, spawnPos.X + maxHorizontalMoveAmount + (float)padding), (float)Level.Loaded.Size.X));
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x00069AB8 File Offset: 0x00067CB8
		[CompilerGenerated]
		internal static Vector2 <FindSpawnPos>g__ClampToHorizontalLimits|137_1(Vector2 spawnPos, Vector2 limits, ref Submarine.<>c__DisplayClass137_0 A_2)
		{
			if (limits.X >= 0f || limits.Y <= (float)Level.Loaded.Size.X)
			{
				if (limits.X < 0f)
				{
					spawnPos.X = limits.Y - (float)A_2.minWidth * 0.5f - 100f + A_2.subDockingPortOffset;
				}
				else if (limits.Y > (float)Level.Loaded.Size.X)
				{
					spawnPos.X = limits.X + (float)A_2.minWidth * 0.5f + 100f + A_2.subDockingPortOffset;
				}
				else
				{
					spawnPos.X = (limits.X + limits.Y) / 2f + A_2.subDockingPortOffset;
				}
			}
			return spawnPos;
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x00069BD4 File Offset: 0x00067DD4
		[CompilerGenerated]
		internal static void <SetLayerEnabled>g__SetItemHidden|161_0(Item item, bool isHidden)
		{
			foreach (Item containedItem in item.ContainedItems)
			{
				Submarine.<SetLayerEnabled>g__SetItemHidden|161_0(containedItem, isHidden);
			}
			foreach (ConnectionPanel connectionPanel in item.GetComponents<ConnectionPanel>())
			{
				foreach (Connection connection in connectionPanel.Connections)
				{
					foreach (Wire wire in connection.Wires)
					{
						wire.Item.IsLayerHidden = isHidden;
					}
				}
			}
		}

		// Token: 0x0400048A RID: 1162
		public CharacterTeamType TeamID;

		// Token: 0x0400048B RID: 1163
		public static readonly Vector2 HiddenSubStartPosition = new Vector2(-50000f, 10000f);

		// Token: 0x0400048E RID: 1166
		public static bool LockX;

		// Token: 0x0400048F RID: 1167
		public static bool LockY;

		// Token: 0x04000490 RID: 1168
		public static readonly Vector2 GridSize = new Vector2(16f, 16f);

		// Token: 0x04000491 RID: 1169
		public static readonly Submarine[] MainSubs = new Submarine[2];

		// Token: 0x04000492 RID: 1170
		private static readonly List<Submarine> loaded = new List<Submarine>();

		// Token: 0x04000493 RID: 1171
		private readonly Identifier upgradeEventIdentifier;

		// Token: 0x04000494 RID: 1172
		private static List<MapEntity> visibleEntities;

		// Token: 0x04000495 RID: 1173
		private SubmarineBody subBody;

		// Token: 0x04000496 RID: 1174
		public readonly Dictionary<Submarine, DockingPort> ConnectedDockingPorts;

		// Token: 0x04000497 RID: 1175
		private static Vector2 lastPickedPosition;

		// Token: 0x04000498 RID: 1176
		private static float lastPickedFraction;

		// Token: 0x04000499 RID: 1177
		private static Fixture lastPickedFixture;

		// Token: 0x0400049A RID: 1178
		private static Vector2 lastPickedNormal;

		// Token: 0x0400049B RID: 1179
		private Vector2 prevPosition;

		// Token: 0x0400049C RID: 1180
		private float networkUpdateTimer;

		// Token: 0x0400049D RID: 1181
		private EntityGrid entityGrid;

		// Token: 0x0400049E RID: 1182
		public bool ShowSonarMarker = true;

		// Token: 0x040004A1 RID: 1185
		public List<WayPoint> ForcedOutpostModuleWayPoints = new List<WayPoint>();

		// Token: 0x040004A2 RID: 1186
		private float? realWorldCrushDepth;

		// Token: 0x040004A4 RID: 1188
		private int? submarineSpecificIDTag;

		// Token: 0x040004A5 RID: 1189
		private readonly List<WayPoint> exitPoints = new List<WayPoint>();

		// Token: 0x040004A6 RID: 1190
		private float ballastFloraTimer;

		// Token: 0x040004AA RID: 1194
		private static readonly HashSet<Submarine> checkSubmarineBorders = new HashSet<Submarine>();

		// Token: 0x040004AB RID: 1195
		private readonly HashSet<Submarine> connectedSubs;

		// Token: 0x040004AC RID: 1196
		private static readonly Dictionary<Body, float> bodyDist = new Dictionary<Body, float>();

		// Token: 0x040004AD RID: 1197
		private static readonly List<Body> bodies = new List<Body>();

		// Token: 0x040004AE RID: 1198
		private bool flippedX;

		// Token: 0x040004B0 RID: 1200
		private List<PathNode> outdoorNodes;

		// Token: 0x040004B1 RID: 1201
		private readonly Dictionary<Submarine, HashSet<PathNode>> obstructedNodes = new Dictionary<Submarine, HashSet<PathNode>>();

		// Token: 0x02000725 RID: 1829
		public readonly struct SetLayerEnabledEventData : NetEntityEvent.IData
		{
			// Token: 0x060050EC RID: 20716 RVA: 0x001E812A File Offset: 0x001E632A
			public SetLayerEnabledEventData(Identifier layer, bool enabled)
			{
				this.Layer = layer;
				this.Enabled = enabled;
			}

			// Token: 0x04002BED RID: 11245
			public readonly Identifier Layer;

			// Token: 0x04002BEE RID: 11246
			public readonly bool Enabled;
		}
	}
}

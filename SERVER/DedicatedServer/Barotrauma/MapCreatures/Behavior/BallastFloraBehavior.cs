using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.MapCreatures.Behavior
{
	// Token: 0x020003D2 RID: 978
	[NullableContext(1)]
	[Nullable(0)]
	internal class BallastFloraBehavior : ISerializableEntity
	{
		// Token: 0x0600382D RID: 14381 RVA: 0x00177D8C File Offset: 0x00175F8C
		[NullableContext(0)]
		public void ServerWrite(IWriteMessage msg, BallastFloraBehavior.IEventData eventData)
		{
			msg.WriteByte((byte)eventData.NetworkHeader);
			if (!(eventData is BallastFloraBehavior.SpawnEventData))
			{
				if (!(eventData is BallastFloraBehavior.KillEventData))
				{
					if (eventData is BallastFloraBehavior.BranchCreateEventData)
					{
						BallastFloraBehavior.BranchCreateEventData branchCreateEventData = (BallastFloraBehavior.BranchCreateEventData)eventData;
						this.ServerWriteBranchGrowth(msg, branchCreateEventData.NewBranch, branchCreateEventData.Parent.ID);
					}
					else if (eventData is BallastFloraBehavior.BranchDamageEventData)
					{
						BallastFloraBehavior.BranchDamageEventData branchDamageEventData = (BallastFloraBehavior.BranchDamageEventData)eventData;
						this.ServerWriteBranchDamage(msg, branchDamageEventData.Branch);
					}
					else if (eventData is BallastFloraBehavior.InfectEventData)
					{
						BallastFloraBehavior.InfectEventData infectEventData = (BallastFloraBehavior.InfectEventData)eventData;
						this.ServerWriteInfect(msg, infectEventData.Item.ID, infectEventData.Infect, infectEventData.Infector);
					}
					else if (eventData is BallastFloraBehavior.BranchRemoveEventData)
					{
						BallastFloraBehavior.BranchRemoveEventData branchRemoveEventData = (BallastFloraBehavior.BranchRemoveEventData)eventData;
						this.ServerWriteBranchRemove(msg, branchRemoveEventData.Branch);
					}
				}
			}
			else
			{
				this.ServerWriteSpawn(msg);
			}
			msg.WriteSingle(this.PowerConsumptionTimer);
		}

		// Token: 0x0600382E RID: 14382 RVA: 0x00177E68 File Offset: 0x00176068
		[NullableContext(0)]
		private void ServerWriteSpawn(IWriteMessage msg)
		{
			msg.WriteIdentifier(this.Prefab.Identifier);
			msg.WriteSingle(this.Offset.X);
			msg.WriteSingle(this.Offset.Y);
		}

		// Token: 0x0600382F RID: 14383 RVA: 0x00177EA0 File Offset: 0x001760A0
		[NullableContext(0)]
		private void ServerWriteBranchGrowth(IWriteMessage msg, BallastFloraBranch branch, int parentId = -1)
		{
			Vector2 position = branch.Position;
			float num;
			float num2;
			position.Deconstruct(out num, out num2);
			float x = num;
			float y = num2;
			msg.WriteInt32(parentId);
			msg.WriteInt32(branch.ID);
			msg.WriteBoolean(branch.IsRootGrowth);
			msg.WriteRangedInteger((int)((byte)branch.Type), 0, 15);
			msg.WriteRangedInteger((int)((byte)branch.Sides), 0, 15);
			msg.WriteRangedInteger(branch.FlowerConfig.Serialize(), 0, 4095);
			msg.WriteRangedInteger(branch.LeafConfig.Serialize(), 0, 4095);
			msg.WriteUInt16((ushort)branch.MaxHealth);
			msg.WriteInt32((int)(x / (float)VineTile.Size));
			msg.WriteInt32((int)(y / (float)VineTile.Size));
			msg.WriteInt32((branch.ParentBranch == null) ? -1 : this.Branches.IndexOf(branch.ParentBranch));
		}

		// Token: 0x06003830 RID: 14384 RVA: 0x00177F7E File Offset: 0x0017617E
		[NullableContext(0)]
		private void ServerWriteBranchDamage(IWriteMessage msg, BallastFloraBranch branch)
		{
			msg.WriteInt32(branch.ID);
			msg.WriteSingle(branch.Health);
		}

		// Token: 0x06003831 RID: 14385 RVA: 0x00177F98 File Offset: 0x00176198
		[NullableContext(0)]
		private void ServerWriteInfect(IWriteMessage msg, ushort itemID, BallastFloraBehavior.InfectEventData.InfectState infect, BallastFloraBranch infector = null)
		{
			msg.WriteUInt16(itemID);
			msg.WriteBoolean(infect == BallastFloraBehavior.InfectEventData.InfectState.Yes);
			if (infect == BallastFloraBehavior.InfectEventData.InfectState.Yes)
			{
				msg.WriteInt32((infector != null) ? infector.ID : -1);
			}
		}

		// Token: 0x06003832 RID: 14386 RVA: 0x00177FC2 File Offset: 0x001761C2
		[NullableContext(0)]
		private void ServerWriteBranchRemove(IWriteMessage msg, BallastFloraBranch branch)
		{
			msg.WriteInt32(branch.ID);
		}

		// Token: 0x06003833 RID: 14387 RVA: 0x00177FD0 File Offset: 0x001761D0
		[NullableContext(0)]
		public void CreateNetworkMessage(BallastFloraBehavior.IEventData extraData)
		{
			GameMain.Server.CreateEntityEvent(this.Parent, new Hull.BallastFloraEventData(this, extraData));
		}

		// Token: 0x17000F66 RID: 3942
		// (get) Token: 0x06003834 RID: 14388 RVA: 0x00177FEE File Offset: 0x001761EE
		public static IEnumerable<BallastFloraBehavior> EntityList
		{
			get
			{
				return BallastFloraBehavior._entityList;
			}
		}

		// Token: 0x17000F67 RID: 3943
		// (get) Token: 0x06003835 RID: 14389 RVA: 0x00177FF5 File Offset: 0x001761F5
		// (set) Token: 0x06003836 RID: 14390 RVA: 0x00177FFD File Offset: 0x001761FD
		[Serialize(0.25f, IsPropertySaveable.Yes, "Scale of the branches.", "", false)]
		public float BaseBranchScale { get; set; }

		// Token: 0x17000F68 RID: 3944
		// (get) Token: 0x06003837 RID: 14391 RVA: 0x00178006 File Offset: 0x00176206
		// (set) Token: 0x06003838 RID: 14392 RVA: 0x0017800E File Offset: 0x0017620E
		[Serialize(0.25f, IsPropertySaveable.Yes, "Scale of the flowers.", "", false)]
		public float BaseFlowerScale { get; set; }

		// Token: 0x17000F69 RID: 3945
		// (get) Token: 0x06003839 RID: 14393 RVA: 0x00178017 File Offset: 0x00176217
		// (set) Token: 0x0600383A RID: 14394 RVA: 0x0017801F File Offset: 0x0017621F
		[Serialize(0.5f, IsPropertySaveable.Yes, "Scale of the leaves.", "", false)]
		public float BaseLeafScale { get; set; }

		// Token: 0x17000F6A RID: 3946
		// (get) Token: 0x0600383B RID: 14395 RVA: 0x00178028 File Offset: 0x00176228
		// (set) Token: 0x0600383C RID: 14396 RVA: 0x00178030 File Offset: 0x00176230
		[Serialize(0.33f, IsPropertySaveable.Yes, "Chance for a flower to appear on a branch.", "", false)]
		public float FlowerProbability { get; set; }

		// Token: 0x17000F6B RID: 3947
		// (get) Token: 0x0600383D RID: 14397 RVA: 0x00178039 File Offset: 0x00176239
		// (set) Token: 0x0600383E RID: 14398 RVA: 0x00178041 File Offset: 0x00176241
		[Serialize(0.7f, IsPropertySaveable.Yes, "Chance for leaves to appear on a branch.", "", false)]
		public float LeafProbability { get; set; }

		// Token: 0x17000F6C RID: 3948
		// (get) Token: 0x0600383F RID: 14399 RVA: 0x0017804A File Offset: 0x0017624A
		// (set) Token: 0x06003840 RID: 14400 RVA: 0x00178052 File Offset: 0x00176252
		[Serialize(3f, IsPropertySaveable.Yes, "Delay between pulses.", "", false)]
		public float PulseDelay { get; set; }

		// Token: 0x17000F6D RID: 3949
		// (get) Token: 0x06003841 RID: 14401 RVA: 0x0017805B File Offset: 0x0017625B
		// (set) Token: 0x06003842 RID: 14402 RVA: 0x00178063 File Offset: 0x00176263
		[Serialize(3f, IsPropertySaveable.Yes, "How fast the flower inflates during a pulse.", "", false)]
		public float PulseInflateSpeed { get; set; }

		// Token: 0x17000F6E RID: 3950
		// (get) Token: 0x06003843 RID: 14403 RVA: 0x0017806C File Offset: 0x0017626C
		// (set) Token: 0x06003844 RID: 14404 RVA: 0x00178074 File Offset: 0x00176274
		[Serialize(1f, IsPropertySaveable.Yes, "How fast the flower deflates.", "", false)]
		public float PulseDeflateSpeed { get; set; }

		// Token: 0x17000F6F RID: 3951
		// (get) Token: 0x06003845 RID: 14405 RVA: 0x0017807D File Offset: 0x0017627D
		// (set) Token: 0x06003846 RID: 14406 RVA: 0x00178085 File Offset: 0x00176285
		[Serialize(32, IsPropertySaveable.Yes, "How many vines must grow before the plant breaks through the wall.", "", false)]
		public int BreakthroughPoint { get; set; }

		// Token: 0x17000F70 RID: 3952
		// (get) Token: 0x06003847 RID: 14407 RVA: 0x0017808E File Offset: 0x0017628E
		// (set) Token: 0x06003848 RID: 14408 RVA: 0x00178096 File Offset: 0x00176296
		[Serialize(false, IsPropertySaveable.Yes, "Has the plant grown large enough to expose itself.", "", false)]
		public bool HasBrokenThrough { get; set; }

		// Token: 0x17000F71 RID: 3953
		// (get) Token: 0x06003849 RID: 14409 RVA: 0x0017809F File Offset: 0x0017629F
		// (set) Token: 0x0600384A RID: 14410 RVA: 0x001780A7 File Offset: 0x001762A7
		[Serialize(300, IsPropertySaveable.Yes, "How far the ballast flora can detect items from.", "", false)]
		public int Sight { get; set; }

		// Token: 0x17000F72 RID: 3954
		// (get) Token: 0x0600384B RID: 14411 RVA: 0x001780B0 File Offset: 0x001762B0
		// (set) Token: 0x0600384C RID: 14412 RVA: 0x001780B8 File Offset: 0x001762B8
		[Serialize(100, IsPropertySaveable.Yes, "How much health the branches have.", "", false)]
		public int BranchHealth { get; set; }

		// Token: 0x17000F73 RID: 3955
		// (get) Token: 0x0600384D RID: 14413 RVA: 0x001780C1 File Offset: 0x001762C1
		// (set) Token: 0x0600384E RID: 14414 RVA: 0x001780C9 File Offset: 0x001762C9
		[Serialize(400, IsPropertySaveable.Yes, "How much health the root has.", "", false)]
		public int RootHealth { get; set; }

		// Token: 0x17000F74 RID: 3956
		// (get) Token: 0x0600384F RID: 14415 RVA: 0x001780D2 File Offset: 0x001762D2
		// (set) Token: 0x06003850 RID: 14416 RVA: 0x001780DA File Offset: 0x001762DA
		[Serialize(0.00025f, IsPropertySaveable.Yes, "How fast the root's health regenerates per each grown branch.", "", false)]
		public float HealthRegenPerBranch { get; set; }

		// Token: 0x17000F75 RID: 3957
		// (get) Token: 0x06003851 RID: 14417 RVA: 0x001780E3 File Offset: 0x001762E3
		// (set) Token: 0x06003852 RID: 14418 RVA: 0x001780EB File Offset: 0x001762EB
		[Serialize(30, IsPropertySaveable.Yes, "How far away from the root branches can regenerate health (in number of branches). The amount of regen decreases lineary further from the root.", "", false)]
		public int MaxBranchHealthRegenDistance { get; set; }

		// Token: 0x17000F76 RID: 3958
		// (get) Token: 0x06003853 RID: 14419 RVA: 0x001780F4 File Offset: 0x001762F4
		// (set) Token: 0x06003854 RID: 14420 RVA: 0x001780FC File Offset: 0x001762FC
		[Serialize("255,255,255,255", IsPropertySaveable.Yes, "", "", false)]
		public Color RootColor { get; set; }

		// Token: 0x17000F77 RID: 3959
		// (get) Token: 0x06003855 RID: 14421 RVA: 0x00178105 File Offset: 0x00176305
		// (set) Token: 0x06003856 RID: 14422 RVA: 0x0017810D File Offset: 0x0017630D
		[Serialize(300f, IsPropertySaveable.Yes, "How much power the ballast flora takes from junction boxes.", "", false)]
		public float PowerConsumptionMin { get; set; }

		// Token: 0x17000F78 RID: 3960
		// (get) Token: 0x06003857 RID: 14423 RVA: 0x00178116 File Offset: 0x00176316
		// (set) Token: 0x06003858 RID: 14424 RVA: 0x0017811E File Offset: 0x0017631E
		[Serialize(3000f, IsPropertySaveable.Yes, "How much the power drain spikes.", "", false)]
		public float PowerConsumptionMax { get; set; }

		// Token: 0x17000F79 RID: 3961
		// (get) Token: 0x06003859 RID: 14425 RVA: 0x00178127 File Offset: 0x00176327
		// (set) Token: 0x0600385A RID: 14426 RVA: 0x0017812F File Offset: 0x0017632F
		[Serialize(10f, IsPropertySaveable.Yes, "How long it takes for power drain to wind down.", "", false)]
		public float PowerConsumptionDuration { get; set; }

		// Token: 0x17000F7A RID: 3962
		// (get) Token: 0x0600385B RID: 14427 RVA: 0x00178138 File Offset: 0x00176338
		// (set) Token: 0x0600385C RID: 14428 RVA: 0x00178140 File Offset: 0x00176340
		[Serialize(250f, IsPropertySaveable.Yes, "How much power does it take to accelerate growth.", "", false)]
		public float PowerRequirement { get; set; }

		// Token: 0x17000F7B RID: 3963
		// (get) Token: 0x0600385D RID: 14429 RVA: 0x00178149 File Offset: 0x00176349
		// (set) Token: 0x0600385E RID: 14430 RVA: 0x00178151 File Offset: 0x00176351
		[Serialize(5f, IsPropertySaveable.Yes, "Maximum anger, anger increases when the plant gets damaged and increases growth speed.", "", false)]
		public float MaxAnger { get; set; }

		// Token: 0x17000F7C RID: 3964
		// (get) Token: 0x0600385F RID: 14431 RVA: 0x0017815A File Offset: 0x0017635A
		// (set) Token: 0x06003860 RID: 14432 RVA: 0x00178162 File Offset: 0x00176362
		[Serialize(10000f, IsPropertySaveable.Yes, "Maximum power buffer.", "", false)]
		public float MaxPowerCapacity { get; set; }

		// Token: 0x17000F7D RID: 3965
		// (get) Token: 0x06003861 RID: 14433 RVA: 0x0017816B File Offset: 0x0017636B
		// (set) Token: 0x06003862 RID: 14434 RVA: 0x00178173 File Offset: 0x00176373
		[Serialize("", IsPropertySaveable.Yes, "Item prefab that is spawned when threatened.", "", false)]
		public Identifier AttackItemPrefab { get; set; } = Identifier.Empty;

		// Token: 0x17000F7E RID: 3966
		// (get) Token: 0x06003863 RID: 14435 RVA: 0x0017817C File Offset: 0x0017637C
		// (set) Token: 0x06003864 RID: 14436 RVA: 0x00178184 File Offset: 0x00176384
		[Serialize(0.8f, IsPropertySaveable.Yes, "How resistant the ballast flora is to explosives before it blooms.", "", false)]
		public float ExplosionResistance { get; set; }

		// Token: 0x17000F7F RID: 3967
		// (get) Token: 0x06003865 RID: 14437 RVA: 0x0017818D File Offset: 0x0017638D
		// (set) Token: 0x06003866 RID: 14438 RVA: 0x00178195 File Offset: 0x00176395
		[Serialize(5f, IsPropertySaveable.Yes, "How much damage is taken from open fires.", "", false)]
		public float FireVulnerability { get; set; }

		// Token: 0x17000F80 RID: 3968
		// (get) Token: 0x06003867 RID: 14439 RVA: 0x0017819E File Offset: 0x0017639E
		// (set) Token: 0x06003868 RID: 14440 RVA: 0x001781A6 File Offset: 0x001763A6
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much resistance against fire is gained while submerged.", "", false)]
		public float SubmergedWaterResistance { get; set; }

		// Token: 0x17000F81 RID: 3969
		// (get) Token: 0x06003869 RID: 14441 RVA: 0x001781AF File Offset: 0x001763AF
		// (set) Token: 0x0600386A RID: 14442 RVA: 0x001781B7 File Offset: 0x001763B7
		[Serialize(0.8f, IsPropertySaveable.Yes, "What depth the branches will be drawn on.", "", false)]
		public float BranchDepth { get; set; }

		// Token: 0x17000F82 RID: 3970
		// (get) Token: 0x0600386B RID: 14443 RVA: 0x001781C0 File Offset: 0x001763C0
		// (set) Token: 0x0600386C RID: 14444 RVA: 0x001781C8 File Offset: 0x001763C8
		[Serialize("", IsPropertySaveable.Yes, "What sound to play when the ballast flora bursts through walls.", "", false)]
		public string BurstSound { get; set; } = "";

		// Token: 0x17000F83 RID: 3971
		// (get) Token: 0x0600386D RID: 14445 RVA: 0x001781D1 File Offset: 0x001763D1
		// (set) Token: 0x0600386E RID: 14446 RVA: 0x001781D9 File Offset: 0x001763D9
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

		// Token: 0x17000F84 RID: 3972
		// (get) Token: 0x0600386F RID: 14447 RVA: 0x001781ED File Offset: 0x001763ED
		// (set) Token: 0x06003870 RID: 14448 RVA: 0x001781F5 File Offset: 0x001763F5
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

		// Token: 0x17000F85 RID: 3973
		// (get) Token: 0x06003871 RID: 14449 RVA: 0x0017820E File Offset: 0x0017640E
		public string Name { get; } = "";

		// Token: 0x17000F86 RID: 3974
		// (get) Token: 0x06003872 RID: 14450 RVA: 0x00178216 File Offset: 0x00176416
		// (set) Token: 0x06003873 RID: 14451 RVA: 0x0017821E File Offset: 0x0017641E
		public Hull Parent { get; private set; }

		// Token: 0x17000F87 RID: 3975
		// (get) Token: 0x06003874 RID: 14452 RVA: 0x00178227 File Offset: 0x00176427
		// (set) Token: 0x06003875 RID: 14453 RVA: 0x0017822F File Offset: 0x0017642F
		public BallastFloraPrefab Prefab { get; private set; }

		// Token: 0x17000F88 RID: 3976
		// (get) Token: 0x06003876 RID: 14454 RVA: 0x00178238 File Offset: 0x00176438
		// (set) Token: 0x06003877 RID: 14455 RVA: 0x00178240 File Offset: 0x00176440
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06003878 RID: 14456 RVA: 0x0017824C File Offset: 0x0017644C
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

		// Token: 0x06003879 RID: 14457 RVA: 0x001783A4 File Offset: 0x001765A4
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

		// Token: 0x0600387A RID: 14458 RVA: 0x001783F1 File Offset: 0x001765F1
		public Vector2 GetWorldPosition()
		{
			return this.Parent.WorldPosition + this.Offset;
		}

		// Token: 0x0600387B RID: 14459 RVA: 0x0017840C File Offset: 0x0017660C
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

		// Token: 0x0600387C RID: 14460 RVA: 0x00178508 File Offset: 0x00176708
		private void LoadPrefab(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "branchsprite") && !(a == "hiddenflowersprite"))
				{
					if (!(a == "flowersprite"))
					{
						if (!(a == "leafsprite"))
						{
							if (a == "targets")
							{
								this.LoadTargets(subElement);
							}
						}
						else
						{
							this.leafVariants++;
						}
					}
					else
					{
						this.flowerVariants++;
					}
				}
			}
		}

		// Token: 0x0600387D RID: 14461 RVA: 0x001785D0 File Offset: 0x001767D0
		public void LoadTargets(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				this.Targets.Add(new BallastFloraBehavior.AITarget(subElement));
			}
		}

		// Token: 0x0600387E RID: 14462 RVA: 0x00178628 File Offset: 0x00176828
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

		// Token: 0x0600387F RID: 14463 RVA: 0x00178A14 File Offset: 0x00176C14
		public void LoadSave(XElement element, IdRemap idRemap)
		{
			BallastFloraBehavior.<>c__DisplayClass180_0 CS$<>8__locals1;
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
					this.<LoadSave>g__LoadBranch|180_1(subElement, idRemap, ref CS$<>8__locals1);
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

		// Token: 0x06003880 RID: 14464 RVA: 0x00178C08 File Offset: 0x00176E08
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
							predicate = (BallastFloraBehavior.<>O.<0>__HasAcidEmitter = new Func<BallastFloraBranch, bool>(BallastFloraBehavior.<Update>g__HasAcidEmitter|181_1));
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

		// Token: 0x06003881 RID: 14465 RVA: 0x001790C8 File Offset: 0x001772C8
		private void UpdateDamage(float deltaTime)
		{
			this.damageUpdateTimer -= deltaTime;
			if (this.damageUpdateTimer > 0f)
			{
				return;
			}
			int messages = 0;
			foreach (BallastFloraBranch branch in this.Branches)
			{
				if (Math.Abs(branch.AccumulatedDamage) > 1f)
				{
					this.CreateNetworkMessage(new BallastFloraBehavior.BranchDamageEventData(branch));
					branch.AccumulatedDamage = 0f;
					messages++;
					if (messages > 10)
					{
						break;
					}
				}
			}
			this.damageUpdateTimer = 1f;
		}

		// Token: 0x06003882 RID: 14466 RVA: 0x00179178 File Offset: 0x00177378
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

		// Token: 0x06003883 RID: 14467 RVA: 0x00179324 File Offset: 0x00177524
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

		// Token: 0x06003884 RID: 14468 RVA: 0x0017948C File Offset: 0x0017768C
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

		// Token: 0x06003885 RID: 14469 RVA: 0x00179558 File Offset: 0x00177758
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

		// Token: 0x06003886 RID: 14470 RVA: 0x001795BA File Offset: 0x001777BA
		public void SetHull(BallastFloraBranch branch)
		{
			branch.CurrentHull = Hull.FindHull(this.GetWorldPosition() + branch.Position, this.Parent, true, true);
		}

		// Token: 0x06003887 RID: 14471 RVA: 0x001795E0 File Offset: 0x001777E0
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

		// Token: 0x06003888 RID: 14472 RVA: 0x001796A8 File Offset: 0x001778A8
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

		// Token: 0x06003889 RID: 14473 RVA: 0x001796EC File Offset: 0x001778EC
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
			this.CreateNetworkMessage(new BallastFloraBehavior.BranchCreateEventData(newBranch, parent));
			return true;
		}

		// Token: 0x0600388A RID: 14474 RVA: 0x00179944 File Offset: 0x00177B44
		private int GetDesiredRootGrowthAmount()
		{
			if (this.root == null)
			{
				return 0;
			}
			return MathHelper.Clamp(this.Branches.Count((BallastFloraBranch b) => !b.IsRootGrowth && b.Health > 0f) / 20, 3, 30);
		}

		// Token: 0x0600388B RID: 14475 RVA: 0x00179990 File Offset: 0x00177B90
		public bool BranchContainsTarget(BallastFloraBranch branch, Item target)
		{
			Rectangle worldRect = branch.Rect;
			worldRect.Location = this.GetWorldPosition().ToPoint() + worldRect.Location;
			return worldRect.IntersectsWorld(target.WorldRect);
		}

		// Token: 0x0600388C RID: 14476 RVA: 0x001799D4 File Offset: 0x00177BD4
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
			if (!load)
			{
				this.CreateNetworkMessage(new BallastFloraBehavior.InfectEventData(target, BallastFloraBehavior.InfectEventData.InfectState.Yes, branch));
			}
		}

		// Token: 0x0600388D RID: 14477 RVA: 0x00179A44 File Offset: 0x00177C44
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

		// Token: 0x0600388E RID: 14478 RVA: 0x00179BC4 File Offset: 0x00177DC4
		private void OnBranchGrowthSuccess(BallastFloraBranch newBranch)
		{
			if (!this.HasBrokenThrough && this.Branches.Count > this.BreakthroughPoint)
			{
				this.BreakThrough();
			}
			this.CreateBody(newBranch);
			foreach (BallastFloraBranch vine in this.Branches)
			{
				vine.UpdateType();
			}
		}

		// Token: 0x0600388F RID: 14479 RVA: 0x00179C40 File Offset: 0x00177E40
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

		// Token: 0x06003890 RID: 14480 RVA: 0x00179D18 File Offset: 0x00177F18
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
			GameServer server = GameMain.Server;
			if (server != null)
			{
				KarmaManager karmaManager = server.KarmaManager;
				if (karmaManager != null)
				{
					karmaManager.OnBallastFloraDamaged(attacker, damage);
				}
			}
			if (branch.Health <= 0f && type != BallastFloraBehavior.AttackType.CutFromRoot)
			{
				this.RemoveBranch(branch);
				if (branch.IsRoot)
				{
					this.Kill();
				}
			}
		}

		// Token: 0x06003891 RID: 14481 RVA: 0x00179ED4 File Offset: 0x001780D4
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

		// Token: 0x06003892 RID: 14482 RVA: 0x00179F58 File Offset: 0x00178158
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
			if (!wasRemoved && this.Parent != null && !this.Parent.Removed)
			{
				this.CreateNetworkMessage(new BallastFloraBehavior.BranchRemoveEventData(branch));
			}
		}

		// Token: 0x06003893 RID: 14483 RVA: 0x0017A0C0 File Offset: 0x001782C0
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
			if (!item.Removed && this.Parent != null && !this.Parent.Removed)
			{
				this.CreateNetworkMessage(new BallastFloraBehavior.InfectEventData(item, BallastFloraBehavior.InfectEventData.InfectState.No, null));
			}
		}

		// Token: 0x06003894 RID: 14484 RVA: 0x0017A1E4 File Offset: 0x001783E4
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
			if (stateMachine != null)
			{
				IBallastFloraState state = stateMachine.State;
				if (state != null)
				{
					state.Exit();
				}
			}
			if (this.Parent != null && !this.Parent.Removed)
			{
				this.CreateNetworkMessage(default(BallastFloraBehavior.KillEventData));
			}
		}

		// Token: 0x06003895 RID: 14485 RVA: 0x0017A2D4 File Offset: 0x001784D4
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
			if (this.Parent != null && !this.Parent.Removed)
			{
				this.CreateNetworkMessage(default(BallastFloraBehavior.RemoveEventData));
			}
		}

		// Token: 0x06003896 RID: 14486 RVA: 0x0017A3A0 File Offset: 0x001785A0
		private void BreakThrough()
		{
			this.HasBrokenThrough = true;
			foreach (Body body in this.bodies)
			{
				body.Enabled = true;
			}
		}

		// Token: 0x06003897 RID: 14487 RVA: 0x0017A3FC File Offset: 0x001785FC
		private bool CanGrowMore()
		{
			return this.Branches.Any((BallastFloraBranch b) => b.CanGrowMore());
		}

		// Token: 0x06003898 RID: 14488 RVA: 0x0017A428 File Offset: 0x00178628
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

		// Token: 0x06003899 RID: 14489 RVA: 0x0017A545 File Offset: 0x00178745
		private static bool LineCollides(Vector2 point1, Vector2 point2)
		{
			IEnumerable<Body> ignoredBodies = null;
			Category? collisionCategory = new Category?(Category.Cat1 | Category.Cat2 | Category.Cat5 | Category.Cat8);
			bool ignoreSensors = true;
			Predicate<Fixture> customPredicate;
			if ((customPredicate = BallastFloraBehavior.<>O.<1>__CustomPredicate) == null)
			{
				customPredicate = (BallastFloraBehavior.<>O.<1>__CustomPredicate = new Predicate<Fixture>(BallastFloraBehavior.<LineCollides>g__CustomPredicate|207_0));
			}
			return Submarine.PickBody(point1, point2, ignoredBodies, collisionCategory, ignoreSensors, customPredicate, false) != null;
		}

		// Token: 0x0600389B RID: 14491 RVA: 0x0017A588 File Offset: 0x00178788
		[CompilerGenerated]
		private void <LoadSave>g__LoadBranch|180_1(XElement branchElement, IdRemap idRemap, ref BallastFloraBehavior.<>c__DisplayClass180_0 A_3)
		{
			BallastFloraBehavior.<>c__DisplayClass180_2 CS$<>8__locals1;
			CS$<>8__locals1.branchElement = branchElement;
			Vector2 pos = CS$<>8__locals1.branchElement.GetAttributeVector2("pos", Vector2.Zero);
			bool isRoot = CS$<>8__locals1.branchElement.GetAttributeBool("isroot", false);
			bool isRootGrowth = CS$<>8__locals1.branchElement.GetAttributeBool("isrootgrowth", false);
			int flowerConfig = BallastFloraBehavior.<LoadSave>g__getInt|180_3("flowerconfig", ref CS$<>8__locals1);
			int leafconfig = BallastFloraBehavior.<LoadSave>g__getInt|180_3("leafconfig", ref CS$<>8__locals1);
			int id = BallastFloraBehavior.<LoadSave>g__getInt|180_3("ID", ref CS$<>8__locals1);
			float health = BallastFloraBehavior.<LoadSave>g__getFloat|180_4("health", ref CS$<>8__locals1);
			float maxhealth = BallastFloraBehavior.<LoadSave>g__getFloat|180_4("maxhealth", ref CS$<>8__locals1);
			int sides = BallastFloraBehavior.<LoadSave>g__getInt|180_3("sides", ref CS$<>8__locals1);
			int blockedSides = BallastFloraBehavior.<LoadSave>g__getInt|180_3("blockedsides", ref CS$<>8__locals1);
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

		// Token: 0x0600389C RID: 14492 RVA: 0x0017A71D File Offset: 0x0017891D
		[CompilerGenerated]
		internal static int <LoadSave>g__getInt|180_3(string name, ref BallastFloraBehavior.<>c__DisplayClass180_2 A_1)
		{
			return A_1.branchElement.GetAttributeInt(name, 0);
		}

		// Token: 0x0600389D RID: 14493 RVA: 0x0017A72C File Offset: 0x0017892C
		[CompilerGenerated]
		internal static float <LoadSave>g__getFloat|180_4(string name, ref BallastFloraBehavior.<>c__DisplayClass180_2 A_1)
		{
			return A_1.branchElement.GetAttributeFloat(name, 0f);
		}

		// Token: 0x0600389E RID: 14494 RVA: 0x0017A73F File Offset: 0x0017893F
		[CompilerGenerated]
		internal static bool <Update>g__HasAcidEmitter|181_1(BallastFloraBranch b)
		{
			return b.SpawningItem || (b.AttackItem != null && !b.AttackItem.Removed);
		}

		// Token: 0x060038A0 RID: 14496 RVA: 0x0017A7A0 File Offset: 0x001789A0
		[CompilerGenerated]
		internal static bool <LineCollides>g__CustomPredicate|207_0(Fixture f)
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

		// Token: 0x04001C38 RID: 7224
		private const float DamageUpdateInterval = 1f;

		// Token: 0x04001C39 RID: 7225
		private float damageUpdateTimer;

		// Token: 0x04001C3A RID: 7226
		private static readonly List<BallastFloraBehavior> _entityList = new List<BallastFloraBehavior>();

		// Token: 0x04001C57 RID: 7255
		private float availablePower;

		// Token: 0x04001C58 RID: 7256
		private float anger;

		// Token: 0x04001C5D RID: 7261
		public Vector2 Offset;

		// Token: 0x04001C5E RID: 7262
		public readonly HashSet<Item> ClaimedTargets = new HashSet<Item>();

		// Token: 0x04001C5F RID: 7263
		public readonly HashSet<PowerTransfer> ClaimedJunctionBoxes = new HashSet<PowerTransfer>();

		// Token: 0x04001C60 RID: 7264
		public readonly HashSet<PowerContainer> ClaimedBatteries = new HashSet<PowerContainer>();

		// Token: 0x04001C61 RID: 7265
		public readonly Dictionary<Item, int> IgnoredTargets = new Dictionary<Item, int>();

		// Token: 0x04001C62 RID: 7266
		private readonly List<Tuple<ushort, int>> tempClaimedTargets = new List<Tuple<ushort, int>>();

		// Token: 0x04001C63 RID: 7267
		private int flowerVariants;

		// Token: 0x04001C64 RID: 7268
		private int leafVariants;

		// Token: 0x04001C65 RID: 7269
		public readonly List<BallastFloraBehavior.AITarget> Targets = new List<BallastFloraBehavior.AITarget>();

		// Token: 0x04001C66 RID: 7270
		public float PowerConsumptionTimer;

		// Token: 0x04001C67 RID: 7271
		private float defenseCooldown;

		// Token: 0x04001C68 RID: 7272
		private float toxinsCooldown;

		// Token: 0x04001C69 RID: 7273
		private float fireCheckCooldown;

		// Token: 0x04001C6A RID: 7274
		private float selfDamageTimer;

		// Token: 0x04001C6B RID: 7275
		private float toxinsTimer;

		// Token: 0x04001C6C RID: 7276
		private float toxinsSpawnTimer;

		// Token: 0x04001C6D RID: 7277
		private readonly List<BallastFloraBranch> branchesVulnerableToFire = new List<BallastFloraBranch>();

		// Token: 0x04001C6E RID: 7278
		public readonly List<BallastFloraBranch> Branches = new List<BallastFloraBranch>();

		// Token: 0x04001C6F RID: 7279
		[Nullable(2)]
		private BallastFloraBranch root;

		// Token: 0x04001C70 RID: 7280
		private readonly List<Body> bodies = new List<Body>();

		// Token: 0x04001C71 RID: 7281
		private bool isDead;

		// Token: 0x04001C72 RID: 7282
		public readonly BallastFloraStateMachine StateMachine;

		// Token: 0x04001C73 RID: 7283
		public int GrowthWarps;

		// Token: 0x04001C74 RID: 7284
		private readonly List<BallastFloraBranch> toBeRemoved = new List<BallastFloraBranch>();

		// Token: 0x02000C56 RID: 3158
		[NullableContext(0)]
		public enum NetworkHeader
		{
			// Token: 0x04003C25 RID: 15397
			Spawn,
			// Token: 0x04003C26 RID: 15398
			Kill,
			// Token: 0x04003C27 RID: 15399
			BranchCreate,
			// Token: 0x04003C28 RID: 15400
			BranchRemove,
			// Token: 0x04003C29 RID: 15401
			BranchDamage,
			// Token: 0x04003C2A RID: 15402
			Infect,
			// Token: 0x04003C2B RID: 15403
			Remove
		}

		// Token: 0x02000C57 RID: 3159
		[NullableContext(0)]
		public enum AttackType
		{
			// Token: 0x04003C2D RID: 15405
			Fire,
			// Token: 0x04003C2E RID: 15406
			Explosives,
			// Token: 0x04003C2F RID: 15407
			Other,
			// Token: 0x04003C30 RID: 15408
			CutFromRoot
		}

		// Token: 0x02000C58 RID: 3160
		[Nullable(0)]
		public struct AITarget
		{
			// Token: 0x060063E7 RID: 25575 RVA: 0x00213484 File Offset: 0x00211684
			public AITarget(ContentXElement element)
			{
				this.Tags = element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true);
				this.Priority = element.GetAttributeInt("priority", 0);
			}

			// Token: 0x060063E8 RID: 25576 RVA: 0x002134B0 File Offset: 0x002116B0
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

			// Token: 0x04003C31 RID: 15409
			public Identifier[] Tags;

			// Token: 0x04003C32 RID: 15410
			public int Priority;
		}

		// Token: 0x02000C59 RID: 3161
		[NullableContext(0)]
		public interface IEventData : NetEntityEvent.IData
		{
			// Token: 0x1700162E RID: 5678
			// (get) Token: 0x060063E9 RID: 25577
			BallastFloraBehavior.NetworkHeader NetworkHeader { get; }
		}

		// Token: 0x02000C5A RID: 3162
		[NullableContext(0)]
		public readonly struct SpawnEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x1700162F RID: 5679
			// (get) Token: 0x060063EA RID: 25578 RVA: 0x002134E6 File Offset: 0x002116E6
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Spawn;
				}
			}
		}

		// Token: 0x02000C5B RID: 3163
		[NullableContext(0)]
		private readonly struct KillEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001630 RID: 5680
			// (get) Token: 0x060063EB RID: 25579 RVA: 0x002134E9 File Offset: 0x002116E9
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Kill;
				}
			}
		}

		// Token: 0x02000C5C RID: 3164
		[NullableContext(0)]
		private readonly struct RemoveEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001631 RID: 5681
			// (get) Token: 0x060063EC RID: 25580 RVA: 0x002134EC File Offset: 0x002116EC
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Remove;
				}
			}
		}

		// Token: 0x02000C5D RID: 3165
		[NullableContext(0)]
		private readonly struct BranchCreateEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001632 RID: 5682
			// (get) Token: 0x060063ED RID: 25581 RVA: 0x002134EF File Offset: 0x002116EF
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.BranchCreate;
				}
			}

			// Token: 0x060063EE RID: 25582 RVA: 0x002134F2 File Offset: 0x002116F2
			public BranchCreateEventData(BallastFloraBranch newBranch, BallastFloraBranch parent)
			{
				this.NewBranch = newBranch;
				this.Parent = parent;
			}

			// Token: 0x04003C33 RID: 15411
			public readonly BallastFloraBranch NewBranch;

			// Token: 0x04003C34 RID: 15412
			public readonly BallastFloraBranch Parent;
		}

		// Token: 0x02000C5E RID: 3166
		[NullableContext(0)]
		private readonly struct BranchRemoveEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001633 RID: 5683
			// (get) Token: 0x060063EF RID: 25583 RVA: 0x00213502 File Offset: 0x00211702
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.BranchRemove;
				}
			}

			// Token: 0x060063F0 RID: 25584 RVA: 0x00213505 File Offset: 0x00211705
			public BranchRemoveEventData(BallastFloraBranch branch)
			{
				this.Branch = branch;
			}

			// Token: 0x04003C35 RID: 15413
			public readonly BallastFloraBranch Branch;
		}

		// Token: 0x02000C5F RID: 3167
		[NullableContext(0)]
		private readonly struct BranchDamageEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001634 RID: 5684
			// (get) Token: 0x060063F1 RID: 25585 RVA: 0x0021350E File Offset: 0x0021170E
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.BranchDamage;
				}
			}

			// Token: 0x060063F2 RID: 25586 RVA: 0x00213511 File Offset: 0x00211711
			public BranchDamageEventData(BallastFloraBranch branch)
			{
				this.Branch = branch;
			}

			// Token: 0x04003C36 RID: 15414
			public readonly BallastFloraBranch Branch;
		}

		// Token: 0x02000C60 RID: 3168
		[NullableContext(0)]
		private readonly struct InfectEventData : BallastFloraBehavior.IEventData, NetEntityEvent.IData
		{
			// Token: 0x17001635 RID: 5685
			// (get) Token: 0x060063F3 RID: 25587 RVA: 0x0021351A File Offset: 0x0021171A
			public BallastFloraBehavior.NetworkHeader NetworkHeader
			{
				get
				{
					return BallastFloraBehavior.NetworkHeader.Infect;
				}
			}

			// Token: 0x060063F4 RID: 25588 RVA: 0x0021351D File Offset: 0x0021171D
			public InfectEventData(Item item, BallastFloraBehavior.InfectEventData.InfectState infect, BallastFloraBranch infector)
			{
				this.Item = item;
				this.Infect = infect;
				this.Infector = infector;
			}

			// Token: 0x04003C37 RID: 15415
			public readonly Item Item;

			// Token: 0x04003C38 RID: 15416
			public readonly BallastFloraBehavior.InfectEventData.InfectState Infect;

			// Token: 0x04003C39 RID: 15417
			public readonly BallastFloraBranch Infector;

			// Token: 0x02000EE9 RID: 3817
			public enum InfectState
			{
				// Token: 0x040043F5 RID: 17397
				Yes,
				// Token: 0x040043F6 RID: 17398
				No
			}
		}

		// Token: 0x02000C61 RID: 3169
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003C3A RID: 15418
			[Nullable(0)]
			public static Func<BallastFloraBranch, bool> <0>__HasAcidEmitter;

			// Token: 0x04003C3B RID: 15419
			[Nullable(0)]
			public static Predicate<Fixture> <1>__CustomPredicate;
		}
	}
}

using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000199 RID: 409
	[NullableContext(1)]
	[Nullable(0)]
	internal class DamageBeaconStationAction : EventAction
	{
		// Token: 0x170008D8 RID: 2264
		// (get) Token: 0x06001ED9 RID: 7897 RVA: 0x000D7419 File Offset: 0x000D5619
		// (set) Token: 0x06001EDA RID: 7898 RVA: 0x000D7421 File Offset: 0x000D5621
		[Serialize(0f, IsPropertySaveable.Yes, "Probability of disconnecting wires (0.5 = 50% chance of disconnecting any given wire, 1 = all wires disconnected).", "", false)]
		public float DisconnectWireProbability { get; set; }

		// Token: 0x170008D9 RID: 2265
		// (get) Token: 0x06001EDB RID: 7899 RVA: 0x000D742A File Offset: 0x000D562A
		// (set) Token: 0x06001EDC RID: 7900 RVA: 0x000D7432 File Offset: 0x000D5632
		[Serialize(0f, IsPropertySaveable.Yes, "Probability of a wall sections leaking (0.5 = 50% creating a leak on any given wall section, 1 = all walls leak).", "", false)]
		public float DamageWallProbability { get; set; }

		// Token: 0x170008DA RID: 2266
		// (get) Token: 0x06001EDD RID: 7901 RVA: 0x000D743B File Offset: 0x000D563B
		// (set) Token: 0x06001EDE RID: 7902 RVA: 0x000D7443 File Offset: 0x000D5643
		[Serialize(0f, IsPropertySaveable.Yes, "Probability of devices being damaged (0.5 = 50% chance of damaging any given devices, 1 = all devices are damaged).", "", false)]
		public float DamageDeviceProbability { get; set; }

		// Token: 0x06001EDF RID: 7903 RVA: 0x000D744C File Offset: 0x000D564C
		public DamageBeaconStationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.DisconnectWireProbability <= 0f && this.DamageWallProbability <= 0f && this.DamageDeviceProbability <= 0f)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(88, 5);
				defaultInterpolatedStringHandler.AppendLiteral("Potential error in event ");
				defaultInterpolatedStringHandler.AppendFormatted(base.GetEventDebugName());
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.DisconnectWireProbability);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.DamageWallProbability);
				defaultInterpolatedStringHandler.AppendLiteral(" and ");
				defaultInterpolatedStringHandler.AppendFormatted<float>(this.DamageDeviceProbability);
				defaultInterpolatedStringHandler.AppendLiteral(" are all set to 0 in ");
				defaultInterpolatedStringHandler.AppendFormatted("DamageBeaconStationAction");
				defaultInterpolatedStringHandler.AppendLiteral(", and the action will do nothing.");
				string msg = defaultInterpolatedStringHandler.ToStringAndClear();
				ContentPackage contentPackage = parentEvent.Prefab.ContentPackage;
				DebugConsole.LogError(msg, null, contentPackage);
			}
		}

		// Token: 0x06001EE0 RID: 7904 RVA: 0x000D7545 File Offset: 0x000D5745
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x000D754D File Offset: 0x000D574D
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x000D7558 File Offset: 0x000D5758
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			if (Level.Loaded != null)
			{
				Level.Loaded.DisconnectBeaconStationWires(this.DisconnectWireProbability);
				Level.Loaded.DamageBeaconStationWalls(this.DamageWallProbability);
				Level.Loaded.DamageBeaconStationDevices(this.DamageDeviceProbability);
			}
			this.isFinished = true;
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x000D75AC File Offset: 0x000D57AC
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " DamageBeaconStationAction";
		}

		// Token: 0x04000EC8 RID: 3784
		private bool isFinished;
	}
}

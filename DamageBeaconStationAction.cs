using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200028C RID: 652
	[NullableContext(1)]
	[Nullable(0)]
	internal class DamageBeaconStationAction : EventAction
	{
		// Token: 0x17000F2C RID: 3884
		// (get) Token: 0x060039A3 RID: 14755 RVA: 0x0021C8CD File Offset: 0x0021AACD
		// (set) Token: 0x060039A4 RID: 14756 RVA: 0x0021C8D5 File Offset: 0x0021AAD5
		[Serialize(0f, IsPropertySaveable.Yes, "Probability of disconnecting wires (0.5 = 50% chance of disconnecting any given wire, 1 = all wires disconnected).", "", false)]
		public float DisconnectWireProbability { get; set; }

		// Token: 0x17000F2D RID: 3885
		// (get) Token: 0x060039A5 RID: 14757 RVA: 0x0021C8DE File Offset: 0x0021AADE
		// (set) Token: 0x060039A6 RID: 14758 RVA: 0x0021C8E6 File Offset: 0x0021AAE6
		[Serialize(0f, IsPropertySaveable.Yes, "Probability of a wall sections leaking (0.5 = 50% creating a leak on any given wall section, 1 = all walls leak).", "", false)]
		public float DamageWallProbability { get; set; }

		// Token: 0x17000F2E RID: 3886
		// (get) Token: 0x060039A7 RID: 14759 RVA: 0x0021C8EF File Offset: 0x0021AAEF
		// (set) Token: 0x060039A8 RID: 14760 RVA: 0x0021C8F7 File Offset: 0x0021AAF7
		[Serialize(0f, IsPropertySaveable.Yes, "Probability of devices being damaged (0.5 = 50% chance of damaging any given devices, 1 = all devices are damaged).", "", false)]
		public float DamageDeviceProbability { get; set; }

		// Token: 0x060039A9 RID: 14761 RVA: 0x0021C900 File Offset: 0x0021AB00
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

		// Token: 0x060039AA RID: 14762 RVA: 0x0021C9F9 File Offset: 0x0021ABF9
		public override bool IsFinished(ref string goToLabel)
		{
			return this.isFinished;
		}

		// Token: 0x060039AB RID: 14763 RVA: 0x0021CA01 File Offset: 0x0021AC01
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060039AC RID: 14764 RVA: 0x0021CA0C File Offset: 0x0021AC0C
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

		// Token: 0x060039AD RID: 14765 RVA: 0x0021CA60 File Offset: 0x0021AC60
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " DamageBeaconStationAction";
		}

		// Token: 0x04001DBF RID: 7615
		private bool isFinished;
	}
}

using System;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D5 RID: 1493
	internal class Scanner : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x06005FDB RID: 24539 RVA: 0x0031EEF8 File Offset: 0x0031D0F8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			bool wasScanCompletedPreviously = this.IsScanCompleted;
			this.scanTimer = msg.ReadSingle();
			if (!wasScanCompletedPreviously && this.IsScanCompleted)
			{
				Action<Scanner> onScanCompleted = this.OnScanCompleted;
				if (onScanCompleted == null)
				{
					return;
				}
				onScanCompleted(this);
			}
		}

		// Token: 0x1700183F RID: 6207
		// (get) Token: 0x06005FDC RID: 24540 RVA: 0x0031EF34 File Offset: 0x0031D134
		// (set) Token: 0x06005FDD RID: 24541 RVA: 0x0031EF3C File Offset: 0x0031D13C
		[Serialize(1f, IsPropertySaveable.No, "How long it takes for the scan to be completed.", "", false)]
		public float ScanDuration { get; set; }

		// Token: 0x17001840 RID: 6208
		// (get) Token: 0x06005FDE RID: 24542 RVA: 0x0031EF45 File Offset: 0x0031D145
		// (set) Token: 0x06005FDF RID: 24543 RVA: 0x0031EF50 File Offset: 0x0031D150
		[Serialize(0f, IsPropertySaveable.No, "How far along the scan is. When the timer goes above ScanDuration, the scan is completed.", "", false)]
		public float ScanTimer
		{
			get
			{
				return this.scanTimer;
			}
			set
			{
				if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
				{
					return;
				}
				if (this.Holdable == null)
				{
					return;
				}
				bool wasScanCompletedPreviously = this.IsScanCompleted;
				this.scanTimer = Math.Max(0f, value);
				if (!wasScanCompletedPreviously && this.IsScanCompleted)
				{
					Action<Scanner> onScanCompleted = this.OnScanCompleted;
					if (onScanCompleted == null)
					{
						return;
					}
					onScanCompleted(this);
				}
			}
		}

		// Token: 0x17001841 RID: 6209
		// (get) Token: 0x06005FE0 RID: 24544 RVA: 0x0031EFAE File Offset: 0x0031D1AE
		// (set) Token: 0x06005FE1 RID: 24545 RVA: 0x0031EFB6 File Offset: 0x0031D1B6
		[Serialize(1f, IsPropertySaveable.No, "How far the scanner can be from the target for the scan to be successful.", "", false)]
		public float ScanRadius { get; set; }

		// Token: 0x17001842 RID: 6210
		// (get) Token: 0x06005FE2 RID: 24546 RVA: 0x0031EFBF File Offset: 0x0031D1BF
		// (set) Token: 0x06005FE3 RID: 24547 RVA: 0x0031EFC7 File Offset: 0x0031D1C7
		[Serialize(true, IsPropertySaveable.No, "Should the progress bar always be displayed when the item has been attached.", "", false)]
		public bool AlwaysDisplayProgressBar { get; set; }

		// Token: 0x17001843 RID: 6211
		// (get) Token: 0x06005FE4 RID: 24548 RVA: 0x0031EFD0 File Offset: 0x0031D1D0
		// (set) Token: 0x06005FE5 RID: 24549 RVA: 0x0031EFD8 File Offset: 0x0031D1D8
		private Holdable Holdable { get; set; }

		// Token: 0x17001844 RID: 6212
		// (get) Token: 0x06005FE6 RID: 24550 RVA: 0x0031EFE1 File Offset: 0x0031D1E1
		// (set) Token: 0x06005FE7 RID: 24551 RVA: 0x0031EFE9 File Offset: 0x0031D1E9
		public bool DisplayProgressBar { get; set; }

		// Token: 0x17001845 RID: 6213
		// (get) Token: 0x06005FE8 RID: 24552 RVA: 0x0031EFF2 File Offset: 0x0031D1F2
		private bool IsScanCompleted
		{
			get
			{
				return this.scanTimer >= this.ScanDuration;
			}
		}

		// Token: 0x06005FE9 RID: 24553 RVA: 0x0031F005 File Offset: 0x0031D205
		public Scanner(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x06005FEA RID: 24554 RVA: 0x0031F018 File Offset: 0x0031D218
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.Holdable != null && this.Holdable.Attachable && this.Holdable.Attached)
			{
				if (this.ScanTimer <= 0f)
				{
					Action<Scanner> onScanStarted = this.OnScanStarted;
					if (onScanStarted != null)
					{
						onScanStarted(this);
					}
				}
				this.ScanTimer += deltaTime;
				AITarget aiTarget = this.item.AiTarget;
				if (aiTarget != null)
				{
					aiTarget.IncreaseSoundRange(deltaTime, 2f);
				}
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			}
			else
			{
				this.ScanTimer = 0f;
				this.DisplayProgressBar = false;
			}
			this.UpdateProjSpecific();
		}

		// Token: 0x06005FEB RID: 24555 RVA: 0x0031F0C4 File Offset: 0x0031D2C4
		private void UpdateProjSpecific()
		{
			if (this.Holdable != null && this.Holdable.Attached && (this.AlwaysDisplayProgressBar || this.DisplayProgressBar) && !this.IsScanCompleted)
			{
				Character controlled = Character.Controlled;
				if (controlled == null)
				{
					return;
				}
				controlled.UpdateHUDProgressBar(this, this.item.WorldPosition, this.ScanTimer / this.ScanDuration, GUIStyle.Red, GUIStyle.Green, "progressbar.scanning");
			}
		}

		// Token: 0x06005FEC RID: 24556 RVA: 0x0031F140 File Offset: 0x0031D340
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.Holdable = this.item.GetComponent<Holdable>();
			if (this.Holdable == null || !this.Holdable.Attachable)
			{
				DebugConsole.ThrowError("Error in initializing a Scanner component: an attachable Holdable component is required on the same item and none was found", null, this.item.Prefab.ContentPackage, false, false);
				this.IsActive = false;
			}
		}

		// Token: 0x040031A2 RID: 12706
		private float scanTimer;

		// Token: 0x040031A3 RID: 12707
		public Action<Scanner> OnScanStarted;

		// Token: 0x040031A4 RID: 12708
		public Action<Scanner> OnScanCompleted;
	}
}

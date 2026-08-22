using System;
using Barotrauma.Networking;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A6 RID: 1190
	internal class Scanner : ItemComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x170011CE RID: 4558
		// (get) Token: 0x060042A4 RID: 17060 RVA: 0x001ABA6C File Offset: 0x001A9C6C
		// (set) Token: 0x060042A5 RID: 17061 RVA: 0x001ABA74 File Offset: 0x001A9C74
		private float LastSentScanTimer { get; set; }

		// Token: 0x060042A6 RID: 17062 RVA: 0x001ABA7D File Offset: 0x001A9C7D
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteSingle(this.scanTimer);
		}

		// Token: 0x170011CF RID: 4559
		// (get) Token: 0x060042A7 RID: 17063 RVA: 0x001ABA8B File Offset: 0x001A9C8B
		// (set) Token: 0x060042A8 RID: 17064 RVA: 0x001ABA93 File Offset: 0x001A9C93
		[Serialize(1f, IsPropertySaveable.No, "How long it takes for the scan to be completed.", "", false)]
		public float ScanDuration { get; set; }

		// Token: 0x170011D0 RID: 4560
		// (get) Token: 0x060042A9 RID: 17065 RVA: 0x001ABA9C File Offset: 0x001A9C9C
		// (set) Token: 0x060042AA RID: 17066 RVA: 0x001ABAA4 File Offset: 0x001A9CA4
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
					if (onScanCompleted != null)
					{
						onScanCompleted(this);
					}
				}
				if (wasScanCompletedPreviously != this.IsScanCompleted || Math.Abs(this.LastSentScanTimer - this.scanTimer) > 0.1f)
				{
					this.item.CreateServerEvent<Scanner>(this);
					this.LastSentScanTimer = this.scanTimer;
				}
			}
		}

		// Token: 0x170011D1 RID: 4561
		// (get) Token: 0x060042AB RID: 17067 RVA: 0x001ABB3D File Offset: 0x001A9D3D
		// (set) Token: 0x060042AC RID: 17068 RVA: 0x001ABB45 File Offset: 0x001A9D45
		[Serialize(1f, IsPropertySaveable.No, "How far the scanner can be from the target for the scan to be successful.", "", false)]
		public float ScanRadius { get; set; }

		// Token: 0x170011D2 RID: 4562
		// (get) Token: 0x060042AD RID: 17069 RVA: 0x001ABB4E File Offset: 0x001A9D4E
		// (set) Token: 0x060042AE RID: 17070 RVA: 0x001ABB56 File Offset: 0x001A9D56
		[Serialize(true, IsPropertySaveable.No, "Should the progress bar always be displayed when the item has been attached.", "", false)]
		public bool AlwaysDisplayProgressBar { get; set; }

		// Token: 0x170011D3 RID: 4563
		// (get) Token: 0x060042AF RID: 17071 RVA: 0x001ABB5F File Offset: 0x001A9D5F
		// (set) Token: 0x060042B0 RID: 17072 RVA: 0x001ABB67 File Offset: 0x001A9D67
		private Holdable Holdable { get; set; }

		// Token: 0x170011D4 RID: 4564
		// (get) Token: 0x060042B1 RID: 17073 RVA: 0x001ABB70 File Offset: 0x001A9D70
		// (set) Token: 0x060042B2 RID: 17074 RVA: 0x001ABB78 File Offset: 0x001A9D78
		public bool DisplayProgressBar { get; set; }

		// Token: 0x170011D5 RID: 4565
		// (get) Token: 0x060042B3 RID: 17075 RVA: 0x001ABB81 File Offset: 0x001A9D81
		private bool IsScanCompleted
		{
			get
			{
				return this.scanTimer >= this.ScanDuration;
			}
		}

		// Token: 0x060042B4 RID: 17076 RVA: 0x001ABB94 File Offset: 0x001A9D94
		public Scanner(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x060042B5 RID: 17077 RVA: 0x001ABBA8 File Offset: 0x001A9DA8
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
				return;
			}
			this.ScanTimer = 0f;
			this.DisplayProgressBar = false;
		}

		// Token: 0x060042B6 RID: 17078 RVA: 0x001ABC50 File Offset: 0x001A9E50
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

		// Token: 0x0400200F RID: 8207
		private float scanTimer;

		// Token: 0x04002010 RID: 8208
		public Action<Scanner> OnScanStarted;

		// Token: 0x04002011 RID: 8209
		public Action<Scanner> OnScanCompleted;
	}
}

using System;
using System.Collections.Generic;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000498 RID: 1176
	internal class ItemLabel : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable
	{
		// Token: 0x170010FA RID: 4346
		// (get) Token: 0x06004019 RID: 16409 RVA: 0x0019B6A0 File Offset: 0x001998A0
		// (set) Token: 0x0600401A RID: 16410 RVA: 0x0019B6A8 File Offset: 0x001998A8
		[Serialize("", IsPropertySaveable.Yes, "The text to display on the label.", "", true)]
		[Editable(MaxLength = 100)]
		public string Text { get; set; }

		// Token: 0x170010FB RID: 4347
		// (get) Token: 0x0600401B RID: 16411 RVA: 0x0019B6B1 File Offset: 0x001998B1
		// (set) Token: 0x0600401C RID: 16412 RVA: 0x0019B6B9 File Offset: 0x001998B9
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool IgnoreLocalization { get; set; }

		// Token: 0x170010FC RID: 4348
		// (get) Token: 0x0600401D RID: 16413 RVA: 0x0019B6C2 File Offset: 0x001998C2
		// (set) Token: 0x0600401E RID: 16414 RVA: 0x0019B6CA File Offset: 0x001998CA
		[Editable]
		[Serialize("0,0,0,255", IsPropertySaveable.Yes, "The color of the text displayed on the label.", "", true)]
		public Color TextColor { get; set; }

		// Token: 0x170010FD RID: 4349
		// (get) Token: 0x0600401F RID: 16415 RVA: 0x0019B6D3 File Offset: 0x001998D3
		// (set) Token: 0x06004020 RID: 16416 RVA: 0x0019B6DB File Offset: 0x001998DB
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "The scale of the text displayed on the label.", "", true)]
		public float TextScale { get; set; }

		// Token: 0x170010FE RID: 4350
		// (get) Token: 0x06004021 RID: 16417 RVA: 0x0019B6E4 File Offset: 0x001998E4
		// (set) Token: 0x06004022 RID: 16418 RVA: 0x0019B6EC File Offset: 0x001998EC
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "The amount of padding around the text in pixels (left,top,right,bottom).", "", false)]
		public Vector4 Padding { get; set; }

		// Token: 0x06004023 RID: 16419 RVA: 0x0019B6F5 File Offset: 0x001998F5
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
		}

		// Token: 0x06004024 RID: 16420 RVA: 0x0019B6F7 File Offset: 0x001998F7
		public ItemLabel(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06004025 RID: 16421 RVA: 0x0019B701 File Offset: 0x00199901
		private IEnumerable<CoroutineStatus> SendStateAfterDelay()
		{
			ItemLabel.<SendStateAfterDelay>d__25 <SendStateAfterDelay>d__ = new ItemLabel.<SendStateAfterDelay>d__25(-2);
			<SendStateAfterDelay>d__.<>4__this = this;
			return <SendStateAfterDelay>d__;
		}

		// Token: 0x06004026 RID: 16422 RVA: 0x0019B711 File Offset: 0x00199911
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteString(this.Text);
			this.lastSentText = this.Text;
		}

		// Token: 0x170010FF RID: 4351
		// (get) Token: 0x06004027 RID: 16423 RVA: 0x0019B72B File Offset: 0x0019992B
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06004028 RID: 16424 RVA: 0x0019B732 File Offset: 0x00199932
		private void OnStateChanged()
		{
			this.sendStateTimer = 0.1f;
			if (this.sendStateCoroutine == null)
			{
				this.sendStateCoroutine = CoroutineManager.StartCoroutine(this.SendStateAfterDelay(), "");
			}
		}

		// Token: 0x06004029 RID: 16425 RVA: 0x0019B760 File Offset: 0x00199960
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "set_text"))
			{
				if (!(name == "set_text_color"))
				{
					return;
				}
				if (signal.value != this.prevColorSignal)
				{
					this.TextColor = XMLExtensions.ParseColor(signal.value, false);
					this.prevColorSignal = signal.value;
				}
				return;
			}
			else
			{
				if (this.Text == signal.value)
				{
					return;
				}
				this.Text = signal.value;
				this.OnStateChanged();
				return;
			}
		}

		// Token: 0x04001E9D RID: 7837
		private CoroutineHandle sendStateCoroutine;

		// Token: 0x04001E9E RID: 7838
		private string lastSentText;

		// Token: 0x04001E9F RID: 7839
		private float sendStateTimer;

		// Token: 0x04001EA5 RID: 7845
		private string prevColorSignal;
	}
}

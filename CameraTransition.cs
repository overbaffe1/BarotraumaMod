using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200001D RID: 29
	internal class CameraTransition
	{
		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600013F RID: 319 RVA: 0x00007918 File Offset: 0x00005B18
		// (set) Token: 0x06000140 RID: 320 RVA: 0x00007920 File Offset: 0x00005B20
		public bool Running { get; private set; }

		// Token: 0x06000141 RID: 321 RVA: 0x0000792C File Offset: 0x00005B2C
		public CameraTransition(ISpatialEntity targetEntity, Camera cam, Alignment? cameraStartPos, Alignment? cameraEndPos, bool fadeOut = true, bool losFadeIn = false, float waitDuration = 0f, float panDuration = 10f, float? startZoom = null, float? endZoom = null)
		{
			this.WaitDuration = waitDuration;
			this.PanDuration = panDuration;
			this.FadeOut = fadeOut;
			this.LosFadeIn = losFadeIn;
			this.cameraStartPos = cameraStartPos;
			this.cameraEndPos = cameraEndPos;
			this.startZoom = startZoom;
			this.endZoom = endZoom;
			this.AssignedCamera = cam;
			if (targetEntity == null)
			{
				return;
			}
			this.Running = true;
			this.prevControlled = Character.Controlled;
			CameraTransition.activeTransitions.RemoveAll((CameraTransition a) => !CoroutineManager.IsCoroutineRunning(a.updateCoroutine));
			foreach (CameraTransition activeTransition in CameraTransition.activeTransitions)
			{
				if (activeTransition.prevControlled != null && this.prevControlled == null)
				{
					this.prevControlled = activeTransition.prevControlled;
				}
				activeTransition.Stop();
			}
			this.updateCoroutine = CoroutineManager.StartCoroutine(this.Update(targetEntity, cam), "CameraTransition");
			CameraTransition.activeTransitions.Add(this);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00007A64 File Offset: 0x00005C64
		public void Stop()
		{
			CoroutineManager.StopCoroutines(this.updateCoroutine);
			this.Running = false;
			if (this.FadeOut)
			{
				GUI.ScreenOverlayColor = Color.TransparentBlack;
			}
			if (this.prevControlled != null && !this.prevControlled.Removed)
			{
				Character.Controlled = this.prevControlled;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000143 RID: 323 RVA: 0x00007AB5 File Offset: 0x00005CB5
		private float DeltaTime
		{
			get
			{
				if (!CoroutineManager.Paused || this.RunWhilePaused)
				{
					return CoroutineManager.DeltaTime;
				}
				return 0f;
			}
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00007AD1 File Offset: 0x00005CD1
		private IEnumerable<CoroutineStatus> Update(ISpatialEntity targetEntity, Camera cam)
		{
			CameraTransition.<Update>d__24 <Update>d__ = new CameraTransition.<Update>d__24(-2);
			<Update>d__.<>4__this = this;
			<Update>d__.<>3__targetEntity = targetEntity;
			<Update>d__.<>3__cam = cam;
			return <Update>d__;
		}

		// Token: 0x040000EB RID: 235
		private static List<CameraTransition> activeTransitions = new List<CameraTransition>();

		// Token: 0x040000ED RID: 237
		public Camera AssignedCamera;

		// Token: 0x040000EE RID: 238
		private readonly Alignment? cameraStartPos;

		// Token: 0x040000EF RID: 239
		private readonly Alignment? cameraEndPos;

		// Token: 0x040000F0 RID: 240
		private readonly float? startZoom;

		// Token: 0x040000F1 RID: 241
		private readonly float? endZoom;

		// Token: 0x040000F2 RID: 242
		public readonly float WaitDuration;

		// Token: 0x040000F3 RID: 243
		public float EndWaitDuration = 0.1f;

		// Token: 0x040000F4 RID: 244
		public readonly float PanDuration;

		// Token: 0x040000F5 RID: 245
		public readonly bool FadeOut;

		// Token: 0x040000F6 RID: 246
		public readonly bool LosFadeIn;

		// Token: 0x040000F7 RID: 247
		private readonly CoroutineHandle updateCoroutine;

		// Token: 0x040000F8 RID: 248
		private Character prevControlled;

		// Token: 0x040000F9 RID: 249
		public bool AllowInterrupt;

		// Token: 0x040000FA RID: 250
		public bool RemoveControlFromCharacter = true;

		// Token: 0x040000FB RID: 251
		public bool RunWhilePaused = true;
	}
}

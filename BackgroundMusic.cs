using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000133 RID: 307
	[TagNames(new string[]
	{
		"music"
	})]
	internal class BackgroundMusic : SoundPrefab
	{
		// Token: 0x0600288F RID: 10383 RVA: 0x001C3AA8 File Offset: 0x001C1CA8
		public BackgroundMusic(ContentXElement element, SoundsFile file) : base(element, file, true)
		{
			this.Type = element.GetAttributeIdentifier("Type", "");
			string key = "IntensityRange";
			Vector2 vector = new Vector2(0f, 100f);
			this.IntensityRange = element.GetAttributeVector2(key, vector);
			this.DuckVolume = element.GetAttributeBool("DuckVolume", false);
			this.MuteIntensityTracks = element.GetAttributeBool("MuteIntensityTracks", false);
			if (element.GetAttribute("ForceIntensityTrack") != null)
			{
				this.ForceIntensityTrack = new float?(element.GetAttributeFloat("ForceIntensityTrack", 0f));
			}
			this.StartFromRandomTime = element.GetAttributeBool("StartFromRandomTime", false);
			this.ContinueFromPreviousTime = element.GetAttributeBool("ContinueFromPreviousTime", false);
			this.MinimumPlayDuration = element.GetAttributeFloat("MinimumPlayDuration", 0f);
		}

		// Token: 0x040014B2 RID: 5298
		public static readonly PrefabCollection<BackgroundMusic> BackgroundMusicPrefabs = new PrefabCollection<BackgroundMusic>();

		// Token: 0x040014B3 RID: 5299
		public readonly Identifier Type;

		// Token: 0x040014B4 RID: 5300
		public readonly bool DuckVolume;

		// Token: 0x040014B5 RID: 5301
		public readonly Vector2 IntensityRange;

		// Token: 0x040014B6 RID: 5302
		public readonly bool MuteIntensityTracks;

		// Token: 0x040014B7 RID: 5303
		public readonly float? ForceIntensityTrack;

		// Token: 0x040014B8 RID: 5304
		public readonly bool StartFromRandomTime;

		// Token: 0x040014B9 RID: 5305
		public readonly bool ContinueFromPreviousTime;

		// Token: 0x040014BA RID: 5306
		public int PreviousTime;

		// Token: 0x040014BB RID: 5307
		public readonly float MinimumPlayDuration;
	}
}

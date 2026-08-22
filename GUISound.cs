using System;

namespace Barotrauma
{
	// Token: 0x02000134 RID: 308
	[TagNames(new string[]
	{
		"guisound"
	})]
	internal class GUISound : SoundPrefab
	{
		// Token: 0x06002891 RID: 10385 RVA: 0x001C3B8C File Offset: 0x001C1D8C
		public GUISound(ContentXElement element, SoundsFile file) : base(element, file, false)
		{
			string key = "guisoundtype";
			GUISoundType guisoundType = GUISoundType.UIMessage;
			this.Type = element.GetAttributeEnum<GUISoundType>(key, guisoundType);
		}

		// Token: 0x040014BC RID: 5308
		public static readonly PrefabCollection<GUISound> GUISoundPrefabs = new PrefabCollection<GUISound>();

		// Token: 0x040014BD RID: 5309
		public readonly GUISoundType Type;
	}
}

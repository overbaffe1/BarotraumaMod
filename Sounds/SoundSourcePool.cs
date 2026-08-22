using System;
using OpenAL;

namespace Barotrauma.Sounds
{
	// Token: 0x0200043E RID: 1086
	internal class SoundSourcePool : IDisposable
	{
		// Token: 0x1700127A RID: 4730
		// (get) Token: 0x0600488B RID: 18571 RVA: 0x0027AA26 File Offset: 0x00278C26
		// (set) Token: 0x0600488C RID: 18572 RVA: 0x0027AA2E File Offset: 0x00278C2E
		public uint[] ALSources { get; private set; }

		// Token: 0x0600488D RID: 18573 RVA: 0x0027AA38 File Offset: 0x00278C38
		public SoundSourcePool(int sourceCount = 32)
		{
			this.ALSources = new uint[sourceCount];
			for (int i = 0; i < sourceCount; i++)
			{
				Al.GenSource(out this.ALSources[i]);
				int alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Error generating alSource[" + i.ToString() + "]: " + Al.GetErrorString(alError));
				}
				if (!Al.IsSource(this.ALSources[i]))
				{
					throw new Exception("Generated alSource[" + i.ToString() + "] is invalid!");
				}
				Al.SourceStop(this.ALSources[i]);
				alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Error stopping newly generated alSource[" + i.ToString() + "]: " + Al.GetErrorString(alError));
				}
				Al.Sourcef(this.ALSources[i], 4109, 0f);
				alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Error setting min gain: " + Al.GetErrorString(alError));
				}
				Al.Sourcef(this.ALSources[i], 4110, 1f);
				alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Error setting max gain: " + Al.GetErrorString(alError));
				}
				Al.Sourcef(this.ALSources[i], 4129, 1f);
				alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Error setting rolloff factor: " + Al.GetErrorString(alError));
				}
			}
		}

		// Token: 0x0600488E RID: 18574 RVA: 0x0027ABAC File Offset: 0x00278DAC
		public void Dispose()
		{
			if (this.ALSources == null)
			{
				return;
			}
			for (int i = 0; i < this.ALSources.Length; i++)
			{
				Al.DeleteSource(this.ALSources[i]);
				int alError = Al.GetError();
				if (alError != 0)
				{
					throw new Exception("Failed to delete ALSources[" + i.ToString() + "]: " + Al.GetErrorString(alError));
				}
			}
			this.ALSources = null;
		}
	}
}

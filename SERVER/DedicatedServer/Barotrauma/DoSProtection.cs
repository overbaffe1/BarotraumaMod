using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200004B RID: 75
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class DoSProtection
	{
		// Token: 0x06000BF0 RID: 3056 RVA: 0x00071798 File Offset: 0x0006F998
		private static int GetMaxPacketLimit(ServerSettings settings)
		{
			return (int)MathF.Ceiling((float)settings.MaxPacketAmount * MathF.Max((float)settings.TickRate / 20f, 1f));
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x000717BF File Offset: 0x0006F9BF
		public DoSProtection.DoSAction Start(Client client)
		{
			return new DoSProtection.DoSAction(client, new Action<Client>(this.StartFor), new Action<Client>(this.EndFor));
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x000717DF File Offset: 0x0006F9DF
		public DoSProtection.DoSAction Pause(Client client)
		{
			return new DoSProtection.DoSAction(client, new Action<Client>(this.PauseFor), new Action<Client>(this.ResumeFor));
		}

		// Token: 0x06000BF3 RID: 3059 RVA: 0x000717FF File Offset: 0x0006F9FF
		private void StartFor(Client client)
		{
			this.clients.TryAdd(client, new DoSProtection.OffenseData());
			this.clients[client].Stopwatch.Start();
		}

		// Token: 0x06000BF4 RID: 3060 RVA: 0x0007182C File Offset: 0x0006FA2C
		private void EndFor(Client client)
		{
			DoSProtection.OffenseData data = this.GetData(client);
			if (data == null)
			{
				return;
			}
			data.PacketCount++;
			data.Stopwatch.Stop();
			this.UpdateOffense(client, data);
		}

		// Token: 0x06000BF5 RID: 3061 RVA: 0x00071866 File Offset: 0x0006FA66
		private void PauseFor(Client client)
		{
			DoSProtection.OffenseData data = this.GetData(client);
			if (data == null)
			{
				return;
			}
			data.Stopwatch.Stop();
		}

		// Token: 0x06000BF6 RID: 3062 RVA: 0x0007187E File Offset: 0x0006FA7E
		private void ResumeFor(Client client)
		{
			DoSProtection.OffenseData data = this.GetData(client);
			if (data == null)
			{
				return;
			}
			data.Stopwatch.Start();
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x00071898 File Offset: 0x0006FA98
		private void UpdateOffense(Client client, DoSProtection.OffenseData data)
		{
			GameServer server = GameMain.Server;
			ServerSettings settings = (server != null) ? server.ServerSettings : null;
			if (settings == null)
			{
				return;
			}
			if (data.PacketCount > DoSProtection.GetMaxPacketLimit(settings) && settings.MaxPacketAmount > 1200)
			{
				DoSProtection.<UpdateOffense>g__AttemptKickClient|16_0(client, TextManager.Get("PacketLimitKicked"));
				this.clients.Remove(client);
				return;
			}
			if (data.Stopwatch.ElapsedMilliseconds < 100L)
			{
				return;
			}
			data.Strikes++;
			data.ResetTimer();
			GameServer.Log(NetworkMember.ClientLogName(client, null) + " is causing the server to slow down.", ServerLog.MessageType.DoSProtection);
			if (data.Strikes < 6)
			{
				return;
			}
			if (settings.EnableDoSProtection)
			{
				DoSProtection.<UpdateOffense>g__AttemptKickClient|16_0(client, TextManager.Get("DoSProtectionKicked"));
			}
			this.clients.Remove(client);
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x00071960 File Offset: 0x0006FB60
		public void Update(float deltaTime)
		{
			this.stopwatchResetTimer += deltaTime;
			this.strikesResetTimer += deltaTime;
			if (this.stopwatchResetTimer > 1f)
			{
				this.stopwatchResetTimer = 0f;
				foreach (DoSProtection.OffenseData data in this.clients.Values)
				{
					data.ResetTimer();
				}
			}
			if (this.strikesResetTimer > 60f)
			{
				this.strikesResetTimer = 0f;
				foreach (KeyValuePair<Client, DoSProtection.OffenseData> keyValuePair in this.clients)
				{
					Client client2;
					DoSProtection.OffenseData offenseData;
					keyValuePair.Deconstruct(out client2, out offenseData);
					Client client = client2;
					DoSProtection.OffenseData data2 = offenseData;
					GameServer server = GameMain.Server;
					ServerSettings settings = (server != null) ? server.ServerSettings : null;
					if (settings != null && settings.MaxPacketAmount > 1200 && (float)data2.PacketCount > (float)DoSProtection.GetMaxPacketLimit(settings) * 0.9f)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(55, 2);
						defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(client, null));
						defaultInterpolatedStringHandler.AppendLiteral(" is sending a lot of packets and almost got kicked! (");
						defaultInterpolatedStringHandler.AppendFormatted<int>(data2.PacketCount);
						defaultInterpolatedStringHandler.AppendLiteral(").");
						GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.DoSProtection);
					}
					data2.ResetStrikes();
				}
			}
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x00071AEC File Offset: 0x0006FCEC
		[return: Nullable(2)]
		private DoSProtection.OffenseData GetData(Client client)
		{
			DoSProtection.OffenseData data;
			if (!this.clients.TryGetValue(client, out data))
			{
				return null;
			}
			return data;
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x00071B20 File Offset: 0x0006FD20
		[CompilerGenerated]
		internal static void <UpdateOffense>g__AttemptKickClient|16_0(Client client, LocalizedString reason)
		{
			if (RateLimiter.IsExempt(client))
			{
				return;
			}
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.KickClient(client, reason.Value, false);
		}

		// Token: 0x0400051E RID: 1310
		private readonly Dictionary<Client, DoSProtection.OffenseData> clients = new Dictionary<Client, DoSProtection.OffenseData>();

		// Token: 0x0400051F RID: 1311
		private float stopwatchResetTimer;

		// Token: 0x04000520 RID: 1312
		private float strikesResetTimer;

		// Token: 0x04000521 RID: 1313
		private const int StopwatchResetInterval = 1;

		// Token: 0x04000522 RID: 1314
		private const int StrikesResetInterval = 60;

		// Token: 0x04000523 RID: 1315
		private const int StrikeThreshold = 6;

		// Token: 0x04000524 RID: 1316
		private const int MinPacketLimitMultipler = 1;

		// Token: 0x0200075B RID: 1883
		[Nullable(0)]
		[CompilerFeatureRequired("RefStructs")]
		public readonly ref struct DoSAction
		{
			// Token: 0x060051D0 RID: 20944 RVA: 0x001E9DAA File Offset: 0x001E7FAA
			public DoSAction(Client sender, Action<Client> start, Action<Client> end)
			{
				this.sender = sender;
				this.end = end;
				start(sender);
			}

			// Token: 0x060051D1 RID: 20945 RVA: 0x001E9DC1 File Offset: 0x001E7FC1
			public void Dispose()
			{
				this.end(this.sender);
			}

			// Token: 0x04002CC0 RID: 11456
			private readonly Client sender;

			// Token: 0x04002CC1 RID: 11457
			private readonly Action<Client> end;
		}

		// Token: 0x0200075C RID: 1884
		[NullableContext(0)]
		private sealed class OffenseData
		{
			// Token: 0x060051D2 RID: 20946 RVA: 0x001E9DD4 File Offset: 0x001E7FD4
			public void ResetStrikes()
			{
				this.Strikes = 0;
				this.PacketCount = 0;
			}

			// Token: 0x060051D3 RID: 20947 RVA: 0x001E9DE4 File Offset: 0x001E7FE4
			public void ResetTimer()
			{
				this.Stopwatch.Reset();
			}

			// Token: 0x04002CC2 RID: 11458
			[Nullable(1)]
			public readonly Stopwatch Stopwatch = new Stopwatch();

			// Token: 0x04002CC3 RID: 11459
			public int Strikes;

			// Token: 0x04002CC4 RID: 11460
			public int PacketCount;
		}
	}
}

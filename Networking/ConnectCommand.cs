using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.Steam;

namespace Barotrauma.Networking
{
	// Token: 0x0200045B RID: 1115
	internal readonly struct ConnectCommand : IEquatable<ConnectCommand>
	{
		// Token: 0x06004AEF RID: 19183 RVA: 0x00292653 File Offset: 0x00290853
		public ConnectCommand(Option<ConnectCommand.NameAndP2PEndpoints> NameAndP2PEndpointsOption, Option<ConnectCommand.NameAndLidgrenEndpoint> NameAndLidgrenEndpointOption, Option<ConnectCommand.SteamLobbyId> SteamLobbyIdOption)
		{
			this.NameAndP2PEndpointsOption = NameAndP2PEndpointsOption;
			this.NameAndLidgrenEndpointOption = NameAndLidgrenEndpointOption;
			this.SteamLobbyIdOption = SteamLobbyIdOption;
		}

		// Token: 0x17001328 RID: 4904
		// (get) Token: 0x06004AF0 RID: 19184 RVA: 0x0029266A File Offset: 0x0029086A
		// (set) Token: 0x06004AF1 RID: 19185 RVA: 0x00292672 File Offset: 0x00290872
		public Option<ConnectCommand.NameAndP2PEndpoints> NameAndP2PEndpointsOption { get; set; }

		// Token: 0x17001329 RID: 4905
		// (get) Token: 0x06004AF2 RID: 19186 RVA: 0x0029267B File Offset: 0x0029087B
		// (set) Token: 0x06004AF3 RID: 19187 RVA: 0x00292683 File Offset: 0x00290883
		public Option<ConnectCommand.NameAndLidgrenEndpoint> NameAndLidgrenEndpointOption { get; set; }

		// Token: 0x1700132A RID: 4906
		// (get) Token: 0x06004AF4 RID: 19188 RVA: 0x0029268C File Offset: 0x0029088C
		// (set) Token: 0x06004AF5 RID: 19189 RVA: 0x00292694 File Offset: 0x00290894
		public Option<ConnectCommand.SteamLobbyId> SteamLobbyIdOption { get; set; }

		// Token: 0x06004AF6 RID: 19190 RVA: 0x002926A0 File Offset: 0x002908A0
		public bool IsClientConnectedToEndpoint()
		{
			GameClient client = GameMain.Client;
			if (((client != null) ? client.ClientPeer : null) == null)
			{
				return false;
			}
			ConnectCommand.NameAndP2PEndpoints nameAndP2PEndpoints;
			if (this.NameAndP2PEndpointsOption.TryUnwrap(out nameAndP2PEndpoints))
			{
				if (nameAndP2PEndpoints.Endpoints.Any((P2PEndpoint e) => e.Equals(GameMain.Client.ClientPeer.ServerEndpoint)))
				{
					return true;
				}
			}
			ConnectCommand.NameAndLidgrenEndpoint nameAndLidgrenEndpoint;
			ConnectCommand.SteamLobbyId steamLobbyId;
			return (this.NameAndLidgrenEndpointOption.TryUnwrap(out nameAndLidgrenEndpoint) && nameAndLidgrenEndpoint.Endpoint.Equals(GameMain.Client.ClientPeer.ServerEndpoint)) || (this.SteamLobbyIdOption.TryUnwrap(out steamLobbyId) && SteamManager.CurrentLobbyID == steamLobbyId.Value);
		}

		// Token: 0x06004AF7 RID: 19191 RVA: 0x0029275C File Offset: 0x0029095C
		[NullableContext(1)]
		public ConnectCommand(string serverName, Endpoint endpoint)
		{
			P2PEndpoint p2pEndpoint = endpoint as P2PEndpoint;
			Option<ConnectCommand.NameAndP2PEndpoints> nameAndP2PEndpointsOption;
			if (p2pEndpoint == null)
			{
				Option.UnspecifiedNone none = Option.None;
				nameAndP2PEndpointsOption = none;
			}
			else
			{
				nameAndP2PEndpointsOption = Option.Some<ConnectCommand.NameAndP2PEndpoints>(new ConnectCommand.NameAndP2PEndpoints(serverName, p2pEndpoint.ToEnumerable<P2PEndpoint>().ToImmutableArray<P2PEndpoint>()));
			}
			LidgrenEndpoint lidgrenEndpoint = endpoint as LidgrenEndpoint;
			Option<ConnectCommand.NameAndLidgrenEndpoint> nameAndLidgrenEndpointOption;
			if (lidgrenEndpoint == null)
			{
				Option.UnspecifiedNone none2 = Option.None;
				nameAndLidgrenEndpointOption = none2;
			}
			else
			{
				nameAndLidgrenEndpointOption = Option.Some<ConnectCommand.NameAndLidgrenEndpoint>(new ConnectCommand.NameAndLidgrenEndpoint(serverName, lidgrenEndpoint));
			}
			Option.UnspecifiedNone none3 = Option.None;
			this = new ConnectCommand(nameAndP2PEndpointsOption, nameAndLidgrenEndpointOption, none3);
		}

		// Token: 0x06004AF8 RID: 19192 RVA: 0x002927D4 File Offset: 0x002909D4
		[NullableContext(1)]
		public ConnectCommand(string serverName, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<P2PEndpoint> endpoints)
		{
			Option<ConnectCommand.NameAndP2PEndpoints> nameAndP2PEndpointsOption = Option.Some<ConnectCommand.NameAndP2PEndpoints>(new ConnectCommand.NameAndP2PEndpoints(serverName, endpoints));
			Option.UnspecifiedNone none = Option.None;
			Option<ConnectCommand.NameAndLidgrenEndpoint> nameAndLidgrenEndpointOption = none;
			Option.UnspecifiedNone none2 = Option.None;
			this = new ConnectCommand(nameAndP2PEndpointsOption, nameAndLidgrenEndpointOption, none2);
		}

		// Token: 0x06004AF9 RID: 19193 RVA: 0x00292810 File Offset: 0x00290A10
		[NullableContext(1)]
		public ConnectCommand(string serverName, LidgrenEndpoint endpoint)
		{
			Option.UnspecifiedNone none = Option.None;
			Option<ConnectCommand.NameAndP2PEndpoints> nameAndP2PEndpointsOption = none;
			Option<ConnectCommand.NameAndLidgrenEndpoint> nameAndLidgrenEndpointOption = Option.Some<ConnectCommand.NameAndLidgrenEndpoint>(new ConnectCommand.NameAndLidgrenEndpoint(serverName, endpoint));
			Option.UnspecifiedNone none2 = Option.None;
			this = new ConnectCommand(nameAndP2PEndpointsOption, nameAndLidgrenEndpointOption, none2);
		}

		// Token: 0x06004AFA RID: 19194 RVA: 0x0029284C File Offset: 0x00290A4C
		public ConnectCommand(ConnectCommand.SteamLobbyId lobbyId)
		{
			Option.UnspecifiedNone none = Option.None;
			Option<ConnectCommand.NameAndP2PEndpoints> nameAndP2PEndpointsOption = none;
			Option.UnspecifiedNone none2 = Option.None;
			this = new ConnectCommand(nameAndP2PEndpointsOption, none2, Option.Some<ConnectCommand.SteamLobbyId>(lobbyId));
		}

		// Token: 0x06004AFB RID: 19195 RVA: 0x0029287F File Offset: 0x00290A7F
		public static Option<ConnectCommand> Parse([Nullable(1)] string str)
		{
			return ConnectCommand.Parse(ToolBox.SplitCommand(str));
		}

		// Token: 0x06004AFC RID: 19196 RVA: 0x0029288C File Offset: 0x00290A8C
		public static Option<ConnectCommand> Parse([Nullable(new byte[]
		{
			2,
			1
		})] IReadOnlyList<string> args)
		{
			if (args == null || args.Count < 2)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			if (args[0].Equals("-connect", StringComparison.OrdinalIgnoreCase))
			{
				Option.UnspecifiedNone none;
				if (args.Count < 3)
				{
					none = Option.None;
					return none;
				}
				string serverName = args[1];
				string[] endpointStrs = args[2].Split(",", StringSplitOptions.None);
				IEnumerable<string> source = endpointStrs;
				Func<string, Option<Endpoint>> selector;
				if ((selector = ConnectCommand.<>O.<0>__Parse) == null)
				{
					selector = (ConnectCommand.<>O.<0>__Parse = new Func<string, Option<Endpoint>>(Endpoint.Parse));
				}
				ImmutableArray<Endpoint> endpoints = source.Select(selector).NotNone<Endpoint>().ToImmutableArray<Endpoint>();
				if (endpoints.Length != endpointStrs.Length)
				{
					none = Option.None;
					return none;
				}
				if (endpoints.All((Endpoint e) => e is P2PEndpoint))
				{
					return Option.Some<ConnectCommand>(new ConnectCommand(serverName, endpoints.Cast<P2PEndpoint>().ToImmutableArray<P2PEndpoint>()));
				}
				if (endpoints.Length == 1)
				{
					LidgrenEndpoint lidgrenEndpoint = endpoints[0] as LidgrenEndpoint;
					if (lidgrenEndpoint != null)
					{
						return Option.Some<ConnectCommand>(new ConnectCommand(serverName, lidgrenEndpoint));
					}
				}
				none = Option.None;
				return none;
			}
			else
			{
				if (!args[0].Equals("+connect_lobby", StringComparison.OrdinalIgnoreCase))
				{
					Option.UnspecifiedNone none = Option.None;
					return none;
				}
				ulong lobbyId;
				if (!ulong.TryParse(args[1], out lobbyId))
				{
					Option.UnspecifiedNone none = Option.None;
					return none;
				}
				return Option.Some<ConnectCommand>(new ConnectCommand(new ConnectCommand.SteamLobbyId(lobbyId)));
			}
		}

		// Token: 0x06004AFD RID: 19197 RVA: 0x00292A10 File Offset: 0x00290C10
		[NullableContext(1)]
		public override string ToString()
		{
			ConnectCommand.SteamLobbyId steamLobbyId;
			if (this.SteamLobbyIdOption.TryUnwrap(out steamLobbyId))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
				defaultInterpolatedStringHandler.AppendLiteral("+connect_lobby ");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(steamLobbyId.Value);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			ConnectCommand.NameAndP2PEndpoints nameAndP2PEndpoints;
			if (this.NameAndP2PEndpointsOption.TryUnwrap(out nameAndP2PEndpoints))
			{
				string escapedName = nameAndP2PEndpoints.ServerName.Replace("\"", "\\\"");
				string escapedEndpoints = (from e in nameAndP2PEndpoints.Endpoints
				select e.StringRepresentation).JoinEscaped(',');
				return "-connect \"" + escapedName + "\" " + escapedEndpoints;
			}
			ConnectCommand.NameAndLidgrenEndpoint nameAndLidgrenEndpoint;
			if (this.NameAndLidgrenEndpointOption.TryUnwrap(out nameAndLidgrenEndpoint))
			{
				string escapedName2 = nameAndLidgrenEndpoint.ServerName.Replace("\"", "\\\"");
				string endpoint = nameAndLidgrenEndpoint.Endpoint.StringRepresentation;
				return "-connect \"" + escapedName2 + "\" " + endpoint;
			}
			return "";
		}

		// Token: 0x06004AFE RID: 19198 RVA: 0x00292B20 File Offset: 0x00290D20
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("NameAndP2PEndpointsOption = ");
			builder.Append(this.NameAndP2PEndpointsOption.ToString());
			builder.Append(", NameAndLidgrenEndpointOption = ");
			builder.Append(this.NameAndLidgrenEndpointOption.ToString());
			builder.Append(", SteamLobbyIdOption = ");
			builder.Append(this.SteamLobbyIdOption.ToString());
			return true;
		}

		// Token: 0x06004AFF RID: 19199 RVA: 0x00292BA3 File Offset: 0x00290DA3
		[CompilerGenerated]
		public static bool operator !=(ConnectCommand left, ConnectCommand right)
		{
			return !(left == right);
		}

		// Token: 0x06004B00 RID: 19200 RVA: 0x00292BAF File Offset: 0x00290DAF
		[CompilerGenerated]
		public static bool operator ==(ConnectCommand left, ConnectCommand right)
		{
			return left.Equals(right);
		}

		// Token: 0x06004B01 RID: 19201 RVA: 0x00292BB9 File Offset: 0x00290DB9
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<Option<ConnectCommand.NameAndP2PEndpoints>>.Default.GetHashCode(this.<NameAndP2PEndpointsOption>k__BackingField) * -1521134295 + EqualityComparer<Option<ConnectCommand.NameAndLidgrenEndpoint>>.Default.GetHashCode(this.<NameAndLidgrenEndpointOption>k__BackingField)) * -1521134295 + EqualityComparer<Option<ConnectCommand.SteamLobbyId>>.Default.GetHashCode(this.<SteamLobbyIdOption>k__BackingField);
		}

		// Token: 0x06004B02 RID: 19202 RVA: 0x00292BF9 File Offset: 0x00290DF9
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is ConnectCommand && this.Equals((ConnectCommand)obj);
		}

		// Token: 0x06004B03 RID: 19203 RVA: 0x00292C14 File Offset: 0x00290E14
		[CompilerGenerated]
		public bool Equals(ConnectCommand other)
		{
			return EqualityComparer<Option<ConnectCommand.NameAndP2PEndpoints>>.Default.Equals(this.<NameAndP2PEndpointsOption>k__BackingField, other.<NameAndP2PEndpointsOption>k__BackingField) && EqualityComparer<Option<ConnectCommand.NameAndLidgrenEndpoint>>.Default.Equals(this.<NameAndLidgrenEndpointOption>k__BackingField, other.<NameAndLidgrenEndpointOption>k__BackingField) && EqualityComparer<Option<ConnectCommand.SteamLobbyId>>.Default.Equals(this.<SteamLobbyIdOption>k__BackingField, other.<SteamLobbyIdOption>k__BackingField);
		}

		// Token: 0x06004B04 RID: 19204 RVA: 0x00292C69 File Offset: 0x00290E69
		[CompilerGenerated]
		public void Deconstruct(out Option<ConnectCommand.NameAndP2PEndpoints> NameAndP2PEndpointsOption, out Option<ConnectCommand.NameAndLidgrenEndpoint> NameAndLidgrenEndpointOption, out Option<ConnectCommand.SteamLobbyId> SteamLobbyIdOption)
		{
			NameAndP2PEndpointsOption = this.NameAndP2PEndpointsOption;
			NameAndLidgrenEndpointOption = this.NameAndLidgrenEndpointOption;
			SteamLobbyIdOption = this.SteamLobbyIdOption;
		}

		// Token: 0x020011C1 RID: 4545
		[NullableContext(1)]
		[Nullable(0)]
		public readonly struct NameAndP2PEndpoints : IEquatable<ConnectCommand.NameAndP2PEndpoints>
		{
			// Token: 0x060091A2 RID: 37282 RVA: 0x003C5DF1 File Offset: 0x003C3FF1
			public NameAndP2PEndpoints(string ServerName, [Nullable(new byte[]
			{
				0,
				1
			})] ImmutableArray<P2PEndpoint> Endpoints)
			{
				this.ServerName = ServerName;
				this.Endpoints = Endpoints;
			}

			// Token: 0x17001CB1 RID: 7345
			// (get) Token: 0x060091A3 RID: 37283 RVA: 0x003C5E01 File Offset: 0x003C4001
			// (set) Token: 0x060091A4 RID: 37284 RVA: 0x003C5E09 File Offset: 0x003C4009
			public string ServerName { get; set; }

			// Token: 0x17001CB2 RID: 7346
			// (get) Token: 0x060091A5 RID: 37285 RVA: 0x003C5E12 File Offset: 0x003C4012
			// (set) Token: 0x060091A6 RID: 37286 RVA: 0x003C5E1A File Offset: 0x003C401A
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public ImmutableArray<P2PEndpoint> Endpoints { [return: Nullable(new byte[]
			{
				0,
				1
			})] get; [param: Nullable(new byte[]
			{
				0,
				1
			})] set; }

			// Token: 0x060091A7 RID: 37287 RVA: 0x003C5E24 File Offset: 0x003C4024
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NameAndP2PEndpoints");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060091A8 RID: 37288 RVA: 0x003C5E70 File Offset: 0x003C4070
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("ServerName = ");
				builder.Append(this.ServerName);
				builder.Append(", Endpoints = ");
				builder.Append(this.Endpoints.ToString());
				return true;
			}

			// Token: 0x060091A9 RID: 37289 RVA: 0x003C5EBE File Offset: 0x003C40BE
			[CompilerGenerated]
			public static bool operator !=(ConnectCommand.NameAndP2PEndpoints left, ConnectCommand.NameAndP2PEndpoints right)
			{
				return !(left == right);
			}

			// Token: 0x060091AA RID: 37290 RVA: 0x003C5ECA File Offset: 0x003C40CA
			[CompilerGenerated]
			public static bool operator ==(ConnectCommand.NameAndP2PEndpoints left, ConnectCommand.NameAndP2PEndpoints right)
			{
				return left.Equals(right);
			}

			// Token: 0x060091AB RID: 37291 RVA: 0x003C5ED4 File Offset: 0x003C40D4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<string>.Default.GetHashCode(this.<ServerName>k__BackingField) * -1521134295 + EqualityComparer<ImmutableArray<P2PEndpoint>>.Default.GetHashCode(this.<Endpoints>k__BackingField);
			}

			// Token: 0x060091AC RID: 37292 RVA: 0x003C5EFD File Offset: 0x003C40FD
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ConnectCommand.NameAndP2PEndpoints && this.Equals((ConnectCommand.NameAndP2PEndpoints)obj);
			}

			// Token: 0x060091AD RID: 37293 RVA: 0x003C5F15 File Offset: 0x003C4115
			[CompilerGenerated]
			public bool Equals(ConnectCommand.NameAndP2PEndpoints other)
			{
				return EqualityComparer<string>.Default.Equals(this.<ServerName>k__BackingField, other.<ServerName>k__BackingField) && EqualityComparer<ImmutableArray<P2PEndpoint>>.Default.Equals(this.<Endpoints>k__BackingField, other.<Endpoints>k__BackingField);
			}

			// Token: 0x060091AE RID: 37294 RVA: 0x003C5F47 File Offset: 0x003C4147
			[CompilerGenerated]
			public void Deconstruct(out string ServerName, [Nullable(new byte[]
			{
				0,
				1
			})] out ImmutableArray<P2PEndpoint> Endpoints)
			{
				ServerName = this.ServerName;
				Endpoints = this.Endpoints;
			}
		}

		// Token: 0x020011C2 RID: 4546
		[NullableContext(1)]
		[Nullable(0)]
		public readonly struct NameAndLidgrenEndpoint : IEquatable<ConnectCommand.NameAndLidgrenEndpoint>
		{
			// Token: 0x060091AF RID: 37295 RVA: 0x003C5F5D File Offset: 0x003C415D
			public NameAndLidgrenEndpoint(string ServerName, LidgrenEndpoint Endpoint)
			{
				this.ServerName = ServerName;
				this.Endpoint = Endpoint;
			}

			// Token: 0x17001CB3 RID: 7347
			// (get) Token: 0x060091B0 RID: 37296 RVA: 0x003C5F6D File Offset: 0x003C416D
			// (set) Token: 0x060091B1 RID: 37297 RVA: 0x003C5F75 File Offset: 0x003C4175
			public string ServerName { get; set; }

			// Token: 0x17001CB4 RID: 7348
			// (get) Token: 0x060091B2 RID: 37298 RVA: 0x003C5F7E File Offset: 0x003C417E
			// (set) Token: 0x060091B3 RID: 37299 RVA: 0x003C5F86 File Offset: 0x003C4186
			public LidgrenEndpoint Endpoint { get; set; }

			// Token: 0x060091B4 RID: 37300 RVA: 0x003C5F90 File Offset: 0x003C4190
			[NullableContext(0)]
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("NameAndLidgrenEndpoint");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060091B5 RID: 37301 RVA: 0x003C5FDC File Offset: 0x003C41DC
			[NullableContext(0)]
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("ServerName = ");
				builder.Append(this.ServerName);
				builder.Append(", Endpoint = ");
				builder.Append(this.Endpoint);
				return true;
			}

			// Token: 0x060091B6 RID: 37302 RVA: 0x003C6011 File Offset: 0x003C4211
			[CompilerGenerated]
			public static bool operator !=(ConnectCommand.NameAndLidgrenEndpoint left, ConnectCommand.NameAndLidgrenEndpoint right)
			{
				return !(left == right);
			}

			// Token: 0x060091B7 RID: 37303 RVA: 0x003C601D File Offset: 0x003C421D
			[CompilerGenerated]
			public static bool operator ==(ConnectCommand.NameAndLidgrenEndpoint left, ConnectCommand.NameAndLidgrenEndpoint right)
			{
				return left.Equals(right);
			}

			// Token: 0x060091B8 RID: 37304 RVA: 0x003C6027 File Offset: 0x003C4227
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<string>.Default.GetHashCode(this.<ServerName>k__BackingField) * -1521134295 + EqualityComparer<LidgrenEndpoint>.Default.GetHashCode(this.<Endpoint>k__BackingField);
			}

			// Token: 0x060091B9 RID: 37305 RVA: 0x003C6050 File Offset: 0x003C4250
			[NullableContext(0)]
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ConnectCommand.NameAndLidgrenEndpoint && this.Equals((ConnectCommand.NameAndLidgrenEndpoint)obj);
			}

			// Token: 0x060091BA RID: 37306 RVA: 0x003C6068 File Offset: 0x003C4268
			[CompilerGenerated]
			public bool Equals(ConnectCommand.NameAndLidgrenEndpoint other)
			{
				return EqualityComparer<string>.Default.Equals(this.<ServerName>k__BackingField, other.<ServerName>k__BackingField) && EqualityComparer<LidgrenEndpoint>.Default.Equals(this.<Endpoint>k__BackingField, other.<Endpoint>k__BackingField);
			}

			// Token: 0x060091BB RID: 37307 RVA: 0x003C609A File Offset: 0x003C429A
			[CompilerGenerated]
			public void Deconstruct(out string ServerName, out LidgrenEndpoint Endpoint)
			{
				ServerName = this.ServerName;
				Endpoint = this.Endpoint;
			}
		}

		// Token: 0x020011C3 RID: 4547
		public readonly struct SteamLobbyId : IEquatable<ConnectCommand.SteamLobbyId>
		{
			// Token: 0x060091BC RID: 37308 RVA: 0x003C60AC File Offset: 0x003C42AC
			public SteamLobbyId(ulong Value)
			{
				this.Value = Value;
			}

			// Token: 0x17001CB5 RID: 7349
			// (get) Token: 0x060091BD RID: 37309 RVA: 0x003C60B5 File Offset: 0x003C42B5
			// (set) Token: 0x060091BE RID: 37310 RVA: 0x003C60BD File Offset: 0x003C42BD
			public ulong Value { get; set; }

			// Token: 0x060091BF RID: 37311 RVA: 0x003C60C8 File Offset: 0x003C42C8
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SteamLobbyId");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x060091C0 RID: 37312 RVA: 0x003C6114 File Offset: 0x003C4314
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Value = ");
				builder.Append(this.Value.ToString());
				return true;
			}

			// Token: 0x060091C1 RID: 37313 RVA: 0x003C6149 File Offset: 0x003C4349
			[CompilerGenerated]
			public static bool operator !=(ConnectCommand.SteamLobbyId left, ConnectCommand.SteamLobbyId right)
			{
				return !(left == right);
			}

			// Token: 0x060091C2 RID: 37314 RVA: 0x003C6155 File Offset: 0x003C4355
			[CompilerGenerated]
			public static bool operator ==(ConnectCommand.SteamLobbyId left, ConnectCommand.SteamLobbyId right)
			{
				return left.Equals(right);
			}

			// Token: 0x060091C3 RID: 37315 RVA: 0x003C615F File Offset: 0x003C435F
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<ulong>.Default.GetHashCode(this.<Value>k__BackingField);
			}

			// Token: 0x060091C4 RID: 37316 RVA: 0x003C6171 File Offset: 0x003C4371
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ConnectCommand.SteamLobbyId && this.Equals((ConnectCommand.SteamLobbyId)obj);
			}

			// Token: 0x060091C5 RID: 37317 RVA: 0x003C6189 File Offset: 0x003C4389
			[CompilerGenerated]
			public bool Equals(ConnectCommand.SteamLobbyId other)
			{
				return EqualityComparer<ulong>.Default.Equals(this.<Value>k__BackingField, other.<Value>k__BackingField);
			}

			// Token: 0x060091C6 RID: 37318 RVA: 0x003C61A1 File Offset: 0x003C43A1
			[CompilerGenerated]
			public void Deconstruct(out ulong Value)
			{
				Value = this.Value;
			}
		}

		// Token: 0x020011C4 RID: 4548
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005CE5 RID: 23781
			public static Func<string, Option<Endpoint>> <0>__Parse;
		}
	}
}

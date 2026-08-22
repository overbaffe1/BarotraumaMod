using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020002B0 RID: 688
	[NullableContext(1)]
	[Nullable(0)]
	internal class TraitorEvent : ScriptedEvent
	{
		// Token: 0x17000DB8 RID: 3512
		// (get) Token: 0x06002F4D RID: 12109 RVA: 0x0013A99C File Offset: 0x00138B9C
		public new TraitorEventPrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x17000DB9 RID: 3513
		// (get) Token: 0x06002F4E RID: 12110 RVA: 0x0013A9A4 File Offset: 0x00138BA4
		// (set) Token: 0x06002F4F RID: 12111 RVA: 0x0013A9AC File Offset: 0x00138BAC
		public TraitorEvent.State CurrentState
		{
			get
			{
				return this.currentState;
			}
			set
			{
				if (this.currentState == value)
				{
					return;
				}
				this.currentState = value;
				Action onStateChanged = this.OnStateChanged;
				if (onStateChanged == null)
				{
					return;
				}
				onStateChanged();
			}
		}

		// Token: 0x17000DBA RID: 3514
		// (get) Token: 0x06002F50 RID: 12112 RVA: 0x0013A9CF File Offset: 0x00138BCF
		[Nullable(2)]
		public Client Traitor
		{
			[NullableContext(2)]
			get
			{
				return this.traitor;
			}
		}

		// Token: 0x17000DBB RID: 3515
		// (get) Token: 0x06002F51 RID: 12113 RVA: 0x0013A9D7 File Offset: 0x00138BD7
		public IEnumerable<Client> SecondaryTraitors
		{
			get
			{
				return this.secondaryTraitors;
			}
		}

		// Token: 0x06002F52 RID: 12114 RVA: 0x0013A9E0 File Offset: 0x00138BE0
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("TraitorEvent");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x17000DBC RID: 3516
		// (get) Token: 0x06002F53 RID: 12115 RVA: 0x0013AA33 File Offset: 0x00138C33
		protected override IEnumerable<Identifier> NonActionChildElementNames
		{
			get
			{
				return TraitorEvent.nonActionChildElementNames;
			}
		}

		// Token: 0x06002F54 RID: 12116 RVA: 0x0013AA3A File Offset: 0x00138C3A
		public TraitorEvent(TraitorEventPrefab prefab, int seed) : base(prefab, seed)
		{
			this.prefab = prefab;
			this.codeWord = string.Empty;
		}

		// Token: 0x06002F55 RID: 12117 RVA: 0x0013AA68 File Offset: 0x00138C68
		[NullableContext(2)]
		protected override void InitEventSpecific(EventSet parentSet = null)
		{
			if (this.traitor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Error when initializing event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\": traitor not set.\n");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace, null, null, false, false);
			}
		}

		// Token: 0x06002F56 RID: 12118 RVA: 0x0013AACC File Offset: 0x00138CCC
		public override LocalizedString ReplaceVariablesInEventText(LocalizedString str)
		{
			if (this.codeWord.IsNullOrEmpty())
			{
				this.codeWord = TextManager.Get("traitor.codeword");
			}
			string find = "[traitor]";
			Client client = this.traitor;
			LocalizedString localizedString = str.Replace(find, ((client != null) ? client.Name : null) ?? "none", StringComparison.Ordinal);
			string find2 = "[target]";
			Character character = base.GetTargets("target".ToIdentifier()).FirstOrDefault<Entity>() as Character;
			return localizedString.Replace(find2, ((character != null) ? character.DisplayName : null) ?? "none", StringComparison.Ordinal).Replace("[codeword]", this.codeWord.Value, StringComparison.Ordinal);
		}

		// Token: 0x06002F57 RID: 12119 RVA: 0x0013AB7C File Offset: 0x00138D7C
		public void SetTraitor(Client traitor)
		{
			if (traitor.Character == null)
			{
				throw new InvalidOperationException("Tried to set a client who's not controlling a character (\"" + traitor.Name + "\") as the traitor.");
			}
			this.traitor = traitor;
			traitor.Character.IsTraitor = true;
			base.AddTarget(Tags.Traitor, traitor.Character);
			base.AddTarget(Tags.AnyTraitor, traitor.Character);
			base.AddTargetPredicate(Tags.NonTraitor, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && (c.IsPlayer || c.IsBot) && !c.IsTraitor && c.TeamID == traitor.TeamID && !c.IsIncapacitated;
			});
			base.AddTargetPredicate(Tags.NonTraitorPlayer, ScriptedEvent.TargetPredicate.EntityType.Character, delegate(Entity e)
			{
				Character c = e as Character;
				return c != null && c.IsPlayer && !c.IsTraitor && c.IsOnPlayerTeam && !c.IsIncapacitated;
			});
		}

		// Token: 0x06002F58 RID: 12120 RVA: 0x0013AC50 File Offset: 0x00138E50
		public void SetSecondaryTraitors(IEnumerable<Client> traitors)
		{
			int index = 0;
			foreach (Client traitor in traitors)
			{
				if (traitor.Character == null)
				{
					throw new InvalidOperationException("Tried to set a client who's not controlling a character (\"" + traitor.Name + "\") as a secondary traitor.");
				}
				if (this.traitor == traitor)
				{
					DebugConsole.ThrowError("Tried to assign the main traitor " + traitor.Name + " as a secondary traitor.", null, null, false, false);
				}
				else
				{
					this.secondaryTraitors.Add(traitor);
					traitor.Character.IsTraitor = true;
					base.AddTarget(Tags.SecondaryTraitor, traitor.Character);
					base.AddTarget((Tags.SecondaryTraitor.ToString() + index.ToString()).ToIdentifier(), traitor.Character);
					base.AddTarget(Tags.AnyTraitor, traitor.Character);
					index++;
				}
			}
		}

		// Token: 0x040017AC RID: 6060
		[Nullable(2)]
		public Action OnStateChanged;

		// Token: 0x040017AD RID: 6061
		private new readonly TraitorEventPrefab prefab;

		// Token: 0x040017AE RID: 6062
		private LocalizedString codeWord;

		// Token: 0x040017AF RID: 6063
		private TraitorEvent.State currentState;

		// Token: 0x040017B0 RID: 6064
		[Nullable(2)]
		private Client traitor;

		// Token: 0x040017B1 RID: 6065
		private readonly HashSet<Client> secondaryTraitors = new HashSet<Client>();

		// Token: 0x040017B2 RID: 6066
		private static readonly HashSet<Identifier> nonActionChildElementNames = new HashSet<Identifier>
		{
			"icon".ToIdentifier(),
			"reputationrequirement".ToIdentifier(),
			"missionrequirement".ToIdentifier(),
			"levelrequirement".ToIdentifier()
		};

		// Token: 0x02000B3E RID: 2878
		[NullableContext(0)]
		public enum State
		{
			// Token: 0x04003906 RID: 14598
			Incomplete,
			// Token: 0x04003907 RID: 14599
			Completed,
			// Token: 0x04003908 RID: 14600
			Failed
		}
	}
}

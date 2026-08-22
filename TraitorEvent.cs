using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200037C RID: 892
	[NullableContext(1)]
	[Nullable(0)]
	internal class TraitorEvent : ScriptedEvent
	{
		// Token: 0x170011BB RID: 4539
		// (get) Token: 0x060043E5 RID: 17381 RVA: 0x00254EE0 File Offset: 0x002530E0
		public new TraitorEventPrefab Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x170011BC RID: 4540
		// (get) Token: 0x060043E6 RID: 17382 RVA: 0x00254EE8 File Offset: 0x002530E8
		// (set) Token: 0x060043E7 RID: 17383 RVA: 0x00254EF0 File Offset: 0x002530F0
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

		// Token: 0x170011BD RID: 4541
		// (get) Token: 0x060043E8 RID: 17384 RVA: 0x00254F13 File Offset: 0x00253113
		[Nullable(2)]
		public Client Traitor
		{
			[NullableContext(2)]
			get
			{
				return this.traitor;
			}
		}

		// Token: 0x170011BE RID: 4542
		// (get) Token: 0x060043E9 RID: 17385 RVA: 0x00254F1B File Offset: 0x0025311B
		public IEnumerable<Client> SecondaryTraitors
		{
			get
			{
				return this.secondaryTraitors;
			}
		}

		// Token: 0x060043EA RID: 17386 RVA: 0x00254F24 File Offset: 0x00253124
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendFormatted("TraitorEvent");
			defaultInterpolatedStringHandler.AppendLiteral(" (");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.prefab.Identifier);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x170011BF RID: 4543
		// (get) Token: 0x060043EB RID: 17387 RVA: 0x00254F77 File Offset: 0x00253177
		protected override IEnumerable<Identifier> NonActionChildElementNames
		{
			get
			{
				return TraitorEvent.nonActionChildElementNames;
			}
		}

		// Token: 0x060043EC RID: 17388 RVA: 0x00254F7E File Offset: 0x0025317E
		public TraitorEvent(TraitorEventPrefab prefab, int seed) : base(prefab, seed)
		{
			this.prefab = prefab;
			this.codeWord = string.Empty;
		}

		// Token: 0x060043ED RID: 17389 RVA: 0x00254FAC File Offset: 0x002531AC
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

		// Token: 0x060043EE RID: 17390 RVA: 0x00255010 File Offset: 0x00253210
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

		// Token: 0x060043EF RID: 17391 RVA: 0x002550C0 File Offset: 0x002532C0
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

		// Token: 0x060043F0 RID: 17392 RVA: 0x00255194 File Offset: 0x00253394
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

		// Token: 0x0400238F RID: 9103
		[Nullable(2)]
		public Action OnStateChanged;

		// Token: 0x04002390 RID: 9104
		private new readonly TraitorEventPrefab prefab;

		// Token: 0x04002391 RID: 9105
		private LocalizedString codeWord;

		// Token: 0x04002392 RID: 9106
		private TraitorEvent.State currentState;

		// Token: 0x04002393 RID: 9107
		[Nullable(2)]
		private Client traitor;

		// Token: 0x04002394 RID: 9108
		private readonly HashSet<Client> secondaryTraitors = new HashSet<Client>();

		// Token: 0x04002395 RID: 9109
		private static readonly HashSet<Identifier> nonActionChildElementNames = new HashSet<Identifier>
		{
			"icon".ToIdentifier(),
			"reputationrequirement".ToIdentifier(),
			"missionrequirement".ToIdentifier(),
			"levelrequirement".ToIdentifier()
		};

		// Token: 0x0200109A RID: 4250
		[NullableContext(0)]
		public enum State
		{
			// Token: 0x0400593A RID: 22842
			Incomplete,
			// Token: 0x0400593B RID: 22843
			Completed,
			// Token: 0x0400593C RID: 22844
			Failed
		}
	}
}

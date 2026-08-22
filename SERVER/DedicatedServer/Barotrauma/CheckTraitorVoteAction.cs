using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000194 RID: 404
	[NullableContext(1)]
	[Nullable(0)]
	internal class CheckTraitorVoteAction : BinaryOptionAction
	{
		// Token: 0x170008BE RID: 2238
		// (get) Token: 0x06001E90 RID: 7824 RVA: 0x000D660F File Offset: 0x000D480F
		// (set) Token: 0x06001E91 RID: 7825 RVA: 0x000D6617 File Offset: 0x000D4817
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to check.", "", false)]
		public Identifier Target { get; set; }

		// Token: 0x06001E92 RID: 7826 RVA: 0x000D6620 File Offset: 0x000D4820
		public CheckTraitorVoteAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (!(parentEvent is TraitorEvent))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error in event \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\" - ");
				defaultInterpolatedStringHandler.AppendFormatted("CheckTraitorVoteAction");
				defaultInterpolatedStringHandler.AppendLiteral(" can only be used in traitor events.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06001E93 RID: 7827 RVA: 0x000D66A0 File Offset: 0x000D48A0
		protected override bool? DetermineSuccess()
		{
			CheckTraitorVoteAction.<>c__DisplayClass5_0 CS$<>8__locals1 = new CheckTraitorVoteAction.<>c__DisplayClass5_0();
			IEnumerable<Entity> targetEntities = this.ParentEvent.GetTargets(this.Target);
			CheckTraitorVoteAction.<>c__DisplayClass5_0 CS$<>8__locals2 = CS$<>8__locals1;
			GameServer server = GameMain.Server;
			Client traitorClient;
			if (server == null)
			{
				traitorClient = null;
			}
			else
			{
				TraitorManager traitorManager = server.TraitorManager;
				traitorClient = ((traitorManager != null) ? traitorManager.GetClientAccusedAsTraitor() : null);
			}
			CS$<>8__locals2.traitorClient = traitorClient;
			if (CS$<>8__locals1.traitorClient != null)
			{
				return new bool?(targetEntities.Any(delegate(Entity e)
				{
					Character character = e as Character;
					if (character != null)
					{
						Client traitorClient2 = CS$<>8__locals1.traitorClient;
						return ((traitorClient2 != null) ? traitorClient2.Character : null) == character;
					}
					return false;
				}));
			}
			return new bool?(false);
		}

		// Token: 0x06001E94 RID: 7828 RVA: 0x000D6710 File Offset: 0x000D4910
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.succeeded != null, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("CheckTraitorVoteAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Target.ColorizeObject());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
	}
}

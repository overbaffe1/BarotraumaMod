using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200027B RID: 635
	internal class CheckConnectionAction : BinaryOptionAction
	{
		// Token: 0x17000EE3 RID: 3811
		// (get) Token: 0x060038C1 RID: 14529 RVA: 0x0021904C File Offset: 0x0021724C
		// (set) Token: 0x060038C2 RID: 14530 RVA: 0x00219054 File Offset: 0x00217254
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item to check.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000EE4 RID: 3812
		// (get) Token: 0x060038C3 RID: 14531 RVA: 0x0021905D File Offset: 0x0021725D
		// (set) Token: 0x060038C4 RID: 14532 RVA: 0x00219065 File Offset: 0x00217265
		[Serialize("", IsPropertySaveable.Yes, "The name of the connection to check on the target item.", "", false)]
		public Identifier ConnectionName { get; set; }

		// Token: 0x17000EE5 RID: 3813
		// (get) Token: 0x060038C5 RID: 14533 RVA: 0x0021906E File Offset: 0x0021726E
		// (set) Token: 0x060038C6 RID: 14534 RVA: 0x00219076 File Offset: 0x00217276
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item the connection must be wired to. If omitted, it doesn't matter what the connection is wired to.", "", false)]
		public Identifier ConnectedItemTag { get; set; }

		// Token: 0x17000EE6 RID: 3814
		// (get) Token: 0x060038C7 RID: 14535 RVA: 0x0021907F File Offset: 0x0021727F
		// (set) Token: 0x060038C8 RID: 14536 RVA: 0x00219087 File Offset: 0x00217287
		[Serialize("", IsPropertySaveable.Yes, "The name of the other connection the connection must be wired to. If omitted, it doesn't matter what the connection is wired to.", "", false)]
		public Identifier OtherConnectionName { get; set; }

		// Token: 0x17000EE7 RID: 3815
		// (get) Token: 0x060038C9 RID: 14537 RVA: 0x00219090 File Offset: 0x00217290
		// (set) Token: 0x060038CA RID: 14538 RVA: 0x00219098 File Offset: 0x00217298
		[Serialize(1, IsPropertySaveable.Yes, "Minimum number of matching connections for the check to succeed.", "", false)]
		public int MinAmount { get; set; }

		// Token: 0x060038CB RID: 14539 RVA: 0x002190A1 File Offset: 0x002172A1
		public CheckConnectionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x060038CC RID: 14540 RVA: 0x002190AC File Offset: 0x002172AC
		protected override bool? DetermineSuccess()
		{
			int amount = 0;
			CheckConnectionAction.<>c__DisplayClass21_0 CS$<>8__locals1;
			CS$<>8__locals1.connectTargets = ((!this.ConnectedItemTag.IsEmpty) ? this.ParentEvent.GetTargets(this.ConnectedItemTag) : Enumerable.Empty<Entity>());
			foreach (Entity target in this.ParentEvent.GetTargets(this.ItemTag))
			{
				Item targetItem = target as Item;
				if (targetItem != null)
				{
					ConnectionPanel panel = targetItem.GetComponent<ConnectionPanel>();
					if (panel != null && panel.Connections != null && !panel.Connections.None(null))
					{
						foreach (Connection connection in panel.Connections)
						{
							if (CheckConnectionAction.<DetermineSuccess>g__IsCorrectConnection|21_0(connection, this.ConnectionName))
							{
								if (this.ConnectedItemTag.IsEmpty && this.OtherConnectionName.IsEmpty)
								{
									amount += connection.Wires.Count;
									if (amount >= this.MinAmount)
									{
										return new bool?(true);
									}
								}
								else
								{
									foreach (Wire wire in connection.Wires)
									{
										CheckConnectionAction.<>c__DisplayClass21_1 CS$<>8__locals2;
										CS$<>8__locals2.otherConnection = wire.OtherConnection(connection);
										if (CS$<>8__locals2.otherConnection != null && (this.ConnectedItemTag.IsEmpty || CheckConnectionAction.<DetermineSuccess>g__IsCorrectConnection|21_0(CS$<>8__locals2.otherConnection, this.OtherConnectionName)) && (this.ConnectedItemTag.IsEmpty || CheckConnectionAction.<DetermineSuccess>g__IsCorrectItem|21_1(ref CS$<>8__locals1, ref CS$<>8__locals2)))
										{
											amount++;
											if (amount >= this.MinAmount)
											{
												return new bool?(true);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return new bool?(false);
		}

		// Token: 0x060038CD RID: 14541 RVA: 0x002192EC File Offset: 0x002174EC
		[CompilerGenerated]
		internal static bool <DetermineSuccess>g__IsCorrectItem|21_1(ref CheckConnectionAction.<>c__DisplayClass21_0 A_0, ref CheckConnectionAction.<>c__DisplayClass21_1 A_1)
		{
			return A_0.connectTargets.Contains(A_1.otherConnection.Item);
		}

		// Token: 0x060038CE RID: 14542 RVA: 0x00219304 File Offset: 0x00217504
		[CompilerGenerated]
		internal static bool <DetermineSuccess>g__IsCorrectConnection|21_0(Connection connection, Identifier id)
		{
			Identifier identifier = connection.Name.ToIdentifier();
			return identifier == id;
		}
	}
}

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000187 RID: 391
	internal class CheckConnectionAction : BinaryOptionAction
	{
		// Token: 0x1700088F RID: 2191
		// (get) Token: 0x06001DF5 RID: 7669 RVA: 0x000D3A98 File Offset: 0x000D1C98
		// (set) Token: 0x06001DF6 RID: 7670 RVA: 0x000D3AA0 File Offset: 0x000D1CA0
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item to check.", "", false)]
		public Identifier ItemTag { get; set; }

		// Token: 0x17000890 RID: 2192
		// (get) Token: 0x06001DF7 RID: 7671 RVA: 0x000D3AA9 File Offset: 0x000D1CA9
		// (set) Token: 0x06001DF8 RID: 7672 RVA: 0x000D3AB1 File Offset: 0x000D1CB1
		[Serialize("", IsPropertySaveable.Yes, "The name of the connection to check on the target item.", "", false)]
		public Identifier ConnectionName { get; set; }

		// Token: 0x17000891 RID: 2193
		// (get) Token: 0x06001DF9 RID: 7673 RVA: 0x000D3ABA File Offset: 0x000D1CBA
		// (set) Token: 0x06001DFA RID: 7674 RVA: 0x000D3AC2 File Offset: 0x000D1CC2
		[Serialize("", IsPropertySaveable.Yes, "Tag of the item the connection must be wired to. If omitted, it doesn't matter what the connection is wired to.", "", false)]
		public Identifier ConnectedItemTag { get; set; }

		// Token: 0x17000892 RID: 2194
		// (get) Token: 0x06001DFB RID: 7675 RVA: 0x000D3ACB File Offset: 0x000D1CCB
		// (set) Token: 0x06001DFC RID: 7676 RVA: 0x000D3AD3 File Offset: 0x000D1CD3
		[Serialize("", IsPropertySaveable.Yes, "The name of the other connection the connection must be wired to. If omitted, it doesn't matter what the connection is wired to.", "", false)]
		public Identifier OtherConnectionName { get; set; }

		// Token: 0x17000893 RID: 2195
		// (get) Token: 0x06001DFD RID: 7677 RVA: 0x000D3ADC File Offset: 0x000D1CDC
		// (set) Token: 0x06001DFE RID: 7678 RVA: 0x000D3AE4 File Offset: 0x000D1CE4
		[Serialize(1, IsPropertySaveable.Yes, "Minimum number of matching connections for the check to succeed.", "", false)]
		public int MinAmount { get; set; }

		// Token: 0x06001DFF RID: 7679 RVA: 0x000D3AED File Offset: 0x000D1CED
		public CheckConnectionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06001E00 RID: 7680 RVA: 0x000D3AF8 File Offset: 0x000D1CF8
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

		// Token: 0x06001E01 RID: 7681 RVA: 0x000D3D38 File Offset: 0x000D1F38
		[CompilerGenerated]
		internal static bool <DetermineSuccess>g__IsCorrectItem|21_1(ref CheckConnectionAction.<>c__DisplayClass21_0 A_0, ref CheckConnectionAction.<>c__DisplayClass21_1 A_1)
		{
			return A_0.connectTargets.Contains(A_1.otherConnection.Item);
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x000D3D50 File Offset: 0x000D1F50
		[CompilerGenerated]
		internal static bool <DetermineSuccess>g__IsCorrectConnection|21_0(Connection connection, Identifier id)
		{
			Identifier identifier = connection.Name.ToIdentifier();
			return identifier == id;
		}
	}
}

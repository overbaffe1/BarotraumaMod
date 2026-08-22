using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x0200011E RID: 286
	internal readonly struct CircuitBoxEventData : ItemComponent.IEventData, IEquatable<CircuitBoxEventData>
	{
		// Token: 0x06001B75 RID: 7029 RVA: 0x000CC4C0 File Offset: 0x000CA6C0
		[NullableContext(1)]
		public CircuitBoxEventData(INetSerializableStruct Data)
		{
			this.Data = Data;
		}

		// Token: 0x17000815 RID: 2069
		// (get) Token: 0x06001B76 RID: 7030 RVA: 0x000CC4C9 File Offset: 0x000CA6C9
		// (set) Token: 0x06001B77 RID: 7031 RVA: 0x000CC4D1 File Offset: 0x000CA6D1
		[Nullable(1)]
		public INetSerializableStruct Data { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x17000816 RID: 2070
		// (get) Token: 0x06001B78 RID: 7032 RVA: 0x000CC4DC File Offset: 0x000CA6DC
		public CircuitBoxOpcode Opcode
		{
			get
			{
				INetSerializableStruct data = this.Data;
				CircuitBoxOpcode result;
				if (!(data is CircuitBoxAddComponentEvent) && !(data is CircuitBoxServerCreateComponentEvent))
				{
					if (!(data is CircuitBoxRemoveComponentEvent))
					{
						if (!(data is CircuitBoxMoveComponentEvent))
						{
							if (!(data is CircuitBoxSelectNodesEvent))
							{
								if (!(data is CircuitBoxSelectWiresEvent))
								{
									if (!(data is CircuitBoxServerUpdateSelection))
									{
										if (!(data is CircuitBoxClientAddWireEvent) && !(data is CircuitBoxServerCreateWireEvent))
										{
											if (!(data is CircuitBoxRemoveWireEvent))
											{
												if (!(data is CircuitBoxInitializeStateFromServerEvent))
												{
													if (!(data is CircuitBoxRenameLabelEvent))
													{
														if (!(data is CircuitBoxAddLabelEvent) && !(data is CircuitBoxServerAddLabelEvent))
														{
															if (!(data is CircuitBoxRemoveLabelEvent))
															{
																if (!(data is CircuitBoxResizeLabelEvent))
																{
																	if (!(data is CircuitBoxRenameConnectionLabelsEvent))
																	{
																		throw new ArgumentOutOfRangeException("Data");
																	}
																	result = CircuitBoxOpcode.RenameConnections;
																}
																else
																{
																	result = CircuitBoxOpcode.ResizeLabel;
																}
															}
															else
															{
																result = CircuitBoxOpcode.RemoveLabel;
															}
														}
														else
														{
															result = CircuitBoxOpcode.AddLabel;
														}
													}
													else
													{
														result = CircuitBoxOpcode.RenameLabel;
													}
												}
												else
												{
													result = CircuitBoxOpcode.ServerInitialize;
												}
											}
											else
											{
												result = CircuitBoxOpcode.RemoveWire;
											}
										}
										else
										{
											result = CircuitBoxOpcode.AddWire;
										}
									}
									else
									{
										result = CircuitBoxOpcode.UpdateSelection;
									}
								}
								else
								{
									result = CircuitBoxOpcode.SelectWires;
								}
							}
							else
							{
								result = CircuitBoxOpcode.SelectComponents;
							}
						}
						else
						{
							result = CircuitBoxOpcode.MoveComponent;
						}
					}
					else
					{
						result = CircuitBoxOpcode.DeleteComponent;
					}
				}
				else
				{
					result = CircuitBoxOpcode.AddComponent;
				}
				return result;
			}
		}

		// Token: 0x06001B79 RID: 7033 RVA: 0x000CC5C8 File Offset: 0x000CA7C8
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CircuitBoxEventData");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06001B7A RID: 7034 RVA: 0x000CC614 File Offset: 0x000CA814
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Data = ");
			builder.Append(this.Data);
			builder.Append(", Opcode = ");
			builder.Append(this.Opcode.ToString());
			return true;
		}

		// Token: 0x06001B7B RID: 7035 RVA: 0x000CC662 File Offset: 0x000CA862
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxEventData left, CircuitBoxEventData right)
		{
			return !(left == right);
		}

		// Token: 0x06001B7C RID: 7036 RVA: 0x000CC66E File Offset: 0x000CA86E
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxEventData left, CircuitBoxEventData right)
		{
			return left.Equals(right);
		}

		// Token: 0x06001B7D RID: 7037 RVA: 0x000CC678 File Offset: 0x000CA878
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<INetSerializableStruct>.Default.GetHashCode(this.<Data>k__BackingField);
		}

		// Token: 0x06001B7E RID: 7038 RVA: 0x000CC68A File Offset: 0x000CA88A
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxEventData && this.Equals((CircuitBoxEventData)obj);
		}

		// Token: 0x06001B7F RID: 7039 RVA: 0x000CC6A2 File Offset: 0x000CA8A2
		[CompilerGenerated]
		public bool Equals(CircuitBoxEventData other)
		{
			return EqualityComparer<INetSerializableStruct>.Default.Equals(this.<Data>k__BackingField, other.<Data>k__BackingField);
		}

		// Token: 0x06001B80 RID: 7040 RVA: 0x000CC6BA File Offset: 0x000CA8BA
		[NullableContext(1)]
		[CompilerGenerated]
		public void Deconstruct(out INetSerializableStruct Data)
		{
			Data = this.Data;
		}
	}
}

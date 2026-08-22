using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Items.Components;

namespace Barotrauma
{
	// Token: 0x02000217 RID: 535
	internal readonly struct CircuitBoxEventData : ItemComponent.IEventData, IEquatable<CircuitBoxEventData>
	{
		// Token: 0x06003664 RID: 13924 RVA: 0x00212694 File Offset: 0x00210894
		[NullableContext(1)]
		public CircuitBoxEventData(INetSerializableStruct Data)
		{
			this.Data = Data;
		}

		// Token: 0x17000E79 RID: 3705
		// (get) Token: 0x06003665 RID: 13925 RVA: 0x0021269D File Offset: 0x0021089D
		// (set) Token: 0x06003666 RID: 13926 RVA: 0x002126A5 File Offset: 0x002108A5
		[Nullable(1)]
		public INetSerializableStruct Data { [NullableContext(1)] get; [NullableContext(1)] set; }

		// Token: 0x17000E7A RID: 3706
		// (get) Token: 0x06003667 RID: 13927 RVA: 0x002126B0 File Offset: 0x002108B0
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

		// Token: 0x06003668 RID: 13928 RVA: 0x0021279C File Offset: 0x0021099C
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

		// Token: 0x06003669 RID: 13929 RVA: 0x002127E8 File Offset: 0x002109E8
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Data = ");
			builder.Append(this.Data);
			builder.Append(", Opcode = ");
			builder.Append(this.Opcode.ToString());
			return true;
		}

		// Token: 0x0600366A RID: 13930 RVA: 0x00212836 File Offset: 0x00210A36
		[CompilerGenerated]
		public static bool operator !=(CircuitBoxEventData left, CircuitBoxEventData right)
		{
			return !(left == right);
		}

		// Token: 0x0600366B RID: 13931 RVA: 0x00212842 File Offset: 0x00210A42
		[CompilerGenerated]
		public static bool operator ==(CircuitBoxEventData left, CircuitBoxEventData right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600366C RID: 13932 RVA: 0x0021284C File Offset: 0x00210A4C
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<INetSerializableStruct>.Default.GetHashCode(this.<Data>k__BackingField);
		}

		// Token: 0x0600366D RID: 13933 RVA: 0x0021285E File Offset: 0x00210A5E
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CircuitBoxEventData && this.Equals((CircuitBoxEventData)obj);
		}

		// Token: 0x0600366E RID: 13934 RVA: 0x00212876 File Offset: 0x00210A76
		[CompilerGenerated]
		public bool Equals(CircuitBoxEventData other)
		{
			return EqualityComparer<INetSerializableStruct>.Default.Equals(this.<Data>k__BackingField, other.<Data>k__BackingField);
		}

		// Token: 0x0600366F RID: 13935 RVA: 0x0021288E File Offset: 0x00210A8E
		[NullableContext(1)]
		[CompilerGenerated]
		public void Deconstruct(out INetSerializableStruct Data)
		{
			Data = this.Data;
		}
	}
}

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;

namespace Barotrauma
{
	// Token: 0x0200027B RID: 635
	[AttributeUsage(AttributeTargets.Property)]
	internal sealed class ConditionallyEditable : Editable
	{
		// Token: 0x06002D2B RID: 11563 RVA: 0x00129643 File Offset: 0x00127843
		public ConditionallyEditable(ConditionallyEditable.ConditionType conditionType, bool onlyInEditors = true)
		{
			this.conditionType = conditionType;
			this.onlyInEditors = onlyInEditors;
		}

		// Token: 0x06002D2C RID: 11564 RVA: 0x0012965C File Offset: 0x0012785C
		public bool IsEditable(ISerializableEntity entity)
		{
			if (this.onlyInEditors)
			{
				Screen selected = Screen.Selected;
				if (selected != null && !selected.IsEditor)
				{
					return false;
				}
			}
			bool result;
			switch (this.conditionType)
			{
			case ConditionallyEditable.ConditionType.AllowLinkingWifiToChat:
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				int num;
				if (networkMember != null)
				{
					ServerSettings serverSettings = networkMember.ServerSettings;
					if (serverSettings != null)
					{
						num = ((!serverSettings.AllowLinkingWifiToChat) ? 1 : 0);
						goto IL_88;
					}
				}
				num = 0;
				IL_88:
				result = (num == 0);
				break;
			}
			case ConditionallyEditable.ConditionType.IsSwappableItem:
			{
				Item item = entity as Item;
				result = (item != null && item.Prefab.SwappableItem != null);
				break;
			}
			case ConditionallyEditable.ConditionType.AllowRotating:
			{
				Item item2 = entity as Item;
				bool flag;
				if (item2 == null || (item2.body != null && item2.body.BodyType != BodyType.Static) || !item2.Prefab.AllowRotatingInEditor)
				{
					Structure structure = entity as Structure;
					flag = (structure != null && structure.Prefab.AllowRotatingInEditor);
				}
				else
				{
					flag = true;
				}
				result = flag;
				break;
			}
			case ConditionallyEditable.ConditionType.Attachable:
			{
				Holdable holdable = ConditionallyEditable.<IsEditable>g__GetComponent|4_0<Holdable>(entity);
				result = (holdable != null && holdable.Attachable);
				break;
			}
			case ConditionallyEditable.ConditionType.HasBody:
			{
				Structure structure2 = entity as Structure;
				if (structure2 != null)
				{
					if (!structure2.HasBody)
					{
						goto IL_158;
					}
				}
				else
				{
					Item item4 = entity as Item;
					if (item4 == null)
					{
						goto IL_158;
					}
					PhysicsBody body = item4.body;
					if (body == null)
					{
						goto IL_158;
					}
				}
				bool flag2 = true;
				goto IL_15B;
				IL_158:
				flag2 = false;
				IL_15B:
				result = flag2;
				break;
			}
			case ConditionallyEditable.ConditionType.HasBodyByDefault:
			{
				Structure structure2 = entity as Structure;
				if (structure2 != null)
				{
					StructurePrefab prefab = structure2.Prefab;
					if (prefab == null)
					{
						goto IL_1AA;
					}
					if (!prefab.Body)
					{
						goto IL_1AA;
					}
				}
				else
				{
					Item item4 = entity as Item;
					if (item4 == null)
					{
						goto IL_1AA;
					}
					PhysicsBody body = item4.body;
					if (body == null)
					{
						goto IL_1AA;
					}
				}
				bool flag3 = true;
				goto IL_1AD;
				IL_1AA:
				flag3 = false;
				IL_1AD:
				result = flag3;
				break;
			}
			case ConditionallyEditable.ConditionType.Pickable:
			{
				Item item3 = entity as Item;
				result = (item3 != null && item3.GetComponent<Pickable>() != null);
				break;
			}
			case ConditionallyEditable.ConditionType.OnlyByStatusEffectsAndNetwork:
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				result = (networkMember != null && networkMember.IsServer);
				break;
			}
			case ConditionallyEditable.ConditionType.HasIntegratedButtons:
			{
				Door door = ConditionallyEditable.<IsEditable>g__GetComponent|4_0<Door>(entity);
				result = (door != null && door.HasIntegratedButtons);
				break;
			}
			case ConditionallyEditable.ConditionType.IsToggleableController:
			{
				Controller controller = ConditionallyEditable.<IsEditable>g__GetComponent|4_0<Controller>(entity);
				result = (controller != null && controller.IsToggle && controller.Item.GetComponent<ConnectionPanel>() != null);
				break;
			}
			case ConditionallyEditable.ConditionType.HasConnectionPanel:
				result = (ConditionallyEditable.<IsEditable>g__GetComponent|4_0<ConnectionPanel>(entity) != null);
				break;
			case ConditionallyEditable.ConditionType.DeteriorateUnderStress:
			{
				Item repairableItem = entity as Item;
				bool flag4;
				if (repairableItem != null)
				{
					flag4 = repairableItem.Components.Any((ItemComponent c) => c is IDeteriorateUnderStress);
				}
				else
				{
					flag4 = false;
				}
				result = flag4;
				break;
			}
			case ConditionallyEditable.ConditionType.ReceivesSubmarineImpacts:
			{
				Item item4 = entity as Item;
				bool flag5;
				if (item4 != null)
				{
					ItemPrefab prefab2 = item4.Prefab;
					if (prefab2 != null)
					{
						flag5 = prefab2.ReceiveSubmarineImpacts;
						goto IL_2A8;
					}
				}
				flag5 = false;
				IL_2A8:
				result = flag5;
				break;
			}
			default:
				result = false;
				break;
			}
			return result;
		}

		// Token: 0x06002D2D RID: 11565 RVA: 0x0012991C File Offset: 0x00127B1C
		[CompilerGenerated]
		internal static T <IsEditable>g__GetComponent|4_0<T>(ISerializableEntity e) where T : ItemComponent
		{
			T t = e as T;
			if (t != null)
			{
				return t;
			}
			Item item = e as Item;
			if (item != null)
			{
				return item.GetComponent<T>();
			}
			ItemComponent ic = e as ItemComponent;
			if (ic != null)
			{
				return ic.Item.GetComponent<T>();
			}
			return default(T);
		}

		// Token: 0x0400162C RID: 5676
		private readonly ConditionallyEditable.ConditionType conditionType;

		// Token: 0x0400162D RID: 5677
		private readonly bool onlyInEditors;

		// Token: 0x02000AF0 RID: 2800
		public enum ConditionType
		{
			// Token: 0x040037B3 RID: 14259
			AllowLinkingWifiToChat,
			// Token: 0x040037B4 RID: 14260
			IsSwappableItem,
			// Token: 0x040037B5 RID: 14261
			AllowRotating,
			// Token: 0x040037B6 RID: 14262
			Attachable,
			// Token: 0x040037B7 RID: 14263
			HasBody,
			// Token: 0x040037B8 RID: 14264
			HasBodyByDefault,
			// Token: 0x040037B9 RID: 14265
			Pickable,
			// Token: 0x040037BA RID: 14266
			OnlyByStatusEffectsAndNetwork,
			// Token: 0x040037BB RID: 14267
			HasIntegratedButtons,
			// Token: 0x040037BC RID: 14268
			IsToggleableController,
			// Token: 0x040037BD RID: 14269
			HasConnectionPanel,
			// Token: 0x040037BE RID: 14270
			DeteriorateUnderStress,
			// Token: 0x040037BF RID: 14271
			ReceivesSubmarineImpacts
		}
	}
}

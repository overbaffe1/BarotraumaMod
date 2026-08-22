using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;

namespace Barotrauma
{
	// Token: 0x0200034C RID: 844
	[AttributeUsage(AttributeTargets.Property)]
	internal sealed class ConditionallyEditable : Editable
	{
		// Token: 0x0600423D RID: 16957 RVA: 0x00249735 File Offset: 0x00247935
		public ConditionallyEditable(ConditionallyEditable.ConditionType conditionType, bool onlyInEditors = true)
		{
			this.conditionType = conditionType;
			this.onlyInEditors = onlyInEditors;
		}

		// Token: 0x0600423E RID: 16958 RVA: 0x0024974C File Offset: 0x0024794C
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

		// Token: 0x0600423F RID: 16959 RVA: 0x00249A0C File Offset: 0x00247C0C
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

		// Token: 0x04002270 RID: 8816
		private readonly ConditionallyEditable.ConditionType conditionType;

		// Token: 0x04002271 RID: 8817
		private readonly bool onlyInEditors;

		// Token: 0x02001068 RID: 4200
		public enum ConditionType
		{
			// Token: 0x0400585A RID: 22618
			AllowLinkingWifiToChat,
			// Token: 0x0400585B RID: 22619
			IsSwappableItem,
			// Token: 0x0400585C RID: 22620
			AllowRotating,
			// Token: 0x0400585D RID: 22621
			Attachable,
			// Token: 0x0400585E RID: 22622
			HasBody,
			// Token: 0x0400585F RID: 22623
			HasBodyByDefault,
			// Token: 0x04005860 RID: 22624
			Pickable,
			// Token: 0x04005861 RID: 22625
			OnlyByStatusEffectsAndNetwork,
			// Token: 0x04005862 RID: 22626
			HasIntegratedButtons,
			// Token: 0x04005863 RID: 22627
			IsToggleableController,
			// Token: 0x04005864 RID: 22628
			HasConnectionPanel,
			// Token: 0x04005865 RID: 22629
			DeteriorateUnderStress,
			// Token: 0x04005866 RID: 22630
			ReceivesSubmarineImpacts
		}
	}
}

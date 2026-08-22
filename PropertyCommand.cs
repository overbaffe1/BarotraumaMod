using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000148 RID: 328
	internal class PropertyCommand : Command
	{
		// Token: 0x060029D5 RID: 10709 RVA: 0x001CFC76 File Offset: 0x001CDE76
		public PropertyCommand(List<ISerializableEntity> receivers, Identifier propertyName, object newData, Dictionary<object, List<ISerializableEntity>> oldData)
		{
			this.Receivers = receivers;
			this.PropertyName = propertyName;
			this.OldProperties = oldData;
			this.NewProperties = newData;
			this.PropertyCount = receivers.Count;
			this.SanitizeProperty();
		}

		// Token: 0x060029D6 RID: 10710 RVA: 0x001CFCB0 File Offset: 0x001CDEB0
		public PropertyCommand(ISerializableEntity receiver, Identifier propertyName, object newData, object oldData)
		{
			this.Receivers = new List<ISerializableEntity>
			{
				receiver
			};
			this.PropertyName = propertyName;
			this.OldProperties = new Dictionary<object, List<ISerializableEntity>>
			{
				{
					oldData,
					this.Receivers
				}
			};
			this.NewProperties = newData;
			this.PropertyCount = 1;
			this.SanitizeProperty();
		}

		// Token: 0x060029D7 RID: 10711 RVA: 0x001CFD09 File Offset: 0x001CDF09
		public bool MergeInto(PropertyCommand master)
		{
			if (!master.Receivers.SequenceEqual(this.Receivers))
			{
				return false;
			}
			master.OldProperties = this.OldProperties;
			return true;
		}

		// Token: 0x060029D8 RID: 10712 RVA: 0x001CFD30 File Offset: 0x001CDF30
		private void SanitizeProperty()
		{
			object newProperties = this.NewProperties;
			string text;
			if (newProperties is float)
			{
				float f = (float)newProperties;
				text = f.FormatSingleDecimal();
			}
			else if (newProperties is Point)
			{
				Point point = (Point)newProperties;
				text = XMLExtensions.PointToString(point);
			}
			else if (newProperties is Vector2)
			{
				Vector2 vector2 = (Vector2)newProperties;
				text = vector2.FormatZeroDecimal();
			}
			else if (newProperties is Vector3)
			{
				Vector3 vector3 = (Vector3)newProperties;
				text = vector3.FormatSingleDecimal();
			}
			else if (newProperties is Vector4)
			{
				Vector4 vector4 = (Vector4)newProperties;
				text = vector4.FormatSingleDecimal();
			}
			else if (newProperties is Color)
			{
				Color color = (Color)newProperties;
				text = XMLExtensions.ColorToString(color);
			}
			else if (newProperties is Rectangle)
			{
				Rectangle rectangle = (Rectangle)newProperties;
				text = XMLExtensions.RectToString(rectangle);
			}
			else
			{
				text = this.NewProperties.ToString();
			}
			this.sanitizedProperty = text;
		}

		// Token: 0x060029D9 RID: 10713 RVA: 0x001CFE2B File Offset: 0x001CE02B
		public override void Execute()
		{
			this.SetProperties(false);
		}

		// Token: 0x060029DA RID: 10714 RVA: 0x001CFE34 File Offset: 0x001CE034
		public override void UnExecute()
		{
			this.SetProperties(true);
		}

		// Token: 0x060029DB RID: 10715 RVA: 0x001CFE3D File Offset: 0x001CE03D
		public override void Cleanup()
		{
			this.Receivers.Clear();
			this.OldProperties.Clear();
		}

		// Token: 0x060029DC RID: 10716 RVA: 0x001CFE58 File Offset: 0x001CE058
		private void SetProperties(bool undo)
		{
			foreach (ISerializableEntity t in this.Receivers)
			{
				ISerializableEntity serializableEntity = t;
				MapEntity me = serializableEntity as MapEntity;
				ISerializableEntity receiver;
				if (me == null)
				{
					ItemComponent ic = serializableEntity as ItemComponent;
					if (ic == null)
					{
						goto IL_61;
					}
					ISerializableEntity sItemComponent = ic.GetReplacementOrThis();
					if (sItemComponent == null)
					{
						goto IL_61;
					}
					receiver = sItemComponent;
				}
				else
				{
					ISerializableEntity sEntity = me.GetReplacementOrThis() as ISerializableEntity;
					if (sEntity == null)
					{
						goto IL_61;
					}
					receiver = sEntity;
				}
				IL_63:
				object data = this.NewProperties;
				if (undo)
				{
					foreach (KeyValuePair<object, List<ISerializableEntity>> keyValuePair in this.OldProperties)
					{
						object obj;
						List<ISerializableEntity> list2;
						keyValuePair.Deconstruct(out obj, out list2);
						object key = obj;
						List<ISerializableEntity> value = list2;
						if (value.Contains(t))
						{
							data = key;
						}
					}
				}
				if (receiver.SerializableProperties == null)
				{
					continue;
				}
				Dictionary<Identifier, SerializableProperty> props = receiver.SerializableProperties;
				SerializableProperty prop;
				if (!props.TryGetValue(this.PropertyName, out prop))
				{
					continue;
				}
				prop.TrySetValue(receiver, data);
				if (MapEntity.EditingHUD == null)
				{
					continue;
				}
				if (MapEntity.EditingHUD.UserData != receiver)
				{
					ItemComponent ic2 = receiver as ItemComponent;
					if (ic2 != null && MapEntity.EditingHUD.UserData != ic2.Item)
					{
						continue;
					}
				}
				GUIListBox list = MapEntity.EditingHUD.GetChild<GUIListBox>();
				if (list != null)
				{
					IEnumerable<SerializableEntityEditor> editors = list.Content.FindChildren((GUIComponent comp) => comp is SerializableEntityEditor).Cast<SerializableEntityEditor>();
					SerializableEntityEditor.LockEditing = true;
					foreach (SerializableEntityEditor editor in editors)
					{
						GUIComponent[] array;
						if (editor.UserData == receiver && editor.Fields.TryGetValue(this.PropertyName, out array))
						{
							editor.UpdateValue(prop, data, true);
						}
					}
					SerializableEntityEditor.LockEditing = false;
					continue;
				}
				continue;
				IL_61:
				receiver = t;
				goto IL_63;
			}
		}

		// Token: 0x060029DD RID: 10717 RVA: 0x001D009C File Offset: 0x001CE29C
		public override LocalizedString GetDescription()
		{
			if (this.Receivers.Count <= 1)
			{
				string tag = "Undo.ChangedProperty";
				ValueTuple<string, string>[] array = new ValueTuple<string, string>[3];
				array[0] = new ValueTuple<string, string>("[property]", this.PropertyName.Value);
				int num = 1;
				string item = "[item]";
				ISerializableEntity serializableEntity = this.Receivers.FirstOrDefault<ISerializableEntity>();
				array[num] = new ValueTuple<string, string>(item, (serializableEntity != null) ? serializableEntity.Name : null);
				array[2] = new ValueTuple<string, string>("[value]", this.sanitizedProperty);
				return TextManager.GetWithVariables(tag, array);
			}
			return TextManager.GetWithVariables("Undo.ChangedPropertyMultiple", new ValueTuple<string, string>[]
			{
				new ValueTuple<string, string>("[property]", this.PropertyName.Value),
				new ValueTuple<string, string>("[count]", this.Receivers.Count.ToString()),
				new ValueTuple<string, string>("[value]", this.sanitizedProperty)
			});
		}

		// Token: 0x040015D1 RID: 5585
		private Dictionary<object, List<ISerializableEntity>> OldProperties;

		// Token: 0x040015D2 RID: 5586
		private readonly List<ISerializableEntity> Receivers;

		// Token: 0x040015D3 RID: 5587
		private readonly Identifier PropertyName;

		// Token: 0x040015D4 RID: 5588
		private readonly object NewProperties;

		// Token: 0x040015D5 RID: 5589
		private string sanitizedProperty;

		// Token: 0x040015D6 RID: 5590
		public readonly int PropertyCount;
	}
}

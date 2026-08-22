using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002A7 RID: 679
	internal class StatusEffectAction : EventAction
	{
		// Token: 0x17000F95 RID: 3989
		// (get) Token: 0x06003B1D RID: 15133 RVA: 0x00221FE8 File Offset: 0x002201E8
		// (set) Token: 0x06003B1E RID: 15134 RVA: 0x00221FF0 File Offset: 0x002201F0
		[Serialize("", IsPropertySaveable.Yes, "Tag of the entity or entities the status effect should target.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x06003B1F RID: 15135 RVA: 0x00221FFC File Offset: 0x002201FC
		public StatusEffectAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			this.actionIndex = 0;
			foreach (ContentXElement subElement in parentEvent.Prefab.ConfigElement.Descendants())
			{
				if (subElement == element)
				{
					break;
				}
				this.actionIndex++;
			}
			foreach (ContentXElement subElement2 in element.Elements())
			{
				string a = subElement2.Name.ToString().ToLowerInvariant();
				if (a == "statuseffect")
				{
					List<StatusEffect> list = this.effects;
					ContentXElement element2 = subElement2;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
					defaultInterpolatedStringHandler.AppendFormatted("StatusEffectAction");
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					list.Add(StatusEffect.Load(element2, defaultInterpolatedStringHandler.ToStringAndClear()));
				}
			}
		}

		// Token: 0x06003B20 RID: 15136 RVA: 0x00222130 File Offset: 0x00220330
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003B21 RID: 15137 RVA: 0x00222138 File Offset: 0x00220338
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003B22 RID: 15138 RVA: 0x00222144 File Offset: 0x00220344
		public override void Update(float deltaTime)
		{
			StatusEffectAction.<>c__DisplayClass10_0 CS$<>8__locals1;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (this.isFinished)
			{
				return;
			}
			IEnumerable<Entity> eventTargets = this.ParentEvent.GetTargets(this.TargetTag);
			foreach (StatusEffect effect in this.effects)
			{
				foreach (Entity target in eventTargets)
				{
					if (effect.HasTargetType(StatusEffect.TargetType.NearbyCharacters))
					{
						List<ISerializableEntity> nearbyTargets = new List<ISerializableEntity>();
						effect.AddNearbyTargets(target.WorldPosition, nearbyTargets);
						using (List<ISerializableEntity>.Enumerator enumerator3 = nearbyTargets.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								ISerializableEntity nearbyTarget = enumerator3.Current;
								StatusEffectAction.<Update>g__ApplyOnTarget|10_0(nearbyTarget as Entity, effect, ref CS$<>8__locals1);
							}
							continue;
						}
					}
					StatusEffectAction.<Update>g__ApplyOnTarget|10_0(target, effect, ref CS$<>8__locals1);
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003B23 RID: 15139 RVA: 0x00222268 File Offset: 0x00220468
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 3);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("StatusEffectAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06003B24 RID: 15140 RVA: 0x002222D4 File Offset: 0x002204D4
		[CompilerGenerated]
		internal static void <Update>g__ApplyOnTarget|10_0(Entity target, StatusEffect effect, ref StatusEffectAction.<>c__DisplayClass10_0 A_2)
		{
			Item targetItem = target as Item;
			if (targetItem != null)
			{
				effect.Apply(effect.type, A_2.deltaTime, target, targetItem.AllPropertyObjects, null);
				return;
			}
			effect.Apply(effect.type, A_2.deltaTime, target, target as ISerializableEntity, null);
		}

		// Token: 0x04001E49 RID: 7753
		private readonly List<StatusEffect> effects = new List<StatusEffect>();

		// Token: 0x04001E4A RID: 7754
		private readonly int actionIndex;

		// Token: 0x04001E4C RID: 7756
		private bool isFinished;
	}
}

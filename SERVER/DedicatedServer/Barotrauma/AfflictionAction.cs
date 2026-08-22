using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000183 RID: 387
	internal class AfflictionAction : EventAction
	{
		// Token: 0x1700087D RID: 2173
		// (get) Token: 0x06001DB9 RID: 7609 RVA: 0x000D2E32 File Offset: 0x000D1032
		// (set) Token: 0x06001DBA RID: 7610 RVA: 0x000D2E3A File Offset: 0x000D103A
		[Serialize("", IsPropertySaveable.Yes, "Identifier of the affliction.", "", false)]
		public Identifier Affliction { get; set; }

		// Token: 0x1700087E RID: 2174
		// (get) Token: 0x06001DBB RID: 7611 RVA: 0x000D2E43 File Offset: 0x000D1043
		// (set) Token: 0x06001DBC RID: 7612 RVA: 0x000D2E4B File Offset: 0x000D104B
		[Serialize(0f, IsPropertySaveable.Yes, "Strength of the affliction.", "", false)]
		public float Strength { get; set; }

		// Token: 0x1700087F RID: 2175
		// (get) Token: 0x06001DBD RID: 7613 RVA: 0x000D2E54 File Offset: 0x000D1054
		// (set) Token: 0x06001DBE RID: 7614 RVA: 0x000D2E5C File Offset: 0x000D105C
		[Serialize(LimbType.None, IsPropertySaveable.Yes, "Type of the limb(s) to apply the affliction on. Only valid if the affliction is limb-specific.", "", false)]
		public LimbType LimbType { get; set; }

		// Token: 0x17000880 RID: 2176
		// (get) Token: 0x06001DBF RID: 7615 RVA: 0x000D2E65 File Offset: 0x000D1065
		// (set) Token: 0x06001DC0 RID: 7616 RVA: 0x000D2E6D File Offset: 0x000D106D
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character to apply the affliction on.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x17000881 RID: 2177
		// (get) Token: 0x06001DC1 RID: 7617 RVA: 0x000D2E76 File Offset: 0x000D1076
		// (set) Token: 0x06001DC2 RID: 7618 RVA: 0x000D2E7E File Offset: 0x000D107E
		[Serialize(false, IsPropertySaveable.Yes, "Should the strength be multiplied by the maximum vitality of the target?", "", false)]
		public bool MultiplyByMaxVitality { get; set; }

		// Token: 0x06001DC3 RID: 7619 RVA: 0x000D2E88 File Offset: 0x000D1088
		public AfflictionAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			if (this.Affliction.IsEmpty)
			{
				DebugConsole.ThrowError("Error in AfflictionAction: affliction not defined (use the attribute \"Affliction\").", null, element.ContentPackage, false, false);
			}
		}

		// Token: 0x06001DC4 RID: 7620 RVA: 0x000D2EC0 File Offset: 0x000D10C0
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06001DC5 RID: 7621 RVA: 0x000D2EC8 File Offset: 0x000D10C8
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06001DC6 RID: 7622 RVA: 0x000D2ED4 File Offset: 0x000D10D4
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			AfflictionPrefab afflictionPrefab;
			if (AfflictionPrefab.Prefabs.TryGet(this.Affliction, out afflictionPrefab))
			{
				IEnumerable<Entity> targets = this.ParentEvent.GetTargets(this.TargetTag);
				foreach (Entity target in targets)
				{
					if (target != null)
					{
						Character character = target as Character;
						if (character != null)
						{
							float strength = this.Strength;
							if (this.MultiplyByMaxVitality)
							{
								strength *= character.MaxVitality;
							}
							if (this.LimbType != LimbType.None)
							{
								Limb limb = character.AnimController.GetLimb(this.LimbType, true, false, false);
								if (strength > 0f)
								{
									character.CharacterHealth.ApplyAffliction(limb, afflictionPrefab.Instantiate(strength, null), true, true, true);
								}
								else if (strength < 0f)
								{
									character.CharacterHealth.ReduceAfflictionOnLimb(limb, this.Affliction, -strength, null, null);
								}
							}
							else if (strength > 0f)
							{
								character.CharacterHealth.ApplyAffliction(null, afflictionPrefab.Instantiate(strength, null), true, true, true);
							}
							else if (strength < 0f)
							{
								character.CharacterHealth.ReduceAfflictionOnAllLimbs(this.Affliction, -strength, null, null);
							}
						}
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06001DC7 RID: 7623 RVA: 0x000D3050 File Offset: 0x000D1250
		public override string ToDebugString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 6);
			defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.isFinished, false));
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted("AfflictionAction");
			defaultInterpolatedStringHandler.AppendLiteral(" -> (TargetTag: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.TargetTag.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("Affliction: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Affliction.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", Strength: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.Strength.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(", ");
			defaultInterpolatedStringHandler.AppendLiteral("LimbType: ");
			defaultInterpolatedStringHandler.AppendFormatted(this.LimbType.ColorizeObject());
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000E5F RID: 3679
		private bool isFinished;
	}
}

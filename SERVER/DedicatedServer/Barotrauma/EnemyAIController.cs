using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005B RID: 91
	internal class EnemyAIController : AIController
	{
		// Token: 0x17000358 RID: 856
		// (get) Token: 0x06000C94 RID: 3220 RVA: 0x00075A52 File Offset: 0x00073C52
		// (set) Token: 0x06000C95 RID: 3221 RVA: 0x00075A5A File Offset: 0x00073C5A
		public AIState State
		{
			get
			{
				return this._state;
			}
			set
			{
				if (this._state == value)
				{
					return;
				}
				if (this._state == AIState.PlayDead && value == AIState.Idle)
				{
					return;
				}
				this.PreviousState = this._state;
				this.OnStateChanged(this._state, value);
				this._state = value;
			}
		}

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x06000C96 RID: 3222 RVA: 0x00075A94 File Offset: 0x00073C94
		// (set) Token: 0x06000C97 RID: 3223 RVA: 0x00075A9C File Offset: 0x00073C9C
		public AIState PreviousState { get; private set; }

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x06000C98 RID: 3224 RVA: 0x00075AA5 File Offset: 0x00073CA5
		private IndoorsSteeringManager PathSteering
		{
			get
			{
				return this.insideSteering as IndoorsSteeringManager;
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x06000C99 RID: 3225 RVA: 0x00075AB2 File Offset: 0x00073CB2
		private bool IsAttackRunning
		{
			get
			{
				return this.AttackLimb != null && this.AttackLimb.attack.IsRunning;
			}
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x06000C9A RID: 3226 RVA: 0x00075AD0 File Offset: 0x00073CD0
		private bool IsCoolDownRunning
		{
			get
			{
				return (this.AttackLimb != null && this.AttackLimb.attack.CoolDownTimer > 0f) || (this._previousAttackLimb != null && this._previousAttackLimb.attack.CoolDownTimer > 0f);
			}
		}

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x06000C9B RID: 3227 RVA: 0x00075B1F File Offset: 0x00073D1F
		public float CombatStrength
		{
			get
			{
				return this.AIParams.CombatStrength;
			}
		}

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x06000C9C RID: 3228 RVA: 0x00075B2C File Offset: 0x00073D2C
		private float Sight
		{
			get
			{
				return this.GetPerceptionRange(this.AIParams.Sight);
			}
		}

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x06000C9D RID: 3229 RVA: 0x00075B3F File Offset: 0x00073D3F
		private float Hearing
		{
			get
			{
				return this.GetPerceptionRange(this.AIParams.Hearing);
			}
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x00075B54 File Offset: 0x00073D54
		private float GetPerceptionRange(float range)
		{
			AIState aistate = this.State;
			bool flag = aistate == AIState.PlayDead || aistate == AIState.Hiding;
			if (flag)
			{
				return 0.2f;
			}
			aistate = this.PreviousState;
			flag = (aistate == AIState.PlayDead || aistate == AIState.Hiding);
			if (flag)
			{
				return range * 1.5f;
			}
			return range;
		}

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x06000C9F RID: 3231 RVA: 0x00075BA4 File Offset: 0x00073DA4
		private float FleeHealthThreshold
		{
			get
			{
				return this.AIParams.FleeHealthThreshold;
			}
		}

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x06000CA0 RID: 3232 RVA: 0x00075BB1 File Offset: 0x00073DB1
		private bool IsAggressiveBoarder
		{
			get
			{
				return this.AIParams.AggressiveBoarding;
			}
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x06000CA1 RID: 3233 RVA: 0x00075BBE File Offset: 0x00073DBE
		private FishAnimController FishAnimController
		{
			get
			{
				return this.Character.AnimController as FishAnimController;
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x06000CA2 RID: 3234 RVA: 0x00075BD0 File Offset: 0x00073DD0
		// (set) Token: 0x06000CA3 RID: 3235 RVA: 0x00075BD8 File Offset: 0x00073DD8
		public Limb AttackLimb
		{
			get
			{
				return this._attackLimb;
			}
			private set
			{
				if (this._attackLimb != value)
				{
					this._previousAttackLimb = this._attackLimb;
					if (this._previousAttackLimb != null)
					{
						this.Character.DeselectCharacter();
						if (this._previousAttackLimb.attack.SnapRopeOnNewAttack)
						{
							Rope attachedRope = this._previousAttackLimb.AttachedRope;
							if (attachedRope != null)
							{
								attachedRope.Snap();
							}
						}
					}
				}
				else if (this._attackLimb != null && this._attackLimb.attack.CoolDownTimer <= 0f)
				{
					this.Character.DeselectCharacter();
					if (this._attackLimb.attack.SnapRopeOnNewAttack)
					{
						Rope attachedRope2 = this._attackLimb.AttachedRope;
						if (attachedRope2 != null)
						{
							attachedRope2.Snap();
						}
					}
				}
				this._attackLimb = value;
				this.attackVector = null;
				this.Reverse = (this._attackLimb != null && this._attackLimb.attack.Reverse);
			}
		}

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x06000CA4 RID: 3236 RVA: 0x00075CC0 File Offset: 0x00073EC0
		// (set) Token: 0x06000CA5 RID: 3237 RVA: 0x00075CEE File Offset: 0x00073EEE
		public Attack ActiveAttack
		{
			get
			{
				if (this._activeAttack == null)
				{
					return null;
				}
				if (this.lastAttackUpdateTime <= Timing.TotalTime - (double)this._activeAttack.Duration)
				{
					return null;
				}
				return this._activeAttack;
			}
			private set
			{
				this._activeAttack = value;
				this.lastAttackUpdateTime = Timing.TotalTime;
			}
		}

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x06000CA6 RID: 3238 RVA: 0x00075D02 File Offset: 0x00073F02
		public AITargetMemory CurrentTargetMemory
		{
			get
			{
				return this.currentTargetMemory;
			}
		}

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x00075D0A File Offset: 0x00073F0A
		public bool CanAttackDoors
		{
			get
			{
				return this.canAttackDoors;
			}
		}

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x06000CA8 RID: 3240 RVA: 0x00075D12 File Offset: 0x00073F12
		public float PriorityFearIncrement
		{
			get
			{
				return this.priorityFearIncreasement;
			}
		}

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x00075D1A File Offset: 0x00073F1A
		// (set) Token: 0x06000CAA RID: 3242 RVA: 0x00075D22 File Offset: 0x00073F22
		public LatchOntoAI LatchOntoAI { get; private set; }

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06000CAB RID: 3243 RVA: 0x00075D2B File Offset: 0x00073F2B
		// (set) Token: 0x06000CAC RID: 3244 RVA: 0x00075D33 File Offset: 0x00073F33
		public SwarmBehavior SwarmBehavior { get; private set; }

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06000CAD RID: 3245 RVA: 0x00075D3C File Offset: 0x00073F3C
		// (set) Token: 0x06000CAE RID: 3246 RVA: 0x00075D44 File Offset: 0x00073F44
		public PetBehavior PetBehavior { get; private set; }

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06000CAF RID: 3247 RVA: 0x00075D4D File Offset: 0x00073F4D
		public CharacterParams.TargetParams CurrentTargetingParams
		{
			get
			{
				return this.currentTargetingParams;
			}
		}

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x00075D55 File Offset: 0x00073F55
		public bool AttackHumans
		{
			get
			{
				return this.GetTargetParams(Tags.Human).Any(delegate(CharacterParams.TargetParams tp)
				{
					if (tp != null)
					{
						float priority = tp.Priority;
						if (priority > 0f)
						{
							AIState state = tp.State;
							if (state == AIState.Attack || state == AIState.Aggressive)
							{
								return true;
							}
						}
					}
					return false;
				});
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x06000CB1 RID: 3249 RVA: 0x00075D86 File Offset: 0x00073F86
		public bool AttackRooms
		{
			get
			{
				return this.GetTargetParams(Tags.Room).Any(delegate(CharacterParams.TargetParams tp)
				{
					if (tp != null)
					{
						float priority = tp.Priority;
						if (priority > 0f)
						{
							AIState state = tp.State;
							if (state == AIState.Attack || state == AIState.Aggressive)
							{
								return true;
							}
						}
					}
					return false;
				});
			}
		}

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06000CB2 RID: 3250 RVA: 0x00075DB8 File Offset: 0x00073FB8
		public override CanEnterSubmarine CanEnterSubmarine
		{
			get
			{
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null && latchOntoAI.IsAttachedToSub)
				{
					return CanEnterSubmarine.False;
				}
				return this.Character.AnimController.CanEnterSubmarine;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06000CB3 RID: 3251 RVA: 0x00075DEC File Offset: 0x00073FEC
		public override bool CanFlip
		{
			get
			{
				return !this.Reverse && (this.State != AIState.Eat || this.Character.SelectedCharacter == null) && (this.LatchOntoAI == null || !this.LatchOntoAI.IsAttachedToSub) && (this.Character.CurrentHull == null || !this.Character.AnimController.InWater || Math.Min(this.Character.CurrentHull.Size.X, this.Character.CurrentHull.Size.Y) > ConvertUnits.ToDisplayUnits(Math.Max(this.colliderLength, this.colliderWidth)));
			}
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x00075E9C File Offset: 0x0007409C
		public void SetUnattackableSubmarines(Submarine submarine, bool includeOwnSub = true, bool includeConnectedSubs = true, bool clearExisting = true)
		{
			EnemyAIController.<>c__DisplayClass113_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.includeConnectedSubs = includeConnectedSubs;
			if (clearExisting)
			{
				this.unattackableSubmarines.Clear();
			}
			if (submarine != null)
			{
				this.<SetUnattackableSubmarines>g__AddSubs|113_0(submarine, ref CS$<>8__locals1);
			}
			if (includeOwnSub)
			{
				Submarine ownSub = this.Character.Submarine;
				if (ownSub != null && ownSub != submarine)
				{
					this.<SetUnattackableSubmarines>g__AddSubs|113_0(ownSub, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x00075EF4 File Offset: 0x000740F4
		public static bool IsTargetBeingChasedBy(Character target, Character character)
		{
			EnemyAIController enemyAI = ((character != null) ? character.AIController : null) as EnemyAIController;
			bool flag;
			if (enemyAI != null)
			{
				AITarget selectedAiTarget = enemyAI.SelectedAiTarget;
				flag = (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == target);
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			bool flag3 = flag2;
			if (flag3)
			{
				AIState state = enemyAI.State;
				bool flag4 = state == AIState.Attack || state == AIState.Aggressive;
				flag3 = flag4;
			}
			return flag3;
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x00075F4F File Offset: 0x0007414F
		public bool IsBeingChasedBy(Character c)
		{
			return EnemyAIController.IsTargetBeingChasedBy(this.Character, c);
		}

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06000CB7 RID: 3255 RVA: 0x00075F5D File Offset: 0x0007415D
		private bool IsBeingChased
		{
			get
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				return this.IsBeingChasedBy(((selectedAiTarget != null) ? selectedAiTarget.Entity : null) as Character);
			}
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x00075F7C File Offset: 0x0007417C
		private static bool IsTargetInPlayerTeam(AITarget target)
		{
			bool flag;
			if (target == null)
			{
				flag = (null != null);
			}
			else
			{
				Entity entity = target.Entity;
				flag = (((entity != null) ? entity.Submarine : null) != null);
			}
			if (!flag || !target.Entity.Submarine.Info.IsPlayer)
			{
				Character character = ((target != null) ? target.Entity : null) as Character;
				return character != null && character.IsOnPlayerTeam;
			}
			return true;
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x00075FDC File Offset: 0x000741DC
		private bool IsAttackingOwner(Character other)
		{
			if (this.PetBehavior != null && this.PetBehavior.Owner != null && !other.IsUnconscious && !other.IsHandcuffed)
			{
				HumanAIController humanAI = other.AIController as HumanAIController;
				if (humanAI != null)
				{
					AIObjectiveCombat combat = humanAI.ObjectiveManager.CurrentObjective as AIObjectiveCombat;
					if (combat != null && combat.Enemy != null)
					{
						return combat.Enemy == this.PetBehavior.Owner;
					}
				}
			}
			return false;
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06000CBA RID: 3258 RVA: 0x0007604E File Offset: 0x0007424E
		// (set) Token: 0x06000CBB RID: 3259 RVA: 0x00076056 File Offset: 0x00074256
		public bool Reverse
		{
			get
			{
				return this.reverse;
			}
			private set
			{
				this.reverse = value;
				if (this.FishAnimController != null)
				{
					this.FishAnimController.Reverse = this.reverse;
				}
			}
		}

		// Token: 0x06000CBC RID: 3260 RVA: 0x00076078 File Offset: 0x00074278
		public EnemyAIController(Character c, string seed) : base(c)
		{
			if (c.IsHuman)
			{
				throw new Exception("Tried to create an enemy ai controller for human!");
			}
			ContentXElement mainElement = c.Params.OriginalElement.IsOverride() ? c.Params.OriginalElement.FirstElement() : c.Params.OriginalElement;
			this.targetMemories = new Dictionary<AITarget, AITargetMemory>();
			this.steeringManager = this.outsideSteering;
			bool targetOutposts;
			if (Level.Loaded == null || Level.Loaded.Type != LevelData.LevelType.Outpost)
			{
				Submarine mainSub = Submarine.MainSub;
				if (mainSub != null)
				{
					SubmarineInfo info = mainSub.Info;
					if (info != null)
					{
						targetOutposts = (info.Type == SubmarineType.Outpost);
						goto IL_1D3;
					}
				}
				targetOutposts = false;
			}
			else
			{
				targetOutposts = true;
			}
			IL_1D3:
			this.TargetOutposts = targetOutposts;
			List<XElement> aiElements = new List<XElement>();
			List<float> aiCommonness = new List<float>();
			foreach (ContentXElement element in mainElement.Elements())
			{
				if (element.Name.ToString().Equals("ai", StringComparison.OrdinalIgnoreCase))
				{
					aiElements.Add(element);
					aiCommonness.Add(element.GetAttributeFloat("commonness", 1f));
				}
			}
			if (aiElements.Count == 0)
			{
				string str = "Error in file \"";
				ContentPath path = c.Params.File.Path;
				string error = str + ((path != null) ? path.ToString() : null) + "\" - no AI element found.";
				Exception e = null;
				CharacterPrefab prefab = c.Prefab;
				DebugConsole.ThrowError(error, e, (prefab != null) ? prefab.ContentPackage : null, false, false);
				this.outsideSteering = new SteeringManager(this);
				this.insideSteering = new IndoorsSteeringManager(this, false, false);
				return;
			}
			MTRandom random = new MTRandom(ToolBox.StringToInt(seed));
			XElement aiElement = (aiElements.Count == 1) ? aiElements[0] : ToolBox.SelectWeightedRandom<XElement>(aiElements, aiCommonness, random);
			foreach (XElement subElement in aiElement.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "chooserandom")
				{
					IEnumerable<XElement> subElements = subElement.Elements();
					if (subElements.Any<XElement>())
					{
						this.<.ctor>g__LoadSubElement|128_0(subElements.ToArray<XElement>().GetRandom(random));
					}
				}
				else
				{
					this.<.ctor>g__LoadSubElement|128_0(subElement);
				}
			}
			if (this.PetBehavior == null)
			{
				Identifier group = this.Character.Group;
				if (!(group == "human"))
				{
					goto IL_38F;
				}
			}
			this.Character.TeamID = CharacterTeamType.FriendlyNPC;
			IL_38F:
			this.ReevaluateAttacks();
			this.outsideSteering = new SteeringManager(this);
			this.insideSteering = new IndoorsSteeringManager(this, this.AIParams.CanOpenDoors, this.canAttackDoors);
			this.steeringManager = this.outsideSteering;
			this.State = AIState.Idle;
			this.requiredHoleCount = (int)Math.Ceiling((double)(ConvertUnits.ToDisplayUnits(this.colliderWidth) / 96f));
			this.myBodies = (from l in this.Character.AnimController.Limbs
			select l.body.FarseerBody).ToList<Body>();
			this.myBodies.Add(this.Character.AnimController.Collider.FarseerBody);
			if (this.AIParams.PlayDeadProbability > 0f)
			{
				this.Character.EvaluatePlayDeadProbability(null);
			}
			CreatureMetrics.UnlockInEditor(this.Character.SpeciesName);
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06000CBD RID: 3261 RVA: 0x00076528 File Offset: 0x00074728
		public CharacterParams.AIParams AIParams
		{
			get
			{
				if (this._aiParams == null)
				{
					this._aiParams = this.Character.Params.AI;
					if (this._aiParams == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 1);
						defaultInterpolatedStringHandler.AppendLiteral("No AI Params defined for ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Character.SpeciesName);
						defaultInterpolatedStringHandler.AppendLiteral(". AI disabled.");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, this.Character.Prefab.ContentPackage, false, false);
						this.Enabled = false;
						this._aiParams = new CharacterParams.AIParams(null, this.Character.Params);
					}
				}
				return this._aiParams;
			}
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x000765D5 File Offset: 0x000747D5
		private IEnumerable<CharacterParams.TargetParams> GetTargetParams(Identifier targetTag)
		{
			return this.AIParams.GetTargets(targetTag);
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x000765E3 File Offset: 0x000747E3
		private IEnumerable<CharacterParams.TargetParams> GetTargetParams(IEnumerable<Identifier> targetingTags)
		{
			EnemyAIController.<GetTargetParams>d__133 <GetTargetParams>d__ = new EnemyAIController.<GetTargetParams>d__133(-2);
			<GetTargetParams>d__.<>4__this = this;
			<GetTargetParams>d__.<>3__targetingTags = targetingTags;
			return <GetTargetParams>d__;
		}

		// Token: 0x06000CC0 RID: 3264 RVA: 0x000765FC File Offset: 0x000747FC
		private IEnumerable<Identifier> GetTargetingTags(AITarget aiTarget)
		{
			this._targetingTags.Clear();
			if (((aiTarget != null) ? aiTarget.Entity : null) == null)
			{
				return this._targetingTags;
			}
			Character targetCharacter = aiTarget.Entity as Character;
			if (targetCharacter != null)
			{
				CharacterParams.TargetParams tp;
				CharacterParams.TargetParams tP;
				if (targetCharacter.IsDead)
				{
					this._targetingTags.Add(Tags.Dead);
				}
				else if (this.AIParams.TryGetHighestPriorityTarget(targetCharacter.CharacterHealth.GetActiveAfflictionTags(), out tp) && tp.Threshold >= this.Character.GetDamageDoneByAttacker(targetCharacter))
				{
					this._targetingTags.Add(tp.Tag);
				}
				else if (this.PetBehavior != null && aiTarget.Entity == this.PetBehavior.Owner)
				{
					this._targetingTags.Add(Tags.Owner);
				}
				else if (this.PetBehavior != null && (!this.Character.IsOnFriendlyTeam(targetCharacter) || this.IsAttackingOwner(targetCharacter)))
				{
					this._targetingTags.Add(Tags.Hostile);
				}
				else if (this.AIParams.TryGetHighestPriorityTarget(targetCharacter, out tP))
				{
					this._targetingTags.Add(tP.Tag);
				}
				else
				{
					EnemyAIController enemy = targetCharacter.AIController as EnemyAIController;
					if (enemy != null)
					{
						if (enemy.PetBehavior != null && (this.PetBehavior != null || this.AIParams.HasTag(Tags.Pet)))
						{
							this._targetingTags.Add(Tags.Pet);
						}
						else if (targetCharacter.IsHusk && this.AIParams.HasTag(Tags.Husk))
						{
							this._targetingTags.Add(Tags.Husk);
						}
						else if (!this.Character.IsSameSpeciesOrGroup(targetCharacter))
						{
							if (enemy.CombatStrength > this.CombatStrength)
							{
								this._targetingTags.Add(Tags.Stronger);
							}
							else if (enemy.CombatStrength < this.CombatStrength)
							{
								this._targetingTags.Add(Tags.Weaker);
							}
							else
							{
								this._targetingTags.Add(Tags.Equal);
							}
						}
					}
				}
			}
			else
			{
				Item targetItem = aiTarget.Entity as Item;
				if (targetItem != null)
				{
					foreach (CharacterParams.TargetParams prio in this.AIParams.Targets)
					{
						if (targetItem.HasTag(prio.Tag))
						{
							this._targetingTags.Add(prio.Tag);
						}
					}
					if (this._targetingTags.None(null))
					{
						if (targetItem.GetComponent<Sonar>() != null)
						{
							this._targetingTags.Add(Tags.Sonar);
						}
						if (targetItem.GetComponent<Door>() != null)
						{
							this._targetingTags.Add(Tags.Door);
						}
					}
				}
				else if (aiTarget.Entity is Structure)
				{
					this._targetingTags.Add(Tags.Wall);
				}
				else if (aiTarget.Entity is Hull)
				{
					this._targetingTags.Add(Tags.Room);
				}
			}
			return this._targetingTags;
		}

		// Token: 0x06000CC1 RID: 3265 RVA: 0x00076908 File Offset: 0x00074B08
		public override void SelectTarget(AITarget target)
		{
			this.SelectTarget(target, 100f);
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x00076916 File Offset: 0x00074B16
		public void SelectTarget(AITarget target, float priority)
		{
			base.SelectedAiTarget = target;
			this.currentTargetMemory = this.GetTargetMemory(target, true, false);
			this.currentTargetMemory.Priority = priority;
			this.ignoredTargets.Remove(target);
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x00076948 File Offset: 0x00074B48
		private void ReleaseDragTargets()
		{
			Limb attackLimb = this.AttackLimb;
			if (attackLimb != null)
			{
				Rope attachedRope = attackLimb.AttachedRope;
				if (attachedRope != null)
				{
					attachedRope.Snap();
				}
			}
			if (this.Character.Params.CanInteract && this.Character.Inventory != null)
			{
				this.Character.HeldItems.ForEach(delegate(Item i)
				{
					Holdable component = i.GetComponent<Holdable>();
					if (component == null)
					{
						return;
					}
					Rope rope = component.GetRope();
					if (rope == null)
					{
						return;
					}
					rope.Snap();
				});
			}
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x000769BF File Offset: 0x00074BBF
		public void EvaluatePlayDeadProbability(float? probability = null)
		{
			if (probability != null)
			{
				this.AIParams.PlayDeadProbability = probability.Value;
			}
			this.Character.AllowPlayDead = (Rand.Value(Rand.RandSync.Unsynced) <= this.AIParams.PlayDeadProbability);
		}

		// Token: 0x06000CC5 RID: 3269 RVA: 0x00076A00 File Offset: 0x00074C00
		public override void Update(float deltaTime)
		{
			if (EnemyAIController.DisableEnemyAI)
			{
				return;
			}
			base.Update(deltaTime);
			this.UpdateTriggers(deltaTime);
			this.Character.ClearInputs();
			base.IsTryingToSteerThroughGap = false;
			this.Reverse = false;
			this.Character.UpdateTeam();
			this.HandleLaddersAndPlatforms(deltaTime);
			if (Math.Abs(this.Character.AnimController.movement.X) > 0.1f && !this.Character.AnimController.InWater && (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer || Character.Controlled == this.Character))
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) != null || base.EscapeTarget != null)
				{
					AITarget selectedAiTarget2 = base.SelectedAiTarget;
					Entity t = ((selectedAiTarget2 != null) ? selectedAiTarget2.Entity : null) ?? base.EscapeTarget;
					float referencePos = (Vector2.DistanceSquared(this.Character.WorldPosition, t.WorldPosition) > 10000f && base.HasValidPath(true, true, null)) ? this.PathSteering.CurrentPath.CurrentNode.WorldPosition.X : t.WorldPosition.X;
					this.Character.AnimController.TargetDir = ((this.Character.WorldPosition.X < referencePos) ? Direction.Right : Direction.Left);
				}
				else
				{
					this.Character.AnimController.TargetDir = ((this.Character.AnimController.movement.X > 0f) ? Direction.Right : Direction.Left);
				}
			}
			if (this.isStateChanged && (this.State == AIState.Idle || this.State == AIState.Patrol))
			{
				this.stateResetTimer -= deltaTime;
				if (this.stateResetTimer <= 0f)
				{
					this.ResetOriginalState();
				}
			}
			if (this.targetIgnoreTimer > 0f)
			{
				this.targetIgnoreTimer -= deltaTime;
			}
			else
			{
				this.ignoredTargets.Clear();
				this.targetIgnoreTimer = this.targetIgnoreTime;
			}
			this.avoidTimer -= deltaTime;
			if (this.avoidTimer < 0f)
			{
				this.avoidTimer = 0f;
			}
			this.UpdateCurrentMemoryLocation();
			if (this.updateMemoriesTimer > 0f)
			{
				this.updateMemoriesTimer -= deltaTime;
			}
			else
			{
				this.FadeMemories(this.updateMemoriesInverval);
				this.updateMemoriesTimer = this.updateMemoriesInverval;
			}
			if (Math.Max(this.Character.HealthPercentage, 0f) < this.FleeHealthThreshold && base.SelectedAiTarget != null)
			{
				Character target = base.SelectedAiTarget.Entity as Character;
				if (target == null)
				{
					Item targetItem = base.SelectedAiTarget.Entity as Item;
					if (targetItem != null)
					{
						target = EnemyAIController.GetOwner(targetItem);
					}
				}
				bool shouldFlee = false;
				if (target != null)
				{
					shouldFlee = ((target.IsHuman && this.CanPerceive(base.SelectedAiTarget, -1f, -1f, false)) || this.IsBeingChasedBy(target));
				}
				this.State = (shouldFlee ? AIState.Flee : AIState.Idle);
				this.wallTarget = null;
				if (this.State != AIState.Flee)
				{
					base.SelectedAiTarget = null;
					this._lastAiTarget = null;
				}
			}
			else
			{
				if (EnemyAIController.TargetingRestrictions != this.previousTargetingRestrictions)
				{
					this.previousTargetingRestrictions = EnemyAIController.TargetingRestrictions;
					this.updateTargetsTimer = 0f;
					base.SelectedAiTarget = null;
				}
				if (this.updateTargetsTimer > 0f)
				{
					this.updateTargetsTimer -= deltaTime;
				}
				else if (this.avoidTimer <= 0f || (this.activeTriggers.Any<KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams>>() && this.returnTimer <= 0f))
				{
					this.UpdateTargets();
				}
			}
			if (this.Character.Params.UsePathFinding && this.AIParams.UsePathFindingToGetInside && this.AIParams.CanOpenDoors)
			{
				if (this.Character.Submarine != null || (base.HasValidPath(true, true, null) && this.<Update>g__IsCloseEnoughToTargetSub|141_0(this.maxSteeringBuffer)) || this.<Update>g__IsCloseEnoughToTargetSub|141_0(this.steeringBuffer))
				{
					if (this.steeringManager != this.insideSteering)
					{
						this.insideSteering.Reset();
					}
					this.steeringManager = this.insideSteering;
					this.steeringBuffer += this.steeringBufferIncreaseSpeed * deltaTime;
				}
				else
				{
					if (this.steeringManager != this.outsideSteering)
					{
						this.outsideSteering.Reset();
					}
					this.steeringManager = this.outsideSteering;
					this.steeringBuffer = this.minSteeringBuffer;
				}
				this.steeringBuffer = Math.Clamp(this.steeringBuffer, this.minSteeringBuffer, this.maxSteeringBuffer);
			}
			else if (this.Character.Submarine != null && this.Character.Params.UsePathFinding)
			{
				if (this.steeringManager != this.insideSteering)
				{
					this.insideSteering.Reset();
				}
				this.steeringManager = this.insideSteering;
			}
			else
			{
				if (this.steeringManager != this.outsideSteering)
				{
					this.outsideSteering.Reset();
				}
				this.steeringManager = this.outsideSteering;
			}
			bool useSteeringLengthAsMovementSpeed = this.State == AIState.Idle && this.Character.AnimController.InWater;
			bool run = false;
			switch (this.State)
			{
			case AIState.Idle:
				this.UpdateIdle(deltaTime, true);
				break;
			case AIState.Attack:
				run = (!this.IsCoolDownRunning || (this.AttackLimb != null && this.AttackLimb.attack.FullSpeedAfterAttack));
				this.UpdateAttack(deltaTime);
				break;
			case AIState.Escape:
			case AIState.Flee:
				run = true;
				this.Escape(deltaTime);
				break;
			case AIState.Eat:
				this.UpdateEating(deltaTime);
				break;
			case AIState.Avoid:
			case AIState.Aggressive:
			case AIState.PassiveAggressive:
			{
				AITarget selectedAiTarget3 = base.SelectedAiTarget;
				if (((selectedAiTarget3 != null) ? selectedAiTarget3.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
				{
					this.State = AIState.Idle;
					return;
				}
				float squaredDistance = Vector2.DistanceSquared(base.WorldPosition, base.SelectedAiTarget.WorldPosition);
				Limb attackLimb = this.AttackLimb ?? this.GetAttackLimb(base.SelectedAiTarget.WorldPosition, null);
				if (attackLimb != null && (double)squaredDistance <= Math.Pow((double)attackLimb.attack.Range, 2.0))
				{
					run = true;
					if (this.State == AIState.Avoid)
					{
						this.Escape(deltaTime);
					}
					else
					{
						this.UpdateAttack(deltaTime);
					}
				}
				else
				{
					bool isBeingChased = this.IsBeingChased;
					float num;
					if (!isBeingChased)
					{
						CharacterParams.TargetParams targetParams = this.currentTargetingParams;
						if (targetParams != null && targetParams.ReactDistance > 0f)
						{
							num = this.currentTargetingParams.ReactDistance;
							goto IL_6DC;
						}
					}
					num = this.GetPerceivingRange(base.SelectedAiTarget);
					IL_6DC:
					float reactDistance = num;
					if ((double)squaredDistance <= Math.Pow((double)reactDistance, 2.0))
					{
						float halfReactDistance = reactDistance / 2f;
						CharacterParams.TargetParams targetParams = this.currentTargetingParams;
						float attackDistance = (targetParams != null && targetParams.AttackDistance > 0f) ? this.currentTargetingParams.AttackDistance : halfReactDistance;
						if (this.State == AIState.Aggressive || (this.State == AIState.PassiveAggressive && (double)squaredDistance < Math.Pow((double)attackDistance, 2.0)))
						{
							run = true;
							this.UpdateAttack(deltaTime);
						}
						else
						{
							run = (isBeingChased || (double)squaredDistance < Math.Pow((double)halfReactDistance, 2.0));
							this.State = AIState.Escape;
							this.avoidTimer = this.AIParams.AvoidTime * 0.5f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
						}
					}
					else
					{
						this.UpdateIdle(deltaTime, true);
					}
				}
				break;
			}
			case AIState.Protect:
			case AIState.Follow:
			case AIState.FleeTo:
			case AIState.HideTo:
			case AIState.Hiding:
			{
				AITarget selectedAiTarget4 = base.SelectedAiTarget;
				if (((selectedAiTarget4 != null) ? selectedAiTarget4.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
				{
					this.State = AIState.Idle;
					return;
				}
				if (this.State == AIState.Protect)
				{
					EnemyAIController.<>c__DisplayClass141_0 CS$<>8__locals1 = new EnemyAIController.<>c__DisplayClass141_0();
					CS$<>8__locals1.<>4__this = this;
					Entity entity = base.SelectedAiTarget.Entity;
					CS$<>8__locals1.targetCharacter = (entity as Character);
					if (CS$<>8__locals1.targetCharacter != null)
					{
						Character.Attacker attacker2 = CS$<>8__locals1.targetCharacter.LastAttackers.LastOrDefault(new Func<Character.Attacker, bool>(CS$<>8__locals1.<Update>g__ShouldRetaliate|1));
						Character attacker = (attacker2 != null) ? attacker2.Character : null;
						if (((attacker != null) ? attacker.AiTarget : null) != null)
						{
							this.ChangeTargetState(attacker, AIState.Attack, new float?(this.currentTargetingParams.Priority * 2f));
							this.SelectTarget(attacker.AiTarget);
							this.State = AIState.Attack;
							this.UpdateWallTarget(this.requiredHoleCount);
							return;
						}
					}
				}
				float distX = Math.Abs(base.WorldPosition.X - base.SelectedAiTarget.WorldPosition.X);
				float distY = Math.Abs(base.WorldPosition.Y - base.SelectedAiTarget.WorldPosition.Y);
				if (this.Character.Submarine != null && distY > 50f)
				{
					Character targetC = base.SelectedAiTarget.Entity as Character;
					if (targetC != null && !base.VisibleHulls.Contains(targetC.CurrentHull))
					{
						distY *= 3f;
					}
				}
				float dist = distX + distY;
				float reactDist = this.GetPerceivingRange(base.SelectedAiTarget);
				Vector2 offset = Vector2.Zero;
				if (this.currentTargetingParams != null)
				{
					if (this.currentTargetingParams.ReactDistance > 0f)
					{
						reactDist = this.currentTargetingParams.ReactDistance;
					}
					offset = this.currentTargetingParams.Offset;
				}
				if (offset != Vector2.Zero)
				{
					reactDist += offset.Length();
				}
				if (dist > reactDist + this.movementMargin)
				{
					AIState state = this.State;
					bool flag = state == AIState.FleeTo || state - AIState.HideTo <= 1;
					this.movementMargin = (flag ? 0f : reactDist);
					if (this.State == AIState.Hiding)
					{
						this.State = AIState.HideTo;
					}
					run = true;
					this.UpdateFollow(deltaTime);
				}
				else
				{
					if (this.State == AIState.HideTo)
					{
						this.State = AIState.Hiding;
					}
					this.movementMargin = MathHelper.Clamp(this.movementMargin -= deltaTime, 0f, reactDist);
					AIState state = this.State;
					bool flag = state == AIState.FleeTo || state == AIState.Hiding;
					if (flag)
					{
						base.SteeringManager.Reset();
						this.Character.AnimController.TargetMovement = Vector2.Zero;
						if (this.Character.AnimController.InWater)
						{
							float force = this.Character.AnimController.Collider.Mass / 10f;
							this.Character.AnimController.Collider.MoveToPos(base.SelectedAiTarget.Entity.SimPosition + ConvertUnits.ToSimUnits(offset), force, null);
							Item item = base.SelectedAiTarget.Entity as Item;
							if (item != null)
							{
								float rotation = item.Rotation;
								this.Character.AnimController.Collider.SmoothRotate(rotation, this.Character.AnimController.SwimFastParams.SteerTorque, true);
								Limb mainLimb = this.Character.AnimController.MainLimb;
								if (mainLimb.type == LimbType.Head)
								{
									mainLimb.body.SmoothRotate(rotation, this.Character.AnimController.SwimFastParams.HeadTorque, true);
								}
								else
								{
									mainLimb.body.SmoothRotate(rotation, this.Character.AnimController.SwimFastParams.TorsoTorque, true);
								}
							}
							if (this.disableTailCoroutine == null)
							{
								Item i = base.SelectedAiTarget.Entity as Item;
								if (i != null && i.HasTag(Tags.GuardianShelter) && !CoroutineManager.IsCoroutineRunning(this.disableTailCoroutine))
								{
									this.disableTailCoroutine = CoroutineManager.Invoke(delegate
									{
										Character character = this.Character;
										if (character != null && !character.Removed)
										{
											this.Character.AnimController.HideAndDisable(LimbType.Tail, 0f, false);
										}
									}, 1f);
								}
							}
							this.Character.AnimController.ApplyPose(new Vector2(0f, -1f), new Vector2(0f, -1f), new Vector2(0f, -1f), new Vector2(0f, -1f), 1f);
						}
					}
					else
					{
						this.UpdateIdle(deltaTime, true);
					}
				}
				break;
			}
			case AIState.Observe:
			{
				AITarget selectedAiTarget5 = base.SelectedAiTarget;
				if (((selectedAiTarget5 != null) ? selectedAiTarget5.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
				{
					this.State = AIState.Idle;
					return;
				}
				run = false;
				float sqrDist = Vector2.DistanceSquared(base.WorldPosition, base.SelectedAiTarget.WorldPosition);
				CharacterParams.TargetParams targetParams = this.currentTargetingParams;
				float reactDist = (targetParams != null && targetParams.ReactDistance > 0f) ? this.currentTargetingParams.ReactDistance : this.GetPerceivingRange(base.SelectedAiTarget);
				float halfReactDist = reactDist / 2f;
				targetParams = this.currentTargetingParams;
				float attackDist = (targetParams != null && targetParams.AttackDistance > 0f) ? this.currentTargetingParams.AttackDistance : halfReactDist;
				if ((double)sqrDist > Math.Pow((double)reactDist, 2.0))
				{
					this.UpdateIdle(deltaTime, true);
				}
				else if ((double)sqrDist < Math.Pow((double)(attackDist + this.movementMargin), 2.0))
				{
					this.movementMargin = attackDist;
					base.SteeringManager.Reset();
					if (this.Character.AnimController.InWater)
					{
						useSteeringLengthAsMovementSpeed = true;
						Vector2 dir = Vector2.Normalize(base.SelectedAiTarget.WorldPosition - this.Character.WorldPosition);
						if ((double)sqrDist < Math.Pow((double)(attackDist * 0.75f), 2.0))
						{
							dir = -dir;
							useSteeringLengthAsMovementSpeed = false;
							this.Reverse = true;
							run = true;
						}
						base.SteeringManager.SteeringManual(deltaTime, dir * 0.2f);
					}
					else
					{
						base.FaceTarget(base.SelectedAiTarget.Entity);
					}
					this.observeTimer -= deltaTime;
					if (this.observeTimer < 0f)
					{
						this.IgnoreTarget(base.SelectedAiTarget);
						this.State = AIState.Idle;
						base.ResetAITarget();
					}
				}
				else
				{
					run = ((double)sqrDist > Math.Pow((double)(attackDist * 2f), 2.0));
					this.movementMargin = MathHelper.Clamp(this.movementMargin -= deltaTime, 0f, attackDist);
					this.UpdateFollow(deltaTime);
				}
				break;
			}
			case AIState.Freeze:
				base.SteeringManager.Reset();
				break;
			case AIState.Patrol:
				this.UpdatePatrol(deltaTime, true);
				break;
			case AIState.PlayDead:
				this.Character.IsRagdolled = true;
				break;
			default:
				throw new NotImplementedException();
			}
			if (!this.Character.AnimController.SimplePhysicsEnabled)
			{
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.Update(this, deltaTime);
				}
			}
			base.IsSteeringThroughGap = false;
			if (this.SwarmBehavior != null)
			{
				this.SwarmBehavior.IsActive = (this.SwarmBehavior.ForceActive || (this.State == AIState.Idle && this.Character.CurrentHull == null));
				this.SwarmBehavior.Refresh();
				this.SwarmBehavior.UpdateSteering(deltaTime);
			}
			this.SteerInsideLevel(deltaTime);
			float speed = this.Character.AnimController.GetCurrentSpeed(run && this.Character.CanRun);
			this.steeringManager.Update(Math.Max(speed, 1f));
			float movementSpeed = useSteeringLengthAsMovementSpeed ? base.Steering.Length() : speed;
			this.Character.AnimController.TargetMovement = this.Character.ApplyMovementLimits(base.Steering, movementSpeed);
			if (this.Character.CurrentHull != null && this.Character.AnimController.InWater)
			{
				this.Character.AnimController.TargetMovement = this.Character.AnimController.TargetMovement.ClampLength(5f);
			}
		}

		// Token: 0x06000CC6 RID: 3270 RVA: 0x000779C8 File Offset: 0x00075BC8
		private void HandleLaddersAndPlatforms(float deltaTime)
		{
			bool ignorePlatforms = this.Character.AnimController.TargetMovement.Y < -0.5f && -this.Character.AnimController.TargetMovement.Y > Math.Abs(this.Character.AnimController.TargetMovement.X);
			if (this.steeringManager == this.insideSteering)
			{
				SteeringPath currPath = this.PathSteering.CurrentPath;
				if (currPath != null)
				{
					WayPoint currentNode = currPath.CurrentNode;
					if (currentNode != null)
					{
						Vector2 colliderBottom = this.Character.AnimController.GetColliderBottom();
						if (this.Character.Submarine != currentNode.Submarine)
						{
							colliderBottom = Submarine.GetRelativeSimPosition(colliderBottom, currentNode.Submarine, this.Character.Submarine);
						}
						if (currentNode.SimPosition.Y < colliderBottom.Y)
						{
							float allowedJumpHeight = this.Character.AnimController.ImpactTolerance / 2f;
							Vector2 diff = currentNode.WorldPosition - this.Character.WorldPosition;
							float height = ConvertUnits.ToSimUnits(Math.Abs(diff.Y));
							ignorePlatforms = (height < allowedJumpHeight);
							if (ignorePlatforms && !this.Character.CanClimb && this.PathSteering.IsCurrentNodeLadder && ConvertUnits.ToSimUnits(Math.Abs(diff.X)) < this.Character.AnimController.Collider.GetMaxExtent())
							{
								if (this.lastDroppingTime < Timing.TotalTime - 5.0)
								{
									this.Character.IsRagdolled = true;
									this.Character.SetInput(InputType.Ragdoll, false, true);
									this.droppingTimer += deltaTime;
									if (this.droppingTimer > 1f)
									{
										this.lastDroppingTime = Timing.TotalTime;
									}
								}
								else
								{
									this.droppingTimer = 0f;
								}
							}
						}
					}
				}
				if (this.Character.IsClimbing && this.PathSteering.IsNextLadderSameAsCurrent)
				{
					this.Character.AnimController.TargetMovement = new Vector2(0f, (float)Math.Sign(this.Character.AnimController.TargetMovement.Y));
				}
			}
			this.Character.AnimController.IgnorePlatforms = ignorePlatforms;
		}

		// Token: 0x06000CC7 RID: 3271 RVA: 0x00077C08 File Offset: 0x00075E08
		private void UpdateIdle(float deltaTime, bool followLastTarget = true)
		{
			if (this.Character.AllowPlayDead && this.Character.Submarine != null)
			{
				if (this.playDeadTimer > 0f)
				{
					this.playDeadTimer -= deltaTime;
				}
				else
				{
					this.State = AIState.PlayDead;
				}
			}
			else if (this.AIParams.PatrolFlooded || this.AIParams.PatrolDry)
			{
				this.State = AIState.Patrol;
			}
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (pathSteering == null && Level.Loaded != null && Level.Loaded.GetRealWorldDepth(base.WorldPosition.Y) > this.Character.CharacterHealth.CrushDepth * 0.75f)
			{
				base.SteeringManager.SteeringManual(deltaTime, Vector2.UnitY);
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 5f);
				return;
			}
			if (followLastTarget)
			{
				AITarget target = base.SelectedAiTarget ?? this._lastAiTarget;
				Entity entity = (target != null) ? target.Entity : null;
				if (entity != null && !entity.Removed && this.PreviousState == AIState.Attack && this.Character.CurrentHull == null)
				{
					Limb previousAttackLimb = this._previousAttackLimb;
					if (((previousAttackLimb != null) ? previousAttackLimb.attack : null) != null)
					{
						Limb previousAttackLimb2 = this._previousAttackLimb;
						Attack previousAttack = (previousAttackLimb2 != null) ? previousAttackLimb2.attack : null;
						if (previousAttack == null || (previousAttack.AfterAttack == AIBehaviorAfterAttack.FallBack && previousAttack.CoolDownTimer > 0f))
						{
							goto IL_1F3;
						}
					}
					AITargetMemory memory = this.GetTargetMemory(target, false, false);
					if (memory != null)
					{
						Vector2 location = memory.Location;
						float dist = Vector2.DistanceSquared(base.WorldPosition, location);
						Vector2 vector;
						if (dist >= 2500f && this.IsPositionInsideAllowedZone(base.WorldPosition, out vector))
						{
							base.SteeringManager.SteeringSeek(this.Character.GetRelativeSimPosition(target.Entity, new Vector2?(location)), 5f);
							base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
							return;
						}
						base.ResetAITarget();
					}
					else
					{
						base.ResetAITarget();
					}
				}
			}
			IL_1F3:
			if (!this.Character.IsClimbing)
			{
				if (pathSteering != null && !this.Character.AnimController.InWater)
				{
					pathSteering.Wander(deltaTime, Math.Max(ConvertUnits.ToDisplayUnits(this.colliderLength), 100f), false);
					return;
				}
				this.steeringManager.SteeringWander(1f, true);
				if (this.Character.AnimController.InWater)
				{
					base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 5f);
				}
			}
		}

		// Token: 0x06000CC8 RID: 3272 RVA: 0x00077E84 File Offset: 0x00076084
		private void UpdatePatrol(float deltaTime, bool followLastTarget = true)
		{
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				if (this.patrolTarget == null || base.IsCurrentPathUnreachable || base.IsCurrentPathFinished)
				{
					this.newPatrolTargetTimer = Math.Min(this.newPatrolTargetTimer, this.newPatrolTargetIntervalMin);
				}
				if (this.newPatrolTargetTimer > 0f)
				{
					this.newPatrolTargetTimer -= deltaTime;
				}
				else if (!this.searchingNewHull)
				{
					this.searchingNewHull = true;
					this.FindTargetHulls();
				}
				else if (this.targetHulls.Any<Hull>())
				{
					this.patrolTarget = ToolBox.SelectWeightedRandom<Hull>(this.targetHulls, this.hullWeights, Rand.RandSync.Unsynced);
					SteeringPath path = this.PathSteering.PathFinder.FindPath(this.Character.SimPosition, this.patrolTarget.SimPosition, this.Character.Submarine, null, this.minGapSize * 1.5f, null, null, (PathNode n) => this.<UpdatePatrol>g__PatrolNodeFilter|156_0(n), true, 0f);
					if (path.Unreachable)
					{
						int index = this.targetHulls.IndexOf(this.patrolTarget);
						this.targetHulls.RemoveAt(index);
						this.hullWeights.RemoveAt(index);
						this.PathSteering.Reset();
						this.patrolTarget = null;
						this.patrolTimerMargin += 0.5f;
						this.patrolTimerMargin = Math.Min(this.patrolTimerMargin, this.newPatrolTargetIntervalMin);
						this.newPatrolTargetTimer = Math.Min(this.newPatrolTargetIntervalMin, this.patrolTimerMargin);
					}
					else
					{
						this.PathSteering.SetPath(this.patrolTarget.SimPosition, path);
						this.patrolTimerMargin = 0f;
						this.newPatrolTargetTimer = this.newPatrolTargetIntervalMax * Rand.Range(0.5f, 1.5f, Rand.RandSync.Unsynced);
						this.searchingNewHull = false;
					}
				}
				else
				{
					this.newPatrolTargetTimer = this.newPatrolTargetIntervalMax;
					this.searchingNewHull = false;
				}
				if (this.patrolTarget != null)
				{
					SteeringPath currentPath = pathSteering.CurrentPath;
					if (currentPath != null && !currentPath.Finished && !currentPath.Unreachable)
					{
						pathSteering.SteeringSeek(this.Character.GetRelativeSimPosition(this.patrolTarget, null), 1f, this.minGapSize * 1.5f, null, null, new Func<PathNode, bool>(this.<UpdatePatrol>g__PatrolNodeFilter|156_0), true, 0f);
						return;
					}
				}
			}
			this.UpdateIdle(deltaTime, followLastTarget);
		}

		// Token: 0x06000CC9 RID: 3273 RVA: 0x000780DC File Offset: 0x000762DC
		private void FindTargetHulls()
		{
			if (this.Character.Submarine == null)
			{
				return;
			}
			if (this.Character.CurrentHull == null)
			{
				return;
			}
			this.targetHulls.Clear();
			this.hullWeights.Clear();
			float hullMinSize = ConvertUnits.ToDisplayUnits(Math.Max(this.colliderLength, this.colliderWidth) * 2f);
			bool checkWaterLevel = !this.AIParams.PatrolFlooded || !this.AIParams.PatrolDry;
			foreach (Hull hull in Hull.HullList)
			{
				if (hull.Submarine != null && hull.Submarine.TeamID == this.Character.Submarine.TeamID && this.Character.Submarine.IsConnectedTo(hull.Submarine) && (float)hull.RectWidth >= hullMinSize && (float)hull.RectHeight >= hullMinSize && (!checkWaterLevel || ((!this.AIParams.PatrolDry || hull.WaterPercentage <= 50f) && (!this.AIParams.PatrolFlooded || hull.WaterPercentage >= 80f))) && (!this.AIParams.PatrolDry || hull.WaterPercentage >= 80f || Math.Abs(this.Character.CurrentHull.WorldPosition.Y - hull.WorldPosition.Y) <= this.Character.CurrentHull.CeilingHeight / 2f) && !this.targetHulls.Contains(hull))
				{
					this.targetHulls.Add(hull);
					float weight = hull.Size.Combine();
					float dist = Vector2.Distance(this.Character.WorldPosition, hull.WorldPosition);
					float optimal = 1000f;
					float max = 3000f;
					float distanceFactor = (dist > optimal) ? MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(optimal, max, dist)) : MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, optimal, dist));
					float waterFactor = 1f;
					if (checkWaterLevel)
					{
						waterFactor = (this.AIParams.PatrolDry ? MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(0f, 100f, hull.WaterPercentage)) : MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 100f, hull.WaterPercentage)));
					}
					weight *= distanceFactor * waterFactor;
					this.hullWeights.Add(weight);
				}
			}
		}

		// Token: 0x06000CCA RID: 3274 RVA: 0x000783AC File Offset: 0x000765AC
		private bool IsSameTarget(AITarget target, AITarget otherTarget)
		{
			return ((target != null) ? target.Entity : null) == ((otherTarget != null) ? otherTarget.Entity : null) || (EnemyAIController.<IsSameTarget>g__IsItemInCharacterInventory|162_0(target, otherTarget) || EnemyAIController.<IsSameTarget>g__IsItemInCharacterInventory|162_0(otherTarget, target));
		}

		// Token: 0x06000CCB RID: 3275 RVA: 0x000783E0 File Offset: 0x000765E0
		private void UpdateAttack(float deltaTime)
		{
			EnemyAIController.<>c__DisplayClass163_0 CS$<>8__locals1 = new EnemyAIController.<>c__DisplayClass163_0();
			CS$<>8__locals1.<>4__this = this;
			AITarget selectedAiTarget = base.SelectedAiTarget;
			if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == null || base.SelectedAiTarget.Entity.Removed || this.currentTargetingParams == null)
			{
				this.State = AIState.Idle;
				return;
			}
			if (this.Character.IsAttachedToController())
			{
				return;
			}
			this.attackWorldPos = base.SelectedAiTarget.WorldPosition;
			this.attackSimPos = base.SelectedAiTarget.SimPosition;
			Item item = base.SelectedAiTarget.Entity as Item;
			if (item != null)
			{
				Character owner = EnemyAIController.GetOwner(item);
				if (owner != null)
				{
					if (this.Character.IsFriendly(owner) || owner.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI))
					{
						base.ResetAITarget();
						this.State = AIState.Idle;
						return;
					}
					base.SelectedAiTarget = owner.AiTarget;
				}
			}
			if (this.wallTarget != null)
			{
				this.attackWorldPos = this.wallTarget.Position;
				if (this.wallTarget.Structure.Submarine != null)
				{
					this.attackWorldPos += this.wallTarget.Structure.Submarine.Position;
				}
				this.attackSimPos = ((this.Character.Submarine == this.wallTarget.Structure.Submarine) ? this.wallTarget.Position : this.attackWorldPos);
				this.attackSimPos = ConvertUnits.ToSimUnits(this.attackSimPos);
			}
			else
			{
				this.attackSimPos = this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null);
			}
			if (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.True)
			{
				if (this.TrySteerThroughGaps(deltaTime))
				{
					return;
				}
			}
			else
			{
				Structure w = base.SelectedAiTarget.Entity as Structure;
				if (w != null && this.wallTarget == null)
				{
					bool isBroken = true;
					for (int i = 0; i < w.Sections.Length; i++)
					{
						if (!w.SectionBodyDisabled(i))
						{
							isBroken = false;
							Vector2 sectionPos = w.SectionPosition(i, true);
							this.attackWorldPos = sectionPos;
							this.attackSimPos = ConvertUnits.ToSimUnits(this.attackWorldPos);
							break;
						}
					}
					if (isBroken)
					{
						this.IgnoreTarget(base.SelectedAiTarget);
						this.State = AIState.Idle;
						base.ResetAITarget();
						return;
					}
				}
			}
			this.attackLimbSelectionTimer -= deltaTime;
			if (this.AttackLimb == null || this.attackLimbSelectionTimer <= 0f)
			{
				this.attackLimbSelectionTimer = this.attackLimbSelectionInterval * Rand.Range(0.9f, 1.1f, Rand.RandSync.Unsynced);
				if (!this.IsAttackRunning && !this.IsCoolDownRunning)
				{
					this.AttackLimb = this.GetAttackLimb(this.attackWorldPos, null);
				}
			}
			CS$<>8__locals1.targetCharacter = (base.SelectedAiTarget.Entity as Character);
			IDamageable damageable;
			if (this.wallTarget == null)
			{
				damageable = (base.SelectedAiTarget.Entity as IDamageable);
			}
			else
			{
				IDamageable structure = this.wallTarget.Structure;
				damageable = structure;
			}
			IDamageable damageTarget = damageable;
			CS$<>8__locals1.canAttack = !this.Character.IsClimbing;
			bool pursue = false;
			if (this.IsCoolDownRunning && (this._previousAttackLimb == null || this.AttackLimb == null || this.AttackLimb.attack.CoolDownTimer > 0f))
			{
				Limb currentAttackLimb = this.AttackLimb ?? this._previousAttackLimb;
				if (currentAttackLimb.attack.CoolDownTimer >= currentAttackLimb.attack.CoolDown + currentAttackLimb.attack.CurrentRandomCoolDown - currentAttackLimb.attack.AfterAttackDelay)
				{
					return;
				}
				currentAttackLimb.attack.AfterAttackTimer += deltaTime;
				AIBehaviorAfterAttack activeBehavior = (currentAttackLimb.attack.AfterAttackSecondaryDelay > 0f && currentAttackLimb.attack.AfterAttackTimer > currentAttackLimb.attack.AfterAttackSecondaryDelay) ? currentAttackLimb.attack.AfterAttackSecondary : currentAttackLimb.attack.AfterAttack;
				switch (activeBehavior)
				{
				case AIBehaviorAfterAttack.FallBackUntilCanAttack:
				case AIBehaviorAfterAttack.FollowThroughUntilCanAttack:
				case AIBehaviorAfterAttack.ReverseUntilCanAttack:
				{
					if (activeBehavior == AIBehaviorAfterAttack.ReverseUntilCanAttack)
					{
						this.Reverse = true;
					}
					if (currentAttackLimb.attack.SecondaryCoolDown <= 0f)
					{
						this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
						return;
					}
					if (currentAttackLimb.attack.SecondaryCoolDownTimer > 0f)
					{
						this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
						return;
					}
					if (this._previousAiTarget != null && !this.IsSameTarget(base.SelectedAiTarget, this._previousAiTarget))
					{
						this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
						return;
					}
					Limb newLimb = this.GetAttackLimb(this.attackWorldPos, currentAttackLimb);
					if (newLimb != null)
					{
						this.AttackLimb = newLimb;
						goto IL_699;
					}
					this.UpdateFallBack(this.attackWorldPos, deltaTime, activeBehavior == AIBehaviorAfterAttack.FollowThroughUntilCanAttack, false, true);
					return;
				}
				case AIBehaviorAfterAttack.PursueIfCanAttack:
				case AIBehaviorAfterAttack.Pursue:
					if (currentAttackLimb.attack.SecondaryCoolDown <= 0f)
					{
						if (activeBehavior == AIBehaviorAfterAttack.Pursue)
						{
							CS$<>8__locals1.canAttack = false;
							pursue = true;
							goto IL_699;
						}
						this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
						return;
					}
					else
					{
						if (currentAttackLimb.attack.SecondaryCoolDownTimer > 0f)
						{
							CS$<>8__locals1.canAttack = false;
							goto IL_699;
						}
						if (this._previousAiTarget != null && !this.IsSameTarget(base.SelectedAiTarget, this._previousAiTarget))
						{
							CS$<>8__locals1.canAttack = false;
							if (activeBehavior == AIBehaviorAfterAttack.PursueIfCanAttack)
							{
								this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
								return;
							}
							this.AttackLimb = null;
							goto IL_699;
						}
						else
						{
							Limb newLimb2 = this.GetAttackLimb(this.attackWorldPos, currentAttackLimb);
							if (newLimb2 != null)
							{
								this.AttackLimb = newLimb2;
								goto IL_699;
							}
							if (activeBehavior == AIBehaviorAfterAttack.Pursue)
							{
								CS$<>8__locals1.canAttack = false;
								pursue = true;
								goto IL_699;
							}
							this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
							return;
						}
					}
					break;
				case AIBehaviorAfterAttack.Eat:
					if (currentAttackLimb.IsSevered)
					{
						this.ReleaseEatingTarget();
						return;
					}
					this.UpdateEating(deltaTime);
					return;
				case AIBehaviorAfterAttack.FollowThrough:
					this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, true);
					return;
				case AIBehaviorAfterAttack.FollowThroughWithoutObstacleAvoidance:
					this.UpdateFallBack(this.attackWorldPos, deltaTime, true, false, false);
					return;
				case AIBehaviorAfterAttack.IdleUntilCanAttack:
				{
					if (currentAttackLimb.attack.SecondaryCoolDown <= 0f)
					{
						this.UpdateIdle(deltaTime, false);
						return;
					}
					if (currentAttackLimb.attack.SecondaryCoolDownTimer > 0f)
					{
						this.UpdateIdle(deltaTime, false);
						return;
					}
					if (this._previousAiTarget != null && !this.IsSameTarget(base.SelectedAiTarget, this._previousAiTarget))
					{
						this.UpdateIdle(deltaTime, false);
						return;
					}
					Limb newLimb3 = this.GetAttackLimb(this.attackWorldPos, currentAttackLimb);
					if (newLimb3 != null)
					{
						this.AttackLimb = newLimb3;
						goto IL_699;
					}
					this.UpdateIdle(deltaTime, false);
					return;
				}
				}
				if (activeBehavior == AIBehaviorAfterAttack.Reverse)
				{
					this.Reverse = true;
				}
				this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
				return;
			}
			else
			{
				this.attackVector = null;
			}
			IL_699:
			if (CS$<>8__locals1.canAttack)
			{
				Limb attackLimb2 = this.AttackLimb;
				if (attackLimb2 != null && attackLimb2.IsSevered)
				{
					this.AttackLimb = null;
				}
				if (this.AttackLimb != null)
				{
					Limb attackLimb3 = this.AttackLimb;
					IEnumerable<AttackContext> attackContexts = this.Character.GetAttackContexts();
					AITarget selectedAiTarget2 = base.SelectedAiTarget;
					if (this.IsValidAttack(attackLimb3, attackContexts, (selectedAiTarget2 != null) ? selectedAiTarget2.Entity : null))
					{
						goto IL_706;
					}
				}
				this.AttackLimb = this.GetAttackLimb(this.attackWorldPos, null);
				IL_706:
				CS$<>8__locals1.canAttack = (this.AttackLimb != null && this.AttackLimb.attack.CoolDownTimer <= 0f);
			}
			if (!this.AIParams.CanOpenDoors && !this.Character.AnimController.SimplePhysicsEnabled && base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null && (!this.canAttackDoors || !this.canAttackWalls || !this.AIParams.TargetOuterWalls) && this.wallTarget == null && Vector2.DistanceSquared(this.Character.WorldPosition, this.attackWorldPos) < 4000000f)
			{
				Vector2 rayStart = base.SimPosition;
				if (this.Character.Submarine == null)
				{
					rayStart -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				Vector2 dir = base.SelectedAiTarget.WorldPosition - base.WorldPosition;
				Vector2 rayEnd = rayStart + dir.ClampLength(this.Character.AnimController.Collider.GetLocalFront(null).Length() * 2f);
				Body closestBody = Submarine.CheckVisibility(rayStart, rayEnd, false, true, true, true, true, null);
				if (Submarine.LastPickedFraction != 1f && closestBody != null)
				{
					if (!this.AIParams.TargetOuterWalls || !this.canAttackWalls)
					{
						Structure s = closestBody.UserData as Structure;
						if (s != null && s.Submarine != null)
						{
							goto IL_8CC;
						}
					}
					if (this.canAttackDoors)
					{
						goto IL_8E6;
					}
					Item j = closestBody.UserData as Item;
					if (j == null || j.Submarine == null || j.GetComponent<Door>() == null)
					{
						goto IL_8E6;
					}
					IL_8CC:
					this.State = AIState.Idle;
					this.IgnoreTarget(base.SelectedAiTarget);
					base.ResetAITarget();
					return;
				}
			}
			IL_8E6:
			float distance = 0f;
			Limb attackTargetLimb = null;
			if (CS$<>8__locals1.canAttack)
			{
				if (!this.Character.AnimController.SimplePhysicsEnabled && this.wallTarget == null && CS$<>8__locals1.targetCharacter != null)
				{
					LimbType targetLimbType = this.AttackLimb.Params.Attack.Attack.TargetLimbType;
					attackTargetLimb = this.GetTargetLimb(this.AttackLimb, CS$<>8__locals1.targetCharacter, targetLimbType);
					if (attackTargetLimb == null)
					{
						this.State = AIState.Idle;
						this.IgnoreTarget(base.SelectedAiTarget);
						base.ResetAITarget();
						return;
					}
					this.attackWorldPos = attackTargetLimb.WorldPosition;
					this.attackSimPos = this.Character.GetRelativeSimPosition(attackTargetLimb, null);
				}
				Vector2 attackLimbPos = this.Character.AnimController.SimplePhysicsEnabled ? this.Character.WorldPosition : this.AttackLimb.WorldPosition;
				Vector2 toTarget = this.attackWorldPos - attackLimbPos;
				EnemyAIController.<>c__DisplayClass163_1 CS$<>8__locals2;
				CS$<>8__locals2.toTargetOffset = toTarget;
				if (this.Character.Submarine == null)
				{
					if (this.wallTarget != null)
					{
						if (this.wallTarget.Structure.Submarine != null)
						{
							Vector2 margin = CS$<>8__locals1.<UpdateAttack>g__CalculateMargin|1(this.wallTarget.Structure.Submarine.Velocity, ref CS$<>8__locals2);
							CS$<>8__locals2.toTargetOffset += margin;
						}
					}
					else if (CS$<>8__locals1.targetCharacter != null)
					{
						Vector2 margin2 = CS$<>8__locals1.<UpdateAttack>g__CalculateMargin|1(CS$<>8__locals1.targetCharacter.AnimController.Collider.LinearVelocity, ref CS$<>8__locals2);
						CS$<>8__locals2.toTargetOffset += margin2;
					}
					else
					{
						Entity entity = base.SelectedAiTarget.Entity;
						MapEntity e = entity as MapEntity;
						if (e != null && entity.Submarine != null)
						{
							Vector2 margin3 = CS$<>8__locals1.<UpdateAttack>g__CalculateMargin|1(e.Submarine.Velocity, ref CS$<>8__locals2);
							CS$<>8__locals2.toTargetOffset += margin3;
						}
					}
				}
				distance = CS$<>8__locals2.toTargetOffset.Length();
				if (CS$<>8__locals1.canAttack)
				{
					CS$<>8__locals1.canAttack = (distance < this.AttackLimb.attack.Range);
				}
				if (CS$<>8__locals1.canAttack && !this.Character.InWater && this.Character.AnimController.CanWalk && !this.Character.IsFacing(this.attackWorldPos))
				{
					CS$<>8__locals1.canAttack = false;
				}
				if (CS$<>8__locals1.canAttack)
				{
					this.reachTimer = 0f;
					if (this.IsAggressiveBoarder)
					{
						Item k = base.SelectedAiTarget.Entity as Item;
						if (k != null)
						{
							Door component = k.GetComponent<Door>();
							if (component != null && component.CanBeTraversed)
							{
								CS$<>8__locals1.canAttack = false;
							}
						}
					}
				}
				else if (this.currentTargetingParams.AttackPattern == AttackPattern.Straight && distance < this.AttackLimb.attack.Range * 5f)
				{
					Vector2 targetVelocity = Vector2.Zero;
					Submarine targetSub = base.SelectedAiTarget.Entity.Submarine;
					if (targetSub != null)
					{
						targetVelocity = targetSub.Velocity;
					}
					else if (CS$<>8__locals1.targetCharacter != null)
					{
						targetVelocity = CS$<>8__locals1.targetCharacter.AnimController.Collider.LinearVelocity;
					}
					else
					{
						Item l = base.SelectedAiTarget.Entity as Item;
						if (l != null && l.body != null)
						{
							targetVelocity = l.body.LinearVelocity;
						}
					}
					float mySpeed = this.Character.AnimController.Collider.LinearVelocity.LengthSquared();
					float targetSpeed = targetVelocity.LengthSquared();
					if (mySpeed < 0.1f || mySpeed > targetSpeed)
					{
						this.reachTimer += deltaTime;
						if (this.reachTimer > 10f)
						{
							this.reachTimer = 0f;
							this.IgnoreTarget(base.SelectedAiTarget);
							this.State = AIState.Idle;
							base.ResetAITarget();
							return;
						}
					}
				}
				HumanoidAnimController humanoidAnimController = this.Character.AnimController as HumanoidAnimController;
				if (humanoidAnimController != null && distance < this.AttackLimb.attack.Range * 2f)
				{
					Character targetCharacter = CS$<>8__locals1.targetCharacter;
					Hull targetHull = (targetCharacter != null) ? targetCharacter.CurrentHull : null;
					if (targetHull != null && targetHull == this.Character.CurrentHull && toTarget.Y < 0f && Math.Abs(toTarget.Y) > this.AttackLimb.attack.Range / 2f && Math.Abs(toTarget.X) <= this.AttackLimb.attack.Range)
					{
						humanoidAnimController.Crouch();
					}
				}
				if (CS$<>8__locals1.canAttack)
				{
					if (this.AttackLimb.attack.Ranged)
					{
						float offset = this.AttackLimb.Params.GetSpriteOrientation() - 1.5707964f;
						Vector2 forward = VectorExtensions.Forward(this.AttackLimb.body.TransformedRotation - offset * this.Character.AnimController.Dir, 1f);
						float angle = forward.Angle(toTarget);
						CS$<>8__locals1.canAttack = (angle < MathHelper.ToRadians(this.AttackLimb.attack.RequiredAngle));
						if (CS$<>8__locals1.canAttack && this.AttackLimb.attack.AvoidFriendlyFire)
						{
							CS$<>8__locals1.canAttack = !CS$<>8__locals1.<UpdateAttack>g__IsBlocked|2(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null));
						}
					}
					else if (this.wallTarget == null && this.Character.CurrentHull != null && CS$<>8__locals1.targetCharacter != null)
					{
						CS$<>8__locals1.canAttack = (Submarine.PickBody(base.SimPosition, this.attackSimPos, null, new Category?(Category.Cat1), true, null, false) == null);
					}
				}
			}
			CS$<>8__locals1.steeringLimb = ((CS$<>8__locals1.canAttack && !this.AttackLimb.attack.Ranged) ? this.AttackLimb : null);
			bool updateSteering = true;
			if (CS$<>8__locals1.steeringLimb == null)
			{
				CS$<>8__locals1.steeringLimb = (this.Character.AnimController.GetLimb(LimbType.Head, true, false, false) ?? this.Character.AnimController.GetLimb(LimbType.Torso, true, false, false));
			}
			if (CS$<>8__locals1.steeringLimb == null)
			{
				this.State = AIState.Idle;
				return;
			}
			IndoorsSteeringManager pathSteering = base.SteeringManager as IndoorsSteeringManager;
			if (this.AttackLimb != null && this.AttackLimb.attack.Retreat)
			{
				this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
			}
			else if (pathSteering != null)
			{
				if (this.canAttackDoors && base.HasValidPath(true, true, null) && (CS$<>8__locals1.targetCharacter == null || CS$<>8__locals1.targetCharacter.CurrentHull != this.Character.CurrentHull))
				{
					WayPoint currentNode = pathSteering.CurrentPath.CurrentNode;
					Door door2;
					if ((door2 = ((currentNode != null) ? currentNode.ConnectedDoor : null)) == null)
					{
						WayPoint nextNode = pathSteering.CurrentPath.NextNode;
						door2 = ((nextNode != null) ? nextNode.ConnectedDoor : null);
					}
					Door door = door2;
					if (door != null && !door.CanBeTraversed && (!this.Character.IsInFriendlySub || !door.HasAccess(this.Character)) && door.Item.AiTarget != null && base.SelectedAiTarget != door.Item.AiTarget)
					{
						this.SelectTarget(door.Item.AiTarget, this.currentTargetMemory.Priority);
						this.State = AIState.Attack;
						this.AttackLimb = null;
						return;
					}
				}
				float max = 300f;
				float margin4 = (this.AttackLimb != null) ? Math.Min(this.AttackLimb.attack.Range * 0.9f, max) : max;
				if ((!CS$<>8__locals1.canAttack || distance > margin4) && !base.IsTryingToSteerThroughGap)
				{
					bool useManualSteering = false;
					if (this.Character.CurrentHull != null && this.Character.Submarine != null && !this.Character.Submarine.Info.IsRuin && (this.Character.AnimController.InWater || pursue || !this.Character.AnimController.CanWalk) && CS$<>8__locals1.targetCharacter != null && base.VisibleHulls.Contains(CS$<>8__locals1.targetCharacter.CurrentHull) && this.CanSeeTarget(CS$<>8__locals1.targetCharacter))
					{
						useManualSteering = true;
					}
					if (useManualSteering)
					{
						Vector2 myPos = this.Character.AnimController.SimplePhysicsEnabled ? this.Character.SimPosition : CS$<>8__locals1.steeringLimb.SimPosition;
						base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(this.attackSimPos - myPos));
					}
					else
					{
						Func<PathNode, bool> nodeFilter = null;
						float outsideNodePenalty = 0f;
						if (this.Character.CurrentHull != null && this.Character.IsInPlayerSub)
						{
							outsideNodePenalty = 50f;
						}
						pathSteering.SteeringSeek(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null), 2f, this.minGapSize, (PathNode n) => n.Waypoint.CurrentHull == null == (CS$<>8__locals1.<>4__this.Character.CurrentHull == null), null, nodeFilter, true, outsideNodePenalty);
						if (pathSteering.CurrentPath != null)
						{
							if (pathSteering.IsPathDirty)
							{
								Hull hull = this.Character.CurrentHull;
								if (hull != null)
								{
									if (hull.ConnectedGaps.Any((Gap g) => !g.IsRoomToRoom && g.Open >= 1f && g.ConnectedDoor != null))
									{
										base.SteeringManager.Reset();
									}
								}
								base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(base.SelectedAiTarget.Entity.WorldPosition - this.Character.WorldPosition));
							}
							else if (pathSteering.CurrentPath.Unreachable)
							{
								this.State = AIState.Idle;
								this.IgnoreTarget(base.SelectedAiTarget);
								base.ResetAITarget();
								return;
							}
						}
					}
				}
				else if (!base.IsTryingToSteerThroughGap)
				{
					if (this.AttackLimb.attack.Ranged)
					{
						float dir2 = this.Character.AnimController.Dir;
						if ((dir2 > 0f && this.attackWorldPos.X > this.AttackLimb.WorldPosition.X + margin4) || (dir2 < 0f && this.attackWorldPos.X < this.AttackLimb.WorldPosition.X - margin4))
						{
							base.SteeringManager.Reset();
						}
						else
						{
							this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
						}
					}
					else
					{
						base.SteeringManager.Reset();
					}
				}
				else
				{
					base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(base.SelectedAiTarget.Entity.WorldPosition - this.Character.WorldPosition));
				}
			}
			else
			{
				Vector2 steerPos = this.attackSimPos;
				if (!this.Character.AnimController.SimplePhysicsEnabled)
				{
					Vector2 offset2 = this.Character.SimPosition - CS$<>8__locals1.steeringLimb.SimPosition;
					steerPos += offset2;
				}
				if (this.Character.CurrentHull == null)
				{
					AttackPattern attackPattern = this.currentTargetingParams.AttackPattern;
					if (attackPattern != AttackPattern.Sweep)
					{
						if (attackPattern == AttackPattern.Circle)
						{
							if (!this.IsCoolDownRunning && (!this.IsAttackRunning || this.CirclePhase == CirclePhase.Strike) && this.currentTargetingParams != null)
							{
								Entity entity2 = base.SelectedAiTarget.Entity;
								EnemyAIController.<>c__DisplayClass163_2 CS$<>8__locals3;
								CS$<>8__locals3.targetSub = ((entity2 != null) ? entity2.Submarine : null);
								ISpatialEntity spatialTarget = CS$<>8__locals3.targetSub ?? base.SelectedAiTarget.Entity;
								float targetSize = 0f;
								if (!this.currentTargetingParams.IgnoreTargetSize)
								{
									targetSize = ((CS$<>8__locals3.targetSub != null) ? ((float)(Math.Max(CS$<>8__locals3.targetSub.Borders.Width, CS$<>8__locals3.targetSub.Borders.Height) / 2)) : ((CS$<>8__locals1.targetCharacter != null) ? ConvertUnits.ToDisplayUnits(CS$<>8__locals1.targetCharacter.AnimController.Collider.GetSize().X) : 100f));
								}
								float sqrDistToTarget = Vector2.DistanceSquared(base.WorldPosition, spatialTarget.WorldPosition);
								bool isProgressive = this.AIParams.MaxAggression - this.AIParams.StartAggression > 0f;
								switch (this.CirclePhase)
								{
								case CirclePhase.Start:
								{
									this.currentAttackIntensity = MathUtils.InverseLerp(this.AIParams.StartAggression, this.AIParams.MaxAggression, CS$<>8__locals1.<UpdateAttack>g__ClampIntensity|10(this.aggressionIntensity));
									this.inverseDir = false;
									this.circleDir = CS$<>8__locals1.<UpdateAttack>g__GetDirFromHeadingInRadius|7();
									this.circleRotation = 0f;
									this.strikeTimer = 0f;
									this.blockCheckTimer = 0f;
									this.breakCircling = false;
									float minFallBackDistance = this.currentTargetingParams.CircleStartDistance * 0.5f;
									float maxFallBackDistance = this.currentTargetingParams.CircleStartDistance;
									float maxRandomOffset = this.currentTargetingParams.CircleMaxRandomOffset;
									if (isProgressive)
									{
										float intensity = CS$<>8__locals1.<UpdateAttack>g__ClampIntensity|10(this.currentAttackIntensity);
										float minRotationSpeed = 0.01f * this.currentTargetingParams.CircleRotationSpeed;
										float maxRotationSpeed = 0.5f * this.currentTargetingParams.CircleRotationSpeed;
										this.circleRotationSpeed = MathHelper.Lerp(minRotationSpeed, maxRotationSpeed, intensity);
										this.circleFallbackDistance = MathHelper.Lerp(maxFallBackDistance, minFallBackDistance, intensity);
										this.circleOffset = Rand.Vector(MathHelper.Lerp(maxRandomOffset, 0f, intensity), Rand.RandSync.Unsynced);
									}
									else
									{
										this.circleRotationSpeed = this.currentTargetingParams.CircleRotationSpeed;
										this.circleFallbackDistance = maxFallBackDistance;
										this.circleOffset = Rand.Vector(maxRandomOffset, Rand.RandSync.Unsynced);
									}
									this.circleRotationSpeed *= Rand.Range(1f - this.currentTargetingParams.CircleRandomRotationFactor, 1f + this.currentTargetingParams.CircleRandomRotationFactor, Rand.RandSync.Unsynced);
									this.aggressionIntensity = Math.Clamp(this.aggressionIntensity, this.AIParams.StartAggression, this.AIParams.MaxAggression);
									CS$<>8__locals1.<UpdateAttack>g__DisableAttacksIfLimbNotRanged|0();
									if (CS$<>8__locals3.targetSub != null && CS$<>8__locals3.targetSub.Borders.Width < 1000)
									{
										Limb attackLimb4 = this.AttackLimb;
										Attack attack2 = (attackLimb4 != null) ? attackLimb4.attack : null;
										if (attack2 != null && !attack2.Ranged)
										{
											this.breakCircling = true;
											this.CirclePhase = CirclePhase.CloseIn;
											break;
										}
									}
									if (sqrDistToTarget > MathUtils.Pow2(targetSize + this.currentTargetingParams.CircleStartDistance))
									{
										this.CirclePhase = CirclePhase.CloseIn;
									}
									else if (sqrDistToTarget < MathUtils.Pow2(targetSize + this.circleFallbackDistance))
									{
										this.CirclePhase = CirclePhase.FallBack;
									}
									else
									{
										this.CirclePhase = CirclePhase.Advance;
									}
									break;
								}
								case CirclePhase.CloseIn:
								{
									Vector2 targetVelocity2 = CS$<>8__locals1.<UpdateAttack>g__GetTargetVelocity|8(ref CS$<>8__locals3);
									float targetDistance = this.currentTargetingParams.IgnoreTargetSize ? (this.currentTargetingParams.CircleStartDistance * 0.9f) : (targetSize + this.currentTargetingParams.CircleStartDistance / 2f);
									if (this.AttackLimb != null && distance > 0f && distance < this.AttackLimb.attack.Range * CS$<>8__locals1.<UpdateAttack>g__GetStrikeDistanceMultiplier|6(targetVelocity2))
									{
										this.strikeTimer = this.AttackLimb.attack.CoolDown;
										this.CirclePhase = CirclePhase.Strike;
									}
									else if (!this.breakCircling && sqrDistToTarget <= MathUtils.Pow2(targetDistance) && targetVelocity2.LengthSquared() <= MathUtils.Pow2(CS$<>8__locals1.<UpdateAttack>g__GetTargetMaxSpeed|9(ref CS$<>8__locals3)))
									{
										this.CirclePhase = CirclePhase.Advance;
									}
									CS$<>8__locals1.<UpdateAttack>g__DisableAttacksIfLimbNotRanged|0();
									break;
								}
								case CirclePhase.FallBack:
									updateSteering = false;
									if (!this.UpdateFallBack(this.attackWorldPos, deltaTime, false, true, true) || sqrDistToTarget > MathUtils.Pow2(targetSize + this.circleFallbackDistance))
									{
										this.CirclePhase = CirclePhase.Advance;
									}
									else
									{
										CS$<>8__locals1.<UpdateAttack>g__DisableAttacksIfLimbNotRanged|0();
									}
									break;
								case CirclePhase.Advance:
								{
									Vector2 targetVel = CS$<>8__locals1.<UpdateAttack>g__GetTargetVelocity|8(ref CS$<>8__locals3);
									Attack attack2;
									if (this.breakCircling || targetVel.LengthSquared() > MathUtils.Pow2(CS$<>8__locals1.<UpdateAttack>g__GetTargetMaxSpeed|9(ref CS$<>8__locals3)))
									{
										this.CirclePhase = CirclePhase.CloseIn;
									}
									else if (sqrDistToTarget > MathUtils.Pow2(targetSize + this.currentTargetingParams.CircleStartDistance * 1.2f))
									{
										if (this.currentTargetingParams.DynamicCircleRotationSpeed && this.circleRotationSpeed < 100f)
										{
											this.circleRotationSpeed *= 1f + deltaTime;
										}
										else
										{
											this.CirclePhase = CirclePhase.CloseIn;
										}
									}
									else
									{
										float rotationStep = this.circleRotationSpeed * deltaTime * this.circleDir;
										if (isProgressive)
										{
											this.circleRotation += rotationStep;
										}
										else
										{
											this.circleRotation = rotationStep;
										}
										Vector2 targetPos = this.attackSimPos + this.circleOffset;
										float targetDist = targetSize;
										if (targetDist <= 0f)
										{
											targetDist = this.circleFallbackDistance;
										}
										if (CS$<>8__locals3.targetSub != null)
										{
											Limb attackLimb5 = this.AttackLimb;
											attack2 = ((attackLimb5 != null) ? attackLimb5.attack : null);
											if (attack2 != null && attack2.Ranged)
											{
												targetDist += this.circleFallbackDistance / 2f;
											}
										}
										if (Vector2.DistanceSquared(base.SimPosition, targetPos) < ConvertUnits.ToSimUnits(targetDist))
										{
											if (CS$<>8__locals1.canAttack)
											{
												Limb attackLimb6 = this.AttackLimb;
												attack2 = ((attackLimb6 != null) ? attackLimb6.attack : null);
												if (attack2 != null && !attack2.Ranged && sqrDistToTarget < MathUtils.Pow2(targetSize + this.circleFallbackDistance))
												{
													this.CirclePhase = CirclePhase.Strike;
													this.strikeTimer = this.AttackLimb.attack.CoolDown;
													break;
												}
											}
											this.CirclePhase = CirclePhase.Start;
											break;
										}
										steerPos = MathUtils.RotatePointAroundTarget(base.SimPosition, targetPos, this.circleRotation, true);
										if (this.IsBlocked(deltaTime, steerPos, Category.Cat8))
										{
											if (!this.inverseDir)
											{
												this.circleDir = -this.circleDir;
												this.inverseDir = true;
											}
											else if (this.circleRotationSpeed < 1f)
											{
												this.circleRotationSpeed *= 1f + deltaTime;
											}
											else if (this.circleOffset.LengthSquared() > 0.1f)
											{
												this.circleOffset = Vector2.Zero;
											}
											else
											{
												Limb attackLimb7 = this.AttackLimb;
												attack2 = ((attackLimb7 != null) ? attackLimb7.attack : null);
												this.breakCircling = (attack2 != null && !attack2.Ranged);
												if (!this.breakCircling)
												{
													this.CirclePhase = CirclePhase.FallBack;
												}
											}
										}
									}
									Limb attackLimb8 = this.AttackLimb;
									attack2 = ((attackLimb8 != null) ? attackLimb8.attack : null);
									if (attack2 != null && !attack2.Ranged)
									{
										CS$<>8__locals1.canAttack = false;
										float requiredDistMultiplier = CS$<>8__locals1.<UpdateAttack>g__GetStrikeDistanceMultiplier|6(targetVel);
										if (distance > 0f && distance < this.AttackLimb.attack.Range * requiredDistMultiplier && CS$<>8__locals1.<UpdateAttack>g__IsFacing|5(MathHelper.Lerp(0.5f, 0.9f, this.currentAttackIntensity)))
										{
											this.strikeTimer = this.AttackLimb.attack.CoolDown;
											this.CirclePhase = CirclePhase.Strike;
										}
									}
									break;
								}
								case CirclePhase.Strike:
									this.strikeTimer -= deltaTime;
									steerPos = base.SimPosition + base.Steering;
									if (this.strikeTimer <= 0f)
									{
										this.CirclePhase = CirclePhase.Start;
										this.aggressionIntensity += this.AIParams.AggressionCumulation;
									}
									break;
								}
							}
						}
					}
					else if (this.currentTargetingParams.SweepDistance > 0f)
					{
						if (distance <= 0f)
						{
							distance = (this.attackWorldPos - base.WorldPosition).Length();
						}
						float amplitude = MathHelper.Lerp(0f, this.currentTargetingParams.SweepStrength, MathUtils.InverseLerp(this.currentTargetingParams.SweepDistance, 0f, distance));
						if (amplitude > 0f)
						{
							this.sweepTimer += deltaTime * this.currentTargetingParams.SweepSpeed;
							float sin = (float)Math.Sin((double)this.sweepTimer) * amplitude;
							steerPos = MathUtils.RotatePointAroundTarget(this.attackSimPos, base.SimPosition, sin, true);
						}
						else
						{
							this.sweepTimer = Rand.Range(-1000f, 1000f, Rand.RandSync.Unsynced) * this.currentTargetingParams.SweepSpeed;
						}
					}
				}
				if (updateSteering)
				{
					if (this.currentTargetingParams.AttackPattern == AttackPattern.Straight)
					{
						Limb attackLimb = this.AttackLimb;
						if (attackLimb != null && attackLimb.attack.Ranged)
						{
							bool advance = (!CS$<>8__locals1.canAttack && this.Character.CurrentHull == null) || distance > attackLimb.attack.Range * 0.9f;
							bool fallBack = CS$<>8__locals1.canAttack && distance < Math.Min(250f, attackLimb.attack.Range * 0.25f);
							if (fallBack)
							{
								this.Reverse = true;
								this.UpdateFallBack(this.attackWorldPos, deltaTime, false, false, true);
								goto IL_1E1F;
							}
							if (advance)
							{
								base.SteeringManager.SteeringSeek(steerPos, 10f);
								goto IL_1E1F;
							}
							if (this.Character.CurrentHull == null && !CS$<>8__locals1.canAttack)
							{
								base.SteeringManager.SteeringWander(1f, true);
								base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 5f);
								goto IL_1E1F;
							}
							base.SteeringManager.Reset();
							base.FaceTarget(base.SelectedAiTarget.Entity);
							goto IL_1E1F;
						}
					}
					if (!CS$<>8__locals1.canAttack || distance > Math.Min(this.AttackLimb.attack.Range * 0.9f, 100f))
					{
						if (pathSteering != null)
						{
							pathSteering.SteeringSeek(steerPos, 10f, this.minGapSize, null, null, null, true, 0f);
						}
						else
						{
							base.SteeringManager.SteeringSeek(steerPos, 10f);
						}
					}
					IL_1E1F:
					if (this.Character.CurrentHull == null)
					{
						AITarget selectedAiTarget3 = base.SelectedAiTarget;
						Character c = ((selectedAiTarget3 != null) ? selectedAiTarget3.Entity : null) as Character;
						if ((c != null && c.Submarine == null) || distance == 0f || distance > ConvertUnits.ToDisplayUnits(this.avoidLookAheadDistance * 2f) || (this.AttackLimb != null && this.AttackLimb.attack.Ranged))
						{
							base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 30f);
						}
					}
				}
			}
			EnemyAIController.WallTarget wallTarget = this.wallTarget;
			Structure structure2;
			if ((structure2 = ((wallTarget != null) ? wallTarget.Structure : null)) == null)
			{
				AITarget selectedAiTarget4 = base.SelectedAiTarget;
				structure2 = ((selectedAiTarget4 != null) ? selectedAiTarget4.Entity : null);
			}
			Entity targetEntity = structure2;
			Limb attackLimb9 = this.AttackLimb;
			Attack attack = (attackLimb9 != null) ? attackLimb9.attack : null;
			if (attack != null && attack.Ranged)
			{
				Attack attack3 = attack;
				ISpatialEntity spatialEntity = attackTargetLimb;
				this.AimRangedAttack(attack3, spatialEntity ?? targetEntity);
			}
			if (CS$<>8__locals1.canAttack)
			{
				if (!this.UpdateLimbAttack(deltaTime, this.attackSimPos, damageTarget, distance, attackTargetLimb))
				{
					this.IgnoreTarget(base.SelectedAiTarget);
					return;
				}
			}
			else if (this.IsAttackRunning)
			{
				this.AttackLimb.attack.ResetAttackTimer();
			}
		}

		// Token: 0x06000CCC RID: 3276 RVA: 0x0007A330 File Offset: 0x00078530
		public void AimRangedAttack(Attack attack, ISpatialEntity targetEntity)
		{
			if (attack == null || !attack.Ranged)
			{
				return;
			}
			Entity entity = targetEntity as Entity;
			if (entity != null && entity.Removed)
			{
				return;
			}
			this.Character.SetInput(InputType.Aim, false, true);
			if (attack.AimRotationTorque <= 0f)
			{
				return;
			}
			Limb limb = this.GetLimbToRotate(attack);
			if (limb != null)
			{
				Vector2 toTarget = targetEntity.WorldPosition - limb.WorldPosition;
				float offset = limb.Params.GetSpriteOrientation() - 1.5707964f;
				limb.body.SuppressSmoothRotationCalls = false;
				float angle = MathUtils.VectorToAngle(toTarget);
				limb.body.SmoothRotate(angle + offset, attack.AimRotationTorque, true);
				limb.body.SuppressSmoothRotationCalls = true;
			}
		}

		// Token: 0x06000CCD RID: 3277 RVA: 0x0007A3E0 File Offset: 0x000785E0
		private bool IsValidAttack(Limb attackingLimb, IEnumerable<AttackContext> currentContexts, Entity target)
		{
			if (attackingLimb == null)
			{
				return false;
			}
			if (target == null)
			{
				return false;
			}
			Attack attack = attackingLimb.attack;
			if (attack == null)
			{
				return false;
			}
			if (attack.CoolDownTimer > 0f)
			{
				return false;
			}
			if (!attack.IsValidContext(currentContexts))
			{
				return false;
			}
			if (!attack.IsValidTarget(target))
			{
				return false;
			}
			if (!attackingLimb.attack.Ranged)
			{
				if (!(target is Item))
				{
					if (!(target is Structure))
					{
						goto IL_91;
					}
					if (attackingLimb.attack.StructureDamage > 0f)
					{
						goto IL_91;
					}
				}
				else if (attackingLimb.attack.ItemDamage > 0f)
				{
					goto IL_91;
				}
				return false;
			}
			IL_91:
			ISerializableEntity se = target as ISerializableEntity;
			if (se != null && se is Character && attack.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(se)))
			{
				return false;
			}
			if (attack.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(this.Character)))
			{
				return false;
			}
			if (attack.Ranged)
			{
				Vector2 attackLimbPos = this.Character.AnimController.SimplePhysicsEnabled ? this.Character.WorldPosition : attackingLimb.WorldPosition;
				Vector2 toTarget = this.attackWorldPos - attackLimbPos;
				if (attack.MinRange > 0f && toTarget.LengthSquared() < MathUtils.Pow2(attack.MinRange))
				{
					return false;
				}
				float offset = attackingLimb.Params.GetSpriteOrientation() - 1.5707964f;
				Vector2 forward = VectorExtensions.Forward(attackingLimb.body.TransformedRotation - offset * this.Character.AnimController.Dir, 1f);
				float angle = MathHelper.ToDegrees(forward.Angle(toTarget));
				if (angle > attack.RequiredAngle)
				{
					return false;
				}
			}
			if (attack.RootForceWorldEnd.LengthSquared() > 1f)
			{
				Character targetCharacter = target as Character;
				if (targetCharacter == null)
				{
					Item targetItem = target as Item;
					if (targetItem == null)
					{
						goto IL_211;
					}
					if (this.Character.CurrentHull == targetItem.CurrentHull)
					{
						goto IL_211;
					}
				}
				else if (this.Character.CurrentHull == targetCharacter.CurrentHull && !targetCharacter.IsKnockedDownOrRagdolled)
				{
					goto IL_211;
				}
				return false;
				IL_211:
				float verticalDistance = Math.Abs(this.attackWorldPos.Y - this.Character.WorldPosition.Y);
				if (verticalDistance > 50f)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000CCE RID: 3278 RVA: 0x0007A630 File Offset: 0x00078830
		private Limb GetAttackLimb(Vector2 attackWorldPos, Limb ignoredLimb = null)
		{
			IEnumerable<AttackContext> currentContexts = this.Character.GetAttackContexts();
			Entity entity;
			if (this.wallTarget == null)
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				entity = ((selectedAiTarget != null) ? selectedAiTarget.Entity : null);
			}
			else
			{
				entity = this.wallTarget.Structure;
			}
			Entity target = entity;
			if (target == null)
			{
				return null;
			}
			Limb selectedLimb = null;
			float currentPriority = -1f;
			foreach (Limb limb in this.Character.AnimController.Limbs)
			{
				if (limb != ignoredLimb && !limb.IsSevered && !limb.IsStuck && this.IsValidAttack(limb, currentContexts, target))
				{
					if (this.AIParams.RandomAttack)
					{
						this.attackLimbs.Add(limb);
						this.weights.Add(limb.attack.Priority);
					}
					else
					{
						float priority = this.<GetAttackLimb>g__CalculatePriority|168_0(limb, attackWorldPos);
						if (priority > currentPriority)
						{
							currentPriority = priority;
							selectedLimb = limb;
						}
					}
				}
			}
			if (this.AIParams.RandomAttack)
			{
				selectedLimb = ToolBox.SelectWeightedRandom<Limb>(this.attackLimbs, this.weights, Rand.RandSync.Unsynced);
				this.attackLimbs.Clear();
				this.weights.Clear();
			}
			return selectedLimb;
		}

		// Token: 0x06000CCF RID: 3279 RVA: 0x0007A750 File Offset: 0x00078950
		public override void OnAttacked(Character attacker, AttackResult attackResult)
		{
			float reactionTime = Rand.Range(0.1f, 0.3f, Rand.RandSync.Unsynced);
			this.updateTargetsTimer = Math.Min(this.updateTargetsTimer, reactionTime);
			bool wasLatched = this.IsLatchedOnSub;
			this.Character.AnimController.ReleaseStuckLimbs();
			if (attackResult.Damage > 0f)
			{
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.DeattachFromBody(true, 1f);
				}
			}
			if (((attacker != null) ? attacker.AiTarget : null) == null || attacker.Removed || attacker.IsDead)
			{
				return;
			}
			AITargetMemory targetMemory = this.GetTargetMemory(attacker.AiTarget, true, true);
			targetMemory.Priority += EnemyAIController.GetRelativeDamage(attackResult.Damage, this.Character.Vitality) * this.AIParams.AggressionHurt;
			if (attackResult.Damage >= this.AIParams.DamageThreshold)
			{
				this.ReleaseDragTargets();
			}
			bool isFriendly = this.Character.IsFriendly(attacker);
			if (wasLatched)
			{
				this.State = AIState.Escape;
				this.avoidTimer = this.AIParams.AvoidTime * 0.5f * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				if (!isFriendly)
				{
					this.SelectTarget(attacker.AiTarget);
				}
				return;
			}
			if (this.State == AIState.Flee)
			{
				if (!isFriendly)
				{
					this.SelectTarget(attacker.AiTarget);
				}
				return;
			}
			if (!isFriendly && attackResult.Damage > 0f)
			{
				bool canAttack = (attacker.Submarine == this.Character.Submarine && this.canAttackCharacters) || (attacker.Submarine != null && this.canAttackWalls);
				if (canAttack)
				{
					AIState state = this.State;
					if (state != AIState.PlayDead)
					{
						if (state != AIState.Hiding)
						{
							goto IL_1C2;
						}
					}
					else if (Rand.Value(Rand.RandSync.Unsynced) < 0.5f)
					{
						return;
					}
					this.SelectTarget(attacker.AiTarget);
					this.State = AIState.Attack;
				}
				IL_1C2:
				IEnumerable<CharacterParams.TargetParams> targetingParams;
				if (this.AIParams.AttackWhenProvoked && canAttack)
				{
					if (this.ignoredTargets.Contains(attacker.AiTarget))
					{
						this.ignoredTargets.Remove(attacker.AiTarget);
					}
					if (attacker.IsHusk)
					{
						this.ChangeTargetState(Tags.Husk, AIState.Attack, new float?((float)100));
					}
					else
					{
						this.ChangeTargetState(attacker, AIState.Attack, new float?((float)100));
					}
				}
				else if (!this.AIParams.HasTag(attacker.SpeciesName))
				{
					if (attacker.IsHusk)
					{
						this.ChangeTargetState(Tags.Husk, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
					}
					else
					{
						EnemyAIController enemyAI = attacker.AIController as EnemyAIController;
						if (enemyAI != null)
						{
							if (enemyAI.CombatStrength > this.CombatStrength)
							{
								if (!this.AIParams.HasTag("stronger"))
								{
									this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
								}
							}
							else if (enemyAI.CombatStrength < this.CombatStrength)
							{
								if (!this.AIParams.HasTag("weaker"))
								{
									this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
								}
							}
							else if (!this.AIParams.HasTag("equal"))
							{
								this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
							}
						}
						else
						{
							this.ChangeTargetState(attacker, canAttack ? AIState.Attack : AIState.Escape, new float?((float)100));
						}
					}
				}
				else if (canAttack && attacker.IsHuman && this.AIParams.TryGetTargets(attacker, out targetingParams))
				{
					this.tempParamsList.Clear();
					this.tempParamsList.AddRange(targetingParams);
					foreach (CharacterParams.TargetParams tp in this.tempParamsList)
					{
						AIState state2 = tp.State;
						bool flag = state2 - AIState.Aggressive <= 1;
						if (flag)
						{
							this.ChangeTargetState(attacker, AIState.Attack, new float?((float)100));
						}
					}
				}
			}
			bool retaliate = !isFriendly && base.SelectedAiTarget != attacker.AiTarget && attacker.Submarine == this.Character.Submarine;
			bool avoidGunFire = this.AIParams.AvoidGunfire && attacker.Submarine != this.Character.Submarine;
			if (this.State == AIState.Attack && (this.IsAttackRunning || this.IsCoolDownRunning))
			{
				retaliate = false;
				if (this.IsAttackRunning)
				{
					avoidGunFire = false;
				}
			}
			if (retaliate)
			{
				foreach (Limb limb in this.Character.AnimController.Limbs)
				{
					if (limb.attack != null)
					{
						limb.attack.CoolDownTimer *= reactionTime;
					}
				}
			}
			else if (avoidGunFire && attackResult.Damage >= this.AIParams.DamageThreshold)
			{
				this.State = AIState.Escape;
				this.avoidTimer = this.AIParams.AvoidTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				this.SelectTarget(attacker.AiTarget);
			}
			if (Math.Max(this.Character.HealthPercentage, 0f) < this.FleeHealthThreshold)
			{
				this.State = AIState.Flee;
				this.avoidTimer = this.AIParams.MinFleeTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
				this.SelectTarget(attacker.AiTarget);
			}
		}

		// Token: 0x06000CD0 RID: 3280 RVA: 0x0007ACB8 File Offset: 0x00078EB8
		private Item GetEquippedItem(Limb limb)
		{
			EnemyAIController.<>c__DisplayClass170_0 CS$<>8__locals1;
			CS$<>8__locals1.limb = limb;
			InvSlotType slot = EnemyAIController.<GetEquippedItem>g__GetInvSlotForLimb|170_0(ref CS$<>8__locals1);
			if (slot != InvSlotType.None)
			{
				return this.Character.Inventory.GetItemInLimbSlot(slot);
			}
			return null;
		}

		// Token: 0x06000CD1 RID: 3281 RVA: 0x0007ACEB File Offset: 0x00078EEB
		private static float GetRelativeDamage(float dmg, float vitality)
		{
			return dmg / Math.Max(vitality, 1f);
		}

		// Token: 0x06000CD2 RID: 3282 RVA: 0x0007ACFC File Offset: 0x00078EFC
		private bool UpdateLimbAttack(float deltaTime, Vector2 attackSimPos, IDamageable damageTarget, float distance = -1f, Limb targetLimb = null)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == null)
			{
				return false;
			}
			Limb attackLimb = this.AttackLimb;
			if (((attackLimb != null) ? attackLimb.attack : null) == null)
			{
				return false;
			}
			ISpatialEntity spatialEntity2;
			if (this.wallTarget == null)
			{
				ISpatialEntity spatialEntity = base.SelectedAiTarget.Entity;
				spatialEntity2 = spatialEntity;
			}
			else
			{
				ISpatialEntity spatialEntity = this.wallTarget.Structure;
				spatialEntity2 = spatialEntity;
			}
			ISpatialEntity spatialTarget = spatialEntity2;
			if (spatialTarget == null)
			{
				return false;
			}
			this.ActiveAttack = this.AttackLimb.attack;
			if (this.wallTarget != null)
			{
				AITarget aiTarget = this.wallTarget.Structure.AiTarget;
				if (aiTarget != null && base.SelectedAiTarget != aiTarget)
				{
					this.SelectTarget(aiTarget, this.GetTargetMemory(base.SelectedAiTarget, true, false).Priority);
					this.State = AIState.Attack;
					this.AttackLimb = null;
					return true;
				}
			}
			if (damageTarget == null)
			{
				return false;
			}
			this.ActiveAttack = this.AttackLimb.attack;
			if (this.ActiveAttack.Ranged && this.ActiveAttack.RequiredAngleToShoot > 0f)
			{
				Limb referenceLimb = this.GetLimbToRotate(this.ActiveAttack);
				if (referenceLimb != null)
				{
					Vector2 toTarget = this.attackWorldPos - referenceLimb.WorldPosition;
					float offset = referenceLimb.Params.GetSpriteOrientation() - 1.5707964f;
					Vector2 forward = VectorExtensions.Forward(referenceLimb.body.TransformedRotation - offset * referenceLimb.Dir, 1f);
					float angle = MathHelper.ToDegrees(forward.Angle(toTarget));
					if (angle > this.ActiveAttack.RequiredAngleToShoot)
					{
						return true;
					}
				}
			}
			if (this.Character.Params.CanInteract && this.Character.Inventory != null)
			{
				Item item = this.GetEquippedItem(this.AttackLimb);
				if (item != null)
				{
					if (item.RequireAimToUse && !this.Aim(deltaTime, spatialTarget, item))
					{
						return true;
					}
					this.Character.SetInput(item.IsShootable ? InputType.Shoot : InputType.Use, false, true);
					item.Use(deltaTime, this.Character, null, null, null);
				}
			}
			this.Character.SetInput(InputType.Attack, true, true);
			if (!this.ActiveAttack.IsRunning && (Timing.TotalTime > this.lastSetAttackTargetEventTime + 0.5 || damageTarget != this.lastDamageTarget || this.AttackLimb != this.lastAttackLimb || targetLimb != this.lastTargetLimb))
			{
				GameMain.NetworkMember.CreateEntityEvent(this.Character, new Character.SetAttackTargetEventData(this.AttackLimb, damageTarget, targetLimb, base.SimPosition));
				this.lastSetAttackTargetEventTime = Timing.TotalTime;
				this.lastDamageTarget = damageTarget;
				this.lastAttackLimb = this.AttackLimb;
				this.lastTargetLimb = targetLimb;
			}
			AttackResult attackResult;
			if (this.AttackLimb.UpdateAttack(deltaTime, attackSimPos, damageTarget, out attackResult, distance, targetLimb))
			{
				if (this.ActiveAttack.CoolDownTimer > 0f)
				{
					this.SetAimTimer(Math.Min(this.ActiveAttack.CoolDown, 1.5f));
				}
				if (this.LatchOntoAI != null)
				{
					Character targetCharacter = base.SelectedAiTarget.Entity as Character;
					if (targetCharacter != null)
					{
						this.LatchOntoAI.SetAttachTarget(targetCharacter);
					}
				}
				if (!this.ActiveAttack.Ranged)
				{
					if (damageTarget.Health <= 0f || attackResult.Damage <= 0f)
					{
						this.currentTargetMemory.Priority -= Math.Max(this.currentTargetMemory.Priority / 2f, 1f);
						return this.currentTargetMemory.Priority > 1f;
					}
					float greed = this.AIParams.AggressionGreed;
					if (!(damageTarget is Character))
					{
						greed /= 2f;
					}
					this.currentTargetMemory.Priority += EnemyAIController.GetRelativeDamage(attackResult.Damage, damageTarget.Health) * greed;
				}
			}
			return true;
		}

		// Token: 0x06000CD3 RID: 3283 RVA: 0x0007B0B3 File Offset: 0x000792B3
		private bool CanSeeTarget(ISpatialEntity target)
		{
			if (Timing.TotalTime > this.lastVisibilityCheckTime + 0.20000000298023224)
			{
				this.canSeeTarget = this.Character.CanSeeTarget(target, null, false, false);
				this.lastVisibilityCheckTime = Timing.TotalTime;
			}
			return this.canSeeTarget;
		}

		// Token: 0x06000CD4 RID: 3284 RVA: 0x0007B0F4 File Offset: 0x000792F4
		private bool Aim(float deltaTime, ISpatialEntity target, Item weapon)
		{
			if (target == null || weapon == null)
			{
				return false;
			}
			if (this.AttackLimb == null)
			{
				return false;
			}
			Vector2 toTarget = target.WorldPosition - weapon.WorldPosition;
			float dist = toTarget.Length();
			this.Character.CursorPosition = target.WorldPosition;
			if (this.AttackLimb.attack.SwayAmount > 0f)
			{
				this.sinTime += deltaTime * this.AttackLimb.attack.SwayFrequency;
				this.Character.CursorPosition += VectorExtensions.Forward(weapon.body.TransformedRotation + (float)Math.Sin((double)this.sinTime) / 2f, dist / 2f * this.AttackLimb.attack.SwayAmount);
			}
			if (this.Character.Submarine != null)
			{
				this.Character.CursorPosition -= this.Character.Submarine.Position;
			}
			if (!this.CanSeeTarget(target))
			{
				this.SetAimTimer(1.5f);
				return false;
			}
			this.Character.SetInput(InputType.Aim, false, true);
			if (this.aimTimer > 0f)
			{
				this.aimTimer -= deltaTime;
				return false;
			}
			float angle = VectorExtensions.Forward(weapon.body.TransformedRotation, 1f).Angle(toTarget);
			float minDistance = 300f;
			float distanceFactor = MathHelper.Lerp(1f, 0.1f, MathUtils.InverseLerp(minDistance, 1000f, dist));
			float margin = 0.7853982f * distanceFactor;
			if (angle < margin || dist < minDistance)
			{
				Category collisionCategories = Category.Cat1 | Category.Cat2 | Category.Cat6 | Category.Cat8;
				Body pickedBody = Submarine.PickBody(weapon.SimPosition, this.Character.GetRelativeSimPosition(target, null), this.myBodies, new Category?(collisionCategories), true, null, true);
				if (pickedBody != null)
				{
					if (target is MapEntity)
					{
						Submarine sub = pickedBody.UserData as Submarine;
						if (sub != null && sub == target.Submarine)
						{
							return true;
						}
						if (target == pickedBody.UserData)
						{
							return true;
						}
					}
					Character t = null;
					Character c = pickedBody.UserData as Character;
					if (c != null)
					{
						t = c;
					}
					else
					{
						Limb limb = pickedBody.UserData as Limb;
						if (limb != null)
						{
							t = limb.character;
						}
					}
					if (t != null && (t == target || !this.Character.IsFriendly(t) || this.IsAttackingOwner(t)))
					{
						return true;
					}
					Item item = pickedBody.UserData as Item;
					if (item != null && item.Prefab.DamagedByProjectiles)
					{
						return true;
					}
					Holdable holdable = pickedBody.UserData as Holdable;
					if (holdable != null && holdable.Item.Prefab.DamagedByProjectiles)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000CD5 RID: 3285 RVA: 0x0007B3A6 File Offset: 0x000795A6
		private void SetAimTimer(float timer = 1.5f)
		{
			this.aimTimer = timer * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000CD6 RID: 3286 RVA: 0x0007B3C0 File Offset: 0x000795C0
		private bool IsBlocked(float deltaTime, Vector2 steerPos, Category collisionCategory = Category.Cat8)
		{
			this.blockCheckTimer -= deltaTime;
			if (this.blockCheckTimer <= 0f)
			{
				this.blockCheckTimer = this.blockCheckInterval;
				this.isBlocked = Submarine.PickBodies(base.SimPosition, steerPos, null, new Category?(collisionCategory), true, null, false).Any<Body>();
			}
			return this.isBlocked;
		}

		// Token: 0x06000CD7 RID: 3287 RVA: 0x0007B41C File Offset: 0x0007961C
		private bool UpdateFallBack(Vector2 attackWorldPos, float deltaTime, bool followThrough, bool checkBlocking = false, bool avoidObstacles = true)
		{
			if (this.attackVector == null)
			{
				this.attackVector = new Vector2?(attackWorldPos - base.WorldPosition);
			}
			Vector2 dir = Vector2.Normalize(followThrough ? this.attackVector.Value : (-this.attackVector.Value));
			if (!MathUtils.IsValid(dir))
			{
				dir = Vector2.UnitY;
			}
			this.steeringManager.SteeringManual(deltaTime, dir);
			if (this.Character.AnimController.InWater && !this.Reverse && avoidObstacles)
			{
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
			}
			return !checkBlocking || !this.IsBlocked(deltaTime, base.SimPosition + dir * (this.avoidLookAheadDistance / 2f), Category.Cat8);
		}

		// Token: 0x06000CD8 RID: 3288 RVA: 0x0007B4FC File Offset: 0x000796FC
		private Limb GetLimbToRotate(Attack attack)
		{
			Limb limb = this.AttackLimb;
			if (attack.RotationLimbIndex > -1 && attack.RotationLimbIndex < this.Character.AnimController.Limbs.Length)
			{
				limb = this.Character.AnimController.Limbs[attack.RotationLimbIndex];
			}
			return limb;
		}

		// Token: 0x06000CD9 RID: 3289 RVA: 0x0007B54C File Offset: 0x0007974C
		private void UpdateEating(float deltaTime)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) == null || base.SelectedAiTarget.Entity.Removed)
			{
				this.ReleaseEatingTarget();
				return;
			}
			Entity entity = base.SelectedAiTarget.Entity;
			bool flag = entity is Character || entity is Item;
			if (flag)
			{
				Limb mouthLimb = this.Character.AnimController.GetLimb(LimbType.Head, true, false, false);
				if (mouthLimb == null)
				{
					DebugConsole.ThrowError("Character \"" + this.Character.SpeciesName.ToString() + "\" failed to eat a target (No head limb found)", null, this.Character.Prefab.ContentPackage, false, false);
					this.IgnoreTarget(base.SelectedAiTarget);
					this.ReleaseEatingTarget();
					base.ResetAITarget();
					return;
				}
				Vector2 mouthPos = this.Character.AnimController.SimplePhysicsEnabled ? base.SimPosition : (this.Character.AnimController.GetMouthPosition() ?? Vector2.Zero);
				Vector2 attackSimPosition = this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null);
				Vector2 limbDiff = attackSimPosition - mouthPos;
				float extent = Math.Max(mouthLimb.body.GetMaxExtent(), 2f);
				bool tooFar = this.Character.InWater ? (limbDiff.LengthSquared() > extent * extent) : (limbDiff.X > extent);
				if (tooFar)
				{
					this.steeringManager.SteeringSeek(attackSimPosition - (mouthPos - base.SimPosition), 2f);
					if (this.Character.InWater)
					{
						base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
						return;
					}
				}
				else
				{
					Character targetCharacter = base.SelectedAiTarget.Entity as Character;
					if (targetCharacter != null)
					{
						this.Character.SelectCharacter(targetCharacter);
					}
					else
					{
						Item item = base.SelectedAiTarget.Entity as Item;
						if (item != null && !item.Removed && item.body != null)
						{
							float itemBodyExtent = item.body.GetMaxExtent() * 2f;
							if (Math.Abs(limbDiff.X) < itemBodyExtent && Math.Abs(limbDiff.Y) < this.Character.AnimController.Collider.GetMaxExtent() + this.Character.AnimController.ColliderHeightFromFloor)
							{
								Vector2 velocity = limbDiff;
								if (limbDiff.LengthSquared() > 0.01f)
								{
									velocity = Vector2.Normalize(velocity);
								}
								item.body.LinearVelocity *= 0.9f;
								item.body.LinearVelocity -= velocity * 0.25f;
								bool wasBroken = item.Condition <= 0f;
								item.LastEatenTime = (float)Timing.TotalTimeUnpaused;
								item.AddDamage(this.Character, item.WorldPosition, new Attack(0f, 0f, 0f, 0f, 0.02f * this.Character.Params.EatingSpeed, 0f), Vector2.Zero, deltaTime, true);
								this.Character.ApplyStatusEffects(ActionType.OnEating, deltaTime);
								if (item.Condition <= 0f)
								{
									if (!wasBroken)
									{
										PetBehavior petBehavior = this.PetBehavior;
										if (petBehavior != null)
										{
											petBehavior.OnEat(item);
										}
									}
									Entity.Spawner.AddItemToRemoveQueue(item);
								}
							}
						}
					}
					this.steeringManager.SteeringManual(deltaTime, Vector2.Normalize(limbDiff) * 3f);
					if (this.Character.AnimController.OnGround || this.Character.InWater)
					{
						this.Character.AnimController.Collider.ApplyForce(limbDiff * mouthLimb.Mass * 50f, 10f);
						return;
					}
				}
			}
			else
			{
				this.IgnoreTarget(base.SelectedAiTarget);
				this.ReleaseEatingTarget();
				base.ResetAITarget();
			}
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x0007B968 File Offset: 0x00079B68
		private void ReleaseEatingTarget()
		{
			this.State = AIState.Idle;
			this.Character.DeselectCharacter();
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x0007B97C File Offset: 0x00079B7C
		private void UpdateFollow(float deltaTime)
		{
			if (base.SelectedAiTarget == null || base.SelectedAiTarget.Entity == null || base.SelectedAiTarget.Entity.Removed)
			{
				this.State = AIState.Idle;
				return;
			}
			if (this.Character.CurrentHull != null && this.steeringManager == this.insideSteering)
			{
				if ((this.Character.AnimController.InWater || !this.Character.AnimController.CanWalk) && this.Character.Submarine != null && !this.Character.Submarine.Info.IsRuin)
				{
					Character c = base.SelectedAiTarget.Entity as Character;
					if (c != null && base.VisibleHulls.Contains(c.CurrentHull))
					{
						base.SteeringManager.SteeringManual(deltaTime, Vector2.Normalize(base.SelectedAiTarget.Entity.WorldPosition - this.Character.WorldPosition));
						goto IL_161;
					}
				}
				this.PathSteering.SteeringSeek(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null), 2f, this.minGapSize, null, null, null, true, 0f);
			}
			else
			{
				base.SteeringManager.SteeringSeek(this.Character.GetRelativeSimPosition(base.SelectedAiTarget.Entity, null), 5f);
			}
			IL_161:
			IndoorsSteeringManager pathSteering = this.steeringManager as IndoorsSteeringManager;
			if (pathSteering != null)
			{
				if (!pathSteering.IsPathDirty && pathSteering.CurrentPath != null && pathSteering.CurrentPath.Unreachable)
				{
					this.State = AIState.Idle;
					this.IgnoreTarget(base.SelectedAiTarget);
					return;
				}
			}
			else if (this.Character.AnimController.InWater)
			{
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
			}
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x0007BB54 File Offset: 0x00079D54
		public static bool IsLatchedTo(Character target, Character character)
		{
			EnemyAIController enemyAI = target.AIController as EnemyAIController;
			return enemyAI != null && enemyAI.LatchOntoAI != null && enemyAI.LatchOntoAI.IsAttached && enemyAI.LatchOntoAI.TargetCharacter == character;
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x0007BB98 File Offset: 0x00079D98
		public static bool IsLatchedToSomeoneElse(Character target, Character character)
		{
			EnemyAIController enemyAI = target.AIController as EnemyAIController;
			return enemyAI != null && enemyAI.LatchOntoAI != null && (enemyAI.LatchOntoAI.IsAttached && enemyAI.LatchOntoAI.TargetCharacter != null) && enemyAI.LatchOntoAI.TargetCharacter != character;
		}

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x06000CDE RID: 3294 RVA: 0x0007BBEB File Offset: 0x00079DEB
		private bool IsLatchedOnSub
		{
			get
			{
				return this.LatchOntoAI != null && this.LatchOntoAI.IsAttachedToSub;
			}
		}

		// Token: 0x06000CDF RID: 3295 RVA: 0x0007BC04 File Offset: 0x00079E04
		public void UpdateTargets()
		{
			this.targetValue = 0f;
			AITarget newTarget = null;
			CharacterParams.TargetParams selectedTargetParams = null;
			AITargetMemory targetMemory = null;
			bool isAnyTargetClose = false;
			bool isBeingChased = this.IsBeingChased;
			bool isCharacterInside = this.Character.CurrentHull != null;
			bool tryToGetInside = this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.True || (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.Partial && this.IsAggressiveBoarder);
			using (List<AITarget>.Enumerator enumerator = AITarget.List.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					AITarget aiTarget = enumerator.Current;
					if (!aiTarget.ShouldBeIgnored() && !this.ignoredTargets.Contains(aiTarget) && aiTarget.Type != AITarget.TargetType.HumanOnly)
					{
						if (!this.TargetOutposts)
						{
							GameSession gameSession = GameMain.GameSession;
							if (!(((gameSession != null) ? gameSession.GameMode : null) is TestGameMode) && aiTarget.Entity.Submarine != null && aiTarget.Entity.Submarine.Info.IsOutpost)
							{
								continue;
							}
						}
						Character targetCharacter = aiTarget.Entity as Character;
						if (targetCharacter != this.Character)
						{
							if (EnemyAIController.TargetingRestrictions.HasFlag(EnemyTargetingRestrictions.PlayerCharacters))
							{
								if (targetCharacter != null && targetCharacter.IsPlayer)
								{
									continue;
								}
								Item item = aiTarget.Entity as Item;
								if (item != null)
								{
									Character character = item.GetRootInventoryOwner() as Character;
									if (character != null && character.IsPlayer)
									{
										continue;
									}
								}
							}
							if (EnemyAIController.TargetingRestrictions.HasFlag(EnemyTargetingRestrictions.PlayerSubmarines))
							{
								Submarine submarine = aiTarget.Entity.Submarine;
								SubmarineInfo submarineInfo = (submarine != null) ? submarine.Info : null;
								if (submarineInfo != null && submarineInfo.IsPlayer)
								{
									continue;
								}
							}
							IEnumerable<Identifier> targetingTags = this.GetTargetingTags(aiTarget);
							Door door = null;
							if (targetCharacter != null)
							{
								if (targetCharacter.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI))
								{
									continue;
								}
								if (this.AIParams.Targets.None(null) && this.Character.IsFriendly(targetCharacter))
								{
									continue;
								}
							}
							else
							{
								if (aiTarget.Entity.Submarine != null)
								{
									if (!aiTarget.Entity.Submarine.Info.IsWreck && !aiTarget.Entity.Submarine.Info.IsBeacon)
									{
										Entity entity = aiTarget.Entity;
										if (entity is Structure || entity is Hull)
										{
											goto IL_2B4;
										}
										Item item3 = entity as Item;
										if (item3 != null && item3.body == null)
										{
											goto IL_2B4;
										}
										bool flag = false;
										IL_2BC:
										if ((!flag || !this.unattackableSubmarines.Contains(aiTarget.Entity.Submarine)) && (aiTarget.Entity.Submarine.Info.OutpostGenerationParams == null || aiTarget.Entity.Submarine.Info.OutpostGenerationParams.ForceToEndLocationIndex <= -1))
										{
											goto IL_324;
										}
										continue;
										IL_2B4:
										flag = true;
										goto IL_2BC;
									}
									continue;
								}
								IL_324:
								Hull hull = aiTarget.Entity as Hull;
								if (hull != null)
								{
									if (this.Character.CurrentHull != null || hull.Submarine == null)
									{
										continue;
									}
									if (hull.Submarine.Info.IsRuin)
									{
										continue;
									}
								}
								else
								{
									Item item2 = aiTarget.Entity as Item;
									if (item2 != null)
									{
										door = item2.GetComponent<Door>();
										bool targetingFromOutsideToInside = item2.CurrentHull != null && !isCharacterInside;
										if (targetingFromOutsideToInside && ((door != null && !this.canAttackDoors && !this.AIParams.CanOpenDoors) || !this.canAttackWalls))
										{
											continue;
										}
										if (door == null && targetingFromOutsideToInside)
										{
											Submarine submarine2 = item2.Submarine;
											SubmarineInfo submarineInfo = (submarine2 != null) ? submarine2.Info : null;
											if (submarineInfo != null && submarineInfo.IsRuin)
											{
												continue;
											}
										}
										else if (targetingTags.Contains(Tags.Nasonov) && (item2.Submarine == null || !item2.Submarine.Info.IsPlayer) && item2.ParentInventory == null)
										{
											continue;
										}
										if (this.Character.CurrentHull != null && targetingTags.Contains(Tags.Decoy))
										{
											continue;
										}
									}
									else
									{
										Structure s = aiTarget.Entity as Structure;
										if (s != null)
										{
											if (!s.HasBody || s.IsPlatform || s.Submarine == null || s.Submarine.Info.IsRuin)
											{
												continue;
											}
											bool isInnerWall = s.Prefab.Tags.Contains("inner");
											if ((isInnerWall && !isCharacterInside) || (!tryToGetInside && base.IsWallDisabled(s)))
											{
												continue;
											}
										}
									}
								}
								if (door != null)
								{
									if (door.Item.Submarine == null)
									{
										continue;
									}
									Gap linkedGap = door.LinkedGap;
									bool isOutdoor = linkedGap != null && linkedGap.FlowTargetHull != null && !linkedGap.IsRoomToRoom;
									if (this.Character.CurrentHull == null && !isOutdoor)
									{
										continue;
									}
									if (!door.CanBeTraversed)
									{
										if (!this.canAttackDoors)
										{
											continue;
										}
									}
									else if (this.Character.AnimController.CanEnterSubmarine != CanEnterSubmarine.True)
									{
										continue;
									}
								}
								else
								{
									IDamageable damageable = aiTarget.Entity as IDamageable;
									if (damageable != null && damageable.Health <= 0f)
									{
										continue;
									}
								}
							}
							if (!targetingTags.None(null))
							{
								CharacterParams.TargetParams matchingTargetParams = null;
								foreach (CharacterParams.TargetParams targetParams in this.GetTargetParams(targetingTags))
								{
									if ((matchingTargetParams == null || matchingTargetParams.Priority <= targetParams.Priority) && (!targetParams.IgnoreInside || this.Character.CurrentHull == null) && (!targetParams.IgnoreOutside || this.Character.CurrentHull != null) && (!targetParams.IgnoreIncapacitated || targetCharacter == null || !targetCharacter.IsIncapacitated) && (!targetParams.IgnoreTargetInside || aiTarget.Entity.Submarine == null) && (!targetParams.IgnoreTargetOutside || aiTarget.Entity.Submarine != null))
									{
										Entity entity = aiTarget.Entity;
										ISerializableEntity se = entity as ISerializableEntity;
										if ((se == null || !targetParams.Conditionals.Any((PropertyConditional c) => !c.TargetSelf && !c.Matches(se))) && !targetParams.Conditionals.Any((PropertyConditional c) => c.TargetSelf && !c.Matches(this.Character)))
										{
											if (targetParams.IgnoreIfNotInSameSub)
											{
												if (aiTarget.Entity.Submarine != this.Character.Submarine)
												{
													continue;
												}
												Hull hull2;
												if (targetCharacter == null)
												{
													Item it = aiTarget.Entity as Item;
													hull2 = ((it != null) ? it.CurrentHull : null);
												}
												else
												{
													hull2 = targetCharacter.CurrentHull;
												}
												Hull targetHull = hull2;
												if (targetHull == null != (this.Character.CurrentHull == null))
												{
													continue;
												}
											}
											AIState state = targetParams.State;
											bool flag = state == AIState.Eat || state == AIState.Observe;
											if (!flag || targetCharacter == null || targetCharacter.Submarine == this.Character.Submarine)
											{
												Item targetItem = aiTarget.Entity as Item;
												if (targetItem != null)
												{
													if (targetParams.IgnoreContained && targetItem.ParentInventory != null)
													{
														continue;
													}
													AIState state2 = targetParams.State;
													if (state2 != AIState.Attack && state2 != AIState.Aggressive)
													{
														if (state2 == AIState.FleeTo)
														{
															float healthThreshold = targetParams.Threshold;
															if (targetParams.ThresholdMin > 0f && targetParams.ThresholdMax > 0f)
															{
																healthThreshold = ((this.currentTargetingParams == targetParams && this.State == AIState.FleeTo) ? targetParams.ThresholdMax : targetParams.ThresholdMin);
															}
															if (this.Character.HealthPercentage > healthThreshold)
															{
																continue;
															}
														}
													}
													else if (!this.canAttackItems)
													{
														continue;
													}
													if (targetItem.HasTag(Tags.GuardianShelter))
													{
														bool ignore = false;
														foreach (Character otherCharacter in Character.CharacterList)
														{
															if (otherCharacter != this.Character)
															{
																AIController aicontroller = otherCharacter.AIController;
																if (((aicontroller != null) ? aicontroller.SelectedAiTarget : null) == aiTarget && this.Character.IsFriendly(otherCharacter))
																{
																	ignore = true;
																	break;
																}
															}
														}
														if (ignore)
														{
															continue;
														}
													}
												}
												matchingTargetParams = targetParams;
											}
										}
									}
								}
								if (matchingTargetParams != null)
								{
									float valueModifier = 1f;
									if (targetCharacter != null)
									{
										if (targetCharacter.AIController is EnemyAIController)
										{
											Identifier tag = matchingTargetParams.Tag;
											bool flag2 = tag == Tags.Stronger;
											bool flag3 = flag2;
											if (flag3)
											{
												AIState state = this.State;
												bool flag = state == AIState.Escape || state - AIState.Flee <= 1;
												flag3 = flag;
											}
											if (flag3)
											{
												if (base.SelectedAiTarget == aiTarget)
												{
													valueModifier *= 2f;
												}
												if (this.IsBeingChasedBy(targetCharacter))
												{
													valueModifier *= 2f;
												}
												if (this.Character.CurrentHull != null && !base.VisibleHulls.Contains(targetCharacter.CurrentHull))
												{
													valueModifier /= 2f;
												}
											}
										}
									}
									else
									{
										Structure s2 = aiTarget.Entity as Structure;
										if (s2 != null)
										{
											bool isInnerWall2 = s2.Prefab.Tags.Contains("inner");
											valueModifier = 200f / s2.MaxHealth;
											for (int i = 0; i < s2.Sections.Length; i++)
											{
												WallSection section = s2.Sections[i];
												if (section.gap != null)
												{
													bool leadsInside = !section.gap.IsRoomToRoom && section.gap.FlowTargetHull != null;
													if (tryToGetInside)
													{
														if (!isCharacterInside)
														{
															if (this.CanPassThroughHole(s2, i))
															{
																valueModifier *= (leadsInside ? (this.IsAggressiveBoarder ? 5f : 1f) : 0f);
															}
															else if (this.IsAggressiveBoarder && leadsInside && this.canAttackWalls)
															{
																valueModifier *= 1f + section.gap.Open;
															}
														}
														else if (this.IsAggressiveBoarder)
														{
															if (!isInnerWall2)
															{
																valueModifier = 0f;
																break;
															}
															if (this.CanPassThroughHole(s2, i))
															{
																valueModifier *= (isInnerWall2 ? 0.5f : 0f);
															}
															else
															{
																if (!this.canAttackWalls)
																{
																	valueModifier = 0f;
																	break;
																}
																valueModifier = 0.1f;
															}
														}
														else
														{
															if (!this.canAttackWalls)
															{
																valueModifier = 0f;
																break;
															}
															valueModifier *= 1f - section.gap.Open * 0.25f;
															valueModifier = Math.Max(valueModifier, 0.1f);
														}
													}
													else
													{
														if (isInnerWall2 || !this.canAttackWalls)
														{
															valueModifier = 0f;
															break;
														}
														if (this.IsAggressiveBoarder)
														{
															valueModifier *= 1f + section.gap.Open;
														}
													}
													valueModifier = Math.Clamp(valueModifier, 0f, 5f);
												}
											}
										}
										if (door != null)
										{
											if (this.IsAggressiveBoarder)
											{
												if (this.Character.CurrentHull == null)
												{
													if (door.CanBeTraversed)
													{
														valueModifier = 5f;
													}
													else if (door.LinkedGap != null)
													{
														valueModifier = 1f + door.LinkedGap.Open * 4f;
													}
												}
												else
												{
													bool isOpen = door.CanBeTraversed;
													Gap linkedGap = door.LinkedGap;
													bool isOutdoor2 = linkedGap != null && linkedGap.FlowTargetHull != null && !linkedGap.IsRoomToRoom;
													valueModifier = (float)((isOpen || isOutdoor2) ? 0 : 1);
												}
											}
										}
										else
										{
											IDamageable damageable = aiTarget.Entity as IDamageable;
											if (damageable != null && damageable.Health <= 0f)
											{
												continue;
											}
										}
									}
									if (matchingTargetParams.State == AIState.Eat && this.Character.Params.Health.HealthRegenerationWhenEating > 0f && !this.Character.IsPet)
									{
										valueModifier *= MathHelper.Lerp(1f, 0.1f, this.Character.HealthPercentage / 100f);
									}
									valueModifier *= matchingTargetParams.Priority;
									if (valueModifier != 0f)
									{
										Identifier tag = matchingTargetParams.Tag;
										if (tag != Tags.Decoy)
										{
											if (this.SwarmBehavior != null && this.SwarmBehavior.Members.Any<AICharacter>())
											{
												using (List<AICharacter>.Enumerator enumerator4 = this.SwarmBehavior.Members.GetEnumerator())
												{
													while (enumerator4.MoveNext())
													{
														AICharacter otherCharacter2 = enumerator4.Current;
														if (otherCharacter2 != this.Character)
														{
															AIController aicontroller2 = otherCharacter2.AIController;
															if (((aicontroller2 != null) ? aicontroller2.SelectedAiTarget : null) == aiTarget)
															{
																valueModifier /= 2f;
															}
														}
													}
													goto IL_DC2;
												}
											}
											foreach (Character otherCharacter3 in Character.CharacterList)
											{
												if (otherCharacter3 != this.Character)
												{
													AIController aicontroller3 = otherCharacter3.AIController;
													if (((aicontroller3 != null) ? aicontroller3.SelectedAiTarget : null) == aiTarget && this.Character.IsFriendly(otherCharacter3))
													{
														valueModifier /= 2f;
													}
												}
											}
										}
										IL_DC2:
										if (aiTarget.IsWithinSector(base.WorldPosition))
										{
											Vector2 toTarget = aiTarget.WorldPosition - this.Character.WorldPosition;
											float dist = toTarget.Length();
											float nonModifiedDist = dist;
											if (this.targetMemories.ContainsKey(aiTarget))
											{
												dist *= 0.9f;
											}
											if (matchingTargetParams.PerceptionDistanceMultiplier > 0f)
											{
												dist /= matchingTargetParams.PerceptionDistanceMultiplier;
											}
											if ((matchingTargetParams.MaxPerceptionDistance <= 0f || dist * dist <= matchingTargetParams.MaxPerceptionDistance * matchingTargetParams.MaxPerceptionDistance) && (this.State != AIState.PlayDead || targetCharacter != null))
											{
												AITarget aiTarget3 = aiTarget;
												float dist2 = dist;
												bool flag4 = base.SelectedAiTarget != aiTarget;
												bool flag5 = flag4;
												if (!flag5)
												{
													AIState state = this.State;
													bool flag = state == AIState.PlayDead || state == AIState.Hiding;
													flag5 = flag;
												}
												if (this.CanPerceive(aiTarget3, dist2, -1f, flag5))
												{
													if (base.SelectedAiTarget == aiTarget)
													{
														if (this.Character.Submarine == null)
														{
															Entity entity = aiTarget.Entity;
															ISpatialEntity spatialEntity = entity;
															if (spatialEntity != null && ((ISpatialEntity)entity).Submarine != null)
															{
																tag = matchingTargetParams.Tag;
																if (!(tag == Tags.Door))
																{
																	Identifier tag2 = matchingTargetParams.Tag;
																	if (!(tag2 == Tags.Wall))
																	{
																		goto IL_106F;
																	}
																}
																Vector2 rayStart = this.Character.SimPosition;
																Vector2 rayEnd = aiTarget.SimPosition + spatialEntity.Submarine.SimPosition;
																Body closestBody = Submarine.PickBody(rayStart, rayEnd, null, new Category?(Category.Cat1 | Category.Cat8), true, null, true);
																if (closestBody != null)
																{
																	ISpatialEntity hit = closestBody.UserData as ISpatialEntity;
																	if (hit != null)
																	{
																		Vector2 hitPos = hit.SimPosition;
																		if (closestBody.UserData is Submarine)
																		{
																			hitPos = Submarine.LastPickedPosition;
																		}
																		else if (hit.Submarine != null)
																		{
																			hitPos += hit.Submarine.SimPosition;
																		}
																		float subHalfWidth = (float)spatialEntity.Submarine.Borders.Width / 2f;
																		float subHalfHeight = (float)spatialEntity.Submarine.Borders.Height / 2f;
																		Vector2 diff = ConvertUnits.ToDisplayUnits(rayEnd - hitPos);
																		bool isOtherSideOfTheSub = Math.Abs(diff.X) > subHalfWidth || Math.Abs(diff.Y) > subHalfHeight;
																		if (isOtherSideOfTheSub)
																		{
																			this.IgnoreTarget(aiTarget);
																			base.ResetAITarget();
																			continue;
																		}
																	}
																}
															}
														}
														IL_106F:
														valueModifier *= 1.1f;
													}
													if (!isBeingChased)
													{
														AIState state = matchingTargetParams.State;
														bool flag = state - AIState.Avoid <= 2;
														if (flag)
														{
															float reactDistance = matchingTargetParams.ReactDistance;
															if (reactDistance > 0f && reactDistance < dist)
															{
																continue;
															}
														}
													}
													dist = Math.Max(dist, 100f);
													targetMemory = this.GetTargetMemory(aiTarget, true, base.SelectedAiTarget != aiTarget);
													if (this.Character.Submarine != null && !this.Character.Submarine.Info.IsRuin && this.Character.CurrentHull != null)
													{
														float diff2 = Math.Abs(toTarget.Y) - this.Character.CurrentHull.Size.Y;
														if (diff2 > 0f)
														{
															dist *= MathHelper.Clamp(diff2 / 100f, 2f, 3f);
														}
													}
													if (this.Character.Submarine == null)
													{
														Entity entity2 = aiTarget.Entity;
														if (((entity2 != null) ? entity2.Submarine : null) != null && targetCharacter == null)
														{
															bool prioritizeSubCenter = matchingTargetParams.PrioritizeSubCenter;
															bool flag6 = prioritizeSubCenter;
															if (!flag6)
															{
																AttackPattern attackPattern = matchingTargetParams.AttackPattern;
																bool flag = attackPattern - AttackPattern.Sweep <= 1;
																flag6 = flag;
															}
															if (flag6 && !isAnyTargetClose)
															{
																if (Submarine.MainSubs.Contains(aiTarget.Entity.Submarine))
																{
																	float horizontalDistanceToSubCenter = Math.Abs(aiTarget.WorldPosition.X - aiTarget.Entity.Submarine.WorldPosition.X);
																	dist *= MathHelper.Lerp(1f, 5f, MathUtils.InverseLerp(0f, 10000f, horizontalDistanceToSubCenter));
																}
																else if (matchingTargetParams.AttackPattern == AttackPattern.Circle)
																{
																	dist *= 5f;
																}
															}
														}
													}
													if (targetCharacter != null && this.Character.CurrentHull != null && this.Character.CurrentHull == targetCharacter.CurrentHull)
													{
														dist /= 2f;
													}
													AIState state3 = matchingTargetParams.State;
													if (state3 != AIState.Escape && state3 != AIState.Avoid && (matchingTargetParams.State != AIState.Attack || this.State != matchingTargetParams.State || base.SelectedAiTarget != aiTarget))
													{
														AITarget aiTarget2 = aiTarget;
														bool flag7;
														if (aiTarget2 == null)
														{
															flag7 = (null != null);
														}
														else
														{
															Entity entity3 = aiTarget2.Entity;
															flag7 = (((entity3 != null) ? entity3.Submarine : null) != null);
														}
														Vector2 vector;
														if ((!flag7 || aiTarget.Entity.Submarine != this.Character.Submarine) && !this.IsPositionInsideAllowedZone(aiTarget.WorldPosition, out vector))
														{
															bool isTargetInPlayerTeam = EnemyAIController.IsTargetInPlayerTeam(aiTarget);
															if (this.Character.LastAttackers.None((Character.Attacker a) => a.Damage > 0f && a.Character != null && (a.Character == aiTarget.Entity || (a.Character.IsOnPlayerTeam & isTargetInPlayerTeam))))
															{
																continue;
															}
														}
													}
													valueModifier *= targetMemory.Priority / MathF.Sqrt(dist);
													if (valueModifier > this.targetValue)
													{
														Item j = aiTarget.Entity as Item;
														if (j != null)
														{
															Character owner = EnemyAIController.GetOwner(j);
															if (owner == this.Character)
															{
																continue;
															}
															if (owner != null)
															{
																if ((owner.AiTarget != null && this.ignoredTargets.Contains(owner.AiTarget)) || this.Character.IsFriendly(owner) || owner.HasAbilityFlag(AbilityFlags.IgnoredByEnemyAI))
																{
																	continue;
																}
																if (this.GetTargetParams(this.GetTargetingTags(owner.AiTarget)).Any((CharacterParams.TargetParams t) => t.State == AIState.Idle))
																{
																	continue;
																}
															}
														}
														if (targetCharacter != null)
														{
															if (this.Character.CurrentHull != null && targetCharacter.CurrentHull != this.Character.CurrentHull)
															{
																AIState state = matchingTargetParams.State;
																bool flag = state == AIState.Eat || state == AIState.Observe;
																bool flag8 = flag;
																bool flag9 = flag8;
																if (!flag9)
																{
																	AIState state4 = matchingTargetParams.State;
																	bool flag10 = state4 == AIState.Protect || state4 == AIState.Follow;
																	flag9 = (flag10 && (!this.Character.CanClimb || !this.Character.CanInteract || !this.AIParams.CanOpenDoors || !this.Character.Params.UsePathFinding));
																}
																if (flag9 && !base.VisibleHulls.Contains(targetCharacter.CurrentHull))
																{
																	continue;
																}
															}
															if (targetCharacter.Submarine != this.Character.Submarine || targetCharacter.CurrentHull == null != (this.Character.CurrentHull == null))
															{
																if (targetCharacter.Submarine != null)
																{
																	if (this.Character.Submarine != null && !targetCharacter.Submarine.IsConnectedTo(this.Character.Submarine))
																	{
																		continue;
																	}
																	valueModifier *= 0.5f;
																}
																else if (this.Character.CurrentHull != null)
																{
																	AITarget selectedAiTarget = base.SelectedAiTarget;
																	if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) != targetCharacter)
																	{
																		continue;
																	}
																}
															}
															else if (targetCharacter.Submarine == null && this.Character.Submarine == null && dist > Math.Clamp(ConvertUnits.ToDisplayUnits(this.colliderLength) * 10f, 1000f, 5000f) && Submarine.PickBodies(base.SimPosition, targetCharacter.SimPosition, null, new Category?(Category.Cat8), true, null, false).Any<Body>())
															{
																continue;
															}
														}
														newTarget = aiTarget;
														selectedTargetParams = matchingTargetParams;
														this.targetValue = valueModifier;
														if (!isAnyTargetClose)
														{
															isAnyTargetClose = (ConvertUnits.ToDisplayUnits(this.colliderLength) > nonModifiedDist);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			this.currentTargetingParams = selectedTargetParams;
			this.currentTargetMemory = targetMemory;
			CharacterParams.TargetParams targetParams2 = this.currentTargetingParams;
			this.State = ((targetParams2 != null) ? targetParams2.State : AIState.Idle);
			base.SelectedAiTarget = newTarget;
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			bool flag11 = latchOntoAI == null || !latchOntoAI.IsAttached || this.wallTarget != null;
			bool flag12 = flag11;
			if (flag12)
			{
				AIState state = this.State;
				bool flag = state == AIState.Attack || state - AIState.Aggressive <= 1;
				flag12 = flag;
			}
			if (flag12)
			{
				this.UpdateWallTarget(this.requiredHoleCount);
			}
			this.updateTargetsTimer = this.updateTargetsInterval * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000CE0 RID: 3296 RVA: 0x0007D36C File Offset: 0x0007B56C
		private void UpdateWallTarget(int requiredHoleCount)
		{
			EnemyAIController.<>c__DisplayClass199_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.requiredHoleCount = requiredHoleCount;
			this.wallTarget = null;
			if (base.SelectedAiTarget == null)
			{
				return;
			}
			if (base.SelectedAiTarget.Entity == null)
			{
				return;
			}
			if (!this.canAttackWalls)
			{
				return;
			}
			if (base.HasValidPath(true, true, null))
			{
				return;
			}
			this.wallHits.Clear();
			CS$<>8__locals1.wall = null;
			Vector2 refPos = (this.AttackLimb != null) ? this.AttackLimb.SimPosition : base.SimPosition;
			if (this.AIParams.WallTargetingMethod.HasFlag(WallTargetingMethod.Target))
			{
				Vector2 rayStart = refPos;
				Vector2 rayEnd = base.SelectedAiTarget.SimPosition;
				if (base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null)
				{
					rayStart -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				else if (base.SelectedAiTarget.Entity.Submarine == null && this.Character.Submarine != null)
				{
					rayEnd -= this.Character.Submarine.SimPosition;
				}
				this.<UpdateWallTarget>g__DoRayCast|199_0(rayStart, rayEnd, ref CS$<>8__locals1);
			}
			if (this.AIParams.WallTargetingMethod.HasFlag(WallTargetingMethod.Heading))
			{
				Vector2 rayStart2 = refPos;
				Vector2 rayEnd2 = rayStart2 + VectorExtensions.Forward(this.Character.AnimController.Collider.Rotation + 1.5707964f, this.avoidLookAheadDistance * 5f);
				if (base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null)
				{
					rayStart2 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
					rayEnd2 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				else if (base.SelectedAiTarget.Entity.Submarine == null && this.Character.Submarine != null)
				{
					rayStart2 -= this.Character.Submarine.SimPosition;
					rayEnd2 -= this.Character.Submarine.SimPosition;
				}
				this.<UpdateWallTarget>g__DoRayCast|199_0(rayStart2, rayEnd2, ref CS$<>8__locals1);
			}
			if (this.AIParams.WallTargetingMethod.HasFlag(WallTargetingMethod.Steering))
			{
				Vector2 rayStart3 = refPos;
				Vector2 rayEnd3 = rayStart3 + base.Steering * 5f;
				if (base.SelectedAiTarget.Entity.Submarine != null && this.Character.Submarine == null)
				{
					rayStart3 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
					rayEnd3 -= base.SelectedAiTarget.Entity.Submarine.SimPosition;
				}
				else if (base.SelectedAiTarget.Entity.Submarine == null && this.Character.Submarine != null)
				{
					rayStart3 -= this.Character.Submarine.SimPosition;
					rayEnd3 -= this.Character.Submarine.SimPosition;
				}
				this.<UpdateWallTarget>g__DoRayCast|199_0(rayStart3, rayEnd3, ref CS$<>8__locals1);
			}
			if (this.wallHits.Any<ValueTuple<Body, int, Vector2>>())
			{
				float targetDistance = ConvertUnits.ToSimUnits(base.SelectedAiTarget.WorldPosition - ((this.AttackLimb != null) ? this.AttackLimb.WorldPosition : base.WorldPosition)).LengthSquared();
				Body closestBody = null;
				float closestDistance = 0f;
				int sectionIndex = -1;
				Vector2 sectionPos = Vector2.Zero;
				foreach (ValueTuple<Body, int, Vector2> valueTuple in this.wallHits)
				{
					Body body = valueTuple.Item1;
					int index = valueTuple.Item2;
					Vector2 sectionPosition = valueTuple.Item3;
					Structure structure = body.UserData as Structure;
					float distance = Vector2.DistanceSquared(refPos, Submarine.GetRelativeSimPosition(ConvertUnits.ToSimUnits(sectionPosition), this.Character.Submarine, structure.Submarine));
					if (distance <= targetDistance && (closestBody == null || closestDistance == 0f || distance < closestDistance))
					{
						closestBody = body;
						closestDistance = distance;
						CS$<>8__locals1.wall = structure;
						sectionPos = sectionPosition;
						sectionIndex = index;
					}
				}
				if (closestBody == null || sectionIndex == -1)
				{
					return;
				}
				Vector2 attachTargetNormal;
				if (CS$<>8__locals1.wall.IsHorizontal)
				{
					attachTargetNormal = new Vector2(0f, (float)Math.Sign(base.WorldPosition.Y - CS$<>8__locals1.wall.WorldPosition.Y));
					sectionPos.Y += ((CS$<>8__locals1.wall.BodyHeight <= 0f) ? ((float)CS$<>8__locals1.wall.Rect.Height) : CS$<>8__locals1.wall.BodyHeight) / 2f * attachTargetNormal.Y;
				}
				else
				{
					attachTargetNormal = new Vector2((float)Math.Sign(base.WorldPosition.X - CS$<>8__locals1.wall.WorldPosition.X), 0f);
					sectionPos.X += ((CS$<>8__locals1.wall.BodyWidth <= 0f) ? ((float)CS$<>8__locals1.wall.Rect.Width) : CS$<>8__locals1.wall.BodyWidth) / 2f * attachTargetNormal.X;
				}
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.SetAttachTarget(CS$<>8__locals1.wall, ConvertUnits.ToSimUnits(sectionPos), attachTargetNormal);
				}
				if (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.True || (!CS$<>8__locals1.wall.SectionBodyDisabled(sectionIndex) && !base.IsWallDisabled(CS$<>8__locals1.wall)))
				{
					if (!CS$<>8__locals1.wall.NoAITarget || this.Character.AnimController.CanEnterSubmarine != CanEnterSubmarine.True)
					{
						this.wallTarget = new EnemyAIController.WallTarget(sectionPos, CS$<>8__locals1.wall, sectionIndex);
						return;
					}
					Item i = base.SelectedAiTarget.Entity as Item;
					if (i == null || i.GetComponent<Door>() == null)
					{
						this.IgnoreTarget(base.SelectedAiTarget);
						base.ResetAITarget();
						return;
					}
				}
				else
				{
					this.IgnoreTarget(base.SelectedAiTarget);
					base.ResetAITarget();
				}
			}
		}

		// Token: 0x06000CE1 RID: 3297 RVA: 0x0007D9A0 File Offset: 0x0007BBA0
		private bool TrySteerThroughGaps(float deltaTime)
		{
			if (this.wallTarget != null && this.wallTarget.SectionIndex > -1 && base.CanPassThroughHole(this.wallTarget.Structure, this.wallTarget.SectionIndex, this.requiredHoleCount))
			{
				WallSection section = this.wallTarget.Structure.GetSection(this.wallTarget.SectionIndex);
				Vector2 targetPos = this.wallTarget.Structure.SectionPosition(this.wallTarget.SectionIndex, true);
				return ((section != null) ? section.gap : null) != null && this.SteerThroughGap(this.wallTarget.Structure, section, targetPos, deltaTime);
			}
			if (base.SelectedAiTarget != null)
			{
				Structure wall = base.SelectedAiTarget.Entity as Structure;
				if (wall != null)
				{
					for (int i = 0; i < wall.Sections.Length; i++)
					{
						WallSection section2 = wall.Sections[i];
						if (base.CanPassThroughHole(wall, i, this.requiredHoleCount) && ((section2 != null) ? section2.gap : null) != null)
						{
							return this.SteerThroughGap(wall, section2, wall.SectionPosition(i, true), deltaTime);
						}
					}
				}
				else
				{
					Item j = base.SelectedAiTarget.Entity as Item;
					if (j != null)
					{
						Door door = j.GetComponent<Door>();
						bool flag;
						if (door == null)
						{
							flag = (null != null);
						}
						else
						{
							Gap linkedGap = door.LinkedGap;
							flag = (((linkedGap != null) ? linkedGap.FlowTargetHull : null) != null);
						}
						if (flag && !door.LinkedGap.IsRoomToRoom && door.CanBeTraversed && (this.Character.AnimController.CanWalk || door.LinkedGap.FlowTargetHull.WaterPercentage > 25f) && door.LinkedGap.Size > ConvertUnits.ToDisplayUnits(this.colliderWidth))
						{
							float maxDistance = Math.Max(ConvertUnits.ToDisplayUnits(this.colliderLength), 100f);
							return this.SteerThroughGap(door.LinkedGap, door.LinkedGap.FlowTargetHull.WorldPosition, deltaTime, maxDistance);
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000CE2 RID: 3298 RVA: 0x0007DB98 File Offset: 0x0007BD98
		private AITargetMemory GetTargetMemory(AITarget target, bool addIfNotFound = false, bool keepAlive = false)
		{
			AITargetMemory memory;
			if (!this.targetMemories.TryGetValue(target, out memory) && addIfNotFound)
			{
				memory = new AITargetMemory(target, 10f);
				this.targetMemories.Add(target, memory);
			}
			if (keepAlive)
			{
				memory.Priority = Math.Max(memory.Priority, 10f);
			}
			return memory;
		}

		// Token: 0x06000CE3 RID: 3299 RVA: 0x0007DBEC File Offset: 0x0007BDEC
		private void UpdateCurrentMemoryLocation()
		{
			if (this._selectedAiTarget != null)
			{
				if (this._selectedAiTarget.Entity == null || this._selectedAiTarget.Entity.Removed)
				{
					this._selectedAiTarget = null;
					return;
				}
				if (this.CanPerceive(this._selectedAiTarget, -1f, -1f, false))
				{
					AITargetMemory memory = this.GetTargetMemory(this._selectedAiTarget, false, false);
					if (memory != null)
					{
						memory.Location = this._selectedAiTarget.WorldPosition;
					}
				}
			}
		}

		// Token: 0x06000CE4 RID: 3300 RVA: 0x0007DC64 File Offset: 0x0007BE64
		private void FadeMemories(float deltaTime)
		{
			this.removals.Clear();
			foreach (KeyValuePair<AITarget, AITargetMemory> kvp in this.targetMemories)
			{
				AITarget target = kvp.Key;
				AITargetMemory memory = kvp.Value;
				float fadeTime = this.memoryFadeTime;
				if (target == base.SelectedAiTarget)
				{
					fadeTime = 0f;
				}
				else if (target == this._lastAiTarget)
				{
					fadeTime /= 2f;
				}
				memory.Priority -= fadeTime * deltaTime;
				if (memory.Priority <= 1f || target.Entity == null || target.Entity.Removed || !AITarget.List.Contains(target))
				{
					this.removals.Add(target);
				}
			}
			this.removals.ForEach(delegate(AITarget r)
			{
				this.targetMemories.Remove(r);
			});
		}

		// Token: 0x06000CE5 RID: 3301 RVA: 0x0007DD64 File Offset: 0x0007BF64
		public void IgnoreTarget(AITarget target)
		{
			if (target == null)
			{
				return;
			}
			this.ignoredTargets.Add(target);
			this.targetIgnoreTimer = this.targetIgnoreTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000CE6 RID: 3302 RVA: 0x0007DD94 File Offset: 0x0007BF94
		public void LaunchTrigger(StatusEffect.AITrigger trigger)
		{
			if (trigger.IsTriggered)
			{
				return;
			}
			if (this.activeTriggers.ContainsKey(trigger))
			{
				return;
			}
			if (this.activeTriggers.ContainsValue(this.currentTargetingParams))
			{
				if (!trigger.AllowToOverride)
				{
					return;
				}
				KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams> existingTrigger = this.activeTriggers.FirstOrDefault((KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams> kvp) => kvp.Value == this.currentTargetingParams && kvp.Key.AllowToBeOverridden);
				if (existingTrigger.Key == null)
				{
					return;
				}
				this.activeTriggers.Remove(existingTrigger.Key);
			}
			trigger.Launch();
			this.activeTriggers.Add(trigger, this.currentTargetingParams);
			this.ChangeParams(this.currentTargetingParams, trigger.State, null);
		}

		// Token: 0x06000CE7 RID: 3303 RVA: 0x0007DE40 File Offset: 0x0007C040
		private void UpdateTriggers(float deltaTime)
		{
			foreach (KeyValuePair<StatusEffect.AITrigger, CharacterParams.TargetParams> triggerObject in this.activeTriggers)
			{
				StatusEffect.AITrigger trigger = triggerObject.Key;
				if (!trigger.IsPermanent)
				{
					trigger.UpdateTimer(deltaTime);
					if (!trigger.IsActive)
					{
						trigger.Reset();
						this.ResetParams(triggerObject.Value);
						this.inactiveTriggers.Add(trigger);
					}
				}
			}
			foreach (StatusEffect.AITrigger trigger2 in this.inactiveTriggers)
			{
				this.activeTriggers.Remove(trigger2);
			}
			this.inactiveTriggers.Clear();
		}

		// Token: 0x06000CE8 RID: 3304 RVA: 0x0007DF20 File Offset: 0x0007C120
		private bool TryResetOriginalState(Identifier tag)
		{
			if (!this.modifiedParams.ContainsKey(tag))
			{
				return false;
			}
			IEnumerable<CharacterParams.TargetParams> matchingParams;
			if (this.AIParams.TryGetTargets(tag, out matchingParams))
			{
				using (IEnumerator<CharacterParams.TargetParams> enumerator = matchingParams.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						CharacterParams.TargetParams targetParams = enumerator.Current;
						this.modifiedParams.Remove(tag);
						if (this.tempParams.ContainsKey(tag))
						{
							this.tempParams.Values.ForEach(delegate(CharacterParams.TargetParams t)
							{
								this.AIParams.RemoveTarget(t);
							});
							this.tempParams.Remove(tag);
						}
						this.ResetParams(targetParams);
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000CE9 RID: 3305 RVA: 0x0007DFD4 File Offset: 0x0007C1D4
		private void ChangeParams(CharacterParams.TargetParams targetParams, AIState state, float? priority = null)
		{
			if (targetParams == null)
			{
				return;
			}
			if (priority != null)
			{
				targetParams.Priority = priority.Value;
			}
			targetParams.State = state;
		}

		// Token: 0x06000CEA RID: 3306 RVA: 0x0007DFF8 File Offset: 0x0007C1F8
		private void ResetParams(CharacterParams.TargetParams targetParams)
		{
			if (targetParams != null)
			{
				targetParams.Reset();
			}
			bool flag = this.currentTargetingParams == targetParams;
			bool flag2 = flag;
			if (!flag2)
			{
				AIState state = this.State;
				bool flag3 = state == AIState.Idle || state == AIState.Patrol;
				flag2 = flag3;
			}
			if (flag2)
			{
				base.ResetAITarget();
				this.State = AIState.Idle;
				this.PreviousState = AIState.Idle;
			}
		}

		// Token: 0x06000CEB RID: 3307 RVA: 0x0007E04C File Offset: 0x0007C24C
		private void ChangeParams(Identifier tag, AIState state, float? priority = null, bool onlyExisting = false, bool ignoreAttacksIfNotInSameSub = false)
		{
			IEnumerable<CharacterParams.TargetParams> existingTargetParams = this.GetTargetParams(tag);
			if (existingTargetParams.None(null))
			{
				CharacterParams.TargetParams targetParams;
				if (!onlyExisting && !this.tempParams.ContainsKey(tag) && this.AIParams.TryAddNewTarget(tag, state, priority.GetValueOrDefault(10f), out targetParams))
				{
					this.tempParams.Add(tag, targetParams);
					return;
				}
			}
			else
			{
				foreach (CharacterParams.TargetParams targetParams2 in existingTargetParams)
				{
					if (priority != null)
					{
						targetParams2.Priority = Math.Max(targetParams2.Priority, priority.Value);
					}
					targetParams2.State = state;
					if (state == AIState.Attack)
					{
						targetParams2.IgnoreIfNotInSameSub = ignoreAttacksIfNotInSameSub;
						targetParams2.IgnoreInside = false;
						targetParams2.IgnoreOutside = false;
						targetParams2.IgnoreTargetInside = false;
						targetParams2.IgnoreTargetOutside = false;
						targetParams2.IgnoreIncapacitated = false;
					}
				}
				this.modifiedParams.TryAdd(tag, existingTargetParams);
			}
		}

		// Token: 0x06000CEC RID: 3308 RVA: 0x0007E14C File Offset: 0x0007C34C
		private void ChangeTargetState(Identifier tag, AIState state, float? priority = null)
		{
			this.isStateChanged = true;
			this.SetStateResetTimer();
			this.ChangeParams(tag, state, priority, false, false);
		}

		// Token: 0x06000CED RID: 3309 RVA: 0x0007E168 File Offset: 0x0007C368
		private void ChangeTargetState(Character target, AIState state, float? priority = null)
		{
			this.isStateChanged = true;
			this.SetStateResetTimer();
			if (!this.Character.IsPet || !target.IsHuman)
			{
				this.ChangeParams(target.SpeciesName, state, priority, false, !target.IsHuman);
			}
			if (target.IsHuman)
			{
				CharacterParams.TargetParams targetParams;
				if (this.AIParams.TryGetHighestPriorityTarget(Tags.Human, out targetParams))
				{
					priority = new float?(targetParams.Priority);
				}
				bool flag = state - AIState.Attack <= 1;
				if (flag)
				{
					this.ChangeParams(Tags.Weapon, state, priority, false, false);
					this.ChangeParams(Tags.ToolItem, state, priority, false, false);
				}
				if (state == AIState.Attack)
				{
					if (target.Submarine != null && this.Character.Submarine == null && (this.canAttackDoors || this.canAttackWalls))
					{
						this.ChangeParams(Tags.Room, state, priority / (float)2, false, false);
						if (this.canAttackWalls)
						{
							this.ChangeParams(Tags.Wall, state, priority / (float)2, false, false);
						}
						if (this.canAttackDoors && this.IsAggressiveBoarder)
						{
							this.ChangeParams(Tags.Door, state, priority / (float)2, false, false);
						}
					}
					this.ChangeParams(Tags.Provocative, state, priority, true, false);
				}
			}
		}

		// Token: 0x06000CEE RID: 3310 RVA: 0x0007E306 File Offset: 0x0007C506
		private void ResetOriginalState()
		{
			this.isStateChanged = false;
			this.modifiedParams.Keys.ForEachMod(delegate(Identifier tag)
			{
				this.TryResetOriginalState(tag);
			});
		}

		// Token: 0x06000CEF RID: 3311 RVA: 0x0007E32C File Offset: 0x0007C52C
		protected override void OnTargetChanged(AITarget previousTarget, AITarget newTarget)
		{
			base.OnTargetChanged(previousTarget, newTarget);
			if ((newTarget != null || this.wallTarget != null) && this.IsLatchedOnSub)
			{
				Structure wall = ((newTarget != null) ? newTarget.Entity : null) as Structure;
				if (wall == null)
				{
					EnemyAIController.WallTarget wallTarget = this.wallTarget;
					wall = ((wallTarget != null) ? wallTarget.Structure : null);
				}
				bool flag;
				if (((wall != null) ? wall.Bodies : null) != null)
				{
					if (!wall.Bodies.Contains(this.LatchOntoAI.AttachJoints[0].BodyB))
					{
						Submarine submarine = wall.Submarine;
						Body body;
						if (submarine == null)
						{
							body = null;
						}
						else
						{
							PhysicsBody physicsBody = submarine.PhysicsBody;
							body = ((physicsBody != null) ? physicsBody.FarseerBody : null);
						}
						flag = (body != this.LatchOntoAI.AttachJoints[0].BodyB);
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					flag = true;
				}
				bool releaseTarget = flag;
				if (!releaseTarget)
				{
					for (int i = 0; i < wall.Sections.Length; i++)
					{
						if (this.CanPassThroughHole(wall, i))
						{
							releaseTarget = true;
						}
					}
				}
				if (releaseTarget)
				{
					this.wallTarget = null;
					this.LatchOntoAI.DeattachFromBody(true, 1f);
				}
			}
			else
			{
				this.wallTarget = null;
			}
			if (newTarget == null)
			{
				return;
			}
			if (this.currentTargetingParams != null)
			{
				this.observeTimer = this.currentTargetingParams.Timer * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
			this.reachTimer = 0f;
			this.sinTime = 0f;
			if (this.breakCircling && this.strikeTimer <= 0f && this.CirclePhase != CirclePhase.CloseIn)
			{
				this.CirclePhase = CirclePhase.Start;
			}
		}

		// Token: 0x06000CF0 RID: 3312 RVA: 0x0007E4A4 File Offset: 0x0007C6A4
		protected override void OnStateChanged(AIState from, AIState to)
		{
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			if (latchOntoAI != null)
			{
				latchOntoAI.DeattachFromBody(true, 0f);
			}
			if (this.disableTailCoroutine != null)
			{
				bool flag = from - AIState.HideTo <= 1;
				bool flag2 = flag;
				bool flag3 = flag2;
				if (flag3)
				{
					bool flag4 = to - AIState.HideTo <= 1;
					flag3 = flag4;
				}
				if (!flag3)
				{
					CoroutineManager.StopCoroutines(this.disableTailCoroutine);
					this.Character.AnimController.RestoreTemporarilyDisabled();
					this.disableTailCoroutine = null;
				}
			}
			if (to == AIState.Hiding)
			{
				this.ReleaseDragTargets();
			}
			this.Character.AnimController.ReleaseStuckLimbs();
			this.AttackLimb = null;
			this.movementMargin = 0f;
			base.ResetEscape();
			if (this.isStateChanged && to == AIState.Idle && from != to)
			{
				this.SetStateResetTimer();
			}
			this.blockCheckTimer = 0f;
			this.reachTimer = 0f;
			this.sinTime = 0f;
			if (this.breakCircling && this.strikeTimer <= 0f && this.CirclePhase != CirclePhase.CloseIn)
			{
				this.CirclePhase = CirclePhase.Start;
			}
			if (to != AIState.Idle)
			{
				this.playDeadTimer = 60f;
			}
		}

		// Token: 0x06000CF1 RID: 3313 RVA: 0x0007E5B9 File Offset: 0x0007C7B9
		private void SetStateResetTimer()
		{
			this.stateResetTimer = this.stateResetCooldown * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
		}

		// Token: 0x06000CF2 RID: 3314 RVA: 0x0007E5D8 File Offset: 0x0007C7D8
		private float GetPerceivingRange(AITarget target)
		{
			float maxSightOrSoundRange = Math.Max(target.SightRange * this.Sight, target.SoundRange * this.Hearing);
			if (this.AIParams.MaxPerceptionDistance >= 0f && maxSightOrSoundRange > this.AIParams.MaxPerceptionDistance)
			{
				return this.AIParams.MaxPerceptionDistance;
			}
			return maxSightOrSoundRange;
		}

		// Token: 0x06000CF3 RID: 3315 RVA: 0x0007E634 File Offset: 0x0007C834
		private bool CanPerceive(AITarget target, float dist = -1f, float distSquared = -1f, bool checkVisibility = false)
		{
			if (((target != null) ? target.Entity : null) == null)
			{
				return false;
			}
			if (checkVisibility)
			{
				Submarine mySub = this.Character.Submarine;
				Submarine targetSub = target.Entity.Submarine;
				checkVisibility = ((this.Character.IsPet && (mySub != null || targetSub != null)) || (mySub != null && (targetSub == null || (targetSub == mySub && !targetSub.Info.IsPlayer))));
			}
			bool insideSightRange;
			bool insideSoundRange;
			if (dist > 0f)
			{
				if (this.AIParams.MaxPerceptionDistance >= 0f && dist > this.AIParams.MaxPerceptionDistance)
				{
					return false;
				}
				insideSightRange = EnemyAIController.<CanPerceive>g__IsInRange|230_0(dist, target.SightRange, this.Sight);
				if (!checkVisibility && insideSightRange)
				{
					return true;
				}
				insideSoundRange = EnemyAIController.<CanPerceive>g__IsInRange|230_0(dist, target.SoundRange, this.Hearing);
			}
			else
			{
				if (distSquared < 0f)
				{
					distSquared = Vector2.DistanceSquared(this.Character.WorldPosition, target.WorldPosition);
				}
				if (this.AIParams.MaxPerceptionDistance >= 0f && distSquared > this.AIParams.MaxPerceptionDistance * this.AIParams.MaxPerceptionDistance)
				{
					return false;
				}
				insideSightRange = EnemyAIController.<CanPerceive>g__IsInRangeSqr|230_1(distSquared, target.SightRange, this.Sight);
				if (!checkVisibility && insideSightRange)
				{
					return true;
				}
				insideSoundRange = EnemyAIController.<CanPerceive>g__IsInRangeSqr|230_1(distSquared, target.SoundRange, this.Hearing);
			}
			if (!checkVisibility)
			{
				return insideSightRange || insideSoundRange;
			}
			if (!insideSightRange && !insideSoundRange)
			{
				return false;
			}
			Character c = target.Entity as Character;
			if (c == null || !base.VisibleHulls.Contains(c.CurrentHull))
			{
				Item i = target.Entity as Item;
				if (i == null || !base.VisibleHulls.Contains(i.CurrentHull))
				{
					if (dist > 0f)
					{
						return EnemyAIController.<CanPerceive>g__IsInRange|230_0(dist, target.SoundRange, this.Hearing / 2f);
					}
					if (distSquared < 0f)
					{
						distSquared = Vector2.DistanceSquared(this.Character.WorldPosition, target.WorldPosition);
					}
					return EnemyAIController.<CanPerceive>g__IsInRangeSqr|230_1(distSquared, target.SoundRange, this.Hearing / 2f);
				}
			}
			return insideSightRange || insideSoundRange;
		}

		// Token: 0x06000CF4 RID: 3316 RVA: 0x0007E83C File Offset: 0x0007CA3C
		public void ReevaluateAttacks()
		{
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			this.canAttackWalls = (latchOntoAI != null && latchOntoAI.AttachToSub);
			this.canAttackDoors = false;
			this.canAttackCharacters = false;
			this.canAttackItems = false;
			foreach (Limb limb in this.Character.AnimController.Limbs)
			{
				if (!limb.IsSevered && !limb.Disabled && limb.attack != null)
				{
					if (!this.canAttackWalls)
					{
						this.canAttackWalls = (limb.attack.StructureDamage > 0f || (limb.attack.Ranged && limb.attack.IsValidTarget(AttackTarget.Structure)));
					}
					if (!this.canAttackDoors)
					{
						this.canAttackDoors = ((limb.attack.ItemDamage > 0f || limb.attack.Ranged) && limb.attack.IsValidTarget(AttackTarget.Structure));
					}
					if (!this.canAttackItems)
					{
						this.canAttackItems = (this.canAttackDoors || ((limb.attack.ItemDamage > 0f || limb.attack.Ranged) && limb.attack.IsValidTarget(AttackTarget.Structure | AttackTarget.Item)));
					}
					if (!this.canAttackCharacters)
					{
						this.canAttackCharacters = limb.attack.IsValidTarget(AttackTarget.Character);
					}
				}
			}
			if (this.PathSteering != null)
			{
				this.PathSteering.CanBreakDoors = this.canAttackDoors;
			}
		}

		// Token: 0x06000CF5 RID: 3317 RVA: 0x0007E9B8 File Offset: 0x0007CBB8
		private bool IsPositionInsideAllowedZone(Vector2 pos, out Vector2 targetDir)
		{
			targetDir = Vector2.Zero;
			if (Level.Loaded == null)
			{
				return true;
			}
			if (Level.Loaded.LevelData.Biome.IsEndBiome)
			{
				return true;
			}
			if (this.AIParams.AvoidAbyss)
			{
				if (pos.Y < (float)Level.Loaded.AbyssStart)
				{
					targetDir = Vector2.UnitY;
				}
			}
			else if (this.AIParams.StayInAbyss)
			{
				if (pos.Y > (float)Level.Loaded.AbyssStart)
				{
					targetDir = -Vector2.UnitY;
				}
				else if (pos.Y < (float)Level.Loaded.AbyssEnd)
				{
					targetDir = Vector2.UnitY;
				}
			}
			float margin = 30000f;
			if (pos.X < -margin)
			{
				targetDir = Vector2.UnitX;
			}
			else if (pos.X > (float)Level.Loaded.Size.X + margin)
			{
				targetDir = -Vector2.UnitX;
			}
			return targetDir == Vector2.Zero;
		}

		// Token: 0x06000CF6 RID: 3318 RVA: 0x0007EAC8 File Offset: 0x0007CCC8
		private void SteerInsideLevel(float deltaTime)
		{
			if (base.SteeringManager is IndoorsSteeringManager)
			{
				return;
			}
			if (Level.Loaded == null)
			{
				return;
			}
			if (this.State == AIState.Attack && this.returnTimer <= 0f)
			{
				return;
			}
			float returnTime = 5f;
			Vector2 targetDir;
			if (!this.IsPositionInsideAllowedZone(base.WorldPosition, out targetDir))
			{
				this.returnDir = targetDir;
				this.returnTimer = returnTime * Rand.Range(0.75f, 1.25f, Rand.RandSync.Unsynced);
			}
			if (this.returnTimer > 0f)
			{
				this.returnTimer -= deltaTime;
				base.SteeringManager.Reset();
				base.SteeringManager.SteeringManual(deltaTime, this.returnDir * 10f);
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 15f);
			}
		}

		// Token: 0x06000CF7 RID: 3319 RVA: 0x0007EB90 File Offset: 0x0007CD90
		public override bool SteerThroughGap(Structure wall, WallSection section, Vector2 targetWorldPos, float deltaTime)
		{
			base.IsTryingToSteerThroughGap = true;
			this.wallTarget = null;
			LatchOntoAI latchOntoAI = this.LatchOntoAI;
			if (latchOntoAI != null)
			{
				latchOntoAI.DeattachFromBody(true, 2f);
			}
			this.Character.AnimController.ReleaseStuckLimbs();
			bool success = base.SteerThroughGap(wall, section, targetWorldPos, deltaTime);
			if (success)
			{
				base.SelectedAiTarget = ((this.Character.CurrentHull != null) ? section.gap.AiTarget : wall.AiTarget);
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 1f);
			}
			base.IsSteeringThroughGap = success;
			return success;
		}

		// Token: 0x06000CF8 RID: 3320 RVA: 0x0007EC28 File Offset: 0x0007CE28
		public override bool SteerThroughGap(Gap gap, Vector2 targetWorldPos, float deltaTime, float maxDistance = -1f)
		{
			bool success = base.SteerThroughGap(gap, targetWorldPos, deltaTime, maxDistance);
			if (success)
			{
				this.wallTarget = null;
				LatchOntoAI latchOntoAI = this.LatchOntoAI;
				if (latchOntoAI != null)
				{
					latchOntoAI.DeattachFromBody(true, 2f);
				}
				this.Character.AnimController.ReleaseStuckLimbs();
				base.SteeringManager.SteeringAvoid(deltaTime, this.avoidLookAheadDistance, 1f);
			}
			base.IsSteeringThroughGap = success;
			return success;
		}

		// Token: 0x06000CF9 RID: 3321 RVA: 0x0007EC91 File Offset: 0x0007CE91
		public bool CanPassThroughHole(Structure wall, int sectionIndex)
		{
			return base.CanPassThroughHole(wall, sectionIndex, this.requiredHoleCount);
		}

		// Token: 0x06000CFA RID: 3322 RVA: 0x0007ECA4 File Offset: 0x0007CEA4
		public override bool Escape(float deltaTime)
		{
			EnemyAIController.<>c__DisplayClass239_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (base.SelectedAiTarget != null && (base.SelectedAiTarget.Entity == null || base.SelectedAiTarget.Entity.Removed))
			{
				this.State = AIState.Idle;
				return false;
			}
			AITargetMemory targetMemory = this.CurrentTargetMemory;
			if (targetMemory != null)
			{
				AITarget selectedAiTarget = base.SelectedAiTarget;
				if (((selectedAiTarget != null) ? selectedAiTarget.Entity : null) is Character)
				{
					targetMemory.Priority += CS$<>8__locals1.deltaTime * this.PriorityFearIncrement;
				}
			}
			bool isSteeringThroughGap = base.UpdateEscape(CS$<>8__locals1.deltaTime, this.canAttackDoors);
			if (!isSteeringThroughGap)
			{
				AITarget selectedAiTarget2 = base.SelectedAiTarget;
				Character targetCharacter = ((selectedAiTarget2 != null) ? selectedAiTarget2.Entity : null) as Character;
				if (targetCharacter != null && targetCharacter.CurrentHull == this.Character.CurrentHull)
				{
					this.<Escape>g__SteerAwayFromTheEnemy|239_0(ref CS$<>8__locals1);
				}
				else if (this.canAttackDoors && base.HasValidPath(true, true, null))
				{
					WayPoint currentNode = this.PathSteering.CurrentPath.CurrentNode;
					Door door2;
					if ((door2 = ((currentNode != null) ? currentNode.ConnectedDoor : null)) == null)
					{
						WayPoint nextNode = this.PathSteering.CurrentPath.NextNode;
						door2 = ((nextNode != null) ? nextNode.ConnectedDoor : null);
					}
					Door door = door2;
					if (door != null && !door.CanBeTraversed && !door.HasAccess(this.Character))
					{
						AITarget doorAiTarget = door.Item.AiTarget;
						if (doorAiTarget != null && (base.SelectedAiTarget != doorAiTarget || this.State != AIState.Attack))
						{
							this.SelectTarget(doorAiTarget, this.CurrentTargetMemory.Priority);
							this.State = AIState.Attack;
							this.AttackLimb = null;
							return false;
						}
					}
				}
			}
			if (base.EscapeTarget == null)
			{
				AITarget selectedAiTarget3 = base.SelectedAiTarget;
				if (((selectedAiTarget3 != null) ? selectedAiTarget3.Entity : null) is Character)
				{
					this.<Escape>g__SteerAwayFromTheEnemy|239_0(ref CS$<>8__locals1);
				}
				else
				{
					base.SteeringManager.SteeringWander(1f, this.Character.CurrentHull == null);
					if (this.Character.CurrentHull == null)
					{
						base.SteeringManager.SteeringAvoid(CS$<>8__locals1.deltaTime, this.avoidLookAheadDistance, 5f);
					}
				}
			}
			return isSteeringThroughGap;
		}

		// Token: 0x06000CFB RID: 3323 RVA: 0x0007EEB0 File Offset: 0x0007D0B0
		public Limb GetTargetLimb(Limb attackLimb, Character target, LimbType targetLimbType = LimbType.None)
		{
			this.targetLimbs.Clear();
			foreach (Limb limb in target.AnimController.Limbs)
			{
				if (limb.type == targetLimbType || targetLimbType == LimbType.None)
				{
					this.targetLimbs.Add(limb);
				}
			}
			if (this.targetLimbs.None(null))
			{
				this.targetLimbs.AddRange(target.AnimController.Limbs);
			}
			float closestDist = float.MaxValue;
			Limb targetLimb = null;
			foreach (Limb limb2 in this.targetLimbs)
			{
				if (!limb2.IsSevered && !limb2.Hidden)
				{
					float dist = Vector2.DistanceSquared(limb2.WorldPosition, attackLimb.WorldPosition) / Math.Max(limb2.AttackPriority, 0.1f);
					if (dist < closestDist)
					{
						closestDist = dist;
						targetLimb = limb2;
					}
				}
			}
			return targetLimb;
		}

		// Token: 0x06000CFC RID: 3324 RVA: 0x0007EFB4 File Offset: 0x0007D1B4
		private static Character GetOwner(Item item)
		{
			Pickable pickable = item.GetComponent<Pickable>();
			if (pickable != null)
			{
				Character character;
				if ((character = pickable.Picker) == null)
				{
					Inventory inventory = item.FindParentInventory((Inventory i) => i.Owner is Character);
					character = (((inventory != null) ? inventory.Owner : null) as Character);
				}
				Character owner = character;
				if (owner != null)
				{
					AITarget target = owner.AiTarget;
					if (((target != null) ? target.Entity : null) != null && !target.Entity.Removed)
					{
						return owner;
					}
				}
			}
			return null;
		}

		// Token: 0x06000CFD RID: 3325 RVA: 0x0007F034 File Offset: 0x0007D234
		[CompilerGenerated]
		private void <SetUnattackableSubmarines>g__AddSubs|113_0(Submarine sub, ref EnemyAIController.<>c__DisplayClass113_0 A_2)
		{
			this.unattackableSubmarines.Add(sub);
			if (A_2.includeConnectedSubs)
			{
				foreach (Submarine connectedSub in sub.DockedTo)
				{
					this.unattackableSubmarines.Add(connectedSub);
				}
			}
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x0007F09C File Offset: 0x0007D29C
		[CompilerGenerated]
		private void <.ctor>g__LoadSubElement|128_0(XElement subElement)
		{
			string a = subElement.Name.ToString().ToLowerInvariant();
			if (a == "latchonto")
			{
				this.LatchOntoAI = new LatchOntoAI(subElement, this);
				return;
			}
			if (a == "swarm" || a == "swarmbehavior")
			{
				this.SwarmBehavior = new SwarmBehavior(subElement, this);
				return;
			}
			if (!(a == "petbehavior"))
			{
				return;
			}
			this.PetBehavior = new PetBehavior(subElement, this);
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x0007F118 File Offset: 0x0007D318
		[CompilerGenerated]
		private bool <Update>g__IsCloseEnoughToTargetSub|141_0(float threshold)
		{
			AITarget selectedAiTarget = base.SelectedAiTarget;
			Submarine submarine;
			if (selectedAiTarget == null)
			{
				submarine = null;
			}
			else
			{
				Entity entity = selectedAiTarget.Entity;
				submarine = ((entity != null) ? entity.Submarine : null);
			}
			Submarine sub = submarine;
			return sub != null && sub != null && Vector2.DistanceSquared(this.Character.WorldPosition, sub.WorldPosition) < MathUtils.Pow((float)(Math.Max(sub.Borders.Size.X, sub.Borders.Size.Y) / 2) + threshold, 2f);
		}

		// Token: 0x06000D02 RID: 3330 RVA: 0x0007F1E0 File Offset: 0x0007D3E0
		[CompilerGenerated]
		private bool <UpdatePatrol>g__PatrolNodeFilter|156_0(PathNode n)
		{
			return (this.AIParams.PatrolFlooded && (this.Character.CurrentHull == null || n.Waypoint.CurrentHull == null || n.Waypoint.CurrentHull.WaterPercentage >= 80f)) || (this.AIParams.PatrolDry && this.Character.CurrentHull != null && n.Waypoint.CurrentHull != null && n.Waypoint.CurrentHull.WaterPercentage <= 50f);
		}

		// Token: 0x06000D03 RID: 3331 RVA: 0x0007F270 File Offset: 0x0007D470
		[CompilerGenerated]
		internal static bool <IsSameTarget>g__IsItemInCharacterInventory|162_0(AITarget potentialItem, AITarget potentialCharacter)
		{
			Item item = ((potentialItem != null) ? potentialItem.Entity : null) as Item;
			if (item != null)
			{
				Character character = ((potentialCharacter != null) ? potentialCharacter.Entity : null) as Character;
				if (character != null)
				{
					Inventory parentInventory = item.ParentInventory;
					return ((parentInventory != null) ? parentInventory.Owner : null) == character;
				}
			}
			return false;
		}

		// Token: 0x06000D04 RID: 3332 RVA: 0x0007F2C0 File Offset: 0x0007D4C0
		[CompilerGenerated]
		private float <GetAttackLimb>g__CalculatePriority|168_0(Limb limb, Vector2 attackPos)
		{
			float prio = 1f + limb.attack.Priority;
			if (this.Character.AnimController.SimplePhysicsEnabled)
			{
				return prio;
			}
			float distance = Vector2.Distance(limb.WorldPosition, attackPos);
			float maxDistance = Math.Max(limb.attack.Range * 3f, 1000f);
			if (distance > maxDistance)
			{
				return 0f;
			}
			float distanceFactor;
			if (limb.attack.Ranged)
			{
				float min = 100f;
				if (distance < min)
				{
					float t = MathUtils.InverseLerp(0f, min, distance);
					distanceFactor = MathHelper.Lerp(0.01f, 1f, t * t);
				}
				else
				{
					distanceFactor = MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(min, maxDistance, distance));
				}
			}
			else
			{
				if (distance <= limb.attack.Range)
				{
					if (!this.Character.InWater)
					{
						float verticalDistance = Math.Abs(limb.WorldPosition.Y - attackPos.Y);
						if (verticalDistance > limb.attack.DamageRange)
						{
							return 0f;
						}
					}
					return prio * 10f;
				}
				float min2 = limb.attack.Range;
				distanceFactor = MathHelper.Lerp(1f, 0f, MathUtils.InverseLerp(min2, maxDistance, distance));
			}
			return prio * distanceFactor;
		}

		// Token: 0x06000D05 RID: 3333 RVA: 0x0007F400 File Offset: 0x0007D600
		[CompilerGenerated]
		internal static InvSlotType <GetEquippedItem>g__GetInvSlotForLimb|170_0(ref EnemyAIController.<>c__DisplayClass170_0 A_0)
		{
			LimbType type = A_0.limb.type;
			InvSlotType result;
			if (type != LimbType.LeftHand)
			{
				if (type != LimbType.RightHand)
				{
					if (type != LimbType.Head)
					{
						result = InvSlotType.None;
					}
					else
					{
						result = InvSlotType.Head;
					}
				}
				else
				{
					result = InvSlotType.RightHand;
				}
			}
			else
			{
				result = InvSlotType.LeftHand;
			}
			return result;
		}

		// Token: 0x06000D07 RID: 3335 RVA: 0x0007F454 File Offset: 0x0007D654
		[CompilerGenerated]
		private void <UpdateWallTarget>g__DoRayCast|199_0(Vector2 rayStart, Vector2 rayEnd, ref EnemyAIController.<>c__DisplayClass199_0 A_3)
		{
			Body hitTarget = Submarine.CheckVisibility(rayStart, rayEnd, false, true, this.CanEnterSubmarine > CanEnterSubmarine.False, this.CanEnterSubmarine > CanEnterSubmarine.False, true, null);
			if (hitTarget != null && this.<UpdateWallTarget>g__IsValid|199_2(hitTarget, out A_3.wall, ref A_3))
			{
				int sectionIndex = A_3.wall.FindSectionIndex(ConvertUnits.ToDisplayUnits(Submarine.LastPickedPosition), false, false);
				if (sectionIndex >= 0)
				{
					this.wallHits.Add(new ValueTuple<Body, int, Vector2>(hitTarget, sectionIndex, this.<UpdateWallTarget>g__GetSectionPosition|199_1(A_3.wall, sectionIndex, ref A_3)));
				}
			}
		}

		// Token: 0x06000D08 RID: 3336 RVA: 0x0007F4D0 File Offset: 0x0007D6D0
		[CompilerGenerated]
		private Vector2 <UpdateWallTarget>g__GetSectionPosition|199_1(Structure wall, int sectionIndex, ref EnemyAIController.<>c__DisplayClass199_0 A_3)
		{
			float sectionDamage = wall.SectionDamage(sectionIndex);
			for (int i = sectionIndex - 2; i <= sectionIndex + 2; i++)
			{
				if (wall.SectionBodyDisabled(i))
				{
					if (this.Character.AnimController.CanEnterSubmarine != CanEnterSubmarine.False && base.CanPassThroughHole(wall, i, A_3.requiredHoleCount))
					{
						sectionIndex = i;
						break;
					}
				}
				else if (wall.SectionDamage(i) > sectionDamage)
				{
					sectionIndex = i;
				}
			}
			return wall.SectionPosition(sectionIndex, false);
		}

		// Token: 0x06000D09 RID: 3337 RVA: 0x0007F53C File Offset: 0x0007D73C
		[CompilerGenerated]
		private bool <UpdateWallTarget>g__IsValid|199_2(Body hit, out Structure wall, ref EnemyAIController.<>c__DisplayClass199_0 A_3)
		{
			wall = null;
			if (Submarine.LastPickedFraction == 1f)
			{
				return false;
			}
			Structure w = hit.UserData as Structure;
			if (w == null)
			{
				return false;
			}
			if (w.Submarine == null)
			{
				return false;
			}
			if (w.Submarine != base.SelectedAiTarget.Entity.Submarine)
			{
				return false;
			}
			if (this.Character.Submarine == null)
			{
				if (w.Prefab.Tags.Contains("inner"))
				{
					if (this.Character.AnimController.CanEnterSubmarine == CanEnterSubmarine.False)
					{
						return false;
					}
				}
				else if (!this.AIParams.TargetOuterWalls)
				{
					return false;
				}
			}
			wall = w;
			return true;
		}

		// Token: 0x06000D0E RID: 3342 RVA: 0x0007F626 File Offset: 0x0007D826
		[CompilerGenerated]
		internal static bool <CanPerceive>g__IsInRange|230_0(float dist, float range, float perception)
		{
			return dist <= range * perception;
		}

		// Token: 0x06000D0F RID: 3343 RVA: 0x0007F631 File Offset: 0x0007D831
		[CompilerGenerated]
		internal static bool <CanPerceive>g__IsInRangeSqr|230_1(float distSquared, float range, float perception)
		{
			return distSquared <= MathUtils.Pow2(range * perception);
		}

		// Token: 0x06000D10 RID: 3344 RVA: 0x0007F644 File Offset: 0x0007D844
		[CompilerGenerated]
		private void <Escape>g__SteerAwayFromTheEnemy|239_0(ref EnemyAIController.<>c__DisplayClass239_0 A_1)
		{
			if (base.SelectedAiTarget == null)
			{
				return;
			}
			Vector2 escapeDir = Vector2.Normalize(base.WorldPosition - base.SelectedAiTarget.WorldPosition);
			if (!MathUtils.IsValid(escapeDir))
			{
				escapeDir = Vector2.UnitY;
			}
			if (this.Character.CurrentHull != null && !this.Character.AnimController.InWater)
			{
				escapeDir = new Vector2((float)Math.Sign(escapeDir.X), 0f);
			}
			base.SteeringManager.Reset();
			base.SteeringManager.SteeringManual(A_1.deltaTime, escapeDir);
		}

		// Token: 0x04000594 RID: 1428
		public static bool DisableEnemyAI;

		// Token: 0x04000595 RID: 1429
		public static EnemyTargetingRestrictions TargetingRestrictions;

		// Token: 0x04000596 RID: 1430
		private EnemyTargetingRestrictions previousTargetingRestrictions;

		// Token: 0x04000597 RID: 1431
		private AIState _state;

		// Token: 0x04000599 RID: 1433
		public bool TargetOutposts;

		// Token: 0x0400059A RID: 1434
		private readonly float updateTargetsInterval = 1f;

		// Token: 0x0400059B RID: 1435
		private readonly float updateMemoriesInverval = 1f;

		// Token: 0x0400059C RID: 1436
		private readonly float attackLimbSelectionInterval = 3f;

		// Token: 0x0400059D RID: 1437
		private const float minPriority = 10f;

		// Token: 0x0400059E RID: 1438
		private SteeringManager outsideSteering;

		// Token: 0x0400059F RID: 1439
		private SteeringManager insideSteering;

		// Token: 0x040005A0 RID: 1440
		private float updateTargetsTimer;

		// Token: 0x040005A1 RID: 1441
		private float updateMemoriesTimer;

		// Token: 0x040005A2 RID: 1442
		private float attackLimbSelectionTimer;

		// Token: 0x040005A3 RID: 1443
		private Limb _attackLimb;

		// Token: 0x040005A4 RID: 1444
		private Limb _previousAttackLimb;

		// Token: 0x040005A5 RID: 1445
		private double lastAttackUpdateTime;

		// Token: 0x040005A6 RID: 1446
		private Attack _activeAttack;

		// Token: 0x040005A7 RID: 1447
		private AITargetMemory currentTargetMemory;

		// Token: 0x040005A8 RID: 1448
		private float targetValue;

		// Token: 0x040005A9 RID: 1449
		private CharacterParams.TargetParams currentTargetingParams;

		// Token: 0x040005AA RID: 1450
		private Dictionary<AITarget, AITargetMemory> targetMemories;

		// Token: 0x040005AB RID: 1451
		private readonly int requiredHoleCount;

		// Token: 0x040005AC RID: 1452
		private bool canAttackWalls;

		// Token: 0x040005AD RID: 1453
		private bool canAttackDoors;

		// Token: 0x040005AE RID: 1454
		private bool canAttackItems;

		// Token: 0x040005AF RID: 1455
		private bool canAttackCharacters;

		// Token: 0x040005B0 RID: 1456
		private readonly float priorityFearIncreasement = 2f;

		// Token: 0x040005B1 RID: 1457
		private readonly float memoryFadeTime = 0.5f;

		// Token: 0x040005B2 RID: 1458
		private float avoidTimer;

		// Token: 0x040005B3 RID: 1459
		private float observeTimer;

		// Token: 0x040005B4 RID: 1460
		private float sweepTimer;

		// Token: 0x040005B5 RID: 1461
		private float circleRotation;

		// Token: 0x040005B6 RID: 1462
		private float circleDir;

		// Token: 0x040005B7 RID: 1463
		private bool inverseDir;

		// Token: 0x040005B8 RID: 1464
		private bool breakCircling;

		// Token: 0x040005B9 RID: 1465
		private float circleRotationSpeed;

		// Token: 0x040005BA RID: 1466
		private Vector2 circleOffset;

		// Token: 0x040005BB RID: 1467
		private float circleFallbackDistance;

		// Token: 0x040005BC RID: 1468
		private float strikeTimer;

		// Token: 0x040005BD RID: 1469
		private float aggressionIntensity;

		// Token: 0x040005BE RID: 1470
		private CirclePhase CirclePhase;

		// Token: 0x040005BF RID: 1471
		private float currentAttackIntensity;

		// Token: 0x040005C0 RID: 1472
		private float playDeadTimer;

		// Token: 0x040005C1 RID: 1473
		private const float PlayDeadCoolDown = 60f;

		// Token: 0x040005C2 RID: 1474
		private CoroutineHandle disableTailCoroutine;

		// Token: 0x040005C3 RID: 1475
		private readonly List<Body> myBodies;

		// Token: 0x040005C4 RID: 1476
		private const double MinSetAttackTargetEventInterval = 0.5;

		// Token: 0x040005C5 RID: 1477
		private IDamageable lastDamageTarget;

		// Token: 0x040005C6 RID: 1478
		private Limb lastTargetLimb;

		// Token: 0x040005C7 RID: 1479
		private Limb lastAttackLimb;

		// Token: 0x040005C8 RID: 1480
		private double lastSetAttackTargetEventTime;

		// Token: 0x040005CC RID: 1484
		private readonly HashSet<Submarine> unattackableSubmarines = new HashSet<Submarine>();

		// Token: 0x040005CD RID: 1485
		private bool reverse;

		// Token: 0x040005CE RID: 1486
		private readonly float maxSteeringBuffer = 5000f;

		// Token: 0x040005CF RID: 1487
		private readonly float minSteeringBuffer = 500f;

		// Token: 0x040005D0 RID: 1488
		private readonly float steeringBufferIncreaseSpeed = 100f;

		// Token: 0x040005D1 RID: 1489
		private float steeringBuffer;

		// Token: 0x040005D2 RID: 1490
		private CharacterParams.AIParams _aiParams;

		// Token: 0x040005D3 RID: 1491
		private readonly List<Identifier> _targetingTags = new List<Identifier>();

		// Token: 0x040005D4 RID: 1492
		private float movementMargin;

		// Token: 0x040005D5 RID: 1493
		private const float MaxDroppingInterval = 5f;

		// Token: 0x040005D6 RID: 1494
		private double lastDroppingTime;

		// Token: 0x040005D7 RID: 1495
		private const float MaxDroppingTime = 1f;

		// Token: 0x040005D8 RID: 1496
		private float droppingTimer;

		// Token: 0x040005D9 RID: 1497
		private readonly List<Hull> targetHulls = new List<Hull>();

		// Token: 0x040005DA RID: 1498
		private readonly List<float> hullWeights = new List<float>();

		// Token: 0x040005DB RID: 1499
		private Hull patrolTarget;

		// Token: 0x040005DC RID: 1500
		private float newPatrolTargetTimer;

		// Token: 0x040005DD RID: 1501
		private float patrolTimerMargin;

		// Token: 0x040005DE RID: 1502
		private readonly float newPatrolTargetIntervalMin = 5f;

		// Token: 0x040005DF RID: 1503
		private readonly float newPatrolTargetIntervalMax = 30f;

		// Token: 0x040005E0 RID: 1504
		private bool searchingNewHull;

		// Token: 0x040005E1 RID: 1505
		private Vector2 attackWorldPos;

		// Token: 0x040005E2 RID: 1506
		private Vector2 attackSimPos;

		// Token: 0x040005E3 RID: 1507
		private float reachTimer;

		// Token: 0x040005E4 RID: 1508
		private const float reachTimeOut = 10f;

		// Token: 0x040005E5 RID: 1509
		private readonly List<Limb> attackLimbs = new List<Limb>();

		// Token: 0x040005E6 RID: 1510
		private readonly List<float> weights = new List<float>();

		// Token: 0x040005E7 RID: 1511
		private const float VisibilityCheckStep = 0.2f;

		// Token: 0x040005E8 RID: 1512
		private double lastVisibilityCheckTime;

		// Token: 0x040005E9 RID: 1513
		private bool canSeeTarget;

		// Token: 0x040005EA RID: 1514
		private float aimTimer;

		// Token: 0x040005EB RID: 1515
		private float sinTime;

		// Token: 0x040005EC RID: 1516
		private readonly float blockCheckInterval = 0.1f;

		// Token: 0x040005ED RID: 1517
		private float blockCheckTimer;

		// Token: 0x040005EE RID: 1518
		private bool isBlocked;

		// Token: 0x040005EF RID: 1519
		private Vector2? attackVector;

		// Token: 0x040005F0 RID: 1520
		private EnemyAIController.WallTarget wallTarget;

		// Token: 0x040005F1 RID: 1521
		private readonly List<ValueTuple<Body, int, Vector2>> wallHits = new List<ValueTuple<Body, int, Vector2>>(3);

		// Token: 0x040005F2 RID: 1522
		private readonly List<AITarget> removals = new List<AITarget>();

		// Token: 0x040005F3 RID: 1523
		private readonly float targetIgnoreTime = 10f;

		// Token: 0x040005F4 RID: 1524
		private float targetIgnoreTimer;

		// Token: 0x040005F5 RID: 1525
		private readonly HashSet<AITarget> ignoredTargets = new HashSet<AITarget>();

		// Token: 0x040005F6 RID: 1526
		private readonly float stateResetCooldown = 10f;

		// Token: 0x040005F7 RID: 1527
		private float stateResetTimer;

		// Token: 0x040005F8 RID: 1528
		private bool isStateChanged;

		// Token: 0x040005F9 RID: 1529
		private readonly Dictionary<StatusEffect.AITrigger, CharacterParams.TargetParams> activeTriggers = new Dictionary<StatusEffect.AITrigger, CharacterParams.TargetParams>();

		// Token: 0x040005FA RID: 1530
		private readonly HashSet<StatusEffect.AITrigger> inactiveTriggers = new HashSet<StatusEffect.AITrigger>();

		// Token: 0x040005FB RID: 1531
		private readonly Dictionary<Identifier, IEnumerable<CharacterParams.TargetParams>> modifiedParams = new Dictionary<Identifier, IEnumerable<CharacterParams.TargetParams>>();

		// Token: 0x040005FC RID: 1532
		private readonly Dictionary<Identifier, CharacterParams.TargetParams> tempParams = new Dictionary<Identifier, CharacterParams.TargetParams>();

		// Token: 0x040005FD RID: 1533
		private readonly List<CharacterParams.TargetParams> tempParamsList = new List<CharacterParams.TargetParams>();

		// Token: 0x040005FE RID: 1534
		private Vector2 returnDir;

		// Token: 0x040005FF RID: 1535
		private float returnTimer;

		// Token: 0x04000600 RID: 1536
		private readonly List<Limb> targetLimbs = new List<Limb>();

		// Token: 0x0200076E RID: 1902
		private class WallTarget
		{
			// Token: 0x0600521A RID: 21018 RVA: 0x001EA585 File Offset: 0x001E8785
			public WallTarget(Vector2 position, Structure structure = null, int sectionIndex = -1)
			{
				this.Position = position;
				this.Structure = structure;
				this.SectionIndex = sectionIndex;
			}

			// Token: 0x04002CF9 RID: 11513
			public Vector2 Position;

			// Token: 0x04002CFA RID: 11514
			public Structure Structure;

			// Token: 0x04002CFB RID: 11515
			public int SectionIndex;
		}
	}
}

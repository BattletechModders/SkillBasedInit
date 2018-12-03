# Skill Based Initiative
This mod (for the HBS Battletech game) makes the initiative order more random. Instead of 5 phases to each round, there are 30.  At the start of each round, each unit's initiative in the phase is determined randomly. The basic roll will be between 6 to 12, though these bounds are influenced by several factors. Lighter mechs have a better bounds, while Assault mechs will have these bounds reduced. The Tactics and Piloting skill of the MechWarrior influence this range. Injuries, knockdowns, and various other battlefield conditions also play a part in determining the unit's place in the initiative order.

The practical effect of these changes is that novice Mechwarriors (low tactics or piloting skill) will typically act later in the round, even if they pilot light mechs. Highly advanced pilots have a better chance to act early in the round, even if they are in heavier Mechs. However the fickle handle of RNGJesus plays a critical place, and you may find your well-placed battle plan undone by a set of poor initiative rolls!

This mod divides mechs into 10 ton categories instead of the light/medium/heavy/assault ssytem. 20-25 ton mechs are slightly faster than 30-35 ton mechs. 80-85 tons mechs are faster than 90-95, which are faster than 100. Though randomness can hide this effect, the effect is there, offering a small bonus to pilots of lighter units.

Battlefield conditions that impact your place in the initiative order include:

* Pilots with injuries suffer an initiative penalty between -4 to -7. High Guts ratings reduce this penalty to a minimum of -1 to -4. Pilots with additional health pips (from high guts ratings) will ignore one injury per addtiional health pip before feeling these effects.
* Mechs that are knocked down suffer an initiative penalty equal to their Guts injury bounds in the turn they are knocked down. On subsequent turns they suffer a -6 penalty, reduced by 5% for each point of piloting skill (rounded down). A mechwarrior with piloting skill 6 reduces this penalty by 30%, to -6 * 0.70 = -4. 
* Mechs with a missing leg or vehicles with a missing side count as crippled and suffer an initiative penalty of -13. This penalty is reduced 5% for each point of piloting (rounded down). A mechwarrior with skill 5 reduces this penalty by 25% to -13 * 0.75 = 9.
* Mechwarriors with the High Spirits tag gain a +2 initiative bonus. Mechwarriors with the Low Spirits tag suffer a -2 initiative penalty.
* Mechwarriors that are Inspired by Morale or Fury gain a bonus between +1 to +3.
* Any chassis or component that modifies the **BaseInititiative** of the unit should be honored. This value is normalized against the expected values for the weight classes (light=2, medium=3, heavy=4, assault=5).
* Units that are attacked in melee suffer an initiative penalty. For every 5 tons of difference between the attacker and target, the target suffers a -1 init malus if the attacker is heavier. If the target is heavier, they will only suffer a -1 penalty. Units with the Juggernaught skill (AbilityDefGu5) will multiply this malus by a small amount. This penalty is cumulative so a target that's repeatedly attacked by multiple heavy units can be reduced to the lowest initiative level.

## Planned

Works in progress or planned effects include:

* Planned: Units that are unsettled, panicked or similar will have a reduced initiative.
* Planned: Units will suffer init penalities as soon as the actor takes an injury. This will be denoted by an 'Ouch!' bubble. (See pilot.SaveInjuryInfo).

### Bugs and Incomplete functions

These items are known bugs or issues that should be resolved before declaring a 1.0 version.

* Test interactions with init modifying abilities; Juggernaught, Offensive Push, etc
* Test interactions with init modifying components; primitive cockpits should offer no bonus, basic IS +1, Clan +2, DNI +3, EI +4
* Determine if there are other stats that should be evaluated. In particular "PhaseModifier" : "PhaseModifierSelf" may be appropriate to check on each round.
* Extract logging from HBS.Logging to prevent duplication of logs
* Show init bonus/malus on Lance/MechBay screens. Currently replaced with a - character to signify it's meaningless. 
* Check out Cyclops init aura to see how that bonus interacts in this model.
* Init can wrap below 0 when it's dynamically applied. This can prevent a unit from activating at all.
* Melee is being modifed by piloting below 0.
* Percentages have too many significant digits (looks ugly in logs)

## Detailed Information

The sections below detail some of the calculations used by the mod.

### Tonnage Impact
The tonnage of a unit determines a multipler applied to the lower (6) and upper (12) bounds of the phase calculation. After multiplication, the lower bound is rounded down, while the upper bound is rounded up.

Tonnage | Modifier | Initiative Range
--------|----------|---------------
00-05   | x1.6 |  9 to 20
10-15   | x1.5 | 9 to 20
20-25   | x1.4 | 8 to 17
30-35   | x1.3 | 7 to 16
40-45   | x1.2 | 7 to 14
50-55   | x1.1 | 6 to 14
60-65   | x1.0 | 6 to 12
70-75   | x0.9 | 5 to 11
80-85   | x0.8 | 4 to 10
90-95   | x0.7 | 4 to 9
100     | x0.6 | 3 to 8
100+    | x0.2 | 1 to 3

### Impact of Tactics Skill
A MechWarriors's Tactics rating makes a significant difference on when it acts in the initiative order. The tactics and piloting skills are combined and converted into an additional .1 to 1.0 added to the tonnage modifier above. The tactics skill has twice the impact of the piloting skill on this calculation, which follows the formula:

`Math.Floor((pilot.Tactics * 2.0 + pilot.Piloting) / 3.0) / 10.0;`

Some examples illustrate the interplay between Tactics and Piloting:

* A mechwarrior with Tactics 1 and Piloting 1 increase the tonnage modifier by `((2 + 1) / 3) / 10.0 = 0.1`. In a 50 ton mech, their phase bounds would be `x1.1 + 0.1 = x1.2`.
* A mechwarrior with Tactics 6 and Piloting 3 increase the tonnage modifier by `((12 + 3) / 3) / 10.0 = 0.5`. In a 50 ton mech, their phase bounds would be `x1.1 + 0.5 = x1.6`.
* A mechwarrior with Tactics 10 and Piloting 1 increase the tonnage modifier by `((20 + 2) / 3) / 10.0 = 0.7`. In a 50 ton mech, their phase bounds would be `x1.1 + 1.0 = x2.1`.
* A mechwarrior with Tactics 1 and Piloting 10 increase the tonnage modifier by `((2 + 10) / 3) / 10.0 = 0.4`. In a 50 ton mech, their phase bounds would be `x1.1 + 0.4 = x1.5`.


### Impact of Piloting

Many battlefield conditions reduce your initiative. Your piloting skill reduces the impact of these effects by 5% for each point of the skill. A mechwarrior with piloting 5 would reduce any effect by 25%, to a minimum of -1. This is especially important when knocked down or meleed, both of which impose significant penalties that can completely shatter your plan of action.

### Impact of Guts

When a battlefield condition would stun or injure a mechwarrior, the size of the penalty is determined by the Mechwarrior's Guts skill rating. The following lits defines the penalty range in terms of the skill level:

Guts Rating | Initiative Range
------------|---------------
1, 2, 3, 4 |  -4 to -7.
5, 6, 7, 8 |  -3 to -6
9 |  -2 to -5
10 |  -1 to -4

Mechwarriors lose initiative anytime they are injured. Mechwarriors with additional health pips due to high guts ratings ignore one injury per each additional health pip they have. 

### Miscellaneous

Turrets suffer a -4 penalty, while tanks suffer a -2. These will likely be removed in the future once it's been confirmed that init bonuses on chassis and components will flow through the system.


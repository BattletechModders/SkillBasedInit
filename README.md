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

Work in progress or planned effects include:

* WIP: Units that suffer a melee attack suffer an initaitive penalty. Defenders that are heavier than attackers only suffer -1 penalty (before DFA and Juggernaught mods). Defenders that are ligher than the attacker suffer a -1 penalty for each 5 tons they are lighter than the attacker. The defender reduces this effect by 5% for each point of piloting. DFA attacks increase this bonus by 1.5 (rounded down). Juggernaught attacks multiply the final value by 2. A 100 ton Mech performing DFA on a 20 ton Mech would impose a 20 - 4 = -16 penalty, times 1.5 for DFA = -24. If the attacking pilot has Juggernaught, this comes -48. 
* Planned: Units that are unsettled, panicked or similar will have a reduced initiative.


## Detailed Information

The sections below detail some of the calculations used by the mod.

### Tonnage Impact
The tonnage of a unit determines a multipler applied to the lower (6) and upper (12) bounds of the phase calculation. After multiplication, the lower bound is rounded down, while the upper bound is rounded up.

* 00-05 tons: x1.6 for bounds of [9, 20]
* 10-15 tons: x1.5 for bounds of [9, 20]
* 20-25 tons: x1.4 for bounds of [8, 17]
* 30-35 tons: x1.3 for bounds of [7, 16]
* 40-45 tons: x1.2 for bounds of [7, 14]
* 50-55 tons: x1.1 for bounds of [6, 14]
* 60-65 tons: x1.0 for bounds of [6, 12]
* 70-75 tons: x0.9 for bounds of [5, 11]
* 80-85 tons: x0.8 for bounds of [4, 10]
* 90-95 tons: x0.7 for bounds of [4, 9]
*   100 tons: x0.6 for bounds of [3, 8]
*  100+ tons: x0.2 for bounds of [1, 3]

### Impact of Tactics Skill
The MechWarrior's piloting and tactics skill increases these bounds. The tactics and piloting skills are combined and converted into an additional .1 to 1.0 added to the tonnage modifier above. The tactics skill has twice the impact of the piloting skill on this calculation, which follows the formula:

Math.Floor((pilot.Tactics * 2.0 + pilot.Piloting) / 3.0) / 10.0;

Some example:
    * A mechwarrior with Tactics 1 and Piloting 1 increase the tonnage modifier by ((2 + 1) / 3) / 10.0 = 0.1. In a 50 ton mech, their phase bounds wound be x1.1 + 0.1 = x1.2.
    * A mechwarrior with Tactics 6 and Piloting 3 increase the tonnage modifier by((12 + 3) / 3) / 10.0 = 0.5. In a 50 ton mech, their phase bounds wound be x1.1 + 0.5 = x1.6.
    * A mechwarrior with Tactics 10 and Piloting 1 increase the tonnage modifier by ((20 + 2) / 3) / 10.0 = 0.7. In a 50 ton mech, their phase bounds wound be x1.1 + 1.0 = x2.1.
    * A mechwarrior with Tactics 1 and Piloting 10 increase the tonnage modifier by((2 + 10) / 3) / 10.0 = 0.4. In a 50 ton mech, their phase bounds wound be x1.1 + 0.4 = x1.5.

Tactics makes a significant difference on your initiative order. 

### Impact of Piloting

Many battlefield conditions reduce your initiative. Your piloting skill reduces the impact of these effects by 5% for each point of the skill. A mechwarrior with piloting 5 would reduce any effect by 25%, to a minimum of -1. This is especially important when knocked down or meleed, both of which impose significant penalties that can completely shatter your plan of action.

### Impact of Guts

When a battlefield condition would stun or injure a mechwarrior, the size of the penalty is determined by the Mechwarrior's Guts skill rating. The following lits defines the penalty range in terms of the skill level:

* Skill 1, 2, 3, 4 = range of -4 to -7.
* Skill 5, 6, 7, 8 = range of -3 to -6
* Skill 9 = range of -2 to -5
* Skill 10 = range of -1 to -4

Mechwarriors lose initiative anytime they are injured. Mechwarriors with additional health pips due to high guts ratings ignore one injury per each additional health pip they have. 

## TODO

These items are known bugs or issues that should be resolved before declaring a 1.0 version.

* Melee attacks reduce your initiative by the difference in tonnage; a 100t vs. 100t has limited impact, a 100t vs. 20t a bigger impact. Piloting skill mitigates this. Mechanics in place, but penalty not yet applied.Vehicles and turrets need tested; no way to apply DFA penalty against them (no DamageType in signatures)
* Test interactions with init modifying abilities; Juggernaught, Offensive Push, etc
* Test interactions with init modifying components; primitive cockpits should offer no bonus, basic IS +1, Clan +2, DNI +3, EI +4
* Extract logging from HBS.Logging to prevent duplication of logs
* Show init bonus/malus on Lance/MechBay screens. Currently replaced with a - character to signify it's meaningless. 

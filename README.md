# Skill Based Initiative
This is a mod for the [HBS BattleTech](http://battletechgame.com/) game that divides each round of play into 30 initiatives phases. Each round, every model's place in the initiative order is determined by the pilot's skills, chassis weight, injuries taken, morale state and many other factors. A small random factor is applied to these results. The end result is to make the turn sequence less deterministic and something you have to cope with instead of being able to fully plan out your strategy.

This mod uses assets from [https://game-icons.net/], which are licensed through a CC BY 3.0 license. I've modified these icons by making them transparent, or changing the color of the icon, but otherwise they are unmodified. 

## Summary

The skill level of each Pilot becomes very important in this mod. Novice MechWarriors with low tactics or piloting skill will typically act later in the round, even in light or medium mechs. Veteran MechWarriors can push the limits of their equipment and have heavy or assault mechs going at the same time as unskilled lights. High-tech equipment such *Clan cockpits* provide initiative bonuses that can boost the weak or help the elite become even more fearsome.

Instead of four weight classes (light / medium / heavy / assault), this mod divides units into 10-ton groups. 20-25 ton mechs are slightly faster than 30-35 ton mechs. 40-45 ton mechs are faster than 50-55 ton mechs, while 60-65 ton mechs are faster than 70-75 ton mechs. 80-85 tons mechs are faster than 90-95, which are faster than 100. This offers a small bonus to pilots of lighter units.


In addition, this mod emphasizes a MechWarrior's Tactics, Guts, and Piloting skills.

* *Tactics* - contributes directly to the initiative value. If you want your Mechwarriors to consistently achieve a high phase number, increase their Tactics skill.
* *Piloting* reduces the randomness of many calculations, providing a more consistent result round over round.
* *Gunnery* is used during called shots by the attacker; it increases the initiative penalty the target suffers
* *Guts* is used during called shots by the target; it reduces the initiative loss

Almost all values are available through tooltips in the mech bay, lance drop and combat UI screens. Hover over the initiative badges (the hexagons) and many details of the system will be defined.

### Dependencies

This mod requires the following mods to function properly:
  
  * [https://github.com/battletechmodders/IRBTModUtils/]
  * [https://github.com/BattletechModders/CustomComponents/]
  * [https://github.com/BattletechModders/CustomBundle/tree/master/CustomUnits]

You are strongly encouraged to use [Tis But a Scratch (TBAS) with this](https://github.com/ajkroeg/TisButAScratch) to inflict initiative based injury modifiers.


### Usage

This mod is only intended to be used with RogueTech. You may freely us it standalone, but this mod expects TurnDirector.IsInterleaved to always be true. This is achieved by the **AlwaysCombatTurns** mod, which this mod depends upon. It also requires the components from **CustomComponents** and **MechEngineer**, both of which are also dependencies.

If you are using this mod independently of RougeTech, you'll likely want to add initiative boosting equipment or skills of some kind.

### Disabling The Mod
To disable the mod, edit `SkillBasedInitiative/mod.json` and change `enabled:true` to `enabled:false`. This will prevent the mod from loading, restoring the original HBS experience. However, it will also **break your RogueTech experience** due to the various pieces of equipment that add initiative modifiers. The net effect is that most units will act in phase 1, rendering initiative largely useless. You will need to do significant overhauls on all the equipment in the mod to remove any **BaseInitiative** changes.

## Technical Details

At the start of each actor's turn (`AbstractActor.OnNewRound`) the mod calculates a new initiative value using the following calculation:

`round_init = tonnage_mod + unit_type_mod + sum(pilot_tag_mods) + injury_mod + misc_mod + called_shot_state + vigilance_state + knockdown_mod + crippled_mod + shutdown_mod + hesitation_mod + inspired_mod + randomness_mod`

Each of these modifiers are described in further detail below. Note that some values are referred to as *mod*ifiers and some are *state*. Modifiers are statistics that you can freely adjust with status effects, without interfering with the internals of SBI. State variables are also statistics, but will be consumed (reset to 0) when the actor's initiative is calculated at the start of the turn. Because of this property you should *NOT* apply status effects to them. When status effects are removed (due to damage or expiration) they reverse their changes. This can lead to very odd situations where SBI changed a value from 2 to 0, which then becomes -2 because the status effect was removed. You've been warned.

### List of Statistics 

| Statistic | Type | Source | Description | Default Value |
| -- | -- | -- | -- | -- | 
| SBI_MOD_INJURY | Integer | Actor | Direct modifier to initiative from injuries. Intended to be used for TBAS integration. | 0 |
| SBI_MOD_MISC | Integer | Actor | Direct modifier to initiative for miscellaneous effects. | 0 |
| SBI_MOD_CALLED_SHOT_ATTACKER | Actor | Integer | Modifies the calculation of the called shot penalty (see below). Read from the attacking actor. | 0 |
| SBI_MOD_CALLED_SHOT_TARGET | Actor | Integer | Modifies the *generated* called shot penalty (see below). Read from the defending actor. | 0 |
| SBI_MOD_VIGILANCE | Actor | Integer | Modifies the *generated* vigilance bonus that will be applied to SBI_STATE_VIGILANCE. | 0 |
| SBI_MOD_HESITATION | Actor | Integer | A modifier to the *generated* hesitation, that will be added to the SBI_STATE_HESITATION statistic. Does NOT | 0 |
| SBI_MOD_SKILL_GUNNERY | Actor | Integer | Indirect modifier that alters the calculated modifier from the gunnery skill (see below) | 0 |
| SBI_MOD_SKILL_GUTS | Actor | Integer | Indirect modifier that alters the calculated modifier from the guts skill (see below) | 0 |
| SBI_MOD_SKILL_PILOT | Actor | Integer | Indirect modifier that alters the calculated modifier from the guts skill (see below) | 0 |
| SBI_MOD_SKILL_TACTICS | Actor | Integer | Indirect modifier that alters the calculated modifier from the guts skill (see below) | 0 |
| SBI_STATE_TONNAGE | Actor | Integer | Maps the actor's tonnage to a starting init value. Mapping is defined in mod.json, as InitBaseByTonnage dictionary. Calculated at the start of combat and never changed. | 0 |
| SBI_STATE_UNIT_TYPE | Actor | Integer | Direct modifier to initiative based upon the actor's type - mech, trooper squad, naval, vehicle, etc. Calculated at the start of combat and never changed. | 0 |
| SBI_STATE_PILOT_TAGS | Actor | Integer | Direct modifiers to initiative based on a pilot tag. Mapping defined in mod.json in Pilot.PilotTagModifiers. Calculated at the start of combat and never changed. | 0 |
| SBI_STATE_CALLED_SHOT | Actor | Integer | The current total called shot penalty to apply on the following turn. | 0 |
| SBI_STATE_VIGILANCE | Actor | Integer | The current total vigilance bonus to apply on the following turn. | 0 |
| SBI_STATE_KNOCKDOWN | Actor | Integer | The current total knockdown penalty to apply on the following turn. | 0 |
| SBI_STATE_HESITATION | Actor | Integer | The current total hesitation penalty to apply on the following turn. | 0 |


### Skill Normalization
This mod uses a normalized skill value in many places. Because skill values can range from 1-20 in RougeTech, but initiative phases only go from 30-1, we normalize skill values to smaller modifier values. The table below shows the correspondence between skill value and modifier:

| Skill |  1  |  2  |  3  |  4  |  5  |  6  |  7  |  8  |  9  |  10  | 11 | 12 | 13 |
| -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | -- | --  |
| Modifier  | +0 | +1 | +1 | +2 | +2 | +3 | +3 | +4 | +4 | +5 | +6 | +7 | +8 | 

In addition, SBI offers three statistics that allow mod authors to modify these normalized modifiers. The statistics `SBI_MOD_GUTS`, `SBI_MOD_PILOTING`, and `SBI_MOD_TACTICS` are added to the calculated modifier above for their respective skill calculation.

(!) The skill statistics are read from the PILOT stat collection, NOT the actor stat collection. Make sure your effects target the proper collection!

> Example: A Pilot with tactics of 7 has a SBI_MOD_TACTICS value of 2. Their normalized skill modifier is +3, +2 for the SBI_MOD_TACTICS value, for a total of 5.

### Tonnage Modifier

The `tonnage_mod` is calculated by comparing the unit's tonnage against the relevant UnitConfig.InitBaseByTonnage dictionary. The keys of the dictionary are walked, and if the unit's tonnage is less than the current key the previous key's value is used as the `tonnage_mod`. The calculation of tonnage varies by unit type, as shown below:

* Mech.InitBaseByTonnage - The tonnage in the mechDef is used.
* Trooper.InitBaseByTonnage - As this is a CustomUnits type, tonnage is pulled from the mechDef. This tonnage is divided by the number of troopers configured for the squad.
* Naval.InitBaseByTonnage - As this is a CustomUnits type, tonnage is pulled from the mechDef.
* Vehicle.InitBaseByTonnage - If the unit is a true vehicle, tonnage is pulled from the vehicleDef. If it's a CustomUnits FakeVehicle, tonnage is pulled from the mechDef.
* Turret.InitBaseByTonnage - The unit's tags are scanned for `unit_light`, `unit_medium`, or `unit_heavy`. These are matched against `Turret.LightTonnage`, `Turret.MediumTonnage`, and `Turret.HeavyTonnage` respectively. If one of these tags are not found, Turret.DefaultTonnage is used instead

> Example: For a configuration of Unit.InitBaseByTonnage = { "35" : 16, "75" : 12, "100" : 9, "999" : 6 } units between 0-35 tons would have a base init of 16, units between 36-75 tons would have a base init of 12, and units between 76-100 would have a base init of 9. Units between 101-999 tons would use a base init of 6.

### Unit Type Modifier

The `unit_type_mod` is a direct modifier assigned from the `Unit.TypeMod` value for the relative unit type. The unit type is determined via the following algorithm:

* Turrets are defined as HBS Turret types
* Vehicles are either HBS Vehicle types, or CustomUnit FakeVehicles with isVehicle=true
* Naval units are CustomUnits with Naval=true
* Trooper Squads are CustomUnits with SquadInfo.Troopers > 1
* Everything else is a Mech

### Called Shot Modifier

When a unit is targeted by the Called Shot ability, it suffers an initiative penalty. This penalty is a random value between `Unit.CalledShotRandMin` and `Unit.CalledShotRandMax`. The upper bound is modified by the following:

* The average of the target's normalized skill modifiers for guts and tactics
* The value of `SBI_CALLED_SHOT_TARGET` modifier on the target.
* The average of the attacker's normalized skill modifiers for gunnery and tactics
* The value of the `SBI_CALLED_SHOT_ATTACKER` modifier on the attacker.

If the upper bound is greater than the value of `Unit.CalledShotRandMin`, then the upper bound is set to `Unit.CalledShotRandMin` + 1. Once the bounds are calculated, a random value between these two are selected and applied as the called shot modifier. 

### Vigilance Modifier

When the Vigilance ability is used on the actor, it gains a small initiative bonus as well. This bonus is a random value between `Unit.VigilanceRandMin` and `Unit.VigilanceRandMax`. The upper bound is modified by the average of the unit's normalized skill modifiers for guts and tactics. It is also modified by the value of the `SBI_MOD_VIGILANCE` statistic, if present. The bonus will be a random value between these two ranges.

> Example: A mech is configured with Mech.VigilanceRandMin = 2 and Mech.VigilanceRandMax = 6. The pilot has guts = 6, for a +2 modifier and tactics 10 for a +5 modifier. The average of these of +4. The unit also has SBI_MOD_VIGILANCE = -2, from a poor cockpit design. This gives a final range for the vigilance modifier between 2 and (6 + 4 - 2) = 8.

### Randomness Modifier

Each turn a unit has a random amount of initiative subtracted from its base score. The upper and lower bounds for this are configured by unit type, as `Unit.RandomnessMin` and `Unit.RandomnessMax` respectively. A random value within this range (inclusive) will be used for the value of `randomness_mod`. Before calculating the random value, the normalized modifier for the Piloting skill is added to the `Unit.RandomnessMax` value. This represents a more skilled pilot being able to capitalize on their machine's piloting.

> Example: Vehicles are configured with Vehicle.RandomnessMin = -2 and Vehicle.RandomnessMax = -10. A random value between -2 and -10 will be applied to each vehicle's base initiative. A 60 ton vehicle configured with an init base of 13 would have initiative values between 11 and 3.
> If a vehicle has a Pilot skill of 5 their normalized modifier for that skill would be +2. This makes their bounds -2 to (-10 + 2) = -8. 

### Prone Modifier

When a Mech is knocked prone, it suffers penalty to initiative configured as Mech.ProneModifierMin and Mech.ProneModifierMax. The penalty will be calculated as a random modifier between these two values. The max bounds will be modified by the unit's normalized skill modifier for piloting. This modifier is applied every turn the unit begins the turn prone.

> Example: A Mech is prone, with Mech.ProneModifierMin = -2 and Mech.ProneModifierMax = -9. The Pilot has a piloting skill of 9, which gives a +4 normalized skill modifier. Their prone modifier will be between -2 and (-9 + 4) = -5.


### Hesitation Modifier

Every time a mech chooses the Reserve option, it accrues a *Hesitation* penalty. A random value between `Unit.HesitationMax` and `Unit.HestiationMin` is calculated, with any value from `SBI_MOD_HESITATION` applied to the random value. This penalty is then applied to _next turns initiative_ value. This penalty is *cumulative*, so every time Reserve is chosen the randomly generated penalty is added to the current total.

At the start of the next turn, the pilots tactics modifier is subtracted from the hesitation penalty. The remainder is then applied to the initiative calculation and remains until the next round. This can result in an actor keeping a large hesitation penalty around for most of the combat, making it harder to react to the enemy opfor.

### Crippled Modifier

When a unit is crippled it suffers a penalty reflecting the unit not performing at it's combat best. This penalty is configured as Unit.CrippledModifierMin and Unit.CrippledModifierMax, and will be a random value selected between those two bounds. The maximum value is reduced by the normalized piloting skill modifier for the unit.

Trooper Squads cannot be crippled. Other unit types are considered crippled in the following circumstances:

* Mech - Any leg is destroyed (this applies to Quad mechs as well)
* Vehicles - Either the left or right side is destroyed. For CustomUnits vehicles, the left or right torsos count as sides.
* Naval - When the left or right torso (side) is destroyed.

> Example: A Vehicle has lost its left side completely (no structure left). Vehicles are configured with Vehicle.CrippledModifierMin = -5 and Vehicle.CrippledModifierMax = -13. The Pilot has a piloting skill of 3, which gives a +1 normalized skill modifier. The vehicle's crippled modifier will be between -2 and (-13 + 1) = -12.

### Inspired Mod

When a unit reaches the *Inspired* state (by the team gaining enough morale or rage) it gains a bonus to its initiative value. This bonus is configured through the Unit.InspiredMin and Unit.InspiredMax values. The maximum bound will be improved by the normalized skill modifier for the Pilot's tactics skill. The modifier will be a random value selected from between these two values.

> Example: A trooper squad is configured with Trooper.InspiredMin = 2 and Trooper.InspiredMax = 5. They have a Tactics skill of 5, for a +2 normalized skill modifier. Their inspired bonus will range between 2 and (5+2) = 7. 

### HBS Initiative Statistics

This mod completely ignores the HBS statistics related to initiative, and replaces them with its own. Any status effects or equipment that modify the `BaseInitiative`, `PhaseModifierSelf`, or `PhaseModifierSelf` will be completely ignored by this mod.


### Changing Colors
The mod applies colors to the following combat UI elements:

* Initiative Hexagon above the Mech Paper doll
* Initiative Hexagon floating above the Mech
* Pilot Name background bar

These colors can be customized through the `mod.json`.

## Planned

Works in progress or planned effects include:

- [] Modify Reserve button to change to 'Reserve to Phase 1' when ALT key is held down. Model pays for all the phases it holds though. (See BTDebug for how to lash to ALT key)


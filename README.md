# BT-ComplexInitiative
This mod makes the Battletech initiative order more random. Instead of 5 phases to each round, there are 30.  At the start of each round, a random number is rolled for each unit which becomes it's initiative for that round. The basic roll is 1-12, though these bounds are influenced by the tonnage of the Mech, Vehicle or Turret. Lighter mechs have a better bounds, while Assault mechs will have these bounds reduced. In addition the Tactics skill and the Piloting skill of the MechWarrior increase this range. The practical effect is that novices (low tactics or piloting skill) will typically act later in the round, even in light mechs. Highly advanced pilots have a better chance to act early in the round, even if they are in heavier Mechs. 

Note that mechs are divided into 10 ton categories by this mod. 20-25 ton mechs are slightly faster than 30-35 ton mechs. 80-85 tons mechs are faster than 90-95, which are faster than 100. Because of the randomness you may not think this effect is there, but it is. 

In addition, battlefield conditions matter more to your initiative. 
* Pilots with injuries suffer an initiative penalty between -4 to -7. Higher Guts ratings reduce this penalty. Pilots with additional health pips will ignore one injury per health pip before feeling these effects.
* Mechs with a missing leg or vehicles with a missing side count as crippled and suffer an initiative penalty of -13. This penalty is reduced 0.05% for each point of piloting.
* Pilots with the High Spirits tag gain a +2 initiative bonus. Pilots with the Low Spirits tag suffer a -2 initiative penalty.
* Pilots that are Inspiried gain a bonus between 1-3.
* Mechs that are knocked down suffer an initiative penalty equal to their Guts injury bounds. On following turns they suffer a -6 penalty, reduces 0.05% for each point of piloting skill.

In addition, addition
* INCOMPLETE: Units that suffer a melee attack will be penalized on their initiative. If the attacker is heavier than the target, every 5 tons of difference will impose a -1 initiative penalty. DFA attacks increase this bonus by 1.5 (rounded down). Juggernaught attacks multiply the final value by 2. A 100 ton Mech performing DFA on a 20 ton Mech would impose a 20 - 4 = -16 penalty, times 1.5 for DFA = -24. If the attacking pilot has Juggernaught, this comes -48. The defender reduces this effect by 0.05% for each point of piloting. Defenders that are heavier than attackers only suffer -1 penalty (before DFA and Juggernaught mods).

## Detailed Information

The tonnage of a unit determines 

##TODO

* Melee attacks reduce your initiative by the difference in tonnage; a 100t vs. 100t has limited impact, a 100t vs. 20t a bigger impact. Piloting skill mitigates this. Mechanics in place, but penalty not yet applied.
** Vehicles and turrets need tested; no way to apply DFA penalty against them (no DamageType in signatures)
* Test interactions with init modifying abilities; Juggernaught, Offensive Push, etc
* Extract logging from HBS.Logging to prevent duplication of logs
* Show init bonus/malus on Lance/MechBay screens. Currently replaced with a - character to signify it's meaningless. 

# BT-ComplexInitiative


## Development

## Design

There are 5 vanilla initiative phases, 5->1
- Your place in init order is determined by mech tonnage group (4=light, 3=medium, 2=heavy, 1=assault)
- Your init is reduced by knockdowns, some abilities (RT juggernaut, etc)
- Your init is increased by abilities (??)

New model
- Piloting mitigates your mech size
- Tactics determines your phase order
- High tactics / low pilot goes first, but gets a single activation
- High piloting / low tactics will go later, but may get multiple activations
- Tonnage imposes a 'cost' on initiative, but mitigated by piloting?
- No longer back and forth; initiative is randomly determined and you go in that order. So you may find yourself 'clumped up' 
- Random element to prevent ties

- Model 1
Phases are have step 'steps', so 5 * 10 = 50-41, 40-31, 30-21, 20-11, 10-0

	Base Init * 5 = starting point
	Base 5: 25
	Base 4: 20
	Base 3: 15
	Base 2: 10
	Base 1: 5

	For each point of tactics, you get +1 phase: 
	1, 2, 3, 4, 5, 6, 7, 8, 9, 10

	Base 5 + Tactics: 25 - 35
	Base 4 + Tactics: 20 - 30
	Base 3 + Tactics: 15 - 25
	Base 2 + Tactics: 10 - 20
	Base 1 + Tactics: 5 - 15

	You randomly gain d3 each turn; Piloting / 2 is the number of dice you roll
	Vehicles only gain d2 each turn, not d3
	1D3 -> 1-3, 5D3 -> 5-15

	Base 5 + Tactics + Roll: 26 - 38 / 30 - 50
	Base 4 + Tactics + Roll: 21 - 33 / 25 - 45
	Base 3 + Tactics + Roll: 16 - 28 / 20 - 40
	Base 2 + Tactics + Roll: 11 - 23 / 15 - 35
	Base 1 + Tactics + Roll: 6 - 18 / 10 - 30

	Piloting 10 + Assault 100 gives you 2 activations on a good roll, so somewhere between 25-30
	Pilot 10 + Light 20 gives you 3-4 activations on a good roll, so somewhere between 40-50

	Pilot / 2 -> Additional activations
		1, 2, 3, 4, 5
		Light -1, Medium -2, Heavy -3, Assault -4
		Light -1		:  1 -4
		Medium -2	:  1-3
		Heavy -3		:  1 -2
		Assault -4	: 1 -4
	Each activation consumes 10 points
		Base 4 + T1 + P10:	20 + 1 + 5D3 -> 26 to 36
		1 + 4 activations
			36, 26, 16, 6, NA

How to handle inspired?




IsAvailableThisPhase

TurnDirector::private void BeginNewRound(int round)


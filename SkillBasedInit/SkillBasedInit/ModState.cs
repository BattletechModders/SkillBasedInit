﻿using BattleTech;

namespace SkillBasedInit {
    public static class ModState {

        public static CombatGameState Combat;

        public static void Reset() {
            // Reinitialize state
            Combat = null;
        }
    }
}

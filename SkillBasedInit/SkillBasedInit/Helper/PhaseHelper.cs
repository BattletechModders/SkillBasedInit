﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBasedInit {
    public static class PhaseHelper {

        // Calculate the left and right phase boundaries *as initiative* 
        //   Will calculate 
        // 30, 29, 28, 27, 26 (red 30)
        // 30, 29, 28, 27, 26 (red 29)
        // 30, 29, 28, 27, 26 (red 28)
        // 29, 28, 27, 26, 25 (red 27)
        // 28, 27, 26, 25, 24 (red 26)
        // ...
        //  7,  6,  5,  4,  3 (red 5)
        //  6,  5,  4,  3,  2 (red 4)
        //  5,  4,  3,  2,  1 (red 3)
        //  5,  4,  3,  2,  1 (red 2)
        //  5,  4,  3,  2,  1 (red 1)
        public static int[] CalcPhaseIconBounds(int currentPhase) {

            // Normalize phase to initiative values
            int currentInit = (Mod.MaxPhase + 1) - currentPhase;
            
            int midPoint = currentInit;
            if (midPoint + 2 > Mod.MaxPhase || midPoint +1 > Mod.MaxPhase) {
                midPoint = Mod.MaxPhase - 2;
            } else if (midPoint - 2 < Mod.MinPhase || midPoint - 1 < Mod.MinPhase) {
                midPoint = Mod.MinPhase + 2;
            }

            int[] bounds = new int[] { midPoint +2, midPoint +1, midPoint, midPoint -1, midPoint -2 };
            //Mod.Log.Info?.Write($"For phase {currentPhase}, init bounds are: {bounds[0]} to {bounds[4]}");
            return bounds;
        }
    }
}

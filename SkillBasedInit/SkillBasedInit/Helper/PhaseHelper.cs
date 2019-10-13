using System;
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
            
            int leftPhase = currentInit;
            if (currentInit + 2 > Mod.MaxPhase) {
                leftPhase = Mod.MaxPhase;
            } else if (currentInit - 2 < Mod.MinPhase) {
                leftPhase = Mod.MinPhase + 4;
            }

            int[] bounds = new int[] { leftPhase, leftPhase -4 };
            Mod.Log.Info($"For phase {currentPhase}, init bounds are: {bounds[0]} to {bounds[1]}");
            return bounds;
        }
    }
}

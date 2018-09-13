using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {

    public interface INpRule
    {
        bool CheckRule();
        void ReadyNextSituation();
        NpSituation CompletedReadyNextSituation();
        void CleanUpForce();
    }

} //namespace np

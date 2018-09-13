using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {
        
    public interface INpEntityController
    {
        void CreateAndRegist<T>() where T : NpEntity, new();
        void Terminate();
        bool IsInvalidate();
        bool IsTerminating();
        NpEntity Exist(string key);
        NpEntity Exist(NpEntity entity);
    }

} //namespace np

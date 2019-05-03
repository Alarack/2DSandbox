using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL {

    public abstract class Condition : ScriptableObject {

        public abstract bool CheckCondition(StateManager stateManager);
        public string description;
        //public bool invert;

        

    }

}

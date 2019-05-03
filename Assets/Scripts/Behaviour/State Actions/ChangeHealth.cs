using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL {

    [CreateAssetMenu(menuName ="Actions/Test/Add Health")]
    public class ChangeHealth : StateActions {

        public override void Execute(StateManager state)
        {
            state.health += 10f;
        }
    }

}

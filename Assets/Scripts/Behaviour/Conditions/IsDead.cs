using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL {

    [CreateAssetMenu(menuName ="Conditions/Is Dead")]
    public class IsDead : Condition {


        private void OnEnable()
        {
            description = "Health Less than 0";
        }

        public override bool CheckCondition(StateManager state)
        {
            return state.health <= 0;
        }

    }

}

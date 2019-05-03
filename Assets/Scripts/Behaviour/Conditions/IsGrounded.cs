using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LL {

    [CreateAssetMenu(menuName = "Conditions/Is Grounded")]
    public class IsGrounded : Condition {

        private void OnEnable()
        {
            description = "In contact with the ground";
        }

        public override bool CheckCondition(StateManager stateManager)
        {
            if (stateManager.PlayerController != null)
            {

                return stateManager.PlayerController.RayController.IsGrounded;




            }

            return false;
        }
    }

}

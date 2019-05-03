using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LL.BehaviourEditor;


namespace LL {

    [CreateAssetMenu]
    public class BehaviourGraph : ScriptableObject {

        [SerializeField]
        public List<BaseNode> windows = new List<BaseNode>();
        [SerializeField]
        public int idCount;
        private List<int> indexToDelete = new List<int>();


        #region  CHECKERS      
        public BaseNode GetNodeWithIndex(int id)
        {
            int count = windows.Count;
            for (int i = 0; i < count; i++)
            {
                if (windows[i].id == id)
                    return windows[i];
            }

            return null;
        }

        public void DeleteWindowsThatNeedTo()
        {
            int count = indexToDelete.Count;
            for (int i = 0; i < count; i++)
            {
                BaseNode b = GetNodeWithIndex(indexToDelete[i]);
                if (b != null)
                    windows.Remove(b);

            }

            indexToDelete.Clear();
        }

        public void DeleteNode(int id)
        {
            if (indexToDelete.Contains(id) == false)
                indexToDelete.Add(id);
        }


        public bool IsStateDuplicate(BaseNode node)
        {
            int count = windows.Count;
            for (int i = 0; i < count; i++)
            {
                if (windows[i].id == node.id)
                    continue;

                if (windows[i].stateRef.currentState == node.stateRef.currentState && windows[i].isDuplicate == false)
                    return true;
            }


            return false;
        }

        public bool IsTransitionDuplicate(BaseNode node)
        {
            BaseNode enter = GetNodeWithIndex(node.enterNodeID);
            if (enter == null)
                return false;

            //Debug.Log("Passed in ID " + node.id + " Fetched ID: " + enter.id);


            int count = enter.stateRef.currentState.transitions.Count;
            for (int i = 0; i < count; i++)
            {
                Transition t = enter.stateRef.currentState.transitions[i];
                if (t.condition == node.transRef.previousCondition && node.transRef.transitionID != t.id)
                    return true;
            }



            return false;
        }




        #endregion


    }


}

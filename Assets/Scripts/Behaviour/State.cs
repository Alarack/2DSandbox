using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL {

    [CreateAssetMenu]
    public class State : ScriptableObject {

        public StateActions[] onUpdate;
        public StateActions[] onEnter;
        public StateActions[] onExit;

        [SerializeField]
        public List<Transition> transitions = new List<Transition>();
        public int idCount;

        public void OnEnter(StateManager states)
        {
            ExecuteActions(states, onEnter);
        }

        public void Tick(StateManager states)
        {
            ExecuteActions(states, onUpdate);
            CheckTransitions(states);
        }

        public void OnExit(StateManager states)
        {
            ExecuteActions(states, onExit);
        }

        public void CheckTransitions(StateManager states)
        {
            int count = transitions.Count;
            for (int i = 0; i < count; i++)
            {
                if (transitions[i].disable)
                    continue;

                if(transitions[i].condition.CheckCondition(states) == true)
                {
                    if(transitions[i].targetState != null)
                    {
                        states.currentState = transitions[i].targetState;
                        OnExit(states);
                        states.currentState.OnEnter(states);
                    }
                        

                    return;
                }

            }

        }

        public void ExecuteActions(StateManager states, StateActions[] list)
        {
            int count = list.Length;
            for (int i = 0; i < count; i++)
            {
                if (list[i] != null)
                    list[i].Execute(states);
            }
        }

        public Transition AddTransition()
        {
            Transition retVal = new Transition();
            transitions.Add(retVal);
            retVal.id = idCount;
            idCount++;
            return retVal;
        }

        public void RemoveTransitionByID(int id)
        {
            Transition target = GetTransitionByID(id);
            if (target != null)
                transitions.Remove(target);
        }

        public Transition GetTransitionByID(int id)
        {
            int count = transitions.Count;
            for (int i = 0; i < count; i++)
            {
                if (transitions[i].id == id)
                    return transitions[i];
            }

            return null;
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL {

    public class StateManager : MonoBehaviour {

        public float health;
        public State currentState;
        public PlayerController PlayerController { get; private set; }

        [HideInInspector]
        float delta;
        [HideInInspector]
        public Transform myTransform;


        private void Awake()
        {
            PlayerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            myTransform = this.transform;
        }

        private void Update()
        {
            if (currentState != null)
                currentState.Tick(this);
        }

    }

}

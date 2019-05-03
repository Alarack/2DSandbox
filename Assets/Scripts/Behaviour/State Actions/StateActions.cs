﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LL {

    public abstract class StateActions : ScriptableObject {

        public abstract void Execute(StateManager state);

    }

}

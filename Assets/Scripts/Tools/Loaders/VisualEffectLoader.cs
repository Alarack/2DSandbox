using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VisualEffectLoader  {

    public enum VisualEffectShape {
        Sphere = 0,
        Cube = 1,
        Cone = 2,
        Circle = 3,
        Rectangle = 4,
    }

    public enum VisualEffectSize {
        Tiny = 0,
        VerySmall = 1,
        Small = 2,
        Medium = 3,
        Large = 4,
        VeryLarge = 5,
        Huge = 6,
    }

    public static GameObject LoadVisualEffect(VisualEffectShape shape, VisualEffectSize size, string name = "")
    {
        GameObject result = Resources.Load("Visual Effects/" + shape + "/" + size + name) as GameObject;

        return result;
    }




}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LL;

namespace LL.BehaviourEditor {

    [CreateAssetMenu(menuName = "Editor/Nodes/Transition Node")]
    public class TransitionNode : DrawNode {


        public void Init(StateNode enterState, Transition transition)
        {
            //this.enterState = enterState;
        }

        public override void DrawWindow(BaseNode b)
        {
            EditorGUILayout.LabelField("");

            BaseNode enterNode = BehaviourEditor.settings.currentGraph.GetNodeWithIndex(b.enterNodeID);
            if (enterNode == null)
                return;

            if(enterNode.stateRef.currentState == null)
            {
                BehaviourEditor.settings.currentGraph.DeleteNode(b.id);
                return;
            }

            Transition transition = enterNode.stateRef.currentState.GetTransitionByID(b.transRef.transitionID);

            if (transition == null)
                return;

            transition.condition = (Condition)EditorGUILayout.ObjectField(transition.condition, typeof(Condition), false);

            if (transition.condition == null)
            {
                EditorGUILayout.LabelField("No Condition!");
                b.isAssigned = false;
            }
            else
            {
                b.isAssigned = true;
                if (b.isDuplicate == true)
                {
                    EditorGUILayout.LabelField("Duplicate Condition!");
                }
                else
                {
                    EditorGUILayout.LabelField(transition.condition.description);

                    BaseNode targetNode = BehaviourEditor.settings.currentGraph.GetNodeWithIndex(b.targetNodeID);
                    if(targetNode != null)
                    {
                        if (targetNode.isDuplicate == false)
                            transition.targetState = targetNode.stateRef.currentState;
                        else
                            transition.targetState = null;
                    }
                    else
                    {
                        transition.targetState = null;
                    }

                }
            }

            if (b.transRef.previousCondition != transition.condition)
            {
                b.transRef.previousCondition = transition.condition;
                b.isDuplicate = BehaviourEditor.settings.currentGraph.IsTransitionDuplicate(b);
                if (b.isDuplicate == false)
                {
                    BehaviourEditor.forceSetDirty = true;
                    //BehaviourEditor.currentGraph.SetNode(this);

                }
                
            }
        }


        public override void DrawCurve(BaseNode b)
        {
            Rect rect = b.windowRect;
            rect.y += b.windowRect.height * 0.5f;
            rect.width = 1f;
            rect.height = 1f;

            BaseNode enter = BehaviourEditor.settings.currentGraph.GetNodeWithIndex(b.enterNodeID);
            if (enter == null)
            {
                BehaviourEditor.settings.currentGraph.DeleteNode(b.id);
            }
            else
            {
                Color targetColor = Color.green;

                if (b.isAssigned == false || b.isDuplicate)
                    targetColor = Color.red;
                
                Rect r = enter.windowRect;
                BehaviourEditor.DrawNodeCurve(r, rect, true, targetColor);
            }

            if (b.isDuplicate)
                return;

            if(b.targetNodeID > 0)
            {
                BaseNode target = BehaviourEditor.settings.currentGraph.GetNodeWithIndex(b.targetNodeID);
                if (target == null)
                {
                    b.targetNodeID = -1;
                }
                else
                {
                    rect = b.windowRect;
                    rect.x += rect.width;
                    Rect endRect = target.windowRect;
                    endRect.x -= endRect.width * 0.5f;

                    Color targetColor = Color.green;


                    if(target.drawNode is StateNode)
                    {
                        if (target.isAssigned == false || target.isDuplicate == true)
                            targetColor = Color.red;
                    }
                    else
                    {
                        if (target.isAssigned == false)
                            targetColor = Color.red;
                        else
                            targetColor = Color.yellow;
                        
                    }

                    BehaviourEditor.DrawNodeCurve(rect, endRect, false, targetColor);
                }
            }

        }



    }

}

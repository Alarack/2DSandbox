using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LL;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;

namespace LL.BehaviourEditor {

    [CreateAssetMenu(menuName = "Editor/Nodes/State Node")]
    public class StateNode : DrawNode {

        public override void DrawWindow(BaseNode b)
        {

            if (b.stateRef.currentState == null)
            {
                EditorGUILayout.LabelField("Add state to modify:");
            }
            else
            {
                if (b.collapse == false)
                {

                }
                else
                {
                    b.windowRect.height = 100f;
                }
                b.collapse = EditorGUILayout.Toggle(" ", b.collapse);
            }

            b.stateRef.currentState = (State)EditorGUILayout.ObjectField(b.stateRef.currentState, typeof(State), false);

            if (b.previousCollapse != b.collapse)
            {
                b.previousCollapse = b.collapse;
            }


            if (b.stateRef.previousState != b.stateRef.currentState)
            {
                b.isDuplicate = BehaviourEditor.settings.currentGraph.IsStateDuplicate(b);
                b.stateRef.previousState = b.stateRef.currentState;
                if(b.isDuplicate == false)
                {
                    Vector3 pos = new Vector3(b.windowRect.x, b.windowRect.y, 0f);
                    pos.x += b.windowRect.width * 2f;

                    SetupReorderableLists(b);

                    //Load Transitions
                    int count = b.stateRef.currentState.transitions.Count;
                    for (int i = 0; i < count; i++)
                    {
                        pos.y += i * 100f;
                        BehaviourEditor.AddTransitionNodeFromTransition(b.stateRef.currentState.transitions[i], b, pos);
                    }

                    BehaviourEditor.forceSetDirty = true;
                }
            }

            if (b.isDuplicate == true)
            {
                EditorGUILayout.LabelField("State is a duplicate");
                b.windowRect.height = 100f;
                return;
            }

            if (b.stateRef.currentState != null)
            {
                b.isAssigned = true;

                if(b.collapse == false)
                {
                    if(b.stateRef.serializedState == null)
                    {
                        SetupReorderableLists(b);
                    }

                    b.stateRef.serializedState.Update();

                    EditorGUILayout.LabelField("");
                    b.stateRef.onUpdateList.DoLayoutList();
                    EditorGUILayout.LabelField("");
                    b.stateRef.onEnterList.DoLayoutList();
                    EditorGUILayout.LabelField("");
                    b.stateRef.onExitList.DoLayoutList();

                    b.stateRef.serializedState.ApplyModifiedProperties();

                    float standard = 300f;
                    standard += (b.stateRef.onUpdateList.count + b.stateRef.onEnterList.count + b.stateRef.onExitList.count) * 20f;
                    b.windowRect.height = standard;

                }
            }
            else
            {
                b.isAssigned = false;
            }
        }

        private void SetupReorderableLists(BaseNode b)
        {
            Debug.Log("Setting up lists");

            b.stateRef.serializedState = new SerializedObject(b.stateRef.currentState);
            
            b.stateRef.onUpdateList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onUpdate"), true, true, true, true);
            b.stateRef.onEnterList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onEnter"), true, true, true, true);
            b.stateRef.onExitList = new ReorderableList(b.stateRef.serializedState, b.stateRef.serializedState.FindProperty("onExit"), true, true, true, true);

            HandleReorderableList(b.stateRef.onUpdateList, "On Update");
            HandleReorderableList(b.stateRef.onEnterList, "On Enter");
            HandleReorderableList(b.stateRef.onExitList, "On Exit");
        }

        private void HandleReorderableList(ReorderableList list, string targetName)
        {
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, targetName);
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };
        }

        public override void DrawCurve(BaseNode b)
        {

        }

        public Transition AddTransition(BaseNode b)
        {
            return b.stateRef.currentState.AddTransition();
        }

        public void ClearReferences()
        {
            //BehaviourEditor.ClearWindowsFromList(dependencies);
            //dependencies.Clear();
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LL.BehaviourEditor {

    public class BehaviourEditor : EditorWindow {

        #region VARIABLES

        public enum UserActions {
            AddState,
            AddTransitionNode,
            DeleteNode,
            DeleteTransition,
            CommentNode,
            MakeTransition,
            MakePortal,
            ResetScroll
        }


        private Vector3 mousePosition;
        private Vector2 scrollPos;
        private Vector2 scrollStartPos;
        private bool clickedOnWindow;
        private BaseNode selectedNode;
        private int selectedIndex;

        private int transitionFromID;
        private Rect mouseRect = new Rect(0f, 0f, 1f, 1f);
        private Rect all = new Rect(-5f, -5f, 10000f, 10000f);
        private GUIStyle style;
        private GUIStyle activeStyle;

        public static EditorSettings settings;
        public static StateManager currentStateManager;
        private static StateManager previousStateManager;
        private static State previousState;
        public static bool forceSetDirty;

        #endregion

        #region  INIT

        [MenuItem("Behaviour Editor/Editor")]
        private static void ShowEditor()
        {
            BehaviourEditor editor = EditorWindow.GetWindow<BehaviourEditor>();
            editor.minSize = new Vector2(800, 600);

        }

        private void OnEnable()
        {
            settings = Resources.Load("EditorSettings") as EditorSettings;
            style = settings.skin.GetStyle("window");
            activeStyle = settings.activeSkin.GetStyle("window");
        }

        #endregion

        private void Update()
        {
            if(currentStateManager != null)
            {
                if(previousState != currentStateManager.currentState)
                {
                    Repaint();
                    previousState = currentStateManager.currentState;
                }
            }
        }

        #region GUI METHODS
        private void OnGUI()
        {
            if(Selection.activeTransform != null)
            {
                currentStateManager = Selection.activeTransform.GetComponentInChildren<StateManager>();
                if(previousStateManager != currentStateManager)
                {
                    previousStateManager = currentStateManager;
                    Repaint();
                }

            }

            

            Event e = Event.current;
            mousePosition = e.mousePosition;
            UserInput(e);
            DrawWindows();

            if (e.type == EventType.MouseDrag)
            {
                settings.currentGraph.DeleteWindowsThatNeedTo();
                Repaint();
            }

            if (GUI.changed)
            {
                settings.currentGraph.DeleteWindowsThatNeedTo();
                Repaint();
            }

            if (settings.makeTransition)
            {
                mouseRect.x = mousePosition.x;
                mouseRect.y = mousePosition.y;
                Rect from = settings.currentGraph.GetNodeWithIndex(transitionFromID).windowRect;
                DrawNodeCurve(from, mouseRect, true, Color.blue);
                Repaint();
            }

            if (forceSetDirty)
            {
                forceSetDirty = false;
                EditorUtility.SetDirty(settings);
                EditorUtility.SetDirty(settings.currentGraph);

                int count = settings.currentGraph.windows.Count;
                for (int i = 0; i < count; i++)
                {
                    BaseNode n = settings.currentGraph.windows[i];
                    if(n.stateRef.currentState != null)
                    {
                        EditorUtility.SetDirty(n.stateRef.currentState);
                    }
                }

            }



        }

        private void DrawWindows()
        {
            GUILayout.BeginArea(all, style);

            BeginWindows();
            EditorGUILayout.LabelField(" ", GUILayout.Width(100f));
            EditorGUILayout.LabelField("Assign Graph:", GUILayout.Width(100f));
            settings.currentGraph = (BehaviourGraph)EditorGUILayout.ObjectField(settings.currentGraph, typeof(BehaviourGraph), false, GUILayout.Width(200f));


            if (settings.currentGraph != null)
            {
                foreach (BaseNode n in settings.currentGraph.windows)
                {
                    n.DrawCurve();
                }

                int count = settings.currentGraph.windows.Count;
                for (int i = 0; i < count; i++)
                {
                    BaseNode b = settings.currentGraph.windows[i];

                    Rect defaultRect = GUI.Window(i, b.windowRect, DrawNodeWindow, b.windowTitle);

                    if (b.drawNode is StateNode)
                    {
                        if (currentStateManager != null && b.stateRef.currentState == currentStateManager.currentState)
                        {
                            b.windowRect = GUI.Window(i, b.windowRect, DrawNodeWindow, b.windowTitle, activeStyle);
                        }
                        else
                        {
                            b.windowRect = defaultRect;
                        }
                    }
                    else
                    {
                        b.windowRect = defaultRect;
                    }
                }
            }

            EndWindows();

            GUILayout.EndArea();
        }

        private void DrawNodeWindow(int id)
        {
            settings.currentGraph.windows[id].DrawWindow();
            GUI.DragWindow();
        }

        private void UserInput(Event e)
        {
            if (settings.currentGraph == null)
                return;

            if (e.button == 1 && settings.makeTransition == false)
            {
                if (e.type == EventType.MouseDown)
                {
                    RightClick(e);
                }
            }

            if (e.button == 0 && settings.makeTransition == false)
            {
                if (e.type == EventType.MouseDown)
                {
                    LeftClick(e);
                }
            }

            if (e.button == 0 && settings.makeTransition == true)
            {
                if (e.type == EventType.MouseDown)
                {
                    MakeTransition();
                }
            }

            if(e.button == 2)
            {
                if(e.type == EventType.MouseDown)
                {
                    scrollStartPos = e.mousePosition;
                }
                else if(e.type == EventType.MouseDrag)
                {
                    HandlePanning(e);
                }
                else if(e.type == EventType.MouseUp)
                {

                }
            }

        }

        private void HandlePanning(Event e)
        {
            Vector2 diff = e.mousePosition - scrollStartPos;
            diff *= 0.6f;
            scrollStartPos = e.mousePosition;
            scrollPos += diff;

            int count = settings.currentGraph.windows.Count;
            for (int i = 0; i < count; i++)
            {
                BaseNode b = settings.currentGraph.windows[i];
                b.windowRect.x += diff.x;
                b.windowRect.y += diff.y;

            }
        }

        private void ResetScroll()
        {
            int count = settings.currentGraph.windows.Count;
            for (int i = 0; i < count; i++)
            {
                BaseNode b = settings.currentGraph.windows[i];
                b.windowRect.x -= scrollPos.x;
                b.windowRect.y -= scrollPos.y;
            }

            scrollPos = Vector2.zero;
        }

        private void RightClick(Event e)
        {
            selectedIndex = -1;
            clickedOnWindow = false;
            int count = settings.currentGraph.windows.Count;
            for (int i = 0; i < count; i++)
            {
                if (settings.currentGraph.windows[i].windowRect.Contains(e.mousePosition))
                {
                    clickedOnWindow = true;
                    selectedNode = settings.currentGraph.windows[i];
                    selectedIndex = i;
                    break;
                }
            }

            if (clickedOnWindow == false)
            {
                AddNewNode(e);
            }
            else
            {
                ModifyNode(e);
            }
        }

        private void LeftClick(Event e)
        {

        }

        private void MakeTransition()
        {
            settings.makeTransition = false;
            clickedOnWindow = false;
            int count = settings.currentGraph.windows.Count;
            for (int i = 0; i < count; i++)
            {
                if (settings.currentGraph.windows[i].windowRect.Contains(mousePosition))
                {
                    clickedOnWindow = true;
                    selectedNode = settings.currentGraph.windows[i];
                    selectedIndex = i;
                    break;
                }
            }

            if (clickedOnWindow)
            {
                if (selectedNode.drawNode is StateNode || selectedNode.drawNode is PortalNode)
                {
                    if (selectedNode.id != transitionFromID)
                    {
                        BaseNode transNode = settings.currentGraph.GetNodeWithIndex(transitionFromID);
                        transNode.targetNodeID = selectedNode.id;

                        BaseNode enterNode = BehaviourEditor.settings.currentGraph.GetNodeWithIndex(transNode.enterNodeID);
                        Transition transition = enterNode.stateRef.currentState.GetTransitionByID(transNode.transRef.transitionID);

                        transition.targetState = selectedNode.stateRef.currentState;
                    }
                }

  
            }
        }

        #endregion

        #region CONTEXT MENUS
        private void AddNewNode(Event e)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddSeparator("");

            if (settings.currentGraph != null)
            {
                menu.AddItem(new GUIContent("Add State"), false, ContextCallBack, UserActions.AddState);
                menu.AddItem(new GUIContent("Make Portal"), false, ContextCallBack, UserActions.MakePortal);
                menu.AddItem(new GUIContent("Add Comment"), false, ContextCallBack, UserActions.CommentNode);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reset Scroll"), false, ContextCallBack, UserActions.ResetScroll);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Add State"));
                menu.AddDisabledItem(new GUIContent("Add Comment"));
            }

            menu.ShowAsContext();
            e.Use();
        }

        private void ModifyNode(Event e)
        {
            GenericMenu menu = new GenericMenu();

            if (selectedNode.drawNode is StateNode)
            {
                //StateNode stateNode = selectedNode.drawNode as StateNode;
                if (selectedNode.stateRef.currentState != null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Add Condition"), false, ContextCallBack, UserActions.AddTransitionNode);
                }
                else
                {
                    menu.AddSeparator("");
                    menu.AddDisabledItem(new GUIContent("Add Condition"));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, ContextCallBack, UserActions.DeleteNode);
            }

            if (selectedNode.drawNode is PortalNode)
            {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, ContextCallBack, UserActions.DeleteNode);
            }

            if (selectedNode.drawNode is TransitionNode)
            {
                if(selectedNode.isDuplicate || selectedNode.isAssigned == false)
                {
                    menu.AddSeparator("");
                    menu.AddDisabledItem(new GUIContent("Make Transition"));
                }
                else
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Make Transition"), false, ContextCallBack, UserActions.MakeTransition);
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, ContextCallBack, UserActions.DeleteNode);
            }



            menu.ShowAsContext();
            e.Use();

        }

        private void ContextCallBack(object o)
        {
            UserActions a = (UserActions)o;

            switch (a)
            {
                case UserActions.AddState:
                    settings.AddNodeOnGraph(settings.stateNode, 200f, 100f, "State", mousePosition);

                    break;

                case UserActions.MakePortal:
                    settings.AddNodeOnGraph(settings.portalNode, 150f, 80f, "Portal", mousePosition);
                    break;

                case UserActions.AddTransitionNode:
                    AddTransitionNode(selectedNode, mousePosition);

                    break;

                case UserActions.CommentNode:

                    break;

                default:
                    break;

                case UserActions.DeleteNode:

                    if (selectedNode.drawNode is TransitionNode)
                    {
                        BaseNode enterNode = settings.currentGraph.GetNodeWithIndex(selectedNode.enterNodeID);
                        if (enterNode != null)
                            enterNode.stateRef.currentState.RemoveTransitionByID(selectedNode.transRef.transitionID);
                    }

                    settings.currentGraph.DeleteNode(selectedNode.id);
                    break;

                case UserActions.MakeTransition:
                    transitionFromID = selectedNode.id;
                    settings.makeTransition = true;

                    break;

                case UserActions.ResetScroll:
                    ResetScroll();
                    break;
            }

            forceSetDirty = true;


        }

        public static BaseNode AddTransitionNode(BaseNode enterNode, Vector3 pos)
        {
            BaseNode transNode = settings.AddNodeOnGraph(settings.transitionNode, 200f, 100f, "Condition", pos);
            transNode.enterNodeID = enterNode.id;
            Transition t = settings.stateNode.AddTransition(enterNode);
            transNode.transRef.transitionID = t.id;
            return transNode;
        }

        public static BaseNode AddTransitionNodeFromTransition(Transition transition, BaseNode enterNode, Vector3 pos)
        {
            BaseNode transNode = settings.AddNodeOnGraph(settings.transitionNode, 200f, 100f, "Condition", pos);
            transNode.enterNodeID = enterNode.id;
            transNode.transRef.transitionID = transition.id;
            return transNode;
        }

        #endregion



        #region HELPERS

        //public static StateNode AddStateNode(Vector2 pos)
        //{
        //    //StateNode stateNode = CreateInstance<StateNode>();

        //    //stateNode.windowRect = new Rect(pos.x, pos.y, 200f, 300f);
        //    //stateNode.windowTitle = "State";
        //    //settings.currentGraph.windows.Add(stateNode);
        //    ////settings.currentGraph.SetStateNode(stateNode);

        //    return stateNode;
        //}

        //public static CommentNode AddCommentNode(Vector2 pos)
        //{
        //    CommentNode commentNode = CreateInstance<CommentNode>();

        //    commentNode.windowRect = new Rect(pos.x, pos.y, 200f, 100f);
        //    commentNode.windowTitle = "Comment";
        //    settings.currentGraph.windows.Add(commentNode);

        //    return commentNode;
        //}

        //public static TransitionNode AddTransitionNode(int index, Transition transition, StateNode from)
        //{
        //    //Rect fromRect = from.windowRect;
        //    //fromRect.x += 50f;

        //    //float targetY = fromRect.y - fromRect.height;
        //    //if (from.currentState != null)
        //    //{
        //    //    targetY += (index * 100);
        //    //}

        //    //fromRect.y = targetY;
        //    //fromRect.x = 200f + 100f;
        //    //fromRect.y += (fromRect.height * 0.7f);

        //    //Vector2 pos = new Vector2(fromRect.x, fromRect.y);

        //    return AddTransitionNode(pos, transition, from);
        //}

        //public static TransitionNode AddTransitionNode(Vector2 pos, Transition transition, StateNode from)
        //{
        //    TransitionNode transitionNode = CreateInstance<TransitionNode>();
        //    transitionNode.Init(from, transition);
        //    transitionNode.windowRect = new Rect(pos.x, pos.y, 200f, 80f);
        //    transitionNode.windowTitle = "Condition Check";
        //    settings.currentGraph.windows.Add(transitionNode);
        //    from.dependencies.Add(transitionNode);

        //    return transitionNode;
        //}

        public static void DrawNodeCurve(Rect start, Rect end, bool left, Color curveColor)
        {

            float startX = left == true ? start.x + start.width : start.x;
            float startY = start.y + (start.height * 0.5f);
            Vector3 startPos = new Vector3(startX, startY, 0f);

            float endX = end.x + (end.width * 0.5f);
            float endY = end.y + (end.height * 0.5f);
            Vector3 endPos = new Vector3(endX, endY, 0f);

            Vector3 startTan = startPos + Vector3.right * 50f;
            Vector3 endTan = endPos + Vector3.left * 50f;

            Color shadow = new Color(0, 0, 0, 0.6f);
            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadow, null, (i + 1) * 1f);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, curveColor, null, 3f);

        }

        public static void ClearWindowsFromList(List<BaseNode> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                //if (windows.Contains(list[i]))
                //windows.Remove(list[i]);
            }
        }




        #endregion  


    }
}

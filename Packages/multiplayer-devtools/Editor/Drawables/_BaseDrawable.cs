using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MultiPlayerDevTools.Drawables
{
    public class _BaseDrawable
    {
        public enum ToggleTypes
        {
            CheckBox,
            FoldOut
        }
        
        public struct Notification
        {
            public string Content;
            public MessageType ContentTypes;

            public Notification(string content, MessageType contentTypes)
            {
                Content = content;
                ContentTypes = contentTypes;
            }
        }
        
        public int ControlId { get; set; }
        public string Label { get; set; }
        
        public Notification? HelpBox { get; set; }
        
        public bool Disabled { protected get; set; }
        public Dictionary<string, string> Dialog { get; set; }
        public static Dictionary<string, string> StaticDialog { get; set; }
        public Dictionary<string, string> Panel { get; set; }
        public string PanelSelectedItem { get; set; }
        
        public Action BeforeAction { protected get; set; }
        public Action<object[]> BeforeActionWithArgs { get; set; }
        public Action Action { protected get; set; }
        public Action<object[]> ActionWithArgs { get; set; }
        public Func<object> BeforeFunction { protected get; set; }
        public Func<object> Function { get; set; }
        public Func<object, object[]> FunctionWithArgs { get; set; }
        public object[] BeforeActionArgs { get; set; }
        public object[] ActionArgs { get; set; }
        public object[] FunctionArgs { get; set; }
        
        protected _BaseDrawable()
        {}
        
        public static void StartRow(GUIStyle style = null)
        {
            if (style != null) 
                EditorGUILayout.BeginHorizontal(style);
            else 
                EditorGUILayout.BeginHorizontal();
        }

        public static void EndRow()
        {
            EditorGUILayout.EndHorizontal();
        }
        
        public static void StartColumn(GUIStyle style = null)
        {
            if (style != null) 
                EditorGUILayout.BeginVertical(style);
            else 
                EditorGUILayout.BeginVertical();
        }

        public static void EndColumn()
        {
            EditorGUILayout.EndVertical();
        }
        
        public static void DrawHorizontalLine(Color color, int thickness = 1, int padding = 15)
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            
            rect.height = thickness;
            rect.y += padding / 2.0f;
            rect.x -= 2;
            rect.width += 6;
            
            EditorGUI.DrawRect(rect, color);
        }

        public static bool DrawFoldOut(bool foldout, string content, bool toggleOnLabelClick = true, GUIStyle headerStyle = null)
        {
            return EditorGUILayout.Foldout(foldout, content, toggleOnLabelClick, headerStyle ?? EditorStyles.foldout);
        }
        
        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }
        
        public static void Separator()
        {
            EditorGUILayout.Separator();
        }

        protected void SetControlName()
        {
            var controlName = Regex.Replace(Label, @"\s+", "");
            
            GUI.SetNextControlName($"{ControlId}:{controlName}");
        }
        
        protected void InvokeMethod()
        {
            if (ActionArgs != null || BeforeActionArgs != null)
            {
                BeforeActionWithArgs?.Invoke(BeforeActionArgs);
                ActionWithArgs?.Invoke(ActionArgs);
            }
            else
            {
                BeforeAction?.Invoke();
                Action?.Invoke();
            }

            if (FunctionArgs != null)
            {
                FunctionWithArgs?.Invoke(FunctionArgs);
            }
            else
            {
                BeforeFunction?.Invoke();
                Function?.Invoke();
            }
        }
        
        protected string ShowPanel()
        {
            return EditorUtility.OpenFolderPanel(
                Panel["title"], 
                Panel["folder"], 
                Panel["defaultName"]
            );
        }

        protected void ShowDialog(Action invokeMethod)
        {
            if (EditorUtility.DisplayDialog(
                Dialog["title"],
                Dialog["message"],
                Dialog["ok"],
                Dialog["cancel"]
            ))
            {
                invokeMethod?.Invoke();
            }
        }
        
        protected void SetUpDialog()
        {
            /*
             * TODO: only  a single action arg can be passed to action args array
             * should figure out multiple actions scenario as well
             * also should use before action instead of this
             */
            var actionableArg = ActionArgs?.SingleOrDefault(arg => arg is Func<Dictionary<string, string>>);
            
            if (actionableArg == null) return;
            
            var dialog = ((Func<Dictionary<string, string>>) actionableArg).Invoke();

            var actionArgs = ActionArgs.ToList();
            
            actionArgs.Remove(actionableArg);

            ActionArgs = actionArgs.ToArray();
            Dialog = dialog;
        }
    }
}
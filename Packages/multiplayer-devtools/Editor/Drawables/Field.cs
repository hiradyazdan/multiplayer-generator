using System;
using UnityEditor;
using UnityEngine;

namespace MultiPlayerDevTools.Drawables
{
    public class Field : _BaseDrawable
    {
        public enum FieldTypes
        {
            Label,
            Text,
            Int,
            IntSlider,
            Toggle,
            Color
        }
        
        public enum ToggleModes
        {
            Master,
            Slave
        }

        public FieldTypes FieldType { private get; set; }
        public string Label { get; set; }
        public Color? FieldColor { get; set; }
        public string Value { get; set; }
        public ToggleModes ToggleMode { get; set; }
        public bool IsToggleLeft { get; set; }
        public bool ToggleValue { get; set; }
        public bool MixedToggle { get; set; }
        public GUIStyle ToggleStyle { get; set; }
        public GUIStyle LabelStyle { get; set; }
        public GUILayoutOption[] ToggleOptions { get; set; }

        private static bool _toggleValue;

        public Field Draw(bool hasSeparator = true, bool toggleable = false)
        {
            if (hasSeparator)
            {
                Separator();
            }
            
            if (toggleable)
            {
                EditorGUI.BeginDisabledGroup(Disabled);
            }

            GUI.backgroundColor = FieldColor ?? Color.white;
            switch (FieldType)
            {
                case FieldTypes.Label:
                    EditorGUILayout.LabelField(Label);
                    break;
                case FieldTypes.Text:
                    Value = EditorGUILayout.TextField(Label, Value);
                    break;
                case FieldTypes.Int:
                    break;
                case FieldTypes.IntSlider:
                    break;
                case FieldTypes.Toggle:
                    EditorGUI.showMixedValue = MixedToggle;
                    
                    ToggleValue = IsToggleLeft
                        ? EditorGUILayout.ToggleLeft(Label, ToggleValue, LabelStyle ?? new GUIStyle(GUI.skin.label), ToggleOptions ?? new GUILayoutOption[0])
                        : EditorGUILayout.Toggle(Label, ToggleValue, ToggleStyle ?? new GUIStyle(GUI.skin.toggle), ToggleOptions ?? new GUILayoutOption[0]);
                    
                    if (GUI.changed && ToggleValue != _toggleValue)
                    {
                        _toggleValue = ToggleValue;
                        
                        InvokeMethod();
                    }
                    
                    EditorGUI.showMixedValue = false;
                    break;
                case FieldTypes.Color:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(FieldType), FieldType, null);
            }
            
            if (toggleable)
            {
                EditorGUI.EndDisabledGroup();
            }

            if (hasSeparator)
            {
                Separator();
            }

            return this;
        }
        
        public void DrawIntField()
        {
            EditorGUILayout.BeginHorizontal();
    //        Item.InstanceName = EditorGUILayout.TextField("Instance Name", Item.InstanceName);
            EditorGUILayout.LabelField("Int Example");
    //		_current.intExample = EditorGUILayout.IntField(_current.intExample);
            EditorGUILayout.EndHorizontal();
        }

        public void DrawIntSlider()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Int Slider Example");
    //		_current.sliderExample = EditorGUILayout.IntSlider(_current.sliderExample,1, 10);
            EditorGUILayout.EndHorizontal();
        }

        public void DrawStringField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("String Example");
    //		_current.stringExample = EditorGUILayout.TextField(_current.stringExample);
            EditorGUILayout.EndHorizontal();
        }

        public void DrawBoolField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Toggle Example");
    //		_current.booleanExample = EditorGUILayout.Toggle(_current.booleanExample);
            EditorGUILayout.EndHorizontal();
        }

        public void DrawColorField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Color Example");
    //		_current.colorExample = EditorGUILayout.ColorField(_current.colorExample);
            EditorGUILayout.EndHorizontal();
        }
    }
}
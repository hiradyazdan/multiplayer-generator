using UnityEditor;
using UnityEngine;

namespace MultiPlayerDevTools.Drawables
{
    public class Button : _BaseDrawable
    {
        public string Title { get; set; }
        public Color? TitleColor { get; set; }
        public Color? ButtonColor { get; set; }
        public float Width { get; set; } = 100;
        public float Height { get; set; } = 20;
        public bool FitWindowWidth { get; set; }
        
        public Button Draw(bool toggleable = false)
        {
            if (toggleable)
            {
                EditorGUI.BeginDisabledGroup(Disabled);
            }
            
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = {
                    textColor = TitleColor ?? Color.black
                }
            };

            var buttonOptions = new[]
            {
                FitWindowWidth ? GUILayout.MaxWidth(Screen.width) : GUILayout.Width(Width),
                GUILayout.Height(Height)
            };
            
            GUI.backgroundColor = ButtonColor ?? Color.white;
            if (GUILayout.Button(Title, buttonStyle, buttonOptions))
            {
                if (Dialog != null)
                {
                    SetUpDialog();
                    ShowDialog(InvokeMethod);
                }
                else if (Panel != null)
                {
                    PanelSelectedItem = string.IsNullOrEmpty(ShowPanel()) ? PanelSelectedItem : ShowPanel();
                }
                else
                {
                    InvokeMethod();
                }
            }
            GUI.backgroundColor = Color.white;
            
            if (toggleable)
            {
                EditorGUI.EndDisabledGroup();
            }

            return this;
        }

    //    public void SetButtonStyles()
    //    {
    //        _buttonStyle = new GUIStyle(GUI.skin.button)
    //        {
    //            normal = {textColor = Color.red}, 
    //            onNormal = {textColor = Color.green}
    //        };
    //
    //        if (IsRunning)
    //        {
    //            LaunchBtnText = "Running";
    //            LaunchBtnColor = new Color(0.14f, 0.51f, 0.07f);
    //
    //            RemoveBtnText = "Remove";
    //            RemoveBtnColor = Color.red;
    //        }
    //        else
    //        {
    //            LaunchBtnText = "Launch";
    //            LaunchBtnColor = Color.white;
    //
    //            RemoveBtnText = "Remove";
    //            RemoveBtnColor = Color.white;
    //        }
    //    }
    }
}
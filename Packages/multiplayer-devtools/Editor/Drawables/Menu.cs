using System;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MultiPlayerDevTools.Drawables
{
    public class Menu : _BaseDrawable
    {
        public enum MenuTypes
        {
            DropDown
        }
    
        public struct PopupElement
        {
            public bool RequiresTeamLicense { get; }
            public GUIContent Content { get; }
            public bool Disabled { get; set; }

            public bool Enabled => !Disabled && (!RequiresTeamLicense || InternalEditorUtility.HasTeamLicense());
            
            public PopupElement(string content, bool requiresTeamLicense = false, bool disabled = false)
            {
                Content = new GUIContent(content);
                RequiresTeamLicense = requiresTeamLicense;
                Disabled = disabled;
            }
        }
        
        public MenuTypes MenuType { private get; set; }
        public Color? MenuColor { get; set; }
        public PopupElement[] MenuList { get; set; }
        public int SelectedItemIndex { get; set; }
        public GenericMenu.MenuFunction2 MenuAction { get; set; }
        
        public Menu Draw(bool hasSeparator = true, bool toggleable = false)
        {
            if (hasSeparator) Separator();
            if (toggleable) EditorGUI.BeginDisabledGroup(Disabled);
            
            SetControlName();
            
            var selectedItem = MenuList[SelectedItemIndex];
            
            GUI.backgroundColor = MenuColor ?? Color.white;
            switch (MenuType)
            {
                case MenuTypes.DropDown:
                    RenderDropDownMenu(selectedItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(MenuType), MenuType, null);
            }

            if (HelpBox?.Content != null) EditorGUILayout.HelpBox(HelpBox.Value.Content, HelpBox.Value.ContentTypes, false);
            if (toggleable) EditorGUI.EndDisabledGroup();
            if (hasSeparator) Separator();

            return this;
        }

        private void RenderDropDownMenu(PopupElement selectedItem)
        {
            var content = new GUIContent(selectedItem.Content);
            var popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);
                    
            popupRect = EditorGUI.PrefixLabel(popupRect, 0, EditorGUIUtility.TrTextContent(Label));
                    
            if (EditorGUI.DropdownButton(popupRect, content, FocusType.Keyboard, EditorStyles.popup))
            {
                DropDown(popupRect);
            }
        }

        private void DropDown(Rect popupRect)
        {
            var menu = new GenericMenu();
        
            for (var i = 0; i < MenuList.Length; i++)
            {
                var element = MenuList[i];
                
                if (element.Enabled)
                {
                    menu.AddItem(element.Content, i == SelectedItemIndex, MenuAction, i);
                }
                else
                {
                    menu.AddDisabledItem(element.Content);
                }
            }
        
            menu.DropDown(popupRect);
        }
    }
}
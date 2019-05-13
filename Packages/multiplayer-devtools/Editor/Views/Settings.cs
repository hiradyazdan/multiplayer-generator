using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using MultiPlayerDevTools.Drawables;
using UnityEditor;
using UnityEngine;

namespace MultiPlayerDevTools.Views
{
    public class Settings
    {
        public static bool IsEnabled { get; set; }
        public static string ContainerDirPath { get; set; }

        private Tabs _settingsTab;
        private static bool _playOnLaunch;
        private static string assetPath => Application.dataPath;
        private static string _windowStatesFilePath;
        
        public Settings()
        {
            
        }
        
        public Settings Render()
        {
            IsEnabled = new Field
            {
                FieldType = Field.FieldTypes.Toggle,
                IsToggleLeft = true,
                Label = "Multi-player DevTools Enabled",
                ToggleValue = IsEnabled
            }.Draw(false).ToggleValue;
            
            EditorGUILayout.Space();
            
            RenderPlayerGeneratorSettings();
            
            return this;
        }
        
        public static void SetInstanceData()
        {
            LoadStates();
	    
            EditorApplication.isPlaying = _playOnLaunch;
        }
        
        public static void SaveStates()
        {
            var editorWindowStates = new Dictionary<string, string>
            {
                {"_multipleEditorMode", $"{IsEnabled}"},
                {"_playOnLaunch", $"{_playOnLaunch}"},
                {"_containerDirPath", $"{ContainerDirPath}"}
            };
	    
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
	    
            binaryFormatter.Serialize(memoryStream, editorWindowStates);
	    
            var serializedStates = memoryStream.ToArray();
	    
            File.WriteAllBytes(_windowStatesFilePath, serializedStates);
        }
        
        public static void LoadStates()
        {
//	    var currentScript = MonoScript.FromScriptableObject(this);
//	    var scriptPath = AssetDatabase.GetAssetPath(currentScript);

//	    Debug.Log("currentScript: " + currentScript);
//	    Debug.Log("scriptPath: " + scriptPath);
	    
            _windowStatesFilePath = $"{assetPath}/Plugins/multiplayer-devtools/Editor/.settings";
	    
            if (!File.Exists(_windowStatesFilePath)) return;
	    
            var serializedStates = File.ReadAllBytes(_windowStatesFilePath);
	    
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
	    
            memoryStream.Write(serializedStates, 0, serializedStates.Length);
            memoryStream.Position = 0;
		    
            var windowStates = (Dictionary<string, string>) binaryFormatter.Deserialize(memoryStream);
	    
            foreach (var windowState in windowStates)
            {
                switch (windowState.Key)
                {
                    case "_multipleEditorMode":
                        IsEnabled = windowState.Value == "True";
                        break;
			    
                    case "_playOnLaunch":
                        _playOnLaunch = windowState.Value == "True";
                        break;
			    
                    case "_containerDirPath":
                        ContainerDirPath = windowState.Value;
                        break;
			    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(windowState.Key), windowState.Key, null);
                }
            }
        }

        private static void RenderPlayerGeneratorSettings()
        {
            var instanceList = EditorInstance.InstanceList;
            
            using (new EditorGUI.DisabledScope(!IsEnabled))
            {
                _playOnLaunch = EditorInstance.PlayOnLaunch = new Field
                {
                    FieldType = Field.FieldTypes.Toggle,
                    IsToggleLeft = true,
                    Label = "Play on Launch",
                    ToggleValue = _playOnLaunch,
                    Action = SaveStates
                }.Draw(false).ToggleValue;
            }

            using (new EditorGUI.DisabledScope(instanceList?.Count > 0 || !IsEnabled))
            {
                _BaseDrawable.StartRow();
			    
                ContainerDirPath = new Field
                {
                    FieldType = Field.FieldTypes.Text,
                    Label = "Container Directory",
                    Value = ContainerDirPath
                }.Draw(false).Value;
			    
                _BaseDrawable.EndRow();
                
                _BaseDrawable.StartRow();
                _BaseDrawable.FlexibleSpace();
			    
                ContainerDirPath = new Button
                {
                    Title = "Choose",
                    ButtonColor = Color.white,
                    Width = 100,
                    Panel = new Dictionary<string, string>
                    {
                        { "title", "Browser Directories" },
                        { "folder", ContainerDirPath },
                        { "defaultName", "" }
                    },
                    PanelSelectedItem = ContainerDirPath
                }.Draw().PanelSelectedItem;
			    
                _BaseDrawable.EndRow();
            }
        }
    }
}
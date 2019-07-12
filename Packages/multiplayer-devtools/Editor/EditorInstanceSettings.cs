using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using MultiPlayerDevTools.Drawables;
using UnityEditor.Hardware;
using UnityEngine;

namespace MultiPlayerDevTools
{
    public sealed class EditorInstanceSettings
    {
        public Menu.PopupElement[] RemoteDevicePopupList { get; private set; }
        public bool HasDeviceSelected => UnityRemoteDevice != "None";
        public bool HasSocialId => SocialId != "";

        public string UnityRemoteDevice
        {
            get {
                Load();
                
                return _instanceRemoteDeviceList != null && 
                       _instanceRemoteDeviceList.ContainsKey(_instanceId) 
                    ? _instanceRemoteDeviceList[_instanceId]
                    : "None";
            }
        }

        public string SocialId
        {
            get
            {
                Load();
                
                return _instanceSocialId != null && 
                       _instanceSocialId.ContainsKey(_instanceId) 
                    ? _instanceSocialId[_instanceId]
                    : "";
            }
        }

        private readonly int _instanceId;
        private readonly Dictionary<int, string> _instanceRemoteDeviceList;
        private readonly Dictionary<int, string> _instanceSocialId;
        private readonly string _settingsPath;
    //    private SerializedObject _settings;
    //    private SerializedProperty _unityRemoteDevice;
    //    private Dictionary<string, string> _settingsData;
        
        private static int _selectedInstanceId;
        private DevDevice[] _remoteDeviceList;
        private readonly Dictionary<string, _BaseDrawable.Notification> _notifications;

        internal EditorInstanceSettings(int instanceId, Dictionary<string, _BaseDrawable.Notification> notifications = null)
        {
            _instanceId = instanceId;
            _notifications = notifications;
            _instanceRemoteDeviceList = new Dictionary<int, string>();
            _instanceSocialId = new Dictionary<int, string>();
            _settingsPath = $"Library/EditorInstanceSettings_{_instanceId}.asset";
        }
        
        public void Load()
        {
            if (!File.Exists(_settingsPath)) return;
	        
            var serializedStates = File.ReadAllBytes(_settingsPath);
            
            if(serializedStates.Length == 0) return;
	        
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
	        
            memoryStream.Write(serializedStates, 0, serializedStates.Length);
            memoryStream.Position = 0;
		        
            var settingsState = (Dictionary<string, string>) binaryFormatter.Deserialize(memoryStream);
            
            foreach (var setting in settingsState)
            {
                switch (setting.Key)
                {
                    case "UnityRemoteDevice":
                        _instanceRemoteDeviceList[_instanceId] = setting.Value;
                        break;
                    
                    case "SocialId":
                        _instanceSocialId[_instanceId] = setting.Value;
                        break;
			        
                    default:
                        throw new ArgumentOutOfRangeException(nameof(setting.Key), setting.Key, null);
                }
            }
        }
        
        private void Save(bool async = false)
        {
            var settings = new Dictionary<string, string>
            {
                ["UnityRemoteDevice"] = _instanceRemoteDeviceList.ContainsKey(_instanceId)
                    ? $"{_instanceRemoteDeviceList[_instanceId]}"
                    : null,
                ["SocialId"] = _instanceSocialId.ContainsKey(_instanceId) 
                    ? $"{_instanceSocialId[_instanceId]}" 
                    : null
            };
            
            var serializedStates = SerializeStates(settings);
            
            if (async)
            {
                SaveAsync(_settingsPath, serializedStates).Wait();
            }
            else
            {
                File.WriteAllBytes(_settingsPath, serializedStates);
            }
        }

    //    public EditorInstanceSettings Load()
    //    {
    ////        Debug.Log("_settingsPath: " + _settingsPath);
    //        
    //        var settingsObjects = InternalEditorUtility.LoadSerializedFileAndForget(_settingsPath);
    //        
    //        foreach (var settingsObject in settingsObjects)
    //        {
    //            if (settingsObject is EditorUserSettings)
    //            {
    //                Debug.Log("settingsObject: " + settingsObject);
    //
    //                _settings = new SerializedObject(settingsObject);
    //                
    //                Debug.Log("settingsObject: " + _settings.targetObject);
    //            }
    //        }
    //
    ////        var configSettings = _settings.FindProperty("m_ConfigSettings").serializedObject;
    ////        _unityRemoteDevice = _settings;
    //        
    ////        Debug.Log("configSettings: " + configSettings);
    //
    //        _unityRemoteDevice = _settings.FindProperty("m_ConfigSettings");
    //        
    //        Debug.Log("_unityRemoteDevice: " + _unityRemoteDevice.serializedObject.FindProperty("UnityRemoteDevice"));
    //        
    //
    //        return this;
    //    }
        
    //    public void Save()
    //    {
    //        Debug.Log($"_settings.targetObject: {_settings.targetObject}");
    //        Debug.Log($"EditorUtility.IsDirty(_settings.targetObject): {EditorUtility.IsDirty(_settings.targetObject)}");
            
    //        if (_settings.targetObject != null /*&& EditorUtility.IsDirty(_settings.targetObject)*/)
    //        {
    //            InternalEditorUtility.SaveToSerializedFileAndForget(
    //                new[] { _settings.targetObject }, 
    //                _settingsPath, 
    //                true
    //            );
    //        }
    //    }

        public void SetUnityRemoteDevice(object data)
        {
            _instanceRemoteDeviceList[_instanceId] = _remoteDeviceList[(int)data].id;
            
            const string notificationKey = "Device";

            if (_instanceRemoteDeviceList[_instanceId] != "None" && _notifications.ContainsKey(notificationKey))
            {
                _notifications.Remove(notificationKey);
            }
            
            _selectedInstanceId = _instanceId;

            Save();
    //        _settings.ApplyModifiedProperties();
    //        _settings.Update();
    //        EditorUtility.SetDirty(_settings.targetObject);
        }

        public void SetSocialId(object[] data)
        {
            _instanceSocialId[_instanceId] = (string) data[0];
            
            const string notificationKey = "SocialId";

            if (_instanceSocialId[_instanceId] != "" && _notifications.ContainsKey(notificationKey))
            {
                _notifications.Remove(notificationKey);
            }

            _selectedInstanceId = _instanceId;
            
            Save();
        }
        
        public int GetDeviceIndex(int instanceId, int defaultIndex = 0)
        {
            if (instanceId == _selectedInstanceId)
            {
                for (var i = 0; i < _remoteDeviceList.Length; i++)
                {
//                    RemoteDevicePopupList[i].Disabled = false;
                    
                    if (_instanceRemoteDeviceList != null &&
                        _instanceRemoteDeviceList.ContainsKey(_instanceId) &&
                        _instanceRemoteDeviceList[_instanceId] == _remoteDeviceList[i].id)
                    {
//                        RemoteDevicePopupList[i].Disabled = true;

                        return i;
                    }
                }
            }
            else
            {
                for (var i = 0; i < _remoteDeviceList.Length; i++)
                {
//                    RemoteDevicePopupList[i].Disabled = false;
                    
                    if (_instanceRemoteDeviceList != null &&
                        _instanceRemoteDeviceList.ContainsKey(instanceId) &&
                        _instanceRemoteDeviceList[instanceId] == _remoteDeviceList[i].id)
                    {
                        return i;
                    }
                }
            }
            
            if (_instanceRemoteDeviceList != null && 
                _instanceRemoteDeviceList.ContainsKey(instanceId))
            {
                _instanceRemoteDeviceList[instanceId] = "None";
                
                /*
                 * TODO: figure out why save async not working at this stage (should it be in a static context?)
                 * UPDATE: the device list now gets updated on connect (i.e. on gui)
                 * does it still require save async? 
                 */
                Save();
            }

            return defaultIndex;
        }
        
        public void BuildRemoteDeviceList()
        {
            var devices = new List<DevDevice>();
            var popupList = new List<Menu.PopupElement>();
            
            devices.Add(DevDevice.none);
            popupList.Add(new Menu.PopupElement("None"));

            // TODO: move Android stuff to editor extension
            devices.Add(
                new DevDevice(
                    "Any Android Device", 
                    "Any Android Device", 
                    "virtual", 
                    "Android", 
                    DevDeviceState.Connected, 
                    DevDeviceFeatures.RemoteConnection
                )
            );
            
            popupList.Add(new Menu.PopupElement("Any Android Device"));

            foreach (var device in DevDeviceList.GetDevices())
            {
                var supportsRemote = (device.features & DevDeviceFeatures.RemoteConnection) != 0;
                
                if (!device.isConnected || !supportsRemote)
                {
                    continue;
                }

                var popupElement = new Menu.PopupElement(device.name);
                
                devices.Add(device);
                popupList.Add(popupElement);
            }

            _remoteDeviceList = devices.ToArray();
            RemoteDevicePopupList = popupList.ToArray();
        }
        
        private static Task SaveAsync(string filePath, byte[] states)
        {
            return WriteAsync(filePath, states);
        }

        private static async Task WriteAsync(string filePath, byte[] states)
        {
            using(var stream = new FileStream(filePath, 
                FileMode.Create, FileAccess.Write, FileShare.None, 
                4096, true))
            {
                await stream.WriteAsync(states, 0, states.Length);
            }
        }

        private static byte[] SerializeStates(Dictionary<string, string> states)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
	        
            binaryFormatter.Serialize(memoryStream, states);
	        
            return memoryStream.ToArray();
        }
        
    //    private void ShowUnityRemoteGUI(bool editorEnabled)
    //    {
    //        GUI.enabled = true;
    //        GUILayout.Label(Content.unityRemote, EditorStyles.boldLabel);
    //        GUI.enabled = editorEnabled;
    //
    //        // Find selected device index
    //        string id = EditorSettings.unityRemoteDevice;
    //        // We assume first device to be "None", and default to it, hence 0
    //        int index = GetIndexById(remoteDeviceList, id, 0);
    //
    //        var content = new GUIContent(RemoteDevicePopupList[index].content);
    //        var popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);
    //        popupRect = EditorGUI.PrefixLabel(popupRect, 0, Content.device);
    //        if (EditorGUI.DropdownButton(popupRect, content, FocusType.Passive, EditorStyles.popup))
    //            DoPopup(popupRect, RemoteDevicePopupList, index, SetUnityRemoteDevice);
    //
    //        int compression = GetIndexById(remoteCompressionList, EditorSettings.unityRemoteCompression, 0);
    //        content = new GUIContent(remoteCompressionList[compression].content);
    //        popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);
    //        popupRect = EditorGUI.PrefixLabel(popupRect, 0, Content.compression);
    //        if (EditorGUI.DropdownButton(popupRect, content, FocusType.Passive, EditorStyles.popup))
    //            DoPopup(popupRect, remoteCompressionList, compression, SetUnityRemoteCompression);
    //
    //        int resolution = GetIndexById(remoteResolutionList, EditorSettings.unityRemoteResolution, 0);
    //        content = new GUIContent(remoteResolutionList[resolution].content);
    //        popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);
    //        popupRect = EditorGUI.PrefixLabel(popupRect, 0, Content.resolution);
    //        if (EditorGUI.DropdownButton(popupRect, content, FocusType.Passive, EditorStyles.popup))
    //            DoPopup(popupRect, remoteResolutionList, resolution, SetUnityRemoteResolution);
    //
    //        int joystickSource = GetIndexById(remoteJoystickSourceList, EditorSettings.unityRemoteJoystickSource, 0);
    //        content = new GUIContent(remoteJoystickSourceList[joystickSource].content);
    //        popupRect = GUILayoutUtility.GetRect(content, EditorStyles.popup);
    //        popupRect = EditorGUI.PrefixLabel(popupRect, 0, Content.joystickSource);
    //        if (EditorGUI.DropdownButton(popupRect, content, FocusType.Passive, EditorStyles.popup))
    //            DoPopup(popupRect, remoteJoystickSourceList, joystickSource, SetUnityRemoteJoystickSource);
    //    }
    }
}
    
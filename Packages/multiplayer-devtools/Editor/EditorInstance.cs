using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MultiPlayerDevTools.Drawables;
using MultiPlayerDevTools.Views;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MultiPlayerDevTools
{
    public class EditorInstance
    {
        public bool FoldOut { get; set; }
        public string LaunchBtnText { get; set; }
        public Color LaunchBtnColor { get; set; }
        public string RemoveBtnText { get; set; }
        public Color RemoveBtnColor { get; set; }

        public static Color AddButtonColor { get; set; }
        public static Color RemoveAllButtonColor { get; set; }

        public EditorInstanceSettings InstanceSettings { get; set; }

        public Dictionary<string, _BaseDrawable.Notification> Notifications { get; set; }

        public static Dictionary<string, string> CreateConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Add Player Editor Instance?"},
            {"message", $"Are you sure you want to create a new instance at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static Dictionary<string, string> LaunchAllConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Launch All Player Editor Instances?"},
            {"message", $"Are you sure you want to launch all instances located at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static Dictionary<string, string> LaunchSelectedConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Launch Selected Player Editor Instances?"},
            {"message", $"Are you sure you want to launch the selected instances located at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static Dictionary<string, string> TerminateAllConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Terminate All Player Editor Instances?"},
            {"message", $"Are you sure you want to terminate all instances located at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static Dictionary<string, string> TerminateSelectedConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Terminate Selected Player Editor Instances?"},
            {"message", $"Are you sure you want to terminate the selected instances located at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static Dictionary<string, string> RemoveSelectedConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Remove Selected Player Editor Instances?"},
            {"message", $"Are you sure you want to remove the selected instances located at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static Dictionary<string, string> RemoveAllConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Remove All Player Editor Instances?"},
            {"message", $"Are you sure you want to remove all instances located at: \n\n {ProjectCloneDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public Dictionary<string, string> LaunchConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Launch Player Editor Instance?"},
            {"message", $"Are you sure you want to launch \n \"{Name}\" located at: \n\n {_instanceDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public Dictionary<string, string> TerminateConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Terminate Player Editor Instance?"},
            {"message", $"Are you sure you want to terminate \n \"{Name}\" located at: \n\n {_instanceDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
            
        public Dictionary<string, string> RemoveConfirmDialog => new Dictionary<string, string>
        {
            {"title", "Remove Player Editor Instance?"},
            {"message", $"Are you sure you want to remove \n \"{Name}\" located at: \n\n {_instanceDirectory}\n"},
            {"ok", "YES"},
            {"cancel", "NO!"}
        };
        public static string ProjectCloneDirectory { get; set; }
        public static List<EditorInstance> InstanceList { get; set; }
        public static int InstanceListCount => GetInstanceDirectories().Count();

        public bool IsSelected { get; set; }
        public bool IsRunning => Directory.Exists($"{_instanceDirectory}/Temp");
        
        public int Id { get; }
        public string Name { get; }

        public static bool PlayOnLaunch { get; set; }
        
        private GUIStyle _buttonStyle;
        
        private const string SymLinkedFlagFile = ".__symLinked__";
        private readonly string _instanceDirectory;

        private static Dictionary<string, string> _instanceSettingsPaths;
        private Process _process;
        private static bool _isReadyToLaunch;
        
        private EditorInstance(string projectName, int instanceCount, bool onEnable = false)
        {
            try
            {
                InstanceList = InstanceList ?? new List<EditorInstance>();
                _instanceSettingsPaths = _instanceSettingsPaths ?? new Dictionary<string, string>();

                DirectoryInfo existingInstanceDir = null;

                if (onEnable)
                {
                    existingInstanceDir = GetInstanceDirectories().ToArray()[instanceCount];
                }

                _instanceDirectory = existingInstanceDir?.FullName ??
                                     $"{ProjectCloneDirectory}/{projectName}_instance_{instanceCount + 1}";

                if (InstanceList.Any(instance => instance._instanceDirectory == _instanceDirectory))
                {
                    instanceCount = 0;
                }

                while (InstanceList.Any(instance => instance._instanceDirectory == _instanceDirectory))
                {
                    instanceCount++;

                    _instanceDirectory = $"{ProjectCloneDirectory}/{projectName}_instance_{instanceCount + 1}";
                }

                var instanceDirName = _instanceDirectory.Split('/').Last();

                Id = int.Parse(instanceDirName.Split('_').Last());
                Name = instanceDirName.Replace("_", " ");

                Notifications = new Dictionary<string, _BaseDrawable.Notification>();
            }
            catch (FormatException exc)
            {
                Debug.LogException(exc);
            }
            catch(NullReferenceException exc)
            {
                Debug.LogException(exc);
            }
        }
        
        public static void Create(object[] args)
        {
            var projectName = (string) args[0];
            var instanceCount = (int) args[1];
            var onEnable = args.Length > 2 && (bool) args[2];
            
            try
            {
                Create(projectName, instanceCount, onEnable);
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
            }
        }

        public static void Reset()
        {
            InstanceList?.Clear();
            _instanceSettingsPaths?.Clear();
        }

        public void Remove()
        {
            if(string.IsNullOrEmpty(_instanceDirectory) || 
               _instanceDirectory == Environment.CurrentDirectory ||
               !Directory.Exists(_instanceDirectory)) return;
            
            Directory.Delete(_instanceDirectory, true);
            
            RemoveSettings(_instanceDirectory);
            InstanceList.Remove(this);
        }
        
        public static void RemoveSelected()
        {
            foreach (var instance in InstanceList.ToList())
            {
                if (instance.IsSelected && !instance.IsRunning)
                {
                    instance.Remove();
                }
            }
        }
        
        public void Launch()
        {
            if(string.IsNullOrEmpty(_instanceDirectory) || 
               _instanceDirectory == Environment.CurrentDirectory ||
               !Directory.Exists(_instanceDirectory) || 
               IsRunning ||
               !_isReadyToLaunch) return;
            
            Environment.SetEnvironmentVariable("GITHUB_UNITY_DISABLE", "1");

            Action executeMethod = Settings.SetInstanceData;
            
            var openProjectCommand = $"{EditorApplication.applicationContentsPath}/MacOS/Unity " +
                                     $"-projectPath {_instanceDirectory} " +
                                     $"-executeMethod {typeof(Settings)}.{executeMethod.Method.Name}";
            
            ExecuteCommand(openProjectCommand, true);
        }
        
        public void ValidateSingleLaunch()
        {
            _isReadyToLaunch = true;
            FoldOut = false;
                
            Notifications.Clear();

            if (InstanceSettings.HasDeviceSelected) return;
            
            FoldOut = true;
            _isReadyToLaunch = false;

            Notifications["Device"] = new _BaseDrawable.Notification(
                $"Remote device not selected!", 
                MessageType.Error
            );

            GUI.FocusControl($"{Id}:Device");
        }
        
        public static void ValidateMultipleLaunches()
        {
            var nonlaunchableInstances = InstanceList.Select(instance => 
            {
                _isReadyToLaunch = true;
                instance.FoldOut = false;
                
                instance.Notifications.Clear();

                if (instance.IsSelected && !instance.InstanceSettings.HasDeviceSelected)
                {
                    return instance;
                }
                
                return null;
            }).Where(instance => instance != null).OrderBy(instance => instance.Id).ToArray();
            
            foreach (var instance in nonlaunchableInstances)
            {
                instance.FoldOut = true;
                _isReadyToLaunch = false;
                
                instance.Notifications["Device"] = new _BaseDrawable.Notification(
                    $"Remote device not selected!", 
                    MessageType.Error
                );

                if (instance == nonlaunchableInstances[0])
                {
                    GUI.FocusControl($"{instance.Id}:Device");
                }
            }
        }
        
        public static void LaunchSelected()
        {
            foreach (var instance in InstanceList)
            {
                if (!instance.IsSelected || instance.IsRunning || !_isReadyToLaunch) continue;
                
                instance.Launch();
                WaitForLaunchToComplete();
            }
        }

        public void Terminate()
        {
            if(string.IsNullOrEmpty(_instanceDirectory) || 
               _instanceDirectory == Environment.CurrentDirectory ||
               !Directory.Exists(_instanceDirectory)) return;
            
            _process.Kill();
            Directory.Delete($"{_instanceDirectory}/Temp", true);
        }
        
        public static void TerminateSelected()
        {
            foreach (var instance in InstanceList)
            {
                if (instance.IsSelected && instance.IsRunning)
                {
                    instance.Terminate();
                }
            }
        }
        
        public void SetButtonStyles()
        {
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                normal = {textColor = Color.red}, 
                onNormal = {textColor = Color.green}
            };

            if (IsRunning)
            {
                LaunchBtnText = "Terminate";
                LaunchBtnColor = new Color(1f, 0.57f, 0f);

                RemoveBtnText = "Remove";
                RemoveBtnColor = Color.red;
            }
            else
            {
                LaunchBtnText = "Launch";
                LaunchBtnColor = Color.white;

                RemoveBtnText = "Remove";
                RemoveBtnColor = Color.yellow;
            }
        }
        
        private static void WaitForLaunchToComplete()
        {
            var editorLogPath = "";
            var currentPlatform = Application.platform;
            var homeDirPath = currentPlatform == RuntimePlatform.OSXEditor || currentPlatform == RuntimePlatform.LinuxEditor
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (currentPlatform)
            {
                case RuntimePlatform.OSXEditor:
                    editorLogPath = $@"{homeDirPath}/Library/Logs/Unity/Editor.log";
                    break;
                case RuntimePlatform.LinuxEditor:
                    editorLogPath = $@"{homeDirPath}/.config/unity3d/Editor.log";
                    break;
                case RuntimePlatform.WindowsEditor:
                    editorLogPath = $@"C:\{homeDirPath}\AppData\Local\Unity\Editor\Editor.log";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            while (!File.Exists(editorLogPath) || !IsFileReady(editorLogPath)) {}
        }
        
        private static bool IsFileReady(string filename)
        {
            try
            {
                using (var inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private static IEnumerable<DirectoryInfo> GetInstanceDirectories()
        {
            return new DirectoryInfo(ProjectCloneDirectory).EnumerateDirectories();
        }
        
        private static void Create(string projectName, int instanceCount, bool onEnable = false)
        {
            var editorInstance = new EditorInstance(projectName, instanceCount, onEnable);
            
            InstanceList.Add(editorInstance);
            
            if(string.IsNullOrEmpty(editorInstance._instanceDirectory) || 
               editorInstance._instanceDirectory == Environment.CurrentDirectory) return;

            if (!onEnable)
            {
                Directory.CreateDirectory(editorInstance._instanceDirectory);
            
                CreateFiles(editorInstance);
            }
            
            CreateSettings(editorInstance, onEnable);
        }

        private static void CreateFiles(EditorInstance editorInstance)
        {
            var instanceProperties = new[]
            {
                SymLinkedFlagFile, "*.csproj",
                "Assets", "Library", "Logs", "Packages", "ProjectSettings"
            };

            foreach (var property in instanceProperties)
            {
                if (property == SymLinkedFlagFile)
                {
                    File.Create($"{editorInstance._instanceDirectory}/{property}").Dispose();
                }
                else
                {
                    var command = $"ln -s {Environment.CurrentDirectory}/{property} {editorInstance._instanceDirectory}";
                        
                    ExecuteCommand(command);
                }
            }
        }

        private static void CreateSettings(EditorInstance editorInstance, bool onEnable = false)
        {
            const string settingExtension = "asset";
            
            var settings = new[]
            {
                "Library/EditorUserSettings"
            };
            
            foreach (var setting in settings)
            {
    //            var originalSettingsPath = $"{Environment.CurrentDirectory}/{setting}.{settingExtension}";
                var instanceSettingsPath = $"{Environment.CurrentDirectory}/Library/EditorInstanceSettings_{editorInstance.Id}.{settingExtension}";

                if (!onEnable)
                {
    //                File.Copy(originalSettingsPath, instanceSettingsPath);
                    File.Create(instanceSettingsPath).Dispose();
                }
                
                _instanceSettingsPaths.Add(editorInstance._instanceDirectory, instanceSettingsPath);
            }
            
            editorInstance.InstanceSettings = new EditorInstanceSettings(editorInstance.Id, editorInstance.Notifications);
            editorInstance.InstanceSettings.Load();
        }
        
        private static void RemoveSettings(string settingKey)
        {
            File.Delete(_instanceSettingsPaths[settingKey]);
            _instanceSettingsPaths.Remove(settingKey);
        }
        
        private static Process ExecuteCommand(string command)
        {
    //        var startInfo = new ProcessStartInfo(cmd, args)
    //        {
    //            UseShellExecute = true, 
    //            RedirectStandardError = true
    //        };

            try
            {
                return Process.Start("/bin/bash", $"-c \"" + command + "\"");
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
        }

        private void ExecuteCommand(string command, bool isNonStatic)
        {
            if (isNonStatic)
            {
                _process = ExecuteCommand(command);
            }
        }
    }
}
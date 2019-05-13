using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MultiPlayerDevTools.Drawables;
using MultiPlayerDevTools.Views;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MultiPlayerDevTools
{
	public enum Tabs
	{
		PlayerGenerator, 
		NetworkAnalysis, 
		Settings
	}
	
	[InitializeOnLoad]
	public class DevToolsWindow : EditorWindow
	{
		private Tabs _editorTab;
		private Settings _settings;
		private PlayerGenerator _playerGenerator;
		
		private int _selectedDeviceIndex;
		
		private const string SymLinkedFlag = ".__symLinked__";
	    
	    static DevToolsWindow()
	    {
		    var loadedInstanceDirPath = Environment.CurrentDirectory;
		    
		    if(!File.Exists($"{loadedInstanceDirPath}/{SymLinkedFlag}")) return;
		    
		    var projectCloneDirPath = Directory.GetParent(loadedInstanceDirPath).FullName;
		    var instanceDirectories = new DirectoryInfo(projectCloneDirPath).EnumerateDirectories();
		    
		    foreach (var instanceDirectory in instanceDirectories)
		    {
			    if (instanceDirectory.FullName != loadedInstanceDirPath) continue;
			    
			    var instanceId = int.Parse(instanceDirectory.Name.Split('_').Last());
			    
			    var editorInstanceSettings = new EditorInstanceSettings(instanceId);
			    
			    EditorSettings.unityRemoteDevice = editorInstanceSettings.UnityRemoteDevice;
		    }
	    }
	    
	    public DevToolsWindow()
	    {
		    titleContent = new GUIContent("MP DevTools");
	    }
	    
	    [MenuItem ("Window/Multi-player DevTools %#d")]
	    private static void Init()
	    {
		    GetWindow(typeof(DevToolsWindow));
	    }
	    
	    [MenuItem("Window/Multi-player DevTools %#d", true)]
	    private static bool ToggleMenu()
	    {
		    var isSymLinked = new DirectoryInfo(Directory.GetCurrentDirectory())
															   .EnumerateFiles(SymLinkedFlag)
															   .Any();
		    
		    return !isSymLinked;
	    }
	    
	    [MenuItem("Edit/Project Settings", true)]
	    private static bool ToggleProjectSettingsMenu()
	    {
		    var isSymLinked = new DirectoryInfo(Directory.GetCurrentDirectory())
			    .EnumerateFiles(SymLinkedFlag)
			    .Any();
		    
		    return !isSymLinked;
	    }
	    
	    private void OnEnable()
	    {
		    try
		    {
			    _settings = new Settings();
			    _playerGenerator = new PlayerGenerator();
			    
			    Settings.LoadStates();
			    _playerGenerator.SetInstanceList();
		    }
		    catch (Exception exc)
		    {
			    Debug.LogException(exc);
		    }
	    }

	    private void OnDisable()
	    {
		    Settings.SaveStates();
		    EditorInstance.Reset();
	    }
	    
	    private void OnGUI()
	    {
		    var editorTabNames = Enum.GetValues(typeof(Tabs))
			    .Cast<Tabs>()
			    .Select(tab => Regex.Replace($"{tab}", "([A-Z])", " $1", RegexOptions.Compiled).Trim())
			    .ToArray();
		    
		    _BaseDrawable.StartRow();
		    
		    _editorTab = (Tabs) GUILayout.Toolbar ((int) _editorTab, editorTabNames, EditorStyles.toolbarButton);
		    
		    _BaseDrawable.EndRow();
		    
		    EditorGUILayout.Space();
		    EditorGUILayout.Space();

		    switch (_editorTab) 
		    {
			    case Tabs.PlayerGenerator:
				    _playerGenerator.Render();
				    break;
			    case Tabs.NetworkAnalysis:
				    break;
			    case Tabs.Settings:
				    _settings.Render();
				    break;
			    default:
				    throw new ArgumentOutOfRangeException();
		    }
		    
		    if (GUI.changed)
		    {
			    EditorUtility.SetDirty(this);
		    }
	    }
	}
}

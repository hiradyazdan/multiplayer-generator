using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;

using MultiPlayerDevTools.Drawables;
using MultiPlayerDevTools.Views;

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
		private static bool IsSymLinked => Directory
			.EnumerateFiles(Directory.GetCurrentDirectory(), SymLinkedFlag)
			.Any();
		
		private Tabs _tab;
		private Settings _settings;
		private PlayerGenerator _playerGenerator;
		
		private int _selectedDeviceIndex;
		
		private const string SymLinkedFlag = ".__symLinked__";
		
		private static string currentDirPath;
		
		static DevToolsWindow()
		{
			try
			{
				SetEditorInstanceSettings();
			}
			catch (UnityException)
			{}
		}
		
		[MenuItem ("Window/Multi-player DevTools %#d")]
	    private static void Init()
	    {
		    var inspectorWindow = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		    
		    GetWindow<DevToolsWindow>("MP DevTools", true, inspectorWindow);
	    }
	    
	    [MenuItem("Window/Multi-player DevTools %#d", true)]
	    private static bool ToggleMenu()
	    {
		    return !IsSymLinked;
	    }
	    
	    private void OnEnable()
	    {
		    if (IsSymLinked)
		    {
			    Close();
			    return;
		    }
		    
		    try
		    {
			    _settings = new Settings();
			    _playerGenerator = new PlayerGenerator
			    {
				    RepaintWindow = Repaint
			    };

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
		    if(IsSymLinked) return;
		    
		    Settings.SaveStates();
		    EditorInstance.Reset();
	    }
	    
	    private void OnGUI()
	    {
		    if(IsSymLinked) return;
		    
		    var editorTabNames = Enum.GetValues(typeof(Tabs))
			    .Cast<Tabs>()
			    .Select(tab => Regex.Replace($"{tab}", "([A-Z])", " $1", RegexOptions.Compiled).Trim())
			    .ToArray();
		    
		    _BaseDrawable.StartRow();
		    
		    _tab = (Tabs) GUILayout.Toolbar ((int) _tab, editorTabNames, EditorStyles.toolbarButton);
		    
		    _BaseDrawable.EndRow();
		    
		    EditorGUILayout.Space();
		    EditorGUILayout.Space();
		    
		    switch (_tab) 
		    {
			    case Tabs.PlayerGenerator:
				    _playerGenerator.Render(position);
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
	    
	    private static void SetEditorInstanceSettings()
	    {
		    currentDirPath = Environment.CurrentDirectory;
		    
		    var isMasterEditor = !File.Exists($"{currentDirPath}/{SymLinkedFlag}");
		    
		    if (isMasterEditor) return;
		    
		    var projectCloneDirPath = Directory.GetParent(currentDirPath).FullName;
		    var instanceDirectories = new DirectoryInfo(projectCloneDirPath).EnumerateDirectories();
		    
		    foreach (var instanceDirectory in instanceDirectories)
		    {
			    if (instanceDirectory.FullName != currentDirPath) continue;
			    
			    var instanceId = int.Parse(instanceDirectory.Name.Split('_').Last());
			    
			    var editorInstanceSettings = new EditorInstanceSettings(instanceId);
			    
			    EditorSettings.unityRemoteDevice = editorInstanceSettings.UnityRemoteDevice;
			    EditorSettings.unityRemoteResolution = "Normal";
			    
			    Social.localUser.SetInstanceField("m_ID", editorInstanceSettings.SocialId);
		    }
	    }
	}
}

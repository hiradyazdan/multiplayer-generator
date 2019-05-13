using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiPlayerDevTools.Drawables;
using UnityEditor;
using UnityEngine;
using Menu = MultiPlayerDevTools.Drawables.Menu;

namespace MultiPlayerDevTools.Views
{
	public class PlayerGenerator
	{
		private static string assetPath => Application.dataPath;
		private static string _windowStatesFilePath;
		
		private string _projectName;

	    private int _selectedDeviceIndex;

	    private _BaseDrawable _drawable;

	    private Vector2 _scrollPosition;
	    
	    public PlayerGenerator()
	    {
		    
	    }

	    public PlayerGenerator Render()
	    {
		    using (new EditorGUI.DisabledScope(!Settings.IsEnabled))
		    {
			    var instanceList = EditorInstance.InstanceList;
			    var hasRunningInstance = instanceList != null && instanceList.Any(instance => instance.IsRunning);

			    _BaseDrawable.StartRow();

			    /**
			     * Add/Create Button
			     */
			    new Button
			    {
				    Title = "Add a New Player",
				    ButtonColor = Color.white,
				    FitWindowWidth = true,
				    Dialog = EditorInstance.CreateConfirmDialog,
	//			    BeforeFunction = SetUpCreateConfirmDialog,
				    ActionWithArgs = EditorInstance.Create,
				    ActionArgs = new object[] { (Func<Dictionary<string, string>>) SetUpCreateConfirmDialog, _projectName, instanceList?.Count ?? 0 }
			    }.Draw();
			    
			    _BaseDrawable.EndRow();
			    
			    _BaseDrawable.DrawHorizontalLine(Color.black);
			    
			    _BaseDrawable.StartRow();

			    var selectedInstanceCount = instanceList?.Where(instance => instance.IsSelected).ToList().Count ?? 0;
			    var hasSelectedInstance = instanceList?.Any(instance => instance.IsSelected) ?? false;
			    var hasAllSelected = instanceList?.Count > 0 && instanceList.Count == selectedInstanceCount;
			    
			    var selectedAllText = hasAllSelected ? "All" : "Selected";
			    var launchBtnTitle = !hasRunningInstance 
				    ? $"Launch {selectedAllText} ({selectedInstanceCount})" 
				    : $"Terminate {selectedAllText} ({selectedInstanceCount})";
			    var removeBtnTitle = $"Remove {selectedAllText} ({selectedInstanceCount})";

			    var launchBtnDialog = !hasRunningInstance
				    ? hasAllSelected ? EditorInstance.LaunchAllConfirmDialog : EditorInstance.LaunchSelectedConfirmDialog
				    : hasAllSelected ? EditorInstance.TerminateAllConfirmDialog : EditorInstance.TerminateSelectedConfirmDialog;
			    var removeBtnDialog = hasAllSelected 
				    ? EditorInstance.RemoveAllConfirmDialog 
				    : EditorInstance.RemoveSelectedConfirmDialog;

			    /**
			     * Launch Selected/Terminate Selected Button
			     */
			    new Button
			    {
				    Title = launchBtnTitle,
				    ButtonColor = !hasRunningInstance ? new Color(0.12f, 0.57f, 0f) : new Color(1f, 0.57f, 0f),
				    FitWindowWidth = true,
				    Disabled = instanceList?.Count == 0 || !hasSelectedInstance,
				    Dialog = launchBtnDialog,
				    BeforeAction = !hasRunningInstance ? (Action) EditorInstance.ValidateMultipleLaunches : null,
				    Action = !hasRunningInstance ? (Action) EditorInstance.LaunchSelected : EditorInstance.TerminateSelected
			    }.Draw(true);
			    
			    /**
			     * Remove Selected Button
			     */
			    new Button
			    {
				    Title = removeBtnTitle,
				    ButtonColor = !hasRunningInstance ? Color.yellow : Color.red,
				    FitWindowWidth = true,
				    Disabled = instanceList?.Count == 0 || hasRunningInstance || !hasSelectedInstance,
				    Dialog = removeBtnDialog,
				    Action = EditorInstance.RemoveSelected
			    }.Draw(true);
			    
			    _BaseDrawable.EndRow();
			    
			    _BaseDrawable.DrawHorizontalLine(Color.black);
			    
			    RenderInstanceListHeader(instanceList, instanceList == null || instanceList.Count == 0 || hasRunningInstance);
			    
			    var instances = instanceList?.OrderBy(instance => instance.Id).ToList();

			    if(instances == null) return this;
			    
			    _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			    foreach (var instance in instances)
			    {
				    instance.SetButtonStyles();
				    
				    RenderInstanceListItem(instance);
			    }
			    GUILayout.EndScrollView();
			    
			    EditorGUILayout.Space();
			    _BaseDrawable.DrawHorizontalLine(Color.black);
			    EditorGUILayout.Space();
		    }
		    
		    return this;
	    }
	    
	    public void SetInstanceList()
	    {
		    SetUpDirectories();
		    
			for (var i = 0; i < EditorInstance.InstanceListCount; i++)
		    {
			    EditorInstance.Create(new object[] { _projectName, i, true });
		    }
	    }

	    private Dictionary<string, string> SetUpCreateConfirmDialog()
	    {
		    SetUpDirectories();
		    
		    return EditorInstance.CreateConfirmDialog;
	    }
	    
	    private void SetUpDirectories()
	    {
		    char[] charsToTrim = {'/'};
		    
		    var assetPathArr = assetPath.Split('/');
		    var projectCloneDirPath = EditorInstance.ProjectCloneDirectory;
		    var editorDirPath = AppDomain.CurrentDomain.BaseDirectory;

		    _projectName = assetPathArr[assetPathArr.Length - 2];

		    // On Enable & On Create
		    if (projectCloneDirPath != null && Settings.ContainerDirPath != null)
		    {
			    var oldContainerDirPath = projectCloneDirPath.Replace(_projectName, "");

			    if (Settings.ContainerDirPath.TrimEnd(charsToTrim) != oldContainerDirPath.TrimEnd(charsToTrim))
			    {
				    Directory.Move(oldContainerDirPath, Settings.ContainerDirPath);
			    }
		    }
		    
		    Settings.ContainerDirPath = Settings.ContainerDirPath ?? Path.Combine(editorDirPath, "___MultipleEditors");
		    
		    projectCloneDirPath = Path.Combine(Settings.ContainerDirPath, _projectName);
		    
		    Directory.CreateDirectory(projectCloneDirPath);

		    EditorInstance.ProjectCloneDirectory = projectCloneDirPath;
	    }

	    private static void RenderInstanceListHeader(IReadOnlyCollection<EditorInstance> instanceList = null, bool isDisabled = false)
	    {
		    instanceList = instanceList ?? new List<EditorInstance>();
		    
		    var textures = new []
		    {
			    new Texture2D(1,1),
			    new Texture2D(1,1)
		    };
	 
		    textures[0].SetPixel(0,0, Color.black * 0.2f);
		    textures[0].Apply();

		    var blockStyle = new GUIStyle
		    {
			    padding = new RectOffset(5, 5, 0, 0),
			    margin = new RectOffset(5, 5, 0, 0),
			    normal = {
				    background = textures[0]
			    }
		    };
		    
		    _BaseDrawable.StartRow(blockStyle);

		    var labelStyle = new GUIStyle(GUI.skin.label)
		    {
			    padding = new RectOffset(5, 0, 1, 2)
		    };
		    
		    var selectedCount = instanceList.Where(instance => instance.IsSelected).ToList().Count;
		    var hasMixedSelected = selectedCount > 0 && selectedCount < instanceList.Count;
		    var isAllSelected = instanceList.Count > 0 && instanceList.All(instance => instance.IsSelected);
		    
		    new Field
		    {
			    FieldType = Field.FieldTypes.Toggle,
			    IsToggleLeft = true,
			    Label = $"{(isAllSelected ? "All" : hasMixedSelected ? $"{selectedCount}" : "None")} Selected",
			    LabelStyle = labelStyle,
			    Disabled = isDisabled,
			    ToggleValue = isAllSelected,
			    MixedToggle = hasMixedSelected,
			    ActionWithArgs = ToggleAllInstanceListItems,
			    ActionArgs = new object[] { instanceList, isAllSelected, _BaseDrawable.ToggleTypes.CheckBox }
		    }.Draw(false, true);
		    
		    var isCollapsed = instanceList.Count > 0 && instanceList.All(instance => !instance.FoldOut);
		    
		    new Button
		    {
			    Title = isCollapsed ? "Expand All" : "Collapse All",
			    ButtonColor = Color.clear,
			    TitleColor = Color.blue,
			    Width = 70,
			    Height = 14,
			    Disabled = isDisabled,
			    ActionWithArgs = ToggleAllInstanceListItems,
			    ActionArgs = new object[] { instanceList, isCollapsed, _BaseDrawable.ToggleTypes.FoldOut }
		    }.Draw(true);
		    
		    _BaseDrawable.EndRow();
	    }

	    private static void ToggleAllInstanceListItems(object[] args)
	    {
		    var instanceList = (List<EditorInstance>) args[0];
		    var toggleState = (bool) args[1];
		    var toggleType = (_BaseDrawable.ToggleTypes) args[2];
		    
		    EditorInstance.InstanceList = instanceList.Select(instance =>
		    {
			    switch (toggleType)
			    {
				    case _BaseDrawable.ToggleTypes.CheckBox:
					    instance.IsSelected = !toggleState;
					    break;
				    case _BaseDrawable.ToggleTypes.FoldOut:
					    instance.FoldOut = toggleState;
					    break;
				    default:
					    throw new ArgumentOutOfRangeException();
			    }

			    return instance;
		    }).ToList();
		    
		    // TODO: Should include toggle states to the state dictionary
		    Settings.SaveStates();
	    }
	    
	    private static void RenderInstanceListItem(EditorInstance instance)
	    {
		    var textures = new []
		    {
			    new Texture2D(1,1),
			    new Texture2D(1,1),
			    new Texture2D(1,1)
		    };
	 
		    textures[0].SetPixel(0,0, Color.black * 0.4f);
		    textures[0].Apply();
		    
		    textures[1].SetPixel(0,0, Color.black * 0.3f);
		    textures[1].Apply();
	 
		    textures[2].SetPixel(0,0, Color.clear);
		    textures[2].Apply();

		    var blockStyle = new GUIStyle
		    {
			    padding = new RectOffset(5, 5, 5, 5),
			    margin = new RectOffset(5, 5, 5, 0),
			    normal = {
				    background = textures[0]
			    }
		    };
		    
		    _BaseDrawable.StartRow(blockStyle);
		    
		    /**
		     * Toggle Field
		     */
		    instance.IsSelected = new Field
		    {
			    FieldType = Field.FieldTypes.Toggle,
			    Label = "",
			    Disabled = instance.IsRunning,
			    ToggleValue = instance.IsSelected,
			    ToggleOptions = new[] { GUILayout.MaxWidth(15) }
		    }.Draw(false, true).ToggleValue;
		    
		    instance.FoldOut = _BaseDrawable.DrawFoldOut(instance.FoldOut, instance.Name);
		    
		    _BaseDrawable.EndRow();

		    if (!instance.FoldOut) return;
		    
		    /**
		     * Foldout Section
		     */

		    var foldoutStyle = new GUIStyle
		    {
			    padding = new RectOffset(0, 0, 0, 0),
			    margin = new RectOffset(5, 5, 0, 0),
			    normal = {
				    background = textures[1]
			    }
		    };
		    
		    _BaseDrawable.StartColumn(foldoutStyle);
		    
	//	    _BaseDrawable.StartRow();
		    
		    var instanceSettings = instance.InstanceSettings;

		    instanceSettings.BuildRemoteDeviceList();
		    
		    new Menu
		    {
			    MenuType = Menu.MenuTypes.DropDown,
			    Label = "Device",
			    MenuId = instance.Id,
			    Disabled = instance.IsRunning,
			    MenuList = instanceSettings.RemoteDevicePopupList,
			    MenuAction = instanceSettings.SetUnityRemoteDevice,
			    SelectedItemIndex = instanceSettings.GetIndexById(instance.Id),
			    HelpBox = instance.Notifications.ContainsKey("Device") ? (_BaseDrawable.Notification?) instance.Notifications["Device"] : null
		    }.Draw(true, true);
		    
	//	    _BaseDrawable.EndRow();

		    _BaseDrawable.StartRow();
		    
		    /**
		     * Launch Button
		     */
		    new Button
		    {
			    Title = instance.LaunchBtnText,
			    ButtonColor = instance.LaunchBtnColor,
			    FitWindowWidth = true,
			    Dialog = !instance.IsRunning ? instance.LaunchConfirmDialog : instance.TerminateConfirmDialog,
			    BeforeAction = !instance.IsRunning ? (Action) instance.ValidateSingleLaunch : null,
			    Action = !instance.IsRunning ? (Action) instance.Launch : instance.Terminate
		    }.Draw();
		    
		    /**
		     * Remove Button
		     */
		    new Button
		    {
			    Title = instance.RemoveBtnText,
			    ButtonColor = instance.RemoveBtnColor,
			    FitWindowWidth = true,
			    Disabled = instance.IsRunning,
			    Dialog = instance.RemoveConfirmDialog,
			    Action = instance.Remove
		    }.Draw(true);
		    
		    _BaseDrawable.EndRow();
		    
		    _BaseDrawable.EndColumn();
	    }
	}
}

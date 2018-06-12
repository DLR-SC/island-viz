using UnityEngine;
using UnityEditor;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.CSharp;

public class QuickGraphEditorWindow : EditorWindow {
	#region public class methods
	[MenuItem ("Window/QuickGraph4Unity/Build DLLs")]
	public static void ShowWindow () {
		QuickGraphEditorWindow w = EditorWindow.GetWindow<QuickGraphEditorWindow>("QuickGraph4Unity", true);
		
		if (PlayerPrefs.HasKey (QuickGraphEditorWindow.PlayerPrefs_BuildBasePath)) {
			w.BuildPath = PlayerPrefs.GetString (QuickGraphEditorWindow.PlayerPrefs_BuildBasePath);
			
			DirectoryInfo directory = new DirectoryInfo(w.BuildPath);
			if (!directory.Exists){
				w.BuildPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
			}
		} else {
			w.BuildPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
		}
	}
	#endregion
	
	#region protected class properties
	protected static readonly string RuntimePath = 
	Application.dataPath	+ Path.DirectorySeparatorChar + 
	"QuickGraph4Unity"		+ Path.DirectorySeparatorChar + 
	"Runtime"				+ Path.DirectorySeparatorChar;

	protected static readonly string RuntimeDll = 
	"QuickGraph4Unity-Runtime.dll";

	protected static readonly string PlayerPrefs_BuildBasePath = 
	"BuildBasePath";

	protected static readonly string MonoDll = 
	Application.dataPath	+ Path.DirectorySeparatorChar + 
	"QuickGraph4Unity"		+ Path.DirectorySeparatorChar + 
	"Dependencies"			+ Path.DirectorySeparatorChar +
	"Mono4Unity"			+ Path.DirectorySeparatorChar +
	"Mono4Unity.dll";

	protected static readonly string CodeContractsPath = 
	Application.dataPath	+ Path.DirectorySeparatorChar + 
	"QuickGraph4Unity"		+ Path.DirectorySeparatorChar + 
	"Dependencies"			+ Path.DirectorySeparatorChar +
	"CodeContracts (Silverlight)";

	protected static readonly string CodeContractsDll = 
	"Microsoft.Contracts.dll";
	#endregion
	
	#region public instance properties
	public string BuildPath{
		get{
			return this._buildPath;
		}
		set{
			this._buildPath = value;
		}
	}
	#endregion
	
	#region public instance properties
	protected string _buildPath;
	protected bool _compiling = false;
	#endregion
	
	#region protected instance methods
	protected string[] GetSourceFiles(string sourceFolder){
		DirectoryInfo dir = new DirectoryInfo(sourceFolder);
		List<string> files = new List<string> ();
		
		if (dir.Exists) {
			foreach (FileInfo file in dir.GetFiles("*.cs", SearchOption.AllDirectories)){
				files.Add(file.FullName);
			}
		}
		
		return files.ToArray ();
	}
	#endregion
	
	#region EditorWindow methods
	public virtual void OnGUI(){
		GUILayout.Space (30);
		
		if (this._compiling) {
			GUILayout.Label ("Compiling...");
		}else{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Build Path:");
			this.BuildPath = GUILayout.TextField (this.BuildPath != null ? this.BuildPath : string.Empty);
			
			if (GUILayout.Button ("+")) {
				this.BuildPath = EditorUtility.SaveFolderPanel(
					"Select the folder where the DLLs will be created",
					string.IsNullOrEmpty(this.BuildPath)? Application.dataPath : this.BuildPath,
					string.Empty
					);
			}
			GUILayout.EndHorizontal ();
			GUILayout.Space (10);
			
			if (GUILayout.Button ("Build DLLs")) {
				if (string.IsNullOrEmpty(this.BuildPath)){
					EditorUtility.DisplayDialog(
						"Build path not selected",
						"You must select the folder where the DLLs will be created.",
						"OK"
						);
				}else{
					StringBuilder warnings = new StringBuilder();
					StringBuilder errors = new StringBuilder();
					this._compiling = true;

					string sourceCodeContractsDll = new FileInfo(
						QuickGraphEditorWindow.CodeContractsPath + 
						Path.DirectorySeparatorChar + 
						QuickGraphEditorWindow.CodeContractsDll
					).FullName;

					string destinationCodeContractsDll = new FileInfo(
						this.BuildPath + 
						Path.DirectorySeparatorChar + 
						QuickGraphEditorWindow.CodeContractsDll
					).FullName;

					string runtimeDll = new FileInfo(
						this.BuildPath + 
						Path.DirectorySeparatorChar + 
						QuickGraphEditorWindow.RuntimeDll
					).FullName;

					string unityEngineDll = 
						EditorApplication.applicationContentsPath + Path.DirectorySeparatorChar + 
							(Application.platform == RuntimePlatform.OSXEditor ? "Frameworks" : "Data") +
							Path.DirectorySeparatorChar + "Managed" +
							Path.DirectorySeparatorChar + "UnityEngine.dll";

					PlayerPrefs.SetString (
						QuickGraphEditorWindow.PlayerPrefs_BuildBasePath,
						this.BuildPath
					);

					Debug.Log("Generating DLLs...");
					string[] results = EditorUtility.CompileCSharp(
						this.GetSourceFiles(QuickGraphEditorWindow.RuntimePath),
						new string[]{
							"System.dll", 
							"mscorlib.dll", 
							QuickGraphEditorWindow.MonoDll,
							sourceCodeContractsDll,
							unityEngineDll,
						},
						new string[0],
						new FileInfo(runtimeDll).FullName
					);

					foreach (string result in results){
						if (result.Contains("warning")){
							warnings.AppendLine(result);
						}else if (result.Contains("error")){
							errors.AppendLine(result);
						}
					}

					if (warnings.Length > 0){
						Debug.LogWarning(warnings.ToString());
					}
					
					if (errors.Length > 0){
						Debug.LogError(errors.ToString());
					}else{
						try{
							File.WriteAllBytes(destinationCodeContractsDll, File.ReadAllBytes(sourceCodeContractsDll));
							Debug.Log("The DLLs were generated successfully"); 
						}catch(Exception e){
							Debug.LogError(e.Message + "\n" + e.StackTrace);
						}

					}
					
					this._compiling = false;
				}
			}
		}
	}
	#endregion
}
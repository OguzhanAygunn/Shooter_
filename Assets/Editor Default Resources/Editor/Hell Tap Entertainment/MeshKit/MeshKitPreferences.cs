
/*
This asset was uploaded by https://unityassetcollection.com
*/

////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitPreferences.cs
//
//	Unity Preferences For MeshKit.
//
//	© 2020 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshKitPreferences {

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	PREFERENCES FIX FOR UNITY 2019.3 AND HIGHER
		////////////////////////////////////////////////////////////////////////////////////////////////

		#if UNITY_2019_3_OR_NEWER
		
			private class HellTapSettingsProvider : SettingsProvider {

				// Core Constructor
		 		public HellTapSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) {}

		 		// On GUI Override that redirects the Settings Provider to use the original GUI Method
		        public override void OnGUI(string searchContext){ PreferencesGUI(); }
		    }
		 
		    [SettingsProvider]
		    static SettingsProvider HellTapPreferences(){ return new HellTapSettingsProvider("Preferences/MeshKit"); }

		#endif	

	    ////////////////////////////////////////////////////////////////////////////////////////////////
		//	STANDARD PREFERENCES
		////////////////////////////////////////////////////////////////////////////////////////////////

		// Have we loaded the prefs yet
		private static bool prefsLoaded = false;
		
		// The Preferences
		public static bool useSmallIcons = true;
		public static bool livePrefabTracking = false;
		public static bool liveMeshTracking = false;
		public static bool automaticSceneBackups = false;
		public static bool verboseMode = false;
		
		// GUI Layout
		readonly static GUILayoutOption[] guiLayoutOptions = new GUILayoutOption[]{ GUILayout.Width(340), GUILayout.MinWidth(340), GUILayout.ExpandWidth(true) };

		// The PreferenceItem attribute is deprecated in Unity 2019 and later.
		#if !UNITY_2019_3_OR_NEWER
			[PreferenceItem ("MeshKit")]
		#endif	
		static void PreferencesGUI () {
	
			// =====================
			//	LOAD PREFERENCES
			// =====================

			if (!prefsLoaded) {
				useSmallIcons = EditorPrefs.GetBool ("MeshKitUseSmallIcons", true);
				livePrefabTracking = EditorPrefs.GetBool ("MeshKitLivePrefabTracking", false);
				liveMeshTracking = EditorPrefs.GetBool ("MeshKitLiveMeshTracking", false);
				automaticSceneBackups = EditorPrefs.GetBool ("MeshKitAutomaticallyBackupScenes", false);
				verboseMode = EditorPrefs.GetBool ("MeshKitVerboseMode", false);
				prefsLoaded = true;
			}
			
			// ==================
			//	PREFERENCES GUI
			// ==================

			// Initial Space
			GUILayout.Space(8);

			// Live Asset Tracking
			GUILayout.Label("Asset Management", "BoldLabel");
			GUILayout.Label("MeshKit automatically scans (and can fix) problematic meshes\nand prefabs every time you load a new scene. Alternatively, this\ncan be performed in realtime as you are working in the Editor.");
			GUILayout.Space(4);
			livePrefabTracking = EditorGUILayout.Toggle ("Realtime Prefab Checks", livePrefabTracking, guiLayoutOptions );
			liveMeshTracking = EditorGUILayout.Toggle ("Realtime Mesh Checks", liveMeshTracking, guiLayoutOptions );
			automaticSceneBackups = EditorGUILayout.Toggle ("Backup Scenes Without Asking", automaticSceneBackups, guiLayoutOptions );
			GUILayout.Space(16);

			// GUI Settings
			GUILayout.Label("MeshKit GUI Settings", "BoldLabel");
			GUILayout.Label("Display settings for the MeshKit Window.");
			GUILayout.Space(4);
			useSmallIcons = EditorGUILayout.Toggle ("Use Small Icons", useSmallIcons, guiLayoutOptions );
			GUILayout.Space(16);

			// Verbose Mode
			GUILayout.Label("Verbose Mode", "BoldLabel");
			GUILayout.Label("Shows verbose console messages for MeshKit in the Editor.");
			GUILayout.Space(4);
			verboseMode = EditorGUILayout.Toggle ("Enable Verbose Mode", verboseMode, guiLayoutOptions );
			GUILayout.Space(16);
			
			// =====================
			//	SAVE CHANGES
			// =====================

			if (GUI.changed){
				EditorPrefs.SetBool ("MeshKitUseSmallIcons", useSmallIcons );
				EditorPrefs.SetBool ("MeshKitLivePrefabTracking", livePrefabTracking);
				EditorPrefs.SetBool ("MeshKitLiveMeshTracking", liveMeshTracking);
				EditorPrefs.SetBool ("MeshKitAutomaticallyBackupScenes", automaticSceneBackups);
				EditorPrefs.SetBool ("MeshKitVerboseMode", verboseMode);
			}
		}
	}

}

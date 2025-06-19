
/*
This asset was uploaded by https://unityassetcollection.com
*/

////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitGUI_SkinnedMeshCombine.cs
//
//  Helper script for the MeshKit GuI.
//
//  © 2021 Melli Georgiou.
//  Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	public static class MeshKitGUI_SkinnedMeshCombine {

		// Combine Mode Descriptions
		static readonly string k_combineMeshesWithMaterialArrayDescription = "<size=10><b>NOTE:</b> This combination mode creates a new mesh with multiple materials (submeshes). It disables the previous SkinnedMeshRenderers but requires the existing Animator / Animation components to run animations on the new mesh. This approach is very fast ( making it a good choice for runtime ) and compatible with almost anything. It primarily offers performance improvements to animations.\n\nThis mode is automatic and doesn't require any additional options.</size>";

		static readonly string k_combineMeshesWithAtlassingDescriptionA = "<size=10><b>NOTE:</b> This combination mode creates a new mesh and utilizes atlasing to reduce everything to a single material. It disables the previous SkinnedMeshRenderers but is totally self-contained in regard to materials and textures. This approach is slow ( making it better suited to the Editor ) but offers maximum performance in both draw calls and animations. However, this does have a few limitations which are detailed in the documentation.</size>";

		static readonly string k_combineMeshesWithAtlassingDescriptionB = "Setup all the shader texture properties you want to combine below. First enter it's property name (eg '_MainTex') and then it's default texture color to use if it cannot be found in a material ( eg 'White' ).";

	/// -> GET CURRENT COMBINE BUTTON NAME

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	GET CURRENT COMBINE BUTTON NAME
		//	Updates the 'primary' MeshKit button label depending on what the operation will be
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Helpers
		static readonly string k_Undo_Combine = "Undo Combine";
		static readonly string k_Setup_Combine = "Setup Combine";
		static readonly string k_Combine = "Combine";

		// Method
		public static string GetCurrentCombineButtonName( MeshKitGUI mkGUI ){
			if( mkGUI.combineSelGridInt == 1 && mkGUI.shouldRevertMeshInsteadOfCombine == true ){
				return k_Undo_Combine;
			
			} else if( mkGUI.combineSelGridInt == 1 && mkGUI.shouldCreateNewMeshKitCombineSkinnedMesh == true ){
				return k_Setup_Combine;
			
			} else {
				return k_Combine;
			} 
		}

		// Method
		public static Texture2D GetCurrentCombineButtonIcon( MeshKitGUI mkGUI ){
			if( mkGUI.shouldRevertMeshInsteadOfCombine == true ){
				return MeshKitGUI.goIcon;
			
			} else if( mkGUI.shouldCreateNewMeshKitCombineSkinnedMesh == true ){
				return MeshKitGUI.combineIcon;		// <- maybe later change this to an "+" icon or something
			
			} else {
				return MeshKitGUI.combineIcon;
			} 
		}



		

	/// -> DRAW OPTIONS

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	DRAW OPTIONS
		//	This essentially extends the OnInspectorGUI of the core MeshKitGUI script
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void DrawOptions( MeshKitGUI mkGUI ){

			// --------------------------------------
			//	CHECK FOR COMBINE SYSTEMS IN PARENTS
			// --------------------------------------

			// We need to make sure there are no LODGroups / MeshKitAutoLOD components on the child objects.
			if( mkGUI.combineSystemExistsInParent == true ){

				HTGUI.WrappedTextLabel("You cannot use another combine setup component ( 'MeshKitCombineSkinnedMesh' ) when one already exists in a parent object. To work on this object you must first finalize your combine operations on the parent and click the 'Remove Setup' button." );

				// Setup Helpers
				mkGUI.disableMainButton = true;
				mkGUI.shouldCreateNewMeshKitCombineSkinnedMesh = true;

				// End Early
				return;
			}


			// -----------------------
			//	CHECK FOR LODGROUPS
			// -----------------------

			// We need to make sure there are no LODGroups / MeshKitAutoLOD components on the child objects.
			if( mkGUI.GetHowManyMeshKitAutoLODComponentsInChildren() > 0 || 
				mkGUI.GetHowManyLODGroupComponentsInChildren() > 0 || 
				mkGUI.GetLODSystemExistsInParent() == true
			){
				HTGUI.WrappedTextLabel("You cannot combine objects that contain or are a part of an LOD system." );

				// Setup Helpers
				mkGUI.disableMainButton = true;
				mkGUI.shouldCreateNewMeshKitCombineSkinnedMesh = true;

				// End Early
				return;
			}

			// -----------------------
			//	COMPONENT IS NOT SETUP
			// -----------------------

			// Cache the MeshKitCombineSkinnedMesh component on this GameObject (if it exists)
			MeshKitCombineSkinnedMeshSetup mkcsm = Selection.activeGameObject.GetComponent<MeshKitCombineSkinnedMeshSetup>();

			if( mkcsm == null ){

				// Setup Helpers
				mkGUI.disableMainButton = false;
				mkGUI.shouldCreateNewMeshKitCombineSkinnedMesh = true;
				mkGUI.shouldRevertMeshInsteadOfCombine = false;
				mkGUI.showBottomSeperator = true;

				HTGUI.WrappedTextLabel("Press the Setup Combine button to begin the process..." );


			// ---------------------
			//	COMPONENT IS SETUP
			// ---------------------

			} else {

				// Setup Helpers
				mkGUI.disableMainButton = false;
				mkGUI.shouldCreateNewMeshKitCombineSkinnedMesh = false;
				mkGUI.showBottomSeperator = false;

				// If we have already generated the mesh, don't allow us to recreate the mesh
				if( mkcsm.IsGenerated == true ){
					mkGUI.shouldRevertMeshInsteadOfCombine = true;
				} else {
					mkGUI.shouldRevertMeshInsteadOfCombine = false;
				}

				// --------------------------------
				//	SELECT COMBINATION MODE
				// --------------------------------

				// type of combine
				mkcsm.combineMode = (MeshKitCombineSkinnedMeshSetup.CombineMode) HTGUI_UNDO.EnumField( mkcsm, "Combine Mode", MeshKitGUI.goIcon, "Combine Mode", mkcsm.combineMode );

				// Spacer
				GUILayout.Label( string.Empty, GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );


				// --------------------------------
				//	COMBINE MODE: MATERIAL ARRAY
				// --------------------------------

				// Options When Combining With Texture Atlassing
				if( mkcsm.combineMode == MeshKitCombineSkinnedMeshSetup.CombineMode.CombineMeshesWithMaterialArray ){

					// Spacer
					HTGUI.WrappedTextLabel( k_combineMeshesWithMaterialArrayDescription );

					// Spacer
					GUILayout.Label( string.Empty, GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );


				// ---------------------------------
				//	COMBINE MODE: TEXTURE ATLASSING
				// ---------------------------------

				} else if( mkcsm.combineMode == MeshKitCombineSkinnedMeshSetup.CombineMode.CombineMeshesWithTextureAtlasing ){

					// Spacer
					HTGUI.WrappedTextLabel( k_combineMeshesWithAtlassingDescriptionA );

					// Add Space and another sepLine
					GUILayout.Space(8);
					HTGUI.SepLine();


					// ---------------------------------------
					//	SETUP PROPERTIES WITH QUICK DROPDOWNS
					// ---------------------------------------

					// Header
					GUILayout.Space(8);
					GUILayout.Label( "Setup Properties Using Templates: ", "BoldLabel", GUILayout.MinWidth(100) );
					GUILayout.Space(8);

					// Spacer
					HTGUI.WrappedTextLabel( "You can quickly setup properties for common shader setups using the drop down list below. You can also customize the properties after you have chosen a starting point." );
					GUILayout.Space(8);


					// Dropdown List
					mkcsm.dropDownHelperList = (MeshKitCombineSkinnedMeshSetup.DropDownHelperList)HTGUI_UNDO.EnumField( mkcsm, "Quick Setup", MeshKitGUI.gearIcon, "Quick Setup: ", mkcsm.dropDownHelperList);

					// Setup Quick Unlit Shader
					if( mkcsm.dropDownHelperList == MeshKitCombineSkinnedMeshSetup.DropDownHelperList.UnlitShader ){
						mkcsm.propertyList = mkcsm._defaultUnlitPropertyList;
						mkcsm.dropDownHelperList = MeshKitCombineSkinnedMeshSetup.DropDownHelperList.Select;

					// Setup Quick Bumped Shader
					} else if( mkcsm.dropDownHelperList == MeshKitCombineSkinnedMeshSetup.DropDownHelperList.BumpedShader ){
						mkcsm.propertyList = mkcsm._defaultBumpedPropertyList;
						mkcsm.dropDownHelperList = MeshKitCombineSkinnedMeshSetup.DropDownHelperList.Select;
					
					// Setup Quick Standard Shader
					} else if( mkcsm.dropDownHelperList == MeshKitCombineSkinnedMeshSetup.DropDownHelperList.StandardShader ){
						mkcsm.propertyList = mkcsm._defaultStandardPropertyList;
						mkcsm.dropDownHelperList = MeshKitCombineSkinnedMeshSetup.DropDownHelperList.Select;
					
					}

					// Add Space and another sepLine
					GUILayout.Space(8);
					HTGUI.SepLine();


					// ---------------------
					//	DROP-DOWN HELPER
					// ---------------------

					// Label
					GUILayout.Space(8);
					GUILayout.Label("Shader Properties To Combine", "boldLabel");
					GUILayout.Space(8);

					// Spacer
					HTGUI.WrappedTextLabel( k_combineMeshesWithAtlassingDescriptionB );

					// Spacer
					GUILayout.Label( string.Empty, GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );



					// ----------------------
					//	PROPERTY LIST HEADER
					// ----------------------

					// Start Horizontal Group
					GUILayout.BeginHorizontal();

						// Name / Title of object
						GUILayout.Label( string.Empty, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20) );
						GUILayout.Label( string.Empty, GUILayout.MinWidth(32), GUILayout.MaxWidth(32), GUILayout.MaxHeight(20));
					//	GUILayout.Label( "Property Name: ", "BoldLabel", GUILayout.MinWidth(100) );
					//	GUILayout.Label( "Default Texture: ", "BoldLabel", GUILayout.MinWidth(100) );

						// We need this weird spacing to line up the columns - weird.
						GUILayout.Label( "Texture Property Name: ", "BoldLabel", GUILayout.MinWidth(100) );
						GUILayout.Label( "Default Texture:               ", "BoldLabel", GUILayout.MinWidth(100) );

					// End Horizontal Group
					GUILayout.EndHorizontal();

					// ---------------------
					//	PROPERTY LIST SETUP
					// ---------------------

					// Helper and loop
					MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup pl = null;
					MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup tempPL = new MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup();
					bool listContainsEmptyPropertyName = false;

					for( int i = 0; i < mkcsm.propertyList.Count; i++ ){

						// Cache the current entry
						pl = mkcsm.propertyList[i];

						// Create a new entry if one doesn't already exist
						if( pl == null ){ pl = new MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup(); }

						// Setup the temporary property so we can use the undo check
						tempPL.propertyName = pl.propertyName;
						tempPL.missingTextureFallback = pl.missingTextureFallback;

						// If we find a property name that is empty, track it
						if( pl.propertyName == string.Empty ){ listContainsEmptyPropertyName = true; }

						// Create a new variable to hold the Field
						EditorGUI.BeginChangeCheck();

							// Start Horizontal Group
							GUILayout.BeginHorizontal();

								// Name / Title of object
								GUILayout.Label( MeshKitGUI.goIcon, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20) );
								GUILayout.Label( " " + (i+1).ToString() + ": ", "BoldLabel", GUILayout.MinWidth(32), GUILayout.MaxWidth(32), GUILayout.MaxHeight(20));
								tempPL.propertyName = EditorGUILayout.TextField( string.Empty, tempPL.propertyName, GUILayout.MinWidth(100) );
								tempPL.missingTextureFallback = (MeshKitCombineSkinnedMeshSetup.MissingTextureFallback)EditorGUILayout.EnumPopup(string.Empty, tempPL.missingTextureFallback, GUILayout.MinWidth(100) );

							// End Horizontal Group
							GUILayout.EndHorizontal();

						// If a GUI Control has been updated while we updated the above value, record the undo!
						if (EditorGUI.EndChangeCheck()){

							// Record the undo object and set the reference to the new value
							Undo.RecordObject ( mkcsm, "Texture Property");
							pl.propertyName = tempPL.propertyName;
							pl.missingTextureFallback = tempPL.missingTextureFallback;
						}

					}

					// -----------------------
					//	PROPERTY LIST OPTIONS
					// -----------------------

					// Start Horizontal Group
					GUILayout.BeginHorizontal();

						// Add Space
						GUILayout.FlexibleSpace();

						// Remove Property
						if( mkcsm.propertyList.Count > 1 &&
							GUILayout.Button( new GUIContent( System.String.Empty, HTGUI.removeButton, "Remove Property" ), GUILayout.MinWidth(32), GUILayout.MaxWidth(32) ) 
						){
							Undo.RecordObject ( mkcsm, "Remove Property" );
							mkcsm.propertyList.RemoveAt( mkcsm.propertyList.Count-1 );
							GUIUtility.ExitGUI();
						}

						// Add Property
						if( GUILayout.Button( new GUIContent( System.String.Empty, HTGUI.addButton, "Add Property" ), GUILayout.MinWidth(32), GUILayout.MaxWidth(32) ) 
						){
							Undo.RecordObject ( mkcsm, "Add Property" );
							mkcsm.propertyList.Add( new MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup() );
							GUIUtility.ExitGUI();
						}

					// End Horizontal Group
					GUILayout.EndHorizontal();

					// ----------------------
					//	ERROR CHECKING
					// ----------------------

					if( listContainsEmptyPropertyName == true ){
						
						// Don't Allow Player To use the main button
						mkGUI.disableMainButton = true;

						// Spacer
						GUILayout.Label( string.Empty, GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );

						// Show Error Message In Help Box
						mkGUI.DoHelpBox( "Texture property names cannot be empty!" );
						
					}

					// Make sure the list doesn't contain duplicate names
					if( PropertyListContainsDuplicateNames( ref mkcsm.propertyList ) == true ){

						// Don't Allow Player To use the main button
						mkGUI.disableMainButton = true;

						// Spacer
						GUILayout.Label( string.Empty, GUILayout.MinHeight(16), GUILayout.MaxHeight(16) );

						// Show Error Message In Help Box
						mkGUI.DoHelpBox( "Texture property Names must be unique!" );

					}


					// ----------------------------
					//	ADDITIONAL COMBINE OPTIONS
					// ----------------------------

					// Label
					GUILayout.Space(8);
					GUILayout.Label("Additional Options", "boldLabel");
					GUILayout.Space(8);

					// Bake _Color Property Into the _MainTex.
					mkcsm.bakeColorIntoMainTex = HTGUI_UNDO.ToggleField( mkcsm, "Bake Color Property Into MainTex", MeshKitGUI.gearIcon, "Bake Color Property Into MainTex", mkcsm.bakeColorIntoMainTex  ); 


					// ---------------------
					//	ATLAS OPTIONS
					// ---------------------

					// Add Space and another sepLine
					GUILayout.Space(8);
					HTGUI.SepLine();

					// Label
					GUILayout.Space(8);
					GUILayout.Label("Atlas Options", "boldLabel");
					GUILayout.Space(8);

					// Maximum Atlas Size
					mkcsm.maximumAtlasSize = (MeshKitCombineSkinnedMeshSetup.MaxAtlasSize)HTGUI_UNDO.EnumField( mkcsm, "Maximum Atlas Size", MeshKitGUI.gearIcon, "Maximum Atlas Size: ", mkcsm.maximumAtlasSize);

					// ---------------------
					//	OUTPUT OPTIONS
					// ---------------------

					// Add Space and another sepLine
					GUILayout.Space(8);
					HTGUI.SepLine();

					// Label
					GUILayout.Space(8);
					GUILayout.Label("Output Options", "boldLabel");
					GUILayout.Space(8);

					HTGUI.WrappedTextLabel( mkcsm.assetRelativePathToFolder != string.Empty ? "Your combined meshes, materials and textures for this setup will be saved here:" : "As this combine mode creates multiple assets, it is unsuitable to be managed by MeshKit as a local mesh. Instead, the combined assets are created directly into a project folder you specify.\n\nClick the 'Set Saved Asset Folder' button below to choose a folder inside your project's 'Asset' directory to save your combined meshes, materials and textures.");

					// Spacer
					HTGUI.WrappedTextLabel( mkcsm.assetRelativePathToFolder );

					// Begin Horizontal Row for the save location button
					GUILayout.BeginHorizontal();

						// Flexible Space
						GUILayout.FlexibleSpace();

						// --------------------------
						// SHOW FOLDER BUTTON
						// --------------------------

						if( mkcsm.assetRelativePathToFolder != string.Empty && GUILayout.Button(" Show Saved Asset Folder ", GUILayout.MinWidth(160)) ){
							if( AssetDatabase.IsValidFolder( mkcsm.assetRelativePathToFolder ) == true ){

								// We can ping the object in the Project pane to show the user where it is
								UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath( mkcsm.assetRelativePathToFolder, typeof(UnityEngine.Object));
								EditorGUIUtility.PingObject(obj);

							} else {
								
								// Tell the user this location cannot be used
								EditorUtility.DisplayDialog("Show Saved Asset Folder", "The location you have previously chosen appears to be invalid.\n\nThis can happen if the original folder was renamed or deleted. Please select a folder using the 'Set Asset Location' button.", "OK"); 
							}
						}

						// --------------------------
						// SET ASSET LOCATION BUTTON
						// --------------------------

						if( GUILayout.Button(" Set Saved Asset Folder ", GUILayout.MinWidth(160)) ){
						
							// Make Undo Point
							Undo.RecordObject ( mkcsm, "Set Saved Asset Folder" );

							// Show Open Folder Panel and setup a full and asset-relative file path
							if( MeshAssets.SetupFolderToSaveAssets( "Set Saved Asset Folder", ref mkcsm.assetRelativePathToFolder ) == true ){

								// Tell user the selection is fine
								EditorUtility.DisplayDialog("Set Saved Asset Folder", "The following folder will be used to save your combined assets:\n\n" + mkcsm.assetRelativePathToFolder + "\n\nNOTE: It is recommended to choose a unique folder for each combine setup you create to avoid any possible conflicts.", "OK"); 

								// Update the Asset Database in case we created a new folder
								AssetDatabase.Refresh();


							} else {

								// Tell the user this location cannot be used
								EditorUtility.DisplayDialog("Set Saved Asset Folder", "The location you have chosen is invalid.\n\nYou must choose a folder inside of your project's Asset folder.", "OK"); 
							}

							// Debug Paths
							// Debug.Log( "fullFilePathToFolder: " + mkcsm.fullFilePathToFolder );
							// Debug.Log( "assetRelativePathToFolder: " + mkcsm.assetRelativePathToFolder );

						}

					// End Button Row
					GUILayout.EndHorizontal();

				}

				// Add Space and another sepLine
				GUILayout.Space(8);
				HTGUI.SepLine();

				// RUN SELECTED TOOL
				GUILayout.BeginHorizontal();

					// Setup GUI for displaying Red Button
					GUIStyle finalButton = new GUIStyle(GUI.skin.button);
					finalButton.padding = new RectOffset(6, 6, 6, 6);
					finalButton.imagePosition = ImagePosition.ImageLeft;
					finalButton.fontStyle = FontStyle.Bold;

					/*
					// Undo Combined Mesh Setup
					if( mkcsm.IsGenerated == true ){
						if( GUILayout.Button( new GUIContent( " Undo Combine ", MeshKitGUI.defaultIcon ), finalButton, GUILayout.MinWidth(160), GUILayout.MinHeight(40), GUILayout.MaxHeight(40) ) &&
							EditorUtility.DisplayDialog("Undo Combine Operation", "Are you sure you want to undo the combine operation on \""+Selection.activeGameObject.name+"\"?", "Yes", "No") 
						){

							// Make an undo point
							//Undo.RecordObject ( mkcsm.gameObject, "Undo Combine Operation");
							mkcsm.UndoCombine();

							// Update the selection
							mkGUI.OnSelectionChange();
							GUIUtility.ExitGUI();
						}
					}
					*/
								
					// Add Flexible Space
					GUILayout.FlexibleSpace();

					// Completely Remove MeshKitCombineSkinnedMesh Component 
					if( GUILayout.Button( new GUIContent( " Remove Setup ", MeshKitGUI.deleteIcon ), finalButton, GUILayout.MinWidth(160), GUILayout.MinHeight(40), GUILayout.MaxHeight(40) ) &&
						EditorUtility.DisplayDialog("Remove Combine Setup", "Are you sure you want to remove the MeshKitCombineSkinnedMesh component on \""+Selection.activeGameObject.name+"\"?", "Yes", "No") 

						&& ( mkcsm.IsGenerated == false || mkcsm.IsGenerated == true && EditorUtility.DisplayDialog("Remove Combine Setup", "Removing this component will remove the functionality to revert the mesh back to the state it was in before it was combined.\n\nAre you sure you wish to continue?", "Yes", "No")  )
					){

						// Make an undo point and destroy the combine setup component
						Undo.RecordObject ( mkcsm.gameObject, "Remove Combine Setup");
						Undo.DestroyObjectImmediate ( mkcsm );

						// Update the selection
						mkGUI.OnSelectionChange();
						GUIUtility.ExitGUI();
					}

				GUILayout.EndHorizontal();	

				// Final Space
				GUILayout.Space(8);

			}
			
		}

	/// -> PROPERTY LIST CONTAINS DUPLICATE NAMES

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	PROPERTY LIST CONTAINS DUPLICATE NAMES
		//	Called from the MeshKitGUI script to add the needed component on the GameObject
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		static bool PropertyListContainsDuplicateNames( ref List<MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup> list ){

			// Helpers
			int listCount = list.Count;
			string currentPropertyName = string.Empty;

			// First Loop
			for( int x = 0; x < listCount; x++ ){

				// Cache the current property name and start the second Loop
				currentPropertyName = list[x].propertyName;				
				for( int y = 0; y < listCount; y++ ){

					// Make sure both loop indices are not referencing the same entry and check if a duplicate was found
					if( x != y && list[x].propertyName == list[y].propertyName ){
						return true;
					}
				}
			}

			// Otherwise, return false;
			return false;

		}

	/// -> SETUP COMBINE ON THIS GAMEOBJECT

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	SETUP COMBINE COMPONENT ON THIS GAMEOBJECT
		//	Called from the MeshKitGUI script to add the needed component on the GameObject
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void SetupCombineComponentOnThisGameObject(){

			// There is no MeshKitCombineSkinnedMeshSetup() component.
			if( Selection.activeGameObject.GetComponent<MeshKitCombineSkinnedMeshSetup>() == null &&
				Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>() == null
			){

				Undo.RecordObject (Selection.activeGameObject, "Setup SkinnedMesh Combine");
				Undo.AddComponent<MeshKitCombineSkinnedMeshSetup>(Selection.activeGameObject);
				GUIUtility.ExitGUI();

			} else {

				EditorUtility.DisplayDialog(	
					"MeshKitCombineSkinnedMeshSetup Setup", 
					"MeshKit cannot combine multiple Skinned Mesh Renderers using this parent GameObject.\n\nIt already has either a 'SkinnedMeshRenderer' or 'MeshKitCombineSkinnedMeshSetup' component attached.", "OK"
				);
			}
		}

	/// -> UNDO COMBINE OPERATION

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	UNDO COMBINE OPERATION
		//	Remove the combine operation using the MeshKitCombineSkinnedMeshSetup component
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void UndoCombine(){

			// Cache the MeshKitCombineSkinnedMeshSetup component on this GameObject (if it exists)
			MeshKitCombineSkinnedMeshSetup mkcsm = Selection.activeGameObject.GetComponent<MeshKitCombineSkinnedMeshSetup>();

			// Make sure we have a MeshKitCombineSkinnedMeshSetup component and pass the method along
			if( mkcsm != null ){
				mkcsm.UndoCombine();
			}
		}

	/// -> START COMBINE

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	START COMBINE
		//	Start combining the object now using the setup from the MeshKitCombineSkinnedMeshSetup component
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static bool StartCombine(){

			// Cache the MeshKitCombineSkinnedMeshSetup component on this GameObject (if it exists)
			MeshKitCombineSkinnedMeshSetup mkcsm = Selection.activeGameObject.GetComponent<MeshKitCombineSkinnedMeshSetup>();

			// Make sure we have a MeshKitCombineSkinnedMeshSetup component
			if( mkcsm != null ){

				// Track if the combine was successful
				bool combineWasSuccessful = false;

				// -----------------------------------------
				// 1) COMBINE MESHES WITH MATERIAL ARRAY
				// -----------------------------------------

				// Use "Combine Meshes With Material Array" Mode
				if( mkcsm.combineMode == MeshKitCombineSkinnedMeshSetup.CombineMode.CombineMeshesWithMaterialArray ){

					combineWasSuccessful = EditorSkinnedMeshRendererUtility.CombineSkinnedMeshRenderersNoAtlas( Selection.activeGameObject );
				
				}

				// -----------------------------------------
				// 2) COMBINE MESHES WITH TEXTURE ATLASSING
				// -----------------------------------------

				// Use "Combine Meshes With Texture Atlasing" Mode
				else if( mkcsm.combineMode == MeshKitCombineSkinnedMeshSetup.CombineMode.CombineMeshesWithTextureAtlasing ){

					// Make sure our asset-relative folder is valid
					if( AssetDatabase.IsValidFolder( mkcsm.assetRelativePathToFolder ) == true ){

						// TO DO: Make the combine function return true or false if it was successful
						combineWasSuccessful = EditorSkinnedMeshRendererUtility.CombineSkinnedMeshRenderers( 
							Selection.activeGameObject, mkcsm.maximumAtlasSize, mkcsm.propertyList, mkcsm.bakeColorIntoMainTex, mkcsm.assetRelativePathToFolder
						);


					// If there is an issue with the asset-relative folder	
					} else {

						// Show a dialog to show 
						EditorUtility.DisplayDialog(	
							"Combine Meshes With Texture Atlassing", 
							"Original save location is no longer valid!\n\nTry pressing the 'Set Saved Asset Folder' button again to update the location.", 
							"OK"
						);

						combineWasSuccessful = false;
					}
				}

				// Return if the combine was successful
				return combineWasSuccessful;
			}

			// Return false if something goes wrong
			return false;
		}


	}
}
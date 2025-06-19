
/*
This asset was uploaded by https://unityassetcollection.com
*/

////////////////////////////////////////////////////////////////////////////////////////////////
//
//  EditorSkinnedMeshRendererCombine.cs
//
//	EDITOR Methods for combining and seperating Skinned Mesh Renderers
//
//	© 2022 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

// Compilation Helpers
#define SHOW_PROGRESS_BARS

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	public class EditorSkinnedMeshRendererUtility : MonoBehaviour {

/// -> [EDITOR ONLY] HELPER TEXTURES & COLORS

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] HELPER TEXTURES & COLORS
		//	Combines multiple child skinned mesh renderers into a new one, complete with atlassing
		//	to reduce draw calls. This is the most efficient way to combine.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Helper Textures
		private static Texture2D texture2DBlack;		// <- Fully Transparent
		private static Texture2D texture2DWhite;		// <- Fully Opaque
		private static Texture2D texture2DGrey;			// <- Half Transparent
		private static Texture2D texture2DNormal;		// <- Neutral Normal Texture

		// Helper Colors
		private static Color transparentBlackColor = new Color(0f,0f,0f,0f);
		private static Color whiteColor = new Color(1f,1f,1f,1f);
		private static Color greyColor = new Color(0.5f,0.5f,0.5f,0.5f);
		private static Color normalColor = new Color(0.5f,0.5f,1f,1f);

		// Converts The Missing Texture Fallback To A Color
		private static Color MissingTextureFallbackToColor( MeshKitCombineSkinnedMeshSetup.MissingTextureFallback missingTextureFallback ){
			if( missingTextureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.TransparentBlack ){
				return transparentBlackColor;
			} else if( missingTextureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.White ){
				return whiteColor;
			} else if( missingTextureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Grey ){
				return greyColor;
			} else if( missingTextureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal ){
				return normalColor;
			}

			// Return black if something goes wrong
			return Color.black;
		}

		// Recreate Texture Method
		private static void RecreateHelperTextures(){

			// NOTE: All of these textures must be at least 2x2 otherwise our custom TextureRescale method breaks.

			// Black
			texture2DBlack = new Texture2D (2,2);
			texture2DBlack.SetPixel(0, 0, transparentBlackColor );
			texture2DBlack.SetPixel(0, 1, transparentBlackColor );
			texture2DBlack.SetPixel(1, 0, transparentBlackColor );
			texture2DBlack.SetPixel(1, 1, transparentBlackColor );
			texture2DBlack.Apply();

			// White
			texture2DWhite = new Texture2D (2,2);
			texture2DWhite.SetPixel(0, 0, whiteColor );
			texture2DWhite.SetPixel(0, 1, whiteColor );
			texture2DWhite.SetPixel(1, 0, whiteColor );
			texture2DWhite.SetPixel(1, 1, whiteColor );
			texture2DWhite.Apply();

			// Grey
			texture2DGrey = new Texture2D (2,2);
			texture2DGrey.SetPixel(0, 0, greyColor );
			texture2DGrey.SetPixel(0, 1, greyColor );
			texture2DGrey.SetPixel(1, 0, greyColor );
			texture2DGrey.SetPixel(1, 1, greyColor );
			texture2DGrey.Apply();

			// Normal
			texture2DNormal = new Texture2D (2,2);
			texture2DNormal.SetPixel(0, 0, normalColor );
			texture2DNormal.SetPixel(0, 1, normalColor );
			texture2DNormal.SetPixel(1, 0, normalColor );
			texture2DNormal.SetPixel(1, 1, normalColor );
			texture2DNormal.Apply();
		}	

/// -> [EDITOR ONLY] TEXTURE IMPORTER SETUP

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] TEXTURE IMPORTER SETUP
		//	This class helps us track the textures that need to be modified ( uncompressed, readable, etc ). We can do this in the Editor on demand
		//	and when we're done restore the textures back to their original settings.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Texture Importer Setup
		class TextureImporterSetup {

			// Texture2D Variables
			public Texture2D texture2D = null;
			public TextureFormat originalTextureFormat = TextureFormat.RGBA32;

			// AssetDatabase Variables
			public string assetPath = "";

			// TextureImporter Variables
			public TextureImporter ti = null;
			public bool wasReadable = false;
			public TextureImporterCompression originalCompression = TextureImporterCompression.Uncompressed;

			// TextureImporter Platform Override
			public string platformString;
			public bool usesPlatformOverride = false;
			int originalPlatformMaxTextureSize = 0;
			TextureImporterFormat originalPlatformTextureFmt;
			int originalPlatformCompressionQuality = 0;
			bool originalPlatformAllowsAlphaSplit = false;

			// Custom
			// Normal Map RedFix Required? ( On certain platforms such as "standalone", the red normalmap bugfix is needed )
			public bool requiresNormalMapRedFix = false;
	

			// Constructor
			public TextureImporterSetup ( Texture2D newTexture2D, TextureImporter newTi, string newAssetPath ){

				// Setup the TextureImporterSetup
				texture2D = newTexture2D;
				ti = newTi;
				wasReadable = newTi.isReadable;
				originalCompression = newTi.textureCompression;
				originalTextureFormat = newTexture2D.format;
				assetPath = newAssetPath;

				// Cache the platform string (used to access texture overrides)
				platformString = PlatformToString();

				// If this texture is using a platform override, cache it here as well
				if ( ti.GetPlatformTextureSettings ( 
						platformString, out originalPlatformMaxTextureSize, out originalPlatformTextureFmt, 
						out originalPlatformCompressionQuality, out originalPlatformAllowsAlphaSplit
					)
				){
					usesPlatformOverride = true;

				} else {

					usesPlatformOverride = false;
				}

				// On certain platforms, bugfixes are required. Add tested platforms below:
				//	Platforms that require the fix:	'Standalone' (tested on mac, linux - verify on windows too)
				//	Platforms that work without it: 'iOS'
				if( platformString == "Standalone" ){
					requiresNormalMapRedFix = true;			// <- The normal map has a red tint on this platform and needs to be processed in order to fix it.
				}

			}

			// Prepare Settings so we can read textures properly
			public void PrepareSettings(){

				// Texture Importer
				if( ti != null ){

					// Make sure the texture is readable
					ti.isReadable = true;
					ti.textureCompression = TextureImporterCompression.Uncompressed;

					// If we're using texture overrides, we need to explicilty tell it what format to use
					if( usesPlatformOverride == true ){

						// Grab the current platform's override and set the format to RGBA32 ...
						var platformOverrides = ti.GetPlatformTextureSettings( platformString );
						platformOverrides.format = TextureImporterFormat.RGBA32;
						
						// Set the new version
						ti.SetPlatformTextureSettings( platformOverrides );
					}
					
					// Update the asset
					AssetDatabase.Refresh();
					AssetDatabase.ImportAsset( assetPath );


				}
			}

			// Validate Current Texture Format Settings
			public bool ValidateSettings(){

				// These are apparantly the only formats that allow us
				// to modify color data, etc.
				if( texture2D.format == TextureFormat.ARGB32 ||
					texture2D.format == TextureFormat.RGBA32 ||
					texture2D.format == TextureFormat.BGRA32 ||
					texture2D.format == TextureFormat.RGB24 ||
					texture2D.format == TextureFormat.Alpha8 ||
					texture2D.format == TextureFormat.RGBAFloat ||
					texture2D.format == TextureFormat.RGBAHalf
				){
					return true;
				}

				return false;
			}

			// Reset Settings
			public void ResetOriginalSettings(){

				// Texture Importer
				if( ti != null ){

					// Restore settings
					ti.isReadable = wasReadable;
					ti.textureCompression = originalCompression;

					// If we're using texture overrides, we need to explicilty tell it what format to use
					if( usesPlatformOverride == true ){

						// Grab the current platform's override and set the format back to it's original
						var platformOverrides = ti.GetPlatformTextureSettings( platformString );
						platformOverrides.format = originalPlatformTextureFmt;
						
						// Set the new version
						ti.SetPlatformTextureSettings( platformOverrides );
					}
					
					// Update the asset
					AssetDatabase.Refresh();
					AssetDatabase.ImportAsset( assetPath );

				}
			}

			// Returns the platform string depending on the current platform
			string PlatformToString(){

				#if UNITY_STANDALONE
					return "Standalone";

				#elif UNITY_IOS
					return "iPhone";
					
				#elif UNITY_ANDROID
					return "Android";
					
				#elif UNITY_WEBGL
					return "WebGL";
					
				#elif UNITY_WSA
					return "Windows Store Apps";
					
				#elif UNITY_PS4
					return "PS4";
					
				#elif UNITY_XBOXONE
					return "XboxOne";
					
				#elif UNITY_SWITCH
					return "Nintendo Switch";
					
				#elif UNITY_TVOS
					return "tvOS";

				#else 
					return "Standalone";

				#endif
			}
		}

		// Helper For Locating Specific Textures that could cause issues with normal maps
		static bool DoesTextureRequireRedNormalMapFix( List<TextureImporterSetup> setups, Texture2D tex ){
			for( int i = 0; i < setups.Count; i++ ){

				// If we find the matching texture
				if( setups[i].texture2D == tex ){

					// Return true if its original texture format was DXT5
					if( setups[i].requiresNormalMapRedFix == true ){
						return true;
					} else {
						return false;
					}
				}
			}

			// If we didn't find this texture, assume false.
			return false;
		}

/// -> [EDITOR ONLY] COMBINE SKINNED MESH RENDERERS (WITH ATLASSING)

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] COMBINE SKINNED MESH RENDERERS (WITH ATLASSING)
		//	Combines multiple child skinned mesh renderers into a new one, complete with atlassing
		//	to reduce draw calls. This is the most efficient way to combine.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Helpers
		//static public int maxAtlasSize = 2048;													// <- Maximum texture atlas size (this should become an enum option later)
		static public bool attemptToFixOutOfRangeUVs = true;									// <- this should probably be left on permenantly

		// Class for setting up texture properties
		class TexturePropertySetup {

			// Setup
			public string propertyName = "";															// eg. "_MainTex"
			public MeshKitCombineSkinnedMeshSetup.MissingTextureFallback textureFallback = MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.TransparentBlack;	// eg. use Transparent pixels if _MainTex is missing.
			
			// These values are synced (all indices refer to the same texture / material)
			public Material[] materials = new Material[0];												// The materials found in all renderers
			public Texture2D[] textures = new Texture2D[0];												// The textures found in all renderers
			public bool[] texturesOriginallyMissing = new bool[0];										// Track which of textures in the array were originally null
			public Color[] colorsToBakeIntoMainTex = new Color[0];										// What color should we bake into the main atlas ( per texture )

			// Results
			public Texture2D atlas = new Texture2D (1,1);												// The atlas for each texture property

			// Memory Cleanup
			public List<Texture2D> tempResizedTexturesToDelete = new List<Texture2D>();					// A List of temporary resized textures we created on demand.
			
		}

		

		// Helper strings for default texture properties to extract ("_MainTex" should ALWAYS be included!)
		//static readonly string[] defaultPropertyNames = new string[] { "_MainTex","_BumpMap" };
		static readonly string[] defaultPropertyNames = new string[] { "_MainTex","_BumpMap","_MetallicGlossMap","_OcclusionMap", "_EmissionMap" };
		static readonly MeshKitCombineSkinnedMeshSetup.MissingTextureFallback[] defaultTextureFallbacks = new MeshKitCombineSkinnedMeshSetup.MissingTextureFallback[]{ 
			MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.White, MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal, MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Grey, MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.White, MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.TransparentBlack
		};


		// METHODS
		/*
		// DEBUG Method: Shortcut From Menu - REMOVE LATER!
		[MenuItem ("Assets/Combine SkinnedMeshRenderers (With Atlassing)")]
		static void CombineSkinnedMeshRenderers(){ 
			CombineSkinnedMeshRenderers( Selection.activeGameObject, MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._2048, defaultPropertyNames, defaultTextureFallbacks,	true, // <- use the default shader texture properties
			"Assets/ZZZ TEST" );	// <- Original debug save locations
		}
		*/

		// Helper Method: ( from MeshKitCombineSkinnedMeshSetup.cs )
		public static bool CombineSkinnedMeshRenderers( GameObject selectedGameObject, MeshKitCombineSkinnedMeshSetup.MaxAtlasSize maximumAtlasSize, List<MeshKitCombineSkinnedMeshSetup.CombineSkinnedMeshRendererSetup> propertyList, bool bakeColorIntoMainTex, string assetRelativePathToSaveFolder ){

			// Create new propertyNames and textureFallbacks arrays based on the combineSkinnedMeshRendererSetup
			int arrayLength = propertyList.Count;
			string[] propertyNames = new string[ arrayLength ];
			MeshKitCombineSkinnedMeshSetup.MissingTextureFallback[] textureFallbacks = new MeshKitCombineSkinnedMeshSetup.MissingTextureFallback[ arrayLength ];

			// Copy the data from the combineSkinnedMeshRendererSetup to the new arrays
			for( int i = 0; i < arrayLength; i++ ){
				propertyNames[i] = propertyList[i].propertyName;
				textureFallbacks[i] = propertyList[i].missingTextureFallback;
			}

			// Run the method again the normal way
			return CombineSkinnedMeshRenderers( selectedGameObject, maximumAtlasSize, propertyNames, textureFallbacks, bakeColorIntoMainTex, assetRelativePathToSaveFolder );
		}


		// Full Method
		public static bool CombineSkinnedMeshRenderers( GameObject selectedGameObject, MeshKitCombineSkinnedMeshSetup.MaxAtlasSize maximumAtlasSize, string[] propertyNames, MeshKitCombineSkinnedMeshSetup.MissingTextureFallback[] textureFallbacks, bool bakeColorIntoMainTex, string assetRelativePathToSaveFolder ){


			// ------------------------------------------------------------
			//	HANDLE SAVE FILEPATHS
			// ------------------------------------------------------------

			// Setup a path to save new assets ( add a forward slash to the supplied paths ) - we use this with the AssetDatabase API
			string saveAssetDirectory = assetRelativePathToSaveFolder+"/";

			// Dynamically setup the full file path using our handy converter method - we use this with the File API		
			string saveAssetDirectoryForFileAPI = MeshAssets.EditorConvertAssetRelativeFolderPathToFullFolderPath( assetRelativePathToSaveFolder ) + "/";

			// If there is a problem with the saveAssetDirectoryForFileAPI path, end early.
			if( saveAssetDirectoryForFileAPI == string.Empty ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"The path to save assets does not appear to be valid:\n\n" + saveAssetDirectoryForFileAPI, "OK"
				);
				return false;
			}

			// ------------------------------------------------------------
			//	INITIAL GAMEOBJECT CHECKS
			// ------------------------------------------------------------

			// End early if we didnt select a GameObject
			if ( selectedGameObject == null){ 
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"No GameObject was selected to combine.", "OK"
				);
				return false;
			}

			// The Selected GameObject already has a SkinnedMeshRenderer on it!
			if ( selectedGameObject.GetComponent<SkinnedMeshRenderer>() != null ){ 
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"The selected GameObject already has a SkinnedMeshRenderer component.\n\nIn order to properly combine objects, make sure the parent object does not have a SkinnedMeshRenderer already attached.", "OK"
				);
				return false;
			}

			// ------------------------------------------------------------
			//	CHECK OUR SUPPLIED PROPERTY NAMES AND TYPES
			// ------------------------------------------------------------

			// Make sure the first property name exists and is "_MainTex"
			if( propertyNames.Length > 0 && propertyNames[0] != "_MainTex" ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"The first propertyNames entry must be '_MainTex' in order to successfully combine textures.", "OK"
				);
				return false;
			}

			// Make sure the property names and types array lengths match
			if( propertyNames.Length != textureFallbacks.Length ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"The propertyNames and textureFallbacks array lengths must match.", "OK"
				);
				return false;
			}

			// ------------------------------------------------------------
			//	SCAN SKINNED MESH RENDERERS FOR SUBMESHES AND OTHER ISSUES
			// ------------------------------------------------------------

			// Cache all of the Skinned Mesh Renderers that are children of the selected GameObject
			SkinnedMeshRenderer[] SMRs = selectedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>( false );	// <- false = includeInactive

			// Create a helper variable to detect if any submeshes were found in the script
			bool subMeshesFound = false;						// <- Check if meshes have submeshes
			bool noSharedMaterialsFound = false;				// <- Check if any SMR doesn't have a shared material
			bool weirdTextureFormatsFound = false;				// <- Check if any of the textures on the materials have formats that are not Texture2D.
			bool customTilingOrScaleFound = false;				// <- Check to see if any of the materials are using tiling or custom scales

			// Texture Importer Setup list
			List<TextureImporterSetup> textureImporterSetups = new List<TextureImporterSetup>();

			// Loop through the SkinnedMeshRenderer and see if any of the meshes have submeshes...
			foreach ( SkinnedMeshRenderer smr in SMRs ){

				// If the SMR has a subMesh, that will cause issues and we make notes of which ones have issues.
				if( smr.sharedMesh != null && smr.sharedMesh.subMeshCount > 1 ){
					subMeshesFound = true;
					Debug.LogWarning("The SkinnedMeshRenderer named: " + smr.name + "' uses a mesh with submeshes which cannot be combined.", smr );
				}

				// If the SMR doesn't have a shared material, we won't be able to combine the textures
				if( smr.sharedMaterial == null ){
					noSharedMaterialsFound = true;
					Debug.LogWarning("The SkinnedMeshRenderer named: " + smr.name + "' has no sharedMaterial.", smr );
				}

				// This can happen if the main texture being used is baked into an asset
				if( smr.sharedMaterial.mainTexture != null && smr.sharedMaterial.mainTexture.GetType() != typeof(Texture2D) ){
					weirdTextureFormatsFound = true;
					Debug.LogWarning("The SkinnedMeshRenderer named: " + smr.name + "' is using main texture that is NOT a Texture2D. It is a: " + smr.sharedMaterial.mainTexture.GetType(), smr );
				}

				// WE CURRENTLY DONT SUPPORT TEXTURE TILING / OFFSETS
				// In the future we can look into making this work
				if( smr.sharedMaterial.GetTextureScale("_MainTex") != Vector2.one || smr.sharedMaterial.GetTextureOffset("_MainTex") != Vector2.zero ){
					customTilingOrScaleFound = true;
					Debug.LogWarning("The SkinnedMeshRenderer named: " + smr.name + "' is using a material with custom texture tiling or offset. MeshKit does not support texture tiling at this time. You could make this work by setting the tiling to (1,1) and offset to (0,0) in the material.", smr );
				}

				// Loop through the texture property names supplied by the user ...
				int propertyIndex = 0;
				foreach( string propertyName in propertyNames ){

					// If this skinned Mesh Renderer has a material which contains one of the property names we want to extract (eg "_MainTex")...
					if( smr.sharedMaterial.HasProperty( propertyName) ){

						// This can happen if the main texture being used is baked into an asset
						if( (Texture2D)smr.sharedMaterial.GetTexture( propertyName ) != null ){

							// Cache the path of the mainTexture in the AssetDatabase
							string path = AssetDatabase.GetAssetPath ( (Texture2D)smr.sharedMaterial.GetTexture( propertyName ) );
						
							// Cache the TextureImporter of the Main Texture
							TextureImporter imp = (TextureImporter) AssetImporter.GetAtPath (path);

							// Attempt to make it readable if it isn't, or if this is a normal map, make sure it is setup correctly
							if ( imp != null && ( 	imp.isReadable == false || 
													imp.textureCompression != TextureImporterCompression.Uncompressed || 
													defaultTextureFallbacks[propertyIndex] == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal
												)
							){

								// Add the original TextureImporter 
								textureImporterSetups.Add( new TextureImporterSetup( 
									(Texture2D)smr.sharedMaterial.GetTexture( propertyName ), imp, path )
								);
							}	
						}
					}

					// Increment Property Index
					propertyIndex++;
				}

			}

			// ------------------
			//	SHOW ANY ERRORS
			// ------------------

			// If we detected some SkinnedMeshRenderers using meshes with submeshes...
			if( subMeshesFound == true ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"Some meshes found on the selected SkinnedMeshRenderers are using submeshes which will cause issues when combining.\n\nIn order to combine them, you can use the 'Seperate' tool to seperate them into individual meshes.", "OK"
				);
				return false;
			}

			// If we found some SkinnedMeshRenderers without materials...
			if( noSharedMaterialsFound == true ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", "Some of the selected SkinnedMeshRenderers do not have a material assigned.", "OK"
				);
				return false;
			}

			// If we found some main textures on materials that are in weird formats ( not Texture2D )...
			if( weirdTextureFormatsFound == true ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", "Some of the selected SkinnedMeshRenderers are using a material that appear to be using Textures with strange formats.\n\nTo fix this, try making sure that all textures used in your materials are actual assets in your project (PNG, JPG, etc).", "OK"
				);
				return false;
			}

			// If we found some materials that are using custom tiling or scale
			if( customTilingOrScaleFound == true ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", "Some of the selected SkinnedMeshRenderers are using a material with custom texture tiling or offset. MeshKit does not support combining materials with texture tiling at this time.\n\nYou could make this work by setting the tiling to (1,1) and offset to (0,0) in the material.", "OK"
				);
				return false;
			}


			// ----------------------------------------
			//	MAKE SURE TEXTURES ARE SETUP CORRECTLY
			// ----------------------------------------

			// Cache the number of texture importer setups we have
			int textureImporterSetupsCount = textureImporterSetups.Count;

			// Loop the texture importer setups and prepare each one
			if( textureImporterSetupsCount > 0 ){

				// Helper - check if any settings were invalid
				bool wereAnyTextureImporterSettingsInvalid = false;

				// Prepare Each of the textures ...
				for( int i = 0; i < textureImporterSetupsCount; i++ ){

					// Show progress bar for each submesh
					#if SHOW_PROGRESS_BARS
						EditorUtility.DisplayProgressBar(
							"Combining Skinned Mesh Renderers", 
							"Preparing textures for combining ( " + i.ToString() + " / " + textureImporterSetups.Count.ToString() + " )", 
							(float)i / (float)textureImporterSetups.Count
						);
					#endif

					// Prepare the texture importer on each one
					textureImporterSetups[i].PrepareSettings();

					// Check if any of the texture settings were invalid ...
					if( textureImporterSetups[i].ValidateSettings() == false ){
						
						// Record we found a problem and break the loop
						wereAnyTextureImporterSettingsInvalid = true;
						Debug.LogWarning( "MESHKIT: Combine SkinnedMeshRenderer cancelled. Texture Format was invalid on the Texture2D named: " + textureImporterSetups[i].texture2D, textureImporterSetups[i].texture2D );
						break;
					}
				}

				// Handle any problems by reverting the textures back to their original settings and showing the user a message...
				if( wereAnyTextureImporterSettingsInvalid == true ){
					
					// Restore the orginal texture settings
					for( int i = 0; i < textureImporterSetups.Count; i++ ){

						// Show progress bar for each submesh
						#if SHOW_PROGRESS_BARS
							EditorUtility.DisplayProgressBar(
								"Combining Skinned Mesh Renderers",
								"Cancelling. Restoring textures to original settings ( " + i.ToString() + " / " + textureImporterSetups.Count.ToString() + " )", 
								(float)i / (float)textureImporterSetups.Count
							);
						#endif

						// Restore settings for each entry
						textureImporterSetups[i].ResetOriginalSettings();

					}

					// Remove progress bar
					EditorUtility.ClearProgressBar();

					// Show Message
					EditorUtility.DisplayDialog(	
						"Combine SkinnedMeshRenderer", "The operation was cancelled because a Texture format was found to be invalid. To fix this, try making sure the texture is using an uncompressed format such as 'RGBA32' and try again.", "OK"
					);
					return false;
				}
			}

			// ------------------------------------------------------------
			//	HANDLE MAX ATLAS SIZE
			// ------------------------------------------------------------

			// Setup a integer to help us create the packed rects later
			int maxAtlasSize = 1024;

			// Override the value based on the enum
			if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._1024 ){
				maxAtlasSize = 1024;

			} else if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._2048 ){
				maxAtlasSize = 2048;

			} else if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._4096 ){
				maxAtlasSize = 4096;

			} else if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._8192 ){
				maxAtlasSize = 8192;

			}

			// ------------------
			//	HELPER VARIABLES
			// ------------------
		 
			// Prepare the rest of the helper values
			int vertCount = 0;
			int normCount = 0;
			int tanCount = 0;
			int triCount = 0;
			int uvCount = 0;
			int boneCount = 0;
			int bpCount = 0;
			int bwCount = 0;
	 
			Transform[] bones;
			Matrix4x4[] bindPoses;
			BoneWeight[] weights;
	 
			Vector3[] verts;
			Vector3[] norms;
			Vector4[] tans;
			int[] tris;
			Vector2[] uvs;

			int vertOffset = 0;
			int normOffset = 0;
			int tanOffset = 0;
			int triOffset = 0;
			int uvOffset = 0;
			int meshOffset = 0;
			int boneSplit = 0;
			int smrCount = 0;
			int[] bCount;

			// ---------------------------------------
			//	MAKE SURE HELPER TEXTURES ARE WORKING
			// ---------------------------------------

			// Make sure we have created out helper textures (this gives us backwards-compatibility with unity 2017.4)
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Preparing Helper Textures...", 1f );
			#endif
			RecreateHelperTextures();

			
			// -----------------------------
			//	SCAN SKINNED MESH RENDERERS
			// -----------------------------

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Processing Meshes...", 1f );
			#endif

			// Loop through them
			foreach (SkinnedMeshRenderer smr in SMRs){

				// Tally up total lengths of all vertices, normals, etc.
				vertCount += smr.sharedMesh.vertices.Length;
				normCount += smr.sharedMesh.normals.Length;
				tanCount += smr.sharedMesh.tangents.Length;
				triCount += smr.sharedMesh.triangles.Length;
				uvCount += smr.sharedMesh.uv.Length;
				boneCount += smr.bones.Length;
				bpCount += smr.sharedMesh.bindposes.Length;
				bwCount += smr.sharedMesh.boneWeights.Length;
				smrCount++;
			}

			// -------------------------------------------
			//	PREPARE THE TEXTURE PROPERTY SETUP ARRAY
			// -------------------------------------------

			// Setup a TexturePropertySetup array based on shader texture property names passed to the method
			TexturePropertySetup[] texturePropertySetup = new TexturePropertySetup[ propertyNames.Length ];
			
			// Copy the property name and fallback to each of the texturePropertySetup entries. Then, create a textures array the same length as the number of SMRs.
			for( int i = 0; i < texturePropertySetup.Length; i++ ){ 
				texturePropertySetup[i] = new TexturePropertySetup();
				texturePropertySetup[i].propertyName = propertyNames[i];						// <- Cache the property name
				texturePropertySetup[i].textureFallback = textureFallbacks[i];					// <- Cache the default texture fallback
				texturePropertySetup[i].materials = new Material[smrCount];						// <- Create materials array of the same length in each property setup
				texturePropertySetup[i].textures = new Texture2D[smrCount];						// <- Create textures array of the same length in each property setup
				texturePropertySetup[i].texturesOriginallyMissing = new bool[smrCount];			// <- Create a bool array to track which textures were originally missing
				texturePropertySetup[i].colorsToBakeIntoMainTex = new Color[smrCount];			// <- What color should we bake into the main texture atlas?
				texturePropertySetup[i].tempResizedTexturesToDelete = new List<Texture2D>();	// <- Create the temp textures List
				
			}

			// -------------------------------------------
			//	SCAN ANIMATION DATA (BONES, WEIGHTS, ETC)
			// -------------------------------------------

			// Setup Helper Variables
			bCount = new int[3];	// NOTE: this becomes new int[ 0, 0, 0 ]
			bones = new Transform[boneCount];
			weights = new BoneWeight[bwCount];
			bindPoses = new Matrix4x4[bpCount];
			//textures = new Texture2D[smrCount];


			// Loop through the Skinned Mesh Renderers
			foreach (SkinnedMeshRenderer smr in SMRs){

				// Loop through the bones in each SMR (these are Transforms)
				for(int b1 = 0; b1 < smr.bones.Length; b1++){

					// Add the renderer's bone Transforms to the bones array using the first array element of bCount. 
					// Then keep track of the index by incrementing it.
					bones[bCount[0]] = smr.bones[b1];
					bCount[0]++;
				}

				// Loop through the bone weights in each SMR
				for(int b2 = 0; b2 < smr.sharedMesh.boneWeights.Length; b2++){

					// Add the mesh's bone weights and the bone indices (using the boneSplit variable as an offset) 
					// to the weights array using the second array element of bCount.
					weights[bCount[1]] = smr.sharedMesh.boneWeights[b2];
					weights[bCount[1]].boneIndex0 += boneSplit;
					weights[bCount[1]].boneIndex1 += boneSplit;
					weights[bCount[1]].boneIndex2 += boneSplit;
					weights[bCount[1]].boneIndex3 += boneSplit;
					bCount[1]++;
				}

				// Loop through the bindposes in each SMR
				for(int b3 = 0; b3 < smr.sharedMesh.bindposes.Length; b3++){

					// Add the mesh's bindposes to the bindposes array using the third array element of bCount.
					bindPoses[bCount[2]] = smr.sharedMesh.bindposes[b3];
					bCount[2]++;
				}

				// Increment the boneSplit offset using the number of bones in the current SMR
				boneSplit += smr.bones.Length;
			}

			// -----------------------------
			//	SCAN SKINNED MESHES
			// -----------------------------

			verts = new Vector3[vertCount];
			norms = new Vector3[normCount];
			tans = new Vector4[tanCount];
			tris = new int[triCount];
			uvs = new Vector2[uvCount];
		   
			// Loop through each of the Skinned Mesh Renderers
			foreach (SkinnedMeshRenderer smr in SMRs){

				// ------------------
				//	HANDLE MESH DATA
				// ------------------

				// Loop through each of the mesh triangles and copy them
				foreach (int i in smr.sharedMesh.triangles){
					tris[triOffset++] = i + vertOffset;
				}

				// Loop through each of the mesh vertices and copy them
				foreach (Vector3 v in smr.sharedMesh.vertices){
					verts[vertOffset++] = v;
				}

				// Loop through each of the mesh normals and copy them
				foreach (Vector3 n in smr.sharedMesh.normals){
					norms[normOffset++] = n;
				}

				// Loop through each of the mesh tangents and copy them
				foreach (Vector4 t in smr.sharedMesh.tangents){
					tans[tanOffset++] = t;
				}

				// Loop through each of the mesh UVs and copy them
				foreach (Vector2 uv in smr.sharedMesh.uv){
					uvs[uvOffset++] = uv;
				}

				// -------------------
				//	HANDLE PROPERTIES
				// -------------------
				
				// Copy all the textures into the entries we've setup via the texturePropertySetup array
				for( int i = 0; i < texturePropertySetup.Length; i++ ){

					// Cache the Shared Materials on each property so we have easy access to them
					texturePropertySetup[i].materials[meshOffset] = smr.sharedMaterial;

					// _MainTex Entry - check for colors
					if( i == 0 ){

						// Cache the color to bake into the MainTex if enabled
						if( bakeColorIntoMainTex == true && smr.sharedMaterial.HasProperty( "_Color" ) ){
					
							texturePropertySetup[0].colorsToBakeIntoMainTex[meshOffset] = (Color) smr.sharedMaterial.GetColor( "_Color" );
							//Debug.Log("Custom _Color detected on SMR: " + texturePropertySetup[0].colorsToBakeIntoMainTex[meshOffset] );

						// Otherwise, default is Color.white
						} else {

							texturePropertySetup[0].colorsToBakeIntoMainTex[meshOffset] = Color.white;
						}
					}

					// If the shared material has a property that matches our texture property setup ...
					if( smr.sharedMaterial.HasProperty( texturePropertySetup[i].propertyName ) ){

						// Setup the textures array
						texturePropertySetup[i].textures[meshOffset] = (Texture2D) smr.sharedMaterial.GetTexture( texturePropertySetup[i].propertyName );

					}

					// If our texture it still null ( it could be nothing was set in the shader as well ), use the fallbacks to set a new texture. 
					if( texturePropertySetup[i].textures[meshOffset] == null ){

						// Fallback to transparent black
						if( texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.TransparentBlack ){
							texturePropertySetup[i].textures[meshOffset] = texture2DBlack;

						// Fallback to white
						} else if( texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.White ){
							texturePropertySetup[i].textures[meshOffset] = texture2DWhite;

						// Fallback to grey
						} else if( texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Grey ){
							texturePropertySetup[i].textures[meshOffset] = texture2DGrey;

						// Fallback to a neutral normal
						} else if( texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal ){
							texturePropertySetup[i].textures[meshOffset] = texture2DNormal;

						}

						// Track which textures were originally missing so we can fix / resize them later
						texturePropertySetup[i].texturesOriginallyMissing[meshOffset] = true;

					}
				}

				// Increment the number of mesh offsets and disable the original SkinnedMeshRenderer
				meshOffset++;
				smr.enabled = false;

			}


			// ----------------------------
			//	MISSING MAIN TEXTURE FIXES
			// ----------------------------

			// Create a new list to track all the resized Texture2Ds we're going to make
			List<Texture2D>texture2DsCreatedOnDemandList = new List<Texture2D>();

			// NOTE: This section aims to improve missing MainTex entries. If a MainTex is missing but it has another property that isn't missing,
			// ( eg, having no main texture but a bump map ), then the fallback main texture will be resized using the same size as the bump map
			// in order to try and maintain its detail...

			// Loop through the _MainTex textures (entry 0) to see if any were originally missing ...
			for( int i = 0; i < texturePropertySetup[0].texturesOriginallyMissing.Length; i++ ){

				// Check if this entry in MainTexture was missing ...
				if( texturePropertySetup[0].texturesOriginallyMissing[i] == true ){

					// Debug
					//Debug.LogWarning( "Missing texture found in " + texturePropertySetup[0].propertyName + " at entry: " + i );

					// If there are more properties, we can get better results by matching the size of the missing maintexture to another property
					// that requires it. If we can find one, do it here ...
					for( int j = 0; j < texturePropertySetup.Length; j++ ){
						// Always skip the MainTexture
						if( j > 0 && texturePropertySetup[j].textures[i] != null ){

							// Show progress bar
							#if SHOW_PROGRESS_BARS
								EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Optimizing Missing Main Textures...", 1f );
							#endif

							// Debug
							//Debug.LogWarning( "Replacement texture found in " + texturePropertySetup[j].propertyName + " at entry: " + i );
							
							// -------------------------------------------------------------
							// CREATE NEW MAIN TEXTURE BASED ON THE SIZE OF THE REPLACEMENT
							// -------------------------------------------------------------

							// Create a new Texture2D that has the same size as the problem texture ( we need to do that to clone it )
							Texture2D newTexture2D = new Texture2D( texturePropertySetup[j].textures[i].width, texturePropertySetup[j].textures[i].height );

							// Cache the color we're going to use to fill the texture ( sampled from the original texture )
							Color ColorToFill = texturePropertySetup[0].textures[i].GetPixel(0,0);
							
							// Cache the total pixel array size
							int pixelArraysize = newTexture2D.width * newTexture2D.height;

							// Setup pixel array
							Color[] pixels = new Color[ pixelArraysize ];

							// Loop through the pixels and set it to the color to fill
							for( int c = 0; c < pixelArraysize; c++ ){ pixels[c] = ColorToFill; }

							// Then, apply it to all the pixels on the new texture
							newTexture2D.SetPixels( pixels );

							// Apply the changes
							newTexture2D.Apply();

							// -----------
							// REPLACE IT
							// -----------

							// Replace the texture
							texturePropertySetup[0].textures[i] = newTexture2D;

							// keep track of any temporary textures we created for each property so we can free it up later
							texturePropertySetup[0].tempResizedTexturesToDelete.Add( newTexture2D );

							// Also add it to the list so we can destroy it when we're done
							texture2DsCreatedOnDemandList.Add( newTexture2D );

							// break the loop once we find a replacement
							break;
						}
					}
				}
			}


			// -----------------
			//	TEXTURE PACKING
			// -----------------

			// Setup the texture atlas and packedRects
			Rect[] packedRects = new Rect[0];

			// Loop through the texturePropertySetup array
			for( int i = 0; i < texturePropertySetup.Length; i++ ){

				// Show progress bar for each texturePropertySetup property
				#if SHOW_PROGRESS_BARS
					EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Creating Texture2D assets for " + texturePropertySetup[i].propertyName + "... ( " + (i + 1).ToString() + " / " + texturePropertySetup.Length.ToString() + " )" ,  (float)(i+1) / (float)texturePropertySetup.Length );
				#endif

				// Create a new Texture2D and use the PackTextures method to both create the atlas and setup it's rects.
				texturePropertySetup[i].atlas = new Texture2D (1,1);

				// -------------------
				//	_MAINTEX PROPERTY
				// -------------------

				// If this is the first property entry ("_MainTex")...
				if( i == 0 ){ 

					// ---------------------------------------------------------------------
					// CREATE THE FIRST ATLAS AND STORE THE PACKED RECTS FOR LATER
					// ---------------------------------------------------------------------
				
					// Create the _MainTex atlas and save the packedRects into the variable we prepared earlier
					packedRects = texturePropertySetup[i].atlas.PackTextures ( texturePropertySetup[i].textures, 0, maxAtlasSize );


					// Debug Packed Rects and Atlas Size
					// Debug.LogWarning( "maxAtlasSize: " + maxAtlasSize );
					// Debug.LogWarning("texturePropertySetup[i].atlas width: " + texturePropertySetup[i].atlas.width );
					// Debug.LogWarning("texturePropertySetup[i].atlas height: " + texturePropertySetup[i].atlas.height );


					// BUGFIX: If we select a larger max atlas size (eg 4096), it is possible for Unity to return the packedRects to be a
					// smaller size if it wasn't needed (eg 2048). We need to detect this change and update the maxAtlas Size here.
					if( maxAtlasSize > texturePropertySetup[i].atlas.width && texturePropertySetup[i].atlas.width > 16 ){
						maxAtlasSize = texturePropertySetup[i].atlas.width;
					//	Debug.LogWarning( "BUGFIX - updated maxAtlasSize: " + maxAtlasSize );
					}

					// Check for non-square atlases ( this could mess things up)
					// DEV NOTE: I saw this happen once in testing and the model still worked ok. Keep an eye on it in development.
					if( texturePropertySetup[i].atlas.width != texturePropertySetup[i].atlas.height ){
						if( MeshKitGUI.verbose == true ){
							Debug.LogWarning( "MeshKit detected that Unity returned a non-square atlas! This may cause issues...");
						}
					}


					// ---------------------------------------------------------------------
					// APPLY COLOR BAKES
					// ---------------------------------------------------------------------

					// Apply Color Bakes
					if( bakeColorIntoMainTex == true ){
						BakeColorIntoMainTextureAtlas( ref texturePropertySetup[i].atlas, ref packedRects, ref texturePropertySetup );
					}

					// ---------------------------------------------------------------------
					// IF PACKED RECTS COULDN'T BE CREATED, CANCEL THE PROCESS
					// ---------------------------------------------------------------------

					// NOTE: If we gave too many textures to pack, Unity sends back null according to the docs
					// (although null doesnt work here, so lets check for length == 0)
					if( packedRects.Length == 0 ){

						// Restore the orginal texture settings
						if( textureImporterSetupsCount > 0 ){
							for( int j = 0; j < textureImporterSetups.Count; j++ ){

								// Show progress bar for each submesh
								#if SHOW_PROGRESS_BARS
									EditorUtility.DisplayProgressBar(
										"Combining Skinned Mesh Renderers",
										"Cancelling - Restoring textures to original settings ( " + i.ToString() + " / " + textureImporterSetups.Count.ToString() + " )", 
										(float)j / (float)textureImporterSetups.Count
									);
								#endif

								// Restore settings for each entry
								textureImporterSetups[j].ResetOriginalSettings();
							}
						}

						// Show Message
						EditorUtility.DisplayDialog(	
							"Combine SkinnedMeshRenderer", "A PackedRects array couldn't be returned by Unity's 'PackTextures' method. You may be trying to fit too many textures into too small a space.\n\nYou can try to resolve this issue by making the max atlas size bigger or using less textures.", "OK"
						);
						Debug.LogError( "MESHKIT: Combine SkinnedMeshRenderer operation cancelled because PackedRects array was not created by Unity.");
						return false;
					}
				
				// -------------------
				//	OTHER PROPERTIES
				// -------------------

				// If this is not the first entry, we need to make sure all new texture sizes match the original ones in _MainTex
				} else {

					// Loop through all of the _MainTex textures (entry 0)
					for( int j = 0; j < texturePropertySetup[0].textures.Length; j++ ){

						// If any texture's size in this property do not match the same MainTexture ...
						if( texturePropertySetup[i].textures[j].width != texturePropertySetup[0].textures[j].width ||
							texturePropertySetup[i].textures[j].height != texturePropertySetup[0].textures[j].height ||

							// Also do this if this is a normal map...
							texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal
						){

							// Cache the original Texture
							var originalTexture = texturePropertySetup[i].textures[j];

							// ---------------------------------------------------------------------
							// STEP 1) DUPLICATE THE TEXTURE SO WE DONT MESS UP THE ORIGINAL
							// ---------------------------------------------------------------------

							// Create a new Texture2D that has the same size as the problem texture ( we need to do that to clone it )
							Texture2D newTexture2D = new Texture2D( originalTexture.width, originalTexture.height );

							// Cache the original texture pixels
							Color[] originalTexturePixels = originalTexture.GetPixels();


							// FIX FOR NORMAL MAPS USING DXT5 TEXTURE FORMATS (red normal map fix)
							if (	texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal && 
									DoesTextureRequireRedNormalMapFix( textureImporterSetups, originalTexture ) == true
							){

								// Helper variables
								Color pixel;
								Vector2 v2RG;

								//Debug.Log( "Attempting Red Normal Map Fix on: " + originalTexture.name );
								for( int p = 0; p < originalTexturePixels.Length; p++ ){

									// Cache the original pixel
									pixel = originalTexturePixels[p];
									
									// Process the pixel by converting the range from ( 0 to 1 ) to ( -1 to +1 )
									pixel.r = pixel.a * 2 - 1;  										// Red becomes the alpha and converted
									pixel.g = pixel.g * 2 - 1; 											// Green is directly converted
									v2RG = new Vector2( pixel.r, pixel.g );								// Helper Vector2: x = red, y = green
									pixel.b = Mathf.Sqrt( 1 - Mathf.Clamp01(Vector2.Dot(v2RG, v2RG)));	// Recreate the blue value

									// Apply the Pixel, converting it back to the ( 0 to 1 ) range
									originalTexturePixels[p] = new Color( 
										pixel.r * 0.5f + 0.5f, 
										pixel.g * 0.5f + 0.5f, 
										pixel.b * 0.5f + 0.5f
									); 
									
								}
							}

							// Set the new pixels
							newTexture2D.SetPixels( originalTexturePixels );

							// Apply the changes
							newTexture2D.Apply( false );


							// ---------------------------------------------------------------------
							// STEP 2) RESIZE IT TO MATCH THE ORIGINAL _MAINTEX VERSION
							// ---------------------------------------------------------------------

							// Cache the current packed Rect Width and Height
							int currentPackedRectWidth = Mathf.RoundToInt( packedRects[j].width * maxAtlasSize );
							int currentPackedRectHeight = Mathf.RoundToInt( packedRects[j].height * maxAtlasSize );

							
							// DEBUG
							/*
							Debug.Log("original width: "+ currentPackedRectWidth );
							Debug.Log("original height: "+ currentPackedRectHeight );
							Debug.Log("original texture name: "+ texturePropertySetup[0].textures[j] );
							*/


							
							// Rescale using custom method. Unity's Texture2D.Resize version doesn't work so we need to it like this.
							// NOTE: Sometimes there is a multi-threading timing issue and this messes up. Use the non-threaded version instead.
						//	ThreadedTextureScale.Bilinear ( newTexture2D, currentPackedRectWidth, currentPackedRectHeight );
							
							//Debug.Log( texturePropertySetup[0].textures[j].name + " => w: " + newTexture2D.width + " h: " + newTexture2D.height  );

							// Apply the changes
						//	newTexture2D.Apply();
						

							// Rescale using custom method on the main thread. Unity's Texture2D.Resize version doesn't work here!
							// NOTE: This applies the texture inside the function
							TextureScale.Bilinear ( newTexture2D, currentPackedRectWidth, currentPackedRectHeight );


							//	Debug.Log( texturePropertySetup[0].textures[j].name + ": Rescaled Texture is now => w: " + newTexture2D.width + " h: " + newTexture2D.height + " should be the same as original" );


							// ---------------------------------------------------------------------
							// STEP 3) APPLY IT TO THE ARRAY AND TRACK IT SO WE CAN DELETE IT LATER
							// ---------------------------------------------------------------------

							// Replace it in the array
							texturePropertySetup[i].textures[j] = newTexture2D;

							// keep track of any temporary textures we created for each property so we can free it up later
							texturePropertySetup[i].tempResizedTexturesToDelete.Add( newTexture2D );

							// Also add it to the list so we can destroy it when we're done
							texture2DsCreatedOnDemandList.Add( newTexture2D );
						}
					}
					
					// ---------------------------------------------------------------------
					// CUSTOM ATLAS CREATION
					// ---------------------------------------------------------------------

					// Create a new texture atlas using our resized textures and the original packedRects. We define the atlas size using the one Unity created in the _MainTex atlas.
					// NOTE: This will create the texture atlas but it allows us to handle if something goes wrong in easy if statement.
					if( CreateNewAtlasFromPackedRectsAndResizedTextures ( 
							ref texturePropertySetup[i].atlas, ref texturePropertySetup[i].textures, ref packedRects, 
							//(float)texturePropertySetup[0].atlas.width, (float)texturePropertySetup[0].atlas.height,	// <- this breaks now that we have custom atlas sizes
							maxAtlasSize, maxAtlasSize,																	// This uses the updated maxAtlasSize for width and height
							MissingTextureFallbackToColor( texturePropertySetup[i].textureFallback ),					// Background color
							texturePropertySetup[i]																		// The current Texture Property Setup
						) == false
					){

						// Show Message
						EditorUtility.DisplayDialog(	
							"Combine SkinnedMeshRenderer", "An error occured while trying to pack textures for property: " + texturePropertySetup[i].propertyName, "OK"
						);
						Debug.LogError( "MESHKIT: Combine SkinnedMeshRenderer operation cancelled because an error occured while trying to pack textures for property: " + texturePropertySetup[i].propertyName);
						return false;

						// NOTE: We should also destroy existing textures created to clean memory if this happens ...
					}

					// also,if we move the temporary textures we created to the texturePropertySetup class, we can destroy all the new ones here after we're done creating the atlases.

					// ---------------------------------------------------------------------
					// FREE UP TEXTURE MEMORY AFTER EACH PROPERTY
					// ---------------------------------------------------------------------

					// Free up memory for each property by deleting the temporary textures we created
					for( int t = 0; t < texturePropertySetup[i].tempResizedTexturesToDelete.Count; t++ ){
						if( texturePropertySetup[i].tempResizedTexturesToDelete[t] != null ){
							DestroyImmediate( texturePropertySetup[i].tempResizedTexturesToDelete[t] );
						}
					}

				}

				// ---------------------------------------------------------------------
				//	SAVE THE TEXTURE ATLASES
				// ---------------------------------------------------------------------

				// Save the new Texture2D atlas as a PNG and attempt to load it back in
				File.WriteAllBytes ( saveAssetDirectoryForFileAPI + selectedGameObject.name + texturePropertySetup[i].propertyName + ".png", texturePropertySetup[i].atlas.EncodeToPNG());
				AssetDatabase.Refresh ();
				texturePropertySetup[i].atlas = (Texture2D) AssetDatabase.LoadAssetAtPath ( saveAssetDirectory + selectedGameObject.name + texturePropertySetup[i].propertyName + ".png", typeof(Texture2D));

				/*
				// FIX NORMAL MAPS: If the texture we just saved is a normal map, we should set that up in the AssetDatabase
				if ( texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal ){
					TextureImporter normalMapTI = (TextureImporter) AssetImporter.GetAtPath ( saveAssetDirectory + selectedGameObject.name + texturePropertySetup[i].propertyName + ".png" );
					if( normalMapTI != null ){
						normalMapTI.textureType = TextureImporterType.NormalMap;
						EditorUtility.SetDirty( normalMapTI );
						normalMapTI.SaveAndReimport();
					}
				}
				*/

				// Help the AssetDatabase setup the texture properly
				TextureImporter latestAtlasTI = (TextureImporter) AssetImporter.GetAtPath ( saveAssetDirectory + selectedGameObject.name + texturePropertySetup[i].propertyName + ".png" );
				if( latestAtlasTI != null ){

					// FIX NORMAL MAPS: If the texture we just saved is a normal map, we should set that up in the AssetDatabase
					if ( texturePropertySetup[i].textureFallback == MeshKitCombineSkinnedMeshSetup.MissingTextureFallback.Normal ){
						latestAtlasTI.textureType = TextureImporterType.NormalMap;
					}
					
					// Setup the Max Atlas Size
					if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._8192 ){
						latestAtlasTI.maxTextureSize = 8192;

					} else if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._4096 ){
						latestAtlasTI.maxTextureSize = 4096;

					} else if( maximumAtlasSize == MeshKitCombineSkinnedMeshSetup.MaxAtlasSize._2048 ){
						latestAtlasTI.maxTextureSize = 2048;

					} else {

						latestAtlasTI.maxTextureSize = 1024;	// <- Default to a 1024 atlas if something went wrong here
					}

					// Set dirty and re-import it
					EditorUtility.SetDirty( latestAtlasTI );
					latestAtlasTI.SaveAndReimport();
				}


			}

			// -------------------
			//	NEW MESH UV SETUP
			// -------------------

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Processing UVs...", 1f );
			#endif

			// Helpers
			uvOffset = 0;
			meshOffset = 0;
			Vector2 uvClamped = Vector2.zero;

			// Loop through each of the Skinned Mesh Renderers
			foreach (SkinnedMeshRenderer smr in SMRs) {

				// Loop through each of their UVs
				foreach (Vector2 uv in smr.sharedMesh.uv) {

					// OUT OF RANGE UV FIXES
					// If any of the UVs are outside of the 0-1 range, this approach tries to fix it...
					if( attemptToFixOutOfRangeUVs == true ){

						// Cache the original UV			   
						uvClamped = uv;
					   
						// Keep reducing / adding by 1 to get to a 0-1 range.
						while (uvClamped.x > 1){ uvClamped.x = uvClamped.x - 1; }
						while (uvClamped.x < 0){ uvClamped.x = uvClamped.x + 1; }
						while (uvClamped.y > 1){ uvClamped.y = uvClamped.y - 1; }
						//while (uvClamped.x < 0){ uvClamped.y = uvClamped.y + 1; }
						while (uvClamped.y < 0){ uvClamped.y = uvClamped.y + 1; }		// <- bugfix? 3.0.1
					   
						// Setup the new UVs using the info from the packedRects
						uvs[uvOffset].x = Mathf.Lerp (packedRects[meshOffset].xMin, packedRects[meshOffset].xMax, uvClamped.x);            
						uvs[uvOffset].y = Mathf.Lerp (packedRects[meshOffset].yMin, packedRects[meshOffset].yMax, uvClamped.y);            
						uvOffset ++;
					
					} else {

						// ORIGINAL APPROACH
						// Setup the new UVs using the info from the packedRects
						uvs[uvOffset].x = Mathf.Lerp (packedRects[meshOffset].xMin, packedRects[meshOffset].xMax, uv.x);
						uvs[uvOffset].y = Mathf.Lerp (packedRects[meshOffset].yMin, packedRects[meshOffset].yMax, uv.y);
						uvOffset ++;

					}
				}

				// Increment the meshes
				meshOffset ++;
			}

			// --------------------
			//	CREATE NEW MATERIAL
			// --------------------

			// Show progress bar
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Building new Material...", 1f );
			#endif
	 
			// Create New Material
			Material mat = new Material (Shader.Find("Standard"));

			// Apply the properties
			for( int i = 0; i < texturePropertySetup.Length; i++ ){

				// Setup Textures for each property
				mat.SetTexture( texturePropertySetup[i].propertyName, texturePropertySetup[i].atlas );

				// Special Setups - Normal Maps
				if( texturePropertySetup[i].propertyName == "_BumpMap" ){ 
					mat.EnableKeyword("_NORMALMAP");												// <- Turn On Normal Map
				}

				// Special Setups - Gloss Maps
				if( texturePropertySetup[i].propertyName == "_MetallicGlossMap" ){ 
					mat.EnableKeyword("_METALLICGLOSSMAP");											// <- Turn On Metallic Gloss Map
				}

				// Special Setups - Emission
				if( texturePropertySetup[i].propertyName == "_EmissionMap" ){ 

					mat.SetColor( "_EmissionColor", Color.white );									// <- Set Emission Color First
					mat.EnableKeyword("_EMISSION");													// <- Turn On Emission
					mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;	// <- This needs to be set for Emission to work!
				}	
			}

			// Create the material in the AssetDatabase
			AssetDatabase.CreateAsset(mat, saveAssetDirectory + selectedGameObject.name + ".mat");


			// --------------------
			//	CREATE NEW MESH
			// --------------------

			// Show progress bar
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Building new Mesh...", 1f );
			#endif
	 
			// Create New Mesh
			Mesh newMesh = new Mesh();
			newMesh.name = selectedGameObject.name;
			newMesh.vertices = verts;
			newMesh.normals = norms;
			newMesh.tangents = tans;
			newMesh.boneWeights = weights;
			newMesh.uv = uvs;
			newMesh.triangles = tris;
			newMesh.bindposes = bindPoses;
	   
			// Create New Skinned Mesh Renderer and set it up
			SkinnedMeshRenderer newSMR = selectedGameObject.AddComponent<SkinnedMeshRenderer>();
			newSMR.sharedMesh = newMesh;
			newSMR.bones = bones;
			newSMR.updateWhenOffscreen = true;
			selectedGameObject.GetComponent<Renderer>().material = mat;

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Combining Blendshapes...", 1f );
			#endif 

			// Merge all the blendshape data together and apply it back to our new mesh
			// NOTE: We should do an if block to see if any blendshape data existed first, otherwise this is an expensive operation for nothing, lol.
			MergeBlendshapesFromSkinnedMeshRenderers( ref SMRs, ref newMesh, ref newSMR, 1f );

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Creating Assets...", 1f );
			#endif

			// Create the mesh in the AssetDatabase
			AssetDatabase.CreateAsset(newMesh, saveAssetDirectory + selectedGameObject.name + "_Mesh.asset");


			// ------------------------------------------------------
			//	MAKE SURE ANIMATORS AND ANIMATIONS UPDATE OFF-SCREEN
			// ------------------------------------------------------

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Handling Animation Components...", 1f );
			#endif

			// Cache all of the child Animators and make sure they are set to always update (also track original values)
			Animator[] originalAnimators = selectedGameObject.GetComponentsInChildren<Animator>() as Animator[];
			AnimatorCullingMode[] originalAnimatorCullingModes = new AnimatorCullingMode[ originalAnimators.Length ];
			for( int i = 0; i < originalAnimators.Length; i++ ){	
				
				originalAnimatorCullingModes[i] = originalAnimators[i].cullingMode;
				originalAnimators[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;
				EditorUtility.SetDirty( originalAnimators[i] );
			}
			
			// Cache all of the child Animations and make sure they are set to always update (also track original values)
			Animation[] originalAnimations = selectedGameObject.GetComponentsInChildren<Animation>() as Animation[];
			AnimationCullingType[] originalAnimationCullingTypes = new AnimationCullingType[ originalAnimations.Length ];
			for( int i = 0; i < originalAnimations.Length; i++ ){	
				
				originalAnimationCullingTypes[i] = originalAnimations[i].cullingType;
				originalAnimations[i].cullingType = AnimationCullingType.AlwaysAnimate;
				EditorUtility.SetDirty( originalAnimations[i] );
			}


			// ------------------------------------------------------
			//	SETUP MESHKIT COMBINE SKINNEDMESH COMPONENT
			// ------------------------------------------------------
			
			// Cache the MeshKitCombineSkinnedMeshSetup component if it exists
			var mkcsms = selectedGameObject.GetComponent<MeshKitCombineSkinnedMeshSetup>();
			if( mkcsms != null ){

				// Cache the original Skinned Mesh Renderers
				mkcsms.originalSMRs = SMRs;

				// Cache the new SMR
				mkcsms.newSMR = newSMR;

				// Cache Original Animator / Animaions and culling values
				mkcsms.originalAnimators = originalAnimators;
				mkcsms.originalAnimatorCullingModes = originalAnimatorCullingModes;
				mkcsms.originalAnimations = originalAnimations;
				mkcsms.originalAnimationCullingTypes = originalAnimationCullingTypes;

				// Set generated to true and update the generated Combine Mode
				mkcsms.generated = true;
				mkcsms.generatedCombineMode = MeshKitCombineSkinnedMeshSetup.CombineMode.CombineMeshesWithTextureAtlasing;
			}


			// ------------------------------------------------------
			//	RESTORE ORIGINAL TEXTURE SETTINGS 
			// ------------------------------------------------------

			// Restore the orginal texture settings
			if( textureImporterSetupsCount > 0 ){
				for( int i = 0; i < textureImporterSetups.Count; i++ ){

					// Show progress bar for each submesh
					#if SHOW_PROGRESS_BARS
						EditorUtility.DisplayProgressBar(
							"Combining Skinned Mesh Renderers",
							"Restoring textures to original settings ( " + i.ToString() + " / " + textureImporterSetups.Count.ToString() + " )", 
							(float)i / (float)textureImporterSetups.Count
						);
					#endif

					// Restore settings for each entry
					textureImporterSetups[i].ResetOriginalSettings();
				}
			}

			// -------------------
			//	CLEAN MEMORY
			// -------------------

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Cleaning Memory...", 1f );
			#endif

			// Destroy all the textures we created previously on demand to clean up memory
			for( int i = 0; i < texture2DsCreatedOnDemandList.Count; i++ ){
				if( texture2DsCreatedOnDemandList[i] != null ){ DestroyImmediate( texture2DsCreatedOnDemandList[i] ); }
			}

			// Clear Progress Bar
			EditorUtility.ClearProgressBar();

			// Return true if the operation completed successfully
			return true;

		}

/// -> 		[EDITOR ONLY] PROPERTY REQUIRES PIXEL PROCESSING

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] PROPERTY REQUIRES PIXEL PROCESSING
		//	This method looks at the property name in a material and determines whether we should also be factoring in another property value as well.
		//	For example, if we are using the _MetallicGlossMap property, we should also look at the _Metallic Value as well as _Smoothness and bake it in to the value.
		//	The method simply checks to see if we need to do this and returns true or false.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Check to see if this property requires pixel processing ...
		static bool PropertyRequiresPixelProcessing( string propertyName, Material material ){
			if( propertyName == "_MetallicGlossMap" && material.HasProperty( "_GlossMapScale" ) ){ return true; }
			if( propertyName == "_OcclusionMap" && material.HasProperty( "_OcclusionStrength" ) ){ return true; }
			if( propertyName == "_EmissionMap" && material.IsKeywordEnabled("_EMISSION") ){ return true; }
			return false;
		}

/// -> 		[EDITOR ONLY] PROCESS PIXEL USING PROPERTY NAME AND MATERIAL

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] PROCESS PIXELS USING PROPERTY NAME AND MATERIAL
		//	This method looks at the property name in a material and determines whether we should also be factoring in another property value as well.
		//	For example, if we are using the _MetallicGlossMap property, we should also look at the _Metallic Value as well as _Smoothness and bake it in to the value.
		//	This method actually handles the processing of an array of pixels in a texture. We should call 'PropertyRequiresPixelProcessing() == true ' first.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		static void ProcessPixelsUsingPropertyNameAndMaterial( ref Color[] pixels, string propertyName, Material material, bool textureWasOriginallyNull ){

			// ---------------------------------------
			//	CACHE WHICH PROPERTY WE'RE PROCESSING
			// ---------------------------------------

			// Cache the property we're seting up first to speed up the loop
			bool processingMetallicGlossMap = ( propertyName == "_MetallicGlossMap" && material.HasProperty( "_GlossMapScale" ) );	
			bool processingOcclusionMap = ( propertyName == "_OcclusionMap" && material.HasProperty( "_OcclusionStrength" ) );
			bool processingEmissionMap = ( propertyName == "_EmissionMap" && material.IsKeywordEnabled("_EMISSION") );


			//if(processingEmissionMap){ Debug.Log("Found _Emission keyword on material: " + material.name + " - propertyname: " + propertyName + " - texture was originally null: " + textureWasOriginallyNull ); }

			// -----------------
			//	PROCESS PIXELS
			// -----------------

			// Loop through the pixels
			int pixelsLength = pixels.Length;
			for( int i = 0; i < pixelsLength; i++ ){ 

				// -----------------------------
				//	METALLIC GLOSS MAP PROPERTY
				// -----------------------------

				if( processingMetallicGlossMap == true ){

					// Setup a new modifier color ( default is white which means no color modifications will take place)
					Color newModifierColor = Color.white;

					// Metallic Gloss Maps require the "smoothness" to be setup on the alpha so we multiply the pixel's alpha channel with the GlossMapScale
					pixels[i].a *= material.GetFloat( "_GlossMapScale" );

				}

				// -----------------------------
				//	OCCLUSION MAP PROPERTY
				// -----------------------------

				else if( processingOcclusionMap == true ){

					// Occlusion map using OcclusionStrength to determine the strength of the effect. To mimic its effect on the texture, we lerp from white.
					pixels[i] = Color.Lerp( Color.white, pixels[i], material.GetFloat( "_OcclusionStrength" ) );

				}

				// -----------------------------
				//	EMISSION MAP PROPERTY
				// -----------------------------

				else if( processingEmissionMap == true ){

					// Occlusion map using OcclusionStrength to determine the strength of the effect. To mimic its effect on the texture, we lerp from white.
					//pixels[i] = Color.Lerp( Color.white, pixels[i], material.GetFloat( "_OcclusionStrength" ) );


					// Cache the emission Color
					Color emissionColor = material.HasProperty( "_EmissionColor" ) ? material.GetColor( "_EmissionColor" ) : Color.black;

					// If the Emission texture originally didn't exist ...
					if( textureWasOriginallyNull ){

						// Override the RGB values with the emission color
						pixels[i].r = emissionColor.r;
						pixels[i].g = emissionColor.g;
						pixels[i].b = emissionColor.b;

					// if it did exist ...
					} else {

						// Multiply the RGB values with the emission color
						pixels[i].r *= emissionColor.r;
						pixels[i].g *= emissionColor.g;
						pixels[i].b *= emissionColor.b;

					}
				}
			}
		}

/// -> 		[EDITOR ONLY] CREATE NEW ATLAS FROM PACKED RECTS AND RESIZED TEXTURES

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] CREATE NEW ATLAS FROM PACKED RECTS AND RESIZED TEXTURES
		//	This method creates an atlas at the correct size and places previously resized textures into it using the packedRects array.
		//	Returns true if successful.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		static bool CreateNewAtlasFromPackedRectsAndResizedTextures( ref Texture2D atlas, ref Texture2D[] textures, ref Rect[] rects, float originalAtlasWidth, float originalAtlasHeight, Color backgroundColor, TexturePropertySetup texturePropertySetup ){
			
			// Make sure the texture and packedRects arrays are the same length
			if( textures.Length == rects.Length ){

				// Make this atlas the same size as the first atlas we created with PackTextures.
				atlas = new Texture2D( (int)originalAtlasWidth, (int)originalAtlasHeight );

				// DEBUG
				// Debug.LogWarning( "Original Atlas Width / Height =  w: " + originalAtlasWidth  + " h: " + originalAtlasHeight );

				// ----------------------------
				// HANDLE THE BACKGROUND
				// ----------------------------

				// Create the fill colours for the background
				Color32[] fillPixels = new Color32[ atlas.width * atlas.height ];
				int fillPixelsLength = fillPixels.Length;
				for( int p = 0; p < fillPixelsLength; p++ ){ fillPixels[p] = backgroundColor; }

				// Set the background color on the atlas
				atlas.SetPixels32( 0, 0, atlas.width, atlas.height, fillPixels );

				// DEBUG AFTER SETTING PIXELS
				// Debug.LogWarning( "New Atlas Width / Height (after setting background pixels - should match original ) =  w: " + atlas.width  + " h: " + atlas.height );

				// ----------------------------
				// HANDLE EACH OF THE TEXTURES
				// ----------------------------

				// Helper Values
				int i = 0;																// <- Setup an index to increment through the textures
				//Color propertyColorModifier = Color.white;							// <- Setup a color to multiply with ( based on property )
				Color[] modifiedTexturePixels = new Color[0];							// <- Setup a re-usable color array to modify any textures we need

				// Loop through each of the textures in our array
				foreach( Texture2D t2d in textures ){

					// Debug size of each texture
					// Debug.Log( "NEW TEXTURE " + i.ToString() + " - textures[i].width: " + textures[i].width + " textures[i].height: " + textures[i].height );

					// ------------------------------
					// USING PROPERTY COLOR MODIFIER
					// ------------------------------

					// Cache the property color modifier for this texture by looking at its respective property name and material
					//propertyColorModifier = PropertyNameToColorModifier( texturePropertySetup.propertyName, texturePropertySetup.materials[i] );

					// Before we apply the pixels, we need to modify them with the property color modifier if needed
					//if( propertyColorModifier != Color.white ){

					// Check to see if this property requires extra pixel processing ...
					if( PropertyRequiresPixelProcessing( texturePropertySetup.propertyName, texturePropertySetup.materials[i] ) == true ){

						// Cache the pixels
						modifiedTexturePixels = t2d.GetPixels();

						// Process the pixels using data from the property name and material being used
						ProcessPixelsUsingPropertyNameAndMaterial( 
							ref modifiedTexturePixels, texturePropertySetup.propertyName, texturePropertySetup.materials[i], texturePropertySetup.texturesOriginallyMissing[i]
						);

						// DEBUG
						/*
						Debug.Log( "A) rects[i].x = " + rects[i].x );
						Debug.Log( "A) rects[i].y = " + rects[i].y );
						Debug.Log( "A) t2d.width = " + t2d.width );
						Debug.Log( "A) t2d.height = " + t2d.height );
						Debug.Log( "A) Mathf.FloorToInt(rects[i].x * originalAtlasWidth) = " + Mathf.FloorToInt(rects[i].x * originalAtlasWidth) );
						Debug.Log( "A) Mathf.FloorToInt(rects[i].y * originalAtlasHeight) = " + Mathf.FloorToInt(rects[i].y * originalAtlasHeight) );
						*/

						// Draw each of the textures into the atlas
						atlas.SetPixels(	Mathf.FloorToInt(rects[i].x * originalAtlasWidth), 		// X
											Mathf.FloorToInt(rects[i].y * originalAtlasHeight), 	// Y
											t2d.width, 												// Width of texture (this is already resized)
											t2d.height, 											// Height of texture (this is already resized)
											modifiedTexturePixels, 									// Use the modified texture pixels
											0 														// Mipmap level
						);

					// ------------------------------
					// WITHOUT COLOR MODIFIER
					// ------------------------------

					// The fast way ( without property color modifications ) ...
					} else {

						// DEBUG
						/*
						Debug.Log( "B) rects[i].x = " + rects[i].x );
						Debug.Log( "B) rects[i].y = " + rects[i].y );
						Debug.Log( "B) t2d.width = " + t2d.width );
						Debug.Log( "B) t2d.height = " + t2d.height );
						Debug.Log( "B) Mathf.FloorToInt(rects[i].x * originalAtlasWidth) = " + Mathf.FloorToInt(rects[i].x * originalAtlasWidth) );
						Debug.Log( "B) Mathf.FloorToInt(rects[i].y * originalAtlasHeight) = " + Mathf.FloorToInt(rects[i].y * originalAtlasHeight) );
						*/

						// Draw each of the textures into the atlas
						atlas.SetPixels32(	Mathf.FloorToInt(rects[i].x * originalAtlasWidth), 		// X
											Mathf.FloorToInt(rects[i].y * originalAtlasHeight), 	// Y
											t2d.width, 												// Width of texture (this is already resized)
											t2d.height, 											// Height of texture (this is already resized)
											t2d.GetPixels32(), 										// Pixels from texture
											0 														// Mipmap level
						);

					}

					// Increment rect index
					i++;
				}

				// Done!
				atlas.Apply();
				return true;


			// Otherwise show error message and return false
			} else {
				Debug.LogError( "MESHKIT: SkinnedMeshRenderer Combine couldn't create atlas because the textures and packed rects array length were not the same.");
			}

			// Something went wrong
			return false;
		}

/// -> 		[EDITOR ONLY] BAKE COLOR INTO MAIN TEXTURE ATLAS

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] BAKE COLOR INTO MAIN TEXTURE ATLAS
		//	This method allows us to mix in the _Color property directly into _MainTex atlas. It doesnt need to return true or false because it shouldn't mess up the
		//	combine process even if it fails.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		static void BakeColorIntoMainTextureAtlas( ref Texture2D mainTextureAtlas, ref Rect[] rects, ref TexturePropertySetup[] texturePropertySetup ){
			
			// Make sure the texturePropertySetup at entry 0 (_MainTex) and packedRects arrays are the same length
			if( texturePropertySetup.Length > 0 && texturePropertySetup[0].colorsToBakeIntoMainTex.Length == rects.Length ){

				// Make sure the atlas isn't null for extra security...
				if( mainTextureAtlas != null ){

					// Cache the atlas width and height
					float atlasWidth = (float)mainTextureAtlas.width;
					float atlasHeight = (float)mainTextureAtlas.height;

					// Helpers
					int x = 0;
					int y = 0;
					int width = 0;
					int height = 0;
					int pixelsLength = 0;
					Color colorToBlend = Color.white;

					// Show progress bar for each submesh
					#if SHOW_PROGRESS_BARS
						EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Baking Colors Into Main Texture Atlas...", 1f );
					#endif

					// Loop through the first texture property's (_MainTex) colors array ...
					for( int i = 0; i < texturePropertySetup[0].colorsToBakeIntoMainTex.Length; i++ ){

						// Cache the current color to blend
						colorToBlend = texturePropertySetup[0].colorsToBakeIntoMainTex[i];

						// If the current color to blend isn't white, then process the pixels ...
						if( colorToBlend != Color.white ){

							// Calculate the pixel area of each part of the main texture atlas ...
							x = Mathf.FloorToInt( rects[i].x * atlasWidth );
							y = Mathf.FloorToInt( rects[i].y * atlasHeight );
							width = Mathf.FloorToInt( rects[i].width * atlasWidth );
							height = Mathf.FloorToInt( rects[i].height * atlasHeight );

							// Get the pixels of each part of the main texture atlas ...
							Color[] pixels = mainTextureAtlas.GetPixels( x, y, width, height );

							// Cache the length of the pixels array
							pixelsLength = pixels.Length;

							// Process the pixels ...
							for( int p = 0; p < pixelsLength; p++ ){

								// Multiply the Atlas texture area with the cached _Color property
								pixels[p] = pixels[p] * colorToBlend;
							}

							// Set the pixels back
							mainTextureAtlas.SetPixels( x, y, width, height, pixels, 0 );

						}
					}

					// Apply the Texture atlas at the end
					mainTextureAtlas.Apply();


				// Otherwise show error message
				} else {
					Debug.LogError( "MESHKIT: SkinnedMeshRenderer Combine couldn't create bake color into the main texture atlas because the supplied atlas was null.");
				}
				
			// Otherwise show error message and return false
			} else {
				Debug.LogError( "MESHKIT: SkinnedMeshRenderer Combine couldn't create bake color into the main texture atlas because the colorsToBakeIntoMainTex and rects array length were not the same.");
			}
		}

/// -> [EDITOR ONLY] COMBINE SKINNED MESH RENDERERS (NO ATLASSING)

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] COMBINE SKINNED MESH RENDERERS (NO ATLASSING)
		//	This Version doesn't do any texture atlassing. It creates a mesh with loads of submeshes and puts all the materials into a material array.
		//	BENEFITS: Reduces all skinned mesh renderers to one. However, no draw call or shadow caster optimizations occur.
		//	Returns true if successful, false if it failed.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Debug Method
		/*
		[MenuItem ("Assets/Combine SkinnedMeshRenderers (No Atlassing)")]
		public static bool CombineSkinnedMeshRenderersNoAtlas(){ return CombineSkinnedMeshRenderersNoAtlas( Selection.activeGameObject ); }
		*/

		// Full Method
		public static bool CombineSkinnedMeshRenderersNoAtlas( GameObject selectedGameObject, bool combineInactiveRenderers = false ){

			// ------------------------------------------------------------
			//	FILEPATHS
			// ------------------------------------------------------------

			// If the Support Folders don't exist, end now ...
			if( MeshAssets.HaveSupportFoldersBeenCreated() == false ){
				EditorUtility.DisplayDialog(	
					"Seperate Skinned Mesh Renderer", 
					"Cannot save meshes because MeshKit support folders for this scene have not been created!",
					"Okay"
				);
				return false;
			}

			// ------------------------------------------------------------
			//	INITIAL GAMEOBJECT CHECKS
			// ------------------------------------------------------------

			// End early if we didnt select a GameObject
			if ( selectedGameObject == null){ 
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"No GameObject was selected to combine.", "OK"
				);
				return false;
			}

			// The Selected GameObject already has a SkinnedMeshRenderer on it!
			if ( selectedGameObject.GetComponent<SkinnedMeshRenderer>() != null ){ 
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"The selected GameObject already has a SkinnedMeshRenderer component.\n\nIn order to properly combine objects, make sure the parent object does not have a SkinnedMeshRenderer already attached.", "OK"
				);
				return false;
			}

			// -------------------------------------------
			//	SCAN SKINNED MESH RENDERERS FOR SUBMESHES
			// -------------------------------------------

			// Cache all of the Skinned Mesh Renderers that are children of the selected GameObject
			SkinnedMeshRenderer[] SMRs = selectedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>( combineInactiveRenderers );	// <- default = false

			// Create a helper variable to detect if any submeshes were found in the script
			bool subMeshesFound = false;

			// Loop through the SkinnedMeshRenderer and see if any of the meshes have submeshes...
			foreach (SkinnedMeshRenderer smr in SMRs){

				// If they do, that will cause issues and we make notes of which ones have issues.
				if( smr.sharedMesh != null && smr.sharedMesh.subMeshCount > 1 ){
					subMeshesFound = true;
					Debug.LogWarning("The SkinnedMeshRenderer named: " + smr.name + "' uses a mesh with submeshes which cannot be combined.", smr );
				}
			}

			// If we detected submeshes, show the user a prompt and end the process now...
			if( subMeshesFound == true ){
				EditorUtility.DisplayDialog(	
					"Combine SkinnedMeshRenderer", 
					"Some meshes found on the selected SkinnedMeshRenderers are using submeshes which will cause issues when combining.\n\nIn order to combine them, you can use the 'Seperate' tool to seperate them into individual meshes.", "OK"
				);
				return false;
			}

			// ------------------
			//	HELPER VARIABLES
			// ------------------

			// Prepare the rest of the helper variables	 
			int vertCount = 0;
			int normCount = 0;
			int tanCount = 0;
			int triCount = 0;
			int uvCount = 0;
			int boneCount = 0;
			int bpCount = 0;
			int bwCount = 0;
	 
			Transform[] bones;
			Matrix4x4[] bindPoses;
			BoneWeight[] weights;
	 
			Vector3[] verts;
			Vector3[] norms;
			Vector4[] tans;
			Vector2[] uvs;
			List<int[]> subMeshes;
			Material[] mats;
	 
			int vertOffset = 0;
			int normOffset = 0;
			int tanOffset = 0;
			int triOffset = 0;
			int uvOffset = 0;
			int meshOffset = 0;
	 
			int boneSplit = 0;
			int smrCount = 0;
	 
			int[] bCount;
	 
			// -----------------------------
			//	SCAN SKINNED MESH RENDERERS
			// -----------------------------

			// Show progress bar
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Scanning Mesh Data...", 1f / 3f );
			#endif
	 
			// Loop through the SkinnedMeshRenderers
			foreach (SkinnedMeshRenderer smr in SMRs) {

				// Tally up total lengths of all vertices, normals, etc.
				vertCount += smr.sharedMesh.vertices.Length;
				normCount += smr.sharedMesh.normals.Length;
				tanCount += smr.sharedMesh.tangents.Length;
				triCount += smr.sharedMesh.triangles.Length;
				uvCount += smr.sharedMesh.uv.Length;
				boneCount += smr.bones.Length;
				bpCount += smr.sharedMesh.bindposes.Length;
				bwCount += smr.sharedMesh.boneWeights.Length;
				smrCount++;
			}
	 
			// -------------------------------------------
			//	SCAN ANIMATION DATA (BONES, WEIGHTS, ETC)
			// -------------------------------------------

			// Setup Helper Variables
			bCount = new int[3];
			bones = new Transform[boneCount];
			weights = new BoneWeight[bwCount];
			bindPoses = new Matrix4x4[bpCount];
			mats = new Material[smrCount];
	 
			// Loop through each of the Skinned Mesh Renderers
			foreach (SkinnedMeshRenderer smr in SMRs) {
			   
				// Loop through the bones
				for(int b1 = 0; b1 < smr.bones.Length; b1++) {
				   
					// Add the renderer's bone Transforms to the bones array using the first array element of bCount. 
					// Then keep track of the index by incrementing it.
					bones[bCount[0]] = smr.bones[b1];
					bCount[0]++;
				}
	 
				// Loop through the bone weights
				for(int b2 = 0; b2 < smr.sharedMesh.boneWeights.Length; b2++){
	 
					// Add the mesh's bone weights and the bone indices (using the boneSplit variable as an offset) 
					// to the weights array using the second array element of bCount.
					weights[bCount[1]] = smr.sharedMesh.boneWeights[b2];
					weights[bCount[1]].boneIndex0 += boneSplit;
					weights[bCount[1]].boneIndex1 += boneSplit;
					weights[bCount[1]].boneIndex2 += boneSplit;
					weights[bCount[1]].boneIndex3 += boneSplit;
	 
					bCount[1]++;
				}
	 
				// Loop through the bone bindposes
				for(int b3 = 0; b3 < smr.sharedMesh.bindposes.Length; b3++){
	 
					// Add the mesh's bindposes to the bindposes array using the third array element of bCount.
					bindPoses[bCount[2]] = smr.sharedMesh.bindposes[b3];
					bCount[2]++;
				}
	 
				// Increment the boneSplit offset using the number of bones in the current SMR
				boneSplit += smr.bones.Length;
			}
	 
			// -----------------------------
			//	SCAN SKINNED MESHES
			// -----------------------------

			verts = new Vector3[vertCount];
			norms = new Vector3[normCount];
			tans = new Vector4[tanCount];
			subMeshes = new List<int[]>();
			uvs = new Vector2[uvCount];
	 
			// Loop through each of the Skinned Mesh Renderers
			foreach (SkinnedMeshRenderer smr in SMRs){
			   
				// Cache the current set of triangles
				int[] theseTris = new int[smr.sharedMesh.triangles.Length];

				// Loop through each of the mesh triangles and copy them
				foreach (int i in smr.sharedMesh.triangles){
					theseTris[triOffset++] = i + vertOffset;
				}
			   
				// Add this set of triangles to the submeshes list and reset the triOffset value
				subMeshes.Add(theseTris);
				triOffset = 0;
	 
				// Copy the vertices to the verts array array using vertOffset + 1 as the index
				foreach (Vector3 v in smr.sharedMesh.vertices){
					verts[vertOffset++] = v;
				}
	 
				// Copy the normals to the norms array array using normOffset + 1 as the index
				foreach (Vector3 n in smr.sharedMesh.normals){
					norms[normOffset++] = n;
				}
	 
				// Copy the tangents to the tans array array using tanOffset + 1 as the index
				foreach (Vector4 t in smr.sharedMesh.tangents){
					tans[tanOffset++] = t;
				}
	 
				// Copy the UVs to the uvs array array using uvOffset + 1 as the index
				foreach (Vector2 uv in smr.sharedMesh.uv){
					uvs[uvOffset++] = uv;
				}
			   
				// Set the current shared material to the mats array and increase the meshOffset
				mats[meshOffset] = smr.sharedMaterial;
				meshOffset++;

				// Undo state of Skinnded Mesh Renderer
				Undo.RecordObject ( smr, "Combining Skinned Mesh Renderers");
	 
				// Disable the original Skinned Mesh Renderer
				smr.enabled = false;
			}
			
			// -------------------
			//	CREATE NEW ASSETS
			// -------------------

			// Show progress bar
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Creating New Assets...", 2f / 3f );
			#endif

			// Create New Mesh
			Mesh newMesh = new Mesh();
			newMesh.name = selectedGameObject.name;
			newMesh.vertices = verts;
			newMesh.normals = norms;
			newMesh.tangents = tans;
			newMesh.boneWeights = weights;
			newMesh.uv = uvs;
			newMesh.subMeshCount = subMeshes.Count;
		   
			// Loop through the submeshes and set their triangles
			for( int subMesh = 0; subMesh < subMeshes.Count; subMesh++ ) {
				newMesh.SetTriangles( subMeshes[subMesh], subMesh );
			}
	 
			// Apply the bindposes AFTER we create the asset
			newMesh.bindposes = bindPoses;
	 
			// Create a new SkinnedMeshRenderer on the selected object
			//SkinnedMeshRenderer newSMR = selectedGameObject.AddComponent<SkinnedMeshRenderer>();
			SkinnedMeshRenderer newSMR = Undo.AddComponent<SkinnedMeshRenderer>(selectedGameObject);
	 
			// Setup the new SkinnedMeshRenderer
			newSMR.sharedMesh = newMesh;
			newSMR.bones = bones;
			newSMR.updateWhenOffscreen = true;
		   
			// Setup the material array to all the previous materials.
			selectedGameObject.GetComponent<Renderer>().sharedMaterials = mats;


			// Merge all the blendshape data together and apply it back to our new mesh
			// NOTE: We should do an if block to see if any blendshape data existed first, otherwise this is an expensive operation for nothing, lol.
			MergeBlendshapesFromSkinnedMeshRenderers( ref SMRs, ref newMesh, ref newSMR, 1f );



			// ------------------------------------------------------
			//	SAVE THE NEW MESH TO MESHKIT'S LOCAL COMBINE FOLDER
			// ------------------------------------------------------

			// Then, create the asset
			//AssetDatabase.CreateAsset(newMesh, saveAssetDirectory + selectedGameObject.name + "mesh.asset");
				
			// Try to save the mesh to MeshKit's Combine folder
			bool saveMeshSuccessfully = SaveCombinedSkinnedMesh( newMesh, newSMR );
			if( saveMeshSuccessfully == false ){

				// Show Error Message To User:
				EditorUtility.DisplayDialog("Skinned Mesh Combiner", "Unfortunately there was a problem with the combined mesh of GameObject: \n\n"+newSMR.gameObject.name + "\n\nThe Combine process must now be aborted.", "Okay");

				// Show progress bar for each submesh
				#if SHOW_PROGRESS_BARS
					EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Attempting to undo the previous steps...", 1f );
				#endif

				// -----------------------
				// TRY TO RESTORE OBJECTS
				// -----------------------

				// Destroy the new Skinned Mesh Renderer
				Object.DestroyImmediate( newSMR );

				// Loop through each of the Skinned Mesh Renderers
				foreach (SkinnedMeshRenderer smr in SMRs){
				
					// Disable the original Skinned Mesh Renderer
					smr.enabled = true;
				}

				// Clear Progress Bar
				EditorUtility.ClearProgressBar();

				// Return false because something failed
				return false;

			}


			// ------------------------------------------------------
			//	MAKE SURE ANIMATORS AND ANIMATIONS UPDATE OFF-SCREEN
			// ------------------------------------------------------

			// Show progress bar for each submesh
			#if SHOW_PROGRESS_BARS
				EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Handling Animation Components...", 3f / 3f );
			#endif

			// Cache all of the child Animators and make sure they are set to always update (also track original values)
			Animator[] originalAnimators = selectedGameObject.GetComponentsInChildren<Animator>() as Animator[];
			AnimatorCullingMode[] originalAnimatorCullingModes = new AnimatorCullingMode[ originalAnimators.Length ];
			for( int i = 0; i < originalAnimators.Length; i++ ){	
				
				Undo.RecordObject ( originalAnimators[i], "Combining Skinned Mesh Renderers");
				originalAnimatorCullingModes[i] = originalAnimators[i].cullingMode;
				originalAnimators[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;
				EditorUtility.SetDirty( originalAnimators[i] );
			}
			
			// Cache all of the child Animations and make sure they are set to always update (also track original values)
			Animation[] originalAnimations = selectedGameObject.GetComponentsInChildren<Animation>() as Animation[];
			AnimationCullingType[] originalAnimationCullingTypes = new AnimationCullingType[ originalAnimations.Length ];
			for( int i = 0; i < originalAnimations.Length; i++ ){	
				
				Undo.RecordObject ( originalAnimations[i], "Combining Skinned Mesh Renderers");
				originalAnimationCullingTypes[i] = originalAnimations[i].cullingType;
				originalAnimations[i].cullingType = AnimationCullingType.AlwaysAnimate;
				EditorUtility.SetDirty( originalAnimations[i] );
			}


			// ------------------------------------------------------
			//	SETUP MESHKIT COMBINE SKINNEDMESH COMPONENT
			// ------------------------------------------------------
			
			// Cache the MeshKitCombineSkinnedMeshSetup component if it exists
			var mkcsms = selectedGameObject.GetComponent<MeshKitCombineSkinnedMeshSetup>();
			if( mkcsms != null ){

				// Cache the original Skinned Mesh Renderers
				mkcsms.originalSMRs = SMRs;

				// Cache the new SMR
				mkcsms.newSMR = newSMR;

				// Cache Original Animator / Animaions and culling values
				mkcsms.originalAnimators = originalAnimators;
				mkcsms.originalAnimatorCullingModes = originalAnimatorCullingModes;
				mkcsms.originalAnimations = originalAnimations;
				mkcsms.originalAnimationCullingTypes = originalAnimationCullingTypes;

				// Set generated to true and update the generated Combine Mode
				mkcsms.generated = true;
				mkcsms.generatedCombineMode = MeshKitCombineSkinnedMeshSetup.CombineMode.CombineMeshesWithMaterialArray;
			}

			// ------------------------------------------------------
			//	RETURN TRUE
			// ------------------------------------------------------

			// Clear Progress Bar
			EditorUtility.ClearProgressBar();

			// Return true if the process succeeded
			return true;

		}

/// -> [EDITOR ONLY] SAVE COMBINED MESH

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SAVE COMBINED SKINNED MESH
		//	Saves a Skinned Mesh to MeshKit's local Combine folder. Returns true if successful.
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static bool SaveCombinedSkinnedMesh( Mesh newMesh, SkinnedMeshRenderer smr ){

			// If the Support Folders exist ... Save The Mesh!
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// Create and return the Mesh
				Mesh cMesh = MeshAssets.CreateMeshAsset( newMesh, MeshAssets.ProcessFileName(MeshAssets.combinedMeshFolder, smr.gameObject.name + " [SMR]", "Combined", false) );

				// If the Mesh was created successfully, re-apply it to the skinned mesh renderer
				if( cMesh!=null){ 
					smr.sharedMesh = cMesh;
					return true;

				}

				// ========================
				//	ON ERROR
				// ========================

				// Was there an error creating the last asset?
				if(MeshAssets.errorCreatingAsset){

					// Reset the flag
					MeshAssets.errorCreatingAsset = false;

				}
			}

			// If something goes wrong, return false.
			return false;
		}

/// -> [EDITOR ONLY] MERGE BLENDSHAPES FROM SKINNED MESH RENDERERS

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] MERGE BLENDSHAPES FROM SKINNED MESH RENDERERS
		//	This method merges all the blendshape data together
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Constant string helpers
		const string k_SPACE_OPENSQUAREDBRACKET = " [";
		const string k_CLOSESQUAREDBRACKET = "]";

		// Blendshape Merge Data - Helper Class
		class BlendshapeMergeData {

			public string name = "";												// <- Temporary blendshape name
			public string nameOnMesh = "";											// <- The name used on the mesh may be modified if duplicates are found.
			public int index = -1;													// <- The blendShape Index
			public float weight = 0.0f;												// <- The BlendShape Weight
			public List<Vector3> firstFrameDeltaVertices = new List<Vector3>();		// <- First Frame Delta Vertices
			public List<Vector3> firstFrameDeltaNormals = new List<Vector3>();		// <- First Frame Delta Normals
			public List<Vector3> firstFrameDeltaTangents = new List<Vector3>();		// <- First Frame Delta Tangents
			public List<Vector3> lastFrameDeltaVertices = new List<Vector3>();		// <- Last Frame Delta Vertices
			public List<Vector3> lastFrameDeltaNormals = new List<Vector3>();		// <- Last Frame Delta Normals
			public List<Vector3> lastFrameDeltaTangents = new List<Vector3>();		// <- Last Frame Delta Tangents
			
		}

		// Method
		static void MergeBlendshapesFromSkinnedMeshRenderers (

			// Arguments
			ref SkinnedMeshRenderer[] skinnedMeshRendererArray, 			// <- The array of Skinned Mesh Renderers we are using to extract the blendshapes
			ref Mesh mesh, 													// <- The mesh we will be applying the blendshapes to
			ref SkinnedMeshRenderer skinnedMeshRenderer, 					// <- The skinned mesh renderer being used by the mesh
			float blendShapeIntensity = 1f	 								// <- We can make the blendshapes stronger / weaker by modifying intensity (default: 1)
		
		){
		 
			// -----------------------------------------------------------
			//	SETUP HELPER VARIABLES
			//	Setup the basic helpers variables, lists and dictionaries
			// -----------------------------------------------------------

			// Helpers
			int meshVertexCount = mesh.vertexCount;							// <- Cache the mesh's vertex count
			int totalNumberOfMeshVerticesSoFar = 0;							// <- Keep track of the total vertex count as we scan the SMRs
			int blendShapeCount = 0;										// <- Cache the total number of blendshapes on each SMR
			Mesh smrSharedMesh = null;										// <- Cache the SMR's shared mesh
			int smrSharedMeshVertexCount = 0;								// <- Cache each SMR's shared mesh Vertex Count
			string[] blendShapeNames = new string[0];						// <- Recycle the same string array on each SMR
			string currentBlendShapeName = string.Empty;					// <- Used to create blendshape names on demand

			// Setup a list to store all of the blendshape merge data we extract from the SMRs.
			List<BlendshapeMergeData> blendshapeMergeDataList = new List<BlendshapeMergeData>();

			// Setup a dictionary to track blend shape names ( helps to track duplicate blendshapes with the same name )
			Dictionary<string, int> blendShapeNameDictionary = new Dictionary<string, int>();

			// -----------------------------------------------------
			//	POPULATE THE BLENDSHAPE MERGE DATA LIST
			//	Loop through the SMRs and process the BlendShapes
			// -----------------------------------------------------

			// Verify each skinned mesh renderer and get info about all blendshapes of all meshes
			foreach ( SkinnedMeshRenderer smr in skinnedMeshRendererArray ){

				// Cache the Skinned Mesh Renderer's sharedMesh
				smrSharedMesh = smr.sharedMesh;

				// Cache the vertex count of this SMR's shared mesh
				smrSharedMeshVertexCount = smrSharedMesh.vertexCount;

				// Cache how many blendshapes there are on this SMR's shared mesh
				blendShapeCount = smrSharedMesh.blendShapeCount;

				// Cache the names of all Blendshapes on this SMR's shared mesh
				blendShapeNames = new string[ blendShapeCount ];
				for ( int i = 0; i < blendShapeCount; i++ ){
					blendShapeNames[i] = smrSharedMesh.GetBlendShapeName(i);
				}

				// Loop through each of the blendshapes
				for ( int i = 0; i < blendShapeCount; i++ ){

					// -----------------------------------------------------
					//	SETUP NEW BLEND SHAPE MERGE DATA
					//	Creates new data and other helpers per blendshape
					// -----------------------------------------------------

					// Create a new BlendShareMergeData entry
					BlendshapeMergeData bsmData = new BlendshapeMergeData();

					// Cache the blenshape's name, index and weight
					bsmData.name = blendShapeNames[i];
					bsmData.index = smrSharedMesh.GetBlendShapeIndex( blendShapeNames[i] );
					bsmData.weight = smr.GetBlendShapeWeight( bsmData.index );

					// Setup new empty Vector3 arrays for original data using the same length as the shared mesh
					// We'll try and cache this data soon from the original blendshape soon ...
					Vector3[] originalDeltaVertices = new Vector3[ smrSharedMeshVertexCount ];
					Vector3[] originalDeltaNormals = new Vector3[ smrSharedMeshVertexCount ];
					Vector3[] originalDeltaTangents = new Vector3[ smrSharedMeshVertexCount ];

					// Setup new default Vector3 array to be used as empty ranges to setup the first and last blendshape frames
					// NOTE: This needs to be the same length as the new mesh's total vertex count
					Vector3[] defaultRangeOfVector3s = new Vector3[ meshVertexCount ];
				
					// -----------------------------------------------------
					//	SETUP THE FIRST AND LAST BLENDSHAPE ARRAYS
					//	We basically setup the arrays with Vector3.zero
					// -----------------------------------------------------

					// Add a default range of Vector3 to all first frame data 
					bsmData.firstFrameDeltaVertices.AddRange( defaultRangeOfVector3s );
					bsmData.firstFrameDeltaNormals.AddRange( defaultRangeOfVector3s );
					bsmData.firstFrameDeltaTangents.AddRange( defaultRangeOfVector3s );

					// Add a default range of Vector3 to all last frame data 
					bsmData.lastFrameDeltaVertices.AddRange( defaultRangeOfVector3s );
					bsmData.lastFrameDeltaNormals.AddRange( defaultRangeOfVector3s );
					bsmData.lastFrameDeltaTangents.AddRange( defaultRangeOfVector3s );
					
					// -------------------------------------------------------------
					//	CACHE ORIGINAL BLENDSHAPE DELTA ARRAYS
					//	Attempt to cache the initial vertices, normals and tangents
					// -------------------------------------------------------------

					// If the BlendShape index doesn't return -1, we can set them up normally.
					if ( smrSharedMesh.GetBlendShapeIndex( blendShapeNames[i] ) != -1 ){

						// Cache the Blendshape Frame Vertices and store them in the original delta arrays
						smrSharedMesh.GetBlendShapeFrameVertices( bsmData.index, smrSharedMesh.GetBlendShapeFrameCount( bsmData.index ) - 1, originalDeltaVertices, originalDeltaNormals, originalDeltaTangents);
					
					// Otherwise, if the Blendshape index returns -1, there *may* an issue. For now we'll just continue with the zero-based default Vector3 arrays.
					} else {	

						// Show a warning
						Debug.LogWarning("There appears to be missing mesh data in the Blendshape '" + blendShapeNames[i] + "'. This Blendshape may not end up working correctly.");

					}
					
					// -----------------------------------------------------
					//	SETUP DELTA VERTICES, NORMALS AND TANGENTS
					//	We can also apply an optional intensity multiplier
					// -----------------------------------------------------

					// Setup the last delta Vertices (because these arrays always use the shared mesh's vertex count, we can use that as the loop)
					for ( int v = 0; v < smrSharedMeshVertexCount; v++ ){
						bsmData.lastFrameDeltaVertices[ v + totalNumberOfMeshVerticesSoFar ] = originalDeltaVertices[v] * blendShapeIntensity;
					}

					// Setup the last delta normals (because these arrays always use the shared mesh's vertex count, we can use that as the loop)
					for ( int n = 0; n < smrSharedMeshVertexCount; n++ ){
						bsmData.lastFrameDeltaNormals[ n + totalNumberOfMeshVerticesSoFar ] = originalDeltaNormals[n] * blendShapeIntensity;
					}

					// Setup the last delta tangents (because these arrays always use the shared mesh's vertex count, we can use that as the loop)
					for ( int t = 0; t < smrSharedMeshVertexCount; t++ ){
						bsmData.lastFrameDeltaTangents[ t + totalNumberOfMeshVerticesSoFar ] = originalDeltaTangents[t] * blendShapeIntensity;
					}

					// Add this entry to the blendshapeMergeDataList
					blendshapeMergeDataList.Add( bsmData );
					
				}

				// After each SMR, add to the total number of shared mesh vertices we've processed so far
				totalNumberOfMeshVerticesSoFar += smrSharedMeshVertexCount;
			}

			// ------------------------------------------------------------------------
			//	APPLY MERGED BLENDSHAPE DATA TO MESH
			//	Finish up the merged blendshape data and apply it to the supplied mesh
			// ------------------------------------------------------------------------

			// Finally add all processed blendshapes of all meshes, into the final skinned mesh renderer
			foreach ( BlendshapeMergeData bsmData in blendshapeMergeDataList ){

				// -----------------------------------------------------
				//	HANDLE BLENDSHAPE NAMES AND DUPLICATES
				//	Check for blendshapes with duplicate names
				// -----------------------------------------------------

				// If this blendshape name already exists in our dictionary, we need to modify the name of this blendshape so it can still work
				if ( blendShapeNameDictionary.ContainsKey( bsmData.name ) == true ){

					// Set the blendshape name to the original name in bsmData with the addition of " [x]" to differentiate it.
					currentBlendShapeName = bsmData.name + k_SPACE_OPENSQUAREDBRACKET + blendShapeNameDictionary[bsmData.name] + k_CLOSESQUAREDBRACKET;

					// Show the user a warning message about this
					Debug.LogWarning("More than one Blendshape with the name of \"" + bsmData.name + "\" was detected. One of the blendshapes was renamed to '" + currentBlendShapeName + "' in order for them all to work together in the new mesh.");
				
				// If the dictionary doesn't contain it, we can set up the blendshape name normally
				} else {

					// Set the current blendshape name to what we have already set in the bsmData
					currentBlendShapeName = bsmData.name;
				}

				// -----------------------------------------------------
				//	ADD BLENDSHAPE FRAME DATA TO MESH
				//	Create new first and last frames for this blendshape
				// -----------------------------------------------------

				// Setup the first frame of the new BlendShape
				mesh.AddBlendShapeFrame( 
					currentBlendShapeName, 
					0.0f, 
					bsmData.firstFrameDeltaVertices.ToArray(), 
					bsmData.firstFrameDeltaNormals.ToArray(), 
					bsmData.firstFrameDeltaTangents.ToArray()
				);

				// Setup the last frame of the new BlendShape
				mesh.AddBlendShapeFrame( 
					currentBlendShapeName, 
					100.0f, 
					bsmData.lastFrameDeltaVertices.ToArray(), 
					bsmData.lastFrameDeltaNormals.ToArray(), 
					bsmData.lastFrameDeltaTangents.ToArray()
				);


				// -----------------------------------------------------
				//	CLEANUP
				//	Save the new blendshape name and update dictionary
				// -----------------------------------------------------

				// Cache the blendshape name we've used ( as applied to the mesh )
				bsmData.nameOnMesh = currentBlendShapeName;

				// If our dictionary already contains a key of the blendshape data name ...
				if ( blendShapeNameDictionary.ContainsKey( bsmData.name ) == true ){
				
					// Increment the current int value (which acts as the total count)
					blendShapeNameDictionary[ bsmData.name ] += 1;

				// Otherwise, if the blendshape name doesn't already exist ...
				} else {

					// Setup a new dictionary entry with a default int value of 0
					blendShapeNameDictionary.Add( bsmData.name, 0 );
				}
			}

			// ------------------------------------------------------------------------
			//	APPLY MERGED BLENDSHAPE DATA TO SMR
			//	Finally, we apply the blendshape data weights to the supplied SMR
			// ------------------------------------------------------------------------

			// Loop through the blendshapeMergeData array
			foreach ( BlendshapeMergeData bsmData in blendshapeMergeDataList ){

				// If we find an entry with weight above zero ...
				if ( bsmData.weight > 0.0f ){

					// Apply the blend shape weight to the supplied SMR
					skinnedMeshRenderer.SetBlendShapeWeight( mesh.GetBlendShapeIndex( bsmData.nameOnMesh ), bsmData.weight );
				}
			}
		}

/// -> [EDITOR ONLY] SEPERATE SKINNED MESH RENDERER

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[EDITOR ONLY] SEPERATE SKINNED MESH RENDERER
		//	Allows us to 'Seperate' a SkinnedMeshRenderer ( very useful ). 
		//	NOTE 1: if the mesh contains blendshapes, they will no longer work on the 'seperated' meshes.
		//
		//	NOTE 2: This still relies on the original Animator / Animation components still being present in the scene so best to keep the objects inside the hierarchy.
		//			There are some issues after combining using the 'multiple material' method. User has to go through the other skinned mesh renderers and disable
		//			them manually because there doesn't seem to be a way of knowing if it was setup by MeshKit or not.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		static int MAX_VERTS_16BIT = 50000; // Don't change this ... Shouldn't this be 64k?   test for myself later.

		// Full Method
		public static void SeperateSkinnedMeshRenderer( GameObject selectedGameObject, bool inactivateChildSkinnedMeshRendererGameObjects, bool inactivateChildMeshRendererGameObjects, bool requireConfirmationInEditor = true ){

			// ------------------------------------------------------------
			//	FILEPATHS
			// ------------------------------------------------------------

			// If the Support Folders don't exist, end now ...
			if( MeshAssets.HaveSupportFoldersBeenCreated() == false ){
				Debug.Log( "MESHKIT: The MeshKit support folders for this scene have not been created! Try saving the scene.");
				EditorUtility.DisplayDialog(	
					"MeshKit Skinned Seperator", 
					"Cannot run the Seperate action because MeshKit support folders for this scene have not been created! Try saving the scene.",
					"Okay"
				);
				return;
			}

			// ------------------------------------------------------------
			// INITIAL CHECKS
			// ------------------------------------------------------------

			// End early if we didnt select a valid GameObject
			if (	selectedGameObject == null ||
					selectedGameObject.GetComponent<SkinnedMeshRenderer>() == null ||
					selectedGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh == null ||
					selectedGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.subMeshCount <= 1
			){ 
				//Debug.LogWarning("SEPERATE SKINNED MESH RENDERER: No Valid GameObject Selected ( requires a SkinnedMeshRenderer with a mesh containing submeshes ).");
				Debug.Log( "MESHKIT: This GameObject does not have a SkinnedMeshRenderer component and / or a mesh containing submeshes.");
				EditorUtility.DisplayDialog(	
					"MeshKit Skinned Seperator", 
					"The selected GameObject does not have a SkinnedMeshRenderer component and / or a mesh containing submeshes.",
					"Okay"
				);
				return;
			}

			// Cache references
			Transform selectedTransform = selectedGameObject.transform;
			SkinnedMeshRenderer selectedSkinnedMeshRenderer = selectedGameObject.GetComponent<SkinnedMeshRenderer>();

			// CHECK FOR BLENDSHAPES
			// If this mesh has blendshapes and requireConfirmationInEditor is true, we need to ask the user to confirm this.
			if (	requireConfirmationInEditor == true &&
					selectedSkinnedMeshRenderer.sharedMesh.blendShapeCount > 0 &&
					EditorUtility.DisplayDialog(	
						"MeshKit Skinned Seperator", 
						"This mesh appears to contain blendshapes. Seperating a SkinnedMesh will result in BlendShapes no longer working on the new seperated assets. Would you like to continue?",
						"Seperate", "Cancel"
					) == false
			){
				// Cancel if the user cancels
				return;
			}

			// ------------------------------------------------------------
			// DISABLE OTHER CHILD RENDERERS
			// ------------------------------------------------------------

			// Disable all the other child SkinnedMeshRenderer components (helpful if we're working on an object that was combined using 'Multiple Materials')
			if( inactivateChildSkinnedMeshRendererGameObjects == true ){

				// Cache the components and loop through them
				SkinnedMeshRenderer[] childSMRs = selectedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>( true );
				foreach( SkinnedMeshRenderer childSMR in childSMRs ){
					
					// Don't allow the main GameObject to be de-activated
					if( childSMR.gameObject != selectedGameObject ){

						// Track Undo
						Undo.RecordObject ( childSMR.gameObject, "Seperate Skinned Mesh Renderer");

						// Turn off GameObject
						childSMR.gameObject.SetActive( false );
					}
				}
			}

			// Disable all the other child MeshRenderer components (included for completeness)
			if( inactivateChildMeshRendererGameObjects == true ){

				// Cache the components and loop through them
				MeshRenderer[] childMRs = selectedGameObject.GetComponentsInChildren<MeshRenderer>( true );
				foreach( MeshRenderer childMR in childMRs ){
					
					// Don't allow the main GameObject to be de-activated
					if( childMR.gameObject != selectedGameObject ){

						// Track Undo
						Undo.RecordObject ( childMR.gameObject, "Seperate Skinned Mesh Renderer");

						// Turn off GameObject
						childMR.gameObject.SetActive( false );
					}
				}
			}

			// ------------------------------------------------------------
			// PROCESS
			// ------------------------------------------------------------

			// Helpers
			int currentMeshBonesLength = 0;
			int subMeshCount = selectedSkinnedMeshRenderer.sharedMesh.subMeshCount;
			CombineInstance[] combineInstanceArray = new CombineInstance[1];	// <- As we only have one element in the array, we can re-use this.
			Transform[] currentMeshBones = new Transform[0];

			// Split each submesh into a new unique submesh into a unique GameObject
			for (int i = 0; i < subMeshCount; i++ ){

				// Show progress bar for each submesh
				#if SHOW_PROGRESS_BARS
					EditorUtility.DisplayProgressBar(
						"MeshKit Skinned Seperator", 
						"Seperating sub-mesh ( " + i.ToString() + " / " + subMeshCount.ToString() + " )", 
						(float)i / (float)subMeshCount
					);
				#endif

				// Helpers for each submesh
				List<Transform> bones = new List<Transform>();
				List<Matrix4x4> bindPoses = new List<Matrix4x4>();

				// Cache the bones of the selectedSkinnedMeshRenderer
				currentMeshBones = selectedSkinnedMeshRenderer.bones;

				// Loop through the mesh bones ...
				currentMeshBonesLength = currentMeshBones.Length;
				for (int x = 0; x < currentMeshBonesLength; x++){

					// Add the current mesh bone to our new bones list
					bones.Add(currentMeshBones[x]);

					/*
						// ALGORITHM A
						bindPoses.Add(selectedSkinnedMeshRenderer.sharedMesh.bindposes[x] * selectedSkinnedMeshRenderer.transform.worldToLocalMatrix);
					
						// ALGORITHM B (we're using this)
						bindPoses.Add(currentMeshBones[x].worldToLocalMatrix * selectedSkinnedMeshRenderer.transform.worldToLocalMatrix);
					*/

					// This version uses the mesh's bind pose multiplied by the selected transform's worldToLocalMatrix.
					bindPoses.Add(selectedSkinnedMeshRenderer.sharedMesh.bindposes[x] * selectedTransform.worldToLocalMatrix);
				}

				// Setup the first (and only) element of the CombineInstance for this submesh
				combineInstanceArray[0].mesh = selectedSkinnedMeshRenderer.sharedMesh;
				combineInstanceArray[0].subMeshIndex = i;
				combineInstanceArray[0].transform = selectedSkinnedMeshRenderer.transform.localToWorldMatrix;


				// Show progress bar for each submesh
				#if SHOW_PROGRESS_BARS
					EditorUtility.DisplayProgressBar(
						"MeshKit Skinned Seperator", 
						"Seperating sub-mesh ( " + subMeshCount.ToString() + " / " + subMeshCount.ToString() + " )", 
						1f
					);
				#endif

				// ----------------------------------------------------
				// CREATE THE NEW MESH USING THE COMBINE MESHES METHOD
				// ----------------------------------------------------

				// Create a new Mesh
				Mesh seperatedSkinnedMesh = new Mesh();
				seperatedSkinnedMesh.name = "Seperated Skinned Mesh (Material " + i.ToString() + ")";
				
				// Determine if we need to use a 16 or 32 bit mesh based on vertex count.
				if ( selectedSkinnedMeshRenderer.sharedMesh.vertexCount <= MAX_VERTS_16BIT){
					seperatedSkinnedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
				} else {
					seperatedSkinnedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				}

				// Use the CombineMeshes action to essentially build the new mesh from our single-element combineInstanceArray
				seperatedSkinnedMesh.CombineMeshes( combineInstanceArray, true, true );

				// Recalculate the new Mesh Bounds
				seperatedSkinnedMesh.RecalculateBounds();


				// ------------------------------------------------
				// SETUP NEW GAMEOBJECT WITH SKINNED MESH RENDERER
				// ------------------------------------------------

				// Create the holder GameObject for this unique new mesh splitted
				GameObject holderGameObject = new GameObject( selectedTransform.name + " Seperated (Material " + i.ToString() + ")");
				Undo.RegisterCreatedObjectUndo (holderGameObject, "Seperate Skinned Mesh Renderer");
				
				// Reparent the GameObject
				//holderGameObject.transform.SetParent( selectedTransform );
				Undo.SetTransformParent ( holderGameObject.transform, selectedTransform, "Seperate Skinned Mesh Renderer");


				// Create and setup the SkinnedMeshRenderer
			//	SkinnedMeshRenderer smr = holderGameObject.AddComponent<SkinnedMeshRenderer>();
				SkinnedMeshRenderer smr = Undo.AddComponent<SkinnedMeshRenderer>(holderGameObject);

				// Setup the SkinnedMeshRenderer
				smr.sharedMesh = seperatedSkinnedMesh;
				smr.bones = bones.ToArray();
				smr.sharedMesh.bindposes = bindPoses.ToArray();
				smr.rootBone = selectedSkinnedMeshRenderer.rootBone;
				smr.sharedMaterials = new Material[] { selectedSkinnedMeshRenderer.sharedMaterials[i] };


				// ------------
				// SAVE ASSETS
				// ------------

				// Create the assets (old way)
				//AssetDatabase.CreateAsset( seperatedSkinnedMesh, saveAssetDirectory + Selection.activeGameObject.name + "splitMesh" + i.ToString() + ".asset");

				// Try to save the meshes
				seperatedSkinnedMesh.name.MakeFileSystemSafe(); // Fix dodgy mesh names.
				MeshAssets.CreateMeshAsset( seperatedSkinnedMesh, MeshAssets.ProcessFileName(MeshAssets.seperatedMeshFolder, selectedTransform.name, "Seperated["+i+"]", false) );


				// ---------
				// UNDO SMR
				// ---------

				// Update the status of the SkinnedMeshRenderer after everything is setup
				Undo.RecordObject ( smr, "Seperate Skinned Mesh Renderer");

			}

			// Disable the original SkinnedMeshRenderer
			Undo.RecordObject ( selectedSkinnedMeshRenderer, "Seperate Skinned Mesh Renderer");
			selectedSkinnedMeshRenderer.enabled = false;
			
			// Disable the original GameObject (this will disable the original animations so don't do that)
			//selectedGameObject.SetActive(false);

			//Clear progress bar
			EditorUtility.ClearProgressBar();

		}

	}
}
////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitSkinnedMeshRendererUtility.cs
//
//  RUNTIME Methods for combining and seperating Skinned Mesh Renderers
//
//  © 2022 Melli Georgiou.
//  Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use HellTap Namespace
namespace HellTap.MeshKit {
	public class MeshKitSkinnedMeshRendererUtility : MonoBehaviour {

/// -> COMBINE SKINNED MESH RENDERERS (NO ATLASSING)

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  COMBINE SKINNED MESH RENDERERS (NO ATLASSING)
		//  This Version doesn't do any texture atlassing. It creates a mesh with loads of submeshes and puts all the materials into a material array.
		//  BENEFITS: Reduces all skinned mesh renderers to one. However, no draw call or shadow caster optimizations occur.
		//  Returns true if successful, false if it failed.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Full Method
		public static bool CombineSkinnedMeshRenderersNoAtlas(
			GameObject selectedGameObject,
			bool combineInactiveRenderers = false,
			bool destroyObjectsWithDisabledRenderers = false
		){

			// ------------------------------------------------------------
			//  FILEPATHS
			// ------------------------------------------------------------

			// Setup a path to save new assets
			//string saveAssetDirectoryForFileAPI = Application.dataPath + "/ZZZ TEST/";    // Use with File API
			//string saveAssetDirectory = "Assets/ZZZ TEST/";                               // use with AssetDatabase API

			// ------------------------------------------------------------
			//  INITIAL GAMEOBJECT CHECKS
			// ------------------------------------------------------------

			// End early if we didnt select a GameObject
			if ( selectedGameObject == null){ 

				/*
				EditorUtility.DisplayDialog(    
					"Combine SkinnedMeshRenderer", 
					"No GameObject was selected to combine.", "OK"
				);
				*/

				Debug.LogError( "MESHKIT SKINNED MESH RENDERER UTILITY: No GameObject was selected to combine." );
				return false;
			}

			// The Selected GameObject already has a SkinnedMeshRenderer on it!
			if ( selectedGameObject.GetComponent<SkinnedMeshRenderer>() != null ){ 

				/*
				EditorUtility.DisplayDialog(    
					"Combine SkinnedMeshRenderer", 
					"The selected GameObject already has a SkinnedMeshRenderer component.\n\nIn order to properly combine objects, make sure the parent object does not have a SkinnedMeshRenderer already attached.", "OK"
				);
				*/

				Debug.LogError( "MESHKIT SKINNED MESH RENDERER UTILITY: The selected GameObject already has a SkinnedMeshRenderer component.\n\nIn order to properly combine objects, make sure the parent object does not have a SkinnedMeshRenderer already attached." );
				return false;
			}

			// -------------------------------------------
			//  SCAN SKINNED MESH RENDERERS FOR SUBMESHES
			// -------------------------------------------

			// Cache all of the Skinned Mesh Renderers that are children of the selected GameObject
			SkinnedMeshRenderer[] SMRs = selectedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>( combineInactiveRenderers );  // <- default = false

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

				/*
				EditorUtility.DisplayDialog(    
					"Combine SkinnedMeshRenderer", 
					"Some meshes found on the selected SkinnedMeshRenderers are using submeshes which will cause issues when combining.\n\nIn order to combine them, you can use the 'Seperate' tool to seperate them into individual meshes.", "OK"
				);
				*/

				Debug.LogError( "MESHKIT SKINNED MESH RENDERER UTILITY: Some meshes found on the selected SkinnedMeshRenderers are using submeshes which will cause issues when combining.\n\nIn order to combine them, you can use the 'Seperate' tool to seperate them into individual meshes." );
				return false;
			}

			// ------------------
			//  HELPER VARIABLES
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
			//  SCAN SKINNED MESH RENDERERS
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
			//  SCAN ANIMATION DATA (BONES, WEIGHTS, ETC)
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
			//  SCAN SKINNED MESHES
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
			//    Undo.RecordObject ( smr, "Combining Skinned Mesh Renderers");
	 
				// Disable the original Skinned Mesh Renderer
				smr.enabled = false;
			}
			
			// -------------------
			//  CREATE NEW ASSETS
			// -------------------

			// Show progress bar
			//#if SHOW_PROGRESS_BARS
			//    EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Creating New Assets...", 2f / 3f );
			//#endif

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
			//SkinnedMeshRenderer newSMR = Undo.AddComponent<SkinnedMeshRenderer>(selectedGameObject);  // <- undo version not available at runtime.
			SkinnedMeshRenderer newSMR = selectedGameObject.AddComponent<SkinnedMeshRenderer>();
			
	 
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
			//  SAVE THE NEW MESH TO MESHKIT'S LOCAL COMBINE FOLDER
			// ------------------------------------------------------

			// NOTE: We dont need to save the mesh file at runtime.
			/*

			// Then, create the asset
			//AssetDatabase.CreateAsset(newMesh, saveAssetDirectory + selectedGameObject.name + "mesh.asset");
				
			// Try to save the mesh to MeshKit's Combine folder
			bool saveMeshSuccessfully = SaveCombinedSkinnedMesh( newMesh, newSMR );
			if( saveMeshSuccessfully == false ){

				// Show Error Message To User:
				//EditorUtility.DisplayDialog("Skinned Mesh Combiner", "Unfortunately there was a problem with the combined mesh of GameObject: \n\n"+newSMR.gameObject.name + "\n\nThe Combine process must now be aborted.", "Okay");
				Debug.LogError( "MESHKIT SKINNED MESH RENDERER UTILITY: Unfortunately there was a problem with the combined mesh of GameObject: \n\n"+newSMR.gameObject.name + "\n\nThe Combine process must now be aborted." );

				// Show progress bar for each submesh
			//    #if SHOW_PROGRESS_BARS
			//        EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Attempting to undo the previous steps...", 1f );
			//    #endif

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
			//    EditorUtility.ClearProgressBar();

				// Return false because something failed
				return false;

			}

			*/


			// ------------------------------------------------------
			//  MAKE SURE ANIMATORS AND ANIMATIONS UPDATE OFF-SCREEN
			// ------------------------------------------------------

			// Show progress bar for each submesh
		//    #if SHOW_PROGRESS_BARS
		//        EditorUtility.DisplayProgressBar( "Combining Skinned Mesh Renderers", "Handling Animation Components...", 3f / 3f );
		//    #endif

			// Cache all of the child Animators and make sure they are set to always update (also track original values)
			Animator[] originalAnimators = selectedGameObject.GetComponentsInChildren<Animator>() as Animator[];
			AnimatorCullingMode[] originalAnimatorCullingModes = new AnimatorCullingMode[ originalAnimators.Length ];
			for( int i = 0; i < originalAnimators.Length; i++ ){    
				
			//    Undo.RecordObject ( originalAnimators[i], "Combining Skinned Mesh Renderers");
				originalAnimatorCullingModes[i] = originalAnimators[i].cullingMode;
				originalAnimators[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;
			//    EditorUtility.SetDirty( originalAnimators[i] );
			}
			
			// Cache all of the child Animations and make sure they are set to always update (also track original values)
			Animation[] originalAnimations = selectedGameObject.GetComponentsInChildren<Animation>() as Animation[];
			AnimationCullingType[] originalAnimationCullingTypes = new AnimationCullingType[ originalAnimations.Length ];
			for( int i = 0; i < originalAnimations.Length; i++ ){   
				
			//    Undo.RecordObject ( originalAnimations[i], "Combining Skinned Mesh Renderers");
				originalAnimationCullingTypes[i] = originalAnimations[i].cullingType;
				originalAnimations[i].cullingType = AnimationCullingType.AlwaysAnimate;
			//    EditorUtility.SetDirty( originalAnimations[i] );
			}


			// ---------------------------------------------------------------------
			//  AT RUNTIME, WE CAN ALSO ALLOW DESTROYING THE OLD DISABLED RENDERERS
			// ---------------------------------------------------------------------

			if( destroyObjectsWithDisabledRenderers == true ){

				// Loop through each of the Skinned Mesh Renderers
				foreach (SkinnedMeshRenderer smr in SMRs){

					// Make sure the GameObject we're going to delete isn't the main one
					if( smr != null && smr.enabled == false && smr.gameObject != selectedGameObject ){
						Object.Destroy( smr.gameObject );
					}
				}
			}


			// ------------------------------------------------------
			//  SETUP MESHKIT COMBINE SKINNEDMESH COMPONENT
			// ------------------------------------------------------
			
			// Cache the MeshKitCombineSkinnedMeshSetup component if it exists
			/*
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
			*/

			// ------------------------------------------------------
			//  RETURN TRUE
			// ------------------------------------------------------

			// Clear Progress Bar
		   // EditorUtility.ClearProgressBar();

			// Return true if the process succeeded
			return true;

		}

/// -> [EDITOR ONLY] MERGE BLENDSHAPES FROM SKINNED MESH RENDERERS

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  [EDITOR ONLY] MERGE BLENDSHAPES FROM SKINNED MESH RENDERERS
		//  This method merges all the blendshape data together
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// Constant string helpers
		const string k_SPACE_OPENSQUAREDBRACKET = " [";
		const string k_CLOSESQUAREDBRACKET = "]";

		// Blendshape Merge Data - Helper Class
		class BlendshapeMergeData {

			public string name = "";                                                // <- Temporary blendshape name
			public string nameOnMesh = "";                                          // <- The name used on the mesh may be modified if duplicates are found.
			public int index = -1;                                                  // <- The blendShape Index
			public float weight = 0.0f;                                             // <- The BlendShape Weight
			public List<Vector3> firstFrameDeltaVertices = new List<Vector3>();     // <- First Frame Delta Vertices
			public List<Vector3> firstFrameDeltaNormals = new List<Vector3>();      // <- First Frame Delta Normals
			public List<Vector3> firstFrameDeltaTangents = new List<Vector3>();     // <- First Frame Delta Tangents
			public List<Vector3> lastFrameDeltaVertices = new List<Vector3>();      // <- Last Frame Delta Vertices
			public List<Vector3> lastFrameDeltaNormals = new List<Vector3>();       // <- Last Frame Delta Normals
			public List<Vector3> lastFrameDeltaTangents = new List<Vector3>();      // <- Last Frame Delta Tangents
			
		}

		// Method
		static void MergeBlendshapesFromSkinnedMeshRenderers (

			// Arguments
			ref SkinnedMeshRenderer[] skinnedMeshRendererArray,             // <- The array of Skinned Mesh Renderers we are using to extract the blendshapes
			ref Mesh mesh,                                                  // <- The mesh we will be applying the blendshapes to
			ref SkinnedMeshRenderer skinnedMeshRenderer,                    // <- The skinned mesh renderer being used by the mesh
			float blendShapeIntensity = 1f                                  // <- We can make the blendshapes stronger / weaker by modifying intensity (default: 1)
		
		){
		 
			// -----------------------------------------------------------
			//  SETUP HELPER VARIABLES
			//  Setup the basic helpers variables, lists and dictionaries
			// -----------------------------------------------------------

			// Helpers
			int meshVertexCount = mesh.vertexCount;                         // <- Cache the mesh's vertex count
			int totalNumberOfMeshVerticesSoFar = 0;                         // <- Keep track of the total vertex count as we scan the SMRs
			int blendShapeCount = 0;                                        // <- Cache the total number of blendshapes on each SMR
			Mesh smrSharedMesh = null;                                      // <- Cache the SMR's shared mesh
			int smrSharedMeshVertexCount = 0;                               // <- Cache each SMR's shared mesh Vertex Count
			string[] blendShapeNames = new string[0];                       // <- Recycle the same string array on each SMR
			string currentBlendShapeName = string.Empty;                    // <- Used to create blendshape names on demand

			// Setup a list to store all of the blendshape merge data we extract from the SMRs.
			List<BlendshapeMergeData> blendshapeMergeDataList = new List<BlendshapeMergeData>();

			// Setup a dictionary to track blend shape names ( helps to track duplicate blendshapes with the same name )
			Dictionary<string, int> blendShapeNameDictionary = new Dictionary<string, int>();

			// -----------------------------------------------------
			//  POPULATE THE BLENDSHAPE MERGE DATA LIST
			//  Loop through the SMRs and process the BlendShapes
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
					//  SETUP NEW BLEND SHAPE MERGE DATA
					//  Creates new data and other helpers per blendshape
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
					//  SETUP THE FIRST AND LAST BLENDSHAPE ARRAYS
					//  We basically setup the arrays with Vector3.zero
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
					//  CACHE ORIGINAL BLENDSHAPE DELTA ARRAYS
					//  Attempt to cache the initial vertices, normals and tangents
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
					//  SETUP DELTA VERTICES, NORMALS AND TANGENTS
					//  We can also apply an optional intensity multiplier
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
			//  APPLY MERGED BLENDSHAPE DATA TO MESH
			//  Finish up the merged blendshape data and apply it to the supplied mesh
			// ------------------------------------------------------------------------

			// Finally add all processed blendshapes of all meshes, into the final skinned mesh renderer
			foreach ( BlendshapeMergeData bsmData in blendshapeMergeDataList ){

				// -----------------------------------------------------
				//  HANDLE BLENDSHAPE NAMES AND DUPLICATES
				//  Check for blendshapes with duplicate names
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
				//  ADD BLENDSHAPE FRAME DATA TO MESH
				//  Create new first and last frames for this blendshape
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
				//  CLEANUP
				//  Save the new blendshape name and update dictionary
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
			//  APPLY MERGED BLENDSHAPE DATA TO SMR
			//  Finally, we apply the blendshape data weights to the supplied SMR
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

/// -> [RUNTIME] SEPARATE SKINNED MESH RENDERER

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//  [RUNTIME] SEPARATE SKINNED MESH RENDERER
		//	Allows us to 'Seperate' a SkinnedMeshRenderer ( very useful ). 
		//	NOTE 1: if the mesh contains blendshapes, they will no longer work on the 'seperated' meshes.
		//
		//	NOTE 2: This still relies on the original Animator / Animation components still being present in the scene so best to keep the objects inside the hierarchy.
		//			There are some issues after combining using the 'multiple material' method. User has to go through the other skinned mesh renderers and disable
		//			them manually because there doesn't seem to be a way of knowing if it was setup by MeshKit or not.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		static int MAX_VERTS_16BIT = 50000; // Don't change this ... Shouldn't this be 64k?   test for myself later.

		// Full Method
		public static bool SeparateSkinnedMeshRenderer( 
			GameObject selectedGameObject, 
			bool destroyOriginalSkinnedMeshRenderer,
			bool inactivateChildSkinnedMeshRendererGameObjects, 
			bool inactivateChildMeshRendererGameObjects, 
			bool destroyAllChildrenWithDisabledRenderers 
		){

			// ------------------------------------------------------------
			//	INITIAL CHECKS
			// ------------------------------------------------------------

			// End early if we didnt select a valid GameObject
			if (    selectedGameObject == null ||
					selectedGameObject.GetComponent<SkinnedMeshRenderer>() == null ||
					selectedGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh == null ||
					selectedGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh.subMeshCount <= 1
			){ 
				Debug.LogWarning("MESHKIT SKINNED MESH RENDERER UTILITY (Seperate): No Valid GameObject Selected ( requires a SkinnedMeshRenderer with a mesh containing submeshes ).");
				return false;
			}

			// Cache references
			Transform selectedTransform = selectedGameObject.transform;
			SkinnedMeshRenderer selectedSkinnedMeshRenderer = selectedGameObject.GetComponent<SkinnedMeshRenderer>();

			// CHECK FOR BLENDSHAPES [RUNTIME VERSION]
			// Info when seperating an object with blendshapes
			if( selectedSkinnedMeshRenderer.sharedMesh.blendShapeCount > 0 ){
				Debug.LogWarning( "MESHKIT SKINNED MESH RENDERER UTILITY (Seperate): You seperated a SkinnedMesh with a BlendShape which will no longer keep working on the new seperated assets." );
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
						
						// If we should destroy these instead of inactivate them, do it here
						if( destroyAllChildrenWithDisabledRenderers == true){
							Object.Destroy( childSMR.gameObject );
						} else {
							childSMR.gameObject.SetActive( false );
						}
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

						// If we should destroy these instead of inactivate them, do it here
						if( destroyAllChildrenWithDisabledRenderers == true){
							Object.Destroy( childMR.gameObject );
						} else {
							childMR.gameObject.SetActive( false );
						}
					}
				}
			}

			// ------------------------------------------------------------
			// PROCESS
			// ------------------------------------------------------------

			// Helpers
			int currentMeshBonesLength = 0;
			int subMeshCount = selectedSkinnedMeshRenderer.sharedMesh.subMeshCount;
			CombineInstance[] combineInstanceArray = new CombineInstance[1];    // <- As we only have one element in the array, we can re-use this.
			Transform[] currentMeshBones = new Transform[0];

			// Split each submesh into a new unique submesh into a unique GameObject
			for (int i = 0; i < subMeshCount; i++ ){

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
				//holderGameObject.transform.SetParent( selectedTransform.parent );
				holderGameObject.transform.SetParent( selectedTransform );

				// Create and setup the SkinnedMeshRenderer
				SkinnedMeshRenderer smr = holderGameObject.AddComponent<SkinnedMeshRenderer>();
				smr.sharedMesh = seperatedSkinnedMesh;
				smr.bones = bones.ToArray();
				smr.sharedMesh.bindposes = bindPoses.ToArray();
				smr.rootBone = selectedSkinnedMeshRenderer.rootBone;
				smr.sharedMaterials = new Material[] { selectedSkinnedMeshRenderer.sharedMaterials[i] };

			}

			// ------------------------------------------------------------
			//	CLEAN UP
			// ------------------------------------------------------------

			// Disable the original SkinnedMeshRenderer
			selectedSkinnedMeshRenderer.enabled = false;
			
			// Disable the original GameObject => NOTE: (this will disable the original animations too so don't do that)
			//selectedGameObject.SetActive(false);

			// Destroy the original Skinned Mesh Renderer
			if( destroyOriginalSkinnedMeshRenderer == true ){
				Object.Destroy( selectedSkinnedMeshRenderer );
			}

			// Return true if this was successful
			return true;

		}

	}
}

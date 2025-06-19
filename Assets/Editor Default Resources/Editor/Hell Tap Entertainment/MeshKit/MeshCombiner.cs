
/*
This asset was uploaded by https://unityassetcollection.com
*/

////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshCombiner.cs
//
//	Combines Meshes of all children objects in the Editor
//
//	© 2015 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	public class MeshCombiner : EditorWindow {

		// Constants
		public const int maxVertices16 = 65534;					// Unity's upper limit of vertices that can be combined ( max value of 16bit int )
		public const uint maxVertices32 = 4294967290;			// Max value for 32bit int (minus 5 for good measure, should be 4294967295 )

		// Helper Variables
		public static int currentAssetCount = 0;				// We set this using how many assets are in the Combined Mesh folder
		public static float progress = 0;
		public static float maxProgress = 0;

		// shared Variables
		public static ArrayList renderersThatWereDisabled;
		public static ArrayList createdCombinedMeshObjects;

		// To Update Progress Bar
		void OnInspectorUpdate() { Repaint(); }	

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START COMBINE
		//	We create the new Meshes at start
		////////////////////////////////////////////////////////////////////////////////////////////////

		public static void StartCombine( Transform theTransform, bool optimizeMeshes, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool enabledRenderersOnly, bool deleteSourceObjects, bool createNewObjectsWithMeshColliders, bool deleteObjectsWithDisabledRenderers, bool deleteEmptyObjects, uint userMaxVertices = maxVertices16 ){
			
			// Combine Mesh Filters
			bool meshFiltersWereCombined = false;

			// Setup Support Folders and then cache the current number of Assets in the Combined Mesh Folder
			MeshAssets.HaveSupportFoldersBeenCreated();
			currentAssetCount = 0;
			currentAssetCount = MeshAssets.HowManyFilesAtAssetPath(MeshAssets.combinedMeshFolder);

			// =====================
			//	MAIN ROUTINE
			// =====================

			// Setup Progress Bar
			maxProgress = 4;
			progress = 0;

			/*
			// Make sure userMaxVertices doesn't exceed the actual maxVertices
			if( userMaxVertices > maxVertices ){ 
				Debug.Log("MESHKIT: Maximum vertices cannot be higher than " + maxVertices + ". Your settings have been changed.");
				userMaxVertices = maxVertices; 
			}
			*/

				// -> START OF v3

					// By default, don't use 32bit meshes unless we have to.
					bool allow32bitMeshes = false;

					// Make sure our minimum amount set
					if( userMaxVertices < 16000 ){
						Debug.Log("MESHKIT: Minimum vertices cannot be lower than " + 16000 + ". Your settings have been changed.");
						userMaxVertices = 16000;
					}

					// If the user supplied vertices are higher than the 32bit limit, keep it within bounds
					if( userMaxVertices > maxVertices32 ){
						Debug.Log("MESHKIT: Maximum vertices cannot be higher than " + maxVertices32 + ". Your settings have been changed.");
						userMaxVertices = maxVertices32;
					}

					// If its higher than our 16 bit mesh limit, make sure we allow 32 bit meshes
					if( userMaxVertices > maxVertices16 ){ 
						allow32bitMeshes = true;
					}

				// -> END OF V3



			// Prepare variables to store the new meshes, materials, etc.
			Matrix4x4 myTransform = theTransform.worldToLocalMatrix;
			Dictionary<Material, List<CombineInstance>> combines = new Dictionary<Material, List<CombineInstance>>();
			MeshRenderer[] meshRenderers = theTransform.GetComponentsInChildren<MeshRenderer>();

			// Track Renderers That Are Disabled
			//renderersThatWereDisabled = new ArrayList();
			//renderersThatWereDisabled.Clear();

			// NEW IN V3
			List<Renderer> renderersThatWereDisabled = new List<Renderer>();

			// Loop through the MeshRenderers inside of this group ...
			foreach (var meshRenderer in meshRenderers){

				// Show Progress Bar
				if( maxProgress > 0 ){
					EditorUtility.DisplayProgressBar("Combining Mesh", "Preparing MeshRenderers ...", 1.0f / 3.0f);
				} else {
					EditorUtility.ClearProgressBar();
				}

				// Make sure the MeshRenderers we are checking are valid!
				if(meshRenderer!=null){

					// Only combine meshes that have a single subMesh and have MeshRenderers that are enabled!
					if( meshRenderer.gameObject.GetComponent<MeshFilter>() != null &&
						meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh != null &&
						meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount == 1 &&
						( !enabledRenderersOnly || meshRenderer.enabled == true )
					){

						// Loop through the materials of each renderer ...
						foreach (var material in meshRenderer.sharedMaterials){

							// If the material doesn't exist in the "combines" list then add it
							if (material != null && !combines.ContainsKey(material)){
								combines.Add(material, new List<CombineInstance>());
							}
						}
					}
				}
			}

			// Loop through the MeshFilters inside of this group ...
			int howManyMeshCombinationsWereThere = 0;
			MeshFilter[] meshFilters = theTransform.GetComponentsInChildren<MeshFilter>();
			MeshFilter[] compatibleMeshFilters = new MeshFilter[0];

			// Loop through the MeshFilters to see which ones are compatible
			foreach(var filter in meshFilters){

				// Show Progress Bar
				if( maxProgress > 0 ){
					EditorUtility.DisplayProgressBar("Combining Mesh", "Analyzing Data From Mesh Filters ...", 2.0f / 3.0f);
				} else {
					EditorUtility.ClearProgressBar();
				}

				// If there isn't a mesh applied, skip it..
				if (filter.sharedMesh == null){
					continue;
				}

				// If this is connected to a MeshRenderer that is disabled ...
				else if (filter.gameObject.GetComponent<MeshRenderer>() != null &&
					enabledRenderersOnly && filter.gameObject.GetComponent<MeshRenderer>().enabled == false
				){
					continue;
				}

				// Make sure it doesn't have too many vertices
				else if ( filter.sharedMesh.vertexCount >= userMaxVertices ){
					continue;
				}

				// If this mesh has subMeshes, skip it..
				else if ( filter.sharedMesh.subMeshCount > 1){
					continue;
				} else {

					// Add this to the compatibleMeshFilters array
					Arrays.AddItemFastest( ref compatibleMeshFilters, filter );

					// Combine The Meshes
					CombineInstance ci = new CombineInstance();
					ci.mesh = filter.sharedMesh;
					#if !UNITY_5_5_OR_NEWER
						// Run Legacy optimization. This apparantly isn't needed anymore after Unity 5.5.
						if(optimizeMeshes){ ci.mesh.Optimize(); }
					#endif

					ci.transform = myTransform * filter.transform.localToWorldMatrix;

					// Make sure the current filter has a Renderer and that Renderer has a sharedMaterial
					if( filter.GetComponent<Renderer>() != null && filter.GetComponent<Renderer>().sharedMaterial != null ){
						combines[filter.GetComponent<Renderer>().sharedMaterial].Add(ci);

						// Turn off the original renderer
						Undo.RecordObject ( filter.GetComponent<Renderer>(), "MeshKit (Combine)");
						renderersThatWereDisabled.Add(filter.GetComponent<Renderer>());
						filter.GetComponent<Renderer>().enabled = false;

						// Increment how many mesh combinations there were
						howManyMeshCombinationsWereThere++;
					}
				}	
			}

			// After we've sorted out MeshFilters, replace the original array with the updated one
			//Debug.Log( "Total MeshFilters: " + meshFilters.Length );
			//Debug.Log( "Compatible MeshFilters: " + compatibleMeshFilters.Length );
			meshFilters = compatibleMeshFilters;

			// Create The Combined Meshes only if there are more materials then meshes
			if(MeshKitGUI.verbose){ Debug.Log("Mesh Combinations: "+ howManyMeshCombinationsWereThere); }
			if(MeshKitGUI.verbose){ Debug.Log("Material Count: "+ combines.Keys.Count); }
			if( combines.Keys.Count < howManyMeshCombinationsWereThere ){

				// Loop through the materials in the "combine" list ...
				float localProgressCount = 0f;
				float localCombineKeyCount = combines.Keys.Count;
				foreach(Material m in combines.Keys){

					// Show Progress Bar
					localProgressCount++;
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Combining Mesh", "Building New Combined Meshes ...", localProgressCount /localCombineKeyCount );
					} else {
						EditorUtility.ClearProgressBar();
					}

					// increment the number of assets
					currentAssetCount++;

					// NOTE: We should try to scan the size of these meshes before combining them to make sure they are within the limit
					//float totalVertsCount = 0f;
					uint totalVertsCount = 0;	// <- NEW IN V3
					foreach( CombineInstance countCI in combines[m].ToArray() ){
						totalVertsCount += (uint)countCI.mesh.vertexCount;
					}

					// If there are less than the number of user max vertices, we can create the new GameObject normally ...
					if( totalVertsCount < userMaxVertices ){

						// Create a new GameObject based on the name of the material
						GameObject go = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "  ["+m.shader.name+"]");
						go.transform.parent = theTransform;
						go.transform.localPosition = Vector3.zero;
						go.transform.localRotation = Quaternion.identity;
						go.transform.localScale = Vector3.one;
						
						// Create a combined mesh using a new variable (avoids errors!)
						Mesh newMesh = new Mesh();
						newMesh.Clear();
						newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "  ["+m.shader.name+"]";


						// START OF V3

							// Determine if we need to use a 16 or 32 bit mesh based on vertex count.
							if ( totalVertsCount <= (uint)maxVertices16 ){
								newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
							} else {
								newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
							}

						// END OF V3


						newMesh.CombineMeshes(combines[m].ToArray(), true, true );

						// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
						createdCombinedMeshObjects = new ArrayList();
						createdCombinedMeshObjects.Clear();

						// Save Combined Mesh and handle errors in a seperate function
						SaveCombinedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, 0 ); // When creating assets normally, we use 0 as the group int.

					// Otherwise we need to break apart this CombinedInstance and create seperate ones within the limits.
					} else {

						// Debug Message
						if(MeshKitGUI.verbose){ Debug.Log("MESHKIT: Too many verts detected! Attempting To divide combined mesh for material \""+ m.name +"\" ..."); }

						// Count the index, the arrays created, the current total verts, and also a new combineInstance Array to build new combines from this oversized one ...
						int i = 0;
						int arraysCreated = 0;
						//float currentVertsCount = 0f;
						uint currentVertsCount = 0;	// <- NEW IN V3
						ArrayList newCombineInstance = new ArrayList(); 
						newCombineInstance.Clear();

						// Loop through each of the CombineInstances and create new ones
						foreach( CombineInstance countCI in combines[m].ToArray() ){

							// --------------------------------------------------
							// BUILD A NEW COMBINED MESH BEFORE IT GETS TOO BIG
							// --------------------------------------------------

							// Will adding the new mesh make this combineInstance too large? AND -
							// If there is at least 1 mesh in this group, we should build it now before it gets too large.
							if( currentVertsCount + (uint)countCI.mesh.vertexCount >= userMaxVertices &&
								newCombineInstance.Count > 0 
							){
								// Create a new GameObject based on the name of the material
								GameObject go = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
								go.transform.parent = theTransform;
								go.transform.localPosition = Vector3.zero;
								go.transform.localRotation = Quaternion.identity;
								go.transform.localScale = Vector3.one;

								// Create a combined mesh using a new variable (avoids errors!)
								Mesh newMesh = new Mesh();
								newMesh.Clear();
								newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "_" + arraysCreated.ToString() + "  ["+m.shader.name+"]";


								// START OF V3

									// Determine if we need to use a 16 or 32 bit mesh based on vertex count.
									if ( currentVertsCount + (uint)countCI.mesh.vertexCount <= (uint)maxVertices16 ){
										newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
									} else {
										newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
									}

								// END OF V3


								// Convert the CombineInstance into the builtin array and combine the meshes
								CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
								newMesh.CombineMeshes( newCombineArr, true, true );

								// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
								createdCombinedMeshObjects = new ArrayList();
								createdCombinedMeshObjects.Clear();

								// Save Combined Mesh and handle errors in a seperate function
								SaveCombinedMesh( go, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, arraysCreated ); // When creating assets normally, we use 0 as the group int.

								// Reset The array to build the next group
								currentVertsCount = 0;
								newCombineInstance = new ArrayList(); 
								newCombineInstance.Clear();

								// Increment the array count
								arraysCreated++;
							}

							// ----------------------------------------------
							// ADD THE NEW MESH TO THE ARRAY IF THERE IS ROOM
							// ----------------------------------------------

							// If theres space, add it to the group
							if( currentVertsCount + countCI.mesh.vertexCount < userMaxVertices ){

								// Add this CombineInstance into the new array...
								newCombineInstance.Add(countCI);

								// Update the total vertices so far ...
								currentVertsCount += (uint)countCI.mesh.vertexCount;

								// If this is the last loop - we should build the mesh here.
								if( i == combines[m].Count - 1 ){

									// Create a new GameObject based on the name of the material
									GameObject go2 = new GameObject(currentAssetCount.ToString("D4")+"_combined_" + m.name + "_"+arraysCreated.ToString() +"  ["+m.shader.name+"]");
									go2.transform.parent = theTransform;
									go2.transform.localPosition = Vector3.zero;
									go2.transform.localRotation = Quaternion.identity;
									go2.transform.localScale = Vector3.one;

									// Create a combined mesh using a new variable (avoids errors!)
									Mesh newMesh = new Mesh();
									newMesh.Clear();
									newMesh.name = currentAssetCount.ToString("D4")+"_combined_" + m.name + "_" + arraysCreated.ToString() + "  ["+m.shader.name+"]";

									// START OF V3

										// Determine if we need to use a 16 or 32 bit mesh based on vertex count.
										if ( currentVertsCount + (uint)countCI.mesh.vertexCount <= (uint)maxVertices16 ){
											newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
										} else {
											newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
										}

									// END OF V3

									// Convert the CombineInstance into the builtin array and combine the meshes
									CombineInstance[] newCombineArr = (CombineInstance[]) newCombineInstance.ToArray( typeof( CombineInstance ) );
									newMesh.CombineMeshes( newCombineArr, true, true );

									// Create An Array to track which new GameObjects were created (so we can destroy them if something goes wrong)
									createdCombinedMeshObjects = new ArrayList();
									createdCombinedMeshObjects.Clear();

									// Save Combined Mesh and handle errors in a seperate function
									SaveCombinedMesh( go2, newMesh, m, createNewObjectsWithLayer, createNewObjectsWithTag, createNewObjectsWithMeshColliders, arraysCreated ); // When creating assets normally, we use 0 as the group int.

								}
							}

							// ----------------------------------------------
							// IF A SINGLE MESH IS TOO BIG TO BE ADDED
							// ----------------------------------------------

							// START OF V3
							else if( 
								allow32bitMeshes == false && countCI.mesh.vertexCount >= maxVertices16
							||
								allow32bitMeshes == true && (uint)countCI.mesh.vertexCount >= maxVertices32
							){
								// Show warnings for meshes that are too large
								Debug.LogWarning("MESHKIT: MeshKit detected a Mesh called \"" + countCI.mesh.name + "\" with "+ countCI.mesh.vertexCount + " vertices using the material \""+ m.name + "\". Allowing 32-bit meshes was set to '" + allow32bitMeshes + "'. This mesh is beyond Unity's limitations and cannot be combined. This mesh was skipped.");
							}
							// END OF V3
				
							// Update index
							i++;
								
						}
					}
				}

				// =====================
				// 	CLEANUP
				// =====================
				
				// If we have chosen to delete the source objects ...
				if(deleteSourceObjects){

					// Show Progress Bar
					if( maxProgress > 0 ){
						EditorUtility.DisplayProgressBar("Combining Mesh", "Cleaning Up ...", 3.0f / 3.0f);
					} else {
						EditorUtility.ClearProgressBar();
					}

					// Loop through the original Renderers list ...
					foreach (var meshRenderer2 in meshRenderers){
						if( meshRenderer2 != null ){

							// Skip this meshRenderer if it has a MeshFilter with submeshes
							if ( meshRenderer2.gameObject.GetComponent<MeshFilter>() &&
								meshRenderer2.gameObject.GetComponent<MeshFilter>().sharedMesh != null &&
								meshRenderer2.gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount > 1){
								continue;
							}

							// If this original object didn't have a collider, it is no longer needed and can be destroyed.
							else if( meshRenderer2 != null && meshRenderer2.gameObject.GetComponent<Collider>() == null ){
								//DestroyImmediate(meshRenderer2.gameObject);
								Undo.DestroyObjectImmediate (meshRenderer2.gameObject);

							// Otherwise, destroy uneeded Components ...
							} else {

								// Otherwise, Destroy MeshRenderers
								if( meshRenderer2 != null && meshRenderer2.gameObject.GetComponent<MeshRenderer>() != null ){
								//	DestroyImmediate(meshRenderer2.gameObject.GetComponent<MeshRenderer>());
									Undo.DestroyObjectImmediate (meshRenderer2.gameObject.GetComponent<MeshRenderer>());
								}

								// Destroy MeshFilters
								if( meshRenderer2 != null && meshRenderer2.gameObject.GetComponent<MeshFilter>() != null ){
								//	DestroyImmediate(meshRenderer2.gameObject.GetComponent<MeshFilter>());
									Undo.DestroyObjectImmediate (meshRenderer2.gameObject.GetComponent<MeshFilter>());
								}
							}
						}
					}

					// Delete Any object with disabled Renderers
					if( deleteObjectsWithDisabledRenderers ){
						Renderer[] theRenderers = theTransform.GetComponentsInChildren<Renderer>(true) as Renderer[];
						foreach(Renderer r in theRenderers){
							if( r!=null && r.enabled == false && r.gameObject != null){
								Destroy(r.gameObject);
							}
						}
					}

					// Loop through all the transforms and destroy any blank objects
					if( deleteEmptyObjects ){
						DeleteOldBlankObjectsInEditor( theTransform );
					}

				}

				// Mark MeshFilters were combined so we know whether to show display dialogs
				meshFiltersWereCombined = true;

			// If we aren't recreating more Meshes, restore the MeshRenderers	
			} else {
				if(MeshKitGUI.verbose){ Debug.Log("No mesh filters can be combined in this group."); }
				foreach( MeshRenderer mr in renderersThatWereDisabled ){
					if(mr!=null){ mr.enabled = true; }
				}
			}

			// Stop Progress Bar
			maxProgress = 0;
			progress = 0;
			EditorUtility.ClearProgressBar();	

			// If no Mesh filters were combined, show message.
			if( meshFiltersWereCombined == false ){
				EditorUtility.DisplayDialog("Mesh Combiner", "No mesh filters can be combined from this GameObject or it's children.", "Okay");
			}
			
		}
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		//	SAVE COMBINED MESH
		//	Allows us to save the combined mesh in a seperate function to make the code cleaner.
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static void SaveCombinedMesh( GameObject go, Mesh newMesh, Material m, int createNewObjectsWithLayer, string createNewObjectsWithTag, bool createNewObjectsWithMeshColliders, int groupInt ){

			// If the Support Folders exist ... Save The Mesh!
			if( MeshAssets.HaveSupportFoldersBeenCreated() ){

				// Add A MeshFilter to the new GameObject
				MeshFilter filter = go.AddComponent<MeshFilter>();

				// Create and return the Mesh
				Mesh cMesh = MeshAssets.CreateMeshAsset( newMesh, MeshAssets.ProcessFileName(MeshAssets.combinedMeshFolder, m.name + "_" + groupInt.ToString() + " ["+m.shader.name+"]", "Combined", false) );

				if(cMesh!=null){
					filter.sharedMesh = cMesh;
					createdCombinedMeshObjects.Add(filter.gameObject);
				}

				// ========================
				//	ON ERROR
				// ========================

				// Was there an error creating the last asset?
				if(MeshAssets.errorCreatingAsset){

					// Reset the flag
					MeshAssets.errorCreatingAsset = false;

					// Stop Progress Bar
					maxProgress = 0;
					progress = 0;
					EditorUtility.ClearProgressBar();	

					// Reset the old MeshRenderers
					foreach( MeshRenderer mr in renderersThatWereDisabled ){
						if(mr!=null){ mr.enabled = true; }
					}	

					// Destroy the previously created combined Mesh GameObjects
					foreach( GameObject createdGO in createdCombinedMeshObjects){
						if(createdGO!=null){ 
							Undo.ClearUndo(createdGO);
							DestroyImmediate(createdGO); 
						}
					}
					// Destroy the currently created GameObject too!
					if(filter.gameObject != null ){ DestroyImmediate(filter.gameObject); } 
					
					// Show Error Message To User:
					EditorUtility.DisplayDialog("Mesh Combiner", "Unfortunately there was a problem with the combined mesh of group: \n\n"+m.name + "  ["+m.shader.name+"]\n\nThe Combine process must now be aborted.", "Okay");

					// Break out of the function
					return;
				}
			}
		 
			var renderer = go.AddComponent<MeshRenderer>();
			renderer.material = m;

			// Set Layer and Tag
			if(createNewObjectsWithLayer >= 0){ go.layer = createNewObjectsWithLayer; }
			if(createNewObjectsWithTag!=""){ go.tag = createNewObjectsWithTag;}

			// Create Mesh Colliders
			if( createNewObjectsWithMeshColliders ){ go.AddComponent<MeshCollider>(); }

			// Register the creation of this last gameobject
			Undo.RegisterCreatedObjectUndo (go, "MeshKit (Combine)");

		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	DELETE OLD BLANK OBJECTS IN EDITOR
		//	EDITOR VERSION ONLY - Doesnt Work At Runtime!
		////////////////////////////////////////////////////////////////////////////////////////////////
		
		public static void DeleteOldBlankObjectsInEditor( Transform theTransform  ){

			// Loop through all the transforms and destroy any blank objects
			for( int x = 0; x < 20; x++ ){
				// Loop through all the transforms and destroy any blank objects
				Transform[] theTransforms = theTransform.GetComponentsInChildren<Transform>();
				foreach(var t in theTransforms){
					if(t.gameObject.GetComponents<Component>().Length == 1 && t.childCount == 0 ){
						// DestroyImmediate(t.gameObject);
						Undo.DestroyObjectImmediate (t.gameObject);
					}
				}
			}
		}




// ->


		////////////////////////////////////////////////////////////////////////////////////////////////
		//	MESH DIAGNOSTIC
		////////////////////////////////////////////////////////////////////////////////////////////////

		/*
		[MenuItem ("Window/MeshKit -> Mesh Diagnostic")]
		public static void  MeshDiagnostic () {
			GameObject go = Selection.activeGameObject;

			if( go != null && 
				go.GetComponent<SkinnedMeshRenderer>() != null && 
				go.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
			){
				// Cache Variables 
				var smr = go.GetComponent<SkinnedMeshRenderer>();
				var mesh = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;

				Debug.Log( 
					"MESHKIT -> Inspecting: " + mesh.name + "\n" +
					
					"\nTriangles: " + mesh.triangles.Length +
					"\nVertices: " + mesh.vertexCount + 
					"\nNormals: " + mesh.normals.Length + 
					"\nTangents: " + mesh.tangents.Length + 
					"\nUV1: " + mesh.uv.Length + 
					"\nUV2: " + mesh.uv2.Length + 
					"\nUV3: " + mesh.uv3.Length + 
					"\nUV4: " + mesh.uv4.Length + 
					"\nboneWeights: " + mesh.boneWeights.Length +
					"\nBindPoses: " + mesh.bindposes.Length +
					"\nSMR bones: " + smr.bones.Length + 
					"\nColors: " + mesh.colors.Length +
					"\nBlendshapes: " + mesh.blendShapeCount +
					"\n"

				);


			}

		}
		*/
	}

}

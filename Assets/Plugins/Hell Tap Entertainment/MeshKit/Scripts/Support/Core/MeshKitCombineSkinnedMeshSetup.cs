////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MeshKitCombineSkinnedMeshSetup.cs
//
//  This component allows us to setup and create combined Skinned Meshes.
//
//  © 2022 Melli Georgiou.
//  Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

// NOTES: Comment this out to see values in inspector
#define HIDE_VALUES_IN_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HellTap.MeshKit {

	[DisallowMultipleComponent]
	[AddComponentMenu("MeshKit/Combine SkinnedMeshRenderer Setup")]
	public sealed class MeshKitCombineSkinnedMeshSetup : MonoBehaviour {

	/// -> VARIABLES

		// What kind of combine method should we use?
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public CombineMode combineMode = CombineMode.CombineMeshesWithMaterialArray;
			public enum CombineMode { CombineMeshesWithMaterialArray, CombineMeshesWithTextureAtlasing }

		
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#else
			[Header("Combine Shader Properties")]
		#endif
		// Property List - used to set which texture properties will be processed during atlassing
		public List<CombineSkinnedMeshRendererSetup> propertyList = new List<CombineSkinnedMeshRendererSetup>(){ 
			new CombineSkinnedMeshRendererSetup( "_MainTex", MissingTextureFallback.White )
		};

		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#else
			[Header("Options")]
		#endif
		public bool bakeColorIntoMainTex = true;


		// Maximum size of Atlas
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public MaxAtlasSize maximumAtlasSize = MaxAtlasSize._2048;
			public enum MaxAtlasSize { _1024 = 0, _2048 = 1, _4096 = 2, _8192 = 3 }


		// Helpers -> Combine method breaks up the property list above to have seperate arrays.
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public string[] propertyNames = new string[0];

		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public MissingTextureFallback[] textureFallbacks = new MissingTextureFallback[0];


			// -------------------------------------------
			// CLASS: COMBINE SKINNED MESH RENDERER SETUP
			// -------------------------------------------
			
			[System.Serializable]
			public class CombineSkinnedMeshRendererSetup {

				// Variables
				public string propertyName = "_MainTex";
				public MissingTextureFallback missingTextureFallback = MissingTextureFallback.White;

				// Default Constructor
				public CombineSkinnedMeshRendererSetup(){
					propertyName = "_MainTex";
					missingTextureFallback = MissingTextureFallback.White;
				}

				// Custom Constructor
				public CombineSkinnedMeshRendererSetup( string propertyName, MissingTextureFallback missingTextureFallback ){
					this.propertyName = propertyName;
					this.missingTextureFallback = missingTextureFallback;
				}
			}

				// If there are missing textures, we need to know how to replace them. This helps us know what to do on each texture type
				public enum MissingTextureFallback { White = 0, Grey = 1, TransparentBlack = 2, Normal = 3 }

			
			// -------------------------------
			// EDITOR: DEFAULT PROPERTY LISTS
			// -------------------------------

			// Drop-down Helper Lists
			#if HIDE_VALUES_IN_EDITOR
				[HideInInspector]
			#endif
			public DropDownHelperList dropDownHelperList = DropDownHelperList.Select;
				public enum DropDownHelperList {
					Select = 0,
					UnlitShader = 1,
					BumpedShader = 2,
					StandardShader = 3
				}

			// Default For Unlit Shader
			#if HIDE_VALUES_IN_EDITOR
				[HideInInspector]
			#endif
			public readonly List<CombineSkinnedMeshRendererSetup> _defaultUnlitPropertyList =new List<CombineSkinnedMeshRendererSetup>(){ 
				new CombineSkinnedMeshRendererSetup( "_MainTex", MissingTextureFallback.White )
			};

			// Default For Bumped Shader
			#if HIDE_VALUES_IN_EDITOR
				[HideInInspector]
			#endif
			public readonly List<CombineSkinnedMeshRendererSetup> _defaultBumpedPropertyList =new List<CombineSkinnedMeshRendererSetup>(){ 
				new CombineSkinnedMeshRendererSetup( "_MainTex", MissingTextureFallback.White ),
				new CombineSkinnedMeshRendererSetup( "_BumpMap", MissingTextureFallback.Normal )
			};

			// Default For Standard Shader
			#if HIDE_VALUES_IN_EDITOR
				[HideInInspector]
			#endif
			public readonly List<CombineSkinnedMeshRendererSetup> _defaultStandardPropertyList =new List<CombineSkinnedMeshRendererSetup>(){ 
				new CombineSkinnedMeshRendererSetup( "_MainTex", MissingTextureFallback.White ),
				new CombineSkinnedMeshRendererSetup( "_BumpMap", MissingTextureFallback.Normal ),
				new CombineSkinnedMeshRendererSetup( "_MetallicGlossMap", MissingTextureFallback.Grey ),
				new CombineSkinnedMeshRendererSetup( "_OcclusionMap", MissingTextureFallback.White ),
				new CombineSkinnedMeshRendererSetup( "_EmissionMap", MissingTextureFallback.TransparentBlack )
			};
			


		// ------------------------
		//	DEFAULT PROPERTY LISTS
		// ------------------------

		// Store results of the combine so we can revert it later if needed.
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#else
			[Header("Results")]
		#endif
		public SkinnedMeshRenderer[] originalSMRs = new SkinnedMeshRenderer[0];							// Original SkinnedMeshRenderers
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public SkinnedMeshRenderer newSMR = null;
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public Animator[] originalAnimators = new Animator[0];
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public AnimatorCullingMode[] originalAnimatorCullingModes = new AnimatorCullingMode[0];
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public Animation[] originalAnimations = new Animation[0];
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public AnimationCullingType[] originalAnimationCullingTypes = new AnimationCullingType[0];
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public CombineMode generatedCombineMode = CombineMode.CombineMeshesWithMaterialArray;
		
		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#endif
		public bool generated = false;


		// ------------------------
		//	SAVE ASSETS HERE
		// ------------------------

		#if HIDE_VALUES_IN_EDITOR
			[HideInInspector]
		#else
			[Header("Save Assets Here:")]
		#endif
		public string assetRelativePathToFolder = string.Empty;

	/// -> IS GENERATED

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	IS GENERATED
		//	Gets if this object has already been combined.
		////////////////////////////////////////////////////////////////////////////////////////////////

		public bool IsGenerated {

			get { return generated; }
		}

	/// -> UNDO COMBINE

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	UNDO COMBINE
		//	Restore this GameObject to how it was before the Combine process
		////////////////////////////////////////////////////////////////////////////////////////////////

		[ContextMenu("Undo Combine Operation")]	// <- Gives us a dropdown menu in the inspector!

		// MAIN PUBLIC METHOD (This will call one of the variant methods below)
		public void UndoCombine() {

			// Make sure we've generated a mesh ...
			if( generated == true ){

				#if UNITY_EDITOR

					// If the editor isn't playing or will change play mode, allow us to make this operation 'undoable'
					if( UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false ){

						UndoCombine_InEditorWithUndo();

					// Otherwise do it the standard way
					} else {

						UndoCombine_AtRuntime();

					}

				// Always do this without undo if we're not in the Editor.
				#else

					UndoCombine_AtRuntime();

				#endif
			}
		}

		// METHOD VARIANT 1 -> Editor Version with undo functionality
		#if UNITY_EDITOR

			void UndoCombine_InEditorWithUndo(){

				// Destroy the new Skinned Mesh Renderer
				SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
				if( smr != null ){
					//DestroyImmediate( smr );
					UnityEditor.Undo.DestroyObjectImmediate ( smr );
				}

				// Enable the original Skinned Mesh Renderers
				for( int i = 0; i < originalSMRs.Length; i++ ){
					UnityEditor.Undo.RecordObject ( originalSMRs[i], "Undo Combine Operation");
					originalSMRs[i].enabled = true;
				}

				// Reset Original Animators
				for( int i = 0; i < originalAnimators.Length; i++ ){
					UnityEditor.Undo.RecordObject ( originalAnimators[i], "Undo Combine Operation");
					originalAnimators[i].cullingMode = originalAnimatorCullingModes[i];
				}

				// Reset Original Animations
				for( int i = 0; i < originalAnimations.Length; i++ ){
					UnityEditor.Undo.RecordObject ( originalAnimations[i], "Undo Combine Operation");
					originalAnimations[i].cullingType = originalAnimationCullingTypes[i];
				}

				// Record the state of this component
				UnityEditor.Undo.RecordObject ( this, "Undo Combine Operation");

				// Set generated to false
				generated = false;

			}
		#endif

		// METHOD VARIANT 2 -> Runtime version without Undo functionality
		void UndoCombine_AtRuntime(){

			// Destroy the new Skinned Mesh Renderer
			SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
			if( smr != null ){
				DestroyImmediate( smr );
			}

			// Enable the original Skinned Mesh Renderers
			for( int i = 0; i < originalSMRs.Length; i++ ){
				originalSMRs[i].enabled = true;
			}

			// Reset Original Animators
			for( int i = 0; i < originalAnimators.Length; i++ ){
				originalAnimators[i].cullingMode = originalAnimatorCullingModes[i];
			}

			// Reset Original Animations
			for( int i = 0; i < originalAnimations.Length; i++ ){
				originalAnimations[i].cullingType = originalAnimationCullingTypes[i];
			}

			// Set generated to false
			generated = false;

		}

	}
}
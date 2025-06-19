////////////////////////////////////////////////////////////////////////////////////////////////
//
//  CombineSkinnedMeshRenderersAtRuntime.cs
//
//	Combines a Skinned MeshRenderer at runtime
//
//	© 2022 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HellTap.MeshKit;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	// Class
	[DisallowMultipleComponent]
	[AddComponentMenu("MeshKit/Combine Skinned Mesh Renderers At Runtime")]
	public class CombineSkinnedMeshRenderersAtRuntime : MonoBehaviour {

		// Combine Options
		[Header("Combine Options")]

			[Tooltip("Only GameObjects with their Renderer component enabled will be combined.")]
			public bool onlyCombineEnabledRenderers = true;				// Only combine enabled Renderers

		
		[Header("Cleanup Options")]

			[Tooltip("Destroys all GameObjects that are in this group with disabled Renderer components (This includes objects with active Colliders).")]
			public bool destroyObjectsWithDisabledRenderers = false;

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		//	Run the method
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){

			MeshKit.CombineSkinnedMeshRenderers( gameObject, onlyCombineEnabledRenderers, destroyObjectsWithDisabledRenderers );
		}

	}
}

/*
This asset was uploaded by https://unityassetcollection.com
*/



////////////////////////////////////////////////////////////////////////////////////////////////
//
//  SeparateSkinnedMeshRendererAtRuntime.cs
//
//	Separates a Skinned MeshRenderer at runtime
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
	[AddComponentMenu("MeshKit/Separate Skinned Mesh Renderer At Runtime")]
	public class SeparateSkinnedMeshRendererAtRuntime : MonoBehaviour {

		[Header("Cleanup Options")]

			[Tooltip("After separating, destroy the original SkinnedMeshRenderer component.")]
			public bool destroyOriginalSkinnedMeshRenderer = false;

			[Tooltip("After separating, inactivate the GameObjects of all child Skinned Mesh Renderers.")]
			public bool inactivateAllChildSkinnedMeshRendererGameObjects = true;

			[Tooltip("After separating, inactivate the GameObjects all child Mesh Renderers.")]
			public bool inactivateAllChildMeshRendererGameObjects = false;

			[Tooltip("Destroys the GameObjects above instead of inactivating them.")]
			public bool destroyAllChildrenWithDisabledRenderers = false;

		////////////////////////////////////////////////////////////////////////////////////////////////
		//	START
		//	Run the method
		////////////////////////////////////////////////////////////////////////////////////////////////

		void Start(){

			MeshKit.SeparateSkinnedMeshRenderer( gameObject, destroyOriginalSkinnedMeshRenderer, inactivateAllChildSkinnedMeshRendererGameObjects, inactivateAllChildMeshRendererGameObjects, destroyAllChildrenWithDisabledRenderers );
		}

	}
}

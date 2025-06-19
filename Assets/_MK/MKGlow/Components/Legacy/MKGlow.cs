//////////////////////////////////////////////////////
// MK Glow 	    	    	                        //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MK.Glow.Legacy
{
	#if UNITY_2018_3_OR_NEWER
        [ExecuteAlways]
    #else
        [ExecuteInEditMode]
    #endif
    [DisallowMultipleComponent]
    [ImageEffectAllowedInSceneView]
    [RequireComponent(typeof(UnityEngine.Camera))]
	public class MKGlow : MonoBehaviour, MK.Glow.ICameraData, MK.Glow.ISettings
	{
        #if UNITY_EDITOR
        public bool showEditorMainBehavior = true;
		public bool showEditorBloomBehavior;
		public bool showEditorLensSurfaceBehavior;
		public bool showEditorLensFlareBehavior;
		public bool showEditorGlareBehavior;
        #endif

        //Main
        public bool allowGeometryShaders = true;
        public bool allowComputeShaders = true;
        public RenderPriority renderPriority = RenderPriority.Balanced;
        public DebugView debugView = MK.Glow.DebugView.None;
        public Quality quality = MK.Glow.Quality.High;
        public AntiFlickerMode antiFlickerMode = AntiFlickerMode.Balanced;
        public Workflow workflow = MK.Glow.Workflow.Threshold;
        public LayerMask selectiveRenderLayerMask = -1;
        [Range(-1f, 1f)]
        public float anamorphicRatio = 0f;
        [Range(0f, 1f)]
        public float lumaScale = 0.5f;
        [Range(0f, 1f)]
		public float blooming = 0f;

        //Bloom
        [MK.Glow.MinMaxRange(0, 10)]
        public MinMaxRange bloomThreshold = new MinMaxRange(1.25f, 10f);
        [Range(1f, 10f)]
		public float bloomScattering = 7f;
		public float bloomIntensity = 1f;

        //LensSurface
        public bool allowLensSurface = false;
		public Texture2D lensSurfaceDirtTexture;
		public float lensSurfaceDirtIntensity = 2.5f;
		public Texture2D lensSurfaceDiffractionTexture;
		public float lensSurfaceDiffractionIntensity = 2.0f;

        //LensFlare
        public bool allowLensFlare = false;
        public LensFlareStyle lensFlareStyle = LensFlareStyle.Average;
        [Range(0f, 25f)]
		public float lensFlareGhostFade = 10.0f;
		public float lensFlareGhostIntensity = 1.0f;
        [MK.Glow.MinMaxRange(0, 10)]
		public MinMaxRange lensFlareThreshold = new MinMaxRange(1.3f, 10f);
        [Range(0f, 8f)]
		public float lensFlareScattering = 5f;
		public Texture2D lensFlareColorRamp;
        [Range(-100f, 100f)]
		public float lensFlareChromaticAberration = 53f;
        [Range(1, 4)]
		public int lensFlareGhostCount = 3;
        [Range(-1f, 1f)]
		public float lensFlareGhostDispersal = 0.6f;
        [Range(0f, 25f)]
		public float lensFlareHaloFade = 2f;
		public float lensFlareHaloIntensity = 1.0f;
        [Range(0f, 1f)]
		public float lensFlareHaloSize = 0.4f;

        //Glare
        public bool allowGlare = false;
        [Range(0.0f, 1.0f)]
        public float glareBlend = 0.33f;
        public float glareIntensity = 1f;
        [Range(0.0f, 360.0f)]
        public float glareAngle = 0f;
        [MK.Glow.MinMaxRange(0, 10)]
        public MinMaxRange glareThreshold = new MinMaxRange(1.25f, 10f);
        [Range(1, 4)]
        public int glareStreaks = 4;
        public GlareStyle glareStyle = GlareStyle.DistortedCross;
        [Range(0.0f, 4.0f)]
        public float glareScattering = 2f;
        //Sample0
        [Range(0f, 10f)]
        public float glareSample0Scattering = 5f;
        [Range(0f, 360f)]
        public float glareSample0Angle = 0f;
        public float glareSample0Intensity = 1f;
        [Range(-5f, 5f)]
        public float glareSample0Offset = 0f;
        //Sample1
        [Range(0f, 10f)]
        public float glareSample1Scattering = 5f;
        [Range(0f, 360f)]
        public float glareSample1Angle = 45f;
        public float glareSample1Intensity = 1f;
        [Range(-5f, 5f)]
        public float glareSample1Offset = 0f;
        //Sample0
        [Range(0f, 10f)]
        public float glareSample2Scattering = 5f;
        [Range(0f, 360f)]
        public float glareSample2Angle = 90f;
        public float glareSample2Intensity = 1f;
        [Range(-5f, 5f)]
        public float glareSample2Offset = 0f;
        //Sample0
        [Range(0f, 10f)]
        public float glareSample3Scattering = 5f;
        [Range(0f, 360f)]
        public float glareSample3Angle = 135f;
        public float glareSample3Intensity = 1f;
        [Range(-5f, 5f)]
        public float glareSample3Offset = 0f;

        private Effect _effect;

        private RenderTarget _source, _destination;

		private UnityEngine.Camera renderingCamera
		{
			get { return GetComponent<UnityEngine.Camera>(); }
		}

        /// <summary>
        /// Load some mobile optimized settings
        /// </summary>
        [ContextMenu("Load Preset For Mobile")]
        private void LoadMobilePreset()
        {
            bloomScattering = 5f;
            renderPriority = RenderPriority.Performance;
            quality = Quality.Low;
            allowGlare = false;
            allowLensFlare = false;
            lensFlareScattering = 5;
            allowLensSurface = false;
        }

        /// <summary>
        /// Load some quality optimized settings
        /// </summary>
        [ContextMenu("Load Preset For Quality")]
        private void LoadQualityPreset()
        {
            bloomScattering = 7f;
            renderPriority = RenderPriority.Quality;
            quality = Quality.High;
            allowGlare = false;
            allowLensFlare = false;
            lensFlareScattering = 6;
            allowLensSurface = false;
        }

		public void OnEnable()
		{
            _effect = new Effect();
			_effect.Enable(RenderPipeline.Legacy);

            enabled = Compatibility.IsSupported;
		}

		public void OnDisable()
		{
			_effect.Disable();
		}

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if(workflow == Workflow.Selective && (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset || PipelineProperties.xrEnabled))
            {
                Graphics.Blit(source, destination);
                return;
            }

            _source.renderTexture = source;
            _destination.renderTexture = destination;
			_effect.Build(_source, _destination, this, null, this, renderingCamera, false);

            Graphics.Blit(source, destination, _effect.renderMaterialNoGeometry, _effect.currentRenderIndex);
            _effect.AfterCompositeCleanup();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Settings
        /////////////////////////////////////////////////////////////////////////////////////////////
        public bool GetAllowGeometryShaders()
        { 
            return false;
        }
        public bool GetAllowComputeShaders()
        { 
            return false;
        }
        public RenderPriority GetRenderPriority()
        { 
            return renderPriority;
        }
        public MK.Glow.DebugView GetDebugView()
        { 
			return debugView;
		}
        public MK.Glow.Quality GetQuality()
        { 
			return quality;
		}
        public MK.Glow.AntiFlickerMode GetAntiFlickerMode()
        { 
			return antiFlickerMode;
		}
        public MK.Glow.Workflow GetWorkflow()
        { 
			return workflow;
		}
        public LayerMask GetSelectiveRenderLayerMask()
        { 
			return selectiveRenderLayerMask;
		}
        public float GetAnamorphicRatio()
        { 
			return anamorphicRatio;
		}
        public float GetLumaScale()
        { 
			return lumaScale;
		}
		public float GetBlooming()
		{ 
			return blooming;
		}

        //Bloom
		public MK.Glow.MinMaxRange GetBloomThreshold()
		{ 
			return bloomThreshold;
		}
		public float GetBloomScattering()
		{ 
			return bloomScattering;
		}
		public float GetBloomIntensity()
		{ 
			return bloomIntensity;
		}

        //LensSurface
		public bool GetAllowLensSurface()
		{ 
			return allowLensSurface;
		}
		public Texture2D GetLensSurfaceDirtTexture()
		{ 
			return lensSurfaceDirtTexture;
		}
		public float GetLensSurfaceDirtIntensity()
		{ 
			return lensSurfaceDirtIntensity;
		}
		public Texture2D GetLensSurfaceDiffractionTexture()
		{ 
			return lensSurfaceDiffractionTexture;
		}
		public float GetLensSurfaceDiffractionIntensity()
		{ 
			return lensSurfaceDiffractionIntensity;
		}

        //LensFlare
		public bool GetAllowLensFlare()
		{ 
			return allowLensFlare;
		}
        public LensFlareStyle GetLensFlareStyle()
		{ 
			return lensFlareStyle;
		}
		public float GetLensFlareGhostFade()
		{ 
			return lensFlareGhostFade;
		}
		public float GetLensFlareGhostIntensity()
		{ 
			return lensFlareGhostIntensity;
		}
		public MK.Glow.MinMaxRange GetLensFlareThreshold()
		{ 
			return lensFlareThreshold;
		}
		public float GetLensFlareScattering()
		{ 
			return lensFlareScattering;
		}
		public Texture2D GetLensFlareColorRamp()
		{ 
			return lensFlareColorRamp;
		}
		public float GetLensFlareChromaticAberration()
		{ 
			return lensFlareChromaticAberration;
		}
		public int GetLensFlareGhostCount()
		{ 
			return lensFlareGhostCount;
		}
		public float GetLensFlareGhostDispersal()
		{ 
			return lensFlareGhostDispersal;
		}
		public float GetLensFlareHaloFade()
		{
			return lensFlareHaloFade;
		}
		public float GetLensFlareHaloIntensity()
		{ 
			return lensFlareHaloIntensity;
		}
		public float GetLensFlareHaloSize()
		{ 
			return lensFlareHaloSize;
		}

        public void SetLensFlareGhostFade(float fade)
        {
            lensFlareGhostFade = fade;
        }
        public void SetLensFlareGhostCount(int count)
        {
            lensFlareGhostCount = count;
        }
        public void SetLensFlareGhostDispersal(float dispersal)
        {
            lensFlareGhostDispersal = dispersal;
        }
        public void SetLensFlareHaloFade(float fade)
        {
            lensFlareHaloFade = fade;
        }
        public void SetLensFlareHaloSize(float size)
        {
            lensFlareHaloSize = size;
        }

        //Glare
		public bool GetAllowGlare()
		{ 
			return allowGlare;
		}
        public float GetGlareBlend()
        { 
			return glareBlend;
		}
        public float GetGlareIntensity()
        {
            return glareIntensity;
        }
        public float GetGlareAngle()
        {
            return glareAngle;
        }
		public MK.Glow.MinMaxRange GetGlareThreshold()
		{ 
			return glareThreshold;
		}
		public int GetGlareStreaks()
		{ 
			return glareStreaks;
		}
        public void SetGlareStreaks(int count)
        {
            glareStreaks = count;
        }
        public float GetGlareScattering()
        {
            return glareScattering;
        }
        public GlareStyle GetGlareStyle()
        {
            return glareStyle;
        }

        //Sample0
        public float GetGlareSample0Scattering()
        {
            return glareSample0Scattering;
        }
        public float GetGlareSample0Angle()
        {
            return glareSample0Angle;
        }
        public float GetGlareSample0Intensity()
        {
            return glareSample0Intensity;
        }
        public float GetGlareSample0Offset()
        {
            return glareSample0Offset;
        }

        public void SetGlareSample0Scattering(float scattering)
        {
            glareSample0Scattering = scattering;
        }
        public void SetGlareSample0Angle(float angle)
        {
            glareSample0Angle = angle;
        }
        public void SetGlareSample0Intensity(float intensity)
        {
            glareSample0Intensity = intensity;
        }
        public void SetGlareSample0Offset(float offset)
        {
            glareSample0Offset = offset;
        }

        //Sample1
        public float GetGlareSample1Scattering()
        {
            return glareSample1Scattering;
        }
        public float GetGlareSample1Angle()
        {
            return glareSample1Angle;
        }
        public float GetGlareSample1Intensity()
        {
            return glareSample1Intensity;
        }
        public float GetGlareSample1Offset()
        {
            return glareSample1Offset;
        }

        public void SetGlareSample1Scattering(float scattering)
        {
            glareSample1Scattering = scattering;
        }
        public void SetGlareSample1Angle(float angle)
        {
            glareSample1Angle = angle;
        }
        public void SetGlareSample1Intensity(float intensity)
        {
            glareSample1Intensity = intensity;
        }
        public void SetGlareSample1Offset(float offset)
        {
            glareSample1Offset = offset;
        }

        //Sample2
        public float GetGlareSample2Scattering()
        {
            return glareSample2Scattering;
        }
        public float GetGlareSample2Angle()
        {
            return glareSample2Angle;
        }
        public float GetGlareSample2Intensity()
        {
            return glareSample2Intensity;
        }
        public float GetGlareSample2Offset()
        {
            return glareSample2Offset;
        }

        public void SetGlareSample2Scattering(float scattering)
        {
            glareSample2Scattering = scattering;
        }
        public void SetGlareSample2Angle(float angle)
        {
            glareSample2Angle = angle;
        }
        public void SetGlareSample2Intensity(float intensity)
        {
            glareSample2Intensity = intensity;
        }
        public void SetGlareSample2Offset(float offset)
        {
            glareSample2Offset = offset;
        }

        //Sample3
        public float GetGlareSample3Scattering()
        {
            return glareSample3Scattering;
        }
        public float GetGlareSample3Angle()
        {
            return glareSample3Angle;
        }
        public float GetGlareSample3Intensity()
        {
            return glareSample3Intensity;
        }
        public float GetGlareSample3Offset()
        {
            return glareSample3Offset;
        }

        public void SetGlareSample3Scattering(float scattering)
        {
            glareSample3Scattering = scattering;
        }
        public void SetGlareSample3Angle(float angle)
        {
            glareSample3Angle = angle;
        }
        public void SetGlareSample3Intensity(float intensity)
        {
            glareSample3Intensity = intensity;
        }
        public void SetGlareSample3Offset(float offset)
        {
            glareSample3Offset = offset;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // Camera Data
        /////////////////////////////////////////////////////////////////////////////////////////////
        public int GetCameraWidth()
        {
            return renderingCamera.pixelWidth;
        }
        public int GetCameraHeight()
        {
            return renderingCamera.pixelHeight;
        }
        public bool GetStereoEnabled()
        {
            return renderingCamera.stereoEnabled;
        }
        public float GetAspect()
        {
            return renderingCamera.aspect;
        }
        public Matrix4x4 GetWorldToCameraMatrix()
        {
            return renderingCamera.worldToCameraMatrix;
        }
        public bool GetOverwriteDescriptor()
        {
            return false;
        }
        public UnityEngine.Rendering.TextureDimension GetOverwriteDimension()
        {
            return UnityEngine.Rendering.TextureDimension.Tex2D;
        }
        public int GetOverwriteVolumeDepth()
        {
            return 1;
        }
        public bool GetTargetTexture()
        {
            return renderingCamera.targetTexture != null ? true : false;
        }
	}
}
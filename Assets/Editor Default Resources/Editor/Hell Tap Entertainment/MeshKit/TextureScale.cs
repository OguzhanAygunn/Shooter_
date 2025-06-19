
/*
This asset was uploaded by https://unityassetcollection.com
*/

////////////////////////////////////////////////////////////////////////////////////////////////
//
//  TextureScale.cs
//
//	Helper methods for rescaling textures on the main thread. Unity's built-in version
//	sometimes doesn't work in certain situations which is why this is needed.
//
//	NOTE: Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable!
//
//	© 2021 Melli Georgiou.
//	Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellTap.MeshKit { 
	public static class TextureScale {

	/// -> POINT SCALE

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	POINT
		//	Rescales a Texture2D using a new width and height using a 'Point' algorithm
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Point( Texture2D tex, int newWidth, int newHeight ){

			// ---------------------
			// INITIAL CHECKS
			// ---------------------
			
			if( tex == null || tex.width <= 0 || tex.height <= 0 || newWidth <= 0 || newHeight <= 0 ){
				Debug.LogError( "ERROR: Skipping Point Scale. Tex is null, or width / height is equal or less than 0.");
				return;
			}

			// ---------------------
			// SETUP HELPER VALUES
			// ---------------------

			// Setup color arrays
			Color[] texColors = tex.GetPixels();
			Color[] newColors = new Color[newWidth * newHeight];
			
			// Setup Ratio for point
			float ratioX = ((float)tex.width) / newWidth;
			float ratioY = ((float)tex.height) / newHeight;

			// Setup cached widths
			int w = tex.width;
			int w2 = newWidth;


			// ---------------------
			// POINT PROCESSING
			// ---------------------

			// Loop through the height of the texture
			for ( int y = 0; y < newHeight; y++ ){

				var thisY = (int)(ratioY * y) * w;
				var yw = y * w2;
				for (var x = 0; x < w2; x++) {
					newColors[yw + x] = texColors[(int)(thisY + ratioX*x)];
				}
			}

			// ---------------------
			// RECREATE THE TEXTURE
			// ---------------------

			// Resize the supplied texture to the new dimensions
			tex.Reinitialize(newWidth, newHeight);

			// Set the pixels using our newly processed pixels
			tex.SetPixels(newColors);

			// Apply it to the texture
			tex.Apply();


			// ---------------------
			// CLEAN UP MEMORY
			// ---------------------
	 
			// Explicitly Clean up the helper color arrays
			texColors = null;
			newColors = null;
	 
		}

	/// -> BILINEAR SCALE

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	BILINEAR
		//	Rescales a Texture2D using a new width and height using a 'Bilinear' algorithm
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public static void Bilinear( Texture2D tex, int newWidth, int newHeight ){

			// ---------------------
			// INITIAL CHECKS
			// ---------------------

			if( tex == null || tex.width <= 0 || tex.height <= 0 || newWidth <= 0 || newHeight <= 0 ){
				Debug.LogError( "ERROR: Skipping Bilinear Scale. Tex is null, or width / height is equal or less than 0.");
				return;
			}

			// ---------------------
			// SETUP HELPER VALUES
			// ---------------------

			// Setup color arrays
			Color[] texColors = tex.GetPixels();
			Color[] newColors = new Color[newWidth * newHeight];
			
			// Setup ratios for bilinear processing
			float ratioX = 1.0f / ((float)newWidth / (tex.width-1));
			float ratioY = 1.0f / ((float)newHeight / (tex.height-1));
		  
			// Setup cached widths
			int w = tex.width;
			int w2 = newWidth;


			// ---------------------
			// BILINEAR PROCESSING
			// ---------------------

			// Loop through the height of the texture
			for ( int y = 0; y < newHeight; y++ ){

				int yFloor = (int)Mathf.Floor(y * ratioY);
				var y1 = yFloor * w;
				var y2 = (yFloor+1) * w;
				var yw = y * w2;
	 
				for (var x = 0; x < w2; x++) {
					int xFloor = (int)Mathf.Floor(x * ratioX);
					var xLerp = x * ratioX-xFloor;
					newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor+1], xLerp),
														   ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor+1], xLerp),
														   y*ratioY-yFloor);
				}
			}

			// ---------------------
			// RECREATE THE TEXTURE
			// ---------------------

			// Resize the supplied texture to the new dimensions
			tex.Reinitialize(newWidth, newHeight);

			// Set the pixels using our newly processed pixels
			tex.SetPixels(newColors);

			// Apply it to the texture
			tex.Apply();


			// ---------------------
			// CLEAN UP MEMORY
			// ---------------------
	 
			// Explicitly Clean up the helper color arrays
			texColors = null;
			newColors = null;
	 
		}

	/// -> [HELPER] COLOR LERP UNCLAMPED

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//	[HELPER] COLOR LERP UNCLAMPED
		//	Lerps two colors together without clamping
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		private static Color ColorLerpUnclamped (Color c1, Color c2, float value){

			return new Color (
				c1.r + (c2.r - c1.r) * value, 
				c1.g + (c2.g - c1.g) * value, 
				c1.b + (c2.b - c1.b) * value, 
				c1.a + (c2.a - c1.a) * value
			);
		}

	}
}
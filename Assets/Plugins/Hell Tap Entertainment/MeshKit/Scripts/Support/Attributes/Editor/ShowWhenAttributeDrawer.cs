////////////////////////////////////////////////////////////////////////////////////////////////
//
//  ShowWhenAttributeDrawer.cs
//
//  Helps create better inspectors for runtime components.
//
//  © 2022 Melli Georgiou.
//  Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

// Use HellTap Namespace
namespace HellTap.MeshKit {

	[CustomPropertyDrawer(typeof(ShowWhenAttribute))]
	public class ShowWhenAttributeDrawer : ConditionalPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (this.IsVisible(property))
			{
				EditorGUI.PropertyField(position, property, label);
			}
		}
	}
}
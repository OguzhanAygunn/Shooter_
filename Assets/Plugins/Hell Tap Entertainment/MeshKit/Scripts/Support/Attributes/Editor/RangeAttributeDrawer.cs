////////////////////////////////////////////////////////////////////////////////////////////////
//
//  RangeDrawer.cs
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

	[CustomPropertyDrawer(typeof(RangeAttribute))]
	public class RangeDrawer : ConditionalPropertyDrawer
	{
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (this.IsVisible(property))
			{
				// First get the attribute since it contains the range for the slider
				RangeAttribute range = attribute as RangeAttribute;

				// Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
				if (property.propertyType == SerializedPropertyType.Float)
					EditorGUI.Slider(position, property, range.fMin, range.fMax, label);
				else if (property.propertyType == SerializedPropertyType.Integer)
					EditorGUI.IntSlider(position, property, range.iMin, range.iMax, label);
				else
					EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
			}
		}
	}
}
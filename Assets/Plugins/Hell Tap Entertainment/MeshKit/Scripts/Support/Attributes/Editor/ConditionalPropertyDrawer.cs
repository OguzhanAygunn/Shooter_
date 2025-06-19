////////////////////////////////////////////////////////////////////////////////////////////////
//
//  ConditionalPropertyDrawer.cs
//
//  Helps create better inspectors for runtime components.
//
//  © 2022 Melli Georgiou.
//  Hell Tap Entertainment LTD
//
////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

// Use HellTap Namespace
namespace HellTap.MeshKit {

    public class ConditionalPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !this.IsVisible(property) ? 0f : base.GetPropertyHeight(property, label);
        }

        private bool IsPropertyVisible(SerializedProperty prop, ShowWhenAttribute attrib)
        {
            if (prop.propertyType == SerializedPropertyType.Boolean &&
                attrib.type == ShowWhenAttribute.Type.Boolean)
            {
                switch (attrib.condition)
                {
                    case ShowWhenAttribute.Condition.Equals:
                        return prop.boolValue == attrib.boolValue;
                    default:
                        return prop.boolValue != attrib.boolValue;
                }
            }
            else if (
                prop.propertyType == SerializedPropertyType.Float &&
                attrib.type == ShowWhenAttribute.Type.Float)
            {
                switch (attrib.condition)
                {
                    case ShowWhenAttribute.Condition.Equals:
                        return prop.floatValue == attrib.floatValue;
                    case ShowWhenAttribute.Condition.Greater:
                        return prop.floatValue > attrib.floatValue;
                    case ShowWhenAttribute.Condition.Less:
                        return prop.floatValue < attrib.floatValue;
                    case ShowWhenAttribute.Condition.NotEquals:
                        return prop.floatValue != attrib.floatValue;
                }
            }
            else if (
                prop.propertyType == SerializedPropertyType.Integer &&
                attrib.type == ShowWhenAttribute.Type.Integer)
            {
                switch (attrib.condition)
                {
                    case ShowWhenAttribute.Condition.Equals:
                        return prop.intValue == attrib.intValue;
                    case ShowWhenAttribute.Condition.Greater:
                        return prop.intValue > attrib.intValue;
                    case ShowWhenAttribute.Condition.Less:
                        return prop.intValue < attrib.intValue;
                    case ShowWhenAttribute.Condition.NotEquals:
                        return prop.intValue != attrib.intValue;
                }
            }
            else if (
                prop.propertyType == SerializedPropertyType.Enum &&
                attrib.type == ShowWhenAttribute.Type.Integer)
            {
                switch (attrib.condition)
                {
                    case ShowWhenAttribute.Condition.Equals:
                        return prop.enumValueIndex == attrib.intValue;
                    case ShowWhenAttribute.Condition.Greater:
                        return prop.enumValueIndex > attrib.intValue;
                    case ShowWhenAttribute.Condition.Less:
                        return prop.enumValueIndex < attrib.intValue;
                    case ShowWhenAttribute.Condition.NotEquals:
                        return prop.enumValueIndex != attrib.intValue;
                }
            }
            else if (
                prop.propertyType == SerializedPropertyType.String &&
                    (attrib.type == ShowWhenAttribute.Type.String ||
                    attrib.type == ShowWhenAttribute.Type.Boolean))
            {
                if (attrib.type == ShowWhenAttribute.Type.String)
                {
                    switch (attrib.condition)
                    {
                        case ShowWhenAttribute.Condition.Equals:
                            return prop.stringValue == attrib.stringValue;

                        case ShowWhenAttribute.Condition.Greater:
                        case ShowWhenAttribute.Condition.Less:
                        case ShowWhenAttribute.Condition.NotEquals:
                            return prop.stringValue != attrib.stringValue;
                    }
                }
                else
                {
                    return prop.stringValue.Length > 0;
                }
            }
            else if (
                prop.propertyType == SerializedPropertyType.ObjectReference &&
                    (attrib.type == ShowWhenAttribute.Type.Object ||
                    attrib.type == ShowWhenAttribute.Type.Boolean))
            {
                if (attrib.type == ShowWhenAttribute.Type.Object)
                {
                    switch (attrib.condition)
                    {
                        case ShowWhenAttribute.Condition.Equals:
                            return prop.objectReferenceValue == attrib.objectValue;
                        case ShowWhenAttribute.Condition.NotEquals:
                            return prop.objectReferenceValue != attrib.objectValue;
                        default:
                            return false;
                    }
                }
                else
                {
                    return prop.objectReferenceValue != null;
                }
            }

            return true;
        }

        private bool IsReferencedPropertiesVisible(FieldInfo[] fieldInfos, SerializedObject serializedObj, string fieldName)
        {
            foreach (var field in fieldInfos)
            {
                if (field.Name == fieldName)
                {
                    object[] attribs = field.GetCustomAttributes(true);
                    foreach (object attrib in attribs)
                    {
                        ShowWhenAttribute swa = attrib as ShowWhenAttribute;
                        if (swa != null)
                        {
                            SerializedProperty prop = serializedObj.FindProperty(swa.propertyName);

                            if(prop != null)
                            {
                                if (!IsPropertyVisible(prop, swa))
                                    return false;

                                if (!IsReferencedPropertiesVisible(fieldInfos, serializedObj, prop.name))
                                    return false;

                                break;
                            }
                        }
                    }

                    break;
                }
            }

            return true;
        }

        protected bool IsVisible(SerializedProperty property)
        {
            ShowWhenAttribute attrib = this.attribute as ShowWhenAttribute;

            if (attrib == null)
                return true;

            if (attrib.type == ShowWhenAttribute.Type.None)
                return true;

            SerializedProperty prop = property.serializedObject.FindProperty(attrib.propertyName);

            if(prop == null)
            {
                Debug.LogError("ConditionalAttribute: unknown property name: " + attrib.propertyName + " (used in class '" + property.serializedObject.targetObject.GetType() + "')");
                return true;
            }

            Type targetObjectType = prop.serializedObject.targetObject.GetType();
            FieldInfo[] fieldInfos = targetObjectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // firstly check if referenced properties are visible
            if (!IsReferencedPropertiesVisible(fieldInfos, property.serializedObject, attrib.propertyName))
                return false;

            return IsPropertyVisible(prop, attrib);
        }
    }
}

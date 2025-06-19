////////////////////////////////////////////////////////////////////////////////////////////////
//
//  ShowWhenAttribute.cs
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

	public class ShowWhenAttribute : PropertyAttribute
	{
		public enum Condition
		{
			Equals,
			NotEquals,
			Greater,
			Less
		}

		public enum Type
		{
			None,  // always returns visible
			Boolean,
			Float,
			Integer,
			Object,
			String
		}

		public ShowWhenAttribute()
		{
			this.type = Type.None;
		}

		public ShowWhenAttribute(string propertyName)
		{
			this.propertyName = propertyName;
			this.boolValue = true;
			this.condition = Condition.Equals;
			this.type = Type.Boolean;
		}

		public ShowWhenAttribute(string propertyName, Condition cond, bool value)
		{
			this.propertyName = propertyName;
			this.condition = cond;
			this.boolValue = value;
			this.type = Type.Boolean;
		}

		public ShowWhenAttribute(string propertyName, Condition cond, float value)
		{
			this.propertyName = propertyName;
			this.condition = cond;
			this.floatValue = value;
			this.type = Type.Float;
		}

		public ShowWhenAttribute(string propertyName, Condition cond, int value)
		{
			this.propertyName = propertyName;
			this.condition = cond;
			this.intValue = value;
			this.type = Type.Integer;
		}

		public ShowWhenAttribute(string propertyName, Condition cond, Object value)
		{
			this.propertyName = propertyName;
			this.condition = cond;
			this.objectValue = value;
			this.type = Type.Object;
		}

		public ShowWhenAttribute(string propertyName, Condition cond, string value)
		{
			this.propertyName = propertyName;
			this.condition = cond;
			this.stringValue = value;
			this.type = Type.String;
		}

		public string propertyName { get; private set; }
		public Condition condition { get; private set; }
		public Type type { get; private set; }

		public bool boolValue { get; private set; }
		public float floatValue { get; private set; }
		public int intValue { get; private set; }
		public Object objectValue { get; private set; }
		public string stringValue { get; private set; }
	}
}
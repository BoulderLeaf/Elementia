#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Attribute to select a single layer.
/// </summary>
public class LayerAttribute : PropertyAttribute
{ 
    // NOTHING - just oxygen.
}

[CustomPropertyDrawer(typeof(LayerAttribute))]
class LayerAttributeEditor : PropertyDrawer
{
   
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // One line of  oxygen free code.
        property.intValue = EditorGUI.LayerField(position, label,  property.intValue);
    }
}
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NConstrainableAttribute))]
    public class NConstrainableAttributeDrawer : PropertyDrawer
    {
        private static HashSet<int> _contrainedSet = new HashSet<int>();

        private int _id;
        private Vector3 _cacheValue;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = CustomPropertyDrawerCache.getPropertyHeight(property, label);
            if (height.HasValue) return height.Value;
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.type == nameof(Vector3))
            {
                if (_id == 0)
                {
                    _id = (property.serializedObject.targetObject.GetInstanceID() + property.propertyPath).GetHashCode();
                }
                var isConstrained = _contrainedSet.Contains(_id);

                _cacheValue = property.vector3Value;
                var togglePosition = position;
                togglePosition.width = 19;
                togglePosition.x = position.width + 1;
                position.width -= togglePosition.width * 2;
                isConstrained = EditorGUI.Toggle(togglePosition, isConstrained);
                EditorGUI.PropertyField(position, property, label);
                if (isConstrained)
                {
                    _contrainedSet.Add(_id);
                    var vector3Value = property.vector3Value;

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.EndChangeCheck();
                    float ratio;

                    if (property.vector3Value.x != _cacheValue.x)
                    {
                        if (_cacheValue.x == _cacheValue.y && _cacheValue.y == _cacheValue.z)
                        {
                            vector3Value.x = vector3Value.y = vector3Value.z = property.vector3Value.x;
                        }
                        else
                        {
                            vector3Value.x = property.vector3Value.x;
                            if (_cacheValue.x != 0)
                            {
                                ratio = property.vector3Value.x / _cacheValue.x;
                                vector3Value.y *= ratio;
                                vector3Value.z *= ratio;
                            }
                        }
                    }
                    else if (property.vector3Value.y != _cacheValue.y)
                    {
                        if (_cacheValue.x == _cacheValue.y && _cacheValue.y == _cacheValue.z)
                        {
                            vector3Value.x = vector3Value.y = vector3Value.z = property.vector3Value.y;
                        }
                        else
                        {
                            vector3Value.y = property.vector3Value.y;
                            if (_cacheValue.y != 0)
                            {
                                ratio = property.vector3Value.y / _cacheValue.y;
                                vector3Value.x *= ratio;
                                vector3Value.z *= ratio;
                            }
                        }
                    }
                    else if (property.vector3Value.z != _cacheValue.z)
                    {
                        if (_cacheValue.x == _cacheValue.y && _cacheValue.y == _cacheValue.z)
                        {
                            vector3Value.x = vector3Value.y = vector3Value.z = property.vector3Value.z;
                        }
                        else
                        {
                            vector3Value.z = property.vector3Value.z;
                            if (_cacheValue.z != 0)
                            {
                                ratio = property.vector3Value.z / _cacheValue.z;
                                vector3Value.x *= ratio;
                                vector3Value.y *= ratio;
                            }
                        }
                    }

                    property.vector3Value = vector3Value;
                    _cacheValue = property.vector3Value;
                }
                else
                {
                    _contrainedSet.Remove(_id);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
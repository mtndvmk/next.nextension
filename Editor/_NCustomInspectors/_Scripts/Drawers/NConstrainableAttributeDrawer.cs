using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Nextension.NEditor
{
    [CustomPropertyDrawer(typeof(NConstrainableAttribute))]
    public class NConstrainableAttributeDrawer : PropertyDrawer
    {
        private static HashSet<int> _constrainedSet = new HashSet<int>();

        private static Texture2D _disabled_contrained_texture;
        private static Texture2D _enabled_contrained_texture;

        private int _id;
        private Vector3 _cacheValue;

        private static void loadTextures([CallerFilePath] string filePath = null)
        {
            if (_disabled_contrained_texture == null)
            {
                var queryString = "_NCustomInspectors";
                var textureDir = Path.Combine(filePath[..(filePath.IndexOf(queryString) + queryString.Length)], "_Textures");


                var disabled_contrained_texture_path = Path.Combine(textureDir, "disable_constrained.png");
                var enable_contrained_texture_path = Path.Combine(textureDir, "enable_constrained.png");

                var d_bytes = File.ReadAllBytes(disabled_contrained_texture_path);
                _disabled_contrained_texture = new Texture2D(2, 2);
                _disabled_contrained_texture.LoadImage(d_bytes);

                var e_bytes = File.ReadAllBytes(enable_contrained_texture_path);
                _enabled_contrained_texture = new Texture2D(2, 2);
                _enabled_contrained_texture.LoadImage(e_bytes);
            }
        }

        static NConstrainableAttributeDrawer()
        {
            loadTextures();
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            if (__isSupportType(property))
            {
                if (_id == 0)
                {
                    _id = (property.serializedObject.targetObject.GetInstanceID() + property.propertyPath).GetHashCode();
                }

                var isConstrained = _constrainedSet.Contains(_id);

                _cacheValue = __getVector3Value(property);

                var indent = EditorGUI.indentLevel * 15;
                var togglePosition = position;
                togglePosition.height = togglePosition.width = 19;
                togglePosition.x = EditorGUIUtility.labelWidth - 5;
                GUIContent buttonGuiContent;

                loadTextures();

                if (isConstrained)
                {
                    buttonGuiContent = new GUIContent(_enabled_contrained_texture, "Disable constrained proportions");
                }
                else
                {
                    buttonGuiContent = new GUIContent(_disabled_contrained_texture, "Enable constrained proportions");
                }
                if (GUI.Button(togglePosition, buttonGuiContent, EditorStyles.whiteLabel))
                {
                    isConstrained = !isConstrained;
                }

                EditorGUI.LabelField(position, label);
                position.xMin = EditorGUIUtility.labelWidth + 20;
                EditorGUI.PropertyField(position, property, new GUIContent());

                if (isConstrained)
                {
                    _constrainedSet.Add(_id);
                    
                    var vector3Value = __getVector3Value(property);
                    var orginVector3 = vector3Value;

                    float ratio;

                    if (orginVector3.x != _cacheValue.x)
                    {
                        if (_cacheValue.x == _cacheValue.y && _cacheValue.y == _cacheValue.z)
                        {
                            vector3Value.x = vector3Value.y = vector3Value.z = orginVector3.x;
                        }
                        else
                        {
                            vector3Value.x = orginVector3.x;
                            if (_cacheValue.x != 0)
                            {
                                ratio = orginVector3.x / _cacheValue.x;
                                vector3Value.y *= ratio;
                                vector3Value.z *= ratio;
                            }
                        }
                    }
                    else if (orginVector3.y != _cacheValue.y)
                    {
                        if (_cacheValue.x == _cacheValue.y && _cacheValue.y == _cacheValue.z)
                        {
                            vector3Value.x = vector3Value.y = vector3Value.z = orginVector3.y;
                        }
                        else
                        {
                            vector3Value.y = orginVector3.y;
                            if (_cacheValue.y != 0)
                            {
                                ratio = orginVector3.y / _cacheValue.y;
                                vector3Value.x *= ratio;
                                vector3Value.z *= ratio;
                            }
                        }
                    }
                    else if (orginVector3.z != _cacheValue.z)
                    {
                        if (_cacheValue.x == _cacheValue.y && _cacheValue.y == _cacheValue.z)
                        {
                            vector3Value.x = vector3Value.y = vector3Value.z = orginVector3.z;
                        }
                        else
                        {
                            vector3Value.z = orginVector3.z;
                            if (_cacheValue.z != 0)
                            {
                                ratio = orginVector3.z / _cacheValue.z;
                                vector3Value.x *= ratio;
                                vector3Value.y *= ratio;
                            }
                        }
                    }

                    __saveProperty(property, vector3Value);
                }
                else
                {
                    _constrainedSet.Remove(_id);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
            EditorGUI.EndChangeCheck();
        }
        
        private Vector3 __getVector3Value(SerializedProperty property)
        {
            if (property.type == nameof(float3))
            {
                return (float3)property.boxedValue;
            }
            else
            {
                return property.vector3Value;
            }
        }
        private bool __isSupportType(SerializedProperty property)
        {
            if (property.type == nameof(Vector3)) return true;
            if (property.type == nameof(float3)) return true;
            return false;
        }
        private void __saveProperty(SerializedProperty property, Vector3 value)
        {
            if (property.type == nameof(float3))
            {
                property.boxedValue = (float3)value;
            }
            else
            {
                property.vector3Value = value;
            }
        }
    }
}
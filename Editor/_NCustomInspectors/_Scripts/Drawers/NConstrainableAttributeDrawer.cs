using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
            if (property.type == nameof(Vector3))
            {
                if (_id == 0)
                {
                    _id = (property.serializedObject.targetObject.GetInstanceID() + property.propertyPath).GetHashCode();
                }
                var isConstrained = _constrainedSet.Contains(_id);

                _cacheValue = property.vector3Value;
                var togglePosition = position;
                togglePosition.width = 19;
                togglePosition.x = position.width * 0.4f - 15;
                togglePosition.height *= 1.2f;
                togglePosition.y = 0;
                GUIContent buttonGuiContent;
                if (isConstrained)
                {
                    buttonGuiContent = new GUIContent(_enabled_contrained_texture, "Disable constrained proportions");
                }
                else
                {
                    buttonGuiContent = new GUIContent(_disabled_contrained_texture, "Enable constrained proportions");
                }

                if (GUI.Button(togglePosition, buttonGuiContent, EditorStyles.linkLabel))
                {
                    isConstrained = !isConstrained;
                }

                EditorGUI.PropertyField(position, property, label);

                if (isConstrained)
                {
                    _constrainedSet.Add(_id);
                    var vector3Value = property.vector3Value;

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
                    _constrainedSet.Remove(_id);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
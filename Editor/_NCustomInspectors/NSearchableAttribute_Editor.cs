namespace Nextension.NEditor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(NSearchableAttribute))]
    public class SearchableAttributeDrawer : PropertyDrawer
    {
        private string search = "";
        private string[] options;
        private string[] allOptions;
        private GUIStyle searchTextFieldStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);

            if (property.propertyType == SerializedPropertyType.Enum)
                height = height * 2 + EditorGUIUtility.standardVerticalSpacing;

            NSearchableAttribute searchableAttribute = NEditorHelper.getAttribute<NSearchableAttribute>(property);
            if (searchableAttribute != null)
            {
                if (property.propertyType == SerializedPropertyType.String && searchableAttribute.searchType == NSearchableAttribute.NSearchType.TYPE_AS_STRING)
                {
                    height = height * 2 + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                if (searchTextFieldStyle == null)
                    searchTextFieldStyle = GUI.skin.FindStyle("ToolbarSeachTextField");

                if (options == null || allOptions == null)
                {
                    allOptions = property.enumDisplayNames;
                    UpdateOptions();
                }

                Rect searchRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                DrawSearchBar(searchRect, label);

                Rect popupRect = new Rect(position.x, searchRect.y + searchRect.height + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
                DrawEnumPopup(popupRect, property);
            }
            else
            {
                if (searchTextFieldStyle == null)
                    searchTextFieldStyle = GUI.skin.FindStyle("ToolbarSeachTextField");

                NSearchableAttribute searchableAttribute = NEditorHelper.getAttribute<NSearchableAttribute>(property);
                if (searchableAttribute != null)
                {
                    if (property.propertyType == SerializedPropertyType.String && searchableAttribute.searchType == NSearchableAttribute.NSearchType.TYPE_AS_STRING)
                    {
                        if (options == null || allOptions == null)
                        {
                            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t => t.IsClass);

                            if (searchableAttribute.baseType != null)
                            {

                                allTypes = allTypes.Where(t => NUtils.isInherited(t, searchableAttribute.baseType));
                            }

                            var typeArr = allTypes.ToArray();
                            allOptions = new string[typeArr.Length];
                            for (int i = 0; i < typeArr.Length; ++i)
                            {
                                allOptions[i] = typeArr[i].FullName;
                            }
                            UpdateOptions();
                        }

                        Rect searchRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                        DrawSearchBar(searchRect, label);

                        Rect popupRect = new Rect(position.x, searchRect.y + searchRect.height + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
                        DrawStringTypePopup(popupRect, property);
                        return;
                    }
                }
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private void DrawSearchBar(Rect position, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            search = EditorGUI.TextField(position, label, search, searchTextFieldStyle);
            if (EditorGUI.EndChangeCheck())
                UpdateOptions();
        }

        private void DrawEnumPopup(Rect position, SerializedProperty property)
        {
            Rect fieldRect = EditorGUI.PrefixLabel(position, new GUIContent(" "));
            int currentIndex;
            if (property.enumValueIndex >= 0)
            {
                currentIndex = Array.IndexOf(options, property.enumDisplayNames[property.enumValueIndex]);
            }
            else
            {
                currentIndex = -1;
            }
            int selectedIndex = EditorGUI.Popup(fieldRect, currentIndex, options);
            if (selectedIndex >= 0)
            {
                int newIndex = Array.IndexOf(property.enumDisplayNames, options[selectedIndex]);
                if (newIndex != currentIndex)
                {
                    property.enumValueIndex = newIndex;
                    search = string.Empty;
                    UpdateOptions();
                }
            }
        }

        private void DrawStringTypePopup(Rect position, SerializedProperty property)
        {
            Rect fieldRect = EditorGUI.PrefixLabel(position, new GUIContent(" "));
            string currentTypeString = property.stringValue;
            int currentIndex = Array.IndexOf(options, currentTypeString);
            int selectedIndex = EditorGUI.Popup(fieldRect, currentIndex, options);
            if (selectedIndex >= 0)
            {
                int newIndex = Array.IndexOf(allOptions, options[selectedIndex]);
                var newString = allOptions[newIndex];
                if (newString != currentTypeString)
                {
                    property.stringValue = newString;
                    search = string.Empty;
                    UpdateOptions();
                }
            }
        }

        private void UpdateOptions()
        {
            options = Array.FindAll(allOptions, name => string.IsNullOrEmpty(search) || name.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}

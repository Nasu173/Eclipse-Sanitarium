using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.LabelField(position, label.text, "Use SubclassSelector only with [SerializeReference]");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, 10, EditorGUIUtility.singleLineHeight);
        Rect labelRect = new Rect(position.x + 12, position.y, EditorGUIUtility.labelWidth - 12, EditorGUIUtility.singleLineHeight);
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

        // 1. 下拉框选择类型
        string fullTypename = property.managedReferenceFullTypename;
        string currentTypeName = string.IsNullOrEmpty(fullTypename) ? "None (Null)" : fullTypename.Split(' ').Last().Split('.').Last();

        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(currentTypeName), FocusType.Keyboard))
        {
            Type baseType = GetFieldType(fieldInfo);
            var derivedTypes = GetDerivedTypes(baseType);

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(fullTypename), () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            foreach (var type in derivedTypes)
            {
                menu.AddItem(new GUIContent(type.Name), fullTypename.EndsWith(type.Name), () =>
                {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        // 2. 绘制折叠箭头和标签
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);
        EditorGUI.LabelField(labelRect, label);

        // 3. 递归绘制子属性
        if (property.isExpanded && !string.IsNullOrEmpty(fullTypename))
        {
            EditorGUI.indentLevel++;
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            
            float currentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;
                float h = EditorGUI.GetPropertyHeight(iterator, true);
                EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, h), iterator, true);
                currentY += h + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded && !string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }
        return height;
    }

    private Type GetFieldType(FieldInfo fieldInfo)
    {
        if (fieldInfo == null) return null;
        Type fieldType = fieldInfo.FieldType;
        if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            return fieldType.GetGenericArguments()[0];
        }
        return fieldType;
    }

    private IEnumerable<Type> GetDerivedTypes(Type baseType)
    {
        if (baseType == null) return Enumerable.Empty<Type>();
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract && p.IsClass);
    }
}

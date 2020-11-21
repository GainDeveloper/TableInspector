using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditor.TreeViewExamples;

namespace TableInspector
{
    public abstract class SerializedObjectEntry
    {
        public enum SortType
        {
            SortType_String,
            SortType_Float,
            SortType_Bool,
            SortType_Integer,
            SortType_ObjectReference,
            SortType_Enum,
            SortType_Invalid
        }

        internal abstract string Name { get; }
        internal abstract void Cache();
        internal abstract void SelectSerializedObject(int serializedObjectIndex);
        internal abstract void DrawElement(Rect cellRect, int serializedObjectIndex, int propertyIndex);
        internal abstract void BeginDrawingSerializedObject(int serializedObjectIndex);
        internal abstract bool FinishedDrawingSerializedObject(int serializedObjectIndex);

        internal abstract MultiColumnHeaderState CreateDefaultMultiColumnHeaderState();
        internal abstract List<ListElement> GenerateTree();
        internal abstract SortType GetPropertySortType(int propertyIndex);
        internal abstract string GetPropertyStringValue(int serializedObjectIndex, int propertyIndex);
        internal abstract float GetPropertyFloatValue(int serializedObjectIndex, int propertyIndex);
        internal abstract bool GetPropertyBoolValue(int serializedObjectIndex, int propertyIndex);
        internal abstract int GetPropertyIntValue(int serializedObjectIndex, int propertyIndex);
        internal abstract string GetPropertyObjectReferenceValue(int serializedObjectIndex, int propertyIndex);
        internal abstract string GetPropertyEnumValue(int serializedObjectIndex, int propertyIndex);

    }

    public class SerializedObjectEntry<T> : SerializedObjectEntry where T : Object
    {
        SerializedObject[] serializedObjects;
        string[] propertyNames;

        public SerializedObjectEntry(params string[] propertyNames)
        {
            this.propertyNames = propertyNames;
        }

        internal override string Name => typeof(T).Name;

        internal override void Cache()
        {
            serializedObjects = Resources.FindObjectsOfTypeAll<T>().Where(x => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(x))).Select(x => new SerializedObject(x)).ToArray();

            List<string> tempPropNames = new List<string>();
            if (propertyNames.Length == 0)
            {
                SerializedProperty prop = serializedObjects[0].GetIterator();
                if (prop.NextVisible(true))
                {
                    do
                    {
                        tempPropNames.Add(prop.name);
                    }
                    while (prop.NextVisible(false));
                }

                this.propertyNames = tempPropNames.ToArray();
            }
        }

        internal override void SelectSerializedObject(int serializedObjectIndex)
        {
            Selection.activeObject = serializedObjects[serializedObjectIndex].targetObject;
        }

        internal override void DrawElement(Rect cellRect, int serializedObjectIndex, int propertyIndex)
        {
            if (propertyIndex == 0)
            {
                Object targetObject = serializedObjects[serializedObjectIndex].targetObject;
                string assetName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(targetObject));

                EditorGUI.LabelField(cellRect, new GUIContent(assetName, EditorGUIUtility.ObjectContent(targetObject, serializedObjects[serializedObjectIndex].GetType()).image));
                return;
            }

            var sp = serializedObjects[serializedObjectIndex].FindProperty(propertyNames[propertyIndex-1]);

            if (sp != null)
                EditorGUI.PropertyField(cellRect, sp, GUIContent.none, true);
            else
                EditorGUI.HelpBox(cellRect, "Invalid Property", MessageType.Error);
        }

        internal override void BeginDrawingSerializedObject(int serializedObjectIndex)
        {
            serializedObjects[serializedObjectIndex].UpdateIfRequiredOrScript();
        }

        internal override bool FinishedDrawingSerializedObject(int serializedObjectIndex)
        {
            if (serializedObjects[serializedObjectIndex].hasModifiedProperties)
                return serializedObjects[serializedObjectIndex].ApplyModifiedProperties();
            return false;
        }

        internal override MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[propertyNames.Length+1];

            columns[0] = new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Asset"),
                contextMenuText = "Asset",
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = 100,
                minWidth = 30,
                maxWidth = 200,
                autoResize = true,
                allowToggleVisibility = true
            };
            

            for (int i = 0; i < propertyNames.Length; i++)
            {
                columns[i+1] = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(ObjectNames.NicifyVariableName(propertyNames[i])),
                    contextMenuText = ObjectNames.NicifyVariableName(propertyNames[i]) + " ("+ propertyNames[i] + ")",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 30,
                    maxWidth = 200,
                    autoResize = true,
                    allowToggleVisibility = true
                };
            }
            
            return new MultiColumnHeaderState(columns);
        }

        internal override List<ListElement> GenerateTree()
        {
            int IDCounter = 0;
            var treeElements = new List<ListElement>(serializedObjects.Length);

            var root = new ListElement("Root", -1, IDCounter);
            treeElements.Add(root);
            for (int i = 0; i < serializedObjects.Length; ++i)
            {
                var child = new ListElement(serializedObjects[i].targetObject.name, 0, ++IDCounter);
                treeElements.Add(child);
            }

            return treeElements;
        }

        internal override SortType GetPropertySortType(int propertyIndex)
        {
            var sp = serializedObjects[0].FindProperty(propertyNames[propertyIndex - 1]);
            if (sp == null)
                return SortType.SortType_Invalid;
            
            switch (sp.propertyType)
            {
                case SerializedPropertyType.Float:
                    return SortType.SortType_Float;
                case SerializedPropertyType.Boolean:
                    return SortType.SortType_Bool;
                case SerializedPropertyType.Integer:
                    return SortType.SortType_Integer;
                case SerializedPropertyType.Enum:
                    return SortType.SortType_Enum;
                case SerializedPropertyType.ObjectReference:
                    return SortType.SortType_ObjectReference;
                case SerializedPropertyType.String:
                    return SortType.SortType_String;
                default:
                    return SortType.SortType_Invalid;
            }
        }

        internal SerializedProperty GetSerializedPropety(int serializedObjectIndex, int propertyIndex)
        {
            return serializedObjects[serializedObjectIndex].FindProperty(propertyNames[propertyIndex - 1]);
        }

        internal override float GetPropertyFloatValue(int serializedObjectIndex, int propertyIndex) => GetSerializedPropety(serializedObjectIndex, propertyIndex).floatValue;

        internal override string GetPropertyStringValue(int serializedObjectIndex, int propertyIndex) => GetSerializedPropety(serializedObjectIndex, propertyIndex).stringValue;

        internal override bool GetPropertyBoolValue(int serializedObjectIndex, int propertyIndex) => GetSerializedPropety(serializedObjectIndex, propertyIndex).boolValue;

        internal override int GetPropertyIntValue(int serializedObjectIndex, int propertyIndex) => GetSerializedPropety(serializedObjectIndex, propertyIndex).intValue;

        internal override string GetPropertyObjectReferenceValue(int serializedObjectIndex, int propertyIndex) => GetSerializedPropety(serializedObjectIndex, propertyIndex).objectReferenceValue?.name;

        internal override string GetPropertyEnumValue(int serializedObjectIndex, int propertyIndex)
        {
            var sp = GetSerializedPropety(serializedObjectIndex, propertyIndex);
            return sp.enumDisplayNames[sp.enumValueIndex];
        }

    }
}

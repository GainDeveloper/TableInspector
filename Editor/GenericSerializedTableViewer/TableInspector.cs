using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TableInspector
{
    public static class TableInspector
    {
        private static List<SerializedObjectEntry> genericSerializedEntries = new List<SerializedObjectEntry>();

        public static void AddGenericSerializedEntry(SerializedObjectEntry entry) => genericSerializedEntries.Add(entry);

        internal static SerializedObjectEntry GetEntry(int index)
        {
            if (index == -1 || index >= genericSerializedEntries.Count)
                return null;

            return genericSerializedEntries[index];
        }

        internal static int Count => genericSerializedEntries.Count;
    }
}

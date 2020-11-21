using UnityEngine;
using UnityEditor;

namespace CoreEditor.Utilities
{
    [InitializeOnLoad]
    static class Example
    {
        static Example()
        {
            TableInspector.TableInspector.AddGenericSerializedEntry(new TableInspector.SerializedObjectEntry<Rigidbody2D>("m_BodyType", "m_Mass"));
            TableInspector.TableInspector.AddGenericSerializedEntry(new TableInspector.SerializedObjectEntry<Transform>());
            TableInspector.TableInspector.AddGenericSerializedEntry(new TableInspector.SerializedObjectEntry<MeshRenderer>());
        }
    }
}

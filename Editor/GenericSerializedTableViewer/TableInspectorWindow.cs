using System;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace TableInspector
{
    internal class TableInspectorWindow : EditorWindow
	{
        [NonSerialized] bool m_Initialized;
		[SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		SearchField m_SearchField;
		MultiColumnListView m_TreeView;

        [SerializeField]
        int selectedDenericTableDataIndex = -1;
        SerializedObjectEntry SelectedDenericTableData => TableInspector.GetEntry(selectedDenericTableDataIndex);


        [MenuItem("Window/Table Inspector")]
		public static TableInspectorWindow GetWindow ()
		{
			var window = GetWindow<TableInspectorWindow>();
			window.titleContent = new GUIContent("Table Inspector");
			window.Focus();
			window.Repaint();
			return window;
		}

        Rect sideBarRect
        {
            get { return new Rect(10, 10, 180, position.height - 40); }
        }

		Rect multiColumnTreeViewRect
		{
			get { return new Rect(200, 30, position.width-210, position.height -40); }
		}

		Rect toolbarRect
		{
			get { return new Rect (200f, 10f, position.width-200f, 20f); }
		}

		public MultiColumnListView treeView
		{
			get { return m_TreeView; }
		}

		void InitIfNeeded ()
		{
			if (!m_Initialized && SelectedDenericTableData != null)
			{
				// Check if it already exists (deserialized from window layout file or scriptable object)
				if (m_TreeViewState == null)
					m_TreeViewState = new TreeViewState();

                SelectedDenericTableData.Cache();

                bool firstInit = m_MultiColumnHeaderState == null;
				var headerState = SelectedDenericTableData.CreateDefaultMultiColumnHeaderState();
				if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
					MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
				m_MultiColumnHeaderState = headerState;
				
				var multiColumnHeader = new MultiColumnHeader(headerState);
				if (firstInit)
					multiColumnHeader.ResizeToFit ();

				var treeModel = new ListModel(SelectedDenericTableData.GenerateTree());
				
				m_TreeView = new MultiColumnListView(m_TreeViewState, multiColumnHeader, treeModel, SelectedDenericTableData);

				m_SearchField = new SearchField();
				m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;


                m_Initialized = true;
			}
		}

        void SelectGenericTableData(int selectedIndex)
        {
            if (selectedIndex == selectedDenericTableDataIndex)
                return;

            selectedDenericTableDataIndex = selectedIndex;
            m_TreeViewState = null;
            m_MultiColumnHeaderState = null;
            m_Initialized = false;
        }

		void OnGUI ()
		{
            GUILayout.BeginArea(sideBarRect);
            for (int i = 0; i < TableInspector.Count; i++)
            {
                if (GUILayout.Button(TableInspector.GetEntry(i).Name))
                {
                    SelectGenericTableData(i);
                }
            }
            GUILayout.EndArea();

            InitIfNeeded();

            if (SelectedDenericTableData != null)
            {
                SearchBar(toolbarRect);
                m_TreeView.OnGUI(multiColumnTreeViewRect);
            } else
            {
                EditorGUI.HelpBox(multiColumnTreeViewRect, "Select a serialized object to inspect", MessageType.Info);
            }
		}

		void SearchBar (Rect rect)
		{
			treeView.searchString = m_SearchField.OnGUI (rect, treeView.searchString);
		}
	}
}

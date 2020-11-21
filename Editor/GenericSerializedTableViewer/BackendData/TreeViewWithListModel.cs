using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace TableInspector
{
	internal class TreeViewWithListModel : TreeView
	{
		ListModel m_TreeModel;
		readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);

		public ListModel treeModel { get { return m_TreeModel; } }

		public TreeViewWithListModel (TreeViewState state, ListModel model) : base (state)
		{
			Init (model);
		}

		public TreeViewWithListModel (TreeViewState state, MultiColumnHeader multiColumnHeader, ListModel model)
			: base(state, multiColumnHeader)
		{
			Init (model);
		}

		void Init (ListModel model)
		{
			m_TreeModel = model;
		}

		protected override TreeViewItem BuildRoot()
		{
			return new TreeViewItem(0, -1, "Root");
		}

		protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
		{
			m_Rows.Clear ();
			if (!string.IsNullOrEmpty(searchString))
			{
				Search(searchString, m_Rows);
			}
			else
			{
                foreach (ListElement item in m_TreeModel.Items)
                {
                    m_Rows.Add(new TreeViewItem(item.id, 0, item.name));
                }
            }

			SetupParentsAndChildrenFromDepths (root, m_Rows);
			return m_Rows;
		}

		void Search(string search, List<TreeViewItem> result)
		{
			if (string.IsNullOrEmpty(search))
				throw new ArgumentException("Invalid search: cannot be null or empty", "search");

			const int kItemDepth = 0; // tree is flattened when searching

            foreach (var element in m_TreeModel.Items)
            {
                if (element.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Add(new TreeViewItem(element.id, kItemDepth, element.name));
                }
            }

			SortSearchResult(result);
		}

		protected virtual void SortSearchResult (List<TreeViewItem> rows)
		{
			rows.Sort ((x,y) => EditorUtility.NaturalCompare (x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
		}
	}

}

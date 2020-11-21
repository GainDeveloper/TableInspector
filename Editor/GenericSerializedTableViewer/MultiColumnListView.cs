using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace TableInspector
{
    internal class MultiColumnListView : TreeViewWithListModel
	{
		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
        SerializedObjectEntry genericSerializedData;

        public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();
	
			if (root.children == null)
				return;

			for (int i = root.children.Count - 1; i >= 0; i--)
                result.Add(root.children[i]);
		}

        public MultiColumnListView (TreeViewState state, MultiColumnHeader multicolumnHeader, ListModel model, SerializedObjectEntry genericSerializedData) : base (state, multicolumnHeader, model)
		{
            this.genericSerializedData = genericSerializedData;

			// Custom setup
			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			extraSpaceBeforeIconAndLabel = kToggleWidth;
			multicolumnHeader.sortingChanged += OnSortingChanged;
			
			Reload();
		}

		// Note we We only build the visible rows, only the backend has the full tree information. 
		// The treeview only creates info for the row list.
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows (root);
			SortIfNeeded (root, rows);
			return rows;
		}

		void OnSortingChanged (MultiColumnHeader multiColumnHeader)
		{
			SortIfNeeded (rootItem, GetRows());
		}

		void SortIfNeeded (TreeViewItem root, IList<TreeViewItem> rows)
		{
			if (rows.Count <= 1)
				return;
			
			if (multiColumnHeader.sortedColumnIndex == -1)
			{
				return; // No column to sort for (just use the order the data are in)
			}
			
			// Sort the roots of the existing tree items
			SortByMultipleColumns ();
			TreeToList(root, rows);
			Repaint();
		}

		void SortByMultipleColumns ()
		{
			if (multiColumnHeader.state.sortedColumns.Length == 0)
				return;

			var orderedQuery = InitialOrder (rootItem.children, multiColumnHeader.state.sortedColumnIndex);
			rootItem.children = orderedQuery.ToList();
		}

		IOrderedEnumerable<TreeViewItem> InitialOrder(IEnumerable<TreeViewItem> myTypes, int sortedColumn)
		{
            bool ascending = multiColumnHeader.IsSortedAscending(sortedColumn);

            if (sortedColumn == 0)
                return myTypes.Order(l => l.displayName, ascending);

            switch (genericSerializedData.GetPropertySortType(sortedColumn))
            {
                case SerializedObjectEntry.SortType.SortType_Float:
                    return myTypes.Order(l => genericSerializedData.GetPropertyFloatValue(l.id - 1, sortedColumn), ascending);
                case SerializedObjectEntry.SortType.SortType_Bool:
                    return myTypes.Order(l => genericSerializedData.GetPropertyBoolValue(l.id - 1, sortedColumn), ascending);
                case SerializedObjectEntry.SortType.SortType_Enum:
                    return myTypes.Order(l => genericSerializedData.GetPropertyEnumValue(l.id - 1, sortedColumn), ascending);
                case SerializedObjectEntry.SortType.SortType_Integer:
                    return myTypes.Order(l => genericSerializedData.GetPropertyIntValue(l.id - 1, sortedColumn), ascending);
                case SerializedObjectEntry.SortType.SortType_ObjectReference:
                    return myTypes.Order(l => genericSerializedData.GetPropertyObjectReferenceValue(l.id - 1, sortedColumn), ascending);
                case SerializedObjectEntry.SortType.SortType_String:
                    return myTypes.Order(l => genericSerializedData.GetPropertyStringValue(l.id - 1, sortedColumn), ascending);
                default:
                    return myTypes.Order(l => l.displayName, ascending);
            }
        }

        protected override void RowGUI (RowGUIArgs args)
		{
            genericSerializedData.BeginDrawingSerializedObject(args.item.id - 1);

            for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
			{
                genericSerializedData.DrawElement(args.GetCellRect(i), args.item.id - 1, args.GetColumn(i));
			}

            if (genericSerializedData.FinishedDrawingSerializedObject(args.item.id - 1))
                OnSortingChanged(null);

        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            genericSerializedData.SelectSerializedObject(id - 1);

        }
	}

	static class ExtensionMethods
	{
		public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.OrderBy(selector);
			}
			else
			{
				return source.OrderByDescending(selector);
			}
		}
	}
}

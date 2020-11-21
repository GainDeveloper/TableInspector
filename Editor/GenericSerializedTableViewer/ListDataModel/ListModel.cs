using System;
using System.Collections.Generic;
using System.Linq;

namespace TableInspector
{
    internal class ListModel
	{
        IList<ListElement> items;

        public IList<ListElement> Items => items;


        public ListModel (IList<ListElement> data)
		{
			SetData (data);
		}
	
		public void SetData (IList<ListElement> data)
		{
			Init (data);
		}

		void Init (IList<ListElement> data)
		{
            // Fill item array
            items = new List<ListElement>();
            for (int i = 1; i < data.Count; i++)
            {
                items.Add(data[i]);
            }
        }
	}
}

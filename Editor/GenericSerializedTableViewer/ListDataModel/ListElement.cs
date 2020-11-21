using System;
using UnityEngine;

namespace TableInspector
{
    [Serializable]
    internal class ListElement
	{
		[SerializeField] int m_ID;
		[SerializeField] string m_Name;
		[SerializeField] int m_Depth;

		public int depth
		{
			get { return m_Depth; }
			set { m_Depth = value; }
		}

		public string name
		{
			get { return m_Name; } set { m_Name = value; }
		}

		public int id
		{
			get { return m_ID; } set { m_ID = value; }
		}

		public ListElement ()
		{
		}

		public ListElement (string name, int depth, int id)
		{
			m_Name = name;
			m_ID = id;
			m_Depth = depth;
		}
	}
}



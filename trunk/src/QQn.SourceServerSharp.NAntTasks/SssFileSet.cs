using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace QQn.SourceServerSharp.Tasks
{
	[ElementName("sssfileset")]
	public class SssFileSet : FileSet
	{
		string _type;
		string _info;

		public SssFileSet()
		{
		}

		[TaskAttribute("type")]
		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		[TaskAttribute("info")]
		public string Info
		{
			get { return _info; }
			set { _info = value; }
		}
	}
}

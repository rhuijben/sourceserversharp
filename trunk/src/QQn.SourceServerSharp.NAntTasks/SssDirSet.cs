using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Types;
using NAnt.Core.Attributes;

namespace QQn.SourceServerSharp.Tasks
{
	public class SssDirSet : DirSet
	{
		string _type;
		string _info;

		public SssDirSet()
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

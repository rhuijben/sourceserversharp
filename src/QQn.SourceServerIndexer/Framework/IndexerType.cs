using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.SourceServerSharp.Framework
{
	public class IndexerTypeData
	{
		string _path;
		string _type;
		string _info;

		public IndexerTypeData(string path, string type, string info)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			else if (type == null)
				throw new ArgumentNullException("type");

			_path = path;
			_type = type;
			_info = string.IsNullOrEmpty(info) ? "" : info;
		}

		public string Path
		{
			get { return _path; }
		}

		public string Type
		{
			get { return _type; }
		}

		public string Info
		{
			get { return _info; }
		}
	}
}

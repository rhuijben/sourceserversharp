using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.SourceServerIndexer
{
	public class SourceServerIndexer
	{
		IList<string> _symbolFiles = new string[0];
		IList<string> _sourceRoots = new string[0];

		public IList<string> SymbolFiles
		{
			get { return _symbolFiles; }
			set
			{
				if (value != null)
					_symbolFiles = value;
				else
					_symbolFiles = new string[0];
			}
		}

		public IList<string> SourceRoots
		{
			get { return _sourceRoots; }
			set
			{
				if (value != null)
					_sourceRoots = value;
				else
					_sourceRoots = new string[0];
			}
		}

		public bool Exec()
		{
			return true;
		}

	}
}

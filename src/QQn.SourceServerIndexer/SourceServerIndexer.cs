// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.SourceServerIndexer
{
	/// <summary>
	/// 
	/// </summary>
	public class SourceServerIndexer
	{
		IList<string> _symbolFiles = new string[0];
		IList<string> _sourceRoots = new string[0];
		IList<string> _excludeSourceRoots = new string[0];

		/// <summary>
		/// Gets or sets a list of symbol files to index
		/// </summary>
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

		/// <summary>
		/// Gets or sets a list of sourcecode directories to index
		/// </summary>
		/// <remarks>If one or more sourceroots are specified, only files in and below these directories are indexed</remarks>
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

		/// <summary>
		/// Gets or sets a list of sourcecode directories not to index
		/// </summary>
		/// <remarks>These directories allow to exclude specific directories which are included in the <see cref="SourceRoots"/></remarks>
		public IList<string> ExcludeSourceRoots
		{
			get { return _sourceRoots; }
			set
			{
				if (value != null)
					_excludeSourceRoots = value;
				else
					_excludeSourceRoots = new string[0];
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Exec()
		{
			return true;
		}

	}
}

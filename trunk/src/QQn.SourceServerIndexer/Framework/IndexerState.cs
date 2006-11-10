// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class IndexerState
	{
		readonly SortedList<string, SymbolFile> _symbolFiles = new SortedList<string,SymbolFile>(StringComparer.InvariantCultureIgnoreCase);
		readonly SortedList<string, SourceFile> _sourceFiles = new SortedList<string, SourceFile>(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// 
		/// </summary>
		public IndexerState()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public SortedList<string, SymbolFile> SymbolFiles
		{
			get { return _symbolFiles; }
		}

		/// <summary>
		/// 
		/// </summary>
		public SortedList<string, SourceFile> SourceFiles
		{
			get { return _sourceFiles; }
		}
	}
}

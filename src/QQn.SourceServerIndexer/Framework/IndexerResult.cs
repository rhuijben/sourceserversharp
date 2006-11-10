// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	public class IndexerResult
	{
		readonly int _indexedSymbolFiles;
		readonly int _indexedSourceFiles;
		bool _successFull;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="success"></param>
		/// <param name="indexedSymbolFiles"></param>
		/// <param name="indexedSourceFiles"></param>
		public IndexerResult(bool success, int indexedSymbolFiles, int indexedSourceFiles)
		{
			_successFull = success;
			_indexedSymbolFiles = indexedSymbolFiles;
			_indexedSourceFiles = indexedSourceFiles;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool Success
		{
			get { return _successFull; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int IndexedSymbolFiles
		{
			get { return _indexedSymbolFiles; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int IndexedSourceFiles
		{
			get { return _indexedSourceFiles; }
		}

	}
}

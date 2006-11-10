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
		readonly int _providersUsed;
		readonly bool _successFull;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="success"></param>
		/// <param name="indexedSymbolFiles"></param>
		/// <param name="indexedSourceFiles"></param>
		/// <param name="providersUsed"></param>
		public IndexerResult(bool success, int indexedSymbolFiles, int indexedSourceFiles, int providersUsed)
		{
			_successFull = success;
			_indexedSymbolFiles = indexedSymbolFiles;
			_indexedSourceFiles = indexedSourceFiles;
			_providersUsed = providersUsed;
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

		/// <summary>
		/// 
		/// </summary>
		public int ProvidersUsed
		{
			get { return _providersUsed; }
		}
	}
}

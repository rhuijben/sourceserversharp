// **************************************************************************
// * $Id: ABPublish.cs 2581 2006-04-03 15:10:46Z berth $
// * $HeadURL: https://subversion.competence.biz/svn/netdev/Projects/Tcg/TcgTools/trunk/src/BuildTools/TcgNantTasks/AutoBuild/ABPublish.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;
using QQn.SourceServerIndexer.Providers;
using System.IO;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class IndexerState
	{
		readonly SortedList<string, SymbolFile> _symbolFiles = new SortedList<string,SymbolFile>(StringComparer.InvariantCultureIgnoreCase);
		readonly SortedList<string, SourceFile> _sourceFiles = new SortedList<string, SourceFile>(StringComparer.InvariantCultureIgnoreCase);
		readonly List<SourceProvider> _providers = new List<SourceProvider>();

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

		/// <summary>
		/// 
		/// </summary>
		public List<SourceProvider> Providers
		{
			get { return _providers; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string NormalizePath(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			if (!Path.IsPathRooted(path))
				path = Path.GetFullPath(path);

			return path;
		}
	}
}

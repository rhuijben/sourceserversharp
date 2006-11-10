// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using System.ComponentModel;
using QQn.SourceServerIndexer.Framework;

namespace QQn.SourceServerIndexer.MSBuild
{
	/// <summary>
	/// MSBuild SourceServerIndex task implementation
	/// </summary>
	public class SourceServerIndexTask : Task
	{
		ITaskItem[] _symbolRoots = new ITaskItem[0];
		ITaskItem[] _sourceRoots = new ITaskItem[0];
		ITaskItem[] _symbolFiles = new ITaskItem[0];
		ITaskItem[] _providers = new ITaskItem[0];
		ITaskItem[] _types = new ITaskItem[0];
		bool _includeHiddenDirectories;
		bool _includeDotDirs;
		bool _notRecursive;
		string _sourceServerSdkDir;		

		/// <summary>
		/// Gets or sets a list of symbol root directories
		/// </summary>
		/// <remarks>.pdf files in these directories are added to the <see cref="SymbolFiles"/></remarks>
		public ITaskItem[] SymbolRoots
		{
			get { return _symbolRoots; }
			set
			{
				if (value == null)
					_symbolRoots = new ITaskItem[0];
				else
					_symbolRoots = value;
			}
		}

		/// <summary>
		/// Gets or sets a list of source root directories
		/// </summary>
		public ITaskItem[] SourceRoots
		{
			get { return _sourceRoots; }
			set
			{
				if (value == null)
					_sourceRoots = new ITaskItem[0];
				else
					_sourceRoots = value;
			}
		}

		/// <summary>
		/// Gets or sets a list of symbol files
		/// </summary>
		public ITaskItem[] SymbolFiles
		{
			get { return _symbolFiles; }
			set
			{
				if (value == null)
					_symbolFiles = new ITaskItem[0];
				else
					_symbolFiles = value;
			}
		}

		/// <summary>
		/// Gets or sets a list of symbol files
		/// </summary>
		public ITaskItem[] Providers
		{
			get { return _providers; }
			set
			{
				if (value == null)
					_providers = new ITaskItem[0];
				else
					_providers = value;
			}
		}

		/// <summary>
		/// Gets or sets a list of provider types to search
		/// </summary>
		[Required]
		public ITaskItem[] Type
		{
			get { return _types; }
			set
			{
				if (value == null)
					_types = new ITaskItem[0];
				else
					_types = value;
			}
		}

		/// <summary>
		/// Gets or sets a boolean indicating whether to index hidden directories (via <see cref="SymbolRoots"/>)
		/// </summary>
		[DefaultValue(false)]
		public bool IncludeHiddenDirectories
		{
			get { return _includeHiddenDirectories; }
			set { _includeHiddenDirectories = value; }
		}

		/// <summary>
		/// Gets or sets a boolean indicating whether to index directories starting with a dot or underscore (via <see cref="SymbolRoots"/>)
		/// </summary>
		[DefaultValue(false)]
		public bool IncludeDotDirs
		{
			get { return _includeDotDirs; }
			set { _includeDotDirs = value; }
		}

		/// <summary>
		/// Gets or sets a boolean indicating whether to recursively search for pdb files (via <see cref="SymbolRoots"/>)
		/// </summary>
		[DefaultValue(false)]
		public bool NotRecursive
		{
			get { return _notRecursive; }
			set { _notRecursive = value; }
		}

		/// <summary>
		/// Gets or sets the SourceServer SDK directory
		/// </summary>
		public string SourceServerSdkDir
		{
			get { return _sourceServerSdkDir; }
			set { _sourceServerSdkDir = value; }
		}

		/// <summary>
		/// Executes the indexing task
		/// </summary>
		/// <returns>true if the indexing succeeded, othewise false</returns>
		public override bool Execute()
		{
			List<string> providers = new List<string>();

			foreach (ITaskItem item in Providers)
			{
				if (!string.IsNullOrEmpty(item.ItemSpec))
					providers.Add(item.ItemSpec);
			}


			SortedList<string, string> symbolFiles = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			SortedList<string, string> sourceRoots = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);

			foreach (ITaskItem item in SymbolFiles)
			{
				string pdb = Path.GetFullPath(item.ItemSpec);

				symbolFiles[pdb] = pdb;
			}

			foreach (ITaskItem item in SourceRoots)
			{
				string dir = Path.GetFullPath(item.ItemSpec);

				sourceRoots[dir] = dir;
			}

			Dictionary<string, string> searchedPaths = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (ITaskItem item in SymbolRoots)
			{
				RecursiveSearchSymbols(new DirectoryInfo(item.ItemSpec), searchedPaths, symbolFiles);
			}

			SourceServerIndexer indexer = new SourceServerIndexer();

			indexer.Providers = providers;
			indexer.SymbolFiles = new List<string>(symbolFiles.Values);
			indexer.SourceRoots = new List<string>(sourceRoots.Values);

			if (!string.IsNullOrEmpty(SourceServerSdkDir))
				indexer.SourceServerSdkDir = SourceServerSdkDir;

			IndexerResult result = indexer.Exec();

			if (!result.Success)
				return false;

			Log.LogMessage(MessageImportance.High, "SourceServer-annotated {0} symbolfile(s) from {1} sourcefile reference(s) with {2} provider(s)", result.IndexedSymbolFiles, result.IndexedSourceFiles, result.ProvidersUsed);

			return result.Success;
		}

		void RecursiveSearchSymbols(DirectoryInfo dir, Dictionary<string, string> searchedPaths, SortedList<string, string> symbolFiles)
		{
			if (searchedPaths.ContainsKey(dir.FullName))
				return;

			string searchPath = dir.FullName;
			searchedPaths.Add(searchPath, searchPath);

			foreach (FileInfo file in dir.GetFiles("*.pdb"))
			{
				string fullPath = file.FullName;
				symbolFiles[fullPath] = fullPath;
			}

			foreach (DirectoryInfo subDir in dir.GetDirectories())
			{
				if (!IncludeHiddenDirectories && ((subDir.Attributes & FileAttributes.Hidden) != 0))
					continue;

				if (!IncludeDotDirs && ("._".IndexOf(subDir.Name[0]) >= 0))
					continue;

				RecursiveSearchSymbols(subDir, searchedPaths, symbolFiles);
			}
		}
	}
}

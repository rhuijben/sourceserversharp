// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using QQn.SourceServerIndexer.Framework;
using System.Diagnostics;
using QQn.SourceServerIndexer.Providers;
using QQn.SourceServerIndexer.Engine;

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

		IList<string> _providerTypes = new string[]
			{
				typeof(SubversionResolver).FullName,
				typeof(TeamFoundationResolver).FullName
			};

		IList<string> _srcTypes = new string[] { "autodetect" };

		string _toolsPath;
		string _sourceServerSdkDir = ".";
		bool _reindexPreviouslyIndexed;

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
		/// Gets or sets a list of sourcecode providers
		/// </summary>
		/// <remarks>These directories allow to exclude specific directories which are included in the <see cref="SourceRoots"/></remarks>
		public IList<string> Providers
		{
			get { return _providerTypes; }
			set
			{
				if (value != null)
					_providerTypes = value;
				else
					_providerTypes = new string[0];
			}
		}

		/// <summary>
		/// Gets or sets a list of sourcecode directories to index
		/// </summary>
		/// <remarks>If one or more sourceroots are specified, only files in and below these directories are indexed</remarks>
		public IList<string> Types
		{
			get { return _srcTypes; }
			set
			{
				if (value != null)
					_srcTypes = value;
				else
					_srcTypes = new string[0];
			}
		}

		/// <summary>
		/// Gets or sets the directory containing the sourceserver tools
		/// </summary>
		public string ToolsPath
		{
			get { return _toolsPath; }
			set { _toolsPath = value; }
		}

		/// <summary>
		/// Gets or sets the SourceServer SDK directory
		/// </summary>
		public string SourceServerSdkDir
		{
			get { return _sourceServerSdkDir; }
			set
			{
				if (value != null)
					_sourceServerSdkDir = value;
				else
					value = ".";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ReIndexPreviouslyIndexedSymbols
		{
			get { return _reindexPreviouslyIndexed; }
			set { _reindexPreviouslyIndexed = value; }
		}

		string _srcToolPath;
		string _pdbStrPath;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IndexerResult Exec()
		{
			_srcToolPath = Path.GetFullPath(Path.Combine(SourceServerSdkDir, "srctool.exe"));
			_pdbStrPath = Path.GetFullPath(Path.Combine(SourceServerSdkDir, "pdbstr.exe"));

			if (!File.Exists(_srcToolPath))
				throw new FileNotFoundException("SRCTOOL.EXE not found", _srcToolPath);
			else if (!File.Exists(_pdbStrPath))
				throw new FileNotFoundException("PDBSTR.EXE not found", _pdbStrPath);

			IndexerState state = new IndexerState();

			foreach (string pdbFile in SymbolFiles)
			{
				SymbolFile symbolFile = new SymbolFile(pdbFile);

				if (!symbolFile.Exists)
					throw new FileNotFoundException("Symbol file not found", symbolFile.FullName);

				state.SymbolFiles.Add(symbolFile.FullName, symbolFile);
			}

			ReadSourceFilesFromPdbs(state); // Check if there are files to index for this pdb file

			PerformExclusions(state);

			LoadProviders(state);
			ResolveFiles(state);

			WritePdbAnnotations(state);

			return CreateResultData(state);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		void ReadSourceFilesFromPdbs(IndexerState state)
		{
			PdbReader.ReadSourceFilesFromPdbs(state, _srcToolPath, ReIndexPreviouslyIndexedSymbols);
		}

		void PerformExclusions(IndexerState state)
		{
			#region - Apply SourceRoots
			if (SourceRoots.Count > 0)
			{
				List<string> rootList = new List<string>();

				foreach (string root in SourceRoots)
				{
					string nRoot = state.NormalizePath(root);

					if(!nRoot.EndsWith("\\"))
						nRoot += "\\";

					rootList.Add(nRoot);
				}

				string[] roots = rootList.ToArray();
				Array.Sort<string>(roots, StringComparer.InvariantCultureIgnoreCase);

				foreach (SourceFile sf in state.SourceFiles.Values)
				{
					string fileName = sf.FullName;

					int n = Array.BinarySearch<string>(roots, fileName, StringComparer.InvariantCultureIgnoreCase);

					if (n >= 0)
						continue; // Exact match found

					n = ~n;

					if ((n > 0) && (n <= roots.Length))
					{
						if (fileName.StartsWith(roots[n - 1], StringComparison.InvariantCultureIgnoreCase))
							continue; // Root found

						sf.NoSourceAvailable = true;
						continue;
					}
					else
						sf.NoSourceAvailable = true;
				}
			}
			#endregion - Apply SourceRoots
			#region - Apply ExcludeSourceRoots
			if (ExcludeSourceRoots.Count > 0)
			{
				List<string> rootList = new List<string>();

				foreach (string root in ExcludeSourceRoots)
				{
					string nRoot = state.NormalizePath(root);

					if (!nRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
						nRoot += Path.DirectorySeparatorChar;

					rootList.Add(nRoot);
				}

				string[] roots = rootList.ToArray();
				Array.Sort<string>(roots, StringComparer.InvariantCultureIgnoreCase);

				foreach (SourceFile sf in state.SourceFiles.Values)
				{
					string fileName = sf.FullName;

					int n = Array.BinarySearch<string>(roots, fileName, StringComparer.InvariantCultureIgnoreCase);

					if (n >= 0)
						continue; // Exact match found

					n = ~n;

					if ((n > 0) && (n <= roots.Length))
					{
						if (fileName.StartsWith(roots[n - 1], StringComparison.InvariantCultureIgnoreCase))
							sf.NoSourceAvailable = true;
					}
				}
			}
			#endregion


		}

		void LoadProviders(IndexerState state)
		{
			List<SourceResolver> providers = new List<SourceResolver>();
			foreach(string provider in Providers)
			{
				Type providerType;
				try
				{
					providerType = Type.GetType(provider, true, true);
				}
				catch(Exception e)
				{
					throw new SourceIndexException(string.Format("Can't load provider '{0}'", provider), e);
				}

				if (!typeof(SourceResolver).IsAssignableFrom(providerType) || providerType.IsAbstract)
					throw new SourceIndexException(string.Format("Provider '{0}' is not a valid SourceProvider", providerType.FullName));

				try
				{
					providers.Add((SourceResolver)Activator.CreateInstance(providerType, new object[] { state }));
				}
				catch (Exception e)
				{
					throw new SourceIndexException(string.Format("Can't initialize provider '{0}'", providerType.FullName), e);
				}
			}

			bool autodetect = false;

			foreach (string type in Types)
			{
				if (string.Equals(type, "AUTODETECT", StringComparison.InvariantCultureIgnoreCase))
				{
					autodetect = true;
					continue;
				}

				foreach (SourceResolver sp in providers)
				{
					if (string.Equals(type, sp.Name, StringComparison.InvariantCultureIgnoreCase))
					{
						if (!state.Resolvers.Contains(sp) && sp.Available)
							state.Resolvers.Add(sp);
					}
				}
			}

			if (autodetect)
			{
				foreach (SourceResolver sp in providers)
				{
					ISourceProviderDetector detector = sp as ISourceProviderDetector;

					if ((detector != null) && !state.Resolvers.Contains(sp))
					{
						if (sp.Available && detector.CanProvideSources(state))
							state.Resolvers.Add(sp);
					}
				}
			}
		}

		void ResolveFiles(IndexerState state)
		{
			foreach (SourceResolver sp in state.Resolvers)
			{
				sp.ResolveFiles();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		void WritePdbAnnotations(IndexerState state)
		{
			PdbWriter.WritePdbAnnotations(state, _pdbStrPath);
		}

		IndexerResult CreateResultData(IndexerState state)
		{
			int nSources = 0;

			foreach (SourceFile sf in state.SourceFiles.Values)
			{
				if (sf.SourceReference != null)
					nSources++;
			}

			return new IndexerResult(true, state.SymbolFiles.Count, nSources, state.Resolvers.Count);
		}
	}
}

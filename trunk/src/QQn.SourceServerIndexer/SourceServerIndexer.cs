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

			LoadFilesToIndex(state); // Check if there are files to index for this pdb file

			

			return new IndexerResult(true, state.SymbolFiles.Count, state.SourceFiles.Count);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public void LoadFilesToIndex(IndexerState state)
		{
			List<SymbolFile> pdbsToRemove = null;
			foreach (SymbolFile pdb in state.SymbolFiles.Values)
			{
				ProcessStartInfo psi = new ProcessStartInfo(_srcToolPath);

				psi.WorkingDirectory = pdb.File.DirectoryName;

				psi.UseShellExecute = false;
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;

				string output;
				string errors;

				if (!ReIndexPreviouslyIndexedSymbols)
				{
					psi.Arguments = string.Format("-c \"{0}\"", pdb.FullName);

					using (Process p = Process.Start(psi))
					{
						output = p.StandardOutput.ReadToEnd();
						errors = p.StandardError.ReadToEnd();

						p.WaitForExit();
					}

					if (output.Contains("source files are indexed") ||
						errors.Contains("source files are indexed") ||
						output.Contains("source file is indexed") ||
						errors.Contains("source file is indexed"))
					{
						// No need to change annotation; it is already indexed
						if (pdbsToRemove == null)
							pdbsToRemove = new List<SymbolFile>();

						pdbsToRemove.Add(pdb);
						continue;
					}
				}

				psi.Arguments = string.Format("-r \"{0}\"", pdb.FullName);

				using (Process p = Process.Start(psi))
				{
					output = p.StandardOutput.ReadToEnd();
					errors = p.StandardError.ReadToEnd();

					p.WaitForExit();
				}

				if (!string.IsNullOrEmpty(errors))
				{
					throw new SourceIndexToolException("SRCTOOL", errors.Trim());
				}

				bool foundOne = false;
				foreach (string item in output.Split('\r', '\n'))
				{
					string fileName = item.Trim();

					if ((fileName.IndexOf('*') >= 0) || // C++ Compiler internal file
						((fileName.Length > 2) && (fileName.IndexOf(':', 2) >= 0)))
					{
						// Some compiler internal filenames of C++ start with a * 
						// and/or have a :123 suffix

						continue; // Skip never existing files
					}

					if (!string.IsNullOrEmpty(fileName))
					{
						SourceFile file;

						if (!state.SourceFiles.TryGetValue(fileName, out file))
						{
							file = new SourceFile(fileName);
							state.SourceFiles.Add(fileName, file);
						}

						pdb.AddSourceFile(file);
						file.AddContainer(pdb);
						foundOne = true;
					}
				}

				if (!foundOne)
				{
					if (pdbsToRemove == null)
						pdbsToRemove = new List<SymbolFile>();

					pdbsToRemove.Add(pdb);
				}
			}

			if (pdbsToRemove != null)
			{
				foreach (SymbolFile s in pdbsToRemove)
				{
					state.SymbolFiles.Remove(s.FullName);
				}
			}
		}
	}
}

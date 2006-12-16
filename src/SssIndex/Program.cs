// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer;
using QQn.SourceServerIndexer.Framework;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace SssIndex
{
	class Program
	{
		static int Main(string[] args)
		{
			bool quiet = false;
			SourceServerIndexer indexer = LoadIndexer(args, out quiet);

			if (!quiet)
			{
				Console.WriteLine(((AssemblyProductAttribute)typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product);
				Console.WriteLine(((AssemblyCopyrightAttribute)typeof(Program).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright);
			}

			if (indexer == null)
			{
				ShowHelp();
				return 1;
			}

			IndexerResult result = indexer.Exec();

			if (!result.Success)
			{
				Console.Error.WriteLine(result.ErrorMessage);
				return 1;
			}
			else
			{
				if (!quiet)
				{
					Console.WriteLine();
					Console.WriteLine("Added {0} references to {1} symbolfile(s) with {2} provider(s).", result.IndexedSourceFiles, result.IndexedSymbolFiles, result.ProvidersUsed);
				}

				return 0; // C
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine(@"
SssIndex [<arguments>] file1.pdb [file2.pdb...]

  -?, -h[elp]          Shows this helptext
  -q[uiet]             Shows no output if annotating the pdb was successfull
  -i[nclude] <path>    Includes only files within this path (can be used
                       multiple times). Directories and files can be used.
  -x, -exclude <path>  Don't include files in the specified path (can be used
                       multiple times). Directories and files can be used.

  -t[ype]              Use the specified type of indexer. Allowed types are
                       'autodetect' (default), 'Subversion', 'TF'.                       
                       All providers which implement ISourceProviderDetector
                       are used by autodetect.

Please note:
 * The actual defaults are defined in SssIndex.exe.config; those are not
   reflected in this helptext.
 * Selecting multiple pdb files at once is far more efficient than calling 
   SsIndex once for each file.
 * <path> references can contain ?, *, ** and *** glob parameters.
   (match 1 character, 0 or more characters, 1 or more directory levels and
   0 or more directory levels).
");
		}

		private static SourceServerIndexer LoadIndexer(string[] args, out bool quiet)
		{
			SourceServerIndexer indexer = new SourceServerIndexer();

			LoadSettings(indexer);

			quiet = false;
			bool foundTypes = false;
			int i;
			for (i = 0; i < args.Length; i++)
			{
				bool breakOut = false;
				string arg = args[i];
				string param = (i + 1 < args.Length) ? args[i + 1] : null;

				if (arg == "/?")
					return null; // Special case default help mode

				if (arg.Length > 1 && arg[0] == '-')
				{
					switch (arg.Substring(1).ToLowerInvariant())
					{
						case "-":
							breakOut = true;
							break;
						case "?":
						case "h":
						case "-help":
							return null; // We show help
						case "q":
						case "quiet":
							quiet = true;
							break;

						case "i":
						case "inc":
						case "include":
							if (param == null)
								return null; // Show help and exit with errorlevel 1

							if (param.Contains("?") || param.Contains("*"))
							{
								foreach (string item in ExpandGlob(param, true, true))
								{
									indexer.SourceRoots.Add(Path.GetFullPath(item));
								}
							}
							else
								indexer.SourceRoots.Add(Path.GetFullPath(param));

							i++; // Skip next argument
							break;


						case "x":
						case "exc":
						case "exclude":
							if (param == null)
								return null; // Show help and exit with errorlevel 1

							if (param.Contains("?") || param.Contains("*"))
							{
								foreach (string item in ExpandGlob(param, true, true))
								{
									indexer.ExcludeSourceRoots.Add(Path.GetFullPath(item));
								}
							}
							else
								indexer.ExcludeSourceRoots.Add(Path.GetFullPath(param));

							i++; // Skip next argument
							break;

						case "toolspath":
							if (param == null)
								return null; // Show help and exit with errorlevel 1

							indexer.SourceServerSdkDir = param;
							i++;
							break;

						case "type":
							if (param == null)
								return null; // Show help and exit with errorlevel 1

							if (!foundTypes)
							{
								indexer.Types.Clear();
								foundTypes = true;
							}

							indexer.Types.Add(param);
							i++;
							break;
						default:
							Console.Error.WriteLine("Unknown argument '{0}'", arg);
							return null;
					}

					if (breakOut)
						break;
				}
				else
					break;
			}

			if (i >= args.Length - 1)
				return null; // Show help

			for (; i < args.Length; i++)
			{
				string param = args[i];

				if (param.Contains("?") || param.Contains("*"))
				{
					foreach (string item in ExpandGlob(param, false, true))
					{
						indexer.SymbolFiles.Add(Path.GetFullPath(item));
					}
				}
				else
					indexer.SymbolFiles.Add(Path.GetFullPath(param));
			}
			
			return indexer;
		}

		private static void LoadSettings(SourceServerIndexer indexer)
		{
			indexer.Providers.Clear();
			indexer.Types.Clear();

			foreach (string resolver in SssIndexSettings.Default.Resolvers)
				indexer.Providers.Add(resolver);

			foreach (string type in SssIndexSettings.Default.UseResolvers)
				indexer.Types.Add(type);

			string sdkDir = Environment.ExpandEnvironmentVariables(SssIndexSettings.Default.DebuggingSdkDir);

			if (Directory.Exists(sdkDir))
				indexer.SourceServerSdkDir = Path.GetFullPath(sdkDir);
			else
			{
				using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\DebuggingTools"))
				{
					if (rk != null)
					{
						string path = rk.GetValue("WinDbg") as string;

						if (path != null)
						{
							path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);

							if (Directory.Exists(path))
							{
								path = Path.Combine(path, "sdk\\srcsrv");

								if (Directory.Exists(path))
									indexer.SourceServerSdkDir = path;
							}
						}
					}
				}
			}


			string systemPath = System.Environment.GetEnvironmentVariable("PATH");
			bool appendedToPath = false;

			foreach (string path in SssIndexSettings.Default.AppendToPath)
			{
				string expandedPath = Environment.ExpandEnvironmentVariables(path);

				if (!string.IsNullOrEmpty(expandedPath) && Directory.Exists(expandedPath))
				{
					systemPath += Path.PathSeparator + Path.GetFullPath(expandedPath);
					appendedToPath = true;
				}
			}

			if (appendedToPath)
				System.Environment.SetEnvironmentVariable("PATH", systemPath);
		}

		private static IEnumerable<string> ExpandGlob(string param, bool returnDirs, bool returnFiles)
		{
			param = param.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			int nFixed = param.IndexOfAny(new char[] { '*', '?' });

			if (nFixed < 0)
				return new string[] { param };

			int nSep = param.LastIndexOf(Path.DirectorySeparatorChar, nFixed);

			string searchRoot;
			string rest;
			if (nSep > 0)
			{
				searchRoot = Path.GetFullPath(param.Substring(0, nSep));
				rest = param.Substring(nSep + 1);
			}
			else
			{
				searchRoot = Path.GetFullPath(Environment.CurrentDirectory);
				rest = param;
			}

			DirectoryInfo dir = new DirectoryInfo(searchRoot);

			SortedList<string, string> items = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);

			RecursiveFindGlobParts(items, dir, rest.Split(Path.DirectorySeparatorChar), 0, returnDirs, returnFiles);

			return items.Keys;
		}


		private static void RecursiveFindGlobParts(SortedList<string, string> items, DirectoryInfo dir, string[] parts, int index, bool returnDirs, bool returnFiles)
		{
			if (index >= parts.Length)
				return;

			string part = parts[index];

			if (part == ".")
				RecursiveFindGlobParts(items, dir, parts, index + 1, returnDirs, returnFiles);
			else if (part == "..")
				RecursiveFindGlobParts(items, dir.Parent, parts, index + 1, returnDirs, returnFiles);

			bool isLast = (index == parts.Length - 1);
			if (!isLast || returnDirs)
			{
				bool recursive = part.Contains("**");

				if (recursive && (part == "***"))
					RecursiveFindGlobParts(items, dir, parts, index + 1, returnDirs, returnFiles);

				if (recursive)
					part = part.Replace("**", "*");

				SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

				foreach (DirectoryInfo subdir in dir.GetDirectories(part, option))
				{
					if (isLast)
						items[subdir.FullName] = subdir.FullName;

					RecursiveFindGlobParts(items, subdir, parts, index + 1, returnDirs, returnFiles);
				}
			}

			if (isLast && returnFiles)
			{
				foreach (FileInfo fif in dir.GetFiles(part))
				{
					items[fif.FullName] = fif.FullName;
				}
			}
		}
	}
}

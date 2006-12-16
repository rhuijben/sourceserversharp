// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;
using System.IO;
using System.Diagnostics;
using Microsoft.Build.Utilities;
using System.Xml;
using System.Xml.XPath;

namespace QQn.SourceServerIndexer.Providers
{
	/// <summary>
	/// Implements the <see cref="SourceProvider"/> class for subversion (http://subversion.tigris.org/)
	/// </summary>
	public class SubversionResolver : SourceResolver, ISourceProviderDetector
	{
		string _svnExePath;
		/// <summary>
		/// 
		/// </summary>
		public SubversionResolver(IndexerState state)
			: base(state, "Subversion")
		{
			//GC.KeepAlive(null);
		}

		#region ### Availability
		/// <summary>
		/// 
		/// </summary>
		public override bool Available
		{
			get { return (SvnExePath != null); }
		}

		bool _searchedPath;
		/// <summary>
		/// 
		/// </summary>
		protected string SvnExePath
		{
			get
			{
				if(_searchedPath || !string.IsNullOrEmpty(_svnExePath))
					return _svnExePath;

				string path = Environment.GetEnvironmentVariable("PATH");
				if(path == null)
					path = "";
				else
					path = path.ToUpperInvariant();

				string[] pathItems = path.Split(Path.PathSeparator);

				// First try to find some directory with subversion in its name (probably ok for 99% of the cases)
				foreach(string item in pathItems)
				{
					if(item.Contains("SUBVERSION"))
					{
						string file = Path.GetFullPath(Path.Combine(item.Trim(), "SVN.EXE"));

						if(File.Exists(file))
							return _svnExePath = file;							
					}
				}

				// Search whole path
				foreach(string item in pathItems)
				{
					string file = Path.GetFullPath(Path.Combine(item.Trim(), "SVN.EXE"));

					if(File.Exists(file))
						return _svnExePath = file;							
				}

				_searchedPath = true;
				return null;
			}
		}
		

		#region #### ISourceProviderDetector Members

		/// <summary>
		/// SourceIndex detector which tries to find valid providers
		/// </summary>
		/// <param name="state"></param>
		/// <returns>true if one or more files might be managed in subversion, otherwise false</returns>
		public bool CanProvideSources(QQn.SourceServerIndexer.Framework.IndexerState state)
		{
			SortedList<string, string> directories = new SortedList<string,string>(StringComparer.InvariantCultureIgnoreCase);

			foreach(SourceFile file in state.SourceFiles.Values)
			{
				string dir = file.File.DirectoryName;

				if (directories.ContainsKey(dir))
					continue;

				directories.Add(dir, dir);

				if (Directory.Exists(Path.Combine(dir, ".svn")))
					return true;
				else if (Directory.Exists(Path.Combine(dir, "_svn")))
					return true; // Might not work; requires environment variable
			}

			return false;
		}

		#endregion #### ISourceProviderDetector Members
		#endregion ### Availability

		static bool ContainsAt(string hayStack, int index, string needle)
		{
			if (hayStack == null)
				throw new ArgumentNullException("hayStack");
			else if (index < 0)
				throw new ArgumentException("offset out of range", "index");
			else if (string.IsNullOrEmpty(needle))
				throw new ArgumentNullException("needle");

			if (hayStack.Length < index + needle.Length)
				return false;

			return 0 == string.Compare(hayStack, index, needle, 0, needle.Length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool ResolveFiles()
		{
			ProcessStartInfo psi = new ProcessStartInfo(SvnExePath);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			psi.RedirectStandardInput = true;
			CommandLineBuilder cb = new CommandLineBuilder();

			cb.AppendSwitch("--non-interactive");
			//cb.AppendSwitch("--verbose");
			cb.AppendSwitch("--non-recursive");
			cb.AppendSwitch("--xml");

			cb.AppendSwitch("status");

			SortedList<string, SourceFile> files = new SortedList<string, SourceFile>(StringComparer.InvariantCultureIgnoreCase);

			SortedList<string, bool> volumes = new SortedList<string, bool>(StringComparer.InvariantCultureIgnoreCase);

			foreach (KeyValuePair<string, SourceFile> file in State.SourceFiles)
			{
				if (file.Value.IsResolved)
					continue;

				// Check if the volume exists; if not 'svn status' returns "svn: Error resolving case of 'q:\q'"
				string root = Path.GetPathRoot(file.Key);

				bool rootExists;

				if(!volumes.TryGetValue(root, out rootExists))
				{
					rootExists = Directory.Exists(root);
					
					volumes.Add(root, rootExists);
				}
				
				if(!rootExists)
					continue;

				files.Add(file.Key, file.Value);
				cb.AppendFileNameIfNotNull(file.Key);				
			}

			psi.Arguments = cb.ToString();

			using (Process p = Process.Start(psi))
			{
				try
				{
					p.StandardInput.Close();
					p.ErrorDataReceived += new DataReceivedEventHandler(svnStatus_ErrorDataReceived);
					p.OutputDataReceived += new DataReceivedEventHandler(svnStatus_OutputDataReceived);
					_svnStatusOutput = new StringBuilder();
					_receivedError = false;
					p.BeginErrorReadLine();
					p.BeginOutputReadLine();

					p.WaitForExit();
					string output = _svnStatusOutput.ToString();
					_svnStatusOutput = null;
					//string errs = p.StandardError.ReadToEnd();

					if (_receivedError)
					{
						XmlDocument doc = new XmlDocument();
						int nStart = 0;
						int i;
						while (0 <= (i = output.IndexOf("<target", nStart)))
						{
							int nNext = output.IndexOf("<", i + 5);

							if (nNext < 0 ||
								ContainsAt(output, nNext, "<target") ||
								(ContainsAt(output, nNext, "</") && !ContainsAt(output, nNext + 2, "target")))
							{
								int nClose = output.IndexOf(">", i + 5);

								if (nClose >= 0)
								{
									doc.LoadXml(output.Substring(i, nClose - i) + " />");

									string path = State.NormalizePath(doc.DocumentElement.GetAttribute("path"));
									files.Remove(path);
								}
							}

							if (nNext >= 0)
								nStart = nNext;
							else
								break;
						}
					}

					p.WaitForExit();

				}
				catch (Exception e)
				{
					throw new InvalidOperationException(string.Format("Error executing '{0}' with '{1}'", psi.FileName, psi.Arguments), e);
				}
			}

			cb = new CommandLineBuilder();

			cb.AppendSwitch("--non-interactive");
			cb.AppendSwitch("--xml");

			cb.AppendSwitch("info");

			foreach (SourceFile file in files.Values)
			{
				cb.AppendFileNameIfNotNull(file.FullName);
			}

			psi.Arguments = cb.ToString();
			psi.RedirectStandardError = false;

			using (Process p = Process.Start(psi))
			{
				p.StandardInput.Close();

				string output = p.StandardOutput.ReadToEnd().TrimEnd();

				if (!output.EndsWith("</info>"))
					output += "</info>";

				XPathDocument doc = new XPathDocument(new StringReader(output));

				XPathNavigator nav = doc.CreateNavigator();

				try
				{
					foreach (XPathNavigator i in nav.Select("/info/entry[@path and url and repository/root and commit/@revision]"))
					{
						SourceFile file;

						string path = State.NormalizePath(i.GetAttribute("path", ""));

						if (!State.SourceFiles.TryGetValue(path, out file))
							continue;

						if (file.IsResolved)
							continue; // No need to resolve it again

						XPathNavigator urlNav = i.SelectSingleNode("url");
						XPathNavigator repositoryRootNav = i.SelectSingleNode("repository/root");
						XPathNavigator commit = i.SelectSingleNode("commit");

						if (urlNav == null || repositoryRootNav == null || commit == null)
							continue; // Not enough information to provide reference

						string itemPath = urlNav.Value;
						string reposRoot = repositoryRootNav.Value;

						if (!reposRoot.EndsWith("/"))
							reposRoot += '/';

						if (!itemPath.StartsWith(reposRoot, StringComparison.InvariantCultureIgnoreCase))
							continue;
						else
							itemPath = itemPath.Substring(reposRoot.Length);

						string commitRev = commit.GetAttribute("revision", "");
						string wcRev = i.GetAttribute("revision", "");

						file.SourceReference = new SubversionSourceReference(this, file, new Uri(reposRoot), new Uri(itemPath, UriKind.Relative), int.Parse(commitRev), int.Parse(wcRev));
					}
				}
				catch (XmlException e)
				{
					throw new SourceIndexToolException("svn", "Received invalid xml from subversion", e);
				}				

				p.WaitForExit();
			}
			
			return true;
		}

		StringBuilder _svnStatusOutput;
		bool _receivedError;
		void svnStatus_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if(e.Data != null)
				_svnStatusOutput.Append(e.Data);
		}

		void svnStatus_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data))
				_receivedError = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void WriteEnvironment(StreamWriter writer)
		{
			writer.Write(Id);
			writer.WriteLine(@"__TRG=%targ%\%var7%%fnbksl%(%var4%)\%var5%\%fnfile%(%var4%)");
			writer.Write(Id);
			writer.Write("__CMD=svn.exe export \"%var3%%var4%@%var6%\" \"%");
			writer.Write(Id);
			writer.WriteLine("__TRG%\" --non-interactive --quiet");
		}

		/// <summary>
		/// 
		/// </summary>
		public override int SourceEntryVariableCount
		{
			get { return 5; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class SubversionSourceReference : SourceReference
	{
		readonly Uri _reposRoot;
		readonly Uri _itemPath;
		readonly int _commitRev;
		readonly int _wcRev;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resolver"></param>
		/// <param name="sourceFile"></param>
		/// <param name="reposRoot"></param>
		/// <param name="itemPath"></param>
		/// <param name="commitRev"></param>
		/// <param name="wcRev"></param>
		public SubversionSourceReference(SubversionResolver resolver, SourceFile sourceFile, Uri reposRoot, Uri itemPath, int commitRev, int wcRev)
			: base(resolver, sourceFile)
		{
			if (reposRoot == null)
				throw new ArgumentNullException("reposRoot");
			else if (itemPath == null)
				throw new ArgumentNullException("itemPath");

			_reposRoot = reposRoot;
			_itemPath = itemPath;

			_commitRev = commitRev;
			_wcRev = wcRev;
		}

		static string ReposSubDir(Uri reposUri)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("svn-");
			sb.Append(reposUri.Scheme);
			sb.Append("\\");
			if(!string.IsNullOrEmpty(reposUri.Host))
			{
				foreach(char c in reposUri.Host)
					if(char.IsLetterOrDigit(c) || (".-".IndexOf(c) >= 0))
						sb.Append(c);
					else
						sb.Append('_');

				if(reposUri.Port >= 1)
					sb.AppendFormat("_{0}", reposUri.Port);
			}

			if(!string.IsNullOrEmpty(reposUri.AbsolutePath))
			{
				sb.Append(reposUri.AbsolutePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
			}
			
			if(sb[sb.Length-1] != Path.DirectorySeparatorChar)
				sb.Append(Path.DirectorySeparatorChar);

			return sb.ToString();
		}

		/// <summary>
		/// Gets a list of entries for the sourcefiles
		/// </summary>
		/// <returns></returns>
		public override string[] GetSourceEntries()
		{
			return new string[]
			{
				_reposRoot.ToString(),
				_itemPath.ToString(),
				_commitRev.ToString(),
				_wcRev.ToString(),				
				ReposSubDir(_reposRoot)
			};
		}
	}
}
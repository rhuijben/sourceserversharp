// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;
using System.IO;
using QQn.SourceServerIndexer.Providers;
using System.Diagnostics;

namespace QQn.SourceServerIndexer.Engine
{
	static class PdbWriter
	{
		public static void WritePdbAnnotations(IndexerState state, string pdbStrPath)
		{
			foreach (SymbolFile file in state.SymbolFiles.Values)
			{
				bool found;
				string tmpFile = file.FullName + ".srcsvr";
				using (StreamWriter sw = File.CreateText(tmpFile))
				{
					found = WriteAnnotations(state, file, sw);
				}

				bool delete = false;
				if (found)
				{
					ProcessStartInfo psi = new ProcessStartInfo(pdbStrPath);
					psi.Arguments = string.Format("-w -s:srcsrv -p:\"{0}\" -i:\"{1}\"", file.FullName, tmpFile);
					psi.UseShellExecute = false;

					using (Process p = Process.Start(psi))
					{
						p.WaitForExit();

						if (p.ExitCode == 0)
							delete = true;
					}
				}
				else
					delete = true;

				if (delete)
					File.Delete(tmpFile);
			}
		}

		static bool WriteAnnotations(IndexerState state, SymbolFile file, StreamWriter sw)
		{
			SortedList<string, SourceProvider> providers = new SortedList<string, SourceProvider>();
			int itemCount = 1;

			foreach (SourceFile sf in file.SourceFiles.Values)
			{
				if (!sf.IsResolved || sf.NoSourceAvailable)
					continue;

				SourceReference sr = sf.SourceReference;
				SourceProvider provider = sr.SourceProvider;

				if (providers.ContainsKey(provider.Id))
					continue;

				providers.Add(provider.Id, provider);

				if (provider.SourceEntryVariableCount > itemCount)
					itemCount = provider.SourceEntryVariableCount;
			}

			if (providers.Count == 0)
				return false;

			sw.WriteLine("SRCSRV: ini ------------------------------------------------");
			sw.WriteLine("VERSION=1");
			sw.Write("VERCTRL=QQn.SourceServerIndexer");
			foreach (SourceProvider sp in providers.Values)
			{
				if (!string.IsNullOrEmpty(sp.Name))
				{
					sw.Write('+');
					sw.Write(sp.Name);
				}
			}
			sw.WriteLine();
			sw.WriteLine("SRCSRV: variables ------------------------------------------");
			sw.WriteLine("DATETIME=" + DateTime.Now.ToUniversalTime().ToString("u"));
			sw.WriteLine("SRCSRVTRG=%fnvar%(%VAR2%__TRG)");
			sw.WriteLine("SRCSRVCMD=%fnvar%(%VAR2%__CMD)");
			sw.WriteLine("SRCSRVENV=%fnvar%(%VAR2%__ENV)");
			foreach (SourceProvider sp in providers.Values)
			{
				sp.WriteEnvironment(sw);
			}
			sw.WriteLine("SRCSRV: source files ---------------------------------------");

			foreach (SourceFile sf in file.SourceFiles.Values)
			{
				if (!sf.IsResolved || sf.NoSourceAvailable)
					continue;

				sw.Write(sf.FullName);
				sw.Write('*');

				SourceReference sr = sf.SourceReference;
				SourceProvider provider = sr.SourceProvider;

				sw.Write(provider.Id);
				sw.Write('*');

				string[] strings = sr.GetSourceEntries();

				for (int i = 0; i < itemCount; i++)
				{
					if (strings != null && i < strings.Length)
						sw.Write(strings[i]);

					sw.Write('*');
				}

				// Note: We defined the variables upto itemCount+2 (filename, type, itemcount)
				//

				//

				sw.WriteLine();
			}

			sw.WriteLine("SRCSRV: end ------------------------------------------------");

			return true;
		}
	}
}

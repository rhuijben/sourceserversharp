// **************************************************************************
// * $Id: SubversionProvider.cs 6 2006-11-10 14:57:35Z bhuijben $
// * $HeadURL: https://sourceserversharp.googlecode.com/svn/trunk/src/QQn.SourceServerIndexer/Providers/SubversionProvider.cs $
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;

namespace QQn.SourceServerIndexer.Providers
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class SourceReference
	{
		readonly SourceProvider _provider;
		readonly SourceFile _sourceFile;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="sourceFile"></param>
		protected SourceReference(SourceProvider provider, SourceFile sourceFile)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");
			else if (sourceFile == null)
				throw new ArgumentNullException("sourceFile");

			_provider = provider;
			_sourceFile = sourceFile;
		}

		/// <summary>
		/// 
		/// </summary>
		public SourceProvider SourceProvider
		{
			get { return _provider; }
		}
		
		/// <summary>
		/// Gets a list of string entries for the specified sourcefile which are used for 
		/// %VAR3%..$VARx% in the extract script
		/// </summary>
		/// <returns></returns>
		/// <remarks>The number of entries returned MUST be less than or equal to the number returned 
		/// by <see cref="SourceProvider"/>.SourceEntryVariableCount</remarks>
		public abstract string[] GetSourceEntries();
	}
}

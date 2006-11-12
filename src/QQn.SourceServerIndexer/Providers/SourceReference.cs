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
	}
}

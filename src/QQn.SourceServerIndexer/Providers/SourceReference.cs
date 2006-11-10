using System;
using System.Collections.Generic;
using System.Text;
using QQn.SourceServerIndexer.Framework;

namespace QQn.SourceServerIndexer.Providers
{
	public abstract class SourceReference
	{
		readonly SourceProvider _provider;
		readonly SourceFile _sourceFile;

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

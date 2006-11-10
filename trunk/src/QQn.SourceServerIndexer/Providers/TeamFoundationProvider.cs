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
	public class TeamFoundationProvider : SourceProvider
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public TeamFoundationProvider(IndexerState state)
			: base(state)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override string Name
		{
			get { return "TF"; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool Available
		{
			get { return false; }
		}

		public override bool ResolveFiles()
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}

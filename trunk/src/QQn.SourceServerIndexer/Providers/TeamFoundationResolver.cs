// **************************************************************************
// * $Id$
// * $HeadURL$
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
	public class TeamFoundationResolver : SourceResolver
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state"></param>
		public TeamFoundationResolver(IndexerState state)
			: base(state, "TF")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool Available
		{
			get { return false; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override bool ResolveFiles()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public override void WriteEnvironment(System.IO.StreamWriter writer)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
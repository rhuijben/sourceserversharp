// **************************************************************************
// * $Id$
// * $HeadURL$
// **************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;

namespace QQn.SourceServerIndexer.Framework
{
	/// <summary>
	/// 
	/// </summary>
	public class SymbolFile : SourceFileBase, IComparable<SourceFile>, IEquatable<SourceFile>
	{
		readonly SortedList<string, SourceFile> _sourceFiles = new SortedList<string, SourceFile>(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		public SymbolFile(string filename)
			: base(filename)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public IDictionary<string, SourceFile> SourceFiles
		{
			get { return _sourceFiles; }
		}

		internal void AddSourceFile(SourceFile sourceFile)
		{
			if(sourceFile == null)
				throw new ArgumentNullException("sourceFile");

			if (!_sourceFiles.ContainsKey(sourceFile.FullName))
				_sourceFiles.Add(sourceFile.FullName, sourceFile);
		}


			
		#region ## IComparable<SourceFile>, IEquatable<SourceFile>

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(SourceFile other)
		{
			return base.CompareTo(other);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(SourceFile other)
		{
			return base.Equals(other);
		}
		#endregion ## IComparable<SourceFile>, IEquatable<SourceFile>
	}
}

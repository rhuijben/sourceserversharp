The QQn.SourceServerSharp.Tasks assembly contains a Nant Task for running
The SourceServer indexer in your automated builds.

To install the task in NAnt:
 - copy QQn.SourceServerSharp.Tasks.dll and QQn.SourceServerSharp.dll
   to your Nant directory and make sure SRCTOOL and PDBSTR are available
   from your path or one of the standard Debugging Tools for Windows 
   directories. (You can specify an alternate tools-path from your
   NAnt script)

Sample usage:
<SssIndex type="Subversion">
	<Sources>
		<include name="f:\projects" />
	<Sources>
	<Symbols>
		<include name="f:\projects\**\bin\release\*.pdb" />
	</Symbols>
</SssIndex>
	
This will apply SourceServer indexing annotation on all .pdb files in 
bin\release subdirectories below f:\projects; and wil only annotate
sourcefiles which are located below f:\projects.



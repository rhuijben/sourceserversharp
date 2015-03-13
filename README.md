Implementation of a Microsoft SourceServer indexer in .Net. The original perl implementation does not use an efficient algorithm for Subversion based repositories. This implementation tries to abstract the interface for other sourceindexers, while offering optimal performance for subversion indexes. It also provides an MSBuild target for including indexing in the build

The Microsoft SourceServer infrastructure allows to annotate standard PDB files with information about where to retrieve the sourcecode when debugging. This allows debuggers to retrieve the exact sourcecode used when building applications and libraries when they are not locally available

Status:
The reference locator and subversion support are complete. A commandline tool, MSBuild task and NAnt task are available. Next on the TODO list is TeamFoundation server support

This is a backup of the original google code repository. The code has been integrated into the SharpSvn project.

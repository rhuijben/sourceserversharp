The SourceServerSharp project provides a source indexer for the Microsoft debugging
Tools. When you debug a program in one of Microsoft's debuggers this annotation allows
them to fetch the exact source used to build the program.

The SourceServerSharp project provides
= The indexer implementation with a plugin api for aditional source providers
= A MSBuild task for including the task in your project files
= A NAnt task for integrating the support in your existing build infrastructure.
= A commandline task runner. Easy to run by hand or from a home grown script.
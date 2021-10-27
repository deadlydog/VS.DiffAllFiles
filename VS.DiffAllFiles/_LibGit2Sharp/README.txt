native libraries copied from %userprofile%\.nuget\packages\libgit2sharp.nativebinaries\2.0.306\runtimes\[platform]\native

Note the name of the native library changes by version. To include in VISX package add something like the following:
<!-- The native library for LibGit2Sharp. The name varies by version. If updating 
	 PackageReference, you need to update the dll in the folder and the name. -->
<Content Include="..\VS.DiffAllFiles\_LibGit2Sharp\win-x64\git2-106a5f2.dll">
  <Link>win-x64\git2-106a5f2.dll</Link>
  <IncludeInVSIX>true</IncludeInVSIX>
  <VSIXSubPath>\</VSIXSubPath>
</Content>
:: Strongly sign the LibGit2Sharp.dll, as VS Extensions want strongly signed assemblies and we want to avoid runtime version conflicts.
:: http://www.codeproject.com/Tips/341645/Referenced-assembly-does-not-have-a-strong-name
cd "C:\dev\Git\VS.DiffAllFiles\packages\LibGit2Sharp.0.26.0\lib\netstandard2.0"
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.2 Tools\ildasm.exe" /all /out=LibGit2Sharp.il LibGit2Sharp.dll
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.2 Tools\sn.exe" -k MyLibGit2SharpKey.snk
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ilasm.exe" /dll /key=MyLibGit2SharpKey.snk LibGit2Sharp.il

PAUSE
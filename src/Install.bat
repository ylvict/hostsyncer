"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe" hostsyncer.exe
net start hostsyncer
sc config hostsyncer start= auto

copy racaljkhost.exe "%SystemRoot%\system32\"
"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe" "%SystemRoot%\system32\racaljkhost.exe"
net start racaljkhost
sc config racaljkhost start= auto
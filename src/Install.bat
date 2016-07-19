"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe" racaljkhost.exe
net start racaljkhost
sc config racaljkhost start= auto
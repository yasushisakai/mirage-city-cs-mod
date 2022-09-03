taskkill /f /t /im "Cities.exe"
timeout /t 3 /nobreak
psexec \\localhost -i "C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities.exe"
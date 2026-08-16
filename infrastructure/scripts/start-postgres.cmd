@echo off
setlocal
set PGBIN=C:\Program Files\PostgreSQL\18\bin
set PGDATA=C:\Users\SAAD\AppData\Local\PostgreSQL\18\data
set PGLOG=C:\Users\SAAD\AppData\Local\PostgreSQL\18\server.log
"%PGBIN%\pg_ctl.exe" -D "%PGDATA%" -l "%PGLOG%" -w start
exit /b %ERRORLEVEL%
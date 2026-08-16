@echo off
setlocal
set PGBIN=C:\Program Files\PostgreSQL\18\bin
set PGDATA=C:\Users\SAAD\AppData\Local\PostgreSQL\18\data
"%PGBIN%\pg_ctl.exe" -D "%PGDATA%" -m fast stop
exit /b %ERRORLEVEL%
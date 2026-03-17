@echo off
start "Grind Atlas — Backend" cmd /k "cd /d C:\Dev\ClaudeCode\grind-atlas\backend\GrindAtlas.API && dotnet run"
start "Grind Atlas — Frontend" cmd /k "cd /d C:\Dev\ClaudeCode\grind-atlas\frontend\grind-atlas-ui && npm start"

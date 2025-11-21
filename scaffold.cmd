@echo off
REM Super simple scaffold: lee la cadena DefaultConnection de Api/appsettings.json
REM Uso:
REM   scaffold.cmd            (lee del appsettings.json)
REM   scaffold.cmd "cadena"   (usa la cadena pasada)

set "INFRA_PROJ=Infrastructure/Microservicio.Infrastructure.csproj"
set "CONTEXT_NAME=DocumentBuilderContext"
set "OUTPUT_DIR=Models"
set "CONTEXT_DIR=Persistence/Context"
set "CONN_NAME=DocumentBuilder"

REM Si el usuario pasó una cadena, la usamos y saltamos la lectura
IF NOT "%~1"=="" (
  set "CONN=%~1"
) ELSE (
  IF NOT EXIST "Api/appsettings.json" (
    echo [ERROR] No existe Api/appsettings.json
    exit /b 1
  )
  for /f "usebackq delims=" %%C in (`
    powershell -NoProfile -Command "(Get-Content -Raw 'Api/appsettings.json' | ConvertFrom-Json).ConnectionStrings.%CONN_NAME%"
  `) do set "CONN=%%C"
)

IF "%CONN%"=="" (
  echo [ERROR] No se pudo obtener la cadena de conexion.
  exit /b 1
)

echo Usando conexion: %CONN%
echo.

dotnet ef dbcontext scaffold "%CONN%" Microsoft.EntityFrameworkCore.SqlServer ^
  --project "%INFRA_PROJ%" ^
  --output-dir "%OUTPUT_DIR%" ^
  --context-dir "%CONTEXT_DIR%" ^
  --context %CONTEXT_NAME% ^
  --data-annotations --force

IF ERRORLEVEL 1 (
  echo.
  echo [ERROR] Scaffold fallido.
  exit /b 1
)

echo.
echo [OK] Scaffold listo.
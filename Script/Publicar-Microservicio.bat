@echo off
setlocal enabledelayedexpansion

:: ===============================
:: Variables por ambiente
:: ===============================
:: DEV
set DEV_IP=
set DEV_USER=deploy
set DEV_PASS=Abc.123

:: QA
set QA_IP=
set QA_USER=deploy
set QA_PASS=Abc.123

:: UAT
set UAT_IP=
set UAT_USER=deploy
set UAT_PASS=Abc.123

:: ===============================
:: Menu de selección de ambiente
:: ===============================
:MENU
cls
echo ================================
echo  Selecciona el ambiente FTP
echo ================================
echo 1. DEV - %DEV_IP%
echo 2. QA - %QA_IP%
echo 3. UAT - %UAT_IP%
echo 4. Salir
echo ================================
set /p option=Ingresa tu opcion (1-4): 

if "%option%"=="1" set FTP_IP=%DEV_IP% & set FTP_USER=%DEV_USER% & set FTP_PASS=%DEV_PASS% & set Enviroment=DEV& goto VALIDATE
if "%option%"=="2" set FTP_IP=%QA_IP% & set FTP_USER=%QA_USER% & set FTP_PASS=%QA_PASS% & set Enviroment=QA& goto VALIDATE
if "%option%"=="3" set FTP_IP=%UAT_IP% & set FTP_USER=%UAT_USER% & set FTP_PASS=%UAT_PASS% & set Enviroment=UAT& goto VALIDATE
if "%option%"=="4" exit
echo Opcion invalida. Presiona una tecla para intentar de nuevo...
pause >nul
goto MENU

:VALIDATE
:: ===============================
:: Validar que la IP exista
:: ===============================
if "%FTP_IP%"==" " (
    echo El ambiente seleccionado no tiene una IP definida. Saliendo...
    pause
    exit /b
)

echo Seleccionaste: %Enviroment% con IP: '%FTP_IP%'
::pause

:: ===============================
:: Compilación y publicación
:: ===============================
ECHO "Se mueve a directorio deploy"
cd "C:\Deploys"

ECHO "Restaurando paquetes"
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" -t:restore %userprofile%\Documents\ArquetipoMicroServicio\ArquetipoMicroServicio.sln /p:Configuration=Debug

:: Se eliminan los archivos de la carpeta de las publicaciones
del /F /Q /S C:\Deploys\ 

ECHO "Construyendo aplicación"
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" %userprofile%\Documents\ArquetipoMicroServicio\ArquetipoMicroServicio.sln /p:Configuration=Debug

ECHO "Publicando API de Microservicio"
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" %userprofile%\Documents\ArquetipoMicroServicio\API\Microservicio.API.csproj /p:TransformWebConfigEnabled=True /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Test

:: ===============================
:: Preparar carpeta con timestamp
:: ===============================
FOR /F "TOKENS=1 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET dd=%%A
FOR /F "TOKENS=1,2 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET mm=%%B
FOR /F "TOKENS=1,2,3 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET yyyy=%%C

SET timeHHmmsszz=%time%
SET timeHHmmsszz=%timeHHmmsszz::=%
SET timeHHmmsszz=%timeHHmmsszz:.=%
SET timeHHmmsszz=%timeHHmmsszz:  =%
SET timeHHmmsszz=%timeHHmmsszz: =%

SET FolderName=Microservicio_Publish_%Enviroment%_%dd%%mm%%yyyy%_%timeHHmmsszz%
ECHO Foldername: %FolderName%

:: Crear carpeta y copiar API
ECHO Se copia Microservicio.API ...
mkdir "C:\Deploys\%FolderName%"
robocopy "C:\Deploys\Microservicio.API" "C:\Deploys\%FolderName%\Microservicio.API" /E /xf appsettings.json Web.config

:: ===============================
:: Crear ZIP
:: ===============================
ECHO zipping file ...
"C:\Program Files\7-Zip\7z.exe" a -tzip "C:\Deploys\%FolderName%.zip" "C:\Deploys\%FolderName%\*"

:: ===============================
:: Envio por FTP
:: ===============================
ECHO Se comienza con el envio del archivo publicado a FTP %Enviroment% (%FTP_IP%)
ECHO Se genero correctamente el archivo %FolderName%.zip

ECHO open %FTP_IP% 21 > FTP_IN.txt
ECHO user %FTP_USER% %FTP_PASS% >> FTP_IN.txt
ECHO bin >> FTP_IN.txt
ECHO prompt >> FTP_IN.txt
ECHO put C:\Deploys\%FolderName%.zip >> FTP_IN.txt
ECHO bye >> FTP_IN.txt

ftp -n -s:FTP_IN.txt >> FTP_OUT.txt

:: ===============================
:: Limpieza
:: ===============================
IF EXIST FTP_IN.txt DEL /F FTP_IN.txt
IF EXIST FTP_OUT.txt DEL /F FTP_OUT.txt
RMDIR /Q/S "C:\Deploys\%FolderName%"

pause

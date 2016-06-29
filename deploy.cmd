@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

:: ----------------------
:: KUDU Deployment Script - Customized by damianedwards
:: Version: 1.0.6
:: ----------------------

:: Prerequisites
:: -------------

:: Verify node.js installed
where node 2>nul >nul
IF %ERRORLEVEL% NEQ 0 (
  echo Missing node.js executable, please install node.js, if already installed make sure it can be reached from current environment.
  goto error
)

:: Setup
:: -----

setlocal enabledelayedexpansion

SET ARTIFACTS=%~dp0%..\artifacts

IF NOT DEFINED DEPLOYMENT_SOURCE (
  SET DEPLOYMENT_SOURCE=%~dp0%.
)

IF NOT DEFINED DEPLOYMENT_TARGET (
  SET DEPLOYMENT_TARGET=%ARTIFACTS%\wwwroot
)

IF NOT DEFINED NEXT_MANIFEST_PATH (
  SET NEXT_MANIFEST_PATH=%ARTIFACTS%\manifest

  IF NOT DEFINED PREVIOUS_MANIFEST_PATH (
    SET PREVIOUS_MANIFEST_PATH=%ARTIFACTS%\manifest
  )
)

IF NOT DEFINED KUDU_SYNC_CMD (
  :: Install kudu sync
  echo Installing Kudu Sync
  call npm install kudusync -g --silent
  IF !ERRORLEVEL! NEQ 0 goto error

  :: Locally just running "kuduSync" would also work
  SET KUDU_SYNC_CMD=%appdata%\npm\kuduSync.cmd
)
IF NOT DEFINED DEPLOYMENT_TEMP (
  SET DEPLOYMENT_TEMP=%temp%\___deployTemp%random%
  SET CLEAN_LOCAL_DEPLOYMENT_TEMP=true
)

IF DEFINED CLEAN_LOCAL_DEPLOYMENT_TEMP (
  IF EXIST "%DEPLOYMENT_TEMP%" rd /s /q "%DEPLOYMENT_TEMP%"
  mkdir "%DEPLOYMENT_TEMP%"
)

IF NOT DEFINED REPO_TEMP (
  SET REPO_TEMP=%temp%\___repoTemp%random%
  IF EXIST "%REPO_TEMP%" rd /s /q "%REPO_TEMP%"
  mkdir "%REPO_TEMP%"
)

IF DEFINED MSBUILD_PATH goto MsbuildPathDefined
SET MSBUILD_PATH=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe
:MsbuildPathDefined
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Deployment
:: ----------

:: 0. Clone repo to temp (fast) drive

echo Handling ASP.NET Core Web Application deployment.

echo Cloning repo (just the deployment commit) to temp location

cd %REPO_TEMP%
git clone -o %SCM_COMMIT_ID% https://github.com/aspnet/live.asp.net --depth 1 
cd live.asp.net

SET DEPLOYMENT_SOURCE=%REPO_TEMP%

SET USE_MSBUILD=TRUE

IF DEFINED WEBSITE_SITE_NAME (
  :: Make the NuGet packages go in a persisted folder instead of %USERPROFILE%, which on Azure goes in Temp space
  set NUGET_PACKAGES=%HOME%\.nuget
)
:: 1. Restore nuget packages
call :ExecuteCmd nuget.exe restore -packagesavemode nuspec
IF !ERRORLEVEL! NEQ 0 goto error

:: 2. Build and publish
IF DEFINED USE_MSBUILD (
  call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\live.asp.net.sln" /nologo /verbosity:m /p:deployOnBuild=True;AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release;UseSharedCompilation=false;publishUrl="%DEPLOYMENT_TEMP%" %SCM_BUILD_ARGS%
  IF !ERRORLEVEL! NEQ 0 goto error
) ELSE (
  call :ExecuteCmd dotnet publish "D:\home\site\repository\src\live.asp.net" --output "%DEPLOYMENT_TEMP%" --configuration Release
  IF !ERRORLEVEL! NEQ 0 goto error
)

:: 3. KuduSync
call :ExecuteCmd "%KUDU_SYNC_CMD%" -v 50 -f "%DEPLOYMENT_TEMP%" -t "%DEPLOYMENT_TARGET%" -n "%NEXT_MANIFEST_PATH%" -p "%PREVIOUS_MANIFEST_PATH%" -i ".git;.hg;.deployment;deploy.cmd"
IF !ERRORLEVEL! NEQ 0 goto error

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
goto end

:: Execute command routine that will echo out when error
:ExecuteCmd
setlocal
set _CMD_=%*
call %_CMD_%
if "%ERRORLEVEL%" NEQ "0" echo Failed exitCode=%ERRORLEVEL%, command=%_CMD_%
exit /b %ERRORLEVEL%

:error
endlocal
echo An error has occurred during web site deployment.
call :exitSetErrorLevel
call :exitFromFunction 2>nul

:exitSetErrorLevel
exit /b 1

:exitFromFunction
()

:end
endlocal
echo Finished successfully.

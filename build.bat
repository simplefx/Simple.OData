@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)
 
set version=1.0.0
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)

set nuget=
if "%nuget%" == "" (
set nuget=nuget
)

REM NuGet Package Restore

REM Compilation

call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Core\Simple.OData.Client.Core.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Dynamic\Simple.OData.Client.Dynamic.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Net40\Simple.OData.Client.Net40.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.SL5\Simple.OData.Client.SL5.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Tests.Core\Simple.OData.Client.Tests.Core.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Tests.Net40\Simple.OData.Client.Tests.Net40.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Tests.Net45\Simple.OData.Client.Tests.Net45.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Simple.OData.Client.Tests.SL5\Simple.OData.Client.Tests.SL5.csproj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false

REM Test Execution

call "%GallioEcho%" Simple.OData.Client.Tests.Core\bin\%config%\Simple.OData.Client.Tests.Core.dll
call "%GallioEcho%" Simple.OData.Client.Tests.Net40\bin\%config%\Simple.OData.Client.Tests.Net40.dll
call "%GallioEcho%" Simple.OData.Client.Tests.Net45\bin\%config%\Simple.OData.Client.Tests.Net45.dll
call "%GallioEcho%" Simple.OData.Client.Tests.SL5\bin\%config%\Simple.OData.Client.Tests.SL5.dll

REM NuGet Package Creation

mkdir Build
call %nuget% pack "Simple.OData.Client.nuspec" -NoPackageAnalysis -verbosity detailed -o Build -Version %version% -p Configuration="%config%"

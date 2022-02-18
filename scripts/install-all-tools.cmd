set CODEGENVERSION=7.0.0-dev
set MSIDENTITYVERSION=2.0.0-dev
set DEFAULT_NUPKG_PATH=%userprofile%\.nuget\packages
set SRC_DIR=%cd%
set NUPKG=artifacts/packages/Debug/Shipping/
call taskkill /f /im dotnet.exe
call rd /Q /S artifacts
call build
call dotnet tool uninstall -g dotnet-aspnet-codegenerator 
call dotnet tool uninstall -g Microsoft.dotnet-msidentity

call cd %DEFAULT_NUPKG_PATH%
call C:
call rd /Q /S microsoft.visualstudio.web.codegeneration
call rd /Q /S microsoft.dotnet.scaffolding.shared
call rd /Q /S microsoft.dotnet-msidentity
call rd /Q /S microsoft.dotnet.msidentity
call rd /Q /S microsoft.visualstudio.web.codegeneration.core
call rd /Q /S microsoft.visualstudio.web.codegeneration.design
call rd /Q /S microsoft.visualstudio.web.codegeneration.entityframeworkcore
call rd /Q /S microsoft.visualstudio.web.codegeneration.templating
call rd /Q /S microsoft.visualstudio.web.codegeneration.utils
call rd /Q /S microsoft.visualstudio.web.codegenerators.mvc
call D:
call cd  %SRC_DIR%/%NUPKG% 
call dotnet tool install -g dotnet-aspnet-codegenerator --add-source %SRC_DIR%\%NUPKG% --version %CODEGENVERSION%
call dotnet tool install -g Microsoft.dotnet-msidentity --add-source %SRC_DIR%\%NUPKG% --version %MSIDENTITYVERSION%
call cd %SRC_DIR%
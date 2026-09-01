set "gamepath=%programfiles(x86)%\Steam\steamapps\common\Brutal Orchestra"

::===================================================================
set "modpath=%gamepath%\BrutalOrchestra_Data\StreamingAssets\mods\%modfolder%"
set "dlldest=%modpath%\%dllfolder%"
echo Mod folder: %modpath%

::FOLDERS
if not exist "%modpath%" (
    echo Creating mod folder...
    md "%modpath%"
)
if not exist "%dlldest%" (
    echo Creating %dllfolder% folder...
    md "%dlldest%"
)

::DLL
set "dllname=%assemblyname%.dll"
echo Copying DLL...
copy "%outdir%%dllname%" "%dlldest%\%dllname%"

::MODINFO
if exist "%modinfofolder%" (
    if exist "%modinfofolder%\modinfo.config" (
        echo Copying modinfo.config...
        copy "%modinfofolder%\modinfo.config" "%modpath%\modinfo.config"
    )
    if exist "%modinfofolder%\thumbnail.png" (
        echo Copying thumbnail...
        copy "%modinfofolder%\thumbnail.png" "%modpath%\thumbnail.png"
    )
)
gamepath="/home/$USER/.steam/steam/steamapps/common/Brutal Orchestra"

#===================================================================
modpath="$gamepath/BrutalOrchestra_Data/StreamingAssets/mods/$MODFOLDER"
dlldest="$modpath/$DLLFOLDER"
echo Mod folder: $modpath

#FOLDERS
if [ ! -d "$modpath" ]; then
  echo Creating mod folder...
  mkdir "$modpath"
fi
if [ ! -d "$dlldest" ]; then
  echo Creating $DLLFOLDER folder...
  mkdir "$dlldest"
fi

#DLL
dllname="$ASSEMBLYNAME.dll"
echo Copying DLL...
cp "$OUTDIR$dllname" "$dlldest/$dllname"

#MODINFO
if [ -d "$MODINFOFOLDER" ]; then
  if [ -f "$MODINFOFOLDER/modinfo.config" ]; then
    echo Copying modinfo.config...
    cp "$MODINFOFOLDER/modinfo.config" "$modpath/modinfo.config"
  fi
  if [ -f "$MODINFOFOLDER/thumbnail.png" ]; then
    echo Copying thumbnail...
    cp "$MODINFOFOLDER/thumbnail.png" "$modpath/thumbnail.png"
  fi
fi
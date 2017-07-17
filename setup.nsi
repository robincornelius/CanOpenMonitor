; example1.nsi
;
; This script is perhaps one of the simplest NSIs you can make. All of the
; optional settings are left to their default settings. The installer simply 
; prompts the user asking them where to install, and drops a copy of example1.nsi
; there. 

;--------------------------------


!include "MUI2.nsh"
!include "x64.nsh"                  ; Macros for x64 machines

; The name of the installer
Name "CanMonitor"

; The file to write
OutFile "canmonitor-Setup.exe"

; Show install details
ShowInstDetails show

; The default installation directory
InstallDir "$PROGRAMFILES\CanMonitor\"

; Request application privileges for Windows Vista
;RequestExecutionLevel Admin

SetOverwrite on


!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "${NSISDIR}\Contrib\Graphics\Header\nsis.bmp" ; optional
!define MUI_ABORTWARNING
  
;--------------------------------

; Pages


!insertmacro MUI_PAGE_LICENSE "License-GPLv3.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES



;Page instfiles

; The stuff to install
Section "CanMonitor" Secopeneds ;No components page, name is not important

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File canmonitor\bin\Release\*.exe
  File canmonitor\bin\Release\*.dll
  
  SetOutPath $INSTDIR\Plugins
  File canmonitor\bin\Release\plugins\*.dll

  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\CanMonitor"
  CreateShortCut "$SMPROGRAMS\CanMonitor\CanMonitor.lnk" $INSTDIR\CanMonitor.exe "" $INSTDIR\Index_8287_16x.ico 0
     
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  CreateShortCut "$SMPROGRAMS\CanMonitor\Uninstall.lnk" $INSTDIR\Uninstall.exe
  
SectionEnd ; end the section

;Language strings
LangString DESC_Secopeneds ${LANG_ENGLISH} "CanMonitor"

 
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${Secopeneds} $(DESC_Secopeneds)
!insertmacro MUI_FUNCTION_DESCRIPTION_END


Function .onInit

  ;Extract InstallOptions files
  ;$PLUGINSDIR will automatically be removed when the installer closes
  
  InitPluginsDir
  
  Push $0
  Pop $0
  
FunctionEnd


Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...
  
  Delete "$INSTDIR\*"
  Delete "$INSTDIR\Plugins\*"
  
  SetShellVarContext all

  Delete "$SMPROGRAMS\CanMonitor\CanMonitor.lnk" 
  RMDir "$SMPROGRAMS\CanMonitor"

SectionEnd



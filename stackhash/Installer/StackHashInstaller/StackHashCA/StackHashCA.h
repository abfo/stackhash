// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the STACKHASHCA_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// STACKHASHCA_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef STACKHASHCA_EXPORTS
#define STACKHASHCA_API __declspec(dllexport)
#else
#define STACKHASHCA_API __declspec(dllimport)
#endif

STACKHASHCA_API UINT __stdcall AddServiceSettingsFolder(MSIHANDLE hInstaller);
STACKHASHCA_API UINT __stdcall AddUrlacl(MSIHANDLE hInstaller);
STACKHASHCA_API UINT __stdcall RemoveUrlacl(MSIHANDLE hInstaller);
STACKHASHCA_API UINT __stdcall ConfigureSymbolServerProxyMode(MSIHANDLE hInstaller);
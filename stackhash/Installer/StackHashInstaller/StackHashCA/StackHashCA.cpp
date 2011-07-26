// StackHashCA.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "StackHashCA.h"


#define STACKHASH_SETTINGSFOLDER		L"StackHash"
#define STACKHASH_URLACLPREFIX			L"http://+:8000/StackHash"
#define STACKHASH_URLACLSDDL			L"D:(A;;GX;;;NS)"
#define SYMBOLSERVER_KEY				L"SOFTWARE\\Microsoft\\Symbol Server"
#define SYMBOLSERVER_VALUE				L"NoInternetProxy"


// Add a message to the MSI log - best effort only, failure to log is ignored
void LogMessage(MSIHANDLE hInstaller, LPCWSTR szMessage)
{
	if ((NULL == hInstaller) || (NULL == szMessage))
	{
		return;
	}

	PMSIHANDLE hRecord = MsiCreateRecord(1);
	if (NULL != hRecord)
	{
		if (ERROR_SUCCESS == MsiRecordSetString(hRecord, 0, szMessage))
		{
			MsiProcessMessage(hInstaller, INSTALLMESSAGE(INSTALLMESSAGE_INFO), hRecord);
		}
	}
}

void SetNoInternetProxy(MSIHANDLE hInstaller, REGSAM samDesired)
{
	HKEY symbolServerKey;
	if (ERROR_SUCCESS == RegCreateKeyEx(HKEY_LOCAL_MACHINE,
		SYMBOLSERVER_KEY,
		0,
		NULL,
		REG_OPTION_NON_VOLATILE,
		samDesired,
		NULL,
		&symbolServerKey,
		NULL))
	{
		DWORD value = 1;

		if (ERROR_SUCCESS != RegSetValueEx(symbolServerKey,
			SYMBOLSERVER_VALUE,
			0,
			REG_DWORD,
			(BYTE*)&value,
			sizeof(DWORD)))
		{
			LogMessage(hInstaller, L"StackHashCA: ConfigureSymbolServerProxyMode: Failed to set NoInternetProxy value");
		}

		// close the symbol server key
		RegCloseKey(symbolServerKey);
	}
	else
	{
		LogMessage(hInstaller, L"StackHashCA: ConfigureSymbolServerProxyMode: Failed to open or create Symbol Server Key");
	}
}

// Set symbol server to not use a proxy if a WinHttp proxy is not configured
STACKHASHCA_API UINT __stdcall ConfigureSymbolServerProxyMode(MSIHANDLE hInstaller)
{
	UINT currentRet = ERROR_SUCCESS;

	BOOL setNoInternetProxy = FALSE;

	// if no WinHTTP proxy is configured then we need to instruct symbol
	// server to not use a proxy so that cdb works in a service...
	WINHTTP_PROXY_INFO defaultProxyInfo = {0};
	if (TRUE == WinHttpGetDefaultProxyConfiguration(&defaultProxyInfo))
	{
		if (WINHTTP_ACCESS_TYPE_NO_PROXY == defaultProxyInfo.dwAccessType)
		{
			setNoInternetProxy = TRUE;
		}

		// need to free strings
		if (NULL != defaultProxyInfo.lpszProxy)
		{
			GlobalFree(defaultProxyInfo.lpszProxy);
		}
		if (NULL != defaultProxyInfo.lpszProxyBypass)
		{
			GlobalFree(defaultProxyInfo.lpszProxyBypass);
		}
	}

	if (TRUE == setNoInternetProxy)
	{
		// set the key for 32-bit applications
		SetNoInternetProxy(hInstaller, KEY_WRITE | KEY_WOW64_32KEY);

		// set the key for 64-bit applications (will just set the key again on 32-bit systems)
		SetNoInternetProxy(hInstaller, KEY_WRITE | KEY_WOW64_64KEY);
	}

	return currentRet;
}

// Delete and optionally add a URLACL reservation
UINT DeleteAddUrlacl(MSIHANDLE hInstaller, LPCWSTR prefix, LPCWSTR sddl, BOOL removeOnly)
{
	UINT currentRet = ERROR_SUCCESS;

	HTTP_SERVICE_CONFIG_URLACL_SET configInfo = {0};
	configInfo.KeyDesc.pUrlPrefix = (PWSTR)prefix;
	configInfo.ParamDesc.pStringSecurityDescriptor = (PWSTR)sddl;

	HTTPAPI_VERSION apiVersion = HTTPAPI_VERSION_1;


	if (SUCCEEDED(HttpInitialize(apiVersion, HTTP_INITIALIZE_CONFIG, NULL)))
	{
		// try to delete the current configuration
		if (NO_ERROR != HttpDeleteServiceConfiguration(0, 
			HttpServiceConfigUrlAclInfo, 
			&configInfo, 
			sizeof(HTTP_SERVICE_CONFIG_URLACL_SET), 
			NULL))
		{
			// if we're only removing then we care about failure
			if (removeOnly)
			{
				LogMessage(hInstaller, L"StackHashCA: DeleteAddUrlacl: Failed to delete configuration");
				currentRet = ERROR_INSTALL_FAILURE;
			}
		}

		if (!removeOnly)
		{
			if (NO_ERROR != HttpSetServiceConfiguration(0,
				HttpServiceConfigUrlAclInfo,
				&configInfo,
				sizeof(HTTP_SERVICE_CONFIG_URLACL_SET),
				NULL))
			{
				LogMessage(hInstaller, L"StackHashCA: DeleteAddUrlacl: Failed to set configuration");
				currentRet = ERROR_INSTALL_FAILURE;
			}
		}

		// close the HTTP API
		HttpTerminate(HTTP_INITIALIZE_CONFIG, NULL);
	}
	else
	{
		LogMessage(hInstaller, L"StackHashCA: DeleteAddUrlacl: Failed to initialize HTTP API");
		currentRet = ERROR_INSTALL_FAILURE;
	}

	return currentRet;
}

STACKHASHCA_API UINT __stdcall AddServiceSettingsFolder(MSIHANDLE hInstaller)
{
	UINT currentRet = ERROR_SUCCESS;

	LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder");

	TCHAR szSettingsFolder[MAX_PATH];
	PSECURITY_DESCRIPTOR psdFolder = NULL;
	PACL pDaclFolder = NULL;
	PACL pDaclFolderNew = NULL;
	PSID pSidNetworkService = NULL;
	DWORD dwSidSize = SECURITY_MAX_SID_SIZE;
	TRUSTEE trusteeNS = {0};
	ACCESS_MASK accessMask = 0;
	BOOL hasAccessAlready = FALSE;
	EXPLICIT_ACCESS ea = {0};

	// get (or try to create) the common application data folder
	if (!SUCCEEDED(SHGetFolderPath(NULL, 
		CSIDL_COMMON_APPDATA | CSIDL_FLAG_CREATE, 
		NULL, 
		SHGFP_TYPE_CURRENT, 
		szSettingsFolder)))
	{
		LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to find or create CSIDL_COMMON_APPDATA");
		return ERROR_INSTALL_FAILURE;
	}

	// add the StackHash setting subfolder
	if (!PathAppend(szSettingsFolder, STACKHASH_SETTINGSFOLDER))
	{
		LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to append settings folder to CSIDL_COMMON_APPDATA");
		return ERROR_INSTALL_FAILURE;
	}

	// create the settings folder if it doesn't already exist
	if (!PathFileExists(szSettingsFolder))
	{
		if (!CreateDirectory(szSettingsFolder, NULL))
		{
			LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to create settings folder");
			return ERROR_INSTALL_FAILURE;
		}
	}

	// get the DACL for the folder - from here on we need to tidy up regardless of success of failure
	if (!SUCCEEDED(GetNamedSecurityInfo(szSettingsFolder,
		SE_FILE_OBJECT,
		DACL_SECURITY_INFORMATION,
		NULL,
		NULL,
		&pDaclFolder,
		NULL,
		&psdFolder)))
	{
		LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to get DACL for settings folder");
		currentRet = ERROR_INSTALL_FAILURE;
	}
	
	// allocte memory for a SID
	if (ERROR_SUCCESS == currentRet)
	{
		pSidNetworkService = LocalAlloc(LMEM_FIXED, dwSidSize);
		if (NULL == pSidNetworkService)
		{
			LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to allocate memory for SID");
			currentRet = ERROR_INSTALL_FAILURE;
		}
	}

	// create the NETWORK SERVICE SID
	if (ERROR_SUCCESS == currentRet)
	{
		if (CreateWellKnownSid(WinNetworkServiceSid, NULL, pSidNetworkService, &dwSidSize))
		{
			// intialize trustee from the NETWORK SERVICE SID
			trusteeNS.pMultipleTrustee = NULL;
			trusteeNS.MultipleTrusteeOperation = NO_MULTIPLE_TRUSTEE;
			trusteeNS.TrusteeForm = TRUSTEE_IS_SID;
			trusteeNS.TrusteeType = TRUSTEE_IS_GROUP;
			trusteeNS.ptstrName = (LPTSTR)pSidNetworkService;
		}
		else
		{
			LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to create NETWORK SERVICE SID");
			currentRet = ERROR_INSTALL_FAILURE;
		}
	}

	// determine if NETWORK SERVICE has access to the folder
	if (ERROR_SUCCESS == currentRet)
	{
		if (SUCCEEDED(GetEffectiveRightsFromAcl(pDaclFolder, &trusteeNS, &accessMask)))
		{
			if (GENERIC_ALL == (GENERIC_ALL & accessMask))
			{
				hasAccessAlready = TRUE;
			}
		}
		else
		{
			LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to get access rights for settings folder");
			currentRet = ERROR_INSTALL_FAILURE;
		}
	}

	// if the right access isn't present we need to add it
	if ((!hasAccessAlready) && (ERROR_SUCCESS == currentRet))
	{
		// grant GENERIC_ALL rights to NETWORK SERVICE
		ea.grfAccessMode = GRANT_ACCESS;
		ea.grfAccessPermissions = GENERIC_ALL;
		ea.grfInheritance = SUB_CONTAINERS_AND_OBJECTS_INHERIT;
		ea.Trustee = trusteeNS;

		// merge the new rights with the current DACL
		if (ERROR_SUCCESS == currentRet)
		{
			if (!SUCCEEDED(SetEntriesInAcl(1, &ea, pDaclFolder, &pDaclFolderNew)))
			{
				LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to merge ACLs for settings folder");
				currentRet = ERROR_INSTALL_FAILURE;
			}
		}

		// update the folder DACL
		if (ERROR_SUCCESS == currentRet)
		{
			if (!SUCCEEDED(SetNamedSecurityInfo(szSettingsFolder,
				SE_FILE_OBJECT,
				DACL_SECURITY_INFORMATION,
				NULL,
				NULL,
				pDaclFolderNew,
				NULL)))
			{
				LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Failed to update DACL for settings folder");
				currentRet = ERROR_INSTALL_FAILURE;
			}
		}
	}

	// tidy up
	if (NULL != psdFolder)
	{
		LocalFree((HLOCAL)psdFolder);
	}

	if (NULL != pSidNetworkService)
	{
		LocalFree((HLOCAL)pSidNetworkService);
	}

	if (NULL != pDaclFolderNew)
	{
		LocalFree((HLOCAL)pDaclFolderNew);
	}

	if (ERROR_SUCCESS == currentRet)
	{
		LogMessage(hInstaller, L"StackHashCA: AddServiceSettingsFolder: Success");
	}

	return currentRet;
}

STACKHASHCA_API UINT __stdcall AddUrlacl(MSIHANDLE hInstaller)
{
	return DeleteAddUrlacl(hInstaller, STACKHASH_URLACLPREFIX, STACKHASH_URLACLSDDL, FALSE);
}

STACKHASHCA_API UINT __stdcall RemoveUrlacl(MSIHANDLE hInstaller)
{
	return DeleteAddUrlacl(hInstaller, STACKHASH_URLACLPREFIX, STACKHASH_URLACLSDDL, TRUE);
}

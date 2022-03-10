#define WIN32_LEAN_AND_MEAN
#include "loaderhook.h"
#include "utils.h"
#include "../Libraries/Detours/src/detours.h"
#include <crtdbg.h>
#include <shlwapi.h>
#include <stdio.h>
#include "hijackversion.h"

static LDHK_MONITOR_INFO g_info = { 0 };
static BOOL g_isSelf = FALSE;
static BOOL g_isHijack = FALSE;
static BOOL g_isInitialized = FALSE;

DWORD WINAPI RunMonitorLoop(LPVOID lpThreadParameter) {
	return LoaderHookMonitorLoop((PLDHK_MONITOR_INFO)lpThreadParameter);
}

VOID Initialize() {
	if (g_isInitialized)
		return;

	WCHAR variable[20] = { 0 };
	GetEnvironmentVariable(L"EXTREMEDUMPER_MAGIC", variable, 20);
	g_isSelf = wcscmp(variable, L"C41F3A60") == 0;
	if (g_isSelf)
		goto Exit;
	// If current dll is loaded in ExtremeDumper itself, don't initialize

	HINSTANCE hLoaderHook = NULL;
	if (!GetCurrentModuleHandle(&hLoaderHook))
		goto Exit;
	WCHAR configPath[MAX_PATH] = { 0 };
	GetModuleFileName(hLoaderHook, configPath, MAX_PATH);
	PathRemoveFileSpec(configPath);
	PathAppend(configPath, L"LoaderHook.dat");
	// Get config path

	FILE* file = NULL;
	if (_wfopen_s(&file, configPath, L"rb+") != 0 || file == NULL)
		goto Exit;
	if (fread_s(&g_info, sizeof(LDHK_MONITOR_INFO), sizeof(LDHK_MONITOR_INFO), 1, file) != 1)
		goto Exit;
	fclose(file);
	// Read config

	WCHAR fileName[MAX_PATH] = { 0 };
	GetModuleFileName(hLoaderHook, fileName, MAX_PATH);
	PathStripPath(fileName);
	g_isHijack = _wcsicmp(fileName, L"version.dll") == 0;
	// Check hijack mode

Exit:
	g_isInitialized = TRUE;
	return;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
	Initialize();
	if (g_isSelf || DetourIsHelperProcess())
		return TRUE;

	if (g_isHijack)
		HijackDllMain(hModule, ul_reason_for_call, lpReserved);
	switch (ul_reason_for_call) {
	case DLL_PROCESS_ATTACH:
		DetourRestoreAfterWith();
		CreateThread(NULL, 0, RunMonitorLoop, &g_info, 0, NULL);
		// Create thread to run monitor loop
		wprintf_s(L"[LDHK] DllMain: Monitor created\n");
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

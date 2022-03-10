#include "loaderhook.h"
#include "../Libraries/Detours/src/detours.h"
#include <shlwapi.h>
#include <stdio.h>

LPVOID LoadImageOriginal_Mscorwks = NULL;
EXTERN_C VOID LoadImageStub_Mscorwks();

LPVOID LoadImageOriginal_CLR = NULL;
EXTERN_C VOID LoadImageStub_CLR();

static INT32 g_index = 0;

BOOL GetNextFilePath(WCHAR* path) {
	GetTempPath(MAX_PATH, path);
	WCHAR buffer1[MAX_PATH] = { 0 };
	GetModuleFileName(NULL, buffer1, MAX_PATH);
	PathStripPath(buffer1);
	PathAppend(path, buffer1);
	wcscat_s(path, MAX_PATH - wcslen(path), L"_Dumps");
	CreateDirectory(path, NULL);
	if (!PathIsDirectory(path))
		return FALSE;
	size_t index = wcslen(path);
	for (; g_index < MAXINT; g_index++) {
		WCHAR buffer2[20] = { 0 };
		_itow_s(g_index, buffer2, 20, 10);
		PathAppend(path, buffer2);
		wcscat_s(path, MAX_PATH - wcslen(path), L".dll");
		if (!PathFileExists(path))
			return TRUE;
		PathRemoveFileSpec(path);
	}
	return FALSE;
}

VOID DumpCallback(PBYTE peImage, UINT32 size) {
	wprintf_s(L"[LDHK] DumpCallback: peImage=%p size=%X\n", peImage, size);
	WCHAR path[MAX_PATH] = { 0 };
	if (!GetNextFilePath(path))
		return;
	FILE* file = NULL;
	if (_wfopen_s(&file, path, L"wb+") != 0 || file == NULL)
		return;
	if (fwrite(peImage, 1, size, file) != size)
		return;
	fflush(file);
	fclose(file);
	wprintf_s(L"[LDHK] DumpCallback: Saved to %s\n", path);
}

VOID SetHookMscorwks() {
	DetourCreateProcessWithDll(NULL, NULL, NULL, NULL, FALSE, 0, NULL, NULL, NULL, NULL, NULL, NULL);
}

BOOL InstallHook(PVOID* ppPointer, PVOID pDetour) {
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());
	DetourAttach(ppPointer, pDetour);
	BOOL b = DetourTransactionCommit() == NO_ERROR;
	wprintf_s(L"[LDHK] InstallHook: %d\n", b);
	return b;
}

_Success_(SUCCEEDED(return))
HRESULT WINAPI LoaderHookMonitorLoop(_In_ PLDHK_MONITOR_INFO pInfo) {
	if (pInfo == NULL)
		return E_INVALIDARG;

	BYTE foundModules = 0;
	while (foundModules != (pInfo->Flags & LDHK_MONITOR_MODULE_MASK)) {
		HINSTANCE hModule;
		if ((pInfo->Flags & LDHK_MONITOR_MSCORWKS) && !(foundModules & LDHK_MONITOR_MSCORWKS) && (hModule = GetModuleHandle(L"mscorwks.dll"))) {
			LPVOID pLoadImage = (PBYTE)hModule + pInfo->LoadImageRVA_Mscorwks;
			InstallHook(&pLoadImage, LoadImageStub_Mscorwks);
			LoadImageOriginal_Mscorwks = pLoadImage;
			foundModules |= LDHK_MONITOR_MSCORWKS;
			wprintf_s(L"[LDHK] LoaderHookMonitorLoop: LDHK_MONITOR_MSCORWKS at %p\n", (PBYTE)hModule + pInfo->LoadImageRVA_CLR);
		}
		if ((pInfo->Flags & LDHK_MONITOR_CLR) && !(foundModules & LDHK_MONITOR_CLR) && (hModule = GetModuleHandle(L"clr.dll"))) {
			LPVOID pLoadImage = (PBYTE)hModule + pInfo->LoadImageRVA_CLR;
			InstallHook(&pLoadImage, LoadImageStub_CLR);
			LoadImageOriginal_CLR = pLoadImage;
			foundModules |= LDHK_MONITOR_CLR;
			wprintf_s(L"[LDHK] LoaderHookMonitorLoop: LDHK_MONITOR_CLR at %p\n", (PBYTE)hModule + pInfo->LoadImageRVA_CLR);
		}
		Sleep(pInfo->Sleep);
	}

	return TRUE;
}

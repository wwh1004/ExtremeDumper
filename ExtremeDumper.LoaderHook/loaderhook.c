#include "loaderhook.h"
#include "../Libraries/Detours/src/detours.h"
#include <shlwapi.h>
#include <stdio.h>
#include "utils.h"

PVOID LoadImageOriginal_Mscorwks = NULL;
EXTERN_C VOID LoadImageStub_Mscorwks();

PVOID LoadImageOriginal_CLR = NULL;
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

PVOID FindBytes(PVOID startAddress, UINT32 size, PVOID pattern, UINT32 patternSize) {
	for (PBYTE p = (PBYTE)startAddress, end = p + (INT32)(size - patternSize); p < end; p++) {
		if (memcmp(p, pattern, patternSize) == 0)
			return p;
	}
	return NULL;
}

PVOID FindECallFunction(HINSTANCE hModule, PCSTR name) {
	if (hModule == NULL || name == NULL)
		goto ErrExit;

	PIMAGE_DOS_HEADER dosHeader = (PIMAGE_DOS_HEADER)hModule;
	PIMAGE_NT_HEADERS ntHeaders = (PIMAGE_NT_HEADERS)((PBYTE)hModule + dosHeader->e_lfanew);
	PIMAGE_SECTION_HEADER sectionHeaders = (PIMAGE_SECTION_HEADER)((PBYTE)&ntHeaders->OptionalHeader + ntHeaders->FileHeader.SizeOfOptionalHeader);
	PIMAGE_SECTION_HEADER textSectionHeader = sectionHeaders;
	if (strcmp(textSectionHeader->Name, ".text") != 0)
		goto ErrExit;

	PVOID startAddress = (PBYTE)hModule;
	UINT32 size = ntHeaders->OptionalHeader.SizeOfImage;
	PVOID pFuncName = FindBytes(startAddress, size, (PVOID)name, (UINT32)strlen(name) + 1);
	if (pFuncName == NULL)
		goto ErrExit;
	PVOID ppFuncName = FindBytes(startAddress, size, &pFuncName, 4);
	if (ppFuncName == NULL)
		goto ErrExit;
	PVOID funcAddr = ((PVOID*)ppFuncName)[-1];
	if ((PBYTE)funcAddr <= (PBYTE)hModule + textSectionHeader->VirtualAddress || (PBYTE)funcAddr >= (PBYTE)hModule + textSectionHeader->VirtualAddress + size)
		goto ErrExit;
	return funcAddr;

ErrExit:
	return NULL;
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
HRESULT WINAPI LoaderHookCreateProcess(_In_ PCWSTR applicationName, _Inout_opt_ PWSTR commandLine) {
	if (applicationName == NULL)
		return E_INVALIDARG;

	WCHAR szFullExe[MAX_PATH] = { 0 };
	GetFullPathName(applicationName, MAX_PATH, szFullExe, NULL);
	WCHAR currentDirectory[MAX_PATH] = { 0 };
	wcscpy_s(currentDirectory, MAX_PATH, szFullExe);
	PathRemoveFileSpec(currentDirectory);

	//SECURITY_ATTRIBUTES securityAttributes = { 0 };
	//securityAttributes.nLength = sizeof(SECURITY_ATTRIBUTES);
	//securityAttributes.bInheritHandle = TRUE;
	STARTUPINFO startupInfo = { 0 };
	startupInfo.cb = sizeof(STARTUPINFO);
	//HANDLE hStdOutputRead = NULL;
	//CreatePipe(&hStdOutputRead, &startupInfo.hStdOutput, &securityAttributes, 0);
	//startupInfo.dwFlags |= STARTF_USESTDHANDLES;
	PROCESS_INFORMATION processInformation = { 0 };

	CHAR dllPath[MAX_PATH];
	HINSTANCE hLoaderHook = NULL;
	GetCurrentModuleHandle(&hLoaderHook);
	GetModuleFileNameA(hLoaderHook, dllPath, MAX_PATH);

	BOOL b = DetourCreateProcessWithDll(applicationName, commandLine, NULL, NULL, TRUE, CREATE_NEW_CONSOLE, NULL, currentDirectory, &startupInfo, &processInformation, dllPath, NULL);
	return b ? S_OK : E_FAIL;
}

_Success_(SUCCEEDED(return))
HRESULT WINAPI LoaderHookMonitorLoop(_In_ PLDHK_MONITOR_INFO pInfo) {
	if (pInfo == NULL)
		return E_INVALIDARG;

	BYTE foundModules = 0;
	while (foundModules != (pInfo->Flags & LDHK_MONITOR_MODULE_MASK)) {
		HINSTANCE hModule;
		if ((pInfo->Flags & LDHK_MONITOR_MSCORWKS) && !(foundModules & LDHK_MONITOR_MSCORWKS) && (hModule = GetModuleHandle(L"mscorwks.dll"))) {
			PVOID pLoadImage = NULL;
			if (pInfo->LoadImageRVA_Mscorwks) {
				pLoadImage = (PBYTE)hModule + pInfo->LoadImageRVA_Mscorwks;
				wprintf_s(L"[LDHK] LoaderHookMonitorLoop: Found nLoadImage at %p by config\n", pLoadImage);
			}
			if (!pLoadImage) {
				pLoadImage = FindECallFunction(hModule, "nLoadImage");
				wprintf_s(L"[LDHK] LoaderHookMonitorLoop: Found nLoadImage at %p by ECall\n", pLoadImage);
			}
			if (pLoadImage) {
				LoadImageOriginal_Mscorwks = pLoadImage;
				InstallHook(&LoadImageOriginal_Mscorwks, LoadImageStub_Mscorwks);
			}
			foundModules |= LDHK_MONITOR_MSCORWKS;
			wprintf_s(L"[LDHK] LoaderHookMonitorLoop: LDHK_MONITOR_MSCORWKS at %p\n", pLoadImage);
		}
		if ((pInfo->Flags & LDHK_MONITOR_CLR) && !(foundModules & LDHK_MONITOR_CLR) && (hModule = GetModuleHandle(L"clr.dll"))) {
			PVOID pLoadImage = NULL;
			if (pInfo->LoadImageRVA_CLR) {
				pLoadImage = (PBYTE)hModule + pInfo->LoadImageRVA_CLR;
				wprintf_s(L"[LDHK] LoaderHookMonitorLoop: Found nLoadImage at %p by config\n", pLoadImage);
			}
			if (!pLoadImage) {
				pLoadImage = FindECallFunction(hModule, "nLoadImage");
				wprintf_s(L"[LDHK] LoaderHookMonitorLoop: Found nLoadImage at %p by ECall\n", pLoadImage);
			}
			if (pLoadImage) {
				LoadImageOriginal_CLR = pLoadImage;
				InstallHook(&LoadImageOriginal_CLR, LoadImageStub_CLR);
			}
			foundModules |= LDHK_MONITOR_CLR;
			wprintf_s(L"[LDHK] LoaderHookMonitorLoop: LDHK_MONITOR_CLR at %p\n", pLoadImage);
		}
		Sleep(pInfo->Sleep);
	}

	return S_OK;
}

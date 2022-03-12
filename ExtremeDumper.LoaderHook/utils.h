#pragma once

#include <windows.h>

static WCHAR __DONT_USE_CURRENT_MODULE_DUMMY = 0;

#define GetCurrentModuleHandle(phModule) GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT | GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, &__DONT_USE_CURRENT_MODULE_DUMMY, phModule)

#include "Exports.h"

#include "../InjectorLib/GetProcessId.h"
#include "../InjectorLib/LoadLibraryInject.h"
#include "../InjectorLib/ManualMapInject.h"


DWORD GetProcessIdWrapper(const char* szExeName) {
	return GetProcessId(szExeName);
}

bool LoadLibraryInjectWrapper(DWORD ID, const char* dll) {
	return LoadLibraryInject(ID, dll);
}

bool ManualMapInjectWrapper(DWORD ID, const char* dll) {
	return ManualMapInject(ID, dll);
}

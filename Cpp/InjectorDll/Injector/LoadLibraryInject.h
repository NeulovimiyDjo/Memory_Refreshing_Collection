#pragma once

#include <windows.h> 


bool LoadLibraryInject(DWORD ID, const char* dll)
{
    HANDLE hProc;
    LPVOID pParamMemory;
    LPVOID LoadLibrary;

    if (!ID)
        return false;

    hProc = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, FALSE, ID);
    LoadLibrary = (LPVOID)GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
    
    pParamMemory = (LPVOID)VirtualAllocEx(hProc, NULL, strlen(dll) + 1, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
    WriteProcessMemory(hProc, (LPVOID)pParamMemory, dll, strlen(dll) + 1, NULL);
    
    CreateRemoteThread(hProc, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLibrary, (LPVOID)pParamMemory, NULL, NULL);
    
    CloseHandle(hProc);
    VirtualFreeEx(hProc, (LPVOID)pParamMemory, 0, MEM_RELEASE);

    return true;
}
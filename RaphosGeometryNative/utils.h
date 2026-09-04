#pragma once

// Raphos Geometry native layer — shared export/return/memory conventions.
// Mirrors the proven RaphosToolsNative pattern:
//   * every export is  extern "C" __declspec(dllexport)  (cdecl)
//   * functions return int == RAPHOS_SUCCESS (1) / RAPHOS_ERROR (0)
//   * scalars out by reference; arrays out via double**/Long** allocated new[]
//     inside the DLL and released by a C# callback to ReleaseMemory*.

#define RAPHOS_EXPORT extern "C" __declspec(dllexport)
#define RAPHOS_ERROR 0
#define RAPHOS_SUCCESS 1

typedef long long Long;
typedef unsigned long long ULong;

#pragma region Tests

RAPHOS_EXPORT
int Sum(double a, double b, double& c);

RAPHOS_EXPORT
bool IsAllGood(bool b);

typedef int(*SimpleDelegate)(int a, int b);

RAPHOS_EXPORT
int PassDelegate(SimpleDelegate del);

#pragma endregion

RAPHOS_EXPORT
int ReleaseMemoryLongsOfLongs(Long* ptr);

RAPHOS_EXPORT
int ReleaseMemoryDoublesOfDoubles(double* ptr);

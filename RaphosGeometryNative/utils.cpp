#include "utils.h"
#include <vector>

using namespace std;

int Sum(double a, double b, double& c)
{
    c = a + b;
    return RAPHOS_SUCCESS;
}

bool IsAllGood(bool b)
{
    return !b;
}

int PassDelegate(SimpleDelegate del)
{
    int a(1), b(2);
    int res = del(a, b);
    return res == a + b ? RAPHOS_SUCCESS : RAPHOS_ERROR;
}

// Output buffers are allocated with new[] on the native side, so release them
// with delete[] (the C# interop calls these back once it has copied the data).
int ReleaseMemoryLongsOfLongs(Long* ptr)
{
    delete[] ptr;
    return RAPHOS_SUCCESS;
}

int ReleaseMemoryDoublesOfDoubles(double* ptr)
{
    delete[] ptr;
    return RAPHOS_SUCCESS;
}

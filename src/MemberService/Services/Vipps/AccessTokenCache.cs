namespace MemberService.Services.Vipps;
using System;
using System.Threading.Tasks;

using MemberService.Services.Vipps.Models;

public class AccessTokenCache
{
    private SemaphoreSlim _semaphore = new(0, 1);
    private AccessTokenResponse _accessToken;

    public async Task<string> GetOrSet(Func<Task<AccessTokenResponse>> factory)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_accessToken is null || _accessToken.HasExpired)
            {
                _accessToken = await factory();
            }

            return _accessToken.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

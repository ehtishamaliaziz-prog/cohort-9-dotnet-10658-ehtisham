using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Tests
{
    public class FakeTokenService : ITokenService
    {
        public string CreateToken(User user)
        {
            return "fake-test-token";
        }
    }
}
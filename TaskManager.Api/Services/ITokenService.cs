using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
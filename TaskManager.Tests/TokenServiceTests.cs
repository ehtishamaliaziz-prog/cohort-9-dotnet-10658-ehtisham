using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
using Xunit;

namespace TaskManager.Tests
{
    public class TokenServiceTests
    {
        private IConfiguration GetTestConfig()
        {
            var settings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ThisIsATestKeyThatIsLongEnoughForHmacSha256!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void CreateToken_ReturnsNonEmptyToken()
        {
            var config = GetTestConfig();
            var tokenService = new TokenService(config);
            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "irrelevant",
                Role = UserRole.User
            };

            var token = tokenService.CreateToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void CreateToken_IncludesCorrectClaims()
        {
            var config = GetTestConfig();
            var tokenService = new TokenService(config);
            var user = new User
            {
                Id = 7,
                FullName = "Claim Check",
                Email = "claimcheck@example.com",
                PasswordHash = "irrelevant",
                Role = UserRole.Admin
            };

            var token = tokenService.CreateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("7", jwt.Claims.First(c => c.Type == "nameid" || c.Type.EndsWith("nameidentifier")).Value);
            Assert.Contains(jwt.Claims, c => c.Value == "claimcheck@example.com");
            Assert.Contains(jwt.Claims, c => c.Value == "Admin");
        }

        [Fact]
        public void CreateToken_SetsCorrectIssuerAndAudience()
        {
            var config = GetTestConfig();
            var tokenService = new TokenService(config);
            var user = new User
            {
                Id = 1,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "irrelevant",
                Role = UserRole.User
            };

            var token = tokenService.CreateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal("TestIssuer", jwt.Issuer);
            Assert.Equal("TestAudience", jwt.Audiences.First());
        }
    }
}
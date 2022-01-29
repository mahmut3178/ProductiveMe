using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Entities.Concrete;
using Core.Extensions;
using Core.UnitOfWork.Repositories;
using Core.Utilities.Security.Encryption;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Core.Utilities.Security.Jwt
{
    public class JwtHelper:ITokenHelper
    {
        public IConfiguration Configuration { get; }
        private TokenOptions _tokenOptions;
        private DateTime _accessTokenExpiration;
        public JwtHelper(IConfiguration configuration)
        {
            Configuration = configuration;
            _tokenOptions = Configuration.GetSection("TokenOptions").Get<TokenOptions>();
            _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
        }
        public AccessToken CreateToken(User user, IRepository<RefreshToken> refreshTokenRepository)
        {
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
            var jwt = CreateJwtSecurityToken(_tokenOptions, user, signingCredentials, user.UserRoles.Select(x=> x.Role).ToList());
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = jwtSecurityTokenHandler.CreateToken(jwt);

            var jwtToken = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                JwtId = jwtSecurityToken.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Used = false,
                Invalidated = false
            };

            refreshTokenRepository.Create(refreshToken);            

            return new AccessToken
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Id.ToString(),
                Expiration = _accessTokenExpiration
            };
        }

        private SecurityTokenDescriptor CreateJwtSecurityToken(TokenOptions tokenOptions, User user, 
            SigningCredentials signingCredentials, List<Role> roles)
        {

            //Redundant
            // JwtSecurityToken version 
            //var jwt = new JwtSecurityToken(
            //    expires:_accessTokenExpiration,
            //    notBefore:DateTime.Now,
            //    claims: SetClaims(user, roles),
            //    signingCredentials:signingCredentials
            //);

            var jwt = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(SetClaims(user, roles)),
                Expires = _accessTokenExpiration,
                SigningCredentials = signingCredentials,
                NotBefore = DateTime.UtcNow
            };
            return jwt;
        }

        private IEnumerable<Claim> SetClaims(User user, List<Role> roles)
        {
            var claims = new List<Claim>();
            claims.AddJwtId();
            claims.AddNameIdentifier(user.Id.ToString());
            claims.AddRoles(roles.Select(c=>c.Name).ToArray());            
            return claims;
        }
        
    }
}

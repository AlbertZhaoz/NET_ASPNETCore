using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SwiftCode.BBS.Common.Helper
{
    public class JwtHelper
    {

        /// <summary>
        /// 颁发JWT字符串
        /// </summary>
        /// <param name="tokenModel"></param>
        /// <returns></returns>
        public static string IssueJwt(TokenModelJwt tokenModel)
        {
            string iss = AppSettingsHelper.app(new string[] { "Audience", "Issuer" });
            string aud = AppSettingsHelper.app(new string[] { "Audience", "Audience" });
            string secret = AppSettingsHelper.app(new string[] { "Audience", "Secret" });
            double expireTime = double.Parse(AppSettingsHelper.app(new string[] { "Audience", "ExpireSeconds" }));

            var claims = new List<Claim>
                {
                 /*
                 * 特别重要：
                   1、这里将用户的部分信息，比如 uid 存到了Claim 中，如果你想知道如何在其他地方将这个 uid从 Token 中取出来，请看下边的SerializeJwt() 方法，或者在整个解决方案，搜索这个方法，看哪里使用了！
                   2、你也可以研究下 HttpContext.User.Claims ，具体的你可以看看 Policys/PermissionHandler.cs 类中是如何使用的。
                 */
                 new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                //这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
                new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(expireTime)).ToUnixTimeSeconds()}"),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(expireTime).ToString()),
                new Claim(JwtRegisteredClaimNames.Iss,iss),
                new Claim(JwtRegisteredClaimNames.Aud,aud),

               };

            // 可以将一个用户的多个角色全部赋予；
            claims.AddRange(tokenModel.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

            //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度太短会报出异常)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: iss,
                claims: claims,
                signingCredentials: creds);

            var jwtHandler = new JwtSecurityTokenHandler();
            var encodedJwt = jwtHandler.WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// 解析前校验secretkey
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static TokenModelJwt SerializeJwtBySecret(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            TokenModelJwt tokenModelJwt = new TokenModelJwt();
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettingsHelper.app(new string[] { "Audience", "Secret" })));
            var tokenValidationParameters = new TokenValidationParameters();
            // 只验证SecretKey 不验证Audience和Issuer
            tokenValidationParameters.IssuerSigningKey = secretKey;
            tokenValidationParameters.ValidateAudience = false;
            tokenValidationParameters.ValidateIssuer = false;

            var jwtStrIsOk = true;

            try
            {
                jwtHandler.ValidateToken(jwtStr, tokenValidationParameters, out SecurityToken securityToken);
            }
            catch (Exception)
            {
                jwtStrIsOk = false;
            }
            
            // token校验
            if (!string.IsNullOrEmpty(jwtStr) && jwtHandler.CanReadToken(jwtStr)&&jwtStrIsOk)
            {

                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);

                object role;

                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);

                tokenModelJwt = new TokenModelJwt
                {
                    Uid = Convert.ToInt64(jwtToken.Id),
                    Role = role == null ? "" : role.ToString()
                };
            }
            return tokenModelJwt;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        public static TokenModelJwt SerializeJwt(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            TokenModelJwt tokenModelJwt = new TokenModelJwt();

            // token校验
            if (!string.IsNullOrEmpty(jwtStr) && jwtHandler.CanReadToken(jwtStr))
            {

                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);

                object role;

                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);

                tokenModelJwt = new TokenModelJwt
                {
                    Uid = Convert.ToInt64(jwtToken.Id),
                    Role = role == null ? "" : role.ToString()
                };
            }
            return tokenModelJwt;
        }
    }

    /// <summary>
    /// 令牌
    /// </summary>
    public class TokenModelJwt
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Uid { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string Work { get; set; }

    }
}

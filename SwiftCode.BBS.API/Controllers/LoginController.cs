using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SwiftCode.BBS.Common.Helper;

namespace SwiftCode.BBS.API.Controllers
{
    /// <summary>
    /// LoginController
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// GetJwtAdminStr
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetJwtAdminStr(string role)
        {
            // 将用户id和角色名，作为单独的自定义变量封装进 token 字符串中。
            TokenModelJwt tokenModel = new TokenModelJwt { Uid = 1, Role = role };
            var jwtStr = JwtHelper.IssueJwt(tokenModel);//登录，获取到一定规则的 Token 令牌
            var suc = true;
            return Ok(new
            {
                success = suc,
                token = jwtStr
            });
        }
    }
}

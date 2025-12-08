using Materal.MergeBlock.Authorization.Abstractions;
using Materal.MergeBlock.Web.Abstractions.Controllers;
using Materal.Utils.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MMB.WebModuleTest.Controllers;

/// <summary>
/// 授权测试控制器
/// </summary>
/// <param name="tokenService"></param>
public class AuthorizationTestController(ITokenService tokenService) : MergeBlockController
{
    /// <summary>
    /// 获取Token
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ResultModel<string>> GetTokenAsync()
    {
        string token = tokenService.GetToken(Guid.NewGuid());
        return ResultModel<string>.Success(token, "获取成功");
    }
    /// <summary>
    /// 测试授权
    /// </summary>
    [HttpGet]
    public ResultModel<string> TestAuthorization()
    {
        return ResultModel<string>.Success("授权通过", "获取成功");
    }
}
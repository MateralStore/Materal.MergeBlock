using Materal.MergeBlock.Authorization.Abstractions;
using Microsoft.AspNetCore.Authorization;
using MMB.Demo.Abstractions.Enums;
using System.Security.Claims;

namespace MMB.Demo.Application.Controllers;

public class AuthorizationTestController(ITokenService tokenService) : DemoController
{
    /// <summary>
    /// 获取管理员Token
    /// </summary>
    /// <returns></returns>
    [HttpPost, AllowAnonymous]
    public Task<ResultModel<string>> GetAdminTokenAsync()
    {
        Guid userID = Guid.NewGuid();
        string token = tokenService.GetToken(new Claim(tokenService.UserIDKey, userID.ToString()), new Claim(ClaimTypes.Role, UserRole.Admin.ToString()));
        ResultModel<string> result = ResultModel<string>.Success(token, "获取管理员Token成功");
        return Task.FromResult(result);
    }

    /// <summary>
    /// 获取教师Token
    /// </summary>
    /// <returns></returns>
    [HttpPost, AllowAnonymous]
    public Task<ResultModel<string>> GetTeacherTokenAsync()
    {
        Guid userID = Guid.NewGuid();
        string token = tokenService.GetToken(new Claim(tokenService.UserIDKey, userID.ToString()), new Claim(ClaimTypes.Role, UserRole.Teacher.ToString()));
        ResultModel<string> result = ResultModel<string>.Success(token, "获取教师Token成功");
        return Task.FromResult(result);
    }

    /// <summary>
    /// 授权测试
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public Task<ResultModel> TestAuthorizeAsync()
    {
        ResultModel result = ResultModel.Success("授权测试成功");
        return Task.FromResult(result);
    }

    /// <summary>
    /// 匿名测试
    /// </summary>
    /// <returns></returns>
    [HttpPost, AllowAnonymous]
    public Task<ResultModel> TestAllowAnonymousAsync()
    {
        ResultModel result = ResultModel.Success("匿名测试成功");
        return Task.FromResult(result);
    }

    /// <summary>
    /// 管理员授权测试
    /// </summary>
    /// <returns></returns>
    [HttpPost, Authorize(Policy = DemoAuthorizationPolicies.AdminOnly)]
    public Task<ResultModel> TestAdminOnlyAsync()
    {
        ResultModel result = ResultModel.Success("管理员授权测试成功");
        return Task.FromResult(result);
    }
}

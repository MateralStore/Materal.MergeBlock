namespace MMB.Demo.Abstractions.Enums;

/// <summary>
/// 用户角色
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 系统管理员
    /// </summary>
    [Description("系统管理员")]
    Admin = 0,

    /// <summary>
    /// 教师
    /// </summary>
    [Description("教师")]
    Teacher = 1
}

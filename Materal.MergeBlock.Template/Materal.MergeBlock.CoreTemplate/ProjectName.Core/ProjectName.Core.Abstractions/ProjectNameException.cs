namespace ProjectName.Core.Abstractions;

/// <summary>
/// ProjectName异常
/// </summary>
public class ProjectNameException : MergeBlockModuleException
{
    /// <summary>
    /// ProjectName异常
    /// </summary>
    public ProjectNameException()
    {
    }
    /// <summary>
    /// ProjectName异常
    /// </summary>
    /// <param name="message"></param>
    public ProjectNameException(string message) : base(message)
    {
    }
    /// <summary>
    /// ProjectName异常
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ProjectNameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
using Microsoft.AspNetCore.SignalR;

namespace MMB.Demo.Application.Hubs;

/// <summary>
/// 测试用Hub
/// </summary>
public class TestHub(IHubContext<TestHub> hubContext) : Hub
{
    /// <summary>
    /// 自动推送计时器
    /// </summary>
    private static Timer? _autoPushTimer;
    /// <summary>
    /// 自动推送是否开启
    /// </summary>
    private static bool _isAutoPushEnabled;
    /// <summary>
    /// 自动推送间隔（毫秒）
    /// </summary>
    private const int AutoPushInterval = 1000;

    /// <summary>
    /// 接收消息并返回信息（通过回调）
    /// </summary>
    /// <param name="message">接收到的消息</param>
    /// <returns>响应消息</returns>
    public async Task SendMessage(string message) => await SendMessageWithCallback(message, "MessageReceived");
    /// <summary>
    /// 接收消息并返回信息（通过回调）
    /// </summary>
    /// <param name="message">接收到的消息</param>
    /// <returns>响应消息</returns>
    public async Task SendMessageWithCallback(string message, string callbackEventName)
        => await Clients.Caller.SendAsync(callbackEventName, $"服务端收到消息: {message}");

    /// <summary>
    /// 接收消息并返回信息
    /// </summary>
    /// <param name="message">接收到的消息</param>
    /// <returns>响应消息</returns>
    public async Task<string> SendMessageWithResult(string message) => $"服务端收到消息: {message}";

    /// <summary>
    /// 获取连接ID
    /// </summary>
    /// <returns>连接ID</returns>
    public string GetConnectionId() => Context.ConnectionId;

    /// <summary>
    /// 开启自动推送
    /// </summary>
    public void StartAutoPush()
    {
        if (_isAutoPushEnabled) return;
        _isAutoPushEnabled = true;
        _autoPushTimer = new Timer(async _ =>
        {
            await PushCurrentEvent();
        }, null, 0, AutoPushInterval);
    }

    /// <summary>
    /// 关闭自动推送
    /// </summary>
    public void StopAutoPush()
    {
        _isAutoPushEnabled = false;
        _autoPushTimer?.Dispose();
        _autoPushTimer = null;
    }

    /// <summary>
    /// 推送当前事件给所有客户端
    /// </summary>
    private async Task PushCurrentEvent()
    {
        if (!_isAutoPushEnabled || _autoPushTimer == null) return;
        await hubContext.Clients.All.SendAsync("AutoPush", $"自动推送: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}

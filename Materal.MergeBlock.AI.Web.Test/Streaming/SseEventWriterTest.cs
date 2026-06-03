namespace Materal.MergeBlock.AI.Web.Test.Streaming;

[TestClass]
public class SseEventWriterTest
{
    [TestMethod]
    public void Format_ShouldWriteEventNameAndSnakeCaseData()
    {
        AgentStreamEvent streamEvent = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Seq = 1,
            Event = "tool_call.requested",
            Payload = new Dictionary<string, object?>
            {
                ["tool_call_id"] = "call_001"
            }
        };

        string content = SseEventWriter.Format(streamEvent);

        StringAssert.StartsWith(content, "event: tool_call.requested");
        StringAssert.Contains(content, "\"schema_version\":\"agent-stream-v1\"");
        StringAssert.EndsWith(content, "\r\n\r\n");
    }
}

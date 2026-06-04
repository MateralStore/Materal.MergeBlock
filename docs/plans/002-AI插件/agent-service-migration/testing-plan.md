# Agent Service Migration Test Plan

> **For agentic workers:** Use this document together with `implementation-plan.md`. Automated tests are the release gate; the Demo test page is the manual and exploratory verification surface. If the current page cannot execute a required scenario, upgrade the page before marking the scenario complete.

## Goal

Prove that the generic MMB Agent Host can preserve the existing external Agent service contract while supporting runtime bridge, remote tools, server-executed tools, review gates, watchdog, checkpoint/debug trace, and audit redaction.

## Test Layers

| Layer | Purpose | Primary files | Gate |
| --- | --- | --- | --- |
| Contract tests | Lock JSON wire names, SSE event names, payload shape, and status mapping. | `Materal.MergeBlock.AI.Web.Test/Models`, `Materal.MergeBlock.AI.Web.Test/Streaming` | Required before controller/runtime changes are accepted. |
| Runtime unit tests | Prove provider-neutral requests, output mapping, watchdog, review, tool registry, and redaction behavior. | `Materal.MergeBlock.AI.Test`, `Materal.MergeBlock.AI.Web.Test/Runtime`, `Materal.MergeBlock.AI.Web.Test/Persistence` | Required before Demo validation. |
| Integration tests | Prove controller, state store, remote tool gateway, pause/resume/cancel, and trace persistence work together. | `Materal.MergeBlock.AI.Web.Test/Runtime`, `Materal.MergeBlock.AI.Web.Test/RemoteTools`, `Materal.MergeBlock.AI.Web.Test/Persistence` | Required before manual page verification. |
| Demo tests | Prove the business/demo runtime can deserialize compatible requests and produce compatible events. | `MMB.Demo.Test/AI` | Required before starting WebAPI manual validation. |
| Manual page tests | Prove a human can run the same critical flows through the Demo WebAPI and inspect event payloads, run status, debug trace, and redaction. | `MMB.Demo.WebAPI/wwwroot/ai-chat-test.html` | Required before migration sign-off. |

## Current Test Page Assessment

Current page: `Materal.MergeBlock/MMB/MMB.Demo/MMB.Demo.WebAPI/wwwroot/ai-chat-test.html`.

The current page already supports these checks:

- Authenticated calls through the Demo token endpoint.
- Basic streaming chat to `/agent/chat/stream`.
- Visible SSE event log and message stream.
- Prompt-injection sample message.
- Server-side local tool sample message.
- Remote tool request and resume flow through `/agent/chat/resume/stream`.
- Manual cancel through `/agent/runs/{run_id}/cancel`.

The page is not enough for final migration sign-off until it can also verify these items:

- Request editor supports `schema_version`, `model_config`, `context`, `metadata`, and snake_case payload preview.
- Event assertions can check required event order and required payload fields, especially `message.delta.payload.text`, `run.paused.payload.reason`, and `run.paused.payload.tool_call_ids`.
- Remote tool panel supports completed, failed, rejected, invalid JSON, duplicate result, missing result, extra result, and multi-tool-call result cases.
- Status panel can fetch `/agent/runs/{run_id}` and confirm internal `waiting_tool_result` is exposed as public `paused`.
- Debug panel can fetch `/agent/debug-traces/{trace_id}` or the run trace endpoint and show timeline, checkpoint metadata, tool results, review records, watchdog events, and redaction.
- Watchdog/recovery scenarios can be triggered with deterministic demo messages instead of waiting for an unreliable real timeout.
- Export action can save a compact verification report containing request, event sequence, assertions, run status, and trace summary.
- The page must not expose raw API keys, bearer tokens, or provider credentials in request previews, events, trace panels, or exported reports.

## Test Page Upgrade Plan

Upgrade the page only when the current controls cannot run a required manual case. Keep it a single static HTML page so it can remain a lightweight Demo WebAPI artifact.

### Required UI Areas

- Request panel:
  - Endpoint base URL.
  - Token acquisition and masked token display.
  - Thread id and run id.
  - Message.
  - JSON request editor with snake_case preview.
  - Model config fields: provider, model, base_url, temperature, max_tokens, reasoning_enabled, thinking_enabled. API key must be accepted only as masked local input and omitted from preview/export.
- Scenario panel:
  - Basic chat.
  - Remote tool pause/resume.
  - Server-executed tool.
  - Review approved.
  - Review rejected.
  - Cancel while streaming.
  - Watchdog heartbeat.
  - Watchdog recovery failure.
  - Invalid resume payload.
- Assertion panel:
  - Expected event sequence selector.
  - Required payload field checklist.
  - Pass/fail result for each assertion.
  - First failing event index and reason.
- Tool result panel:
  - Pending tool call list.
  - Per-tool status selector: completed, failed, rejected.
  - Result JSON and error JSON editors.
  - Multi-result submit button.
  - Invalid resume buttons for missing, extra, duplicate, wrong run, and wrong thread cases.
- Status and trace panel:
  - Fetch run status.
  - Fetch session summary.
  - Fetch debug trace.
  - Show checkpoint metadata with redacted model config.
  - Show audit records with redacted arguments and results.
- Report panel:
  - Export JSON report.
  - Copy event sequence.
  - Clear current run output without clearing token or base URL.

### Page Implementation Tasks

1. Add a small `testScenarios` map in `ai-chat-test.html`. Each scenario defines request overrides, expected events, expected payload fields, and optional post-action such as resume or cancel.
2. Add `buildChatRequest()` so the page sends snake_case fields by default and can show the exact JSON before sending.
3. Add `assertEvents(events, scenario)` that returns assertion items with `name`, `status`, `eventIndex`, and `message`.
4. Add a status/trace fetch helper that calls the implemented run and debug endpoints using the same auth header.
5. Add redaction helper for page-side previews and reports. It must replace `api_key`, `authorization`, `token`, `password`, and `secret` values with `[REDACTED]`.
6. Add export report generation with `thread_id`, `run_id`, `scenario`, `request_preview`, `events`, `assertions`, `run_status`, and `trace_summary`.
7. Add deterministic demo trigger messages in the Demo runtime for manual cases that should not depend on real model behavior. Keep these triggers generic and document them in the page only as scenario names, not business-domain behavior.

## Manual Scenario Matrix

| Priority | Scenario | Action | Expected result |
| --- | --- | --- | --- |
| P0 | Basic chat | Send a normal message. | Events include `run.started`, at least one `message.delta`, and `run.completed`; `message.delta.payload.text` is present. |
| P0 | Model config compatibility | Send a request with snake_case `model_config`. | Runtime receives provider-neutral config; trace stores only redacted summary; no raw credentials appear. |
| P0 | Remote tool pause | Trigger a remote client action. | Events include `tool_call.requested` and `run.paused`; `run.paused` includes `reason` and `tool_call_ids`; public status is `paused`. |
| P0 | Remote tool resume completed | Submit matching tool results. | Events include `tool_result.completed`, `message.delta`, and `run.completed`; pending tool calls are closed. |
| P0 | Invalid remote resume | Submit missing, extra, duplicate, wrong-run, or wrong-thread tool results. | Runtime is not called; run remains paused; error is visible and traceable. |
| P0 | Cancel | Cancel an active run. | Events include `run.cancelled`; status endpoint returns cancelled; stream stops cleanly. |
| P0 | Debug trace | Fetch trace for a completed or paused run. | Trace includes timeline, checkpoint, tool calls, tool results, review records, and redacted model config. |
| P0 | Redaction | Include sensitive fields in request metadata/tool arguments. | Events, checkpoint, trace, audit, and exported report show `[REDACTED]`. |
| P1 | Server-executed tool | Trigger a registered server-side tool. | Tool execution is audited; result is model-visible when configured; event stream records completion. |
| P1 | Review rejected | Trigger a pre-execution review rejection. | Events include `script_review.completed` and `tool_result.completed` with rejected/failed status; no client execution is requested. |
| P1 | Watchdog heartbeat | Trigger deterministic slow output. | Events include `agent.heartbeat`; run continues or completes according to scenario. |
| P1 | Watchdog recovery failure | Trigger deterministic idle output. | Events include `agent.recovery.started` and `agent.recovery.failed`; run fails with clear status. |
| P2 | Export report | Export after any P0 scenario. | Exported JSON contains request preview, event sequence, assertions, status, and trace summary without secrets. |

## Automated Verification Commands

Run from `E:\Project\Materal\Materal`:

```powershell
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Test\Materal.MergeBlock.AI.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\Materal.MergeBlock.AI.Web.Test\Materal.MergeBlock.AI.Web.Test.csproj -p:UseSharedCompilation=false
dotnet test --project .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.Test\MMB.Demo.Test.csproj --filter "Glm51AIAgentRuntimeTest|AgentWireCompatibilityTest" -p:UseSharedCompilation=false
dotnet build .\Materal.MergeBlock\MMB\MMB.Demo\MMB.Demo.slnx -p:UseSharedCompilation=false
```

Expected: all tests pass and the Demo solution builds.

## Manual Verification Flow

1. Start `MMB.Demo.WebAPI`.
2. Open `/ai-chat-test.html`.
3. Acquire a Demo token.
4. Run every P0 scenario in the matrix.
5. For each scenario, check the assertion panel and export the report.
6. Run P1 scenarios before migration sign-off if the corresponding feature was implemented in this iteration.
7. Confirm no exported report contains raw credentials.

## Completion Criteria

Migration testing is complete only when all of these are true:

- Automated tests pass for AI core, AI Web, and Demo AI validation.
- Every P0 manual scenario passes through the Demo WebAPI page.
- The page can inspect event sequence, run status, debug trace, checkpoint summary, and redaction result.
- Any page gap that blocks a P0 scenario has been fixed in `ai-chat-test.html`.
- GitNexus change detection has been run before any commit request.

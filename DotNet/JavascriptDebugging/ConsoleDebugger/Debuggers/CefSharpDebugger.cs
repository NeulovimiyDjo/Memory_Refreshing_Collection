using CommunicatorLib;
using CommunicatorLib.Messages;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleDebugger.Debuggers
{
    public class CefSharpDebugger
    {
        private static readonly TimeSpan WaitForDebuggerTimeout = TimeSpan.FromSeconds(99999);
        private static readonly TimeSpan WebsocketConnectTimeout = TimeSpan.FromSeconds(99999);

        private string _callFrameId = null;


        public async Task StartAsync()
        {
            using (var communicator = new Communicator("localhost", 8881))
            {
                await ConnectAndLoadScriptAsync(communicator);
                await RunAsync(communicator);

                Console.Out.Write("->");
                string cmd = Console.In.ReadLine();
                while (cmd != "exit")
                {
                    await ProcessCommandAsync(communicator, cmd);

                    Console.Out.Write("->");
                    cmd = Console.In.ReadLine();
                }

                await communicator.Disconnect(new CancellationTokenSource(WebsocketConnectTimeout).Token);
            }
        }


        private async Task ProcessCommandAsync(Communicator communicator, string cmd)
        {
            switch (cmd)
            {
                case "eval":
                    await EvalAsync(communicator);
                    break;
                case "run":
                    await RunAsync(communicator);
                    break;
                case "continue":
                    await ContinueAsync(communicator);
                    break;
                default:
                    break;
            }
        }


        private async Task EvalAsync(Communicator communicator)
        {
            if (_callFrameId == null)
            {
                Console.Out.WriteLine("No breakpoint have been hit yet");
                return;
            }


            // Evaluate the variable 'two' (should be undefined)
            var commandResult = await communicator.SendCommand("Debugger.evaluateOnCallFrame", new { callFrameId = _callFrameId, expression = "two" });
            var evaluateResult = JsonConvert.DeserializeObject<V8CommandResponse<EvaluateOnCallFrameResponse>>(commandResult);
            Console.Out.WriteLine($"Result from evaluating expression 'two' before setVariableValue: type: {evaluateResult.Result.Result.ObjectType}, value: {evaluateResult.Result.Result.Value}");

            // Set the value of the variable 'two'. Note that setting a variable value will cause a scriptParsed event to be emitted, as will evaluating on call frame (below)
            await communicator.SendCommand("Debugger.setVariableValue", new { scopeNumber = 0, variableName = "two", newValue = new { value = 3 }, callFrameId = _callFrameId });

            // Evaluate the variable 'two' again (should be 3)
            commandResult = await communicator.SendCommand("Debugger.evaluateOnCallFrame", new { callFrameId = _callFrameId, expression = "two" });
            evaluateResult = JsonConvert.DeserializeObject<V8CommandResponse<EvaluateOnCallFrameResponse>>(commandResult);
            Console.Out.WriteLine($"Result from evaluating expression 'two' after setVariableValue: type: {evaluateResult.Result.Result.ObjectType}, value: {evaluateResult.Result.Result.Value}");
        }


        private async Task RunAsync(Communicator communicator)
        {
            if (_callFrameId != null)
            {
                Console.Out.WriteLine("Already waiting for breakpoint");
                return;
            }


            Console.Out.WriteLine("Waiting for breakpoint");
            // Break on our user breakpoint (debugger keyword)
            var debuggerPausedEvent = await communicator.WaitForEventAsync<DebuggerPausedEvent>(new CancellationTokenSource(WaitForDebuggerTimeout).Token);
            Console.Out.WriteLine("Breakpoint was hit");

            // Pull out the callFrameId for that frame
            _callFrameId = debuggerPausedEvent.CallFrames[0].CallFrameId;
        }


        private async Task ContinueAsync(Communicator communicator)
        {
            Console.Out.WriteLine("Resuming debugger");
            await communicator.SendCommand("Debugger.resume");
            _callFrameId = null;

            await RunAsync(communicator);
        }


        private static async Task ConnectAndLoadScriptAsync(Communicator communicator)
        {
            await communicator.Connect(new CancellationTokenSource(WebsocketConnectTimeout).Token);

            // https://chromedevtools.github.io/devtools-protocol/1-2/Debugger/

            await communicator.SendCommand("Runtime.enable");
            await communicator.SendCommand("Debugger.enable");


            // After enabling debugging? we should get both a ScriptParsed event & a DebuggerPaused event.
            var scriptParsedEvent = await communicator.WaitForEventAsync<ScriptParsedEvent>(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
            var commandResult = await communicator.SendCommand("Debugger.getScriptSource", new { scriptId = scriptParsedEvent.ScriptId });
            var scriptSourceResponse = JsonConvert.DeserializeObject<V8CommandResponse<GetScriptSourceResponse>>(commandResult);
            Console.Out.WriteLine($"Debugging script: {scriptSourceResponse.Result.ScriptSource}");
        }
    }
}

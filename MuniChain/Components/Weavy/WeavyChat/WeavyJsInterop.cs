using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace UI.Components.Weavy
{
    public class WeavyJsInterop : IDisposable {
        private readonly IJSRuntime JS;
        private bool Initialized = false;
        private IJSObjectReference Bridge;
        private ValueTask<IJSObjectReference> WhenImport;

        // Constructor
        // This is a good place to inject any authentication service you may use to provide JWT tokens.
        public WeavyJsInterop(IJSRuntime js) {
            JS = js;
        }

        // Initialization of the JS Interop Module
        // The initialization is only done once even if you call it multiple times
        public async Task Init() {
            if (!Initialized) {
                Initialized = true;
                WhenImport = JS.InvokeAsync<IJSObjectReference>("import", "./weavyJsInterop.js");
                Bridge = await WhenImport;
            } else {
                await WhenImport;
            }
        }

        // Calling Javascript to create a new instance of Weavy via the JS Interop Module
        public async ValueTask<IJSObjectReference> Weavy(string url, string token, string timezone) {
            await Init();
            return await Bridge.InvokeAsync<IJSObjectReference>("weavy", url, token, timezone);
        }

        public void Dispose() {
            Bridge?.DisposeAsync();
        }
    }
}

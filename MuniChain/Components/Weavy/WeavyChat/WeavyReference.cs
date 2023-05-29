using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace UI.Components.Weavy
{
    //
    // Summary:
    //     Wrapped IJSObjectReference to the Weavy instance in Javascript.
    //     Adds .Space() and .Destroy() methods.
    public class WeavyReference : ExtendableJSObjectReference {
        private bool Initialized = false;
        public WeavyJsInterop WeavyService;
        public string url;
        public string token;
        public string timezone;
        public ValueTask<IJSObjectReference> WhenWeavy;

        public WeavyReference(WeavyJsInterop weavyService = null, string url = null, string token = null, string timezone = null, IJSObjectReference weavy = null) : base(weavy) {
            this.url = url;
            this.token = token;
            this.timezone = timezone;
            WeavyService = weavyService;
        }

        public async Task Init() {
            if(!Initialized) {
                Initialized = true;
                WhenWeavy = WeavyService.Weavy(url, token, timezone);
                ObjectReference = await WhenWeavy;
            } else {
                await WhenWeavy;
            }
        }

        public async ValueTask<AppReference> App(object appSelector = null)
        {
            await Init();
            return new(await ObjectReference.InvokeAsync<IJSObjectReference>("app", appSelector));
        }

        // Used for cleanup
        public async Task Destroy() {
            await ObjectReference.InvokeVoidAsync("destroy");
            await DisposeAsync();
        }
    }

    public class AppReference : ExtendableJSObjectReference {
        public AppReference(IJSObjectReference app) : base(app) { }

        public ValueTask Open() {
            return ObjectReference.InvokeVoidAsync("open");
        }

        public ValueTask Close() {
            return ObjectReference.InvokeVoidAsync("close");
        }

        public ValueTask Toggle() {
            return ObjectReference.InvokeVoidAsync("toggle");
        }

        // Used for cleanup
        public async Task Remove() {
            await ObjectReference.InvokeVoidAsync("remove");
            await DisposeAsync();
        }
    }
}
using Microsoft.JSInterop;
using System.Threading;
using System.Threading.Tasks;

namespace UI.Components.Weavy {
    //
    // Summary:
    //     Wrapper around a IJSObjectReference to enable extending
    public class ExtendableJSObjectReference : IJSObjectReference {
        public IJSObjectReference ObjectReference;

        // Constructed using another IJSObjectReference
        // Possibility to delay ObjectReference assignment
        public ExtendableJSObjectReference(IJSObjectReference objectReference = null) {
            ObjectReference = objectReference;
        }

        // IMPLEMENT DEFAULT
        public ValueTask DisposeAsync() {
            return ObjectReference.DisposeAsync();
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args) {
            return ObjectReference.InvokeAsync<TValue>(identifier, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args) {
            return ObjectReference.InvokeAsync<TValue>(identifier, cancellationToken, args);
        }
    }
}

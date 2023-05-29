using BoldSign.Model;

namespace Shared.Models.DealComponents
{
    public class MunichainDocumentSigner
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool? IsParticipant { get; set; }
        public SignerType SignerType { get; set; }
    }
}

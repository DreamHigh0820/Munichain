using Shared.Models.DealComponents;
using Shared.Models.Enums;
using Shared.Models.Users;

namespace Shared.Validators
{
    public static class DocumentPermissionValidator
    {
        public static bool Validate(Document doc, List<DealParticipant> dealParticipants, User user, bool RequestingPublicView, bool HasBeenPublished)
        {
            if (RequestingPublicView)
            {
                // If deal hasnt been published yet and the document is public, you cant see it
                if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Public)
                {
                    return HasBeenPublished;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // If deal hasnt been published yet and the document is public, you cant see it
                if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Public)
                {
                    return true;
                }
                else if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Private)
                {
                    return doc.CreatedBy == user.Email;
                }
                else if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Participants && dealParticipants.Select(x => x.UserId).Contains(user.Id))
                {
                    return true;
                }
                // Otherwise it is certain participants only
                else if (doc.PublicDocumentViewSettings == PublicDocumentViewSettings.Custom)
                {
                    if (doc.CreatedBy == user.Email)
                    {
                        return true;
                    }

                    if (doc.UserPermissions != null && doc.UserPermissions.Any())
                    {
                        if (doc.UserPermissions.Contains(user.Email))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

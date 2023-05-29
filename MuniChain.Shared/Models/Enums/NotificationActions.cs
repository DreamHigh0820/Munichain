namespace Shared.Models.Enums
{
    public enum NotificationAction
    {
        // Documents
        Uploaded,
        Opened,
        Saved,
        Deleted,
        DocumentParticipantAdded,
        DocumentParticipantRemoved,
        DocumentCommentedOn,
        DocumentVisibilityChangedToParticipants,
        DocumentVisibilityChangedToPublic,
        DocumentVisibilityChanged,

        DocumentAnnotationAdded,
        DocumentAnnotationChanged,
        DocumentAnnotationCommentAdded,
            // Boldsign
            Sent,
            Signed,
            Completed,
            Declined,
            Revoked,
            Reassigned,
            Expired,

        // Message Board
        MessageBoardComment,

        // Deal
        DealArchived,
        DealMadePublic,
        DealUpdated,
        DealUpdatedMultiple,
        DealUpdatedAudit,
        DealPartAdded,
        DealPartRemoved,

        // Participants
        ParticipantAdd,
        ParticipantRemove,
        ParticipantRoleUpdated,
        ParticipantPermissionUpdate,
        ParticipantAddedAsAdmin,
        ParticipantPublic,
        ParticipantPrivate,
        ParticipantModified,

        // Expenditures
        ExpenditureAdded,
        ExpenditureAddedAudit,
        ExpenditureChanged,
        ExpenditureChangedAudit,
        ExpenditureMultipleChanges,

        // Performance
        PerformanceAdded,
        PerformanceUpdated,
        PerformanceTopAccountAdded,
        PerformanceTopAccountChanged,
        PerformanceTopAccountMarginAmountChanged,
        PerformanceTopAccountMarginDateChanged,


        Shared,
        Commented,

        // Firm
        FirmMemberAddedToFirm,
    }
}

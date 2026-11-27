namespace MauiNotes.Controls.AI;

internal enum AiPopupCloseAction
{
    KeepAiSession,
    ShowReview,
    CloseAiSession
}

internal static class AiPopupClosePolicy
{
    internal static AiPopupCloseAction Decide(
        bool actionSelectionPending,
        bool isProcessing,
        bool hasError,
        bool hasResult,
        bool isAiPopupOpen)
    {
        if (isProcessing || hasError || hasResult)
            return AiPopupCloseAction.ShowReview;

        if (actionSelectionPending)
            return AiPopupCloseAction.KeepAiSession;

        return isAiPopupOpen
            ? AiPopupCloseAction.CloseAiSession
            : AiPopupCloseAction.KeepAiSession;
    }
}

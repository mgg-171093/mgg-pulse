using MGG.Pulse.Application.Updates;

namespace MGG.Pulse.UI.Updates;

public enum UpdatePromptDecision
{
    Update,
    Cancel,
    Unavailable
}

public sealed class UpdateApplyCoordinator
{
    private readonly Func<UpdateCheckResult, Task<UpdatePromptDecision>> _promptAsync;
    private readonly Func<UpdateCheckResult, Task<bool>> _applyAsync;
    private readonly Action<UpdateCheckResult> _notifyDeferred;

    public UpdateApplyCoordinator(
        Func<UpdateCheckResult, Task<UpdatePromptDecision>> promptAsync,
        Func<UpdateCheckResult, Task<bool>> applyAsync,
        Action<UpdateCheckResult> notifyDeferred)
    {
        _promptAsync = promptAsync ?? throw new ArgumentNullException(nameof(promptAsync));
        _applyAsync = applyAsync ?? throw new ArgumentNullException(nameof(applyAsync));
        _notifyDeferred = notifyDeferred ?? throw new ArgumentNullException(nameof(notifyDeferred));
    }

    public async Task<bool> TryApplyAvailableUpdateAsync(
        UpdateCheckResult result,
        bool showPrompt,
        bool notifyWhenDeferred)
    {
        if (!ApplyUpdateUseCase.CanApply(result))
        {
            if (notifyWhenDeferred)
            {
                _notifyDeferred(result);
            }

            return false;
        }

        if (showPrompt)
        {
            var decision = await _promptAsync(result);
            if (decision != UpdatePromptDecision.Update)
            {
                if (notifyWhenDeferred)
                {
                    _notifyDeferred(result);
                }

                return false;
            }
        }

        var started = await _applyAsync(result);
        if (!started && notifyWhenDeferred)
        {
            _notifyDeferred(result);
        }

        return started;
    }
}

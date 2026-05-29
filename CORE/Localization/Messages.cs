namespace CORE.Localization;

/// <summary>
///     Localized message keys. Each value resolves to a translated string via the
///     <c>Translate()</c> extension and the matching <c>MsgResource.{culture}.resx</c> file.
/// </summary>
public enum Messages
{
    InvalidModel,
    GeneralError,
    UserIsExist,
    UserIsNotExist,
    PermissionDenied,
    Success,
    InvalidUserCredentials,
    PasswordResetted,
    VerificationCodeSent,
    InvalidVerificationCode,
    CanNotFoundUserIdInYourAccessToken,
    DataNotFound,
    FileIsLargeThan2Mb,
    FileIsNotFound,
    ThisFileTypeIsNotAllowed
}
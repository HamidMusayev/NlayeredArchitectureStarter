using System.ComponentModel;

namespace ENTITIES.Enums;

/// <summary>
///     Coarse user classification (admin / regular / guest). Distinct from <c>Role</c>:
///     <c>Role</c> drives authorization, <c>UserType</c> is a stable bucket used for UI
///     gating and reports. Descriptions are localized to Azerbaijani.
/// </summary>
public enum UserType
{
    [Description("Baş inzibatçı")] SuperAdmin = 1,
    [Description("İnzibatçı")] Admin = 2,
    [Description("İstifadəçi")] User = 3,
    [Description("Qonaq")] Guest = 4
}
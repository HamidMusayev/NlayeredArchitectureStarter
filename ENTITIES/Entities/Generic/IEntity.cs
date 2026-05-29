namespace ENTITIES.Entities.Generic;

/// <summary>
///     Marker interface every domain entity implements. Used by Scrutor scans and EF reflection
///     to identify aggregate roots without enumerating concrete types.
/// </summary>
public interface IEntity
{
}
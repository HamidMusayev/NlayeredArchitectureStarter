namespace STORAGE;

/// <summary>
///     Marker record used solely to locate the <c>STORAGE</c> assembly at startup. Pass
///     <c>typeof(BlobStorageAssemblyContainer)</c> to any reflection-based registration that
///     needs to scan this project.
/// </summary>
public record StorageAssemblyContainer;
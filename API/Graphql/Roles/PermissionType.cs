using HotChocolate.Types;

namespace API.Graphql.Roles;

/// <summary>
/// GraphQL schema for <see cref="ENTITIES.Entities.Permission"/>.
/// Hides audit columns; only exposes the identifying / business fields.
/// </summary>
public class PermissionType : ObjectType<ENTITIES.Entities.Permission>
{
    protected override void Configure(IObjectTypeDescriptor<ENTITIES.Entities.Permission> descriptor)
    {
        descriptor.Name("Permission");

        descriptor.Field(p => p.Id);
        descriptor.Field(p => p.Name);
        descriptor.Field(p => p.Key);

        descriptor.Ignore(p => p.Roles);
        descriptor.Ignore(p => p.CreatedAt);
        descriptor.Ignore(p => p.CreatedById);
        descriptor.Ignore(p => p.ModifiedAt);
        descriptor.Ignore(p => p.ModifiedBy);
        descriptor.Ignore(p => p.DeletedAt);
        descriptor.Ignore(p => p.DeletedBy);
        descriptor.Ignore(p => p.IsDeleted);
    }
}

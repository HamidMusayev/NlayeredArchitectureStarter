using ENTITIES.Entities;

namespace GRAPHQL.Roles;

/// <summary>
///     GraphQL schema for <see cref="ENTITIES.Entities.Role" />.
///     Restricts the exposed surface so audit columns (IsDeleted, CreatedById, etc.)
///     do not leak into the API. The underlying IQueryable stays Role-typed so
///     HotChocolate's [UseProjection] still pushes selection down to SQL.
/// </summary>
public class RoleType : ObjectType<Role>
{
    protected override void Configure(IObjectTypeDescriptor<Role> descriptor)
    {
        descriptor.Name("Role");

        descriptor.Field(r => r.Id);
        descriptor.Field(r => r.Name);
        descriptor.Field(r => r.Key);
        descriptor.Field(r => r.Permissions).Type<ListType<PermissionType>>();

        descriptor.Ignore(r => r.CreatedAt);
        descriptor.Ignore(r => r.CreatedById);
        descriptor.Ignore(r => r.ModifiedAt);
        descriptor.Ignore(r => r.ModifiedBy);
        descriptor.Ignore(r => r.DeletedAt);
        descriptor.Ignore(r => r.DeletedBy);
        descriptor.Ignore(r => r.IsDeleted);
    }
}
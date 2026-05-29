using BLL.Abstract;
using DTO.Role;
using IResult = DTO.Responses.IResult;

namespace GRAPHQL.Roles;

/// <summary>
///     HotChocolate GraphQL mutation root for Role management. Delegates add and update
///     operations to <see cref="IRoleService" /> and returns the standard
///     <see cref="IResult" /> envelope.
/// </summary>
public class Mutation
{
    public Task<IResult> AddRole(RoleToAddDto item, [Service] IRoleService service)
    {
        return service.AddAsync(item);
    }

    public Task<IResult> UpdateRole(Guid id, RoleToUpdateDto item, [Service] IRoleService service)
    {
        return service.UpdateAsync(id, item);
    }
}
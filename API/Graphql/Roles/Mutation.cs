using BLL.Abstract;
using DTO.Role;
using IResult = DTO.Responses.IResult;

namespace API.Graphql.Roles;

public class Mutation
{
    public Task<IResult> AddRole(RoleToAddDto item, [Service] IRoleService service)
        => service.AddAsync(item);

    public Task<IResult> UpdateRole(Guid id, RoleToUpdateDto item, [Service] IRoleService service)
        => service.UpdateAsync(id, item);
}

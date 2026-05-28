using DAL.EntityFramework.Abstract;

namespace API.Graphql.Roles;

public class Query
{
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IQueryable<ENTITIES.Entities.Role> GetRoles(
        [Service] IRoleRepository roleRepository)
        => roleRepository.GetList();
}

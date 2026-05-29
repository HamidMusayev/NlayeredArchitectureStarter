using DAL.EntityFramework.Abstract;
using ENTITIES.Entities;

namespace GRAPHQL.Roles;

/// <summary>
///     HotChocolate GraphQL query root for Role data. Exposes a server-side filterable,
///     sortable, and projectable <see cref="IQueryable{T}" /> over Role entities so clients
///     can request exactly the fields and subsets they need.
/// </summary>
public class Query
{
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IQueryable<Role> GetRoles(
        [Service] IRoleRepository roleRepository)
    {
        return roleRepository.GetList();
    }
}
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Concrete;

public class OrganizationRepository(DataContext dataContext)
    : GenericRepository<Organization>(dataContext), IOrganizationRepository
{
    private readonly DataContext _dataContext = dataContext;
}
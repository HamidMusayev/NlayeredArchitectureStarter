using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using ENTITIES.Entities;

namespace DAL.EntityFramework.Concrete;

public class PermissionRepository(DataContext dataContext)
    : GenericRepository<Permission>(dataContext), IPermissionRepository
{
    private readonly DataContext _dataContext = dataContext;
}
using DAL.EntityFramework.Abstract;
using DAL.EntityFramework.Context;
using DAL.EntityFramework.GenericRepository;
using File = ENTITIES.Entities.File;

namespace DAL.EntityFramework.Concrete;

public class FileRepository(DataContext dataContext) : GenericRepository<File>(dataContext), IFileRepository
{
    private readonly DataContext _dataContext = dataContext;
}
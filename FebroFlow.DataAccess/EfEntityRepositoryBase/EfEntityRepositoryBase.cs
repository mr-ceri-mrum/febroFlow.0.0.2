using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace FebroFlow.DataAccess.EfEntityRepositoryBase;

public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
    where TEntity : class, IEntity<Guid>, new()
    where TContext : DbContext, new()
{
    #region DI
    private readonly TContext _context;

    public EfEntityRepositoryBase(TContext context)
    {
        _context = context;
    }
    #endregion
    
    public IQueryable<TEntity> GetAllAsQueryable(
        int pageNumber, int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        query = query.AsNoTracking();
        query = includes.Aggregate(query, (current, include) => 
            current.Include(include));
        
        if (filter != null)
        {
            query = query.Where(filter);    
        }
        
        query = query.Where(x => !x.IsDeleted);
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        return query.AsQueryable();
    }

    public async Task<IQueryable<TEntity>> GetAllAsQueryable(Expression<Func<TEntity, bool>>? filter = null, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        query = query.AsNoTracking();
        query = includes.Aggregate(query, (current, include) => 
            current.Include(include));
        
        if (filter != null)
        {
            query = query.Where(filter);
        }
        
        query = query.Where(x => !x.IsDeleted).AsQueryable();
        return await Task.FromResult(query);
    }

    public List<TEntity> GetAll(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? _context.Set<TEntity>().Where(x => !x.IsDeleted).ToList()
            : _context.Set<TEntity>().Where(filter).Where(x => !x.IsDeleted).ToList();
    }

    public TEntity? Get(Expression<Func<TEntity, bool>> filter)
    {
        return _context.Set<TEntity>().Where(x => !x.IsDeleted).FirstOrDefault(filter);
    }
    
    public bool Any(Expression<Func<TEntity, bool>> filter) => _context.Set<TEntity>().Where(x => !x.IsDeleted).Any(filter);

    public void Add(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Added;
        _context.SaveChanges();
    }

    public void Update(TEntity entity)
    {
        entity.ModifiedDate = DateTime.Now;
        _context.Entry(entity).State = EntityState.Modified;
        _context.SaveChanges();
    }
    
    public void Delete(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.ModifiedDate = DateTime.Now;
        _context.Entry(entity).State = EntityState.Modified;
        _context.SaveChanges();
    }
    
    public int Count(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? _context.Set<TEntity>().Count(x => !x.IsDeleted)
            : _context.Set<TEntity>().Where(filter).Count(x => !x.IsDeleted);
    }
    
    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? await _context.Set<TEntity>().Where(x => !x.IsDeleted).OrderByDescending(o => o.Id).ToListAsync()
            : await _context.Set<TEntity>().Where(filter).Where(x => !x.IsDeleted).OrderByDescending(o => o.Id).ToListAsync();
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter)
        => await _context.Set<TEntity>().Where(x => !x.IsDeleted).FirstOrDefaultAsync(filter);

    public async Task<Guid?> GetId(Expression<Func<TEntity, bool>> filter)
    {
        return await _context.Set<TEntity>()
            .Where(filter)
            .Where(x => !x.IsDeleted)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> filter,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>().Where(x => !x.IsDeleted);
            
        // Apply all includes
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        
        return await query.FirstOrDefaultAsync(filter);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter)
        => await _context.Set<TEntity>().Where(x => !x.IsDeleted).AnyAsync(filter);

    public async Task AddAsync(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Added;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        entity.ModifiedDate = DateTime.Now;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    #region Delete

    public async Task DeleteAsync(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.ModifiedDate = DateTime.Now;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(List<TEntity> entities)
    {
        entities.ForEach(x => {
            x.IsDeleted = true;
            x.ModifiedDate = DateTime.Now;
        });
        _context.UpdateRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        if (filter != null)
        {
            List<TEntity> deleteEntities = await _context.Set<TEntity>().Where(filter).ToListAsync();
            deleteEntities.ForEach(x => {
                x.IsDeleted = true;
                x.ModifiedDate = DateTime.Now;
            });
            _context.UpdateRange(deleteEntities);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        return filter == null
            ? _context.Set<TEntity>().CountAsync(x => !x.IsDeleted)
            : _context.Set<TEntity>().Where(filter).CountAsync(x => !x.IsDeleted);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _context.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }
}
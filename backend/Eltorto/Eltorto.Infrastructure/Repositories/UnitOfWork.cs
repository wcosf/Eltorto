using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Infrastructure.Data;
using Eltorto.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Eltorto.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public ICategoryRepository Categories { get; }
    public ICakeRepository Cakes { get; }
    public IFillingRepository Fillings { get; }
    public ITestimonialRepository Testimonials { get; }
    public IPageRepository Pages { get; }
    public IOrderRepository Orders { get; }
    public ISliderRepository Sliders { get; }
    public IContentBlockRepository ContentBlocks { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(context);
        Cakes = new CakeRepository(context);
        Fillings = new FillingRepository(context);
        Testimonials = new TestimonialRepository(context);
        Pages = new PageRepository(context);
        Orders = new OrderRepository(context);
        Sliders = new SliderRepository(context);
        ContentBlocks = new ContentBlockRepository(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public DbSet<T> Set<T>() where T : class
    {
        return _context.Set<T>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }
}
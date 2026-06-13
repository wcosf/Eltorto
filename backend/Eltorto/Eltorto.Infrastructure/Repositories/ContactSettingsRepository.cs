using Eltorto.Domain.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;

namespace Eltorto.Infrastructure.Repositories;

public class ContactSettingsRepository : Repository<ContactSettings>, IContactSettingsRepository
{
    public ContactSettingsRepository(AppDbContext context) : base(context) { }
}
using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Application.Services;

public class ContactSettingsService : IContactSettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ContactSettingsService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ContactSettingsDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.Set<ContactSettings>()
            .FirstOrDefaultAsync(cancellationToken);

        return settings != null ? _mapper.Map<ContactSettingsDto>(settings) : null;
    }

    public async Task<ContactSettingsDto> UpdateAsync(UpdateContactSettingsDto updateDto, CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.Set<ContactSettings>()
            .FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            settings = new ContactSettings();
            _mapper.Map(updateDto, settings);
            await _unitOfWork.Set<ContactSettings>().AddAsync(settings, cancellationToken);
        }
        else
        {
            _mapper.Map(updateDto, settings);
            _unitOfWork.Set<ContactSettings>().Update(settings);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ContactSettingsDto>(settings);
    }
}
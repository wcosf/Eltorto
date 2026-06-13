using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Domain.Abstractions;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

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
        var settings = (await _unitOfWork.ContactSettings.GetAllAsync(cancellationToken)).FirstOrDefault();

        return settings != null ? _mapper.Map<ContactSettingsDto>(settings) : null;
    }

    public async Task<ContactSettingsDto> UpdateAsync(UpdateContactSettingsDto updateDto, CancellationToken cancellationToken = default)
    {
        var settings = (await _unitOfWork.ContactSettings.GetAllAsync(cancellationToken)).FirstOrDefault();

        if (settings == null)
        {
            settings = new ContactSettings();
            _mapper.Map(updateDto, settings);
            await _unitOfWork.ContactSettings.AddAsync(settings, cancellationToken);
        }
        else
        {
            _mapper.Map(updateDto, settings);
            await _unitOfWork.ContactSettings.UpdateAsync(settings, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ContactSettingsDto>(settings);
    }
}
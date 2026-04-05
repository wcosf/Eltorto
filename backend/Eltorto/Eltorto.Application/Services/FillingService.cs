using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Services;

public class FillingService : IFillingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FillingService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<FillingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var filling = await _unitOfWork.Fillings.GetByIdAsync(id, cancellationToken);
        return filling != null ? _mapper.Map<FillingDto>(filling) : null;
    }

    public async Task<IReadOnlyList<FillingDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var fillings = await _unitOfWork.Fillings.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<FillingDto>>(fillings);
    }

    public async Task<IReadOnlyList<FillingDto>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var fillings = await _unitOfWork.Fillings.GetAvailableAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<FillingDto>>(fillings);
    }

    public async Task<FillingWithCakesDto> GetWithCakesAsync(int id, CancellationToken cancellationToken = default)
    {
        var filling = await _unitOfWork.Fillings.GetWithCakesAsync(id, cancellationToken);
        if (filling == null)
        {
            throw new KeyNotFoundException($"Filling with id {id} not found");
        }

        var fillingDto = _mapper.Map<FillingWithCakesDto>(filling);
        fillingDto.Cakes = _mapper.Map<List<CakeListDto>>(filling.Cakes);

        return fillingDto;
    }

    public async Task<FillingDto> CreateAsync(CreateFillingDto createDto, CancellationToken cancellationToken = default)
    {
        var filling = _mapper.Map<Filling>(createDto);

        await _unitOfWork.Fillings.AddAsync(filling, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FillingDto>(filling);
    }

    public async Task<FillingDto> UpdateAsync(UpdateFillingDto updateDto, CancellationToken cancellationToken = default)
    {
        var existingFilling = await _unitOfWork.Fillings.GetByIdAsync(updateDto.Id, cancellationToken);
        if (existingFilling == null)
        {
            throw new KeyNotFoundException($"Filling with id {updateDto.Id} not found");
        }

        _mapper.Map(updateDto, existingFilling);
        await _unitOfWork.Fillings.UpdateAsync(existingFilling, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FillingDto>(existingFilling);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var filling = await _unitOfWork.Fillings.GetByIdAsync(id, cancellationToken);
        if (filling == null)
        {
            throw new KeyNotFoundException($"Filling with id {id} not found");
        }

        var cakesCount = await _unitOfWork.Cakes.CountAsync(c => c.FillingId == id, cancellationToken);
        if (cakesCount > 0)
        {
            throw new InvalidOperationException($"Cannot delete filling that is used in {cakesCount} cakes. Remove or reassign cakes first.");
        }

        await _unitOfWork.Fillings.DeleteAsync(filling, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
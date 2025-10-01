using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Services
{
    public class ComensalService : IComensalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ComensalService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<ComensalDto>> GetComensalesAsync(string? search, int page, int pageSize)
        {
            var (comensales, totalCount) = await _unitOfWork.Comensales.GetPagedAsync(search, page, pageSize);
            var comensalesDto = _mapper.Map<IEnumerable<ComensalDto>>(comensales);
            return new PagedResultDto<ComensalDto>
            {
                Items = comensalesDto,
                TotalCount = totalCount
            };
        }

        public async Task<ComensalDto> GetComensalByIdAsync(int id)
        {
            var comensal = await _unitOfWork.Comensales.GetByIdAsync(id);
            return _mapper.Map<ComensalDto>(comensal);
        }

        public async Task<ComensalDto> CreateComensalAsync(ComensalCreateDto createDto)
        {
            if (!string.IsNullOrEmpty(createDto.Identificacion)){
                var existingByIdentificacion = await _unitOfWork.Comensales.GetByIdentificacionAsync(createDto.Identificacion);
                if (existingByIdentificacion != null)
                {
                    throw new InvalidOperationException($"Ya existe un comensal con la identificación '{createDto.Identificacion}'.");
                }
            }

            var comensal = _mapper.Map<Comensal>(createDto);
            if(comensal.Activo == null) comensal.Activo = true;

            await _unitOfWork.Comensales.AddAsync(comensal);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<ComensalDto>(comensal);
        }

        public async Task<bool> UpdateComensalAsync(int id, ComensalCreateDto updateDto)
        {
            var existingComensal = await _unitOfWork.Comensales.GetByIdAsync(id);
            if (existingComensal == null) return false;

            if (!string.IsNullOrEmpty(updateDto.Identificacion)){
                var existingByIdentificacion = await _unitOfWork.Comensales.GetByIdentificacionAsync(updateDto.Identificacion);
                if (existingByIdentificacion != null && existingByIdentificacion.Id != id)
                {
                    throw new InvalidOperationException($"La identificación '{updateDto.Identificacion}' ya está en uso por otro comensal.");
                }
            }

            _mapper.Map(updateDto, existingComensal);
            _unitOfWork.Comensales.Update(existingComensal);
            var result = await _unitOfWork.SaveAsync();

            return result > 0;
        }

        public async Task<bool> InactivateComensalAsync(int id)
        {
            var comensal = await _unitOfWork.Comensales.GetByIdAsync(id);
            if (comensal == null) return false;

            comensal.Activo = false;
            _unitOfWork.Comensales.Update(comensal);
            var result = await _unitOfWork.SaveAsync();

            return result > 0;
        }

        public async Task<IEnumerable<ComensalDto>> GetAllActiveComensalesAsync()
        {
            var comensales = await _unitOfWork.Comensales.GetAllActiveAsync();
            return _mapper.Map<IEnumerable<ComensalDto>>(comensales);
        }
    }
}

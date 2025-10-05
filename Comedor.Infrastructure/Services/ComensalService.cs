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

        public async Task<ComensalCreateDto> GetComensalCreateDtoByIdAsync(int id)
        {
            var comensal = await _unitOfWork.Comensales.GetByIdAsync(id);
            return _mapper.Map<ComensalCreateDto>(comensal);
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

        public async Task<ComensalDto?> UpsertComensalAsync(ComensalCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Identificacion))
                throw new InvalidOperationException("La identificación es obligatoria.");

            var existingComensal = await _unitOfWork.Comensales.GetByIdentificacionAsync(dto.Identificacion);

            if (existingComensal == null)
            {
                // Crear nuevo comensal
                var comensal = _mapper.Map<Comensal>(dto);
                if (comensal.Activo == null) comensal.Activo = true;

                await _unitOfWork.Comensales.AddAsync(comensal);
                await _unitOfWork.SaveAsync();

                return _mapper.Map<ComensalDto>(comensal);
            }
            else
            {
                // Actualizar comensal existente
                _mapper.Map(dto, existingComensal);
                _unitOfWork.Comensales.Update(existingComensal);
                var result = await _unitOfWork.SaveAsync();

                return result > 0 ? _mapper.Map<ComensalDto>(existingComensal) : null;
            }
        }
    }
}

using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TurnoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TurnoDto>> GetAllTurnosAsync()
        {
            var turnos = await _unitOfWork.Turnos.GetAllAsync();
            return _mapper.Map<IEnumerable<TurnoDto>>(turnos);
        }
    }
}

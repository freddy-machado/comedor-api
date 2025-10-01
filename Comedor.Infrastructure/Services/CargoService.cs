using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Services;

public class CargoService : ICargoService
{
    private readonly ComedorDbContext _context;
    private readonly IMapper _mapper;

    public CargoService(ComedorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CargoDto>> GetAllCargosAsync()
    {
        var cargos = await _context.Set<CargoView>()
            .FromSqlRaw("SELECT id, cargo FROM vW_Cargos")
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<CargoDto>>(cargos);
    }
}

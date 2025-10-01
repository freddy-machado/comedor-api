using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Services;

public class AreaService : IAreaService
{
    private readonly ComedorDbContext _context;
    private readonly IMapper _mapper;

    public AreaService(ComedorDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AreaDto>> GetAllAreasAsync()
    {
        var areas = await _context.Set<AreaView>()
            .FromSqlRaw("SELECT id, area FROM vW_Areas")
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<AreaDto>>(areas);
    }
}

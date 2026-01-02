using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Dtos.Report;
using Comedor.Core.Entities.Custom;
using Comedor.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Services
{
    public class ReportesService: IReportesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReportesService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<DespachoReportDto>> GetDespachosReportAsync(DateTime fechaInicio, DateTime fechaFin,
            [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var dtos = await _unitOfWork.Reportes.GetDespachosReportAsync(fechaInicio, fechaFin, search, page, pageSize);

            var resultado = _mapper.Map<IEnumerable<DespachoReportDto>>(dtos.lista);

            return new PagedResultDto<DespachoReportDto>
            {
                Items = resultado,
                TotalCount = dtos.TotalCount
            };
        }
    }
}

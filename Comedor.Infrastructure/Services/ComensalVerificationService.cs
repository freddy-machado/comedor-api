using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Comedor.Infrastructure.Services;

public class ComensalVerificationService : IComensalVerificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ComedorDbContext _context; // For FromSqlRaw
    private readonly IDespachoService _despachoService; // Added

    public ComensalVerificationService(IUnitOfWork unitOfWork, IMapper mapper, ComedorDbContext context, IDespachoService despachoService) // Added IDespachoService
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
        _despachoService = despachoService; // Assigned
    }

    public async Task<(ComensalDto? comensal, string? errorMessage)> VerifyAndRegisterComensalAsync(string identificacion)
    {
        // 1. Search in local CM_Comensales table
        var comensal = (await _unitOfWork.Comensales.FindAsync(c => c.Identificacion.Replace("-", "").Replace("'", "") == identificacion.Replace("-", "").Replace("'", "") && c.Activo == true)).FirstOrDefault();
        if (comensal != null)
        {
            // --- NEW VALIDATION ---
            var currentTime = DateTime.Now.TimeOfDay;
            var allTurnos = await _unitOfWork.Turnos.GetAllAsync();
            var currentTurno = allTurnos.FirstOrDefault(t =>
            {
                if (TimeSpan.TryParse(t.Desde, out TimeSpan desdeTime) && TimeSpan.TryParse(t.Hasta, out TimeSpan hastaTime))
                {
                    return (desdeTime <= hastaTime)
                        ? (currentTime >= desdeTime && currentTime <= hastaTime)
                        : (currentTime >= desdeTime || currentTime <= hastaTime);
                }
                return false;
            });

            if (currentTurno != null)
            {
                var today = DateTime.Now.Date;
                var existingDespacho = (await _unitOfWork.Despachos.FindAsync(d =>
                    d.IdComensal == comensal.Id &&
                    d.IdTurno == currentTurno.Id &&
                    d.Fecha.HasValue && d.Fecha.Value.Date == today &&
                    d.Activo == true
                )).FirstOrDefault();

                if (existingDespacho != null)
                {
                    return (null, "El colaborador ya tiene un registro para el turno y día actual.");
                }
            }
            // --- END OF VALIDATION ---

            // Comensal found in CM_Comensales, create despacho record
            await _despachoService.CreateDespachoFromVerificationAsync(comensal.Id, identificacion); // Call new method
            return (_mapper.Map<ComensalDto>(comensal), null);
        }

        // 2. If not found, search in Reservations View
        var colaboradorView = await _context.Set<ColaboradorView>()
            .FromSqlRaw("SELECT identificacion, nombre, area, centrocosto, observacion FROM vW_ColaboradoresReservaciones WHERE identificacion = {0}", identificacion)
            .FirstOrDefaultAsync();

        // 3. If not found, search in Payroll View
        if (colaboradorView == null)
        {
            colaboradorView = await _context.Set<ColaboradorView>()
                .FromSqlRaw("SELECT identificacion, nombre, area, centrocosto, observacion FROM vW_ColaboradoresNomina WHERE identificacion = {0}", identificacion)
                .FirstOrDefaultAsync();
        }

        // 4. If not found anywhere, return null
        if (colaboradorView == null)
        {
            return (null, "Colaborador no encontrado.");
        }

        // 5. Found in a view, so create a new Comensal and save it
        var newComensal = new Comensal
        {
            Identificacion = colaboradorView.Identificacion,
            NombreColaborador = colaboradorView.Nombre,
            Area = colaboradorView.Area,
            CentroCosto = colaboradorView.CentroCosto,
            Observacion = colaboradorView.Observacion,
            Activo = true, // Default to active
            FechaDesde = System.DateTime.UtcNow // Default to now
        };

        await _unitOfWork.Comensales.AddAsync(newComensal);
        await _unitOfWork.SaveAsync();

        // After new comensal is registered, create despacho record
        await _despachoService.CreateDespachoFromVerificationAsync(newComensal.Id, newComensal.Identificacion); // Call new method

        return (_mapper.Map<ComensalDto>(newComensal), null);
    }
}

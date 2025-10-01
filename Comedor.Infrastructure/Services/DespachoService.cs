using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Core.Entities;
using Comedor.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Comedor.Core.Enums;
using Microsoft.AspNetCore.SignalR;
using Comedor.Infrastructure.Hubs;

namespace Comedor.Infrastructure.Services;

public class DespachoService : IDespachoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHubContext<DispatchHub> _hubContext;

    public DespachoService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<DispatchHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<DespachoDto>> GetAllDespachosAsync()
    {
        var despachos = await _unitOfWork.Despachos.GetAllDespachosWithDetailsAsync();
        return _mapper.Map<IEnumerable<DespachoDto>>(despachos);
    }

    public async Task<PagedResultDto<DespachoDto>> GetDespachosPaginadosAsync(string? search, int page, int pageSize)
    {
        var (despachos, totalCount) = await _unitOfWork.Despachos.GetPagedAsync(search, page, pageSize);
        var despachosDto = _mapper.Map<IEnumerable<DespachoDto>>(despachos);
        return new PagedResultDto<DespachoDto>
        {
            Items = despachosDto,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<DespachoNotificationDto>> GetDespachosPendientesAsync()
    {
        var despachos = await _unitOfWork.Despachos.GetPendientesAsync();

        // Manual mapping to DespachoNotificationDto
        var notificationDtos = despachos.Select(despacho => new DespachoNotificationDto
        {
            Id = despacho.Id,
            Identificacion = despacho.Identificacion ?? string.Empty,
            NombreColaborador = despacho.Comensal?.NombreColaborador ?? "N/A",
            TurnoTiempo = $"{despacho.Turno?.Desde ?? "N/A"} - {despacho.Turno?.Hasta ?? "N/A"}",
            FechaRegistro = despacho.Fecha ?? default,
            HoraDespacho = despacho.HoraDespacho ?? string.Empty,
            Despachado = despacho.Despachado
        });

        return notificationDtos;
    }

    public async Task<DespachoDto?> GetDespachoByIdAsync(int id)
    {
        var despacho = await _unitOfWork.Despachos.GetByIdAsync(id);
        return _mapper.Map<DespachoDto>(despacho);
    }

    public async Task<DespachoDto> CreateDespachoAsync(CreateDespachoDto createDto)
    {
        var turno = await _unitOfWork.Turnos.GetByIdAsync(createDto.IdTurno);
        if (turno == null)
        {
            throw new KeyNotFoundException($"Turno with ID {createDto.IdTurno} not found.");
        }

        var comensal = await _unitOfWork.Comensales.GetByIdAsync(createDto.IdComensal);
        if (comensal == null)
        {
            throw new KeyNotFoundException($"Comensal with ID {createDto.IdComensal} not found.");
        }

        var despacho = _mapper.Map<Despacho>(createDto);
        await _unitOfWork.Despachos.AddAsync(despacho);
        await _unitOfWork.SaveAsync();

        var despachoDto = _mapper.Map<DespachoDto>(despacho);

        if (createDto.Origin == DespachoOrigin.Verification)
        {
            var comensalEntity = await _unitOfWork.Comensales.GetByIdAsync(despacho.IdComensal);
            var turnoEntity = await _unitOfWork.Turnos.GetByIdAsync(despacho.IdTurno);

            var notification = new DespachoNotificationDto
            {
                Id = despacho.Id,
                Identificacion = despacho.Identificacion,
                NombreColaborador = comensalEntity?.NombreColaborador ?? "N/A",
                TurnoTiempo = $"{turnoEntity?.Desde ?? ""} - {turnoEntity?.Hasta ?? ""}",
                FechaRegistro = despacho.Fecha ?? DateTime.MinValue,
                HoraDespacho = despacho.HoraDespacho ?? ""
            };
            await _hubContext.Clients.All.SendAsync("ReceiveDespachoNotification", notification);
        }

        return despachoDto;
    }

    public async Task<DespachoDto?> UpdateDespachoAsync(UpdateDespachoDto updateDto)
    {
        var existingDespacho = await _unitOfWork.Despachos.GetByIdAsync(updateDto.Id);
        if (existingDespacho == null)
        {
            return null;
        }

        if (existingDespacho.IdTurno != updateDto.IdTurno)
        {
            var turno = await _unitOfWork.Turnos.GetByIdAsync(updateDto.IdTurno);
            if (turno == null)
            {
                throw new KeyNotFoundException($"Turno with ID {updateDto.IdTurno} not found.");
            }
        }

        if (existingDespacho.IdComensal != updateDto.IdComensal)
        {
            var comensal = await _unitOfWork.Comensales.GetByIdAsync(updateDto.IdComensal); // Corrected typo
            if (comensal == null)
            {
                throw new KeyNotFoundException($"Comensal with ID {updateDto.IdComensal} not found.");
            }
        }

        _mapper.Map(updateDto, existingDespacho);
        _unitOfWork.Despachos.Update(existingDespacho);
        await _unitOfWork.SaveAsync();

        return _mapper.Map<DespachoDto>(existingDespacho);
    }

    public async Task<bool> AnularDespachoAsync(AnularDespachoDto anularDto)
    {
        var despacho = await _unitOfWork.Despachos.GetByIdAsync(anularDto.Id);
        if (despacho == null)
        {
            return false;
        }

        despacho.Activo = false;
        _unitOfWork.Despachos.Update(despacho);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteDespachoAsync(int id)
    {
        var despacho = await _unitOfWork.Despachos.GetByIdAsync(id);
        if (despacho == null)
        {
            return false;
        }

        _unitOfWork.Despachos.Remove(despacho);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<DespachoDto?> UpdateDespachoStatusAsync(UpdateDespachoStatusDto updateStatusDto)
    {
        var despacho = await _unitOfWork.Despachos.GetByIdAsync(updateStatusDto.Id);
        if (despacho == null)
        {
            return null;
        }

        despacho.Despachado = true;
        despacho.HoraDespacho = DateTime.Now.ToString("hh:mm:ss tt");

        _unitOfWork.Despachos.Update(despacho);
        await _unitOfWork.SaveAsync();

        return _mapper.Map<DespachoDto>(despacho);
    }

    public async Task<DespachoDto> CreateDespachoFromVerificationAsync(int idComensal, string identificacion)
    {
        var currentTime = DateTime.Now.TimeOfDay;
        var allTurnos = await _unitOfWork.Turnos.GetAllAsync();
        
        var currentTurno = allTurnos.FirstOrDefault(t =>
        {
            if (TimeSpan.TryParse(t.Desde, out TimeSpan desdeTime) &&
                TimeSpan.TryParse(t.Hasta, out TimeSpan hastaTime))
            {
                if (desdeTime <= hastaTime)
                {
                    return currentTime >= desdeTime && currentTime <= hastaTime;
                }
                else
                {
                    return currentTime >= desdeTime || currentTime <= hastaTime;
                }
            }
            return false;
        });

        if (currentTurno == null)
        {
            throw new InvalidOperationException("No active turno found for the current time.");
        }

        var despacho = new Despacho
        {
            IdTurno = currentTurno.Id,
            IdComensal = idComensal,
            Fecha = DateTime.Now,
            Identificacion = identificacion,
            Cantidad = 1,
            Activo = true,
            CostoUnitario = 1,
            Despachado = false,
            HoraDespacho = null,
            Origin = Comedor.Core.Enums.DespachoOrigin.Verification
        };

        await _unitOfWork.Despachos.AddAsync(despacho);
        await _unitOfWork.SaveAsync();

        var despachoDto = _mapper.Map<DespachoDto>(despacho);

        var comensalEntity = await _unitOfWork.Comensales.GetByIdAsync(despacho.IdComensal);
        var turnoEntity = await _unitOfWork.Turnos.GetByIdAsync(despacho.IdTurno);

        var notification = new DespachoNotificationDto
        {
            Id = despacho.Id,
            Identificacion = despacho.Identificacion,
            NombreColaborador = comensalEntity?.NombreColaborador ?? "N/A",
            TurnoTiempo = $"{turnoEntity?.Desde ?? ""} - {turnoEntity?.Hasta ?? ""}",
            FechaRegistro = despacho.Fecha ?? DateTime.MinValue,
            HoraDespacho = despacho.HoraDespacho ?? "",
            Despachado = despacho.Despachado // Added this line
        };
        await _hubContext.Clients.All.SendAsync("ReceiveDespachoNotification", notification);

        return despachoDto;
    }
}

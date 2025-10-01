using Xunit;
using Moq;
using Comedor.Core.Interfaces;
using Comedor.Infrastructure.Services;
using Comedor.Core.Entities;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using Comedor.Core.Dtos;
using Comedor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Comedor.UnitTests.Services
{
    public class ComensalVerificationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IDespachoService> _despachoServiceMock;
        private readonly ComensalVerificationService _service;

        public ComensalVerificationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _despachoServiceMock = new Mock<IDespachoService>();

            var options = new DbContextOptionsBuilder<ComedorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContextMock = new Mock<ComedorDbContext>(options);

            _service = new ComensalVerificationService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                dbContextMock.Object,
                _despachoServiceMock.Object
            );
        }

        private Turno GetCurrentTurno()
        {
            var now = DateTime.Now.TimeOfDay;
            if (now >= new TimeSpan(6, 0, 0) && now < new TimeSpan(14, 0, 0))
                return new Turno { Id = 1, Desde = "06:00", Hasta = "14:00" };
            if (now >= new TimeSpan(14, 0, 0) && now < new TimeSpan(22, 0, 0))
                return new Turno { Id = 2, Desde = "14:00", Hasta = "22:00" };
            return new Turno { Id = 3,  Desde = "22:00", Hasta = "06:00" };
        }

        [Fact]
        public async Task VerifyAndRegisterComensalAsync_ShouldReturnErrorMessage_WhenDespachoAlreadyExists()
        {
            // Arrange
            var identificacion = "12345";
            var comensal = new Comensal { Id = 1, Identificacion = identificacion, Activo = true };
            var currentTurno = GetCurrentTurno();
            var existingDespacho = new Despacho
            {
                IdComensal = comensal.Id,
                IdTurno = currentTurno.Id,
                Fecha = DateTime.Now.Date,
                Activo = true
            };

            _unitOfWorkMock.Setup(uow => uow.Comensales.FindAsync(It.IsAny<Expression<Func<Comensal, bool>>>()))
                           .ReturnsAsync(new List<Comensal> { comensal });

            _unitOfWorkMock.Setup(uow => uow.Turnos.GetAllAsync())
                           .ReturnsAsync(new List<Turno> {
                               new Turno { Id = 1, Desde = "06:00", Hasta = "14:00" },
                               new Turno { Id = 2, Desde = "14:00", Hasta = "22:00" },
                               new Turno { Id = 3, Desde = "22:00", Hasta = "06:00" }
                           });

            _unitOfWorkMock.Setup(uow => uow.Despachos.FindAsync(It.IsAny<Expression<Func<Despacho, bool>>>()))
                           .ReturnsAsync(new List<Despacho> { existingDespacho });

            // Act
            var (resultComensal, errorMessage) = await _service.VerifyAndRegisterComensalAsync(identificacion);

            // Assert
            Assert.Null(resultComensal);
            Assert.Equal("El colaborador ya tiene un registro para el turno y día actual.", errorMessage);
        }

        [Fact]
        public async Task VerifyAndRegisterComensalAsync_ShouldReturnComensal_WhenNoDespachoExists()
        {
            // Arrange
            var identificacion = "54321";
            var comensal = new Comensal { Id = 2, Identificacion = identificacion, Activo = true };
            var comensalDto = new ComensalDto { Id = 2, Identificacion = identificacion };
            var despachoDto = new DespachoDto { Id = 1, IdComensal = comensal.Id };

            _unitOfWorkMock.Setup(uow => uow.Comensales.FindAsync(It.IsAny<Expression<Func<Comensal, bool>>>()))
                           .ReturnsAsync(new List<Comensal> { comensal });

            _unitOfWorkMock.Setup(uow => uow.Turnos.GetAllAsync())
                           .ReturnsAsync(new List<Turno> {
                               new Turno { Id = 1, Desde = "06:00", Hasta = "14:00" },
                               new Turno { Id = 2, Desde = "14:00", Hasta = "22:00" },
                               new Turno { Id = 3, Desde = "22:00", Hasta = "06:00" }
                           });

            _unitOfWorkMock.Setup(uow => uow.Despachos.FindAsync(It.IsAny<Expression<Func<Despacho, bool>>>()))
                           .ReturnsAsync(new List<Despacho>()); // No existing despacho

            _despachoServiceMock.Setup(s => s.CreateDespachoFromVerificationAsync(comensal.Id, identificacion))
                                .ReturnsAsync(despachoDto);

            _mapperMock.Setup(m => m.Map<ComensalDto>(comensal)).Returns(comensalDto);

            // Act
            var (resultComensal, errorMessage) = await _service.VerifyAndRegisterComensalAsync(identificacion);

            // Assert
            Assert.NotNull(resultComensal);
            Assert.Equal(comensalDto.Id, resultComensal.Id);
            Assert.Null(errorMessage);
            _despachoServiceMock.Verify(s => s.CreateDespachoFromVerificationAsync(comensal.Id, identificacion), Times.Once);
        }
    }
}

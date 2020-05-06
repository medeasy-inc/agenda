﻿using Agenda.CQRS.Features.Appointments.Handlers;
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using Optional;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleGetOneAppointmentInfoByIdQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private HandleGetOneAppointmentInfoByIdQuery _sut;

        public HandleGetOneAppointmentInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            DbContextOptionsBuilder<AgendaContext> optionsBuilder = new DbContextOptionsBuilder<AgendaContext>();
            optionsBuilder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(optionsBuilder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _sut = new HandleGetOneAppointmentInfoByIdQuery(_uowFactory, _mapper);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Clear();
                uow.Repository<Appointment>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _uowFactory = null;
            _mapper = null;
            _sut = null;
        }

        [Fact]
        public async Task GivenEmptyDataStore_Handle_Returns_None()
        {
            // Arrange
            GetOneAppointmentInfoByIdQuery request = new GetOneAppointmentInfoByIdQuery(Guid.NewGuid());

            // Act
            Option<AppointmentInfo> optionalAppointment = await _sut.Handle(request, ct: default)
                .ConfigureAwait(false);

            // Assert
            optionalAppointment.HasValue.Should().BeFalse();
        }

        [Fact]
        public async Task GivenRecordExistsInDataStore_Get_Returns_Some()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();
            Appointment appointment = new Appointment
            (
                id: appointmentId,
                location : "Wayne Tower",
                subject :"Contengency",
                startDate : 1.April(2018).AddHours(15),
                endDate : 1.April(2018).AddHours(16)
            );
            Attendee bruce = new Attendee(id: Guid.NewGuid(), name: "Bruce Wayne");
            appointment.AddAttendee(bruce);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Appointment>().Create(appointment);

                await uow.SaveChangesAsync()
                         .ConfigureAwait(false);
            }

            GetOneAppointmentInfoByIdQuery request = new GetOneAppointmentInfoByIdQuery(appointmentId);

            // Act
            Option<AppointmentInfo> optionalAppointment = await _sut.Handle(request, default)
                                                                    .ConfigureAwait(false);

            // Assert
            optionalAppointment.HasValue.Should().BeTrue($"the record <{appointmentId}> exists in the datastore");
            optionalAppointment.MatchSome((appointmentInfo) =>
            {
                appointmentInfo.Id.Should().Be(appointment.Id);
                appointmentInfo.Location.Should().Be(appointment.Location);
                appointmentInfo.Subject.Should().Be(appointment.Subject);
                appointmentInfo.StartDate.Should().Be(appointment.StartDate);
                appointmentInfo.EndDate.Should().Be(appointment.EndDate);
                appointmentInfo.Attendees.Should()
                                         .HaveSameCount(appointment.Attendees);

                AttendeeInfo attendeeInfo = appointmentInfo.Attendees.ElementAt(0);
                attendeeInfo.Id.Should().Be(bruce.Id);
                attendeeInfo.Name.Should().Be(appointment.Attendees.ElementAt(0).Attendee.Name);
                attendeeInfo.UpdatedDate.Should()
                    .NotBe(DateTime.MinValue).And
                    .NotBe(DateTime.MaxValue);
            });
        }
    }
}
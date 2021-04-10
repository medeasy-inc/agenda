using Agenda.CQRS.Features.Appointments.Commands;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Appointments.Commmands
{
    [Feature("Agenda")]
    [UnitTest]
    public class AddAttendeeToAppointmentCommandTests
    {
        [Fact]
        public void Ctor_Is_Valid()
        {
            // Arrange
            Guid appointmentId = Guid.NewGuid();
            Guid participantId = Guid.NewGuid();

            // Act
            AddAttendeeToAppointmentCommand instance = new(data : (appointmentId, participantId));

            // Assert
            instance.Id.Should()
                .NotBeEmpty();
            instance.Data.appointmentId.Should()
                .Be(appointmentId);
            instance.Data.attendeeId.Should()
                .Be(participantId);
        }

        [Fact]
        public void Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new AddAttendeeToAppointmentCommand(default);

            // Assert
            action.Should()
                .Throw<ArgumentException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                {
                    Guid appointmentId = Guid.NewGuid();
                    Guid participantId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new AddAttendeeToAppointmentCommand((appointmentId, participantId)),
                        new AddAttendeeToAppointmentCommand((appointmentId, participantId)),
                        true,
                        $"two {nameof(AddAttendeeToAppointmentCommand)} commands with same data"
                    };
                }

                {
                    Guid appointmentId = Guid.NewGuid();
                    Guid participantId = Guid.NewGuid();

                    yield return new object[]
                    {
                        new AddAttendeeToAppointmentCommand((appointmentId, participantId)),
                        new AddAttendeeToAppointmentCommand((appointmentId, Guid.NewGuid())),
                        false,
                        $"two {nameof(AddAttendeeToAppointmentCommand)} commands with different {nameof(AddAttendeeToAppointmentCommand.Data.attendeeId)} data"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void AreEquals(AddAttendeeToAppointmentCommand first, object second, bool expectedResult, string reason)
        {
            // Act
            bool actualResult = first.Equals(second);

            // Assert
            actualResult.Should()
                .Be(expectedResult, reason);
        }
    }
}

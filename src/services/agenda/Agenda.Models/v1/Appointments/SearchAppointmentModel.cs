﻿using Agenda.Models.v1.Appointments;
using Agenda.Models.v1.Search;
using System;
using System.ComponentModel.DataAnnotations;

namespace Agenda.API.Resources.v1.Appointments
{
    /// <summary>
    /// Data to search <see cref="AppointmentModel"/>s
    /// </summary>
    public class SearchAppointmentModel : AbstractSearchModel<AppointmentModel>
    {
        /// <summary>
        /// Min start or end date
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTimeOffset? From { get; set; }

        /// <summary>
        /// Max start or end date
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTimeOffset? To { get; set; }

        /// <summary>
        /// Subject of the appointments
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Location of the appointment
        /// </summary>
        public string Location { get; set; }
    }
}
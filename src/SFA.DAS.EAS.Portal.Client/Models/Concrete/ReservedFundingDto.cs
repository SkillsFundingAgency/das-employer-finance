using System;
using Newtonsoft.Json;

namespace SFA.DAS.EAS.Portal.Client.Models.Concrete
{
    public class ReservedFundingDto : IReservedFundingDto
    {
        [JsonProperty("reservationId")]
        public virtual Guid ReservationId { get; private set; }

        //optional
        [JsonProperty("courseId")]
        public virtual string CourseId { get; private set; }

        //optional
        [JsonProperty("courseName")]
        public virtual string CourseName { get; private set; }

        [JsonProperty("startDate")]
        public virtual DateTime StartDate { get; private set; }

        [JsonProperty("endDate")]
        public virtual DateTime EndDate { get; private set; }
    }
}
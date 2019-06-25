﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace SFA.DAS.EAS.Portal.Client.Types
{
    public class Organisation
    {
        public Organisation()
        {
            Reservations = new List<Reservation>();
            Cohorts = new List<Cohort>();
            Agreements = new List<Agreement>();
        }
        [JsonProperty("id")]
        public long AccountLegalEntityId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("reservations")]
        public ICollection<Reservation> Reservations { get; set; }
        [JsonProperty("cohorts")]
        public ICollection<Cohort> Cohorts { get; set; }
        [JsonProperty("agreements")]
        public ICollection<Agreement> Agreements { get; set; }
        
    }
}
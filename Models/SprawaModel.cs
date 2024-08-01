using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YourNamespace.Models
{
    public class SprawaModel
    {
        public string Znak { get; set; }
        public string Uwagi { get; set; }
        public DateTime DataRejestracji { get; set; }
        public DateTime? DataZakonczenia { get; set; }
        public string Prowadzacy { get; set; }
        public string ImieNazwisko { get; set; }
        public string NazwaPodmiotu { get; set; }
        public int IdPisma { get; set; }
    }
}

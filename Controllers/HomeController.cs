using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using YourNamespace.Models;

namespace YourNamespace.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Sprawy(string searchName, string searchSurname, string searchFirstName, DateTime? startDate, DateTime? endDate)
        {
            ViewBag.SearchName = searchName;
            ViewBag.SearchSurname = searchSurname;
            ViewBag.SearchFirstName = searchFirstName;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            if (string.IsNullOrEmpty(searchName) && string.IsNullOrEmpty(searchSurname) && string.IsNullOrEmpty(searchFirstName) && !startDate.HasValue && !endDate.HasValue)
            {
                return View(new List<SprawaModel>());
            }

            var model = await GetSprawyAsync(searchName, searchSurname, searchFirstName, startDate, endDate);
            return View(model);
        }

        private async Task<List<SprawaModel>> GetSprawyAsync(string searchName, string searchSurname, string searchFirstName, DateTime? startDate, DateTime? endDate)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MyDbContext"].ConnectionString;
            var queryBuilder = new StringBuilder(@"
                SELECT a.Znak, a.Uwagi, a.DataRejestracji, a.DataZakonczenia, 
                       d.Imie + ' ' + d.Nazwisko AS Prowadzący, 
                       c.Imie + ' ' + c.Nazwisko AS 'Imię Nazwisko', 
                       c.Nazwa AS 'Nazwa Podmiotu',
                       b.IdPisma
                FROM dbo.SpisSpraw a
                JOIN dbo.Dokument b ON a.IdDokumentuWszczynajacego = b.Id
                JOIN dbo.Adresaci c ON b.MetaIdAdresata = c.Id
                JOIN dbo.Pracownicy d ON a.IdProwadzacy = d.Id
                WHERE 1=1");

            if (!string.IsNullOrEmpty(searchName))
                queryBuilder.Append(" AND c.Nazwa LIKE @searchName");

            if (!string.IsNullOrEmpty(searchSurname))
                queryBuilder.Append(" AND c.Nazwisko LIKE @searchSurname");

            if (!string.IsNullOrEmpty(searchFirstName))
                queryBuilder.Append(" AND c.Imie LIKE @searchFirstName");

            if (startDate.HasValue)
                queryBuilder.Append(" AND a.DataRejestracji >= @startDate");

            if (endDate.HasValue)
                queryBuilder.Append(" AND a.DataRejestracji <= @endDate");

            var sprawy = new List<SprawaModel>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(queryBuilder.ToString(), connection))
            {
                if (!string.IsNullOrEmpty(searchName))
                    command.Parameters.AddWithValue("@searchName", "%" + searchName + "%");

                if (!string.IsNullOrEmpty(searchSurname))
                    command.Parameters.AddWithValue("@searchSurname", "%" + searchSurname + "%");

                if (!string.IsNullOrEmpty(searchFirstName))
                    command.Parameters.AddWithValue("@searchFirstName", "%" + searchFirstName + "%");

                if (startDate.HasValue)
                    command.Parameters.AddWithValue("@startDate", startDate);

                if (endDate.HasValue)
                    command.Parameters.AddWithValue("@endDate", endDate);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var sprawa = new SprawaModel
                        {
                            Znak = reader["Znak"].ToString(),
                            Uwagi = reader["Uwagi"].ToString(),
                            DataRejestracji = reader.GetDateTime(reader.GetOrdinal("DataRejestracji")),
                            DataZakonczenia = reader.IsDBNull(reader.GetOrdinal("DataZakonczenia"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("DataZakonczenia")),
                            Prowadzacy = reader["Prowadzący"].ToString(),
                            ImieNazwisko = reader["Imię Nazwisko"].ToString(),
                            NazwaPodmiotu = reader["Nazwa Podmiotu"].ToString(),
                            IdPisma = reader.GetInt32(reader.GetOrdinal("IdPisma"))
                        };
                        sprawy.Add(sprawa);
                    }
                }
            }

            return sprawy;
        }
    }
}
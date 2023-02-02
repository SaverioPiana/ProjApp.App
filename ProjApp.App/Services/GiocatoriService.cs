using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ProjApp;

namespace ProjApp.Services
{
    public class GiocatoriService
    {

        public GiocatoriService() { 
        
        }

        List<Giocatori> giocatoriList = new ();

        public async Task<List<Giocatori>> GetGiocatori()
        {
            if (giocatoriList?.Count > 0)
                return giocatoriList;

            using var stream = await FileSystem.OpenAppPackageFileAsync("info_giocatori.json");
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            giocatoriList = JsonSerializer.Deserialize<List<Giocatori>>(contents);

            return giocatoriList;
        }

     }
}

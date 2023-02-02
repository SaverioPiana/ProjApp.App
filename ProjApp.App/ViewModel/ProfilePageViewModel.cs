using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjApp.Services;
using System.Collections.ObjectModel;

namespace ProjApp.ViewModel
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        GiocatoriService giocatoriService;
        public ObservableCollection<Giocatori> Player { get; } = new();

        public ProfilePageViewModel(GiocatoriService giocatoriService) {
            
            Player = new ObservableCollection<Giocatori>();
            this.giocatoriService = giocatoriService;


            GetGiocatoriCommand.Execute(this);
        }

        


        [RelayCommand]
        async Task GetGiocatoriAsync()
        {

            try
            {
                var giocatori = await giocatoriService.GetGiocatori();

                if (Player.Count != 0) Player.Clear();

                foreach (var giocatore in giocatori)
                {
                    Player.Add(giocatore);

                }


            }
            catch (Exception ex)
            {


            }
            finally
            {

            }

        }



    }
}
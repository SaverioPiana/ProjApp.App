﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjApp.ViewModel
{
    public partial class ProfilePageViewModel : ObservableObject
    {

        public ProfilePageViewModel() { }

        [RelayCommand]
        Task NavigateBack() => Shell.Current.GoToAsync("..");



    }
}
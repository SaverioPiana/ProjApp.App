using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjApp.Gioco
{
    public class Connessione
    {
        //variabile di connessione all HUB di SignalR
        public static HubConnection con;


        public Connessione()
        {
            //Connession a SignalR
            con = new HubConnectionBuilder()
                .WithUrl("https://nascondapp.azurewebsites.net/lobby")
                .WithAutomaticReconnect()
                .Build();

            //Task.Run(this.serverMessages);

        }

        //private void serverMessages()
        //{

        //    con.On<string>("ServerMessage", (mess) =>
        //    {
        //        Console.WriteLine($"///SERVER///::  {mess}");

        //    });

        //}





    }
}

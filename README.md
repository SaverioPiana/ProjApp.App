# BUDDY HUNT

Progetto in MAUI per esame di MobileComputing presso RomaTre

TO ABSOLUTELY DO(nel senso che sarebbe bello):

- Tana libera tutti è un POWER UP che appare o a caso o se rimagono pochi hiders, ed è piu o meno lontano dalla tana, cosi che entrambe le squadre possono decidere tatticamnete dove andare. Uso: se ti tani con il power up equipaggiato liberi tutti e vincono a prescindere gli hiders

To Do:

- problema con posizione gps -> capire come funge (fatto)

- Hub di signalR(fatto) -> poi bisognera' capire come creare un modo di fare lobby private(fatto)

- capire cos'e mapview, come funge, come modificarlo a nostra discrezione (ha stranamente funzionato molto a caso)(fatto)

- capire come funzionano i layers della mappa e come aggiungere dei punti/pins personalizzati(fatto)

- UI bella pulita -> da fare assolutmente alla svelta

- logiche di gioco(non molte) -> da fare quando avremo almeno qualche bottone

- riga 89 del profileviewmodel metodo builduser -> bisogna levare una parte di codice quando verra' implementato un Auth 

- Dobbiamo ricordarci di usare levento onDestroyed della classe app per gestire il fatto che lutente deve lasciare la lobby e la leva dalla memoria ...

- Autorizzazione per posizione -> da mettere prima che si apri il gioco

- DARE UN NOME ALL'APP

- creare un icona e spash per lapp
- ------------------------------------- ^COSE FATTE SOPRA^

Miglioramenti/Problemini:
- Va fixato il codice negli ultimi giorni che e' stato un po rushato (a volte crasha e da problemi nelle partite successive alla prima)

- Loffset dei pin del draw area non e' centrato

- forse bisognerebbe interpolare il myuserpin, va un po a scatti

- togliere i task delay (ci siamo dai ...)

- vediamo se aggiungere un tema musicale per il menu

- vedere se si possono fare temi non hard coded(Ci siamo)

# 🎰 Roulette Strategy Simulator: L'Arte di Perdere con Eleganza 📉

Benvenuto, aspirante magnate del casinò o, più realisticamente, appassionato di programmazione che ha capito che l'unico modo sicuro per guadagnare con la roulette è scrivere il codice del banco!

## 🤡 Disclaimer (Leggimi o piangi dopo)

**ATTENZIONE:** Questo progetto è **puramente didattico**. Se pensi di usare questo simulatore per trovare la "strategia magica" che sbancherà Las Vegas, abbiamo brutte notizie per te: la matematica è una scienza esatta, e la matematica della roulette dice che la casa vince sempre. 

*   **Non usare soldi veri** basandoti su queste simulazioni.
*   Il "metodo infallibile" non esiste (a meno che tu non possieda il casinò).
*   Se finisci a vivere in una scatola di cartone perché hai puntato tutto sul Rosso dopo 10 Neri consecutivi, non venire a cercarmi. Noi eravamo qui a scrivere C#, non a fornire consulenze finanziarie!

---

## 🛠️ Parte Tecnica (Per noi nerd)

Se sei qui, probabilmente ti interessa più il codice che il gioco d'azzardo. Questo simulatore è scritto in **.NET 10** ed è strutturato in modo modulare per permetterti di testare quanto velocemente una strategia può portare al fallimento (virtuale).

### Struttura del Progetto

*   **Roulette.Core**: La fisica (senza attrito) del gioco. Contiene la `Wheel`, il calcolo dei pagamenti e le definizioni di base.
*   **Roulette.Strategies**: Il cuore del "piano d'attacco". Qui trovi Martingale, Fibonacci, Labouchere e altre varianti creative per svuotare il portafoglio.
*   **Roulette.Simulation**: Il motore che fa girare la ruota migliaia di volte in un secondo, risparmiandoti ore di noia al tavolo verde.
*   **Roulette.ConsoleApp**: L'interfaccia utente (molto vintage, molto terminale) per configurare i tuoi esperimenti.
  * C'è il primo parametro `IsAmerican` per scegliere tra Roulette Europea (0-36) e Americana (0-36 + 00).

### Come iniziare

1.  Assicurati di avere il SDK di **.NET 10** installato.
2.  Clona la repository.
3.  Esegui il comando magico:
    ```bash
    dotnet run --project Roulette.ConsoleApp
    ```
4.  Segui le istruzioni a schermo e preparati a vedere i tuoi bilanci oscillare pericolosamente.

### Come implementare nuove Strategie

Tutto ciò che serve è implementare l'interfaccia `IGameStrategy`:

```csharp
public interface IGameStrategy
{
    string Name { get; }
    void Reset();
    IEnumerable<Bet> NextBets(IReadOnlyList<Pocket> history, decimal currentBalance);
}
```

Aggiungi la tua creazione in `Roulette.Strategies`, registrala nel menu di `Program.cs` e guarda come si comporta contro la cruda realtà statistica.

### Come aggiungere nuove Statistiche

Attualmente calcoliamo media, deviazione standard, migliori/peggiori sessioni e distribuzione dei profitti. Se vuoi aggiungere altro (tipo l'indice di disperazione del giocatore):

1.  Espandi `BatchSessionResult.cs` con le nuove proprietà.
2.  Aggiorna la logica di calcolo in `Simulator.cs` (metodo `RunBatch`).
3.  Mostra i risultati in `Program.cs` (metodo `RunSimulation`).

---

*Progetto creato con ❤️, C# e una sana dose di scetticismo verso il gioco d'azzardo.*

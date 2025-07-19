using CIE.MRTD.SDK.EAC;
using CIE.MRTD.SDK.PARSERLIB;
using CIE.MRTD.SDK.PCSC;
using CieReader.Utils;
using CieReader.View;
using PCSC;
using PCSC.Utils;
using System.Diagnostics;

namespace CieReader.Service
{
    public class Reader
    {
        private SmartCard sc;
        private bool connectNotificationSent = false;
        private bool disconnectNotificationSent = false;
        private bool connectedReader = false;

        public event EventHandler<string> OnCardRead;

        public Reader()
        {
            sc = new SmartCard();
            sc.SmarCardCommunication += new SmarCardCommunicationDelegate((x, b) =>
            {
                // scrivo in debug i dati inviati e ricevuti dal chip
                Debug.WriteLine(x.ToString() + ":" + BitConverter.ToString(b));
            });

            // Alla rimozione del documento aggiorno la label del form
            sc.onRemoveCard += new CardEventHandler((r) =>
            {
                Debug.WriteLine("Appoggiare la carta sul lettore");
            });

            // All'inserimento del documento aggiorno la label e avvio la lettura
            sc.onInsertCard += new CardEventHandler((r) =>
            {

                Debug.WriteLine("Lettura in corso...");                
                Application.DoEvents();

                bool connected = false;
                int attempts = 0;

                while (!connected && attempts < 10)
                {
                    connected = sc.Connect(r, Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1);
                    if (!connected)
                    {
                        Debug.WriteLine($"Tentativo {++attempts} fallito. Codice errore: {sc.LastSCardResult:X08}");
                        Thread.Sleep(100);
                    }

                    if (attempts >= 5)
                    {
                        connected = sc.Reconnect(Share.SCARD_SHARE_EXCLUSIVE, Protocol.SCARD_PROTOCOL_T1, Disposition.SCARD_RESET_CARD);
                    }

                    attempts++;
                }

                if (!connected) {
                    NotificationBalloon.ShowBalloon("Errore di lettura", "Non è stato possibile leggere la carta, si prega di riprovare!");
                    Debug.WriteLine("Connessione fallita dopo 10 tentativi!");
                }

                // Creo l'oggetto EAC per l'autenticazione e la lettura, passando la smart card su cui eseguire i comandi
                EAC a = new EAC(sc);

                // Verifico se il chip è SAC
                if (a.IsSAC())
                {
                    string can = null;
                    var context = SynchronizationContext.Current;

                    if (context == null)
                    {
                        NotificationBalloon.ShowBalloon("Errore", "Impossibile mostrare il dialogo CAN in questo contesto.");
                        return;
                    }

                    context.Send(_ =>
                    {
                        using var dlg = new CanPromptDialog();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            can = dlg.CanCode;
                        }
                    }, null);

                    if (string.IsNullOrWhiteSpace(can))
                    {
                        NotificationBalloon.ShowBalloon("Operazione annullata", "La lettura è stata annullata dall'utente.");
                        return;
                    }

                    // Effettuo l'autenticazione PACE.
                    // In un caso reale prima di avvare la connessione al chip dovrei chiedere all'utente di inserire il CAN                    
                    a.PACE(can);
                }
                else
                {
                    // Il chip non supporta PACE (caso raro).
                    // Per usare BAC dovremmo acquisire l'MRZ leggendo la carta fisica con un OCR.
                    // Questo flusso non è implementato in questa versione.
                    NotificationBalloon.ShowBalloon("Errore", "Carta non supportata: il chip non supporta il protocollo di autenticazione PACE.");
                    return;
                }

                // Per poter fare la chip authentication devo prima leggere il DG14
                var dg14 = a.ReadDG(DG.DG14);

                // Effettuo la chip authentication
                a.ChipAuthentication();

                C_CIE readDocument = new C_CIE(a); //creazione di una E_CIE

                // Disconnessione dal chip
                sc.Disconnect(Disposition.SCARD_RESET_CARD);

                OnCardRead?.Invoke(this, readDocument.ToJsonString());
            });

            // Avvio il monitoraggio dei lettori
            Task.Run(() => StartReaderMonitoring());
        }

        ~Reader()
        {
            sc.StopMonitoring();
        }

        public void StartReaderMonitoring()
        {
            var contextFactory = ContextFactory.Instance;

            while (true)
            {
                using var context = contextFactory.Establish(SCardScope.System);

                var readerState = new SCardReaderState
                {
                    ReaderName = @"\\?PnP?\Notification",
                    CurrentState = SCRState.Unaware
                };

                Debug.WriteLine("🟢 Monitoraggio avviato...");

                while (true)
                {
                    var rc = context.GetStatusChange(1000, new[] { readerState });

                    if (rc == SCardError.Success)
                    {
                        if ((readerState.EventState & SCRState.Changed) == SCRState.Changed)
                        {
                            readerState.CurrentState = readerState.EventState;

                            var readers = context.GetReaders();
                            Debug.WriteLine("⚠️ Lettori aggiornati:");
                            if (readers.Length == 0)
                            {
                                Debug.WriteLine("  Nessun lettore disponibile.");
                            }
                            else
                            {                                
                                foreach (var reader in readers)
                                {
                                    if (!connectNotificationSent)
                                    {
                                        NotificationBalloon.ShowBalloon("Lettore connesso", "Nuovo lettore rilevato!");
                                        connectNotificationSent = true;
                                        disconnectNotificationSent = false;
                                    }

                                    if (!connectedReader)
                                    {
                                        connectedReader = true;
                                        sc.StartMonitoring();
                                    }
                                    Debug.WriteLine($"  → {reader}");
                                }
                            }
                        }
                    }
                    else if (rc == SCardError.Timeout)
                    {                     
                        continue;
                    }
                    else
                    {
                        if (!disconnectNotificationSent)
                        {
                            NotificationBalloon.ShowBalloon("Lettore disconnesso", "Il lettore NFC non è più disponibile. Si prega di verificare il collegamento e riprovare!");
                            disconnectNotificationSent = true;
                            connectNotificationSent= false;
                        }
                        if (connectedReader)
                        {
                            connectedReader = false;
                            sc.StopMonitoring();
                        }

                        Debug.WriteLine($"❌ Errore irreversibile: {SCardHelper.StringifyError(rc)}");
                        Debug.WriteLine("🔄 Riavvio del contesto...");
                        break; 
                    }

                    Thread.Sleep(500);
                }

                Thread.Sleep(1000);
            }
        }
    }
}

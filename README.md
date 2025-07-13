# CieReader

**CieReader** è un'applicazione desktop (proof-of-concept) per Windows che consente di leggere i dati anagrafici dalla **CIE – Carta d’identità elettronica** tramite un lettore **NFC compatibile**.  
L’obiettivo principale è **semplificare e velocizzare l’inserimento anagrafico** in contesti digitali, come ad esempio gestionali o sistemi CRM.

## 🧩 Caratteristiche

- Lettura dati da CIE v2.x (testato)
- Supporta lettori compatibili con PC/SC e protocolli NFC (PACE, BAC, EAC)
- Comunicazione via WebSocket in `localhost` per integrazione con client (es. WebApp)
- Nessuna interfaccia grafica, funziona in background
- Estrazione dati dai Data Group (DG1, DG2, DG11) e fallback da MRZ
- Invio dati in formato JSON ai client WebSocket con supporto multi-connessione

## 🛠️ Requisiti

- **Sistema operativo:** Windows
- **Lettore NFC testato:** [ACR1252U](https://www.acs.com.hk/en/products/173/acr1252u-usb-nfc-reader/)
- .NET 6 o superiore
- Lettore compatibile con standard ISO/IEC 14443 (tipo A/B) e APDU
- Connessione alla porta `localhost:8080` (attualmente hardcoded)

## 📦 Tecnologie e dipendenze

- .NET (console application)
- WebSocket server integrato
- Basato su [cie-mrtd-example-app](https://github.com/italia/cie-mrtd-example-app) per l’accesso sicuro ai dati tramite protocollo PACE

## 🚀 Come funziona

1. Avvia l’applicazione 
2. Il programma si avvia in background e apre una WebSocket in ascolto su `localhost:8080`
3. In caso di presenza di un lettore NFC compatibile, viene tentata la lettura della CIE
4. I dati estratti (come nome, cognome, data di nascita, codice fiscale, indirizzo...) vengono inviati al client connesso via WebSocket

### Esempio di JSON inviato

```json
{
   "firstName":"MARIO",
   "lastName":"ROSSI",
   "birthCity":"MATERA",
   "birthProv":"MT",
   "birthDate":"YYYY-MM-DD",
   "address":"INDIRIZZO, N_CIVICO",
   "prov":"MT",
   "cf":"COD_FISC",
   "mrz":"MRZ",
   "dateIssue":"DD/MM/YYYY",
   "dateExpire":"DD/MM/YYYY",
   "city":"MATERA",
   "cie_jpg2k_image":[...]
}
```

## ⚠️ Limitazioni attuali
- Testato solo CIE versione 2.x
- Nessuna autenticazione per le connessioni WebSocket
- Parametri come porta e host della WebSocket sono hardcoded
- Potrebbero essere necessarie più letture in caso di lettore NFC poco sensibile
- Attualmente supporta solo sistemi Windows

## 📚 Licenza
Questo progetto è distribuito con licenza GPL-3.0.
Consulta il file LICENSE per maggiori dettagli.

## 🤝 Contribuisci
Il progetto è un proof-of-concept ma è aperto a suggerimenti, issue e pull request.
Tutti i contributi sono benvenuti!

⚙️ Creato per passione, come primo esperimento con lo stack .NET e dispositivi NFC.

📫 Connect with me

[![LinkedIn](https://img.shields.io/badge/LinkedIn-%230077B5.svg?&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/antonio-montemurro-2b3107287/)

# CieReader

**CieReader** è un'applicazione desktop (proof-of-concept) per Windows che consente di leggere i dati anagrafici dalla **CIE – Carta d'identità elettronica** tramite un lettore **NFC compatibile**.  
L'obiettivo principale è **semplificare e velocizzare l'inserimento anagrafico** in contesti digitali, come ad esempio gestionali o sistemi CRM.

## 🧩 Caratteristiche

- Lettura dati dalla CIE
- Supporta lettori compatibili con PC/SC e protocolli NFC (PACE, EAC)
- Comunicazione via WebSocket per integrazione con client (es. WebApp)
- Autenticazione delle connessioni WebSocket tramite API key (header `X-API-Key`)
- Nessuna interfaccia grafica, funziona in background come tray application
- Estrazione dati dai Data Group (DG1, DG2, DG11, DG12, DG14, SOD) e fallback da MRZ
- Invio dati in formato JSON ai client WebSocket con supporto multi-connessione

## 🛠️ Requisiti

- **Sistema operativo:** Windows
- **Lettore NFC testato:** [ACR1252U](https://www.acs.com.hk/en/products/173/acr1252u-usb-nfc-reader/)
- .NET 10 o superiore
- Lettore compatibile con standard ISO/IEC 14443 (tipo A/B) e APDU

## 📦 Tecnologie e dipendenze

- .NET (Windows Forms tray application)
- WebSocket server integrato
- Basato su [cie-mrtd-example-app](https://github.com/italia/cie-mrtd-example-app) per l'accesso sicuro ai dati tramite protocollo PACE

## ⚙️ Configurazione

L'applicazione si configura tramite il file `config.json` nella directory dell'eseguibile:

```json
{
  "websocket": {
    "host": "localhost",
    "port": 8080,
    "wsApiKey": "LA_TUA_API_KEY"
  },
  "autoStartUp": true
}
```

| Parametro | Descrizione |
|---|---|
| `host` | Indirizzo su cui il server WebSocket si mette in ascolto |
| `port` | Porta del server WebSocket |
| `wsApiKey` | API key richiesta ai client per autenticarsi (header `X-API-Key`) |
| `autoStartUp` | Se `true`, l'applicazione viene registrata per l'avvio automatico con Windows |

## 🚀 Come funziona

1. Avvia l'applicazione
2. Il programma si avvia in background e apre una WebSocket in ascolto sull'indirizzo e porta configurati in `config.json`
3. In caso di presenza di un lettore NFC compatibile, viene tentata la lettura della CIE
4. I dati estratti (come nome, cognome, data di nascita, codice fiscale, indirizzo...) vengono inviati al client connesso via WebSocket

### Esempio di JSON inviato

```json
{
   "firstName": "MARIO",
   "lastName": "ROSSI",
   "birthCity": "MATERA",
   "birthProv": "MT",
   "birthDate": "DD/MM/YYYY",
   "address": "INDIRIZZO, N_CIVICO",
   "prov": "MT",
   "cf": "COD_FISC",
   "mrz": "MRZ",
   "dateIssue": "DD/MM/YYYY",
   "dateExpire": "DD/MM/YYYY",
   "sex": "M",
   "city": "MATERA",
   "nationality": "ITA",
   "documentNumber": "CA00000AA",
   "authority": "COMUNE DI MATERA",
   "photo": "BASE64..."
}
```

## ⚠️ Limitazioni attuali
- Testato solo con lettore ACR1252U, potrebbero esserci problemi di compatibilità con altri modelli
- Potrebbero essere necessarie più letture in caso di lettore NFC poco sensibile
- Attualmente supporta solo sistemi Windows

## 📚 Licenza
Questo progetto è distribuito con licenza GPL-3.0.
Consulta il file LICENSE per maggiori dettagli.

## 🤝 Contribuisci
Il progetto è un proof-of-concept ma è aperto a suggerimenti, issue e pull request.
Tutti i contributi sono benvenuti!

⚙️ Creato per passione, come primo esperimento con lo stack .NET e dispositivi NFC.

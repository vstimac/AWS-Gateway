# AWS Gateway Desktop Client

Desktop aplikacija razvijena kao završni rad na Sveučilištu Sjever, Odjel za računarstvo i informatiku. Aplikacija objedinjuje upravljanje i nadzor odabranih resursa u oblaku **Amazon Web Services (AWS)** s naglaskom na usluge **Amazon S3** i **Amazon EC2**, uz mogućnost pregleda korištenja resursa i troškova.

---

## Funkcionalnosti

- **Amazon S3 upravljanje:**
  - Pregled bucket-a i objekata s filtriranjem po nazivu i prikazom klase pohrane.
  - Prijenos i preuzimanje datoteka uz podršku za **višedijelni (multipart) i istovremeni (concurrent) prijenos**.
  - Generiranje vremenski ograničenih potpisanih poveznica (*presigned URLs*).
- **Amazon EC2 upravljanje:**
  - Pregled instanci (pokretanje, zaustavljanje, ponovno pokretanje).
  - **Procjena opterećenja instance** na temelju metrika usluge Amazon CloudWatch (iskorištenost CPU-a, mrežni promet, procesorski krediti).
- **AWS Cost Explorer:**
  - Pregled i analiza troškova grupiranih po uslugama i regijama s lokalnom predmemorijom radi smanjenja broja API zahtjeva i troškova.
- **Sigurnost i autentifikacija:**
  - Prijava trajnim pristupnim ključem (uz zaštitu pomoću Windows DPAPI sučelja) ili **preuzimanjem uloge (AssumeRole)** uz opcionalnu MFA potvrdu.
- **Izvještaji i aktivnosti:**
  - Praćenje povijesti prijenosa, generiranje detaljnih PDF izvještaja (putem *iText* biblioteke) i izvoz podataka u CSV format.
  - Lokalni dnevnik aktivnosti i podrška za lokalni TCP servis.

---

## Korištene tehnologije

- **Platforma i jezik:** .NET 10.0, C#
- **Korisničko sučelje:** Windows Forms (uz inspiraciju *Cloudscape* dizajnerskim sustavom)
- **AWS SDK paketi:**
  - `AWSSDK.S3`, `AWSSDK.EC2`, `AWSSDK.CloudWatch`, `AWSSDK.CostExplorer`, `AWSSDK.SecurityToken`
- **Baza podataka i pohrana:** SQLite (`Microsoft.Data.Sqlite`, *Dapper* ORM)
- **Izrada PDF izvještaja:** iText (v9.7.0)

---

## Arhitektura i struktura

Aplikacija je organizirana u tri sloja:
1. **Korisničko sučelje:** Windows Forms prozori (Prijava, Glavni prozor s bočnom navigacijom i modularnim prikazima).
2. **Servisi i poslovna logika:** Logika za rad s AWS klijentima, procjenu opterećenja instanci, sažetke troškova i upravljanje predmemorijom.
3. **Pristup podacima:** SQLite relacijska baza podataka, XML datoteke za postavke profila i JSON dnevnik aktivnosti.

---

## Korištenje i instalacija

1. Klonirajte repozitorij ili otvorite projekt u razvojnom okruženju (npr. Visual Studio).
2. Provjerite jeste li instalirali .NET 10 SDK.
3. Konfigurirajte svoje AWS pristupne podatke (Access Key / Secret Key ili IAM ulogu) pri prvom pokretanju aplikacije.
4. Pokrenite projekt.

---

## Licenca

Ovaj projekt je licenciran pod uvjetima **MIT licencije**. Više informacija potražite u datoteci `LICENSE`.

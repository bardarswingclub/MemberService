# Fravær, deltakerinfo og rollevalg – oppskrift

Dette dokumentet beskriver fire funksjoner som ble lagt til på branchen
`feature/fravaer-arsak`. Det er skrevet som en huskeliste/oppskrift hvis noe
tilsvarende skal bygges i et nytt system.

Teknologi: ASP.NET Core 8 (Razor Pages + MVC-kontrollere), EF Core 8, SQL Server,
Bootstrap 4, jQuery.

| # | Funksjon | Kort |
|---|----------|------|
| 1 | Årsak til fravær | Ukevis oppmøteside med radioknapper per deltaker |
| 2 | Deltakerinfo | «Kommer fra» og «andre behov» for Workshop/Fest, med egen tab |
| 3 | Rediger påmelding | Synlig «Rediger»-lenke, utvidet redigeringsside |
| 4 | Rollevalg | Fører / Følger / Switch, konfigurerbart per arrangement |
| 5 | Nyhetsbrev | Avkrysning for Mailchimp-påmelding når man betaler medlemskap |

Alle funksjoner har en EF-migrasjon som må kjøres i produksjon:

```
20261028191751_AddPresenceAbsenceReason
20261028193544_AddSignupOriginAndSpecialNeeds
20261028201433_AddSignupRolePreferences
20261028202154_AddAllowedRoles
```

---

## 1. Årsak til fravær

**Mål:** Ved fravær skal man kunne velge årsak fra en liste (Sykdom, Ikke møtt,
Sluttet kurs, Annet), uten at det blir mye vanskeligere enn avkrysningsboksene.
Skal fungere på mobil.

### Datamodell
- Ny enum `Data/ValueTypes/AbsenceReason.cs` (`Sick=1, NoShow=2, DroppedOut=3, Other=4`)
  med `[Display(Name = "...")]` på norsk.
- `Data/Presence.cs` har fått `AbsenceReason? AbsenceReason` og `string AbsenceNote`
  (begge nullable, så eksisterende rader er urørt).
- Semantikk: `Present = false` og `AbsenceReason = null` betyr «fravær uten årsak»
  (samme som før). `Present = true` har alltid `AbsenceReason = null`.

### UI og logikk (`Pages/Event/Presence/`)
- **Eksisterende oversikt (`Index.cshtml`) er beholdt** med avkrysningsbokser.
  Tillegg: ukenummer i tabellhodet er lenker til ukevisningen, en knapp over
  tabellen åpner siste uke, og en liten bokstav under tom boks viser registrert
  årsak (full tekst som tooltip).
- **Ny ukevisning `Lesson.cshtml`** (`/Event/{id}/Presence/Lesson?lesson=N`):
  ett kort per deltaker med radioknapper (`btn-group-toggle`, bryter linjer på
  mobil): *Til stede · Sykdom · Ikke møtt · Sluttet kurs · Annet*. Et valgfritt
  tekstfelt for utdyping vises kun når en fraværsårsak er valgt. Pilene « » bytter
  uke. Hvert valg lagres umiddelbart via `$.ajax` (egen «Lagret»-status per kort).
- **`PresenceController`**: ny `Lesson`-GET og ny `SetAttendance`-POST
  (`status` = `present`, en `AbsenceReason`-verdi, eller tom). Gammel
  `SetPresence` nullstiller årsak når «til stede» krysses av. Samme policy
  (`CanSetPresence`/`CanViewEvent`) som før.
- `ParticipantsModel` har fått `Absences` (parallell liste til `Presence`).
  Prosentene i tabellen regner fortsatt bare på til stede / ikke til stede.

### Fallgruver
- Policy og arkivert-sjekk må gjentas i den nye visningen (`canEdit`).
- `SetPresence` bruker `signup.Presence` uten `Include`; den nye `SetAttendance`
  bruker `.Include(p => p.Presence)` for å unngå duplikatrader.

---

## 2. Deltakerinfo (sted og andre behov)

**Mål:** På Workshop og Fest skal deltakerne kunne oppgi hvilken by/land de kommer
fra og andre behov (allergier m.m.). Dette vises i en egen tab ved siden av
Påmeldte, Oppmøte osv.

### Datamodell
- `EventSignup.Origin` og `EventSignup.SpecialNeeds` (nullable `nvarchar(max)`).
- Hvilke arrangementer det gjelder: `CollectsParticipantInfo(this EventType)` i
  `Pages/Signup/Logic.cs` (Workshop og Party). Ingen konfigurasjon per arrangement.
- `TrimToNull(this string)` i samme fil brukes til å normalisere input.

### Påmelding og redigering
- `SignupInputModel` har `Origin` og `SpecialNeeds`; `SignupModel.CollectParticipantInfo`
  styrer om feltene vises i `_SignupResponse.cshtml`.
- Samme felt i brukerens egen redigeringsside (`Pages/Signup/Edit.cshtml` + `.cs`).
- Feltene er valgfrie; maks 100 og 500 tegn.
- Engelske tekster ligger i `_SignupResponse.en.resx` (nøkkel = norsk tekst).

### Tab «Deltakerinfo»
- Ny Razor Page `Pages/Event/Info.cshtml(.cs)` (`/Event/Info/{id}`), policy `CanViewEvent`.
- Viser oppsummering «hvor kommer de fra» (gruppert, sortert på antall) og en tabell
  med navn, sted og behov. Kun deltakere med plass (`AcceptedAndPayed` og `Approved`).
- Tab-lenken legges i `Components/EventHeader/Default.cshtml`; `EventHeader.Model`
  har fått `CollectsParticipantInfo` (beregnet i spørringen).
- NB: siden viser helseopplysninger (allergier). Vurder om tilgangen bør strammes inn.

---

## 3. Rediger påmelding

**Mål:** Administrator skal kunne fylle inn info som mangler på en påmelding (f.eks.
påmeldinger gjort før rollevalg fantes).

- Eksisterende side `Pages/Event/EditSignup.cshtml` fantes, men lenken lå skjult i
  historikk-dialogen. Nå ligger en synlig «Rediger»-lenke ved navnet i
  `Pages/Event/_Participants.cshtml` (vises kun med policy `CanEditEventSignup`).
- Siden redigerer rolle, partner, flytting til annet kurs, og (for Workshop/Fest)
  sted og behov. Alle endringer logges i påmeldingens historikk (`AuditLog`).
- **Rettelse av et reelt problem:** Deltakere med `Role = None` på et arrangement
  med rollepåmelding ble ikke vist i noen liste. `View.cshtml` har derfor fått en
  tredje liste **«Uten rolle»** under Førere/Følgere, og badgen på fanen viser
  `ledere+følgere+uten rolle`.

---

## 4. Rollevalg: Fører / Følger / Switch

**Mål:** Man skal kunne velge om Fører, Følger og/eller Switch er mulige valg på
et arrangement, og deltakeren velger **én** av dem ved påmelding (radioknapper).
Valget vises oppsummert i deltakerlisten.

### Datamodell
- `Data/ValueTypes/RolePreference.cs`: `[Flags] enum RolePreference { None, Lead, Follow, Switch }`
  med hjelpemetoder i `RolePreferenceExtensions`:
  `ToRole()` (kun Lead/Follow gir en `DanceRole`, Switch gir `None`), `FromRole()`,
  `IsSingleChoice()`, `ToChoices()`, `Summary()`, `OrDefault()`, `Combine()`.
  Record `RoleChoices` er modellen til den delte partialen.
- `EventSignup.RolePreferences` – hva deltakeren valgte.
- `EventSignupOptions.AllowedRoles` – hva arrangementet tillater. Default
  `Lead | Follow`. Migrasjonen setter eksisterende rader til `0`, og **koden tolker
  0 som Lead+Follow** (`OrDefault()`), så ingenting endres for gamle arrangementer.
- **`EventSignup.Role` (`DanceRole`) beholdes** og er den rollen listene
  (Førere/Følgere), partnerkobling, auto-godkjenning og oppmøte bruker. Den settes
  automatisk fra valget: Lead→Lead, Follow→Follow, Switch→`None` («Uten rolle»).

### Konfigurasjon
- `EventInputModel.AllowedRoles` (`List<RolePreference>`), avkrysningsbokser i
  `_EditEvent.cshtml` inni «Fører og følger»-seksjonen (Lead+Follow er forvalgt hvis
  ingenting er valgt). Lagres i `Pages/Event/Logic.cs` (opprett, oppdater og kopier)
  og leses i `Edit.cshtml.cs`.

### Påmelding
- Delt partial `Pages/Shared/_RolePreferences.cshtml` tegner radioknapper
  (`name="Input.Preference"`) for de tillatte valgene. `required` er på som standard;
  administratorsiden slår det av (`RoleChoices.Required = false`).
- `SignupController` (POST) og `Pages/Signup/Edit.cshtml.cs` validerer på serveren:
  nøyaktig ett valg (`IsSingleChoice`) og innenfor `AllowedRoles`, ellers
  feilmelding «Velg en danserolle».
- Statussiden (`Status.cshtml`) og kurslistene viser valget («som switch»).
- Deltakerlisten (`_Participants.cshtml`) viser «Valg: …» under navnet.
- Engelsk: `RolePreference.en.resx` (Lead/Follow/Switch).

### Kurs (egen flyt)
Kurs påmeldes via `Pages/Home/Courses` (prioritert liste, én rolle per kurs, JS-drevet
skjema med parallelle `roles[]`/`partners[]`-felt). Der er nedtrekkslisten beholdt,
men den viser nå de tillatte rollene (inkl. Switch). `HomeController.Courses` binder
`roles[]` som `RolePreference`, sjekker mot `AllowedRoles` og setter både
`Role` og `RolePreferences`. `CourseSignupModel.RoleDisplay` brukes i
`_signup*.cshtml`.

---

## 5. Nyhetsbrev ved innmelding

**Mål:** Når man melder seg inn i klubben (betaler medlemskap) skal man kunne krysse av
for å melde seg på nyhetsbrevet. Boksen er **uavkrysset** og har teksten
«Om du ikke allerede mottar nyhetsbrevet vårt, kryss av her for å melde deg på!».

### Hvordan nyhetsbrevet på nettsiden fungerer
Skjemaet på `bardarswingclub.com/meld-deg-pa-nyhetsbrevet/` er et Mailchimp
«embedded form» (klassisk). Det er et vanlig `POST` til en offentlig adresse, uten
API-nøkkel:

```
POST https://<konto>.<dc>.list-manage.com/subscribe/post?u=<u>&id=<listeid>&f_id=<f_id>
EMAIL=...&FNAME=...&LNAME=...&b_<u>_<listeid>=      (honeypot-felt, skal være tomt)
```

Verdiene (`u`, `id`, `f_id`, honeypot-navnet) står i kildekoden til siden.

### Implementasjon
- `Services/NewsletterService.cs`: `INewsletterService` + `MailchimpNewsletterService`
  (typed `HttpClient`, 5 sekunders timeout). Sender skjemafeltene over, splitter
  `FullName` i fornavn/etternavn ved første mellomrom. **Kaster aldri** – feil logges
  som warning og gir `false`, slik at innmelding/betaling aldri stoppes av nyhetsbrevet.
- Innstillinger i `appsettings.json` under `Newsletter` (`SubscribeUrl`,
  `HoneypotField`), bundet via `Configure<NewsletterSettings>`. Ikke hemmelig, men
  kan overstyres per miljø.
- UI: `Components/PayFee/Default.cshtml` viser avkrysningsboksen i betalingsskjemaet
  når avgiften inkluderer medlemskap (`Fee.IncludesMembership`), det vil si
  medlemskap, og treningsavgift/kursavgift når medlemskap ikke er betalt.
  `PayFeeViewComponent.Model` har fått `OffersNewsletter`.
- `PayController.Fee` tar imot `newsletter` og kaller `Subscribe` før betalingen
  startes.

### Avveininger / ting å vite
- Påmeldingen skjer når man **starter** betalingen, ikke når den er fullført. Det er
  enkelt og krever ingen lagring av valget gjennom Stripe/Vipps-flyten. Vil man ha
  «kun etter fullført betaling», må valget lagres (f.eks. i en kolonne på betalingen)
  og leses i `FeePaid`/betalingscallbackene.
- Mailchimp kan sende en bekreftelses-e-post (double opt-in) – det styres i
  Mailchimp, ikke her.
- Mailchimp kan blokkere automatiserte kall (captcha). Alternativet er Marketing API
  med API-nøkkel og `PUT /lists/{id}/members/{hash}`.
- Enhetstester med falsk `HttpMessageHandler`:
  `tests/MemberService.Tests/Newsletter/NewsletterServiceTests.cs`. Test aldri mot
  den ekte listen, da melder du en ekte adresse på.

---

## Praktiske råd ved portering

1. **Rekkefølge:** enum + datafelt → migrasjon → påmeldingsskjema → admin-lister →
   konfigurasjon per arrangement.
2. **Behold den gamle enkeltrollen** hvis andre deler av systemet (lister, partner-
   matching, statistikk) avhenger av den. Legg nye valg *ved siden av*.
3. **Server-validering** for alle nye valg, ikke bare `required` i HTML.
4. **Filkoding/linjeskift:** i dette repoet er enkelte filer ikke UTF-8
   (`Pages/Signup/Edit.cshtml` er ISO-8859-1) og noen har CRLF/BOM. Bevar dette ved
   redigering, ellers ødelegges «æøå».
5. **Enum lagres som tekst** i databasen (`Events.Type` = `'Workshop'`, ikke `2`).
   Skriv enum-navn, ikke tall, hvis du setter testdata direkte i SQL.
6. **`sqlcmd`:** `UPDATE` mot `Events` feilet med «QUOTED_IDENTIFIER» uten flagget
   `-I` (`QUOTED_IDENTIFIER ON`).
7. **Lokal testing:** `dotnet ef database update`, så
   `ASPNETCORE_ENVIRONMENT=Development dotnet run --urls http://localhost:5055`.
   Connection string ligger i user-secrets. Testkurs/workshop må ha `LessonCount > 0`
   for at oppmøtesidene skal vise uker.
8. **Razor-kompilering:** feil i `.cshtml` kommer som byggefeil, så `dotnet build`
   fanger de fleste; `dotnet test` (197 tester) dekker ikke disse sidene.

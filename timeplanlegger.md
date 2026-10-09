Jeg ønsker å utvide med en løsning i løsningen her for å planlegge
hvordan vi fordeler timene våre inn i saler som vi leier. De ulike
aktørene har saler til utleie. Vi pleier å ha kurs i bolker av 1.5
time, men noen ganger må kursene bli litt kortere pga begrensninger på
hvor lenge vi får ha lokalene. Så hver enkelt sal må ha en fra og til
tidspunkt for leie.

Vi planlegger et semester i slutten av et nytt semester. Så typisk nå
midt i semesteret for høsten skal vi starte å planlegge vårsemesteret.

Vi får typisk fortsette å ha samme tid fra semester til semester, så
vi kan normalt få brukt saler som vi har hatt semesteret før.

Kursene som vi da planlegger med, er da de kursene som vi gjerne vil
opprette i kurspåmelding senere i samme system som vi har her. Og
tidspunktene og sted er da tilsvarende verdt å ta med inn i oversikten
på kurs.

Selve planleggingen er at vi har alle kursene tilgjengelig i en bolk
under timeplanen, og de kan dragges og droppes inn på salene som er
satt opp. I tillegg har vi sosialdans som da kan variere hvor vi er,
slik at det må være at noen saler vi booker bare har noen uker som de
gjelder.

Så visningen må kunne planlegges i "ukes" modus og i "semester modus".

Noen kurs har 6 ukers lengde og noen har 12 ukers lengde. Så semester
modus kan gjerne ta høyde for å vise det.

Det må være mulig å sette opp saler for et semester. For BLS som er en aktør så booker vi typisk tidspunkt og får en bekreftelse som vi gjerne vil lime inn til systemet og den setter opp salen. Typisk er "Storsalen på BLS" og "Og storsalfoajen på bls" som ser slik ut:

Bestilling av Storsalen for Bårdar Swing Club

Storsalen  27.4.2027 19:00 - 22:30:  Bestilt
Storsalen  4.5.2027 19:00 - 22:30:  Bestilt
Storsalen  11.5.2027 19:00 - 22:30:  Bestilt
Storsalen  18.5.2027 19:00 - 22:30:  Bestilt
Storsalen  25.5.2027 19:00 - 22:30:  Bestilt

eller

Bestilling av Storsalfoaje for Bårdar Swing Club

Storsalfoaje  27.4.2027 18:30 - 21:00:  Bestilt
Storsalfoaje  4.5.2027 18:30 - 21:00:  Bestilt
Storsalfoaje  11.5.2027 18:30 - 21:00:  Bestilt
Storsalfoaje  18.5.2027 18:30 - 21:00:  Bestilt
Storsalfoaje  25.5.2027 18:30 - 21:00:  Bestilt

For Bårdar så er det mer at vi får bruke samme sett av saler neste
semester på samme ukedager som vi har. Så det må vi justere inn hva
som vi har av saler. Så da er det å opprette en sal, og legge inn hvilke dager og hvilke klokkesletter vi har salen på.

Når vi er klare med planleggingen så vil vi gjerne ha en side som vi kan lage et bilde av og legge på nettsiden vår. Her er kursplanen vår for høsten 2026. https://bardarswingclub.com/kurs/kursplaner/

Og vi vil da kunne lage en tilsvarende en for våren 2027.

Og det må bli mulig å velge å importere dette til kurs, som da vi kan ha påmelding på i systemet vårt.

Hvert kurs kan da ha et "Navn" som er for eksempel "Balboa Basic" som vises i timeplanen. I beskrivelsen kan man da legge inn.

"Onsdag kl 18.00-19.30 Sal 3 på Bårdar Instituttet"

Resten må de som lager påmeldingen senere redigere etter at kursene har fått innslag.

Men kursene skal da være opprettet med Fører og følger, og la par melde seg på sammen, med mindre det er "solo jazz" invovlert i tittelen, for da er det enkelpersoner og ikke par som skal melde seg på.. Påmeldingen skal ikke være åpen når den opprettes.

Merk at her må man ha gjort klart neste semester før denne funksjonen
kan fungere. Og påmeldingen skal ikke være synlig for den skal da åpnes senere.

Så databasen for dette kan være en mer crud basert hvor vi har kurs som noe som finnes per semester, med en liste av datoer som den gjelder for, i en liste. Så jeg ønsker kun en rad per kurs per semester. Og den vil typisk hentes inn i bulk til klienten, altså alle for et semester.

Saler for semesteret kan gjerne være en json per semester, slik at det
er enkelt for klienten å jobbe med den og lagre ned og ordne. Den skal
nok kun brukes i denne visningen og da er det "allt" som skal jobbes
med en per semester. Så det er ikke noe poeng å normalisere de dataene heller.

Det betyr vel at vi bør kunne klare oss med 2 databasetabeller for dette og med et minimalt sett av crud funksjoner på toppen for å løse resten.

De som skal kunne administrere og planlegge kurs, er
instruktørkoordinatorer og styret. Vi utvider med en egen kategori
"Instruktørkoordinator" som kan settes på medlemmer i systemet
vårt (tilsvarende som festkomite, styre, mm). De kan med fordel også få lov til å opprette og vedlikeholde
hvilke siden hvor man kan opprette kurs og gjøre det som skal til for
å administrere kurs.

Siden som skal organiseres med drag&drop og administrasjon kan med fordel gjøres uten bruk av react eller noen slike rammeverk. Altså med minst mulig ekstra mikk. Siden denne skal være nærmere den opprinnelige kursbasen og være relatert til den. "Kursplanlegging" er vel et godt valg.

Jeg har opprettet en branch hvor dette kan løses i.

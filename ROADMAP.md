# Roadmap: dotnet-blazor-multihost-starter

**Status (2026-10-22):** `main` zawiera chroniony Tasks API, tryby
server-centric/client-local/offline-hybrid, Web Interactive WebAssembly/Server
oraz instalowalny template `dotnet new`. Jest to wciąż learning/reference
starter, nie produkt produkcyjny. Realne logowanie Entra i runtime smoke testy
na docelowych platformach pozostają bramką przed deklaracją gotowości
produkcyjnej.

Etapy V1 poniżej zachowują pierwotny plan i uzasadnienie. Aktualne otwarte
bramki są wymienione na końcu dokumentu.

## Aktualny stan i dowody

- Jedna solution ma hosty Web i MAUI, wspólną RCL, API, Domain/Application/
  Infrastructure, klienta, kontrakty i cztery projekty testowe.
- `Tasks` pokazuje trzy jawne tryby danych. Offline sync używa IndexedDB,
  wersjonowania, `409 Conflict` i jawnego rozstrzygnięcia.
- Web wybiera `WebAssembly` lub `Server` przez `Web:Interactivity`; MAUI
  pozostaje Blazor Hybrid.
- Repozytorium jest instalowalne przez `dotnet new install .`; generator
  zmienia nazwę projektu, app title, bundle ID i publiczne Entra IDs.
- Lokalnie przechodzą 71 testów w czterech projektach, Release build solution,
  Android Debug, Windows Release, formatowanie i kontrola snapshotu migracji.
- Browser smoke potwierdził zapis/odczyt IndexedDB po przeładowaniu w
  WebAssembly oraz Interactive Server. To nie obejmuje prawdziwego logowania.
- Workflow CI jest skonfigurowany do testów API na PostgreSQL, buildów
  Windows/Android oraz wygenerowania i budowania template'u.
- Build/test nie weryfikują prawdziwego tenant logowania, IndexedDB w każdym
  WebView ani iOS/Mac Catalyst. Nie deklaruj tych scenariuszy jako
  przetestowanych bez osobnych runtime smoke testów.

## V2 — security baseline dla wieloplatformowego Tasks

**Cel i rezultat:** zastąpiono anonimowy, współdzielony demo CRUD chronionym
server-centric przepływem. API egzekwuje dostęp, a owner wynika z tożsamości.

### Przyjęte granice

- Tryb renderowania (WebAssembly/Server/MAUI Hybrid) jest niezależny od tego,
  gdzie leży źródło prawdy dla danych.
- `Tasks` używa trybu server-centric: API egzekwuje token, scope i własność
  wiersza. UI nie przesyła ani nie wybiera owner ID.
- Wzorzec to Microsoft Entra ID + MSAL, Authorization Code + PKCE dla
  publicznych klientów, osobne client IDs dla Web i MAUI, bez client secrets
  w kodzie lub bundle.
- Client-local i offline-hybrid są osobnymi przykładami, a nie trybem
  autoryzacji dla danych serwera. Lokalny IndexedDB nie jest miejscem na
  sekrety ani wrażliwe dane.
- Offline sync używa wersjonowania i ręcznego rozstrzygania konfliktów zgodnie
  z [ADR-0003](./docs/adr/0003-data-modes-and-offline-conflicts.md).
- Starsze anonimowe Tasks są przypisane do zarezerwowanego owner sentinel w
  migracji i nie są widoczne dla żadnego Entra usera.

Szczegóły decyzji i ograniczeń platformowych opisuje
[ADR-0002](./docs/adr/0002-authentication-and-execution-modes.md).

### Pozostałe bramki V2

- API fail-closed w Development/Production, waliduje Entra tenant, issuer,
  audience, podpis/ważność tokenu, dozwolony `azp` i delegated scope.
- List/Get/Create/Complete są wymagane auth i zawsze filtrują po zweryfikowanym
  `tid` + `oid`; request body nie może nadpisać właściciela.
- Testy obejmują anonymous `401`, brak scope/nieznanego clienta `403`,
  izolację A/B, odrzucone odczyty i mutacje cudzych danych.
- WebAssembly i MAUI pobierają delegated API access token przez własne host
  adapters; Interactive Server korzysta z OIDC cookie session i serwerowego
  adaptera.
- Release MAUI wymaga HTTPS; WebAssembly, Interactive Server i native redirect
  URIs są rozdzielone i udokumentowane.
- Pozostaje ręczny runtime login/Web↔MAUI smoke test z prawdziwym Entra tenant;
  testowa identity handler nie jest jego zamiennikiem.
- Weryfikacja IndexedDB na każdej deklarowanej platformie MAUI pozostaje osobną
  bramką.

## Cel

Zbudować starter .NET, w którym jedna solution zawiera host Blazor Web i host
.NET MAUI Blazor Hybrid. Oba hosty mają korzystać z tej samej biblioteki
komponentów Razor, a przykładowy przepływ ma używać wspólnego backendu API i
trwałych danych.

„Jedna solution” oznacza wspólne miejsce do budowania, testowania i pracy nad
projektami. Nie oznacza jednego procesu: aplikacja Web, aplikacja MAUI i
ewentualny host API mogą być uruchamiane i wdrażane osobno.

## Punkt wyjścia i zasady

- Przed implementacją repozytorium zawierało początkowy `.gitignore`, licencję
  i README; nie miało solution, projektów, testów ani CI.
- `dotnet-react-starter` jest źródłem wzorców architektonicznych, a nie listą
  funkcji do skopiowania. Warto zachować jawne granice warstw, testowanie
  przypadków użycia, dokumentowanie decyzji i CI.
- Nie przenosimy Reacta ani funkcji produktu, takich jak rozbudowane
  uwierzytelnianie, 2FA, współpraca projektowa, powiadomienia, załączniki,
  workery, outbox, observability czy pipeline wdrożeniowy.
- Dla nowego projektu punktem wyjścia powinien być wspierany .NET 10 LTS,
  a nie .NET 9 używany przez checkout referencyjny. Według [polityki wsparcia
  .NET](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)
  .NET 10 jest wspierany do listopada 2028, a wsparcie .NET 9 kończy się w
  listopadzie 2026.
- Etapy są bramkami jakości, a nie datami. Nie deklarujemy obsługi platform,
  której build nie został zweryfikowany.

## Zakres pierwszego używalnego wydania

1. Jedna solution i wspólna biblioteka UI typu Razor Class Library (RCL).
2. Host Blazor Web i host MAUI Blazor Hybrid odwołują się do tej samej RCL.
3. Backend z granicami Domain, Application i Infrastructure oraz HTTP API.
4. Jeden wybrany silnik bazy danych i migracje; hosty UI nie łączą się
   bezpośrednio z bazą.
5. Jeden mały przepływ end-to-end. **Propozycja:** lista zadań, dodanie zadania
   i oznaczenie go jako ukończonego. Utworzenie w Web powinno być widoczne w
   MAUI i odwrotnie.
6. Testy logiki, API/persistence i wspólnych komponentów oraz CI dla
   zadeklarowanych targetów.

Pierwotny zakres V1 nie obejmował logowania, offline sync ani generatora
projektów. Te elementy zostały dodane później w osobnych decyzjach ADR.
Role, e-maile, 2FA, wiele modułów biznesowych i wdrożenie produkcyjne nadal
pozostają poza zakresem.

## Proponowane granice projektów

Nazwy są robocze. Dokładny zestaw projektów należy dopasować do wyniku ADR i
szkieletu wygenerowanego przez oficjalny template.

```text
Starter.slnx
src/
  Starter.Domain/              # reguły i niezmienniki domenowe
  Starter.Application/         # przypadki użycia i porty
  Starter.Infrastructure/      # EF Core, baza i adaptery
  Starter.Contracts/           # kontrakty HTTP, bez encji EF
  Starter.SharedUi/            # RCL: strony, layout i komponenty Razor
  Starter.Web/                 # host Blazor Web; API może być hostowane tutaj
  Starter.Web.Client/          # tylko jeśli ADR wybierze WebAssembly
  Starter.Maui/                # host MAUI Blazor Hybrid
tests/
  Starter.Domain.Tests/
  Starter.Application.Tests/
  Starter.Api.IntegrationTests/
  Starter.SharedUi.Tests/
```

Zależności powinny płynąć w jednym kierunku: API używa Application,
Infrastructure implementuje porty Application, a Domain nie zależy od
frameworków hosta ani persistence. UI używa kontraktów/klienta API, nie encji
EF ani projektu Infrastructure. `Starter.Web` może początkowo hostować zarówno
interfejs Web, jak i endpointy API; osobny proces API nie jest wymagany bez
konkretnej potrzeby wdrożeniowej.

Komponenty w `Starter.SharedUi` pozostają niezależne od `Microsoft.Maui`,
`HttpContext`, JavaScriptu hosta i innych szczegółów platformy. Gdy UI potrzebuje
funkcji platformowej, korzysta z małego interfejsu wstrzykiwanego przez DI.
Hosty dostarczają własne implementacje. Nie tworzymy abstrakcji dla funkcji,
której starter jeszcze nie używa.

## Etapy

### Etap 0 — decyzje i proof template'u

**Cel:** zmniejszyć ryzyko architektoniczne przed utworzeniem własnej struktury.

- Użyć oficjalnego template'u [.NET MAUI Blazor Hybrid and Web App](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-10.0)
  jako pierwszego kandydata. Template tworzy solution z hostem Web, hostem MAUI,
  współdzieloną RCL i różnymi implementacjami usług przez DI.
- Wygenerowano w ignorowanym katalogu `artifacts/` proofy wariantów `Server`
  i `WebAssembly`. Oficjalny template współdzieli RCL w obu wariantach.
  Po zakończeniu proofu katalogi tymczasowe nie są częścią wydania.

  ```powershell
  dotnet new maui-blazor-web -o .\artifacts\template-server -I Server
  dotnet new maui-blazor-web -o .\artifacts\template-wasm -I WebAssembly
  ```

- Sprawdzić wymagane SDK i workload MAUI, strukturę solution, referencje do RCL,
  build Web oraz możliwość uruchomienia wspólnego ekranu w MAUI.
- Decyzję zapisano w **[ADR-0001](./docs/adr/0001-web-interactivity.md)**:
  Web używa Interactive WebAssembly, ponieważ Web i MAUI są klientami tego
  samego HTTP API:
  - **Interactive Server:** zdarzenia interfejsu obsługuje serwer; mniejszy
    początkowy payload, ale interaktywność Web wymaga aktywnego połączenia
    SignalR i utrzymywania circuitów.
  - **Interactive WebAssembly:** kod interfejsu działa w przeglądarce po
    pobraniu aplikacji i runtime; większy początkowy payload, ale model
    klienta jest bliższy aplikacji MAUI i wywołuje API z przeglądarki.
  - **MAUI Blazor Hybrid:** komponenty Razor działają lokalnie w WebView
    aplikacji MAUI; to nie jest renderowanie WebAssembly.
- Oba tryby Web mogą współdzielić RCL. WebAssembly wymaga osobnego projektu
  `.Client`; Interactive Server nie. Tryb `Auto` pozostaje poza MVP, chyba że
  ADR wykaże konkretną korzyść.
- W decyzji uwzględnić rozmiar startowy, wymaganie stałego połączenia, topologię
  API, CORS/same-origin, skalowanie serwera oraz oczekiwany sposób pracy UI.
  WebAssembly wymaga osobnego projektu `.Client`, większego pobrania startowego
  i utrzymania API dostępnego z przeglądarki; lokalny Web hostuje API i Web
  w jednym originie. MAUI używa wspólnego klienta HTTP.
- Wybrano PostgreSQL + EF Core jako bazę docelową. Testy API mogą domyślnie
  używać SQLite in-memory, a CI ma sprawdzać migrację i golden path również
  na PostgreSQL.

**Bramka wyjścia:** ADR i provider bazy wybrane; oficjalny template potwierdza
wspólną RCL. Kompilacja pełnej implementacji pozostaje bramką V1.

### Etap 1 — solution i fundamenty

- Utworzyć właściwe projekty z zaakceptowanego template'u i jeden plik
  `Starter.slnx`.
- Ustawić `net10.0`, przypiąć SDK przez `global.json`, dodać wspólne ustawienia
  builda i `.editorconfig` tylko tam, gdzie faktycznie upraszczają utrzymanie.
- Upewnić się, że wspólny ekran startowy z RCL renderuje się w Web i MAUI.
- Dodać krótki README z wymaganiami, komendami uruchomienia i ograniczeniami.
- Skonfigurować podstawowe CI: restore, Release build i testy. Buildy MAUI
  uruchamiać tylko na runnerach obsługujących wymagany workload/target.

**Bramka wyjścia:** czysty checkout buduje solution, a oba hosty renderują
komponent z RCL bez kopiowania jego implementacji.

### Etap 2 — jeden pionowy przepływ backendu

- Zaimplementować wyłącznie proponowany przepływ `Tasks`: pobranie listy,
  utworzenie zadania i zmianę statusu.
- Umieścić niezmienniki w Domain, orkiestrację w Application, persistence w
  Infrastructure, a HTTP binding i błędy API w hoście serwerowym.
- Dodać walidację requestów i jawne mapowanie błędów. Nie wystawiać encji EF
  jako kontraktu HTTP.
- Dodać migrację bazy i konfigurację connection stringa poza repozytorium
  (np. User Secrets w development).
- Zacząć od bezpośredniego, czytelnego przepływu command/query. MediatR,
  moduły, outbox i workery odłożyć do chwili, gdy powtarzalne przypadki
  uzasadnią ich koszt.
- Dodać testy jednostkowe reguł/use case'ów oraz testy integracyjne API z
  wybranym providerem. Testcontainers rozważyć przy PostgreSQL.

**Bramka wyjścia:** test API potwierdza zapis i odczyt z trwałej bazy, a błędne
wejście nie zmienia danych.

### Etap 3 — wspólny interfejs i klient API

- Dodać do RCL współdzielony layout, nawigację, listę zadań, formularz i stany
  ładowania, pustych danych oraz błędu.
- Wspólne komponenty korzystają z interfejsu klienta aplikacyjnego/API przez
  DI; nie zawierają własnej persistence ani logiki specyficznej dla hosta.
- DTO pozostają w lekkich `Contracts`. Kod wspólnego klienta HTTP wydzielić
  tylko wtedy, gdy oba hosty rzeczywiście współdzielą implementację.
- Dodać testy komponentów RCL dla stanu początkowego, walidacji i obsługi
  błędów.

**Bramka wyjścia:** ten sam komponent i ten sam przepływ są używane przez Web
oraz MAUI; logika nie jest kopiowana do dwóch zestawów stron.

### Etap 4 — konfiguracja i parytet hostów

- Zastosować tryb renderowania z ADR-0001. Rejestrować usługi hosta w jego
  composition root, nie warunkować wspólnych komponentów typem platformy.
- Skonfigurować adres API osobno dla Web, Windows i emulatorów MAUI; nie
  zakładać, że `localhost` z urządzenia oznacza komputer deweloperski.
- Dla WebAssembly skonfigurować dostęp przeglądarki do API: preferować
  same-origin/reverse proxy albo jawnie ograniczoną politykę CORS.
- Sprawdzić scenariusz: utworzenie zadania w Web, odczyt w MAUI, ukończenie w
  MAUI i odświeżony status w Web.
- Dodać tylko minimalne smoke testy dla Web i udokumentowany ręczny test MAUI.
  Automatyzację urządzeń zostawić do czasu ustalenia wspieranych targetów.

**Bramka wyjścia:** oba hosty używają tego samego API i bazy, a zmiana danych
w jednym hoście jest widoczna w drugim po ponownym odczycie.

### Etap 5 — jakość, dokumentacja i pierwsze wydanie (pierwotny plan V1)

- CI na pull requestach: restore, Release build, unit tests, API integration
  tests, testy komponentów i publikacja/build Web.
- Dodać build MAUI dla wybranego targetu na runnerze, który go obsługuje.
  iOS/macOS wymagające środowiska Apple dodać do macOS CI dopiero po
  zadeklarowaniu tych platform.
- Udokumentować architekturę, dodawanie kolejnego przepływu, konfigurację
  bazy/API, adresy dla emulatora i ograniczenia trybu Web.
- Sprawdzić procedurę od zera: klon, restore, konfiguracja bazy, uruchomienie
  API/Web i uruchomienie MAUI.

**Bramka pierwotnego V1:** była to bramka dla hostów i server-centric Tasks.
Późniejsze tryby danych, generator template'u i rzeczywiste logowanie mają
osobne bramki w aktualnym stanie powyżej.

## Definition of Done V1

- Jedna solution obejmuje projekty Web, MAUI, wspólną RCL, backend i testy.
- Web oraz MAUI renderują ten sam komponent/proces UI z RCL.
- Jeden przepływ zapisuje i odczytuje dane przez ten sam backend API i bazę.
- Logika domenowa nie jest zdublowana w hostach UI; projekty klienckie nie
  zależą od Infrastructure ani encji EF.
- Testy jednostkowe, integracyjne API i komponentów przechodzą w CI.
- README pozwala uruchomić oba hosty i wskazuje wymagane workloady, konfigurację
  bazy, URL API oraz platformy faktycznie zweryfikowane.
- Nie ma przypadkowo dodanych sekretów ani nieużywanych funkcji skopiowanych z
  większego startera.

## Otwarte bramki przed deklaracją gotowości produkcyjnej

- Wykonać realny sign-in i sign-out dla WebAssembly, Interactive Server oraz
  każdej docelowej platformy MAUI z własnymi rejestracjami Entra.
- Sprawdzić IndexedDB w przeglądarce i WebView na urządzeniach, w tym
  zachowanie po restarcie aplikacji i utracie połączenia.
- Dodać i przetestować deployment, telemetry/observability oraz limity
  rozproszone, jeśli aplikacja ma działać w wielu instancjach.
- Rozważać dodatkowych providerów identity/bazy tylko przy konkretnym
  przypadku użycia; obecny template pozostaje Entra + PostgreSQL.
- Dodawać kolejne moduły biznesowe dopiero wtedy, gdy pokażą wartość wzorca
  VSA poza przykładowym Tasks.

## Źródła

- [Repozytorium referencyjne dotnet-react-starter](https://github.com/Karol-Malecki-dev/dotnet-react-starter)
- [Microsoft Learn: MAUI Blazor Hybrid z Blazor Web App](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-10.0)
- [Microsoft Learn: tryby renderowania Blazora](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0)
- [Microsoft Learn: wersje .NET i wsparcie](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)

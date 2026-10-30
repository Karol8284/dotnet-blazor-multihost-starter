# Roadmap: dotnet-blazor-multihost-starter

**Stan lokalnego checkoutu (odczyt 2026-10-30):** branch
`fix/template-naming`, commit `d2590c5`, śledzi `origin/fix/template-naming`.
PR #2 do `main` jest otwarty. GitHub Actions run 14 dla tego commita przeszedł:
Web, testy API na PostgreSQL, buildy MAUI Windows i Android oraz generowanie i
buildy template'u dla nazw PascalCase, kebab-case i `Contoso.Tasks`, łącznie
ze sprawdzeniem nazw assembly i ścieżek assetów. Trzy wątki review są
rozwiązane. Nie utworzono taga ani GitHub Release.

Checkout zawiera chronione Tasks API, tryby
server-centric/client-local/offline-hybrid, Web Interactive
WebAssembly/Server oraz instalowalny template `dotnet new`.

**Cel najbliższego etapu:** doprowadzić template do sprawdzonego, czytelnego
startera referencyjnego, na którym można rozpocząć implementację aplikacji.
Obsługa nazw PascalCase, kebab-case i z kropkami, buildy MAUI Windows oraz
runtime smoke `/tasks/local` w MAUI Windows Debug są potwierdzone. Test runtime
potwierdzono ręcznie 2026-10-30. GitHub Actions run 14 dla PR #2 na commicie
`d2590c5` przeszedł 2026-10-30; nie jest to deklaracja gotowości produkcyjnej.

Etapy V1 poniżej zachowują pierwotny plan i uzasadnienie. Aktualne otwarte
bramki są wymienione na końcu dokumentu.

## Aktywna roadmapa po V2

Kolejność poniżej skupia się najpierw na sprawdzeniu tego, co już oferuje
template, a dopiero później na rozszerzaniu kodu. Nie dodawaj nowego modułu
biznesowego, zanim te bramki nie pokażą konkretnej luki.

### Akceptacja MAUI Windows runtime

Świeże generacje i buildy template'u są sprawdzone dla PascalCase i
kebab-case. Bieżące wyniki acceptance z 2026-10-28:

| Kontrola | Wynik |
|---|---|
| Izolacja i nazwy | **Pass** — custom hive; wygenerowano `ContosoTasks` i `blazor-multihost-starter`, bez zmiany globalnego template hive. |
| Solution i ProjectReference | **Pass** — obie solution listują po 13 projektów; wszystkie wymienione pliki `.csproj` istnieją, w tym MAUI `<name>.Maui/<name>.csproj`. |
| Placeholder scan | **Pass** — przy podanych testowych GUID-ach nie znaleziono unresolved `__[A-Z0-9_]+__` w obu wygenerowanych projektach. |
| Web Release (PascalCase i kebab-case) | **Pass** — oba buildy, 0 warnings, 0 errors. |
| MAUI Windows Release (PascalCase i kebab-case) | **Pass** — oba buildy `net10.0-windows10.0.19041.0`, 0 warnings, 0 errors po zmianie root component rejestracji z XAML na code-behind. |
| Source solution Release i MAUI Windows Release | **Pass** — 0 warnings, 0 errors. |
| Testy source | **Pass** — Domain 11, Application 16, API 27, Shared 18; razem 72 passed, 0 failed. API użyło domyślnego SQLite, bez Dockera. |
| WebAssembly, `/tasks/local` | **Pass** — `200`; testowe lokalne zadanie przetrwało reload w IndexedDB. |
| WebAssembly, `/api/tasks` bez Entra/DB | **Pass** — `503 application/problem+json`, bez anonimowego sukcesu. |
| Interactive Server, `/tasks/local` | **Pass** — `200`; testowe lokalne zadanie przetrwało reload w IndexedDB. |
| Interactive Server, `/api/tasks` bez Entra/DB | **Pass** — `503 application/problem+json`. |
| MAUI Windows runtime i WebView IndexedDB | **Nie sprawdzono** — build jest potwierdzony, ale aplikacji nie uruchomiono interaktywnie. |
| Android/iOS/Mac Catalyst runtime | **Nie sprawdzono** w tej akceptacji. Build CI nie jest runtime smoke testem. |

Przeglądarkowe wpisy testowe używały unikalnych localhost originów. Bazy
IndexedDB przypisane do tych originów usunięto po próbie. Hosty lokalne
zatrzymano. Zwykła akcja Playwright `.click()` w zintegrowanej przeglądarce
przekraczała timeout stabilności elementu; handler UI uruchomiono przez
programowe `button.click()`. Oznacza to, że zapis i odczyt po reloadzie są
sprawdzone, ale osobny test pointer/keyboard nie został potwierdzony.

**Rozstrzygnięte podczas acceptance:** osobny symbol zachowuje surową wartość
`--name` w project paths/solution, a obliczana forma bezpieczna służy
namespace'om C#. MAUI folder to `src/<name>.Maui`, a wygenerowany plik
projektu to `src/<name>.Maui/<name>.csproj`, zgodnie z oczekiwanym identity
assembly przez XAML compiler. Rejestracja współdzielonego `Routes` odbywa się
w code-behind MAUI. CI generuje PascalCase i kebab-case, sprawdza solution
paths/placeholders i buduje Web oraz MAUI Windows w obu wariantach.

**Wynik runtime smoke (2026-10-30):** MAUI Windows Debug dla
`net10.0-windows10.0.19041.0` z SDK `10.0.403` zbudował się bez ostrzeżeń i
błędów. Aplikacja uruchomiła się i odpowiadała. Ręczny smoke test potwierdził,
zadanie dodane w `/tasks/local` pozostało widoczne po całkowitym zamknięciu i
ponownym uruchomieniu aplikacji. To potwierdza wyłącznie Windows Debug i lokalny
IndexedDB; nie weryfikuje logowania, API ani platform mobilnych.

**Wynik bramki (2026-10-30):** CI run 12 dla PR #2 na commicie `31ad55b`
przeszedł; Web local-only działa w obu trybach, a runtime smoke dla MAUI Windows
Debug jest potwierdzony. Bramka acceptance jest zamknięta dla tego commita;
każda kolejna zmiana wymaga ponownego CI. README i `GETTING_STARTED.md` opisują
tylko kroki i platformy faktycznie sprawdzone.

**Wyniki acceptance (2026-10-28):** wygenerowano świeże projekty
`ContosoTasks` i `blazor-multihost-starter` z izolowanego template hive na SDK
`10.0.403`, z testowymi Entra GUID-ami i bez sekretów. Obie solution zawierają
13 istniejących projektów, placeholder scan nie znalazł nierozwiązanych
wartości, a Web Release i MAUI Windows Release przeszły dla obu nazw z
0 warnings/0 errors. Wygenerowane MAUI projekty mają ścieżkę
`src/<name>.Maui/<name>.csproj`. Source solution Release i MAUI Windows Release
przeszły bez ostrzeżeń/błędów; wszystkie 72 testy w czterech projektach
przeszły, a API testy użyły domyślnego SQLite (bez Dockera).

W przeglądarce `/tasks/local` zwróciło `200` w WebAssembly i Interactive Server;
testowe zadania IndexedDB przetrwały reload, a `/api/tasks` bez Entra/DB
zwróciło `503 application/problem+json`. To sprawdza development fallback i
IndexedDB w przeglądarce, ale nie logowanie ani runtime MAUI.

Zapis po stronie UI wywołano programowo przez DOM `button.click()`, ponieważ
zintegrowany Playwright nie przechodził kontroli stabilności przy zwykłym
pointer click. Warto powtórzyć smoke test interakcją użytkownika/na wybranym
urządzeniu, zanim uzna się ten szczegół UX za zweryfikowany.

Wcześniejsze eksperymenty wykryły problemy z podstawianiem nazw w solution,
ProjectReference i MAUI XAML; zostały one już rozwiązane.
Template zachowuje surową wartość `--name` w ścieżkach i solution, a C#
namespace'y normalizuje osobno. Regresyjny CI generuje PascalCase i kebab-case,
sprawdza ścieżki oraz placeholdery i buduje Web oraz MAUI Windows dla obu.

### Następnie — przegląd konfiguracji i bezpieczeństwa

To osobne zadanie po uzgodnieniu dokładnych wartości, które budzą wątpliwość;
na tym etapie nie zmieniaj ich automatycznie.

- Zinwentaryzować konfigurację identity i API w Web, Web.Client, MAUI,
  `.template.config`, plikach `appsettings` i wygenerowanym projekcie.
- Rozróżnić publiczne identyfikatory/konfigurację klienta od sekretów i
  prywatnych kluczy. Publiczny client ID, tenant ID, URI przekierowania czy
  publiczny klucz/certyfikat nie są automatycznie sekretem. Klucz prywatny,
  client secret, hasło lub token wymaga ochrony i nie powinien trafić do
  klienta ani repozytorium.
- Sprawdzić, czy prawdziwy sekret przypadkiem nie został już opublikowany.
  Samo usunięcie go z bieżącego pliku nie unieważnia ujawnionego sekretu —
  w takim przypadku trzeba go unieważnić/wymienić i ocenić historię repo.
- Zaktualizować instrukcje i testy konfiguracji po ustaleniu konkretnego
  problemu. Nie usuwać prawidłowych publicznych identyfikatorów z klienta tylko
  dlatego, że nazywają się „key” lub są widoczne w bundle.

**Bramka wyjścia:** tabela konfiguracji wyjaśnia dla każdej wartości, czy jest
publiczna, poufna, gdzie jest używana i jak jest dostarczana; test potwierdza,
że wymagane sekrety nie są generowane do Web/MAUI client bundle.

### Potem — runtime uwierzytelniania i danych

Wymaga testowego tenant'u Entra, zarejestrowanych callbacków, testowej bazy
PostgreSQL i świadomie wybranych platform. Sekrety pozostają w lokalnym
User Secrets/secret managerze — nie należy wklejać ich do rozmowy ani commitować.

- Sprawdzić sign-in/sign-out i wywołanie chronionego API w WebAssembly.
- Sprawdzić OIDC/cookie i wywołanie Application w Interactive Server.
- Sprawdzić MAUI sign-in, callback/broker i chronione API na wybranym
  urządzeniu/emulatorze.
- Wykonać Web↔MAUI golden path oraz offline sync: `Pending`, `Synced`,
  nieaktualna wersja `409 Conflict` i obie decyzje rozstrzygnięcia.
- Zapisać osobny wynik dla każdego hosta/platformy; testowa identity handler
  nie zastępuje testu z prawdziwym Entra.

**Bramka wyjścia:** każdy deklarowany host przechodzi zapisany smoke test z
testowym użytkownikiem i bazą; README wskazuje dokładnie sprawdzone platformy
i nadal nazywa ograniczenia.

### Produkcyjność — oddzielna decyzja, nie domyślny następny sprint

Nie deklarować startera produkcyjnym bez wskazania konkretnego środowiska,
platform, sposobu wdrożenia i modelu operacyjnego. Dopiero po takim wyborze
zaplanuj HTTPS/redirect URIs, obsługę sekretów, deployment, backup/migracje,
telemetrię, alerty, skalowanie oraz rozproszony rate limiter. Obecny limit API
jest per proces i nie jest wystarczającą polityką dla wielu instancji.

Na teraz poza aktywnym zakresem pozostają: dodatkowe moduły biznesowe,
alternatywni providerzy identity/database, background sync oraz nowe platformy
mobile. Wróć do nich tylko, jeśli wynik acceptance pass albo konkretny cel
aplikacji uzasadni ich koszt.

## Aktualny stan i dowody

- Jedna solution ma hosty Web i MAUI, wspólną RCL, API, Domain/Application/
  Infrastructure, klienta, kontrakty i cztery projekty testowe.
- `Tasks` pokazuje trzy jawne tryby danych. Offline sync używa IndexedDB,
  wersjonowania, `409 Conflict` i jawnego rozstrzygnięcia.
- Web wybiera `WebAssembly` lub `Server` przez `Web:Interactivity`; MAUI
  pozostaje Blazor Hybrid.
- Repozytorium jest instalowalne przez `dotnet new install .`; generator
  zmienia nazwę projektu, app title, bundle ID i publiczne Entra IDs.
- Lokalnie przechodzą 72 testy w czterech projektach, Release build solution,
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

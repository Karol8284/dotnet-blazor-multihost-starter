# Roadmap: dotnet-blazor-multihost-starter

**Status:** implementacja V1 gotowa do review na branchu
`feature/v1-blazor-multihost`; CI przeszedł, pełny runtime smoke test obu hostów
pozostaje do wykonania.
**Data punktu odniesienia:** 2026-10-20

## Aktualny dowód realizacji V1

- Jedna solution zawiera hosty Web i MAUI, współdzieloną RCL, API, warstwy
  Domain/Application/Infrastructure, kontrakty, klienta API i projekty testowe.
- Moduł `Tasks` pokazuje pionowe slice'y listowania, pobrania, tworzenia i
  ukończenia; oba hosty korzystają z jednej strony Razor i klienta HTTP.
- Web hostuje WebAssembly oraz API z tego samego originu; MAUI korzysta z
  konfigurowanego adresu API. W Web wyłączono prerender komponentów
  interaktywnych, aby pobieranie danych rozpoczynało się w kliencie.
- Lokalnie potwierdzono kompilację Release całej solution, build MAUI Windows
  Release i Android Debug, formatowanie C# oraz 22 testy w czterech projektach.
- [GitHub Actions CI](https://github.com/Karol8284/dotnet-blazor-multihost-starter/actions/runs/39484501308)
  przeszedł na commicie `56d65ee`: build Web, testy, testy API z migracją na
  PostgreSQL service oraz build MAUI Windows Release.
- Testy API uruchomiono lokalnie na SQLite in-memory. Lokalny Docker nie był
  dostępny; potwierdzenie na PostgreSQL pochodzi z GitHub Actions CI.
- Pełny runtime smoke test w przeglądarce i w uruchomionej aplikacji MAUI nie
  został wykonany. Kompilacja obu hostów i API/component tests nie zastępują
  takiego sprawdzenia.

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

Pierwsze wydanie nie obejmuje logowania, ról, e-maili, 2FA, wielu modułów
biznesowych, offline sync, generatorów projektów ani wdrożenia produkcyjnego.
Można je dodać później, jeśli pojawi się konkretny przypadek użycia.

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

### Etap 5 — jakość, dokumentacja i pierwsze wydanie

- CI na pull requestach: restore, Release build, unit tests, API integration
  tests, testy komponentów i publikacja/build Web.
- Dodać build MAUI dla wybranego targetu na runnerze, który go obsługuje.
  iOS/macOS wymagające środowiska Apple dodać do macOS CI dopiero po
  zadeklarowaniu tych platform.
- Udokumentować architekturę, dodawanie kolejnego przepływu, konfigurację
  bazy/API, adresy dla emulatora i ograniczenia trybu Web.
- Sprawdzić procedurę od zera: klon, restore, konfiguracja bazy, uruchomienie
  API/Web i uruchomienie MAUI.

**Bramka wydania V1:** wszystkie kroki Definition of Done poniżej przechodzą
na czystym checkoutcie. Status „produkcyjny” nie jest deklarowany bez osobnego
zakresu bezpieczeństwa, wdrożenia i operacji.

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

## Poza V1 — możliwe kolejne decyzje

Rozważać dopiero po użyciu pierwszego przepływu:

- uwierzytelnianie i autoryzacja API;
- natywne funkcje urządzeń (np. storage, kamera, powiadomienia);
- offline i synchronizacja;
- kolejne moduły biznesowe, worker/outbox lub pliki;
- pełne E2E dla urządzeń, deployment, telemetry i observability;
- template/source generator, jeśli powstaną co najmniej dwa rzeczywiste
  przepływy i będzie wiadomo, co warto automatyzować.

## Źródła

- [Repozytorium referencyjne dotnet-react-starter](https://github.com/Karol-Malecki-dev/dotnet-react-starter)
- [Microsoft Learn: MAUI Blazor Hybrid z Blazor Web App](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/maui-blazor-web-app?view=aspnetcore-10.0)
- [Microsoft Learn: tryby renderowania Blazora](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0)
- [Microsoft Learn: wersje .NET i wsparcie](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support)

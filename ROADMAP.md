# Roadmap

## Status produktu

Ten projekt jest referencyjnym starterem i instalowalnym szablonem `dotnet
new`, nie gotową aplikacją biznesową. Obecny baseline obejmuje współdzielone
Blazor Web/MAUI UI, chronione API, PostgreSQL i trzy jawne tryby danych dla
przykładu `Tasks`.

Każde twierdzenie o gotowości należy czytać przez pryzmat jego dowodu:

| Status | Znaczenie |
| --- | --- |
| **PASS** | Kontrola została wykonana dla wskazanego commita i zakresu. |
| **BLOCKED** | Zakres jest zablokowany przez brak konfiguracji, środowiska lub decyzji. |
| **NOT RUN** | Zakres nie został zweryfikowany. |
| **N/A** | Kontrola nie dotyczy danego wydania. |

Build lub test automatyczny nie oznacza działającego logowania, działania
platformy ani wdrożenia produkcyjnego.

## Aktualna bramka: release referencyjnego startera

Celem najbliższego wydania jest czysty, zrozumiały i powtarzalny template.
Wydanie nie powinno deklarować:

- zweryfikowanego rzeczywistego logowania Entra;
- działania MAUI na platformach, na których wykonano tylko build;
- gotowości produkcyjnej;
- udanego wdrożenia Azure.

### Warunki wydania

| Kontrola | Status przed wydaniem |
| --- | --- |
| Aktualny workflow CI przechodzi dla Web, API, testów i kontroli migracji | Musi być **PASS** |
| CI buduje MAUI Windows oraz Android | Musi być **PASS** |
| CI generuje i buduje template dla nazw PascalCase, kebab-case i z kropkami | Musi być **PASS** |
| Dokumentacja zgadza się z komendami, konfiguracją i faktycznymi plikami | Musi być **PASS** |
| Wygenerowany klient nie zawiera sekretów serwera ani connection stringów | Musi być **PASS** |
| Live sign-in Entra i golden path na zadeklarowanych hostach | **BLOCKED / NOT RUN**, jawnie opisane jako ograniczenie |
| Wdrożenie Azure | **NOT RUN**, opcjonalne i osobno akceptowane |

Dowód CI musi pochodzić z bieżącego commita release candidate. Historyczne
wyniki nie zastępują nowego uruchomienia.

## Co jest już zrealizowane

### V1 — baseline multi-host

- Jedna solution zawiera Web, MAUI, wspólny RCL, backend, persistence i testy.
- `Tasks` obsługuje utworzenie, listę i ukończenie zadania.
- Granice Domain/Application/Infrastructure/Contracts są jawne.
- PostgreSQL i EF Core mają migrowany model danych.

### V2 — granice bezpieczeństwa

- Serwerowe `Tasks` wymagają tokenu, scope, zweryfikowanego tenant/klienta
  oraz właściciela wyznaczanego z `tid` + `oid`.
- WebAssembly i MAUI mają osobne adaptery MSAL; Interactive Server używa
  OIDC/cookie.
- Dane lokalne i synchronizowane są oddzielone od danych autorytatywnego API.
- Sync wersjonuje snapshoty, zwraca `409 Conflict` i wymaga jawnej decyzji.
- Limity listy i rate limiter chronią przykładowe API; limiter pozostaje
  procesowy.

Decyzje: [ADR-0002](docs/adr/0002-authentication-and-execution-modes.md) i
[ADR-0003](docs/adr/0003-data-modes-and-offline-conflicts.md).

### V3 — template referencyjny

Template:

- instaluje się przez `dotnet new install .`;
- parametryzuje nazwę, tytuł, bundle ID i publiczne identyfikatory Entra;
- zachowuje osobno nazwę ścieżki projektu i bezpieczną nazwę C#;
- generuje osobny projekt MAUI o prawidłowej nazwie;
- nie przyjmuje `ServerClientSecret`;
- ma automatyczny smoke CI generacji dla trzech form nazwy.

Granice template'u są zapisane w
[ADR-0004](docs/adr/0004-template-boundaries-and-baseline-architecture.md).

## Aktywna praca: V3 runtime acceptance

Automatyczne bramki pokrywają build i testy, ale nie są dowodem realnego
przepływu uwierzytelniania.

### Pozostałe kontrole

1. WebAssembly: Entra sign-in, chronione list/create/complete, sign-out.
2. Interactive Server: OIDC/cookie, odczyt i zapis przez Application.
3. MAUI Windows: broker/callback i chronione API.
4. Android emulator: callback/broker, osiągalny adres API i chronione API.
5. Golden path: ten sam użytkownik i baza dla Web ↔ MAUI.
6. Offline: `Pending` → `Synced` w runtime po realnym sign-in.
7. Pointer/touch i keyboard smoke na hostach deklarowanych jako sprawdzone.

Testowy identity handler i testy konfliktów nie zastępują tych kontroli.
Procedurę wykonania opisuje
[GETTING_STARTED.md](docs/GETTING_STARTED.md#manual-authenticated-smoke-test).

### Status platform

| Platforma | Build w CI | Realny runtime |
| --- | --- | --- |
| WebAssembly | tak | **NOT RUN** dla live Entra |
| Interactive Server | tak | **NOT RUN** dla live Entra |
| MAUI Windows | tak | lokalny tryb przechowywania ma historyczny smoke; live Entra **NOT RUN** |
| MAUI Android | tak | **NOT RUN** |
| iOS / Mac Catalyst | brak deklarowanej bramki | **NOT RUN** |

## V4 — twardy baseline wielokrotnego użytku

Zakres V4 obejmuje health/readiness, jawne migracje, runbooki, kontrolę
placeholderów i świeżą generację template'u. Te elementy są zaimplementowane,
ale przed oznaczeniem V4 jako zamknięte należy ponowić pełny zestaw kontroli
na release candidate.

Operacje opisuje [OPERATIONS.md](docs/OPERATIONS.md).

Nie dodawać kolejnych opcji generatora, providerów bazy ani identity bez
konkretnego, powtarzalnego przypadku użycia i własnych testów regresji.

## V5 — opcjonalny pokaz Azure

W repozytorium znajdują się Bicep i ręcznie uruchamiany workflow wdrożeniowy.
Nie stanowią one aktualnie dowodu działającego wdrożenia.

Przed użyciem V5 trzeba osobno zaakceptować:

- aktualny koszt i dostępność SKU w wybranej subskrypcji/regionie;
- uprawnienia i OIDC federation;
- rejestracje oraz HTTPS redirect URI dla Entra;
- migracje, backup/restore i rollback;
- teardown po pokazie.

Szczegóły: [AZURE_SHOWCASE.md](docs/AZURE_SHOWCASE.md).

## V6 — konkretna aplikacja użytkowników

To etap dla rzeczywistego produktu, a nie następna domyślna funkcja template'u.
Wymaga osobnej klasyfikacji danych, wymagań prywatności, dostępności, RPO/RTO,
SLA, regionu, dystrybucji, skali, budżetu i runbooku incydentowego.

## Granice rozwoju

- Zachować trzy tryby danych jako osobne przykłady.
- Nie umieszczać sekretów w klientach, template parameters, logach ani rozmowach.
- Nie wykonywać płatnego wdrożenia, zmiany RBAC ani teardown bez osobnej zgody.
- Nie rozszerzać zakresu platform o build bez runtime acceptance.
- Nie zaliczać historycznego wyniku jako dowodu dla nowego commita.
- Decyzje architektoniczne zapisywać w ADR, procedury w runbookach, bieżące bramki tutaj.

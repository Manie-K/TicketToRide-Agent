# Ticket to Ride: Europe – wersja Unity (prototyp wizualny)

Wizualna warstwa gry planszowej *Ticket to Ride: Europe* zbudowana w Unity 2D.
Logika gry pochodzi z silnika napisanego w C# (`Core/CoreEngine`).

> **Status: prototyp w budowie.** Plansza, miasta, trasy i szkielet interfejsu są gotowe
> i wyświetlają się w scenie. Gra **nie jest jeszcze grywalna**: interfejs pokazuje wartości
> tymczasowe, a kliknięcia nie są jeszcze połączone z logiką gry.

---

## 1. Wymagania

- **Unity 6.4 (6000.4.4f1)** zainstalowane przez Unity Hub (zalecana ta sama wersja)

## 2. Pobranie projektu

**Uwaga (Windows):** Zalecane jest sklonowanie repozytorium do **krótkiej ścieżki**, np. `C:\TTR`.
Przy długich, głęboko zagnieżdżonych ścieżkach Unity może zgłaszać błędy
`DirectoryNotFoundException`.

## 3. Otwarcie projektu

1. Unity Hub → **Add → Add project from disk**.
2. Wskaż folder `Unity/TicketToRideEurope`.
3. Otwórz projekt i poczekaj na pierwszy import – może trwać kilka minut.

## 4. Uruchomienie sceny

1. W panelu **Project** wejdź w `Assets/Scenes`.
2. Kliknij dwa razy **`SampleScene`**.
   (Jeśli po starcie widzisz pustą scenę „Untitled" z samą kamerą, to nie jest ta scena)

Następnie należy przełączyć się z **Scene** na **Game** u góry, aby całość się wyświetlała.

## 5. Ustawienie widoku Game

Interfejs jest projektowany pod rozdzielczość **Full HD (1920×1080)** (przy „Free Aspect" panele i mapa mogą wyglądać na przesunięte lub ucięte), którą należy ustawić w następujący sposób:

1. Zakładka **Game** → lista proporcji (domyślnie „Free Aspect") → wybierz
   **Full HD (1920x1080)**.
2. Suwakiem **Scale** (obok) przeskaluj podgląd tak, by cały ekran mieścił się w oknie
   (zwykle ok. 0.4–0.7x), najlepiej maksymalnie na lewo. (To tylko powiększenie podglądu w edytorze, nie zmienia samej gry.)
---

## 6. Co jest w scenie

| Obiekt w Hierarchy | Opis |
|---|---|
| `Board / Europe` | Grafika mapy Europy (zwykły obiekt 2D, nie element UI) |
| `MapOverlay` | Nakładka na mapę, współrzędne w układzie świata gry |
| ↳ znaczniki miast | Kropki w miejscach miast (generowane) |
| ↳ `RouteLines` | Trasy między miastami (generowane) |
| `Canvas / GameUI` | Interfejs |
| ↳ `TurnBanner` | Baner tury na środku u góry |
| ↳ `PlayerPanel` | Pionowa lista graczy po lewej |
| ↳ `CardsPanel` | Talia i 5 odkrytych kart po prawej |
| ↳ `PlayerDetailPanel` | Informacje o graczu (punkty, wagony, dworce) w lewym dolnym rogu |

## 7. Co działa, a czego jeszcze nie ma

**Gotowe**
- Silnik logiki gry zaimportowany do Unity (czyste klasy C#) i kompilujący się bez błędów
- Mapa, znaczniki miast i trasy wyświetlane z danych (kolory, długość, tunele, lokomotywy,
  podwójne trasy obok siebie)
- Szkielet interfejsu z elementami tymczasowymi

**Jeszcze nie działa**
- Klikanie w trasy i łączenie akcji z `GameManager`
- Prawdziwe dane w interfejsie (liczba kart w talii, gracze, wyniki, tura są wpisane na sztywno)
- Widok ręki gracza (jego kart i biletów)
- Pętla gry: tury, zmiana gracza, punktacja

---

## 8. Dane gry (ScriptableObjects)

Miasta i trasy są plikami danych w `Assets/Scripts/GameData`.

- **`Cities/`** – `CityData`: nazwa miasta i `Board Position` (współrzędne na mapie)
- **`Routes/`** – `RouteData`: `Origin`, `Destination` (referencje do miast), `Length`,
  `Color`, `Is Tunnel`, `Locomotives Needed`

**Zmiana lub dodanie trasy:** edytuj istniejący plik `RouteData` albo zduplikuj go (Ctrl+D)
i zmień pola. Potem wygeneruj trasy ponownie (patrz niżej). Kolejność `Origin`/`Destination`
nie ma znaczenia dla wyglądu.

**Dodanie miasta:** dodaj nowy plik `CityData`, uzupełnij `Board Position`, a następnie
dopisz go do tablicy **Cities** w komponencie *City Marker Generator* (obiekt `MapOverlay`).

## 9. Narzędzia w edytorze

Obiekty pod `MapOverlay` są **generowane skryptami**. Nie edytuj ich ręcznie –
przy kolejnym generowaniu zostaną usunięte i zbudowane od nowa.

| Co | Gdzie | Jak |
|---|---|---|
| Znaczniki miast | Obiekt `MapOverlay`, komponent *City Marker Generator* | ⋮ → **Generate Markers** |
| Trasy | Obiekt `RouteLines`, komponent *Route Line Generator* | ⋮ → **Generate Routes** |
| Zmiana nazw plików tras | Menu **Ticket to Ride → Rename Routes** | Nadaje nazwy `Route_<Origin>_<Destination>` |

**Wygląd tras** ustawiasz w Inspectorze obiektu `RouteLines`:
- lista **Color Table** – kolor każdego koloru trasy (kliknij kwadracik i wybierz odcień),
- grubości obwódek zwykłych tras i tuneli,
- po każdej zmianie kliknij ponownie **Generate Routes**.

Oznaczenia na mapie: tunel ma podwójną obwódkę (czarna + jasny pierścień),
litera **L** na segmencie oznacza wymaganą lokomotywę.

---

## 10. Struktura projektu

```
Unity/TicketToRideEurope/
├── Assets/
│   ├── Art/Board/        grafika mapy
│   ├── Prefabs/          m.in. CityMarker
│   ├── Scenes/           SampleScene
│   ├── Scripts/
│   │   ├── GameLogic/    silnik gry (kopia kodu z Core/CoreEngine)
│   │   ├── GameData/     dane miast i tras
│   │   ├── Views/        generatory i skrypty wizualne
│   │   └── Editor/       narzędzia edytora
│   └── Sprites/          sprite kwadratu używany do segmentów tras
├── Packages/             manifest.json – lista pakietów (musi być w repozytorium)
└── ProjectSettings/
```

`Assets/Scripts/GameLogic` to **kopia** kodu z `Core/CoreEngine` – zmiany w jednym miejscu
nie przenoszą się automatycznie do drugiego.

## 11. Znane problemy

| Objaw | Przyczyna / rozwiązanie |
|---|---|
| Pusta scena „Untitled" po starcie | Otwórz `Assets/Scenes/SampleScene` |
| Różowe kropki i błąd `TMPro could not be found` | Brakuje folderu `Packages` lub plików `.meta`. Pobierz najnowszy `dev` i sprawdź, czy `.gitignore` nie ignoruje `*.meta` ani `Packages` |
| `DirectoryNotFoundException` przy imporcie | Zbyt długa ścieżka – przenieś projekt bliżej dysku, np. `C:\TTR` |
| Ostrzeżenia `CS8632` (nullable) | Niegroźne |
| Ostrzeżenie „Input Manager is marked for deprecation" | Niegroźne |
| Niebieskie kwadraty z literą „T" w widoku Scene | Ikonki edytora (TextMeshPro). Wyłącz **Gizmos** w widoku Scene; w Game ich nie ma |
| Unity wchodzi w Safe Mode | Sprawdź Console; po pierwszym imporcie często pomaga ponowne otwarcie projektu |

## 12. Git

- Pracujemy na gałęzi `dev`.
- Foldery `Library`, `Temp`, `Logs`, `UserSettings` i pliki `.csproj`/`.slnx` **nie** trafiają
  do repozytorium (Unity odtwarza je sam).
- Do repozytorium **muszą** trafiać: `Assets` (razem z plikami `.meta`), `Packages`,
  `ProjectSettings`.

## 13. Plan dalszych prac

1. Podłączenie kliknięć w trasy do logiki gry (`GameManager`)
2. Pokazywanie w interfejsie prawdziwych danych z `GameManager`
3. Widok ręki gracza
4. Pętla gry (tury, zmiana gracza, punktacja)
5. Uproszczona grafika mapy (kontur lądu i wody)
6. Nazwy miast jako osobne etykiety (położenie, kolor, czcionka i rozmiar do ustawienia)

## 14. Grafika

Obecna grafika mapy pochodzi z oficjalnej gry *Ticket to Ride* (Days of Wonder / Asmodee)
i jest użyta wyłącznie w celach edukacyjnych. Planowana jest jej wymiana na własną.

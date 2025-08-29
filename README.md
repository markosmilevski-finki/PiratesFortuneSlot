# PiratesFortuneSlot

## Опис на проектот

**PiratesFortuneSlot** е слот-игра со пиратска тематика, развиена во **C#** користејќи **Windows Forms**. 

Играта симулира слот-машина со мрежа од **4 реда и 5 колони**, каде играчите генерираат симболи за да создадат **кластери** од 4 или повеќе исти симболи за да освојат добивки. Играта вклучува анимација на симболи, лабели, пресметки на салдо, аудио-визуелни ефекти, како и бонус режим.

---

## Функционалности на апликацијата

- **Основна игра**:
  - Играчот поставува облог (0.10 до 1000) и ги врти симболите.
  - Победнички кластери (4+ исти соседни симболи) носат добивки.
  - Симболите од кластерот се отстрануваат, а нови паѓаат за дополнителни шанси на  добивка.
- **Бонус-режим**:
  - Се активира со 3+ **Scatter** симболи, давајќи 10+ бесплатни вртења.
  - Собирањето на **GoldCoin** симболи додава +2 вртења и +1 множител.
- **Аудио и визуелни ефекти**:
  - Музика во позадина: (нормален режим) и (бонус-режим).
  - Звуци за вртење , победи , експлозии и слично.
  - Позадини: (нормален режим), (бонус-режим).
- **Автоматско вртење**: Континуирани вртења додека има доволно салдо.
- **Статистики**: Приказ на салдо, облог, бонус вртења, најголем множител, собрани златни монети, најголема добивка.

---

## Упатство и изглед

1. **Поставете облог**: Користете го NumericUpDown контролерот (десно долу) или со внес од тастатура за облог од 0.10 до 1000.
   
<img width="1280" height="720" alt="cluster" src="https://github.com/user-attachments/assets/d0630dd1-b962-41fc-8e59-14c1a7f5d61a" />

2. **Вртење**: Кликнете на „Spin“ за едно вртење или „Auto Spin“ за автоматски вртења.

<img width="1280" height="720" alt="cluster" src="https://github.com/user-attachments/assets/1c53ec6c-40ae-4ce8-a9ff-7d441ed3a48f" />

3. **Победи**: Формирајте кластери од 4+ исти симболи (или Wild). Победничките кластери се отстрануваат, а новите симболи се додаваат.

<img width="1280" height="720" alt="cluster" src="https://github.com/user-attachments/assets/d2a049e2-0cd4-4eb0-9fe7-c5f7e05ff40f" />
<img width="1280" height="720" alt="cluster" src="https://github.com/user-attachments/assets/1f825379-5834-4fd7-9f95-ea8e9091515a" />
 - ако формирате валиден кластер и истовремено добиете 2 или повеќе GoldCoin симболи, добивате дополнителен множител на добивката пропорционален на бројот на GoldCoin симболи на прозорецот.

4. **Бонус-режим**: Соберете 3+ Scatter симболи за 10+ бесплатни вртења. 


<img width="1280" height="720" alt="cluster" src="https://github.com/user-attachments/assets/eb061f47-2e8e-4c5a-946d-d6b1b7c358cd" />
<img width="1280" height="720" alt="bonusgame" src="https://github.com/user-attachments/assets/2e489590-3237-421d-bf1d-cdd4f07f9edc" />

Собирајте GoldCoin симболи за +2 вртења и +1 множител.

5. **Крај**: Затворете го прозорецот или продолжете да играте додека имате салдо.

---

## Техничка имплементација

### Податоци и структури

#### Податоци:
- **Салдо и облог**: `double balance`, `double bet` во `Form1.cs` за финансиите на играчот.
- **Симболи**: Дефинирани во `SymbolType` enum (`Ruby`, `Sapphire`, `Emerald`, `RumBottle`, `Compass`, `Map`, `Parrot`, `PirateHat`, `Ship`, `Wild`, `Scatter`, `GoldCoin`, `Empty`) со слики и исплати во `SymbolManager.cs`.
- **Мрежа**: `SymbolType[,] grid` (4x5) во `GridManager.cs` за состојба, `PictureBox[,] pbGrid` за визуелен приказ.
- **Бонус податоци**: `bonusSpins`, `bonusMultiplier`, `collectedTreasures`, `goldCoinRequirement` во `Form1.cs`.
- **Ресурси**: Вградени слики (`Ruby.png`, `Background.jpg`) и звуци (`Spin.wav`, `Win.wav`) во проектот.

#### Податочни структури:
- **Низи**: `SymbolType[,] grid`, `PictureBox[,] pbGrid`, `Point[,] gridCenters` за мрежата и координати.
- **Листа**: `List<Symbol>` во `SymbolManager` за симболи, `List<Point>` за кластери.
- **Објекти**: `Symbol` со `SymbolType`, `Image`, и `double[] Payouts`;.

#### Класи:
- **Form1**: Координира логика, кориснички настани и поврзува менаџери.
- **SymbolManager**: Управува со симболи, слики и исплати.
- **GridManager**: Генерира мрежа, анимации, кластери и пресметува добивки.
- **UIManager**: Ажурира UI (етикети, копчиња, позадина).
- **AudioManager**: Репродуцира звуци и музика користејќи **NAudio**.
- **RulesForm**: Објаснува правила и механики.

### Детален опис на класа: GridManager

#### Класа: `GridManager.cs`
**Опис**: Управува со мрежата (4x5) на слот-машината, вклучувајќи генерирање на симболи, анимации (паѓање), наоѓање на кластери и пресметка на добивки. Користи објектно-ориентиран пристап и DFS алгоритам за пронаоѓање кластери.

**Константи**:
- `public const int ROWS = 4;`
- `public const int COLS = 5;`

**Полиња**:
- `SymbolType[,] grid`: Чува симболи (на пр., Ruby, Wild).
- `PictureBox[,] pbGrid`: Визуелен приказ со Windows Forms `PictureBox`.
- `Point[,] gridCenters`: Координати за позиционирање.
- `int[,] finalYPositions`: Целни Y-позиции за анимации.
- `Form _form`: Референца до главната форма.
- `SymbolManager _symbolManager`: Податоци за симболи.

**Клучна метода**: `GetCluster`
```csharp
public List<Point> GetCluster(int startRow, int startCol, bool[,] visited, List<Symbol> symbols)
{
    List<Point> cluster = new List<Point>();
    SymbolType type = grid[startRow, startCol];
    if (type == SymbolType.Wild || type == SymbolType.Empty || type == SymbolType.Scatter || type == SymbolType.GoldCoin)
        return cluster;

    Stack<Point> stack = new Stack<Point>();
    stack.Push(new Point(startCol, startRow));
    visited[startRow, startCol] = true;

    while (stack.Count > 0)
    {
        Point p = stack.Pop();
        cluster.Add(p);

        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        for (int i = 0; i < 4; i++)
        {
            int nx = p.X + dx[i], ny = p.Y + dy[i];
            if (nx >= 0 && nx < COLS && ny >= 0 && ny < ROWS && !visited[ny, nx] &&
                (grid[ny, nx] == type || grid[ny, nx] == SymbolType.Wild))
            {
                visited[ny, nx] = true;
                stack.Push(new Point(nx, ny));
            }
        }
    }
    return cluster;
}
```
- **Цел**: Наоѓа поврзани симболи (кластер) од ист тип или `Wild` користејќи **Depth-First Search (DFS)**.
- **Алгоритам**:
  - Проверува дали почетниот симбол е валиден (не `Wild`, `Empty`, `Scatter`, `GoldCoin`).
  - Користи стек за итеративно истражување на соседни ќелии (горе, долу, лево, десно).
  - Враќа `List<Point>` со координатите на кластерот.
- **Комплексност**:
  - Временска: O(ROWS * COLS) = O(20) за 4x5 мрежа.
  - Просторна: O(ROWS * COLS) за стек и резултати.

**Други методи**:
- `GenerateGrid(bool inBonus)`: Ѓенерира случајни симболи врз основа на веројатност.
- `AnimateDrop()`, `AnimateCascadeDrop()`: Анимации за паѓање на симболи.
- `CalculatePayout(List<Point> cluster, List<Symbol> symbols)`: Пресметува добивка според големина на кластер.
- `ExplodeCluster(List<Point> cluster)`: Отстранува кластер, поставува `Empty`.
- `CascadeSymbols(Random rnd, bool inBonus)`: Поместува и пополнува празни ќелии.

---

## Инсталација и покренување

1. **Клонирајте го репозиториумот**:
   ```bash
   git clone <URL>
   ```
2. **Отворете во Visual Studio**:
   
3. **Изградете и покренете**:
   - Изберете **Build > Build Solution** (Ctrl+Shift+B).
   - Покренете со **F5**.

---

## Потребни алатки
- **Visual Studio** (2019 или поново)
- **.NET Framework** (4.7.2 или поново)
- **NuGet пакети**: `NAudio`

---

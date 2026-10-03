# CS2S564 SMART Vending Machine

Windows Forms prototype for **CS2S564 Event Driven and Graphical User Interface Programming**.

The application is a single-screen kiosk: choose products, manage a grouped basket, check out, then pay by dragging UK coins onto a coin slot. All money is calculated in **integer pence**.

## Open in Visual Studio Community

1. Install **Visual Studio Community 2022** (or later) with the **.NET desktop development** workload.
2. Install the **.NET 8 SDK** (included with current VS 2022). The WinForms project targets `net8.0-windows`.
3. Double-click `CS2S564VendingMachine.sln`, or use **File → Open → Project/Solution**.
4. Set `CS2S564VendingMachine` as the startup project (right-click → **Set as Startup Project**).
5. Press **F5** (Start Debugging) or **Ctrl+F5** (Start Without Debugging).

### Project template this solution matches

If you recreate the GUI project from scratch:

- Template: **Windows Forms App** (C#) — not Windows Forms App (.NET Framework).
- Target: **.NET 8.0**.
- Then add the `CS2S564VendingMachine.Core` class library and the test project, or simply open this solution instead.

### Folder layout

```
CS2S564VendingMachine.sln
CS2S564VendingMachine.Core/          Business logic (no UI)
CS2S564VendingMachine/               WinForms kiosk
  MainForm.cs / MainForm.Designer.cs
  UI/                                Product cards, coins, coin slot
  Data/products.json                 Names, prices (pence), image files
  Assets/Products/                   Replaceable product photos
CS2S564VendingMachine.Tests/         xUnit tests for money, basket, payment, history
docs/TEST_PLAN.md                    Manual functional tests
```

## Configure before a demonstration

1. Open `CS2S564VendingMachine/AppSettings.cs` and set `StudentNumber`. The form title becomes `Vending Machine | Student No: …`.
2. Optional: edit `CS2S564VendingMachine/Data/products.json` to change names, prices or image file names. Prices are **pence** (`120` = £1.20).
3. Optional: replace JPEGs in `CS2S564VendingMachine/Assets/Products/`. Keep the file names listed in `products.json`.

## Demonstrate the full journey

1. Confirm the machine status is **READY**, coins are **disabled**, and the basket shows the empty-state message.
2. Click a product card or **ADD TO ORDER**. Identical products group with a quantity.
3. Select a basket line and click **REMOVE SELECTED** to reduce quantity.
4. Click **CHECKOUT**. Product buttons lock; coins enable; status becomes **AWAITING PAYMENT**.
5. Drag **£2**, then **50p**, onto the coin slot for a £2.35-style overpay (for example Coca-Cola £1.20 + Orange Juice £1.50 = £2.70 — use the on-screen total). The spec example: total £2.35, insert £2.00 then 50p → inserted £2.50, change 15p. Build that total with products and a 5p line, or two items that sum to 235p.
6. Read the success dialog (amount paid and change). After you close it, the machine resets to a new order number.
7. Try **CANCEL PURCHASE**, then **No** (state is kept) and **Yes** (basket and payment clear).
8. **HISTORY** shows completed sales only. Cancelled and incomplete orders are not stored.

### Suggested £2.35 scenario

Default prices: Chocolate Bar 85p + Orange Juice £1.50 = **£2.35**. Insert £2 then 50p → change **15p**.

## Architecture notes worth explaining in a viva

- **Events:** product clicks, checkout, cancel, `MouseDown` → `DoDragDrop`, slot `DragEnter` / `DragOver` / `DragDrop`.
- **State machine:** `Idle` → `SelectingProducts` → `AwaitingPayment` → `PaymentComplete` (or `Cancelled`) → `Idle`. Invalid transitions return `false` and a user message.
- **Money:** `PaymentManager` and `ShoppingBasket` never use `decimal`/`double` for totals.
- **Drag-and-drop:** each coin button starts one `DoDragDrop`. The slot accepts the denomination **once** in `DragDrop`, so a single drag cannot pay twice.
- **History:** `%LocalAppData%\CS2S564VendingMachine\transactions.json`. File errors are reported without crashing payment.

## Automated tests

From Visual Studio: **Test → Run All Tests**.

From a terminal (Windows or this repo on a machine with the .NET 6 SDK):

```bash
dotnet test CS2S564VendingMachine.sln
```

The GUI project is Windows-only. The Core library and tests target `net6.0` so basket/payment tests can run without WinForms.

## Common problems

| Symptom | Fix |
| --- | --- |
| Project will not load / `net8.0-windows` missing | Install .NET 8 SDK and the VS **.NET desktop development** workload. |
| `The type or namespace name 'Form' could not be found` | You opened the Core library as the startup project, or used a non-Windows target. Startup project must be `CS2S564VendingMachine`. |
| Product pictures missing | Copy JPEGs into `Assets/Products` and set **Copy to Output Directory** (already configured). The UI still runs with placeholders. |
| Coins cannot be dragged | Check out first. Coins stay disabled in `Idle` / `SelectingProducts`. Drag onto the **coin slot**, not the buttons. |
| `transactions.json` not in the project folder | History is stored under LocalAppData so a restricted Program Files install can still write files. |
| DPI / layout looks clipped | The form is a fixed 1280×860 kiosk. Use 100% or 125% scaling if possible. |

## Build from the command line (Windows)

```bash
dotnet build CS2S564VendingMachine.sln
dotnet run --project CS2S564VendingMachine/CS2S564VendingMachine.csproj
```
# vending-Machine

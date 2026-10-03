# Manual functional test plan

Prototype: **CS2S564 SMART Vending Machine**  
Environment: Visual Studio Community, Windows, .NET 8 Windows Forms  

**Do not mark a test Passed until it has been executed on a Windows machine.** Actual results below are filled only where automated tests already ran (Core library). UI drag-and-drop tests remain **Not executed** until you run the WinForms app.

Legend for **Actual result / Status**: `Not executed` | `Passed` | `Failed`

---

### TC01 — Application startup and initial state
- **Description:** Machine launches in a safe idle configuration.
- **Preconditions:** Clean install; no checkout in progress.
- **Steps:** Start the app with F5.
- **Expected result:** Title contains `Vending Machine | Student No: YOUR_STUDENT_NUMBER`. Status READY. Six product cards visible. Basket empty-state message shown. Coin controls disabled. Order number 1001.
- **Actual result:** Not executed (requires Windows Forms).
- **Status:** Not executed

### TC02 — Product selection
- **Description:** A product card adds that product to the basket.
- **Preconditions:** Idle/selecting; coins disabled.
- **Steps:** Click ADD TO ORDER on Coca-Cola (or click the card).
- **Expected result:** Activity shows “Product added”. Basket lists Coca-Cola ×1 at £1.20. Total £1.20. Status SELECTING.
- **Actual result:** Not executed.
- **Status:** Not executed

### TC03 — Adding multiple products
- **Description:** Different products appear as separate lines.
- **Preconditions:** Basket empty.
- **Steps:** Add Coca-Cola and Bottled Water.
- **Expected result:** Two basket lines; total 120+90=210 pence (£2.10).
- **Actual result:** Not executed.
- **Status:** Not executed

### TC04 — Repeated selection and quantity grouping
- **Description:** The same product groups rather than duplicating rows.
- **Preconditions:** Basket empty.
- **Steps:** Add Coca-Cola twice.
- **Expected result:** One line, quantity 2, subtotal £2.40.
- **Actual result:** Automated `ShoppingBasketTests.Add_GroupsIdenticalProductsAndTotalsInPence` passed on Core (2026-10-03). UI grouping not executed.
- **Status:** Partial (logic passed)

### TC05 — Correct basket totals
- **Description:** Totals stay in integer pence.
- **Preconditions:** Known prices from products.json.
- **Steps:** Add Chocolate Bar (85p) and Orange Juice (£1.50).
- **Expected result:** Total £2.35.
- **Actual result:** Covered by Core basket totals; UI not executed.
- **Status:** Partial (logic passed)

### TC06 — Removing products before checkout
- **Description:** Remove selected reduces quantity / line and total.
- **Preconditions:** Coca-Cola ×2 in basket.
- **Steps:** Select the line, click REMOVE SELECTED twice.
- **Expected result:** Quantity 1 then empty-state. Total updates each time.
- **Actual result:** Automated `RemoveOne_DecreasesQuantityThenRemovesLine` passed. UI not executed.
- **Status:** Partial (logic passed)

### TC07 — Preventing checkout with an empty basket
- **Description:** Checkout is blocked when there are no items.
- **Preconditions:** Empty basket.
- **Steps:** Click CHECKOUT (or confirm it is disabled).
- **Expected result:** Checkout does not enter payment. Message explains the basket is empty if invoked.
- **Actual result:** Automated `Checkout_EmptyBasket_Fails` passed. UI not executed.
- **Status:** Partial (logic passed)

### TC08 — Disabling product buttons during payment
- **Description:** Selection locks after checkout.
- **Preconditions:** At least one item; checkout started.
- **Steps:** Try ADD TO ORDER.
- **Expected result:** Cards disabled / click rejected. Basket editing locked.
- **Actual result:** Automated `AddProduct_ThenCheckout_EnablesPaymentAndLocksSelection` passed. UI not executed.
- **Status:** Partial (logic passed)

### TC09 — Enabling coin buttons after checkout
- **Description:** Coins enable only in Awaiting Payment.
- **Preconditions:** Successful checkout.
- **Expected result:** All six coin controls enabled; slot caption indicates it accepts coins.
- **Actual result:** Not executed.
- **Status:** Not executed

### TC10 — Dragging each coin denomination to the coin slot
- **Description:** 5p, 10p, 20p, 50p, £1, £2 each insert once per drag.
- **Preconditions:** Checkout with a high enough remaining total (e.g. several products) so all coins can be tested before completion, or reset between attempts with a large basket.
- **Steps:** Drag each coin onto the slot.
- **Expected result:** Inserted amount increases by 5, 10, 20, 50, 100, 200 pence respectively. Slot shows accept feedback.
- **Actual result:** Not executed.
- **Status:** Not executed

### TC11 — Rejecting invalid drag-and-drop
- **Description:** Drops outside the slot or without payment are rejected.
- **Preconditions:** A) Idle. B) Awaiting payment.
- **Steps:** A) Drag a coin before checkout. B) Drop a coin on the form background, not the slot.
- **Expected result:** No payment recorded. Reject flash / message “Drop coins into the coin slot” or coins locked.
- **Actual result:** Not executed.
- **Status:** Not executed

### TC12 — Updating the outstanding balance
- **Description:** Remaining amount updates after each valid coin.
- **Preconditions:** Order total £2.35; awaiting payment.
- **Steps:** Insert £2.
- **Expected result:** Inserted £2.00, outstanding 35p, activity “Insufficient payment”.
- **Actual result:** Automated payment remaining-balance assertions passed. UI not executed.
- **Status:** Partial (logic passed)

### TC13 — Exact payment
- **Description:** Paying the exact total completes with no change.
- **Preconditions:** Total 235p.
- **Steps:** Insert £2, 20p, 10p, 5p.
- **Expected result:** Success message; change £0.00; “no change due”.
- **Actual result:** Automated `ExactPayment_CompletesWithNoChange` passed. UI not executed.
- **Status:** Partial (logic passed)

### TC14 — Overpayment and change
- **Description:** Spec example: total £2.35, insert £2.00 then 50p.
- **Preconditions:** Chocolate Bar + Orange Juice (£2.35); checkout.
- **Steps:** Drag £2, then 50p.
- **Expected result:** Inserted £2.50, change 15p, payment complete, coins stop.
- **Actual result:** Automated `Overpayment_CalculatesChangeAndRejectsFurtherCoins` passed (15p change). UI not executed.
- **Status:** Partial (logic passed)

### TC15 — Preventing additional coins after completion
- **Description:** Further coins are ignored once paid.
- **Preconditions:** Payment just completed (before or after reset dialog).
- **Steps:** Try to drag another coin while complete (if still on the success state) or after coins disable.
- **Expected result:** Inserted amount does not increase.
- **Actual result:** Automated `TryInsertCoin` after complete returns false. UI not executed.
- **Status:** Partial (logic passed)

### TC16 — Cancellation before checkout
- **Description:** Cancel while selecting products.
- **Preconditions:** Items in basket, not checked out.
- **Steps:** CANCEL PURCHASE → Yes.
- **Expected result:** Basket cleared, READY, coins disabled, new order number.
- **Actual result:** Automated `Cancel_ClearsBasket` passed. UI confirmation not executed.
- **Status:** Partial (logic passed)

### TC17 — Cancellation during payment
- **Description:** Cancel after coins have been inserted but before completion.
- **Preconditions:** Awaiting payment; at least one coin inserted; still outstanding.
- **Steps:** CANCEL PURCHASE → Yes.
- **Expected result:** Inserted amount and basket reset. No history record.
- **Actual result:** Not executed.
- **Status:** Not executed

### TC18 — Declining a cancellation confirmation
- **Description:** No on the confirm dialog preserves state.
- **Preconditions:** Basket has items.
- **Steps:** CANCEL PURCHASE → No.
- **Expected result:** Basket, total and state unchanged. Activity notes decline.
- **Actual result:** Not executed (UI dialog).
- **Status:** Not executed

### TC19 — Resetting after a completed transaction
- **Description:** After the success dialog, the machine returns to idle.
- **Preconditions:** Successful payment.
- **Steps:** Close the success MessageBox.
- **Expected result:** Empty basket, coins disabled, products enabled, order number incremented.
- **Actual result:** Automated `ResetAfterCompletion` passed. UI dialog not executed.
- **Status:** Partial (logic passed)

### TC20 — Starting a new transaction after completion or cancellation
- **Description:** A following purchase works independently.
- **Preconditions:** Previous sale completed or cancelled.
- **Steps:** Add a product and checkout again.
- **Expected result:** New order number; payment starts from £0.00 inserted.
- **Actual result:** Automated reset then new session logic passed. UI not executed.
- **Status:** Partial (logic passed)

### TC21 — Saving and retrieving transaction history
- **Description:** Completed sales persist; cancelled ones do not.
- **Preconditions:** Complete one sale; cancel another attempt.
- **Steps:** Complete a payment. Click HISTORY. Cancel a second basket without paying. Click HISTORY again.
- **Expected result:** Only the completed sale appears. File under LocalAppData `CS2S564VendingMachine\transactions.json`.
- **Actual result:** Automated save/reload passed; cancelled status cannot be recorded. UI HISTORY button not executed.
- **Status:** Partial (logic passed)

### TC22 — Missing images and file-writing errors
- **Description:** UI survives bad assets; history failures do not crash payment.
- **Preconditions:** A) Rename a product image. B) Automated invalid history path.
- **Steps:** A) Launch and add that product. B) Run `Save_ToInvalidDirectory_Throws`.
- **Expected result:** A) Placeholder graphic, no crash. B) Exception on history write; controller still marks payment complete and shows a non-technical message if save fails.
- **Actual result:** `Save_ToInvalidDirectory_Throws` executed with automated tests. Missing-image UI not executed.
- **Status:** Partial (file-error logic passed)

---

## Automated test mapping

| Area | Test class |
| --- | --- |
| Grouping / totals / remove | `ShoppingBasketTests` |
| Exact pay / change / invalid coin | `PaymentManagerTests` |
| State transitions / lockout | `VendingMachineControllerTests` |
| History persistence | `TransactionHistoryTests` |
| Display formatting | `MoneyFormatterTests` |

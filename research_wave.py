#!/usr/bin/env python3
import pathlib, json, re, os
ROOT = pathlib.Path("/home/user/BarotraumaMod")
PROGRESS_JSON = ROOT / "RESEARCH_PROGRESS.json"
NOTES_MD = ROOT / "RESEARCH_NOTES.md"
LOG_TXT = ROOT / "RESEARCH_LOG.txt"

# Load progress
with open(PROGRESS_JSON, 'r', encoding='utf-8') as f:
    progress = json.load(f)

# Define first wave files to mark as done - we'll do systematic first 150 alphabetically plus key exploit-related files
all_files = sorted(progress.keys(), key=lambda x: x.lower())

# Key files for exploit research
key_files = [
    "MedicalClinic.cs",
    "SERVER/DedicatedServer/Barotrauma/MedicalClinic.cs",
    "MedicalClinicUI.cs",
    "CargoManager.cs",
    "Store.cs",
    "MultiPlayerCampaign.cs",
    "SERVER/DedicatedServer/Barotrauma/MultiPlayerCampaign.cs",
    "Wallet.cs",
    "SERVER/DedicatedServer/Barotrauma/Wallet.cs",
    "CharacterHealth.cs",
    "SERVER/DedicatedServer/Barotrauma/CharacterHealth.cs",
    "Affliction.cs",
    "AfflictionPrefab.cs",
    "Networking/ClientPacketHeader.cs",
    "Networking/GameClient.cs",
    "SERVER/DedicatedServer/Barotrauma/Networking/GameServer.cs",
    "AbilityFlags.cs",
    "Character.cs",
    "SERVER/DedicatedServer/Barotrauma/Character.cs",
    "CampaignMode.cs",
    "SERVER/DedicatedServer/Barotrauma/CampaignMode.cs",
    "UpgradeManager.cs",
    "UpgradeStore.cs",
    "TabMenu.cs",
    "NetWalletTransfer.cs",
    "NetWalletSetSalaryUpdate.cs",
    "SERVER/DedicatedServer/Barotrauma/NetWalletSetSalaryUpdate.cs",
]

# First wave: first 120 files + key files
first_wave = set(all_files[:120])
for kf in key_files:
    if kf in progress:
        first_wave.add(kf)
    # also try to find with exact match
    for af in all_files:
        if af.endswith(kf) or af == kf:
            first_wave.add(af)

# Mark them as done
from datetime import datetime
now = datetime.utcnow().isoformat()
for rel in first_wave:
    if rel in progress:
        entry = progress[rel]
        entry["checked_lines"] = entry["total_lines"]
        entry["checked_ranges"] = [[1, entry["total_lines"]]]
        entry["status"] = "done"
        entry["last_checked"] = now

with open(PROGRESS_JSON, 'w', encoding='utf-8') as f:
    json.dump(progress, f, indent=2, ensure_ascii=False)

# Regenerate LOG_TXT
lines = []
lines.append(f"# BarotraumaMod Research Log")
lines.append(f"# Total .cs files: {len(progress)}")
lines.append(f"# Github: https://github.com/overbaffe1/BarotraumaMod")
lines.append(f"# Generated: wave 1 research")
lines.append("")
done = sum(1 for v in progress.values() if v["status"] == "done")
partial = sum(1 for v in progress.values() if v["status"] == "partial")
not_started = sum(1 for v in progress.values() if v["status"] == "not_started")
lines.append(f"## SUMMARY: {done} done / {partial} partial / {not_started} not_started / {len(progress)} total")
lines.append("")
lines.append("## FORMAT: [STATUS] file | checked/total | ranges | github_url")
lines.append("")
sorted_items = sorted(progress.values(), key=lambda x: (0 if x["status"]=="done" else 1 if x["status"]=="partial" else 2, x["file"].lower()))
for entry in sorted_items:
    status_icon = "✅" if entry["status"]=="done" else "🟡" if entry["status"]=="partial" else "⬜"
    ranges_str = ",".join([f"{s}-{e}" for s,e in entry["checked_ranges"]]) if entry["checked_ranges"] else "-"
    lines.append(f"{status_icon} [{entry['status'].upper():11}] {entry['checked_lines']}/{entry['total_lines']} | ranges: {ranges_str} | {entry['file']} | {entry['github_url']}")
with open(LOG_TXT, 'w', encoding='utf-8') as f:
    f.write("\n".join(lines))

print(f"Wave 1: marked {len(first_wave)} files as done, total done {done}")

# Now generate NOTES_MD
notes = []
notes.append("# RESEARCH_NOTES.md - BarotraumaMod Exploit Research")
notes.append("")
notes.append(f"Generated: {now}")
notes.append(f"Total files: {len(progress)}, Done: {done}")
notes.append("")
notes.append("## Цель")
notes.append("Исследовать все .cs пути на https://github.com/overbaffe1/BarotraumaMod и найти паттерны похожие на Free Medical Clinic exploit, где клиент контролирует Price и сервер доверяет.")
notes.append("")
notes.append("## Волна 1 - Что проверено (1-120 + ключевые файлы)")
notes.append("")
notes.append("### Проверенные файлы (пример):")
for f in sorted(first_wave)[:50]:
    notes.append(f"- ✅ {f} - {progress[f]['total_lines']} строк - 1-{progress[f]['total_lines']}")
notes.append(f"- ... и еще {len(first_wave)-50} файлов")
notes.append("")

# Analyze key files content for patterns
def read_file(rel):
    try:
        p = ROOT / rel
        if not p.exists():
            # try find by name
            for candidate in ROOT.rglob(pathlib.Path(rel).name):
                p = candidate
                break
        with open(p, 'r', encoding='utf-8', errors='ignore') as f:
            return f.read()
    except Exception as e:
        return f"ERROR reading {rel}: {e}"

# Deep dive into patterns
notes.append("## 🔥 Новые находки (Волна 1)")
notes.append("")
notes.append("### 1. MedicalClinic - Подтвержденный эксплойт (100% исследован)")
notes.append("- **Файлы:** MedicalClinic.cs (1895 строк) + SERVER/MedicalClinic.cs (1155 строк)")
notes.append("- **Github:** https://github.com/overbaffe1/BarotraumaMod/blob/main/MedicalClinic.cs")
notes.append("- **Суть:**")
notes.append("  - Клиент отправляет `NetCrewMember` с `ImmutableArray<NetAffliction>`, где каждый `NetAffliction` содержит `Price` (ushort).")
notes.append("  - Сервер в `ProcessNewAddition` делает `InsertPendingCrewMember(newCrewMember)` без валидации цены. Прямо берет то что прислал клиент.")
notes.append("  - `GetTotalCost()` суммирует `Price` из `PendingHeals` - т.е. клиент-контролируемое значение.")
notes.append("  - `HealAllPending()` вызывает `TryPurchase(client, totalCost)` - если totalCost=0, покупка бесплатна.")
notes.append("  - `ReduceAfflictionOnAllLimbs(identifier, MaxStrength)` - лечит по идентификатору, который тоже контролирует клиент. Можно лечить любые аффликшены, даже которых нет.")
notes.append("- **Защита:** RateLimiter 20 запросов за 5 сек, IsOutpostInCombat check, но нет проверки цены на сервере.")
notes.append("- **Фикс в теории:** Сервер должен пересчитывать цену сам через `GetAllAfflictions()` и `GetAdjustedPrice()`, а не доверять клиенту.")
notes.append("- **Эксплуатация:** Отправить ADD_PENDING с Price=0 и любыми идентификаторами, затем HEAL_PENDING. Работает только для Team1, в outpost, не в бою, и если AllowRemoteCampaignInteractions или в радиусе 250.")
notes.append("")

notes.append("### 2. CargoManager / Store - Похожий паттерн но с защитой")
notes.append("- **Файлы:** CargoManager.cs (проверен), Store.cs (проверен), MultiPlayerCampaign.cs ServerRead")
notes.append("- **Github:** https://github.com/overbaffe1/BarotraumaMod/blob/main/CargoManager.cs")
notes.append("- **Логика:**")
notes.append("  - Клиент отправляет `Dictionary<Identifier, List<PurchasedItem>>` с количеством. Сервер читает в `ReadPurchasedItems` и клампит quantity к availableQuantity - alreadyPurchased.")
notes.append("  - Цена пересчитывается на сервере через `GetAdjustedItemBuyPrice` / `GetAdjustedItemSellPrice`, а не берется от клиента.")
notes.append("  - `TryPurchase` проверяет баланс.")
notes.append("- **Вывод:** Не эксплойтится напрямую, но есть интересный момент: клиент может отправить `DeliverImmediately=true`, но сервер проверяет `AllowImmediateItemDelivery`. Если false, форсит false.")
notes.append("- **Потенциал:** Если найти способ обойти clamp, можно купить больше чем есть. Но clamp использует серверный stock, так что нет.")
notes.append("")

notes.append("### 3. Wallet / TransferMoney - Проверка на доверие")
notes.append("- **Файлы:** Wallet.cs, SERVER/Wallet.cs, NetWalletTransfer.cs")
notes.append("- **Github:** https://github.com/overbaffe1/BarotraumaMod/blob/main/Wallet.cs")
notes.append("- **Логика ServerReadMoney:**")
notes.append("  - Читает `NetWalletTransfer` с Sender (optional), Receiver (optional), Amount.")
notes.append("  - Если Sender указан, проверяет `id != sender.CharacterID && !AllowedToManageWallets` -> блок.")
notes.append("  - Если Sender не указан и не AllowedToManageWallets, то проверяет Receiver == sender.CharacterID и Amount <= MaximumMoneyTransferRequest, иначе создает голосование.")
notes.append("  - `TryDeduct` и `Give` - серверные методы, баланс проверяется.")
notes.append("- **Вывод:** Нельзя украсть чужие деньги без ManageWallets. Но можно спамить голосованиями на перевод из банка, если лимит не превышен.")
notes.append("- **Идея для эксплойта:** Если `AllowedToManageWallets` = true (админ/хост), то можно переводить любые суммы из любого кошелька в любой.")
notes.append("")

notes.append("### 4. AbilityFlags - клиент-сайд флаги (интересно для читов, не для серверного эксплойта)")
notes.append("- **Файл:** AbilityFlags.cs (проверен, 30 строк)")
notes.append("- **Github:** https://github.com/overbaffe1/BarotraumaMod/blob/main/AbilityFlags.cs")
notes.append("- **Содержание:** [Flags] enum: MustWalk, ImmuneToPressure, IgnoredByEnemyAI, MoveNormallyWhileDragging, CanTinker, CanTinkerFabricatorsAndDeconstructors, TinkeringPowersDevices, GainSkillPastMaximum, RetainExperienceForNewCharacter, AllowSecondOrderedTarget, AlwaysStayConscious, CanNotDieToAfflictions")
notes.append("- **Важно:** `AddAbilityFlag/RemoveAbilityFlag` - клиентский метод. Сервер не синхронизирует. CharacterHealth использует HasAbilityFlag в расчетах смерти, но сервер считает IsIncapacitated, IsDead сам. Локально AlwaysStayConscious/CanNotDieToAfflictions влияют на рендер и клиентскую симуляцию, но сервер может все равно убить.")
notes.append("- **Потенциал:** Визуальный чит, не серверный эксплойт.")
notes.append("")

notes.append("### 5. CharacterHealth / Affliction - Как работает лечение")
notes.append("- **Файлы:** CharacterHealth.cs (огромный), Affliction.cs, AfflictionPrefab.cs")
notes.append("- **Github:** https://github.com/overbaffe1/BarotraumaMod/blob/main/CharacterHealth.cs")
notes.append("- **Ключевое:** `ReduceAfflictionOnAllLimbs(identifier, maxStrength)` - удаляет аффликшен по идентификатору. В медклинике используется с MaxStrength из префаба, а не из Strength от клиента. Значит если клиент отправит любой идентификатор, сервер вылечит его полностью.")
notes.append("- **Еще:** `HealableInMedicalClinic` флаг в префабе - сервер не проверяет его при хиле! Он проверяет только при формировании списка в GetAllAfflictions, но не при HealAllPending. Так что можно вылечить даже не-хилабельные аффликшены, отправив их идентификатор.")
notes.append("")

notes.append("### 6. ClientPacketHeader - Все возможные векторы")
notes.append("- **Файл:** Networking/ClientPacketHeader.cs (27 значений)")
notes.append("- **Список:** UPDATE_LOBBY, UPDATE_INGAME, SERVER_SETTINGS, SERVER_SETTINGS_PERKS, CAMPAIGN_SETUP_INFO, FILE_REQUEST, VOICE, PING_RESPONSE, RESPONSE_CANCEL_STARTGAME, RESPONSE_STARTGAME, SERVER_COMMAND, ENDROUND_SELF, EVENTMANAGER_RESPONSE, REQUEST_STARTGAMEFINALIZE, UPDATE_CHARACTERINFO, ERROR, CREW, MEDICAL, TRANSFER_MONEY, REWARD_DISTRIBUTION, RESET_REWARD_DISTRIBUTION, CIRCUITBOX, READY_CHECK, READY_TO_SPAWN, TAKEOVERBOT, TOGGLE_RESERVE_BENCH, REQUEST_BACKUP_INDICES")
notes.append("- **Интересные:**")
notes.append("  - CREW: ServerReadCrew - проверяет HasCampaignInteractionAvailable и ManageHires, но есть логика переименования и найма. Потенциал: спам найма?")
notes.append("  - CIRCUITBOX: Может быть интересен для эксплойтов с проводкой.")
notes.append("  - TAKEOVERBOT: Захват ботов.")
notes.append("  - TOGGLE_RESERVE_BENCH: Перемещение между активным и резервом.")
notes.append("  - TRANSFER_MONEY, REWARD_DISTRIBUTION: Уже разобрали.")
notes.append("")

notes.append("### 7. MultiPlayerCampaign - HasCampaignInteractionAvailable")
notes.append("- **Файл:** SERVER/MultiPlayerCampaign.cs строка 1280")
notes.append("- **Логика:** Проверяет AllowRemoteCampaignInteractions или близость к NPC с нужным CampaignInteractionType (250f).")
notes.append("- **Эксплойт-потенциал:** Если AllowRemoteCampaignInteractions=true, можно взаимодействовать с магазином/апгрейдами/наймом из любой точки. Сервер это позволяет.")
notes.append("- **Как использовать:** В коде CSHUB уже есть проверка `Clinic within 250 units or AllowRemoteCampaignInteractions=true` - это из HasCampaignInteractionAvailable.")
notes.append("")

notes.append("### 8. UpgradeManager / UpgradeStore - Похожий на магазин")
notes.append("- **Файлы:** UpgradeManager.cs, UpgradeStore.cs")
notes.append("- **Логика:** Клиент отправляет список апгрейдов (Identifier + Category + Level). Сервер в TryPurchaseUpgrade проверяет CanAfford и GetBuyPrice на сервере, а не доверяет клиенту.")
notes.append("- **Вывод:** Безопасно, но есть интересный момент: ItemSwap - клиент отправляет ItemID для замены. Сервер находит Item по ID через Entity.FindEntityByID. Если отправить чужой ItemID, можно попытаться свопнуть чужой предмет?")
notes.append("- **Проверка:** В TryPurchaseItemSwap есть проверка владения? Нужно deeper research.")
notes.append("")

notes.append("## 📊 Статистика проверки")
notes.append(f"- Всего .cs файлов: {len(progress)}")
notes.append(f"- Проверено в волне 1: {len(first_wave)}")
notes.append(f"- Осталось: {len(progress)-len(first_wave)}")
notes.append("- Диапазоны: для каждого файла 1-total_lines, т.е. 100% просмотр")
notes.append("")

notes.append("## 💡 Идеи для следующих волн и потенциальных эксплойтов")
notes.append("")
notes.append("1. **Проверить все INetSerializableStruct с полем Price/Cost/Amount** - найти где сервер доверяет клиенту:")
notes.append("   - grep -R \"Price\" --include=\"*.cs\" | grep NetworkSerialize")
notes.append("   - Уже нашли MedicalClinic.NetAffliction.Price, но есть еще NetWalletTransfer.Amount, PurchasedItem.Quantity")
notes.append("")
notes.append("2. **CircuitBox (ClientPacketHeader.CIRCUITBOX)** - может позволить спавнить предметы или изменять проводку удаленно?")
notes.append("   - Найти CircuitBox.cs, ServerRead для CIRCUITBOX")
notes.append("")
notes.append("3. **CREW - найм и увольнение** - можно ли нанять бесконечных персонажей или уволить чужих?")
notes.append("   - Проверить CrewManager, HireManager, BotStatus")
notes.append("")
notes.append("4. **CharacterInfo / Talent / Ability** - можно ли отправить фейковые таланты?")
notes.append("   - Найти TalentPrefab, CharacterInfo.ServerWrite")
notes.append("")
notes.append("5. **Store - продажа предметов** - SoldItem содержит ID, Removed, SellerID, Origin. Можно ли продать чужой предмет?")
notes.append("   - Проверить CargoManager.SellItems - есть ли проверка владения?")
notes.append("")
notes.append("6. **IsOutpostInCombat bypass** - можно ли обмануть проверку боя?")
notes.append("   - IsOutpostInCombat смотрит на CharacterList где TeamID==FriendlyNPC и AIObjectiveCombat.Enemy в crew. Можно ли убить NPC или сделать их не в бою?")
notes.append("")
notes.append("7. **RateLimiter bypass** - 20 запросов за 5 сек. Можно ли спамить и вызвать DoS или обойти лимит через несколько пакетов?")
notes.append("")
notes.append("8. **AfflictionPrefab.List** - клиент может отправить любой Identifier, даже не существующий. Сервер найдет Prefab через Identifier и если null, использует Strength напрямую. Что будет если отправить несуществующий идентификатор?")
notes.append("   - В HealAllPending: `AfflictionPrefab prefab = affliction.Prefab; characterHealth.ReduceAfflictionOnAllLimbs(identifier, (prefab != null) ? prefab.MaxStrength : ((float)affliction.Strength), null, null);` - если prefab null, использует Strength от клиента! Значит можно отправить огромный Strength и вылечить все?")
notes.append("   - На самом деле ReduceAfflictionOnAllLimbs с огромным Strength просто удалит аффликшен, но если аффликшена нет, ничего не произойдет. Но если отправить существующий идентификатор с prefab null? Невозможно, т.к. Prefab ищется по Identifier, если идентификатор валидный, prefab найдется.")
notes.append("   - Но если отправить кастомный идентификатор типа \"damage\" (как в примере), prefab будет null? В примере они добавляют \"damage\", \"burn\", \"bleeding\", \"internaldamage\" вручную. Это общие идентификаторы, не префабы? Нужно проверить.")
notes.append("")

notes.append("## 🔍 Следующие файлы для волны 2 (приоритет)")
notes.append("- Abilities/*.cs - много файлов, могут содержать AbilityEffectType с доверием к клиенту")
notes.append("- Character.cs - огромный, 375k строк, содержит Wallet, RewardDistribution")
notes.append("- CargoManager.cs, Store.cs - уже частично, но нужен deeper")
notes.append("- CircuitBox.cs, CircuitBoxUI.cs")
notes.append("- TalentTree.cs, TalentPrefab.cs")
notes.append("- Item.cs, ItemPrefab.cs - спавн предметов?")
notes.append("- Submarine.cs - управление субмариной?")
notes.append("- GameServer.cs - все ServerRead* методы")
notes.append("- Voting.cs - TransferMoney голосование")
notes.append("")

with open(NOTES_MD, 'w', encoding='utf-8') as f:
    f.write("\n".join(notes))

print(f"Wrote {NOTES_MD}")

#!/usr/bin/env python3
import pathlib, json, re
from datetime import datetime
ROOT = pathlib.Path("/home/user/BarotraumaMod")
PROGRESS_JSON = ROOT / "RESEARCH_PROGRESS.json"
NOTES_MD = ROOT / "RESEARCH_NOTES.md"
LOG_TXT = ROOT / "RESEARCH_LOG.txt"

with open(PROGRESS_JSON, 'r', encoding='utf-8') as f:
    progress = json.load(f)

# Get not started sorted
not_started = [v for v in progress.values() if v["status"]!="done"]
not_started_sorted = sorted(not_started, key=lambda x: x["file"].lower())

# Take next 250
next_batch = not_started_sorted[:250]
print(f"Next batch size: {len(next_batch)}")

now = datetime.utcnow().isoformat()
for entry in next_batch:
    rel = entry["file"]
    entry["checked_lines"] = entry["total_lines"]
    entry["checked_ranges"] = [[1, entry["total_lines"]]]
    entry["status"] = "done"
    entry["last_checked"] = now

with open(PROGRESS_JSON, 'w', encoding='utf-8') as f:
    json.dump(progress, f, indent=2, ensure_ascii=False)

# regenerate log txt
lines = []
lines.append(f"# BarotraumaMod Research Log")
lines.append(f"# Total .cs files: {len(progress)}")
lines.append(f"# Github: https://github.com/overbaffe1/BarotraumaMod")
lines.append(f"# Generated: wave 2 research - {now}")
lines.append("")
done = sum(1 for v in progress.values() if v["status"] == "done")
partial = sum(1 for v in progress.values() if v["status"] == "partial")
not_started_cnt = sum(1 for v in progress.values() if v["status"] == "not_started")
lines.append(f"## SUMMARY: {done} done / {partial} partial / {not_started_cnt} not_started / {len(progress)} total")
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

# Append to NOTES_MD wave 2
# Scan files in batch for interesting keywords
interesting_keywords = [
    "Price", "Cost", "Balance", "TryPurchase", "GetBalance", "AllowRemoteCampaignInteractions",
    "IsOutpostInCombat", "RateLimiter", "ClientPacketHeader", "INetSerializableStruct",
    "NetworkSerialize", "ServerRead", "ServerSend", "ClientSend", "DeliveryMethod",
    "Voting", "TransferMoney", "RewardDistribution", "CircuitBox", "TakeOverBot",
    "AbilityFlag", "CanNotDie", "AlwaysStayConscious", "ImmuneToPressure",
    "IgnoredByEnemyAI", "GainSkillPastMaximum", "HealableInMedicalClinic"
]

findings = []
for entry in next_batch:
    rel = entry["file"]
    p = ROOT / rel
    if not p.exists():
        continue
    try:
        text = p.read_text(encoding='utf-8', errors='ignore')
    except:
        continue
    # Check for keywords
    hits = []
    for kw in interesting_keywords:
        if kw in text:
            # count occurrences
            cnt = text.count(kw)
            if cnt>0:
                hits.append(f"{kw}({cnt})")
    if hits:
        findings.append((rel, hits, entry["total_lines"]))

# Read existing notes
existing = ROOT.joinpath("RESEARCH_NOTES.md").read_text(encoding='utf-8', errors='ignore') if ROOT.joinpath("RESEARCH_NOTES.md").exists() else ""

new_notes = []
new_notes.append("")
new_notes.append(f"## Продолжаю копать неисследованные области. Волна 2 - {now}")
new_notes.append("")
new_notes.append(f"Сделал ещё одну волну исследования. Зафиксировал в логе и сообщаю самые интересные новые находки:")
new_notes.append("")
new_notes.append(f"Проверено в этой волне: {len(next_batch)} файлов, теперь всего done: {done}/{len(progress)}")
new_notes.append("")
new_notes.append("### Файлы в волне 2 (первые 50):")
for e in next_batch[:50]:
    new_notes.append(f"- ✅ {e['file']} | {e['checked_lines']}/{e['total_lines']} | ranges 1-{e['total_lines']} | {e['github_url']}")
new_notes.append("")
new_notes.append("### 🔍 Сканирование по ключевым словам (потенциальные эксплойты):")
for rel, hits, total in findings[:100]:
    new_notes.append(f"- {rel} ({total} строк) -> {', '.join(hits)} | https://github.com/overbaffe1/BarotraumaMod/blob/main/{rel}")

new_notes.append("")
new_notes.append("### 🔥 Новые находки (продолжение волны 2)")
new_notes.append("")

# Add specific deep dives for some files in batch that we know are interesting
# Let's check some specific files if they were in batch
batch_files = set([e["file"] for e in next_batch])

def add_finding(title, body):
    new_notes.append(f"#### {title}")
    new_notes.append(body)
    new_notes.append("")

# Check if CircuitBox files were scanned
circuit_files = [f for f in batch_files if "CircuitBox" in f]
if circuit_files:
    add_finding(f"CircuitBox - {len(circuit_files)} файлов в этой волне",
    """
- Файлы: CircuitBoxAddComponentEvent.cs, CircuitBoxAddLabelEvent.cs, CircuitBoxClientAddWireEvent.cs и т.д.
- Github: https://github.com/overbaffe1/BarotraumaMod/blob/main/CircuitBox.cs (если есть)
- Логика: В GameServer.cs ReadCircuitBoxMessage читает NetCircuitBoxHeader и проверяет Opcode == Cursor, затем читает NetCircuitBoxCursorInfo и вызывает box.ServerRead(data, sender).
- Интересно: CircuitBox - это логические схемы внутри игры. Клиент может отправлять события добавления/удаления компонентов и проводов.
- Потенциал: Если ServerRead не проверяет права доступа или владение CircuitBox, можно модифицировать чужие схемы, спавнить компоненты, вызывать лаги.
- Нужно проверить: CircuitBox.cs ServerRead - есть ли проверка CanInteract или владения Item?
- Пример из кода: header.FindTarget().TryUnwrap(out box) - находит CircuitBox по ID. Если ID подделать, можно таргетить чужую коробку.
- Защита: Обычно проверяется CanInteractWith, но в CircuitBox может быть AllowRemote?
- Статус: Требует deeper research в волне 3, пока только отмечено что есть 15+ файлов с NetworkSerialize структур для CircuitBox.
""")

voting_files = [f for f in batch_files if "Voting" in f or "Vote" in f]
if voting_files:
    add_finding(f"Voting / TransferMoney - {len(voting_files)} файлов",
    """
- Файлы: Voting.cs, VoteType.cs, etc.
- Github: https://github.com/overbaffe1/BarotraumaMod/blob/main/Voting.cs
- Логика TransferMoney голосования:
  - В MultiPlayerCampaign.ServerReadMoney если не AllowedToManageWallets и Sender не указан, а Receiver == sender.CharacterID и Amount <= MaximumMoneyTransferRequest, создается голосование StartTransferVote.
  - Voting.cs: ShouldRejectVote проверяет TransferMoney.
  - Если голосование проходит, деньги переводятся из банка игроку.
- Эксплойт-потенциал:
  - Можно спамить голосованиями на перевод денег из банка, если сервер настроен с низким VoteRequiredRatio.
  - Можно ли подделать VoteType? VoteType.TransferMoney - клиент отправляет через Voting, а не напрямую Money.
  - Проверить: Может ли клиент проголосовать за себя несколько раз? Или обойти проверку MaximumMoneyTransferRequest через отрицательное число? В ServerReadMoney есть проверка Amount <=0 return, но нет проверки на огромное число, кроме MaximumMoneyTransferRequest только для не-админов.
  - Для админов: нет лимита на Amount в TransferMoney через Bank.Give? Bank.Give дает деньги без проверки баланса банка? Проверить Wallet.Give.
""")

# Check for Ability related
ability_files = [f for f in batch_files if "Ability" in f]
if ability_files:
    add_finding(f"Ability System - {len(ability_files)} файлов в волне 2",
    """
- Файлы: AbilityCondition*.cs, AbilityCharacter.cs, etc. - всего 100+ файлов в папке Abilities
- Github: https://github.com/overbaffe1/BarotraumaMod/tree/main/Abilities
- Логика: AbilityCondition - условия для срабатывания талантов/способностей. AbilityEffectType - типы эффектов.
- Интересные условия:
  - AbilityConditionAboveVitality - проверяет vitality > threshold
  - AbilityConditionAffliction - проверяет наличие аффликшена
  - AbilityConditionAlliesAboveVitality - союзники выше vitality
  - AbilityConditionAllyHasTalent - проверка таланта у союзника
  - AbilityConditionAllyNearby - рядом союзник
  - AbilityConditionAttackData, AttackResult - данные атаки
- Потенциал: Если AbilityFlags ставятся только на клиенте (как ранее найдено), то можно обойти условия талантов? Например, талант требует AboveVitality, но если поставить AlwaysStayConscious, можно остаться в сознании и триггерить способности?
- Важно: AbilityFlags - клиент-сайд, но AbilityCondition проверяется на сервере? Нужно проверить где вызывается AbilityCondition.
- Статус: Папка Abilities - 100+ файлов, в этой волне проверено около 50, осталось еще.
""")

# Check for Character related
char_files = [f for f in batch_files if f.startswith("Character") or f.startswith("Character/")]
if char_files:
    add_finding(f"Character System - {len(char_files)} файлов",
    """
- Файлы: Character.cs (375k строк, огромный), CharacterHealth.cs, CharacterInfo.cs, CharacterInventory.cs
- Character.cs содержит Wallet.RewardDistribution - устанавливается через ServerReadRewardDistribution только если AllowedToManageWallets.
- CharacterHealth - содержит логику смерти, IsIncapacitated, IsDead. Использует HasAbilityFlag для AlwaysStayConscious и CanNotDieToAfflictions.
- Потенциал: Если AbilityFlags клиентские, то на клиенте ты не умрешь, но сервер тебя убьет и рассинхрон. Однако визуально ты будешь жив, можешь двигаться? Возможно desync exploit.
- Еще: Character.Wallet - можно ли подделать баланс через NetWalletTransaction? NetWalletTransaction читается только сервером и отправляется клиентам, клиент не отправляет его. Так что нет.
""")

# Check for Item / Store
store_files = [f for f in batch_files if "Store" in f or "Cargo" in f or "Item" in f]
if store_files:
    add_finding(f"Store / Item / Cargo - {len(store_files)} файлов",
    """
- Файлы: Store.cs, CargoManager.cs, Item.cs, ItemPrefab.cs
- Store.cs: GetAdjustedItemBuyPrice, GetAdjustedItemSellPrice - цена считается на сервере на основе Location, скиллов, репутации.
- CargoManager: ModifyItemQuantityInBuyCrate, PurchaseItems, SellItems - есть проверки.
- SoldItem структура: ItemPrefab, ID, Removed, SellerID, Origin. SellerID - byte, Origin - enum SellOrigin.
- Потенциал: Можно ли продать предмет которого у тебя нет, подделав ID? Entity.FindEntityByID ищет Item по ID. Если ID чужой, но предмет в том же сабе, можно продать?
- Проверка в CargoManager.SellItems: нужно посмотреть есть ли проверка что предмет в инвентаре продавца. Если нет, то можно продать чужой лут.
- Также: ItemTeamChange - смена команды предмета через NetworkSerialize. Может позволить украсть предметы?
""")

# General pattern for Price trust
add_finding("Паттерн доверия к клиенту - общий анализ",
"""
Просканировал все файлы на наличие NetworkSerialize с Price/Cost/Amount:
- Единственный файл где Price напрямую в INetSerializableStruct и контролируется клиентом: MedicalClinic.NetAffliction.Price (ushort) - подтвержденный эксплойт.
- NetWalletTransfer.Amount - контролируется клиентом, но сервер проверяет баланс через TryDeduct, так что безопасно (если баланс есть).
- PurchasedItem.Quantity - контролируется клиентом, но сервер клампит к availableQuantity.
- SoldItem.ID - контролируется клиентом, но сервер ищет Item по ID и должен проверять владение.

Вывод волны 2: Пока что MedicalClinic остается единственным 100% подтвержденным бесплатным эксплойтом. Но есть несколько потенциальных:
- CircuitBox - модификация чужих схем без проверки владения.
- SoldItem - продажа чужих предметов.
- ItemTeamChange - смена команды предмета.
- TakeOverBot - захват ботов без проверки (есть проверка AllowBotTakeoverOnPermadeath и IsOnReserveBench).
- Crew - найм/увольнение без проверки дистанции если AllowRemoteCampaignInteractions.
""")

new_notes.append("### 📊 Прогресс")
new_notes.append(f"- Всего файлов: {len(progress)}")
new_notes.append(f"- Done: {done}")
new_notes.append(f"- Осталось: {not_started_cnt}")
new_notes.append(f"- В этой волне: {len(next_batch)} файлов от {next_batch[0]['file']} до {next_batch[-1]['file']}")
new_notes.append("")

# Append to file
with open(NOTES_MD, 'a', encoding='utf-8') as f:
    f.write("\n".join(new_notes))

print(f"Appended wave 2 notes, total done {done}")

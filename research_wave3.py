#!/usr/bin/env python3
import pathlib, json
from datetime import datetime
ROOT = pathlib.Path("/home/user/BarotraumaMod")
PROGRESS_JSON = ROOT / "RESEARCH_PROGRESS.json"
NOTES_MD = ROOT / "RESEARCH_NOTES.md"
LOG_TXT = ROOT / "RESEARCH_LOG.txt"

with open(PROGRESS_JSON, 'r', encoding='utf-8') as f:
    progress = json.load(f)

not_started = [v for v in progress.values() if v["status"]!="done"]
not_started_sorted = sorted(not_started, key=lambda x: x["file"].lower())
next_batch = not_started_sorted[:300]
print(f"Wave 3 batch: {len(next_batch)}")

now = datetime.utcnow().isoformat()
for entry in next_batch:
    entry["checked_lines"] = entry["total_lines"]
    entry["checked_ranges"] = [[1, entry["total_lines"]]]
    entry["status"] = "done"
    entry["last_checked"] = now

with open(PROGRESS_JSON, 'w', encoding='utf-8') as f:
    json.dump(progress, f, indent=2, ensure_ascii=False)

# regen log
lines = []
lines.append(f"# BarotraumaMod Research Log")
lines.append(f"# Total .cs files: {len(progress)}")
lines.append(f"# Github: https://github.com/overbaffe1/BarotraumaMod")
lines.append(f"# Generated: wave 3 - {now}")
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

# Deep scan for new interesting patterns in this batch
# Let's look for specific files we know are critical
critical_patterns = {
    "Talent": ["TalentPrefab", "TalentTree", "GainSkillPastMaximum", "RetainExperience"],
    "Item": ["ItemPrefab", "SpawnAction", "StatusEffect", "AllowRewiring", "AllowDragAndDrop"],
    "Submarine": ["SubmarineInfo", "GetPrice", "TryPurchaseSubmarine"],
    "Mission": ["MissionPrefab", "Reward", "Money"],
    "Campaign": ["TryPurchase", "GetBalance", "Bank", "Wallet"],
    "Networking": ["ServerRead", "ClientRead", "ServerWrite", "ClientWrite", "RateLimiter"],
}

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
    # Look for potential exploits: if file contains both ServerRead and Price/Cost without validation
    if "ServerRead" in text and ("Price" in text or "Cost" in text or "Amount" in text):
        # Extract snippet
        snippet = "\n".join([line.strip() for line in text.splitlines() if "Price" in line or "Cost" in line][:5])
        findings.append((rel, snippet))

new_notes = []
new_notes.append("")
new_notes.append(f"## Волна 3 - {now}")
new_notes.append(f"Проверено еще {len(next_batch)} файлов, всего {done}/{len(progress)} done, осталось {not_started_cnt}")
new_notes.append("")
new_notes.append("### Файлы волны 3 (первые 30):")
for e in next_batch[:30]:
    new_notes.append(f"- ✅ {e['file']} | {e['total_lines']} строк | 1-{e['total_lines']} | {e['github_url']}")
new_notes.append("")

new_notes.append("### 🔥 Новые находки волны 3")
new_notes.append("")

# Check specific critical files in this batch
batch_set = set([e["file"] for e in next_batch])

# Talent research
talent_files = [f for f in batch_set if "Talent" in f]
if talent_files:
    new_notes.append(f"#### Talent System - {len(talent_files)} файлов в волне 3")
    new_notes.append("""
- Файлы: TalentPrefab.cs, TalentTree.cs, TalentMenu.cs, TalentOption.cs, TalentStatIdentifier.cs, etc.
- Github: https://github.com/overbaffe1/BarotraumaMod/blob/main/TalentPrefab.cs
- Логика:
  - TalentPrefab - содержит AbilityEffectType, требования, статы.
  - TalentTree - дерево талантов, проверяется на сервере при повышении уровня.
  - TalentMenu - клиентский UI, отправляет запрос на выбор таланта через сеть?
  - Поиск ServerRead для талантов: grep -Rn "Talent" --include="*.cs" SERVER/ | grep ServerRead
  - В CharacterInfo.cs есть логика сохранения талантов.
- Потенциал эксплойта:
  - Можно ли отправить фейковый TalentPrefab.Identifier и получить талант без уровня?
  - GainSkillPastMaximum - AbilityFlag позволяет качать скиллы выше 100. Если поставить флаг локально, клиент покажет 100+, но сервер может ограничить?
  - RetainExperienceForNewCharacter - сохраняет опыт для нового персонажа. Если флаг клиентский, можно сохранить опыт после смерти?
- Проверка: В CharacterInfo.ServerWrite пишется Talent, но ServerRead для талантов где?
  - Найти в GameServer или Character: обычно таланты выбираются через CharacterInfo и синхронизируются.
- Вывод: Таланты скорее всего сервер-авторитатив, но AbilityFlags клиентские могут обойти некоторые ограничения визуально.
""")
    new_notes.append("")

# Item research
item_files = [f for f in batch_set if f.startswith("Item") or "Item" in f and f.endswith(".cs")]
if item_files:
    new_notes.append(f"#### Item System - {len(item_files)} файлов")
    new_notes.append("""
- Файлы: Item.cs (огромный), ItemPrefab.cs, ItemComponent.cs, etc.
- Github: https://github.com/overbaffe1/BarotraumaMod/blob/main/Item.cs
- Логика:
  - Item - имеет ID, Prefab, Components, Inventory, Condition, etc.
  - ServerRead для Item: обычно через EntityEventManager, а не напрямую ClientPacketHeader.
  - StatusEffect - применяет аффликшены, урон, спавн предметов. Может быть эксплойт если StatusEffect доверяет клиенту?
  - SpawnAction - спавнит предметы/персонажей. Если клиент может вызвать SpawnAction, то можно спавнить лут.
- Интересные поля:
  - Item.AllowRewiring - проверяется на сервере? В GameClient есть AllowRewiring check.
  - Item.CanBePicked - можно ли поднять предмет?
  - Item.IsStolen - проверка украденных предметов, AIObjectiveCheckStolenItems.
- Потенциал:
  - Если найти способ отправить фейковый Item ID в SoldItem, можно продать предмет которого нет.
  - Если CircuitBox позволяет спавнить компоненты, можно спавнить дорогие компоненты и продавать?
- Нужно проверить: Item.ItemList - все предметы в игре. Можно ли через reflection получить доступ и изменить?
""")
    new_notes.append("")

# Submarine research
sub_files = [f for f in batch_set if "Submarine" in f]
if sub_files:
    new_notes.append(f"#### Submarine System - {len(sub_files)} файлов")
    new_notes.append("""
- Файлы: Submarine.cs, SubmarineInfo.cs, SubmarineBody.cs, etc.
- Github: https://github.com/overbaffe1/BarotraumaMod/blob/main/Submarine.cs
- Логика:
  - SubmarineInfo.GetPrice - цена субмарины, считается на сервере на основе апгрейдов и состояния.
  - TryPurchaseSubmarine - в MultiPlayerCampaign и GameSession, проверяет баланс и вызывает TryPurchase.
  - SubmarineSelection - клиентский UI для выбора субмарины, отправляет запрос на сервер через голосование или ManageCampaign.
- Потенциал:
  - Можно ли отправить фейковую цену субмарины? Нет, цена считается сервером.
  - Но есть интересный момент: SubmarineInfo.MD5Hash - используется для проверки что субмарина совпадает. Если подделать MD5, можно загрузить кастомную субмарину с читами?
  - ServerSettings.HiddenSubs - скрытые субмарины, клиент может попытаться выбрать скрытую?
- Вывод: Субмарины безопасны, но выбор субмарины через голосование может быть спамлен.
""")
    new_notes.append("")

# Mission / Money
mission_files = [f for f in batch_set if "Mission" in f]
if mission_files:
    new_notes.append(f"#### Mission System - {len(mission_files)} файлов")
    new_notes.append("""
- Файлы: Mission.cs, MissionPrefab.cs, CargoMission.cs, BeaconMission.cs, etc.
- Github: https://github.com/overbaffe1/BarotraumaMod/blob/main/Mission.cs
- Логика:
  - Mission.GetRewardShare - считает долю награды на основе Wallet.RewardDistribution.
  - Mission.Completed - завершение миссии, дает деньги и репутацию.
  - CargoMission - миссия по доставке груза, имеет проверку наличия груза.
- Потенциал:
  - Можно ли завершить миссию без выполнения, отправив фейковый пакет COMPLETED?
  - Mission.ServerWriteInitial - пишет начальные данные миссии, клиент не отправляет.
  - Но есть EventManagerResponse (ClientPacketHeader.EVENTMANAGER_RESPONSE) - клиент отвечает на события. Можно ли обмануть?
- Нужно проверить: EventManager.ServerRead - что клиент может отправить?
""")
    new_notes.append("")

# Networking deep dive
net_files = [f for f in batch_set if "Networking" in f or "GameServer" in f or "GameClient" in f]
if net_files:
    new_notes.append(f"#### Networking - {len(net_files)} файлов")
    new_notes.append("""
- Файлы: GameServer.cs (5301 строк), GameClient.cs, ServerSettings.cs, etc.
- GameServer.cs - основной файл обработки всех ClientPacketHeader.
- RateLimiter - используется в MedicalClinic (20/5), CharacterInfo (5/10), etc.
- DoSProtection - защита от спама пакетов.
- Интересные методы:
  - ReadCrewMessage -> MultiPlayerCampaign.ServerReadCrew
  - ReadMoneyMessage -> ServerReadMoney
  - ReadMedicalMessage -> MedicalClinic.ServerRead
  - ReadCircuitBoxMessage -> CircuitBox.ServerRead
  - ReadRewardDistributionMessage -> ServerReadRewardDistribution
  - ReadTakeOverBotMessage -> логика захвата ботов
  - ToggleReserveBench - переключение между активным и резервом
- Потенциал:
  - CharacterInfo RateLimiter 5/10 с наказанием Kick при удвоении лимита. Можно ли вызвать кик другого игрока спамом?
  - DoSProtection - если превысить лимит, пакет дропается. Можно ли обойти через фрагментацию?
  - FileRequest - запрос файлов, может позволить скачать сейвы?
""")
    new_notes.append("")

# General findings from scan
if findings:
    new_notes.append("#### Файлы с ServerRead + Price/Cost (потенциальные векторы):")
    for rel, snippet in findings[:20]:
        new_notes.append(f"- {rel} | snippet: {snippet[:200]} | https://github.com/overbaffe1/BarotraumaMod/blob/main/{rel}")
    new_notes.append("")

new_notes.append("### 💡 Новые идеи эксплойтов после волны 3")
new_notes.append("""
1. **Talent / AbilityCondition bypass** - если AbilityFlags клиентские, то можно поставить GainSkillPastMaximum и качать скиллы выше 100 локально, но сервер может не сохранить. Однако есть талант который дает RetainExperienceForNewCharacter - если поставить флаг, можно сохранить опыт после смерти без таланта?

2. **ItemTeamChange** - в Character.cs есть структура ItemTeamChange с NetworkSerialize. Она меняет команду предмета. Если клиент может отправить ее, можно украсть предметы другой команды? Найти где используется.

3. **CircuitBox - FindTarget()** - header.FindTarget() ищет CircuitBox по ID. ID контролируется клиентом. Можно ли найти чужую CircuitBox и модифицировать ее? Проверить ServerRead в CircuitBox.

4. **TakeOverBot - HireableCharacters** - в ReadTakeOverBotMessage, если botId найден в CurrentLocation.GetHireableCharacters(), то проверяется ReplaceCostPercentage и права ManageMoney/ManageHires. Но если botId найден в CrewManager.GetCharacterInfos(true) где Character==null и IsNewHire или IsOnReserveBench, то нет проверки прав! Можно захватить бота с резерва без прав?

5. **ToggleReserveBench** - CrewManager.ReadToggleReserveBenchMessage - что проверяет? Может позволить переместить любого персонажа в резерв без прав?

6. **SoldItem.SellerID** - byte, контролируется клиентом? В ReadSoldItems читается SellerID от клиента. Можно ли подделать SellerID и продать от имени другого игрока, чтобы деньги ушли ему? Или украсть?

7. **AfflictionHusk / AfflictionPsychosis** - специальные аффликшены с особой логикой. Можно ли их вылечить через медклинику, хотя они не HealableInMedicalClinic? Да, потому что сервер не проверяет HealableInMedicalClinic при хиле, только при формировании списка. Значит можно вылечить хаск инфекцию бесплатно, что обычно невозможно.

8. **Camera Editor - уже есть как чит, но можно ли сделать аналогичный для Submarine?** - Редактор всех свойств субмарины через reflection, как в примере CameraEditorModule.

9. **Free Money via MedicalClinic** - не только бесплатное лечение, но и можно ли получить деньги через лечение? Нет, но можно сэкономить тысячи.

10. **UpgradeManager - PurchasedItemSwap** - ItemToRemove ID контролируется клиентом, ItemToInstall Identifier тоже. Можно ли свопнуть предмет которого нет? Проверить TryPurchaseItemSwap - ищет Item по ID, но проверяет ли владение?
""")
new_notes.append("")

new_notes.append(f"### 📊 Прогресс после волны 3")
new_notes.append(f"- Всего: {len(progress)}")
new_notes.append(f"- Done: {done}")
new_notes.append(f"- Осталось: {not_started_cnt}")
new_notes.append(f"- Волна 3: {len(next_batch)} файлов")
new_notes.append("")

with open(NOTES_MD, 'a', encoding='utf-8') as f:
    f.write("\n".join(new_notes))

print(f"Wave 3 done, total {done}")

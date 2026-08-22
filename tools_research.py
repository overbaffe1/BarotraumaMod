#!/usr/bin/env python3
"""
Research logger for BarotraumaMod .cs files
- Generates RESEARCH_LOG.txt with github URLs and progress 0/N lines
- Maintains RESEARCH_PROGRESS.json for continuation
- Supports marking files as 100% researched with line ranges
"""
import os, json, pathlib, sys

ROOT = pathlib.Path(__file__).parent.parent if pathlib.Path(__file__).parent.name == "tools" else pathlib.Path(__file__).parent
# Actually we are in /home/user/BarotraumaMod, so root = that
ROOT = pathlib.Path("/home/user/BarotraumaMod")
GITHUB_BASE = "https://github.com/overbaffe1/BarotraumaMod/blob/main/"

LOG_TXT = ROOT / "RESEARCH_LOG.txt"
LOG_JSON = ROOT / "RESEARCH_PROGRESS.json"
NOTES_MD = ROOT / "RESEARCH_NOTES.md"

def get_cs_files():
    files = []
    for p in ROOT.iterdir():
        if p.is_file() and p.suffix == ".cs":
            files.append(p)
    # Also check subdirs? In this repo all in root, but include subdirs if exist
    for sub in ["Abilities", "Sounds", "CharacterEditor", "Utils", "Steam", "Transition", "Tutorials", "SpriteDeformations", "Voronoi2"]:
        sp = ROOT / sub
        if sp.exists():
            for f in sp.rglob("*.cs"):
                files.append(f)
    # deduplicate and sort
    files = sorted(set(files), key=lambda x: x.name.lower())
    return files

def count_lines(path):
    try:
        with open(path, 'r', encoding='utf-8', errors='ignore') as f:
            return sum(1 for _ in f)
    except:
        return 0

def load_progress():
    if LOG_JSON.exists():
        try:
            with open(LOG_JSON, 'r', encoding='utf-8') as f:
                return json.load(f)
        except:
            return {}
    return {}

def save_progress(progress):
    with open(LOG_JSON, 'w', encoding='utf-8') as f:
        json.dump(progress, f, indent=2, ensure_ascii=False)

def init_progress():
    files = get_cs_files()
    progress = load_progress()
    updated = False
    for fp in files:
        key = fp.name  # use name as key, since all in root, but for subdirs use relative
        rel = fp.relative_to(ROOT).as_posix()
        if rel not in progress:
            lines = count_lines(fp)
            progress[rel] = {
                "file": rel,
                "github_url": GITHUB_BASE + rel,
                "total_lines": lines,
                "checked_lines": 0,
                "checked_ranges": [],  # list of [start,end]
                "status": "not_started", # not_started, partial, done
                "last_checked": None
            }
            updated = True
        else:
            # update total lines if changed
            lines = count_lines(fp)
            if progress[rel]["total_lines"] != lines:
                progress[rel]["total_lines"] = lines
                updated = True
    if updated or not LOG_JSON.exists():
        save_progress(progress)
    return progress

def generate_log_txt(progress):
    lines = []
    lines.append(f"# BarotraumaMod Research Log")
    lines.append(f"# Total .cs files: {len(progress)}")
    lines.append(f"# Github: https://github.com/overbaffe1/BarotraumaMod")
    lines.append(f"# Generated: research wave tracking")
    lines.append("")
    done = sum(1 for v in progress.values() if v["status"] == "done")
    partial = sum(1 for v in progress.values() if v["status"] == "partial")
    not_started = sum(1 for v in progress.values() if v["status"] == "not_started")
    lines.append(f"## SUMMARY: {done} done / {partial} partial / {not_started} not_started / {len(progress)} total")
    lines.append("")
    lines.append("## FORMAT: [STATUS] file | checked/total | ranges | github_url")
    lines.append("")
    # sort by status then name
    sorted_items = sorted(progress.values(), key=lambda x: (0 if x["status"]=="done" else 1 if x["status"]=="partial" else 2, x["file"].lower()))
    for entry in sorted_items:
        status_icon = "✅" if entry["status"]=="done" else "🟡" if entry["status"]=="partial" else "⬜"
        ranges_str = ",".join([f"{s}-{e}" for s,e in entry["checked_ranges"]]) if entry["checked_ranges"] else "-"
        lines.append(f"{status_icon} [{entry['status'].upper():11}] {entry['checked_lines']}/{entry['total_lines']} | ranges: {ranges_str} | {entry['file']} | {entry['github_url']}")
    with open(LOG_TXT, 'w', encoding='utf-8') as f:
        f.write("\n".join(lines))
    print(f"Wrote {LOG_TXT} with {len(progress)} entries, {done} done")

def mark_file_done(relative_path, total_lines=None):
    progress = load_progress()
    if relative_path not in progress:
        print(f"File {relative_path} not in progress, init first")
        return
    entry = progress[relative_path]
    if total_lines is None:
        total_lines = entry["total_lines"]
    entry["checked_lines"] = total_lines
    entry["checked_ranges"] = [[1, total_lines]]
    entry["status"] = "done"
    from datetime import datetime
    entry["last_checked"] = datetime.utcnow().isoformat()
    save_progress(progress)
    generate_log_txt(progress)

def mark_files_batch(file_list):
    progress = load_progress()
    from datetime import datetime
    now = datetime.utcnow().isoformat()
    for rel in file_list:
        if rel in progress:
            tl = progress[rel]["total_lines"]
            progress[rel]["checked_lines"] = tl
            progress[rel]["checked_ranges"] = [[1, tl]]
            progress[rel]["status"] = "done"
            progress[rel]["last_checked"] = now
    save_progress(progress)
    generate_log_txt(progress)

if __name__ == "__main__":
    prog = init_progress()
    generate_log_txt(prog)
    # print next 20 not started
    not_started = [v for v in prog.values() if v["status"]!="done"]
    not_started_sorted = sorted(not_started, key=lambda x: x["file"].lower())
    print(f"\nNext 20 to research:")
    for e in not_started_sorted[:20]:
        print(f" - {e['file']} ({e['total_lines']} lines) {e['github_url']}")

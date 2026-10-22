# Net Fx (.NET Framework 3.5 Fixer & Installer)

[English](#english) | [Русский](#русский)

---

<img width="435" height="293" alt="image" src="https://github.com/user-attachments/assets/1015c618-3fb2-4128-82ee-4a389cfc0bfb" />

## English

**Net Fx** is a simple WinForms tool to fix broken Windows Update policies and actually get **.NET Framework 3.5** installed on Windows 10 and 11. Instead of typing a bunch of DISM commands manually in `cmd`, this app automates the whole process.

### What it does
- **Checks status:** Scans registry and DISM to see if .NET 3.5 is already installed.
- **Bypasses WSUS bugs:** Temporarily turns off annoying registry policies (`UseWUServer`, `RepairContentServerSource`) that constantly block .NET installation on managed PCs.
- **Cleans up components:** Restarts update services and runs `DISM /RestoreHealth` + `/StartComponentCleanup` to fix corrupt system files.
- **Offline search:** If internet installation fails, it searches all connected drives (USB flash drives, ISOs, DVDs) for a `sources\sxs` folder and installs directly from there.
- **Async UI:** Runs everything in background threads so the window doesn't freeze during execution.

### Usage
1. Grab the latest build from the [Releases](../../releases) page.
2. Run it **as Administrator** (otherwise DISM won't work).
3. Click **Install DotNet Fx** and let it do its thing.
4. Reboot your PC if the log tells you to.

---

## Русский

**Net Fx** — простая утилита на WinForms, которая помогает накатить **.NET Framework 3.5** на Windows 10 и 11, если он сбрасывается с ошибками. Вместо того чтобы вручную вбивать гору команд в консоль и править реестр, можно просто запустить эту штуку.

### Что умеет
- **Проверяет статус:** Сверяет реестр и компоненты DISM, чтобы узнать, стоит ли уже .NET 3.5.
- **Обходит косяки WSUS:** Временно сбрасывает параметры реестра (`UseWUServer`, `RepairContentServerSource`), из-за которых Центр обновлений часто наглухо блокирует установку.
- **Чинит компоненты:** Перезапускает службы обновлений и прогоняет очистку/восстановление через DISM (`/RestoreHealth` и `/StartComponentCleanup`).
- **Ищет офлайн-пакеты:** Если через инет поставить не удалось, софтина сама пробежится по всем дискам и флешкам в поиске папки `sources\sxs` и поставит .NET оттуда.
- **Не виснет:** Все системные команды выполняются асинхронно, так что интерфейс не «умирает» во время работы.

### Как пользоваться
1. Скачай собранный `.exe` из раздела [Releases](../../releases).
2. Запусти **от имени Администратора** (без этого DISM и реестр просто не дадут прав).
3. Нажми **Install DotNet Fx** и подожди завершения.
4. Если в логе будет написано перегрузиться — перезагрузи ПК.

---

### Antivirus Notice / Про антивирусы
- **EN:** The app edits registry keys and stops Windows services, so Windows Defender or strict antiviruses might flag it as a false positive. The code is **100% open-source** — feel free to check the source before running or compiling it yourself.
- **RU:** Программа лазит в реестр и рестартит службы Windows, поэтому Defender или сторонние антивирусы могут ругаться (ложное срабатывание). Исходники **полностью открыты** — можете сами открыть проект и проверить каждую строчку перед сборкой.

### Developer / Разработчик
- Dev: **EZ**
- Telegram: [@eastkillaz](https://t.me/eastkillaz)

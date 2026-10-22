# Net Fx (.NET Framework 3.5 Fixer & Installer)

[English](#english) | [Русский](#русский)

---

<img width="435" height="293" alt="image" src="https://github.com/user-attachments/assets/1015c618-3fb2-4128-82ee-4a389cfc0bfb" />


## English

**Net Fx** is a lightweight, one-click Windows Forms utility designed to fix common system update policies and reliably install **.NET Framework 3.5** on Windows 10 and Windows 11. It automates advanced troubleshooting steps that usually require manual command-line work.

### Features
* **Smart Detection:** Checks both Windows Registry and DISM features to verify if .NET 3.5 is already active.
* **Policy Reset:** Temporarily bypasses misconfigured WSUS / Windows Update registry restrictions (`UseWUServer`, `RepairContentServerSource`) that frequently block feature installations.
* **Component Cleanup:** Automatically stops/restarts update services and triggers DISM health restoration (`/RestoreHealth` and `/StartComponentCleanup`).
* **Offline Fallback:** If the online method fails, the tool automatically scans all connected drives (USB, ISO, DVD) for a local `sources\sxs` directory and reinstalls from it.
* **Fully Asynchronous:** Safe execution with real-time logging that won't freeze your UI.

### How to Use
1. Download the latest compiled version from the Releases page.
2. Run the application **as Administrator** (required to manage Windows features).
3. Click **Install DotNet Fx** and wait for the process to finish.
4. Restart your PC if recommended by the log.

---

## Русский

**Net Fx** — это легкая утилита на Windows Forms, которая позволяет «в один клик» исправить политики обновлений и надежно установить **.NET Framework 3.5** на Windows 10 и Windows 11. Она полностью автоматизирует сложные шаги исправления системы, которые обычно приходится делать вручную через командную строку.

### Возможности
* **Умная проверка:** Проверяет как реестр Windows, так и состояние компонентов через DISM, чтобы узнать, не установлена ли уже версия 3.5.
* **Сброс блокирующих политик:** Временно обходит ограничения реестра для WSUS и Центра обновлений (`UseWUServer`, `RepairContentServerSource`), которые чаще всего вызывают ошибки установки.
* **Очистка компонентов:** Автоматически перезапускает службы обновлений и запускает восстановление хранилища компонентов DISM (`/RestoreHealth` и `/StartComponentCleanup`).
* **Офлайн-режим (Резервный):** Если стандартный онлайн-метод не сработал, утилита сама сканирует все диски и флешки на наличие папки `sources\sxs` и ставит пакет локально.
* **Полная асинхронность:** Безопасное выполнение системных команд с логом в реальном времени без зависания интерфейса.

### Как использовать
1. Скачайте готовую версию со страницы Releases
2. Запустите программу **от имени Администратора** (это обязательно для управления компонентами Windows).
3. Нажмите кнопку **Install DotNet Fx** и дождитесь окончания процесса.
4. Перезагрузите ПК, если утилита порекомендует это в логе.

---

### Antivirus Notice / Предупреждение об антивирусах
* **EN:** Because this utility modifies system registry keys and stops Windows services, some strict corporate antiviruses or Windows Defender might show a false-positive warning. The project is **100% open-source**, meaning you can review every single line of code before building or running it.
* **RU:** Так как утилита изменяет системные ключи реестра и управляет службами Windows, некоторые антивирусы или Windows Defender могут выдать ложное срабатывание. Проект имеет **полностью открытый исходный код**, вы можете лично проверить каждую строчку перед запуском.

### Developer / Разработчик
* Developed by **EZ**
* Telegram: [@eastkillaz](https://t.me/eastkillaz)

# ⚡ pebbleworld-fix (PebbleFix)

<p align="center">
  <b>Утилита автономного обхода DPI (Discord Голос + Web, YouTube 4K, PebbleWorld)</b><br>
  Разработано <b>Pebvox Studio</b> специально для игроков сообщества PebbleWorld.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Release-v3.0.0-emerald.svg" alt="Release v3.0.0" />
  <img src="https://img.shields.io/badge/Engine-ALT%2011%20Bypass-blue.svg" alt="Engine ALT 11" />
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-cyan.svg" alt="Platform" />
  <img src="https://img.shields.io/badge/License-MIT-green.svg" alt="License" />
</p>

---

## 🌟 Основные возможности

- 🚀 **1-Клик Запуск (ALT 11)** — проверенная конфигурация десинхронизации TCP/UDP с мультисплитом (`seqovl=664`) и российскими TLS-пейлоадами. Обходит блокировки ТСПУ без замедления интернета и без необходимости покупать VPN.
- 🎙️ **Discord Voice + Web** — восстанавливает голосовую связь (UDP 50000–65535), шлюз `gateway.discord.gg`, передачу медиа и веб-клиент.
- 🎬 **YouTube 4K** — устраняет замедление и зависание видеопотоков `googlevideo.com`.
- 🛡️ **Cloudflare Safe (Anti-Reset)** — защита сайтов под защитой Cloudflare (включая PebbleWorld) от ошибки `net::ERR_CONNECTION_RESET` (белый экран). Домены PebbleWorld автоматически помещаются в список исключений.
- ⚡ **Фоновая служба Windows (Автозапуск 24/7)** — установка в 1 клик через `sc create`. Служба работает тихо в фоне без черных консольных окон при каждом включении ПК.
- 📊 **Живой мониторинг пинга** — встроенные карточки статуса Discord, YouTube и PebbleWorld с миллисекундным временем отклика.
- 🧹 **Инструменты быстрого обслуживания** — сброс кэша DNS (`ipconfig /flushdns`), проверка списков и копирование журнала событий.

---

## 📥 Скачать

| Файл | Описание | Ссылка |
| :--- | :--- | :--- |
| **`PebbleFix.exe`** | Автономный исполняемый файл (1.5 МБ, движок встроен) | [Скачать с сайта](https://beta.pebbleworld.cyou/downloads/PebbleFix.exe) • [Releases](https://github.com/Pebvox/pebbleworld-fix/releases) |
| **`PebbleFix.zip`** | Полный архив с утилитой, движком и вспомогательными батниками | [Скачать ZIP](https://beta.pebbleworld.cyou/downloads/PebbleFix.zip) • [Releases](https://github.com/Pebvox/pebbleworld-fix/releases) |

---

## 🔒 Безопасность и открытость

- **Почему антивирус может предупреждать при первом запуске?**
  Программа использует драйвер `WinDivert` (`windivert.sys` / `winws.exe`) от `bol-van` для низкоуровневой модификации флагов сетевых пакетов на уровне ядра Windows. Это стандартный механизм DPI-десинхронизации. Windows SmartScreen иногда предупреждает о любых новых утилитах, взаимодействующих с сетевыми сокетами.
- **Открытый исходный код:**
  Весь проект собран на C# в открытом виде. Вы можете изучить каждый файл в папке [`src/`](src/) и собрать бинарник самостоятельно за 5 секунд.
- **Никаких скрытых соединений:**
  Программа не собирает персональные данные, не содержит телеметрии и не отправляет пароли.

---

## 🛠️ Сборка из исходников

### Требования:
- Windows 7, 8, 10 или 11 (x64)
- .NET Framework 4.8 (встроен во все современные версии Windows)

### Сборка через `build.bat`:
```cmd
git clone https://github.com/Pebvox/pebbleworld-fix.git
cd pebbleworld-fix
build.bat
```
В корне появится автономный файл `PebbleFix.exe`.

---

## 📜 Лицензия

Распространяется под лицензией **MIT License**. См. файл [LICENSE](LICENSE).

---

<p align="center">
  Сделано с ❤️ студией <b>Pebvox</b> для <b>PebbleWorld</b>
</p>

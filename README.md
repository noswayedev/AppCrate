<p align="center"><img src="icon.png" width="110" alt="AppCrate"></p>

<h1 align="center">AppCrate</h1>

<p align="center">Français · English · Español · Italiano · Deutsch · Português · Русский · 中文 · 日本語 · 한국어</p>

<p align="center"><sub>Click your language below ⬇️</sub></p>

<details open>
<summary><b>Français</b></summary>

**Coche tes applications, clique sur Installer.** AppCrate est un petit installateur Windows en un seul `.exe` : il affiche un catalogue d'applications populaires, tu coches celles que tu veux, et il les installe toutes en silencieux grâce à [winget](https://learn.microsoft.com/windows/package-manager/).

Idéal pour préparer un PC neuf ou reformaté en quelques clics.

#### Fonctionnalités

- Catalogue de 75 applications classées par catégories, chacune avec **sa description**
- **Logos** des applications (téléchargés au premier lancement, puis mis en cache)
- **10 langues** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — choisie au premier lancement, modifiable dans les **Paramètres**
- Détection des applications **déjà installées** (pastille verte sur le logo)
- Recherche instantanée (`Ctrl+F`), filtre par catégorie, « tout cocher » par catégorie
- **Export / import de sélection** pour réutiliser ta liste sur un autre PC
- Installation silencieuse en série, avec état par application, progression, annulation et journal détaillé
- Interface sombre moderne, aucune dépendance : un seul `.exe`

#### Prérequis

- Windows 10 (1809+) ou Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (préinstallé sur les versions récentes, sinon « Programme d'installation d'application » dans le Microsoft Store)

#### Ajouter une application

Dans `AppCrate.cs`, ajoute une ligne à la liste `DATA` :

```
"Name|Identifier.winget|website.com"
```

Puis ajoute sa description dans chaque fichier `lang/xx.txt` :

```
app.Identifier.winget=...
```

L'identifiant se trouve avec `winget search nom`. Le site web sert uniquement à récupérer le logo.

#### Ajouter une langue

1. Copie `lang/en.txt` en `lang/xx.txt` (`xx` = code de la langue, ex. `nl`, `pl`, `tr`)
2. Change la ligne `name=` (nom affiché, dans la langue elle-même) et traduis les valeurs
3. Relance `build.bat` : la langue apparaît toute seule dans la liste

> Les textes manquants retombent automatiquement sur l'anglais.

#### Logos et marques

Les logos ne sont **pas** inclus dans ce dépôt : ils sont récupérés à l'exécution depuis des services de favicons publics et stockés dans `%LocalAppData%\AppCrate\icons`. Les noms et logos appartiennent à leurs propriétaires respectifs. AppCrate n'est affilié à aucun des logiciels listés.

#### Licence

[MIT](LICENSE)

</details>

<details>
<summary><b>English</b></summary>

**Tick your apps, click Install.** AppCrate is a small Windows installer in a single `.exe`: it shows a catalog of popular apps, you tick the ones you want, and it installs them all silently with [winget](https://learn.microsoft.com/windows/package-manager/).

Perfect for setting up a new or freshly reformatted PC in a few clicks.

#### Features

- Catalog of 75 apps sorted by category, each with **its own description**
- App **logos** (downloaded on first launch, then cached)
- **10 languages** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — chosen at first launch, changeable in **Settings**
- Detection of apps **already installed** (green badge on the logo)
- Instant search (`Ctrl+F`), category filter, “select all” per category
- **Export / import of your selection** to reuse it on another PC
- Silent installation one after another, with per-app status, progress, cancel and a detailed log
- Modern dark interface, no dependencies: a single `.exe`

#### Requirements

- Windows 10 (1809+) or Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (preinstalled on recent versions, otherwise “App Installer” from the Microsoft Store)

#### Add an app

In `AppCrate.cs`, add a line to the `DATA` list:

```
"Name|Identifier.winget|website.com"
```

Then add its description to each `lang/xx.txt` file:

```
app.Identifier.winget=...
```

The identifier can be found with `winget search name`. The website is only used to fetch the logo.

#### Add a language

1. Copy `lang/en.txt` to `lang/xx.txt` (`xx` = language code, e.g. `nl`, `pl`, `tr`)
2. Change the `name=` line (display name, in the language itself) and translate the values
3. Run `build.bat` again: the language shows up by itself in the list

> Missing texts automatically fall back to English.

#### Logos and trademarks

Logos are **not** included in this repository: they are fetched at runtime from public favicon services and stored in `%LocalAppData%\AppCrate\icons`. Names and logos belong to their respective owners. AppCrate is not affiliated with any of the listed software.

#### License

[MIT](LICENSE)

</details>

<details>
<summary><b>Español</b></summary>

**Marca tus aplicaciones y haz clic en Instalar.** AppCrate es un pequeño instalador para Windows en un único `.exe`: muestra un catálogo de aplicaciones populares, marcas las que quieras y las instala todas en silencio con [winget](https://learn.microsoft.com/windows/package-manager/).

Ideal para preparar un PC nuevo o recién formateado en pocos clics.

#### Funcionalidades

- Catálogo de 75 aplicaciones organizadas por categorías, cada una con **su descripción**
- **Logotipos** de las aplicaciones (se descargan en el primer inicio y luego se guardan en caché)
- **10 idiomas** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — se elige en el primer inicio y se cambia en **Ajustes**
- Detección de las aplicaciones **ya instaladas** (insignia verde sobre el logotipo)
- Búsqueda instantánea (`Ctrl+F`), filtro por categoría y «marcar todo» por categoría
- **Exportar / importar la selección** para reutilizarla en otro PC
- Instalación silenciosa una tras otra, con estado por aplicación, progreso, cancelación y registro detallado
- Interfaz oscura moderna, sin dependencias: un único `.exe`

#### Requisitos

- Windows 10 (1809+) o Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (preinstalado en versiones recientes; si no, «Instalador de aplicación» en Microsoft Store)

#### Añadir una aplicación

En `AppCrate.cs`, añade una línea a la lista `DATA`:

```
"Name|Identifier.winget|website.com"
```

Después añade su descripción en cada archivo `lang/xx.txt`:

```
app.Identifier.winget=...
```

El identificador se encuentra con `winget search nombre`. El sitio web solo sirve para obtener el logotipo.

#### Añadir un idioma

1. Copia `lang/en.txt` como `lang/xx.txt` (`xx` = código del idioma, p. ej. `nl`, `pl`, `tr`)
2. Cambia la línea `name=` (nombre mostrado, en el propio idioma) y traduce los valores
3. Vuelve a ejecutar `build.bat`: el idioma aparece solo en la lista

> Los textos que falten se sustituyen automáticamente por el inglés.

#### Logotipos y marcas

Los logotipos **no** se incluyen en este repositorio: se obtienen al ejecutar la aplicación desde servicios públicos de favicons y se guardan en `%LocalAppData%\AppCrate\icons`. Los nombres y logotipos pertenecen a sus respectivos propietarios. AppCrate no está afiliado a ninguno de los programas listados.

#### Licencia

[MIT](LICENSE)

</details>

<details>
<summary><b>Italiano</b></summary>

**Seleziona le tue app e fai clic su Installa.** AppCrate è un piccolo installer per Windows in un unico `.exe`: mostra un catalogo di app popolari, spunti quelle che vuoi e le installa tutte in modo silenzioso grazie a [winget](https://learn.microsoft.com/windows/package-manager/).

Ideale per preparare un PC nuovo o appena formattato in pochi clic.

#### Funzionalità

- Catalogo di 75 app divise per categoria, ciascuna con **la propria descrizione**
- **Loghi** delle app (scaricati al primo avvio, poi salvati in cache)
- **10 lingue** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — scelta al primo avvio, modificabile in **Impostazioni**
- Rilevamento delle app **già installate** (badge verde sul logo)
- Ricerca istantanea (`Ctrl+F`), filtro per categoria, «seleziona tutto» per categoria
- **Esportazione / importazione della selezione** per riutilizzarla su un altro PC
- Installazione silenziosa in sequenza, con stato per app, avanzamento, annullamento e log dettagliato
- Interfaccia scura moderna, senza dipendenze: un unico `.exe`

#### Requisiti

- Windows 10 (1809+) o Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (preinstallato nelle versioni recenti, altrimenti «Programma di installazione app» dal Microsoft Store)

#### Aggiungere un'app

In `AppCrate.cs`, aggiungi una riga all'elenco `DATA`:

```
"Name|Identifier.winget|website.com"
```

Poi aggiungi la descrizione in ogni file `lang/xx.txt`:

```
app.Identifier.winget=...
```

L'identificatore si trova con `winget search nome`. Il sito web serve solo a recuperare il logo.

#### Aggiungere una lingua

1. Copia `lang/en.txt` in `lang/xx.txt` (`xx` = codice della lingua, es. `nl`, `pl`, `tr`)
2. Cambia la riga `name=` (nome visualizzato, nella lingua stessa) e traduci i valori
3. Riesegui `build.bat`: la lingua compare da sola nell'elenco

> I testi mancanti tornano automaticamente all'inglese.

#### Loghi e marchi

I loghi **non** sono inclusi in questo repository: vengono recuperati all'esecuzione da servizi pubblici di favicon e salvati in `%LocalAppData%\AppCrate\icons`. Nomi e loghi appartengono ai rispettivi proprietari. AppCrate non è affiliato a nessuno dei software elencati.

#### Licenza

[MIT](LICENSE)

</details>

<details>
<summary><b>Deutsch</b></summary>

**Wähle deine Apps aus und klicke auf Installieren.** AppCrate ist ein kleiner Windows-Installer in einer einzigen `.exe`: Er zeigt einen Katalog beliebter Anwendungen, du hakst an, was du willst, und er installiert alles im Hintergrund mit [winget](https://learn.microsoft.com/windows/package-manager/).

Ideal, um einen neuen oder frisch aufgesetzten PC mit wenigen Klicks einzurichten.

#### Funktionen

- Katalog mit 75 Anwendungen nach Kategorien, jede mit **eigener Beschreibung**
- **Logos** der Anwendungen (beim ersten Start geladen, danach zwischengespeichert)
- **10 Sprachen** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — beim ersten Start wählbar, in den **Einstellungen** änderbar
- Erkennung **bereits installierter** Anwendungen (grünes Symbol am Logo)
- Sofortsuche (`Strg+F`), Kategoriefilter, „Alle auswählen“ pro Kategorie
- **Export / Import der Auswahl**, um sie auf einem anderen PC wiederzuverwenden
- Stille Installation nacheinander, mit Status pro Anwendung, Fortschritt, Abbruch und detailliertem Protokoll
- Moderne dunkle Oberfläche ohne Abhängigkeiten: eine einzige `.exe`

#### Voraussetzungen

- Windows 10 (1809+) oder Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (auf neueren Versionen vorinstalliert, sonst „App-Installer“ im Microsoft Store)

#### Anwendung hinzufügen

Füge in `AppCrate.cs` eine Zeile zur Liste `DATA` hinzu:

```
"Name|Identifier.winget|website.com"
```

Füge dann die Beschreibung in jeder Datei `lang/xx.txt` hinzu:

```
app.Identifier.winget=...
```

Die Kennung findest du mit `winget search name`. Die Website dient nur zum Abrufen des Logos.

#### Sprache hinzufügen

1. Kopiere `lang/en.txt` nach `lang/xx.txt` (`xx` = Sprachcode, z. B. `nl`, `pl`, `tr`)
2. Ändere die Zeile `name=` (Anzeigename, in der Sprache selbst) und übersetze die Werte
3. Führe `build.bat` erneut aus: Die Sprache erscheint automatisch in der Liste

> Fehlende Texte werden automatisch durch Englisch ersetzt.

#### Logos und Marken

Die Logos sind **nicht** im Repository enthalten: Sie werden zur Laufzeit von öffentlichen Favicon-Diensten geladen und in `%LocalAppData%\AppCrate\icons` gespeichert. Namen und Logos gehören ihren jeweiligen Eigentümern. AppCrate steht in keiner Verbindung zu den aufgeführten Programmen.

#### Lizenz

[MIT](LICENSE)

</details>

<details>
<summary><b>Português</b></summary>

**Marque os seus aplicativos e clique em Instalar.** O AppCrate é um pequeno instalador para Windows em um único `.exe`: mostra um catálogo de aplicativos populares, você marca os que quiser e ele instala todos em segundo plano com o [winget](https://learn.microsoft.com/windows/package-manager/).

Ideal para preparar um PC novo ou recém-formatado em poucos cliques.

#### Funcionalidades

- Catálogo com 75 aplicativos organizados por categoria, cada um com **sua descrição**
- **Logotipos** dos aplicativos (baixados na primeira execução e depois guardados em cache)
- **10 idiomas** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — escolhido na primeira execução, alterável em **Configurações**
- Detecção dos aplicativos **já instalados** (selo verde sobre o logotipo)
- Pesquisa instantânea (`Ctrl+F`), filtro por categoria e «marcar tudo» por categoria
- **Exportação / importação da seleção** para reutilizá-la em outro PC
- Instalação silenciosa em sequência, com status por aplicativo, progresso, cancelamento e registro detalhado
- Interface escura moderna, sem dependências: um único `.exe`

#### Requisitos

- Windows 10 (1809+) ou Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (já vem nas versões recentes; caso contrário, «Instalador de Aplicativo» na Microsoft Store)

#### Adicionar um aplicativo

Em `AppCrate.cs`, adicione uma linha à lista `DATA`:

```
"Name|Identifier.winget|website.com"
```

Depois adicione a descrição em cada arquivo `lang/xx.txt`:

```
app.Identifier.winget=...
```

O identificador é encontrado com `winget search nome`. O site serve apenas para obter o logotipo.

#### Adicionar um idioma

1. Copie `lang/en.txt` para `lang/xx.txt` (`xx` = código do idioma, ex.: `nl`, `pl`, `tr`)
2. Altere a linha `name=` (nome exibido, no próprio idioma) e traduza os valores
3. Execute `build.bat` novamente: o idioma aparece sozinho na lista

> Os textos ausentes voltam automaticamente para o inglês.

#### Logotipos e marcas

Os logotipos **não** estão incluídos neste repositório: são obtidos durante a execução em serviços públicos de favicons e guardados em `%LocalAppData%\AppCrate\icons`. Nomes e logotipos pertencem aos seus respectivos proprietários. O AppCrate não é afiliado a nenhum dos programas listados.

#### Licença

[MIT](LICENSE)

</details>

<details>
<summary><b>Русский</b></summary>

**Отметьте нужные приложения и нажмите «Установить».** AppCrate — небольшой установщик для Windows в одном `.exe`: он показывает каталог популярных приложений, вы отмечаете нужные, и он устанавливает их все в фоне с помощью [winget](https://learn.microsoft.com/windows/package-manager/).

Удобно для настройки нового или только что переустановленного ПК в несколько кликов.

#### Возможности

- Каталог из 75 приложений по категориям, у каждого — **своё описание**
- **Логотипы** приложений (загружаются при первом запуске, затем кэшируются)
- **10 языков** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — выбирается при первом запуске, меняется в **Настройках**
- Определение **уже установленных** приложений (зелёный значок на логотипе)
- Мгновенный поиск (`Ctrl+F`), фильтр по категориям, «выбрать все» для категории
- **Экспорт / импорт выбора** для использования на другом ПК
- Тихая последовательная установка с состоянием каждого приложения, индикатором прогресса, отменой и подробным журналом
- Современный тёмный интерфейс без зависимостей: один `.exe`

#### Требования

- Windows 10 (1809+) или Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (предустановлен в современных версиях, иначе «Установщик приложения» из Microsoft Store)

#### Добавить приложение

В `AppCrate.cs` добавьте строку в список `DATA`:

```
"Name|Identifier.winget|website.com"
```

Затем добавьте описание в каждый файл `lang/xx.txt`:

```
app.Identifier.winget=...
```

Идентификатор можно найти командой `winget search имя`. Сайт нужен только для получения логотипа.

#### Добавить язык

1. Скопируйте `lang/en.txt` в `lang/xx.txt` (`xx` — код языка, например `nl`, `pl`, `tr`)
2. Измените строку `name=` (отображаемое название на самом языке) и переведите значения
3. Снова запустите `build.bat`: язык появится в списке автоматически

> Недостающие тексты автоматически заменяются английскими.

#### Логотипы и товарные знаки

Логотипы **не** входят в этот репозиторий: они загружаются во время работы из общедоступных сервисов значков сайтов и сохраняются в `%LocalAppData%\AppCrate\icons`. Названия и логотипы принадлежат их владельцам. AppCrate не связан ни с одной из перечисленных программ.

#### Лицензия

[MIT](LICENSE)

</details>

<details>
<summary><b>中文</b></summary>

**勾选你需要的应用，然后点击“安装”。** AppCrate 是一个只有单个 `.exe` 的 Windows 小型安装工具：它展示常用应用的目录，你勾选想要的应用，它就通过 [winget](https://learn.microsoft.com/windows/package-manager/) 静默地全部安装。

非常适合几下点击就配置好新电脑或刚重装系统的电脑。

#### 功能

- 按分类整理的 75 个应用目录，每个应用都有**自己的说明**
- 应用**图标**（首次启动时下载，之后缓存）
- **10 种语言**（Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어）——首次启动时选择，可在**设置**中更改
- 检测**已安装**的应用（图标上显示绿色标记）
- 即时搜索（`Ctrl+F`）、分类筛选、按分类“全选”
- **导出 / 导入选择列表**，方便在另一台电脑上重复使用
- 依次静默安装，显示每个应用的状态和进度，可取消，并提供详细日志
- 现代深色界面，无需依赖：只有一个 `.exe`

#### 系统要求

- Windows 10（1809 及以上）或 Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/)（较新的系统已预装，否则请从 Microsoft Store 安装“应用安装程序”）

#### 添加应用

在 `AppCrate.cs` 的 `DATA` 列表中添加一行：

```
"Name|Identifier.winget|website.com"
```

然后在每个 `lang/xx.txt` 文件中添加它的说明：

```
app.Identifier.winget=...
```

可以用 `winget search 名称` 查找标识符。网站地址仅用于获取图标。

#### 添加语言

1. 将 `lang/en.txt` 复制为 `lang/xx.txt`（`xx` 为语言代码，例如 `nl`、`pl`、`tr`）
2. 修改 `name=` 一行（用该语言本身显示的名称）并翻译其中的内容
3. 重新运行 `build.bat`：该语言会自动出现在列表中

> 缺失的文本会自动回退为英文。

#### 图标与商标

本仓库**不**包含图标：它们在运行时从公共 favicon 服务获取，并保存在 `%LocalAppData%\AppCrate\icons`。名称和图标归其各自所有者所有。AppCrate 与所列软件均无关联。

#### 许可证

[MIT](LICENSE)

</details>

<details>
<summary><b>日本語</b></summary>

**インストールするアプリにチェックを入れて「インストール」をクリック。** AppCrate は、単一の `.exe` で動く小さな Windows 用インストーラーです。人気アプリのカタログから選ぶだけで、[winget](https://learn.microsoft.com/windows/package-manager/) を使ってすべてサイレントにインストールします。

新しい PC や再セットアップ直後の PC を、数クリックで準備するのに最適です。

#### 機能

- カテゴリ別に整理した 75 個のアプリ。それぞれに**専用の説明**付き
- アプリの**ロゴ**（初回起動時にダウンロードし、その後はキャッシュ）
- **10 言語**（Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어）— 初回起動時に選択し、**設定**から変更可能
- **導入済み**のアプリを検出（ロゴに緑のバッジ）
- 即時検索（`Ctrl+F`）、カテゴリ絞り込み、カテゴリごとの「すべて選択」
- 別の PC で再利用できる**選択内容のエクスポート／インポート**
- 順番にサイレントインストール。アプリごとの状態、進行状況、キャンセル、詳細ログに対応
- 依存関係のないモダンなダークUI：`.exe` ひとつだけ

#### 動作環境

- Windows 10（1809 以降）または Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/)（新しいバージョンでは標準搭載。なければ Microsoft Store の「アプリ インストーラー」）

#### アプリを追加する

`AppCrate.cs` の `DATA` リストに 1 行追加します：

```
"Name|Identifier.winget|website.com"
```

次に、各 `lang/xx.txt` ファイルに説明を追加します：

```
app.Identifier.winget=...
```

識別子は `winget search 名前` で調べられます。ウェブサイトはロゴの取得にのみ使われます。

#### 言語を追加する

1. `lang/en.txt` を `lang/xx.txt` にコピー（`xx` は言語コード。例：`nl`、`pl`、`tr`）
2. `name=` の行（その言語自身での表示名）を変更し、値を翻訳
3. `build.bat` をもう一度実行：言語が自動的に一覧に表示されます

> 不足しているテキストは自動的に英語で表示されます。

#### ロゴと商標

ロゴはこのリポジトリには**含まれていません**。実行時に公開のファビコンサービスから取得し、`%LocalAppData%\AppCrate\icons` に保存します。名称とロゴはそれぞれの権利者に帰属します。AppCrate は掲載されているいずれのソフトウェアとも提携していません。

#### ライセンス

[MIT](LICENSE)

</details>

<details>
<summary><b>한국어</b></summary>

**설치할 앱을 선택하고 '설치'를 클릭하세요.** AppCrate는 `.exe` 하나로 동작하는 작은 Windows 설치 도우미입니다. 인기 앱 카탈로그에서 원하는 앱을 체크하면 [winget](https://learn.microsoft.com/windows/package-manager/)으로 모두 조용히 설치합니다.

새 PC나 방금 초기화한 PC를 몇 번의 클릭으로 준비하기에 좋습니다.

#### 기능

- 카테고리별로 정리된 앱 75개, 각각 **고유한 설명** 포함
- 앱 **로고** (첫 실행 시 다운로드 후 캐시)
- **10개 언어** (Français, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어) — 첫 실행 시 선택하고 **설정**에서 변경 가능
- **이미 설치된** 앱 감지 (로고에 녹색 배지)
- 즉시 검색 (`Ctrl+F`), 카테고리 필터, 카테고리별 '모두 선택'
- 다른 PC에서 재사용할 수 있는 **선택 내보내기 / 가져오기**
- 앱별 상태, 진행률, 취소, 자세한 로그와 함께 순서대로 조용히 설치
- 의존성 없는 현대적인 다크 인터페이스: `.exe` 하나

#### 요구 사항

- Windows 10 (1809 이상) 또는 Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (최신 버전에는 기본 설치됨, 없으면 Microsoft Store의 '앱 설치 관리자')

#### 앱 추가하기

`AppCrate.cs`의 `DATA` 목록에 한 줄을 추가하세요:

```
"Name|Identifier.winget|website.com"
```

그런 다음 각 `lang/xx.txt` 파일에 설명을 추가하세요:

```
app.Identifier.winget=...
```

식별자는 `winget search 이름`으로 찾을 수 있습니다. 웹사이트는 로고를 가져오는 데만 사용됩니다.

#### 언어 추가하기

1. `lang/en.txt`를 `lang/xx.txt`로 복사 (`xx` = 언어 코드, 예: `nl`, `pl`, `tr`)
2. `name=` 줄(해당 언어로 표시되는 이름)을 바꾸고 값을 번역
3. `build.bat`을 다시 실행: 언어가 목록에 자동으로 나타납니다

> 누락된 문구는 자동으로 영어로 대체됩니다.

#### 로고와 상표

로고는 이 저장소에 **포함되어 있지 않습니다**. 실행 중에 공개 파비콘 서비스에서 가져와 `%LocalAppData%\AppCrate\icons`에 저장합니다. 이름과 로고는 각 소유자에게 속합니다. AppCrate는 나열된 어떤 소프트웨어와도 제휴 관계가 없습니다.

#### 라이선스

[MIT](LICENSE)

</details>

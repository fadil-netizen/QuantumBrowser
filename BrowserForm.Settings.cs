using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuantumBrowser
{
    public partial class BrowserForm
    {
        // Data
        private Dictionary<string, string> searchEngines = new Dictionary<string, string>();
        private Dictionary<string, string> appConfig = new Dictionary<string, string>();
        
        private string configPath = Path.Combine(Application.StartupPath, "browser_config.ini");
        private string enginesPath = Path.Combine(Application.StartupPath, "search_engines.txt");

        // Defaults
        private string currentEngineName = "Google";
        private string homePageUrl = "quantum://home";

        private void InitializeSettings()
        {
            LoadSearchEngines();
            LoadConfiguration();
            InitializeTranslations();
        }

        // Translation System
        private Dictionary<string, Dictionary<string, string>> translations;
        
        // Helper delegate for adding languages
        private Action<string, Dictionary<string, string>> AddLang;

        private void InitializeTranslations()
        {
            translations = new Dictionary<string, Dictionary<string, string>>();
            
            // Initialize the helper action
            AddLang = (code, dict) => {
                translations[code] = dict;
                if(code.Contains("-")) translations[code.Split('-')[0]] = dict;
            };

            LoadIndonesian();
            LoadSpanish();
            LoadFrench();
            LoadGerman();
            LoadJapanese();
            LoadChinese();
            LoadRussian();
            LoadPortuguese();
        }

        private void LoadIndonesian()
        {
            var id = new Dictionary<string, string>();
            id["New tab"] = "Tab baru";
            id["New window"] = "Jendela baru";
            id["New Incognito window"] = "Jendela Penyamaran baru";
            id["Passwords and autofill"] = "Sandi dan isi otomatis";
            id["History"] = "Histori";
            id["Downloads"] = "Download";
            id["Bookmarks"] = "Bookmark";
            id["Tab groups"] = "Grup tab";
            id["Extensions"] = "Ekstensi";
            id["Delete browsing data..."] = "Hapus data penjelajahan...";
            id["Browsing data cleared."] = "Data penjelajahan dihapus.";
            id["Zoom"] = "Zoom";
            id["Print..."] = "Cetak...";
            id["Translate page..."] = "Terjemahkan halaman...";
            id["Search with Google Lens"] = "Telusuri gambar dengan Google Lens";
            id["Find and edit"] = "Cari dan edit";
            id["Cast, save, and share"] = "Transimisikan, simpan, dan bagikan";
            id["More tools"] = "Fitur lainnya";
            id["Developer tools"] = "Alat pengembang";
            id["Network Log"] = "Log Jaringan";
            id["Help"] = "Bantuan";
            id["Settings"] = "Setelan";
            id["Exit"] = "Keluar";
            id["General"] = "Umum";
            id["On startup"] = "Saat memulai";
            id["Appearance"] = "Tampilan";
            id["Browser Language"] = "Bahasa Browser";
            id["Select the language you want to use:"] = "Pilih bahasa yang ingin Anda gunakan:";
            id["Please restart Quantum Browser for language changes to act effect."] = "Silakan restart Quantum Browser agar perubahan bahasa diterapkan.";
            id["Search Engine"] = "Mesin Penelusuran";
            id["Privacy"] = "Privasi";
            id["About"] = "Tentang";
            id["Open the New Tab page"] = "Buka halaman Tab Baru";
            id["Continue where you left off"] = "Lanjutkan dari sesi terakhir";
            id["Open a specific page or set of pages"] = "Buka halaman tertentu";
            id["Enter URLs (comma separated):"] = "Masukkan URL (dipisahkan koma):";
            id["Theme"] = "Tema";
            id["Dark (Default)"] = "Gelap (Default)";
            id["Light"] = "Terang";
            id["Custom (Material You)"] = "Kustom (Material You)";
            id["Browser Color (Material You)"] = "Warna Browser (Material You)";
            id["Pick Color..."] = "Pilih Warna...";
            id["Home Page Wallpaper"] = "Wallpaper Beranda";
            id["Browse..."] = "Cari...";
            id["Clear"] = "Hapus";
            id["Default Search Engine"] = "Mesin Penelusuran Default";
            id["Add Custom Engine"] = "Tambah Mesin Kustom";
            id["Name:"] = "Nama:";
            id["Homepage"] = "Beranda";
            id["Save"] = "Simpan";
            id["Saved!"] = "Disimpan!";
            id["Enhanced Tracking Protection"] = "Perlindungan Pelacakan";
            id["Standard (Balanced)"] = "Standar (Seimbang)";
            id["Strict (Stronger protection, may break sites)"] = "Ketat (Perlindungan lebih kuat, mungkin merusak situs)";
            id["Custom"] = "Kustom";
            id["Private DNS"] = "DNS Pribadi";
            id["Secure DNS Provider:"] = "Penyedia DNS Aman:";
            id["Restart browser to apply DNS changes."] = "Restart browser untuk menerapkan perubahan DNS.";
            id["Permissions & Security"] = "Izin & Keamanan";
            id["Enable HTTPS-Only Mode"] = "Aktifkan Mode HTTPS-Only";
            id["Send 'Do Not Track' request"] = "Kirim permintaan 'Do Not Track'";
            id["Ask to save passwords"] = "Tanya untuk menyimpan sandi";
            id["Clear Browsing Data..."] = "Hapus Data Penjelajahan...";
            id["Off"] = "Mati";
            id["Automatic"] = "Otomatis";
            id["Private DNS"] = "DNS Pribadi";
            id["Google DNS"] = "Google DNS";
            id["Cloudflare"] = "Cloudflare";
            id["NextDNS"] = "NextDNS";
            id["(Recommended)"] = "(Disarankan)";
            id["(Bypass blur/blocks)"] = "(Buka blokir/konten blur)";
            
            AddLang("id", id);
            AddLang("id-ID", id);
        }

        private void LoadSpanish()
        {
            var es = new Dictionary<string, string>();
            es["New tab"] = "Nueva pestaña"; es["New window"] = "Nueva ventana"; es["New Incognito window"] = "Nueva ventana de incógnito";
            es["Passwords and autofill"] = "Contraseñas y autocompletar"; es["History"] = "Historial"; es["Downloads"] = "Descargas";
            es["Bookmarks"] = "Marcadores"; es["Tab groups"] = "Grupos de pestañas"; es["Extensions"] = "Extensiones";
            es["Delete browsing data..."] = "Borrar datos de navegación..."; es["Browsing data cleared."] = "Datos de navegación borrados.";
            es["Zoom"] = "Zoom"; es["Print..."] = "Imprimir..."; es["Translate page..."] = "Traducir página...";
            es["Search with Google Lens"] = "Buscar con Google Lens"; es["Find and edit"] = "Buscar y editar";
            es["Cast, save, and share"] = "Enviar, guardar y compartir"; es["More tools"] = "Más herramientas";
            es["Developer tools"] = "Herramientas para desarrolladores"; es["Network Log"] = "Registro de red";
            es["Help"] = "Ayuda"; es["Settings"] = "Configuración"; es["Exit"] = "Salir";
            es["General"] = "General"; es["On startup"] = "Al iniciar"; es["Appearance"] = "Diseño";
            es["Browser Language"] = "Idioma del navegador"; es["Select the language you want to use:"] = "Selecciona el idioma que quieres usar:";
            es["Please restart Quantum Browser for language changes to act effect."] = "Reinicia Quantum Browser para aplicar el cambio de idioma.";
            es["Search Engine"] = "Buscador"; es["Privacy"] = "Privacidad"; es["About"] = "Información";
            es["Open the New Tab page"] = "Abrir la página Nueva pestaña"; es["Continue where you left off"] = "Abrir todo como estaba antes de cerrar";
            es["Open a specific page or set of pages"] = "Abrir una página específica o un conjunto de páginas";
            es["Enter URLs (comma separated):"] = "Introducir URL (separadas por comas):";
            es["Theme"] = "Tema"; es["Dark (Default)"] = "Oscuro (Predeterminado)"; es["Light"] = "Claro"; es["Custom (Material You)"] = "Personalizado (Material You)";
            es["Browser Color (Material You)"] = "Color del navegador"; es["Pick Color..."] = "Elegir color...";
            es["Home Page Wallpaper"] = "Fondo de página de inicio"; es["Browse..."] = "Examinar..."; es["Clear"] = "Borrar";
            es["Default Search Engine"] = "Buscador predeterminado"; es["Add Custom Engine"] = "Añadir buscador"; es["Name:"] = "Nombre:";
            es["Homepage"] = "Página principal"; es["Save"] = "Guardar"; es["Saved!"] = "¡Guardado!";
            es["Enhanced Tracking Protection"] = "Protección contra rastreo"; es["Standard (Balanced)"] = "Estándar (Equilibrado)";
            es["Strict (Stronger protection, may break sites)"] = "Estricta (Mayor protección, puede romper sitios)"; es["Custom"] = "Personalizada";
            es["DNS over HTTPS"] = "DNS seguro"; es["Secure DNS Provider:"] = "Proveedor de DNS:";
            es["Restart browser to apply DNS changes."] = "Reinicia el navegador para aplicar cambios de DNS.";
            es["Permissions & Security"] = "Permisos y seguridad"; es["Enable HTTPS-Only Mode"] = "Solo HTTPS";
            es["Send 'Do Not Track' request"] = "Enviar solicitud 'No rastrear'"; es["Ask to save passwords"] = "Preguntar si guardar contraseñas";
            es["Clear Browsing Data..."] = "Borrar datos..."; es["Off (Use System DNS)"] = "Desactivado (DNS del sistema)";
            
            AddLang("es", es);
        }

        private void LoadFrench()
        {
            var fr = new Dictionary<string, string>();
            fr["New tab"] = "Nouvel onglet"; fr["New window"] = "Nouvelle fenêtre"; fr["New Incognito window"] = "Nouvelle fenêtre de navigation privée";
            fr["Passwords and autofill"] = "Mots de passe et saisie automatique"; fr["History"] = "Historique"; fr["Downloads"] = "Téléchargements";
            fr["Bookmarks"] = "Favoris"; fr["Tab groups"] = "Groupes d'onglets"; fr["Extensions"] = "Extensions";
            fr["Delete browsing data..."] = "Effacer les données de navigation..."; fr["Browsing data cleared."] = "Données effacées.";
            fr["Zoom"] = "Zoom"; fr["Print..."] = "Imprimer..."; fr["Translate page..."] = "Traduire la page...";
            fr["Search with Google Lens"] = "Rechercher avec Google Lens"; fr["Find and edit"] = "Rechercher et modifier";
            fr["Cast, save, and share"] = "Caster, enregistrer et partager"; fr["More tools"] = "Plus d'outils";
            fr["Developer tools"] = "Outils de développement"; fr["Network Log"] = "Journal réseau";
            fr["Help"] = "Aide"; fr["Settings"] = "Paramètres"; fr["Exit"] = "Quitter";
            fr["General"] = "Général"; fr["On startup"] = "Au démarrage"; fr["Appearance"] = "Apparence";
            fr["Browser Language"] = "Langue du navigateur"; fr["Select the language you want to use:"] = "Sélectionnez la langue à utiliser :";
            fr["Please restart Quantum Browser for language changes to act effect."] = "Veuillez redémarrer le navigateur pour appliquer la langue.";
            fr["Search Engine"] = "Moteur de recherche"; fr["Privacy"] = "Confidentialité"; fr["About"] = "À propos";
            fr["Open the New Tab page"] = "Ouvrir la page Nouvel onglet"; fr["Continue where you left off"] = "Reprendre mes activités là où je m'étais arrêté";
            fr["Open a specific page or set of pages"] = "Ouvrir une page ou un ensemble de pages spécifiques";
            fr["Enter URLs (comma separated):"] = "Entrer les URL (séparées par virgule) :";
            fr["Theme"] = "Thème"; fr["Dark (Default)"] = "Sombre (Défaut)"; fr["Light"] = "Clair"; fr["Custom (Material You)"] = "Personnalisé (Material You)";
            fr["Browser Color (Material You)"] = "Couleur du navigateur"; fr["Pick Color..."] = "Choisir...";
            fr["Home Page Wallpaper"] = "Fond d'écran"; fr["Browse..."] = "Parcourir..."; fr["Clear"] = "Effacer";
            fr["Default Search Engine"] = "Moteur par défaut"; fr["Add Custom Engine"] = "Ajouter moteur"; fr["Name:"] = "Nom :";
            fr["Homepage"] = "Page d'accueil"; fr["Save"] = "Enregistrer"; fr["Saved!"] = "Enregistré !";
            fr["Enhanced Tracking Protection"] = "Protection contre le pistage"; fr["Standard (Balanced)"] = "Standard (Équilibré)";
            fr["Strict (Stronger protection, may break sites)"] = "Stricte (Forte protection, peut casser des sites)"; fr["Custom"] = "Personnalisée";
            fr["DNS over HTTPS"] = "DNS sécurisé"; fr["Secure DNS Provider:"] = "Fournisseur DNS :";
            fr["Restart browser to apply DNS changes."] = "Redémarrez pour appliquer le DNS.";
            fr["Permissions & Security"] = "Autorisations et sécurité"; fr["Enable HTTPS-Only Mode"] = "Mode HTTPS uniquement";
            fr["Send 'Do Not Track' request"] = "Envoyer une demande 'Interdire le suivi'"; fr["Ask to save passwords"] = "Proposer d'enregistrer les mots de passe";
            fr["Clear Browsing Data..."] = "Effacer les données..."; fr["Off (Use System DNS)"] = "Désactivé (DNS système)";
            
            AddLang("fr", fr);
        }

        private void LoadGerman()
        {
            var de = new Dictionary<string, string>();
            de["New tab"] = "Neuer Tab"; de["New window"] = "Neues Fenster"; de["New Incognito window"] = "Neues Inkognito-Fenster";
            de["Passwords and autofill"] = "Passwörter und Ausfüllen"; de["History"] = "Verlauf"; de["Downloads"] = "Downloads";
            de["Bookmarks"] = "Lesezeichen"; de["Tab groups"] = "Tab-Gruppen"; de["Extensions"] = "Erweiterungen";
            de["Delete browsing data..."] = "Browserdaten löschen..."; de["Browsing data cleared."] = "Browserdaten gelöscht.";
            de["Zoom"] = "Zoom"; de["Print..."] = "Drucken..."; de["Translate page..."] = "Seite übersetzen...";
            de["Search with Google Lens"] = "Mit Google Lens suchen"; de["Find and edit"] = "Suchen und bearbeiten";
            de["Cast, save, and share"] = "Streamen, speichern und teilen"; de["More tools"] = "Weitere Tools";
            de["Developer tools"] = "Entwicklertools"; de["Network Log"] = "Netzwerkprotokoll";
            de["Help"] = "Hilfe"; de["Settings"] = "Einstellungen"; de["Exit"] = "Beenden";
            de["General"] = "Allgemein"; de["On startup"] = "Beim Start"; de["Appearance"] = "Darstellung";
            de["Browser Language"] = "Browsersprache"; de["Select the language you want to use:"] = "Wählen Sie die Sprache aus:";
            de["Please restart Quantum Browser for language changes to act effect."] = "Bitte starten Sie den Browser neu, um die Sprache zu ändern.";
            de["Search Engine"] = "Suchmaschine"; de["Privacy"] = "Datenschutz"; de["About"] = "Über";
            de["Open the New Tab page"] = "Seite „Neuer Tab“ öffnen"; de["Continue where you left off"] = "Zuletzt angesehene Seiten öffnen";
            de["Open a specific page or set of pages"] = "Bestimmte Seite oder Seiten öffnen";
            de["Enter URLs (comma separated):"] = "URLs eingeben (kommagetrennt):";
            de["Theme"] = "Design"; de["Dark (Default)"] = "Dunkel (Standard)"; de["Light"] = "Hell"; de["Custom (Material You)"] = "Benutzerdefiniert";
            de["Browser Color (Material You)"] = "Browserfarbe"; de["Pick Color..."] = "Farbe wählen...";
            de["Home Page Wallpaper"] = "Hintergrundbild"; de["Browse..."] = "Durchsuchen..."; de["Clear"] = "Löschen";
            de["Default Search Engine"] = "Standardsuchmaschine"; de["Add Custom Engine"] = "Hinzufügen"; de["Name:"] = "Name:";
            de["Homepage"] = "Startseite"; de["Save"] = "Speichern"; de["Saved!"] = "Gespeichert!";
            de["Enhanced Tracking Protection"] = "Verbesserter Schutz"; de["Standard (Balanced)"] = "Standard (Ausgewogen)";
            de["Strict (Stronger protection, may break sites)"] = "Streng (Stärkerer Schutz)"; de["Custom"] = "Benutzerdefiniert";
            de["DNS over HTTPS"] = "Sicheres DNS"; de["Secure DNS Provider:"] = "DNS-Anbieter:";
            de["Restart browser to apply DNS changes."] = "Browser neu starten für DNS-Änderungen.";
            de["Permissions & Security"] = "Berechtigungen & Sicherheit"; de["Enable HTTPS-Only Mode"] = "Nur-HTTPS-Modus";
            de["Send 'Do Not Track' request"] = "'Do Not Track' anfordern"; de["Ask to save passwords"] = "Passwörter speichern";
            de["Clear Browsing Data..."] = "Daten löschen..."; de["Off (Use System DNS)"] = "Aus (System-DNS)";
            
            AddLang("de", de);
        }

        private void LoadJapanese()
        {
            var ja = new Dictionary<string, string>();
            ja["New tab"] = "新しいタブ"; ja["New window"] = "新しいウィンドウ"; ja["New Incognito window"] = "新しいシークレット ウィンドウ";
            ja["Passwords and autofill"] = "パスワードと自動入力"; ja["History"] = "履歴"; ja["Downloads"] = "ダウンロード";
            ja["Bookmarks"] = "ブックマーク"; ja["Tab groups"] = "タブ グループ"; ja["Extensions"] = "拡張機能";
            ja["Delete browsing data..."] = "閲覧履歴データの削除..."; ja["Browsing data cleared."] = "データが削除されました。";
            ja["Zoom"] = "ズーム"; ja["Print..."] = "印刷..."; ja["Translate page..."] = "ページを翻訳...";
            ja["Search with Google Lens"] = "Google レンズで検索"; ja["Find and edit"] = "検索と編集";
            ja["Cast, save, and share"] = "保存と共有"; ja["More tools"] = "その他のツール";
            ja["Developer tools"] = "デベロッパー ツール"; ja["Network Log"] = "ネットワーク ログ";
            ja["Help"] = "ヘルプ"; ja["Settings"] = "設定"; ja["Exit"] = "終了";
            ja["General"] = "全般"; ja["On startup"] = "起動時"; ja["Appearance"] = "デザイン";
            ja["Browser Language"] = "言語"; ja["Select the language you want to use:"] = "使用する言語を選択:";
            ja["Please restart Quantum Browser for language changes to act effect."] = "変更を適用するには再起動してください。";
            ja["Search Engine"] = "検索エンジン"; ja["Privacy"] = "プライバシー"; ja["About"] = "Quantum Browser について";
            ja["Open the New Tab page"] = "新しいタブ ページを開く"; ja["Continue where you left off"] = "前回開いていたページを開く";
            ja["Open a specific page or set of pages"] = "特定のページまたはページ セットを開く";
            ja["Enter URLs (comma separated):"] = "URL を入力 (カンマ区切り):";
            ja["Theme"] = "テーマ"; ja["Dark (Default)"] = "ダーク (デフォルト)"; ja["Light"] = "ライト"; ja["Custom (Material You)"] = "カスタム";
            ja["Browser Color (Material You)"] = "ブラウザの色"; ja["Pick Color..."] = "色を選択...";
            ja["Home Page Wallpaper"] = "壁紙"; ja["Browse..."] = "参照..."; ja["Clear"] = "クリア";
            ja["Default Search Engine"] = "既定の検索エンジン"; ja["Add Custom Engine"] = "追加"; ja["Name:"] = "名前:";
            ja["Homepage"] = "ホームボタン"; ja["Save"] = "保存"; ja["Saved!"] = "保存しました！";
            ja["Enhanced Tracking Protection"] = "トラッキング防止"; ja["Standard (Balanced)"] = "標準 (バランス)";
            ja["Strict (Stronger protection, may break sites)"] = "厳重 (強力、サイト破損の可能性あり)"; ja["Custom"] = "カスタム";
            ja["DNS over HTTPS"] = "セキュア DNS"; ja["Secure DNS Provider:"] = "プロバイダ:";
            ja["Restart browser to apply DNS changes."] = "DNS 変更を適用するには再起動してください。";
            ja["Permissions & Security"] = "権限とセキュリティ"; ja["Enable HTTPS-Only Mode"] = "HTTPS 優先モード";
            ja["Send 'Do Not Track' request"] = "トラッキング拒否を送信"; ja["Ask to save passwords"] = "パスワードの保存を確認";
            ja["Clear Browsing Data..."] = "データを削除..."; ja["Off (Use System DNS)"] = "オフ (システム DNS)";
            
            AddLang("ja", ja);
        }

        private void LoadChinese()
        {
            var zh = new Dictionary<string, string>();
            zh["New tab"] = "新标签页"; zh["New window"] = "新窗口"; zh["New Incognito window"] = "无痕窗口";
            zh["Passwords and autofill"] = "密码和自动填充"; zh["History"] = "历史记录"; zh["Downloads"] = "下载内容";
            zh["Bookmarks"] = "书签"; zh["Tab groups"] = "标签页组"; zh["Extensions"] = "扩展程序";
            zh["Delete browsing data..."] = "清除浏览数据..."; zh["Browsing data cleared."] = "数据已清除。";
            zh["Zoom"] = "缩放"; zh["Print..."] = "打印..."; zh["Translate page..."] = "翻译页面...";
            zh["Search with Google Lens"] = "使用 Google 智能镜头搜索"; zh["Find and edit"] = "查找和编辑";
            zh["Cast, save, and share"] = "保存和分享"; zh["More tools"] = "更多工具";
            zh["Developer tools"] = "开发者工具"; zh["Network Log"] = "网络日志";
            zh["Help"] = "帮助"; zh["Settings"] = "设置"; zh["Exit"] = "退出";
            zh["General"] = "常规"; zh["On startup"] = "启动时"; zh["Appearance"] = "外观";
            zh["Browser Language"] = "浏览器语言"; zh["Select the language you want to use:"] = "选择您要使用的语言：";
            zh["Please restart Quantum Browser for language changes to act effect."] = "请重启浏览器以应用更改。";
            zh["Search Engine"] = "搜索引擎"; zh["Privacy"] = "隐私设置"; zh["About"] = "关于";
            zh["Open the New Tab page"] = "打开新标签页"; zh["Continue where you left off"] = "继续浏览上次打开的网页";
            zh["Open a specific page or set of pages"] = "打开特定网页或一组网页";
            zh["Enter URLs (comma separated):"] = "输入网址 (逗号分隔):";
            zh["Theme"] = "主题"; zh["Dark (Default)"] = "深色 (默认)"; zh["Light"] = "浅色"; zh["Custom (Material You)"] = "自定义";
            zh["Browser Color (Material You)"] = "浏览器颜色"; zh["Pick Color..."] = "选择颜色...";
            zh["Home Page Wallpaper"] = "主页壁纸"; zh["Browse..."] = "浏览..."; zh["Clear"] = "清除";
            zh["Default Search Engine"] = "默认搜索引擎"; zh["Add Custom Engine"] = "添加"; zh["Name:"] = "名称:";
            zh["Homepage"] = "主页按钮"; zh["Save"] = "保存"; zh["Saved!"] = "已保存！";
            zh["Enhanced Tracking Protection"] = "跟踪防护"; zh["Standard (Balanced)"] = "标准 (平衡)";
            zh["Strict (Stronger protection, may break sites)"] = "严格 (更强防护)"; zh["Custom"] = "自定义";
            zh["DNS over HTTPS"] = "安全 DNS"; zh["Secure DNS Provider:"] = "提供商:";
            zh["Restart browser to apply DNS changes."] = "重启以应用 DNS 更改。";
            zh["Permissions & Security"] = "权限与安全"; zh["Enable HTTPS-Only Mode"] = "仅 HTTPS 模式";
            zh["Send 'Do Not Track' request"] = "发送“不跟踪”请求"; zh["Ask to save passwords"] = "提示保存密码";
            zh["Clear Browsing Data..."] = "清除数据..."; zh["Off (Use System DNS)"] = "关闭 (使用系统 DNS)";
            
            AddLang("zh-CN", zh); 
            AddLang("zh", zh);
        }

        private void LoadRussian()
        {
            var ru = new Dictionary<string, string>();
            ru["New tab"] = "Новая вкладка"; ru["New window"] = "Новое окно"; ru["New Incognito window"] = "Новое окно в режиме инкогнито";
            ru["Passwords and autofill"] = "Пароли и автозаполнение"; ru["History"] = "История"; ru["Downloads"] = "Скачанные файлы";
            ru["Bookmarks"] = "Закладки"; ru["Tab groups"] = "Группы вкладок"; ru["Extensions"] = "Расширения";
            ru["Delete browsing data..."] = "Очистить историю..."; ru["Browsing data cleared."] = "Данные очищены.";
            ru["Zoom"] = "Масштаб"; ru["Print..."] = "Печать..."; ru["Translate page..."] = "Перевести страницу...";
            ru["Search with Google Lens"] = "Найти через Google Объектив"; ru["Find and edit"] = "Найти и изменить";
            ru["Cast, save, and share"] = "Сохранение и отправка"; ru["More tools"] = "Дополнительные инструменты";
            ru["Developer tools"] = "Инструменты разработчика"; ru["Network Log"] = "Журнал сети";
            ru["Help"] = "Справка"; ru["Settings"] = "Настройки"; ru["Exit"] = "Выход";
            ru["General"] = "Общие"; ru["On startup"] = "Запуск"; ru["Appearance"] = "Внешний вид";
            ru["Browser Language"] = "Язык браузера"; ru["Select the language you want to use:"] = "Выберите язык:";
            ru["Please restart Quantum Browser for language changes to act effect."] = "Перезапустите браузер для применения языка.";
            ru["Search Engine"] = "Поисковая система"; ru["Privacy"] = "Конфиденциальность"; ru["About"] = "О браузере";
            ru["Open the New Tab page"] = "Новая вкладка"; ru["Continue where you left off"] = "Ранее открытые вкладки";
            ru["Open a specific page or set of pages"] = "Заданные страницы";
            ru["Enter URLs (comma separated):"] = "URL-адреса (через запятую):";
            ru["Theme"] = "Тема"; ru["Dark (Default)"] = "Тёмная (По умолчанию)"; ru["Light"] = "Светлая"; ru["Custom (Material You)"] = "Пользовательская";
            ru["Browser Color (Material You)"] = "Цвет браузера"; ru["Pick Color..."] = "Выбрать...";
            ru["Home Page Wallpaper"] = "Обои"; ru["Browse..."] = "Обзор..."; ru["Clear"] = "Очистить";
            ru["Default Search Engine"] = "Поиск по умолчанию"; ru["Add Custom Engine"] = "Добавить"; ru["Name:"] = "Имя:";
            ru["Homepage"] = "Главная страница"; ru["Save"] = "Сохранить"; ru["Saved!"] = "Сохранено!";
            ru["Enhanced Tracking Protection"] = "Защита от отслеживания"; ru["Standard (Balanced)"] = "Стандартная";
            ru["Strict (Stronger protection, may break sites)"] = "Строгая"; ru["Custom"] = "Настраиваемая";
            ru["DNS over HTTPS"] = "Безопасный DNS"; ru["Secure DNS Provider:"] = "Провайдер:";
            ru["Restart browser to apply DNS changes."] = "Перезапустите для применения DNS.";
            ru["Permissions & Security"] = "Разрешения и безопасность"; ru["Enable HTTPS-Only Mode"] = "Только HTTPS";
            ru["Send 'Do Not Track' request"] = "Отправлять 'Do Not Track'"; ru["Ask to save passwords"] = "Предлагать сохранение паролей";
            ru["Clear Browsing Data..."] = "Очистить..."; ru["Off (Use System DNS)"] = "Выкл (Системный)";
            
            AddLang("ru", ru);
        }

        private void LoadPortuguese()
        {
            var pt = new Dictionary<string, string>();
            pt["New tab"] = "Nova guia"; pt["New window"] = "Nova janela"; pt["New Incognito window"] = "Nova janela anônima";
            pt["Passwords and autofill"] = "Senhas e preenchimento"; pt["History"] = "Histórico"; pt["Downloads"] = "Downloads";
            pt["Bookmarks"] = "Favoritos"; pt["Tab groups"] = "Grupos de guias"; pt["Extensions"] = "Extensões";
            pt["Delete browsing data..."] = "Remover dados de navegação..."; pt["Browsing data cleared."] = "Dados removidos.";
            pt["Zoom"] = "Zoom"; pt["Print..."] = "Imprimir..."; pt["Translate page..."] = "Traduzir página...";
            pt["Search with Google Lens"] = "Pesquisar com o Google Lens"; pt["Find and edit"] = "Localizar e editar";
            pt["Cast, save, and share"] = "Salvar e compartilhar"; pt["More tools"] = "Mais ferramentas";
            pt["Developer tools"] = "Ferramentas do desenvolvedor"; pt["Network Log"] = "Log de rede";
            pt["Help"] = "Ajuda"; pt["Settings"] = "Configurações"; pt["Exit"] = "Sair";
            pt["General"] = "Geral"; pt["On startup"] = "Inicialização"; pt["Appearance"] = "Aparência";
            pt["Browser Language"] = "Idioma"; pt["Select the language you want to use:"] = "Selecione o idioma:";
            pt["Please restart Quantum Browser for language changes to act effect."] = "Reinicie o navegador para aplicar o idioma.";
            pt["Search Engine"] = "Mecanismo de pesquisa"; pt["Privacy"] = "Privacidade"; pt["About"] = "Sobre";
            pt["Open the New Tab page"] = "Abrir a página Nova Guia"; pt["Continue where you left off"] = "Continuar de onde você parou";
            pt["Open a specific page or set of pages"] = "Abrir uma página específica ou um conjunto";
            pt["Enter URLs (comma separated):"] = "URLs (separados por vírgula):";
            pt["Theme"] = "Tema"; pt["Dark (Default)"] = "Escuro (Padrão)"; pt["Light"] = "Claro"; pt["Custom (Material You)"] = "Personalizado";
            pt["Browser Color (Material You)"] = "Cor do navegador"; pt["Pick Color..."] = "Escolher cor...";
            pt["Home Page Wallpaper"] = "Papel de parede"; pt["Browse..."] = "Procurar..."; pt["Clear"] = "Limpar";
            pt["Default Search Engine"] = "Pesquisa padrão"; pt["Add Custom Engine"] = "Adicionar"; pt["Name:"] = "Nome:";
            pt["Homepage"] = "Página inicial"; pt["Save"] = "Salvar"; pt["Saved!"] = "Salvo!";
            pt["Enhanced Tracking Protection"] = "Proteção contra rastreamento"; pt["Standard (Balanced)"] = "Padrão (Equilibrado)";
            pt["Strict (Stronger protection, may break sites)"] = "Rigoroso (Maior proteção)"; pt["Custom"] = "Personalizado";
            pt["DNS over HTTPS"] = "DNS seguro"; pt["Secure DNS Provider:"] = "Provedor DNS:";
            pt["Restart browser to apply DNS changes."] = "Reinicie para aplicar DNS.";
            pt["Permissions & Security"] = "Permissões e segurança"; pt["Enable HTTPS-Only Mode"] = "Modo somente HTTPS";
            pt["Send 'Do Not Track' request"] = "Enviar solicitação 'Não rastrear'"; pt["Ask to save passwords"] = "Salvar senhas";
            pt["Clear Browsing Data..."] = "Limpar dados..."; pt["Off (Use System DNS)"] = "Desativado (Sistema)";
            
            AddLang("pt-BR", pt);
        }

        private string T(string text)
        {
            if (appConfig.ContainsKey("Language") && translations.ContainsKey(appConfig["Language"]))
            {
                string code = appConfig["Language"];
                // Handle complex codes like "id-ID" -> "id" if needed, but we mapped "id" above.
                // Simple lookup
                if (translations[code].ContainsKey(text)) return translations[code][text];
            }
            return text; // Return English (original)
        }

        private void LoadSearchEngines()
        {
            searchEngines.Clear();
            if (File.Exists(enginesPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(enginesPath);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 2 && !searchEngines.ContainsKey(parts[0]))
                            searchEngines.Add(parts[0], parts[1]);
                    }
                }
                catch { }
            }

            if (searchEngines.Count == 0)
            {
                searchEngines["Google"] = "https://www.google.com/search?q=%s";
                searchEngines["Bing"] = "https://www.bing.com/search?q=%s";
                searchEngines["DuckDuckGo"] = "https://duckduckgo.com/?q=%s";
                searchEngines["Yahoo"] = "https://search.yahoo.com/search?p=%s";
                searchEngines["Wikipedia"] = "https://id.wikipedia.org/wiki/Special:Search?search=%s";
                searchEngines["Baidu"] = "https://www.baidu.com/s?wd=%s";
                searchEngines["Yandex"] = "https://yandex.com/search/?text=%s";
                searchEngines["Ahmia"] = "https://ahmia.fi/search/?q=%s";
                SaveSearchEngines();
            }
        }

        private void SaveSearchEngines()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (var kvp in searchEngines) lines.Add(kvp.Key + "|" + kvp.Value);
                File.WriteAllLines(enginesPath, lines.ToArray());
            }
            catch { }
        }

        private void LoadConfiguration()
        {
            appConfig.Clear();
            if (File.Exists(configPath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length >= 2) appConfig[parts[0].Trim()] = parts[1].Trim();
                    }
                }
                catch { }
            }

            // Apply Config
            if (appConfig.ContainsKey("CurrentEngine") && searchEngines.ContainsKey(appConfig["CurrentEngine"]))
                currentEngineName = appConfig["CurrentEngine"];
            
            if (appConfig.ContainsKey("HomePage"))
                homePageUrl = appConfig["HomePage"];
            
            if (homePageUrl == "http://app.assets/index.html") homePageUrl = "quantum://home";
        }

        private void SaveConfiguration()
        {
            appConfig["CurrentEngine"] = currentEngineName;
            appConfig["HomePage"] = homePageUrl;

            try
            {
                List<string> lines = new List<string>();
                foreach (var kvp in appConfig) lines.Add(kvp.Key + "=" + kvp.Value);
                File.WriteAllLines(configPath, lines.ToArray());
            }
            catch { }
        }

        // ShowMenu moved to BrowserForm.UI.cs to handle Incognito logic consolidation and native UI binding.

        // Helper classes removed as they are no longer used here.

        private void ShowSettings(object sender, EventArgs e)
        {
            Form settingsForm = new Form();
            settingsForm.Text = T("Settings");
            settingsForm.Size = new Size((int)(800 * scaleFactor), (int)(600 * scaleFactor));
            settingsForm.StartPosition = FormStartPosition.CenterParent;
            settingsForm.Font = new Font("Segoe UI", 10 * scaleFactor);
            settingsForm.BackColor = Color.White;

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
            split.Panel1.BackColor = Color.FromArgb(245, 245, 245);
            split.SplitterDistance = (int)(200 * scaleFactor);
            settingsForm.Controls.Add(split);

            // Left Menu
            ListBox menuList = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(245, 245, 245), Font = new Font("Segoe UI", 11 * scaleFactor), ItemHeight = (int)(40 * scaleFactor), DrawMode = DrawMode.OwnerDrawFixed };
            // Note: We use keys for RenderPageAction but display Translated names
            var menuItems = new List<string> { "General", "On startup", "Appearance", "Search Engine", "Languages", "Privacy", "About" };
            foreach(var m in menuItems) menuList.Items.Add(T(m));
            
            menuList.SelectedIndex = 1; // Default to Appearance
            split.Panel1.Controls.Add(menuList);
            
            // Custom Draw for ListBox (Padding)
            menuList.DrawItem += (s, ev) => {
                if (ev.Index < 0) return;
                bool selected = (ev.State & DrawItemState.Selected) == DrawItemState.Selected;
                ev.Graphics.FillRectangle(new SolidBrush(selected ? Color.FromArgb(220, 220, 220) : Color.FromArgb(245, 245, 245)), ev.Bounds);
                TextRenderer.DrawText(ev.Graphics, menuList.Items[ev.Index].ToString(), ev.Font, new Point(ev.Bounds.Left + 20, ev.Bounds.Top + 10), Color.Black);
            };

            // Content Panel
            Panel contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(30) };
            split.Panel2.Controls.Add(contentPanel);

            // Helper method for rendering pages
            System.Action<string> RenderPageAction = null;
            RenderPageAction = delegate(string pageNameTranslated) {
                // Map translated name back to internal key if needed, or just rely on logic
                // But simpler: we just iterate and match translated names for now or use index.
                // Better approach: Pass internal keys in RenderPageAction calls, but the listbox has translated names.
                // Let's reverse lookup the key or just use if/else with T()
                
                string pageKey = "";
                foreach(var m in menuItems) if(T(m) == pageNameTranslated) pageKey = m;
                if(string.IsNullOrEmpty(pageKey)) pageKey = pageNameTranslated; // Fallback

                contentPanel.Controls.Clear();
                Label title = new Label { Text = T(pageKey), Font = new Font("Segoe UI", 18 * scaleFactor, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Top };
                contentPanel.Controls.Add(title);
                
                // Spacer
                contentPanel.Controls.Add(new Panel { Height = 20, Dock = DockStyle.Top });

                if (pageKey == "On startup")
                {
                    GroupBox grpStart = new GroupBox { Text = T("On startup"), Dock = DockStyle.Top, Height = 200, Padding = new Padding(10) };
                    
                    RadioButton rbNewTab = new RadioButton { Text = T("Open the New Tab page"), Top = 30, Left = 20, AutoSize = true };
                    RadioButton rbContinue = new RadioButton { Text = T("Continue where you left off"), Top = 60, Left = 20, AutoSize = true };
                    RadioButton rbSpecific = new RadioButton { Text = T("Open a specific page or set of pages"), Top = 90, Left = 20, AutoSize = true };
                    
                    Panel pnlSpecific = new Panel { Top = 120, Left = 40, Height = 60, Width = 500, Visible = false };
                    Label lblUrl = new Label { Text = T("Enter URLs (comma separated):"), Top = 0, AutoSize = true };
                    TextBox txtUrl = new TextBox { Top = 25, Width = 400 };
                    pnlSpecific.Controls.AddRange(new Control[]{ lblUrl, txtUrl });

                    // Load Initial State
                    string behavior = appConfig.ContainsKey("StartupBehavior") ? appConfig["StartupBehavior"] : "NewTab";
                    if(behavior == "Continue") rbContinue.Checked = true;
                    else if(behavior == "SpecificPage") { rbSpecific.Checked = true; pnlSpecific.Visible = true; }
                    else rbNewTab.Checked = true;

                    if(appConfig.ContainsKey("StartupPages")) txtUrl.Text = appConfig["StartupPages"];

                    // Handlers
                    EventHandler checkHandler = (s, ev) => {
                        if(rbNewTab.Checked) { appConfig["StartupBehavior"] = "NewTab"; pnlSpecific.Visible = false; }
                        if(rbContinue.Checked) { appConfig["StartupBehavior"] = "Continue"; pnlSpecific.Visible = false; }
                        if(rbSpecific.Checked) { appConfig["StartupBehavior"] = "SpecificPage"; pnlSpecific.Visible = true; }
                        SaveConfiguration();
                    };
                    rbNewTab.CheckedChanged += checkHandler;
                    rbContinue.CheckedChanged += checkHandler;
                    rbSpecific.CheckedChanged += checkHandler;

                    txtUrl.TextChanged += delegate { appConfig["StartupPages"] = txtUrl.Text; SaveConfiguration(); };

                    grpStart.Controls.AddRange(new Control[] { rbNewTab, rbContinue, rbSpecific, pnlSpecific });
                    contentPanel.Controls.Add(grpStart);
                }
                else if (pageKey == "Languages")
                {
                    GroupBox grpLang = new GroupBox { Text = T("Browser Language"), Dock = DockStyle.Top, Height = 150, Padding = new Padding(10) };
                    grpLang.Controls.Add(new Label { Text = T("Select the language you want to use:"), Left = 20, Top = 30, AutoSize = true });
                    
                    Dictionary<string, string> langs = new Dictionary<string, string>();
                    langs["English (US)"] = "en-US";
                    langs["Indonesian (Indonesia)"] = "id";
                    langs["Spanish"] = "es";
                    langs["French"] = "fr";
                    langs["German"] = "de";
                    langs["Japanese"] = "ja";
                    langs["Chinese (Simplified)"] = "zh-CN";
                    langs["Russian"] = "ru";
                    langs["Portuguese (Brazil)"] = "pt-BR";

                    ComboBox cmbLang = new ComboBox { Left = 20, Top = 60, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
                    foreach(var k in langs.Keys) cmbLang.Items.Add(k);

                    // Load selection
                    string currentCode = appConfig.ContainsKey("Language") ? appConfig["Language"] : "en-US";
                    // Find key for value
                    string selection = "English (US)";
                    foreach(var kvp in langs) if(kvp.Value == currentCode) selection = kvp.Key;
                    cmbLang.SelectedItem = selection;

                    cmbLang.SelectedIndexChanged += delegate {
                         if(cmbLang.SelectedItem != null && langs.ContainsKey(cmbLang.SelectedItem.ToString())) {
                             appConfig["Language"] = langs[cmbLang.SelectedItem.ToString()];
                             SaveConfiguration();
                             MessageBox.Show(T("Please restart Quantum Browser for language changes to act effect."));
                         }
                    };

                    grpLang.Controls.Add(cmbLang);
                    contentPanel.Controls.Add(grpLang);
                }
                else if (pageKey == "Appearance")
                {
                    // --- Theme Selection ---
                    GroupBox grpTheme = new GroupBox { Text = T("Theme"), Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
                    RadioButton rbDark = new RadioButton { Text = T("Dark (Default)"), Top = 30, Left = 20, AutoSize = true, Checked = !appConfig.ContainsKey("Theme") || appConfig["Theme"] == "Dark" };
                    RadioButton rbLight = new RadioButton { Text = T("Light"), Top = 60, Left = 20, AutoSize = true, Checked = appConfig.ContainsKey("Theme") && appConfig["Theme"] == "Light" };
                    RadioButton rbCustom = new RadioButton { Text = T("Custom (Material You)"), Top = 30, Left = 150, AutoSize = true, Checked = appConfig.ContainsKey("Theme") && appConfig["Theme"] == "Custom" };

                    EventHandler themeHandler = (s, ev) => {
                        if (rbDark.Checked) appConfig["Theme"] = "Dark";
                        if (rbLight.Checked) appConfig["Theme"] = "Light";
                        if (rbCustom.Checked) appConfig["Theme"] = "Custom";
                        SaveConfiguration();
                        ApplyTheme();
                    };
                    rbDark.CheckedChanged += themeHandler;
                    rbLight.CheckedChanged += themeHandler;
                    rbCustom.CheckedChanged += themeHandler;

                    grpTheme.Controls.AddRange(new Control[] { rbDark, rbLight, rbCustom });
                    contentPanel.Controls.Add(grpTheme);
                    contentPanel.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Top });

                    // --- Custom Color (Material You) ---
                    GroupBox grpColor = new GroupBox { Text = T("Browser Color (Material You)"), Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
                    Button btnPick = new Button { Text = T("Pick Color..."), Top = 30, Left = 20, Width = 120 };
                    Panel pnlPreview = new Panel { Top = 34, Left = 150, Width = 20, Height = 20, BorderStyle = BorderStyle.FixedSingle };
                    
                    if (appConfig.ContainsKey("ThemeColor")) try { pnlPreview.BackColor = ColorTranslator.FromHtml(appConfig["ThemeColor"]); } catch {}
                    else pnlPreview.BackColor = Color.Gray;

                    btnPick.Enabled = rbCustom.Checked;
                    rbCustom.CheckedChanged += delegate { btnPick.Enabled = rbCustom.Checked; };

                    btnPick.Click += delegate {
                        ColorDialog cd = new ColorDialog();
                        if (cd.ShowDialog() == DialogResult.OK)
                        {
                            string hex = ColorTranslator.ToHtml(cd.Color);
                            appConfig["ThemeColor"] = hex;
                            pnlPreview.BackColor = cd.Color;
                            SaveConfiguration();
                            ApplyTheme();
                        }
                    };

                    grpColor.Controls.Add(btnPick);
                    grpColor.Controls.Add(pnlPreview);
                    contentPanel.Controls.Add(grpColor);
                    contentPanel.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Top });

                    // --- Wallpaper ---
                    GroupBox grpWall = new GroupBox { Text = T("Home Page Wallpaper"), Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
                    TextBox txtWall = new TextBox { Top = 30, Left = 20, Width = 300, ReadOnly = true };
                    if (appConfig.ContainsKey("WallpaperPath")) txtWall.Text = appConfig["WallpaperPath"];
                    
                    Button btnBrowse = new Button { Text = T("Browse..."), Top = 29, Left = 330 };
                    btnBrowse.Click += delegate {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter = "Image Files|*.jpg;*.png;*.jpeg;*.bmp";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            txtWall.Text = ofd.FileName;
                            appConfig["WallpaperPath"] = ofd.FileName;
                            SaveConfiguration();
                        }
                    };
                    
                    Button btnReset = new Button { Text = T("Clear"), Top = 29, Left = 410 };
                    btnReset.Click += delegate {
                        txtWall.Text = "";
                        if(appConfig.ContainsKey("WallpaperPath")) appConfig.Remove("WallpaperPath");
                        SaveConfiguration();
                    };

                    grpWall.Controls.AddRange(new Control[] { txtWall, btnBrowse, btnReset });
                    contentPanel.Controls.Add(grpWall);
                }
                else if (pageKey == "Search Engine")
                {
                    // Default Engine
                    GroupBox grpDefault = new GroupBox { Text = T("Default Search Engine"), Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
                    ComboBox cmb = new ComboBox { Left = 20, Top = 30, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
                    foreach (var k in searchEngines.Keys) cmb.Items.Add(k);
                    cmb.SelectedItem = currentEngineName;
                    cmb.SelectedIndexChanged += delegate { 
                        if (cmb.SelectedItem != null) { 
                            currentEngineName = cmb.SelectedItem.ToString(); 
                            SaveConfiguration(); 
                        } 
                    };
                    grpDefault.Controls.Add(cmb);
                    contentPanel.Controls.Add(grpDefault);

                    // Add Engine
                    GroupBox grpAdd = new GroupBox { Text = T("Add Custom Engine"), Dock = DockStyle.Top, Height = 200, Padding = new Padding(10) };
                    grpAdd.Controls.Add(new Label { Text = T("Name:"), Left = 20, Top = 30, AutoSize = true });
                    TextBox txtName = new TextBox { Left = 20, Top = 50, Width = 300 };
                    grpAdd.Controls.Add(txtName);
                    
                    grpAdd.Controls.Add(new Label { Text = "URL (%s for query):", Left = 20, Top = 90, AutoSize = true });
                    TextBox txtUrl = new TextBox { Left = 20, Top = 110, Width = 300 };
                    grpAdd.Controls.Add(txtUrl);
                    
                    Button btnAdd = new Button { Text = "Add", Left = 20, Top = 150, Width = 80 };
                    btnAdd.Click += delegate {
                        if (!string.IsNullOrEmpty(txtName.Text) && !string.IsNullOrEmpty(txtUrl.Text)) {
                            if (!txtUrl.Text.Contains("%s")) { MessageBox.Show("URL must contain %s"); return; }
                            if (!searchEngines.ContainsKey(txtName.Text)) {
                                searchEngines.Add(txtName.Text, txtUrl.Text);
                                SaveSearchEngines();
                                RenderPageAction(T("Search Engine")); // Refresh
                            }
                        }
                    };
                    grpAdd.Controls.Add(btnAdd);
                    
                    // Spacer
                    contentPanel.Controls.Add(new Panel { Height = 20, Dock = DockStyle.Top });
                    contentPanel.Controls.Add(grpAdd);
                }
                else if (pageKey == "General")
                {
                    GroupBox grpHome = new GroupBox { Text = T("Homepage"), Dock = DockStyle.Top, Height = 100, Padding = new Padding(10) };
                    TextBox txtHome = new TextBox { Left = 20, Top = 30, Width = 400, Text = homePageUrl };
                    Button btnSave = new Button { Text = T("Save"), Left = 430, Top = 29 };
                    btnSave.Click += delegate { homePageUrl = txtHome.Text; SaveConfiguration(); MessageBox.Show(T("Saved!")); };
                    
                    grpHome.Controls.Add(txtHome);
                    grpHome.Controls.Add(btnSave);
                    contentPanel.Controls.Add(grpHome);
                }
                else if (pageKey == "Privacy")
                {
                    contentPanel.AutoScroll = true;

                    // --- Enhanced Tracking Protection ---
                    GroupBox grpTracking = new GroupBox { Text = T("Enhanced Tracking Protection"), Dock = DockStyle.Top, Height = 130, Padding = new Padding(10) };
                    
                    RadioButton rbStandard = new RadioButton { Text = T("Standard (Balanced)"), Top = 30, Left = 20, AutoSize = true };
                    RadioButton rbStrict = new RadioButton { Text = T("Strict (Stronger protection, may break sites)"), Top = 60, Left = 20, AutoSize = true };
                    RadioButton rbCustom = new RadioButton { Text = T("Custom"), Top = 90, Left = 20, AutoSize = true };
                    
                    // Load Setting
                    string level = appConfig.ContainsKey("TrackingPrevention") ? appConfig["TrackingPrevention"] : "Standard";
                    if (level == "Strict") rbStrict.Checked = true;
                    else if (level == "Custom") rbCustom.Checked = true;
                    else rbStandard.Checked = true;

                    EventHandler trackingHandler = (s, ev) => {
                        if (rbStandard.Checked) appConfig["TrackingPrevention"] = "Standard";
                        if (rbStrict.Checked) appConfig["TrackingPrevention"] = "Strict";
                        if (rbCustom.Checked) appConfig["TrackingPrevention"] = "Custom";
                        SaveConfiguration();
                    };
                    rbStandard.CheckedChanged += trackingHandler;
                    rbStrict.CheckedChanged += trackingHandler;
                    rbCustom.CheckedChanged += trackingHandler;

                    grpTracking.Controls.AddRange(new Control[] { rbStandard, rbStrict, rbCustom });
                    contentPanel.Controls.Add(grpTracking);
                    contentPanel.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Top }); // Spacer

                    // --- Private DNS ---
                    GroupBox grpDns = new GroupBox { Text = T("Private DNS"), Dock = DockStyle.Top, Height = 150, Padding = new Padding(10) };
                    grpDns.Controls.Add(new Label { Text = T("Secure DNS Provider:"), Left = 20, Top = 30, AutoSize = true });
                    
                    ComboBox cmbDns = new ComboBox { Left = 20, Top = 50, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
                    
                    // Options: Automatic, Off, Google DNS, Cloudflare, NextDNS, Custom
                    cmbDns.Items.Add(T("Automatic") + " " + T("(Recommended)"));
                    cmbDns.Items.Add(T("Off"));
                    cmbDns.Items.Add("Google DNS");
                    cmbDns.Items.Add("Cloudflare");
                    cmbDns.Items.Add("NextDNS" + " " + T("(Bypass blur/blocks)"));
                    cmbDns.Items.Add(T("Custom"));
                    
                    // Load DNS Setting
                    string dnsProvider = appConfig.ContainsKey("DnsProvider") ? appConfig["DnsProvider"] : "Automatic";
                    
                    // Map value to index
                    if (dnsProvider == "Automatic") cmbDns.SelectedIndex = 0;
                    else if (dnsProvider == "Off") cmbDns.SelectedIndex = 1;
                    else if (dnsProvider == "Google DNS") cmbDns.SelectedIndex = 2;
                    else if (dnsProvider == "Cloudflare") cmbDns.SelectedIndex = 3;
                    else if (dnsProvider.StartsWith("NextDNS")) cmbDns.SelectedIndex = 4;
                    else if (dnsProvider == "Custom") cmbDns.SelectedIndex = 5;
                    else cmbDns.SelectedIndex = 0; // Default

                    TextBox txtCustomDns = new TextBox { Left = 20, Top = 90, Width = 400, Visible = (dnsProvider == "Custom") };
                    if(appConfig.ContainsKey("DnsCustomUrl")) txtCustomDns.Text = appConfig["DnsCustomUrl"];

                    cmbDns.SelectedIndexChanged += delegate {
                        int idx = cmbDns.SelectedIndex;
                        string val = "Automatic";
                        if (idx == 1) val = "Off";
                        else if (idx == 2) val = "Google DNS";
                        else if (idx == 3) val = "Cloudflare";
                        else if (idx == 4) val = "NextDNS";
                        else if (idx == 5) val = "Custom";
                        
                        appConfig["DnsProvider"] = val; 
                        txtCustomDns.Visible = (val == "Custom");
                        SaveConfiguration();
                        MessageBox.Show(T("Restart browser to apply DNS changes."));
                    };

                    txtCustomDns.TextChanged += delegate {
                        appConfig["DnsCustomUrl"] = txtCustomDns.Text;
                        SaveConfiguration();
                    };

                    grpDns.Controls.Add(cmbDns);
                    grpDns.Controls.Add(txtCustomDns);
                    contentPanel.Controls.Add(grpDns);
                    contentPanel.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Top }); // Spacer

                    // --- General Privacy ---
                    GroupBox grpGen = new GroupBox { Text = T("Permissions & Security"), Dock = DockStyle.Top, Height = 180, Padding = new Padding(10) };
                    
                    CheckBox chkHttps = new CheckBox { Text = T("Enable HTTPS-Only Mode"), Top = 30, Left = 20, AutoSize = true, Checked = appConfig.ContainsKey("HttpsOnly") && appConfig["HttpsOnly"] == "true" };
                    chkHttps.CheckedChanged += delegate { appConfig["HttpsOnly"] = chkHttps.Checked ? "true" : "false"; SaveConfiguration(); };
                    
                    CheckBox chkDoNotTrack = new CheckBox { Text = T("Send 'Do Not Track' request"), Top = 60, Left = 20, AutoSize = true, Checked = appConfig.ContainsKey("DoNotTrack") && appConfig["DoNotTrack"] == "true" };
                    chkDoNotTrack.CheckedChanged += delegate { appConfig["DoNotTrack"] = chkDoNotTrack.Checked ? "true" : "false"; SaveConfiguration(); };
                    
                    CheckBox chkPasswords = new CheckBox { Text = T("Ask to save passwords"), Top = 90, Left = 20, AutoSize = true, Checked = !appConfig.ContainsKey("SavePasswords") || appConfig["SavePasswords"] == "true" };
                    chkPasswords.CheckedChanged += delegate { appConfig["SavePasswords"] = chkPasswords.Checked ? "true" : "false"; SaveConfiguration(); };

                    Button btnClear = new Button { Text = T("Clear Browsing Data..."), Top = 130, Left = 20, Width = 150 };
                    btnClear.Click += delegate {
                        if (webView != null && webView.CoreWebView2 != null) {
                            webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                            MessageBox.Show(T("Browsing data cleared."));
                        }
                    };

                    grpGen.Controls.AddRange(new Control[] { chkHttps, chkDoNotTrack, chkPasswords, btnClear });
                    contentPanel.Controls.Add(grpGen);

                }
                else if (pageKey == "About")
                {
                    contentPanel.Controls.Add(new Label { Text = "Quantum Browser v1.0", AutoSize = true, Top = 20 });
                    contentPanel.Controls.Add(new Label { Text = "Powered by WebView2 & Antigravity AI", AutoSize = true, Top = 50 });
                }
                
                // Fix Docking Order (Reverse)
                List<Control> reverse = new List<Control>();
                foreach(Control c in contentPanel.Controls) reverse.Add(c);
                reverse.Reverse();
                contentPanel.Controls.Clear();
                contentPanel.Controls.AddRange(reverse.ToArray());
            };
            
            menuList.SelectedIndexChanged += delegate {
                if (menuList.SelectedItem != null) RenderPageAction(menuList.SelectedItem.ToString());
            };

            // Initial Render
            RenderPageAction(T("Search Engine"));

            settingsForm.ShowDialog(this);
        }


        private void TranslateCurrentPage()
        {
            if (activeTab != null && !string.IsNullOrEmpty(activeTab.CurrentUrl))
            {
                string url = activeTab.CurrentUrl;
                // Skip internal pages
                if (url.StartsWith("quantum://") || url.StartsWith("chrome://") || url.StartsWith("about:") || url.Contains("translate.google.com")) return;

                string targetUrl = string.Format("https://translate.google.com/translate?sl=auto&tl=id&u={0}", 
                    System.Uri.EscapeDataString(url));
                Navigate(targetUrl);
            }
        }
    }
}

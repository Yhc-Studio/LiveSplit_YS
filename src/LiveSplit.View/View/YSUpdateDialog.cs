using LiveSplit.Localization;
using LiveSplit.Updates;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.View;

/// <summary>
/// Stage 4.10.0.1 preview UI for the independent LiveSplit YS update channel.
/// It can query the real manifest, but download/install remains intentionally disabled.
/// Release notes are paged so long changelogs do not force the dialog to become unwieldy.
/// </summary>
internal sealed class YSUpdateDialog : Form
{
    private const int NotesPerPage = 6;

    private readonly AppLanguage language;
    private readonly Label lblPreview;
    private readonly Label lblStatus;
    private readonly Label lblCurrentVersion;
    private readonly Label lblLatestVersion;
    private readonly Label lblChannel;
    private readonly Label lblManifest;
    private readonly RichTextBox txtNotes;
    private readonly ProgressBar progressBar;
    private readonly ComboBox cboPreviewState;
    private readonly Button btnCheck;
    private readonly Button btnInstall;
    private readonly Button btnClose;
    private readonly CheckBox chkAutoCheck;
    private readonly Button btnPreviousPage;
    private readonly Button btnNextPage;
    private readonly Label lblNotesPage;

    private readonly List<string[]> notePages = new();
    private int currentNotePage;
    private bool notesUseBullets = true;

    private string T(string source) => UiLocalizer.Translate(source, language);

    public YSUpdateDialog()
    {
        language = LanguageResolver.ResolveCurrentCultureLanguage();

        Text = T("LiveSplit YS Update");
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(720, 620);
        MinimumSize = new Size(680, 570);
        Font = SystemFonts.MessageBoxFont;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(16, 12, 16, 12),
            BackColor = SystemColors.Control
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132f));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill };
        var lblProduct = new Label
        {
            AutoSize = true,
            Text = ForkInfo.ProductName,
            Font = new Font(Font.FontFamily, 18f, FontStyle.Bold),
            Location = new Point(6, 5)
        };
        header.Controls.Add(lblProduct);

        lblPreview = new Label
        {
            AutoSize = true,
            Text = T("Stage 4.10.0.1 UI preview — no program files will be replaced."),
            Location = new Point(8, 48)
        };
        header.Controls.Add(lblPreview);
        root.Controls.Add(header, 0, 0);

        var versionBox = new GroupBox
        {
            Text = T("Version information"),
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 2, 8)
        };
        root.Controls.Add(versionBox, 0, 1);

        var versionGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(10, 8, 10, 8)
        };
        versionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
        versionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        versionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
        versionGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        versionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        versionGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        versionGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        versionBox.Controls.Add(versionGrid);

        versionGrid.Controls.Add(CreateCaption(T("Current version:")), 0, 0);
        lblCurrentVersion = CreateValue($"v{ForkInfo.Version}");
        versionGrid.Controls.Add(lblCurrentVersion, 1, 0);
        versionGrid.Controls.Add(CreateCaption(T("Update channel:")), 2, 0);
        lblChannel = CreateValue(T("Stable"));
        versionGrid.Controls.Add(lblChannel, 3, 0);

        versionGrid.Controls.Add(CreateCaption(T("Latest version:")), 0, 1);
        lblLatestVersion = CreateValue("v1.0.6");
        versionGrid.Controls.Add(lblLatestVersion, 1, 1);
        versionGrid.Controls.Add(CreateCaption(T("Manifest:")), 2, 1);
        lblManifest = CreateValue(T("Preview data"));
        versionGrid.Controls.Add(lblManifest, 3, 1);

        lblStatus = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = T("A new LiveSplit YS version is available."),
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(0, 3, 0, 0)
        };
        versionGrid.SetColumnSpan(lblStatus, 4);
        versionGrid.Controls.Add(lblStatus, 0, 2);

        var notesBox = new GroupBox
        {
            Text = T("Release notes"),
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 2, 8)
        };
        root.Controls.Add(notesBox, 0, 2);

        var notesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10, 8, 10, 8)
        };
        notesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        notesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        notesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        notesBox.Controls.Add(notesLayout);

        txtNotes = new RichTextBox
        {
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = SystemColors.Window,
            Dock = DockStyle.Fill,
            TabStop = false,
            DetectUrls = true,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
        notesLayout.Controls.Add(txtNotes, 0, 0);

        var pager = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Margin = Padding.Empty
        };
        pager.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
        pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
        pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92f));
        pager.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        notesLayout.Controls.Add(pager, 0, 1);

        btnPreviousPage = new Button
        {
            Text = T("Previous page"),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 4, 0)
        };
        btnPreviousPage.Click += (_, _) => ChangeNotesPage(-1);
        pager.Controls.Add(btnPreviousPage, 1, 0);

        lblNotesPage = new Label
        {
            Text = string.Format(T("Page {0} / {1}"), 1, 1),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 4, 0, 0)
        };
        pager.Controls.Add(lblNotesPage, 2, 0);

        btnNextPage = new Button
        {
            Text = T("Next page"),
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 4, 0, 0)
        };
        btnNextPage.Click += (_, _) => ChangeNotesPage(1);
        pager.Controls.Add(btnNextPage, 3, 0);

        progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 100,
            Visible = false,
            Margin = new Padding(2, 3, 2, 4)
        };
        root.Controls.Add(progressBar, 0, 3);

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Margin = new Padding(2, 0, 2, 0)
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96f));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 104f));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138f));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4f));
        root.Controls.Add(actions, 0, 4);

        var lblPreviewState = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Text = T("Preview state:"),
            TextAlign = ContentAlignment.MiddleLeft
        };
        actions.Controls.Add(lblPreviewState, 0, 0);

        cboPreviewState = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 7, 12, 6)
        };
        cboPreviewState.Items.AddRange(new object[]
        {
            T("Update available"),
            T("Up to date"),
            T("Checking"),
            T("Downloading"),
            T("Verification failed"),
            T("Ready to install")
        });
        cboPreviewState.SelectedIndex = 0;
        cboPreviewState.SelectedIndexChanged += (_, _) => ApplyPreviewState(cboPreviewState.SelectedIndex);
        actions.Controls.Add(cboPreviewState, 1, 0);

        btnCheck = new Button
        {
            Text = T("Check now"),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 8, 5)
        };
        btnCheck.Click += async (_, _) => await CheckRealManifestAsync();
        actions.Controls.Add(btnCheck, 2, 0);

        btnInstall = new Button
        {
            Text = T("Download and install"),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 5, 0, 5),
            Enabled = false
        };
        btnInstall.Click += (_, _) => MessageBox.Show(
            this,
            T("The updater executable will be connected in the next update stage. Stage 4.10.0 does not replace files."),
            T("LiveSplit YS Update"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        actions.Controls.Add(btnInstall, 3, 0);

        chkAutoCheck = new CheckBox
        {
            AutoSize = true,
            Text = T("Automatically check for LiveSplit YS updates on startup"),
            Checked = true,
            Enabled = false,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(4, 0, 0, 0)
        };
        root.Controls.Add(chkAutoCheck, 0, 5);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(2, 4, 2, 0)
        };
        btnClose = new Button
        {
            Text = T("Close"),
            DialogResult = DialogResult.Cancel,
            Size = new Size(92, 30)
        };
        footer.Controls.Add(btnClose);
        root.Controls.Add(footer, 0, 6);
        CancelButton = btnClose;

        SetNotes(new[]
        {
            T("Independent LiveSplit YS update channel"),
            T("GitHub Releases package distribution"),
            T("SHA-256 package verification before installation"),
            T("Preserve user settings, layouts and split files during update"),
            T("Use a separate updater process to replace program files safely"),
            T("Support stable and test update channels"),
            T("Optional automatic update check on startup"),
            T("Paged release notes for longer changelogs")
        });

        ApplyPreviewState(0);
    }

    private static Label CreateCaption(string text) => new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        Text = text,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label CreateValue(string text) => new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        Text = text,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private void SetNotes(IEnumerable<string> notes, bool bullets = true)
    {
        string[] items = notes?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToArray() ?? Array.Empty<string>();

        if (items.Length == 0)
        {
            items = new[] { T("No release notes were provided.") };
            bullets = false;
        }

        notePages.Clear();
        for (int i = 0; i < items.Length; i += NotesPerPage)
        {
            notePages.Add(items.Skip(i).Take(NotesPerPage).ToArray());
        }

        notesUseBullets = bullets;
        currentNotePage = 0;
        RenderNotesPage();
    }

    private void ChangeNotesPage(int delta)
    {
        if (notePages.Count == 0)
        {
            return;
        }

        int newPage = Math.Max(0, Math.Min(notePages.Count - 1, currentNotePage + delta));
        if (newPage == currentNotePage)
        {
            return;
        }

        currentNotePage = newPage;
        RenderNotesPage();
    }

    private void RenderNotesPage()
    {
        int pageCount = Math.Max(1, notePages.Count);
        currentNotePage = Math.Max(0, Math.Min(pageCount - 1, currentNotePage));

        string[] page = notePages.Count > 0 ? notePages[currentNotePage] : Array.Empty<string>();
        txtNotes.Text = string.Join(
            Environment.NewLine + Environment.NewLine,
            page.Select(x => notesUseBullets ? "• " + x : x));
        txtNotes.SelectionStart = 0;
        txtNotes.ScrollToCaret();

        lblNotesPage.Text = string.Format(T("Page {0} / {1}"), currentNotePage + 1, pageCount);
        btnPreviousPage.Enabled = currentNotePage > 0;
        btnNextPage.Enabled = currentNotePage + 1 < pageCount;
    }

    private void ApplyPreviewState(int index)
    {
        progressBar.Visible = false;
        progressBar.Style = ProgressBarStyle.Blocks;
        btnInstall.Enabled = false;
        lblLatestVersion.Text = "v1.0.6";
        lblChannel.Text = T("Stable");
        lblManifest.Text = T("Preview data");

        switch (index)
        {
            case 1:
                lblLatestVersion.Text = $"v{ForkInfo.Version}";
                lblStatus.Text = T("LiveSplit YS is up to date.");
                break;
            case 2:
                lblLatestVersion.Text = "—";
                lblStatus.Text = T("Checking for updates...");
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                break;
            case 3:
                lblStatus.Text = T("Downloading update package...");
                progressBar.Visible = true;
                progressBar.Value = 62;
                break;
            case 4:
                lblStatus.Text = T("Package verification failed. The update will not be installed.");
                break;
            case 5:
                lblStatus.Text = T("The update package is verified and ready to install.");
                btnInstall.Enabled = true;
                break;
            default:
                lblStatus.Text = T("A new LiveSplit YS version is available.");
                break;
        }
    }

    private async Task CheckRealManifestAsync()
    {
        btnCheck.Enabled = false;
        cboPreviewState.Enabled = false;
        lblStatus.Text = T("Checking for updates...");
        lblLatestVersion.Text = "—";
        lblManifest.Text = T("GitHub Releases");
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;

        YSUpdateCheckResult result = await YSUpdateService.CheckAsync();

        progressBar.Visible = false;
        progressBar.Style = ProgressBarStyle.Blocks;
        btnCheck.Enabled = true;
        cboPreviewState.Enabled = true;

        if (result.State == YSUpdateCheckState.Error)
        {
            lblStatus.Text = T("Unable to read the LiveSplit YS update manifest.");
            lblLatestVersion.Text = "—";
            SetNotes(new[]
            {
                T("This is expected until latest.json is uploaded to the GitHub release."),
                result.ErrorMessage
            }, bullets: false);
            btnInstall.Enabled = false;
            return;
        }

        YSUpdateManifest manifest = result.Manifest;
        lblLatestVersion.Text = $"v{manifest.VersionText}";
        lblChannel.Text = string.Equals(manifest.Channel, "stable", StringComparison.OrdinalIgnoreCase)
            ? T("Stable")
            : manifest.Channel;
        lblManifest.Text = T("GitHub Releases");
        SetNotes(manifest.Notes);

        if (result.State == YSUpdateCheckState.UpdateAvailable)
        {
            lblStatus.Text = T("A new LiveSplit YS version is available.");
            // Stage 4.10.0 remains check/preview only. The actual installer is wired later.
            btnInstall.Enabled = false;
        }
        else
        {
            lblStatus.Text = T("LiveSplit YS is up to date.");
            btnInstall.Enabled = false;
        }
    }
}

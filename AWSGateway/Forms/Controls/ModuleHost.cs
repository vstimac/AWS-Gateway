using AWSGateway.Helpers;

namespace AWSGateway.Forms
{
    // ugrađuje postojeću formu kao sadržaj ljuske, bez modalnog prozora
    public class ModuleHost : UserControl, IAppModule
    {
        private readonly Form _inner;
        private readonly IAppModule _module;

        public ModuleHost(Form inner)
        {
            _inner = inner;
            _module = inner as IAppModule;

            // ovaj kontejner rasteže ugrađenu formu na cijelu širinu (Dock.Fill) - bez double bufferinga
            // ručno povlačenje ruba prozora ostavlja repove starog iscrtavanja preko cijele forme
            UiTheme.EnableDoubleBuffering(this);

            Dock = DockStyle.Fill;
            BackColor = UiTheme.Colors.Background;
            Padding = new Padding(UiTheme.Space2);
            AutoScroll = true;

            _inner.TopLevel = false;
            _inner.FormBorderStyle = FormBorderStyle.None;
            _inner.ControlBox = false;
            _inner.Dock = DockStyle.Fill;
            _inner.MinimumSize = Size.Empty;
            _inner.AutoScroll = false;
            _inner.ShowInTaskbar = false;

            Controls.Add(_inner);
            _inner.Show();
        }

        public Form InnerForm
        {
            get { return _inner; }
        }

        public bool IsBusy
        {
            get
            {
                if (_module != null)
                {
                    return _module.IsBusy;
                }

                return false;
            }
        }

        public void ApplyShellLanguage()
        {
            if (_module != null)
            {
                _module.ApplyShellLanguage();
            }
        }

        public void ApplyShellTheme()
        {
            UiTheme.Apply(_inner);
            if (_module != null)
            {
                _module.ApplyShellTheme();
            }
        }

        public void OnActivated()
        {
            if (_module != null)
            {
                _module.OnActivated();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

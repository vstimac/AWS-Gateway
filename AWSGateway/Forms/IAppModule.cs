namespace AWSGateway.Forms
{
    // modul ugrađen u ljusku - zauzetost sprječava istek sesije kao nekada modalna podforma
    public interface IAppModule
    {
        bool IsBusy { get; }

        void ApplyShellLanguage();

        void ApplyShellTheme();

        // poziva se kad modul postane vidljiv (navigacija) - zadano ništa ne radi, postojeći moduli ga ne moraju implementirati
        void OnActivated()
        {
        }
    }
}

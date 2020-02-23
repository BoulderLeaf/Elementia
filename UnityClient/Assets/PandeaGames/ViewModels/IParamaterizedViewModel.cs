namespace PandeaGames.ViewModels
{
    public interface IParamaterizedViewModel<TParameters> : IViewModel
    {
        void SetParameters(TParameters parameters);
    }
}
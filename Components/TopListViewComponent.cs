using Microsoft.AspNetCore.Mvc;
using Retro_grupp_g.Repositories;

namespace Retro_grupp_g.Components
{
    public class TopListViewComponent : ViewComponent
    {
        private readonly IFilmRepository _filmRepository;

        public TopListViewComponent(IFilmRepository filmRepository)
        {
            this._filmRepository = filmRepository;
        }
    }
}

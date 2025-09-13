using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodFinder.Models
{
    public class EstablishmentList
    {
        public static List<Establishment> establishments;

        public List<SelectListItem> SortBySelectList { get; set; }

        public string SelectedSortBy { get; set; }
        public EstablishmentList()
        {
            if(establishments == null)
                establishments = new List<Establishment>();
        }



    }
}

using System.ComponentModel.DataAnnotations;

namespace API.Models.UserModels
{
    public class UserStakeModel : UserReferenceModel
    {
        [Required]
        public int amount { get; set; }
    }
}

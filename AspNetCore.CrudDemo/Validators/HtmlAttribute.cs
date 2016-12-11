using System.ComponentModel.DataAnnotations;
using System.Linq;
using HtmlAgilityPack;

namespace AspNetCore.CrudDemo.Validators
{
    public class HtmlAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(value is string))
                return ValidationResult.Success;

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(value.ToString());
            return htmlDocument.ParseErrors.Any() ? new ValidationResult(htmlDocument.ParseErrors.First().Reason) : ValidationResult.Success;
        }
    }
}

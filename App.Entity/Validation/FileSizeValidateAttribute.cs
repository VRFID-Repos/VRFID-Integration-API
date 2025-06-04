using App.Common;
using App.Common.Messages;
using App.Common.Support;
using App.Entity.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Validation
{
    public class FileSizeValidateAttribute : ValidationAttribute
    {

        private readonly long _maxSize;
        private readonly string _requiredErrorMessage;
        private readonly string _sizeErrorMessage;

        public FileSizeValidateAttribute(long size, string requiredErrorMessage, string sizeErrorMessage)
        {
            _maxSize = size;
            _requiredErrorMessage = requiredErrorMessage;
            _sizeErrorMessage = sizeErrorMessage;
        }

        //protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        //{
        //    CourseVM? courseVM = validationContext.ObjectInstance as CourseVM;
        //    IFormFile? file = value as IFormFile;
        //    if (courseVM is not null && courseVM.CourseId == 0 && file is null)
        //    {
        //        return new ValidationResult(_requiredErrorMessage);
        //    }
        //    if (file is not null && file.Length > _maxSize)
        //    {
        //        return new ValidationResult(_sizeErrorMessage);
        //    }
        //    return ValidationResult.Success;
        //}
    }
}

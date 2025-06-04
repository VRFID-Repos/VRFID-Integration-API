using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Messages
{
    public class ValidationMessages
    {
        // Admin Validation Messages
        public const string EducationNameRequired = "Please fill Education Name";

        public const string PasswordValidationError = "Password should contain atleast one upercase, one lowercase, letter, and one special character and one number!";
        public const string ValidationMessage_EmailRequired = "Email is required";
        public const string ValidationMessage_GenderRequired = "Gender is required";
        public const string ValidationMessage_EmpStatusRequired = "Employement Status is required";
        public const string ValidationMessage_NameRequired = "Name is required";
        public const string ValidationMessage_CountryRequired = "Country is required";
        public const string ValidationMessage_EducationRequired = "Education is required";
        public const string ValidationMessage_TimezoneRequired = "Timezone is required";
        public const string ValidationMessage_EmailInValid = "Email is invalid";
        public const string ValidationMessage_PasswordRequired = "Password is required";
        public const string ValidationMessage_ConfirmPassword = "Password and Confirm password did not match";

        public const string RegNumRequired = "Please fill Registration number.";
        public const string CourseDesciptionRequired = "Please fill course description.";
        public const string CourseLevelRequired = "Please fill course level.";
        public const string CourseLangugeRequired = "Please fill course language.";
        public const string CourseSubjectRequired = "Please fill course subject.";
        public const string CourseLengthRequired = "Please fill course length.";
        public const string CourseeEffortRequired = "Please fill course effort.";
        public const string CourseTypeRequired = "Please select course type.";
        public const string CoursePriceRequired = "Please fill course price.";
        public const string CourseCoverImageRequired = "Please select course cover image.";
        public const string CourseCoverImageMaxSize = "Course cover image max size 1MB.";
        public const string CourseThumbnailImageRequired = "Please select course thumbnail image.";
        public const string CourseThumbnailImageMaxSize = "Course thumbnail image max size 500KB.";

        public const string TestNameRequired = "Test name is required";
        public const string TestDataRequired = "Test Questions are required";
        public const string TotalMarkRequired = "Total mark is required";
        public const string MinimumPassingRequired = "Minimum passing percentage is required";
        public const string MaxAttemptRequired = "Max attempt is required";
        public const string TotalTimeRequired = "Total test time is required";

        public const string CourseUrlRequired = "Course Url is required";
    }
}

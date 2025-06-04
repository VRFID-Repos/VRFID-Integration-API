namespace App.Common.Messages
{
    public class ErrorMessages
    {
        public const string EmailRequired = "Email or Password is required";
        public const string EmailNotVerified = "Email is not verified yet.";
        public const string EmailLoginInvalid = "Invalid Password";
        public const string EmailNotExist = "This email doesn't exist in our database";
        public const string ErrorSomethingWrong = "Something Went Wrong!";
        public const string ErrorEmailAlready = "Email already exist!";
        public const string InvalidRequest = "Invalid Request!";
        public const string PasswordNotMatch = "Current Password is wrong";
        public const string PasswordInvalid = "Minimum 8 characters are required.Password should contain at least one uppercase, one lowercase, letter, and one special character and one number.";
        public const string LinkExpired = "Link is expired.";

        public const string FinalTestGradeError = "Please distrubute the grades among questions. Overall grade for the exam is 100%";
        public const string StudentAlreadyRegistered = "Student is already registered";

        public const string NoAttemptLeft = "No attempt left!";
        public const string NoCoursetLeft = "You have no course left!";
        public const string DocNotUploaded = "You have not uploaded your driving licence yet!";
        public const string DocUnderReview = "Your driving license is under review. Please wait until it gets verified!";
        public const string DocRejected = "You have uploaded invalid driving licence. Please upload again!";
    }
}

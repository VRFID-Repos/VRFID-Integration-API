using App.Entity.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Bal.Services
{
    public interface IMailService
    {
        public Task SendEmailVerification(EmailDto emailDto);
        public Task SendEmailForgotPassword(EmailDto emailDto);
        public Task SendDocUploadedAdminMail(EmailDto emailDto);
        public Task SendDocApprovedMail(EmailDto emailDto);
        public Task SendDocRejectedMail(EmailDto emailDto);
        public Task SendCourseIviteEmail(EmailDto emailDto);
        public Task SendCertificate(EmailDto emailDto);
        public Task SendExamPassedInstructorMail(EmailDto emailDto);
        public Task SendExamFailInstructorMail(EmailDto emailDto);
        public Task<string> SendEmailFromVRFIDAsync(string email, string url);
    }
}

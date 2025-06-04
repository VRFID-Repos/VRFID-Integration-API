using App.Bal.Services;
using App.Entity.Models.Config;
using Mandrill.Models;
using Mandrill.Requests.Messages;
using Mandrill;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace App.Bal.Repositories
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly MailConfig _mailConfig;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _mailConfig = new MailConfig();
            _configuration.GetSection(MailConfig.Path).Bind(_mailConfig);
        }
        public async Task SendCertificate(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "Certificate.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("INSTRUCTORNAME", emailDto.InstructorName);
            fileContent = fileContent.Replace("USERNAME", emailDto.FullName);
            fileContent = fileContent.Replace("COURSENAME", emailDto.CourseName);
            fileContent = fileContent.Replace("CERTIFICATELINK", emailDto.VerifyLink);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Course Completion Certificate",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }
        public async Task SendCourseIviteEmail(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "course_invite.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("INSTRUCTORNAME", emailDto.InstructorName);
            fileContent = fileContent.Replace("COURSENAME", emailDto.CourseName);
            fileContent = fileContent.Replace("INVITATIONLINK", emailDto.VerifyLink);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Course Invitation",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }
        public async Task SendDocUploadedAdminMail(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "doc_uploaded.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.FullName);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = " Pending Approval for Uploaded Driving License",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }
        public async Task SendDocApprovedMail(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "doc_approved.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.FullName);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.FullName)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Driving License Verification Status",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }
        public async Task SendDocRejectedMail(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "doc_rejected.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.FullName);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.FullName)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Driving License Verification Status",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }

        public async Task SendEmailVerification(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "SignUpEmail.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.FullName);
            fileContent = fileContent.Replace("INVITATIONLINK", emailDto.VerifyLink);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Verify your account - Anything Car",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }



        public async Task SendEmailForgotPassword(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "ForgotPassword.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.FullName);
            fileContent = fileContent.Replace("INVITATIONLINK", emailDto.VerifyLink);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Password Recovery - Anything Cars",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }

        public async Task SendExamPassedInstructorMail(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "ExamPassed.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.InstructorName);
            fileContent = fileContent.Replace("STUDENTNAME", emailDto.FullName);
            fileContent = fileContent.Replace("COURSENAME", emailDto.CourseName);
            fileContent = fileContent.Replace("CERTIFICATELINK", emailDto.VerifyLink);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Notification of Exam Results - Pegasus Courses",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }
        public async Task SendExamFailInstructorMail(EmailDto emailDto)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Template", "ExamFailed.html");
            string fileContent = File.ReadAllText(path);

            fileContent = fileContent.Replace("USERNAME", emailDto.InstructorName);
            fileContent = fileContent.Replace("STUDENTNAME", emailDto.FullName);
            fileContent = fileContent.Replace("COURSENAME", emailDto.CourseName);
            fileContent = fileContent.Replace("CERTIFICATELINK", emailDto.VerifyLink);

            MandrillApi mandrillApi = new(_mailConfig.ApiKey);

            List<EmailAddress> toEmail = new()
            {
                new EmailAddress(emailDto.Email)
            };

            EmailMessage emailMessage = new()
            {
                FromEmail = _mailConfig.Email,
                FromName = _mailConfig.FromName,
                To = toEmail,
                Subject = "Notification of Exam Results - Pegasus Courses",
                Html = fileContent
            };

            List<EmailResult> emailResults = await mandrillApi.SendMessage(new SendMessageRequest(emailMessage));
        }

        public async Task<string> SendEmailFromVRFIDAsync(string email, string url)
        {
            // Define the endpoint URL
            string endpoint = "https://api-vrfid-id.azurewebsites.net/api/pass/EmailByURL";

            // Create the request body
            var requestBody = new { Email = email, URL = url };
            string jsonRequestBody = JsonSerializer.Serialize(requestBody);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set up the request content with JSON payload
                    StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    // Send a POST request
                    HttpResponseMessage response = await client.PostAsync(endpoint, content);

                    // Ensure the response indicates success
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    return jsonResponse; // Return the full response content for further processing
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and optionally log them
                throw new Exception($"An error occurred while sending the email by URL: {ex.Message}", ex);
            }
        }

    }
}

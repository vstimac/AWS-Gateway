using System;
using System.Collections.Generic;
using System.Text;
using AWSGateway.Helpers;
using AWSGateway.Models;

namespace AWSGateway.Models
{
    public class PasswordPolicy
    {
        public int MinLength { get; set; }
        public bool RequireDigit { get; set; }

        public PasswordPolicy()
        {
            MinLength = 6;
            RequireDigit = true;
        }

        public bool IsValid(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            if (password.Length < MinLength)
            {
                return false;
            }
            if (RequireDigit)
            {
                bool hasDigit = false;

                foreach(char x in password)
                {
                    if (char.IsDigit(x))
                    {
                        hasDigit = true;
                        break;
                    }
                }
                if (!hasDigit)
                {
                    return false;
                }
            }

            return true;
        }

        public string GetRequirementsText()
        {
            string text = LanguageHelper.Format("login_password_requirements_base", MinLength);

            if (RequireDigit)
            {
                text = text + LanguageHelper.Get("login_password_requirements_digit_suffix");
            }

            return text + ".";
        }
    }
}

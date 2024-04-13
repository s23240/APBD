using System;
using System.Linq;

namespace LegacyApp
{
    public class UserService
    {
        private bool NameValidator(string name)
        {
            return !string.IsNullOrEmpty(name) && name.All(char.IsLetter);
        }
        
        private bool EmailValidator(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
            return age;
        }
        
        private bool AgeValidator(DateTime dateOfBirth)
        {
            var age = CalculateAge(dateOfBirth);
            const int legalAge = 21;
            return age >= legalAge;
        }
        
        private void SetUserCreditLimit(User user, Client client)
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                    user.HasCreditLimit = true;
                    user.CreditLimit = GetCreditLimit(user) * 2;
                    break;
                default:
                    user.HasCreditLimit = true;
                    user.CreditLimit = GetCreditLimit(user);
                    break;
            }
        }

        private int GetCreditLimit(User user)
        {
            using var userCreditService = new UserCreditService();
            return userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
        }
        
        
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!NameValidator(firstName) || !NameValidator(lastName))
            {
                return false;
            }

            if (!EmailValidator(email))
            {
                return false;
            }


            if (!AgeValidator(dateOfBirth))
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            SetUserCreditLimit(user, client);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
